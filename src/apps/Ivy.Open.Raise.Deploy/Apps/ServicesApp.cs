using Ivy.Open.Raise.Deploy.Apps.Services;
using Ivy.Sliplane;
using Ivy.Sliplane.Models;

namespace Ivy.Open.Raise.Deploy.Apps;

[App(icon: Icons.Container, title: "Running Services")]
public class ServicesApp : ViewBase
{
    public override object? Build()
    {
        var sliplaneService = UseService<ISliplaneService>();
        var client = UseService<IClientProvider>();

        // State for projects and services
        var projects = UseState<List<Project>>(() => []);
        var selectedProjectId = UseState<string?>(() => null);
        var services = UseState<List<Service>>(() => []);
        var isLoading = UseState(false);
        var error = UseState<string?>(() => null);

        // Load projects on mount
        UseEffect(async () =>
        {
            try
            {
                isLoading.Set(true);
                error.Set((string?)null);
                var projectsList = await sliplaneService.ListProjectsAsync();
                projects.Set(projectsList);
                
                // Auto-select first project if available
                if (projectsList.Count > 0 && selectedProjectId.Value == null)
                {
                    selectedProjectId.Set(projectsList[0].Id);
                }
            }
            catch (Exception ex)
            {
                error.Set($"Failed to load projects: {ex.Message}");
                client.Toast($"Error: {ex.Message}");
            }
            finally
            {
                isLoading.Set(false);
            }
        }, []);

        // Load services when project is selected
        UseEffect(async () =>
        {
            if (string.IsNullOrWhiteSpace(selectedProjectId.Value))
            {
                services.Set([]);
                return;
            }

            try
            {
                isLoading.Set(true);
                error.Set((string?)null);
                var servicesList = await sliplaneService.ListServicesAsync(selectedProjectId.Value!);
                services.Set(servicesList);
            }
            catch (Exception ex)
            {
                error.Set($"Failed to load services: {ex.Message}");
                client.Toast($"Error: {ex.Message}");
                services.Set([]);
            }
            finally
            {
                isLoading.Set(false);
            }
        }, selectedProjectId);

        // Refresh handler
        void RefreshServices()
        {
            if (!string.IsNullOrWhiteSpace(selectedProjectId.Value))
            {
                // Trigger reload by updating the dependency
                selectedProjectId.Set(selectedProjectId.Value);
            }
        }

        // Project options for dropdown
        var projectOptions = projects.Value
            .Select(p => new Option<string>(p.Id ?? "", p.Name ?? p.Id ?? "Unknown"))
            .ToList();

        return Layout.Vertical().Padding(4).Gap(4)
            | Layout.Horizontal().Gap(4)
                | Text.H2("Running Services")
                | new Button("Refresh", RefreshServices)
                    .Variant(ButtonVariant.Secondary)
                    .Disabled(isLoading.Value)

            | (error.Value != null
                ? Text.Block(error.Value).Color(Colors.Red)
                : null)

            | (isLoading.Value && projects.Value.Count == 0
                ? Layout.Vertical().Gap(2).Align(Align.Center).Padding(8)
                    | Text.Block("Loading projects...")
                : null)

            | new Field(
                selectedProjectId.ToSelectInput(projectOptions)
                    .Placeholder("Select a project")
                    .Disabled(isLoading.Value)
            )
            .Label("Project")

            | (isLoading.Value && selectedProjectId.Value != null
                ? Layout.Vertical().Gap(2).Align(Align.Center).Padding(8)
                    | Text.Block("Loading services...")
                : null)

            | (selectedProjectId.Value != null && !isLoading.Value
                ? new ServicesListView(services, isLoading, RefreshServices)
                : null)

            | (selectedProjectId.Value == null && !isLoading.Value
                ? new Card(
                    Layout.Vertical().Gap(2).Align(Align.Center).Padding(8)
                        | new Icon(Icons.Folder, Colors.Gray).Size(Size.Units(4))
                        | Text.H3("Select a Project")
                        | Text.Block("Please select a project to view its running services.")
                            .Color(Colors.Gray)
                )
                : null);
    }
}

