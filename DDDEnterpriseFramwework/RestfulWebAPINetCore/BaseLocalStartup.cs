using Domain.Base.Aggregates;
using Domain.Base.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RestfulWebAPINetCore
{
    public abstract class BaseLocalStartup<TDBContext, TEntity, TId> : BaseStartup<TDBContext, TEntity, TId>
        where TDBContext : DbContext
        where TEntity : BaseEntity<TId>, ICommandAggregateRoot, IQueryableAggregateRoot
        where TId : struct
    {
        public BaseLocalStartup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void ConfigureDistributedCachingService(IServiceCollection services)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost";
                options.InstanceName = "CustomerLoans";
            });
        }
    }
}
