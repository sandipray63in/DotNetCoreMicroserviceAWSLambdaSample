using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Base.AddOnObjects;
using Domain.Base.Aggregates;
using Domain.Base.Entities.Composites;
using Domain.Base.Entities;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Repository.Command
{
    public class EntityFrameworkCodeFirstCommand<TEntity, TId> : ICommand<TEntity>
        where TEntity : BaseEntity<TId>, ICommandAggregateRoot
        where TId : struct
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;

        public EntityFrameworkCodeFirstCommand(DbContext dbContext)
        {
            ContractUtility.Requires<ArgumentNullException>(dbContext.IsNotNull(), "dbContext instance cannot be null");
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<TEntity>();
        }

        #region ICommand<T> Members

        /// <summary>
        /// One good stuff about Entity Framework is that the value of the primary key gets automatically updated
        /// within the supplied object(here the object is "item").One doesn't need to explicitly return some type here
        /// after calling _dbContext.SaveChanges(). 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public void Insert(TEntity item)
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            EntityEntry<TEntity> entry = _dbContext.Entry(item);
            if (entry.State != EntityState.Detached)
            {
                entry.State = EntityState.Added;
            }
            else
            {
                _dbSet.Add(item);
            }
            SaveChanges();
        }

        public void Update(TEntity item)
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            EntityEntry entry = _dbContext.Entry(item);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(item);
            }
            entry.State = EntityState.Modified;
            SaveChanges();
        }

        public void Delete(TEntity item)
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            EntityEntry entry = _dbContext.Entry(item);
            if (entry.State != EntityState.Deleted)
            {
                entry.State = EntityState.Deleted;
            }
            else
            {
                _dbSet.Attach(item);
                _dbSet.Remove(item);
            }
            SaveChanges();
        }

        //Cascading effects to be taken care at Domain level.
        public void Insert(IEnumerable<TEntity> items)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            items.ForEach(item =>
             {
                 EntityEntry entry = _dbContext.Entry(item);
                if (entry.State != EntityState.Detached)
                {
                    entry.State = EntityState.Added;
                }
                else
                {
                    _dbSet.Add(item);
                }
             });
            SaveChanges();
        }

        //Cascading effects to be taken care at Domain level.
        public void Update(IEnumerable<TEntity> items)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            items.ForEach(item =>
            {
                EntityEntry entry = _dbContext.Entry(item);
                if (entry.State == EntityState.Detached)
                {
                    _dbSet.Attach(item);
                }
                entry.State = EntityState.Modified;
            });
            SaveChanges();
        }

        //Cascading effects to be taken care at Domain level.
        public void Delete(IEnumerable<TEntity> items)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            items.ForEach(item =>
            {
                EntityEntry entry = _dbContext.Entry(item);
                if (entry.State != EntityState.Deleted)
                {
                    entry.State = EntityState.Deleted;
                }
                else
                {
                    _dbSet.Attach(item);
                    _dbSet.Remove(item);
                }
            });
            SaveChanges();
        }

        public async Task InsertAsync(TEntity item, CancellationToken token = default)
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            EntityEntry entry = _dbContext.Entry(item);
            if (entry.State != EntityState.Detached)
            {
                entry.State = EntityState.Added;
            }
            else
            {
                await _dbSet.AddAsync(item);
            }
           await SaveChangesAsync(token);
        }

        public async Task UpdateAsync(TEntity item, CancellationToken token = default)
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            EntityEntry entry = _dbContext.Entry(item);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(item);
            }
            entry.State = EntityState.Modified;
            await SaveChangesAsync(token);
        }

        public async Task DeleteAsync(TEntity item, CancellationToken token = default)
        {
            ContractUtility.Requires<ArgumentNullException>(item.IsNotNull(), "item instance cannot be null");
            EntityEntry entry = _dbContext.Entry(item);
            if (entry.State != EntityState.Deleted)
            {
                entry.State = EntityState.Deleted;
            }
            else
            {
                _dbSet.Attach(item);
                _dbSet.Remove(item);
            }
            await SaveChangesAsync(token);
        }

        public async Task InsertAsync(IEnumerable<TEntity> items, CancellationToken token = default)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            items.ForEach(async item =>
            {
                EntityEntry entry = _dbContext.Entry(item);
                if (entry.State != EntityState.Detached)
                {
                    entry.State = EntityState.Added;
                }
                else
                {
                    await _dbSet.AddAsync(item);
                }
            });
            await SaveChangesAsync(token);
        }

        public async Task UpdateAsync(IEnumerable<TEntity> items, CancellationToken token = default)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            items.ForEach(item =>
            {
                EntityEntry entry = _dbContext.Entry(item);
                if (entry.State == EntityState.Detached)
                {
                    _dbSet.Attach(item);
                }
                entry.State = EntityState.Modified;
            });
            await SaveChangesAsync(token);
        }

        public async Task DeleteAsync(IEnumerable<TEntity> items, CancellationToken token = default)
        {
            ContractUtility.Requires<ArgumentNullException>(items.IsNotNull(), "items instance cannot be null");
            ContractUtility.Requires<ArgumentOutOfRangeException>(items.IsNotEmpty(), "items count should be greater than 0");
            items.ForEach(item =>
            {
                EntityEntry entry = _dbContext.Entry(item);
                if (entry.State != EntityState.Deleted)
                {
                    entry.State = EntityState.Deleted;
                }
                else
                {
                    _dbSet.Attach(item);
                    _dbSet.Remove(item);
                }
            });
            await SaveChangesAsync(token);
        }

        #endregion

        private void SaveChanges()
        {
            ApplyAuditInfoRules();
            _dbContext.SaveChanges();
        }

        private async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                ApplyAuditInfoRules();
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                //Do Nothing
            }
        }

        private void ApplyAuditInfoRules()
        {
            IEnumerable<EntityEntry> changedAudits = _dbContext.ChangeTracker.Entries()
                                 .Where(e => e.Entity is BaseEntityComposite<TId,AuditInfo>
                                 && ((e.State == EntityState.Added)
                                 || (e.State == EntityState.Modified)));

            changedAudits.ForEach(entry =>
            {
                var entity = entry.Entity as BaseEntityComposite<TId, AuditInfo>;
                if (entity.T1Data.IsNotNull())
                {
                    if (entry.State == EntityState.Added)
                    {
                        if (!entity.T1Data.PreserveCreatedOn)
                        {
                            entity.T1Data.CreatedOn = DateTime.Now;
                        }
                    }
                    else
                    {
                        entity.T1Data.LastUpdateOn = DateTime.Now;
                    }
                }
            });
        }
    }
}
