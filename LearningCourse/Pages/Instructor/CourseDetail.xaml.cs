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
using LearningCourse.Services;
using LearningCourse.ViewModels;
using System.Globalization;

namespace LearningCourse.Pages.Instructor
{
    /// <summary>
    /// Interaction logic for CourseDetail.xaml
    /// </summary>
    public partial class CourseDetail : Page
    {
        private readonly int _courseId;
        private CourseModel _course;
        private Course parentCoursePage;
        private List<ScheduleModel> schedules;

        public CourseDetail(int courseId, Course parentCoursePage)
        {
            InitializeComponent();
            _courseId = courseId;
            this.parentCoursePage = parentCoursePage;
            LoadCourseDetails();
            LoadSchedule();
        }

        private async void LoadCourseDetails()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Properties.Settings.Default.Token);
                    var response = await client.GetAsync(Connection.URL + $"Course/details/{_courseId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var courseJson = await response.Content.ReadAsStringAsync();
                        var course = JsonConvert.DeserializeObject<CourseModel>(courseJson);
                        _course = course;
                        NameTextBox.Text = course.Name;
                        DescriptionTextBox.Text = course.Description;
                    }
                    else
                    {
                        MessageBox.Show("Không thể tải chi tiết khóa học.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra khi tải chi tiết khóa học: {ex.Message}");
            }
        }
        private async void LoadSchedule()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(Connection.URL + $"Course/schedule/{_courseId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        schedules = JsonConvert.DeserializeObject<List<ScheduleModel>>(jsonResponse);
                        ScheduleListView.ItemsSource = schedules;
                    }
                    else
                    {
                        MessageBox.Show($"Có lỗi xảy ra khi tải lịch học. Mã lỗi: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tải lịch học: {ex.Message}");
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleListView.SelectedItem is ScheduleModel selectedSchedule)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.DeleteAsync(Connection.URL + $"Course/schedule/{selectedSchedule.Id}");

                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show($"Deleted schedule with ID: {selectedSchedule.Id}");
                            schedules.Remove(selectedSchedule);
                            ScheduleListView.ItemsSource = null;
                            ScheduleListView.ItemsSource = schedules;
                        }
                        else
                        {
                            MessageBox.Show($"Failed to delete schedule. Status code: {response.StatusCode}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete schedule: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please select a schedule to delete.");
            }
        }
        private async void AddScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input (you may add more validation logic)
                if (string.IsNullOrWhiteSpace(StartTimeTextBox.Text) || string.IsNullOrWhiteSpace(EndTimeTextBox.Text) ||
                    string.IsNullOrWhiteSpace(DayOfWeekTextBox.Text) || string.IsNullOrWhiteSpace(LocationTextBox.Text))
                {
                    MessageBox.Show("Please fill in all fields.");
                    return;
                }

                string dateFormat = "M/d/yyyy";
                DateOnly startTime, endTime;
                if (!DateOnly.TryParseExact(StartTimeTextBox.Text.Trim(), dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                {
                    MessageBox.Show("Invalid Start Time format. Please enter a valid date.");
                    return;
                }

                if (!DateOnly.TryParseExact(EndTimeTextBox.Text.Trim(), dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out endTime))
                {
                    MessageBox.Show("Invalid End Time format. Please enter a valid date.");
                    return;
                }

                var newSchedule = new ScheduleModel
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    DayOfWeek = int.Parse(DayOfWeekTextBox.Text.Trim()),
                    Location = LocationTextBox.Text.Trim(),
                    CourseId = _course.Id,
                };

                var json = JsonConvert.SerializeObject(newSchedule);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(Connection.URL + "Course/schedule/", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Added new schedule successfully.");

                        StartTimeTextBox.Text = string.Empty;
                        EndTimeTextBox.Text = string.Empty;
                        DayOfWeekTextBox.Text = string.Empty;
                        LocationTextBox.Text = string.Empty;
                        LoadSchedule();
                    }
                    else
                    {
                        MessageBox.Show($"Failed to add schedule. Status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add schedule: {ex.Message}");
            }
        }

        private void ScheduleListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ScheduleListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ScheduleListView.SelectedItem is ScheduleModel selectedSchedule)
            {
                MessageBox.Show($"Double clicked on schedule: {selectedSchedule.Id}");
            }
        }


        private async void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            var updatedCourse = new
            {
                id = _courseId,
                name = NameTextBox.Text,
                description = DescriptionTextBox.Text,
                instructorId = 0
            };

            var json = JsonConvert.SerializeObject(updatedCourse);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Properties.Settings.Default.Token);
                    var response = await client.PutAsync(Connection.URL + $"Course/{_courseId}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Cập nhật khóa học thành công.");
                        if (NavigationService.CanGoBack)
                        {
                            parentCoursePage.Refresh();
                            NavigationService.GoBack();
                            if (NavigationService.Content is Course coursePage)
                            {
                                coursePage = new Course(_course.InstructorId);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật khóa học thất bại.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra khi cập nhật khóa học: {ex.Message}");
            }
        }
    }
}
