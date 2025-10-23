namespace Ivy.Open.Raise.Apps.Settings.Views;

public class CurrencyOrganizationSettingsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int organizationSettingId) : ViewBase
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
            .Builder(e => e.RaiseTargetMin, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.RaiseTargetMax, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.RaiseTicketSize, e => e.ToMoneyInput().Currency("USD"))
            .Place(e => e.OutreachSubject, e => e.OutreachBody)
            .Group("Startup Details", e => e.StartupWebsite, e => e.StartupLinkedinUrl, e => e.StartupDateOfIncorporation, e => e.ElevatorPitch, e => e.Cofounders)
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.CurrencyId)
            .Builder(e => e.CountryId, e => e.ToAsyncSelectInput(QueryCountries(factory), LookupCountry(factory), placeholder: "Select Country"))
            .Builder(e => e.StartupStage, e => e.ToAsyncSelectInput(QueryStartupStages(factory), LookupStartupStage(factory), placeholder: "Select Startup Stage"))
            .ToSheet(isOpen, "Edit Organization Settings");
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