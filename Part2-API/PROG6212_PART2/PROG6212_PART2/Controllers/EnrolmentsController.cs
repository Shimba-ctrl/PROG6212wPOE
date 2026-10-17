
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG6212_PART2.Authorization;
using PROG6212_PART2.Data;
using PROG6212_PART2.Models;

namespace PROG6212_PART2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        // Participants see their own enrolments.
        // Organisers see enrolments for their own events.
        [HttpGet]
        public async Task<IActionResult> GetEnrolments()
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
            {
                return Unauthorized(new
                {
                    message = "Please log in."
                });
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("UserRole");

            if (userId == null || string.IsNullOrEmpty(role))
                return Unauthorized();

            var query = _context.Enrolments.AsNoTracking();

            if (role == "Participant")
            {
                query = query.Where(e =>
                    e.ParticipantId == userId.Value);
            }
            else if (role == "Organiser")
            {
                query = query.Where(e =>
                    e.Category != null &&
                    e.Category.Event != null &&
                    e.Category.Event.OrganiserId == userId.Value);
            }
            else
            {
                return StatusCode(403, new
                {
                    message = "Access denied."
                });
            }

            var enrolments = await query
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.ParticipantId,
                    e.CategoryId,
                    CategoryName = e.Category!.Name,
                    EventId = e.Category!.EventId,
                    EventName = e.Category!.Event!.Name,
                    EventDate = e.Category!.Event!.EventDate,
                    EventLocation = e.Category!.Event!.Location,
                    e.EnrolmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/Enrolments/5
        // Users can only view enrolments they are authorised to access.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
            {
                return Unauthorized(new
                {
                    message = "Please log in."
                });
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("UserRole");

            if (userId == null || string.IsNullOrEmpty(role))
                return Unauthorized();

            var enrolment = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.EnrolmentId == id)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.ParticipantId,
                    e.CategoryId,
                    CategoryName = e.Category!.Name,
                    EventId = e.Category!.EventId,
                    EventName = e.Category!.Event!.Name,
                    e.EnrolmentDate,
                    e.Status
                })
                .FirstOrDefaultAsync();

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment not found."
                });
            }

            if (role == "Participant" &&
                enrolment.ParticipantId != userId.Value)
            {
                return StatusCode(403, new
                {
                    message = "You can only view your own enrolments."
                });
            }

            if (role == "Organiser")
            {
                bool ownsEvent = await _context.Categories
                    .AnyAsync(c =>
                        c.CategoryId == enrolment.CategoryId &&
                        c.Event!.OrganiserId == userId.Value);

                if (!ownsEvent)
                {
                    return StatusCode(403, new
                    {
                        message = "You can only view enrolments for your own events."
                    });
                }
            }
            else if (role != "Participant")
            {
                return StatusCode(403);
            }

            return Ok(enrolment);
        }

        // POST: api/Enrolments
        // Participants enrol themselves in a category.
        [HttpPost]
        public async Task<IActionResult> CreateEnrolment(
            Enrolment request)
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
            {
                return Unauthorized(new
                {
                    message = "Please log in."
                });
            }

            if (!SessionAuthorization.HasRole(
                    HttpContext, "Participant"))
            {
                return StatusCode(403, new
                {
                    message = "Only participants can enrol."
                });
            }

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return Unauthorized();

            if (request.CategoryId <= 0)
            {
                return BadRequest(new
                {
                    message = "A valid category ID is required."
                });
            }

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == request.CategoryId);

            if (category == null || category.Event == null)
            {
                return NotFound(new
                {
                    message = "Category or associated event not found."
                });
            }

            if (category.Event.EventDate < DateTime.Now)
            {
                return BadRequest(new
                {
                    message = "You cannot enrol in an event that has already taken place."
                });
            }

            bool alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantId == userId.Value &&
                    e.CategoryId == request.CategoryId);

            if (alreadyEnrolled)
            {
                return Conflict(new
                {
                    message = "You are already enrolled in this category."
                });
            }

            if (category.MaxParticipants.HasValue)
            {
                int currentEnrolments = await _context.Enrolments
                    .CountAsync(e =>
                        e.CategoryId == request.CategoryId &&
                        e.Status == "Confirmed");

                if (currentEnrolments >= category.MaxParticipants.Value)
                {
                    return Conflict(new
                    {
                        message = "This category has reached its participant limit."
                    });
                }
            }

            var enrolment = new Enrolment
            {
                ParticipantId = userId.Value,
                CategoryId = request.CategoryId,
                EnrolmentDate = DateTime.Now,
                Status = "Confirmed"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.EnrolmentId },
                new
                {
                    message = "Enrolment successful.",
                    enrolment.EnrolmentId,
                    enrolment.ParticipantId,
                    enrolment.CategoryId,
                    EventName = category.Event.Name,
                    CategoryName = category.Name,
                    enrolment.EnrolmentDate,
                    enrolment.Status
                });
        }

        // PUT: api/Enrolments/5/cancel
        // Participants can cancel their own enrolments.
        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
            {
                return Unauthorized(new
                {
                    message = "Please log in."
                });
            }

            if (!SessionAuthorization.HasRole(
                    HttpContext, "Participant"))
            {
                return StatusCode(403, new
                {
                    message = "Only participants can cancel enrolments."
                });
            }

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return Unauthorized();

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentId == id);

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment not found."
                });
            }

            if (enrolment.ParticipantId != userId.Value)
            {
                return StatusCode(403, new
                {
                    message = "You can only cancel your own enrolments."
                });
            }

            if (enrolment.Status == "Cancelled")
            {
                return BadRequest(new
                {
                    message = "This enrolment is already cancelled."
                });
            }

            enrolment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Enrolment cancelled successfully.",
                enrolment.EnrolmentId,
                enrolment.Status
            });
        }
    }
}

