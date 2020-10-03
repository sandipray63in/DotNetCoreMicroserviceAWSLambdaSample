using System;
using Polly;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ExceptionHandling.PollyBasedExceptionHandling.Policies
{
    public class CircuitBreakerPolicy : IPolicy
    {
        private readonly ILogger _logger;
        private readonly int _exceptionsAllowedBeforeBreaking;
        private readonly int _durationOfBreakInMilliseconds;

        public CircuitBreakerPolicy(ILogger logger, int exceptionsAllowedBeforeBreaking, int durationOfBreakInMilliseconds)
        {
            _logger = logger;
            _exceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;
            _durationOfBreakInMilliseconds = durationOfBreakInMilliseconds;
        }

        public Policy GetPolicy(PolicyBuilder policyBuilder)
        {
            return policyBuilder.CircuitBreaker(_exceptionsAllowedBeforeBreaking, TimeSpan.FromMilliseconds(_durationOfBreakInMilliseconds),
                onBreak: (x, y) => _logger.LogError(x, x.Message + " Breaker GetPolicy logging: Breaking the circuit for " + y.TotalMilliseconds + " ms!"),
                    onReset: () => _logger.LogInformation("Breaker GetPolicy logging: Call ok! Closed the circuit again!"),
                    onHalfOpen: () => _logger.LogInformation("Breaker GetPolicy logging: Half-open: Next call is a trial!"));
        }

        public AsyncPolicy GetPolicyAsync(PolicyBuilder policyBuilder)
        {
            return policyBuilder.CircuitBreakerAsync(_exceptionsAllowedBeforeBreaking, TimeSpan.FromMilliseconds(_durationOfBreakInMilliseconds),
                    onBreak: (x, y) => _logger.LogError(x, x.Message + " Breaker GetPolicyAsync logging: Breaking the circuit for " + y.TotalMilliseconds + " ms!"),
                    onReset: () => _logger.LogInformation("Breaker GetPolicyAsync logging: Call ok! Closed the circuit again!"),
                    onHalfOpen: () => _logger.LogInformation("Breaker GetPolicyAsync logging: Half-open: Next call is a trial!"));
        }
    }
}


