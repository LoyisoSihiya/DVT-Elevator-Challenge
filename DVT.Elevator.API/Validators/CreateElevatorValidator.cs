using DVT.Elevator.Domain.DTOs;
using FluentValidation;

namespace DVT.Elevator.API.Validators;

public class CreateElevatorValidator : AbstractValidator<CreateElevatorDto>
{
    public CreateElevatorValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Elevator name is required")
            .MaximumLength(100)
            .WithMessage("Elevator name cannot exceed 100 characters");
        
        RuleFor(x => x.InitialFloor)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial floor must be greater than or equal to 0");
        
        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0)
            .WithMessage("Max capacity must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Max capacity cannot exceed 100");
        
        RuleFor(x => x.Speed)
            .GreaterThan(0)
            .WithMessage("Speed must be greater than 0")
            .LessThanOrEqualTo(300)
            .WithMessage("Speed cannot exceed 300 floors per minute");
        
        RuleFor(x => x.ElevatorTypeId)
            .GreaterThan(0)
            .WithMessage("Elevator type ID must be greater than 0");
        
        RuleFor(x => x.BuildingId)
            .GreaterThan(0)
            .WithMessage("Building ID must be greater than 0");
    }
}
