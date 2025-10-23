namespace Ivy.Open.Raise.Apps.Views;

public class CurrencyListBlade : ViewBase
{
    private record CurrencyListRecord(string Id, string Name, string Symbol);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is string currencyId)
            {
                blades.Pop(this, true);
                blades.Push(this, new CurrencyDetailsBlade(currencyId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var currency = (CurrencyListRecord)e.Sender.Tag!;
            blades.Push(this, new CurrencyDetailsBlade(currency.Id), currency.Name);
        });

        ListItem CreateItem(CurrencyListRecord record) =>
            new(title: record.Name, subtitle: record.Symbol, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create Currency").ToTrigger((isOpen) => new CurrencyCreateDialog(isOpen, refreshToken));

        return new FilteredListView<CurrencyListRecord>(
            fetchRecords: (filter) => FetchCurrencies(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<CurrencyListRecord[]> FetchCurrencies(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Currencies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.Name.Contains(filter) || e.Symbol.Contains(filter) || e.Id.Contains(filter));
        }

        return await linq
            .OrderBy(e => e.Name)
            .Take(50)
            .Select(e => new CurrencyListRecord(e.Id, e.Name, e.Symbol))
            .ToArrayAsync();
    }
}