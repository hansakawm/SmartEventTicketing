using System.ComponentModel.DataAnnotations;

namespace SmartEventTicketing.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Review Date")]
        public DateTime ReviewDate { get; set; } = DateTime.Now;

        // Navigation properties
        public Member? Member { get; set; }
        public Event? Event { get; set; }
    }
}
