namespace KariyerNet.Application.DTOs;

public class JobRecommendationDto
{
        public Guid JobPostingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Reason { get; set; } = string.Empty;
}
