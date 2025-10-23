using Ivy.Open.Raise.Connections.Data;

namespace Ivy.Open.Raise.Apps.Views;

public class CountryListBlade : ViewBase
{
    private record CountryListRecord(int Id, string Name, string Iso);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int countryId)
            {
                blades.Pop(this, true);
                blades.Push(this, new CountryDetailsBlade(countryId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var country = (CountryListRecord)e.Sender.Tag!;
            blades.Push(this, new CountryDetailsBlade(country.Id), country.Name);
        });

        ListItem CreateItem(CountryListRecord record) =>
            new(title: record.Name, subtitle: record.Iso, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create Country").ToTrigger((isOpen) => new CountryCreateDialog(isOpen, refreshToken));

        return new FilteredListView<CountryListRecord>(
            fetchRecords: (filter) => FetchCountries(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<CountryListRecord[]> FetchCountries(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Countries.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.Name.Contains(filter) || e.Iso.Contains(filter));
        }

        return await linq
            .OrderBy(e => e.Name)
            .Take(50)
            .Select(e => new CountryListRecord(e.Id, e.Name, e.Iso))
            .ToArrayAsync();
    }
}