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
    public class RoutinesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoutinesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoutine([FromBody] Routine routine)
        {
            if (string.IsNullOrEmpty(routine.Name))
                return BadRequest("Routine name cannot be empty.");

            routine.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            _context.Routines.Add(routine);
            await _context.SaveChangesAsync();
            return Ok(routine);
        }

        [HttpGet]
        public async Task<IActionResult> GetRoutines()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var routines = await _context.Routines
                .Where(r => r.UserId == userId)
                .Include(r => r.Exercises)
                .ToListAsync();
            return Ok(routines);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoutine(int id, [FromBody] Routine routine)
        {
            var existing = await _context.Routines.FindAsync(id);
            if (existing == null || existing.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value))
                return NotFound();

            existing.Name = routine.Name;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoutine(int id)
        {
            var routine = await _context.Routines.FindAsync(id);
            if (routine == null || routine.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value))
                return NotFound();

            _context.Routines.Remove(routine);
            await _context.SaveChangesAsync();
            return Ok("Routine deleted.");
        }

        [HttpPost("{id}/start-workout")]
        public async Task<IActionResult> StartWorkoutFromRoutine(int id)
        {
            var routine = await _context.Routines
                .Include(r => r.Exercises)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (routine == null || routine.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value))
                return NotFound();

            var workout = new Workout
            {
                Name = $"Workout from {routine.Name}",
                Date = DateTime.Now,
                UserId = routine.UserId,
                Exercises = routine.Exercises.Select(e => new Exercise
                {
                    Name = e.Name,
                    Sets = e.Sets,
                    Reps = e.Reps,
                    Weight = e.Weight
                }).ToList()
            };

            _context.Workouts.Add(workout);
            await _context.SaveChangesAsync();
            return Ok(workout);
        }
    }
}