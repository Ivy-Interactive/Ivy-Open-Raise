namespace Ivy.Open.Raise.Apps.Settings.Views;

public class StartupVerticalListBlade : ViewBase
{
    private record StartupVerticalListRecord(int Id, string Name);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int startupVerticalId)
            {
                blades.Pop(this, true);
                blades.Push(this, new StartupVerticalDetailsBlade(startupVerticalId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var startupVertical = (StartupVerticalListRecord)e.Sender.Tag!;
            blades.Push(this, new StartupVerticalDetailsBlade(startupVertical.Id), startupVertical.Name);
        });

        ListItem CreateItem(StartupVerticalListRecord record) =>
            new(title: record.Name, subtitle: null, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create Startup Vertical").ToTrigger((isOpen) => new StartupVerticalCreateDialog(isOpen, refreshToken));

        return new FilteredListView<StartupVerticalListRecord>(
            fetchRecords: (filter) => FetchStartupVerticals(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<StartupVerticalListRecord[]> FetchStartupVerticals(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.StartupVerticals.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.Name.Contains(filter));
        }

        return await linq
            .OrderBy(e => e.Name)
            .Take(50)
            .Select(e => new StartupVerticalListRecord(e.Id, e.Name))
            .ToArrayAsync();
    }
}