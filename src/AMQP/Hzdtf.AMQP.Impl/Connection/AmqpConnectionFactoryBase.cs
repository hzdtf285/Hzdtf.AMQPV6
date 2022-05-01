using Hzdtf.AMQP.Contract.Connection;
using Hzdtf.AMQP.Model.Connection;
using Hzdtf.Utility.Connection;
using Hzdtf.Utility.Safety;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Impl.Connection
{
    /// <summary>
    /// AMQP连接工厂基类
    /// @ 黄振东
    /// </summary>
    public abstract class AmqpConnectionFactoryBase : ConnectionConfigFactoryBase<IAmqpConnection, AmqpConnectionInfo, AmqpConnectionWrapInfo>, IAmqpConnectionFactory
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="symmetricalEncryption">加密</param>
        /// <param name="config">配置</param>
        public AmqpConnectionFactoryBase(ISymmetricalEncryption symmetricalEncryption = null, IConfiguration config = null)
            : base(symmetricalEncryption, config)
        {
        }

        /// <summary>
        /// 创建并打开
        /// </summary>
        /// <param name="connectionWrap">连接包装信息</param>
        /// <returns>连接</returns>
        public override IAmqpConnection CreateAndOpen(AmqpConnectionWrapInfo connectionWrap = null)
        {
            if (string.IsNullOrWhiteSpace(connectionWrap.HostId))
            {
                return base.CreateAndOpen(connectionWrap);
            }
            else
            {
                var conn = Create();
                conn.OpenByHostId(connectionWrap.HostId);

                return conn;
            }
        }
    }
}
