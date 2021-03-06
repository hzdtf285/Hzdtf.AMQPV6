using Hzdtf.AMQP.Contract.Connection;
using Hzdtf.Consul.ConfigCenter.AspNet;
using Hzdtf.Rabbit.Consul.AspNet;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Rabbit Consul扩展类
    /// @ 黄振东
    /// </summary>
    public static class RabbitConsulExtensions
    {
        /// <summary>
        /// 添加Rabbit连接工厂到Consul配置中心
        /// </summary>
        /// <param name="services">服务收藏</param>
        /// <param name="options">配置回调</param>
        /// <returns>AMQP连接工厂</returns>
        public static IAmqpConnectionFactory AddRabbitConnectionFactoryConsulConfigCenter(this IServiceCollection services, Action<RabbitConsulConnectionOptions> options = null)
        {
            var config = new RabbitConsulConnectionOptions();
            if (options != null)
            {
                options(config);
            }

            return services.AddRabbitConnectionFactoryForConfigurate(op =>
            {
                op.Config = config.Config;
                op.SymmetricalEncryption = config.SymmetricalEncryption;                
            }, (builder, file, data) =>
            {
                if (string.IsNullOrWhiteSpace(file))
                {
                    return;
                }

                builder.AddConsulConfigCenter(config.ConsulConfigFile, options: op =>
                {
                    var key = ConfigCenterUtil.GetKeyPath(file, op.ServiceName);
                    op.Keys.Add(key);
                });
            });
        }

        /// <summary>
        /// 添加Rabbit连接到Consul配置中心
        /// </summary>
        /// <param name="services">服务收藏</param>
        /// <param name="hostId">主机ID</param>
        /// <param name="options">配置回调</param>
        /// <returns>AMQP连接</returns>
        public static IAmqpConnection AddRabbitConnectionAndOpenConsulConfigCenter(this IServiceCollection services, string hostId, Action<RabbitConsulConnectionOptions> options = null)
        {
            var config = new RabbitConsulConnectionOptions();
            if (options != null)
            {
                options(config);
            }

            return services.AddRabbitConnectionAndOpenForConfigurate(op =>
            {
                op.Config = config.Config;
                op.SymmetricalEncryption = config.SymmetricalEncryption;
                op.HostId = hostId;
            }, (builder, file, data) =>
            {
                if (string.IsNullOrWhiteSpace(file))
                {
                    return;
                }

                builder.AddConsulConfigCenter(config.ConsulConfigFile, options: op =>
                {
                    var key = ConfigCenterUtil.GetKeyPath(file, op.ServiceName);
                    op.Keys.Add(key);
                });
            });
        }
    }
}
