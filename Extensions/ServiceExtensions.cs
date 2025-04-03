using System.Text;
using EduInsights.Server.Entities;
using EduInsights.Server.Interfaces;
using EduInsights.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using StackExchange.Redis;

namespace EduInsights.Server.Extensions;

public static class ServiceExtensions
{
    public const string CorsPolicyName = "AllowSpecificOrigins";

    public static void AddApplicationServices(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        //Add cors service
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        //Add mongoDB service
        var mongoDbSettings = configuration.GetSection("EduInsightsDatabase").Get<DatabaseSettings>();
        var mongoClient = new MongoClient(mongoDbSettings!.ConnectionString);
        var database = mongoClient.GetDatabase(mongoDbSettings.DatabaseName);
        builder.Services.AddSingleton<IMongoClient>(mongoClient);
        builder.Services.AddSingleton(database);

        //Add JWT service
        var jwtSettings = configuration.GetSection("JwtSettings");
        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
                };
            });

        // Add Redis service
        builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var redisConnectionString = configuration["Redis:ConnectionString"];
            return ConnectionMultiplexer.Connect(redisConnectionString!);
        });
        builder.Services.AddScoped<IRedisService, RedisService>();

        //Other services
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IRefreshService, TokenService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IInstituteService, InstituteService>();
        builder.Services.AddScoped<IBatchService, BatchService>();
        builder.Services.AddScoped<IStudentService, StudentService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<ISubjectService, SubjectService>();
        builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
    }
}