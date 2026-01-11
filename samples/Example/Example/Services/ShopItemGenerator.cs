using Example.Models;

namespace Example.Services;

public class ShopItemGenerator
{
    private static readonly Random _random = new Random();

    // Categories and their items
    private static readonly Dictionary<string, string[]> _categoryItems = new Dictionary<string, string[]>
    {
        { "Electronics", new[] { "Wireless Headphones", "Smart Watch", "Bluetooth Speaker", "Laptop Stand", "USB-C Hub", "Webcam" } },
        { "Home & Kitchen", new[] { "Coffee Maker", "Blender", "Air Fryer", "Vacuum Cleaner", "Toaster", "Electric Kettle" } },
        { "Sports & Outdoors", new[] { "Yoga Mat", "Dumbbell Set", "Running Shoes", "Water Bottle", "Resistance Bands", "Gym Bag" } },
        { "Books", new[] { "Fiction Novel", "Cookbook", "Self-Help Book", "Biography", "Technical Manual", "Art Book" } },
        { "Clothing", new[] { "T-Shirt", "Jeans", "Sneakers", "Jacket", "Hoodie", "Dress Shirt" } },
        { "Beauty", new[] { "Moisturizer", "Shampoo Set", "Makeup Kit", "Perfume", "Hair Dryer", "Electric Shaver" } },
        { "Toys & Games", new[] { "Board Game", "Puzzle Set", "Action Figure", "Building Blocks", "Card Game", "Remote Control Car" } },
        { "Garden", new[] { "Plant Pot Set", "Garden Tools", "Watering Can", "Seeds Collection", "Garden Gloves", "Pruning Shears" } }
    };

    // Description templates
    private static readonly string[] _descriptionTemplates = new[]
    {
        "High-quality {0} perfect for everyday use",
        "Premium {0} with advanced features",
        "Durable and reliable {0} for your needs",
        "Modern {0} with sleek design",
        "Professional-grade {0} at an affordable price",
        "Best-selling {0} with excellent reviews",
        "Innovative {0} for enhanced performance",
        "Eco-friendly {0} made with sustainable materials"
    };

    // Price ranges for each category (min, max)
    private static readonly Dictionary<string, (decimal min, decimal max)> _priceRanges = new Dictionary<string, (decimal, decimal)>
    {
        { "Electronics", (25m, 300m) },
        { "Home & Kitchen", (30m, 250m) },
        { "Sports & Outdoors", (15m, 150m) },
        { "Books", (10m, 50m) },
        { "Clothing", (20m, 200m) },
        { "Beauty", (15m, 120m) },
        { "Toys & Games", (15m, 100m) },
        { "Garden", (10m, 80m) }
    };

    /// <summary>
    /// Generates a random ShopItem with realistic data
    /// </summary>
    public static ShopItem GenerateRandom()
    {
        var categories = _categoryItems.Keys.ToArray();
        var randomCategory = categories[_random.Next(categories.Length)];

        var items = _categoryItems[randomCategory];
        var randomItem = items[_random.Next(items.Length)];

        var descriptionTemplate = _descriptionTemplates[_random.Next(_descriptionTemplates.Length)];
        var description = string.Format(descriptionTemplate, randomItem.ToLower());

        var priceRange = _priceRanges[randomCategory];
        var currentPrice = Math.Round(
            (decimal)(_random.NextDouble() * (double)(priceRange.max - priceRange.min)) + priceRange.min,
            2);

        // Old price is 10-40% higher than current price (discount scenario)
        var discountPercent = (decimal)(_random.NextDouble() * 0.3 + 0.1); // 10-40%
        var oldPrice = Math.Round(currentPrice * (1 + discountPercent), 2);

        return new ShopItem
        {
            Name = randomItem,
            Description = description,
            Price = currentPrice,
            OldPrice = oldPrice
        };
    }

    /// <summary>
    /// Generates a ShopItem from a specific category
    /// </summary>
    public static ShopItem GenerateByCategory(string category)
    {
        if (!_categoryItems.ContainsKey(category))
        {
            throw new ArgumentException($"Category '{category}' not found", nameof(category));
        }

        var items = _categoryItems[category];
        var randomItem = items[_random.Next(items.Length)];

        var descriptionTemplate = _descriptionTemplates[_random.Next(_descriptionTemplates.Length)];
        var description = string.Format(descriptionTemplate, randomItem.ToLower());

        var priceRange = _priceRanges[category];
        var currentPrice = Math.Round(
            (decimal)(_random.NextDouble() * (double)(priceRange.max - priceRange.min)) + priceRange.min,
            2);

        var discountPercent = (decimal)(_random.NextDouble() * 0.3 + 0.1);
        var oldPrice = Math.Round(currentPrice * (1 + discountPercent), 2);

        return new ShopItem
        {
            Name = randomItem,
            Description = description,
            Price = currentPrice,
            OldPrice = oldPrice
        };
    }

    /// <summary>
    /// Generates multiple random ShopItem instances
    /// </summary>
    public static List<ShopItem> GenerateMultiple(int count)
    {
        var items = new List<ShopItem>();
        for (int i = 0; i < count; i++)
        {
            items.Add(GenerateRandom());
        }
        return items;
    }

    /// <summary>
    /// Generates ShopItem instances ensuring all categories are represented
    /// </summary>
    public static List<ShopItem> GenerateAllCategories()
    {
        var categories = _categoryItems.Keys.ToArray();
        return categories.Select(GenerateByCategory).ToList();
    }

    /// <summary>
    /// Creates a ShopItem with specific parameters
    /// </summary>
    public static ShopItem Create(string name, string description, decimal price, decimal oldPrice)
    {
        return new ShopItem
        {
            Name = name,
            Description = description,
            Price = price,
            OldPrice = oldPrice
        };
    }

    /// <summary>
    /// Gets all available categories
    /// </summary>
    public static string[] GetCategories()
    {
        return _categoryItems.Keys.ToArray();
    }

    /// <summary>
    /// Builder pattern for ShopItem creation
    /// </summary>
    public class ShopItemBuilder
    {
        private string _name;
        private string _description;
        private decimal _price;
        private decimal _oldPrice;
        private string _category;

        public ShopItemBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ShopItemBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public ShopItemBuilder WithPrice(decimal price)
        {
            _price = price;
            return this;
        }

        public ShopItemBuilder WithOldPrice(decimal oldPrice)
        {
            _oldPrice = oldPrice;
            return this;
        }

        public ShopItemBuilder WithCategory(string category)
        {
            _category = category;
            return this;
        }

        public ShopItemBuilder WithRandomName()
        {
            if (!string.IsNullOrEmpty(_category) && _categoryItems.ContainsKey(_category))
            {
                var items = _categoryItems[_category];
                _name = items[_random.Next(items.Length)];
            }
            else
            {
                // Pick from any category
                var categories = _categoryItems.Keys.ToArray();
                var randomCategory = categories[_random.Next(categories.Length)];
                var items = _categoryItems[randomCategory];
                _name = items[_random.Next(items.Length)];
            }
            return this;
        }

        public ShopItemBuilder WithRandomDescription()
        {
            if (string.IsNullOrEmpty(_name))
            {
                _description = "Quality product for your needs";
            }
            else
            {
                var template = _descriptionTemplates[_random.Next(_descriptionTemplates.Length)];
                _description = string.Format(template, _name.ToLower());
            }
            return this;
        }

        public ShopItemBuilder WithRandomPrice()
        {
            if (!string.IsNullOrEmpty(_category) && _priceRanges.ContainsKey(_category))
            {
                var priceRange = _priceRanges[_category];
                _price = Math.Round(
                    (decimal)(_random.NextDouble() * (double)(priceRange.max - priceRange.min)) + priceRange.min,
                    2);
            }
            else
            {
                // Default random price
                _price = Math.Round((decimal)(_random.NextDouble() * 200 + 20), 2);
            }
            return this;
        }

        public ShopItemBuilder WithDiscountedOldPrice(decimal discountPercent = 0.2m)
        {
            if (discountPercent < 0 || discountPercent > 0.9m)
            {
                throw new ArgumentException("Discount percent must be between 0 and 0.9", nameof(discountPercent));
            }
            _oldPrice = Math.Round(_price * (1 + discountPercent), 2);
            return this;
        }

        public ShopItem Build()
        {
            return new ShopItem
            {
                Name = _name,
                Description = _description,
                Price = _price,
                OldPrice = _oldPrice
            };
        }
    }

    /// <summary>
    /// Factory method for creating builder instance
    /// </summary>
    public static ShopItemBuilder CreateBuilder() => new ShopItemBuilder();
}
