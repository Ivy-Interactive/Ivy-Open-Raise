namespace Ivy.Open.Raise.Apps.Settings.InvestorTypes;

public class InvestorTypeEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int investorTypeId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var investorType = UseState(() => factory.CreateDbContext().InvestorTypes.FirstOrDefault(e => e.Id == investorTypeId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            db.InvestorTypes.Update(investorType.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [investorType]);

        return investorType
            .ToForm()
            .Remove(e => e.Id)
            .Place(e => e.Name)
            .ToSheet(isOpen, "Edit Investor Type");
    }
}
