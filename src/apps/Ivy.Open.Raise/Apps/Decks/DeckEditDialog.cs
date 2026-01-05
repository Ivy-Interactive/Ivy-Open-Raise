using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Decks;

public class DeckEditDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid deckId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();

        var deckQuery = UseQuery(
            key: (nameof(DeckEditDialog), deckId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.Decks.FirstOrDefaultAsync(e => e.Id == deckId, ct);
            },
            tags: [(typeof(Deck), deckId)]
        );

        if (deckQuery.Loading || deckQuery.Value == null)
            return Skeleton.Form().ToDialog(isOpen, "Edit Deck");

        return deckQuery.Value
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
            queryService.RevalidateByTag((typeof(Deck), deckId));
            refreshToken.Refresh();
        }
    }
}
