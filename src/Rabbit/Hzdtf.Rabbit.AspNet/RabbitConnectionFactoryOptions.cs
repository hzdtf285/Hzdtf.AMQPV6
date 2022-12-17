using Hzdtf.AMQP.Contract.Config;
using Hzdtf.Utility;
using Hzdtf.Utility.Safety;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.Rabbit.AspNet
{
    /// <summary>
    /// Rabbit连接工厂配置选项
    /// @ 黄振东
    /// </summary>
    public class RabbitConnectionFactoryOptions
    {
        /// <summary>
        /// 加密
        /// </summary>
        public ISymmetricalEncryption SymmetricalEncryption
        {
            get;
            set;
        } = SymmetricalEncryptionUtil.GetSymmetricalEncryption();

        /// <summary>
        /// 配置
        /// </summary>
        public IConfiguration Config
        {
            get;
            set;
        } = App.CurrConfig;
    }

    /// <summary>
    /// Rabbit连接工厂自定义选项配置
    /// @ 黄振东
    /// </summary>
    public class RabbitConnectionFactoryCustomerOptions : RabbitConnectionFactoryOptions
    {
        /// <summary>
        /// 配置读取
        /// </summary>
        public IAmqpConfigReader ConfigReader
        {
            get;
            set;
        }
    }
}
