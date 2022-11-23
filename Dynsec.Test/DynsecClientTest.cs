using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using Xunit.Abstractions;

namespace Dynsec.Test
{
    public class DynsecClientTest
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly MqttClientOptions _mqttOptions;

        public DynsecClientTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            var mqttOptionBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost")
                .WithCredentials("admin", "password");
            _mqttOptions = mqttOptionBuilder.Build();
        }

        [Fact]
        public async Task CanGetDefaultAclAccess()
        {
            using var mqttClient = new MqttFactory().CreateMqttClient();
            await mqttClient.ConnectAsync(_mqttOptions);

            var client = new DynsecClient(mqttClient);

            var response = await client.GetDefaultAclAccessAsync(CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(4, response.Length);
        }

        [Fact]
        public async Task CanGetAnonymousGroup()
        {
            using var mqttClient = new MqttFactory().CreateMqttClient();
            await mqttClient.ConnectAsync(_mqttOptions);

            var client = new DynsecClient(mqttClient);

            var response = await client.GetAnonymousGroupAsync(CancellationToken.None);
            Assert.NotNull(response);
            Assert.True(String.IsNullOrEmpty(response.Groupname));
        }

        [Fact]
        public async Task CanListClients()
        {
            using var mqttClient = new MqttFactory().CreateMqttClient();
            await mqttClient.ConnectAsync(_mqttOptions);

            var client = new DynsecClient(mqttClient);

            var response = await client.ListClientsAsync(cancellationToken:CancellationToken.None);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task CanListClientsVerbose()
        {
            using var mqttClient = new MqttFactory().CreateMqttClient();
            await mqttClient.ConnectAsync(_mqttOptions);

            var client = new DynsecClient(mqttClient);

            var response = await client.ListClientsVerboseAsync(cancellationToken:CancellationToken.None);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task CanGetClient()
        {
            using var mqttClient = new MqttFactory().CreateMqttClient();
            await mqttClient.ConnectAsync(_mqttOptions);

            var client = new DynsecClient(mqttClient);

            var response = await client.GetClientAsync("admin", cancellationToken:CancellationToken.None);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task CanListGroups()
        {
            using var mqttClient = new MqttFactory().CreateMqttClient();
            await mqttClient.ConnectAsync(_mqttOptions);

            var client = new DynsecClient(mqttClient);

            var response = await client.ListGroupsAsync(cancellationToken:CancellationToken.None);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task CanListGroupsVerbose()
        {
            using var mqttClient = new MqttFactory().CreateMqttClient();
            await mqttClient.ConnectAsync(_mqttOptions);

            var client = new DynsecClient(mqttClient);

            var response = await client.ListGroupsVerboseAsync(cancellationToken:CancellationToken.None);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task CanGetGroup()
        {
            using var mqttClient = new MqttFactory().CreateMqttClient();
            await mqttClient.ConnectAsync(_mqttOptions);

            var client = new DynsecClient(mqttClient);

            var response = await client.GetGroupAsync("admin", cancellationToken:CancellationToken.None);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task CanListRoles()
        {
            using var mqttClient = new MqttFactory().CreateMqttClient();
            await mqttClient.ConnectAsync(_mqttOptions);

            var client = new DynsecClient(mqttClient);

            var response = await client.ListRolesAsync(cancellationToken:CancellationToken.None);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task CanListRolesVerbose()
        {
            using var mqttClient = new MqttFactory().CreateMqttClient();
            await mqttClient.ConnectAsync(_mqttOptions);

            var client = new DynsecClient(mqttClient);

            var response = await client.ListRolesVerboseAsync(cancellationToken:CancellationToken.None);
            Assert.NotNull(response);
        }

        [Fact]
        public async Task CanGetRole()
        {
            using var mqttClient = new MqttFactory().CreateMqttClient();
            await mqttClient.ConnectAsync(_mqttOptions);

            var client = new DynsecClient(mqttClient);

            var response = await client.GetRoleAsync("admin", cancellationToken:CancellationToken.None);
            Assert.NotNull(response);
        }
    }
}