using static Ivy.Open.Raise.Apps.Shared;

namespace Ivy.Open.Raise.Apps.Pipeline;

public class DealCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record DealCreateRequest
    {
        [Required] 
        [Range(0, int.MaxValue)] 
        public int Amount { get; set; } = 0;
        
        [Required]
        public Guid ContactId { get; init; }

        [Required]
        public int DealStateId { get; init; }

        [Required]
        public Guid OwnerId { get; init; } = Guid.Empty;
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deal = UseState(() => new DealCreateRequest()
        {
            DealStateId = 1, //todo: Get the state with the lowest order
            //todo: populate OwnerId with current user
        });

        UseEffect(() =>
        {
            var dealId = CreateDeal(factory, deal.Value);
            refreshToken.Refresh(dealId);
        }, [deal]);

        return deal
            .ToForm()
            .Label(e => e.DealStateId, "State")
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(QueryContacts(factory), LookupContact(factory), placeholder: "Select Contact"))
            .Builder(e => e.DealStateId, e => e.ToAsyncSelectInput(QueryDealStates(factory), LookupDealState(factory), placeholder: "Select Deal State"))
            .Builder(e => e.OwnerId, e => e.ToAsyncSelectInput(QueryUsers(factory), LookupUser(factory), placeholder: "Select Owner"))
            .Place(e => e.Amount, e => e.ContactId, e => e.DealStateId)
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