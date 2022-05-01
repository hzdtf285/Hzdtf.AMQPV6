using Hzdtf.AMQP.Contract.Config;
using Hzdtf.AMQP.Contract.Connection;
using Hzdtf.AMQP.Impl;
using Hzdtf.AMQP.Impl.Connection;
using Hzdtf.Utility.Attr;
using Hzdtf.Utility.Safety;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.Rabbit.Impl.Connection
{
    /// <summary>
    /// Rabbit连接工厂
    /// @ 黄振东
    /// </summary>
    [Inject]
    public class RabbitConnectionFactory : AmqpConnectionFactoryBase
    {
        /// <summary>
        /// AMQP配置读取
        /// </summary>
        private readonly IAmqpConfigReader amqpConfigReader;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="symmetricalEncryption">加密</param>
        /// <param name="config">配置</param>
        /// <param name="amqpConfigReader">AMQP配置读取</param>
        public RabbitConnectionFactory(ISymmetricalEncryption symmetricalEncryption = null, IConfiguration config = null, IAmqpConfigReader amqpConfigReader = null)
            : base(symmetricalEncryption, config)
        {
            this.amqpConfigReader = AmqpUtil.GetConfigReader(amqpConfigReader);
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <returns>连接</returns>
        public override IAmqpConnection Create() => new RabbitConnection(amqpConfigReader, symmetricalEncryption);
    }
}
