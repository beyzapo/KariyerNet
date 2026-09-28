namespace KariyerNet.Application.DTOs;

public class CvAnalysisDto
{
        public List<string> Skills { get; set; } = new();
        public string ExperienceLevel { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new();
}
