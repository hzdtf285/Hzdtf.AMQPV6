﻿using Hzdtf.AMQP.Contract.Config;
using Hzdtf.AMQP.Impl.Config;
using Hzdtf.AMQP.Model.BusinessException;
using Hzdtf.AMQP.Model.Config;
using Hzdtf.Logger.Contract;
using Hzdtf.Utility;
using Hzdtf.Utility.Model.Identitys;
using Hzdtf.Utility.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Impl
{
    /// <summary>
    /// AMQP辅助类
    /// @ 黄振东
    /// </summary>
    public static class AmqpUtil
    {
        /// <summary>
        /// 全局配置读取，默认为AmqpConfigCache
        /// </summary>
        public static IAmqpConfigReader GlobalConfigReader = new AmqpConfigCache();

        /// <summary>
        /// 雪法算法ID
        /// </summary>
        private static readonly IIdentity<long> snowflakeId = new SnowflakeId();

        /// <summary>
        /// 获取配置读取，如果传入为空，则取全局配置读取
        /// </summary>
        /// <param name="configReader">配置读取</param>
        /// <returns>配置读取</returns>
        public static IAmqpConfigReader GetConfigReader(IAmqpConfigReader configReader = null)
        {
            return configReader == null ? GlobalConfigReader : configReader;
        }

        /// <summary>
        /// 生成业务异常信息
        /// </summary>
        /// <param name="ex">异常</param>
        /// <param name="queueMessage">队列消息</param>
        /// <param name="amqpQueue">AMQP队列信息</param>
        /// <param name="log">日志</param>
        /// <param name="desc">描述</param>
        /// <returns>业务异常信息</returns>
        public static BusinessExceptionInfo BuilderBusinessException(Exception ex, object queueMessage, AmqpQueueInfo amqpQueue, ILogable log, string desc = null)
        {
            if (log == null)
            {
                throw new ArgumentNullException("日志不能为null");
            }
            string queueMessageJson = null;
            if (queueMessage != null)
            {
                try
                {
                    queueMessageJson = queueMessage.ToJsonString();
                }
                catch (Exception ex1)
                {
                    log.ErrorAsync("JSON序列化业务异常信息出错", ex1, typeof(AmqpUtil).Name);

                    return null;
                }
            }

            var busEx = new BusinessExceptionInfo()
            {
                ExId = snowflakeId.New(),
                HostId = amqpQueue.HostId,
                Time = DateTimeExtensions.Now,
                ServiceName = string.IsNullOrWhiteSpace(amqpQueue.ExceptionHandle.ServiceName) ? App.AppServiceName : amqpQueue.ExceptionHandle.ServiceName,
                ExceptionString = ex.ToString(),
                ExceptionMessage = ex.Message,
                Exchange = amqpQueue.ExchangeName,
                Queue = amqpQueue.Queue.Name,
                QueueMessageJsonString = queueMessageJson,
                Desc = desc,
                ServerMachineName = Environment.MachineName,
                ServerIP = NetworkUtil.LocalIP
            };

            return busEx;
        }
    }
}
