using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileChat.Mqtt
{
    public class MqttProvider
    {
        private IManagedMqttClient mqttClient;
        private IManagedMqttClientOptions mqttClientOptions;

        public Action<string> OnMessage { get; set; }

        public MqttProvider()
        {
            this.mqttClientOptions = new ManagedMqttClientOptions
            {
                ClientOptions = new MqttClientOptions
                {
                    ClientId = Guid.NewGuid().ToString(),
                    Credentials = new MqttClientCredentials
                    {
                        Username = "",
                        Password = Encoding.ASCII.GetBytes("")
                    },
                    ProtocolVersion = MQTTnet.Formatter.MqttProtocolVersion.V311,
                    CleanSession = true,
                    CommunicationTimeout = TimeSpan.FromSeconds(5),
                    KeepAlivePeriod = TimeSpan.FromSeconds(5),
                    ChannelOptions = new MqttClientTcpOptions
                    {
                        Server = "test.mosquitto.org",
                        Port = 1883
                    }
                }
            };

            this.mqttClient = new MqttFactory().CreateManagedMqttClient();

            this.mqttClient.ConnectedHandler = 
                new MqttClientConnectedHandlerDelegate(OnClientConnected);

            this.mqttClient.DisconnectedHandler = 
                new MqttClientDisconnectedHandlerDelegate(OnClientDisconnected);

            this.mqttClient.ConnectingFailedHandler = 
                new ConnectingFailedHandlerDelegate(OnClientConnectingFailed);

            this.mqttClient.ApplicationMessageReceivedHandler = 
                new MqttApplicationMessageReceivedHandlerDelegate(OnMessageReceived);

        }

        public async Task ConnectAsync()
        {
            await this.mqttClient.StartAsync(this.mqttClientOptions);
        }

        public async Task DisconnectAsync()
        {
            await this.mqttClient.StopAsync();
        }

        public async Task SubscribeAsync(string topic = "#")
        {
            await this.mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build());
        }

        public async Task UnSubscribeAsync(string topic = "#")
        {
            await this.mqttClient.UnsubscribeAsync(topic);
        }

        public async Task PublishAsync(string topic, string message)
        {
            await this.mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(message))
                .Build());
        }

        private void OnMessageReceived(MqttApplicationMessageReceivedEventArgs obj)
        {
            var receivedMessage = Encoding.UTF8.GetString(obj.ApplicationMessage.Payload);
            OnMessage?.Invoke(receivedMessage);
        }

        private void OnClientConnectingFailed(ManagedProcessFailedEventArgs obj)
        {
            throw new Exception("Cannot connect to MQTT Broker", obj.Exception);
        }

        private async Task OnClientDisconnected(MqttClientDisconnectedEventArgs obj)
        {
            await this.mqttClient.StartAsync(this.mqttClientOptions);
        }

        private void OnClientConnected(MqttClientConnectedEventArgs obj) { }
    }
}
