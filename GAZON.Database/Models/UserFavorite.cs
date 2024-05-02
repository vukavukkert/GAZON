using System;
using System.Collections.Generic;

namespace GAZON.Database.Models;

public partial class UserFavorite
{
    public int Id { get; set; }

    public int User { get; set; }

    public int Item { get; set; }

    public virtual Item ItemNavigation { get; set; } = null!;

    public virtual User UserNavigation { get; set; } = null!;
}
