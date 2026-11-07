using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PitstopWeb.Models;

namespace PitstopWeb.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (await userManager.FindByEmailAsync("admin@pitstop.bg") is null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@pitstop.bg",
                Email = "admin@pitstop.bg",
                FullName = "PITSTOP Admin",
                EmailConfirmed = true,
            };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (await userManager.FindByEmailAsync("demo@pitstop.bg") is null)
        {
            var demo = new ApplicationUser
            {
                UserName = "demo@pitstop.bg",
                Email = "demo@pitstop.bg",
                FullName = "Демо клиент",
                EmailConfirmed = true,
            };
            await userManager.CreateAsync(demo, "Demo1234!");
            await userManager.AddToRoleAsync(demo, "Customer");
        }

        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var categoryDefs = new (string Name, string Slug)[]
        {
            ("Спирачки", "spirachki"),
            ("Фарове", "farove"),
            ("Окачване", "okachvane"),
            ("Филтри", "filtri"),
            ("Двигател", "dvigatel"),
            ("Електрика", "elektrika"),
            ("Тунинг", "tuning"),
            ("Джанти", "dzhanti"),
        };

        var categories = categoryDefs.Select(c => new Category { Name = c.Name, Slug = c.Slug }).ToList();
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        var catBySlug = categories.ToDictionary(c => c.Slug, c => c);

        var productDefs = new[]
        {
            new { Name = "Комплект накладки Brembo Sport", Slug = "nakladki-brembo-sport", Description = "Предни спирачни накладки за спортно шофиране с висока термична устойчивост.", Price = 64.9m, Sku = "BRM-PAD-001", Brand = "Brembo", Stock = 24, Featured = true, Category = "spirachki" },
            new { Name = "Спирачен диск Bosch вентилиран 300мм", Slug = "disk-bosch-300", Description = "Преден спирачен диск, вентилиран, за подобрено охлаждане.", Price = 48.5m, Sku = "BSH-DSK-300", Brand = "Bosch", Stock = 30, Featured = false, Category = "spirachki" },
            new { Name = "Спирачен маркуч ATE усилен", Slug = "marcuch-ate", Description = "Стоманено оплетен спирачен маркуч за по-точна спирачка.", Price = 18.9m, Sku = "ATE-HOSE-01", Brand = "ATE", Stock = 40, Featured = false, Category = "spirachki" },
            new { Name = "LED фар Osram Night Breaker", Slug = "far-osram-nightbreaker", Description = "LED къси светлини с 200% по-силна светлина.", Price = 129.0m, Sku = "OSR-LED-200", Brand = "Osram", Stock = 15, Featured = true, Category = "farove" },
            new { Name = "Комплект халогенни крушки Philips", Slug = "krushki-philips", Description = "H7 халогенни крушки, дълъг живот, ясна светлина.", Price = 22.4m, Sku = "PHL-H7-01", Brand = "Philips", Stock = 60, Featured = false, Category = "farove" },
            new { Name = "Преден фар VW Golf 7", Slug = "far-vw-golf-7", Description = "Оригинален тип преден фар за VW Golf 7, ляв.", Price = 145.0m, Sku = "VW-FAR-G7L", Brand = "Hella", Stock = 8, Featured = false, Category = "farove" },
            new { Name = "Амортисьор Bilstein B4 преден", Slug = "amortisyor-bilstein-b4", Description = "Газов амортисьор за комфортно и стабилно окачване.", Price = 89.9m, Sku = "BIL-B4-F01", Brand = "Bilstein", Stock = 18, Featured = true, Category = "okachvane" },
            new { Name = "Стабилизираща щанга KYB", Slug = "stabilizatorna-shtanga-kyb", Description = "Долна стабилизираща щанга за окачване.", Price = 27.5m, Sku = "KYB-STB-02", Brand = "KYB", Stock = 35, Featured = false, Category = "okachvane" },
            new { Name = "Комплект пружини Febi Bilstein", Slug = "prujini-febi", Description = "Спортни занижени пружини, комплект за 4 колела.", Price = 156.0m, Sku = "FEB-SPR-04", Brand = "Febi Bilstein", Stock = 10, Featured = false, Category = "okachvane" },
            new { Name = "Маслен филтър Mann Filter", Slug = "maslen-filtar-mann", Description = "Оригинален маслен филтър за бензинови и дизелови двигатели.", Price = 8.9m, Sku = "MNN-OIL-01", Brand = "Mann Filter", Stock = 120, Featured = false, Category = "filtri" },
            new { Name = "Въздушен филтър K&N спортен", Slug = "vazdushen-filtar-kn", Description = "Спортен въздушен филтър с многократна употреба.", Price = 34.9m, Sku = "KN-AIR-01", Brand = "K&N", Stock = 25, Featured = true, Category = "filtri" },
            new { Name = "Купе филтър Bosch с активен въглен", Slug = "kupe-filtar-bosch", Description = "Филтър за климатична инсталация с активен въглен.", Price = 12.9m, Sku = "BSH-CAB-01", Brand = "Bosch", Stock = 80, Featured = false, Category = "filtri" },
            new { Name = "Свещи NGK Iridium (к-т 4бр.)", Slug = "sveshti-ngk-iridium", Description = "Иридиеви запалителни свещи за по-добро горене.", Price = 39.9m, Sku = "NGK-IRI-04", Brand = "NGK", Stock = 45, Featured = false, Category = "dvigatel" },
            new { Name = "Ангренажен комплект Gates", Slug = "angrenazhen-komplekt-gates", Description = "Комплект ангренажен ремък с ролки и помпа.", Price = 112.0m, Sku = "GTS-TIM-01", Brand = "Gates", Stock = 12, Featured = true, Category = "dvigatel" },
            new { Name = "Турбокомпресор Garrett реновиран", Slug = "turbo-garrett", Description = "Реновиран турбокомпресор с гаранция.", Price = 385.0m, Sku = "GRT-TRB-01", Brand = "Garrett", Stock = 5, Featured = false, Category = "dvigatel" },
            new { Name = "Акумулатор Varta Blue Dynamic 60Ah", Slug = "akumulator-varta-60ah", Description = "Стартерен акумулатор 60Ah 540A.", Price = 98.0m, Sku = "VRT-BAT-60", Brand = "Varta", Stock = 20, Featured = false, Category = "elektrika" },
            new { Name = "Алтернатор Bosch реновиран", Slug = "alternator-bosch", Description = "Реновиран алтернатор, готов за монтаж.", Price = 145.0m, Sku = "BSH-ALT-01", Brand = "Bosch", Stock = 9, Featured = false, Category = "elektrika" },
            new { Name = "Спортен ауспух FK Duplex", Slug = "auspuh-fk-duplex", Description = "Спортна ауспухова система с двоен накрайник.", Price = 249.0m, Sku = "FK-EXH-01", Brand = "FK Automotive", Stock = 7, Featured = true, Category = "tuning" },
            new { Name = "Спортен въздухопровод AEM", Slug = "vazduhoprovod-aem", Description = "Директен спортен въздухопровод за по-добро дишане на двигателя.", Price = 179.0m, Sku = "AEM-INT-01", Brand = "AEM", Stock = 11, Featured = false, Category = "tuning" },
            new { Name = "Джанти Alutec 17\" комплект", Slug = "dzhanti-alutec-17", Description = "Алуминиеви джанти 17 цола, комплект от 4 броя.", Price = 420.0m, Sku = "ALT-WHL-17", Brand = "Alutec", Stock = 6, Featured = true, Category = "dzhanti" },
            new { Name = "Джанти OZ Racing 18\"", Slug = "dzhanti-oz-racing-18", Description = "Спортни джанти OZ Racing, 18 цола, леки и здрави.", Price = 620.0m, Sku = "OZ-WHL-18", Brand = "OZ Racing", Stock = 4, Featured = false, Category = "dzhanti" },
        };

        var imageExtensions = new Dictionary<string, string>
        {
            ["disk-bosch-300"] = "avif",
        };

        var products = productDefs.Select(p => new Product
        {
            Name = p.Name,
            Slug = p.Slug,
            Description = p.Description,
            Price = p.Price,
            Sku = p.Sku,
            Brand = p.Brand,
            Stock = p.Stock,
            IsFeatured = p.Featured,
            Images = new List<string>
            {
                $"/img/{p.Slug}.{(imageExtensions.TryGetValue(p.Slug, out var ext) ? ext : "jpg")}",
            },
            CategoryId = catBySlug[p.Category].Id,
        }).ToList();

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var bySlug = products.ToDictionary(p => p.Slug, p => p);

        var compatDefs = new (string Slug, string Make, string Model, int From, int To)[]
        {
            ("nakladki-brembo-sport", "Volkswagen", "Golf", 2013, 2020),
            ("nakladki-brembo-sport", "Audi", "A3", 2012, 2019),
            ("disk-bosch-300", "BMW", "3 Series", 2011, 2018),
            ("far-vw-golf-7", "Volkswagen", "Golf", 2013, 2017),
            ("far-osram-nightbreaker", "Opel", "Astra", 2010, 2021),
            ("amortisyor-bilstein-b4", "Ford", "Focus", 2011, 2018),
            ("amortisyor-bilstein-b4", "Toyota", "Corolla", 2013, 2019),
            ("maslen-filtar-mann", "Volkswagen", "Golf", 2005, 2023),
            ("sveshti-ngk-iridium", "Toyota", "Corolla", 2005, 2023),
            ("akumulator-varta-60ah", "BMW", "3 Series", 2005, 2023),
        };

        foreach (var c in compatDefs)
        {
            context.ProductCompatibilities.Add(new ProductCompatibility
            {
                ProductId = bySlug[c.Slug].Id,
                Make = c.Make,
                Model = c.Model,
                YearFrom = c.From,
                YearTo = c.To,
            });
        }

        await context.SaveChangesAsync();
    }
}
