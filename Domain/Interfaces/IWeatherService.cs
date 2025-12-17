using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IWeatherService
    {
        Task<(bool IsGoodWeather, string Message)> CheckWeatherAsync(string location, DateTime date);
    }
}
