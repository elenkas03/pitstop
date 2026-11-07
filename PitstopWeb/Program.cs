using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PitstopWeb.Data;
using PitstopWeb.Models;
using PitstopWeb.Services;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);
}

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CartService>();

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
Console.WriteLine($"[DIAG] Environment = {builder.Environment.EnvironmentName}");
Console.WriteLine($"[DIAG] Stripe key loaded from user-secrets = {builder.Configuration["Stripe:SecretKey"] != "sk_test_placeholder"}");
Console.WriteLine($"[DIAG] Stripe key length = {builder.Configuration["Stripe:SecretKey"]?.Length ?? 0}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var bgCulture = new CultureInfo("bg-BG");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(bgCulture, bgCulture),
    SupportedCultures = new[] { bgCulture },
    SupportedUICultures = new[] { bgCulture },
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

var imgContentTypeProvider = new FileExtensionContentTypeProvider();
imgContentTypeProvider.Mappings[".avif"] = "image/avif";

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "img")),
    RequestPath = "/img",
    ContentTypeProvider = imgContentTypeProvider,
});

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

app.Run();
