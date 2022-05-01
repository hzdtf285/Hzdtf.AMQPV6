using Hzdtf.AMQP.Contract.Connection;
using Hzdtf.AMQP.Contract.Core;
using Hzdtf.AMQP.Model.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hzdtf.Utility.Utils;
using Hzdtf.Utility.Model;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// 消费者扩展类
    /// @ 黄振东
    /// </summary>
    public static class ConsumerExtensions
    {
        /// <summary>
        /// 启动监听消费集合
        /// </summary>
        /// <param name="lifetime">主机应用生命周期</param>
        /// <param name="provider">服务提供者</param>
        /// <param name="conn">AMQP连接</param>
        /// <param name="options">配置</param>
        /// <returns>主机应用生命周期</returns>
        public static IHostApplicationLifetime ListenConsumers(this IHostApplicationLifetime lifetime, IServiceProvider provider, IAmqpConnection conn, Action<ConsumerOptions> options)
        {
            if (options == null)
            {
                throw new NullReferenceException("配置不能为null");
            }
            
            var config = new ConsumerOptions();
            options(config);

            if (config.ConsumerConfigs.IsNullOrLength0())
            {
                throw new ArgumentException("消费者配置数组不能为空");
            }

            var consumers = new List<IConsumer>(config.ConsumerConfigs.Length);
            lifetime.ApplicationStarted.Register(() =>
            {
                foreach (var con in config.ConsumerConfigs)
                {
                    var title = $"交换机:{con.Exchange},队列:{con.Queue}";
                    if (con.ReceiveMessageType == null)
                    {
                        throw new NullReferenceException($"[{title}]接收消息类型不能为null");
                    }

                    var consumer = conn.CreateConsumer(con.Exchange, con.Queue); // 作为消费者服务，需要输入监听的交换机和队列
                    consumers.Add(consumer);
                    if (con.BytesSerialization != null)
                    {
                        consumer.BytesSerialization = con.BytesSerialization;
                    }
                    else if (config.DefaultBytesSerialization != null)
                    {
                        consumer.BytesSerialization = config.DefaultBytesSerialization;
                    }

                    Console.WriteLine($"{title}开启监听...");

                    if (con.ReceiveMessageType == typeof(string)) // 接收字符串
                    {
                        consumer.Subscribe(receiveMessageFun: (string msg) =>
                        {
                            return HandleMessage(con, provider, msg);
                        }, isAutoAck: con.Autoack);
                    }
                    else if (con.ReceiveMessageType == typeof(byte[])) // 接收字节数组
                    {
                        consumer.Subscribe(receiveMessageFun: (byte[] msg) =>
                        {
                            return HandleMessage(con, provider, msg);
                        }, isAutoAck: con.Autoack);
                    }
                    else
                    {
                        consumer.Subscribe(receiveMessageFun: (object msg) => // 其他统一接收对象
                        {
                            return HandleMessage(con, provider, msg);
                        }, receiveMessageType: con.ReceiveMessageType, isAutoAck: con.Autoack);
                    }
                }
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                for (var i = 0; i < config.ConsumerConfigs.Length; i++)
                {
                    var title = $"交换机:{config.ConsumerConfigs[i].Exchange},队列:{config.ConsumerConfigs[i].Queue}";
                    Console.WriteLine($"[{title}]停止监听");
                    consumers[i].Close();
                    consumers[i].Dispose();
                }

                Console.WriteLine("AMQP停止连接");
                conn.Close();
                conn.Dispose();
            });

            return lifetime;
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="con">消费者选项配置</param>
        /// <param name="provider">服务提供者</param>
        /// <param name="msg">消息</param>
        /// <returns>返回数据</returns>
        private static bool HandleMessage(ConsumerOptionsInfo con, IServiceProvider provider, object msg)
        {
            var re = con.ReceivedCallback(provider, msg);
            if (re == null)
            {
                return true;
            }
            if (re.Failure())
            {
                switch (con.ReturnErrHandleType)
                {
                    case ReceiveReturnErrHandleType.ThrowBusinessException:

                        throw new BusinessException(re.Code, re.Msg, re.Desc);

                    case ReceiveReturnErrHandleType.ReturnFalse:

                        return false;

                    case ReceiveReturnErrHandleType.ReturnTrue:

                        return true;

                    default:

                        return false;
                }
            }

            return true; // 返回是否处理成功；如果为false，默认会把此消息转发到下一个消费者上
        }
    }
}
