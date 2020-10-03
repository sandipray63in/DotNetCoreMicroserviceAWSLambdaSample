using Domain.Base.AddOnObjects;
using LoanMicroserviceLambda.Models;
using System;

namespace LoanMicroserviceLambda.Tests
{
    public static class LoanFakes
    {
        public static Loan GetFakeLoan()
        {
            return new Loan
            {
                Id = 1,
                CustomerID = 2,
                Amount = 10000,
                DurationInMonths = 30,
                LoanType = LoanType.Home,
                LoanDate = DateTime.Now,
                RateOfInterestInPercentage = 11,
                T1Data = new AuditInfo
                { 
                    CreatedBy = "Sandip",
                    CreatedOn = DateTime.Now
                }
            };
        }
    }
}
