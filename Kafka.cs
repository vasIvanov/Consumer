using Confluent.Kafka;
using EqClient.DataLayer.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Consumer
{
    public class Kafka
    {
        private readonly IProducer<int, MessageModel> _producer;

        public Kafka()
        {

            var config = new ProducerConfig()
            {
                BootstrapServers = "localhost:9092"
            };

            _producer = new ProducerBuilder<int, MessageModel>(config)
                .SetValueSerializer(new MsgPackSerializer<MessageModel>())
                .Build();
        }
    }
}
