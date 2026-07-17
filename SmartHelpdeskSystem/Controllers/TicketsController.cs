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

        // =========================================================
        // عرض التذاكر
        // =========================================================

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

        // =========================================================
        // تفاصيل التذكرة
        // =========================================================

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
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") &&
                ticket.UserId != User.Identity!.Name)
            {
                return Forbid();
            }

            ViewBag.Comments = await _context.TicketComments
                .Where(c => c.TicketId == id)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return View(ticket);
        }

        // =========================================================
        // إنشاء تذكرة
        // =========================================================

        // GET: Tickets/Create
        [Authorize(Roles = "User")]
        public IActionResult Create()
        {
           
            LoadCreateDropdowns();

            return View();
        }

        // POST: Tickets/Create
        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Description,DueDate,CategoryId,PriorityId")]
            Ticket ticket)
        {
           
            var openStatus = await _context.Statuses
                .FirstOrDefaultAsync(s => s.StatusName == "Open");

            if (openStatus == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Open status was not found in the database.");

                LoadCreateDropdowns(
                    ticket.CategoryId,
                    ticket.PriorityId);

                return View(ticket);
            }

            
            ticket.StatusId = openStatus.StatusId;
            ticket.CreatedAt = DateTime.Now;
            ticket.UpdatedAt = null;
            ticket.UserId = User.Identity!.Name;
            ticket.AssignedAgentId = null;

          
            ModelState.Remove(nameof(Ticket.StatusId));
            ModelState.Remove(nameof(Ticket.Status));
            ModelState.Remove(nameof(Ticket.UserId));
            ModelState.Remove(nameof(Ticket.AssignedAgentId));

            if (ModelState.IsValid)
            {
                _context.Tickets.Add(ticket);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Ticket created successfully.";

                return RedirectToAction(nameof(Index));
            }

         
            LoadCreateDropdowns(
                ticket.CategoryId,
                ticket.PriorityId);

            return View(ticket);
        }

        // =========================================================
        // تعديل التذكرة
        // =========================================================

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

        
            if (!User.IsInRole("Admin") &&
                ticket.UserId != User.Identity!.Name)
            {
                return Forbid();
            }

            LoadDropdowns(
                ticket.CategoryId,
                ticket.PriorityId,
                ticket.StatusId);

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "TicketId,Title,Description,CreatedAt,DueDate," +
                "AssignedAgentId,CategoryId,PriorityId,StatusId,UserId")]
            Ticket ticket)
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

        
            if (!User.IsInRole("Admin") &&
                originalTicket.UserId != User.Identity!.Name)
            {
                return Forbid();
            }

            if (!User.IsInRole("Admin"))
            {
              
                ticket.UserId = originalTicket.UserId;
                ticket.StatusId = originalTicket.StatusId;
                ticket.AssignedAgentId =
                    originalTicket.AssignedAgentId;
            }
            else
            {
              
                ticket.UserId = originalTicket.UserId;
            }

            ticket.CreatedAt = originalTicket.CreatedAt;

            ModelState.Remove(nameof(Ticket.Status));
            ModelState.Remove(nameof(Ticket.UserId));

            if (ModelState.IsValid)
            {
                try
                {
                    ticket.UpdatedAt = DateTime.Now;

                    _context.Update(ticket);

                    await _context.SaveChangesAsync();

                    TempData["Success"] =
                        "Ticket updated successfully.";

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

            LoadDropdowns(
                ticket.CategoryId,
                ticket.PriorityId,
                ticket.StatusId);

            return View(ticket);
        }

        // =========================================================
        // حذف التذكرة
        // =========================================================

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
                .FirstOrDefaultAsync(t => t.TicketId == id);

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

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Ticket deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // إضافة تعليق
        // =========================================================

        // POST: Tickets/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(
            int ticketId,
            string commentText)
        {
            var ticket = await _context.Tickets
                .FindAsync(ticketId);

            if (ticket == null)
            {
                return NotFound();
            }

          
            if (!User.IsInRole("Admin") &&
                ticket.UserId != User.Identity!.Name)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(commentText))
            {
                return RedirectToAction(
                    nameof(Details),
                    new { id = ticketId });
            }

            var comment = new TicketComment
            {
                TicketId = ticketId,
                CommentText = commentText.Trim(),
                UserId = User.Identity?.Name ?? "Anonymous",
                CreatedAt = DateTime.Now
            };

            _context.TicketComments.Add(comment);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Comment added successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = ticketId });
        }

        // =========================================================
        // دوال مساعدة
        // =========================================================

        private bool TicketExists(int id)
        {
            return _context.Tickets
                .Any(t => t.TicketId == id);
        }

       
        private void LoadCreateDropdowns(
            int? categoryId = null,
            int? priorityId = null)
        {
            ViewData["CategoryId"] = new SelectList(
                _context.Categories,
                "CategoryId",
                "CategoryName",
                categoryId);

            ViewData["PriorityId"] = new SelectList(
                _context.Priorities,
                "PriorityId",
                "PriorityName",
                priorityId);
        }

        private void LoadDropdowns(
            int? categoryId = null,
            int? priorityId = null,
            int? statusId = null)
        {
            ViewData["CategoryId"] = new SelectList(
                _context.Categories,
                "CategoryId",
                "CategoryName",
                categoryId);

            ViewData["PriorityId"] = new SelectList(
                _context.Priorities,
                "PriorityId",
                "PriorityName",
                priorityId);

            ViewData["StatusId"] = new SelectList(
                _context.Statuses,
                "StatusId",
                "StatusName",
                statusId);
        }
    }
}