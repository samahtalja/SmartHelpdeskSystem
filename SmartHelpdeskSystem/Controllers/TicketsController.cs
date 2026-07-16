using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartHelpdeskSystem.Data;
using SmartHelpdeskSystem.Models;

namespace SmartHelpdeskSystem.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            var tickets = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.Identity!.Name;
                tickets = tickets.Where(t => t.UserId == userId);
            }

            return View(await tickets.ToListAsync());
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .FirstOrDefaultAsync(m => m.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && ticket.UserId != User.Identity!.Name)
            {
                return Forbid();
            }

            ViewBag.Comments = await _context.TicketComments
                .Where(c => c.TicketId == id)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return View(ticket);
        }

        // GET: Tickets/Create
        [Authorize(Roles = "User")]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Description,DueDate,CategoryId,PriorityId,StatusId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.CreatedAt = DateTime.Now;
                ticket.UserId = User.Identity!.Name;
                ticket.AssignedAgentId = null;

                _context.Add(ticket);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Ticket created successfully.";
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(ticket.CategoryId, ticket.PriorityId, ticket.StatusId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && ticket.UserId != User.Identity!.Name)
            {
                return Forbid();
            }

            LoadDropdowns(ticket.CategoryId, ticket.PriorityId, ticket.StatusId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("TicketId,Title,Description,CreatedAt,DueDate,AssignedAgentId,CategoryId,PriorityId,StatusId,UserId")] Ticket ticket)
        {
            if (id != ticket.TicketId)
            {
                return NotFound();
            }

            var originalTicket = await _context.Tickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (originalTicket == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && originalTicket.UserId != User.Identity!.Name)
            {
                return Forbid();
            }

            if (!User.IsInRole("Admin"))
            {
                ticket.UserId = originalTicket.UserId;
                ticket.StatusId = originalTicket.StatusId;
                ticket.AssignedAgentId = originalTicket.AssignedAgentId;
            }
            else
            {
                ticket.UserId = originalTicket.UserId;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ticket.UpdatedAt = DateTime.Now;

                    _context.Update(ticket);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Ticket updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.TicketId))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            LoadDropdowns(ticket.CategoryId, ticket.PriorityId, ticket.StatusId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .FirstOrDefaultAsync(m => m.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Ticket deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Tickets/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int ticketId, string commentText)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && ticket.UserId != User.Identity!.Name)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(commentText))
            {
                return RedirectToAction("Details", new { id = ticketId });
            }

            var comment = new TicketComment
            {
                TicketId = ticketId,
                CommentText = commentText,
                UserId = User.Identity?.Name ?? "Anonymous",
                CreatedAt = DateTime.Now
            };

            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Comment added successfully.";
            return RedirectToAction("Details", new { id = ticketId });
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.TicketId == id);
        }

        private void LoadDropdowns(int? categoryId = null, int? priorityId = null, int? statusId = null)
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", categoryId);
            ViewData["PriorityId"] = new SelectList(_context.Priorities, "PriorityId", "PriorityName", priorityId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "StatusId", "StatusName", statusId);
        }
    }
}