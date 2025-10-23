namespace Ivy.Open.Raise.Apps.Views;

public class StartupStageOrganizationSettingsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int organizationSettingId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var organizationSetting = UseState(() => factory.CreateDbContext().OrganizationSettings.FirstOrDefault(e => e.Id == organizationSettingId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            organizationSetting.Value.UpdatedAt = DateTime.UtcNow;
            db.OrganizationSettings.Update(organizationSetting.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [organizationSetting]);

        return organizationSetting
            .ToForm()
            .Builder(e => e.CurrencyId, e => e.ToAsyncSelectInput(QueryCurrencies(factory), LookupCurrency(factory), placeholder: "Select Currency"))
            .Builder(e => e.CountryId, e => e.ToAsyncSelectInput(QueryCountries(factory), LookupCountry(factory), placeholder: "Select Country"))
            .Builder(e => e.RaiseTargetMin, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.RaiseTargetMax, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.RaiseTicketSize, e => e.ToMoneyInput().Currency("USD"))
            .Place(e => e.OutreachSubject, e => e.OutreachBody)
            .Group("Startup Details", e => e.StartupWebsite, e => e.StartupLinkedinUrl, e => e.StartupDateOfIncorporation, e => e.ElevatorPitch, e => e.Cofounders)
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.StartupStage)
            .ToSheet(isOpen, "Edit Organization Settings");
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