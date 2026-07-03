using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEventTicketing.Data;
using SmartEventTicketing.Models;

namespace SmartEventTicketing.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<Member?> GetCurrentMemberAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return null;
            return await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
        }

        // GET: /Reviews/Browse (public)
        [AllowAnonymous]
        public async Task<IActionResult> Browse(string? searchString)
        {
            var query = _context.Reviews
                .Include(r => r.Event)
                    .ThenInclude(e => e!.Category)
                .Include(r => r.Member)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(r => r.Event!.EventName.Contains(searchString));
            }

            var reviews = await query.OrderByDescending(r => r.ReviewDate).ToListAsync();

            ViewBag.SearchString = searchString;
            return View(reviews);
        }

        // GET: /Reviews/Create?eventId=5 (members only)
        [Authorize]
        public async Task<IActionResult> Create(int? eventId)
        {
            if (eventId == null) return NotFound();

            var member = await GetCurrentMemberAsync();
            if (member == null)
            {
                TempData["Error"] = "Only registered members can submit reviews.";
                return RedirectToAction("PublicDetails", "Events", new { id = eventId });
            }

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (ev == null) return NotFound();

            if (ev.EventDate > DateTime.Now)
            {
                TempData["Error"] = "You can only review events that have already taken place.";
                return RedirectToAction("PublicDetails", "Events", new { id = eventId });
            }

            var hasBooking = await _context.Bookings
                .AnyAsync(b => b.MemberId == member.MemberId && b.EventId == eventId && b.PaymentStatus == "Paid");

            if (!hasBooking)
            {
                TempData["Error"] = "You can only review events you have booked.";
                return RedirectToAction("PublicDetails", "Events", new { id = eventId });
            }

            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.MemberId == member.MemberId && r.EventId == eventId);

            if (alreadyReviewed)
            {
                TempData["Error"] = "You have already reviewed this event.";
                return RedirectToAction("PublicDetails", "Events", new { id = eventId });
            }

            ViewBag.Event = ev;
            return View(new Review { EventId = ev.EventId });
        }

        // POST: /Reviews/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Review review)
        {
            var member = await GetCurrentMemberAsync();
            if (member == null) return Forbid();

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == review.EventId);
            if (ev == null) return NotFound();

            // Re-validate business rules on POST
            if (ev.EventDate > DateTime.Now)
            {
                ModelState.AddModelError("", "You can only review events that have already taken place.");
            }

            var hasBooking = await _context.Bookings
                .AnyAsync(b => b.MemberId == member.MemberId && b.EventId == review.EventId && b.PaymentStatus == "Paid");
            if (!hasBooking)
            {
                ModelState.AddModelError("", "You can only review events you have booked.");
            }

            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.MemberId == member.MemberId && r.EventId == review.EventId);
            if (alreadyReviewed)
            {
                ModelState.AddModelError("", "You have already reviewed this event.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Event = ev;
                return View(review);
            }

            review.MemberId = member.MemberId;
            review.ReviewDate = DateTime.Now;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you! Your review has been posted.";
            return RedirectToAction("PublicDetails", "Events", new { id = review.EventId });
        }
    }
}
