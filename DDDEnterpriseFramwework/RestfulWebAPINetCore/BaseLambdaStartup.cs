using Domain.Base.Aggregates;
using Domain.Base.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace RestfulWebAPINetCore
{
    public abstract class BaseLambdaStartup<TDBContext, TEntity, TId> : BaseStartup<TDBContext, TEntity, TId>
        where TDBContext : DbContext
        where TEntity : BaseEntity<TId>, ICommandAggregateRoot, IQueryableAggregateRoot
        where TId : struct
    {
        public BaseLambdaStartup(IConfiguration configuration) : base(configuration)
        {
        }

        protected abstract string LoggingCategoryName { get; }

        protected override void ConfigureLoggingService(IServiceCollection services)
        {
            // Create and populate LambdaLoggerOptions object
            var loggerOptions = new LambdaLoggerOptions();
            loggerOptions.IncludeCategory = true;
            loggerOptions.IncludeLogLevel = true;
            loggerOptions.IncludeNewline = true;
            loggerOptions.IncludeException = true;
            loggerOptions.IncludeEventId = true;
            loggerOptions.IncludeScopes = true;
            // Configure Filter to only log some 
            loggerOptions.Filter = (category, logLevel) =>
            {
                // For some categories, only log events with minimum LogLevel
                if (string.Equals(category, "Default", StringComparison.Ordinal))
                {
                    return (logLevel >= LogLevel.Debug);
                }
                if (string.Equals(category, "Microsoft", StringComparison.Ordinal))
                {
                    return (logLevel >= LogLevel.Information);
                }
                // Log everything else
                return true;
            };
            var loggerFactory = (ILoggerFactory)new LoggerFactory();
            loggerFactory.AddLambdaLogger(loggerOptions);
            var logger = loggerFactory.CreateLogger(LoggingCategoryName);
            services.AddSingleton(logger);
        }

        protected override void ConfigureDistributedCachingService(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();//TODO - Setup AWS Elastic cache for Redis and use that url
        }
    }
}

