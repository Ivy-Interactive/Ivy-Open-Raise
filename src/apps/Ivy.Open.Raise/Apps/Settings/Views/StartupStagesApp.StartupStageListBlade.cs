namespace Ivy.Open.Raise.Apps.Settings.Views;

public class StartupStageListBlade : ViewBase
{
    private record StartupStageListRecord(int Id, string Name);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int startupStageId)
            {
                blades.Pop(this, true);
                blades.Push(this, new StartupStageDetailsBlade(startupStageId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var startupStage = (StartupStageListRecord)e.Sender.Tag!;
            blades.Push(this, new StartupStageDetailsBlade(startupStage.Id), startupStage.Name);
        });

        ListItem CreateItem(StartupStageListRecord record) =>
            new(title: record.Name, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("Create Startup Stage").ToTrigger((isOpen) => new StartupStageCreateDialog(isOpen, refreshToken));

        return new FilteredListView<StartupStageListRecord>(
            fetchRecords: (filter) => FetchStartupStages(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<StartupStageListRecord[]> FetchStartupStages(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.StartupStages.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.Name.Contains(filter));
        }

        return await linq
            .OrderBy(e => e.Id)
            .Take(50)
            .Select(e => new StartupStageListRecord(e.Id, e.Name))
            .ToArrayAsync();
    }
}