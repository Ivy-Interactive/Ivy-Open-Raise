using Ivy.Open.Raise.Apps.Settings.Views;

namespace Ivy.Open.Raise.Apps.Settings;

[App(icon: Icons.Database)]
public class DealStatesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new DealStateListBlade(), "Search");
    }
}