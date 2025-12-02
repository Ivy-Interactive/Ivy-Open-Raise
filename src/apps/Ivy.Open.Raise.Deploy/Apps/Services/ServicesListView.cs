using Ivy.Sliplane;
using Ivy.Sliplane.Models;

namespace Ivy.Open.Raise.Deploy.Apps.Services;

public class ServicesListView : ViewBase
{
    private readonly IState<List<Service>> _services;
    private readonly IState<bool> _isLoading;
    private readonly Action _onRefresh;

    public ServicesListView(
        IState<List<Service>> services,
        IState<bool> isLoading,
        Action onRefresh)
    {
        _services = services;
        _isLoading = isLoading;
        _onRefresh = onRefresh;
    }

    public override object? Build()
    {
        // Filter services to show only running ones (Live status)
        var runningServices = _services.Value
            .Where(s => s.Status == ServiceStatus.Live)
            .Select(s => new ServiceViewModel
            {
                Name = s.Name ?? "Unknown",
                Status = s.Status.ToString(),
                ServerId = s.ServerId ?? "N/A",
                IsPublic = s.Network != null && s.Network.Public ? "Yes" : "No",
                Domain = s.Network != null ? (s.Network.ManagedDomain ?? s.Network.InternalDomain ?? "N/A") : "N/A"
            })
            .ToList();

        if (runningServices.Count > 0)
        {
            return runningServices.AsQueryable().ToDataTable()
                .Width(Size.Full())
                .Height(Size.Full());
        }

        return new Card(
            Layout.Vertical().Gap(2).Align(Align.Center).Padding(8)
                | new Icon(Icons.Container, Colors.Gray).Size(Size.Units(4))
                | Text.H3("No Running Services")
                | Text.Block($"No services with 'Live' status found in this project.")
                    .Color(Colors.Gray)
        );
    }
}

