using Grpc.Net.Client;
using KariyerNet.Agent.Grpc;
using KariyerNet.Application.Interfaces;

namespace KariyerNet.Infrastructure.Agent
{
    public class AgentClient : IAgentClient
    {
        private readonly string _agentServiceUrl;

        public AgentClient()
        {
            _agentServiceUrl = "http://localhost:50051";
        }

        private static string GetMimeType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }

        public async Task<AgentMatchResult> MatchCandidateToJobAsync(
            string cvFilePath, string jobTitle, string jobDescription, string jobRequirements)
        {
            var fileBytes = await File.ReadAllBytesAsync(cvFilePath);
            var mimeType = GetMimeType(cvFilePath);

            using var channel = GrpcChannel.ForAddress(_agentServiceUrl);
            var client = new AgentService.AgentServiceClient(channel);

            var request = new MatchRequest
            {
                CvFile = Google.Protobuf.ByteString.CopyFrom(fileBytes),
                MimeType = mimeType,
                JobTitle = jobTitle,
                JobDescription = jobDescription,
                JobRequirements = jobRequirements
            };

            var response = await client.MatchCandidateToJobAsync(request);

            return new AgentMatchResult
            {
                CompatibilityScore = response.CompatibilityScore,
                Explanation = response.Explanation,
                CandidateExplanation = response.CandidateExplanation,
                MatchingSkills = response.MatchingSkills.ToList(),
                MissingSkills = response.MissingSkills.ToList()
            };
        }

        public async Task<AgentCvAnalysisResult> AnalyzeCvAsync(string cvFilePath)
        {
            var fileBytes = await File.ReadAllBytesAsync(cvFilePath);
            var mimeType = GetMimeType(cvFilePath);

            using var channel = GrpcChannel.ForAddress(_agentServiceUrl);
            var client = new AgentService.AgentServiceClient(channel);

            var request = new AnalyzeCvRequest
            {
                CvFile = Google.Protobuf.ByteString.CopyFrom(fileBytes),
                MimeType = mimeType
            };

            var response = await client.AnalyzeCvAsync(request);

            return new AgentCvAnalysisResult
            {
                Skills = response.Skills.ToList(),
                ExperienceLevel = response.ExperienceLevel,
                Summary = response.Summary,
                Suggestions = response.Suggestions.ToList()
            };
        }
        public async Task<List<JobRecommendationResult>> RecommendJobsAsync(string cvFilePath, List<JobInfoInput> jobs)
        {
            var fileBytes = await File.ReadAllBytesAsync(cvFilePath);
            var mimeType = GetMimeType(cvFilePath);

            using var channel = GrpcChannel.ForAddress(_agentServiceUrl);
            var client = new AgentService.AgentServiceClient(channel);

            var request = new RecommendJobsRequest
            {
                CvFile = Google.Protobuf.ByteString.CopyFrom(fileBytes),
                MimeType = mimeType
            };

            foreach (var job in jobs)
            {
            request.Jobs.Add(new JobInfo
            {
                Id = job.Id.ToString(),
                Title = job.Title,
                Description = job.Description,
                Requirements = job.Requirements
            });
    }

            var response = await client.RecommendJobsAsync(request);

            return response.Recommendations.Select(r => new JobRecommendationResult
            {
                JobId = Guid.TryParse(r.JobId, out var id) ? id : Guid.Empty,
                Score = r.Score,
                Reason = r.Reason
            }).Where(r => r.JobId != Guid.Empty).ToList();
        }
        }
}