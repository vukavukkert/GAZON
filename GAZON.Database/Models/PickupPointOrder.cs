using System;
using System.Collections.Generic;

namespace GAZON.Database.Models;

public partial class PickupPointOrder
{
    public int Id { get; set; }

    public int User { get; set; }

    public int PickupPoint { get; set; }

    public int? Staff { get; set; }

    public int Order { get; set; }

    public DateTime? PickupDate { get; set; }

    public virtual Order OrderNavigation { get; set; } = null!;

    public virtual PickupPoint PickupPointNavigation { get; set; } = null!;

    public virtual Staff? StaffNavigation { get; set; }

    public virtual User UserNavigation { get; set; } = null!;
}
