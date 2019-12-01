using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WEB.Models.Authorization
{
    public class ProfileModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public Guid UserId { get; set; }
        public List<string> Roles { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }

    public class RegisterDTO
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class ResetPasswordDTO
    {
        [Required]
        public string UserName { get; set; }
    }

    public class ResetDTO
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Token { get; set; }
    }

    public class ChangePasswordDTO
    {
        [Required]
        public string CurrentPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
    }
}
