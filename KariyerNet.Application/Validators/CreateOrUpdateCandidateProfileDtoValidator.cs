using FluentValidation;
using KariyerNet.Application.DTOs;

namespace KariyerNet.Application.Validators
{
    public class CreateOrUpdateCandidateProfileDtoValidator : AbstractValidator<CreateOrUpdateCandidateProfileDto>
    {
        public CreateOrUpdateCandidateProfileDtoValidator()
        {
            RuleFor(x => x.CvFilePath)
                .NotEmpty();
        }
    }
}
