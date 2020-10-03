using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserMicroserviceLambda.Models
{
    public class LoginData
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [NotMapped]
        public string Password { get; set; }
    }
}
