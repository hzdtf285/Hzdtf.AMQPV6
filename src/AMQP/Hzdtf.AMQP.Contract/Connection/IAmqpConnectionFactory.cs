using Hzdtf.AMQP.Model.Connection;
using Hzdtf.Utility.Connection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Contract.Connection
{
    /// <summary>
    /// AMQP连接工厂接口
    /// @ 黄振东
    /// </summary>
    public interface IAmqpConnectionFactory : IConnectionFactory<IAmqpConnection, AmqpConnectionInfo, AmqpConnectionWrapInfo>
    {
    }
}
