namespace KariyerNet.Application.DTOs
{
    public class CreateJobPostingDto
    {
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Requirements { get; set; } = default!;
    }
}
