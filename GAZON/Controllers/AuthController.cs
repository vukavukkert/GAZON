using GAZON.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GAZON.Database.Models;
using GAZON.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace MarketPlace.Controllers
{
    public class AuthController : Controller
    {
        private readonly GazonContext _context;

        public AuthController(GazonContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string login, string password)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Login == login);

                if (user == null)
                {
                    return BadRequest(new { success = false, description = "Пользователь не найден" });
                }

                if (!HashService.VerifyHashedPassword(user.Password, password))
                {
                    return BadRequest(new { success = false, description = "Неправильно введены логин или пароль" });
                }
            
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, user.RoleNavigation.Name)
                };
                
                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);

                return Ok(new { success = true });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(new { success = false, description = e.ToString()});
            }
        }
        public async Task<IActionResult> Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            try
            {
                if (_context.Users.Select(u => u.Login).Contains(user.Login))
                    return BadRequest(new
                    {
                        success = false,
                        description = "This login is taken"
                    });
                if (user.Login == null || user.Password == null)
                    return BadRequest(new
                    {
                        success = false,
                        description = "Enter the login or password"
                    });
                var newUser = new User
                {
                    Email = user.Email,
                    Login = user.Login,
                    Password = HashService.HashPassword(user.Password),
                    Name = user.Name,
                    Role = 2
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Ok(new { success = false, description = e.ToString() });
            }
            return Ok(new { success = true, description = user.Login });
        }
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
    }
}
