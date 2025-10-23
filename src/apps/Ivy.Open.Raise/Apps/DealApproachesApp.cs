using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.Handshake, path: ["Apps"])]
public class DealApproachesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new DealApproachListBlade(), "Search");
    }
}