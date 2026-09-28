namespace KariyerNet.Application.Interfaces
{
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
    }
}