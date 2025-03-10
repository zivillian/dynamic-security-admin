﻿using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Dynsec.DTO;
using MQTTnet;
using MQTTnet.Client;

namespace Dynsec
{
    public class DynsecClient:IDisposable
    {
        private readonly IMqttClient _client;
        private readonly TimeSpan _timeout;
        private readonly SemaphoreSlim _subscribeLock = new(1, 1);
        private bool _subscribed = false;
        private const string ResponseTopic = "$CONTROL/dynamic-security/v1/response";
        readonly ConcurrentDictionary<string, TaskCompletionSource<JsonNode>> _waitingCalls = new();
        private readonly JsonSerializerOptions _jsonOptions;

        public DynsecClient(IMqttClient client) : this(client, TimeSpan.FromSeconds(1))
        {
        }

        public DynsecClient(IMqttClient client, TimeSpan timeout)
        {
            _client = client;
            _timeout = timeout;
            _client.ApplicationMessageReceivedAsync += HandleApplicationMessageReceivedAsync;
            _jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        private ValueTask SubscribeAsync(CancellationToken cancellationToken)
        {
            if (_subscribed) return ValueTask.CompletedTask;
            return SubscribeInternalAsync(cancellationToken); 
        }

        private async ValueTask SubscribeInternalAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _subscribeLock.WaitAsync(cancellationToken);
                if (_subscribed) return;

                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(ResponseTopic)
                    .Build();
                await _client.SubscribeAsync(subscribeOptions, cancellationToken);
                _subscribed = true;
            }
            finally
            {
                _subscribeLock.Release();
            }
        }

        public Task UnsubscribeAsync(CancellationToken cancellationToken)
        {
            return _client.UnsubscribeAsync(ResponseTopic, cancellationToken);
        }

        public Task SetDefaultAclAccess(Acl[] acls, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "setDefaultACLAccess",
                ["acls"] = JsonSerializer.SerializeToNode(acls, _jsonOptions)
            };
            return ExecuteAsync(request, cancellationToken);
        }

        public async Task<Acl[]> GetDefaultAclAccessAsync(CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "getDefaultACLAccess"
            };
            var response = await ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
            var acls = response["data"]?["acls"]?.AsArray();
            if (acls is null)
            {
                throw new DynsecProtocolException("'acls' property missing", response.ToJsonString());
            }
            return acls.Deserialize<Acl[]>();
        }
        
        public Task CreateClientAsync(Client client, CancellationToken cancellationToken)
        {
            var request = JsonSerializer.SerializeToNode(client, _jsonOptions).AsObject();
            request["command"] = "createClient";
            return ExecuteAsync(request, cancellationToken);
        }

        public Task DeleteClientAsync(string username, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "deleteClient",
                ["username"] = username
            };
            return ExecuteAsync(request, cancellationToken);
        }

        public Task EnableClientAsync(string username, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "enableClient",
                ["username"] = username
            };
            return ExecuteAsync(request, cancellationToken);
        }

        public Task DisableClientAsync(string username, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "disableClient",
                ["username"] = username
            };
            return ExecuteAsync(request, cancellationToken);
        }

        public async Task<Client> GetClientAsync(string username, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "getClient",
                ["username"] = username
            };
            var response = await ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
            var client = response["data"]?["client"];
            if (client is null)
            {
                throw new DynsecProtocolException("'client' property missing", response.ToJsonString());
            }
            return client.Deserialize<Client>();
        }

        public Task<string[]> ListClientsAsync(CancellationToken cancellationToken)
        {
            return ListClientsAsync(count: -1, offset: 0, cancellationToken: cancellationToken)
                .ContinueWith(x=>x.Result.Items, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public async Task<ListResponse<string>> ListClientsAsync(int count, int offset, CancellationToken cancellationToken)
        {
            var response = await ListAsync("listClients", false, count, offset, cancellationToken).ConfigureAwait(false);
            var clients = response["data"]?["clients"];
            return new ListResponse<string>
            {
                Items = clients.Deserialize<string[]>(),
                Total = response["data"]["totalCount"].GetValue<int>()
            };
        }

        public Task<Client[]> ListClientsVerboseAsync(CancellationToken cancellationToken)
        {
            return ListClientsVerboseAsync(count: -1, offset: 0, cancellationToken: cancellationToken)
                .ContinueWith(x => x.Result.Items, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public async Task<ListResponse<Client>> ListClientsVerboseAsync(int count, int offset, CancellationToken cancellationToken)
        {
            var response = await ListAsync("listClients", true, count, offset, cancellationToken);
            var clients = response["data"]?["clients"];
            return new ListResponse<Client>
            {
                Items = clients.Deserialize<Client[]>(),
                Total = response["data"]["totalCount"].GetValue<int>()
            };
        }

        public Task ModifyClientAsync(Client client, CancellationToken cancellationToken)
        {
            var request = JsonSerializer.SerializeToNode(client, _jsonOptions).AsObject();
            request["command"] = "modifyClient";
            return ExecuteAsync(request, cancellationToken);
        }

        public Task SetClientIdAsync(string username, string clientId, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "setClientId",
                ["username"] = username,
                ["clientid"] = clientId,
            };
            return ExecuteAsync(request, cancellationToken);
        }

        public Task SetPasswordAsync(string username, string password, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "setClientPassword",
                ["username"] = username,
                ["password"] = password,
            };
            return ExecuteAsync(request, cancellationToken);
        }

        public Task AddClientRoleAsync(string username, string rolename, CancellationToken cancellationToken)
        {
            return AddClientRoleAsync(username, rolename, -1, cancellationToken);
        }

        public Task AddClientRoleAsync(string username, string rolename, int priority, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "addClientRole",
                ["username"] = username,
                ["rolename"] = rolename,
                ["priority"] = priority
            };
            return ExecuteAsync(request, cancellationToken);
        }

        public Task RemoveClientRoleAsync(string username, string rolename, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "removeClientRole",
                ["username"] = username,
                ["rolename"] = rolename,
            };
            return ExecuteAsync(request, cancellationToken);
        }

        public Task AddGroupClientAsync(string groupname, string username, CancellationToken cancellationToken)
        {
            return AddGroupClientAsync(groupname, username, -1, cancellationToken);
        }

        public Task AddGroupClientAsync(string groupname, string username, int priority, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "addGroupClient",
                ["groupname"] = groupname,
                ["username"] = username,
                ["priority"] = priority
            };
            return ExecuteAsync(request, cancellationToken);
        }

        public Task CreateGroupAsync(Group group, CancellationToken cancellationToken)
        {
            var request = JsonSerializer.SerializeToNode(group, _jsonOptions).AsObject();
            request["command"] = "createGroup";
            return ExecuteAsync(request, cancellationToken);
        }

        public Task DeleteGroupAsync(string groupname, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "deleteGroup",
                ["groupname"] = groupname
            };
            return ExecuteAsync(request, cancellationToken);
        }

        public async Task<Group> GetGroupAsync(string groupname, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "getGroup",
                ["groupname"] = groupname
            };
            var response = await ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
            var group = response["data"]?["group"];
            if (group is null)
            {
                throw new DynsecProtocolException("'group' property missing", response.ToJsonString());
            }
            return group.Deserialize<Group>();
        }
        
        public Task<string[]> ListGroupsAsync(CancellationToken cancellationToken)
        {
            return ListGroupsAsync(count: -1, offset: 0, cancellationToken: cancellationToken)
                .ContinueWith(x => x.Result.Items, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public async Task<ListResponse<string>> ListGroupsAsync(int count, int offset, CancellationToken cancellationToken)
        {
            var response = await ListAsync("listGroups", false, count, offset, cancellationToken).ConfigureAwait(false);
            var groups = response["data"]?["groups"];
            return new ListResponse<string>
            {
                Items = groups.Deserialize<string[]>(),
                Total = response["data"]["totalCount"].GetValue<int>()
            };
        }

        public Task<Group[]> ListGroupsVerboseAsync(CancellationToken cancellationToken)
        {
            return ListGroupsVerboseAsync(count: -1, offset: 0, cancellationToken: cancellationToken)
                .ContinueWith(x => x.Result.Items, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public async Task<ListResponse<Group>> ListGroupsVerboseAsync(int count, int offset, CancellationToken cancellationToken)
        {
            var response = await ListAsync("listGroups", true, count, offset, cancellationToken);
            var groups = response["data"]?["groups"];
            return new ListResponse<Group>
            {
                Items = groups.Deserialize<Group[]>(),
                Total = response["data"]["totalCount"].GetValue<int>()
            };
        }

        public Task ModifyGroupAsync(Group group, CancellationToken cancellationToken)
        {
            var request = JsonSerializer.SerializeToNode(group, _jsonOptions).AsObject();
            request["command"] = "modifyGroup";
            return ExecuteAsync(request, cancellationToken);
        }

        public Task RemoveGroupClientAsync(string groupname, string username, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "removeGroupClient",
                ["groupname"] = groupname,
                ["username"] = username,
            };
            return ExecuteAsync(request, cancellationToken);
        }

        public Task AddGroupRoleAsync(string groupname, string rolename, int priority, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "addGroupRole",
                ["groupname"] = groupname,
                ["rolename"] = rolename,
                ["priority"] = priority,
            };
            return ExecuteAsync(request, cancellationToken);
        }

        public Task RemoveGroupRoleAsync(string groupname, string rolename, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "removeGroupRole",
                ["groupname"] = groupname,
                ["rolename"] = rolename,
            };
            return ExecuteAsync(request, cancellationToken);
        }

        public Task SetAnonymousGroup(string groupname, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "setAnonymousGroup",
                ["groupname"] = groupname,
            };
            return ExecuteAsync(request, cancellationToken);
        }

        public async Task<string> GetAnonymousGroupAsync(CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "getAnonymousGroup"
            };
            var response = await ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
            var group = response["data"]?["group"];
            if (group is null)
            {
                throw new DynsecProtocolException("'group' property missing", response.ToJsonString());
            }
            return group["groupname"].GetValue<string>();
        }

        public Task CreateRoleAsync(Role role, CancellationToken cancellationToken)
        {
            var request = JsonSerializer.SerializeToNode(role, _jsonOptions).AsObject();
            request["command"] = "createRole";
            return ExecuteAsync(request, cancellationToken);
        }

        public async Task<Role> GetRoleAsync(string rolename, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "getRole",
                ["rolename"] = rolename
            };
            var response = await ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
            var role = response["data"]?["role"];
            if (role is null)
            {
                throw new DynsecProtocolException("'role' property missing", response.ToJsonString());
            }
            return role.Deserialize<Role>();
        }
        
        public Task<string[]> ListRolesAsync(CancellationToken cancellationToken)
        {
            return ListRolesAsync(count: -1, offset: 0, cancellationToken: cancellationToken)
                .ContinueWith(x => x.Result.Items, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public async Task<ListResponse<string>> ListRolesAsync(int count, int offset, CancellationToken cancellationToken)
        {
            var response = await ListAsync("listRoles", false, count, offset, cancellationToken).ConfigureAwait(false);
            var roles = response["data"]?["roles"];
            return new ListResponse<string>
            {
                Items = roles.Deserialize<string[]>(),
                Total = response["data"]["totalCount"].GetValue<int>()
            };
        }

        public Task<Role[]> ListRolesVerboseAsync(CancellationToken cancellationToken)
        {
            return ListRolesVerboseAsync(count: -1, offset: 0, cancellationToken: cancellationToken)
                .ContinueWith(x => x.Result.Items, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public async Task<ListResponse<Role>> ListRolesVerboseAsync(int count, int offset, CancellationToken cancellationToken)
        {
            var response = await ListAsync("listRoles", true, count, offset, cancellationToken);
            var roles = response["data"]?["roles"];
            return new ListResponse<Role>
            {
                Items = roles.Deserialize<Role[]>(),
                Total = response["data"]["totalCount"].GetValue<int>()
            };
        }

        public Task ModifyRoleAsync(Role role, CancellationToken cancellationToken)
        {
            var request = JsonSerializer.SerializeToNode(role, _jsonOptions).AsObject();
            request["command"] = "modifyRole";
            return ExecuteAsync(request, cancellationToken);
        }

        public Task DeleteRoleAsync(string rolename, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "deleteRole",
                ["rolename"] = rolename
            };
            return ExecuteAsync(request, cancellationToken);
        }

        public Task AddRoleACLAsync(string rolename, Acl acl, CancellationToken cancellationToken)
        {
            var request = JsonSerializer.SerializeToNode(acl, _jsonOptions).AsObject();
            request["command"] = "addRoleACL";
            request["rolename"] = rolename;
            return ExecuteAsync(request, cancellationToken);
        }

        public Task RemoveRoleACLAsync(string rolename, AclType type, string topic, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = "removeRoleACL",
                ["rolename"] = rolename,
                ["acltype"] = JsonSerializer.SerializeToNode(type, _jsonOptions),
                ["topic"] = topic,
            };
            return ExecuteAsync(request, cancellationToken);
        }
        
        private Task<JsonNode> ListAsync(string command, bool verbose, int count, int offset, CancellationToken cancellationToken)
        {
            var request = new JsonObject
            {
                ["command"] = command,
                ["verbose"] = verbose,
                ["count"] = count,
                ["offset"] = offset,
            };
            return ExecuteAsync(request, cancellationToken);
        }

        private async Task<JsonNode> ExecuteAsync(JsonObject command, CancellationToken cancellationToken)
        {
            if (command["command"] is null)
            {
                throw new DynsecProtocolException("command is empty", "");
            }
            command = new JsonObject
            {
                ["commands"] = new JsonArray
                {
                    command
                }
            };
            var requestMessage = new MqttApplicationMessageBuilder()
                .WithTopic("$CONTROL/dynamic-security/v1")
                .WithPayload(command.ToJsonString())
                .WithContentType("application/json")
                .Build();

            var awaitable = new TaskCompletionSource<JsonNode>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!_waitingCalls.TryAdd(command["commands"][0]["command"].GetValue<string>(), awaitable))
            {
                throw new InvalidOperationException();
            }
            await SubscribeAsync(cancellationToken);
            await _client.PublishAsync(requestMessage, cancellationToken).ConfigureAwait(false);
            
            using var cts = new CancellationTokenSource(_timeout);
            using var combined = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            await using (combined.Token.Register(() => { awaitable.TrySetCanceled(); }))
            {
                try
                {
                    var response = await awaitable.Task.ConfigureAwait(false);
                    if (response["error"] is not null)
                    {
                        throw new DynsecProtocolException(response["error"].ToString(), response.ToJsonString());
                    }
                    return response;
                }
                catch (OperationCanceledException ex)
                {
                    if (cts.IsCancellationRequested)
                    {
                        throw new DynsecTimeoutException(_timeout, ex);
                    }
                    throw;
                }
            }
        }

        Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            if (eventArgs.ApplicationMessage.Topic != ResponseTopic)
            {
                //not for us - ignore
                return Task.CompletedTask;
            }
            var data = eventArgs.ApplicationMessage.Payload;
            var json = JsonNode.Parse(data);
            var responses = json?["responses"]?.AsArray();
            if (responses is null)
            {
                foreach (var call in _waitingCalls)
                {
                    call.Value.SetException(new DynsecProtocolException("answer does not contain responses property", Encoding.UTF8.GetString(data)));
                }
                return Task.CompletedTask;
            }
            foreach (var response in responses)
            {
                var command = response["command"]?.GetValue<string>();
                if (_waitingCalls.TryRemove(command, out var awaitable))
                {
                    awaitable.SetResult(response);
                }
                else
                {
                    foreach (var call in _waitingCalls)
                    {
                        call.Value.SetException(new DynsecProtocolException($"unexpected reponse to command '{command}'", Encoding.UTF8.GetString(data)));
                    }
                    return Task.CompletedTask;
                }
            }

            // Set this message to handled to that other code can avoid execution etc.
            eventArgs.IsHandled = true;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _client.ApplicationMessageReceivedAsync -= HandleApplicationMessageReceivedAsync;

            foreach (var tcs in _waitingCalls)
            {
                tcs.Value.TrySetCanceled();
            }

            _waitingCalls.Clear();
        }
    }
}