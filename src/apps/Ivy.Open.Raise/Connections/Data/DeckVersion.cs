using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("deck_versions")]
public partial class DeckVersion
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("deck_id")]
    public Guid DeckId { get; set; }

    [Column("file_size")]
    public long FileSize { get; set; }

    [Column("content_type")]
    public string ContentType { get; set; } = null!;

    [Column("file_name")]
    public string FileName { get; set; } = null!;

    [Column("blob_name")]
    public string BlobName { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    [ForeignKey("DeckId")]
    [InverseProperty("DeckVersions")]
    public virtual Deck Deck { get; set; } = null!;
}
