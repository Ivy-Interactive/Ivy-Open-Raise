namespace Ivy.Open.Raise.Apps.Settings.Views;

public class OrganizationSettingListBlade : ViewBase
{
    private record OrganizationSettingListRecord(int Id, string CurrencyName, string CountryName);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<DataContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int organizationSettingId)
            {
                blades.Pop(this, true);
                blades.Push(this, new OrganizationSettingDetailsBlade(organizationSettingId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var organizationSetting = (OrganizationSettingListRecord)e.Sender.Tag!;
            blades.Push(this, new OrganizationSettingDetailsBlade(organizationSetting.Id), organizationSetting.CurrencyName);
        });

        ListItem CreateItem(OrganizationSettingListRecord record) =>
            new(title: record.CurrencyName, subtitle: record.CountryName, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("Create Organization Setting").ToTrigger((isOpen) => new OrganizationSettingCreateDialog(isOpen, refreshToken));

        return new FilteredListView<OrganizationSettingListRecord>(
            fetchRecords: (filter) => FetchOrganizationSettings(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<OrganizationSettingListRecord[]> FetchOrganizationSettings(DataContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.OrganizationSettings
            .Include(e => e.Currency)
            .Include(e => e.Country)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.Currency.Name.Contains(filter) || e.Country.Name.Contains(filter));
        }

        return await linq
            .OrderByDescending(e => e.Id)
            .Take(50)
            .Select(e => new OrganizationSettingListRecord(e.Id, e.Currency.Name, e.Country.Name))
            .ToArrayAsync();
    }
}