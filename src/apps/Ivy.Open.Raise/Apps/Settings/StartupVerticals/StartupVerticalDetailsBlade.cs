using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.StartupVerticals;

public class StartupVerticalDetailsBlade(int startupVerticalId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeService>();
        var queryService = UseService<IQueryService>();
        var refreshToken = UseRefreshToken();

        var query = UseQuery(
            key: (nameof(StartupVerticalDetailsBlade), startupVerticalId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                var startupVertical = await db.StartupVerticals.SingleOrDefaultAsync(e => e.Id == startupVerticalId, ct);
                var investorCount = await db.Investors.CountAsync(e => e.StartupVerticals.Any(v => v.Id == startupVerticalId), ct);
                return (startupVertical, investorCount);
            },
            tags: [(typeof(StartupVertical), startupVerticalId)]
        );

        if (query.Loading) return Skeleton.Card();
        if (query.Value.startupVertical == null)
            return new Callout($"Startup vertical '{startupVerticalId}' not found.").Variant(CalloutVariant.Warning);

        var startupVerticalValue = query.Value.startupVertical;
        var investorCount = query.Value.investorCount;

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Disabled(investorCount > 0).Icon(Icons.Trash).HandleSelect(async () =>
                {
                    await DeleteAsync(factory);
                    queryService.RevalidateByTag(typeof(StartupVertical[]));
                    blades.Pop();
                })
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new StartupVerticalEditSheet(isOpen, refreshToken, startupVerticalId));

        var detailsCard = new Card(
            content: new
            {
                startupVerticalValue.Name,
                Investors = investorCount
            }.ToDetails(),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Startup Vertical Details");

        return new Fragment()
               | new BladeHeader(Text.Literal(startupVerticalValue.Name))
               | (Layout.Vertical() | detailsCard);
    }

    private async Task DeleteAsync(DataContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();

        var connectedInvestorsCount = await db.Investors.CountAsync(e => e.StartupVerticals.Any(v => v.Id == startupVerticalId));

        if (connectedInvestorsCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete startup vertical with {connectedInvestorsCount} connected investor(s).");
        }

        var startupVertical = await db.StartupVerticals.FirstOrDefaultAsync(e => e.Id == startupVerticalId);
        if (startupVertical != null)
        {
            db.StartupVerticals.Remove(startupVertical);
            await db.SaveChangesAsync();
        }
    }
}
