using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Threading;
using OpenTelemetry;
using OpenTelemetry.Resources;
using Rasm.Rhino.Persistence;
using Rasm.Telemetry;
using Rhino;
using Rhino.PlugIns;
using Rhino.Runtime;

namespace Rasm.Rhino;

// --- [SERVICES] ------------------------------------------------------------------------

public static class Session {

    // --- [STATE]

    private sealed record Bound(Either<Skin, PlugIn> Host, Assembly Assembly, ActivitySource Activities, Lazy<Fin<ILogger>> Logger, EnvIO Scope, JoinableTaskCollection Pending);

    private static readonly Atom<Option<JoinableTaskContext>> UiThread = Atom(Option<JoinableTaskContext>.None);

    private static IO<JoinableTaskContext> Threading =>
        IO.liftAsync(static environment => UiThread.Value.IsSome || !RhinoApp.InvokeRequired
                ? Task.FromResult(Joined())
                : Eto.Forms.Application.Instance.InvokeAsync(Joined).WaitAsync(environment.Token))
            .Bind(static joined => IO.lift(joined.ToFin()));

    private static Option<JoinableTaskContext> Joined() => UiThread.Swap(static held => held.IsSome ? held : new JoinableTaskContext());

    private static readonly Ref<Option<Bound>> State = Ref(Option<Bound>.None);

    public static PlugIn PlugIn => Read(static bound => bound.Host.IfLeft(static skin => throw new InvalidOperationException($"{skin.GetType().FullName} binds a skin session, not a plug-in")));

    public static Assembly Assembly => Read(static bound => bound.Assembly);

    public static ActivitySource Activities => Read(static bound => bound.Activities);

    public static ILogger Logger => Read(static bound => bound.Logger.Value.ThrowIfFail());

    public static EnvIO Scope => Read(static bound => bound.Scope);

    private static T Read<T>(Func<Bound, T> read) =>
        State.Value.Map(read).IfNone(static () => throw new InvalidOperationException($"{typeof(Session).Assembly.GetName().Name} holds no bound session"));

    // --- [LIFECYCLE]

    public static IO<Unit> Bind(PlugIn plugIn, Type definition) => Opened(Right(plugIn), definition);

    public static IO<Unit> Bind(Skin skin, Type definition) => Opened(Left(skin), definition);

    public static IO<Unit> Close =>
        (from taken in atomicIO(static () => {
            Option<Bound> bound = State.Value;
            State.Value = None;
            return bound;
        })
         from closed in taken.Traverse(static bound => Products.Each(Seq(
             IO.liftAsync(() => bound.Scope.Source.CancelAsync().ToUnit()),
             IO.liftAsync(() => bound.Pending.JoinTillEmptyAsync(CancellationToken.None).ToUnit()),
             IO.lift(bound.Scope.DisposeResources),
             IO.lift(bound.Scope.DisposeNonResources),
             IO.lift(bound.Activities.Dispose))).Map(unit)).As()
         select unit).Uninterruptible();

    private static IO<Unit> Opened(Either<Skin, PlugIn> host, Type category) =>
        from threading in Threading
        from name in IO.lift(Conversions.Required(category.Assembly.GetName().Name))
        from fresh in IO.lift(() => (Activities: new ActivitySource(name, category.Assembly.GetName().Version?.ToString()), Scope: EnvIO.New(token: CancellationToken.None)) switch {
            var held => new Bound(
                host, category.Assembly, held.Activities,
                new(() => RunSafe(Composed(threading, host, category, held.Activities).Scoped(held.Scope), CancellationToken.None)),
                held.Scope, threading.CreateCollection()),
        })
        from bound in atomicIO(() => State.Swap(prior => prior | Some(fresh)))
        from taken in unless(bound.Exists(held => ReferenceEquals(held, fresh)), IO.lift(() => {
            fresh.Scope.Dispose();
            fresh.Activities.Dispose();
        }) >> IO.fail<Unit>(new RhinoFault(category, RhinoReason.Taken))).As()
        select unit;

    private static IO<ILogger> Composed(JoinableTaskContext threading, Either<Skin, PlugIn> host, Type category, ActivitySource activities) =>
        from settings in use(() => new ConfigurationRoot([
            new PersistentSettingsConfigurationProvider(() => host.Match(Left: static skin => skin.Settings, Right: static plugIn => plugIn.Settings)),
            new EnvironmentVariablesConfigurationProvider(ProcessEnvironment.Key(Some(activities.Name), "")),
        ]))
        from configuration in use(() => new ConfigurationRoot([
            new EnvironmentVariablesConfigurationProvider(),
            new ChainedConfigurationProvider(new ChainedConfigurationSource { Configuration = settings }),
        ]))
        from reloaded in host.Match(Left: static _ => SettingsNode.Application, Right: static plugIn => new SettingsNode(new SettingsRoot.PlugInNode(plugIn.Id))).Saved
            .Attach(_ => IO.lift(settings.Reload).OnUiThread())
        from sdk in use(() => OpenTelemetrySdk.Create(sdk => sdk
            .WithTracing(tracing => tracing.AddSource(activities.Name))
            .UseConfiguredExporters(configuration, RhinoApp.IsInternetAccessAllowed ? SignalExporter.Known : [], resource => resource
                .AddService(activities.Name, serviceVersion: activities.Version)
                .AddOperatingSystemDetector().AddProcessDetector().AddProcessRuntimeDetector())
            .Services
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging(builder => builder.AddProvider(new CommandWindowLoggerProvider(threading)).AddJsonLines(settings, activities.Name, Optional(activities.Version), [activities.Name]))))
        select sdk.GetLoggerFactory().CreateLogger(category);

    // --- [EXECUTION]

    public static Fin<T> RunSafe<T>(IO<T> effect, CancellationToken token) =>
        Threading.RunSafe().Bind(threading => threading.Factory.Run(() => RunSafeAsync(effect, token)));

    public static Task<Fin<T>> RunSafeAsync<T>(IO<T> effect, CancellationToken token, IDisposable? owned = null) =>
        IO.liftVAsync(async _ => {
            using (owned)
                return await effect.RunAsync(token).ConfigureAwait(false);
        }).RunSafeAsync().AsTask();

    public static T Run<T>(IO<T> effect, T failed, [CallerMemberName] string member = "") =>
        Read(bound => RunSafe(effect, CancellationToken.None).IfFail(error => {
            _ = bound.Logger.Value.IfSucc(logger => logger.Failed(error, member));
            return failed;
        }));

    public static Unit Run<T>(IO<T> effect, [CallerMemberName] string member = "") => Run(effect.Map(unit), unit, member);

    public static T Operation<T>(string name, IO<T> effect, T failed) =>
        Read(bound => {
            using Activity? activity = bound.Activities.StartActivity(name);
            return Run(effect, failed, name);
        });

    public static Unit Start<T>(IO<T> effect, [CallerMemberName] string member = "", IDisposable? owned = null) => ignore(StartAsync(effect, member, owned));

    public static Task<Fin<T>> StartAsync<T>(IO<T> effect, [CallerMemberName] string member = "", IDisposable? owned = null) =>
        Read(bound => bound.Pending.Context.CreateFactory(bound.Pending).RunAsync(async () => {
            Fin<T> answer = await RunSafeAsync(effect, bound.Scope.Token, owned).ConfigureAwait(false);
            _ = answer.IfFail(error => bound.Logger.Value.IfSucc(logger => logger.Failed(error, member)));
            return answer;
        }).Task);

    public static Task<Fin<T>> StartOperationAsync<T>(string name, IO<T> effect, IDisposable? owned = null, ActivityKind kind = ActivityKind.Internal, ActivityContext parent = default) =>
        Read(bound => bound.Pending.Context.CreateFactory(bound.Pending).RunAsync(async () => {
            using Activity? activity = bound.Activities.StartActivity(name, kind, parent);
            return await StartAsync(effect, name, owned).ConfigureAwait(false);
        }).Task);

    public static Unit Failed(Error error, [CallerMemberName] string member = "") => Read(bound => bound.Logger.Value.IfSucc(held => held.Failed(error, member)));

    // --- [THREADS]

    extension<T>(IO<T> effect) {
        public IO<T> OnUiThread() =>
            Threading.Bind(threading => IO.liftVAsync(async environment => {
                await threading.Factory.SwitchToMainThreadAsync(environment.Token);
                return await effect.RunAsync(environment).ConfigureAwait(false);
            }));

        public IO<T> Scoped(EnvIO scope) => IO.liftVAsync(_ => effect.RunAsync(scope));

        public IO<T> Scoped(Resources resources) => IO.liftVAsync(async environment => {
            using EnvIO scope = EnvIO.New(resources, environment.Token, environment.Source, environment.SyncContext);
            return await effect.RunAsync(scope).ConfigureAwait(false);
        });

        public IO<T> Scoped() => IO.liftVAsync(_ => effect.RunAsync(Scope));
    }
}

[ProviderAlias("CommandWindow")]
internal sealed class CommandWindowLoggerProvider : ILoggerProvider {

    // --- [STATE]

    private const int Pending = 256;

    private readonly Channel<string> lines = Channel.CreateBounded<string>(new BoundedChannelOptions(Pending) { SingleReader = true, FullMode = BoundedChannelFullMode.DropOldest });
    private readonly JoinableTask drain;

    public CommandWindowLoggerProvider(JoinableTaskContext threading) =>
        drain = threading.Factory.RunAsync(() => Source.lift(lines)
            .ReduceIO(unit, static (_, line) => IO.lift(() => RhinoApp.WriteLine(line)).OnUiThread().Map(static _ => Reduced.Continue(unit)))
            .RunAsync(CancellationToken.None)
            .AsTask());

    // --- [LOGGERS]

    public ILogger CreateLogger(string categoryName) => new CommandWindowLogger(categoryName, lines.Writer);

    // --- [RELEASE]

    public void Dispose() {
        _ = lines.Writer.TryComplete();
        drain.Join();
    }
}

internal sealed class CommandWindowLogger(string category, ChannelWriter<string> lines) : ILogger {
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel is >= LogLevel.Warning and not LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        if (IsEnabled(logLevel)) _ = lines.TryWrite(exception is null ? $"{category}: {formatter(state, exception)}" : $"{category}: {formatter(state, exception)}: {exception.Message}");
    }
}
