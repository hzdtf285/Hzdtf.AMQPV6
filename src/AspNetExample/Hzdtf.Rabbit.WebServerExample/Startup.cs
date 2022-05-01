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

            //var conn = services.AddRabbitConnectionAndOpen("host1");  // ��Ϊ�����Ӧ��ʹ�ô�ע�뷽��
            var conn = services.AddRabbitConnectionAndOpenConsulConfigCenter("host1");
            var consumer = conn.CreateConsumer("TestExchange", "TestQueue1"); // ��Ϊ�����߷�����Ҫ��������Ľ������Ͷ���
            consumer.Subscribe((string msg) =>
            {
                Console.WriteLine("���յ���Ϣ��" + msg);

                throw new Exception("���ݸ�ʽ����"); 

                return true; // �����Ƿ���ɹ������Ϊfalse��Ĭ�ϻ�Ѵ���Ϣת������һ����������
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
