using Ivy.Open.Raise.Apps.Settings;

namespace Ivy.Open.Raise.Apps;

[App(isVisible:false)]
public class ChromeApp : ViewBase
{
    public ChromeSettings Settings => new ChromeSettings()
        .UseTabs(preventDuplicates: true)
        .UseFooterMenuItemsTransformer((menuItems, navigator) =>
        {
            return [ 
                MenuItem.Default("Users").Icon(Icons.User).HandleSelect(() => navigator.Navigate<UsersApp>()),
                MenuItem.Default("Settings").Icon(Icons.Settings) 
                | MenuItem.Default("General").HandleSelect(() => navigator.Navigate<OrganizationSettingsApp>())
                | MenuItem.Default("Deal Approaches").HandleSelect(() => navigator.Navigate<DealApproachesApp>())
                | MenuItem.Default("Deal States").HandleSelect(() => navigator.Navigate<DealStatesApp>())
                | MenuItem.Default("Interaction Types").HandleSelect(() => navigator.Navigate<InteractionTypesApp>())
                | MenuItem.Default("Investor Types").HandleSelect(() => navigator.Navigate<InvestorTypesApp>())
                | MenuItem.Default("Startup Stages").HandleSelect(() => navigator.Navigate<StartupStagesApp>())
                | MenuItem.Default("Verticals").HandleSelect(() => navigator.Navigate<StartupVerticalsApp>()),
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

        return new DefaultSidebarChrome(Settings);
    }
}