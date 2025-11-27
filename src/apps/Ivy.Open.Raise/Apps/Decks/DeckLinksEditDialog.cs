namespace Ivy.Open.Raise.Apps.Decks;

public class DeckLinksEditDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid deckLinkId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState<DeckLink?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            details.Set(await context.DeckLinks.FirstOrDefaultAsync(e => e.Id == deckLinkId));
            loading.Set(false);
        });

        if (loading.Value) return new Loading();

        return details
            .ToForm()
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(Shared.QueryContacts(factory), Shared.LookupContact(factory), placeholder: "Select Contact"))
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
            refreshToken.Refresh();
        }
    }
}