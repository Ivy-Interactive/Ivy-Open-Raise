using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("countries")]
public partial class Country
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("iso")]
    [StringLength(2)]
    public string Iso { get; set; } = null!;

    [InverseProperty("AddressCountry")]
    public virtual ICollection<Investor> Investors { get; set; } = new List<Investor>();

    [InverseProperty("Country")]
    public virtual ICollection<OrganizationSetting> OrganizationSettings { get; set; } = new List<OrganizationSetting>();

    [ForeignKey("CountryId")]
    [InverseProperty("Countries")]
    public virtual ICollection<Investor> InvestorsNavigation { get; set; } = new List<Investor>();
}
