using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CourseServer.Context;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CourseServer.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CourseServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IConfiguration _configuration;

        public UserController(UserContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("studentRegister")]
        public async Task<IActionResult> Register([FromBody] UserModel newUser)
        {
            if (newUser.Username.Length <= 0) { return BadRequest("Tên người dùng không được để trống."); }
            if (newUser.Password.Length <= 0) { return BadRequest("Mật khẩu không được để trống."); }
            if (await _context.Users.AnyAsync(u => u.Username == newUser.Username))
            {
                return BadRequest("Tên người dùng đã tồn tại.");
            }

            // Đăng ký người dùng mới
            var user = new User
            {
                Username = newUser.Username,
                PasswordHash = HashPassword(newUser.Password),
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(); 

            // Người dùng mới được gán là sinh viên
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = 1
            };

            var student = new Student
            {
                UserId = user.Id,
            };

            _context.UserRoles.Add(userRole);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return Ok("Đăng ký thành công.");
        }

        [HttpPost("instructorRegister")]
        public async Task<IActionResult> InstructorRegister([FromBody] UserModel newUser)
        {
            if (newUser.Username.Length <= 0) { return BadRequest("Tên người dùng không được để trống."); }
            if (newUser.Password.Length <= 0) { return BadRequest("Mật khẩu không được để trống."); }
            if (await _context.Users.AnyAsync(u => u.Username == newUser.Username))
            {
                return BadRequest("Tên người dùng đã tồn tại.");
            }

            // Đăng ký người dùng mới
            var user = new User
            {
                Username = newUser.Username,
                PasswordHash = HashPassword(newUser.Password),
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Người dùng mới được gán là gỉảng viên
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = 2
            };

            var student = new Student
            {
                UserId = user.Id,
            };

            _context.UserRoles.Add(userRole);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return Ok("Đăng ký thành công.");
        }
        
        //public async Task<IActionResult> Register([FromForm] RegisterModel registerModel)
        //{
        //    if (await _context.Users.AnyAsync(u => u.Username == registerModel.Username))
        //    {
        //        return BadRequest("Tên người dùng đã tồn tại.");
        //    }

        //    if (registerModel.Password != registerModel.ConfirmPassword)
        //    {
        //        return BadRequest("Mật khẩu xác nhận không đúng!");
        //    }

        //    // Đăng ký người dùng mới
        //    var user = new User
        //    {
        //        Username = registerModel.Username,
        //        PasswordHash = HashPassword(registerModel.Password),
        //        CreatedAt = DateTime.Now
        //    };

        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();

        //    // Người dùng mới được gán là sinh viên
        //    var userRole = new UserRole
        //    {
        //        UserId = user.Id,
        //        RoleId = 1
        //    };


        //    var student = new Student
        //    {
        //        UserId = user.Id,
        //    };

        //    _context.UserRoles.Add(userRole);
        //    _context.Students.Add(student);
        //    await _context.SaveChangesAsync();

        //    // Đăng nhập lại
        //    var token = GenerateJwtToken(user, _configuration);

        //    return Ok(token);
        //}

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserModel userLogin)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userLogin.Username);
            if (user == null || !VerifyPassword(userLogin.Password, user.PasswordHash))
            {
                return Unauthorized("Thông tin không hợp lệ.");
            }
            try
            {
                var userRole = await _context.UserRoles.SingleOrDefaultAsync(ur => ur.UserId == user.Id);
                var role = await _context.Roles.SingleOrDefaultAsync(r => r.Id == userRole.RoleId);

                JwtTokenRequest jwtTokenRequest = new JwtTokenRequest
                {
                    User = user,
                    Role = role
                };
                var token = GenerateJwtToken(jwtTokenRequest, _configuration);
                return Ok(token);
            }
            catch (Exception)
            {
                return Problem("Người dùng chưa được cấp quyền.");
            }
        }
        //public async Task<IActionResult> Login([FromForm] LoginModel loginModel)
        //{
        //    var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginModel.Username);
        //    if (dbUser == null || !VerifyPassword(loginModel.Password, dbUser.PasswordHash))
        //    {
        //        return Unauthorized("Thông tin không hợp lệ.");
        //    }

        //    AddCookie(dbUser, _configuration);
        //    return RedirectToAction("Index", "Home");
        //}

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            if (jwtToken == null)
            {
                return BadRequest("Token không hợp lệ.");
            }
            /*
            var blacklistToken = new TokenBlacklist
            {
                Token = token,
                Expiration = jwtToken.ValidTo
            };

            _context.TokenBlacklists.Add(blacklistToken);
            await _context.SaveChangesAsync();
            */
            HttpContext.Response.Cookies.Delete("jwtToken");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("current")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return NotFound("Không tìm thấy tài khoản!");
                }

                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest("Người dùng không hợp lệ.");
                }

                var user = await _context.Users.FindAsync(userId);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var role = await _context.Roles.SingleOrDefaultAsync(r => r.Id == int.Parse(userRole));

                string name = "";
                switch (role.Rolename)
                {
                    case "Student":
                        var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
                        if (student != null)
                        {
                            name = student.Name;
                        }
                        break;
                    case "Instructor":
                        var teacher = await _context.Instructors.FirstOrDefaultAsync(t => t.UserId == user.Id);
                        if (teacher != null)
                        {
                            name = teacher.Name;
                        }
                        break;
                    default:
                        break;
                }

                if (user == null)
                {
                    return NotFound("Không tìm thấy tài khoản!");
                }
                var userView = new UserView
                {
                    Id = userId,
                    Username = user.Username,
                    Name = name,
                    Rolename = role.Rolename,
                    CreatedAt = user.CreatedAt
                };
                return Ok(userView);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("list")]
        [Authorize]
        public async Task<IActionResult> GetListUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userList = await _context.Users
                .Where(u => u.Id != int.Parse(userIdClaim))
                .Select(u => new {
                    u.Id,
                    u.CreatedAt
                })
                .ToListAsync();
            return Ok(userList);
        }

        [HttpPut("updatePassword")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword([FromBody] User u)
        {
            var user = await _context.Users.FirstOrDefaultAsync(us => us.Username == u.Username);
            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == u.Id);
            if (userRole == null)
            {
                return BadRequest("Người dùng chưa được cấp quyền.");
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == userRole.RoleId);
            if (role == null)
            {
                return BadRequest("Không tìm thấy vai trò của người dùng.");
            }

            switch (role.Rolename)
            {
                case "Student":
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
                    if (student != null)
                    {
                        _context.Students.Update(student);
                    }
                    break;
                case "Instructor":
                    var teacher = await _context.Instructors.FirstOrDefaultAsync(t => t.UserId == user.Id);
                    if (teacher != null)
                    {
                        _context.Instructors.Update(teacher);
                    }
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(u.PasswordHash))
            {
                user.PasswordHash = HashPassword(u.PasswordHash);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("updateInfo")]
        [Authorize]
        public async Task<IActionResult> UpdateInfo([FromBody] UserView u)
        {
            var user = await _context.Users.FirstOrDefaultAsync(us => us.Username == u.Username);
            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == u.Id);
            if (u.Rolename == null)
            {
                return BadRequest("Người dùng chưa được cấp quyền.");
            }

            var role = u.Rolename;
            if (role == null)
            {
                return BadRequest("Không tìm thấy vai trò của người dùng.");
            }

            switch (role)
            {
                case "Student":
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
                    if (student != null)
                    {
                        student.Name = u.Name;
                        _context.Students.Update(student);
                    }
                    else return NotFound("Không tìm thấy sinh viên.");
                    break;
                case "Instructor":
                    var teacher = await _context.Instructors.FirstOrDefaultAsync(t => t.UserId == user.Id);
                    if (teacher != null)
                    {
                        teacher.Name = u.Name;
                        _context.Instructors.Update(teacher);
                    }
                    else return NotFound("Không tìm thấy giảng viên.");
                    break;
                default:
                    break;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("update")]
        [Authorize]
        public async Task<IActionResult> UpdateForm([FromForm] AccountModel registerModel)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == registerModel.Username);
            if (user == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            if (registerModel.Password != registerModel.ConfirmPassword)
            {
                return BadRequest("Mật khẩu xác nhận không đúng!");
            }


            if (!string.IsNullOrEmpty(registerModel.Password))
            {
                user.PasswordHash = HashPassword(registerModel.Password);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok("Cập nhật thông tin người dùng thành công.");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            var enteredHash = HashPassword(enteredPassword);
            return enteredHash == storedHash;
        }

        public string GenerateJwtToken(JwtTokenRequest jwtTokenRequest, IConfiguration configuration)
        {
            var user = jwtTokenRequest.User;
            var role = jwtTokenRequest.Role;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]{
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Định danh thông qua UserID
                new Claim(ClaimTypes.Role, role.Id.ToString()) 
            };

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public class JwtTokenRequest
        {
            public User User { get; set; }
            public Role Role { get; set; }
        }

    }
}
