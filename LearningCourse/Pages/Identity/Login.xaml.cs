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
using LearningCourse.Windows;

namespace LearningCourse.Pages.Identity
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        private readonly HttpClient _httpClient;
        public Login()
        {
            InitializeComponent();
            _httpClient = App.HttpClient;
        }
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            var userLogin = new UserModel
            {
                Username = username,
                Password = password
            };

            var token = await LoginAsync(userLogin);

            if (!string.IsNullOrEmpty(token))
            {
                Properties.Settings.Default.Token = token;
                Properties.Settings.Default.Save();
                MessageBox.Show("Đăng nhập thành công!");
                IdentityWindow identityWindow = Application.Current.Windows.OfType<IdentityWindow>().FirstOrDefault();
                if (identityWindow != null)
                {
                    identityWindow.Close();
                }
            }
            else
            {
                MessageBox.Show("Đăng nhập thất bại.");
            }
        }

        private async Task<string> LoginAsync(UserModel userLogin)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Connection.URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string json = JsonConvert.SerializeObject(userLogin);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("user/login", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        return responseData;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Không thể kết nối tới máy chủ.");
                return null;
            }
        }

        private void Register_Nav(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            if (navService != null)
            {
                navService.Navigate(new Register());
            }
        }
    }
}
