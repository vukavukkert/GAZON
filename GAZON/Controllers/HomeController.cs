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

        public async Task<IActionResult> Index(List<ItemViewModel>? itemList = null)
        {
            string login = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Login == login);
            itemList ??= ItemViewModel.ConvertItemtoModel(await _context.Items.ToListAsync(), user); 
            
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
            string login = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.Login == login);
            var itemViewModel = ItemViewModel.ConvertItemtoModel(itemList, user);
            return View(nameof(Index), itemViewModel);
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

                var reviews = await (from r in _context.Reviews
                    join ir in _context.ItemReviews on r.Id equals ir.Review
                    where ir.Item == item.Id select r).ToListAsync();
                
                var vendor = item.VendorNavigation;

                var detailItem = new DetailViewModel(item, reviews, vendor);
                
                return View(detailItem);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				return BadRequest(new { success = false, description = e.ToString() });
			}
		}
        [Authorize]
        public async Task<IActionResult> Favorites()
        {
            string login = User.Identity.Name;
            if (login == null)
            {
                return Unauthorized();
            }
            var user = _context.Users.FirstOrDefault(u => u.Login == login);
            var itemViewModel = ItemViewModel.ConvertItemtoModel(user.UserFavorites.Select(i => i.ItemNavigation).ToList(), user);
            return View(nameof(Index), itemViewModel);
        }
        [Authorize]
        [HttpPost]
		public async Task<IActionResult> Favorite(int? id, bool isChecked)
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
                
                string login = User.Identity.Name;
                var user = _context.Users.FirstOrDefault(u => u.Login == login);
                
                if (isChecked)
                {
                    var favorite = new UserFavorite();
                    favorite.Item = item.Id;
                    favorite.User = user.Id;
                    user.UserFavorites.Add(favorite);
                    await _context.SaveChangesAsync();
                   return Ok( new {success = true, description = "Successfully added to favorites" }); 
                }
                else
                {
                    user.UserFavorites.Remove(user.UserFavorites.FirstOrDefault(f => f.Item == item.Id));
                    await _context.SaveChangesAsync();
                    return Ok( new {success = true, description = "Successfully removed from favorites" }); 
                }
                
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				return BadRequest(new { success = false, description = "Hi"});
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
