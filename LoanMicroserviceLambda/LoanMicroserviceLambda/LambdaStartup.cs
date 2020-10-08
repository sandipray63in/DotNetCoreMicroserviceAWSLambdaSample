using LoanMicroserviceLambda.EFContextsAndMaps;
using LoanMicroserviceLambda.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestfulWebAPINetCore;

namespace LoanMicroserviceLambda
{
    public class LambdaStartup : BaseLambdaStartup<LoanContext, Loan, int>
    {
        public LambdaStartup(IConfiguration configuration) : base(configuration)
        {

        }

        protected override string ConnectionStringName => "LoanDatabase";

        protected override string SwaggerDocTitle => "Loan API";

        protected override string LoggingCategoryName => "LoanLogs";

        protected override void ConfigureOtherServices(IServiceCollection services)
        {
            services.AddHttpClient();
        }
    }
}
