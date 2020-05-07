using System.ComponentModel.DataAnnotations;

namespace DatingApp.api.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string username { get; set; }
        
        [Required]
        [StringLength(12, MinimumLength = 6, ErrorMessage = "You must specify a password between 6 and 12 chars")]
        public string password { get; set; }
        
    }
}