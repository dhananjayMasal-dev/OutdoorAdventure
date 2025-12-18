OUTDOOR ADVENTURE BOOKING API

A robust, .NET 8 Web API built using Clean Architecture principles.
This system manages user registrations and adventure bookings, featuring intelligent
validation logic driven by real-time weather data and Smart Search location.


KEY FEATURES
------------

1. Weather-Driven Business Logic
Bookings are validated against real-world conditions using the Open-Meteo API.

Rules:
- Bookings are rejected if the forecast predicts Rain, Snow, or Thunderstorms
  (WMO Weather Codes >= 51)
- Bookings are rejected if the temperature is unsafe (below 5°C)


2. Smart Location Search (Fuzzy Matching)
Solves the “Data Availability” problem when users input non-standard or verbose
location names.

How it works:
- Recursive Retry Logic that attempts partial matches
- If no exact match is found, the API returns intelligent suggestions
  instead of a hard failure


3. Performance & Resilience
Designed with production-readiness in mind.

- In-memory caching (IMemoryCache) for weather data (30 minutes)
- Polly retry policies with exponential backoff (3 retries)
- Reduced latency and external API costs


4. Engineering Excellence
- Clean Architecture (Domain, Application, Infrastructure, API)
- Global exception handling middleware
- AutoMapper for DTO to Entity mapping
- FluentValidation integrated into request pipeline
- Fully unit-tested business logic


TECH STACK
----------

Framework    : ASP.NET Core 8 Web API
Language     : C# 12
Database     : SQL Server (Entity Framework Core)
Validation   : FluentValidation
Mapping      : AutoMapper
Resilience   : Microsoft.Extensions.Http.Polly
Caching      : Microsoft.Extensions.Caching.Memory
Testing      : xUnit, Moq
External API : Open-Meteo (Geocoding & Weather)


ARCHITECTURE STRUCTURE
---------------------

OutdoorAdventure.sln
|
|-- Api
|   |-- Controllers
|   |-- Extensions
|   |-- Middleware
|   |-- Mappings
|   |-- Validators
|
|-- Application
|   |-- DTOs
|   |-- Interfaces
|   |-- Services
|
|-- Domain
|   |-- Entities
|   |-- Interfaces
|
|-- Infrastructure
|   |-- Data
|   |-- External Models
|   |-- Repository
|   |-- Service(Weather Service)
|
|-- OutdoorAdventure.Tests


API ENDPOINTS
-------------

1. Register User
POST /api/adventure/users

Request Body:
{
  "name": "John Doe",
  "email": "john@example.com"
}


2. Create Booking
POST /api/adventure/bookings

Request Body:
{
  "userId": 1002,
  "date": "2025-12-23T06:17:00.938Z",
  "location": "Arunachal Pradesh"
}

Response Scenario (Smart Location Suggestion):

{
  "bookingId": 0,
  "success": false,
  "message": "Rejected: Location 'Arunachal Pradesh' not found.
              Did you mean: Arunāchal (India),
              Arunāchalapuram (India),
              Arunāchalapperi (India)?"
}


SETUP & RUN
-----------

1. Clone Repository
git clone https://github.com/yourusername/outdoor-adventure.git
cd outdoor-adventure


2. Configure Database
Ensure SQL Server is running and update ConnectionStrings
in Api/appsettings.json.


3. Apply Migrations
dotnet ef database update --project Infrastructure --startup-project Api


4. Run Application
dotnet run --project Api


5. Swagger UI
http://localhost:5285/swagger


TESTING
-------

Unit tests are implemented using xUnit and Moq.

Covered Tests:
- CreateBooking_ShouldReturnSuccess_WhenWeatherIsGood
- CreateBooking_ShouldFail_WhenWeatherIsBad
- CheckWeather_ShouldReturnSuggestions_WhenExactMatchFails

Run Tests:
dotnet test
