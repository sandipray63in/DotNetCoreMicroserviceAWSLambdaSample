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
    public class Startup : BaseStartup<UserContext,User,int>
    {
        public Startup(IConfiguration configuration):base(configuration)
        {
            
        }

        protected override string ConnectionStringName => "UserDatabase";

        protected override string SwaggerDocTitle => "User API";

        protected override void AddLoggingAndOtherServices(IServiceCollection services)
        {
            var log4netRepository = log4net.LogManager.GetRepository(Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(log4netRepository, new FileInfo("Log4Net.config"));
            var loggerFactory = (ILoggerFactory)new LoggerFactory();
            loggerFactory.AddLog4Net();
            var logger = loggerFactory.CreateLogger("UserLoansLogs");
            services.AddSingleton(logger);
        }
    }
}
