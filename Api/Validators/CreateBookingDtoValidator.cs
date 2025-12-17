using Application.DTOs;
using FluentValidation;

namespace Api.Validators
{
    public class CreateBookingDtoValidator : AbstractValidator<CreateBookingDto>
    {
        public CreateBookingDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required")
                .GreaterThan(0).WithMessage("User ID must be greater than 0.");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Location field can not be empty");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required")
                .GreaterThan(DateTime.Now).WithMessage("Booking date cannot be in the past.")
                .Must(BeValidDate).WithMessage("Invalid date format.");
        }

        private bool BeValidDate(DateTime date)
        {
            return date != default(DateTime);
        }
    }
}