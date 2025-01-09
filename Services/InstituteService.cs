using EduInsights.Server.Entities;
using EduInsights.Server.Interfaces;
using MongoDB.Driver;

namespace EduInsights.Server.Services;

public class InstituteService(IMongoDatabase database) : IInstituteService
{
    private readonly IMongoCollection<Institute>
        _institutesCollection = database.GetCollection<Institute>("institutes");

    public async Task<Institute?> GetInstituteByUserIdAsync(string id)
    {
        try
        {
            return await _institutesCollection.Find(i => i.Id == id).FirstOrDefaultAsync();
        }
        catch
        {
            throw new Exception("An error occurred while retrieving the institute.");
        }
    }

    public async Task AddInstituteAsync(Institute institute)
    {
        try
        {
            await _institutesCollection.InsertOneAsync(institute);
        }
        catch
        {
            throw new Exception("An error occurred while creating the institute.");
        }
    }

    public async Task<List<Institute>?> GetAllInstitutes()
    {
        try
        {
            return await _institutesCollection.Find(_ => true).ToListAsync();
        }
        catch
        {
            throw new Exception("An error occurred while retrieving the institutes.");
        }
    }
}