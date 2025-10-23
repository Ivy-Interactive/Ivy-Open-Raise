namespace Ivy.Open.Raise.Apps.Settings.Views;

public class CountryInvestorsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, Guid investorId) : ViewBase
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
            .Place(e => e.Name, e => e.Thesis)
            .Place(true, e => e.CheckSizeMin, e => e.CheckSizeMax)
            .Group("Address", e => e.AddressStreet, e => e.AddressZip, e => e.AddressCity)
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt, e => e.AddressCountryId, e => e.DeletedAt)
            .ToSheet(isOpen, "Edit Investor");
    }
}