using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;
using EduInsights.Server.Interfaces;
using MongoDB.Driver;

namespace EduInsights.Server.Services;

public class AuthService(
    IMongoDatabase database,
    IUserService userService,
    IRefreshService tokenService,
    IInstituteService instituteService,
    IHttpContextAccessor httpContextAccessor) : IAuthService
{
    private readonly IMongoCollection<User> _userCollection = database.GetCollection<User>("users");

    private readonly HttpContext _httpContext = httpContextAccessor.HttpContext!;

    private void SetCookie(string key, string value)
    {
        _httpContext.Response.Cookies.Append(key, value, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        });
    }

    private void ClearCookie(string key)
    {
        _httpContext.Response.Cookies.Delete(key);
    }

    public async Task Register(RegisterUserRequest request)
    {
        try
        {
            var existingUser = await _userCollection.Find(u => u.UserName == request.UserName).FirstOrDefaultAsync();
            if (existingUser != null)
                throw new Exception("User already exists");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            
            var institute = new Institute
            {
                Name = request.InstituteName,
            };
            await instituteService.AddInstituteAsync(institute);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
                InstituteId = institute.Id,
                Role = request.Role,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.Now
            };
            await _userCollection.InsertOneAsync(user);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error when Register a User {ex.Message}");
        }
    }

    public async Task<RefreshResponse> Refresh()
    {
        try
        {
            var refreshTokenCookie = _httpContext.Request.Cookies["jwt"];

            if (string.IsNullOrWhiteSpace(refreshTokenCookie))
                throw new Exception("Refresh token is missing");

            var refreshToken = await tokenService.GetRefreshToken(refreshTokenCookie);
            if (refreshToken == null)
                throw new Exception("An error occurred while retrieving the refresh token.");

            var userResult = await userService.GetUserByIdAsync(refreshToken.UserId);
            if (userResult?.User == null)
                throw new Exception("User not found");

            var newAccessToken = tokenService.GenerateAccessToken(userResult.User!.Id, userResult.User.Role);

            var isRefreshTokenValid = await tokenService.ValidateRefreshToken(refreshTokenCookie);
            if (isRefreshTokenValid) return new RefreshResponse(newAccessToken, userResult.User);
            await tokenService.RevokeRefreshToken(refreshToken.Token);
            throw new Exception("Invalid or expired refresh token");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error when Register a User {ex.Message}");
        }
    }

    public async Task<LoginUserResponse> Login(LoginUserRequest request)
    {
        try
        {
            var user = await userService.FindUserByUserName(request.UserName);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid username or password");

            tokenService.GenerateAccessToken(user.Id, user.Role);

            var latestRefreshToken = await tokenService.GetRefreshTokenByUserId(user.Id);
            RefreshToken refreshToken;

            if (latestRefreshToken == null || !await tokenService.ValidateRefreshToken(latestRefreshToken.Token))
            {
                // Generate a new refresh token if invalid or not present
                if (latestRefreshToken != null)
                    await tokenService.RevokeRefreshToken(latestRefreshToken.Token);

                refreshToken = await tokenService.GenerateRefreshToken(user.Id);
            }
            else
            {
                refreshToken = latestRefreshToken;
            }

            SetCookie("jwt", refreshToken.Token);
            return new LoginUserResponse(refreshToken.Token, user);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error during user login: {ex.Message}", ex);
        }
    }

    public async Task Logout()
    {
        try
        {
            var refreshTokenCookie = _httpContext.Request.Cookies["jwt"];

            if (string.IsNullOrWhiteSpace(refreshTokenCookie))
                throw new Exception("Refresh token is missing");

            var isRefreshTokenValid = await tokenService.ValidateRefreshToken(refreshTokenCookie);
            if (!isRefreshTokenValid) throw new Exception("Invalid or expired refresh token");

            var refreshToken = await tokenService.GetRefreshToken(refreshTokenCookie);
            await tokenService.RevokeRefreshToken(refreshToken.Token);
            ClearCookie("jwt");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error during user logout: {ex.Message}", ex);
        }
    }
}