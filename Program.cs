using Scalar.AspNetCore;
using EduInsights.Server.EndPoints;
using EduInsights.Server.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference((option) =>
    {
        option
            .WithTitle("EduInsight App")
            .WithTheme(ScalarTheme.BluePlanet)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// app.UseHttpsRedirection();
app.MapAuthEndPoints();
app.MapUsersEndPoints();
app.MapInstitutesEndPoints();
app.MapBatchesEndpoints();

app.UseCors(ServiceExtensions.CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.Run();