using CourseServer.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CourseServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly CourseContext _context;
        private readonly UserContext _userContext;
        public CourseController(CourseContext context, UserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await _context.Courses.ToListAsync());
            }
            catch (Exception)
            {
                return NoContent();
            }
        }

        [HttpGet("{instructorId}")]
        public async Task<IActionResult> Get(int instructorId)
        {
            try
            {
                var courses = await _context.Courses
                                            .Where(c => c.InstructorId == instructorId)
                                            .ToListAsync();

                if (courses == null || !courses.Any())
                {
                    return Ok("Không tìm thấy khóa học của giảng viên.");
                }

                return Ok(courses);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("studentCourse/{studentId}")]
        public async Task<IActionResult> GetCourseByStudentId(int studentId)
        {
            try
            {
                var enrollments = await _context.Enrollments
                                                .Where(e => e.StudentId == studentId)
                                                .ToListAsync();

                if (enrollments == null || !enrollments.Any())
                {
                    return NotFound("Sinh viên chưa đăng ký khóa học nào.");
                }

                List<Course> courses = new List<Course>();

                foreach (var enrollment in enrollments)
                {
                    var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == enrollment.CourseId);
                    if (course != null)
                    {
                        courses.Add(course);
                    }
                }

                if (courses == null || !courses.Any())
                {
                    return Ok("Không tìm thấy khóa học của sinh viên.");
                }

                return Ok(courses);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);

                if (course == null)
                {
                    return NotFound("Course not found.");
                }

                return Ok(course);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("schedule/{id}")]
        public async Task<IActionResult> GetScheduleByCourseId(int id)
        {
            try
            {
                var schedules = await _context.Schedules.Where(s => s.CourseId == id).ToListAsync();

                if (schedules == null)
                {
                    return Ok("Schedules not found.");
                }

                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("schedule/")]
        public async Task<IActionResult> PostSchedule([FromBody] Schedule schedule)
        {
            try
            {
                if (schedule == null)
                {
                    return BadRequest("Dữ liệu lịch học không hợp lệ.");
                }

                var course = await _context.Courses.FindAsync(schedule.CourseId);
                if (course == null)
                {
                    return NotFound("Không tìm thấy khóa học.");
                }


                _context.Schedules.Add(schedule);
                await _context.SaveChangesAsync();

                return CreatedAtAction(course.Name, new { id = schedule.Id }, schedule);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> CourseRegister([FromBody] Enrollment enrollment)
        {
            try
            {
                if (enrollment == null)
                {
                    return BadRequest("Dữ liệu đăng ký học không hợp lệ.");
                }

                var student = await _userContext.Students.FindAsync(enrollment.StudentId);
                var course = await _context.Courses.FindAsync(enrollment.CourseId);

                if (student == null)
                {
                    return NotFound("Không tìm thấy sinh viên.");
                }

                if (course == null)
                {
                    return NotFound("Không tìm thấy khóa học.");
                }

                var existingEnrollment = await _context.Enrollments
                                                    .FirstOrDefaultAsync(e => e.StudentId == enrollment.StudentId && e.CourseId == enrollment.CourseId);

                if (existingEnrollment != null)
                {
                    return BadRequest("Sinh viên đã đăng ký khóa học này.");
                }

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(enrollment), new { id = enrollment.Id }, enrollment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostCourse([FromBody] Course course)
        {
            if (course == null)
            {
                return BadRequest();
            }

            try
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetCourse", new { id = course.Id }, course);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, [FromBody] Course course)
        {
            if (course == null || id != course.Id)
            {
                return BadRequest();
            }

            var existingCourse = await _context.Courses.FindAsync(id);
            if (existingCourse == null)
            {
                return NotFound();
            }

            existingCourse.Name = course.Name;
            existingCourse.Description = course.Description;

            try
            {
                _context.Courses.Update(existingCourse);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    return NotFound();
                }

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                return Ok(); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("schedule/{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            try
            {
                var schedule = await _context.Schedules.FindAsync(id);
                if (schedule == null)
                {
                    return NotFound();
                }

                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
