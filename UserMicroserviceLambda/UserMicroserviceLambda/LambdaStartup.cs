using Microsoft.Extensions.Configuration;
using RestfulWebAPINetCore;
using UserMicroserviceLambda.EFContextsAndMaps;
using UserMicroserviceLambda.Models;

namespace UserMicroserviceLambda
{
    public class LambdaStartup : BaseLambdaStartup<UserContext, User, int>
    {
        public LambdaStartup(IConfiguration configuration) : base(configuration)
        {

        }

        protected override string ConnectionStringName => "UserDatabase";

        protected override string SwaggerDocTitle => "User API";

        protected override string LoggingCategoryName => "UserLogs";
    }
}