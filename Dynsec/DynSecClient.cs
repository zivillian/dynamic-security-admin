using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Dynsec.DTO;
using MQTTnet;
using MQTTnet.Client;

namespace Dynsec
{
    public class DynsecClient:IDisposable
    {
        private readonly IMqttClient _client;
        private const string ResponseTopic = "$CONTROL/dynamic-security/v1/response";
        readonly ConcurrentDictionary<string, TaskCompletionSource<JsonNode>> _waitingCalls = new();

        public DynsecClient(IMqttClient client)
        {
            _client = client;
            _client.ApplicationMessageReceivedAsync += HandleApplicationMessageReceivedAsync;
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

        public async Task<Group> GetAnonymousGroupAsync(CancellationToken cancellationToken)
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
            return group.Deserialize<Group>();
        }

        public Task<string[]> ListClientsAsync(CancellationToken cancellationToken)
        {
            return ListClientsAsync(count: -1, offset: 0, cancellationToken: cancellationToken);
        }

        public async Task<string[]> ListClientsAsync(int count, int offset, CancellationToken cancellationToken)
        {
            var response = await ListAsync("listClients", false, count, offset, cancellationToken).ConfigureAwait(false);
            var clients = response["data"]?["clients"];
            return clients.Deserialize<string[]>();
        }

        public Task<Client[]> ListClientsVerboseAsync(CancellationToken cancellationToken)
        {
            return ListClientsVerboseAsync(count: -1, offset: 0, cancellationToken: cancellationToken);
        }

        public async Task<Client[]> ListClientsVerboseAsync(int count, int offset, CancellationToken cancellationToken)
        {
            var response = await ListAsync("listClients", true, count, offset, cancellationToken);
            var clients = response["data"]?["clients"];
            return clients.Deserialize<Client[]>();
        }

        public Task<string[]> ListGroupsAsync(CancellationToken cancellationToken)
        {
            return ListGroupsAsync(count: -1, offset: 0, cancellationToken: cancellationToken);
        }

        public async Task<string[]> ListGroupsAsync(int count, int offset, CancellationToken cancellationToken)
        {
            var response = await ListAsync("listGroups", false, count, offset, cancellationToken).ConfigureAwait(false);
            var groups = response["data"]?["groups"];
            return groups.Deserialize<string[]>();
        }

        public Task<Group[]> ListGroupsVerboseAsync(CancellationToken cancellationToken)
        {
            return ListGroupsVerboseAsync(count: -1, offset: 0, cancellationToken: cancellationToken);
        }

        public async Task<Group[]> ListGroupsVerboseAsync(int count, int offset, CancellationToken cancellationToken)
        {
            var response = await ListAsync("listGroups", true, count, offset, cancellationToken);
            var groups = response["data"]?["groups"];
            return groups.Deserialize<Group[]>();
        }

        public Task<string[]> ListRolesAsync(CancellationToken cancellationToken)
        {
            return ListRolesAsync(count: -1, offset: 0, cancellationToken: cancellationToken);
        }

        public async Task<string[]> ListRolesAsync(int count, int offset, CancellationToken cancellationToken)
        {
            var response = await ListAsync("listRoles", false, count, offset, cancellationToken).ConfigureAwait(false);
            var roles = response["data"]?["roles"];
            return roles.Deserialize<string[]>();
        }

        public Task<Role[]> ListRolesVerboseAsync(CancellationToken cancellationToken)
        {
            return ListRolesVerboseAsync(count: -1, offset: 0, cancellationToken: cancellationToken);
        }

        public async Task<Role[]> ListRolesVerboseAsync(int count, int offset, CancellationToken cancellationToken)
        {
            var response = await ListAsync("listRoles", true, count, offset, cancellationToken);
            var roles = response["data"]?["roles"];
            return roles.Deserialize<Role[]>();
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

            try
            {
                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(ResponseTopic)
                    .Build();
                await _client.SubscribeAsync(subscribeOptions, cancellationToken).ConfigureAwait(false);

                var awaitable = new TaskCompletionSource<JsonNode>(TaskCreationOptions.RunContinuationsAsynchronously);
                if (!_waitingCalls.TryAdd(command["commands"][0]["command"].GetValue<string>(), awaitable))
                {
                    throw new InvalidOperationException();
                }

                await _client.PublishAsync(requestMessage, cancellationToken).ConfigureAwait(false);

                using (cancellationToken.Register(() => { awaitable.TrySetCanceled(); }))
                {
                    var response = await awaitable.Task.ConfigureAwait(false);
                    if (response["error"] is not null)
                    {
                        throw new DynsecProtocolException(response["error"].ToString(), response.ToJsonString());
                    }
                    return response;
                }
            }
            finally
            {
                // ReSharper disable once MethodSupportsCancellation
                await _client.UnsubscribeAsync(ResponseTopic).ConfigureAwait(false);
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