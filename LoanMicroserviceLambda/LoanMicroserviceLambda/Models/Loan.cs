using Domain.Base.AddOnObjects;
using Domain.Base.Aggregates;
using Domain.Base.Entities.Composites;
using System;
using System.ComponentModel.DataAnnotations;

namespace LoanMicroserviceLambda.Models
{
    public class Loan : BaseEntityComposite<int, AuditInfo>, ICommandAggregateRoot, IQueryableAggregateRoot
    {
        public int CustomerID { get; set; }

        [Required]
        public LoanType LoanType { get; set; }

        [Required]
        public int Amount { get; set; }

        [Required]
        public DateTimeOffset LoanDate { get; set; }

        [Required]
        public int RateOfInterestInPercentage { get; set; }

        [Required]
        public int DurationInMonths { get; set; }
    }
}
