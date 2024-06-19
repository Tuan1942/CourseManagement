using LearningCourse.Services;
using System;
using System.ComponentModel;
using System.Windows;

namespace LearningCourse.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private bool _isLoggedIn = false;

        private UserView _currentUser;
        public UserView CurrentUser
        {
            get { return _currentUser; }
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
            }
        }
        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                _isLoggedIn = value;
                OnPropertyChanged(nameof(IsLoggedIn));
            }
        }

        public MainViewModel()
        {
            _userService = new UserService();
            // Initialize(); // Remove the initialization from constructor
        }

        public async Task InitializeAsync()
        {
            try
            {
                string token = Properties.Settings.Default.Token;
                if (token.StartsWith("\"") && token.EndsWith("\""))
                {
                    token = token.Substring(1, token.Length - 2);
                }
                CurrentUser = await _userService.GetCurrentUserInfoAsync(token);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load user info: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
