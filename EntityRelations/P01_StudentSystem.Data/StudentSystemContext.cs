﻿using Microsoft.EntityFrameworkCore;
using P01_StudentSystem.Data.Configurations;
using P01_StudentSystem.Data.Models;

namespace P01_StudentSystem.Data
{
    public class StudentSystemContext : DbContext
    {
        public StudentSystemContext()
        {

        }

        public StudentSystemContext(DbContextOptions options) 
            : base(options)
        {

        }

        public DbSet<Student> Students { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<Resource> Resources { get; set; }

        public DbSet<Homework> HomeworkSubmissions { get; set; }

        public DbSet<StudentCourse> StudentCourses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Configuration.ConnectionString);
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(s => s.StudentId);

                entity
                    .Property(s => s.Name)
                    .HasMaxLength(100)
                    .IsRequired(true)
                    .IsUnicode(true);

                entity
                    .Property(s => s.PhoneNumber)
                    .HasMaxLength(10)
                    .IsFixedLength(true)
                    .IsRequired(false)
                    .IsUnicode(false);

                entity
                    .Property(s => s.RegisteredOn)
                    .HasDefaultValueSql("GETDATE()")
                    .IsRequired(true);

                entity
                    .Property(s => s.Birthday)
                    .IsRequired(false);
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(c => c.CourseId);

                entity
                    .Property(c => c.Name)
                    .HasMaxLength(80)
                    .IsRequired(true)
                    .IsUnicode(true);

                entity
                    .Property(c => c.Description)
                    .IsRequired(false)
                     .IsUnicode(true);

                entity
                    .Property(c => c.StartDate)
                    .IsRequired(true);

                entity
                    .Property(c => c.EndDate)
                    .IsRequired(true);

                entity
                    .Property(c => c.Price)
                    .IsRequired(true);
            });

            modelBuilder.Entity<Resource>(entity =>
            {
                entity.HasKey(r => r.ResourceId);

                entity
                    .Property(r => r.Name)
                    .HasMaxLength(50)
                    .IsRequired(true)
                    .IsUnicode(true);

                entity
                    .Property(r => r.Url)
                    .IsRequired(false)
                    .IsUnicode(false);

                //TODO
                entity
                    .Property(r => r.ResourceType)
                    .IsRequired(true);

                entity
                    .HasOne(r => r.Course)
                    .WithMany(c => c.Resources)
                    .HasForeignKey(r => r.CourseId);
            });

            modelBuilder.Entity<Homework>(entity =>
            {
                entity.HasKey(h => h.HomeworkId);

                entity
                    .Property(h => h.Content)
                    .IsRequired(true)
                    .IsUnicode(false);

                //TODO
                entity
                    .Property(h => h.ContentType)
                    .IsRequired(true);

                entity
                    .Property(h => h.SubmissionTime)
                    .IsRequired(true);

                entity
                    .HasOne(h => h.Student)
                    .WithMany(s => s.HomeworkSubmissions)
                    .HasForeignKey(h => h.StudentId);

                entity
                   .HasOne(h => h.Course)
                   .WithMany(c => c.HomeworkSubmissions)
                   .HasForeignKey(h => h.CourseId);
            });

            modelBuilder.Entity<StudentCourse>(entity =>
            {
                entity.HasKey(sc => new { sc.StudentId, sc.CourseId });

                entity
                    .HasOne(sc => sc.Student)
                    .WithMany(s => s.CourseEnrollments)
                    .HasForeignKey(sc => sc.StudentId);

                entity
                    .HasOne(sc => sc.Course)
                    .WithMany(c => c.StudentsEnrolled)
                    .HasForeignKey(sc => sc.CourseId);
            });
        }
    }
}
