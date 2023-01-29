using Hzdtf.AMQP.Model.Core;
using Hzdtf.Utility.Data;
using Hzdtf.Utility.Release;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Contract.Core
{
    /// <summary>
    /// 生产者接口
    /// @ 黄振东
    /// </summary>
    public interface IProducer : ICloseable, IDisposable
    {
        /// <summary>
        /// 字节数组序列化
        /// </summary>
        IBytesSerialization BytesSerialization
        {
            get;
            set;
        }

        /// <summary>
        /// 发布状态回复事件
        /// </summary>
        event EventHandler<PublishStatusEventArgs> PublishStatusCall;

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="routingKey">路由键</param>
        void Publish(string message, string routingKey = null);

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="routingKey">路由键</param>
        void Publish(object message, string routingKey = null);

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="routingKey">路由键</param>
        void Publish(byte[] message, string routingKey = null);

        /// <summary>
        /// 发布消息，延迟消费
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="ttl">过期毫秒数</param>
        /// <param name="routingKey">路由键</param>
        void PublishDelay(string message, uint ttl, string routingKey = null);

        /// <summary>
        /// 发布消息，延迟消费
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="ttl">过期毫秒数</param>
        /// <param name="routingKey">路由键</param>
        void PublishDelay(object message, uint ttl, string routingKey = null);

        /// <summary>
        /// 发布消息，延迟消费
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="ttl">过期毫秒数</param>
        /// <param name="routingKey">路由键</param>
        void PublishDelay(byte[] message, uint ttl, string routingKey = null);
    }
}
