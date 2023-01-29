using Hzdtf.AMQP.Contract.Core;
using Hzdtf.Utility.Data;
using Hzdtf.Utility.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using Hzdtf.AMQP.Model.Config;
using Hzdtf.Logger.Contract;
using System.Collections.Concurrent;
using System.Linq;
using Hzdtf.Utility.Factory;
using Hzdtf.AMQP.Model.Core;

namespace Hzdtf.Rabbit.Impl.Core
{
    /// <summary>
    /// Rabbit生产者
    /// @ 黄振东
    /// </summary>
    public class RabbitProducer : RabbitCoreBase, IProducer
    {
        #region 属性与字段

        /// <summary>
        /// 字节数组序列化，默认为JSON序列化
        /// </summary>
        public IBytesSerialization BytesSerialization
        {
            get;
            set;
        } = new JsonBytesSerialization();

        /// <summary>
        /// 发布延迟消费队列列表
        /// </summary>
        private readonly static ConcurrentBag<string> dealyQueues = new ConcurrentBag<string>();

        /// <summary>
        /// 发布状态回复事件
        /// </summary>
        public event EventHandler<PublishStatusEventArgs> PublishStatusCall;

        /// <summary>
        /// 是否已设置确认
        /// </summary>
        private bool isSetConfirmed = false;

        /// <summary>
        /// 消息序列号映射发布状态字典
        /// key：消息序列号
        /// value：发布状态
        /// </summary>
        private readonly IDictionary<ulong, PublishStatusEventArgs> dicMsgSeqNoMapPublishStauts = new ConcurrentDictionary<ulong, PublishStatusEventArgs>();

        #endregion

        #region 构造方法

        /// <summary>
        /// 构造方法
        /// 初始化各个对象以便就绪
        /// 只初始化交换机与基本属性，队列定义请重写Init方法进行操作
        /// </summary>
        /// <param name="channel">渠道</param>
        /// <param name="amqpQueue">AMQP队列信息</param>
        /// <param name="log">日志</param>
        /// <param name="modelFactory">模型工厂</param>
        public RabbitProducer(IModel channel, AmqpQueueInfo amqpQueue, ILogable log = null, IGeneralFactory<IModel> modelFactory = null)
            : base(channel, amqpQueue, false, log, modelFactory)
        {
        }

        #endregion

        #region IProducer 接口

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="routingKey">路由键</param>
        public void Publish(string message, string routingKey = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            Publish(Encoding.UTF8.GetBytes(message), message, routingKey);
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="routingKey">路由键</param>
        public void Publish(object message, string routingKey = null)
        {
            if (message == null)
            {
                return;
            }

            Publish(BytesSerialization.Serialize(message), message, routingKey);
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="routingKey">路由键</param>
        public void Publish(byte[] message, string routingKey = null)
        {
            Publish(BytesSerialization.Serialize(message), message, routingKey);
        }

        /// <summary>
        /// 发布消息，延迟消费
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="ttl">过期毫秒数</param>
        /// <param name="routingKey">路由键</param>
        public void PublishDelay(string message, uint ttl, string routingKey = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            PublishDelay(Encoding.UTF8.GetBytes(message), message, ttl, routingKey);
        }

        /// <summary>
        /// 发布消息，延迟消费
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="ttl">过期毫秒数</param>
        /// <param name="routingKey">路由键</param>
        public void PublishDelay(object message, uint ttl, string routingKey = null)
        {
            if (message == null)
            {
                return;
            }

            PublishDelay(BytesSerialization.Serialize(message), message, ttl, routingKey);
        }

        /// <summary>
        /// 发布消息，延迟消费
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="ttl">过期毫秒数</param>
        /// <param name="routingKey">路由键</param>
        public void PublishDelay(byte[] message, uint ttl, string routingKey = null)
        {
            PublishDelay(message, message, ttl, routingKey);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="origMessage">原始消息</param>
        /// <param name="routingKey">路由键</param>
        private void Publish(byte[] message, object origMessage, string routingKey = null)
        {
            if (message.IsNullOrLength0())
            {
                return;
            }

            string logMsg = string.Format("给路由键:{0},交换机:{1} 发送消息", routingKey, amqpQueue.ExchangeName);
            log.DebugAsync(logMsg, null, typeof(RabbitProducer).Name, amqpQueue.ExchangeName, routingKey);

            SetChannelConfirm();
            SetMsgSeqMapPublishStatus(() =>
            {
                return new PublishStatusEventArgs()
                {
                    Message = origMessage,
                    RoutingKey = routingKey
                };
            });
            channel.BasicPublish(amqpQueue.ExchangeName, routingKey, basicProperties, message);
        }

        /// <summary>
        /// 发布消息，延迟消费
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="origMessage">原始消息</param>
        /// <param name="ttl">过期毫秒数</param>
        /// <param name="routingKey">路由键</param>
        private void PublishDelay(byte[] message, object origMessage, uint ttl, string routingKey = null)
        {
            if (message.IsNullOrLength0())
            {
                return;
            }

            var queue = string.Format("{0}_{1}", amqpQueue.ExchangeName, routingKey);
            if (!dealyQueues.Contains(queue))
            {
                var dict = new Dictionary<string, object>()
                {
                    { "x-dead-letter-exchange", amqpQueue.ExchangeName },//过期消息转向路由
                    { "x-dead-letter-routing-key", routingKey }//过期消息转向路由的routing key
                };
                //声明队列
                channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: dict);
                if (!dealyQueues.Contains(queue))
                {
                    dealyQueues.Add(queue);
                }
            }

            var baProp = CreateDefaultBasicProperties();
            baProp.Expiration = ttl.ToString();
            string logMsg = string.Format("给路由键:{0},交换机:{1} 发送消息", routingKey, amqpQueue.ExchangeName);
            log.DebugAsync(logMsg, null, typeof(RabbitProducer).Name, amqpQueue.ExchangeName, routingKey);

            SetChannelConfirm();
            SetMsgSeqMapPublishStatus(() =>
            {
                return new PublishStatusEventArgs()
                {
                    Message = origMessage,
                    RoutingKey = routingKey
                };
            });
            channel.BasicPublish(exchange: string.Empty, routingKey: queue, basicProperties: baProp, body: message);
        }

        /// <summary>
        /// 执行发布状态回调
        /// </summary>
        /// <param name="e">发布状态事件参数</param>
        private void OnPublishStatusCall(PublishStatusEventArgs e)
        {
            if (PublishStatusCall != null)
            {
                PublishStatusCall(this, e);
            }
        }

        /// <summary>
        /// 设置渠道确认
        /// </summary>
        private void SetChannelConfirm()
        {
            if (isSetConfirmed || PublishStatusCall == null)
            {
                return;
            }

            channel.ConfirmSelect();
            channel.BasicAcks += Channel_BasicAcks;
            channel.BasicNacks += Channel_BasicNacks;
            isSetConfirmed = true;
        }

        /// <summary>
        /// 渠道ack
        /// </summary>
        /// <param name="sender">引发对象</param>
        /// <param name="e">基本ack事件参数</param>
        private void Channel_BasicAcks(object sender, RabbitMQ.Client.Events.BasicAckEventArgs e)
        {
            ExecPublishStatus(e.DeliveryTag, true);
        }

        /// <summary>
        /// 渠道nack
        /// </summary>
        /// <param name="sender">引发对象</param>
        /// <param name="e">基本nack事件参数</param>
        private void Channel_BasicNacks(object sender, RabbitMQ.Client.Events.BasicNackEventArgs e)
        {
            ExecPublishStatus(e.DeliveryTag, false);
        }

        /// <summary>
        /// 执行发布状态
        /// </summary>
        /// <param name="deliveryTag">序列</param>
        /// <param name="success">是否成功</param>
        private void ExecPublishStatus(ulong deliveryTag, bool success)
        {
            if (dicMsgSeqNoMapPublishStauts.ContainsKey(deliveryTag))
            {
                var publishStatus = dicMsgSeqNoMapPublishStauts[deliveryTag];
                publishStatus.Success = success;
                OnPublishStatusCall(publishStatus);
                dicMsgSeqNoMapPublishStauts.RemoveKey(deliveryTag);
            }
        }

        /// <summary>
        /// 设置消息映射发布状态
        /// </summary>
        /// <param name="callbackPublishStatusEvent">回调消息映射发布状态</param>
        private void SetMsgSeqMapPublishStatus(Func<PublishStatusEventArgs> callbackPublishStatusEvent)
        {
            if (PublishStatusCall == null || callbackPublishStatusEvent == null)
            {
                return;
            }

            var e = callbackPublishStatusEvent();
            e.MessageSeqNo = channel.NextPublishSeqNo;
            dicMsgSeqNoMapPublishStauts.Add(e.MessageSeqNo, e);
        }

        #endregion
    }
}
