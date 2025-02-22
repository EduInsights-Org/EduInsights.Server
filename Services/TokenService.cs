using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;
using EduInsights.Server.Interfaces;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace EduInsights.Server.Services;

public class TokenService(IConfiguration configuration, IMongoDatabase database, ILogger<TokenService> logger)
    : IRefreshService
{
    private readonly IMongoCollection<RefreshToken> _refreshTokenCollection =
        database.GetCollection<RefreshToken>("refresh_tokens");

    public ApiResponse<string> GenerateAccessToken(string userId, string userRole)
    {
        try
        {
            var jwtSettings = configuration.GetSection("JwtSettings");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(ClaimTypes.Role, userRole),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var secretKey = jwtSettings["key"]!;
            var expiryMinutes = jwtSettings["ExpiryMinutes"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(expiryMinutes)),
                signingCredentials: credentials
            );

            var writeToken = new JwtSecurityTokenHandler().WriteToken(token);
            return writeToken is null
                ? ApiResponse<string>.ErrorResult("Error when writing Access token", 500)
                : ApiResponse<string>.SuccessResult(writeToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when generating Access token: {ex.Message}", ex.Message);
            return ApiResponse<string>.ErrorResult("Error when generating Access token", 500);
        }
    }

    public async Task<ApiResponse<RefreshToken>> GenerateRefreshToken(string userId)
    {
        try
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var tokenModel = new RefreshToken
            {
                Token = refreshToken,
                UserId = userId,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenCollection.InsertOneAsync(tokenModel);
            return ApiResponse<RefreshToken>.SuccessResult(tokenModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when generating the refresh token: {UserId}", userId);
            return ApiResponse<RefreshToken>.ErrorResult("Error when generating the refresh token.", 500);
        }
    }

    public async Task<ApiResponse<string>> RevokeRefreshToken(string token)
    {
        try
        {
            var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.Token, token);
            var update = Builders<RefreshToken>.Update.Set(rt => rt.Revoked, true);
            var result = await _refreshTokenCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0
                ? ApiResponse<string>.SuccessResult("Refresh token successfully revoked.")
                : ApiResponse<string>.SuccessResult("Refresh token not found or already revoked.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when revoking refresh token: {Token}", token);
            return ApiResponse<string>.ErrorResult("Error when revoking the refresh token.", 500);
        }
    }

    public async Task<ApiResponse<RefreshToken>> GetRefreshToken(string token)
    {
        try
        {
            var refreshToken = await _refreshTokenCollection.Find(rt => rt.Token == token).FirstOrDefaultAsync();
            return refreshToken is null
                ? ApiResponse<RefreshToken>.ErrorResult("Refresh token not found.", 404)
                : ApiResponse<RefreshToken>.SuccessResult(refreshToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when retrieving refresh token: {Token}", token);
            return ApiResponse<RefreshToken>.ErrorResult("Error when retrieving the refresh token.", 500);
        }
    }

    public async Task<ApiResponse<RefreshToken>> GetRefreshTokenByUserId(string userId)
    {
        try
        {
            var token = await _refreshTokenCollection
                .Find(t => t.UserId == userId)
                .SortByDescending(t => t.ExpiryDate)
                .FirstOrDefaultAsync();
            return ApiResponse<RefreshToken>.SuccessResult(token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when retrieving refresh token: {userId}", userId);
            return ApiResponse<RefreshToken>.ErrorResult("Error when retrieving the refresh token.", 500);
        }
    }

    public async Task<ApiResponse<bool>> ValidateRefreshToken(string token)
    {
        try
        {
            var storedRefreshToken = (await GetRefreshToken(token)).Data;
            if (storedRefreshToken is null) return ApiResponse<bool>.SuccessResult(false);

            var isValid = storedRefreshToken.Token == token
                          && !storedRefreshToken.Revoked
                          && storedRefreshToken.ExpiryDate >= DateTime.UtcNow;
            return ApiResponse<bool>.SuccessResult(isValid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when validating refresh token: {Token}", token);
            return ApiResponse<bool>.ErrorResult("Error when validating the refresh token.", 500);
        }
    }
}