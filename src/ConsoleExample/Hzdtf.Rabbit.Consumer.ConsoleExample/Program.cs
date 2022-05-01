using Hzdtf.Rabbit.Impl.Connection;
using System;

namespace Hzdtf.Rabbit.Consumer.ConsoleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var conn = new RabbitConnection();
            conn.OpenByHostId("host1");
            Console.WriteLine("请输入要消费的队列名:");
            var quque = Console.ReadLine();
            var consumer = conn.CreateConsumer("TestExchange", quque);
            consumer.Subscribe((string msg) =>
            {
                Console.WriteLine("接收到数据：" + msg);

                //throw new Exception("测试业务处理异常"); //这里抛出异常，会自动发送到异常队列里

                return true;
            });

            Console.Read();
        }
    }
}
