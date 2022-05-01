using Hzdtf.Utility.Model.Return;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hzdtf.AMQP.Contract.Core
{
    /// <summary>
    /// 消费者接收处理
    /// @ 黄振东
    /// </summary>
    /// <typeparam name="ReceiveDataT">接收数据类型</typeparam>
    public interface IConsumerReceiveHandle<ReceiveDataT>
    {
        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="receiveData">接收数据</param>
        /// <returns>返回信息</returns>
        ReturnInfo<bool> Handle(ReceiveDataT receiveData);
    }

    /// <summary>
    /// 消费者接收处理
    /// @ 黄振东
    /// </summary>
    public interface IConsumerReceiveHandle : IConsumerReceiveHandle<object>
    {
    }
}
