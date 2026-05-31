using DVT.Elevator.Domain.DTOs;
using FluentValidation;

namespace DVT.Elevator.API.Validators;

public class CreatePassengerRequestValidator : AbstractValidator<CreatePassengerRequestDto>
{
    public CreatePassengerRequestValidator()
    {
        RuleFor(x => x.SourceFloor)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Source floor must be greater than or equal to 0");
        
        RuleFor(x => x.DestinationFloor)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Destination floor must be greater than or equal to 0");
        
        RuleFor(x => x.PassengerCount)
            .GreaterThan(0)
            .WithMessage("Passenger count must be greater than 0")
            .LessThanOrEqualTo(50)
            .WithMessage("Passenger count cannot exceed 50");
        
        RuleFor(x => x.BuildingId)
            .GreaterThan(0)
            .WithMessage("Building ID must be greater than 0");
        
        RuleFor(x => x)
            .Must(x => x.SourceFloor != x.DestinationFloor)
            .WithMessage("Source and destination floors must be different");
    }
}
