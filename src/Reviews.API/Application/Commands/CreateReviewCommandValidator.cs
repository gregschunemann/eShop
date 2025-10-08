using FluentValidation;

namespace eShop.Reviews.API.Application.Commands;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("ProductId must be greater than 0");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required")
            .MaximumLength(256)
            .WithMessage("UserId must not exceed 256 characters");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.ReviewText)
            .MaximumLength(2000)
            .WithMessage("Review text must not exceed 2000 characters");
    }
}
