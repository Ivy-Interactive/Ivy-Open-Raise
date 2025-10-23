using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.Settings, path: ["Settings"])]
public class OrganizationSettingsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new OrganizationSettingListBlade(), "Search");
    }
}