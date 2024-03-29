﻿using Hzdtf.Utility.Data;
using Hzdtf.Utility.Release;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Contract.Core
{
    /// <summary>
    /// 消费者接口
    /// @ 黄振东
    /// </summary>
    public interface IConsumer : ICloseable, IDisposable, ISetObject<IExceptionHandle>
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
        /// 订阅消息
        /// </summary>
        /// <param name="receiveMessageFun">接收消息回调</param>
        /// <param name="isAutoAck">是否自动应答，如果为否，则需要在回调里返回true</param>
        void Subscribe(Func<string, bool> receiveMessageFun, bool isAutoAck = false);

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="T">接收类型</typeparam>
        /// <param name="receiveMessageFun">接收消息回调</param>
        /// <param name="isAutoAck">是否自动应答，如果为否，则需要在回调里返回true</param>
        void Subscribe<T>(Func<T, bool> receiveMessageFun, bool isAutoAck = false);

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="receiveMessageFun">接收消息回调</param>
        /// <param name="receiveMessageType">接收消息类型</param>
        /// <param name="isAutoAck">是否自动应答，如果为否，则需要在回调里返回true</param>
        void Subscribe(Func<object, bool> receiveMessageFun, Type receiveMessageType, bool isAutoAck = false);

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="receiveMessageFun">接收消息回调</param>
        /// <param name="isAutoAck">是否自动应答，如果为否，则需要在回调里返回true</param>
        void Subscribe(Func<byte[], bool> receiveMessageFun, bool isAutoAck = false);
    }
}
