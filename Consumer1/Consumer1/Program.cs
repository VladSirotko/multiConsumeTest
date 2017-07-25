using System;

namespace Consumer1
{
    using Newtonsoft.Json;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using System.Text;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var settings = new CommunicationSettingsProvider
            {
                SendEmailQueueName = "Consumer1",
                QueueHostName = "mq.corp.solbeg.com",
                QueuePassword = "BkA3ue5ncD",
                QueuePort = 5672,
                QueueUserName = "mqsolbeg",
                BindingKey = "BindingKey",
                Exchange = "Exchange"
            };
            Consume(settings);
            Console.ReadKey();
        }

        public class CommunicationSettingsProvider
        {
            public string SendEmailQueueName { get; set; }
            public string QueueHostName { get; set; }
            public string QueueUserName { get; set; }
            public string QueuePassword { get; set; }
            public int QueuePort { get; set; }
            public string SmtpServer { get; set; }
            public string BindingKey { get; set; }
            public string Exchange { get; set; }
            public int SmtpPort { get; set; }
            public string SmtpUserName { get; set; }
            public string SmtpPassword { get; set; }
            public string SmtpFromName { get; set; }
            public string SmtpFromEmail { get; set; }
        }
        private static void Consume(CommunicationSettingsProvider communicationSettingsProvider)
        {
            var factory = new ConnectionFactory
            {
                HostName = communicationSettingsProvider.QueueHostName,
                UserName = communicationSettingsProvider.QueueUserName,
                Password = communicationSettingsProvider.QueuePassword,
                Port = communicationSettingsProvider.QueuePort
            };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare(communicationSettingsProvider.Exchange, ExchangeType.Direct, true, false, null);
            channel.QueueDeclare(communicationSettingsProvider.SendEmailQueueName, true, false, false, null);
            channel.QueueBind(communicationSettingsProvider.SendEmailQueueName, communicationSettingsProvider.Exchange, communicationSettingsProvider.BindingKey);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += OnConsumerOnReceived;
            channel.BasicConsume(communicationSettingsProvider.SendEmailQueueName, true, consumer);
        }

        private static void OnConsumerOnReceived(object model, BasicDeliverEventArgs deliverEventArgs)
        {
            var info = Encoding.UTF8.GetString(deliverEventArgs.Body);
            if (string.IsNullOrEmpty(info))
            {
                throw new ArgumentNullException(nameof(deliverEventArgs.Body));
            }
            var convertModel = JsonConvert.DeserializeObject<Model>(info);
            var key = deliverEventArgs.RoutingKey;
            if (convertModel == null) return;
            Console.WriteLine("StringForFirstConsumer = " + convertModel.StringForFirstConsumer);
            Console.WriteLine("RoutingKey = " + key);
            Console.WriteLine(DateTime.UtcNow);
            Console.WriteLine();
        }

        public class Model
        {
            public string StringForFirstConsumer { get; set; }
            public string StringForSecondConsumer { get; set; }
        }
    }
}