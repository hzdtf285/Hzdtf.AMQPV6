using Hzdtf.AMQP.Model.BusinessException;
using Hzdtf.Rabbit.Impl.Connection;
using System;

namespace Hzdtf.Rabbit.ExceptionHandleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var conn = new RabbitConnection();
            conn.OpenByHostId("exhost");
            var consumer = conn.CreateConsumer("ExChange", "ExQueue");

            Console.WriteLine("监听业务异常信息：");

            consumer.Subscribe((BusinessExceptionInfo msg) =>
            {
                Console.WriteLine(msg.ToString());

                return true;
            });

            Console.Read();
        }
    }
}
