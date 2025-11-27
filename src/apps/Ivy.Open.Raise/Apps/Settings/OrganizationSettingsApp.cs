using static Ivy.Open.Raise.Apps.Shared;

namespace Ivy.Open.Raise.Apps.Settings;

[App(order:-1, icon:Icons.Settings, path:["Apps", "Settings"], title:"General", isVisible:false)]
public class OrganizationSettingsApp : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var client = UseService<IClientProvider>();
        var details = UseState<OrganizationSetting?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            details.Set(await context.OrganizationSettings.FirstOrDefaultAsync());
            loading.Set(false);
        });

        if (loading.Value) return null;

        if (details.Value == null) return Callout.Warning("Missing Organization Settings.");

        var form = details
            .ToForm()
            .Builder(e => e.OutreachBody, e => e.ToTextAreaInput().Height(30))
            .Builder(e => e.ElevatorPitch, e => e.ToTextAreaInput().Height(30))
            .Builder(e => e.CurrencyId, SelectCurrencyBuilder(factory))
            .Builder(e => e.CountryId, SelectCountryBuilder(factory))
            .Builder(e => e.StartupStageId, SelectStartupStageBuilder(factory))
            .Builder(e => e.RaiseTargetMin, e => e.ToMoneyInput().Currency(details.Value.CurrencyId))
            .Builder(e => e.RaiseTargetMax, e => e.ToMoneyInput().Currency(details.Value.CurrencyId))
            .Builder(e => e.RaiseTicketSize, e => e.ToMoneyInput().Currency(details.Value.CurrencyId))
            .Place(e => e.OutreachSubject, e => e.OutreachBody)
            .Label(e => e.StartupDateOfIncorporation, "Date of Incorporation")
            .Label(e => e.StartupLinkedinUrl, "LinkedIn URL")
            .Label(e => e.StartupWebsite, "Website")
            .Label(e => e.StartupName, "Company Name")
            .Label(e => e.StartupGovId, "Identification Number")
            .Description(e => e.StartupGovId, "Tax ID, Government Registration Number, etc.")
            .Builder(e => e.StartupGovId, e => e.ToTextInput())
            .Required(e => e.StartupName)
            .Group("Startup",
                e => e.StartupName,
                e => e.StartupGovId,
                e => e.CountryId,
                e => e.StartupWebsite,
                e => e.StartupLinkedinUrl,
                e => e.StartupDateOfIncorporation,
                e => e.ElevatorPitch,
                e => e.Cofounders)
            .Group("Raise",
                e => e.StartupStageId,
                e => e.CurrencyId,
                e => e.RaiseTargetMin,
                e => e.RaiseTargetMax,
                e => e.RaiseTicketSize)
            .Group("Outreach",
                e => e.OutreachSubject,
                e => e.OutreachBody)
            .Remove(e => e.Id)
            .HandleSubmit(OnSubmit);

        return Layout.Vertical()
            | Text.H1("Settings")
            | form;

        async Task OnSubmit(OrganizationSetting? organizationSetting)
        {
            if (organizationSetting == null) return;
            await using var db = factory.CreateDbContext();
            db.OrganizationSettings.Update(organizationSetting);
            await db.SaveChangesAsync();
            client.Toast("Organization settings updated.");
        }
    }
}