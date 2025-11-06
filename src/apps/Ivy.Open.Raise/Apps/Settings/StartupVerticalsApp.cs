using Ivy.Open.Raise.Apps.Settings.Views;

namespace Ivy.Open.Raise.Apps.Settings;

[App(icon: Icons.Briefcase, path:["Apps", "Settings"], title: "Verticals", isVisible:false)]
public class StartupVerticalsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new StartupVerticalListBlade(), "Search");
    }
}