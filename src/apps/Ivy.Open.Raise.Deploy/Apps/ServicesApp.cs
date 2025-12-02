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

        // State for services
        var selectedProjectId = UseState<string?>(() => null);
        var services = UseState<List<Service>>(() => []);
        var isLoading = UseState(false);
        var error = UseState<string?>(() => null);

        // Query function for async select
        async Task<Option<string>[]> QueryProjects(string query)
        {
            try
            {
                var projectsList = await sliplaneService.ListProjectsAsync();
                return projectsList
                    .Where(p => string.IsNullOrWhiteSpace(query) || 
                               (p.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) == true) ||
                               (p.Id?.Contains(query, StringComparison.OrdinalIgnoreCase) == true))
                    .Select(p => new Option<string>(p.Name ?? p.Id ?? "Unknown", p.Id ?? ""))
                    .ToArray();
            }
            catch (Exception ex)
            {
                error.Set($"Failed to load projects: {ex.Message}");
                client.Toast($"Error: {ex.Message}");
                return [];
            }
        }

        // Lookup function for async select
        async Task<Option<string>?> LookupProject(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            try
            {
                var projectsList = await sliplaneService.ListProjectsAsync();
                var project = projectsList.FirstOrDefault(p => p.Id == id);
                return project != null
                    ? new Option<string>(project.Name ?? project.Id ?? "Unknown", project.Id ?? "")
                    : null;
            }
            catch
            {
                return null;
            }
        }

        // Load services when project is selected
        UseEffect(async () =>
        {
            if (string.IsNullOrWhiteSpace(selectedProjectId.Value))
            {
                services.Set([]);
                return;
            }

            var projectId = selectedProjectId.Value.Trim();
            if (string.IsNullOrEmpty(projectId))
            {
                services.Set([]);
                return;
            }

            try
            {
                isLoading.Set(true);
                error.Set((string?)null);
                var servicesList = await sliplaneService.ListServicesAsync(projectId);
                services.Set(servicesList);
            }
            catch (Ivy.Sliplane.SliplaneApiException apiEx)
            {
                var errorMsg = apiEx.StatusCode == System.Net.HttpStatusCode.NotFound
                    ? $"Project '{projectId}' not found. {apiEx.Message}"
                    : $"Failed to load services: {apiEx.Message}";
                error.Set(errorMsg);
                client.Toast(errorMsg);
                services.Set([]);
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

        return Layout.Vertical().Padding(4).Gap(4)
            | Layout.Horizontal().Gap(4)
                | Text.H2("Running Services")
                | new Button("Refresh", RefreshServices)
                    .Variant(ButtonVariant.Secondary)
                    .Disabled(isLoading.Value)

            | (error.Value != null
                ? Text.Block(error.Value).Color(Colors.Red)
                : null)

            | new Field(
                selectedProjectId.ToAsyncSelectInput(QueryProjects, LookupProject, placeholder: "Select a project")
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

