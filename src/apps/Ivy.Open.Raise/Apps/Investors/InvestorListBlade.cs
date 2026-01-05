using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Investors;

public class InvestorListBlade : ViewBase
{
    public record InvestorListRecord(Guid Id, string Name, bool Deal);

    public override object? Build()
    {
        var blades = UseContext<IBladeService>();
        var factory = UseService<DataContextFactory>();
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
            if (refreshToken.ReturnValue is Guid investorId)
            {
                blades.Pop(this);
                blades.Push(this, new InvestorDetailsBlade(investorId));
            }
        }, [refreshToken]);

        var investorsQuery = UseInvestorList(Context, throttledFilter.Value);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var investor = (InvestorListRecord)e.Sender.Tag!;
            blades.Push(this, new InvestorDetailsBlade(investor.Id), investor.Name);
        });

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Ghost().Tooltip("New Investor").ToTrigger((isOpen) => new InvestorCreateDialog(isOpen, refreshToken));

        var items = (investorsQuery.Value ?? [])
            .Select(record => new ListItem(
                title: record.Name,
                subtitle: null,
                badge: record.Deal ? "DEAL" : null,
                onClick: onItemClicked,
                tag: record))
            .ToArray();

        var header = Layout.Horizontal().Gap(1)
                     | filter.ToSearchInput().Placeholder("Search").Width(Size.Grow())
                     | createBtn;

        return new Fragment()
               | new BladeHeader(header)
               | (investorsQuery.Loading ? Text.Muted("Loading...") : new List(items));
    }

    public static QueryResult<InvestorListRecord[]> UseInvestorList(IViewContext context, string filter)
    {
        var factory = context.UseService<DataContextFactory>();
        return context.UseQuery(
            key: (nameof(UseInvestorList), filter),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();

                var linq = db.Investors.Include(e => e.Contacts).ThenInclude(f => f.Deals).Where(e => e.DeletedAt == null);

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var trimmed = filter.Trim();
                    linq = linq.Where(e => e.Name.Contains(trimmed));
                }

                return await linq
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(50)
                    .Select(e => new InvestorListRecord(e.Id, e.Name, e.Contacts.Any() && e.Contacts.Any(c => c.Deals.Any(d => d.DeletedAt == null))))
                    .ToArrayAsync(ct);
            },
            tags: [typeof(Investor[])],
            options: new QueryOptions { KeepPrevious = true }
        );
    }
}
