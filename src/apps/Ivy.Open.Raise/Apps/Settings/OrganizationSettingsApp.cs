using Ivy.Open.Raise.Apps.Settings.Views;

namespace Ivy.Open.Raise.Apps.Settings;

[App(icon: Icons.Settings)]
public class OrganizationSettingsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new OrganizationSettingListBlade(), "Search");
    }
}