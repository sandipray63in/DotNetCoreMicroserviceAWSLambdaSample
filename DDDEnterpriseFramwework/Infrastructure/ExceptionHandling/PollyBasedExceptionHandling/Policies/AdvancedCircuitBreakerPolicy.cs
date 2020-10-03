using System;
using Polly;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ExceptionHandling.PollyBasedExceptionHandling.Policies
{
    class AdvancedCircuitBreakerPolicy : IPolicy
    {
        private readonly ILogger _logger;
        private readonly double _failureThreshold;
        private readonly int _samplingDuration;
        private readonly int _minimumThroughput;
        private readonly int _durationOfBreak;

        public AdvancedCircuitBreakerPolicy(ILogger logger, double failureThreshold, int samplingDuration, int minimumThroughput, int durationOfBreak)
        {
            _logger = logger;
            _failureThreshold = failureThreshold;
            _samplingDuration = samplingDuration;
            _minimumThroughput = minimumThroughput;
            _durationOfBreak = durationOfBreak;
        }

        public Policy GetPolicy(PolicyBuilder policyBuilder)
        {
            return policyBuilder.AdvancedCircuitBreaker(_failureThreshold, TimeSpan.FromMilliseconds(_samplingDuration), _minimumThroughput,TimeSpan.FromMilliseconds(_durationOfBreak),
                    onBreak: (x, y) => _logger.LogError(x,x.Message + " Advanced Breaker GetPolicy logging: Breaking the circuit for " + y.TotalMilliseconds + " ms!"),
                    onReset: () => _logger.LogInformation("Advanced Breaker GetPolicy logging: Call ok! Closed the circuit again!"),
                    onHalfOpen: () => _logger.LogInformation("Advanced Breaker GetPolicy logging: Half-open: Next call is a trial!"));
        }

        public AsyncPolicy GetPolicyAsync(PolicyBuilder policyBuilder)
        {
            return policyBuilder.AdvancedCircuitBreakerAsync(_failureThreshold, TimeSpan.FromMilliseconds(_samplingDuration), _minimumThroughput, TimeSpan.FromMilliseconds(_durationOfBreak),
                    onBreak: (x, y) => _logger.LogError(x, x.Message + " Advanced Breaker GetPolicyAsync logging: Breaking the circuit for " + y.TotalMilliseconds + " ms!"),
                    onReset: () => _logger.LogInformation("Advanced Breaker GetPolicyAsync logging: Call ok! Closed the circuit again!"),
                    onHalfOpen: () => _logger.LogInformation("Advanced Breaker GetPolicyAsync logging: Half-open: Next call is a trial!"));
        }
    }
}


