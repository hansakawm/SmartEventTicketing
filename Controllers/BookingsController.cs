using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEventTicketing.Data;
using SmartEventTicketing.Models;

namespace SmartEventTicketing.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
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

        // GET: /Bookings/Create?eventId=5
        public async Task<IActionResult> Create(int? eventId)
        {
            if (eventId == null) return NotFound();

            var ev = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (ev == null) return NotFound();

            if (ev.EventDate < DateTime.Now)
            {
                TempData["Error"] = "Cannot book tickets for past events.";
                return RedirectToAction("PublicDetails", "Events", new { id = eventId });
            }

            if (ev.AvailableSeats <= 0)
            {
                TempData["Error"] = "This event is sold out.";
                return RedirectToAction("PublicDetails", "Events", new { id = eventId });
            }

            var member = await GetCurrentMemberAsync();
            if (member == null)
            {
                TempData["Error"] = "Only registered members can book tickets.";
                return RedirectToAction("PublicDetails", "Events", new { id = eventId });
            }

            ViewBag.Event = ev;
            return View(new Booking { EventId = ev.EventId });
        }

        // POST: /Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == booking.EventId);
            if (ev == null) return NotFound();

            var member = await GetCurrentMemberAsync();
            if (member == null)
            {
                TempData["Error"] = "Only registered members can book tickets.";
                return RedirectToAction("PublicDetails", "Events", new { id = booking.EventId });
            }

            if (booking.Quantity > ev.AvailableSeats)
            {
                ModelState.AddModelError("Quantity", $"Only {ev.AvailableSeats} seat(s) left. Please reduce the quantity.");
            }

            if (booking.SeatType != "Standard" && booking.SeatType != "VIP")
            {
                ModelState.AddModelError("SeatType", "Please select a valid seat type.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Event = ev;
                return View(booking);
            }

            decimal pricePerSeat = booking.SeatType == "VIP" ? ev.VIPPrice : ev.StandardPrice;
            booking.TotalAmount = pricePerSeat * booking.Quantity;
            booking.MemberId = member.MemberId;
            booking.BookingDate = DateTime.Now;
            booking.PaymentStatus = "Pending";

            // Store in TempData and pass through stub payment screen
            TempData["PendingBookingEventId"] = booking.EventId;
            TempData["PendingBookingSeatType"] = booking.SeatType;
            TempData["PendingBookingQuantity"] = booking.Quantity;
            TempData["PendingBookingTotal"] = booking.TotalAmount.ToString();

            return RedirectToAction("Payment");
        }

        // GET: /Bookings/Payment
        public async Task<IActionResult> Payment()
        {
            if (TempData["PendingBookingEventId"] == null)
            {
                return RedirectToAction("Browse", "Events");
            }

            int eventId = (int)TempData["PendingBookingEventId"]!;
            string seatType = TempData["PendingBookingSeatType"]!.ToString()!;
            int quantity = (int)TempData["PendingBookingQuantity"]!;
            decimal total = decimal.Parse(TempData["PendingBookingTotal"]!.ToString()!);

            // Keep TempData alive for ConfirmPayment
            TempData.Keep("PendingBookingEventId");
            TempData.Keep("PendingBookingSeatType");
            TempData.Keep("PendingBookingQuantity");
            TempData.Keep("PendingBookingTotal");

            var ev = await _context.Events.Include(e => e.Category).FirstOrDefaultAsync(e => e.EventId == eventId);
            if (ev == null) return NotFound();

            ViewBag.Event = ev;
            ViewBag.SeatType = seatType;
            ViewBag.Quantity = quantity;
            ViewBag.Total = total;

            return View();
        }

        // POST: /Bookings/ConfirmPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment()
        {
            if (TempData["PendingBookingEventId"] == null)
            {
                TempData["Error"] = "Booking session expired. Please try again.";
                return RedirectToAction("Browse", "Events");
            }

            int eventId = (int)TempData["PendingBookingEventId"]!;
            string seatType = TempData["PendingBookingSeatType"]!.ToString()!;
            int quantity = (int)TempData["PendingBookingQuantity"]!;
            decimal total = decimal.Parse(TempData["PendingBookingTotal"]!.ToString()!);

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (ev == null) return NotFound();

            var member = await GetCurrentMemberAsync();
            if (member == null) return RedirectToAction("Browse", "Events");

            // Re-check availability (race condition protection)
            if (quantity > ev.AvailableSeats)
            {
                TempData["Error"] = $"Sorry, only {ev.AvailableSeats} seat(s) remain. Please re-book.";
                return RedirectToAction("Create", new { eventId = eventId });
            }

            var booking = new Booking
            {
                MemberId = member.MemberId,
                EventId = eventId,
                BookingDate = DateTime.Now,
                SeatType = seatType,
                Quantity = quantity,
                TotalAmount = total,
                PaymentStatus = "Paid"
            };
            _context.Bookings.Add(booking);

            ev.AvailableSeats -= quantity;

            await _context.SaveChangesAsync();

            return RedirectToAction("Confirmation", new { id = booking.BookingId });
        }

        // GET: /Bookings/Confirmation/5
        public async Task<IActionResult> Confirmation(int? id)
        {
            if (id == null) return NotFound();

            var member = await GetCurrentMemberAsync();
            if (member == null) return Forbid();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e!.Category)
                .Include(b => b.Member)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.MemberId == member.MemberId);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // GET: /Bookings/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var member = await GetCurrentMemberAsync();
            if (member == null) return Forbid();

            var bookings = await _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e!.Category)
                .Where(b => b.MemberId == member.MemberId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }
    }
}
