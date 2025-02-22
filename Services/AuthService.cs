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
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuthService> logger) : IAuthService
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

    public async Task<ApiResponse<User>> Register(RegisterUserRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FirstName)
                || string.IsNullOrWhiteSpace(request.LastName)
                || string.IsNullOrWhiteSpace(request.UserName)
                || string.IsNullOrWhiteSpace(request.Password)
                || string.IsNullOrWhiteSpace(request.InstituteName)
               ) return ApiResponse<User>.ErrorResult("Validation error.", 422);

            var existingUser = await _userCollection.Find(u => u.UserName == request.UserName).FirstOrDefaultAsync();
            if (existingUser != null)
                return ApiResponse<User>.ErrorResult("Can't add already exists user name.", 409);

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

            return ApiResponse<User>.SuccessResult(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating user.");
            return ApiResponse<User>.ErrorResult("Error occurred while creating user.", 500);
        }
    }

    public async Task<ApiResponse<RefreshResponse>> Refresh()
    {
        try
        {
            var refreshTokenCookie = _httpContext.Request.Cookies["jwt"];

            if (string.IsNullOrWhiteSpace(refreshTokenCookie))
                return ApiResponse<RefreshResponse>.ErrorResult("Refresh token is missing", 404);

            var refreshTokenResult = await tokenService.GetRefreshToken(refreshTokenCookie);
            if (!refreshTokenResult.Success)
                return ApiResponse<RefreshResponse>.ErrorResult(refreshTokenResult.Message,
                    refreshTokenResult.StatusCode);

            var userResult = await userService.GetUserByIdAsync(refreshTokenResult.Data!.UserId);
            if (!userResult.Success)
                return ApiResponse<RefreshResponse>.ErrorResult(userResult.Message, userResult.StatusCode);

            var newAccessTokenResult = tokenService.GenerateAccessToken(userResult.Data!.Id, userResult.Data!.Role);
            if (!newAccessTokenResult.Success)
                return ApiResponse<RefreshResponse>.ErrorResult(newAccessTokenResult.Message,
                    newAccessTokenResult.StatusCode);

            var isRefreshTokenValidResult = (await tokenService.ValidateRefreshToken(refreshTokenCookie));
            if (!isRefreshTokenValidResult.Success)
            {
                await tokenService.RevokeRefreshToken(refreshTokenResult.Data!.Token);
                return ApiResponse<RefreshResponse>.ErrorResult(isRefreshTokenValidResult.Message,
                    isRefreshTokenValidResult.StatusCode);
            }

            var refreshResponse = new RefreshResponse(newAccessTokenResult.Data!, userResult.Data!.Id);
            return ApiResponse<RefreshResponse>.SuccessResult(refreshResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when Refreshing token: {ex.Message}", ex.Message);
            return ApiResponse<RefreshResponse>.ErrorResult("Error when Refreshing token", 500);
        }
    }

    public async Task<ApiResponse<LoginUserResponse>> Login(LoginUserRequest request)
    {
        try
        {
            var userResult = await userService.FindUserByUserName(request.UserName);
            if (!userResult.Success)
                return ApiResponse<LoginUserResponse>.ErrorResult(userResult.Message, userResult.StatusCode);

            if (!BCrypt.Net.BCrypt.Verify(request.Password, userResult.Data!.PasswordHash))
                return ApiResponse<LoginUserResponse>.ErrorResult("Invalid username or password", 401);

            var tokenResult = tokenService.GenerateAccessToken(userResult.Data.Id, userResult.Data.Role);
            if (!tokenResult.Success)
                return ApiResponse<LoginUserResponse>.ErrorResult(tokenResult.Message, tokenResult.StatusCode);

            var latestRefreshTokenResult = await tokenService.GetRefreshTokenByUserId(userResult.Data.Id);
            RefreshToken refreshToken;
            if (!latestRefreshTokenResult.Success)
                return ApiResponse<LoginUserResponse>.ErrorResult(latestRefreshTokenResult.Message,
                    latestRefreshTokenResult.StatusCode);

            var latestRefreshToken = latestRefreshTokenResult.Data is null
                ? string.Empty
                : latestRefreshTokenResult.Data.Token;

            var tokenValidationResult = await tokenService.ValidateRefreshToken(latestRefreshToken);
            if (!tokenValidationResult.Success)
            {
                return ApiResponse<LoginUserResponse>.ErrorResult(tokenValidationResult.Message,
                    tokenValidationResult.StatusCode);
            }

            if (latestRefreshToken == string.Empty || tokenValidationResult.Data == false)
            {
                var tokenRevokeResult = await tokenService.RevokeRefreshToken(latestRefreshToken);
                if (!tokenRevokeResult.Success)
                    return ApiResponse<LoginUserResponse>.ErrorResult(tokenRevokeResult.Message,
                        tokenRevokeResult.StatusCode);

                var newRefreshTokenResult = await tokenService.GenerateRefreshToken(userResult.Data.Id);
                if (!newRefreshTokenResult.Success)
                    return ApiResponse<LoginUserResponse>.ErrorResult(newRefreshTokenResult.Message,
                        newRefreshTokenResult.StatusCode);

                refreshToken = newRefreshTokenResult.Data!;
            }
            else
            {
                refreshToken = latestRefreshTokenResult.Data!;
            }

            SetCookie("jwt", refreshToken.Token);
            var loginUserResponse = new LoginUserResponse(refreshToken.Token, userResult.Data);
            return ApiResponse<LoginUserResponse>.SuccessResult(loginUserResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when user login: {ex.Message}", ex.Message);
            return ApiResponse<LoginUserResponse>.ErrorResult("Error when user login", 500);
        }
    }

    public async Task<ApiResponse<LogoutUserResponse>> Logout()
    {
        try
        {
            var refreshTokenCookie = _httpContext.Request.Cookies["jwt"];
            if (string.IsNullOrWhiteSpace(refreshTokenCookie))
                return ApiResponse<LogoutUserResponse>.ErrorResult("Refresh token is missing", 400);

            var refreshToken = (await tokenService.GetRefreshToken(refreshTokenCookie)).Data;
            if (refreshToken == null)
                return ApiResponse<LogoutUserResponse>.ErrorResult("Refresh token is missing", 404);

            var isRefreshTokenValid = (await tokenService.ValidateRefreshToken(refreshTokenCookie)).Data;
            if (!isRefreshTokenValid)
                return ApiResponse<LogoutUserResponse>.ErrorResult("Invalid or expired refresh token", 401);

            var userResult = (await userService.GetUserByIdAsync(refreshToken.UserId)).Data;
            if (userResult == null)
                return ApiResponse<LogoutUserResponse>.ErrorResult("User not found", 404);

            var logoutUser = new LogoutUserResponse(refreshToken.Token, userResult.UserName);

            await tokenService.RevokeRefreshToken(refreshToken.Token);

            ClearCookie("jwt");

            return ApiResponse<LogoutUserResponse>.SuccessResult(logoutUser, 200, "User logged out");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when user logout: {ex.Message}", ex.Message);
            return ApiResponse<LogoutUserResponse>.ErrorResult("Error when user logout", 500);
        }
    }
}