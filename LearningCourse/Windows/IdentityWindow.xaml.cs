using LearningCourse.Pages.Identity;
using LearningCourse.Services;
using LearningCourse.ViewModels;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Windows;

namespace LearningCourse.Windows
{
    /// <summary>
    /// Interaction logic for IdentityWindow.xaml
    /// </summary>
    public partial class IdentityWindow : Window
    {
        public IdentityWindow()
        {
            InitializeComponent();
            IdentityFrame.Navigate(new Login());
        }

        private async Task<string> LoginAsync(UserModel userLogin)
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (IdentityFrame.CanGoBack)
            {
                IdentityFrame.GoBack();
            }
        }
    }
}
