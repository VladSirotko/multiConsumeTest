namespace Receive
{
    using Newtonsoft.Json;
    using RabbitMQ.Client;
    using System.Text;
    using System.Threading.Tasks;
    using System;
    internal class Program
    {
        private static void Main(string[] args)
        {

            var settings = new CommunicationSettingsProvider()
            {
                SendEmailQueueName = "TestQueue",
                QueueHostName = "mq.corp.solbeg.com",
                QueuePassword = "BkA3ue5ncD",
                QueuePort = 5672,
                QueueUserName = "mqsolbeg",
                BindingKey = "BindingKey",
                Exchange = "Exchange"
            };
            var model = new Model
            {
                StringForFirstConsumer = "StringForFirstConsumer",
                StringForSecondConsumer = "StringForSecondConsumer"
            };
            SendAsync(model, settings);
        }

        public class CommunicationSettingsProvider
        {
            public string SendEmailQueueName { get; set; }
            public string QueueHostName { get; set; }
            public string QueueUserName { get; set; }
            public string QueuePassword { get; set; }
            public int QueuePort { get; set; }
            public string BindingKey { get; set; }
            public string Exchange { get; set; }
            public string SmtpServer { get; set; }
            public int SmtpPort { get; set; }
            public string SmtpUserName { get; set; }
            public string SmtpPassword { get; set; }
            public string SmtpFromName { get; set; }
            public string SmtpFromEmail { get; set; }
        }

        public static Task SendAsync(Model model, CommunicationSettingsProvider communicationSettingsProvider)
        {
            var factory = new ConnectionFactory
            {
                HostName = communicationSettingsProvider.QueueHostName,
                UserName = communicationSettingsProvider.QueueUserName,
                Password = communicationSettingsProvider.QueuePassword,
                Port = communicationSettingsProvider.QueuePort
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(communicationSettingsProvider.Exchange, ExchangeType.Direct, true, false, null);
                channel.QueueDeclare("Consumer1", true, false, false, null);
                channel.QueueBind("Consumer1", communicationSettingsProvider.Exchange, communicationSettingsProvider.BindingKey);

                channel.QueueDeclare("Consumer2", true, false, false, null);
                channel.QueueBind("Consumer2", communicationSettingsProvider.Exchange, communicationSettingsProvider.BindingKey);

                var message = JsonConvert.SerializeObject(model);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: communicationSettingsProvider.Exchange,
                    routingKey: communicationSettingsProvider.BindingKey,
                    basicProperties: null,
                    body: body);
            }

            return Task.FromResult(0);
        }

        public class Model
        {
            public string StringForFirstConsumer { get; set; }
            public string StringForSecondConsumer { get; set; }
        }
    }
}