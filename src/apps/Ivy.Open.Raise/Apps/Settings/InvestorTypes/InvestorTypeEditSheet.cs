using Ivy.Hooks;

namespace Ivy.Open.Raise.Apps.Settings.InvestorTypes;

public class InvestorTypeEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int investorTypeId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var queryService = UseService<IQueryService>();

        var investorTypeQuery = UseQuery(
            key: (nameof(InvestorTypeEditSheet), investorTypeId),
            fetcher: async ct =>
            {
                await using var db = factory.CreateDbContext();
                return await db.InvestorTypes.FirstOrDefaultAsync(e => e.Id == investorTypeId, ct);
            },
            tags: [(typeof(InvestorType), investorTypeId)]
        );

        if (investorTypeQuery.Loading || investorTypeQuery.Value == null)
            return Skeleton.Form().ToSheet(isOpen, "Edit Investor Type");

        return investorTypeQuery.Value
            .ToForm()
            .Remove(e => e.Id)
            .Place(e => e.Name)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Investor Type");

        async Task OnSubmit(InvestorType? modifiedInvestorType)
        {
            if (modifiedInvestorType == null) return;
            await using var db = factory.CreateDbContext();
            db.InvestorTypes.Update(modifiedInvestorType);
            await db.SaveChangesAsync();
            queryService.RevalidateByTag((typeof(InvestorType), investorTypeId));
            refreshToken.Refresh();
        }
    }
}
