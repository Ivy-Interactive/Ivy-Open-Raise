using Ivy.Open.Raise.Apps.Investors;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.DollarSign, path: ["Apps"], order: 3)]
public class InvestorsApp : ViewBase
{
    public override object? Build()
    {
        return UseBlades(() => new InvestorListBlade(), "Search");
    }
}