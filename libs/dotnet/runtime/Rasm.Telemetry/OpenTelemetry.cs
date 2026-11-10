using System.Collections.Generic;
using System.Diagnostics;
using LanguageExt.ClassInstances;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Rasm.Telemetry;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record SignalExporter(Option<Action<TracerProviderBuilder>> Traces, Option<Action<MeterProviderBuilder>> Metrics, Option<Action<LoggerProviderBuilder>> Logs) {
    public static SignalExporter Otlp { get; } = new(
        Some<Action<TracerProviderBuilder>>(static tracing => tracing.AddOtlpExporter()),
        Some<Action<MeterProviderBuilder>>(static metrics => metrics.AddOtlpExporter()),
        Some<Action<LoggerProviderBuilder>>(static logging => logging.AddOtlpExporter()));

    public static HashMap<EqStringOrdinalIgnoreCase, string, SignalExporter> Known { get; } = HashMap<EqStringOrdinalIgnoreCase, string, SignalExporter>(("otlp", Otlp));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class ConfiguredExporterExtensions {
    private const string NoExporter = "none";

    private static readonly Seq<(string Variable, Func<SignalExporter, Option<Func<IOpenTelemetryBuilder, IOpenTelemetryBuilder>>> Exported, Func<IOpenTelemetryBuilder, IOpenTelemetryBuilder> Unexported)> Signals = [
        ("OTEL_TRACES_EXPORTER", static exporter => exporter.Traces.Map(static traces => fun((IOpenTelemetryBuilder sdk) => sdk.WithTracing(traces))),
            static sdk => sdk.WithTracing(static tracing => tracing.SetSampler(new AlwaysOffSampler()))),
        ("OTEL_METRICS_EXPORTER", static exporter => exporter.Metrics.Map(static metrics => fun((IOpenTelemetryBuilder sdk) => sdk.WithMetrics(metrics))), identity),
        ("OTEL_LOGS_EXPORTER", static exporter => exporter.Logs.Map(static logs => fun((IOpenTelemetryBuilder sdk) => sdk.WithLogging(logs))), identity),
    ];

    extension(IOpenTelemetryBuilder builder) {
        public IOpenTelemetryBuilder UseConfiguredExporters(IConfiguration configuration, HashMap<EqStringOrdinalIgnoreCase, string, SignalExporter> exporters, Action<ResourceBuilder> resource) {
            Seq<(string Variable, string Name, Option<Func<IOpenTelemetryBuilder, IOpenTelemetryBuilder>> Configure, string Accepted)> named = (
                from signal in Signals
                from names in Optional(configuration[signal.Variable]).ToSeq()
                from name in toSeq(names.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase))
                where !string.Equals(name, NoExporter, StringComparison.OrdinalIgnoreCase)
                select (signal.Variable, name, exporters.Find(name).Bind(signal.Exported),
                    string.Join(", ", exporters.Filter(exporter => signal.Exported(exporter).IsSome).Keys.Add(NoExporter)))).Strict();
            _ = named.Filter(static row => row.Configure.IsNone).Iter(row => builder.Services.AddSingleton(new SettingRejection(row.Variable, row.Name, row.Accepted)));
            Seq<Func<IOpenTelemetryBuilder, IOpenTelemetryBuilder>> exported = named.Choose(static row => row.Configure);
            Seq<Func<IOpenTelemetryBuilder, IOpenTelemetryBuilder>> unexported =
                Signals.Filter(signal => !named.Exists(row => row.Configure.IsSome && string.Equals(row.Variable, signal.Variable, StringComparison.Ordinal))).Map(static signal => signal.Unexported);
            return (exported + unexported).Fold(exported.IsEmpty ? builder : builder.ConfigureResource(resource), static (sdk, configure) => configure(sdk));
        }
    }
}

public static class ProcessEnvironment {
    private static readonly CompositeTextMapPropagator Propagator = new([new TraceContextPropagator(), new BaggagePropagator()]);

    public static Unit Inject(IDictionary<string, string?> environment, Option<string> program) {
        Propagator.Inject(new PropagationContext(Activity.Current?.Context ?? default, Baggage.Current), environment, (carrier, field, value) => carrier[Key(program, field)] = value);
        return unit;
    }

    public static ActivityContext Extract(Option<string> program) {
        PropagationContext parent = Propagator.Extract(default, program, static (held, field) => Optional(Environment.GetEnvironmentVariable(Key(held, field))).ToSeq());
        Baggage.Current = parent.Baggage;
        return parent.ActivityContext;
    }

    public static string Key(Option<string> program, string name) =>
        string.Concat(program.Match(Some: held => $"{held}_{name}", None: () => name).Select(static letter => char.IsAsciiLetterOrDigit(letter) ? char.ToUpperInvariant(letter) : '_'));
}
