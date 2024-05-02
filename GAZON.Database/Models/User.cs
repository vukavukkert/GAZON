using System;
using System.Collections.Generic;

namespace GAZON.Database.Models;

public partial class User
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string Login { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Role { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<PickupPointOrder> PickupPointOrders { get; set; } = new List<PickupPointOrder>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Role RoleNavigation { get; set; } = null!;

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();

    public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();
}
