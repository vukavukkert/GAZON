using System;
using System.Collections.Generic;

namespace GAZON.Database.Models;

public partial class ItemCategory
{
    public int Id { get; set; }

    public int Item { get; set; }

    public int Category { get; set; }

    public virtual Category CategoryNavigation { get; set; } = null!;
}
