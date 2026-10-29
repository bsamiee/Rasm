using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using Microsoft.VisualStudio.Threading;
using Rhino;
using Rhino.PlugIns;
using Rhino.Runtime;

namespace Rasm.Rhino;

// --- [SERVICES] ------------------------------------------------------------------------
public static partial class Session {
    // --- [STATE]
    private sealed record Bound(Either<Skin, PlugIn> Host, Assembly Assembly, ILoggerFactory Loggers, ILogger Logger, EnvIO Scope, JoinableTaskCollection Pending);

    internal static readonly JoinableTaskContext Threading = Eto.Forms.Application.Instance.Invoke(static () =>
        Eto.Forms.Application.Instance.Properties.Create(typeof(JoinableTaskContext), static () => new JoinableTaskContext()));

    private static readonly Atom<Option<Bound>> State = Atom(Option<Bound>.None);

    public static PlugIn PlugIn => Read(static bound => bound.Host.IfLeft(static skin => throw new InvalidOperationException($"{skin.GetType().FullName} binds a skin session, not a plug-in")));

    public static Assembly Assembly => Read(static bound => bound.Assembly);

    public static ILogger Logger => Read(static bound => bound.Logger);

    public static EnvIO Scope => Read(static bound => bound.Scope);

    private static T Read<T>(Func<Bound, T> read) =>
        State.Value.Map(read).IfNone(static () => throw new InvalidOperationException($"{typeof(Session).Assembly.GetName().Name} holds no bound session"));

    // --- [LIFETIME]
    public static IO<Unit> Bind(PlugIn plugIn, Seq<Action<ILoggingBuilder>> logging) => Opened(Right(plugIn), plugIn.GetType(), logging);

    public static IO<Unit> Bind(Skin skin, Seq<Action<ILoggingBuilder>> logging) => Opened(Left(skin), skin.GetType(), logging);

    public static IO<Unit> Close =>
        from taken in IO.lift(static () => State.Swap(static _ => None))
        from closed in taken.Traverse(static bound =>
            from cancelled in IO.liftAsync(() => bound.Scope.Source.CancelAsync().ToUnit())
            from drained in IO.liftAsync(environment => bound.Pending.JoinTillEmptyAsync(environment.Token).ToUnit())
            from scope in IO.lift(bound.Scope.Dispose)
            from loggers in IO.lift(bound.Loggers.Dispose)
            select unit).As()
        select unit;

    private static IO<Unit> Opened(Either<Skin, PlugIn> host, Type category, Seq<Action<ILoggingBuilder>> logging) =>
        from fresh in IO.lift(() => LoggerFactory.Create(builder => (Seq<Action<ILoggingBuilder>>(static window => window.AddProvider(new CommandWindowLoggerProvider())) + logging).Iter(row => row(builder))) switch {
            var loggers => new Bound(host, category.Assembly, loggers, loggers.CreateLogger(category), EnvIO.New(token: CancellationToken.None), Threading.CreateCollection()),
        })
        from bound in IO.lift(() => State.Swap(prior => prior | Some(fresh)))
        from taken in unless(bound.Exists(held => ReferenceEquals(held, fresh)), IO.lift(() => {
            fresh.Scope.Dispose();
            fresh.Loggers.Dispose();
        }) >> IO.fail<Unit>(new RhinoFault(category.FullName ?? category.Name, RhinoReason.Taken))).As()
        select unit;

    // --- [RUNS]
    public static Fin<T> RunSafe<T>(IO<T> effect, CancellationToken token) => Threading.Factory.Run(() => RunSafeAsync(effect, token));

    public static async Task<Fin<T>> RunSafeAsync<T>(IO<T> effect, CancellationToken token) {
        using EnvIO environment = EnvIO.New(token: token, syncContext: SynchronizationContext.Current);
        Task<T> run = effect.RunAsync(environment).AsTask();
        _ = await run.ContinueWith(static _ => unit, TaskScheduler.Default).ConfigureAwait(false);
        return run.IsCompletedSuccessfully ? await run.ConfigureAwait(false) : run.Exception is { } fault ? Error.New(fault.InnerExceptions is [var single] ? single : fault) : Errors.Cancelled;
    }

    public static T Run<T>(IO<T> effect, T failed, [CallerMemberName] string member = "") =>
        RunSafe(effect, CancellationToken.None).IfFail(error => {
            _ = Failed(error, member);
            return failed;
        });

    public static Unit Run<T>(IO<T> effect, [CallerMemberName] string member = "") => Run(effect.Map(unit), unit, member);

    public static Unit Start<T>(IO<T> effect, [CallerMemberName] string member = "") =>
        Read(bound => {
            bound.Pending.Add(Threading.Factory.RunAsync(async () => {
                using EnvIO environment = EnvIO.New(token: bound.Scope.Token);
                _ = await effect.Map(unit).IfFail(error => IO.lift(() => Failed(error, member))).RunAsync(environment).ConfigureAwait(false);
            }));
            return unit;
        });

    // --- [THREADS]
    extension<T>(IO<T> effect) {
        public IO<T> OnUiThread() =>
            IO.liftVAsync(async environment => {
                await Threading.Factory.SwitchToMainThreadAsync(environment.Token);
                return await effect.RunAsync(environment).ConfigureAwait(false);
            });

        public IO<T> Scoped(EnvIO scope) => IO.liftVAsync(_ => effect.RunAsync(scope));
    }

    // --- [LOGS]
    public static Unit Failed(Error error, [CallerMemberName] string member = "") {
        if (Cancellation(error)) Cancelled(Logger, member);
        else if (Logger.IsEnabled(LogLevel.Error)) Failed(Logger, member, Messages(error), Causes(error).Choose(static cause => cause.Exception) switch { [] => null, [var single] => single, var many => new AggregateException(many) });
        return unit;
    }

    public static string Messages(Error error) => string.Join(Environment.NewLine, Causes(error).Map(static cause => cause.Message));

    public static bool Cancellation(Error error) => error.Count is 1 && error.Is(Errors.Cancelled);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Member} failed with {Message}", SkipEnabledCheck = true)]
    private static partial void Failed(ILogger logger, string member, string message, Exception? exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Member} canceled")]
    private static partial void Cancelled(ILogger logger, string member);

    private static Seq<Error> Causes(Error error) => error.FoldM(static leaf => leaf.Cons(leaf.Inner.ToSeq().Bind(Causes))).As();
}

[ProviderAlias("CommandWindow")]
internal sealed class CommandWindowLoggerProvider : ILoggerProvider, ISupportExternalScope {
    // --- [STATE]
    private readonly Channel<string> lines = Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleReader = true });
    private readonly Atom<IExternalScopeProvider> scopes = Atom<IExternalScopeProvider>(new LoggerExternalScopeProvider());
    private readonly JoinableTask drain;

    public CommandWindowLoggerProvider() =>
        drain = Session.Threading.Factory.RunAsync(() => Source.lift(lines)
            .ReduceIO(unit, static (_, line) => IO.lift(() => RhinoApp.WriteLine(line)).OnUiThread().Map(static _ => Reduced.Continue(unit)))
            .RunAsync(CancellationToken.None)
            .AsTask());

    // --- [LOGGERS]
    public ILogger CreateLogger(string categoryName) => new CommandWindowLogger(categoryName, this);

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _ = scopes.Swap(_ => scopeProvider);

    internal IExternalScopeProvider Scopes => scopes.Value;

    internal void Write(string line) => _ = lines.Writer.TryWrite(line);

    // --- [RELEASE]
    public void Dispose() {
        _ = lines.Writer.TryComplete();
        drain.Join();
    }
}

internal sealed class CommandWindowLogger(string category, CommandWindowLoggerProvider provider) : ILogger {
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => provider.Scopes.Push(state);

    public bool IsEnabled(LogLevel logLevel) => logLevel is not LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        if (!IsEnabled(logLevel)) return;
        StringBuilder line = new(category);
        provider.Scopes.ForEachScope(static (scope, text) => text.Append(" => ").Append(scope), line);
        _ = line.Append(": ").Append(formatter(state, exception));
        provider.Write(exception is null ? line.ToString() : line.Append(Environment.NewLine).Append(exception).ToString());
    }
}
