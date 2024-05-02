using System;
using System.Collections.Generic;

namespace GAZON.Database.Models;

public partial class ReviewAttachment
{
    public int Id { get; set; }

    public DateTime? Date { get; set; }

    public string? Image { get; set; }

    public int Review { get; set; }

    public virtual Review ReviewNavigation { get; set; } = null!;
}
