namespace Example.Models;

public class CarItem
{
    public CarBrand Brand { get; set; }
    public string? Model { get; set; }
    public int Year { get; set; }
    public decimal Price { get; set; }

    public string BrandImage => Brand.ToString().ToLower();
}

public enum CarBrand
{
   Bmw,
   Ferrari,
   Ford,
   Honda,
   Lamborghini,
   Maserati,
   Mercedes,
   Tesla
}
