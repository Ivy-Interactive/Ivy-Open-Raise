using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.InvestorTypes;

public class InvestorTypeDetailsBlade(int investorTypeId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var blades = UseContext<IBladeService>();
        var queryService = UseService<IQueryService>();
        var refreshToken = UseRefreshToken();

        var query = UseQuery(
            key: (nameof(InvestorTypeDetailsBlade), investorTypeId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                var investorType = await db.InvestorTypes.SingleOrDefaultAsync(e => e.Id == investorTypeId, ct);
                var investorCount = await db.Investors.CountAsync(e => e.InvestorTypeId == investorTypeId, ct);
                return (investorType, investorCount);
            },
            tags: [(typeof(InvestorType), investorTypeId)]
        );

        if (query.Loading) return Skeleton.Card();
        if (query.Value.investorType == null)
            return new Callout($"Investor type '{investorTypeId}' not found.").Variant(CalloutVariant.Warning);

        var investorTypeValue = query.Value.investorType;
        var investorCount = query.Value.investorCount;

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Disabled(investorCount > 0).Icon(Icons.Trash).HandleSelect(async () =>
                {
                    await DeleteAsync(factory);
                    queryService.RevalidateByTag(typeof(InvestorType[]));
                    blades.Pop();
                })
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new InvestorTypeEditSheet(isOpen, refreshToken, investorTypeId));

        var detailsCard = new Card(
            content: new
            {
                investorTypeValue.Name,
                Investors = investorCount
            }.ToDetails(),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Investor Type Details");

        return new Fragment()
               | new BladeHeader(Text.Literal(investorTypeValue.Name))
               | (Layout.Vertical() | detailsCard);
    }

    private async Task DeleteAsync(DataContextFactory dbFactory)
    {
        await using var db = dbFactory.CreateDbContext();

        var connectedInvestorsCount = await db.Investors.CountAsync(e => e.InvestorTypeId == investorTypeId);

        if (connectedInvestorsCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete investor type with {connectedInvestorsCount} connected investor(s).");
        }

        var investorType = await db.InvestorTypes.FirstOrDefaultAsync(e => e.Id == investorTypeId);
        if (investorType != null)
        {
            db.InvestorTypes.Remove(investorType);
            await db.SaveChangesAsync();
        }
    }
}
