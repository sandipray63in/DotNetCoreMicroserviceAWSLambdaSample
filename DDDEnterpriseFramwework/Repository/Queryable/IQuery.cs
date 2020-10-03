using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Domain.Base.Aggregates;

namespace Repository.Queryable
{
    public interface IQuery<TEntity> : IQueryable<TEntity> where TEntity : IQueryableAggregateRoot
    {
        IQueryable<TEntity> Include(Expression<Func<TEntity, object>> subSelector);
    }
}
