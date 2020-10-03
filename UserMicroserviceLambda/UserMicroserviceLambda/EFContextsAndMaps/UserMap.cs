using UserMicroserviceLambda.Models;
using DomainContextsAndMaps.Base.EFBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestfulWebAPINetCore;

namespace UserMicroserviceLambda.EFContextsAndMaps
{
    public class UserMap : BaseEntityWithAuditInfoRootMap<User, int>
    {
        public override string TableName => "tbl_Users";

        protected override string IDColumnName => "col_id";

        protected override void SetEntitySpecificProperties(EntityTypeBuilder<User> builder)
        {
            builder.OwnsOne(p=>p.LoginData).Property(p => p.UserName).HasColumnName("col_usr_name").IsRequired();
            builder.Property(p => p.PasswordHash).HasColumnName("col_pwd_hash").IsRequired();
            builder.Property(p => p.PasswordSalt).HasColumnName("col_pwd_salt").IsRequired();
            builder.Property(p => p.Name).HasColumnName("col_name").IsRequired();
            builder.Property(p => p.Address).HasColumnName("col_addr").IsRequired();
            builder.Property(p => p.State).HasColumnName("col_state").IsRequired();
            builder.Property(p => p.Country).HasColumnName("col_country").IsRequired();
            builder.Property(p => p.Email).HasColumnName("col_mail").IsRequired();
            builder.Property(p => p.PAN).HasColumnName("col_pan").IsRequired();
            builder.Property(p => p.ContactNumber).HasColumnName("col_phone").IsRequired();
            builder.Property(p => p.DateOfBirth).HasColumnName("col_dob").IsRequired();
            builder.Property(p => p.AccountType).HasColumnName("col_acct_typ").HasConversion(x => (int)x, x => (AccountType)x).IsRequired();
            builder.Property(p => p.Role).HasColumnName("col_role").HasDefaultValue(Role.Customer).HasConversion(x => (int)x, x => (Role)x);
        }
    }
}
