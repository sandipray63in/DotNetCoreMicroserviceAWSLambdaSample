using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base.Aggregates;

namespace DomainServices.Base.CommandDomainServices
{
    public interface ICommandDomainServiceAsync<TEntity>  where TEntity : ICommandAggregateRoot
    {
        Task<bool> InsertAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
        Task<bool> UpdateAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
        Task<bool> DeleteAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
        Task<bool> InsertAsync(IEnumerable<TEntity> items, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
        Task<bool> UpdateAsync(IEnumerable<TEntity> items, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
        Task<bool> DeleteAsync(IEnumerable<TEntity> items, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
    }
}
