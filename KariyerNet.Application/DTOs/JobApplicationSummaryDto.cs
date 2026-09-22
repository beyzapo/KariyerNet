namespace KariyerNet.Application.DTOs
{
    public class JobApplicationSummaryDto
    {
        public Guid Id { get; set; }
        public Guid CandidateId { get; set; }
        public Guid JobPostingId { get; set; }
        public string JobPostingTitle { get; set; } = default!;
        public string CandidateFullName { get; set; } = default!;
        public string CandidateEmail { get; set; } = default!;
        public string Status { get; set; } = default!;
        public DateTime AppliedAt { get; set; }
        public bool HasCv { get; set; }
    }
}
