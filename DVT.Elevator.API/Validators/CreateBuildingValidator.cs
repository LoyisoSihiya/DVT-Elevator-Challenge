using DVT.Elevator.Domain.DTOs;
using FluentValidation;

namespace DVT.Elevator.API.Validators;

public class CreateBuildingValidator : AbstractValidator<CreateBuildingDto>
{
    public CreateBuildingValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Building name is required")
            .MaximumLength(200)
            .WithMessage("Building name cannot exceed 200 characters");
        
        RuleFor(x => x.TotalFloors)
            .GreaterThan(0)
            .WithMessage("Total floors must be greater than 0")
            .LessThanOrEqualTo(200)
            .WithMessage("Total floors cannot exceed 200");
    }
}
