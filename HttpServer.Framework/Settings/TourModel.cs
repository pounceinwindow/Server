using System.Text.Json;


public class Experience
{
    public int Id { get; set; }
    public string Slug { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = "";

    public decimal PriceFrom { get; set; }

    public decimal? Rating { get; set; }
    public int? ReviewsCount { get; set; }

    public string CategoryName { get; set; } = "";
    public string HeroUrl { get; set; } = "";

    public bool InstantConfirmation { get; set; }
    public bool FreeCancellation { get; set; }
    public bool SkipTheLine { get; set; }
    public bool GuidedTour { get; set; }
    public bool EntranceFeesIncluded { get; set; }
    public bool PrivateTour { get; set; }
    public bool MealIncluded { get; set; }
}

public class ExperienceDetails
{
    public int Id { get; set; }
    public int ExperienceId { get; set; }
    public string Category { get; set; } = "";
    public string City { get; set; } = "";
    public string Title { get; set; } = "";


    public decimal? Rating { get; set; }
    public string RatingText { get; set; } = "Excellent";
    public int? Reviews { get; set; }

    public decimal Price { get; set; }

    public DateTime? ValidUntil { get; set; }

    public string DescriptionHtml { get; set; } = "";

    public string ChipsJson { get; set; } = "[]";
    public string LoveJson { get; set; } = "[]";
    public string IncludedJson { get; set; } = "[]";
    public string RememberJson { get; set; } = "[]";
    public string MoreJson { get; set; } = "[]";

    public List<string> Chips =>
        JsonSerializer.Deserialize<List<string>>(ChipsJson ?? "[]") ?? new List<string>();

    public List<string> Love =>
        JsonSerializer.Deserialize<List<string>>(LoveJson ?? "[]") ?? new List<string>();

    public List<string> Included =>
        JsonSerializer.Deserialize<List<string>>(IncludedJson ?? "[]") ?? new List<string>();

    public List<string> Remember =>
        JsonSerializer.Deserialize<List<string>>(RememberJson ?? "[]") ?? new List<string>();

    public List<MoreCard> More =>
        JsonSerializer.Deserialize<List<MoreCard>>(MoreJson ?? "[]") ?? new List<MoreCard>();
}

public class MoreCard
{
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Tag { get; set; } = "";
    public string Img { get; set; } = "";
    public decimal Price { get; set; }
}

public class Review
{
    public int Id { get; set; }
    public int ExperienceId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = "";
    public string Author { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}




public class CategoryItem
{
    public string Name { get; set; } = "";
    public bool Selected { get; set; }
    public int Count { get; set; }
}

public class ProductViewModel
{
    public Experience Experience { get; set; } = new();
    public ExperienceDetails Details { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
    public List<Experience> RelatedTours { get; set; } = new();
}



public class CardVm
{
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string City { get; set; } = "";
    public string CategoryName { get; set; } = "";

    public decimal PriceFrom { get; set; }
    public decimal? Rating { get; set; }
    public int ReviewsCount { get; set; }
    public string HeroUrl { get; set; } = "";
    public string Description { get; set; } = "";

    public bool FreeCancellation { get; set; }
    public bool InstantConfirmation { get; set; }
    public bool GuidedTour { get; set; }
    public bool SkipTheLine { get; set; }
    public bool EntranceFeesIncluded { get; set; }
    public bool PrivateTour { get; set; }
    public bool MealIncluded { get; set; }
}


public class ListingViewModel
{
    public string City { get; set; } = "Tours";

    public List<CardVm> Items { get; set; } = new();
    public int Total { get; set; }

    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public string PriceMinText { get; set; } = "";
    public string PriceMaxText { get; set; } = "";
    public string RatingMinText { get; set; } = "";

    public bool Instant { get; set; }
    public bool Free { get; set; }
    public bool Guided { get; set; }
    public bool SkipLine { get; set; }
    public bool EntranceFees { get; set; }
    public bool PrivateTour { get; set; }
    public bool MealIncluded { get; set; }

    public List<CategoryItem> Categories { get; set; } = new();

    public List<string> SelectedCategories { get; set; } = new();

    public List<CategoryItem> TopCategories { get; set; } = new();  
    public string ActiveCategory { get; set; } = "";

    public string Sort { get; set; } = "popularity";
}
