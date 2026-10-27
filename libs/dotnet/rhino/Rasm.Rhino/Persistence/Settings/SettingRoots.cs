using System.Runtime.CompilerServices;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI;
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

    public IO<T> Current<T>(PlugInSetting<T> row) where T : notnull =>
        PlugInSettings.Store(this, row).Read.Map(held => held.IfNone(row.Default));
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
        node.Path.Fold(Some(Root(node.Root)), static (held, key) => held.Bind(parent => Conversions.Found(parent.TryGetChild(key, out PersistentSettings child), child)));

    // --- [EVENTS]
    public static HostEvent<Unit> Saved(SettingsNode node) =>
        node.Root.Switch(
            applicationNode: static _ => SettingsSaved,
            plugInNode: static plugIn => Heard(() => Optional(PlugIn.Find(plugIn.Id))),
            commandNode: static command => Heard(() => Some(command.Command.PlugIn)));

    public static Func<IPlugInViews, IO<Unit>> SavedByAnotherRhino(Func<PersistentSettingsSavedEventArgs, IO<Unit>> deliver) =>
        views => EventKind.SettingsSaved(views.PlugIn).Choose(static args => Conversions.Found(!args.SavedByThisRhino, args)).Inline(deliver, views);

    private static readonly HostEvent<Unit> SettingsSaved =
        Subscriptions.Host<PersistentSettingsSavedEventArgs>(static handler => add_SettingsSaved(owner: null, handler), static handler => remove_SettingsSaved(owner: null, handler))
            .Choose(static _ => Some(unit));

    private static HostEvent<Unit> Heard(Func<Option<PlugIn>> plugIn, [CallerArgumentExpression(nameof(plugIn))] string label = "") =>
        new(label, (deliver, owner) =>
            IO.lift(plugIn).Bind(found => found.Match(
                Some: held => EventKind.SettingsSaved(held).Choose(static _ => Some(unit)).Inline(deliver, owner),
                None: static () => IO.pure(unit))));

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern void add_SettingsSaved([UnsafeAccessorType("Rhino.RhinoApp, RhinoCommon")] object? owner, EventHandler<PersistentSettingsSavedEventArgs> value);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern void remove_SettingsSaved([UnsafeAccessorType("Rhino.RhinoApp, RhinoCommon")] object? owner, EventHandler<PersistentSettingsSavedEventArgs> value);
}
