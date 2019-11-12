using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WEB.Models
{
    public class UserDTO
    {
        [Required]
        public Guid Id { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false), MaxLength(50)]
        public string FirstName { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false), MaxLength(50)]
        public string LastName { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public IList<Guid> RoleIds { get; set; }

    }

    public static partial class ModelFactory
    {
        public static UserDTO Create(User user)
        {
            if (user == null) return null;

            var roleIds = new List<Guid>();
            if (user.Roles != null)
                foreach (var role in user.Roles)
                    roleIds.Add(role.RoleId);

            var userDTO = new UserDTO();

            userDTO.Id = user.Id;
            userDTO.FirstName = user.FirstName;
            userDTO.LastName = user.LastName;
            userDTO.FullName = user.FullName;
            userDTO.Email = user.Email;
            userDTO.RoleIds = roleIds;

            return userDTO;
        }

        public static void Hydrate(User user, UserDTO userDTO)
        {
            user.UserName = userDTO.Email;
            user.Email = userDTO.Email;
            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
        }
    }
}
