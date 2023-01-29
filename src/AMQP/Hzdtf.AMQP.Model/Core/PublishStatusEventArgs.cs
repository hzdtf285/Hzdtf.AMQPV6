using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hzdtf.AMQP.Model.Core
{
    /// <summary>
    /// 发布状态事件参数
    /// @ 黄振东
    /// </summary>
    public class PublishStatusEventArgs : EventArgs
    {
        /// <summary>
        /// 消息序列号
        /// </summary>
        public ulong MessageSeqNo
        {
            get;
            set;
        }

        /// <summary>
        /// 消息
        /// </summary>
        public object Message
        {
            get;
            set;
        }

        /// <summary>
        /// 路由键
        /// </summary>
        public string RoutingKey
        {
            get;
            set;
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success
        {
            get;
            set;
        }
    }
}
