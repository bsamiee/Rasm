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
        from bound in IO.lift(() => Bound.Swap(prior => prior | Some((
            Logger: loggers.CreateLogger(plugin.GetType()),
            Plugin: plugin,
            Settings: settings,
            Values: new KeyedValues() switch {
                var empty => settings.Get(ValuesKey, empty) as KeyedValues ?? empty,
            }))))
        from held in IO.lift(() => GrasshopperFault.Unless(bound.Exists(session => ReferenceEquals(session.Plugin, plugin)), GrasshopperReason.Taken, plugin.Name))
        select unit;

    internal static IO<Unit> Saved =>
        IO.lift(static () => {
            SettingsFile settings = Session.Settings;
            settings.Set(ValuesKey, Session.Values);
            return GrasshopperFault.Unless(Settings.SandBox || settings.TrySaveSettingsToFile(), GrasshopperReason.Declined, settings.File);
        });

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
            if (error.Count is 1 && error.Is(Errors.Cancelled)) Cancelled(Session.Logger, member);
            else Failed(Session.Logger, member, error.Message, error.Exception.ValueUnsafe());
            return failed;
        });

    [LoggerMessage(Level = LogLevel.Error, Message = "{Member} failed: {Message}")]
    private static partial void Failed(ILogger logger, string member, string message, Exception? exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Member} cancelled")]
    private static partial void Cancelled(ILogger logger, string member);
}
