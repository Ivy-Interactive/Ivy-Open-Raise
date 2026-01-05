using Ivy.Open.Raise.Apps.Settings.InvestorTypes;

namespace Ivy.Open.Raise.Apps.Settings;

[App(icon: Icons.User, path:["Apps", "Settings"], isVisible:false)]
public class InvestorTypesApp : ViewBase
{
    public override object? Build()
    {
        return UseBlades(() => new InvestorTypeListBlade(), "Search");
    }
}