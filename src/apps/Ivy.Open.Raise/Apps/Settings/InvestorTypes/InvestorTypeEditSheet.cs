namespace Ivy.Open.Raise.Apps.Settings.InvestorTypes;

public class InvestorTypeEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int investorTypeId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var details = UseState<InvestorType?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            await using var context = factory.CreateDbContext();
            details.Set(await context.InvestorTypes.FirstOrDefaultAsync(e => e.Id == investorTypeId));
            loading.Set(false);
        });

        if (loading.Value) return null;

        return details
            .ToForm()
            .Remove(e => e.Id)
            .Place(e => e.Name)
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Investor Type");

        async Task OnSubmit(InvestorType? investorType)
        {
            if (investorType == null) return;
            await using var db = factory.CreateDbContext();
            db.InvestorTypes.Update(investorType);
            await db.SaveChangesAsync();
            refreshToken.Refresh();
        }
    }
}
