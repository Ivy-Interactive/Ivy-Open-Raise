using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("decks")]
public partial class Deck
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = null!;

    [Column("file_size")]
    public long FileSize { get; set; }

    [Column("file_type")]
    public string FileType { get; set; } = null!;

    [Column("file_name")]
    public string FileName { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    [InverseProperty("Deck")]
    public virtual ICollection<DeckLink> DeckLinks { get; set; } = new List<DeckLink>();
}
