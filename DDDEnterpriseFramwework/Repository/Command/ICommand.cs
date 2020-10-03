using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base.Aggregates;

namespace Repository.Command
{
    public interface ICommand<TEntity> where TEntity : ICommandAggregateRoot
    {
        void Insert(TEntity item);
        void Update(TEntity item);
        void Delete(TEntity item);
        void Insert(IEnumerable<TEntity> items);
        void Update(IEnumerable<TEntity> items);
        void Delete(IEnumerable<TEntity> items);

        #region Async Versions

        Task InsertAsync(TEntity item, CancellationToken token = default);
        Task UpdateAsync(TEntity item, CancellationToken token = default);
        Task DeleteAsync(TEntity item, CancellationToken token = default);
        Task InsertAsync(IEnumerable<TEntity> items, CancellationToken token = default);
        Task UpdateAsync(IEnumerable<TEntity> items, CancellationToken token = default);
        Task DeleteAsync(IEnumerable<TEntity> items, CancellationToken token = default);

        #endregion

    }
}
