using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.InvestorTypes;

public class InvestorTypeListBlade : ViewBase
{
    public record InvestorTypeListRecord(int Id, string Name);

    public override object? Build()
    {
        var blades = UseContext<IBladeService>();
        var refreshToken = UseRefreshToken();

        var filter = UseState("");
        var throttledFilter = UseState("");

        UseEffect(() =>
        {
            throttledFilter.Set(filter.Value);
            blades.Pop(this);
        }, [filter.Throttle(TimeSpan.FromMilliseconds(250)).ToTrigger()]);

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int investorTypeId)
            {
                blades.Pop(this);
                blades.Push(this, new InvestorTypeDetailsBlade(investorTypeId));
            }
        }, [refreshToken]);

        var investorTypesQuery = UseInvestorTypeList(Context, throttledFilter.Value);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var investorType = (InvestorTypeListRecord)e.Sender.Tag!;
            blades.Push(this, new InvestorTypeDetailsBlade(investorType.Id), investorType.Name);
        });

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("New Investor Type").ToTrigger((isOpen) => new InvestorTypeCreateDialog(isOpen, refreshToken));

        var items = (investorTypesQuery.Value ?? [])
            .Select(record => new ListItem(
                title: record.Name,
                subtitle: null,
                onClick: onItemClicked,
                tag: record))
            .ToArray();

        var header = Layout.Horizontal().Gap(1)
                     | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
                     | createBtn;

        return new Fragment()
               | new BladeHeader(header)
               | (investorTypesQuery.Loading ? Text.Muted("Loading...") : new List(items));
    }

    public static QueryResult<InvestorTypeListRecord[]> UseInvestorTypeList(IViewContext context, string filter)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseInvestorTypeList), filter),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var linq = db.InvestorTypes.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var trimmed = filter.Trim();
                    linq = linq.Where(e => e.Name.Contains(trimmed));
                }

                return await linq
                    .OrderBy(e => e.Name)
                    .Take(50)
                    .Select(e => new InvestorTypeListRecord(e.Id, e.Name))
                    .ToArrayAsync(ct);
            },
            tags: [typeof(InvestorType[])],
            options: new QueryOptions { KeepPrevious = true }
        );
    }
}
