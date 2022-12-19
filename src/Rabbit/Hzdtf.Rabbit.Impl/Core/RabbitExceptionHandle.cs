using Hzdtf.AMQP.Contract.Core;
using Hzdtf.AMQP.Model.BusinessException;
using Hzdtf.AMQP.Model.Config;
using Hzdtf.Logger.Contract;
using Hzdtf.Rabbit.Impl.Connection;
using Hzdtf.Utility.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Hzdtf.AMQP.Contract.Connection;
using Hzdtf.Utility.Data;
using System.Collections.Concurrent;

namespace Hzdtf.Rabbit.Impl.Core
{
    /// <summary>
    /// Rabbit异常处理
    /// @ 黄振东
    /// </summary>
    public class RabbitExceptionHandle : IExceptionHandle, IDisposable
    {
        /// <summary>
        /// AMQP队列信息
        /// </summary>
        private readonly AmqpQueueInfo amqpQueue;

        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogable log = null;

        /// <summary>
        /// 异常处理连接数组
        /// </summary>
        private IAmqpConnection[] exceptionHandleConnections;

        /// <summary>
        /// 异常处理生产者字典；key：生产者，value：路由键
        /// </summary>
        private IDictionary<IProducer, string> dicExceptionHandleProducers;

        /// <summary>
        /// 字节流序列化
        /// </summary>
        private readonly IBytesSerialization bytesSerialization;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="amqpQueue">AMQP队列信息</param>
        /// <param name="log">日志</param>
        /// <param name="bytesSerialization">字节流序列化</param>
        public RabbitExceptionHandle(AmqpQueueInfo amqpQueue, ILogable log = null, IBytesSerialization bytesSerialization = null)
        {
            if (amqpQueue == null)
            {
                throw new ArgumentNullException("AMQP队列信息不能为null");
            }
            this.amqpQueue = amqpQueue;

            if (log == null)
            {
                log = LogTool.DefaultLog;
            }
            else
            {
                this.log = log;
            }
            this.bytesSerialization = bytesSerialization;

            InitExceptionHandle();
        }

        /// <summary>
        /// 初始化异常处理
        /// </summary>
        private void InitExceptionHandle()
        {
            // 如果有定义异常处理
            if (amqpQueue.ExceptionHandle == null)
            {
                Console.WriteLine("InitExceptionHandle:amqpQueue.ExceptionHandle为null");
                return;
            }
            if (amqpQueue.ExceptionHandle.PublishConsumers.IsNullOrLength0())
            {
                Console.WriteLine("InitExceptionHandle:amqpQueue.ExceptionHandle.PublishConsumers.IsNullOrLength0()为空");
                return;
            }

            // 查找不重复的主机ID数组
            var hostIds = amqpQueue.ExceptionHandle.PublishConsumers.Select(p => p.HostId).Distinct().ToArray();
            exceptionHandleConnections = new RabbitConnection[hostIds.Length];
            for (var i = 0; i < hostIds.Length; i++)
            {
                exceptionHandleConnections[i] = new RabbitConnection();
                exceptionHandleConnections[i].OpenByHostId(hostIds[i]);
            }

            dicExceptionHandleProducers = new ConcurrentDictionary<IProducer, string>();
            foreach (var pc in amqpQueue.ExceptionHandle.PublishConsumers)
            {
                var conn = exceptionHandleConnections.Where(p => p.HostId == pc.HostId).FirstOrDefault();
                var producer = conn.CreateProducer(pc.Exchange);
                if (bytesSerialization != null)
                {
                    producer.BytesSerialization = bytesSerialization;
                }
                dicExceptionHandleProducers.Add(producer, pc.RoutingKey);
            }
        }

        /// <summary>
        /// 处理业务异常
        /// </summary>
        /// <param name="businessException">业务异常</param>
        /// <returns>是否处理成功</returns>
        public bool Handle(BusinessExceptionInfo businessException)
        {
            if (businessException == null)
            {
                Console.WriteLine("Handle:businessException为null");
                return false;
            }
            if (dicExceptionHandleProducers.IsNullOrCount0())
            {
                Console.WriteLine("Handle:dicExceptionHandleProducers.IsNullOrCount0()为空");
                return false;
            }

            foreach (var item in dicExceptionHandleProducers)
            {
                item.Key.Publish(businessException, item.Value);
            }

            return true;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            if (!dicExceptionHandleProducers.IsNullOrCount0())
            {
                foreach (var item in dicExceptionHandleProducers)
                {
                    item.Key.Close();
                }
            }

            if (!exceptionHandleConnections.IsNullOrLength0())
            {
                foreach (var item in exceptionHandleConnections)
                {
                    item.Close();
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// 析构方法
        /// </summary>
        ~RabbitExceptionHandle()
        {
            Close();
        }
    }
}
