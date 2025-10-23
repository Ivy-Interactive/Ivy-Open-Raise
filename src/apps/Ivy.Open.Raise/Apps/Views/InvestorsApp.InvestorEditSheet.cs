namespace Ivy.Open.Raise.Apps.Views;

public class InvestorEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid investorId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var investor = UseState(() => factory.CreateDbContext().Investors.FirstOrDefault(e => e.Id == investorId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                investor.Value.UpdatedAt = DateTime.UtcNow;
                db.Investors.Update(investor.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [investor]);

        return investor
            .ToForm()
            .Builder(e => e.Name, e => e.ToTextAreaInput())
            .Builder(e => e.WebsiteUrl, e => e.ToUrlInput())
            .Builder(e => e.LinkedinUrl, e => e.ToUrlInput())
            .Builder(e => e.XUrl, e => e.ToUrlInput())
            .Builder(e => e.AddressStreet, e => e.ToTextAreaInput())
            .Builder(e => e.AddressZip, e => e.ToTextAreaInput())
            .Builder(e => e.AddressCity, e => e.ToTextAreaInput())
            .Builder(e => e.Thesis, e => e.ToTextAreaInput())
            .Builder(e => e.CheckSizeMin, e => e.ToFeedbackInput())
            .Builder(e => e.CheckSizeMax, e => e.ToFeedbackInput())
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt)
            .ToSheet(isOpen, "Edit Investor");
    }
}