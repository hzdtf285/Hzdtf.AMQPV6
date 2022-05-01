using Hzdtf.AMQP.Impl.Connection.Connection;
using Hzdtf.AMQP.Model.Connection;
using Hzdtf.Utility.Connection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Impl.Connection
{
    /// <summary>
    /// AMQP连接辅助类
    /// @ 黄振东
    /// </summary>
    public static class AmqpConnectionUtil
    {
        /// <summary>
        /// 默认连接字符串解析
        /// </summary>
        public readonly static IConnectionStringParse<AmqpConnectionInfo> DefaultConnectionStringParse = new AmqpConnectionStringParse();
    }
}
