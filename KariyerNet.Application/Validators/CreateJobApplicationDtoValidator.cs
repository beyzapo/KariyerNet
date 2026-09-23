using FluentValidation;
using KariyerNet.Application.DTOs;

namespace KariyerNet.Application.Validators
{
    public class CreateJobApplicationDtoValidator : AbstractValidator<CreateJobApplicationDto>
    {
        public CreateJobApplicationDtoValidator()
        {
            RuleFor(x => x.JobPostingId)
                .NotEmpty();
        }
    }
}
