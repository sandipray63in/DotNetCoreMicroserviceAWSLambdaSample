using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Domain.Base.Aggregates;
using Infrastructure;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Repository.Queryable
{
    public class EntityFrameworkCodeFirstQueryable<TEntity> : IQuery<TEntity> where TEntity : class, IQueryableAggregateRoot
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;

        public EntityFrameworkCodeFirstQueryable(DbContext dbContext)
        {
            ContractUtility.Requires<ArgumentNullException>(dbContext.IsNotNull(), "dbContext instance cannot be null");
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<TEntity>();
        }

        #region IQueryable<T> Members

        public IQueryable<TEntity> Include(Expression<Func<TEntity, object>> subSelector)
        {
            ContractUtility.Requires<ArgumentNullException>(subSelector.IsNotNull(), "subSelector instance cannot be null");
            return _dbSet.Include(subSelector);
        }
        #endregion

        #region Facilitation for LINQ based Selects,JOINs etc from the classes, using the instance of this class
        public IEnumerator<TEntity> GetEnumerator()
        {
            return _dbSet.AsQueryable().GetEnumerator();
        }

        public Type ElementType
        {
            get { return _dbSet.AsQueryable().ElementType; }
        }

        public Expression Expression
        {
            get { return _dbSet.AsQueryable().Expression; }
        }

        public IQueryProvider Provider
        {
            get { return _dbSet.AsQueryable().Provider; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
