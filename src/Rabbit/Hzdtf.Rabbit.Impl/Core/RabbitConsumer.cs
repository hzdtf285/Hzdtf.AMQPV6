using Hzdtf.AMQP.Contract.Core;
using Hzdtf.Utility.Data;
using Hzdtf.Utility.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Hzdtf.AMQP.Model.Config;
using System.Linq;
using Hzdtf.Logger.Contract;
using Hzdtf.AMQP.Impl;
using MessagePack;

namespace Hzdtf.Rabbit.Impl.Core
{
    /// <summary>
    /// Rabbit消费者
    /// @ 黄振东
    /// </summary>
    public class RabbitConsumer : RabbitCoreBase, IConsumer
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
        /// 异常处理
        /// </summary>
        private IExceptionHandle exceptionHandle;

        /// <summary>
        /// 异常处理
        /// </summary>
        private IExceptionHandle ExceptionHandle
        {
            get
            {
                if (exceptionHandle == null)
                {
                    exceptionHandle = new RabbitExceptionHandle(amqpQueue, log, BytesSerialization);
                }

                return exceptionHandle;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("异常处理不能为null");
                }
                exceptionHandle = value;
            }
        }

        /// <summary>
        /// JSON序列化
        /// </summary>
        private readonly static JsonBytesSerialization jsonSerialization = new JsonBytesSerialization();

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
        /// <param name="exceptionHandle">异常处理</param>
        public RabbitConsumer(IModel channel, AmqpQueueInfo amqpQueue, ILogable log = null, IExceptionHandle exceptionHandle = null)
            : base(channel, amqpQueue, true, log)
        {
            this.exceptionHandle = exceptionHandle;
        }

        #endregion

        #region IConsumer 接口

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="receiveMessageFun">接收消息回调</param>
        /// <param name="isAutoAck">是否自动应答，如果为否，则需要在回调里返回true</param>
        public void Subscribe(Func<string, bool> receiveMessageFun, bool isAutoAck = false)
        {
            if (receiveMessageFun == null)
            {
                return;
            }
            Subscribe((byte[] x) =>
            {
                string msg = null;
                try
                {
                    msg = Encoding.UTF8.GetString(x);
                }
                catch (Exception ex)
                {
                    string logMsg = $"{GetLogTitleMsg()}.输入参数isAutoAck:{isAutoAck},Encoding.UTF8.GetString发生异常,返回应答:true";
                    log.ErrorAsync(logMsg, ex, tags: GetLogTags());

                    return true;
                }

                try
                {
                    return receiveMessageFun(msg);
                }
                catch (Exception ex)
                {
                    var busEx = AmqpUtil.BuilderBusinessException(ex, msg, amqpQueue, log, ex.Message);
                    var isAck = ExceptionHandle.Handle(busEx);

                    string logMsg = $"{GetLogTitleMsg()}.输入参数isAutoAck:{isAutoAck},业务处理发生异常(返回应答为{isAck})";
                    log.ErrorAsync(logMsg, ex, tags: GetLogTags());

                    return isAck;
                }
            }, isAutoAck);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="T">接收类型</typeparam>
        /// <param name="receiveMessageFun">接收消息回调</param>
        /// <param name="isAutoAck">是否自动应答，如果为否，则需要在回调里返回true</param>
        public void Subscribe<T>(Func<T, bool> receiveMessageFun, bool isAutoAck = false)
        {
            if (receiveMessageFun == null)
            {
                return;
            }
            Subscribe((byte[] x) =>
            {
                T data = default(T);
                try
                {
                    data = BytesSerialization.Deserialize<T>(x);
                }
                catch (MessagePackSerializationException)
                {
                    // 如果messagepack反序列失败，则使用json反列化
                    try
                    {
                        data = jsonSerialization.Deserialize<T>(x);
                    }
                    catch (Exception ex)
                    {
                        string logMsg = $"{GetLogTitleMsg()}.输入参数isAutoAck:{isAutoAck},jsonSerialization.Deserialize发生异常(返回应答为true)，认为是不符合业务规范的数据，应删除消息";
                        log.ErrorAsync(logMsg, ex, tags: GetLogTags());

                        // 反序列异常则返回true
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    string logMsg = $"{GetLogTitleMsg()}.输入参数isAutoAck:{isAutoAck},BytesSerialization.Deserialize发生异常(返回应答为true)，认为是不符合业务规范的数据，应删除消息";
                    log.ErrorAsync(logMsg, ex, tags: GetLogTags());

                    // 反序列异常则返回true
                    return true;
                }

                try
                {
                    return receiveMessageFun(data);
                }
                catch (Exception ex)
                {
                    var busEx = AmqpUtil.BuilderBusinessException(ex, data, amqpQueue, log, ex.Message);
                    var isAck = ExceptionHandle.Handle(busEx);

                    string logMsg = $"{GetLogTitleMsg()}.输入参数isAutoAck:{isAutoAck},业务处理发生异常(返回应答为{isAck})";
                    log.ErrorAsync(logMsg, ex, tags: GetLogTags());

                    return isAck;
                }
            }, isAutoAck);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="receiveMessageFun">接收消息回调</param>
        /// <param name="receiveMessageType">接收消息类型</param>
        /// <param name="isAutoAck">是否自动应答，如果为否，则需要在回调里返回true</param>
        public void Subscribe(Func<object, bool> receiveMessageFun, Type receiveMessageType, bool isAutoAck = false)
        {
            if (receiveMessageFun == null)
            {
                return;
            }
            Subscribe((byte[] x) =>
            {
                object data = null;
                try
                {
                    data = BytesSerialization.Deserialize(x, receiveMessageType);
                }
                catch (MessagePackSerializationException)
                {
                    // 如果messagepack反序列失败，则使用json反列化
                    try
                    {
                        data = jsonSerialization.Deserialize(x, receiveMessageType);
                    }
                    catch (Exception ex)
                    {
                        string logMsg = $"{GetLogTitleMsg()}.输入参数isAutoAck:{isAutoAck},jsonSerialization.Deserialize发生异常(返回应答为true)，认为是不符合业务规范的数据，应删除消息";
                        log.ErrorAsync(logMsg, ex, tags: GetLogTags());

                        // 反序列异常则返回true
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    string logMsg = $"{GetLogTitleMsg()}.输入参数isAutoAck:{isAutoAck},BytesSerialization.Deserialize发生异常(返回应答为true)，认为是不符合业务规范的数据，应删除消息";
                    log.ErrorAsync(logMsg, ex, tags: GetLogTags());

                    // 反序列异常则返回true
                    return true;
                }

                try
                {
                    return receiveMessageFun(data);
                }
                catch (Exception ex)
                {
                    var busEx = AmqpUtil.BuilderBusinessException(ex, data, amqpQueue, log, ex.Message);
                    var isAck = ExceptionHandle.Handle(busEx);

                    string logMsg = $"{GetLogTitleMsg()}.输入参数isAutoAck:{isAutoAck},业务处理发生异常(返回应答为{isAck})";
                    log.ErrorAsync(logMsg, ex, tags: GetLogTags());

                    return isAck;
                }
            }, isAutoAck);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="receiveMessageFun">接收消息回调</param>
        /// <param name="isAutoAck">是否自动应答，如果为否，则需要在回调里返回true</param>
        public void Subscribe(Func<byte[], bool> receiveMessageFun, bool isAutoAck = false)
        {
            if (receiveMessageFun == null)
            {
                return;
            }
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (o, e) =>
            {
                if (e.Body.IsEmpty)
                {
                    // 如果自动回答，则什么都不用干
                    if (isAutoAck)
                    {
                        return;
                    }
                    channel.BasicAck(e.DeliveryTag, false);
                    return;
                }

                bool isAck = true;
                string logMsg = $"{GetLogTitleMsg()}.接收到消息";
                log.DebugAsync(logMsg, null, tags: GetLogTags());

                var data = e.Body.ToArray();
                try
                {
                    isAck = receiveMessageFun(data);
                }
                catch (Exception ex)
                {
                    var busEx = AmqpUtil.BuilderBusinessException(ex, $"字节数组数据转换为Base64:{Convert.ToBase64String(data)}", amqpQueue, log, ex.Message);
                    isAck = ExceptionHandle.Handle(busEx);

                    logMsg = $"{GetLogTitleMsg()}.输入参数isAutoAck:{isAutoAck},业务处理发生异常(返回应答为{isAck})";
                    log.ErrorAsync(logMsg, ex, tags: GetLogTags());
                }

                // 如果自动回答，则什么都不用干
                if (isAutoAck)
                {
                    return;
                }

                // 如果业务端返回应答，则返回MQ已成功处理，否则返回未处理成功，需要将该消息放回队列进行重试
                if (isAck)
                {
                    channel.BasicAck(e.DeliveryTag, false);
                }
                else
                {
                    channel.BasicNack(e.DeliveryTag, false, true);
                }
            };

            channel.BasicConsume(amqpQueue.Queue.Name, isAutoAck, consumer);
        }

        #endregion
        
        /// <summary>
        /// 设置对象
        /// </summary>
        /// <param name="obj">对象</param>
        public void Set(IExceptionHandle obj)
        {
            this.exceptionHandle = obj;
        }

        #region 重写父类的方法

        /// <summary>
        /// 初始化
        /// </summary>
        protected override void Init()
        {
            channel.QueueDeclare(amqpQueue.Queue.Name, amqpQueue.Persistent, false, amqpQueue.Queue.AutoDelQueue, null);
            if (amqpQueue.Queue.RoutingKeys.IsNullOrLength0())
            {
                return;
            }

            foreach (string key in amqpQueue.Queue.RoutingKeys)
            {
                channel.QueueBind(amqpQueue.Queue.Name, amqpQueue.ExchangeName, key);
            }
        }

        #endregion
    }
}
