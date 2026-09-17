using System;
using System.Collections.Generic;
using System.Text;

namespace KariyerNet.Application.DTOs
{
    /// <summary>
    /// raw data transfer object for registering a new user, 
    /// containing the necessary information such as full name,
    /// email, and password.
    /// 
    /// </summary>
    public class RegisterUserDto
    {
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
