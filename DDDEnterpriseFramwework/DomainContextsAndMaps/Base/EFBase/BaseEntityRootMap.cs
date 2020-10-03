using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Base.Entities;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainContextsAndMaps.Base.EFBase
{
    public abstract class BaseEntityRootMap<TEntity, TId> : IEntityTypeConfiguration<TEntity>
        where TEntity : BaseEntity<TId>
        where TId : struct
    {
        protected abstract string IDColumnName { get; }

        protected virtual DatabaseGeneratedOption DbIdGenerationOption { get; } = DatabaseGeneratedOption.Identity;

        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            ContractUtility.Requires<ArgumentNullException>(IDColumnName.IsNotNullOrWhiteSpace(), "IDColumnName cannot be null or empty.");
            builder.HasKey(p => p.Id);
            var idProperty = builder.Property(p => p.Id).HasColumnName(IDColumnName);
            if(DbIdGenerationOption == DatabaseGeneratedOption.Identity)
            {
                idProperty.ValueGeneratedOnAdd();
            }
            SetEntitySpecificProperties(builder);
            builder.ToTable<TEntity>(TableName);
        }

        public abstract string TableName { get;}

        protected abstract void SetEntitySpecificProperties(EntityTypeBuilder<TEntity> builder);

        #region Overrideable Configuration Extensions

        protected virtual void ExtendKeyIDWithOtherConfigurations(KeyBuilder keyBuilder) { }

        protected virtual void ExtendPropertyIDWithOtherConfigurations(PropertyBuilder<TId> propertyBuilder) { }

        #endregion 
    }
}
