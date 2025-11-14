using HttpServer.Framework.core.Attributes;
using System.Net;
using HttpServer.Framework.Core.HttpResponse;
using HttpServer.Framework.Settings;
using MyORM;

[Endpoint]
public class ProductEndpoint : BaseEndpoint
{
    [HttpGet("/product")]
    public IResponseResult Detail()
    {
        var slug = Context.Request.QueryString["slug"];
        if (string.IsNullOrWhiteSpace(slug))
            return Page("sem/404.html", null);
        
        var settings = SettingsManager.Instance.Settings;
        var db = new OrmContext(settings.ConnectionString);

        var exp = db.FirstOrDefault<Experience>(
            e => e.Slug == slug,
            "experiences"
        );
        if (exp == null)
            return Page("sem/404.html", null);

        var details = db.FirstOrDefault<ExperienceDetails>(
            d => d.ExperienceId == exp.Id,
            "experience_details"
        );

        if (details == null)
        {
            details = new ExperienceDetails
            {
                ExperienceId = exp.Id,
                Title = exp.Title,
                City = exp.City,
                Category = exp.CategoryName,
                Price = exp.PriceFrom,
                Rating = exp.Rating,
                Reviews = exp.ReviewsCount
            };
        }

        if (!string.IsNullOrWhiteSpace(details.DescriptionHtml))
            details.DescriptionHtml = WebUtility.HtmlDecode(details.DescriptionHtml).Trim();

        var reviews = db.Where<Review>(
            r => r.ExperienceId == exp.Id,
            "reviews"
        ).ToList();

        var related = db.Where<Experience>(
            e => e.City == exp.City && e.Id != exp.Id,
            "experiences"
        ).Take(4).ToList();

        var vm = new ProductViewModel
        {
            Experience = exp,
            Details = details,
            Reviews = reviews,
            RelatedTours = related
        };
        Console.WriteLine("DescriptionHtml: " + details.DescriptionHtml);
        return Page("sem/product.html", vm);
    }
}