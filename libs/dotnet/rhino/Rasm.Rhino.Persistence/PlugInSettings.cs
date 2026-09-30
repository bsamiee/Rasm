using System.Drawing;
using System.Globalization;
using System.Numerics;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.PlugIns;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SettingType<T>(Func<string, Option<T>> Parse, Action<PersistentSettings, string, T> Write) {
    public Fin<Option<T>> Read(PersistentSettings node, string key) =>
        SettingType.Text(node, key).Traverse(text => Parse(text).ToFin(new TypeMismatch(key, text, typeof(T)))).As();
}

public static class SettingType {
    public static readonly SettingType<bool> Bool = new(static text => Answers.Found(bool.TryParse(text, out bool value), value), static (node, key, value) => node.SetBool(key, value));

    public static readonly SettingType<int> Integer = new(Number<int>, static (node, key, value) => node.SetInteger(key, value));

    public static readonly SettingType<uint> UnsignedInteger = new(Number<uint>, static (node, key, value) => node.SetUnsignedInteger(key, value));

    public static readonly SettingType<double> Double = new(Number<double>, static (node, key, value) => node.SetDouble(key, value));

    public static readonly SettingType<string> String = new(static text => Some(text), static (node, key, value) => node.SetString(key, value));

    public static readonly SettingType<Color> Color = new(
        static text => text.Split(',') is [var alpha, var red, var green, var blue]
            ? from a in Number<byte>(alpha) from r in Number<byte>(red) from g in Number<byte>(green) from b in Number<byte>(blue) select System.Drawing.Color.FromArgb(a, r, g, b)
            : Option<Color>.None,
        static (node, key, value) => node.SetColor(key, value));

    public static SettingType<T> Enumeration<T>() where T : struct, Enum =>
        new(static text => Answers.Found(Enum.TryParse(text, ignoreCase: false, out T value), value), static (node, key, value) => node.SetEnumValue(key, value));

    public static Option<string> Text(PersistentSettings node, string key) =>
        Answers.Found(node.TryGetString(key, out string text), text).Bind(static text => Answers.Present(text));

    private static Option<T> Number<T>(string text) where T : struct, INumberBase<T> =>
        Answers.Found(T.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out T value), value);
}

public sealed record SettingKey<T>(string Name, SettingType<T> Type);

public sealed record SettingsState(Seq<string> Keys, Seq<string> ChildKeys, bool HiddenFromUserInterface);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class PlugInSettingsMapper {
    internal static partial SettingsState ToState(PersistentSettings node);
}

public static class PlugInSettings {
    // --- [NODES]
    public static IO<Option<PersistentSettings>> TryGetChild(PersistentSettings root, Seq<string> path) =>
        IO.lift(() => path.Fold(Some(root), static (node, key) => node.Bind(found => Answers.Found(found.TryGetChild(key, out PersistentSettings child), child))));

    public static IO<PersistentSettings> AddChild(PersistentSettings root, Seq<string> path) =>
        IO.lift(() => path.Fold(root, static (node, key) => node.AddChild(key)));

    public static IO<SettingsState> Describe(PersistentSettings node) =>
        IO.lift(() => PlugInSettingsMapper.ToState(node));

    // --- [READS]
    public static IO<Option<T>> Find<T>(PersistentSettings node, SettingKey<T> key) =>
        IO.lift(() => key.Type.Read(node, key.Name));

    // --- [WRITES]
    public static IO<Unit> Set<T>(PersistentSettings node, SettingKey<T> key, T value) =>
        from writable in IO.lift(() => Writable(node, key.Name))
        from written in IO.lift(() => key.Type.Write(node, key.Name, value))
        select written;

    public static IO<Unit> Delete(PersistentSettings node, string key) =>
        from writable in IO.lift(() => Writable(node, key))
        from deleted in IO.lift(() => node.DeleteItem(key))
        select deleted;

    private static Fin<Unit> Writable(PersistentSettings node, string key) =>
        ReadOnlyKey.Unless(!(node.TryGetSettingIsReadOnly(key, out bool readOnly) && readOnly), key);

    // --- [VALIDATORS]
    public static IO<Unit> RegisterSettingsValidator<T>(PersistentSettings node, SettingKey<T> key, Func<T, T, bool> accept) =>
        IO.lift(() => node.RegisterSettingsValidator<T>(key.Name, (_, args) => args.Cancel = !accept(args.CurrentValue, args.NewValue)));

    // --- [EVENTS]
    public static IO<IDisposable> OnSaved(PlugIn plugIn, Func<PersistentSettingsSavedEventArgs, IO<Unit>> deliver, Action<Error> reject) =>
        Events.Attach(
            h => plugIn.SettingsSaved += h,
            h => plugIn.SettingsSaved -= h,
            Answers.Handler(deliver, reject));
}
