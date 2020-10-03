﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Infrastructure.ExceptionHandling;
using Infrastructure.Utilities;
using Microsoft.Extensions.Logging;

namespace Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ILogger _logger;
        private TransactionScope _scope;
        private readonly IsolationLevel _isoLevel;
        private readonly TransactionScopeOption _scopeOption;
        private Queue<OperationData> _operationsQueue;
        private IExceptionHandler _exceptionHandler;

        public UnitOfWork(IsolationLevel isoLevel = IsolationLevel.ReadCommitted, TransactionScopeOption scopeOption = TransactionScopeOption.RequiresNew, ILogger logger = null)
        {
            ContractUtility.Requires<ArgumentNullException>(logger.IsNotNull(), "logger instance cannot be null");
            _isoLevel = isoLevel;
            _scopeOption = scopeOption;
            _operationsQueue = new Queue<OperationData>();
            _logger = logger;
        }

        public UnitOfWork(IExceptionHandler exceptionHandler, IsolationLevel isoLevel = IsolationLevel.ReadCommitted, TransactionScopeOption scopeOption = TransactionScopeOption.RequiresNew, ILogger logger = null) : this(isoLevel, scopeOption, logger)
        {
            ContractUtility.Requires<ArgumentNullException>(exceptionHandler.IsNotNull(), "exceptionHandler instance cannot be null");
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Change the build Mode to "Test" to execute all "#if TEST" sections
        /// </summary>
#if TEST
        //Ideally, Action delegate should be used here but Unity Container doesn't suport Action delegate and only
        //supports Func<T> or Func<IEnumerable<T>> and that's why using Func<bool>
        //(since that's seems to be the simplest Func supported by Unity Container)
        private readonly Func<bool> _throwExceptionActionToTestRollback;
        private bool _isProcessDataMethodExecutedAtleastOnce;

        public UnitOfWork(Func<bool> throwExceptionActionToTestRollback, IsolationLevel isoLevel = IsolationLevel.ReadCommitted, TransactionScopeOption scopeOption = TransactionScopeOption.RequiresNew) : this(isoLevel, scopeOption,LoggerFactory.GetLogger(LoggerType.Default))
        {
            _throwExceptionActionToTestRollback = throwExceptionActionToTestRollback;
        }
#endif

        public void AddOperation(Action operation)
        {
            _operationsQueue.Enqueue(new OperationData { Operation = operation });
        }

        public void AddOperationAsync(Func<CancellationToken, Task> asyncOperation)
        {
           _operationsQueue.Enqueue(new OperationData { AsyncOperation = asyncOperation });
        }

        /// <summary>
        /// Comits all the data within this unit of work instance in an atomic way i.e. all or none get transacted.
        /// Order of operations of different instances of same type or different types needs to be handled at 
        /// the Business or Service Layer.
        /// </summary>
        /// <param name="shouldAutomaticallyRollBackOnTransactionException">when set to true(default value) 
        /// the RollBack method need not be called from the consumer class</param>
        public void Commit(bool shouldAutomaticallyRollBackOnTransactionException = true, bool shouldThrowOnException = true)
        {
            ContractUtility.Requires<ArgumentOutOfRangeException>(_operationsQueue.IsNotNullOrEmpty(), "Atleast one operation must be there to be executed.");

            ContractUtility.Requires<NotSupportedException>(_operationsQueue.All(x => x.AsyncOperation.IsNull()),
                                    "Async operations are not supported by Commit method.Use CommitAsync instead.");

            ExceptionHandlingUtility.HandleExceptionWithNullCheck(() =>
            {
                _scope = TransactionUtility.GetTransactionScope(_isoLevel, _scopeOption);
                try
                {
                    while (_operationsQueue.Count > 0)
                    {
#if TEST
                    ThrowExceptionForRollbackCheck();
#endif
                        OperationData operationData = _operationsQueue.Dequeue();
                        if (operationData.Operation.IsNotNull())
                        {
                            operationData.Operation();
                        }
                    }
                    CompleteScope(() =>
                    {
                        _scope.Complete(); // this just completes the transaction.Not yet committed here.
                        _scope.Dispose();  // After everthing runs successfully within the transaction 
                                           // and after completion, this should be called to actually commit the data 
                                           // within the transaction scope.
                    }, shouldAutomaticallyRollBackOnTransactionException, shouldThrowOnException);
                }
                catch (Exception ex)
                {
                    //Although ex is not exactly a commit exception but still passing it to reuse Rollback method.Using the Rollback
                    //method to reuse exception handling and dispose the transaction scope object(else it can cause issues for the 
                    //future transactions).
                    Rollback(ex);
                }
            }, _exceptionHandler);//TODO - proper exception handling compensating handler needs to be here
        }

        /// <summary>
        /// This method can run synchronous as well as asynchronous operations within a transaction
        /// in the order in which the operations are written in the consumer class.Here synchronous 
        /// as well as asynchronous operations are hadnled since "Task" also inherently handles both 
        /// synchronous and asynchronous scenarios.
        /// 
        /// </summary>
        /// <param name="shouldCommitSynchronousOperationsFirst"></param>
        /// <param name="shouldAutomaticallyRollBackOnTransactionException"></param>
        /// <param name="shouldThrowOnException"></param>
        /// <returns></returns>
        public async Task CommitAsync(CancellationToken token = default(CancellationToken), bool shouldAutomaticallyRollBackOnTransactionException = true, bool shouldThrowOnException = true)
        {
            ContractUtility.Requires<ArgumentOutOfRangeException>(_operationsQueue.IsNotNullOrEmpty(), "Atleast one operation must be there to be executed.");
            ContractUtility.Requires<NotSupportedException>(_operationsQueue.Any(x => x.AsyncOperation.IsNotNull()),
                "If CommitAsync method is used,there needs to be atleast one async operation exceuted." +
                "Please use Commit method(instead of CommitAsync) if there is not " +
                "a single async operation.");

            await ExceptionHandlingUtility.HandleExceptionWithNullCheck(async x =>
            {
                _scope = TransactionUtility.GetTransactionScope(_isoLevel, _scopeOption, true);
                try
                {
                    while (_operationsQueue.Count > 0)
                    {
#if TEST
                    ThrowExceptionForRollbackCheck();
#endif
                        OperationData operationData = _operationsQueue.Dequeue();
                        if (operationData.Operation.IsNotNull())
                        {
                            operationData.Operation();
                        }
                        else if (operationData.AsyncOperation.IsNotNull())
                        {
                            await operationData.AsyncOperation(x);
                        }
                    }
                    CompleteScope(() =>
                    {
                        _scope.Complete(); // this just completes the transaction.Not yet committed here.
                        _scope.Dispose();  // After everthing runs successfully within the transaction 
                                           // and after completion, this should be called to actually commit the data 
                                           // within the transaction scope.
                    }, shouldAutomaticallyRollBackOnTransactionException, shouldThrowOnException);
                }
                catch (Exception ex)
                {
                    //Although ex is not exactly a commit exception but still passing it to reuse Rollback method.Using the Rollback
                    //method to reuse exception handling and dispose the transaction scope object(else it can cause issues for the 
                    //future transactions).
                    Rollback(ex);
                }
            }, token, _exceptionHandler,null);//TODO - proper exception handling compensating handler needs to be here
        }

        /// <summary>
        /// Explicitly call the Dispose method in a Rollback method and call this Rollback method on transaction exceptions if required.
        /// </summary>
        public void Rollback(Exception commitException = null, bool shouldThrowOnException = true)
        {
            try
            {
                if (_scope.IsNotNull())
                {
                    _scope.Dispose();
                }
            }
            catch (Exception ex)
            {
                if (shouldThrowOnException)
                {
                    var rollBackException = new Exception("Rollback failed for the current transaction.Please check inner exception.", ex);
                    if (commitException.IsNull())
                    {
                        _logger.LogError(rollBackException, rollBackException.Message);
                        throw rollBackException;
                    }
                    else
                    {
                        var exceptionOccurredWhileCommitting = new Exception("Commit failed for the current transaction.Please check inner exception.", commitException);
                        var commitAndRollbackException = new AggregateException("Both commit and rollback failed for the current transaction.Please check inner exceptions.", exceptionOccurredWhileCommitting, rollBackException);
                        _logger.LogError(commitAndRollbackException, commitAndRollbackException.Message);
                        throw commitAndRollbackException;
                    }
                }
                else
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }


        #region Private Methods

        private void CompleteScope(Action action, bool shouldAutomaticallyRollBackOnTransactionException, bool shouldThrowOnException)
        {
            if (shouldAutomaticallyRollBackOnTransactionException)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Rollback();
                    _logger.LogError(ex, ex.Message);
                    if (shouldThrowOnException)
                    {
                        throw new Exception("Commit failed for the current transaction.Please check inner exception", ex);
                    }
                }
            }
            else
            {
                action();
            }
        }

#if TEST
        private void ThrowExceptionForRollbackCheck()
        {
            if (!_isProcessDataMethodExecutedAtleastOnce)
            {
                _isProcessDataMethodExecutedAtleastOnce = true;
            }
            else
            {
                if (_throwExceptionActionToTestRollback.IsNotNull())
                {
                    _throwExceptionActionToTestRollback();
                }
            }
        }
#endif

        #endregion Private Methods

    }
}
