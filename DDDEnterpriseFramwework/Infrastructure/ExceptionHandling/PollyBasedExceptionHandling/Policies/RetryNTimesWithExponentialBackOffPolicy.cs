using System;
using Microsoft.Extensions.Logging;
using Polly;

namespace Infrastructure.ExceptionHandling.PollyBasedExceptionHandling.Policies
{
    public class RetryNTimesWithExponentialBackOffPolicy : IPolicy
    {
        private readonly ILogger _logger;
        private readonly int _retryCount;
        private readonly int _exponentialBackOffFactor;

        public RetryNTimesWithExponentialBackOffPolicy(ILogger logger, int retryCount, int exponentialBackOffFactor)
        {
            _logger = logger;
            _retryCount = retryCount;
            _exponentialBackOffFactor = exponentialBackOffFactor;
        }

        public Policy GetPolicy(PolicyBuilder policyBuilder)
        {
            return policyBuilder.WaitAndRetry(_retryCount, x => TimeSpan.FromMilliseconds(0.1*Math.Pow(_exponentialBackOffFactor,x)), (x, y) => _logger.LogError(x,string.Format("GetPolicy in RetryNTimesWithExponentialBackOffPolicy : Tried {0} number of time(s)", y)));
        }

        public AsyncPolicy GetPolicyAsync(PolicyBuilder policyBuilder)
        {
            return policyBuilder.WaitAndRetryAsync(_retryCount, x => TimeSpan.FromMilliseconds(0.1 * Math.Pow(_exponentialBackOffFactor, x)), (x, y) => _logger.LogError(x,string.Format("GetPolicyAsync in RetryNTimesWithExponentialBackOffPolicy : Tried {0} number of time(s)", y)));
        }
    }
}

