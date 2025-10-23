namespace Ivy.Open.Raise.Apps.Settings.Views;

public class CountryEditSheet(IState<bool> isOpen, RefreshToken refreshToken, int countryId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var country = UseState(() => factory.CreateDbContext().Countries.FirstOrDefault(e => e.Id == countryId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            db.Countries.Update(country.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [country]);

        return country
            .ToForm()
            .Builder(e => e.Name, e => e.ToTextAreaInput())
            .Builder(e => e.Iso, e => e.ToCodeInput())
            .ToSheet(isOpen, "Edit Country");
    }
}