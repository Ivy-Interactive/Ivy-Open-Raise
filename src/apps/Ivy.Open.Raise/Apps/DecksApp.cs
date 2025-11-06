using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.Presentation, path: ["Apps"], order: 3)]
public class DecksApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new DeckListBlade(), "Search");
    }
}