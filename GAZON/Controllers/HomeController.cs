using System.Reflection;
using GAZON.Database.Models;
using GAZON.Database;
using GAZON.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GAZON.Controllers
{
    public class HomeController : Controller
    {
        private readonly GazonContext _context;

        public HomeController(GazonContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(List<Item>? itemList = null)
        {
            itemList ??= await _context.Items.ToListAsync();
            return View(itemList);
        }
        public async Task<IActionResult> Search(string key, string filter)
        {

            if (string.IsNullOrEmpty(key)) return RedirectToAction("Index");
            List<Item> itemList;
            if (filter == "item")
            {
                itemList = await _context.Items.Where(i => i.Name.Contains(key)).ToListAsync();
            }
            else
            {
                itemList = await _context.Items.Where(i => i.VendorNavigation.Name.Contains(key)).ToListAsync();
            }
            return View(nameof(Index), itemList);
        }
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            var vendors = await _context.Vendors.ToListAsync();
            ViewBag.Vendors = new SelectList(vendors, "Id", "Name");
            return View();
        }
        
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> Create(IFormFile? photo, Item item)
        {
            try
            {
                if (photo != null)
                {
                    item.Picture = await PhotoSaver.SavePhoto(photo, "/src/images/Uploaded/Items/");
                }
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return BadRequest();
            }
        }

        [Authorize(Roles = "Administrator, Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                var item =  await _context.Items.FirstOrDefaultAsync(m => m.Id == id);
                if (item == null) { throw new Exception("Item is null"); }
                var vendors = await _context.Vendors.ToListAsync();
                ViewBag.Vendors = new SelectList(vendors, "Id", "Name");
                return View(item);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return NotFound();
            }
        }
        [Authorize(Roles = "Administrator, Staff")]
        [HttpPost]
        public async Task<IActionResult> Edit(IFormFile photo, Item item)
        {
            try
            {
                var updatedItem = await _context.Items.FirstOrDefaultAsync(m => m.Id == item.Id);

                updatedItem!.Amount = item.Amount;

                if (User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value == "Administrator")
                {
                    if (photo != null)
                    {
                        item.Picture = await PhotoSaver.SavePhoto(photo, "/src/images/Uploaded/Items/");
                    }
                    else
                    {
                        item.Picture = updatedItem.Picture;
                    }
                    foreach (PropertyInfo property in typeof(Item).GetProperties().Where(p => p.CanWrite))
                    {
                        property.SetValue(updatedItem, property.GetValue(item, null), null);
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return BadRequest();
            }
        }

		public async Task<IActionResult> Details(int? id)
		{
			try
			{
				if (id == null)
				{
					return BadRequest(new { success = false, description = "ID is not provided" });
				}

				var item = await _context.Items.FirstOrDefaultAsync(m => m.Id == id);
				if (item == null)
				{
					return BadRequest(new { success = false, description = "Item not found" });
				}

				ViewBag.VendorName = await _context.Vendors.Where(x => x.Id == item.Vendor).Select(v => v.Name).FirstOrDefaultAsync();
                return View(item);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				return BadRequest(new { success = false, description = e.ToString() });
			}
		}
        [Authorize]
        [HttpPost]
		public async Task<IActionResult> Favorite(int? id)
		{
			try
			{
				if (id == null)
				{
					return BadRequest(new { success = false, description = "ID is not provided" });
				}

				var item = await _context.Items.FirstOrDefaultAsync(m => m.Id == id);
				if (item == null)
				{
					return BadRequest(new { success = false, description = "Item not found" });
				}
                //logib
                return Ok( new {success = true, description = "Successfully added to favorites" });
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				return BadRequest(new { success = false, description = e.ToString() });
			}
		}
		[Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                var item = await _context.Items.FirstOrDefaultAsync(m => m.Id == id);
                if (item == null) throw new Exception("Not a valid id");
                _context.Items.Remove(item!);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return NotFound();
            }
        }
    }
}
