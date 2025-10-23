using Ivy.Open.Raise.Apps.Settings.Views;

namespace Ivy.Open.Raise.Apps.Settings;

[App(icon: Icons.Globe)]
public class CountriesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new CountryListBlade(), "Search");
    }
}