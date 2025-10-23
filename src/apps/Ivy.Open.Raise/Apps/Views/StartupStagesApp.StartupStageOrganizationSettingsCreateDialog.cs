namespace Ivy.Open.Raise.Apps.Views;

public class StartupStageOrganizationSettingsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, int startup_stage) : ViewBase
{
    private record OrganizationSettingsCreateRequest
    {
        [Required]
        [StringLength(3)]
        public string CurrencyId { get; init; } = "";

        [Required]
        public int CountryId { get; init; }

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
        var organizationSettings = UseState(() => new OrganizationSettingsCreateRequest());

        UseEffect(() =>
        {
            var organizationSettingsId = CreateOrganizationSettings(factory, organizationSettings.Value);
            refreshToken.Refresh(organizationSettingsId);
        }, [organizationSettings]);

        return organizationSettings
            .ToForm()
            .Builder(e => e.CurrencyId, e => e.ToAsyncSelectInput(QueryCurrencies(factory), LookupCurrency(factory), placeholder: "Select Currency"))
            .Builder(e => e.CountryId, e => e.ToAsyncSelectInput(QueryCountries(factory), LookupCountry(factory), placeholder: "Select Country"))
            .ToDialog(isOpen, title: "Create Organization Settings", submitTitle: "Create");
    }

    private int CreateOrganizationSettings(DataContextFactory factory, OrganizationSettingsCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var organizationSettings = new OrganizationSetting
        {
            CurrencyId = request.CurrencyId,
            CountryId = request.CountryId,
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
            StartupStage = startup_stage
        };

        db.OrganizationSettings.Add(organizationSettings);
        db.SaveChanges();

        return organizationSettings.Id;
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

    private static AsyncSelectQueryDelegate<int?> QueryCountries(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Countries
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupCountry(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var country = await db.Countries.FirstOrDefaultAsync(e => e.Id == id);
            if (country == null) return null;
            return new Option<int?>(country.Name, country.Id);
        };
    }
}