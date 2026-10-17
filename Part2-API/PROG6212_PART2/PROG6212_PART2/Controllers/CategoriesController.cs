
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG6212_PART2.Authorization;
using PROG6212_PART2.Data;
using PROG6212_PART2.Models;

namespace PROG6212_PART2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
                return Unauthorized(new { message = "Please log in." });

            var categories = await _context.Categories
                .AsNoTracking()
                .Select(c => new
                {
                    c.CategoryId,
                    c.EventId,
                    c.Name,
                    c.Distance,
                    c.MaxParticipants
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/Categories/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
                return Unauthorized(new { message = "Please log in." });

            var category = await _context.Categories
                .AsNoTracking()
                .Where(c => c.CategoryId == id)
                .Select(c => new
                {
                    c.CategoryId,
                    c.EventId,
                    c.Name,
                    c.Distance,
                    c.MaxParticipants
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound(new { message = "Category not found." });

            return Ok(category);
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
                return Unauthorized(new { message = "Please log in." });

            if (!SessionAuthorization.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Organisers only." });

            bool eventExists = await _context.Events
                .AnyAsync(e => e.EventId == category.EventId);

            if (!eventExists)
                return BadRequest(new { message = "The specified event does not exist." });

            if (string.IsNullOrWhiteSpace(category.Name) || category.Distance <= 0)
                return BadRequest(new { message = "A category name and positive distance are required." });

            if (category.MaxParticipants.HasValue && category.MaxParticipants.Value <= 0)
                return BadRequest(new { message = "Maximum participants must be positive." });

            category.Name = category.Name.Trim();

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryId },
                category);
        }

        // PUT: api/Categories/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(
            int id, Category updatedCategory)
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
                return Unauthorized(new { message = "Please log in." });

            if (!SessionAuthorization.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Organisers only." });

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (string.IsNullOrWhiteSpace(updatedCategory.Name) ||
                updatedCategory.Distance <= 0)
                return BadRequest(new { message = "A category name and positive distance are required." });

            category.Name = updatedCategory.Name.Trim();
            category.Distance = updatedCategory.Distance;
            category.MaxParticipants = updatedCategory.MaxParticipants;

            await _context.SaveChangesAsync();

            return Ok(category);
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
                return Unauthorized(new { message = "Please log in." });

            if (!SessionAuthorization.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Organisers only." });

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully." });
        }
    }
}

