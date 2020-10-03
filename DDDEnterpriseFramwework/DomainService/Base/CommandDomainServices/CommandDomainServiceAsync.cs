using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base.Aggregates;
using Infrastructure.Utilities;
using Repository.Base;
using Microsoft.Extensions.Logging;

namespace DomainServices.Base.CommandDomainServices
{
    public class CommandDomainServiceAsync<TEntity> : ICommandDomainServiceAsync<TEntity> where TEntity : ICommandAggregateRoot
    {
        protected readonly ICommandRepository<TEntity> _repository;

        protected readonly ILogger logger;

        public CommandDomainServiceAsync(ICommandRepository<TEntity> repository, ILogger logger)
        {
            ContractUtility.Requires<ArgumentNullException>(repository != null, "repository instance cannot be null");
            ContractUtility.Requires<ArgumentNullException>(logger != null, "logger instance cannot be null");
            _repository = repository;
            this.logger = logger;
        }

        public virtual async Task<bool> InsertAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            return await InvokeAfterWrappingWithinExceptionHandling(async () => await _repository.InsertAsync(item, token, operationToExecuteBeforeNextOperation).ConfigureAwait(false));
        }

        public virtual async Task<bool> UpdateAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            return await InvokeAfterWrappingWithinExceptionHandling(async () => await _repository.UpdateAsync(item, token, operationToExecuteBeforeNextOperation).ConfigureAwait(false));
        }

        public virtual async Task<bool> DeleteAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            return await InvokeAfterWrappingWithinExceptionHandling(async () => await _repository.DeleteAsync(item, token, operationToExecuteBeforeNextOperation).ConfigureAwait(false));
        }

        public virtual async Task<bool> InsertAsync(IEnumerable<TEntity> items, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            return await InvokeAfterWrappingWithinExceptionHandling(async () => await _repository.InsertAsync(items, token, operationToExecuteBeforeNextOperation).ConfigureAwait(false));
        }

        public virtual async Task<bool> UpdateAsync(IEnumerable<TEntity> items, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            return await InvokeAfterWrappingWithinExceptionHandling(async () => await _repository.UpdateAsync(items, token, operationToExecuteBeforeNextOperation).ConfigureAwait(false));
        }

        public virtual async Task<bool> DeleteAsync(IEnumerable<TEntity> items, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            return await InvokeAfterWrappingWithinExceptionHandling(async () => await _repository.DeleteAsync(items, token, operationToExecuteBeforeNextOperation).ConfigureAwait(false));
        }

        protected async Task<bool> InvokeAfterWrappingWithinExceptionHandling(Action repositoryAction)
        {
            try
            {
                repositoryAction();
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,ex.Message);
                return await Task.FromResult(false);
            }
        }
    }
}
