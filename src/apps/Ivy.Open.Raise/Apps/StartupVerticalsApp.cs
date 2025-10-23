using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.Briefcase, path: ["Apps"])]
public class StartupVerticalsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new StartupVerticalListBlade(), "Search");
    }
}