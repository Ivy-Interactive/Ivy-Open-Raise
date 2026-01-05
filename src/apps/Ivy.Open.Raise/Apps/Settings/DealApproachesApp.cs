using Ivy.Open.Raise.Apps.Settings.DealApproaches;

namespace Ivy.Open.Raise.Apps.Settings;

[App(icon: Icons.Handshake, path:["Apps", "Settings"], isVisible:false)]
public class DealApproachesApp : ViewBase
{
    public override object? Build()
    {
        return UseBlades(() => new DealApproachListBlade(), "Search");
    }
}