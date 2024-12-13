using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Event_Go.Data;
using WebApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace Event_Go.Controllers
{
    [Authorize]
    public class EventstablesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public EventstablesController(AppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        [Authorize(Roles = "Admin,Organizer,User")]
        // GET: Eventstables
        public async Task<IActionResult> Index(string searchBy, string SearchValue)
        {
            // Add event statistics for the dashboard
            ViewBag.UpcomingCount = _context.Eventstables.Count(e => e.Status == "Upcoming");
            ViewBag.OngoingCount = _context.Eventstables.Count(e => e.Status == "Ongoing");
            ViewBag.PlannedCount = _context.Eventstables.Count(e => e.Status == "Planned");
            ViewBag.CancelledCount = _context.Eventstables.Count(e => e.Status == "Cancelled");


            var appDbContext = _context.Eventstables.Include(e => e.Category);

            if (string.IsNullOrEmpty(SearchValue))
            {
                TempData["InfoMessage"] = "Please provide the search value...";
                return View( await appDbContext.ToListAsync());
            }

            if (searchBy.ToLower() == "eventname")
            {
                // Corrected: Use SearchValue in the search condition
                var searchByEventName = appDbContext.Where(p => p.EventName.ToLower().Contains(SearchValue.ToLower()));
                return View(await searchByEventName.ToListAsync());
            }
            else if (searchBy.ToLower() == "bookingusername")
            {
                // Corrected: Use SearchValue and ensure null check
                var searchByBookingUserName = appDbContext.Where(p => p.BookingUserName != null && p.BookingUserName.ToLower().Contains(SearchValue.ToLower()));
                return View(await searchByBookingUserName.ToListAsync());
            }
            return View(await appDbContext.ToListAsync());
        }



        [Authorize(Roles = "Admin,Organizer")]
        // GET: Events/Create
        public IActionResult Create()
        {
            // Populating Category list from the database
            ViewBag.CategoryList = new SelectList(_context.Event_categories, "Id", "Category");

            // Populating Status list as strings
            ViewBag.StatusList = new SelectList(new List<string>
            {
                "Upcoming",
                "Ongoing",
                "Planned",
                "Cancelled"
            });

            return View();
        }

        [Authorize(Roles = "Admin,Organizer")]
        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Eventstable eventstable, IFormFile file)
        {
            // Repopulate dropdowns if ModelState is invalid
            ViewBag.CategoryList = new SelectList(_context.Event_categories, "Id", "Category", eventstable.CategoryId);
            ViewBag.StatusList = new SelectList(new List<string>
            {
                "Upcoming", "Ongoing", "Planned", "Cancelled"
            }, eventstable.Status);


            string path = _hostEnvironment.WebRootPath + "/image/";
            Guid id = Guid.NewGuid();
            string fileName = id.ToString() + "_" + file.FileName;

            string newPath = Path.Combine(path, fileName);
            using FileStream fs = new FileStream(newPath, FileMode.Create);
            await file.CopyToAsync(fs);

            eventstable.ImageName = "/image/" + fileName; // Store only the relative path in the database
            _context.Eventstables.Add(eventstable);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            


        }
        [Authorize(Roles = "Admin,Organizer")]

        // GET: Events/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var eventstable = _context.Eventstables.Find(id);
            if (eventstable == null)
                return NotFound();

            // Populating Category list from the database
            ViewBag.CategoryList = new SelectList(_context.Event_categories, "Id", "Category", eventstable.CategoryId);

            // Populating Status list as strings
            ViewBag.StatusList = new SelectList(new List<string>
            {
                "Upcoming", "Ongoing", "Planned", "Cancelled"
            }, eventstable.Status);

            return View(eventstable);
        }


        [Authorize(Roles = "Admin,Organizer")]

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Eventstable eventstable, IFormFile file)
        {
            // Repopulate dropdowns if ModelState is invalid
            ViewBag.CategoryList = new SelectList(_context.Event_categories, "Id", "Category", eventstable.CategoryId);
            ViewBag.StatusList = new SelectList(new List<string>
    {
        "Upcoming", "Ongoing", "Planned", "Cancelled"
    }, eventstable.Status);

            if (id != eventstable.EventId)
            {
                return NotFound();
            }

            var existingEvent = await _context.Eventstables.FindAsync(id);

            if (existingEvent == null)
            {
                return NotFound();
            }

            // If a new file is uploaded, delete the old image and upload the new one
            if (file != null)
            {
                // Delete the old image if it exists
                if (!string.IsNullOrEmpty(existingEvent.ImageName))
                {
                    var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, existingEvent.ImageName.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Upload the new image
                string path = _hostEnvironment.WebRootPath + "/image/";
                Guid Id = Guid.NewGuid();
                string fileName = Id.ToString() + "_" + file.FileName;
                string newPath = Path.Combine(path, fileName);

                using (FileStream fs = new FileStream(newPath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                // Update the ImageName in the database to the new file's relative path
                existingEvent.ImageName = "/image/" + fileName;
            }

            // Update other properties of the event
            existingEvent.EventName = eventstable.EventName;
            existingEvent.Description = eventstable.Description;
            existingEvent.StartDate = eventstable.StartDate;
            existingEvent.EndDate = eventstable.EndDate;
            existingEvent.Venue = eventstable.Venue;
            existingEvent.Status = eventstable.Status;
            existingEvent.BookingUserName = eventstable.BookingUserName;
            existingEvent.CategoryId = eventstable.CategoryId;

            _context.Update(existingEvent);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Organizer,User")]
        // GET: Events/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();

            var eventstable = _context.Eventstables
                .Include(e => e.Category)
                .FirstOrDefault(m => m.EventId == id);

            if (eventstable == null)
                return NotFound();

            return View(eventstable);
        }


        [Authorize(Roles = "Admin")]
        // GET: Events/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var eventstable = _context.Eventstables
                .Include(e => e.Category)
                .FirstOrDefault(m => m.EventId == id);

            if (eventstable == null)
                return NotFound();

            return View(eventstable);
        }



        [Authorize(Roles = "Admin")]

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var eventstable = _context.Eventstables.Find(id);

            // delete the record
            if (!string.IsNullOrEmpty(eventstable.ImageName))
            {
                var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, eventstable.ImageName.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _context.Eventstables.Remove(eventstable);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

    }
}
