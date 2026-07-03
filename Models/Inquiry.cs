using System.ComponentModel.DataAnnotations;

namespace SmartEventTicketing.Models
{
    public class Inquiry
    {
        public int InquiryId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Your Name")]
        public string GuestName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        [Display(Name = "Email Address")]
        public string GuestEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        [Display(Name = "Inquiry Date")]
        public DateTime InquiryDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Status { get; set; } = "Pending";
    }
}
