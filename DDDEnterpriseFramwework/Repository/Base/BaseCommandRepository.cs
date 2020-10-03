using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base.Aggregates;
using Domain.Base.AddOnObjects;
using Repository.Command;
using Infrastructure.Utilities;

namespace Repository.Base
{
    public abstract class BaseCommandRepository<TEntity> : BaseUnitOfWorkRepository<TEntity>, ICommandRepository<TEntity>
        where TEntity : ICommandAggregateRoot
    {
        #region Private Fields

        private ICommand<TEntity> _command;

        #endregion

        #region Constructors

        /// <summary>
        /// Should be used when unit of work instance is not required 
        /// i.e when explicit transactions management is not required
        /// </summary>
        protected BaseCommandRepository(ICommand<TEntity> command)
        {
            ContractUtility.Requires<ArgumentNullException>(command.IsNotNull(), "Command instance cannot be null");
            _command = command;
        }

        #endregion

        internal void SetCommand(dynamic command)
        {
            _command = command as ICommand<TEntity>;
        }

        internal override void ActualInsert(TEntity item, Action operationToExecuteBeforeNextOperation = null)
        {
            _command.Insert(item);
            ExecuteOperationBeforeNextOperation(operationToExecuteBeforeNextOperation);
        }

        internal override void ActualUpdate(TEntity item, Action operationToExecuteBeforeNextOperation = null)
        {
            _command.Update(item);
            ExecuteOperationBeforeNextOperation(operationToExecuteBeforeNextOperation);
        }

        internal override void ActualDelete(TEntity item, Action operationToExecuteBeforeNextOperation = null)
        {
            if (item.GetType().GetProperties().Any(x => x.GetType() == typeof(SoftDeleteableInfo)))
            {
                ActualUpdate(item);
            }
            else
            {
                _command.Delete(item);
            }
            ExecuteOperationBeforeNextOperation(operationToExecuteBeforeNextOperation);
        }

        internal override void ActualInsert(IEnumerable<TEntity> itemsEnumerable, Action operationToExecuteBeforeNextOperation = null)
        {
            _command.Insert(itemsEnumerable);
            ExecuteOperationBeforeNextOperation(operationToExecuteBeforeNextOperation);
        }

        internal override void ActualUpdate(IEnumerable<TEntity> itemsEnumerable, Action operationToExecuteBeforeNextOperation = null)
        {
            _command.Update(itemsEnumerable);
            ExecuteOperationBeforeNextOperation(operationToExecuteBeforeNextOperation);
        }

        internal override void ActualDelete(IEnumerable<TEntity> itemsEnumerable, Action operationToExecuteBeforeNextOperation = null)
        {
            IEnumerable<TEntity> softDeleteableItems = itemsEnumerable.Where(x => x.GetType().GetProperties().Any(y => y.GetType() == typeof(SoftDeleteableInfo)));
            IEnumerable<TEntity> nonSoftDeleteableItems = itemsEnumerable.Where(x => !(x.GetType().GetProperties().Any(y => y.GetType() == typeof(SoftDeleteableInfo))));
            if (softDeleteableItems.IsNotEmpty())
            {
                ActualUpdate(softDeleteableItems);
            }
            if (nonSoftDeleteableItems.IsNotEmpty())
            {
                _command.Delete(itemsEnumerable);
            }
            ExecuteOperationBeforeNextOperation(operationToExecuteBeforeNextOperation);
        }

        internal override async Task ActualInsertAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            await _command.InsertAsync(item, token).ConfigureAwait(false);
            ExecuteOperationBeforeNextOperation(operationToExecuteBeforeNextOperation);
        }

        internal override async Task ActualUpdateAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            await _command.UpdateAsync(item, token).ConfigureAwait(false);
            ExecuteOperationBeforeNextOperation(operationToExecuteBeforeNextOperation);
        }

        internal override async Task ActualDeleteAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            if (item.GetType().GetProperties().Any(x => x.GetType() == typeof(SoftDeleteableInfo)))
            {
                await ActualUpdateAsync(item, token);
            }
            else
            {
                await _command.DeleteAsync(item, token).ConfigureAwait(false);
            }
            ExecuteOperationBeforeNextOperation(operationToExecuteBeforeNextOperation);
        }

        internal override async Task ActualInsertAsync(IEnumerable<TEntity> itemsEnumerable, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            await _command.InsertAsync(itemsEnumerable, token).ConfigureAwait(false);
            ExecuteOperationBeforeNextOperation(operationToExecuteBeforeNextOperation);
        }

        internal override async Task ActualUpdateAsync(IEnumerable<TEntity> itemsEnumerable, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            await _command.UpdateAsync(itemsEnumerable, token).ConfigureAwait(false);
            ExecuteOperationBeforeNextOperation(operationToExecuteBeforeNextOperation);
        }

        internal override async Task ActualDeleteAsync(IEnumerable<TEntity> itemsEnumerable, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            IEnumerable<TEntity> softDeleteableItems = itemsEnumerable.Where(x => x.GetType().GetProperties().Any(y => y.GetType() == typeof(SoftDeleteableInfo)));
            IEnumerable<TEntity> nonSoftDeleteableItems = itemsEnumerable.Where(x => !(x.GetType().GetProperties().Any(y => y.GetType() == typeof(SoftDeleteableInfo))));
            if (softDeleteableItems.IsNotEmpty())
            {
                await ActualUpdateAsync(softDeleteableItems, token);
            }
            if (nonSoftDeleteableItems.IsNotEmpty())
            {
                await _command.DeleteAsync(itemsEnumerable, token).ConfigureAwait(false);
            }
            ExecuteOperationBeforeNextOperation(operationToExecuteBeforeNextOperation);
        }

        public abstract void Insert(TEntity item, Action operationToExecuteBeforeNextOperation = null);
        public abstract void Update(TEntity item, Action operationToExecuteBeforeNextOperation = null);

        public abstract void Delete(TEntity item, Action operationToExecuteBeforeNextOperation = null);
        public abstract void Insert(IEnumerable<TEntity> items, Action operationToExecuteBeforeNextOperation = null);

        public abstract void Update(IEnumerable<TEntity> items, Action operationToExecuteBeforeNextOperation = null);

        public abstract void Delete(IEnumerable<TEntity> items, Action operationToExecuteBeforeNextOperation = null);

        public abstract Task InsertAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
        public abstract Task UpdateAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
        public abstract Task DeleteAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
        public abstract Task InsertAsync(IEnumerable<TEntity> items, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
        public abstract Task UpdateAsync(IEnumerable<TEntity> items, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);
        public abstract Task DeleteAsync(IEnumerable<TEntity> items, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null);

        #region Private Methods

        private void ExecuteOperationBeforeNextOperation(Action operationToExecuteBeforeNextOperation = null)
        {
            if (operationToExecuteBeforeNextOperation.IsNotNull())
            {
                operationToExecuteBeforeNextOperation();
            }
        }

        #endregion
    }
}
