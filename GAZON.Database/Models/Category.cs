using System;
using System.Collections.Generic;

namespace GAZON.Database.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? MainCategory { get; set; }

    public virtual ICollection<Category> InverseMainCategoryNavigation { get; set; } = new List<Category>();

    public virtual ICollection<ItemCategory> ItemCategories { get; set; } = new List<ItemCategory>();

    public virtual Category? MainCategoryNavigation { get; set; }
}
