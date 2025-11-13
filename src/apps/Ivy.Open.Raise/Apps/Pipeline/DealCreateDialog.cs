using static Ivy.Open.Raise.Apps.Shared;

namespace Ivy.Open.Raise.Apps.Pipeline;

public class DealCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record DealCreateRequest
    {
        [Required]
        public Guid ContactId { get; init; }

        [Required]
        public int DealStateId { get; init; }

        [Required]
        public Guid OwnerId { get; init; } = Guid.Empty;

        [Required]
        [Range(0, int.MaxValue)] //todo ivy
        public int? Amount { get; set; }
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deal = UseState(() => new DealCreateRequest()
        {
            DealStateId = 1
        });

        UseEffect(() =>
        {
            var dealId = CreateDeal(factory, deal.Value);
            refreshToken.Refresh(dealId);
        }, [deal]);

        return deal
            .ToForm()
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(QueryContacts(factory), LookupContact(factory), placeholder: "Select Contact"))
            .Builder(e => e.DealStateId, e => e.ToAsyncSelectInput(QueryDealStates(factory), LookupDealState(factory), placeholder: "Select Deal State"))
            .Remove(e => e.OwnerId)
            .Place(e => e.ContactId, e => e.DealStateId, e => e.Amount)
            .ToDialog(isOpen, title: "Create Deal", submitTitle: "Create");
    }

    private Guid CreateDeal(DataContextFactory factory, DealCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var deal = new Deal()
        {
            ContactId = request.ContactId,
            DealStateId = request.DealStateId,
            OwnerId = request.OwnerId,
            AmountFrom = request.Amount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Deals.Add(deal);
        db.SaveChanges();

        return deal.Id;
    }
}