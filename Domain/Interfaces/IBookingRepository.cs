using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IBookingRepository
    {
        Task AddBookingAsync(Booking booking);
        Task SaveChangesAsync();
        Task<bool> HasBookingOnDateAsync(int userId, DateTime date);

    }
}
