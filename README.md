# Smart Event Management and Ticketing System

A web-based event management platform built for a metropolitan cultural council. Members can browse events, book tickets, and leave reviews. Guests can submit inquiries. Admins run the back office.

Built as coursework for the Data & Web Development module, BSc Data Science.

<img width="1918" height="1023" alt="02_home_hero" src="https://github.com/user-attachments/assets/6b4ea6fd-bed2-4b0e-be39-655da5b17b29" />

---

## What it does

Three types of users. Three experiences.

**Guests** can browse events with limited information, read member reviews, and submit inquiries through a contact form. No account needed.

**Members** register with profile details and event preferences. They can browse events with full pricing, book tickets through a payment flow, and leave reviews after attending.

**Admins** manage events, categories, and inquiries through a secured back office. Full CRUD on all content.

---

## Tech stack

- **Framework:** ASP.NET Core MVC on .NET 9
- **Database:** SQL Server Express
- **ORM:** Entity Framework Core 9 (code-first migrations)
- **Auth:** ASP.NET Identity with custom roles
- **Front-end:** Razor views, custom CSS, no Bootstrap
- **IDE:** Visual Studio 2022

---

## Features

### For everyone
- Browse upcoming events by name, category, date, or price
- View event details including venue, date, and seat availability
- Read reviews from other members
- Submit general inquiries through the contact form

### For members
- Register with full profile and event category preferences
- Book Standard or VIP tickets with a simulated payment flow
- View all past and upcoming bookings in one place
- Submit reviews for events you've attended

### For admins
- Create, edit, and delete events and categories
- Manage inquiries through a status workflow (Pending → Responded → Closed)
- View booking data per event

---

## Screenshots

### Member browsing with full pricing
<img width="1919" height="1017" alt="01_member_browse" src="https://github.com/user-attachments/assets/5fb58321-ccf6-4383-bde5-df909d1a0089" />


### Booking confirmation
<img width="1919" height="1021" alt="05_confirmation" src="https://github.com/user-attachments/assets/068a5c97-ab2b-452f-8c41-b06e3d26ea1d" />


### Admin event management
<img width="1919" height="1024" alt="02_admin_events" src="https://github.com/user-attachments/assets/01c16998-bbe1-4482-99b2-9faede383329" />


### Admin create event
<img width="1913" height="1029" alt="03_admin_create_event" src="https://github.com/user-attachments/assets/288353b8-9315-4943-a254-dd4ebf9f033e" />


### Admin inquiry inbox
<img width="1919" height="1025" alt="08_admin_inquiries" src="https://github.com/user-attachments/assets/c43745c6-b33b-489c-a9c5-8ac52992f4a0" />


### Member review posted
<img width="1918" height="1028" alt="04_review_posted" src="https://github.com/user-attachments/assets/79fbe6d5-8533-43f6-bddf-39368b6847df" />


---

## Database design

Seven application tables plus eight ASP.NET Identity tables. Designed in Oracle SQL Developer Data Modeler, then implemented on SQL Server through EF Core migrations.

### Logical ER Diagram
<img width="966" height="647" alt="ERD_Logical" src="https://github.com/user-attachments/assets/8df8a49e-2f47-49d1-b856-a4b581fa8e82" />


### Core tables
- **EventCategory** — event types (Music, Theatre, Sports, Workshop, Exhibition)
- **Member** — user profiles linked to Identity by UserId
- **MemberPreference** — junction table linking members to preferred categories
- **Event** — events with venue, date, two pricing tiers, and seat capacity
- **Booking** — ticket purchases with payment status
- **Review** — member feedback with 1-5 star ratings
- **Inquiry** — guest contact form submissions

---

## Getting started

### Prerequisites
- .NET 9 SDK
- SQL Server Express (or any SQL Server instance)
- Visual Studio 2022 or VS Code

### Setup

Clone the repo:
```bash
git clone https://github.com/[your-username]/SmartEventTicketing.git
cd SmartEventTicketing
```

Update the connection string in `appsettings.json`:
```json
"ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=SmartEventTicketingDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Apply migrations:
```bash
dotnet ef database update
```

Run the app:
```bash
dotnet run
```

The database seeds automatically on first launch with 5 categories, 8 events, and an admin account.

### Default admin credentials
- **Email:** `admin@cultural.lk`
- **Password:** `Admin@123`

> Change these in `SeedData.cs` before any real deployment.

---

## Project structure

```
SmartEventTicketing/
├── Areas/Identity/Pages/Account/   # Custom registration and login pages
├── Controllers/                     # MVC controllers (Events, Bookings, Reviews, Inquiries)
├── Data/                           # ApplicationDbContext and SeedData
├── Models/                         # Entity classes
├── Views/                          # Razor views grouped by controller
├── wwwroot/                        # Static files (CSS, images)
├── Migrations/                     # EF Core migrations
├── docs/screenshots/               # Screenshots used in documentation
└── Program.cs                      # Application entry point
```

---

## Business rules worth noting

Some rules can't be enforced by the database alone. They live in application code:

**Reviews require three conditions:**
1. The event date must be in the past
2. The member must have a paid booking for it
3. No earlier review by the same member for the same event

**Bookings re-check seat availability at the payment step.** The form check isn't trusted on its own. Someone could edit HTML in dev tools. The server checks again before writing the booking.

**Role-based URLs are blocked at the controller.** Hiding a nav link isn't enough. The `[Authorize(Roles = "Admin")]` attribute blocks direct URL access.

---

## Testing

Twenty test cases ran across six feature areas. All twenty passed. Full test documentation is in the coursework report.

Coverage breakdown:
- Guest features: 4 tests
- Registration and validation: 3 tests
- Login and browsing: 5 tests
- Booking flow: 3 tests
- Reviews: 3 tests
- Admin and security: 2 tests

---

