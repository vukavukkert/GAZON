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
        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                var item = await _context.Items.FirstOrDefaultAsync(m => m.Id == id);
                if (item == null) throw new Exception("Not a valid id");
                _context.Items.Remove(item!);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, description = "Deleted successfully" });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return BadRequest(new { success = false, description = e.ToString()});
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteFavorites()
        {
            try
            {
                string login = User.Identity.Name;
                var user = _context.Users.FirstOrDefault(u => u.Login == login);
                
                user.UserFavorites.Clear();

                await _context.SaveChangesAsync();
                return Ok(new { success = true, description = "Successfully cleared from favorites" });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(new { success = false, description = "Error message: " + e });
            }
        }
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteReview(int? id)
        {
            try
            {
                if (id == null) return BadRequest(new { success = false, description = "Id is null" });
                
                var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
                
                if (review == null) return BadRequest(new { success = false, description = "Review is null" });

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, description = "Successfully removed review" });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(new { success = false, description = "Error message: " + e.Message });
            }
        }
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteReviewAttachment(int? id)
        {
            return Ok();
		}
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditReview(string reviewId, string content, string rating, IFormFile[]? images)
        {
            try
            {
                if (String.IsNullOrEmpty(reviewId)) throw new Exception("review id is null");
                var review = _context.Reviews.FirstOrDefault(r => r.Id == int.Parse(reviewId));
                if (review == null) throw new Exception("review is null");
           
                review.Content = content;
                review.Rating = Decimal.Parse(rating);

                if (images != null)
                {
                    for (int i = 0; i < images.Length; i++)
                    {
                        var imagePath = await PhotoSaver.SavePhoto(images[i], "/src/images/Uploaded/Items/");
                        var image = new ReviewAttachment{Date = DateTime.Now, Image = imagePath};
                        review.ReviewAttachments.Add(image);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return BadRequest(new { success = false, description = "Error message: " + e.Message });
            }
            return Ok(new { success = false, description = "Successfully edited reply"});
        }
    }
}
