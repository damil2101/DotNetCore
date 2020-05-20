using System;
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
        [Required]
        public string Gender { get; set; }
        [Required]        
        public string KnownAs { get; set; }
        [Required]
        public DateTime DOB { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        
        public UserForRegisterDto()
        {   
            Created = DateTime.Now;
            LastActive = DateTime.Now;
        }
    }
}