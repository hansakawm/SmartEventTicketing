# Smart Event Management and Ticketing System

A web application built for a metropolitan cultural council. Members can browse events, book tickets, and leave reviews. Guests can browse events with limited details and submit inquiries. Admins manage everything through a secured back office.

## Tech Stack

- ASP.NET Core MVC (.NET 9)
- Entity Framework Core 9
- ASP.NET Identity with role-based authorization
- SQL Server (via SSMS)
- Custom CSS (no Bootstrap)

## Features

**Guests**
- Browse events with restricted info (name, category, venue, date, availability tag)
- Read reviews
- Submit inquiries

**Members**
- Register with personal info and event preferences
- Full event browsing with pricing and Max Price filter
- Book tickets with a stub payment flow
- View My Bookings
- Submit reviews after attending

**Admins**
- Full CRUD on Events and Categories
- Manage inquiries with status tracking

## Setup

### Prerequisites

- Visual Studio 2022
- SQL Server / SQL Server Express
- .NET 9 SDK

### Steps

1. Clone the repository
   ```bash
   git clone https://github.com/hansakawm/SmartEventTicketing.git
   ```

2. Open `SmartEventTicketing.sln` in Visual Studio 2022

3. Update the connection string in `appsettings.json` to point to your SQL Server instance:
   ```json
   "DefaultConnection": "Server=YOUR_SERVER;Database=SmartEventTicketingDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   ```

4. Open Package Manager Console and run:
   ```
   Update-Database
   ```

5. Press F5 to run

### Default Admin Login

- Email: `admin@cultural.lk`
- Password: `Admin@123`

## Project Structure

```
SmartEventTicketing/
├── Areas/Identity/Pages/Account/    # Login, Register, Logout
├── Controllers/                     # MVC controllers
├── Data/                            # DbContext + Migrations
├── Models/                          # Entity models + SeedData
├── Views/                           # Razor views
├── wwwroot/                         # Static files (CSS, JS)
└── Program.cs                       # App entry point
```

## Notes

- Payment is simulated (stub) — no real payment provider is integrated
- Seeded data creates 5 categories, 8 sample events, and one admin user on first run

