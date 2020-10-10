using Microsoft.Extensions.Configuration;
using UserMicroserviceLambda.EFContextsAndMaps;
using UserMicroserviceLambda.Models;
using RestfulWebAPINetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.IO;

namespace UserMicroserviceLambda
{
    public class LocalStartup : BaseStartup<UserContext,User,int>
    {
        public LocalStartup(IConfiguration configuration):base(configuration)
        {
            
        }

        protected override string ConnectionStringName => "UserDatabase";

        protected override string SwaggerDocTitle => "User API";

        protected override void ConfigureLoggingService(IServiceCollection services)
        {
            var log4netRepository = log4net.LogManager.GetRepository(Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(log4netRepository, new FileInfo("Log4Net.config"));
            var loggerFactory = (ILoggerFactory)new LoggerFactory();
            loggerFactory.AddLog4Net();
            var logger = loggerFactory.CreateLogger("UserLogs");
            services.AddSingleton(logger);
        }
    }
}
