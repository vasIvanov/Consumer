using Confluent.Kafka;
using EqClient.DataLayer.Common;
using MessagePack;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Consumer
{
    class Program
    {

        static void Main(string[] args)
        {
            var config = new ProducerConfig()
            {
                BootstrapServers = "localhost:9092"
            };

            var producer = new ProducerBuilder<int, MessageModel>(config)
                    .SetValueSerializer(new MsgPackSerializer<MessageModel>())
                    .Build();

            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };


            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("Test", ExchangeType.Fanout, false, false);

                    //channel.QueueDeclare(queue: "task-queue", durable: true, exclusive: true, autoDelete: false, arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    Console.WriteLine("waiting");

                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += async (sender, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = MessagePackSerializer.Deserialize<MessageModel>(body);
                        message.MessageLength = message.Message.Length;
                        Console.WriteLine("recived {0} length {1}", message.Message, message.MessageLength);

                        await ProduceResultAsync(message);
                    };

                    channel.BasicConsume(queue: "task_queue", autoAck: true, consumer: consumer );

                    Console.WriteLine("Press [enter] to exit");

                    Console.ReadLine();
                }
            }


            async Task ProduceResultAsync(MessageModel data)
            {
                var msg = new Message<int, MessageModel>()
                {
                    Key = new Random().Next(),
                    Value = data
                };

                await producer.ProduceAsync("messageResult", msg);

            }
        }
    }
}
