using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("deck_links")]
[Index("ContactId", Name = "IX_deck_links_contact_id")]
[Index("DeckId", Name = "IX_deck_links_deck_id")]
public partial class DeckLink
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("secret")]
    public string Secret { get; set; } = null!;

    [Column("reference")]
    public string? Reference { get; set; } = null!;
    
    [Column("contact_id")]
    public Guid? ContactId { get; set; }

    [Column("deck_id")]
    public Guid DeckId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("ContactId")]
    [InverseProperty("DeckLinks")]
    public virtual Contact? Contact { get; set; }

    [ForeignKey("DeckId")]
    [InverseProperty("DeckLinks")]
    public virtual Deck Deck { get; set; } = null!;

    [InverseProperty("DeckLink")]
    public virtual ICollection<DeckLinkView> DeckLinkViews { get; set; } = new List<DeckLinkView>();
}
