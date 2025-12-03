using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("deal_states")]
public partial class DealState
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;
    
    [Column("order")]
    public int Order { get; set; }

    [Column("is_final")]
    public bool IsFinal { get; set; } = false;

    [InverseProperty("DealState")]
    public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();
}
