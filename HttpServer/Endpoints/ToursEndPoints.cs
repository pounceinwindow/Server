using System.Globalization;
using System.Text.RegularExpressions;
using HttpServer.Framework.core.Attributes;
using HttpServer.Framework.Core.HttpResponse;
using HttpServer.Framework.Settings;
using MyORM;

[Endpoint]
public sealed class ToursEndpoint : BaseEndpoint
{
    private static readonly Regex HtmlRx = new("<.*?>", RegexOptions.Compiled);

    [HttpGet("/tours")]
    public IResponseResult List()
    {
        return RenderIndex(false);
    }

    [HttpGet("/tours/partial")]
    public IResponseResult Partial()
    {
        return RenderIndex(true);
    }

    private IResponseResult RenderIndex(bool partial)
    {
        var orm = new OrmContext(SettingsManager.Instance.Settings.ConnectionString!);

        var all = orm.ReadAll<Experience>("experiences").ToList();
        var details = orm.ReadAll<ExperienceDetails>("experience_details").ToList();

        var detailMap = details
            .GroupBy(d => d.ExperienceId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Id).First());

        var minDb = all.Count > 0 ? all.Min(x => x.PriceFrom) : 0m;
        var maxDb = all.Count > 0 ? all.Max(x => x.PriceFrom) : 0m;

        var q = Context.Request.QueryString;

        static decimal ParseDec(string? s, decimal def)
        {
            return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : def;
        }

        var priceMin = ParseDec(q["priceMin"], minDb);
        var priceMax = ParseDec(q["priceMax"], maxDb);

        bool Flag(string name)
        {
            return string.Equals(q[name], "1", StringComparison.OrdinalIgnoreCase);
        }

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

        int CategoryRank(Experience e)
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

        var vm = new ListingViewModel
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

            TopCategories = tabNames
                .Select(n => new CategoryItem { Name = n })
                .ToList()
        };

        if (partial)
            return Page("sem/partials/grid.html", vm); 

        return Page("sem/index.html", vm); 
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