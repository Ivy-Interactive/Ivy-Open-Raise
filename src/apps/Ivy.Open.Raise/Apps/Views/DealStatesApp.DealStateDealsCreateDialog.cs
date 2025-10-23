namespace Ivy.Open.Raise.Apps.Views;

public class DealStateDealsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, int dealStateId) : ViewBase
{
    private record DealCreateRequest
    {
        [Required]
        public Guid ContactId { get; init; }

        [Required]
        public Guid OwnerId { get; init; }

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
        var deal = UseState(() => new DealCreateRequest());

        UseEffect(() =>
        {
            var dealId = CreateDeal(factory, deal.Value);
            refreshToken.Refresh(dealId);
        }, [deal]);

        return deal
            .ToForm()
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(QueryContacts(factory), LookupContact(factory), placeholder: "Select Contact"))
            .Builder(e => e.OwnerId, e => e.ToAsyncSelectInput(QueryUsers(factory), LookupUser(factory), placeholder: "Select Owner"))
            .Builder(e => e.AmountFrom, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.AmountTo, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.Priority, e => e.ToFeedbackInput())
            .ToDialog(isOpen, title: "Create Deal", submitTitle: "Create");
    }

    private Guid CreateDeal(DataContextFactory factory, DealCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var deal = new Deal
        {
            ContactId = request.ContactId,
            OwnerId = request.OwnerId,
            DealStateId = dealStateId,
            AmountFrom = request.AmountFrom,
            AmountTo = request.AmountTo,
            Priority = request.Priority,
            Notes = request.Notes,
            NextAction = request.NextAction,
            NextActionNotes = request.NextActionNotes,
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
            return (await db.Contacts
                    .Where(e => e.FirstName.Contains(query) || e.LastName.Contains(query))
                    .Select(e => new { e.Id, Name = $"{e.FirstName} {e.LastName}" })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<Guid?>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<Guid?> LookupContact(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var contact = await db.Contacts.FirstOrDefaultAsync(e => e.Id == id);
            if (contact == null) return null;
            return new Option<Guid?>(contact.FirstName + " " + contact.LastName, contact.Id);
        };
    }

    private static AsyncSelectQueryDelegate<Guid?> QueryUsers(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Users
                    .Where(e => e.FirstName.Contains(query) || e.LastName.Contains(query))
                    .Select(e => new { e.Id, Name = $"{e.FirstName} {e.LastName}" })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<Guid?>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<Guid?> LookupUser(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var user = await db.Users.FirstOrDefaultAsync(e => e.Id == id);
            if (user == null) return null;
            return new Option<Guid?>(user.FirstName + " " + user.LastName, user.Id);
        };
    }
}