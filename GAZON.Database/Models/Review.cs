using System;
using System.Collections.Generic;

namespace GAZON.Database.Models;

public partial class Review
{
    public int Id { get; set; }

    public int User { get; set; }

    public DateTime? Date { get; set; }

    public string? Content { get; set; }

    public decimal? Rating { get; set; }

    public virtual ICollection<ReviewAttachment> ReviewAttachments { get; set; } = new List<ReviewAttachment>();

    public virtual User UserNavigation { get; set; } = null!;
}
