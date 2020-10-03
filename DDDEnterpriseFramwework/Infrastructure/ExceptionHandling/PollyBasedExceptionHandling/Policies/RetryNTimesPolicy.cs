using System;
using Microsoft.Extensions.Logging;
using Polly;

namespace Infrastructure.ExceptionHandling.PollyBasedExceptionHandling.Policies
{
    public class RetryNTimesPolicy : IPolicy
    {
        private readonly ILogger _logger;
        private readonly int _retryCount;

        public RetryNTimesPolicy(ILogger logger, int retryCount)
        {
            _logger = logger;
            _retryCount = retryCount;
        }

        public Policy GetPolicy(PolicyBuilder policyBuilder)
        {
            return policyBuilder.Retry(_retryCount, (x, y) =>_logger.LogError(x,string.Format("GetPolicy in RetryNTimesPolicy : Tried {0} number of time(s)", y)));
        }

        public AsyncPolicy GetPolicyAsync(PolicyBuilder policyBuilder)
        {
            return policyBuilder.RetryAsync(_retryCount, (x, y) => _logger.LogError(x, string.Format("GetPolicyAsync in RetryNTimesPolicy : Tried {0} number of time(s)", y)));
        }
    }
}
