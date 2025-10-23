namespace Ivy.Open.Raise.Apps.Views;

public class CountryOrganizationSettingsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int organizationSettingsId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var organizationSettings = UseState(() => factory.CreateDbContext().OrganizationSettings.FirstOrDefault(e => e.Id == organizationSettingsId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            organizationSettings.Value.UpdatedAt = DateTime.UtcNow;
            db.OrganizationSettings.Update(organizationSettings.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [organizationSettings]);

        return organizationSettings
            .ToForm()
            .Builder(e => e.CurrencyId, e => e.ToAsyncSelectInput(QueryCurrencies(factory), LookupCurrency(factory), placeholder: "Select Currency"))
            .Builder(e => e.StartupStage, e => e.ToAsyncSelectInput(QueryStartupStages(factory), LookupStartupStage(factory), placeholder: "Select Startup Stage"))
            .Builder(e => e.RaiseTargetMin, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.RaiseTargetMax, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.RaiseTicketSize, e => e.ToMoneyInput().Currency("USD"))
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.CountryId)
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
            var startupStage = await db.StartupStages.FirstOrDefaultAsync(e => e.Id == id);
            if (startupStage == null) return null;
            return new Option<int?>(startupStage.Name, startupStage.Id);
        };
    }
}