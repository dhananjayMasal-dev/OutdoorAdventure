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

        public BookingService(IBookingRepository bookingRepository, IUserRepository userRepository, IWeatherService weatherService)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _weatherService = weatherService;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto dto)
        {
            var user = await _userRepository.GetUserByIdAsync(dto.UserId);
            if (user == null)
            {
                return new BookingResponseDto { Success = false, Message = "User not found." };
            }

            var weatherResult = await _weatherService.CheckWeatherAsync(dto.Location, dto.Date);

            var booking = new Booking
            {
                UserId = dto.UserId,
                Date = dto.Date,
                Location = dto.Location,
                IsConfirmed = weatherResult.IsGoodWeather,
                RejectionReason = weatherResult.IsGoodWeather ? null : weatherResult.Message
            };

            await _bookingRepository.AddBookingAsync(booking);
            await _bookingRepository.SaveChangesAsync();

            return new BookingResponseDto
            {
                BookingId = booking.Id,
                Success = booking.IsConfirmed,
                Message = booking.IsConfirmed ? "Booking Confirmed!" : $"Rejected: {booking.RejectionReason}"
            };
        }
    }
}