using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //request
            CreateMap<RegisterUserDto, User>();
            CreateMap<CreateBookingDto, Booking>();

            //response
            CreateMap<User, UserResponseDto>();
            CreateMap<Booking, BookingResponseDto>();
        }
    }
}