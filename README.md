# 📍 NearMe

**NearMe** is a full-stack web app that helps people find local service providers (restaurants, repair shops, clinics, stores) by category and region, with ratings and reviews from real users.

Built with **ASP.NET Core Web API**, **Entity Framework Core**, **SQL Server**, and a lightweight **HTML/CSS/JavaScript** frontend.

<!-- Add a screenshot or GIF here: ![NearMe screenshot](docs/screenshot.png) -->

## ✨ Features

- 🔍 **Search** providers by name, category, and region, with pagination
- 🗺️ **Open in Google Maps** button that opens each provider's exact location
- ⭐ **Reviews and ratings**: one review per user per provider, editable by the owner
- 🖼️ **Image gallery** for each provider (upload or external link, max 10 images)
- 🔐 **JWT authentication** with ASP.NET Core Identity
- 👥 **Role-based access** with three roles: `User`, `Provider`, `Admin`
- 🛠️ **Admin tools** for managing categories (with icon upload) and regions
- 📄 **Swagger / OpenAPI** documentation with Bearer token support

**Demo Video**

https://drive.google.com/file/d/1HFEW7xMQ6NgkSYmt1yd_2-ym0yPenVdd/view?usp=drive_link

**Home page**

<img width="626" height="313" alt="Screenshot 2026-11-15 193103" src="https://github.com/user-attachments/assets/0157750c-e4c2-4209-bebc-bce4b2cc0de0" />


<img width="626" height="313" alt="Screenshot 2026-11-15 194234" src="https://github.com/user-attachments/assets/982c1a37-fd14-49db-9cb8-ea3f76723948" />

**Register & Login**

<img width="617" height="398" alt="Screenshot 2026-11-22 230431" src="https://github.com/user-attachments/assets/6c73fffd-3a5e-4f1c-adf5-ec260456e081" />

**Provider**

<img width="620" height="311" alt="Screenshot 2026-11-22 230737" src="https://github.com/user-attachments/assets/85e64667-192b-4668-9939-c9eafc7919ca" />

<img width="626" height="310" alt="Screenshot 2026-11-22 230845" src="https://github.com/user-attachments/assets/e6b7925f-c689-4817-a168-e7aae7a49f59" />

**Admin**

<img width="613" height="304" alt="Screenshot 2026-11-22 231209" src="https://github.com/user-attachments/assets/77b37a50-2cbf-4126-99a2-bbddeb2fc8b8" />






## 👥 Roles

| Role | What they can do |
|------|------------------|
| **User** | Browse and search providers, write/edit/delete their own reviews |
| **Provider** | Everything a user can do, plus create and manage their own business profile and gallery |
| **Admin** | Manage categories and regions, delete any provider, review, or image |

## 🧱 Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | C#, ASP.NET Core Web API |
| Database | SQL Server, Entity Framework Core (code-first migrations) |
| Auth | ASP.NET Core Identity, JWT Bearer |
| Docs | Swagger (Swashbuckle) |
| Frontend | HTML, CSS, vanilla JavaScript (served from `wwwroot`) |

## 🏗️ Architecture

```
NearMe/
├── Controllers/   # Thin API controllers
├── Services/      # Business logic (Provider, Review, Category, Region, images)
├── DTOs/          # Request/response models with validation
├── Models/        # EF Core entities
├── Data/          # DbContext and seeding
├── wwwroot/       # Frontend (index.html, app.js, style.css, favicon.svg)
└── Program.cs     # DI, auth, middleware setup
```

Controllers delegate to services, and services return a `ServiceResult<T>` that controllers map to the right HTTP status code (404, 409, 403, and so on).

## 🚀 Getting Started

### Prerequisites

- .NET SDK (the version in the project's `.csproj`)
- SQL Server (LocalDB, Express, or full)

### Setup

1. **Clone the repo**
   ```bash
   git clone https://github.com/<your-username>/NearMe.git
   cd NearMe
   ```

2. **Configure settings**: copy `appsettings.example.json` to `appsettings.json` and fill in your values:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=NearMeDb;Trusted_Connection=True;TrustServerCertificate=True"
     },
     "Jwt": {
       "Key": "REPLACE_WITH_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS",
       "Issuer": "NearMe",
       "Audience": "NearMeUsers"
     }
   }
   ```
   For local development, prefer [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) so the key never lands in Git:
   ```bash
   dotnet user-secrets set "Jwt:Key" "your-long-random-secret"
   ```

3. **Run the app**: migrations and seed data are applied automatically on startup.
   ```bash
   dotnet run
   ```

4. Open the app:
   - Frontend: `https://localhost:<port>/`
   - Swagger UI (Development only): `https://localhost:<port>/swagger`

## 🔌 API Overview

### Auth
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register as `User` or `Provider` |
| POST | `/api/auth/login` | Log in and receive a JWT |

### Providers: `/api/providerprofiles`
| Method | Endpoint | Access |
|--------|----------|--------|
| GET | `/` | Public (paged) |
| GET | `/{id}` | Public |
| GET | `/search?name=&categoryId=&regionId=` | Public |
| GET | `/my-profile` | Provider |
| POST | `/` | Provider |
| PUT | `/{id}` | Provider (owner) |
| DELETE | `/{id}` | Owner or Admin |

### Provider Images: `/api/providerimage`
| Method | Endpoint | Access |
|--------|----------|--------|
| GET | `/{id}`, `/provider/{providerId}` | Public |
| POST | `/` (link), `/upload` (file) | Provider |
| PUT | `/{id}` | Provider (owner) |
| DELETE | `/{id}` | Provider (owner) or Admin |

### Reviews: `/api/review`
| Method | Endpoint | Access |
|--------|----------|--------|
| GET | `/`, `/{id}`, `/provider/{id}` | Public |
| GET | `/provider/{providerId}/mine` | Authenticated |
| POST | `/` | Authenticated (not your own profile, once per provider) |
| PUT | `/{id}` | Review owner |
| DELETE | `/{id}` | Review owner or Admin |

### Categories and Regions: `/api/category`, `/api/region`
| Method | Endpoint | Access |
|--------|----------|--------|
| GET | `/`, `/{id}`, `/{name}` | Public |
| POST, PUT, DELETE | `/`, `/{id}` | Admin |
| POST | `/api/category/upload-icon` | Admin |

> Categories and regions that still have providers can't be deleted.

## 🔒 Security Notes

- Passwords are handled by ASP.NET Core Identity (hashed)
- JWT validates issuer, audience, signing key, and lifetime
- Uploads are restricted to `.jpg`, `.jpeg`, `.png`, `.webp` and max 2 MB, with random file names
- Ownership checks on every update and delete

## 🗺️ Roadmap

- [ ] Sort providers by distance from the user's location
- [ ] Map view with all providers as markers
- [ ] Unit tests for the service layer
- [ ] Docker support
 
## 👩‍💻 Author

**Asma**: [LinkedIn](www.linkedin.com/in/asmaa-amr-521382207) · [GitHub](https://github.com/asmaa33-amr0)
