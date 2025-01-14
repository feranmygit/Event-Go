using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Event_Go.Data;
using WebApp.Models;
using Event_Go.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Authorization;

namespace Event_Go.Controllers
{
    public class Event_categoryController : Controller
    {
        private readonly AppDbContext _context;

        public Event_categoryController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Organizer")]

        // GET: Event_category

        public async Task<IActionResult> Index(string searchBy, string SearchValue)
        {
            var categoryList = _context.Event_categories;

            if (string.IsNullOrEmpty(SearchValue))
            {
                TempData["InfoMessage"] = "Please provide the search value...";
                return View(await categoryList.ToListAsync());
            }

            if (searchBy.ToLower() == "priceperhour")
            {
                if (decimal.TryParse(SearchValue, out decimal price))
                {
                    var searchByPrice = categoryList.Where(p => p.PricePerHour == price);
                    return View(await searchByPrice.ToListAsync());
                }
                else
                {
                    TempData["InfoMessage"] = "Please enter a valid number for Price Per Hour.";
                    return View(await categoryList.ToListAsync());
                }
            }

            else if (searchBy.ToLower() == "category")
            {
                var searchByName = categoryList.Where(p => p.Category != null && p.Category.ToLower().Contains(SearchValue.ToLower()));
                return View(await searchByName.ToListAsync());
            }

            return View(await categoryList.ToListAsync());
        }


        [Authorize(Roles = "Admin,Organizer")]

        // GET: Event_category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var event_category = await _context.Event_categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (event_category == null)
            {
                return NotFound();
            }

            return View(event_category);
        }


        [Authorize(Roles = "Admin")]


        // GET: Event_category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Event_category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Category,Description,PricePerHour,PricePerDay,IsActive,MaxCapacity")] Event_category event_category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(event_category);
                await _context.SaveChangesAsync();
                TempData["success"] = "A new category was created successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(event_category);
        }

        [Authorize(Roles = "Admin")]

        // GET: Event_category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var event_category = await _context.Event_categories.FindAsync(id);
            if (event_category == null)
            {
                return NotFound();
            }
            return View(event_category);
        }

        [Authorize(Roles = "Admin")]

        // POST: Event_category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Category,Description,PricePerHour,PricePerDay,IsActive,MaxCapacity")] Event_category event_category)
        {
            if (id != event_category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(event_category);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "The category was updated successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Event_categoryExists(event_category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(event_category);
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, bool isActive)
        //{
        //    // Find the Event_category from the database by Id
        //    var eventCategory = await _context.Event_categories.FindAsync(id);
        //    if (eventCategory == null)
        //    {
        //        return NotFound();
        //    }
        //    // Update only the IsActive field
        //    eventCategory.IsActive = isActive;

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {

        //            // Update only the IsActive field
        //            _context.Update(eventCategory);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!Event_categoryExists(eventCategory.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }

        //        // Redirect to the Index view after update
        //        return RedirectToAction(nameof(Index));
        //    }

        //    return View(eventCategory); // Return to view if ModelState is invalid
        //}


        [Authorize(Roles = "Admin")]

        // GET: Event_category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var event_category = await _context.Event_categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (event_category == null)
            {
                return NotFound();
            }

            return View(event_category);
        }


        [Authorize(Roles = "Admin")]

        // POST: Event_category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var event_category = await _context.Event_categories.FindAsync(id);
            if (event_category != null)
            {
                _context.Event_categories.Remove(event_category);
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "A category was deleted successfully";
            return RedirectToAction(nameof(Index));
        }


        //[HttpPost]
        //public async Task<IActionResult> UpdateIsActive(int id, string isActive)
        //{
        //    var category = await _context.Event_categories.FindAsync(id);
        //    if (category == null) return NotFound();

        //    category.IsActive = isActive == "true";
        //    _context.Update(category);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //}



        // SearchCategory action with search and pagination
        //public async Task<IActionResult> SearchCategory(string searchTerm, int pageNumber = 1)
        //{
        //    int pageSize = 10;

        //    var query = _context.Event_categories.AsQueryable();

        //    if (!string.IsNullOrEmpty(searchTerm))
        //    {
        //        query = query.Where(e => e.Category.Contains(searchTerm));
        //    }

        //    int totalCount = await query.CountAsync();

        //    var eventsPaged = await query
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        //    var viewModel = new CategoryViewModel
        //    {
        //        EventCategories = eventsPaged,
        //        SearchTerm = searchTerm,
        //        PageNumber = pageNumber,
        //        PageSize = pageSize,
        //        TotalCount = totalCount,
        //        TotalPages = totalPages
        //    };

        //    // Return the view with the view model
        //    return View(viewModel);
        //}

        //public IActionResult Indexex()
        //{
        //    var listAll = _context.Event_categories
        //                    .Include(x => x.Person)
        //                    .ThenInclude(x => x.Entity)
        //                    .ThenInclude(x => x.Country)
        //                    .ToList();

        //    List<website.Models.List> newList = new List<website.Models.List>();
        //    foreach (var item in listAll)
        //    {
        //        website.Models.List listItem = new website.Models.List();
        //        listItem.countryId = item.countryId;
        //        //add your remaining fields
        //        newList.Add(listItem);
        //    }

        //    return View(newList);
        //}

        private bool Event_categoryExists(int id)
        {
            return _context.Event_categories.Any(e => e.Id == id);
        }
    }
}
