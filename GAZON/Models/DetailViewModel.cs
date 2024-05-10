using GAZON.Database.Models;

namespace GAZON.Models;

public class DetailViewModel
{
    public Item item;
    public List<Review>? reviews;
    public Vendor vendor;

    public DetailViewModel(Item item, List<Review>? reviews, Vendor vendor)
    {
        this.item = item;
        this.reviews = reviews;
        this.vendor = vendor;
    }
}