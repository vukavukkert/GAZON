using GAZON.Database.Models;

namespace GAZON.Models;

public class ItemViewModel
{
    public ItemViewModel(int id, decimal price, string name, string? picture, string? description, bool isFavorite)
    {
        Id = id;
        Price = price;
        Name = name;
        Picture = picture;
        Description = description;
        this.isFavorite = isFavorite;
    }

    public static List<ItemViewModel> ConvertItemtoModel(List<Item> itemList, User user = null)
    {
        List<ItemViewModel> viewModelList = new List<ItemViewModel>();
        foreach (var item in itemList)
        {
            bool isFavorite;
            if (user == null)
            {
                isFavorite = false;
            }
            else
            {
                isFavorite = user.UserFavorites.Select(f => f.ItemNavigation).Contains(item);
            }
            viewModelList.Add(new ItemViewModel(
                item.Id, 
                item.Price,
                item.Name, 
                item.Picture, 
                item.Description, 
                isFavorite
            ));
        }

        return viewModelList;
    }
    public int Id { get; set; }
    public decimal Price { get; set; }
    public string Name { get; set; } = null!;
    public string? Picture { get; set; }
    public string? Description { get; set; }
    public bool isFavorite { get; set; }


}