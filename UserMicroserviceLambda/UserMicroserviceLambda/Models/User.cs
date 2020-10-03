using Domain.Base.AddOnObjects;
using Domain.Base.Aggregates;
using Domain.Base.Entities.Composites;
using RestfulWebAPINetCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace UserMicroserviceLambda.Models
{
    public class User : BaseEntityComposite<int, AuditInfo>, ICommandAggregateRoot, IQueryableAggregateRoot
    {
        [Required]
        public LoginData LoginData { get; set; }

        public byte[] PasswordHash { get; set; }

        public byte[] PasswordSalt { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PAN { get; set; }

        [Required]
        public string ContactNumber { get; set; }

        [Required]
        public DateTimeOffset DateOfBirth { get; set; }

        [Required]
        public AccountType AccountType { get; set; }

        public Role Role { get; set; }
    }
}
