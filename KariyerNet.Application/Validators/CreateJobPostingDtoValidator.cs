using FluentValidation;
using KariyerNet.Application.DTOs;

namespace KariyerNet.Application.Validators
{
    public class CreateJobPostingDtoValidator : AbstractValidator<CreateJobPostingDto>
    {
        public CreateJobPostingDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty();

            RuleFor(x => x.Description)
                .NotEmpty();

            RuleFor(x => x.Requirements)
                .NotEmpty();
        }
    }
}
