using Ivy.Sliplane.Console.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Ivy.Sliplane.Console.Commands.Service
{
    public class ServiceLogsCommand : BaseCommand<ServiceLogsCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Project ID")]
            [CommandOption("-p|--project")]
            public string ProjectId { get; set; } = string.Empty;

            [Description("Service ID")]
            [CommandOption("--id")]
            public string ServiceId { get; set; } = string.Empty;

            [Description("Number of log lines to retrieve")]
            [CommandOption("-l|--limit")]
            public int? Limit { get; set; }

            [Description("Show logs since timestamp (ISO 8601 format)")]
            [CommandOption("--since")]
            public string? Since { get; set; }

            [Description("Follow log output (live tail)")]
            [CommandOption("-f|--follow")]
            public bool Follow { get; set; }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(settings.ProjectId))
                {
                    ShowError("Project ID is required");
                    return 1;
                }

                if (string.IsNullOrWhiteSpace(settings.ServiceId))
                {
                    ShowError("Service ID is required");
                    return 1;
                }

                DateTime? sinceDateTime = null;
                if (!string.IsNullOrWhiteSpace(settings.Since))
                {
                    if (!DateTime.TryParse(settings.Since, out var parsed))
                    {
                        ShowError("Invalid date format for --since. Use ISO 8601 format (e.g., 2024-01-01T00:00:00Z)");
                        return 1;
                    }
                    sinceDateTime = parsed;
                }

                var service = GetSliplaneService(settings);

                if (settings.Follow)
                {
                    ShowInfo("Following logs... Press Ctrl+C to stop.");
                    
                    var cts = new CancellationTokenSource();
                    System.Console.CancelKeyPress += (sender, e) =>
                    {
                        e.Cancel = true;
                        cts.Cancel();
                    };

                    var lastTimestamp = sinceDateTime ?? DateTime.UtcNow.AddMinutes(-1);
                    
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var logs = await service.GetServiceLogsAsync(
                                settings.ProjectId, 
                                settings.ServiceId, 
                                settings.Limit ?? 100, 
                                lastTimestamp,
                                cts.Token);

                            if (logs?.Logs != null && logs.Logs.Count > 0)
                            {
                                foreach (var log in logs.Logs)
                                {
                                    WriteLogLine(log);
                                    if (log.Timestamp > lastTimestamp)
                                    {
                                        lastTimestamp = log.Timestamp;
                                    }
                                }
                            }

                            await Task.Delay(2000, cts.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    var logs = await service.GetServiceLogsAsync(
                        settings.ProjectId, 
                        settings.ServiceId, 
                        settings.Limit, 
                        sinceDateTime);

                    if (settings.Output == OutputFormat.Table)
                    {
                        if (logs?.Logs != null)
                        {
                            foreach (var log in logs.Logs)
                            {
                                WriteLogLine(log);
                            }
                        }
                    }
                    else
                    {
                        WriteOutput(logs, settings.Output);
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                return 1;
            }
        }

        private void WriteLogLine(Models.ServiceLog log)
        {
            var timestamp = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            var level = log.Level?.ToUpper() ?? "INFO";
            
            var color = level switch
            {
                "ERROR" => "red",
                "WARN" => "yellow",
                "DEBUG" => "grey",
                _ => "white"
            };

            AnsiConsole.MarkupLine($"[grey]{timestamp}[/] [[{color}]{level}[/]] {Markup.Escape(log.Message ?? "")}");
        }
    }
}
