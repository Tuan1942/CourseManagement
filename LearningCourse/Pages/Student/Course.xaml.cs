using LearningCourse.Pages.Instructor;
using LearningCourse.Services;
using LearningCourse.ViewModels;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LearningCourse.Pages.Student
{
    /// <summary>
    /// Interaction logic for Course.xaml
    /// </summary>
    public partial class Course : Page
    {
        int userId { get; set; }
        public Course(int userId)
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
                    var response = await client.GetAsync(Connection.URL + "Course/");

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
        private async void CoursesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CoursesListView.SelectedItem is CourseModel selectedCourse)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(Connection.URL + "Course/schedule/" + selectedCourse.Id);

                        if (response.IsSuccessStatusCode)
                        {
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            var schedules = JsonConvert.DeserializeObject<List<ScheduleModel>>(jsonResponse);
                            selectedCourse.Schedules = schedules;
                            CoursesListView.Items.Refresh();
                        }
                        else
                        {
                            MessageBox.Show($"Failed to load schedules. Status code: {response.StatusCode}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load schedules: {ex.Message}");
                }
            }
        }

        private async void CourseRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (CoursesListView.SelectedItem is CourseModel selectedCourse)
            {
                try
                {
                    var enrollment = new EnrollmentModel
                    {
                        CourseId = selectedCourse.Id,
                        StudentId = userId 
                    };

                    string json = JsonConvert.SerializeObject(enrollment);
                    using (var client = new HttpClient())
                    {
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(Connection.URL + "course/register", content);

                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Đăng ký khóa học thành công!");
                        }
                        else
                        {
                            throw new Exception(await response.Content.ReadAsStringAsync());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please select a course to register.");
            }
        }

        private void CourseManageRedirect_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CourseManage(userId));
        }
    }
}
