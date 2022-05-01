using Hzdtf.AMQP.Contract.Config;
using Hzdtf.AMQP.Contract.Connection;
using Hzdtf.AMQP.Impl.Connection;
using Hzdtf.Utility.Attr;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.Rabbit.Impl.Connection
{
    /// <summary>
    /// Rabbit RPC客户端方法
    /// @ 黄振东
    /// </summary>
    [Inject]
    public class RabbitRpcClientMethod : AmqpRpcClientMethodBase
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public RabbitRpcClientMethod()
            : this(null, new RabbitConnectionFactory())
        {
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="amqpConfigReader">AMQP配置读取</param>
        /// <param name="connectionFactory">连接工厂</param>
        public RabbitRpcClientMethod(IAmqpConfigReader amqpConfigReader, IAmqpConnectionFactory connectionFactory)
            : base(amqpConfigReader, connectionFactory)
        {
        }
    }
}
