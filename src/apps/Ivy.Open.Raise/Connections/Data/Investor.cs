using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("investors")]
[Index("AddressCountryId", Name = "IX_investors_address_country_id")]
[Index("InvestorTypeId", Name = "IX_investors_investor_type_id")]
public partial class Investor
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("website_url")]
    public string? WebsiteUrl { get; set; }

    [Column("linkedin_url")]
    public string? LinkedinUrl { get; set; }

    [Column("x_url")]
    public string? XUrl { get; set; }

    [Column("address_street")]
    public string? AddressStreet { get; set; }

    [Column("address_zip")]
    public string? AddressZip { get; set; }

    [Column("address_city")]
    public string? AddressCity { get; set; }

    [Column("address_country_id")]
    public int? AddressCountryId { get; set; }

    [Column("investor_type_id")]
    public int InvestorTypeId { get; set; }

    [Column("thesis")]
    public string? Thesis { get; set; }

    [Column("check_size_min")]
    public int? CheckSizeMin { get; set; }

    [Column("check_size_max")]
    public int? CheckSizeMax { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("AddressCountryId")]
    [InverseProperty("Investors")]
    public virtual Country? AddressCountry { get; set; }

    [InverseProperty("Investor")]
    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();

    [ForeignKey("InvestorTypeId")]
    [InverseProperty("Investors")]
    public virtual InvestorType InvestorType { get; set; } = null!;

    [ForeignKey("InvestorId")]
    [InverseProperty("InvestorsNavigation")]
    public virtual ICollection<Country> Countries { get; set; } = new List<Country>();

    [ForeignKey("InvestorId")]
    [InverseProperty("Investors")]
    public virtual ICollection<StartupStage> StartupStages { get; set; } = new List<StartupStage>();

    [ForeignKey("InvestorId")]
    [InverseProperty("Investors")]
    public virtual ICollection<StartupVertical> StartupVerticals { get; set; } = new List<StartupVertical>();
}
