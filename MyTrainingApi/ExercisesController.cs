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
    public class ExercisesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExercisesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateExercise([FromBody] Exercise exercise)
        {
            if (string.IsNullOrEmpty(exercise.Name))
                return BadRequest("Exercise name cannot be empty.");
            if (exercise.Sets < 0 || exercise.Reps < 0 || exercise.Weight < 0)
                return BadRequest("Sets, reps, and weight cannot be negative.");

            var workout = await _context.Workouts.FindAsync(exercise.WorkoutId);
            if (workout == null || workout.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value))
                return NotFound("Workout not found.");

            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            return Ok(exercise);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExercise(int id, [FromBody] Exercise exercise)
        {
             if (exercise == null)
                return BadRequest("Invalid exercise data.");
                
                var existing = await _context.Exercises
                .Include(e => e.Workout) // Load related Workout
                .FirstOrDefaultAsync(e => e.Id == id);

            if (existing == null || existing.Workout.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value))
                return NotFound();

            if (string.IsNullOrEmpty(exercise.Name))
                return BadRequest("Exercise name cannot be empty.");
            if (exercise.Sets < 0 || exercise.Reps < 0 || exercise.Weight < 0)
                return BadRequest("Sets, reps, and weight cannot be negative.");

            existing.Name = exercise.Name;
            existing.Sets = exercise.Sets;
            existing.Reps = exercise.Reps;
            existing.Weight = exercise.Weight;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExercise(int id)
        {
             var exercise = await _context.Exercises
                .Include(e => e.Workout)
                .FirstOrDefaultAsync(e => e.Id == id);
            
            if (exercise == null)
                return NotFound("Exercise not found.");

            // Explicitly check if Workout exists
            var workout = await _context.Workouts.FindAsync(exercise.WorkoutId);
            if (exercise == null || exercise.Workout.UserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value))
                return NotFound();

            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync();
            return Ok("Exercise deleted.");
        }
    }
}