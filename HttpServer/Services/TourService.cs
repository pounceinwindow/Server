using HttpServer.Framework.Settings;
using MyORM;
namespace HttpServer.Services;
public sealed class TourService
{
    private static string Cs => SettingsManager.Instance.Settings.ConnectionString!;

    public List<Experience> All()
    {
        using var db = new OrmContext(Cs);
        return db.ReadAll<Experience>("experiences");
    }

    public Experience Create(string title, string city, string category, decimal price, string hero_url, string description = "")
    {
        var cs   = SettingsManager.Instance.Settings.ConnectionString!;
        var exp  = new Experience
        {
            Slug         = Guid.NewGuid().ToString("n")[..8],
            Title        = title,
            City         = city,
            CategoryName = string.IsNullOrWhiteSpace(category) ? "Activities" : category,
            PriceFrom    = price,
            HeroUrl      = hero_url
            
        };

        using (var db = new OrmContext(cs))
            exp = db.Create(exp, "experiences");

        if (!string.IsNullOrWhiteSpace(description))
        {
            var det = new ExperienceDetails
            {
                ExperienceId   = exp.Id,
                DescriptionHtml = description,
                HeroUrl = hero_url
            };
            using var db = new OrmContext(cs);
            db.Create(det, "experience_details");
        }

        return exp;
    }

    public Experience? FindBySlug(string slug)
    {
        using var db = new OrmContext(Cs);
        return db.ReadAll<Experience>("experiences").FirstOrDefault(x => x.Slug == slug);
    }

    public bool DeleteBySlug(string slug)
    {
        using var db = new OrmContext(Cs);
        var e = db.ReadAll<Experience>("experiences").FirstOrDefault(x => x.Slug == slug);
        if (e == null) return false;
        db.Delete<Experience>(e.Id, "experiences");
        return true;
    }

    public void Delete(int id)
    {
        using var db = new OrmContext(Cs);
        db.Delete<Experience>(id, "experiences");
    }
}