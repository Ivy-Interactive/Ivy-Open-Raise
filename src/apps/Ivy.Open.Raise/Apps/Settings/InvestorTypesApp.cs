using Ivy.Open.Raise.Apps.Settings.Views;

namespace Ivy.Open.Raise.Apps.Settings;

[App(icon: Icons.User, path:["Apps", "Settings"], isVisible:false)]
public class InvestorTypesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new InvestorTypeListBlade(), "Search");
    }
}