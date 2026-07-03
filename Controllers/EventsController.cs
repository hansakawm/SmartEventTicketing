using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartEventTicketing.Data;
using SmartEventTicketing.Models;

namespace SmartEventTicketing.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EventsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================================
        // ADMIN ACTIONS (controller-level [Authorize(Roles="Admin")])
        // ============================================================

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.Category)
                .OrderBy(e => e.EventDate)
                .ToListAsync();
            return View(events);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (ev == null) return NotFound();

            return View(ev);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.EventCategories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventName,CategoryId,Description,Venue,EventDate,TotalSeats,StandardPrice,VIPPrice,ImageUrl")] Event ev)
        {
            if (ModelState.IsValid)
            {
                ev.AvailableSeats = ev.TotalSeats;
                _context.Add(ev);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event created successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.EventCategories, "CategoryId", "CategoryName", ev.CategoryId);
            return View(ev);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.EventCategories, "CategoryId", "CategoryName", ev.CategoryId);
            return View(ev);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,CategoryId,Description,Venue,EventDate,TotalSeats,AvailableSeats,StandardPrice,VIPPrice,ImageUrl")] Event ev)
        {
            if (id != ev.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ev);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Event updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(ev.EventId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.EventCategories, "CategoryId", "CategoryName", ev.CategoryId);
            return View(ev);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (ev == null) return NotFound();

            return View(ev);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev != null)
            {
                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }

        // ============================================================
        // PUBLIC ACTIONS (anyone can access — overrides [Authorize])
        // ============================================================

        // GET: /Events/Browse
        [AllowAnonymous]
        public async Task<IActionResult> Browse(string? searchString, int? categoryId, DateTime? startDate, DateTime? endDate, decimal? maxPrice)
        {
            var query = _context.Events
                .Include(e => e.Category)
                .Where(e => e.EventDate >= DateTime.Now)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(e => e.EventName.Contains(searchString) || e.Venue.Contains(searchString));
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(e => e.CategoryId == categoryId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.EventDate >= startDate);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.EventDate <= endDate);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(e => e.StandardPrice <= maxPrice);
            }

            var events = await query.OrderBy(e => e.EventDate).ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.CategoryId = categoryId;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Categories = await _context.EventCategories.OrderBy(c => c.CategoryName).ToListAsync();

            return View(events);
        }

        // GET: /Events/PublicDetails/5
        [AllowAnonymous]
        public async Task<IActionResult> PublicDetails(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Reviews!)
                    .ThenInclude(r => r.Member)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound();

            // Determine if current user can write a review
            bool canReview = false;
            if (User.Identity?.IsAuthenticated == true && !User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
                if (member != null && ev.EventDate < DateTime.Now)
                {
                    bool hasBooking = await _context.Bookings
                        .AnyAsync(b => b.MemberId == member.MemberId && b.EventId == id && b.PaymentStatus == "Paid");
                    bool alreadyReviewed = await _context.Reviews
                        .AnyAsync(r => r.MemberId == member.MemberId && r.EventId == id);
                    canReview = hasBooking && !alreadyReviewed;
                }
            }

            ViewBag.CanReview = canReview;
            return View(ev);
        }
    }
}
