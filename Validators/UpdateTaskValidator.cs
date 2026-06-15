using FluentValidation;
using TaskManagerAPI.DTOs;

namespace TaskManagerAPI.Validators;

public class UpdateTaskValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title cannot be empty.")
            .MaximumLength(200).WithMessage("Title must be 200 characters or fewer.")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters.")
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must be 1000 characters or fewer.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority must be Low, Medium, or High.")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be Todo, InProgress, or Done.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future.")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than zero.")
            .When(x => x.CategoryId.HasValue);
    }
}