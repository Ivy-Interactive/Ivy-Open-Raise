namespace Ivy.Open.Raise.Apps.Views;

public class UserDealsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid ownerId) : ViewBase
{
    private record DealCreateRequest
    {
        [Required]
        public Guid ContactId { get; init; }

        [Required]
        public int DealStateId { get; init; }

        public int? DealApproachId { get; init; }

        public int? AmountFrom { get; init; }

        public int? AmountTo { get; init; }

        public int? Priority { get; init; }

        public string? Notes { get; init; }

        public DateTime? NextAction { get; init; }

        public string? NextActionNotes { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var dealRequest = UseState(() => new DealCreateRequest());

        UseEffect(() =>
        {
            CreateDeal(factory, dealRequest.Value);
            refreshToken.Refresh();
        }, [dealRequest]);

        return dealRequest
            .ToForm()
            .Builder(e => e.ContactId, e => e.ToTextAreaInput())
            .Builder(e => e.DealStateId, e => e.ToFeedbackInput())
            .Builder(e => e.DealApproachId, e => e.ToFeedbackInput())
            .Builder(e => e.AmountFrom, e => e.ToFeedbackInput())
            .Builder(e => e.AmountTo, e => e.ToFeedbackInput())
            .Builder(e => e.Priority, e => e.ToFeedbackInput())
            .Builder(e => e.Notes, e => e.ToTextAreaInput())
            .Builder(e => e.NextAction, e => e.ToFeedbackInput())
            .Builder(e => e.NextActionNotes, e => e.ToTextAreaInput())
            .ToDialog(isOpen, title: "Create Deal", submitTitle: "Create");
    }

    private void CreateDeal(DataContextFactory factory, DealCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var deal = new Deal()
        {
            Id = Guid.NewGuid(),
            ContactId = request.ContactId,
            DealStateId = request.DealStateId,
            DealApproachId = request.DealApproachId,
            AmountFrom = request.AmountFrom,
            AmountTo = request.AmountTo,
            Priority = request.Priority,
            Notes = request.Notes,
            NextAction = request.NextAction,
            NextActionNotes = request.NextActionNotes,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Deals.Add(deal);
        db.SaveChanges();
    }
}