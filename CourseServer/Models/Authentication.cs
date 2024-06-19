using CourseServer.Context;

namespace CourseServer.Models
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
    public class AccountModel
    {
        public string Username { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public AccountModel(User user)
        {
            Username = user.Username;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }

        public AccountModel()
        {
            Username = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }
    }
}
