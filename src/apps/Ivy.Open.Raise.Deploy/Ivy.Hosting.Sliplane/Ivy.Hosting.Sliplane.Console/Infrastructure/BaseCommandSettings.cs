using Spectre.Console.Cli;
using System.ComponentModel;

namespace Ivy.Hosting.Sliplane.Console.Infrastructure
{
    public abstract class BaseCommandSettings : CommandSettings
    {
        [Description("Override the API key from user secrets")]
        [CommandOption("--api-key")]
        public string? ApiKey { get; set; }

        [Description("Override the organization ID from user secrets")]
        [CommandOption("--org-id")]
        public string? OrganizationId { get; set; }

        [Description("Output format (json, table, yaml)")]
        [CommandOption("-o|--output")]
        [DefaultValue(OutputFormat.Table)]
        public OutputFormat Output { get; set; } = OutputFormat.Table;

        [Description("Enable verbose output")]
        [CommandOption("-v|--verbose")]
        public bool Verbose { get; set; }
    }

    public enum OutputFormat
    {
        Table,
        Json,
        Yaml
    }
}