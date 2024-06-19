using LearningCourse.Services;
using LearningCourse.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace LearningCourse.Pages.Instructor
{
    public partial class Course : Page
    {
        private int instructorId;

        public Course()
        {
            InitializeComponent();
            instructorId = 0; 
        }

        public Course(int instructorId)
        {
            InitializeComponent();
            this.instructorId = instructorId;
            LoadCourseDataAsync(instructorId);
        }

        private async void LoadCourseDataAsync(int instructorId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(Connection.URL + "Course/" + instructorId);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();

                        if (!jsonResponse.TrimStart().StartsWith("[") && !jsonResponse.TrimStart().StartsWith("{"))
                        {
                            throw new Exception(jsonResponse);
                        }

                        var courses = JsonConvert.DeserializeObject<List<CourseModel>>(jsonResponse);

                        if (courses == null || courses.Count == 0)
                        {
                            throw new Exception("Không tìm thấy khóa học.");
                        }

                        CoursesListView.ItemsSource = courses;
                    }
                    else
                    {
                        throw new Exception("Không thể kết nối tới máy chủ.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load courses: {ex.Message}");
            }
        }

        private void CoursesListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CoursesListView.SelectedItem is CourseModel selectedCourse)
            {
                NavigationService.Navigate(new CourseDetail(selectedCourse.Id, this));
            }
        }
        public void Refresh()
        {
            LoadCourseDataAsync(instructorId);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CourseAdd(instructorId, this));
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CoursesListView.SelectedItem is CourseModel selectedCourse)
            {
                var confirmation = MessageBox.Show($"Bạn có chắc chắn muốn xóa khóa học '{selectedCourse.Name}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirmation == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            var response = await client.DeleteAsync(Connection.URL + "Course/" + selectedCourse.Id);

                            if (response.IsSuccessStatusCode)
                            {
                                MessageBox.Show($"Đã xóa khóa học '{selectedCourse.Name}'.");
                                LoadCourseDataAsync(instructorId); // Refresh the course list
                            }
                            else
                            {
                                MessageBox.Show($"Failed to delete course '{selectedCourse.Name}'. Status code: {response.StatusCode}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete course '{selectedCourse.Name}': {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn khóa học để xóa.");
            }
        }
    }
}
