using Ivy.Open.Raise.Apps.Settings.Views;

namespace Ivy.Open.Raise.Apps.Settings;

[App(icon: Icons.Handshake)]
public class DealApproachesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new DealApproachListBlade(), "Search");
    }
}