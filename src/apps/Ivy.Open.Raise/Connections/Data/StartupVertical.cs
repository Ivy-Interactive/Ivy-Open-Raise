using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("startup_vertical")]
public partial class StartupVertical
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [ForeignKey("StartupVerticalId")]
    [InverseProperty("StartupVerticals")]
    public virtual ICollection<Investor> Investors { get; set; } = new List<Investor>();
}
