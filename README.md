# EduInsights - Backend Application

EduInsights is a **University Result Management System** designed to streamline academic performance tracking. This repository contains the **backend** of the application, built with **.NET 9**, **MongoDB**, **JWT authentication**, and **Redis caching**.

## Features

- User Authentication (JWT-based login, registration, email verification)
- Role-based access for **Super Admin, Admin, Data Entry, Lecturer, and Student**
- API endpoints for managing **users, results, and courses**
- Caching with **Redis** for performance optimization
- Email notifications via **MailKit**
- Secure password handling with **BCrypt**

## Tech Stack

- **Backend Framework:** .NET 9
- **Database:** MongoDB
- **Authentication:** JWT (JSON Web Token)
- **Caching:** Redis
- **Email Service:** MailKit
- **Security:** BCrypt for password hashing

## Getting Started

### Prerequisites

Make sure you have the following installed:
- [.NET 9 SDK](https://dotnet.microsoft.com/)
- [MongoDB](https://www.mongodb.com/try/download/community)
- [Redis](https://redis.io/docs/getting-started/)

### Installation

1. Clone the repository:
   ```bash
   git clone git clone https://github.com/EduInsights-Org/EduInsights.Server.git
   cd EduInsights.server
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Create an **appsettings.json** file in the root directory and configure environment variables:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "EduInsightsDatabase": {
       "ConnectionString": "mongodb://localhost:27017",
       "DatabaseName": "eduInsights",
       "UsersCollectionName": "users"
     },
     "JwtSettings": {
       "Key": "your-secure-key",
       "Issuer": "YourApp",
       "Audience": "YourAppUsers",
       "ExpiryMinutes": 60
     },
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "SmtpUsername": "your-email@gmail.com",
       "SmtpPassword": "your-app-password",
       "FromEmail": "your-email@gmail.com"
     },
     "Redis": {
       "ConnectionString": "localhost:6379"
     },
     "AllowedHosts": "*"
   }
   ```

4. Run database migrations (if applicable) and start the server:
   ```bash
   dotnet run
   ```

### Folder Structure

```
EduInsights/server
│── Contracts/       # Interfaces for dependency injection
│── EndPoints/       # API Controllers and routes
│── Entities/        # Database models
│── Enums/           # Enum definitions
│── Errors/          # Custom error handling
│── Extensions/      # Middleware and service extensions
│── Interfaces/      # Interface definitions
│── Services/        # Business logic and service classes
│── Program.cs       # Main entry point
│── appsettings.json # Configuration file
│── EduInsights.Server.csproj # .NET Project file
```

### Dependencies

```xml
<ItemGroup>
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
    <PackageReference Include="MailKit" Version="4.10.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="9.0.2" />
    <PackageReference Include="Microsoft.IdentityModel.Tokens" Version="8.3.0" />
    <PackageReference Include="MongoDB.Driver" Version="3.1.0" />
    <PackageReference Include="Scalar.AspNetCore" Version="1.2.74" />
    <PackageReference Include="StackExchange.Redis" Version="2.8.24" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.3.0" />
</ItemGroup>
```

### Important Configuration in `.csproj`

Ensure that email templates are copied to the output directory:
```xml
<None Update="EmailTemplates\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
```

## Contributing

Contributions are welcome! Feel free to open issues and submit pull requests.

## License

This project is licensed under the [MIT License](LICENSE).
