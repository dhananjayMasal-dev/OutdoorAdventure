using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AdventureDbContext _context;

        public BookingRepository(AdventureDbContext context)
        {
            _context = context;
        }

        public async Task AddBookingAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasBookingOnDateAsync(int userId, DateTime date)
        {
            return await _context.Bookings.AnyAsync(b =>
                b.UserId == userId &&
                b.Date.Date == date.Date &&
                b.IsConfirmed
            );
        }
    }
}
