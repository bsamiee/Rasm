using System.Runtime.CompilerServices;
using Rasm.Rhino.Events;
using Rhino;
using Rhino.Commands;
using Rhino.PlugIns;

namespace Rasm.Rhino.Persistence.Settings;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record SettingsRoot {
    public sealed record ApplicationNode : SettingsRoot;

    public sealed record PlugInNode(Guid Id) : SettingsRoot;

    public sealed record CommandNode(Command Command) : SettingsRoot;
}

public sealed record SettingsNode(SettingsRoot Root, Seq<string> Path = default) : ISettingReader {
    public static readonly SettingsNode Application = new(new SettingsRoot.ApplicationNode());

    public static readonly SettingsNode Options = Application.Child("Options");

    public SettingsNode Child(string key) => this with { Path = Path.Add(key) };

    public IO<TValue> Current<TValue, TRaw>(PlugInSetting<TValue, TRaw> row)
        where TValue : notnull
        where TRaw : notnull, ISpanParsable<TRaw> =>
        PlugInSettings.Store(this, row).Read.Map(held => held.IfNone(row.Key.Default));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SettingRoots {
    // --- [NODES]
    public static IO<Option<PersistentSettings>> TryGetChild(SettingsNode node) =>
        IO.lift(() => Resolved(node));

    public static IO<PersistentSettings> AddChild(SettingsNode node) =>
        IO.lift(() => node.Path.Fold(Root(node.Root), static (parent, key) => parent.AddChild(key)));

    public static IO<Unit> DeleteChild(SettingsNode parent, string key) =>
        IO.lift(() => Resolved(parent).Iter(held => held.DeleteChild(key)));

    private static PersistentSettings Root(SettingsRoot root) =>
        root.Switch(
            applicationNode: static _ => PersistentSettings.RhinoAppSettings,
            plugInNode: static plugIn => PersistentSettings.FromPlugInId(plugIn.Id),
            commandNode: static command => command.Command.Settings);

    internal static Option<PersistentSettings> Resolved(SettingsNode node) =>
        node.Path.Fold(Some(Root(node.Root)), static (held, key) => held.Bind(parent => Callbacks.Found(parent.TryGetChild(key, out PersistentSettings child), child)));

    // --- [EVENTS]
    public static HostEvent<Unit> Saved(SettingsNode node) =>
        node.Root.Switch(
            applicationNode: static _ => SettingsSaved,
            plugInNode: static plugIn => Heard(() => Optional(PlugIn.Find(plugIn.Id))),
            commandNode: static command => Heard(() => Some(command.Command.PlugIn)));

    public static Func<IPlugInSink, Action<IDisposable>, IO<Unit>> SavedByAnotherRhino(Func<PersistentSettingsSavedEventArgs, IO<Unit>> deliver) =>
        (sink, retain) => EventKind.SettingsSaved((PlugIn)sink).Choose(static args => Callbacks.Found(!args.SavedByThisRhino, args)).Inline(deliver, sink, retain);

    private static readonly HostEvent<Unit> SettingsSaved =
        Subscriptions.Host<PersistentSettingsSavedEventArgs>(typeof(RhinoApp), static handler => add_SettingsSaved(owner: null, handler), static handler => remove_SettingsSaved(owner: null, handler))
            .Choose(static _ => Some(unit));

    private static HostEvent<Unit> Heard(Func<Option<PlugIn>> owner) =>
        new(typeof(PlugIn), nameof(PlugIn.SettingsSaved), (deliver, site, retain) =>
            IO.lift(owner).Bind(found => found.Match(
                Some: plugIn => EventKind.SettingsSaved(plugIn).Choose(static _ => Some(unit)).Attach(deliver, site, retain),
                None: static () => IO.pure(unit))));

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern void add_SettingsSaved([UnsafeAccessorType("Rhino.RhinoApp, RhinoCommon")] object? owner, EventHandler<PersistentSettingsSavedEventArgs> value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern void remove_SettingsSaved([UnsafeAccessorType("Rhino.RhinoApp, RhinoCommon")] object? owner, EventHandler<PersistentSettingsSavedEventArgs> value);
}
