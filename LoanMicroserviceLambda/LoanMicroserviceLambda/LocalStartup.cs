using Microsoft.Extensions.Configuration;
using LoanMicroserviceLambda.EFContextsAndMaps;
using RestfulWebAPINetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.IO;
using LoanMicroserviceLambda.Models;

namespace LoadMicroserviceLambda
{
    public class LocalStartup : BaseLocalStartup<LoanContext,Loan,int>
    {
        public LocalStartup(IConfiguration configuration):base(configuration)
        {
            
        }

        protected override string ConnectionStringName => "LoanDatabase";

        protected override string SwaggerDocTitle => "Loan API";

        protected override void ConfigureLoggingService(IServiceCollection services)
        {
            var log4netRepository = log4net.LogManager.GetRepository(Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(log4netRepository, new FileInfo("Log4Net.config"));
            var loggerFactory = (ILoggerFactory)new LoggerFactory();
            loggerFactory.AddLog4Net();
            var logger = loggerFactory.CreateLogger("LoanLogs");
            services.AddSingleton(logger);
        }

        protected override void ConfigureOtherServices(IServiceCollection services)
        {
            services.AddHttpClient();
        }
    }
}
