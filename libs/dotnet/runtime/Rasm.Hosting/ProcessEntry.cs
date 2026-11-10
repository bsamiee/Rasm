using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NLog.Extensions.Logging;
using NLog.Targets;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Rasm.Telemetry;
using static NLog.SetupBuilderExtensions;
using static NLog.SetupLoadConfigurationExtensions;

namespace Rasm.Hosting;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record ProcessServices(string Name, IConfigurationRoot Settings, ILoggerFactory Loggers, ActivitySource Activities);

[SmartEnum<string>]
public sealed partial class UserFolder {
    public static readonly UserFolder Log = new("LOG_DIR", LogFile.UserDirectory);
    public static readonly UserFolder Cache = new("CACHE_DIR", static name =>
        OperatingSystem.IsMacOS() ? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), name)
        : OperatingSystem.IsWindows() ? Path.Join(Local, name, "Cache")
        : Path.Join(Environment.GetEnvironmentVariable("XDG_CACHE_HOME") ?? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache"), name));
    public static readonly UserFolder State = new("STATE_DIR", static name => Path.Join(OperatingSystem.IsLinux() ? LinuxState : Local, name));
    public static readonly UserFolder Runtime = new("RUNTIME_DIR", static name => Path.Join(
        OperatingSystem.IsLinux() ? Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR") ?? Path.Join(Path.GetTempPath(), Environment.UserName) : Path.GetTempPath(), name));

    private static string Local => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private static string LinuxState =>
        Environment.GetEnvironmentVariable("XDG_STATE_HOME") ?? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "state");

    [UseDelegateFromConstructor]
    private partial string Default(string name);

    public string Of(IConfiguration settings, string name) => settings[Key] ?? Default(name);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

public static partial class ProcessEntry {

    // --- [BOUNDS]

    private const int ExportMilliseconds = 1000;

    // --- [HOST_SOURCES]

    private static readonly Seq<string> HostSources = ["System.Net.Http", "Microsoft.AspNetCore"];
    private static readonly Seq<string> HostMeters = ["System.Net.Http", "System.Net.NameResolution", "Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel"];

    // --- [ENTRY]

    public static int Run(Func<ProcessServices, IO<int>> program) {
        string name = AppDomain.CurrentDomain.FriendlyName;
        string? version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
        IConfigurationRoot settings = new ConfigurationBuilder().AddEnvironmentVariables(ProcessEnvironment.Key(Some(name), "")).Build();
        IConfigurationRoot configuration = new ConfigurationBuilder().AddEnvironmentVariables().AddConfiguration(settings).Build();
        using ActivitySource activities = new(name, version);
        using OpenTelemetrySdk sdk = OpenTelemetrySdk.Create(sdk => Exported(sdk, configuration, name, version, [], [])
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging(logging => Logged(logging, settings, name, version)));
        ILoggerFactory loggers = sdk.GetLoggerFactory();
        ILogger logger = loggers.CreateLogger(Assembly.GetEntryAssembly()?.EntryPoint?.DeclaringType ?? typeof(ProcessEntry));
        AppDomain.CurrentDomain.UnhandledException += (_, args) => {
            if (args.ExceptionObject is Exception exception) Unhandled(logger, exception);
            Stop([sdk.LoggerProvider], [sdk.TracerProvider], [sdk.MeterProvider]);
        };
        TaskScheduler.UnobservedTaskException += (_, args) => Unobserved(logger, args.Exception);
        using (activities.StartActivity($"{name}.{nameof(Run)}", ActivityKind.Internal, ProcessEnvironment.Extract(None))) {
            int code = program(new ProcessServices(name, settings, loggers, activities)).RunSafe().IfFail(error => {
                _ = logger.Failed(error, name);
                return 1;
            });
            Stop([sdk.LoggerProvider], [sdk.TracerProvider], [sdk.MeterProvider]);
            return code;
        }
    }

    extension(IHostApplicationBuilder builder) {
        public IHostApplicationBuilder AddTelemetry() {
            string name = builder.Environment.ApplicationName;
            string? version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
            _ = builder.Configuration.AddEnvironmentVariables(ProcessEnvironment.Key(Some(name), ""));
            Logged(builder.Logging.ClearProviders(), builder.Configuration, name, version);
            _ = Exported(builder.Services.AddOpenTelemetry(), builder.Configuration, name, version, HostSources, HostMeters).AddHostedService(static services => new Terminations(
                services.GetRequiredService<ILoggerFactory>(), services.GetServices<LoggerProvider>(), services.GetServices<TracerProvider>(), services.GetServices<MeterProvider>()));
            return builder;
        }
    }

    // --- [PARTS]

    private static IServiceCollection Exported(IOpenTelemetryBuilder sdk, IConfiguration configuration, string name, string? version, Seq<string> sources, Seq<string> meters) =>
        sdk.WithTracing(tracing => tracing.AddSource([name, .. sources]))
            .UseConfiguredExporters(
                configuration,
                SignalExporter.Known.Map(static exporter => exporter with {
                    Traces = exporter.Traces.Map(static traces => new Action<TracerProviderBuilder>(
                        tracing => traces(tracing.ConfigureServices(static services => services.AddLogging(static logging => logging.AddTraceBasedSampler()))))),
                }),
                resource => (bool.TryParse(configuration["DOTNET_RUNNING_IN_CONTAINER"], out bool contained) && contained ? resource.AddContainerDetector() : resource)
                    .AddService(name, serviceVersion: version)
                    .AddHostDetector().AddOperatingSystemDetector().AddProcessDetector().AddProcessRuntimeDetector())
            .Services
            .ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddMeter(["System.Runtime", .. meters]).AddProcessInstrumentation());

    private static void Logged(ILoggingBuilder logging, IConfiguration settings, string name, string? version) {
        _ = logging.AddJsonLines(settings, name, Optional(version), [name]);
        if (settings.GetSection("RandomProbabilisticSampler").Exists()) _ = logging.AddRandomProbabilisticSampler(settings);
        _ = logging.Services.AddSingleton<ILoggerProvider>(_ =>
            LogSettings.Read(settings, name, [name]).File.Target == LogTarget.Stderr
                ? NullLoggerProvider.Instance
                : new NLogLoggerProvider(
                    new NLogProviderOptions { CaptureEventId = EventIdCaptureType.None, IncludeScopes = false, ShutdownOnDispose = true },
                    new NLog.LogFactory { AutoShutdown = false }.Setup().LoadConfiguration(static terminal => terminal.ForLogger().FilterMinLevel(NLog.LogLevel.Warn)
                        .WriteTo(new ConsoleTarget { StdErr = true, Layout = "${message}${onexception:inner=\\: ${exception:format=Message}}" })).LogFactory));
    }

    private static void Stop(IEnumerable<LoggerProvider> logs, IEnumerable<TracerProvider> traces, IEnumerable<MeterProvider> metrics) =>
        Parallel.Invoke([
            .. logs.Select(static provider => new Action(() => _ = provider.Shutdown(ExportMilliseconds))),
            .. traces.Select(static provider => new Action(() => _ = provider.Shutdown(ExportMilliseconds))),
            .. metrics.Select(static provider => new Action(() => _ = provider.Shutdown(ExportMilliseconds))),
        ]);

    private sealed class Terminations(ILoggerFactory loggers, IEnumerable<LoggerProvider> logs, IEnumerable<TracerProvider> traces, IEnumerable<MeterProvider> metrics) : IHostedService {
        private readonly ILogger logger = loggers.CreateLogger<Terminations>();

        public Task StartAsync(CancellationToken cancellationToken) {
            AppDomain.CurrentDomain.UnhandledException += Crashed;
            TaskScheduler.UnobservedTaskException += Faulted;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            AppDomain.CurrentDomain.UnhandledException -= Crashed;
            TaskScheduler.UnobservedTaskException -= Faulted;
            Stop(logs, traces, metrics);
            return Task.CompletedTask;
        }

        private void Crashed(object sender, UnhandledExceptionEventArgs args) {
            if (args.ExceptionObject is Exception exception) Unhandled(logger, exception);
            Stop(logs, traces, metrics);
        }

        private void Faulted(object? sender, UnobservedTaskExceptionEventArgs args) => Unobserved(logger, args.Exception);
    }

    // --- [RECORDS]

    [LoggerMessage(Level = LogLevel.Critical, Message = "Unhandled exception")]
    private static partial void Unhandled(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Unobserved task exception")]
    private static partial void Unobserved(ILogger logger, Exception exception);
}
