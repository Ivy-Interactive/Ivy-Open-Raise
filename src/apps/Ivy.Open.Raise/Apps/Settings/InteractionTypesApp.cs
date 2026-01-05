using Ivy.Open.Raise.Apps.Settings.InteractionTypes;

namespace Ivy.Open.Raise.Apps.Settings;

[App(icon: Icons.Table, path:["Apps", "Settings"], isVisible:false)]
public class InteractionTypesApp : ViewBase
{
    public override object? Build()
    {
        return UseBlades(() => new InteractionTypeListBlade(), "Search");
    }
}