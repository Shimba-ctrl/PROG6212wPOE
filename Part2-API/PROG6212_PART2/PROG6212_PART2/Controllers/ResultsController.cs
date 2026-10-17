
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG6212_PART2.Authorization;
using PROG6212_PART2.Data;
using PROG6212_PART2.Models;

namespace PROG6212_PART2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Results
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
                return Unauthorized(new { message = "Please log in." });

            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("UserRole");

            if (userId == null || string.IsNullOrEmpty(role))
                return Unauthorized();

            var query = _context.Results
                .AsNoTracking()
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                        .ThenInclude(c => c!.Event)
                .AsQueryable();

            if (role == "Participant")
            {
                query = query.Where(r =>
                    r.Enrolment != null &&
                    r.Enrolment.ParticipantId == userId.Value);
            }
            else if (role == "Organiser")
            {
                query = query.Where(r =>
                    r.Enrolment != null &&
                    r.Enrolment.Category != null &&
                    r.Enrolment.Category.Event != null &&
                    r.Enrolment.Category.Event.OrganiserId == userId.Value);
            }
            else
            {
                return StatusCode(403);
            }

            var results = await query
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    ParticipantId = r.Enrolment!.ParticipantId,
                    CategoryName = r.Enrolment!.Category!.Name,
                    EventName = r.Enrolment!.Category!.Event!.Name,
                    r.FinishTime,
                    r.Position,
                    r.CapturedByOrganiserId
                })
                .ToListAsync();

            return Ok(results);
        }

        // POST: api/Results
        // Organisers can record results for their own events
        [HttpPost]
        public async Task<IActionResult> CreateResult(Result request)
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
                return Unauthorized(new { message = "Please log in." });

            if (!SessionAuthorization.HasRole(HttpContext, "Organiser"))
                return StatusCode(403, new { message = "Organisers only." });

            int? organiserId = HttpContext.Session.GetInt32("UserId");

            if (organiserId == null)
                return Unauthorized();

            var enrolment = await _context.Enrolments
                .Include(e => e.Category)
                    .ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == request.EnrolmentId);

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            if (enrolment.Category?.Event?.OrganiserId != organiserId.Value)
                return StatusCode(403, new
                {
                    message = "You can only record results for your own events."
                });

            bool resultExists = await _context.Results
                .AnyAsync(r => r.EnrolmentId == request.EnrolmentId);

            if (resultExists)
                return Conflict(new { message = "A result already exists for this enrolment." });

            if (request.FinishTime < TimeSpan.Zero ||
                request.Position.HasValue && request.Position.Value <= 0)
                return BadRequest(new { message = "Finish time and position must be valid." });

            var result = new Result
            {
                EnrolmentId = request.EnrolmentId,
                FinishTime = request.FinishTime,
                Position = request.Position,
                CapturedByOrganiserId = organiserId.Value
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result recorded successfully.",
                result.ResultId,
                result.EnrolmentId,
                result.FinishTime,
                result.Position,
                result.CapturedByOrganiserId
            });
        }
    }
}
