using Hzdtf.Rabbit.Impl.Connection;
using Hzdtf.Utility.Attr;
using Hzdtf.Utility.ProcessCall;
using Hzdtf.Utility.Proxy;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Hzdtf.Rabbit.Impl.Core
{
    /// <summary>
    /// Rabbit RPC动态代理客户端
    /// @ 黄振东
    /// </summary>
    [Inject]
    public class RabbitRpcDispatchProxyClient : RpcDispatchProxyClient<RabbitRpcDispatchProxyClient>
    {
        /// <summary>
        /// 创建默认Rpc客户端方法
        /// </summary>
        /// <returns>Rpc客户端方法</returns>
        protected override IRpcClientMethod CreateRpcClientMethod() => new RabbitRpcClientMethod();
    }
}
