using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.File, path: ["Apps"], order: 2)]
public class DealsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new DealListBlade(), "Search");
    }
}