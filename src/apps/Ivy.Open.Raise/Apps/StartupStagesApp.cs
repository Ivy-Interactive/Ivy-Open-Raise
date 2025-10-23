using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.Milestone, path: ["Apps"])]
public class StartupStagesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new StartupStageListBlade(), "Search");
    }
}