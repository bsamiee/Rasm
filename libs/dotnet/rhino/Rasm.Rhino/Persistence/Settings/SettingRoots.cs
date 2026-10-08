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

public sealed record SettingsNode(SettingsRoot Root, Seq<string> Path = default) {
    public static readonly SettingsNode Application = new(new SettingsRoot.ApplicationNode());

    public static readonly SettingsNode Options = Application.Child("Options");

    public SettingsNode Child(string key) => this with { Path = Path.Add(key) };
}

public sealed record KeyDescription(Option<Type> RuntimeType, bool ReadOnly, bool HiddenFromUserInterface);

public sealed record NodeDescription(bool HiddenFromUserInterface, HashMap<string, KeyDescription> Keys, HashMap<string, NodeDescription> Children);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SettingRoots {
    // --- [NODES]
    public static IO<Option<PersistentSettings>> TryGetChild(SettingsNode node) =>
        IO.lift(() => Resolved(node));

    public static IO<PersistentSettings> AddChild(SettingsNode node) =>
        IO.lift(() => node.Path.Fold(Root(node.Root), static (parent, key) => parent.AddChild(key)));

    public static IO<Unit> DeleteChild(SettingsNode parent, string key) =>
        IO.lift(() => Resolved(parent).Iter(held => held.DeleteChild(key)));

    public static IO<Option<NodeDescription>> Describe(SettingsNode node) =>
        IO.lift(() => Resolved(node).Map(Tree));

    private static PersistentSettings Root(SettingsRoot root) =>
        root.Switch(
            applicationNode: static _ => PersistentSettings.RhinoAppSettings,
            plugInNode: static plugIn => PersistentSettings.FromPlugInId(plugIn.Id),
            commandNode: static command => command.Command.Settings);

    private static Option<PersistentSettings> Resolved(SettingsNode node) =>
        node.Path.Fold(Some(Root(node.Root)), static (held, key) => held.Bind(parent => Callbacks.Found(parent.TryGetChild(key, out PersistentSettings child), child)));

    private static NodeDescription Tree(PersistentSettings node) =>
        new(
            node.HiddenFromUserInterface,
            toHashMap(toSeq(node.Keys).Choose(key => Entry(node, key).Map(entry => (key, entry)))),
            toHashMap(toSeq(node.ChildKeys).Choose(key => Callbacks.Found(node.TryGetChild(key, out PersistentSettings child), child).Map(found => (key, Tree(found))))));

    private static Option<KeyDescription> Entry(PersistentSettings node, string key) =>
        node.TryGetSettingIsReadOnly(key, out bool readOnly) && node.TryGetSettingIsHiddenFromUserInterface(key, out bool hidden) && node.TryGetSettingType(key, out Type? type)
            ? Some(new KeyDescription(Optional(type), readOnly, hidden))
            : None;

    // --- [CHANGES]
    public static IO<bool> ContainsChangedValues(SettingsNode node) =>
        IO.lift(() => Resolved(node).Exists(static held => held.ContainsChangedValues()));

    public static IO<bool> ContainsModifiedValues(SettingsNode node) =>
        IO.lift(() => Resolved(node).Exists(static held => held.ContainsModifiedValues(allUserSettings: null)));

    public static IO<Unit> ClearChangedFlag(SettingsNode node) =>
        IO.lift(() => Resolved(node).Iter(static held => held.ClearChangedFlag()));

    // --- [EVENTS]
    public static HostEvent<Unit> Saved(SettingsNode node) =>
        node.Root.Switch(
            applicationNode: static _ => SettingsSaved,
            plugInNode: static plugIn => Heard(() => Optional(PlugIn.Find(plugIn.Id))),
            commandNode: static command => Heard(() => Some(command.Command.PlugIn)));

    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> SavedByAnotherRhino(Func<PersistentSettingsSavedEventArgs, IO<Unit>> deliver) =>
        (plugIn, sink) => EventKind.SettingsSaved(plugIn).Choose(static args => Callbacks.Found(!args.SavedByThisRhino, args)).Inline(deliver, sink);

    private static readonly HostEvent<Unit> SettingsSaved =
        Subscriptions.Host<PersistentSettingsSavedEventArgs>(typeof(RhinoApp), static handler => add_SettingsSaved(owner: null, handler), static handler => remove_SettingsSaved(owner: null, handler))
            .Choose(static _ => Some(unit));

    private static HostEvent<Unit> Heard(Func<Option<PlugIn>> owner) =>
        new(typeof(PlugIn), nameof(PlugIn.SettingsSaved), (deliver, site) =>
            IO.lift(owner).Bind(found => found.Match(
                Some: plugIn => EventKind.SettingsSaved(plugIn).Choose(static _ => Some(unit)).Attach(deliver, site),
                None: static () => IO.pure(Thinktecture.Empty.Disposable()))));

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern void add_SettingsSaved([UnsafeAccessorType("Rhino.RhinoApp, RhinoCommon")] object? owner, EventHandler<PersistentSettingsSavedEventArgs> value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern void remove_SettingsSaved([UnsafeAccessorType("Rhino.RhinoApp, RhinoCommon")] object? owner, EventHandler<PersistentSettingsSavedEventArgs> value);
}
