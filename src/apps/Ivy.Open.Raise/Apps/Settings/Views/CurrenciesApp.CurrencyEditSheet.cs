namespace Ivy.Open.Raise.Apps.Settings.Views;

public class CurrencyEditSheet(IState<bool> isOpen, RefreshToken refreshToken, string currencyId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<DataContextFactory>();
        var currency = UseState(() => factory.CreateDbContext().Currencies.FirstOrDefault(e => e.Id == currencyId)!);

        UseEffect(() =>
        {
            using var db = factory.CreateDbContext();
            db.Currencies.Update(currency.Value);
            db.SaveChanges();
            refreshToken.Refresh();
        }, [currency]);

        return currency
            .ToForm()
            .Builder(e => e.Symbol, e => e.ToTextAreaInput())
            .Builder(e => e.Name, e => e.ToTextAreaInput())
            .ToSheet(isOpen, "Edit Currency");
    }
}