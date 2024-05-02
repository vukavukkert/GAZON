using GAZON.Database;
using GAZON.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAZON.Controllers
{
    public class PointsController : Controller
    {
        private readonly GazonContext _context;
        public PointsController(GazonContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(List<PickupPoint>? points = null)
        {
            points ??= await _context.PickupPoints.ToListAsync();
            return View(points);
        }
        public async Task<IActionResult> Employees(int id)
        {
            var staff = await _context.Staff.Where(s => s.PickupPoint == id)
                .ToListAsync();
            ViewBag.Point = await _context.PickupPoints.FirstOrDefaultAsync(p => p.Id == id);
            return View(staff);
        }

        public async Task<IActionResult> Search(string key)
        {
            if (string.IsNullOrEmpty(key)) return RedirectToAction("Index");
            var points = await _context.PickupPoints.Where(p => p.Address.Contains(key)).ToListAsync();
            return View(nameof(Index), points);
        }
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> UserPoint(string name)
        {
            var person = await _context.Staff.FirstOrDefaultAsync(s => s.UserNavigation.Login == name);
            if (person == null) return RedirectToAction("Index");
            var point = person!.PickupPointNavigation;
            var staff = await _context.Staff.Where(s => s.PickupPoint == point.Id)
                .ToListAsync();
            ViewBag.Point = point;
            return View(nameof(Employees), staff);

        }
    }
}
