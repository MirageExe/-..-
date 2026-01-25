using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Content.Server.Database;

[Table("amour_boosters")]
public sealed class AmourBooster
{
    [Key]
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = default!;

    public int? OocColor { get; set; }

    public bool IsActive { get; set; } = false;
}
