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

    private static AsyncSelectQueryDelegate<Guid?> QueryContacts(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Contacts.Include(c => c.Investor)
                        
                    .Where(e => (e.FirstName.Contains(query) || e.LastName.Contains(query) || e.Investor.Name.Contains(query)) && e.DeletedAt == null)
                    .Select(e => new
                    {
                        e.Id, 
                        FullName = e.FirstName + " " + e.LastName,
                        Investor = e.Investor.Name
                    })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<Guid?>(e.FullName, e.Id, description:e.Investor))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<Guid?> LookupContact(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var contact = await db
                .Contacts
                .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);
            if (contact == null) return null;
            return new Option<Guid?>(contact.FirstName + " " + contact.LastName, contact.Id);
        };
    }

    private static AsyncSelectQueryDelegate<int?> QueryDealStates(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.DealStates
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupDealState(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var dealState = await db.DealStates.FirstOrDefaultAsync(e => e.Id == id);
            if (dealState == null) return null;
            return new Option<int?>(dealState.Name, dealState.Id);
        };
    }
}