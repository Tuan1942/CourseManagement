using Microsoft.EntityFrameworkCore;

namespace CourseServer.Context
{
    public class CourseContext : DbContext
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public CourseContext(DbContextOptions<CourseContext> options) : base(options) { }
    }
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int InstructorId {  get; set; }
    }
    public class Schedule
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public DateOnly StartTime { get; set; }
        public DateOnly EndTime { get; set; }
        public int DayOfWeek { get; set; }
        public string Location { get; set; }

    }
    public class Enrollment
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int StudentId { get; set; }
    }
    public class Announcement
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public int CourseId { get; set; }
    }
}
