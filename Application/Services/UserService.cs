using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserResponseDto> RegisterUserAsync(RegisterUserDto dto)
        {

            var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return new UserResponseDto
                {
                    Success = false,
                    Message = $"User with email '{dto.Email}' already exists."
                };
            }

            var user = _mapper.Map<User>(dto);

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            var response = _mapper.Map<UserResponseDto>(user);

            response.Success = true;
            response.Message = "User registered successfully.";

            return response;
        }
    }
}
