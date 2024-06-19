using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CourseServer.Context
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class Role
    {
        public int Id { get; set; }
        public string Rolename { get; set; }
        public string Description { get; set; }
    }
    public class UserRole
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }
    public class Student
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Name { get; set; }
    }
    public class Instructor
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Name { get; set; }
    }
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
    }
}
