using Hzdtf.AMQP.Contract.Connection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hzdtf.Rabbit.WebClientExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        private readonly IAmqpConnectionFactory factory;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IAmqpConnectionFactory factory)
        {
            _logger = logger;
            this.factory = factory;
        }

        [HttpGet]
        public void Get()
        {
            using (var conn = factory.Create()) // 此处是利用工厂创建连接，为了演示所以简例。正式做法是，应该把连接缓存起来
            {
                conn.OpenByHostId("host1");
                using (var producer = conn.CreateProducer("TestExchange")) // 输入要发布目的交换机
                {
                    producer.Publish("这是一个测试数据", "TestKey1");
                }
            }
        }
    }
}
