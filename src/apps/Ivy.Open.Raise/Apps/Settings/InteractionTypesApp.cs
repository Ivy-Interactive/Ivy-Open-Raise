using Ivy.Open.Raise.Apps.Settings.Views;

namespace Ivy.Open.Raise.Apps.Settings;

[App(icon: Icons.Table, path:["Apps", "Settings"], isVisible:false)]
public class InteractionTypesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new InteractionTypeListBlade(), "Search");
    }
}