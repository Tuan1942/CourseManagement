namespace LearningCourse.ViewModels
{
    public class UserModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class UserView
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Rolename { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
    public class UserUpdateModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Rolename { get; set; }
    }
}
