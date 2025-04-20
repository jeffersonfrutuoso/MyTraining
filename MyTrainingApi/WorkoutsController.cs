using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTrainingApi.Models;
using MyTrainingApi.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MyTrainingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkoutsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WorkoutsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkout([FromBody] Workout workout)
        {
            if (string.IsNullOrEmpty(workout.Name))
                return BadRequest("Workout name cannot be empty.");

            workout.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            workout.Date = DateTime.Now;
            _context.Workouts.Add(workout);
            await _context.SaveChangesAsync();
            return Ok(workout);
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkouts()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var workouts = await _context.Workouts
                .Where(w => w.UserId == userId)
                .Include(w => w.Exercises)
                .ToListAsync();
            return Ok(workouts);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkout(int id, [FromBody] Workout workout)
        {
            var existing = await _context.Workouts.FindAsync(id);
            if (existing == null || existing.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return NotFound();

            existing.Name = workout.Name;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkout(int id)
        {
            var workout = await _context.Workouts.FindAsync(id);
            if (workout == null || workout.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return NotFound();

            _context.Workouts.Remove(workout);
            await _context.SaveChangesAsync();
            return Ok("Workout deleted.");
        }
    }
}