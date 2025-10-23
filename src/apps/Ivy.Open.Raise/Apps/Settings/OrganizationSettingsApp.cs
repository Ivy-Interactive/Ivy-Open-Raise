using Ivy.Open.Raise.Apps.Settings.Views;

namespace Ivy.Open.Raise.Apps.Settings;

[App(order:-1, icon:Icons.Settings, path:["Apps", "Settings"], title:"General")]
public class OrganizationSettingsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new OrganizationSettingListBlade(), "Search");
    }
}