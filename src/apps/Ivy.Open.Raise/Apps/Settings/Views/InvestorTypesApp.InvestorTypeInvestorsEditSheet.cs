namespace Ivy.Open.Raise.Apps.Settings.Views;

public class InvestorTypeInvestorsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid investorId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var investor = UseState(() => factory.CreateDbContext().Investors.FirstOrDefault(e => e.Id == investorId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            investor.Value.UpdatedAt = DateTime.UtcNow;
            db.Investors.Update(investor.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [investor]);

        return investor
            .ToForm()
            .Builder(e => e.WebsiteUrl, e => e.ToUrlInput())
            .Builder(e => e.LinkedinUrl, e => e.ToUrlInput())
            .Builder(e => e.XUrl, e => e.ToUrlInput())
            .Builder(e => e.CheckSizeMin, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.CheckSizeMax, e => e.ToMoneyInput().Currency("USD"))
            .Group("Address", e => e.AddressStreet, e => e.AddressZip, e => e.AddressCity, e => e.AddressCountryId)
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.DeletedAt, e => e.InvestorTypeId)
            .ToSheet(isOpen, "Edit Investor");
    }
}