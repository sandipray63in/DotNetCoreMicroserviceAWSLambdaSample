using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;

namespace Infrastructure.ExceptionHandling.PollyBasedExceptionHandling.Policies
{
    public class FallbackPolicy : IPolicy, IFallbackActionPolicy
    {
        private readonly ILogger _logger;
        private Action _fallbackAction;
        private Func<CancellationToken, Task> _fallbackActionAsync;

        public FallbackPolicy(ILogger logger)
        {
            _logger = logger;
        }

        public void SetFallbackAction(Action fallbackAction)
        {
            _fallbackAction = fallbackAction;
        }

        public void SetFallbackAction(Func<CancellationToken, Task> fallbackAction)
        {
            _fallbackActionAsync = fallbackAction;
        }

        public Policy GetPolicy(PolicyBuilder policyBuilder)
        {
            return policyBuilder.Fallback(_fallbackAction, x => _logger.LogError(x,x.Message));
        }

        public AsyncPolicy GetPolicyAsync(PolicyBuilder policyBuilder)
        {
            return policyBuilder.FallbackAsync(_fallbackActionAsync, x => Task.Run(()=> _logger.LogError(x, x.Message)));
        }
    }
}

