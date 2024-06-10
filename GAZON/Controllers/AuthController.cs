using GAZON.Database;
using GAZON.Database.Models;
using GAZON.Models.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace GAZON.Controllers
{
	public class AuthController : Controller
	{
		private readonly GazonContext _context;
		private readonly IHashService _hashService;

		public AuthController(GazonContext context, IHashService hashService)
		{
			_context = context;
			_hashService = hashService;
		}

		public async Task<IActionResult> Index()
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

				if (!_hashService.VerifyHashedPassword(user.Password, password))
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
				return BadRequest(new { success = false, description = e.Message });
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
					Password = _hashService.HashPassword(user.Password),
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
