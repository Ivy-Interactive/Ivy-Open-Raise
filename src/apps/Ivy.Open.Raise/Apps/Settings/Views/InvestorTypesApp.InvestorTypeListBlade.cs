namespace Ivy.Open.Raise.Apps.Settings.Views;

public class InvestorTypeListBlade : ViewBase
{
    private record InvestorTypeListRecord(int Id, string Name);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int investorTypeId)
            {
                blades.Pop(this, true);
                blades.Push(this, new InvestorTypeDetailsBlade(investorTypeId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var investorType = (InvestorTypeListRecord)e.Sender.Tag!;
            blades.Push(this, new InvestorTypeDetailsBlade(investorType.Id), investorType.Name);
        });

        ListItem CreateItem(InvestorTypeListRecord record) =>
            new(title: record.Name, subtitle: null, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create Investor Type").ToTrigger((isOpen) => new InvestorTypeCreateDialog(isOpen, refreshToken));

        return new FilteredListView<InvestorTypeListRecord>(
            fetchRecords: (filter) => FetchInvestorTypes(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<InvestorTypeListRecord[]> FetchInvestorTypes(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.InvestorTypes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.Name.Contains(filter));
        }

        return await linq
            .OrderBy(e => e.Name)
            .Take(50)
            .Select(e => new InvestorTypeListRecord(e.Id, e.Name))
            .ToArrayAsync();
    }
}