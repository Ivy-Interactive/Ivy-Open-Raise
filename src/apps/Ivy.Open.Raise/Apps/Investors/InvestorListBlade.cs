namespace Ivy.Open.Raise.Apps.Investors;

public class InvestorListBlade : ViewBase
{
    private record InvestorListRecord(Guid Id, string Name, bool Deal);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();
        //var showDataTable = UseState(false);

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is Guid investorId)
            {
                blades.Pop(this, true);
                blades.Push(this, new InvestorDetailsBlade(investorId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var investor = (InvestorListRecord)e.Sender.Tag!;
            blades.Push(this, new InvestorDetailsBlade(investor.Id), investor.Name);
        });

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("New Investor").ToTrigger((isOpen) => new InvestorCreateDialog(isOpen, refreshToken));
        
        return new FilteredListView<InvestorListRecord>(
            fetchRecords: (filter) => FetchInvestors(factory, filter),
            createItem: CalculateCreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );

        ListItem CalculateCreateItem(InvestorListRecord record) =>
            new(title: record.Name, subtitle: null, badge: record.Deal ? "DEAL" : null, onClick: onItemClicked, tag: record);
    }

    private async Task<InvestorListRecord[]> FetchInvestors(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Investors.Include(e => e.Contacts).ThenInclude(f => f.Deals).Where(e => e.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.Name.Contains(filter));
        }

        return await linq
            .OrderByDescending(e => e.CreatedAt)
            .Take(50)
            .Select(e => new InvestorListRecord(e.Id, e.Name, e.Contacts.Any() && e.Contacts.Any(c => c.Deals.Any(d => d.DeletedAt == null))))
            .ToArrayAsync();
    }
}
