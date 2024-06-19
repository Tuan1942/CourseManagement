using LearningCourse.Services;
using LearningCourse.ViewModels;
using Newtonsoft.Json;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace LearningCourse.Pages.Student
{
    /// <summary>
    /// Interaction logic for CourseManage.xaml
    /// </summary>
    public partial class CourseManage : Page
    {
        int userId;
        public CourseManage(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadCourseDataAsync();
        }
        private async void LoadCourseDataAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(Connection.URL + $"Course/studentCourse/{userId}");

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
    }
}
