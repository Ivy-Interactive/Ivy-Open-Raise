namespace Ivy.Open.Raise.Apps.Views;

public class DeckEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid deckId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deck = UseState(() => factory.CreateDbContext().Decks.FirstOrDefault(e => e.Id == deckId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            deck.Value.UpdatedAt = DateTime.UtcNow;
            db.Decks.Update(deck.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [deck]);

        return deck
            .ToForm()
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt)
            .ToSheet(isOpen, "Edit Deck");
    }
}