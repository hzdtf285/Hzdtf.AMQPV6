using Hzdtf.AMQP.Model.BusinessException;
using Hzdtf.Utility.Release;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hzdtf.AMQP.Contract.Core
{
    /// <summary>
    /// 异常处理接口
    /// @ 黄振东
    /// </summary>
    public interface IExceptionHandle : IClose
    {
        /// <summary>
        /// 处理业务异常
        /// </summary>
        /// <param name="businessException">业务异常</param>
        /// <returns>是否处理成功</returns>
        bool Handle(BusinessExceptionInfo businessException);
    }
}
