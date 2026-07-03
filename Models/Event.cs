using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEventTicketing.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(200)]
        public string Venue { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Total seats must be greater than 0")]
        [Display(Name = "Total Seats")]
        public int TotalSeats { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Available Seats")]
        public int AvailableSeats { get; set; }

        [Required]
        [Range(0, 100000)]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Standard Price")]
        public decimal StandardPrice { get; set; }

        [Required]
        [Range(0, 100000)]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "VIP Price")]
        public decimal VIPPrice { get; set; }

        [StringLength(300)]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        // Navigation properties
        public EventCategory? Category { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}
