using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreateBookingDto
    {
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
    }
}
