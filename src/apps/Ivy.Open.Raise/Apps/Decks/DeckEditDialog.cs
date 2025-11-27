namespace Ivy.Open.Raise.Apps.Decks;

public class DeckEditDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid deckId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deck = UseState<Deck?>();
        var loading = UseState(true);
        
        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            deck.Set(context.Decks.FirstOrDefault(e => e.Id == deckId));
            loading.Set(false);
        });

        if (loading.Value) return new Loading();

        return deck
            .ToForm()
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt)
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, "Edit Deck");

        async Task OnSubmit(Deck? modifiedDeck)
        {
            if (modifiedDeck == null) return;
            await using var db = factory.CreateDbContext();
            modifiedDeck.UpdatedAt = DateTime.UtcNow;
            db.Decks.Update(modifiedDeck);
            await db.SaveChangesAsync();
            refreshToken.Refresh();
        }
    }
}