using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("currency")]
public partial class Currency
{
    [Key]
    [Column("id")]
    [StringLength(3)]
    public string Id { get; set; } = null!;

    [Column("symbol")]
    public string Symbol { get; set; } = null!;

    [Column("name")]
    public string Name { get; set; } = null!;

    [InverseProperty("Currency")]
    public virtual ICollection<OrganizationSetting> OrganizationSettings { get; set; } = new List<OrganizationSetting>();
}
