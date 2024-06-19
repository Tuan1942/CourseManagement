using LearningCourse.Pages.Identity;
using LearningCourse.Services;
using LearningCourse.ViewModels;
using LearningCourse.Windows;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using Instructor = LearningCourse.Pages.Instructor;
using Student = LearningCourse.Pages.Student;

namespace LearningCourse
{
    public partial class MainWindow : Window
    {
        private readonly UserService _userService;
        public MainViewModel ViewModel { get; set; }
        private bool isIdentityWindowOpen = false;

        public MainWindow()
        {
            InitializeComponent();
            _userService = new UserService();
            ViewModel = new MainViewModel();
            DataContext = ViewModel;
            Activated += MainWindow_Activated;
        }
        private void MenuToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            AccountOptionsMenu.Visibility = Visibility.Visible;
        }

        private void MenuToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            AccountOptionsMenu.Visibility = Visibility.Collapsed;
        }
        private void ManageAccountButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Account(ViewModel.CurrentUser));
        }

        private async Task InitializeAsync()
        {
            try
            {
                string token = Properties.Settings.Default.Token;

                if (token.StartsWith("\"") && token.EndsWith("\""))
                {
                    token = token.Substring(1, token.Length - 2);
                }

                ViewModel.CurrentUser = await _userService.GetCurrentUserInfoAsync(token);

                if (ViewModel.CurrentUser != null)
                {
                    LoginButton.Visibility = Visibility.Hidden;
                    MenuToggleButton.Visibility = Visibility.Visible;
                    ViewModel.IsLoggedIn = true;
                    switch (ViewModel.CurrentUser.Rolename)
                    {
                        case ("Instructor"):
                            MainFrame.Navigate(new Instructor.Course(ViewModel.CurrentUser.Id));
                            break;
                        case ("Student"):
                            MainFrame.Navigate(new Student.Course(ViewModel.CurrentUser.Id));
                            break;
                        default: break;
                    }
                }
                else
                {
                    LoginButton.Visibility = Visibility.Visible;
                    MenuToggleButton.Visibility = Visibility.Hidden;
                    MenuToggleButton.IsChecked = false;
                    AccountOptionsMenu.Visibility = Visibility.Collapsed;
                    ViewModel.IsLoggedIn = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tải dữ liệu người dùng: {ex.Message}");
                ViewModel.IsLoggedIn = false;
            }
        }

        private void OpenIdentityWindow()
        {
            if (isIdentityWindowOpen)
                return;

            IdentityWindow identityWindow = new IdentityWindow();
            identityWindow.Closed += IdentityWindow_Closed;
            identityWindow.Show();
            isIdentityWindowOpen = true;
            Hide();
        }

        private async void IdentityWindow_Closed(object sender, EventArgs e)
        {
            isIdentityWindowOpen = false;
            Show();
            await InitializeAsync();
        }

        private async void MainWindow_Activated(object sender, EventArgs e)
        {
            if (!isIdentityWindowOpen && !ViewModel.IsLoggedIn)
            {
                await InitializeAsync();
            }
        }

        private async void logOutButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Token = "";
            Properties.Settings.Default.Save();
            MainFrame.Content = null;
            ClearNavigationHistory();
            ViewModel.CurrentUser = null;
            await InitializeAsync();
        }
        private void ClearNavigationHistory()
        {
            var navigationService = MainFrame.NavigationService;
            while (navigationService.RemoveBackEntry() != null);
            MainFrame = new System.Windows.Controls.Frame();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            OpenIdentityWindow();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }
    }
}
