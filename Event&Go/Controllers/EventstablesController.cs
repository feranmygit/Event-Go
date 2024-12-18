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
using System.Net.Mail;
using System.Security.Claims;

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
        public async Task<IActionResult> Index(string searchBy, string SearchValue, int page = 1, int pageSize = 4)
        {
            DateTime today = DateTime.Today;

            // Update statuses
            var eventsToUpdate = _context.Eventstables.Where(e => e.Status != "Cancelled").ToList();
            foreach (var ev in eventsToUpdate)
            {
                if (ev.StartDate < today)
                    ev.Status = "Cancelled";
                else if (ev.StartDate == today)
                    ev.Status = "Ongoing";
                else if (ev.StartDate > today && ev.StartDate <= today.AddDays(3))
                    ev.Status = "Upcoming";
                else if (ev.StartDate > today.AddDays(3))
                    ev.Status = "Completed";
            }

            await _context.SaveChangesAsync();

            // Fetch events and statistics
            ViewBag.UpcomingCount = _context.Eventstables.Count(e => e.Status == "Upcoming");
            ViewBag.OngoingCount = _context.Eventstables.Count(e => e.Status == "Ongoing");
            ViewBag.PlannedCount = _context.Eventstables.Count(e => e.Status == "Completed");
            ViewBag.CancelledCount = _context.Eventstables.Count(e => e.Status == "Cancelled");

            // Updated query to exclude Planned events
            var eventsQuery = _context.Eventstables
                .Include(e => e.Category)
                .Where(e => (e.StartDate >= today || e.Status != "Cancelled") && e.Status != "Completed");

            if (!string.IsNullOrEmpty(SearchValue))
            {
                if (searchBy?.ToLower() == "eventname")
                {
                    eventsQuery = eventsQuery.Where(p => p.EventName.ToLower().Contains(SearchValue.ToLower()));
                }
                else if (searchBy?.ToLower() == "bookingusername")
                {
                    eventsQuery = eventsQuery.Where(p => p.BookingUserName != null && p.BookingUserName.ToLower().Contains(SearchValue.ToLower()));
                }
            }

            int totalEvents = await eventsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalEvents / pageSize);

            var events = await eventsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(events);
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
                "Completed",
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
            if (!ModelState.IsValid)
            {
                // Set the creator's user ID
                eventstable.CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);


                // Repopulate dropdowns if ModelState is invalid
                ViewBag.CategoryList = new SelectList(_context.Event_categories, "Id", "Category", eventstable.CategoryId);
                ViewBag.StatusList = new SelectList(new List<string> { "Upcoming", "Ongoing", "Completed", "Cancelled" }, eventstable.Status);

                // Set TempData success message
                TempData["success"] = "Form submitted successfully!";
            }


            DateTime today = DateTime.Today;

            // Automatically set the status if not provided
            if (eventstable.Status == null)
            {
                if (eventstable.StartDate < today)
                    eventstable.Status = "Cancelled";
                else if (eventstable.StartDate == today)
                    eventstable.Status = "Ongoing";
                else if (eventstable.StartDate > today && eventstable.StartDate <= today.AddDays(3))
                    eventstable.Status = "Upcoming";
                else
                    eventstable.Status = "Completed";
            }


            // File Validation
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("ImageFile", "Event image is required.");
                ViewBag.CategoryList = new SelectList(_context.Event_categories, "Id", "Category", eventstable.CategoryId);
                ViewBag.StatusList = new SelectList(new List<string> { "Upcoming", "Ongoing", "Planned", "Cancelled" }, eventstable.Status);
                return View(eventstable);
            }

            var myAppConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            // Email Configuration
            var Username = myAppConfig.GetValue<string>("EmailConfig:Username");
            var Password = myAppConfig.GetValue<string>("EmailConfig:Password");
            var Host = myAppConfig.GetValue<string>("EmailConfig:Host");
            var Port = myAppConfig.GetValue<int>("EmailConfig:Port");
            var FromEmail = myAppConfig.GetValue<string>("EmailConfig:FromEmail");

            try
            {
                // Prepare Email
                MailMessage message = new MailMessage
                {
                    From = new MailAddress(FromEmail),
                    Subject = eventstable.EventName,
                    Body = eventstable.Description,
                    IsBodyHtml = true
                };
                message.To.Add(eventstable.ToEmailAddress);
                message.Attachments.Add(new Attachment(file.OpenReadStream(), file.FileName));

                using SmtpClient mailClient = new SmtpClient(Host)
                {
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(Username, Password),
                    Host = Host,
                    Port = Port,
                    EnableSsl = true
                };

                await mailClient.SendMailAsync(message);
                ViewBag.message = "Email sent successfully.";
            }
            catch (SmtpException smtpEx)
            {
                ViewBag.message = "Email failed to send!!!";
            }
            catch (Exception ex)
            {
                ViewBag.message = "Unexpected error!!!";
            }

            // Save Image
            string path = Path.Combine(_hostEnvironment.WebRootPath, "image");
            string fileName = $"{Guid.NewGuid()}_{file.FileName}";
            string fullPath = Path.Combine(path, fileName);

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                using FileStream fs = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(fs);

                eventstable.ImageName = "/image/" + fileName; 
            }
            catch (Exception ex)
            {
                ViewBag.message = "File upload failed!!";
                return View(eventstable);
            }

            // Save to Database
            try
            {
                _context.Eventstables.Add(eventstable);
                await _context.SaveChangesAsync();
                TempData["success"] = "An event was created successfully";
            }
            catch (Exception ex)
            {
                ViewBag.message = "Database operation failed";
            }

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

            DateTime today = DateTime.Today;

            // Automatically set the status if not provided
            if (eventstable.Status == null)
            {
                if (eventstable.StartDate < today)
                    eventstable.Status = "Cancelled";
                else if (eventstable.StartDate == today)
                    eventstable.Status = "Ongoing";
                else if (eventstable.StartDate > today && eventstable.StartDate <= today.AddDays(3))
                    eventstable.Status = "Upcoming";
                else
                    eventstable.Status = "Planned";
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
            TempData["success"] = "The event was updated successfully";

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
            TempData["success"] = "The event was deleted successfully";
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin,Organizer,User")]
        public async Task<IActionResult> MyEvents()
        {
            // Get the currently logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Fetch events created by this user
            var userEvents = await _context.Eventstables
                .Include(e => e.Category) // Include related category data
                .Where(e => e.CreatedByUserId == userId) // Assuming 'CreatedByUserId' stores the creator's user ID
                .ToListAsync();

            return View(userEvents);
        }



    }
}
