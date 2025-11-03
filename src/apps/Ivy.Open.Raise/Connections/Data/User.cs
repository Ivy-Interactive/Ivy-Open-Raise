using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("users")]
public partial class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("email")]
    public string Email { get; set; } = null!;

    [Column("first_name")]
    public string FirstName { get; set; } = null!;

    [Column("last_name")]
    public string LastName { get; set; } = null!;

    [Column("title")]
    public string? Title { get; set; }

    [Column("calendar_url")]
    public string? CalendarUrl { get; set; }

    [Column("profile_picture_url")]
    public string? ProfilePictureUrl { get; set; }

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

    [InverseProperty("Owner")]
    public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();

    [InverseProperty("User")]
    public virtual ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
}
