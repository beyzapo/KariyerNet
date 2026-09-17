using System;
using System.Collections.Generic;
using System.Text;

namespace KariyerNet.Domain.Entities
{
    public enum UserRole //just two choices for user role
    {
        Employer, //employer user
        Candidate //job seeker user
    }
    public class User
    {
        public Guid Id { get; set; } //unique identifier for the user
        public string FullName { get; set; } //full name of the user
        public string Email { get; set; }  //email address of the user
        public string PasswordHash { get; set; } //hashed password for security
        public UserRole Role { get; set; }  //role of the user (employer or candidate)
        public DateTime CreatedAt {  get; set; } = DateTime.UtcNow; //timestamp for when the user was created
    }

















    }
     