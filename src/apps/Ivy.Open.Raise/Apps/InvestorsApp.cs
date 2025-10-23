using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.User, path: ["Apps"])]
public class InvestorsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new InvestorListBlade(), "Search");
    }
}