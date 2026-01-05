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
        var details = UseState(() => new DealCreateRequest
        {
            DealStateId = 1, //todo: Get the state with the lowest order
            //todo: populate OwnerId with current user
        });

        return details
            .ToForm()
            .Label(e => e.DealStateId, "State")
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(UseContactSearch, UseContactLookup, placeholder: "Select Contact"))
            .Builder(e => e.DealStateId, e => e.ToAsyncSelectInput(UseDealStateSearch, UseDealStateLookup, placeholder: "Select Deal State"))
            .Builder(e => e.OwnerId, e => e.ToAsyncSelectInput(UseUserSearch, UseUserLookup, placeholder: "Select Owner"))
            .Place(e => e.Amount, e => e.ContactId, e => e.DealStateId)
            .HandleSubmit(OnSubmit)
            .ToDialog(isOpen, title: "New Deal", submitTitle: "Create");

        async Task OnSubmit(DealCreateRequest request)
        {
            await using var db = factory.CreateDbContext();

            var deal = new Deal
            {
                Id = Guid.NewGuid(),
                ContactId = request.ContactId,
                DealStateId = request.DealStateId,
                OwnerId = request.OwnerId,
                AmountFrom = request.Amount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Deals.Add(deal);
            await db.SaveChangesAsync();
            refreshToken.Refresh(deal.Id);
        }
    }
}