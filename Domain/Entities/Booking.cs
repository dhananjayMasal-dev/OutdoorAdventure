using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;

        public bool IsConfirmed { get; set; }
        public string? RejectionReason { get; set; }

        public User User { get; set; } = null!;
    }
}
