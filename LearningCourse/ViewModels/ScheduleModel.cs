using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningCourse.ViewModels
{
    internal class ScheduleModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public DateOnly StartTime { get; set; }
        public DateOnly EndTime { get; set; }
        public string Location { get; set; }

        private int dayOfWeek;
        public int DayOfWeek
        {
            get { return dayOfWeek; }
            set
            {
                dayOfWeek = value;
                UpdateWeekDay();
            }
        }

        private string weekDay;
        public string WeekDay
        {
            get { return weekDay; }
            private set { weekDay = value; }
        }

        public ScheduleModel()
        {
            StartTime = new DateOnly();
            EndTime = new DateOnly();
        }

        private void UpdateWeekDay()
        {
            weekDay = "Thứ " + DayOfWeek.ToString();
        }
    }
}
