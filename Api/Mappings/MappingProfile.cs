using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterUserDto, User>();
            CreateMap<CreateBookingDto, Booking>();

            CreateMap<User, UserResponseDto>();

            CreateMap<Booking, BookingResponseDto>()
                .ForMember(dest => dest.Success, opt => opt.MapFrom(src => src.IsConfirmed))

                .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.Id));
        }
    }
}