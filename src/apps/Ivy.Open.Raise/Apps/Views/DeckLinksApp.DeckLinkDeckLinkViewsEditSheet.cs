namespace Ivy.Open.Raise.Apps.Views;

public class DeckLinkDeckLinkViewsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid deckLinkViewId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deckLinkView = UseState(() => factory.CreateDbContext().DeckLinkViews.FirstOrDefault(e => e.Id == deckLinkViewId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            deckLinkView.Value.UpdatedAt = DateTime.UtcNow;
            db.DeckLinkViews.Update(deckLinkView.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [deckLinkView]);

        return deckLinkView
            .ToForm()
            .Remove(e => e.Id, e => e.DeckLinkId, e => e.CreatedAt, e => e.UpdatedAt)
            .Builder(e => e.IpAddress, e => e.ToTextAreaInput())
            .Builder(e => e.UserAgent, e => e.ToTextAreaInput())
            .Place(e => e.ViewedAt, e => e.IpAddress, e => e.UserAgent)
            .ToSheet(isOpen, "Edit Deck Link View");
    }
}