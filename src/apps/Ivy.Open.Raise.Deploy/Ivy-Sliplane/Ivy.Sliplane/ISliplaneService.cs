using Ivy.Sliplane.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ivy.Sliplane
{
    public interface ISliplaneService
    {
        Task<Project> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
        Task<List<Project>> ListProjectsAsync(CancellationToken cancellationToken = default);
        Task<Project> UpdateProjectAsync(string projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default);
        Task DeleteProjectAsync(string projectId, CancellationToken cancellationToken = default);

        Task<Service> CreateServiceAsync(string projectId, CreateServiceRequest request, CancellationToken cancellationToken = default);
        Task<List<Service>> ListServicesAsync(string projectId, CancellationToken cancellationToken = default);
        Task<Service> GetServiceAsync(string projectId, string serviceId, CancellationToken cancellationToken = default);
        Task<Service> UpdateServiceAsync(string projectId, string serviceId, UpdateServiceRequest request, CancellationToken cancellationToken = default);
        Task DeleteServiceAsync(string projectId, string serviceId, CancellationToken cancellationToken = default);

        Task DeployServiceAsync(string projectId, string serviceId, DeployServiceRequest? request = null, CancellationToken cancellationToken = default);

        Task<CustomDomain> AddCustomDomainAsync(string projectId, string serviceId, string domain, CancellationToken cancellationToken = default);
        Task RemoveCustomDomainAsync(string projectId, string serviceId, string domainId, CancellationToken cancellationToken = default);

        Task<ServiceLogsResponse> GetServiceLogsAsync(string projectId, string serviceId, int? limit = null, DateTime? since = null, CancellationToken cancellationToken = default);

        Task<Server> CreateServerAsync(CreateServerRequest request, CancellationToken cancellationToken = default);
        Task<List<Server>> ListServersAsync(CancellationToken cancellationToken = default);
        Task<Server> GetServerAsync(string serverId, CancellationToken cancellationToken = default);
        Task DeleteServerAsync(string serverId, CancellationToken cancellationToken = default);
        Task<Server> RescaleServerAsync(string serverId, RescaleServerRequest request, CancellationToken cancellationToken = default);

        Task<RegistryCredentials> CreateRegistryCredentialsAsync(CreateRegistryCredentialsRequest request, CancellationToken cancellationToken = default);
        Task<List<RegistryCredentials>> ListRegistryCredentialsAsync(CancellationToken cancellationToken = default);
        Task<RegistryCredentials> GetRegistryCredentialsAsync(string credentialsId, CancellationToken cancellationToken = default);
        Task<RegistryCredentials> UpdateRegistryCredentialsAsync(string credentialsId, UpdateRegistryCredentialsRequest request, CancellationToken cancellationToken = default);
        Task DeleteRegistryCredentialsAsync(string credentialsId, CancellationToken cancellationToken = default);
    }
}
