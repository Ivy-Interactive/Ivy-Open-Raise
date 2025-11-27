using Ivy.Open.Raise.Apps.Settings.StartupStages;

namespace Ivy.Open.Raise.Apps.Settings;

[App(icon: Icons.Milestone, path:["Apps", "Settings"], isVisible:false)]
public class StartupStagesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new StartupStageListBlade(), "Search");
    }
}