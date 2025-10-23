namespace Ivy.Open.Raise.Apps.Views;

public class InteractionCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record InteractionCreateRequest
    {
        [Required]
        public Guid ContactId { get; init; }

        [Required]
        public Guid UserId { get; init; }

        [Required]
        public int InteractionType { get; init; }

        public string? Subject { get; init; }

        public string? Notes { get; init; }

        [Required]
        public DateTime OccurredAt { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var interaction = UseState(() => new InteractionCreateRequest());

        UseEffect(() =>
        {
            var interactionId = CreateInteraction(factory, interaction.Value);
            refreshToken.Refresh(interactionId);
        }, [interaction]);

        return interaction
            .ToForm()
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(QueryContacts(factory), LookupContact(factory), placeholder: "Select Contact"))
            .Builder(e => e.UserId, e => e.ToAsyncSelectInput(QueryUsers(factory), LookupUser(factory), placeholder: "Select User"))
            .Builder(e => e.InteractionType, e => e.ToAsyncSelectInput(QueryInteractionTypes(factory), LookupInteractionType(factory), placeholder: "Select Interaction Type"))
            .Builder(e => e.OccurredAt, e => e.ToDateTimeInput())
            .ToDialog(isOpen, title: "Create Interaction", submitTitle: "Create");
    }

    private Guid CreateInteraction(DataContextFactory factory, InteractionCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var interaction = new Interaction
        {
            Id = Guid.NewGuid(),
            ContactId = request.ContactId,
            UserId = request.UserId,
            InteractionType = request.InteractionType,
            Subject = request.Subject,
            Notes = request.Notes,
            OccurredAt = request.OccurredAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Interactions.Add(interaction);
        db.SaveChanges();

        return interaction.Id;
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
}