using AutoMapper; // Needs AutoMapper package in Test Project
using Application.DTOs;
using Application.Mappings; // Namespace where MappingProfile is
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Moq;
using Xunit;

namespace OutdoorAdventure.Tests
{
    public class BookingServiceTests
    {
        private readonly Mock<IBookingRepository> _mockBookingRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IWeatherService> _mockWeatherService;
        private readonly IMapper _mapper;
        private readonly BookingService _service;

        public BookingServiceTests()
        {
            _mockBookingRepo = new Mock<IBookingRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockWeatherService = new Mock<IWeatherService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = config.CreateMapper();

            _service = new BookingService(
                _mockBookingRepo.Object,
                _mockUserRepo.Object,
                _mockWeatherService.Object,
                _mapper
            );
        }

        [Fact]
        public async Task CreateBooking_ShouldReturnSuccess_WhenUserExists_AndWeatherIsGood()
        {
            // Arrange
            var dto = new CreateBookingDto { UserId = 1, Date = DateTime.Now.AddDays(1), Location = "Goa" };

            _mockUserRepo.Setup(repo => repo.GetUserByIdAsync(dto.UserId))
                .ReturnsAsync(new User { Id = 1, Name = "Test User" });

            _mockBookingRepo.Setup(repo => repo.HasBookingOnDateAsync(dto.UserId, dto.Date))
                .ReturnsAsync(false);

            _mockWeatherService.Setup(w => w.CheckWeatherAsync(dto.Location, dto.Date))
                .ReturnsAsync((true, "Sunny"));

            // Act
            var result = await _service.CreateBookingAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Booking Confirmed!", result.Message);

            // Verify
            _mockBookingRepo.Verify(r => r.AddBookingAsync(It.Is<Booking>(b =>
                b.UserId == 1 &&
                b.Location == "Goa" &&
                b.IsConfirmed == true
            )), Times.Once);
        }

        [Fact]
        public async Task CreateBooking_ShouldFail_WhenWeatherIsBad()
        {
            // Arrange
            var dto = new CreateBookingDto { UserId = 1, Date = DateTime.Now.AddDays(1), Location = "London" };

            _mockUserRepo.Setup(repo => repo.GetUserByIdAsync(dto.UserId))
                .ReturnsAsync(new User());

            _mockWeatherService.Setup(w => w.CheckWeatherAsync(dto.Location, dto.Date))
                .ReturnsAsync((false, "Forecast predicts rain or storm."));

            // Act
            var result = await _service.CreateBookingAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Rejected", result.Message);

            // Verify
            _mockBookingRepo.Verify(r => r.AddBookingAsync(It.Is<Booking>(b => b.IsConfirmed == false)), Times.Once);
        }

        [Fact]
        public async Task CreateBooking_ShouldFail_WhenBookingAlreadyExists()
        {
            // Arrange
            var dto = new CreateBookingDto { UserId = 1, Date = DateTime.Now.AddDays(1), Location = "Goa" };

            _mockUserRepo.Setup(repo => repo.GetUserByIdAsync(dto.UserId))
                .ReturnsAsync(new User { Id = 1 });

            _mockBookingRepo.Setup(repo => repo.HasBookingOnDateAsync(dto.UserId, dto.Date))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CreateBookingAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("You already have a confirmed booking for this date.", result.Message);

            _mockBookingRepo.Verify(r => r.AddBookingAsync(It.IsAny<Booking>()), Times.Never);
        }
    }
}