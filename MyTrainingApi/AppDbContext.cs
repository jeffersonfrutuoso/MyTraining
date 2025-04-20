using Microsoft.EntityFrameworkCore;
using MyTrainingApi.Models;

namespace MyTrainingApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Workout> Workouts { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<Routine> Routines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Workout>()
                .HasOne(w => w.User)
                .WithMany(u => u.Workouts)
                .HasForeignKey(w => w.UserId);

            modelBuilder.Entity<Exercise>()
                .HasOne(e => e.Workout)
                .WithMany(w => w.Exercises)
                .HasForeignKey(e => e.WorkoutId);

            modelBuilder.Entity<Exercise>()
                .HasOne(e => e.Routine)
                .WithMany(r => r.Exercises)
                .HasForeignKey(e => e.RoutineId);

            modelBuilder.Entity<Routine>()
                .HasOne(r => r.User)
                .WithMany(r => r.Routines)
                .HasForeignKey(r => r.UserId);
        }
    }
}