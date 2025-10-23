namespace Ivy.Open.Raise.Apps.Views;

public class InvestorTypeEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int investorTypeId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var investorType = UseState(() => factory.CreateDbContext().InvestorTypes.FirstOrDefault(e => e.Id == investorTypeId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                db.InvestorTypes.Update(investorType.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [investorType]);

        return investorType
            .ToForm()
            .Place(e => e.Name)
            .ToSheet(isOpen, "Edit Investor Type");
    }
}