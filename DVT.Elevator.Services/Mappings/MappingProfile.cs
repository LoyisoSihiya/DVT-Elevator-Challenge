using AutoMapper;
using DVT.Elevator.Domain.DTOs;
using DVT.Elevator.Domain.Entities;
using DVT.Elevator.Domain.Enums;
using ElevatorEntity = DVT.Elevator.Domain.Entities.Elevator;

namespace DVT.Elevator.Services.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Building
        CreateMap<Building, BuildingDto>();
        CreateMap<Building, BuildingDetailDto>()
            .ForMember(dest => dest.Elevators, opt => opt.MapFrom(src => src.Elevators))
            .ForMember(dest => dest.Floors, opt => opt.MapFrom(src => src.Floors));
        CreateMap<CreateBuildingDto, Building>();

        // Elevator
        CreateMap<ElevatorEntity, ElevatorDto>()
            .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ElevatorTypeName, opt => opt.MapFrom(src => src.ElevatorType.Name));

        CreateMap<ElevatorEntity, ElevatorStatusDto>()
            .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.UpdatedAt ?? src.CreatedAt));

        CreateMap<CreateElevatorDto, ElevatorEntity>()
            .ForMember(dest => dest.CurrentFloor, opt => opt.MapFrom(src => src.InitialFloor))
            .ForMember(dest => dest.TargetFloor, opt => opt.MapFrom(src => src.InitialFloor));

        // Floor
        CreateMap<Floor, FloorDto>();

        // PassengerRequest
        CreateMap<PassengerRequest, PassengerRequestDto>()
            .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.AssignedElevatorName, opt => opt.MapFrom(src =>
                src.AssignedElevator != null ? src.AssignedElevator.Name : null));

        CreateMap<CreatePassengerRequestDto, PassengerRequest>()
            .ForMember(dest => dest.Direction, opt => opt.MapFrom(src =>
                src.DestinationFloor > src.SourceFloor
                    ? ElevatorDirection.Up
                    : ElevatorDirection.Down));

        // ElevatorType
        CreateMap<ElevatorType, ElevatorTypeDto>();
    }
}
