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
            .Builder(e => e.FileType, e => e.ToTextAreaInput())
            .Builder(e => e.FileName, e => e.ToTextAreaInput())
            .Builder(e => e.StorageWriteUrl, e => e.ToUrlInput())
            .Builder(e => e.StorageReadUrl, e => e.ToUrlInput())
            .Builder(e => e.Title, e => e.ToTextAreaInput())
            .Builder(e => e.FileSize, e => e.ToFeedbackInput())
            .Place(e => e.Title, e => e.FileName)
            .Place(true, e => e.FileType, e => e.FileSize)
            .Group("Storage", e => e.StorageWriteUrl, e => e.StorageReadUrl)
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt)
            .ToSheet(isOpen, "Edit Deck");
    }
}