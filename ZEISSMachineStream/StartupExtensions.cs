using MachineStreamCore.Configuration;
using MachineStreamCore.Interfaces;
using MachineStreamCore.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace ZEISSMachineStream
{
    /// <summary>
    /// Utility class to make more readable the code in the Startup class
    /// </summary>
    public static class StartupExtensions
    {
        /// <summary>
        /// Add and define the CORS policies to implement based on the appsettings configuration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddCorsPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            //add the cors policy using the configuration
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder
                        .WithOrigins(configuration["AllowedHosts"])
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                    });
            });
        }

        /// <summary>
        /// Add and map all the configuration section of the appsettings file
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            //map the appsettings section with the POCO classes
            services.Configure<WebSocketConfiguration>(configuration.GetSection("WebSocket"));
            services.Configure<MongoDBConfiguration>(configuration.GetSection("MongoDB"));
        }

        /// <summary>
        /// Helper method to add and configure the DI to all the services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddDI(this IServiceCollection services, IConfiguration configuration)
        {
            //register the mongo client as singleton
            services.AddSingleton(typeof(IMongoClient), MongoSettings.GetMongoClient(configuration.GetSection("MongoDB:ClientSettings").Get<ClientSettingsConfiguration>()));

            services.AddScoped<IMongo, MongoService>();
            //Prefer to split the store and the fetch of the data since at the moment
            //I use a mongo client to perform both the operation but in the future we could use
            //other tools to ingest or get data from
            //(Es. ingestion with Azure EventHub/Kafka/Amazon Kinesis and get on Azure Data Explorer/Elastic/Hadoop etc...)
            services.AddScoped<IStoreData, StoreDataService>();
            services.AddScoped<IGetData, GetDataService>();

            //add the service used and consumed by the background task
            services.AddScoped<IWebSocket, WebSocketService>();
            services.AddHostedService<WebSocketHostingService>();
        }

        /// <summary>
        /// Add the authentication for the frontend application, but we can configure multiple authentication
        /// for other additional application that contect the this service
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddFrontEndAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
        }
    }
}
