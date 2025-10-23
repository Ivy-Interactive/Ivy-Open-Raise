namespace Ivy.Open.Raise.Apps.Views;

public class CurrencyOrganizationSettingsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, string currencyId) : ViewBase
{
    private record OrganizationSettingsCreateRequest
    {
        [Required]
        public string? OutreachSubject { get; init; }

        [Required]
        public string? OutreachBody { get; init; }

        [Required]
        public string? StartupWebsite { get; init; }

        [Required]
        public string? StartupLinkedinUrl { get; init; }

        [Required]
        public int StartupStage { get; init; }

        [Required]
        public DateTime? StartupDateOfIncorporation { get; init; }

        [Required]
        public int CountryId { get; init; }

        [Required]
        public string? ElevatorPitch { get; init; }

        [Required]
        public int Cofounders { get; init; }

        [Required]
        public int? RaiseTargetMin { get; init; }

        [Required]
        public int? RaiseTargetMax { get; init; }

        [Required]
        public int? RaiseTicketSize { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var organizationSettings = UseState(() => new OrganizationSettingsCreateRequest());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                CreateOrganizationSettings(factory, organizationSettings.Value);
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [organizationSettings]);

        return organizationSettings
            .ToForm()
            .Builder(e => e.CountryId, e => e.ToAsyncSelectInput(QueryCountries(factory), LookupCountry(factory), placeholder: "Select Country"))
            .Builder(e => e.StartupStage, e => e.ToAsyncSelectInput(QueryStartupStages(factory), LookupStartupStage(factory), placeholder: "Select Startup Stage"))
            .ToDialog(isOpen, title: "Create Organization Settings", submitTitle: "Create");
    }

    private void CreateOrganizationSettings(DataContextFactory factory, OrganizationSettingsCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var organizationSetting = new OrganizationSetting
        {
            CurrencyId = currencyId,
            OutreachSubject = request.OutreachSubject,
            OutreachBody = request.OutreachBody,
            StartupWebsite = request.StartupWebsite,
            StartupLinkedinUrl = request.StartupLinkedinUrl,
            StartupStage = request.StartupStage,
            StartupDateOfIncorporation = request.StartupDateOfIncorporation,
            CountryId = request.CountryId,
            ElevatorPitch = request.ElevatorPitch,
            Cofounders = request.Cofounders,
            RaiseTargetMin = request.RaiseTargetMin,
            RaiseTargetMax = request.RaiseTargetMax,
            RaiseTicketSize = request.RaiseTicketSize
        };

        db.OrganizationSettings.Add(organizationSetting);
        db.SaveChanges();
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

    private static AsyncSelectQueryDelegate<int> QueryStartupStages(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.StartupStages
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int> LookupStartupStage(DataContextFactory factory)
    {
        return async id =>
        {
            await using var db = factory.CreateDbContext();
            var stage = await db.StartupStages.FirstOrDefaultAsync(e => e.Id == id);
            if (stage == null) return null;
            return new Option<int>(stage.Name, stage.Id);
        };
    }
}