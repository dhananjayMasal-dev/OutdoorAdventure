using AutoMapper; // 1. Add this namespace
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWeatherService _weatherService;
        private readonly IMapper _mapper;

        public BookingService(IBookingRepository bookingRepository, IUserRepository userRepository, IWeatherService weatherService, IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _weatherService = weatherService;
            _mapper = mapper;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto dto)
        {
            var user = await _userRepository.GetUserByIdAsync(dto.UserId);
            if (user == null)
            {
                return new BookingResponseDto { Success = false, Message = "User not found." };
            }

            bool alreadyBooked = await _bookingRepository.HasBookingOnDateAsync(dto.UserId, dto.Date);
            if (alreadyBooked)
            {
                return new BookingResponseDto
                {
                    Success = false,
                    Message = "You already have a confirmed booking for this date."
                };
            }

            var weatherResult = await _weatherService.CheckWeatherAsync(dto.Location, dto.Date);

            var booking = _mapper.Map<Booking>(dto);

            booking.IsConfirmed = weatherResult.IsGoodWeather;
            booking.RejectionReason = weatherResult.IsGoodWeather ? null : weatherResult.Message;

            await _bookingRepository.AddBookingAsync(booking);
            await _bookingRepository.SaveChangesAsync();

            var response = _mapper.Map<BookingResponseDto>(booking);

            response.Message = booking.IsConfirmed ? "Booking Confirmed!" : $"Rejected: {booking.RejectionReason}";

            return response;
        }
    }
}