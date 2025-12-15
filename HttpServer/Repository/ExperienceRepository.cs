using HttpServer.Infrastructure;

namespace HttpServer.Repositories;

public sealed class ExperienceRepository
{
    private readonly IOrmContextFactory _factory;

    public ExperienceRepository(IOrmContextFactory factory)
    {
        _factory = factory;
    }

    public List<Experience> GetAll()
    {
        using var db = _factory.Create();
        return db.ReadAll<Experience>("experiences");
    }

    public Experience? GetBySlug(string slug)
    {
        var s = (slug ?? "").Trim();
        if (string.IsNullOrWhiteSpace(s)) return null;

        using var db = _factory.Create();
        return db.FirstOrDefault<Experience>(e => e.Slug == s, "experiences");
    }


    public List<Experience> GetRelatedByCity(string city, int excludeId, int take)
    {
        using var db = _factory.Create();
        return db.Where<Experience>(e => e.City == city && e.Id != excludeId, "experiences").Take(take).ToList();
    }

    public Experience Create(Experience exp)
    {
        using var db = _factory.Create();
        return db.Create(exp, "experiences");
    }

    public void DeleteById(int id)
    {
        using var db = _factory.Create();
        db.Delete<Experience>(id, "experiences");
    }
}