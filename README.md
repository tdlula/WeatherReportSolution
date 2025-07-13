WeatherReport Console App
A modular .NET 9 console app that fetches current weather data from OpenWeatherMap API for any city. Designed with clean architecture, DI, and robust error handling.

Key points:
Real-time weather fetching with input validation
Clear logging and error management
Extensible service-based design
Unit and integration tests included

Setup:
Clone repo, configure API key in appsettings.json
Restore dependencies with dotnet restore
Build & run via dotnet run --project WeatherReport/WeatherReport.csproj

Testing:
Run tests with dotnet test

Structure:
WeatherReport/ - console app entry point
WeatherReport.Service/ - API integration & service logic
WeatherReport.Tests/ - test projects
Requires internet access and .NET 9 SDK.