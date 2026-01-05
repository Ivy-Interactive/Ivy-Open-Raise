using Ivy.Hooks;
using static Ivy.Open.Raise.Apps.Shared;

namespace Ivy.Open.Raise.Apps.Settings;

[App(order:-1, icon:Icons.Settings, path:["Apps", "Settings"], title:"General", isVisible:false)]
public class OrganizationSettingsApp : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var client = UseService<IClientProvider>();
        var queryService = UseService<IQueryService>();

        var settingsQuery = Context.UseOrganizationSettings();

        if (settingsQuery.Loading)
            return Skeleton.Form();

        if (settingsQuery.Value == null)
            return Callout.Warning("Missing Organization Settings.");

        var settings = UseState(() => settingsQuery.Value);
        var currency = settingsQuery.Value.CurrencyId;

        var form = settings
            .ToForm()
            .Builder(e => e.OutreachBody, e => e.ToTextAreaInput().Height(30))
            .Builder(e => e.ElevatorPitch, e => e.ToTextAreaInput().Height(30))
            .Builder(e => e.CurrencyId, SelectCurrencyBuilder)
            .Builder(e => e.CountryId, SelectCountryBuilder)
            .Builder(e => e.StartupStageId, SelectStartupStageBuilder)
            .Builder(e => e.RaiseTargetMin, e => e.ToMoneyInput().Currency(currency))
            .Builder(e => e.RaiseTargetMax, e => e.ToMoneyInput().Currency(currency))
            .Builder(e => e.RaiseTicketSize, e => e.ToMoneyInput().Currency(currency))
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

        return
            Layout.TopCenter()
                | (Layout.Vertical().Width(Size.Full().Max(Size.Units(150)))
                    | Text.H1("Settings")
                    | form);

        async Task OnSubmit(OrganizationSetting? modifiedSettings)
        {
            if (modifiedSettings == null) return;
            await using var db = factory.CreateDbContext();
            db.OrganizationSettings.Update(modifiedSettings);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag(typeof(OrganizationSetting));
            client.Toast("Organization settings updated.");
        }
    }
}
