using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("organization_settings")]
[Index("CountryId", Name = "IX_organization_settings_country_id")]
[Index("CurrencyId", Name = "IX_organization_settings_currency_id")]
[Index("StartupStage", Name = "IX_organization_settings_startup_stage")]
public partial class OrganizationSetting
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("currency_id")]
    [StringLength(3)]
    public string CurrencyId { get; set; } = null!;

    [Column("outreach_subject")]
    public string? OutreachSubject { get; set; }

    [Column("outreach_body")]
    public string? OutreachBody { get; set; }

    [Column("startup_website")]
    public string? StartupWebsite { get; set; }

    [Column("startup_linkedin_url")]
    public string? StartupLinkedinUrl { get; set; }

    [Column("startup_stage")]
    public int StartupStage { get; set; }

    [Column("startup_date_of_incorporation")]
    public DateTime? StartupDateOfIncorporation { get; set; }

    [Column("country_id")]
    public int CountryId { get; set; }

    [Column("elevator_pitch")]
    public string? ElevatorPitch { get; set; }

    [Column("cofounders")]
    public int Cofounders { get; set; }

    [Column("raise_target_min")]
    public int? RaiseTargetMin { get; set; }

    [Column("raise_target_max")]
    public int? RaiseTargetMax { get; set; }

    [Column("raise_ticket_size")]
    public int? RaiseTicketSize { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("CountryId")]
    [InverseProperty("OrganizationSettings")]
    public virtual Country Country { get; set; } = null!;

    [ForeignKey("CurrencyId")]
    [InverseProperty("OrganizationSettings")]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey("StartupStage")]
    [InverseProperty("OrganizationSettings")]
    public virtual StartupStage StartupStageNavigation { get; set; } = null!;
}
