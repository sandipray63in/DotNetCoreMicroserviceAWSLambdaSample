using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base.Aggregates;

namespace Repository.Base
{
    /// <summary>
    /// Both Enumerable based operations(e.g. "void Insert(IEnumerable<TEntity> items)") and bulk operations api have been provided 
    /// for a reason.The reason being Enumerable based operations might be required for web service calls while Bulk Operations
    /// gives you api to perform the operations real fast.
    /// 
    /// For Async operations, if Unit of Work(explicit Transactions Management) is used then the 
    /// Cancellation token passed to the CommitAsync method(in UnitOfWork class) will be used 
    /// else Cancellation token passed to individual methods will be used. 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface ICommandRepository<TEntity> where TEntity : ICommandAggregateRoot
    {
        void Insert(TEntity item, Action operationToExecuteBeforeNextOperation = null);
        void Update(TEntity item, Action operationToExecuteBeforeNextOperation = null);
        void Delete(TEntity item, Action operationToExecuteBeforeNextOperation = null);

        // If here DB is being interacted,then each and every internal item will be iterated and then DB call will be there
        // for each item.So performance might degrade.But if one is interacting with a web service via this api and the Web service
        // does not provide some Enumerable based api then this can be used from the Business Layer or UI Layer to pass an Enumerable
        // and internally the implementations will take care of iteratively calling the service for each item.
        void Insert(IEnumerable<TEntity> items, Action operationToExecuteBeforeNextOperation = null);
        void Update(IEnumerable<TEntity> items, Action operationToExecuteBeforeNextOperation = null);
        void Delete(IEnumerable<TEntity> items, Action operationToExecuteBeforeNextOperation = null);

        #region Async Versions

        Task InsertAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
        Task UpdateAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
        Task DeleteAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
        Task InsertAsync(IEnumerable<TEntity> items, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
        Task UpdateAsync(IEnumerable<TEntity> items, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
        Task DeleteAsync(IEnumerable<TEntity> items, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);

        #endregion
    }
}
