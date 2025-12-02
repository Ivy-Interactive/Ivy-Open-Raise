using Ivy.Open.Raise.Apps.Settings.DealStates;

namespace Ivy.Open.Raise.Apps.Settings;

[App(icon: Icons.Database, path:["Apps", "Settings"], isVisible:false)]
public class DealStatesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new DealStateListBlade(), "Search");
    }
}