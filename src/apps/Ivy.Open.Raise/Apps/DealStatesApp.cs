using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.Database, path: ["Settings"])]
public class DealStatesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new DealStateListBlade(), "Search");
    }
}