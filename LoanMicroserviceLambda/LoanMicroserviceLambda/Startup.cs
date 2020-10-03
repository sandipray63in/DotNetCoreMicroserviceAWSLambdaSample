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
    public class Startup : BaseStartup<LoanContext,Loan,int>
    {
        public Startup(IConfiguration configuration):base(configuration)
        {
            
        }

        protected override string ConnectionStringName => "LoanDatabase";

        protected override string SwaggerDocTitle => "Loan API";

        protected override void AddLoggingAndOtherServices(IServiceCollection services)
        {
            services.AddHttpClient();

            var log4netRepository = log4net.LogManager.GetRepository(Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(log4netRepository, new FileInfo("Log4Net.config"));
            var loggerFactory = (ILoggerFactory)new LoggerFactory();
            loggerFactory.AddLog4Net();
            var logger = loggerFactory.CreateLogger("UserLoansLogs");
            services.AddSingleton(logger);
        }
    }
}
