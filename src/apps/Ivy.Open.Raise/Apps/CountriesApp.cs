using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.Globe, path: ["Settings"])]
public class CountriesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new CountryListBlade(), "Search");
    }
}