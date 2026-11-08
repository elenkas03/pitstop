# PITSTOP — Auto Parts Online Store

A web application for managing and selling auto parts online, built with **ASP.NET Core MVC**, **Entity Framework Core** and **PostgreSQL**, with real card payments through **Stripe Checkout**.

Bachelor's thesis project — Plovdiv University "Paisii Hilendarski", Faculty of Mathematics and Informatics, Software Engineering.

## Features

**Storefront**
- Product catalog with filtering by category, brand, price range and text search, paginated (12 per page)
- **Vehicle compatibility filter** — find parts that fit a specific make / model / year
- Product page with stock status and the list of compatible vehicles
- Session-based shopping cart (works for anonymous visitors)
- Registration, login and order history (ASP.NET Core Identity)
- Checkout with Stripe Checkout and webhook-confirmed payment

**Admin panel** (role `Admin`, separate MVC Area)
- Dashboard with product / order counts, revenue and low-stock list
- Full CRUD for products, create / delete for categories
- Order list, details and order status management (Pending → Paid → Shipped → Delivered / Cancelled)
- Automatic URL slugs with Cyrillic-to-Latin transliteration

## Tech stack

| Layer | Technology |
|---|---|
| Backend | C#, ASP.NET Core 9 MVC |
| Data access | Entity Framework Core 9 + Npgsql |
| Database | PostgreSQL (`text[]` for product images) |
| Auth | ASP.NET Core Identity (roles `Admin`, `Customer`) |
| Payments | Stripe.net — Checkout Sessions + signed webhooks |
| Frontend | Razor views, Bootstrap 5, custom CSS |
| Local environment | Docker (PostgreSQL), .NET user-secrets |

## How the payment flow works

1. On checkout the app creates an `Order` with status `Pending`, then a Stripe Checkout Session carrying the order id in its metadata.
2. The customer pays on the Stripe-hosted page — card data never touches this server.
3. Stripe calls `POST /Checkout/Webhook`. The app verifies the signature, then marks the order `Paid` and decreases stock.

The webhook (not the browser redirect) is the source of truth for "paid", because the customer may never return to the site.

## Architecture

```
Browser ──HTTPS──▶ ASP.NET Core MVC (Controllers · Services · Views · Areas/Admin)
                        │                         │
                  EF Core / Npgsql          Stripe Checkout + webhook
                        ▼
                   PostgreSQL
```

```
PitstopWeb/
├── Areas/Admin/   Admin controllers, views, view models
├── Controllers/   Home, Products, Cart, Checkout, Account, Orders
├── Data/          ApplicationDbContext, SeedData
├── Models/        Entities and view models
├── Services/      CartService, SlugHelper
├── Views/         Razor views
└── Migrations/    EF Core migrations
```

Database tables: `AspNetUsers`, `AspNetRoles`, `Categories`, `Products`, `ProductCompatibilities`, `Orders`, `OrderItems`.

## Running locally

Requirements: .NET 9 SDK, Docker, a free Stripe account (test mode) and the Stripe CLI.

```bash
# 1. PostgreSQL
docker run --name pitstop-postgres -e POSTGRES_USER=pitstop -e POSTGRES_PASSWORD=pitstop -p 5434:5432 -d postgres

# 2. Stripe test keys (stored outside the repo)
cd PitstopWeb
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_..."
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_..."

# 3. Run (migrations and demo data are applied automatically)
dotnet run

# 4. In another terminal, forward Stripe webhooks to the app
stripe listen --forward-to http://localhost:5247/Checkout/Webhook
```

Open `http://localhost:5247`. Stripe test card: `4242 4242 4242 4242`, any future date, any CVC.

Demo accounts created by the seed data:

| Role | Email | Password |
|---|---|---|
| Admin | admin@pitstop.bg | Admin123! |
| Customer | demo@pitstop.bg | Demo1234! |

> The committed `appsettings.json` only contains placeholder Stripe values. Real keys are read from user-secrets.

## Author

Elenka Spasova Gishina — Software Engineering, Plovdiv University "Paisii Hilendarski".
