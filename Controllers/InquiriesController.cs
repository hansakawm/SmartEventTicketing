using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEventTicketing.Data;
using SmartEventTicketing.Models;

namespace SmartEventTicketing.Controllers
{
    public class InquiriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InquiriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // PUBLIC — anyone can submit
        // ============================================================

        // GET: /Inquiries/Create
        [AllowAnonymous]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Inquiries/Create
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GuestName,GuestEmail,Subject,Message")] Inquiry inquiry)
        {
            if (ModelState.IsValid)
            {
                inquiry.InquiryDate = DateTime.Now;
                inquiry.Status = "Pending";

                _context.Inquiries.Add(inquiry);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thanks for getting in touch. We'll respond to your inquiry as soon as possible.";
                return RedirectToAction(nameof(Create));
            }
            return View(inquiry);
        }

        // ============================================================
        // ADMIN — inbox + status management
        // ============================================================

        // GET: /Inquiries
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? statusFilter)
        {
            var query = _context.Inquiries.AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                query = query.Where(i => i.Status == statusFilter);
            }

            var inquiries = await query.OrderByDescending(i => i.InquiryDate).ToListAsync();

            ViewBag.StatusFilter = statusFilter ?? "All";
            ViewBag.PendingCount = await _context.Inquiries.CountAsync(i => i.Status == "Pending");

            return View(inquiries);
        }

        // GET: /Inquiries/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var inquiry = await _context.Inquiries.FirstOrDefaultAsync(i => i.InquiryId == id);
            if (inquiry == null) return NotFound();

            return View(inquiry);
        }

        // POST: /Inquiries/UpdateStatus
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var inquiry = await _context.Inquiries.FirstOrDefaultAsync(i => i.InquiryId == id);
            if (inquiry == null) return NotFound();

            if (status != "Pending" && status != "Responded" && status != "Closed")
            {
                TempData["Error"] = "Invalid status value.";
                return RedirectToAction(nameof(Details), new { id });
            }

            inquiry.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Inquiry marked as {status}.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
