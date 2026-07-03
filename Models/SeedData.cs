using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartEventTicketing.Data;

namespace SmartEventTicketing.Models
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Seed roles
            await SeedRolesAsync(roleManager);

            // 2. Seed admin user
            await SeedAdminUserAsync(userManager);

            // 3. Seed event categories
            await SeedCategoriesAsync(context);

            // 4. Seed sample events
            await SeedEventsAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Member" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager)
        {
            const string adminEmail = "admin@cultural.lk";
            const string adminPassword = "Admin@123";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            if (await context.EventCategories.AnyAsync()) return;

            var categories = new List<EventCategory>
            {
                new EventCategory { CategoryName = "Music" },
                new EventCategory { CategoryName = "Theatre" },
                new EventCategory { CategoryName = "Sports" },
                new EventCategory { CategoryName = "Workshop" },
                new EventCategory { CategoryName = "Exhibition" }
            };

            await context.EventCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        private static async Task SeedEventsAsync(ApplicationDbContext context)
        {
            if (await context.Events.AnyAsync()) return;

            var music = await context.EventCategories.FirstAsync(c => c.CategoryName == "Music");
            var theatre = await context.EventCategories.FirstAsync(c => c.CategoryName == "Theatre");
            var sports = await context.EventCategories.FirstAsync(c => c.CategoryName == "Sports");
            var workshop = await context.EventCategories.FirstAsync(c => c.CategoryName == "Workshop");
            var exhibition = await context.EventCategories.FirstAsync(c => c.CategoryName == "Exhibition");

            var events = new List<Event>
            {
                new Event
                {
                    EventName = "Symphony Under the Stars",
                    CategoryId = music.CategoryId,
                    Description = "An open-air orchestral performance featuring classical favourites and modern compositions.",
                    Venue = "Viharamahadevi Park, Colombo",
                    EventDate = DateTime.Now.AddDays(15),
                    TotalSeats = 500,
                    AvailableSeats = 500,
                    StandardPrice = 1500.00m,
                    VIPPrice = 4500.00m
                },
                new Event
                {
                    EventName = "Hamlet: A Modern Retelling",
                    CategoryId = theatre.CategoryId,
                    Description = "Shakespeare's classic reimagined in a contemporary urban setting.",
                    Venue = "Lionel Wendt Theatre",
                    EventDate = DateTime.Now.AddDays(22),
                    TotalSeats = 250,
                    AvailableSeats = 250,
                    StandardPrice = 2000.00m,
                    VIPPrice = 5000.00m
                },
                new Event
                {
                    EventName = "City Marathon 2026",
                    CategoryId = sports.CategoryId,
                    Description = "Annual 10K run through the heart of the city. Open to all fitness levels.",
                    Venue = "Galle Face Green",
                    EventDate = DateTime.Now.AddDays(45),
                    TotalSeats = 1000,
                    AvailableSeats = 1000,
                    StandardPrice = 800.00m,
                    VIPPrice = 2500.00m
                },
                new Event
                {
                    EventName = "Photography Masterclass",
                    CategoryId = workshop.CategoryId,
                    Description = "Hands-on workshop covering composition, lighting, and post-processing techniques.",
                    Venue = "Cultural Centre Studio A",
                    EventDate = DateTime.Now.AddDays(8),
                    TotalSeats = 30,
                    AvailableSeats = 30,
                    StandardPrice = 3500.00m,
                    VIPPrice = 7000.00m
                },
                new Event
                {
                    EventName = "Contemporary Art Showcase",
                    CategoryId = exhibition.CategoryId,
                    Description = "A curated collection of works from emerging South Asian artists.",
                    Venue = "National Art Gallery",
                    EventDate = DateTime.Now.AddDays(30),
                    TotalSeats = 200,
                    AvailableSeats = 200,
                    StandardPrice = 1000.00m,
                    VIPPrice = 3000.00m
                },
                new Event
                {
                    EventName = "Jazz Night with Local Legends",
                    CategoryId = music.CategoryId,
                    Description = "An intimate evening featuring the city's finest jazz musicians.",
                    Venue = "Barefoot Garden Cafe",
                    EventDate = DateTime.Now.AddDays(12),
                    TotalSeats = 80,
                    AvailableSeats = 80,
                    StandardPrice = 2500.00m,
                    VIPPrice = 6000.00m
                },
                new Event
                {
                    EventName = "Pottery Workshop for Beginners",
                    CategoryId = workshop.CategoryId,
                    Description = "Learn the basics of wheel throwing and hand building. All materials provided.",
                    Venue = "Crafts Council Studio",
                    EventDate = DateTime.Now.AddDays(20),
                    TotalSeats = 20,
                    AvailableSeats = 20,
                    StandardPrice = 4000.00m,
                    VIPPrice = 8000.00m
                },
                new Event
                {
                    EventName = "Heritage Cricket Tournament",
                    CategoryId = sports.CategoryId,
                    Description = "Inter-club cricket tournament featuring teams from across the metropolitan area.",
                    Venue = "Sara Stadium",
                    EventDate = DateTime.Now.AddDays(35),
                    TotalSeats = 800,
                    AvailableSeats = 800,
                    StandardPrice = 600.00m,
                    VIPPrice = 2000.00m
                }
            };

            await context.Events.AddRangeAsync(events);
            await context.SaveChangesAsync();
        }
    }
}
