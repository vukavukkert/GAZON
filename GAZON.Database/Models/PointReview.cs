using System;
using System.Collections.Generic;

namespace GAZON.Database.Models;

public partial class PointReview
{
    public int Id { get; set; }

    public int Review { get; set; }

    public int Point { get; set; }

    public virtual PickupPoint PointNavigation { get; set; } = null!;
}
