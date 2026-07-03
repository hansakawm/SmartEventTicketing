using System.ComponentModel.DataAnnotations;

namespace SmartEventTicketing.Models
{
    public class MemberPreference
    {
        public int PreferenceId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        // Navigation properties
        public Member? Member { get; set; }
        public EventCategory? Category { get; set; }
    }
}
