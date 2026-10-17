

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG6212_PART2.Authorization;
using PROG6212_PART2.Data;
using PROG6212_PART2.Models;

namespace PROG6212_PART2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        // Logged-in users can view events
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
                return Unauthorized(new { message = "Please log in." });

            var events = await _context.Events
                .AsNoTracking()
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.OrganiserId,
                    e.CreatedAt
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/Events/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
                return Unauthorized(new { message = "Please log in." });

            var eventItem = await _context.Events
                .AsNoTracking()
                .Where(e => e.EventId == id)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.OrganiserId,
                    e.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (eventItem == null)
                return NotFound(new { message = "Event not found." });

            return Ok(eventItem);
        }

        // POST: api/Events
        // Organisers only
        [HttpPost]
        public async Task<IActionResult> CreateEvent(Event newEvent)
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
                return Unauthorized(new { message = "Please log in." });

            if (!SessionAuthorization.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Organisers only." });

            if (newEvent.EventDate == default ||
                string.IsNullOrWhiteSpace(newEvent.Name) ||
                string.IsNullOrWhiteSpace(newEvent.Location))
            {
                return BadRequest(new
                {
                    message = "Event name, date and location are required."
                });
            }

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return Unauthorized();

            newEvent.OrganiserId = userId.Value;
            newEvent.Name = newEvent.Name.Trim();
            newEvent.Location = newEvent.Location.Trim();
            newEvent.CreatedAt = DateTime.Now;

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = newEvent.EventId },
                newEvent);
        }

        // PUT: api/Events/5
        // Only the organiser who created the event can update it
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(
            int id, Event updatedEvent)
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
                return Unauthorized(new { message = "Please log in." });

            if (!SessionAuthorization.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Organisers only." });

            int? userId = HttpContext.Session.GetInt32("UserId");

            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (existingEvent == null)
                return NotFound(new { message = "Event not found." });

            if (userId == null || existingEvent.OrganiserId != userId.Value)
                return StatusCode(403, new
                {
                    message = "You can only update your own events."
                });

            if (string.IsNullOrWhiteSpace(updatedEvent.Name) ||
                string.IsNullOrWhiteSpace(updatedEvent.Location) ||
                updatedEvent.EventDate == default)
            {
                return BadRequest(new
                {
                    message = "Event name, date and location are required."
                });
            }

            existingEvent.Name = updatedEvent.Name.Trim();
            existingEvent.Description = updatedEvent.Description;
            existingEvent.EventDate = updatedEvent.EventDate;
            existingEvent.Location = updatedEvent.Location.Trim();

            await _context.SaveChangesAsync();

            return Ok(existingEvent);
        }

        // DELETE: api/Events/5
        // Only the organiser who created the event can delete it
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
                return Unauthorized(new { message = "Please log in." });

            if (!SessionAuthorization.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Organisers only." });

            int? userId = HttpContext.Session.GetInt32("UserId");

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
                return NotFound(new { message = "Event not found." });

            if (userId == null || eventItem.OrganiserId != userId.Value)
                return StatusCode(403, new
                {
                    message = "You can only delete your own events."
                });

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully." });
        }
    }
}

