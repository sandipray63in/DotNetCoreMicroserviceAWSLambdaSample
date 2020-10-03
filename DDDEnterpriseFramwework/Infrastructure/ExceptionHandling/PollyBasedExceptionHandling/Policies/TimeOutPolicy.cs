﻿using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;

namespace Infrastructure.ExceptionHandling.PollyBasedExceptionHandling.Policies
{
    public class TimeOutPolicy : IPolicy
    {
        private readonly ILogger _logger;
        private readonly int _timeOutInSeconds;
        private readonly TimeoutStrategy _timeoutStrategy;

        public TimeOutPolicy(ILogger logger, int timeOutInSeconds, TimeoutStrategy timeoutStrategy)
        {
            _logger = logger;
            _timeOutInSeconds = timeOutInSeconds;
            _timeoutStrategy = timeoutStrategy;
        }

        public Policy GetPolicy(PolicyBuilder poliyBuilder)
        {
            return Policy.Timeout(_timeOutInSeconds, _timeoutStrategy, onTimeout: (x, y, z) => _logger.LogInformation("The operation timed out in " + y.TotalMilliseconds + " ms!"));
        }

        public AsyncPolicy GetPolicyAsync(PolicyBuilder poliyBuilder)
        {
            return Policy.TimeoutAsync(_timeOutInSeconds, _timeoutStrategy, onTimeoutAsync: (x, y, z) => Task.Run(()=>_logger.LogInformation("The operation timed out in " + y.TotalMilliseconds + " ms!")));
        }
    }
}


