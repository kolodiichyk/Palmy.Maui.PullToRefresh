using Example.Models;

namespace Example.Services;

public class CarItemGenerator
{
    private static readonly Random _random = new Random();

    // Dictionary of car models for each brand
    private static readonly Dictionary<CarBrand, string[]> _carModels = new Dictionary<CarBrand, string[]>
    {
        { CarBrand.Bmw, new[] { "3 Series", "5 Series", "X3", "X5", "i4", "iX" } },
        { CarBrand.Ferrari, new[] { "488", "F8", "Portofino", "Roma", "SF90", "296 GTB" } },
        { CarBrand.Ford, new[] { "Focus", "Mustang", "Explorer", "F-150", "Escape", "Bronco" } },
        { CarBrand.Honda, new[] { "Civic", "Accord", "CR-V", "Pilot", "Fit", "Ridgeline" } },
        { CarBrand.Lamborghini, new[] { "Huracán", "Aventador", "Urus", "Gallardo", "Murciélago" } },
        { CarBrand.Maserati, new[] { "Ghibli", "Quattroporte", "Levante", "MC20", "GranTurismo" } },
        { CarBrand.Mercedes, new[] { "C-Class", "E-Class", "S-Class", "GLE", "GLA", "AMG GT" } },
        { CarBrand.Tesla, new[] { "Model S", "Model 3", "Model X", "Model Y", "Cybertruck", "Roadster" } }
    };

    // Price ranges for each brand (min, max)
    private static readonly Dictionary<CarBrand, (decimal min, decimal max)> _priceRanges = new Dictionary<CarBrand, (decimal, decimal)>
    {
        { CarBrand.Bmw, (35000m, 120000m) },
        { CarBrand.Ferrari, (200000m, 500000m) },
        { CarBrand.Ford, (20000m, 80000m) },
        { CarBrand.Honda, (18000m, 60000m) },
        { CarBrand.Lamborghini, (200000m, 600000m) },
        { CarBrand.Maserati, (70000m, 180000m) },
        { CarBrand.Mercedes, (35000m, 200000m) },
        { CarBrand.Tesla, (35000m, 130000m) }
    };

    /// <summary>
    /// Generates a random CarItem with realistic data
    /// </summary>
    public static CarItem GenerateRandom()
    {
        var brands = Enum.GetValues<CarBrand>();
        var randomBrand = brands[_random.Next(brands.Length)];

        var models = _carModels[randomBrand];
        var randomModel = models[_random.Next(models.Length)];

        var currentYear = DateTime.Now.Year;
        var randomYear = _random.Next(2015, currentYear + 2); // 2015 to next year

        var priceRange = _priceRanges[randomBrand];
        var randomPrice = Math.Round(
            (decimal)(_random.NextDouble() * (double)(priceRange.max - priceRange.min)) + priceRange.min,
            2);

        return new CarItem
        {
            Brand = randomBrand,
            Model = randomModel,
            Year = randomYear,
            Price = randomPrice
        };
    }

    /// <summary>
    /// Generates a CarItem with specific brand but random other properties
    /// </summary>
    public static CarItem GenerateByBrand(CarBrand brand)
    {
        var models = _carModels[brand];
        var randomModel = models[_random.Next(models.Length)];

        var currentYear = DateTime.Now.Year;
        var randomYear = _random.Next(2015, currentYear + 2);

        var priceRange = _priceRanges[brand];
        var randomPrice = Math.Round(
            (decimal)(_random.NextDouble() * (double)(priceRange.max - priceRange.min)) + priceRange.min,
            2);

        return new CarItem
        {
            Brand = brand,
            Model = randomModel,
            Year = randomYear,
            Price = randomPrice
        };
    }

    /// <summary>
    /// Generates multiple random CarItem instances
    /// </summary>
    public static List<CarItem> GenerateMultiple(int count)
    {
        var cars = new List<CarItem>();
        for (int i = 0; i < count; i++)
        {
            cars.Add(GenerateRandom());
        }
        return cars;
    }

    /// <summary>
    /// Generates CarItem instances ensuring all brands are represented
    /// </summary>
    public static List<CarItem> GenerateAllBrands()
    {
        var brands = Enum.GetValues<CarBrand>();
        return brands.Select(GenerateByBrand).ToList();
    }

    /// <summary>
    /// Creates a CarItem with specific parameters
    /// </summary>
    public static CarItem Create(CarBrand brand, string model, int year, decimal price)
    {
        return new CarItem
        {
            Brand = brand,
            Model = model,
            Year = year,
            Price = price
        };
    }

    /// <summary>
    /// Builder pattern for CarItem creation
    /// </summary>
    public class CarItemBuilder
    {
        private CarBrand _brand;
        private string _model;
        private int _year;
        private decimal _price;

        public CarItemBuilder WithBrand(CarBrand brand)
        {
            _brand = brand;
            return this;
        }

        public CarItemBuilder WithModel(string model)
        {
            _model = model;
            return this;
        }

        public CarItemBuilder WithYear(int year)
        {
            _year = year;
            return this;
        }

        public CarItemBuilder WithPrice(decimal price)
        {
            _price = price;
            return this;
        }

        public CarItemBuilder WithRandomModel()
        {
            if (_carModels.ContainsKey(_brand))
            {
                var models = _carModels[_brand];
                _model = models[_random.Next(models.Length)];
            }
            return this;
        }

        public CarItemBuilder WithRandomPrice()
        {
            if (_priceRanges.ContainsKey(_brand))
            {
                var priceRange = _priceRanges[_brand];
                _price = Math.Round(
                    (decimal)(_random.NextDouble() * (double)(priceRange.max - priceRange.min)) + priceRange.min,
                    2);
            }
            return this;
        }

        public CarItemBuilder WithCurrentYear()
        {
            _year = DateTime.Now.Year;
            return this;
        }

        public CarItem Build()
        {
            return new CarItem
            {
                Brand = _brand,
                Model = _model,
                Year = _year,
                Price = _price
            };
        }
    }

    /// <summary>
    /// Factory method for creating builder instance
    /// </summary>
    public static CarItemBuilder CreateBuilder() => new CarItemBuilder();
}
