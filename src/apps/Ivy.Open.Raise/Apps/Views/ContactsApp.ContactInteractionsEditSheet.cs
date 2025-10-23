namespace Ivy.Open.Raise.Apps.Views;

public class ContactInteractionsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid interactionId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var interaction = UseState(() => factory.CreateDbContext().Interactions.FirstOrDefault(e => e.Id == interactionId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            interaction.Value.UpdatedAt = DateTime.UtcNow;
            db.Interactions.Update(interaction.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [interaction]);

        return interaction
            .ToForm()
            .Builder(e => e.Subject, e => e.ToTextAreaInput())
            .Builder(e => e.Notes, e => e.ToTextAreaInput())
            .Builder(e => e.OccurredAt, e => e.ToDateTimeInput())
            .Place(e => e.Subject, e => e.Notes)
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt, e => e.ContactId)
            .Builder(e => e.InteractionType, e => e.ToAsyncSelectInput(QueryInteractionTypes(factory), LookupInteractionType(factory), placeholder: "Select Interaction Type"))
            .Builder(e => e.UserId, e => e.ToAsyncSelectInput(QueryUsers(factory), LookupUser(factory), placeholder: "Select User"))
            .ToSheet(isOpen, "Edit Interaction");
    }

    private static AsyncSelectQueryDelegate<int?> QueryInteractionTypes(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.InteractionTypes
                    .Where(e => e.Name.Contains(query))
                    .Select(e => new { e.Id, e.Name })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int?>(e.Name, e.Id))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<int?> LookupInteractionType(DataContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var interactionType = await db.InteractionTypes.FirstOrDefaultAsync(e => e.Id == id);
            if (interactionType == null) return null;
            return new Option<int?>(interactionType.Name, interactionType.Id);
        };
    }

    private static AsyncSelectQueryDelegate<Guid?> QueryUsers(DataContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Users
                    .Where(e => e.FirstName.Contains(query) || e.LastName.Contains(query))
                    .Select(e => new { e.Id, FullName = e.FirstName + " " + e.LastName })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<Guid?>(e.FullName, e.Id))
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