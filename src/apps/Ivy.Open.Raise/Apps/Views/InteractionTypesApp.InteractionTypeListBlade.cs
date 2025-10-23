namespace Ivy.Open.Raise.Apps.Views;

public class InteractionTypeListBlade : ViewBase
{
    private record InteractionTypeListRecord(int Id, string Name);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int interactionTypeId)
            {
                blades.Pop(this, true);
                blades.Push(this, new InteractionTypeDetailsBlade(interactionTypeId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var interactionType = (InteractionTypeListRecord)e.Sender.Tag!;
            blades.Push(this, new InteractionTypeDetailsBlade(interactionType.Id), interactionType.Name);
        });

        ListItem CreateItem(InteractionTypeListRecord record) =>
            new(title: record.Name, subtitle: null, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create Interaction Type").ToTrigger((isOpen) => new InteractionTypeCreateDialog(isOpen, refreshToken));

        return new FilteredListView<InteractionTypeListRecord>(
            fetchRecords: (filter) => FetchInteractionTypes(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<InteractionTypeListRecord[]> FetchInteractionTypes(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.InteractionTypes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.Name.Contains(filter));
        }

        return await linq
            .OrderBy(e => e.Name)
            .Take(50)
            .Select(e => new InteractionTypeListRecord(e.Id, e.Name))
            .ToArrayAsync();
    }
}