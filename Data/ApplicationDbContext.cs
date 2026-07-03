using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartEventTicketing.Models;

namespace SmartEventTicketing.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for our application tables
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<MemberPreference> MemberPreferences { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Inquiry> Inquiries { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<EventCategory>().HasKey(e => e.CategoryId);
            builder.Entity<Event>().HasKey(e => e.EventId);
            builder.Entity<Member>().HasKey(e => e.MemberId);
            builder.Entity<MemberPreference>().HasKey(e => e.PreferenceId);
            builder.Entity<Booking>().HasKey(e => e.BookingId);
            builder.Entity<Review>().HasKey(e => e.ReviewId);
            builder.Entity<Inquiry>().HasKey(e => e.InquiryId);
        }
    }
}
