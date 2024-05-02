using System;
using System.Collections.Generic;

namespace GAZON.Database.Models;

public partial class ItemReview
{
    public int Id { get; set; }

    public int Review { get; set; }

    public int Item { get; set; }

    public virtual Item ItemNavigation { get; set; } = null!;
}
