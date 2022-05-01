using Hzdtf.Utility.Data;
using Hzdtf.Utility.Model.Return;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hzdtf.AMQP.Model.Config
{
    /// <summary>
    /// 消费者选项信息
    /// @ 黄振东
    /// </summary>
    [MessagePackObject]
    public class ConsumerOptionsInfo
    {
        /// <summary>
        /// 交换机
        /// </summary>
        [JsonProperty("exchange")]
        [Key("exchange")]
        public string Exchange
        {
            get;
            set;
        }

        /// <summary>
        /// 队列
        /// </summary>
        [JsonProperty("queue")]
        [Key("queue")]
        public string Queue
        {
            get;
            set;
        }

        /// <summary>
        /// 是否自动应答，默认为否
        /// </summary>
        [JsonProperty("autoack")]
        [Key("autoack")]
        public bool Autoack
        {
            get;
            set;
        }

        /// <summary>
        /// 接收到消息回调
        /// </summary>
        [JsonIgnore, IgnoreMember]
        public Func<IServiceProvider, object, BasicReturnInfo> ReceivedCallback
        {
            get;
            set;
        }

        /// <summary>
        /// 接收消息类型
        /// </summary>
        [JsonIgnore, IgnoreMember]
        public Type ReceiveMessageType
        {
            get;
            set;
        }

        /// <summary>
        /// 字节数组序列化
        /// </summary>
        [JsonIgnore, IgnoreMember]
        public IBytesSerialization BytesSerialization
        {
            get;
            set;
        }

        /// <summary>
        /// 返回错误处理类型，默认为抛出业务异常
        /// </summary>
        [JsonProperty("returnErrHandleType")]
        [Key("returnErrHandleType")]
        public ReceiveReturnErrHandleType ReturnErrHandleType
        {
            get;
            set;
        } = ReceiveReturnErrHandleType.ThrowBusinessException;
    }

    /// <summary>
    /// 消费者选项配置
    /// @ 黄振东
    /// </summary>
    public class ConsumerOptions
    {
        /// <summary>
        /// 消费者配置数组
        /// </summary>
        public ConsumerOptionsInfo[] ConsumerConfigs
        {
            get;
            set;
        }

        /// <summary>
        /// 默认字节数组序列化
        /// </summary>
        public IBytesSerialization DefaultBytesSerialization
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 接收返回错误方式枚举
    /// @ 黄振东
    /// </summary>
    public enum ReceiveReturnErrHandleType : byte
    {
        /// <summary>
        /// 抛出业务异常
        /// </summary>
        ThrowBusinessException = 0,

        /// <summary>
        /// 返回true
        /// </summary>
        ReturnTrue = 1,

        /// <summary>
        /// 返回false
        /// </summary>
        ReturnFalse = 2,
    }
}
