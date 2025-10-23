using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("deal_state")]
public partial class DealState
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;
    
    [Column("order")]
    public int Order { get; set; }

    [InverseProperty("DealState")]
    public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();
}
