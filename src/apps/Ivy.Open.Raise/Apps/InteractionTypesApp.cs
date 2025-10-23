using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.Table, path: ["Settings"])]
public class InteractionTypesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new InteractionTypeListBlade(), "Search");
    }
}