using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTrainingApi.Models;
using MyTrainingApi.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;

namespace MyTrainingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("workouts")]
        public async Task<IActionResult> GetWorkouts([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string exerciseName)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                IQueryable<Workout> query = _context.Workouts
                    .Where(w => w.UserId == userId)
                    .Include(w => w.Exercises);
            if (startDate.HasValue)
                query = query.Where(w => w.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(w => w.Date <= endDate.Value);
            if (!string.IsNullOrEmpty(exerciseName))
                query = query.Where(w => w.Exercises.Any(e => e.Name.Contains(exerciseName)));

            var workouts = await query.ToListAsync();
            return Ok(workouts);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var workouts = await _context.Workouts
                .Where(w => w.UserId == userId)
                .Include(w => w.Exercises)
                .ToListAsync();

            var stats = new
            {
                MaxWeight = workouts.SelectMany(w => w.Exercises).Max(e => e.Weight),
                WorkoutsPerWeek = workouts
                    .GroupBy(w => w.Date.Date.AddDays(-(int)w.Date.DayOfWeek))
                    .Count(),
                TotalWorkouts = workouts.Count
            };

            return Ok(stats);
        }

        [HttpGet("progress")]
        public async Task<IActionResult> GetProgressData([FromQuery] string exerciseName)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var exercises = await _context.Exercises
                .Include(e => e.Workout)
                .Where(e => e.Workout.UserId == userId && e.Name == exerciseName)
                .OrderBy(e => e.Workout.Date)
                .Select(e => new { e.Workout.Date, e.Weight })
                .ToListAsync();

            return Ok(exercises);
        }
    }
}