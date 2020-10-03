using Polly;

namespace Infrastructure.ExceptionHandling.PollyBasedExceptionHandling.Policies
{
    public interface IPolicy 
    {
        Policy GetPolicy(PolicyBuilder policyBuilder);

        AsyncPolicy GetPolicyAsync(PolicyBuilder policyBuilder);
    }
}
