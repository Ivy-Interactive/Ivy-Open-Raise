using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Decks;

public class DeckLinksEditDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid deckLinkId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();

        var query = UseQuery(
            key: (nameof(DeckLinksEditDialog), deckLinkId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.DeckLinks.FirstOrDefaultAsync(e => e.Id == deckLinkId, ct);
            },
            tags: [(typeof(DeckLink), deckLinkId)]
        );

        if (query.Loading || query.Value == null)
            return Skeleton.Form().ToDialog(isOpen, "Edit Deck Link");

        return query.Value
            .ToForm()
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(Shared.UseContactSearch, Shared.UseContactLookup, placeholder: "Select Contact"))
            .Remove(e => e.Id, e => e.DeckId, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt, e => e.Secret)
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, "Edit Deck Link");

        async Task OnSubmit(DeckLink? modifiedDeckLink)
        {
            if (modifiedDeckLink == null) return;
            await using var db = factory.CreateDbContext();
            modifiedDeckLink.UpdatedAt = DateTime.UtcNow;
            db.DeckLinks.Update(modifiedDeckLink);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag((typeof(DeckLink), deckLinkId));
            refreshToken.Refresh();
        }
    }
}