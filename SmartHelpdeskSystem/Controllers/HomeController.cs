using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHelpdeskSystem.Data;
using SmartHelpdeskSystem.Models;
using System.Diagnostics;

namespace SmartHelpdeskSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
       
        public async Task<IActionResult> Dashboard()
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.IsAdmin = true;

                ViewBag.TotalTickets = await _context.Tickets.CountAsync();

                ViewBag.OpenTickets = await _context.Tickets
                    .Include(t => t.Status)
                    .CountAsync(t => t.Status != null &&
                                     t.Status.StatusName == "Open");

                ViewBag.InProgressTickets = await _context.Tickets
                    .Include(t => t.Status)
                    .CountAsync(t => t.Status != null &&
                                     t.Status.StatusName == "In Progress");

                ViewBag.ClosedTickets = await _context.Tickets
                    .Include(t => t.Status)
                    .CountAsync(t => t.Status != null &&
                                     t.Status.StatusName == "Closed");

                ViewBag.RecentTickets = await _context.Tickets
                    .Include(t => t.Category)
                    .Include(t => t.Priority)
                    .Include(t => t.Status)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(5)
                    .ToListAsync();
            }
            else
            {
                ViewBag.IsAdmin = false;

                var userEmail = User.Identity!.Name;

                var myTickets = _context.Tickets
                    .Include(t => t.Category)
                    .Include(t => t.Priority)
                    .Include(t => t.Status)
                    .Where(t => t.UserId == userEmail);

                ViewBag.TotalTickets = await myTickets.CountAsync();

                ViewBag.OpenTickets = await myTickets
                    .CountAsync(t => t.Status != null &&
                                     t.Status.StatusName == "Open");

                ViewBag.InProgressTickets = await myTickets
                    .CountAsync(t => t.Status != null &&
                                     t.Status.StatusName == "In Progress");

                ViewBag.ClosedTickets = await myTickets
                    .CountAsync(t => t.Status != null &&
                                     t.Status.StatusName == "Closed");

                ViewBag.RecentTickets = await myTickets
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(5)
                    .ToListAsync();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}