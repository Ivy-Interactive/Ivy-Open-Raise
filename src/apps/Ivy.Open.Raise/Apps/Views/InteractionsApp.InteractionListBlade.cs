namespace Ivy.Open.Raise.Apps.Views;

public class InteractionListBlade : ViewBase
{
    private record InteractionListRecord(Guid Id, string ContactName, string InteractionType, string? Subject, DateTime OccurredAt);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is Guid interactionId)
            {
                blades.Pop(this, true);
                blades.Push(this, new InteractionDetailsBlade(interactionId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var interaction = (InteractionListRecord)e.Sender.Tag!;
            blades.Push(this, new InteractionDetailsBlade(interaction.Id), interaction.Subject ?? "Interaction Details");
        });

        ListItem CreateItem(InteractionListRecord record) =>
            new(title: record.ContactName, subtitle: $"{record.InteractionType} - {record.Subject}", onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create Interaction").ToTrigger((isOpen) => new InteractionCreateDialog(isOpen, refreshToken));

        return new FilteredListView<InteractionListRecord>(
            fetchRecords: (filter) => FetchInteractions(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<InteractionListRecord[]> FetchInteractions(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Interactions
            .Where(i => i.DeletedAt == null)
            .Select(i => new InteractionListRecord(
                i.Id,
                i.Contact.FirstName + " " + i.Contact.LastName,
                i.InteractionTypeNavigation.Name,
                i.Subject,
                i.OccurredAt
            ));

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(i => i.ContactName.Contains(filter) || (i.Subject != null && i.Subject.Contains(filter)));
        }

        return await linq
            .OrderByDescending(i => i.OccurredAt)
            .Take(50)
            .ToArrayAsync();
    }
}