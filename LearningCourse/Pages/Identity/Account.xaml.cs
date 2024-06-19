using LearningCourse.Services;
using LearningCourse.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for Account.xaml
    /// </summary>
    public partial class Account : Page
    {
        UserView userView;
        public Account(UserView userView)
        {
            InitializeComponent();
            this.userView = userView;
            UsernameTextBox.Text = userView.Username;
            NameTextBox.Text = userView.Name;
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var user = new UserView
                {
                    Id = userView.Id,
                    Username = UsernameTextBox.Text.Trim(),
                    Rolename = userView.Rolename,
                    Name = NameTextBox.Text.Trim(),
                };

                string json = JsonConvert.SerializeObject(user);

                using (var client = new HttpClient())
                {
                    string token = Properties.Settings.Default.Token;

                    if (token.StartsWith("\"") && token.EndsWith("\""))
                    {
                        token = token.Substring(1, token.Length - 2);
                    }
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);


                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PutAsync(Connection.URL + "user/updateInfo", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("User information updated successfully!");
                    }
                    else
                    {
                        MessageBox.Show($"Failed to update user information. Status code: {response.Content.ReadAsStringAsync()}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update user information: {ex.Message}");
            }
        }
    }
}
