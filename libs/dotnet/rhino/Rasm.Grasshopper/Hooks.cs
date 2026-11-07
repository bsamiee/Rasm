using System.Runtime.CompilerServices;
using Grasshopper2;
using Grasshopper2.Doc;
using Grasshopper2.Framework;
using Grasshopper2.UI;
using Rhino.Commands;

namespace Rasm.Grasshopper;

// --- [SERVICES] ------------------------------------------------------------------------
public static partial class Hooks {
    // --- [SESSION]
    private const string ValuesKey = "Values";

    private static readonly Atom<Option<(ILogger Logger, Plugin Plugin, SettingsFile Settings, KeyedValues Values)>> Bound =
        Atom(Option<(ILogger Logger, Plugin Plugin, SettingsFile Settings, KeyedValues Values)>.None);

    internal static readonly AtomHashMap<string, IO<Unit>> Themes = AtomHashMap<string, IO<Unit>>();

    internal static EnvIO Scope { get; } = EnvIO.New(token: CancellationToken.None);

    internal static (ILogger Logger, Plugin Plugin, SettingsFile Settings, KeyedValues Values) Session =>
        Bound.Value.IfNone(static () => throw new GrasshopperFault(Path.GetFileNameWithoutExtension(typeof(Hooks).Assembly.Location), GrasshopperReason.Unloaded).ToException());

    internal static IO<Unit> Bind(ILoggerFactory loggers, Plugin plugin) =>
        from settings in IO.lift(() => new SettingsFolder($"Plugins.{plugin.Id:N}").GetSettings("Preferences"))
        from candidate in IO.lift(() => (
            Logger: loggers.CreateLogger(plugin.GetType()),
            Plugin: plugin,
            Settings: settings,
            Values: new KeyedValues() switch {
                var empty => settings.Get(ValuesKey, empty) as KeyedValues ?? empty,
            }))
        from bound in IO.lift(() => Bound.Swap(prior => prior | Some(candidate)))
        from held in IO.lift(GrasshopperFault.Unless(bound.Exists(session => ReferenceEquals(session.Plugin, plugin)), GrasshopperReason.Taken, plugin.Name))
        select unit;

    internal static IO<Unit> Saved =>
        IO.lift(static () => Session).Bind(static session => IO.lift(() => {
            session.Settings.Set(ValuesKey, session.Values);
            return GrasshopperFault.Unless(Settings.SandBox || session.Settings.TrySaveSettingsToFile(), GrasshopperReason.Declined, session.Settings.File);
        }));

    // --- [EVENTS]
    public static IO<Unit> On(Action<EventHandler> add, Action<EventHandler> remove, IO<Unit> react, [CallerArgumentExpression(nameof(add))] string label = "") =>
        Subscribed((_, _) => Run(react, unit, label), add, remove);

    public static IO<Unit> On<TArgs>(Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove, Func<TArgs, IO<Unit>> react, [CallerArgumentExpression(nameof(add))] string label = "") =>
        Subscribed((_, args) => Run(react(args), unit, label), add, remove);

    private static IO<Unit> Subscribed<THandler>(THandler handler, Action<THandler> add, Action<THandler> remove) where THandler : Delegate =>
        use(() => {
            add(handler);
            return handler;
        }, remove).Map(unit);

    // --- [EDITOR]
    public static IO<Unit> Editor(Func<Editor, IO<Unit>> attach) =>
        from scope in envIO
        from present in IO.lift(static () => Optional(Grasshopper2.UI.Editor.Instance))
        from attached in present.Match(Some: attach, None: () => Built(scope, attach))
        select attached;

    private static IO<Unit> Built(EnvIO scope, Func<Editor, IO<Unit>> attach) =>
        from waiting in use(() => EnvIO.New(token: scope.Token))
        from subscribed in IO.liftVAsync(() => On<CommandEventArgs>(
            static h => Command.EndCommand += h,
            static h => Command.EndCommand -= h,
            _ => IO.lift(static () => Optional(Grasshopper2.UI.Editor.Instance))
                .Bind(built => built.Traverse(editor => IO.lift(waiting.Dispose) >> IO.liftVAsync(() => attach(editor).RunAsync(scope))).As())
                .Map(unit)).RunAsync(waiting))
        select subscribed;

    // --- [RUNS]
    internal static T Run<T>(IO<T> effect, T failed, [CallerMemberName] string member = "") =>
        effect.RunSafe().IfFail(error => {
            if (error.Count is 1 && error.Is(Errors.Cancelled)) Canceled(Session.Logger, member);
            else Failed(Session.Logger, member, string.Join(Environment.NewLine, Causes(error).Map(static cause => cause.Message)), Causes(error).Choose(static cause => cause.Exception) switch { [] => null, [var single] => single, var many => new AggregateException(many) });
            return failed;
        });

    private static Seq<Error> Causes(Error error) =>
        toSeq(LanguageExt.List.unfold(error.AsIterable().ToSeq(), static pending => pending.Head.Map(cause =>
            (cause, cause.Inner.ToSeq().Bind(static inner => inner.AsIterable().ToSeq()) + pending.Tail))));

    [LoggerMessage(Level = LogLevel.Error, Message = "{Member} failed: {Message}")]
    private static partial void Failed(ILogger logger, string member, string message, Exception? exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Member} canceled")]
    private static partial void Canceled(ILogger logger, string member);
}
