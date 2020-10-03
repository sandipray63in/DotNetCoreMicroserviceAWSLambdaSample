using System;
using System.Collections.Generic;
using Domain.Base.Aggregates;
using Infrastructure.Utilities;
using Microsoft.Extensions.Logging;
using Repository.Base;

namespace DomainServices.Base.CommandDomainServices
{
    public class CommandDomainService<TEntity> : ICommandDomainService<TEntity> where TEntity : ICommandAggregateRoot
    {
        protected readonly ICommandRepository<TEntity> _repository;

        protected readonly ILogger logger;

        public CommandDomainService(ICommandRepository<TEntity> repository, ILogger logger)
        {
            ContractUtility.Requires<ArgumentNullException>(repository != null, "repository instance cannot be null");
            ContractUtility.Requires<ArgumentNullException>(logger != null, "logger instance cannot be null");
            _repository = repository;
            this.logger = logger;
        }
        
        public virtual bool Insert(TEntity item, Action operationToExecuteBeforeNextOperation = null)
        {
            return InvokeAfterWrappingWithinExceptionHandling(() => _repository.Insert(item, operationToExecuteBeforeNextOperation));
        }

        public virtual bool Update(TEntity item, Action operationToExecuteBeforeNextOperation = null)
        {
            return InvokeAfterWrappingWithinExceptionHandling(() => _repository.Update(item, operationToExecuteBeforeNextOperation));
        }

        public virtual bool Delete(TEntity item, Action operationToExecuteBeforeNextOperation = null)
        {
            return InvokeAfterWrappingWithinExceptionHandling(() => _repository.Delete(item, operationToExecuteBeforeNextOperation));
        }

        public virtual bool Insert(IEnumerable<TEntity> items, Action operationToExecuteBeforeNextOperation = null)
        {
            return InvokeAfterWrappingWithinExceptionHandling(() => _repository.Insert(items, operationToExecuteBeforeNextOperation));
        }

        public virtual bool Update(IEnumerable<TEntity> items, Action operationToExecuteBeforeNextOperation = null)
        {
            return InvokeAfterWrappingWithinExceptionHandling(() => _repository.Update(items, operationToExecuteBeforeNextOperation));
        }

        public virtual bool Delete(IEnumerable<TEntity> items, Action operationToExecuteBeforeNextOperation = null)
        {
            return InvokeAfterWrappingWithinExceptionHandling(() => _repository.Delete(items, operationToExecuteBeforeNextOperation));
        }

        protected bool InvokeAfterWrappingWithinExceptionHandling(Action repositoryAction)
        {
            try
            {
                repositoryAction();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,ex.Message);
                return false;
            }
        }
    }
}
