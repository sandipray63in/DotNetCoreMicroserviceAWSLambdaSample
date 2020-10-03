﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Wrap;
using Infrastructure.ExceptionHandling.PollyBasedExceptionHandling.Policies;
using Infrastructure.Extensions;
using Infrastructure.Utilities;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Infrastructure.ExceptionHandling.PollyBasedExceptionHandling
{
    public class BasicPollyExceptionHandler : BaseExceptionHandler
    {
        private static PolicyBuilder _policyBuilder;
        private static ILogger _staticLogger;
        private static PollyTransientFailureExceptions _pollyTransientFailureExceptions;
        private static IEnumerable<string> _splittedTransientFailureExceptions;
        private readonly bool _shouldThrowOnException;
        private readonly ILogger _logger;
        private readonly Func<IEnumerable<IPolicy>, PolicyWrap> _policyWrapForSyncOperationsFunc = x => Policy.Wrap(x.Select(y => y.GetPolicy(_policyBuilder)).ToArray());
        private readonly Func<IEnumerable<IPolicy>, AsyncPolicyWrap> _policyWrapForAsyncOperationsFunc = x => Policy.WrapAsync(x.Select(y => y.GetPolicyAsync(_policyBuilder)).ToArray());
        private readonly IEnumerable<IPolicy> _policies;
        private bool _areFallbackPoliciesAlreadyHandled;

        /// <summary>
        /// Polly based basic exception handler
        /// </summary>
        /// <param name="policies">while setting up unity config for policies, ideally the policies should be set in the order viz. 
        /// fallback, timeout, retry and then circuit breaker</param>
        /// <param name="logger"></param>
        /// <param name="shouldThrowOnException"></param>
        public BasicPollyExceptionHandler(IPolicy[] policies, ILogger logger, bool shouldThrowOnException)
        {
            _logger = logger;
            _shouldThrowOnException = shouldThrowOnException;
            _policies = policies;
            if (_policyBuilder.IsNull() && _staticLogger.IsNull())
            {
                _staticLogger = logger;
                _policyBuilder = BuildPolicyBuilderFromPollyTransientFailureExceptionsXMLFile();
            }
        }

        public override void HandleException(Action action, Action onExceptionCompensatingHandler = null)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtility.WrapActionWithExceptionHandling(() =>
                {
                    if (CheckIfExceptionsNeedsToBePollyHandled(ex))
                    {
                        PolicyWrap policyWrap = GetPolicyWrapWithProperFallbackActionSetForFallbackPolicies(ex, onExceptionCompensatingHandler);
                        policyWrap.Execute(action);
                    }
                }, _logger);
                HandleExceptionWithThrowCondition(ex, onExceptionCompensatingHandler);
            }
        }

        public override TReturn HandleException<TReturn>(Func<TReturn> action, Action onExceptionCompensatingHandler = null)
        {
            TReturn returnValue = default(TReturn);
            try
            {
                returnValue = action();
            }
            catch (Exception ex)
            {
                returnValue = ExceptionHandlingUtility.WrapFuncWithExceptionHandling(() =>
                {
                    if (CheckIfExceptionsNeedsToBePollyHandled(ex))
                    {
                        PolicyWrap policyWrap = GetPolicyWrapWithProperFallbackActionSetForFallbackPolicies(ex, onExceptionCompensatingHandler);
                        return policyWrap.Execute(action);
                    }
                    return default(TReturn);
                }, _logger);
                HandleExceptionWithThrowCondition(ex, onExceptionCompensatingHandler);
            }
            return returnValue;
        }

        public override async Task HandleExceptionAsync(Func<CancellationToken, Task> action, CancellationToken actionCancellationToken = default, Func<CancellationToken, Task> onExceptionCompensatingHandler = null, CancellationToken onExceptionCompensatingHandlerCancellationToken = default)
        {
            try
            {
                await action(actionCancellationToken);
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtility.WrapActionWithExceptionHandling(async () =>
                {
                    if (CheckIfExceptionsNeedsToBePollyHandled(ex))
                    {
                        AsyncPolicyWrap policyWrap = GetPolicyWrapWithProperFallbackActionSetForFallbackPoliciesAsync(ex, onExceptionCompensatingHandler);
                        await policyWrap.ExecuteAsync(action, actionCancellationToken);
                    }
                }, _logger);
               await HandleExceptionWithThrowCondition(ex, onExceptionCompensatingHandler, onExceptionCompensatingHandlerCancellationToken);
            }
        }

        public override async Task<TReturn> HandleExceptionAsync<TReturn>(Func<CancellationToken, Task<TReturn>> func, CancellationToken funcCancellationToken = default, Func<CancellationToken, Task> onExceptionCompensatingHandler = null, CancellationToken onExceptionCompensatingHandlerCancellationToken = default)
        {
            Task<TReturn> returnValue = default;
            try
            {
                var returnValueFromFunc = await func(funcCancellationToken);
                if(returnValueFromFunc is Task<TReturn>)
                {
                    returnValue = returnValueFromFunc as Task<TReturn>;
                }
                else
                {
                    returnValue = Task.FromResult(returnValueFromFunc);
                }
            }
            catch (Exception ex)
            {
                returnValue = await ExceptionHandlingUtility.WrapFuncWithExceptionHandling(async () =>
                 {
                     if (CheckIfExceptionsNeedsToBePollyHandled(ex))
                     {
                         AsyncPolicyWrap policyWrap = GetPolicyWrapWithProperFallbackActionSetForFallbackPoliciesAsync(ex, onExceptionCompensatingHandler);
                         return await policyWrap.ExecuteAsync(func, funcCancellationToken) as Task<TReturn>;
                     }
                     return default(Task<TReturn>);
                 }, _logger);

                await HandleExceptionWithThrowCondition(ex, onExceptionCompensatingHandler, onExceptionCompensatingHandlerCancellationToken);
            }
            return await returnValue;
        }

        private static PolicyBuilder BuildPolicyBuilderFromPollyTransientFailureExceptionsXMLFile()
        {
            return ExceptionHandlingUtility.WrapFuncWithExceptionHandling(() =>
            {
                PolicyBuilder policyBuilder = null;
                Assembly currentAssembly = typeof(BasicPollyExceptionHandler).Assembly;
                var pollyTransientFailureExceptionsEmbeddedResourceFilename = currentAssembly.GetManifestResourceNames()[0];
                XDocument xDoc = XDocument.Load(currentAssembly.GetManifestResourceStream(pollyTransientFailureExceptionsEmbeddedResourceFilename));
                _pollyTransientFailureExceptions = XMLUtility.DeSerialize<PollyTransientFailureExceptions>(xDoc.ToString());
                if (_pollyTransientFailureExceptions.TransientFailureExceptions.IsNotNull())
                {
                    _splittedTransientFailureExceptions = _pollyTransientFailureExceptions.TransientFailureExceptions.SelectMany(x => x.CommaSeperatedTransientFailureExceptions.Split(",", StringSplitOptions.RemoveEmptyEntries))
                                                          .Distinct().Select(x => x.Trim().ToLower());
                }

                if (_splittedTransientFailureExceptions.IsNotNullOrEmpty())
                {
                    string firstTransientFailureException = _splittedTransientFailureExceptions.First();
                    string assemblyName = _pollyTransientFailureExceptions.TransientFailureExceptions.SingleOrDefault(x => x.CommaSeperatedTransientFailureExceptions.Contains(firstTransientFailureException)).AssemblyName;
                    Type firstTransientFailureExceptionType = MetaDataUtility.GetType(assemblyName, firstTransientFailureException);
                    Type[] transientFailureExceptionTypesArray = new Type[1];
                    transientFailureExceptionTypesArray[0] = firstTransientFailureExceptionType;
                    policyBuilder = MetaDataUtility.InvokeStaticMethod<Policy, PolicyBuilder>("Handle", transientFailureExceptionTypesArray);

                    IEnumerable<string> transientFailureExceptionsOtherThanTheFirst = _splittedTransientFailureExceptions.Skip(1);
                    if (transientFailureExceptionsOtherThanTheFirst.IsNotNullOrEmpty())
                    {
                        transientFailureExceptionsOtherThanTheFirst.ForEach(x =>
                         {
                             assemblyName = _pollyTransientFailureExceptions.TransientFailureExceptions.SingleOrDefault(y => y.CommaSeperatedTransientFailureExceptions.Contains(x)).AssemblyName;
                             Type transientFailureExceptionTypeForOtherThanTheFirst = MetaDataUtility.GetType(assemblyName, x);
                             Type[] transientFailureExceptionTypesArrayForOtherThanTheFirst = new Type[1];
                             transientFailureExceptionTypesArrayForOtherThanTheFirst[0] = transientFailureExceptionTypeForOtherThanTheFirst;
                             policyBuilder = MetaDataUtility.InvokeInstanceMethod<PolicyBuilder, PolicyBuilder>(policyBuilder, "Or", transientFailureExceptionTypesArrayForOtherThanTheFirst);
                         }
                        );
                    }
                }
                return policyBuilder;
            }, _staticLogger);
        }

        private PolicyWrap GetPolicyWrapWithProperFallbackActionSetForFallbackPolicies(Exception ex, Action fallbackAction)
        {
            IEnumerable<IPolicy> policiesForCurrentException = GetPoliciesForException(ex);
            if (fallbackAction.IsNotNull())
            {
                _areFallbackPoliciesAlreadyHandled = true;
                policiesForCurrentException.Where(x => x is IFallbackActionPolicy).Select(x => x as IFallbackActionPolicy).ForEach(x => x.SetFallbackAction(fallbackAction));
            }
            else
            {
                _areFallbackPoliciesAlreadyHandled = false;
                policiesForCurrentException = policiesForCurrentException.Where(x => !(x is IFallbackActionPolicy));
            }
            return _policyWrapForSyncOperationsFunc(policiesForCurrentException);
        }

        private AsyncPolicyWrap GetPolicyWrapWithProperFallbackActionSetForFallbackPoliciesAsync(Exception ex, Func<CancellationToken, Task> fallbackAction)
        {
            IEnumerable<IPolicy> policiesForCurrentException = GetPoliciesForException(ex);
            if (fallbackAction.IsNotNull())
            {
                _areFallbackPoliciesAlreadyHandled = true;
                policiesForCurrentException.Where(x => x is IFallbackActionPolicy).Select(x => x as IFallbackActionPolicy).ForEach(x => x.SetFallbackAction(fallbackAction));
            }
            else
            {
                _areFallbackPoliciesAlreadyHandled = false;
                policiesForCurrentException = policiesForCurrentException.Where(x => !(x is IFallbackActionPolicy));
            }
            return _policyWrapForAsyncOperationsFunc(policiesForCurrentException);
        }

        private IEnumerable<IPolicy> GetPoliciesForException(Exception ex)
        {
            string exceptionTypeName = ex.GetType().Name;
            IEnumerable<string> pollyExceptionPoliciesFromXMLFileToBeAppliedForCurrentException = _pollyTransientFailureExceptions.TransientFailureExceptions
                                                                                                 .SingleOrDefault(x => x.CommaSeperatedTransientFailureExceptions.Contains(exceptionTypeName))
                                                                                                 .CommaSeperatedPollyPoliciesNames.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                                                                                 .Distinct();
            IEnumerable<IPolicy> clonedPolicies = CloningUtility.Clone(_policies);
            clonedPolicies = clonedPolicies.Where(x => pollyExceptionPoliciesFromXMLFileToBeAppliedForCurrentException.Contains(x.GetType().Name));
            IEnumerable<string> pollyExceptionPoliciesPresentInXMLFileForCurrentExceptionButNotInjectedInDependencies =
                                pollyExceptionPoliciesFromXMLFileToBeAppliedForCurrentException.Except(clonedPolicies.Select(x => x.GetType().Name));
            if (pollyExceptionPoliciesPresentInXMLFileForCurrentExceptionButNotInjectedInDependencies.IsNotNullOrEmpty())
            {
                _logger.LogWarning("The following transient failures are part of the polly exception xml file for the current exception viz. "
                                    + exceptionTypeName + " but not injected as part of the dependencies : " + Environment.NewLine
                                    + pollyExceptionPoliciesPresentInXMLFileForCurrentExceptionButNotInjectedInDependencies.Aggregate((a, b) => a + Environment.NewLine + b));
            }
            return clonedPolicies;
        }

        private void HandleExceptionWithThrowCondition(Exception ex, Action onExceptionCompensatingHandler)
        {
            _logger.LogError(ex, ex.Message);
            if (onExceptionCompensatingHandler.IsNotNull() && !_areFallbackPoliciesAlreadyHandled)
            {
                _areFallbackPoliciesAlreadyHandled = true;
                onExceptionCompensatingHandler();
            }
            if (_shouldThrowOnException)
            {
                throw new Exception("Check Inner Exception", ex);
            }

        }

        private async Task HandleExceptionWithThrowCondition(Exception ex, Func<CancellationToken, Task> onExceptionCompensatingHandler, CancellationToken onExceptionCompensatingHandlerCancellationToken)
        {
            await Task.Run(()=>_logger.LogError(ex, ex.Message),default);
            if (onExceptionCompensatingHandler.IsNotNull() && !_areFallbackPoliciesAlreadyHandled)
            {
                _areFallbackPoliciesAlreadyHandled = true;
               await onExceptionCompensatingHandler(onExceptionCompensatingHandlerCancellationToken);
            }
            if (_shouldThrowOnException)
            {
                throw new Exception("Check Inner Exception", ex);
            }
        }

        private bool CheckIfExceptionsNeedsToBePollyHandled(Exception ex)
        {
            var flattenedExceptions = ex.FromHierarchy(x => x.InnerException);
            flattenedExceptions.ToList().Add(ex);
            flattenedExceptions = flattenedExceptions.Distinct();
            return _policies.IsNotNullOrEmpty() 
                   && _splittedTransientFailureExceptions.IsNotNullOrEmpty() 
                   && flattenedExceptions.Any(x => _splittedTransientFailureExceptions.Any(y => x.GetType().Name.Trim().ToLower().Contains(y)));
        }
    }
}
