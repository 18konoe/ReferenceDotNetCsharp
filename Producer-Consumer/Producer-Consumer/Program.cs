using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer_Consumer
{
    class Program
    {
        private static QueueProducer producer;
        static void Main(string[] args)
        {
            producer = new QueueProducer();

            producer.AddExample();
            producer.AddExample();
            producer.AddExample();

            producer.StartConsumer();

            producer.AddExample();
            producer.AddExample();
            producer.AddExample();

            producer.StopConsumer();
            producer.Dispose();
        }
    }
}
