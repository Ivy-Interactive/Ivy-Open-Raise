using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("investor_types")]
public partial class InvestorType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [InverseProperty("InvestorType")]
    public virtual ICollection<Investor> Investors { get; set; } = new List<Investor>();
}
