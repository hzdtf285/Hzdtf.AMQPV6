using Hzdtf.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hzdtf.Rabbit.WebServerExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            App.CurrConfig = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hzdtf.Rabbit.WebServerExample", Version = "v1" });
            });

            //var conn = services.AddRabbitConnectionAndOpen("host1");  // 作为服务端应该使用此注入方法
            var conn = services.AddRabbitConnectionAndOpenConsulConfigCenter("host1");
            var consumer = conn.CreateConsumer("TestExchange", "TestQueue1"); // 作为消费者服务，需要输入监听的交换机和队列
            consumer.Subscribe((string msg) =>
            {
                Console.WriteLine("接收到消息：" + msg);

                throw new Exception("数据格式不对"); 

                return true; // 返回是否处理成功；如果为false，默认会把此消息转发到下一个消费者上
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hzdtf.Rabbit.WebServerExample v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
