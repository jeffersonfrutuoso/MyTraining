using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyTrainingApi.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required] public string Username { get; set; } = null!;
        [Required] public string PasswordHash { get; set; } = null!;
        public List<Workout> Workouts { get; set; } = new List<Workout>();
        public List<Routine> Routines { get; set; } = new List<Routine>();
    }

    public class Workout
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = null!;
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
    }

    public class Exercise
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = null!;
        public int Sets { get; set; }
        public int Reps { get; set; }
        public double Weight { get; set; }
        public int WorkoutId { get; set; }
        public Workout? Workout { get; set; }
        public int? RoutineId { get; set; }
        public Routine? Routine { get; set; }
    }

    public class Routine
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = null!;
        public int UserId { get; set; }
        public User? User { get; set; }
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}