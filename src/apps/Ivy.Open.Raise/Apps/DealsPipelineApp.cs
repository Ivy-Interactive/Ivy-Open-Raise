using Ivy.Open.Raise.Apps.Views;

namespace Ivy.Open.Raise.Apps;

[App(icon: Icons.Kanban, path: ["Apps"], searchHints: ["pipeline", "kanban", "board"])]
public class DealsPipelineApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new DealPipelineBlade(), "Deal Pipeline");
    }
}
