using Domain.Base.Entities;

namespace UserMicroserviceLambda.Models
{
    public class UserDataForUpdate : BaseEntity<int>
    {
        public string Password { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string Email { get; set; }

        public string PAN { get; set; }

        public string ContactNumber { get; set; }
    }
}
