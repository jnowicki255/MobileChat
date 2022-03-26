using MobileChat.Mqtt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MobileChat
{
    public partial class MainPage : ContentPage
    {
        private MqttProvider mqttProvider;
        private string mqttTopic = "XamarinChat";
        public ObservableCollection<string> Messages = new ObservableCollection<string>();

        public MainPage()
        {
            InitializeComponent();

            this.mqttProvider = new MqttProvider();
            this.mqttProvider.OnMessage += MessageReceived;
            this.mqttProvider.ConnectAsync();
            this.mqttProvider.SubscribeAsync(mqttTopic);

            MessageEnt.ReturnCommand = new Command<string>(PublishMessage);
            MessagesLV.ItemsSource = Messages;
        }

        private void MessageReceived(string message)
        {
            Messages.Add(message);
        }

        private void PublishMessage(string message)
        {
            this.mqttProvider.PublishAsync(mqttTopic, MessageEnt.Text);
        }
    }
}
