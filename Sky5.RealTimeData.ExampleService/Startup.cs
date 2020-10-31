using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Sky5.RealTimeData.ExampleService
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Calculator.Source>();
            // ÆôÓÃ¿çÓò
            services.AddCors(options => options.AddPolicy("debugCors",
                builder => builder
                    .WithOrigins("http://localhost:8080", "ws://192.168.1.105:8080")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()));
            
            services.AddSignalR().AddJsonProtocol(options => {
                options.PayloadSerializerOptions.Converters.Add(new NewtonsoftTextJsonConterter());
            });
            services.AddRealTimeManager<RealTimeHub>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors("debugCors");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
                endpoints.MapHub<RealTimeHub>("RealTimeData").RequireCors("debugCors");
                var manager = app.ApplicationServices.GetService<DataSourceManager>();
                foreach (var item in manager.Source)
                {
                    endpoints.MapGet(item.Url, item.HttpHandle);
                }
            });
            
        }
    }
}
