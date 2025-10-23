using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.Currency, path: ["Settings"])]
public class CurrenciesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new CurrencyListBlade(), "Search");
    }
}