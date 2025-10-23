namespace Ivy.Open.Raise.Apps.Views;

public class ContactInteractionsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, Guid contactId) : ViewBase
{
    private record InteractionCreateRequest
    {
        [Required]
        public Guid UserId { get; init; }

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

        UseEffect(() =>
        {
            CreateInteraction(factory, interaction.Value);
            refreshToken.Refresh();
        }, [interaction]);

        return interaction
            .ToForm()
            .Builder(e => e.UserId, e => e.ToAsyncSelectInput(QueryUsers(factory), LookupUser(factory), placeholder: "Select User"))
            .Builder(e => e.InteractionType, e => e.ToAsyncSelectInput(QueryInteractionTypes(factory), LookupInteractionType(factory), placeholder: "Select Interaction Type"))
            .Builder(e => e.Subject, e => e.ToTextAreaInput())
            .Builder(e => e.Notes, e => e.ToTextAreaInput())
            .Builder(e => e.OccurredAt, e => e.ToDateTimeInput())
            .ToDialog(isOpen, title: "Create Interaction", submitTitle: "Create");
    }

    private void CreateInteraction(DataContextFactory factory, InteractionCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var interaction = new Interaction
        {
            Id = Guid.NewGuid(),
            ContactId = contactId,
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