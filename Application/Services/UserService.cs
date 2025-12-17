using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserResponseDto> RegisterUserAsync(RegisterUserDto dto)
        {
            if (!IsValidEmail(dto.Email))
            {
                return new UserResponseDto
                {
                    Success = false,
                    Message = "Invalid email format."
                };
            }

            var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return new UserResponseDto
                {
                    Success = false,
                    Message = $"User with email '{dto.Email}' already exists."
                };
            }

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return new UserResponseDto
            {
                Success = true,
                Message = "User registered successfully.",
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
    }
}
