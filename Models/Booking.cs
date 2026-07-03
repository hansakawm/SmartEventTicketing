using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEventTicketing.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        public int EventId { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        [Display(Name = "Seat Type")]
        public string SeatType { get; set; } = "Standard";

        [Required]
        [Range(1, 20, ErrorMessage = "Quantity must be between 1 and 20")]
        public int Quantity { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Paid";

        // Navigation properties
        public Member? Member { get; set; }
        public Event? Event { get; set; }
    }
}
