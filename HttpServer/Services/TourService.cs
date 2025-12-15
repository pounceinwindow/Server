using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using HttpServer.Framework.Settings;
using MyORM;

namespace HttpServer.Services;

public sealed class TourService
{
    private static readonly Regex HtmlRx = new("<.*?>", RegexOptions.Compiled);

    private readonly string _cs;

    public TourService() : this(SettingsManager.Instance.Settings.ConnectionString!)
    {
        
    }

    public TourService(string connectionString)
    {
        _cs = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private OrmContext Open()
    {
        return new OrmContext(_cs);
    }

    public List<Experience> All()
    {
        using var db = Open();
        return db.ReadAll<Experience>("experiences");
    }

    public Experience Create(
        string title,
        string city,
        string category,
        decimal price,
        string heroUrl,
        string description = "")
    {
        var exp = new Experience
        {
            Slug = Guid.NewGuid().ToString("n")[..8],
            Title = title,
            City = city,
            CategoryName = string.IsNullOrWhiteSpace(category) ? "Activities" : category,
            PriceFrom = price,
            HeroUrl = heroUrl ?? ""
        };

        using (var db = Open())
        {
            exp = db.Create(exp, "experiences");
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            var det = new ExperienceDetails
            {
                ExperienceId = exp.Id,
                DescriptionHtml = description,
                HeroUrl = heroUrl ?? "",
                Title = exp.Title,
                City = exp.City,
                Category = exp.CategoryName,
                Price = exp.PriceFrom,
                Rating = exp.Rating,
                Reviews = exp.ReviewsCount
            };

            using var db = Open();
            db.Create(det, "experience_details");
        }

        return exp;
    }

    public Experience? FindBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        using var db = Open();
        return db.FirstOrDefault<Experience>(x => x.Slug == slug.Trim(), "experiences");
    }

    public bool DeleteBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return false;

        using var db = Open();
        var e = db.FirstOrDefault<Experience>(x => x.Slug == slug.Trim(), "experiences");
        if (e == null) return false;

        db.Delete<Experience>(e.Id, "experiences");
        return true;
    }

    public void Delete(int id)
    {
        using var db = Open();
        db.Delete<Experience>(id, "experiences");
    }


    public ListingViewModel BuildListing(NameValueCollection q)
    {
        using var db = Open();

        var all = db.ReadAll<Experience>("experiences").ToList();
        var details = db.ReadAll<ExperienceDetails>("experience_details").ToList();

        var detailMap = details
            .GroupBy(d => d.ExperienceId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Id).First());

        var minDb = all.Count > 0 ? all.Min(x => x.PriceFrom) : 0m;
        var maxDb = all.Count > 0 ? all.Max(x => x.PriceFrom) : 0m;
        

        static decimal ParseDec(string? s, decimal def)
        {
            return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : def;
        }

        bool Flag(string name)
        {
            return string.Equals(q[name], "1", StringComparison.OrdinalIgnoreCase);
        }

        var priceMin = ParseDec(q["priceMin"], minDb);
        var priceMax = ParseDec(q["priceMax"], maxDb);
        
        var instant = Flag("instant");
        var free = Flag("free");
        var guided = Flag("guided");
        var skip = Flag("skip");
        var fees = Flag("fees");
        var priv = Flag("private");
        var meal = Flag("meal");

        var categoriesSelected = q.GetValues("category")?.ToHashSet(StringComparer.OrdinalIgnoreCase)
                                 ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var filtered =
            all.Where(e => e.PriceFrom >= priceMin && e.PriceFrom <= priceMax)
                .Where(e => categoriesSelected.Count == 0 || categoriesSelected.Contains(e.CategoryName))
                .Where(e => !instant || e.InstantConfirmation)
                .Where(e => !free || e.FreeCancellation)
                .Where(e => !guided || e.GuidedTour)
                .Where(e => !skip || e.SkipTheLine)
                .Where(e => !fees || e.EntranceFeesIncluded)
                .Where(e => !priv || e.PrivateTour)
                .Where(e => !meal || e.MealIncluded);

        var sortParam = (q["sort"] ?? "").ToLowerInvariant();
        var sortKey = sortParam switch
        {
            "rating" => "rating",
            "price_desc" => "price_desc",
            "price_asc" => "price_asc",
            _ => "popularity"
        };

        static int CategoryRank(Experience e)
        {
            return e.CategoryName switch
            {
                "Attractions & guided tours" => 1,
                "Activities" => 2,
                "Tickets & events" => 3,
                _ => 4
            };
        }

        IEnumerable<Experience> sorted = sortKey switch
        {
            "popularity" => filtered
                .OrderBy(CategoryRank)
                .ThenByDescending(e => e.ReviewsCount ?? 0)
                .ThenByDescending(e => e.Rating ?? 0m),

            "rating" => filtered
                .OrderByDescending(e => e.Rating ?? 0m)
                .ThenByDescending(e => e.ReviewsCount ?? 0),

            "price_desc" => filtered.OrderByDescending(e => e.PriceFrom),
            "price_asc" => filtered.OrderBy(e => e.PriceFrom),

            _ => filtered
                .OrderBy(CategoryRank)
                .ThenByDescending(e => e.ReviewsCount ?? 0)
                .ThenByDescending(e => e.Rating ?? 0m)
        };

        var items = sorted
            .Select(e =>
            {
                var desc = "";
                if (detailMap.TryGetValue(e.Id, out var d))
                    desc = Shorten(StripHtml(d.DescriptionHtml ?? ""), 140);

                return new CardVm
                {
                    Slug = e.Slug,
                    Title = e.Title,
                    City = e.City,
                    CategoryName = e.CategoryName,
                    PriceFrom = e.PriceFrom,
                    Rating = e.Rating,
                    ReviewsCount = e.ReviewsCount ?? 0,
                    HeroUrl = e.HeroUrl,
                    Description = desc,

                    FreeCancellation = e.FreeCancellation,
                    InstantConfirmation = e.InstantConfirmation,
                    GuidedTour = e.GuidedTour,
                    SkipTheLine = e.SkipTheLine,
                    EntranceFeesIncluded = e.EntranceFeesIncluded,
                    PrivateTour = e.PrivateTour,
                    MealIncluded = e.MealIncluded
                };
            })
            .ToList();

        var tabNames = new[]
            {
                "Attractions & guided tours",
                "Excursions & day trips",
                "Activities",
                "Experiences for locals",
                "Tickets & events"
            }
            .Where(n => all.Any(a => a.CategoryName.Equals(n, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return new ListingViewModel
        {
            City = "Tours",
            Items = items,
            Total = items.Count,
            MinPrice = minDb,
            MaxPrice = maxDb,
            PriceMinText = priceMin.ToString("0.##", CultureInfo.InvariantCulture),
            PriceMaxText = priceMax.ToString("0.##", CultureInfo.InvariantCulture),

            Instant = instant,
            Free = free,
            Guided = guided,
            SkipLine = skip,
            EntranceFees = fees,
            PrivateTour = priv,
            MealIncluded = meal,
            Sort = sortKey,

            Categories = all
                .GroupBy(e => e.CategoryName)
                .Select(g => new CategoryItem
                {
                    Name = g.Key,
                    Count = g.Count(),
                    Selected = categoriesSelected.Contains(g.Key)
                })
                .OrderByDescending(x => x.Count)
                .ToList(),

            TopCategories = tabNames.Select(n => new CategoryItem { Name = n }).ToList()
        };
    }

    public ProductViewModel? BuildProduct(string slug)
    {
        var s = (slug ?? "").Trim();       
        if (string.IsNullOrWhiteSpace(s)) return null;

        using var db = Open();         
        var exp = db.FirstOrDefault<Experience>(e => e.Slug == s, "experiences"); 
        if (exp == null) return null;

        var details = db.FirstOrDefault<ExperienceDetails>(d => d.ExperienceId == exp.Id, "experience_details");


        if (!string.IsNullOrWhiteSpace(details.DescriptionHtml))
            details.DescriptionHtml = WebUtility.HtmlDecode(details.DescriptionHtml).Trim();

        var reviews = db.Where<Review>(r => r.ExperienceId == exp.Id, "reviews").ToList();

        var related = db.Where<Experience>(e => e.City == exp.City && e.Id != exp.Id, "experiences")
            .Take(4)
            .ToList();

        return new ProductViewModel
        {
            Experience = exp,
            Details = details,
            Reviews = reviews,
            RelatedTours = related
        };
    }

    private static string StripHtml(string s)
    {
        return HtmlRx.Replace(s, " ").Replace("&nbsp;", " ").Trim();
    }

    private static string Shorten(string s, int max)
    {
        return (s.Length <= max) ? s : s[..max].Trim() + "…";
    }
}