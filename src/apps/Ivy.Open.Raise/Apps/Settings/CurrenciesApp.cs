using Ivy.Open.Raise.Apps.Settings.Views;

namespace Ivy.Open.Raise.Apps.Settings;

[App(icon: Icons.Currency)]
public class CurrenciesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new CurrencyListBlade(), "Search");
    }
}