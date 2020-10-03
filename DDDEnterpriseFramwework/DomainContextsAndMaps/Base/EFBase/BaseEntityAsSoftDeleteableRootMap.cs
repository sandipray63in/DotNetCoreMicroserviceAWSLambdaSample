using System;
using Domain.Base.AddOnObjects;
using Domain.Base.Entities.Composites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainContextsAndMaps.Base.EFBase
{
    public abstract class BaseEntityAsSoftDeleteableRootMap<TEntity, TId> : BaseEntityRootMap<TEntity, TId>
        where TEntity : BaseEntityComposite<TId, SoftDeleteableInfo>
        where TId : struct
    {
        protected virtual string IsDeletedColumnName { get; } = "col_is_deleted";
        protected virtual string DeletedOnColumnName { get; } = "col_deleted_on";

        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);
            ExtendIsDeletedPropertyWithOtherConfigurations(builder.Property(p => p.T1Data.IsDeleted).HasColumnName(IsDeletedColumnName));
            var deletedOnProperty = builder.Property(p => p.T1Data.DeletedOn).HasColumnName(DeletedOnColumnName);
            deletedOnProperty.ValueGeneratedOnUpdate();
            ExtendDeletedOnPropertyWithOtherConfigurations(deletedOnProperty);
        }

        #region Overrideable Configuration Extensions

        protected virtual void ExtendIsDeletedPropertyWithOtherConfigurations(PropertyBuilder<bool> isDeletedPropertyBuilder) { }

        protected virtual void ExtendDeletedOnPropertyWithOtherConfigurations(PropertyBuilder<DateTimeOffset?> deletedOnPropertyBuilder) { }

        #endregion
    }
}