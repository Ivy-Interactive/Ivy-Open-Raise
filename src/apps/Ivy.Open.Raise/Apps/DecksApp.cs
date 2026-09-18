using Ivy.Open.Raise.Apps.Decks;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.Presentation, group: ["Apps"], order: 3)]
public class DecksApp : ViewBase
{
    public override object? Build()
    {
        return UseBlades(() => new DeckListBlade(), "Search");
    }
}