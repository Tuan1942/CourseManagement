using LearningCourse.Services;
using LearningCourse.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LearningCourse.Pages.Identity
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Page
    {
        public Register()
        {
            InitializeComponent();
        }
        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPassword.Password;

            if (password != confirmPassword)
            {
                MessageBox.Show("Mật khẩu và xác nhận mật khẩu không khớp.");
                return;
            }
            UserModel user = new UserModel()
            {
                Username = username,
                Password = password
            };
            if (IsInstructor.IsChecked == true)
            {
                MessageBox.Show(await InstructorRegisterAsync(user));
            }
            else
            {
                MessageBox.Show(await StudentRegisterAsync(user));
            }
        }
        private async Task<string> StudentRegisterAsync(UserModel user)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(Connection.URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string json = JsonConvert.SerializeObject(user);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("user/studentRegister", content);
                string message = (await response.Content.ReadAsStringAsync()).Trim('"');
                return message;
            }
        }
        private async Task<string> InstructorRegisterAsync(UserModel user)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(Connection.URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string json = JsonConvert.SerializeObject(user);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("user/instructorRegister", content);
                string message = (await response.Content.ReadAsStringAsync()).Trim('"');
                return message;
            }
        }
    }
}