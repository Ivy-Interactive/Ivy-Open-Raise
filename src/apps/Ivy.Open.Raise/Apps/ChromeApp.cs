using Ivy.Open.Raise.Apps.Settings;

namespace Ivy.Open.Raise.Apps;

[App(isVisible:false)]
public class ChromeApp : ViewBase
{
    public AppShellSettings Settings => new AppShellSettings()
        .UseTabs(preventDuplicates: true)
        .UseFooterMenuItemsTransformer((menuItems, navigator) =>
        {
            return [ 
                MenuItem.Default("Users").Icon(Icons.User).OnSelect(() => navigator.Navigate<UsersApp>()),
                MenuItem.Default("Settings").Icon(Icons.Settings) 
                | MenuItem.Default("General").OnSelect(() => navigator.Navigate<OrganizationSettingsApp>())
                | MenuItem.Default("Deal Approaches").OnSelect(() => navigator.Navigate<DealApproachesApp>())
                | MenuItem.Default("Deal States").OnSelect(() => navigator.Navigate<DealStatesApp>())
                | MenuItem.Default("Interaction Types").OnSelect(() => navigator.Navigate<InteractionTypesApp>())
                | MenuItem.Default("Investor Types").OnSelect(() => navigator.Navigate<InvestorTypesApp>())
                | MenuItem.Default("Startup Stages").OnSelect(() => navigator.Navigate<StartupStagesApp>())
                | MenuItem.Default("Verticals").OnSelect(() => navigator.Navigate<StartupVerticalsApp>()),
                ..menuItems
            ];
        })
        .WallpaperApp<WallpaperApp>();
    
    
    public override object? Build()
    {
        var settings = Context.UseOrganizationSettings();
        if (settings.Loading) return null; 
        
        if(!settings.Value.OnboardingCompleted)
        {
            return new OnboardingApp();
        }

        return new DefaultSidebarAppShell(Settings);
    }
}