using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Open.Raise.Connections.Data;

public partial class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<Deal> Deals { get; set; }

    public virtual DbSet<DealApproach> DealApproaches { get; set; }

    public virtual DbSet<DealState> DealStates { get; set; }

    public virtual DbSet<Deck> Decks { get; set; }

    public virtual DbSet<DeckLink> DeckLinks { get; set; }

    public virtual DbSet<DeckLinkView> DeckLinkViews { get; set; }

    public virtual DbSet<Interaction> Interactions { get; set; }

    public virtual DbSet<InteractionType> InteractionTypes { get; set; }

    public virtual DbSet<Investor> Investors { get; set; }

    public virtual DbSet<InvestorType> InvestorTypes { get; set; }

    public virtual DbSet<OrganizationSetting> OrganizationSettings { get; set; }

    public virtual DbSet<StartupStage> StartupStages { get; set; }

    public virtual DbSet<StartupVertical> StartupVerticals { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Iso).IsFixedLength();
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.Property(e => e.Id).IsFixedLength();
        });

        modelBuilder.Entity<Deal>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<DealApproach>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<DealState>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Deck>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<DeckLink>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<DeckLinkView>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Interaction>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<InteractionType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Investor>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasMany(d => d.Countries).WithMany(p => p.InvestorsNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "InvestorCountry",
                    r => r.HasOne<Country>().WithMany().HasForeignKey("CountryId"),
                    l => l.HasOne<Investor>().WithMany().HasForeignKey("InvestorId"),
                    j =>
                    {
                        j.HasKey("InvestorId", "CountryId");
                        j.ToTable("investor_countries");
                        j.HasIndex(new[] { "CountryId" }, "IX_investor_countries_country_id");
                        j.IndexerProperty<Guid>("InvestorId").HasColumnName("investor_id");
                        j.IndexerProperty<int>("CountryId").HasColumnName("country_id");
                    });

            entity.HasMany(d => d.StartupStages).WithMany(p => p.Investors)
                .UsingEntity<Dictionary<string, object>>(
                    "InvestorStartupStage",
                    r => r.HasOne<StartupStage>().WithMany().HasForeignKey("StartupStage"),
                    l => l.HasOne<Investor>().WithMany().HasForeignKey("InvestorId"),
                    j =>
                    {
                        j.HasKey("InvestorId", "StartupStage");
                        j.ToTable("investor_startup_stages");
                        j.HasIndex(new[] { "StartupStage" }, "IX_investor_startup_stages_startup_stage");
                        j.IndexerProperty<Guid>("InvestorId").HasColumnName("investor_id");
                        j.IndexerProperty<int>("StartupStage").HasColumnName("startup_stage");
                    });

            entity.HasMany(d => d.StartupVerticals).WithMany(p => p.Investors)
                .UsingEntity<Dictionary<string, object>>(
                    "InvestorStartupVertical",
                    r => r.HasOne<StartupVertical>().WithMany()
                        .HasForeignKey("StartupVerticalId")
                        .HasConstraintName("FK_investor_startup_verticals_startup_vertical_startup_vertica~"),
                    l => l.HasOne<Investor>().WithMany().HasForeignKey("InvestorId"),
                    j =>
                    {
                        j.HasKey("InvestorId", "StartupVerticalId");
                        j.ToTable("investor_startup_verticals");
                        j.HasIndex(new[] { "StartupVerticalId" }, "IX_investor_startup_verticals_startup_vertical_id");
                        j.IndexerProperty<Guid>("InvestorId").HasColumnName("investor_id");
                        j.IndexerProperty<int>("StartupVerticalId").HasColumnName("startup_vertical_id");
                    });
        });

        modelBuilder.Entity<InvestorType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<OrganizationSetting>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CurrencyId).IsFixedLength();
        });

        modelBuilder.Entity<StartupStage>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<StartupVertical>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
