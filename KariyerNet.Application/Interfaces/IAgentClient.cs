namespace KariyerNet.Application.Interfaces
{
    public class JobInfoInput
    {
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    }

    public class JobRecommendationResult
    {
    public Guid JobId { get; set; }
    public int Score { get; set; }
    public string Reason { get; set; } = string.Empty;
    }
    public class AgentMatchResult
    {
        public int CompatibilityScore { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public List<string> MatchingSkills { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
        public string CandidateExplanation { get; set; } = string.Empty;
    }

    public class AgentCvAnalysisResult
    {
        public List<string> Skills { get; set; } = new();
        public string ExperienceLevel { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new();
    }

    public interface IAgentClient
    {
        Task<AgentMatchResult> MatchCandidateToJobAsync(
            string cvFilePath, string jobTitle, string jobDescription, string jobRequirements);

        Task<AgentCvAnalysisResult> AnalyzeCvAsync(string cvFilePath);
        Task<List<JobRecommendationResult>> RecommendJobsAsync(string cvFilePath, List<JobInfoInput> jobs);
    }
}