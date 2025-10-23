using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

[Table("startup_stage")]
public partial class StartupStage
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = null!;

    [InverseProperty("StartupStageNavigation")]
    public virtual ICollection<OrganizationSetting> OrganizationSettings { get; set; } = new List<OrganizationSetting>();

    [ForeignKey("StartupStage")]
    [InverseProperty("StartupStages")]
    public virtual ICollection<Investor> Investors { get; set; } = new List<Investor>();
}
