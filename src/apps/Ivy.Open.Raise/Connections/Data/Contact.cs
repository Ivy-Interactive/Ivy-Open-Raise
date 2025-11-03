using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("contacts")]
[Index("InvestorId", Name = "IX_contacts_investor_id")]
public partial class Contact
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("email")]
    public string Email { get; set; } = null!;

    [Column("investor_id")]
    public Guid InvestorId { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; } = null!;

    [Column("last_name")]
    public string LastName { get; set; } = null!;

    [Column("title")]
    public string? Title { get; set; }

    [Column("linkedin_url")]
    public string? LinkedinUrl { get; set; }

    [Column("x_url")]
    public string? XUrl { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [InverseProperty("Contact")]
    public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();

    [InverseProperty("Contact")]
    public virtual ICollection<DeckLink> DeckLinks { get; set; } = new List<DeckLink>();

    [InverseProperty("Contact")]
    public virtual ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();

    [ForeignKey("InvestorId")]
    [InverseProperty("Contacts")]
    public virtual Investor Investor { get; set; } = null!;
}
