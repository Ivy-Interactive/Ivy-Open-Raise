namespace Ivy.Open.Raise.Apps.Views;

public class InvestorListBlade : ViewBase
{
    private record InvestorListRecord(Guid Id, string Name, string? CountryName);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

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

        ListItem CreateItem(InvestorListRecord record) =>
            new(title: record.Name, subtitle: record.CountryName, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("Create Investor").ToTrigger((isOpen) => new InvestorCreateDialog(isOpen, refreshToken));

        return new FilteredListView<InvestorListRecord>(
            fetchRecords: (filter) => FetchInvestors(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<InvestorListRecord[]> FetchInvestors(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var query = db.Investors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            query = query.Where(e => e.Name.Contains(filter) || (e.AddressCountry != null && e.AddressCountry.Name.Contains(filter)));
        }

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .Take(50)
            .Select(e => new InvestorListRecord(e.Id, e.Name, e.AddressCountry != null ? e.AddressCountry.Name : null))
            .ToArrayAsync();
    }
}