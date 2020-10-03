using System;
using Microsoft.Extensions.Logging;
using Polly;

namespace Infrastructure.ExceptionHandling.PollyBasedExceptionHandling.Policies
{
    public class RetryNTimesWithWaitPolicy : IPolicy
    {
        private readonly ILogger _logger;
        private readonly int _retryCount;
        private readonly int _retryWaitTimeInMilliseconds;

        public RetryNTimesWithWaitPolicy(ILogger logger, int retryCount,int retryWaitTimeInMilliseconds)
        {
            _logger = logger;
            _retryCount = retryCount;
            _retryWaitTimeInMilliseconds = retryWaitTimeInMilliseconds;
        }

        public Policy GetPolicy(PolicyBuilder policyBuilder)
        {
            return policyBuilder.WaitAndRetry(_retryCount,x => TimeSpan.FromMilliseconds(_retryWaitTimeInMilliseconds), (x, y) => _logger.LogError(x,string.Format("GetPolicy in RetryNTimesWithWaitPolicy : Tried {0} number of time(s)", y)));
        }

        public AsyncPolicy GetPolicyAsync(PolicyBuilder policyBuilder)
        {
            return policyBuilder.WaitAndRetryAsync(_retryCount, x => TimeSpan.FromMilliseconds(_retryWaitTimeInMilliseconds), (x, y) => _logger.LogError(x,string.Format("GetPolicyAsync in RetryNTimesWithWaitPolicy : Tried {0} number of time(s)", y)));
        }
    }
}

