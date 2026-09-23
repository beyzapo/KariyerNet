using FluentValidation;
using KariyerNet.Application.DTOs;
using KariyerNet.Domain.Entities;

namespace KariyerNet.Application.Validators
{
    public class UpdateJobApplicationStatusDtoValidator : AbstractValidator<UpdateJobApplicationStatusDto>
    {
        public UpdateJobApplicationStatusDtoValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum()
                .Must(status => status is ApplicationStatus.Accepted or ApplicationStatus.Rejected)
                .WithMessage("Durum sadece 'Accepted' veya 'Rejected' olarak güncellenebilir.");
        }
    }
}
