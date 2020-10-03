using System;
using Infrastructure.Utilities;
using Domain.Base.AddOnObjects;
using Domain.Base.Entities.Composites;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace DomainContextsAndMaps.Base.EFBase
{
    public abstract class BaseEntityWithAuditInfoRootMap<TEntity, TId> : BaseEntityRootMap<TEntity, TId>
        where TEntity : BaseEntityComposite<TId, AuditInfo>
        where TId : struct
    {
        #region Actual Audit Columns Overridables

        protected virtual string CreatedByColumnName { get; } = "col_created_by";
        protected virtual string CreationDateColumnName { get; } = "col_creation_date";
        protected virtual string LastUpdatedByColumnName { get; } = "col_last_updated_by";
        protected virtual string LastUpdationDateColumnName { get; } = "col_last_updation_date";

        #endregion

        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            ContractUtility.Requires<ArgumentNullException>(CreatedByColumnName.IsNotNullOrWhiteSpace(), "CreatedByColumnName cannot be null or empty.");
            ContractUtility.Requires<ArgumentNullException>(CreationDateColumnName.IsNotNullOrWhiteSpace(), "CreationDateColumnName cannot be null or empty.");
            ContractUtility.Requires<ArgumentNullException>(LastUpdatedByColumnName.IsNotNullOrWhiteSpace(), "LastUpdatedByColumnName Name cannot be null or empty.");
            ContractUtility.Requires<ArgumentNullException>(LastUpdationDateColumnName.IsNotNullOrWhiteSpace(), "LastUpdationDateColumnName Name cannot be null or empty.");

            base.Configure(builder);

            ExtendCreatedByPropertyWithOtherConfigurations(builder.OwnsOne(p=>p.T1Data).Property(p => p.CreatedBy).HasColumnName(CreatedByColumnName));
            var createdOnProperty = builder.OwnsOne(p=>p.T1Data).Property(p => p.CreatedOn).HasColumnName(CreationDateColumnName);
            createdOnProperty.ValueGeneratedOnAdd();
            ExtendCreationDatePropertyWithOtherConfigurations(createdOnProperty);
            ExtendLastUpdatedByPropertyWithOtherConfigurations(builder.OwnsOne(p => p.T1Data).Property(p => p.LastUpdatedBy).HasColumnName(LastUpdatedByColumnName));
            var lastUpdatedOnProperty = builder.OwnsOne(p => p.T1Data).Property(p => p.LastUpdateOn).HasColumnName(LastUpdationDateColumnName);
            lastUpdatedOnProperty.ValueGeneratedOnUpdate();
            ExtendLastUpdatedDatePropertyWithOtherConfigurations(lastUpdatedOnProperty);
        }

        #region Overrideable Configuration Extensions

        protected virtual void ExtendCreatedByPropertyWithOtherConfigurations(PropertyBuilder<string> createdByPropertyBuilder) { }

        protected virtual void ExtendCreationDatePropertyWithOtherConfigurations(PropertyBuilder<DateTimeOffset> CreatedOnPropertyBuilder) { }

        protected virtual void ExtendLastUpdatedByPropertyWithOtherConfigurations(PropertyBuilder<string> lastUpdatedByPropertyBuilder) { }

        protected virtual void ExtendLastUpdatedDatePropertyWithOtherConfigurations(PropertyBuilder<DateTimeOffset?> LastUpdateOnPropertyBuilder) { }

        #endregion 
    }
}
