using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ZEISSMachineStream
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configuration object
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// The constructor of the StartUp class used in the CreateWebHostBuilder
        /// </summary>
        /// <param name="env">The injected IHostingEnvironment used to describe the enviroment that is running</param>
        public Startup(IHostingEnvironment env)
        {
            string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Environment.SetEnvironmentVariable("LOGDIR", env.IsDevelopment() ? env.ContentRootPath + "/bin/Debug/netcoreapp2.2" : env.ContentRootPath);
            Environment.SetEnvironmentVariable("APPNAME", Assembly.GetExecutingAssembly().GetName().Name);

            var configBuilder = new ConfigurationBuilder();

            configBuilder.SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            if (!string.IsNullOrEmpty(envName))
                configBuilder.AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true);

            Configuration = configBuilder.Build();

            //Add the configuration and based on the configuration
            //and create the static logger based on the configuration
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration).CreateLogger();
            if (env.IsDevelopment())
            {
                Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
                Serilog.Debugging.SelfLog.Enable(Console.Error);
            }
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore)
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "ZEISS Machine Stream", Version = "v1" });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            //add and configure authentication for the frontend API,
            //is not required in this example but is a very important part of an application
            //services.AddFrontEndAuthentication(Configuration);

            // Add CORS policies
            services.AddCorsPolicies(Configuration);

            //register all the configuration from appsettings
            services.AddConfigurations(Configuration);

            //register the interfaces and the scopes of the instances
            services.AddDI(Configuration);
        }

        /// <summary>
        /// This method is called by the runtime to configure the logging
        /// </summary>
        /// <param name="loggingBuilder"></param>
        public void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            //add the base configuration to the logging builder (in Serilog section of appsettings is possible to overwrite them)
            loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ZEISS Machine Stream v1");
            });

            //add serilog to logging pipeline
            loggerFactory.AddSerilog();

            //add the cors policy to the pipeline
            app.UseCors("AllowSpecificOrigin");
            //add htts redirection to the pipeline
            app.UseHttpsRedirection();
            //register the base routing of the application
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "v1/{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
