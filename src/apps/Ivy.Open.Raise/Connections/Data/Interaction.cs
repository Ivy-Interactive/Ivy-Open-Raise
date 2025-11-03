using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("interactions")]
[Index("ContactId", Name = "IX_interactions_contact_id")]
[Index("InteractionType", Name = "IX_interactions_interaction_type")]
[Index("UserId", Name = "IX_interactions_user_id")]
public partial class Interaction
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("contact_id")]
    public Guid ContactId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("interaction_type")]
    public int InteractionType { get; set; }

    [Column("subject")]
    public string? Subject { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("occurred_at")]
    public DateTime OccurredAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("ContactId")]
    [InverseProperty("Interactions")]
    public virtual Contact Contact { get; set; } = null!;

    [ForeignKey("InteractionType")]
    [InverseProperty("Interactions")]
    public virtual InteractionType InteractionTypeNavigation { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Interactions")]
    public virtual User User { get; set; } = null!;
}
