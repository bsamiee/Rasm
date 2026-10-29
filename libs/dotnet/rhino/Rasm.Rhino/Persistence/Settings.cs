using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Rhino;
using Rhino.Commands;
using Rhino.PlugIns;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
internal delegate bool SettingRead<T>(PersistentSettings settings, string key, out T value, IEnumerable<string> legacyKeyList);

internal delegate bool SettingDefault<T>(PersistentSettings settings, string key, out T value);

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record SettingsRoot {
    internal abstract Fin<PersistentSettings> Settings { get; }

    internal abstract Option<PlugIn> Owner { get; }

    public sealed record ApplicationNode : SettingsRoot {
        internal override Fin<PersistentSettings> Settings => PersistentSettings.RhinoAppSettings;

        internal override Option<PlugIn> Owner => None;
    }

    public sealed record PlugInNode(Guid Id) : SettingsRoot {
        internal override Fin<PersistentSettings> Settings => PersistentSettings.FromPlugInId(Id);

        internal override Option<PlugIn> Owner => Optional(PlugIn.Find(Id));
    }

    public sealed record CommandNode(Command Command) : SettingsRoot {
        internal override Fin<PersistentSettings> Settings => Command.Settings;

        internal override Option<PlugIn> Owner => Optional(Command.PlugIn);
    }

    public sealed record WindowPositionNode(Guid Id) : SettingsRoot {
        internal override Fin<PersistentSettings> Settings => Conversions.Required(Owner).Map(static plugIn => plugIn.WindowPositionSettings);

        internal override Option<PlugIn> Owner => Optional(PlugIn.Find(Id));
    }
}

public sealed record SettingsNode(SettingsRoot Root, Seq<string> Path = default) {
    public static readonly SettingsNode Application = new(new SettingsRoot.ApplicationNode());

    public static readonly SettingsNode Options = Application.Child("Options");

    public SettingsNode Child(string key) => this with { Path = Path.Add(key) };

    // --- [NODES]
    public IO<Option<PersistentSettings>> Existing =>
        IO.lift(() => Root.Settings.Map(root => Path.Fold(Some(root), static (held, key) => held.Bind(parent => Conversions.Present(parent.TryGetChild(key, out PersistentSettings child), child)))));

    public IO<PersistentSettings> Created =>
        IO.lift(() => Root.Settings.Map(root => Path.Fold(root, static (parent, key) => parent.AddChild(key))));

    public IO<Unit> Delete(string key) =>
        Existing.Map(held => held.Iter(node => node.DeleteChild(key)));

    // --- [READS]
    public IO<Option<T>> Default<T>(string key) where T : notnull =>
        Existing.Map(held => held.Bind(node => SettingOverloads<T>.Default(node, key)));

    public IO<Seq<(string Key, Option<Type> Type)>> Keys =>
        Existing.Map(static held => (from node in held.ToSeq() from key in toSeq(node.Keys) select (Key: key, Type: Typed(node, key))).Strict());

    private static Option<Type> Typed(PersistentSettings node, string key) => Conversions.Present(node.TryGetSettingType(key, out Type type), type);

    public IO<Seq<SettingsNode>> Children =>
        Existing.Map(held => held.ToSeq().Bind(static node => toSeq(node.ChildKeys)).Map(Child).Strict());

    // --- [EVENTS]
    public HostEvent<Unit> Saved =>
        Root.Switch(
            applicationNode: static _ => SettingsSaved,
            plugInNode: static root => root.Id == RhinoApp.CurrentRhinoId ? SettingsSaved : Owned(root).Choose(static _ => Some(unit)),
            commandNode: static root => Owned(root).Choose(static _ => Some(unit)),
            windowPositionNode: static root => Owned(root).Choose(static _ => Some(unit)));

    public HostEvent<PersistentSettingsSavedEventArgs> SavedByAnotherRhino =>
        Owned(Root).Choose(static args => Conversions.Present(!args.SavedByThisRhino, args));

    private static HostEvent<PersistentSettingsSavedEventArgs> Owned(SettingsRoot root) =>
        new($"{root}.SettingsSaved", deliver =>
            IO.lift(() => root.Owner).Bind(found => found.Match(
                Some: plugIn => Subscriptions.On<PersistentSettingsSavedEventArgs>(h => plugIn.SettingsSaved += h, h => plugIn.SettingsSaved -= h).Attach(deliver),
                None: static () => IO.pure(unit))));

    private static readonly HostEvent<Unit> SettingsSaved =
        Subscriptions.On<PersistentSettingsSavedEventArgs>(static handler => Accessors.add_SettingsSaved(owner: null, handler), static handler => Accessors.remove_SettingsSaved(owner: null, handler))
            .Choose(static _ => Some(unit));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class SettingOverloads<T> where T : notnull {
    // --- [OVERLOADS]
    private const BindingFlags Exact = BindingFlags.Public | BindingFlags.Instance | BindingFlags.ExactBinding;
    private const string Writes = "Set";
    private const string Reads = "TryGet";

    private static readonly Option<MethodInfo> Writer =
        toSeq(typeof(PersistentSettings).GetMethods(BindingFlags.Public | BindingFlags.Instance)).Find(static method => !string.Equals(method.Name, nameof(PersistentSettings.SetDefault), StringComparison.Ordinal) && method.Name.StartsWith(Writes, StringComparison.Ordinal) && !method.IsGenericMethod
            && method.ReturnType == typeof(void) && method.GetParameters() is [{ ParameterType: var key }, { ParameterType: var value }] && key == typeof(string) && value == typeof(T));

    private static readonly Option<SettingRead<T>> Reader =
        Writer.Bind(static writer => Optional(typeof(PersistentSettings).GetMethod(
                string.Concat(Reads, writer.Name.AsSpan(Writes.Length)), Exact, [typeof(string), typeof(T).MakeByRefType(), typeof(IEnumerable<string>)])))
            .Map(static method => method.CreateDelegate<SettingRead<T>>());

    private static readonly Option<Action<PersistentSettings, string, T>> Typed =
        Reader.IsSome ? Writer.Map(static method => method.CreateDelegate<Action<PersistentSettings, string, T>>()) : None;

    private static readonly Option<Action<PersistentSettings, string, T>> Defaulter =
        Optional(typeof(PersistentSettings).GetMethod(nameof(PersistentSettings.SetDefault), Exact, [typeof(string), typeof(T)]))
            .Map(static method => method.CreateDelegate<Action<PersistentSettings, string, T>>());

    private static readonly Option<SettingDefault<T>> DefaultReader =
        Optional(typeof(PersistentSettings).GetMethod(nameof(PersistentSettings.TryGetDefault), Exact, [typeof(string), typeof(T).MakeByRefType()]))
            .Map(static method => method.CreateDelegate<SettingDefault<T>>());

    // --- [VALUES]
    internal static Fin<Unit> Supported => RhinoFault.Unless(typeof(T).IsEnum || Typed.IsSome, RhinoReason.Unsupported, typeof(T).Name);

    internal static Option<T> Read(PersistentSettings settings, string key, Seq<string> legacy) =>
        typeof(T).IsEnum ? SettingOverloads<int>.Stored(settings, key, legacy).Map(Enumerated) : Stored(settings, key, legacy);

    internal static Option<T> Default(PersistentSettings settings, string key) =>
        typeof(T).IsEnum ? SettingOverloads<int>.Defaulted(settings, key).Map(Enumerated) : Defaulted(settings, key);

    internal static Unit Write(PersistentSettings settings, string key, T value) =>
        typeof(T).IsEnum ? SettingOverloads<int>.Stored(settings, key, Numbered(value)) : Stored(settings, key, value);

    internal static Unit Registered(PersistentSettings settings, string key, T fallback, Option<Func<T, bool>> rule) =>
        typeof(T).IsEnum
            ? SettingOverloads<int>.Stored(settings, key, Numbered(fallback), rule.Map(static check => fun((int held) => check(Enumerated(held)))))
            : Stored(settings, key, fallback, rule);

    // --- [HOST]
    private static Option<T> Stored(PersistentSettings settings, string key, Seq<string> legacy) =>
        Reader.Bind(read => Present(read, settings, key, legacy));

    private static Option<T> Defaulted(PersistentSettings settings, string key) =>
        DefaultReader.Bind(read => Present(read, settings, key));

    private static Unit Stored(PersistentSettings settings, string key, T value) =>
        Typed.Iter(write => write(settings, key, value));

    private static Unit Stored(PersistentSettings settings, string key, T fallback, Option<Func<T, bool>> rule) {
        _ = Defaulter.Iter(write => write(settings, key, fallback));
        return rule.Iter(check => settings.RegisterSettingsValidator<T>(key, (_, args) => args.Cancel = !check(args.NewValue)));
    }

    // --- [CODECS]
    private static T Enumerated(int value) => (T)Enum.ToObject(typeof(T), value);

    private static int Numbered(T value) => Convert.ToInt32(value, CultureInfo.InvariantCulture);

    // --- [ADAPTERS]
    private static Option<T> Present(SettingRead<T> read, PersistentSettings settings, string key, Seq<string> legacy) =>
        Conversions.Present(read(settings, key, out T value, legacy), value);

    private static Option<T> Present(SettingDefault<T> read, PersistentSettings settings, string key) =>
        Conversions.Present(read(settings, key, out T value), value);
}
