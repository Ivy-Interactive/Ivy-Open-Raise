namespace Ivy.Open.Raise.Apps.Views;

public class DealStateDealsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid dealId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var deal = UseState(() => factory.CreateDbContext().Deals.FirstOrDefault(e => e.Id == dealId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            deal.Value.UpdatedAt = DateTime.UtcNow;
            db.Deals.Update(deal.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [deal]);

        return deal
            .ToForm()
            .Builder(e => e.AmountFrom, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.AmountTo, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.Notes, e => e.ToTextAreaInput())
            .Builder(e => e.NextActionNotes, e => e.ToTextAreaInput())
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(QueryContacts(factory), LookupContact(factory), placeholder: "Select Contact"))
            .Builder(e => e.DealApproachId, e => e.ToAsyncSelectInput(QueryDealApproaches(factory), LookupDealApproach(factory), placeholder: "Select Deal Approach"))
            .Builder(e => e.OwnerId, e => e.ToAsyncSelectInput(QueryUsers(factory), LookupUser(factory), placeholder: "Select Owner"))
            .Place(e => e.AmountFrom, e => e.AmountTo)
            .Group("Details", e => e.Notes, e => e.NextActionNotes, e => e.Priority, e => e.Order)
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DealStateId, e => e.DeletedAt)
            .ToSheet(isOpen, "Edit Deal");
    }

    private static AsyncSelectQueryDelegate<Guid?> QueryContacts(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Contacts
                    .Where(e => e.FirstName.Contains(query) || e.LastName.Contains(query))
                    .Select(e => new { e.Id, Name = e.FirstName + " " + e.LastName })
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

    private static AsyncSelectQueryDelegate<int?> QueryDealApproaches(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.DealApproaches
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupDealApproach(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var dealApproach = await db.DealApproaches.FirstOrDefaultAsync(e => e.Id == id);
            if (dealApproach == null) return null;
            return new Option<int?>(dealApproach.Name, dealApproach.Id);
        };
    }

    private static AsyncSelectQueryDelegate<Guid?> QueryUsers(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Users
                    .Where(e => e.FirstName.Contains(query) || e.LastName.Contains(query))
                    .Select(e => new { e.Id, Name = e.FirstName + " " + e.LastName })
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