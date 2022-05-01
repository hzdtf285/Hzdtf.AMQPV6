using Hzdtf.AMQP.Model.Config;
using Hzdtf.Utility.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Contract.Config
{
    /// <summary>
    /// AMQP配置读取接口
    /// @ 黄振东
    /// </summary>
    public interface IAmqpConfigReader : IReader<AmqpConfigInfo>
    {
    }
}
