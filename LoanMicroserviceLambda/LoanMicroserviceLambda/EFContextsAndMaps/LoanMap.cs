using DomainContextsAndMaps.Base.EFBase;
using LoanMicroserviceLambda.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanMicroserviceLambda.EFContextsAndMaps
{
    public class LoanMap : BaseEntityWithAuditInfoRootMap<Loan, int>
    {
        public override string TableName => "tbl_Loans";

        protected override string IDColumnName => "col_id";

        protected override void SetEntitySpecificProperties(EntityTypeBuilder<Loan> builder)
        {
            builder.Property(p => p.CustomerID).HasColumnName("col_customer_id").IsRequired();
            builder.Property(p => p.LoanType).HasColumnName("col_loan_typ").HasConversion(x => (int)x, x => (LoanType)x).IsRequired();
            builder.Property(p => p.Amount).HasColumnName("col_amt").IsRequired();
            builder.Property(p => p.LoanDate).HasColumnName("col_loan_dt").IsRequired();
            builder.Property(p => p.RateOfInterestInPercentage).HasColumnName("col_rt_of_int").IsRequired();
            builder.Property(p => p.DurationInMonths).HasColumnName("col_dur_in_mons").IsRequired();
        }
    }
}
