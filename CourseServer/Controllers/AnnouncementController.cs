using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CourseServer.Context;

namespace CourseServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AnnouncementsController : ControllerBase
    {
        private readonly CourseContext _context;

        public AnnouncementsController(CourseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnnouncements()
        {
            try
            {
                var announcements = await _context.Announcements.ToListAsync();
                return Ok(announcements);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnnouncement(int id)
        {
            try
            {
                var announcement = await _context.Announcements.FindAsync(id);
                if (announcement == null)
                {
                    return NotFound();
                }
                return Ok(announcement);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostAnnouncement([FromBody] Announcement announcement)
        {
            if (announcement == null)
            {
                return BadRequest();
            }

            try
            {
                _context.Announcements.Add(announcement);
                await _context.SaveChangesAsync();
                return CreatedAtAction("Created Announcement", new { id = announcement.Id }, announcement);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnnouncement(int id, [FromBody] Announcement announcement)
        {
            if (announcement == null || id != announcement.Id)
            {
                return BadRequest();
            }

            var a = await _context.Announcements.FindAsync(id);
            if (a == null)
            {
                return NotFound();
            }
            a = announcement;

            try
            {
                _context.Announcements.Update(a);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            try
            {
                var announcement = await _context.Announcements.FindAsync(id);
                if (announcement == null)
                {
                    return NotFound();
                }

                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
