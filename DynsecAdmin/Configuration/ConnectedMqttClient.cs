using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;

namespace DynsecAdmin.Configuration
{
    public class ConnectedMqttClient : IMqttClient
    {
        private readonly IMqttClient _client;
        private readonly Task<MqttClientConnectResult> _connect;

        public ConnectedMqttClient(MqttFactory factory, MqttClientOptions options, IHttpContextAccessor contextAccessor)
        {
            _client = factory.CreateMqttClient();
            _connect = _client.ConnectAsync(options, contextAccessor.HttpContext.RequestAborted);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task<MqttClientConnectResult> ConnectAsync(MqttClientOptions options, CancellationToken cancellationToken = default)
        {
            await _connect.ConfigureAwait(false);
            return await _client.ConnectAsync(options, cancellationToken).ConfigureAwait(false);
        }

        public async Task DisconnectAsync(MqttClientDisconnectOptions options, CancellationToken cancellationToken = default)
        {
            await _connect.ConfigureAwait(false);
            await _client.DisconnectAsync(options, cancellationToken).ConfigureAwait(false);
        }

        public async Task PingAsync(CancellationToken cancellationToken = default)
        {
            await _connect.ConfigureAwait(false);
            await _client.PingAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage applicationMessage, CancellationToken cancellationToken = default)
        {
            await _connect.ConfigureAwait(false);
            return await _client.PublishAsync(applicationMessage, cancellationToken).ConfigureAwait(false);
        }

        public async Task SendExtendedAuthenticationExchangeDataAsync(MqttExtendedAuthenticationExchangeData data, CancellationToken cancellationToken = default)
        {
            await _connect.ConfigureAwait(false);
            await _client.SendExtendedAuthenticationExchangeDataAsync(data, cancellationToken).ConfigureAwait(false);
        }

        public async Task<MqttClientSubscribeResult> SubscribeAsync(MqttClientSubscribeOptions options, CancellationToken cancellationToken = default)
        {
            await _connect.ConfigureAwait(false);
            return await _client.SubscribeAsync(options, cancellationToken).ConfigureAwait(false);
        }

        public async Task<MqttClientUnsubscribeResult> UnsubscribeAsync(MqttClientUnsubscribeOptions options, CancellationToken cancellationToken = default)
        {
            await _connect.ConfigureAwait(false);
            return await _client.UnsubscribeAsync(options, cancellationToken).ConfigureAwait(false);
        }

        public bool IsConnected => _client.IsConnected;

        public MqttClientOptions Options => _client.Options;

        public event Func<MqttApplicationMessageReceivedEventArgs, Task>? ApplicationMessageReceivedAsync
        {
            add => _client.ApplicationMessageReceivedAsync += value;
            remove => _client.ApplicationMessageReceivedAsync -= value;
        }

        public event Func<MqttClientConnectedEventArgs, Task>? ConnectedAsync
        {
            add => _client.ConnectedAsync += value;
            remove => _client.ConnectedAsync -= value;
        }

        public event Func<MqttClientConnectingEventArgs, Task>? ConnectingAsync
        {
            add => _client.ConnectingAsync += value;
            remove => _client.ConnectingAsync -= value;
        }

        public event Func<MqttClientDisconnectedEventArgs, Task>? DisconnectedAsync
        {
            add => _client.DisconnectedAsync += value;
            remove => _client.DisconnectedAsync -= value;
        }

        public event Func<InspectMqttPacketEventArgs, Task>? InspectPackage
        {
            add => _client.InspectPackage += value;
            remove => _client.InspectPackage -= value;
        }
    }
}
