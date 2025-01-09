using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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

    public string GenerateAccessToken(string userId, string userRole)
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

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating access token for user ID: {UserId}", userId);
            throw new Exception("An error occurred while generating the access token.");
        }
    }

    public async Task<RefreshToken> GenerateRefreshToken(string userId)
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
            return tokenModel;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating refresh token for user ID: {UserId}", userId);
            throw new Exception("An error occurred while generating the refresh token.");
        }
    }

    public async Task RevokeRefreshToken(string token)
    {
        try
        {
            var filter = Builders<RefreshToken>.Filter.Eq(rt => rt.Token, token);
            var update = Builders<RefreshToken>.Update.Set(rt => rt.Revoked, true);
            await _refreshTokenCollection.UpdateOneAsync(filter, update);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error revoking refresh token: {Token}", token);
            throw new Exception("An error occurred while revoking the refresh token.");
        }
    }

    public async Task<RefreshToken> GetRefreshToken(string token)
    {
        try
        {
            return await _refreshTokenCollection.Find(rt => rt.Token == token).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving refresh token: {Token}", token);
            throw new Exception("An error occurred while retrieving the refresh token.");
        }
    }

    public async Task<RefreshToken?> GetRefreshTokenByUserId(string userId)
    {
        try
        {
            var token = await _refreshTokenCollection
                .Find(t => t.UserId == userId)
                .SortByDescending(t => t.ExpiryDate)
                .FirstOrDefaultAsync();

            return token;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving refresh token: {userId}", userId);
            throw new Exception("An error occurred while retrieving the refresh token.");
        }
    }

    public async Task<bool> ValidateRefreshToken(string token)
    {
        try
        {
            var storedRefreshToken = await GetRefreshToken(token);

            return storedRefreshToken.Token == token
                   && !storedRefreshToken.Revoked
                   && storedRefreshToken.ExpiryDate >= DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating refresh token: {Token}", token);
            throw new Exception("An error occurred while validating the refresh token.");
        }
    }
}