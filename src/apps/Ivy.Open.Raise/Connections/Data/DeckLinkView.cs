using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("deck_link_view")]
[Index("DeckLinkId", Name = "IX_deck_link_view_deck_link_id")]
public partial class DeckLinkView
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("deck_link_id")]
    public Guid DeckLinkId { get; set; }

    [Column("viewed_at")]
    public DateTime ViewedAt { get; set; }

    [Column("ip_address")]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    public string? UserAgent { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("DeckLinkId")]
    [InverseProperty("DeckLinkViews")]
    public virtual DeckLink DeckLink { get; set; } = null!;
}
