namespace Ivy.Open.Raise.Apps.Views;

public class UserInteractionsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid userId) : ViewBase
{
    private record InteractionCreateRequest
    {
        [Required]
        public Guid ContactId { get; init; }

        [Required]
        public int InteractionType { get; init; }

        public string? Subject { get; init; }

        public string? Notes { get; init; }

        [Required]
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    }

    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var interaction = UseState(() => new InteractionCreateRequest());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                var interactionId = CreateInteraction(factory, interaction.Value);
                refreshToken.Refresh(interactionId);
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [interaction]);

        return interaction
            .ToForm()
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(QueryContacts(factory), LookupContact(factory), placeholder: "Select Contact"))
            .Builder(e => e.InteractionType, e => e.ToAsyncSelectInput(QueryInteractionTypes(factory), LookupInteractionType(factory), placeholder: "Select Interaction Type"))
            .ToDialog(isOpen, title: "Create Interaction", submitTitle: "Create");
    }

    private Guid CreateInteraction(DataContextFactory factory, InteractionCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var interaction = new Interaction
        {
            Id = Guid.NewGuid(),
            ContactId = request.ContactId,
            UserId = userId,
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