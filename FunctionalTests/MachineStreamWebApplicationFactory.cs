using MachineStreamCore.Configuration;
using MachineStreamCore.Interfaces;
using MachineStreamCore.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using System;

namespace FunctionalTests
{
    public class MachineStreamWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        public MachineStreamWebApplicationFactory()
        {
            //set the enviroment variable in case we want to app/remove settings based on the enviroment
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return base.CreateWebHostBuilder().ConfigureServices(services =>
            {
                services.AddMvc()
                    .AddJsonOptions(options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore)
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
                //the only option needed since the mongo client is instanciated in the test
                services.Configure<MongoDBConfiguration>(x =>
                {
                    x.DatabaseName = "TestMachine";
                    x.EventCollection = "TestEvents";
                });
                //this services are the same but the mongo client is inject in the test constructor
                services.AddScoped<IMongo, MongoService>();
                services.AddScoped<IStoreData, StoreDataService>();
                services.AddScoped<IGetData, GetDataService>();
                //configure the fake background services
                services.AddScoped<IWebSocket, WebSocketServiceFake>();
                services.AddHostedService<WebSocketHostingServiceFake>();
            })
            .Configure(app =>
            {
                var loggingFactory = new LoggerFactory();
                //add serilog to logging pipeline
                loggingFactory.AddSerilog();
                //register the base routing of the application
                app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "v1/{controller=Home}/{action=Index}/{id?}");
                });
            });
        }
    }
}
