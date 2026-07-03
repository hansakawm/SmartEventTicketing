using System.ComponentModel.DataAnnotations;

namespace SmartEventTicketing.Models
{
    public class EventCategory
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Event>? Events { get; set; }
        public ICollection<MemberPreference>? MemberPreferences { get; set; }
    }
}
