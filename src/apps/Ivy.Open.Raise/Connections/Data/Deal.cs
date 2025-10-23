using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("deal")]
[Index("ContactId", Name = "IX_deal_contact_id")]
[Index("DealApproachId", Name = "IX_deal_deal_approach_id")]
[Index("DealStateId", Name = "IX_deal_deal_state_id")]
[Index("OwnerId", Name = "IX_deal_owner_id")]
public partial class Deal
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("contact_id")]
    public Guid ContactId { get; set; }

    [Column("deal_state_id")]
    public int DealStateId { get; set; }

    [Column("deal_approach_id")]
    public int? DealApproachId { get; set; }

    [Column("owner_id")]
    public Guid OwnerId { get; set; }

    [Column("amount_from")]
    public int? AmountFrom { get; set; }

    [Column("amount_to")]
    public int? AmountTo { get; set; }

    [Column("priority")]
    public int? Priority { get; set; }

    [Column("order")]
    public float Order { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("next_action")]
    public DateTime? NextAction { get; set; }

    [Column("next_action_notes")]
    public string? NextActionNotes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("ContactId")]
    [InverseProperty("Deals")]
    public virtual Contact Contact { get; set; } = null!;

    [ForeignKey("DealApproachId")]
    [InverseProperty("Deals")]
    public virtual DealApproach? DealApproach { get; set; }

    [ForeignKey("DealStateId")]
    [InverseProperty("Deals")]
    public virtual DealState DealState { get; set; } = null!;

    [ForeignKey("OwnerId")]
    [InverseProperty("Deals")]
    public virtual User Owner { get; set; } = null!;
}
