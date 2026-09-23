using System;
using System.Collections.Generic;
using System.Text;

namespace KariyerNet.Domain.Entities
{
    public enum ApplicationStatus
    {
        Pending, //application is pending review
        Accepted, //application has been accepted
        Rejected //application has been rejected
    }
    public class JobApplication
    {
        public Guid Id { get; set; }

        public Guid CandidateId { get; set; } //foreign key to the User entity (candidate)
        public User Candidate { get; set; } = null!; //navigation property to the User entity (candidate)

        public Guid JobPostingId { get; set; } //whose job posting is this application for 
        public JobPosting JobPosting { get; set; } = null!; //navigation property to the JobPosting entity

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending; //firstly status of the application is pending, then it can be accepted or rejected by the employer
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow; //timestamp for when the application was submitted
    }
}
