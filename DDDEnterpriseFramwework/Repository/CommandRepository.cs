﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base.Aggregates;
using Infrastructure.ExceptionHandling;
using Infrastructure.UnitOfWork;
using Infrastructure.Utilities;
using Repository.Base;
using Repository.Command;

namespace Repository
{
    /// <summary>
    /// A Repository per Entity 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class CommandRepository<TEntity> : BaseCommandRepository<TEntity> where TEntity : class, ICommandAggregateRoot
    {
        protected IUnitOfWork _unitOfWork;
        private readonly IExceptionHandler _exceptionHandler;

        /// <summary>
        /// Should be used when unit of work instance is not required 
        /// i.e when explicit transactions management is not required
        /// </summary>
        public CommandRepository(ICommand<TEntity> command)
            : base(command)
        {

        }

        public CommandRepository(ICommand<TEntity> command, IExceptionHandler exceptionHandler)
            : base(command)
        {
            ContractUtility.Requires<ArgumentNullException>(exceptionHandler.IsNotNull(), "exceptionHandler instance cannot be null");
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// The same unit of work instance can be used across different instances of repositories
        /// (if needed)
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="command"></param>
        public CommandRepository(IUnitOfWork unitOfWork, ICommand<TEntity> command)
            : base(command)
        {
            ContractUtility.Requires<ArgumentNullException>(unitOfWork.IsNotNull(), "unitOfWork instance cannot be null");
            _unitOfWork = unitOfWork;
        }

        internal void SetUnitOfWork<TUnitOfWork>(TUnitOfWork unitOfWork)
            where TUnitOfWork : IUnitOfWork
        {
            _unitOfWork = unitOfWork;
        }

        #region ICommandRepository<T> Members
        /// <summary>
        /// While using this API alongwith unit of work instance, this API's return value should 
        /// not be used as the actual return value for the commit.But rather the internal unit of work's 
        /// Commit method's return value should be used.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override void Insert(TEntity item, Action operationToExecuteBeforeNextOperation = null)
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            if (_unitOfWork.IsNotNull())
            {
                _unitOfWork.AddOperation(() => ActualInsert(item, operationToExecuteBeforeNextOperation));
            }
            else
            {
                //TODO - proper exception handling compensating handler needs to be here
                ExceptionHandlingUtility.HandleExceptionWithNullCheck(() => ActualInsert(item, operationToExecuteBeforeNextOperation), _exceptionHandler);
            }
        }
        public override void Update(TEntity item, Action operationToExecuteBeforeNextOperation = null)
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            if (_unitOfWork.IsNotNull())
            {
                _unitOfWork.AddOperation(() => ActualUpdate(item, operationToExecuteBeforeNextOperation));
            }
            else
            {
                //TODO - proper exception handling compensating handler needs to be here
                ExceptionHandlingUtility.HandleExceptionWithNullCheck(() => ActualUpdate(item, operationToExecuteBeforeNextOperation), _exceptionHandler);
            }
        }
        public override void Delete(TEntity item, Action operationToExecuteBeforeNextOperation = null)
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            if (_unitOfWork.IsNotNull())
            {
                _unitOfWork.AddOperation(() => ActualDelete(item, operationToExecuteBeforeNextOperation));
            }
            else
            {
                //TODO - proper exception handling compensating handler needs to be here
                ExceptionHandlingUtility.HandleExceptionWithNullCheck(() => ActualDelete(item, operationToExecuteBeforeNextOperation), _exceptionHandler);
            }
        }

        public override void Insert(IEnumerable<TEntity> items, Action operationToExecuteBeforeNextOperation = null)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            if (_unitOfWork.IsNotNull())
            {
                _unitOfWork.AddOperation(() => ActualInsert(items, operationToExecuteBeforeNextOperation));
            }
            else
            {
                //TODO - proper exception handling compensating handler needs to be here
                ExceptionHandlingUtility.HandleExceptionWithNullCheck(() => ActualInsert(items, operationToExecuteBeforeNextOperation), _exceptionHandler);
            }
        }

        public override void Update(IEnumerable<TEntity> items, Action operationToExecuteBeforeNextOperation = null)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            if (_unitOfWork.IsNotNull())
            {
                _unitOfWork.AddOperation(() => ActualUpdate(items, operationToExecuteBeforeNextOperation));
            }
            else
            {
                //TODO - proper exception handling compensating handler needs to be here
                ExceptionHandlingUtility.HandleExceptionWithNullCheck(() => ActualUpdate(items, operationToExecuteBeforeNextOperation), _exceptionHandler);
            }
        }

        public override void Delete(IEnumerable<TEntity> items, Action operationToExecuteBeforeNextOperation = null)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            if (_unitOfWork.IsNotNull())
            {
                _unitOfWork.AddOperation(() => ActualDelete(items, operationToExecuteBeforeNextOperation));
            }
            else
            {
                //TODO - proper exception handling compensating handler needs to be here
                ExceptionHandlingUtility.HandleExceptionWithNullCheck(() => ActualDelete(items, operationToExecuteBeforeNextOperation), _exceptionHandler);
            }
        }

        public override async Task InsertAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            if (_unitOfWork.IsNotNull())
            {
                _unitOfWork.AddOperationAsync(async x => await ActualInsertAsync(item, token == default ? x : token, operationToExecuteBeforeNextOperation).ConfigureAwait(false));
            }
            else
            {
                //TODO - proper exception handling compensating handler needs to be here
                await ExceptionHandlingUtility.HandleExceptionWithNullCheck(async x => await ActualInsertAsync(item, x, operationToExecuteBeforeNextOperation).ConfigureAwait(false), token, _exceptionHandler, null);
            }
        }

        public override async Task UpdateAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            if (_unitOfWork.IsNotNull())
            {
                _unitOfWork.AddOperationAsync(async x => await ActualUpdateAsync(item, token == default ? x : token, operationToExecuteBeforeNextOperation).ConfigureAwait(false));
            }
            else
            {
                //TODO - proper exception handling compensating handler needs to be here
                await ExceptionHandlingUtility.HandleExceptionWithNullCheck(async x => await ActualUpdateAsync(item, x, operationToExecuteBeforeNextOperation).ConfigureAwait(false), token, _exceptionHandler, null);
            }
        }

        public override async Task DeleteAsync(TEntity item, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            if (_unitOfWork.IsNotNull())
            {
                _unitOfWork.AddOperationAsync(async x => await ActualDeleteAsync(item, token == default ? x : token, operationToExecuteBeforeNextOperation).ConfigureAwait(false));
            }
            else
            {
                //TODO - proper exception handling compensating handler needs to be here
                await ExceptionHandlingUtility.HandleExceptionWithNullCheck(async x => await ActualDeleteAsync(item, x, operationToExecuteBeforeNextOperation).ConfigureAwait(false), token, _exceptionHandler, null);
            }
        }

        public override async Task InsertAsync(IEnumerable<TEntity> items, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            if (_unitOfWork.IsNotNull())
            {
                _unitOfWork.AddOperationAsync(async x => await ActualInsertAsync(items, token == default ? x : token, operationToExecuteBeforeNextOperation).ConfigureAwait(false));
            }
            else
            {
                //TODO - proper exception handling compensating handler needs to be here
                await ExceptionHandlingUtility.HandleExceptionWithNullCheck(async x => await ActualInsertAsync(items, x, operationToExecuteBeforeNextOperation).ConfigureAwait(false), token, _exceptionHandler, null);
            }
        }

        public override async Task UpdateAsync(IEnumerable<TEntity> items, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            if (_unitOfWork.IsNotNull())
            {
                _unitOfWork.AddOperationAsync(async x => await ActualUpdateAsync(items, token == default ? x : token, operationToExecuteBeforeNextOperation).ConfigureAwait(false));
            }
            else
            {
                //TODO - proper exception handling compensating handler needs to be here
                await ExceptionHandlingUtility.HandleExceptionWithNullCheck(async x => await ActualUpdateAsync(items, x, operationToExecuteBeforeNextOperation).ConfigureAwait(false), token, _exceptionHandler, null);
            }
        }

        public override async Task DeleteAsync(IEnumerable<TEntity> items, CancellationToken token = default, Action operationToExecuteBeforeNextOperation = null)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            if (_unitOfWork.IsNotNull())
            {
                _unitOfWork.AddOperationAsync(async x => await ActualDeleteAsync(items, token == default ? x : token, operationToExecuteBeforeNextOperation).ConfigureAwait(false));
            }
            else
            {
                //TODO - proper exception handling compensating handler needs to be here
                await ExceptionHandlingUtility.HandleExceptionWithNullCheck(async x => await ActualDeleteAsync(items, x, operationToExecuteBeforeNextOperation).ConfigureAwait(false), token, _exceptionHandler, null);
            }
        }

        #endregion
    }
}
