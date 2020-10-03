using System;
using UserMicroserviceLambda.Models;

namespace UserMicroserviceLambda.Tests
{
    public static class UserFakes
    {
        public static User GetFakeAdminUser()
        {
            return new User
            {
                Id = 1,
                AccountType = AccountType.Savings,
                Address = "123",
                ContactNumber = "9786878546",
                Country = "India",
                DateOfBirth = DateTimeOffset.MinValue,
                Email = "xyz@xyz.com",
                LoginData = new LoginData
                {
                    UserName = "xyz123",
                    Password = "fgdfgdgfdfgdggfd"
                },
                Name = "Sandip",
                PAN = "Drt4567",
                Role = RestfulWebAPINetCore.Role.Admin,
                State = "Kolkata",
                T1Data = new Domain.Base.AddOnObjects.AuditInfo
                {
                    CreatedBy = "Sandip",
                    CreatedOn = DateTimeOffset.UtcNow
                }
            };
        }

        public static User GetFakeCustomerUser()
        {
            return new User
            {
                Id = 2,
                AccountType = AccountType.Savings,
                Address = "12345",
                ContactNumber = "9786121546",
                Country = "India",
                DateOfBirth = DateTimeOffset.MinValue,
                Email = "xyz@123.com",
                LoginData = new LoginData
                {
                    UserName = "xyz456",
                    Password = "lkjhljklljjkklj"
                },
                Name = "Sandip123",
                PAN = "Drt4654",
                Role = RestfulWebAPINetCore.Role.Customer,
                State = "Kolkata",
                T1Data = new Domain.Base.AddOnObjects.AuditInfo
                {
                    CreatedBy = "Sandip",
                    CreatedOn = DateTimeOffset.UtcNow
                }
            };
        }
    }
}
