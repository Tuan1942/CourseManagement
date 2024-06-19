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

namespace LearningCourse.Pages.Instructor
{
    /// <summary>
    /// Interaction logic for CourseAdd.xaml
    /// </summary>
    public partial class CourseAdd : Page
    {
        private readonly int _instructorId;
        private Course parentCoursePage;

        public CourseAdd(int instructorId, Course parentCoursePage)
        {
            InitializeComponent();
            _instructorId = instructorId;
            this.parentCoursePage = parentCoursePage;
        }

        private async void AddCourseButton_Click(object sender, RoutedEventArgs e)
        {
            var course = new
            {
                name = NameTextBox.Text,
                description = DescriptionTextBox.Text,
                instructorId = _instructorId
            };

            var json = JsonConvert.SerializeObject(course);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Properties.Settings.Default.Token);

                var response = await client.PostAsync(Connection.URL + "Course/", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Khóa học đã được thêm thành công.");
                    parentCoursePage.Refresh();
                    NameTextBox.Text = "";
                    DescriptionTextBox.Text = "";
                }
                else
                {
                    MessageBox.Show("Thêm khóa học thất bại.");
                }
            }
        }
    }
}
