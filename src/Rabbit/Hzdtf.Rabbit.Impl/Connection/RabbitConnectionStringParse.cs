using Hzdtf.AMQP.Impl.Connection.Connection;
using Hzdtf.AMQP.Model.Connection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.Rabbit.Impl.Connection
{
    /// <summary>
    /// Rabbit连接字符串解析
    /// @ 黄振东
    /// </summary>
    public class RabbitConnectionStringParse : AmqpConnectionStringParse
    {
        /// <summary>
        /// 设置默认值
        /// </summary>
        /// <param name="connectionInfo">连接信息</param>
        protected override void SetDefaultValue(AmqpConnectionInfo connectionInfo)
        {
            base.SetDefaultValue(connectionInfo);

            if (connectionInfo.Port == 0)
            {
                connectionInfo.Port = RabbitConnectionUtil.DEFAULT_PORT;
            }
        }
    }
}
