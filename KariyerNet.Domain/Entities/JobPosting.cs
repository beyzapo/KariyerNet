using System;
using System.Collections.Generic;
using System.Text;

namespace KariyerNet.Domain.Entities
{
    public class JobPosting
    {
        public Guid Id { get; set; }

        public Guid EmployerId { get; set; } //foreign key to the User entity (employer)
        public User Employer { get; set; } = null!; //navigation property to the User entity (employer)

        public string Title { get; set; } = string.Empty; //title of the job posting
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; //timestamp for when the job posting was created
    }
}
