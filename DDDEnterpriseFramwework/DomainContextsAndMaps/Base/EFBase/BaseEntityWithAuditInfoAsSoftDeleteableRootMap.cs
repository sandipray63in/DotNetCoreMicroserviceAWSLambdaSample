using Domain.Base.AddOnObjects;
using Domain.Base.Entities.Composites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace DomainContextsAndMaps.Base.EFBase
{
    public abstract class BaseEntityWithAuditInfoAsSoftDeleteableRootMap<TEntity, TId> : BaseEntityWithAuditInfoRootMap<TEntity, TId>
        where TEntity : BaseEntityComposite<TId, AuditInfo, SoftDeleteableInfo>
        where TId : struct
    {
        protected virtual string IsDeletedColumnName { get; } = "col_is_deleted";
        protected virtual string DeletedOnColumnName { get; } = "col_deleted_on";

        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);
            ExtendIsDeletedPropertyWithOtherConfigurations(builder.Property(p => p.T2Data.IsDeleted).HasColumnName(IsDeletedColumnName));
            var deletedOnProperty = builder.Property(p => p.T2Data.DeletedOn).HasColumnName(DeletedOnColumnName);
            deletedOnProperty.ValueGeneratedOnUpdate();
            ExtendDeletedOnPropertyWithOtherConfigurations(deletedOnProperty);
        }

        #region Overrideable Configuration Extensions

        protected virtual void ExtendIsDeletedPropertyWithOtherConfigurations(PropertyBuilder<bool> propertyConfig) { }

        protected virtual void ExtendDeletedOnPropertyWithOtherConfigurations(PropertyBuilder<DateTimeOffset?> propertyConfig) { }

        #endregion
    }
}
