namespace Ivy.Open.Raise.Apps.Settings.Views;

public class CountryOrganizationSettingsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, int countryId) : ViewBase
{
    private record OrganizationSettingsCreateRequest
    {
        [Required]
        [StringLength(3)]
        public string CurrencyId { get; init; } = "";

        [Required]
        public int StartupStage { get; init; }

        [Required]
        public int Cofounders { get; init; }

        public string? OutreachSubject { get; init; }
        public string? OutreachBody { get; init; }
        public string? StartupWebsite { get; init; }
        public string? StartupLinkedinUrl { get; init; }
        public DateTime? StartupDateOfIncorporation { get; init; }
        public string? ElevatorPitch { get; init; }
        public int? RaiseTargetMin { get; init; }
        public int? RaiseTargetMax { get; init; }
        public int? RaiseTicketSize { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var settings = UseState(() => new OrganizationSettingsCreateRequest());

        UseEffect(() =>
        {
            CreateOrganizationSettings(factory, settings.Value);
            refreshToken.Refresh();
        }, [settings]);

        return settings
            .ToForm()
            .Builder(e => e.CurrencyId, e => e.ToAsyncSelectInput(QueryCurrencies(factory), LookupCurrency(factory), placeholder: "Select Currency"))
            .Builder(e => e.StartupStage, e => e.ToAsyncSelectInput(QueryStartupStages(factory), LookupStartupStage(factory), placeholder: "Select Startup Stage"))
            .Builder(e => e.RaiseTargetMin, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.RaiseTargetMax, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.RaiseTicketSize, e => e.ToMoneyInput().Currency("USD"))
            .ToDialog(isOpen, title: "Create Organization Settings", submitTitle: "Create");
    }

    private void CreateOrganizationSettings(DataContextFactory factory, OrganizationSettingsCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var settings = new OrganizationSetting
        {
            CurrencyId = request.CurrencyId,
            StartupStage = request.StartupStage,
            Cofounders = request.Cofounders,
            OutreachSubject = request.OutreachSubject,
            OutreachBody = request.OutreachBody,
            StartupWebsite = request.StartupWebsite,
            StartupLinkedinUrl = request.StartupLinkedinUrl,
            StartupDateOfIncorporation = request.StartupDateOfIncorporation,
            ElevatorPitch = request.ElevatorPitch,
            RaiseTargetMin = request.RaiseTargetMin,
            RaiseTargetMax = request.RaiseTargetMax,
            RaiseTicketSize = request.RaiseTicketSize,
            CountryId = countryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.OrganizationSettings.Add(settings);
        db.SaveChanges();
    }

    private static AsyncSelectQueryDelegate<string?> QueryCurrencies(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Currencies
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<string?>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<string?> LookupCurrency(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var currency = await db.Currencies.FirstOrDefaultAsync(e => e.Id == id);
            if (currency == null) return null;
            return new Option<string?>(currency.Name, currency.Id);
        };
    }

    private static AsyncSelectQueryDelegate<int?> QueryStartupStages(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.StartupStages
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupStartupStage(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var stage = await db.StartupStages.FirstOrDefaultAsync(e => e.Id == id);
            if (stage == null) return null;
            return new Option<int?>(stage.Name, stage.Id);
        };
    }
}