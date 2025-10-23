namespace Ivy.Open.Raise.Apps.Views;

public class DeckLinkDeckLinkViewsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid deckLinkId) : ViewBase
{
    private record DeckLinkViewCreateRequest
    {
        [Required]
        public DateTime ViewedAt { get; init; } = DateTime.UtcNow;

        public string? IpAddress { get; init; }

        public string? UserAgent { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deckLinkViewRequest = UseState(() => new DeckLinkViewCreateRequest());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                CreateDeckLinkView(factory, deckLinkViewRequest.Value);
                refreshToken.Refresh(deckLinkId);
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [deckLinkViewRequest]);

        return deckLinkViewRequest
            .ToForm()
            .Builder(e => e.IpAddress, e => e.ToTextAreaInput())
            .Builder(e => e.UserAgent, e => e.ToTextAreaInput())
            .ToDialog(isOpen, title: "Create DeckLink View", submitTitle: "Create");
    }

    private void CreateDeckLinkView(DataContextFactory factory, DeckLinkViewCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var deckLinkView = new DeckLinkView()
        {
            DeckLinkId = deckLinkId,
            ViewedAt = request.ViewedAt,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.DeckLinkViews.Add(deckLinkView);
        db.SaveChanges();
    }
}