using System;
using System.Collections.Generic;
using Domain.Base.Aggregates;

namespace DomainServices.Base.CommandDomainServices
{
    public interface ICommandDomainService<TEntity>  where TEntity : ICommandAggregateRoot
    {
        bool Insert(TEntity item, Action operationToExecuteBeforeNextOperation = null);
        bool Update(TEntity item, Action operationToExecuteBeforeNextOperation = null);
        bool Delete(TEntity item, Action operationToExecuteBeforeNextOperation = null);
        bool Insert(IEnumerable<TEntity> items, Action operationToExecuteBeforeNextOperation = null);
        bool Update(IEnumerable<TEntity> items, Action operationToExecuteBeforeNextOperation = null);
        bool Delete(IEnumerable<TEntity> items, Action operationToExecuteBeforeNextOperation = null);
    }
}
