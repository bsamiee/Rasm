using Rhino;

namespace Rasm.Rhino.Persistence.Settings;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SettingType<T>(
    Func<PersistentSettings, string, Option<T>> TryGet,
    Action<PersistentSettings, string, T> Set,
    Func<PersistentSettings, string, T, T> Get,
    Option<Action<PersistentSettings, string, Func<T, bool>>> Validator) where T : notnull {
    public void Register(PersistentSettings node, string key, T value, IEnumerable<string> legacy) {
        _ = node.TryGetString(key, out _, legacy);
        _ = Get(node, key, value);
    }

    public Fin<Option<T>> Read(PersistentSettings node, string key) =>
        SettingType.String.TryGet(node, key).Traverse(text => TryGet(node, key).ToFin(new UnreadText(text, typeof(T)))).As();
}

public static class SettingType {
    public static readonly SettingType<Guid> Guid = new(
        static (node, key) => Callbacks.Found(node.TryGetGuid(key, out Guid value), value),
        static (node, key, value) => node.SetGuid(key, value),
        static (node, key, value) => node.GetGuid(key, value),
        None);

    public static readonly SettingType<bool> Bool = new(
        static (node, key) => Callbacks.Found(node.TryGetBool(key, out bool value), value),
        static (node, key, value) => node.SetBool(key, value),
        static (node, key, value) => node.GetBool(key, value),
        None);

    public static readonly SettingType<byte> Byte = new(
        static (node, key) => Callbacks.Found(node.TryGetByte(key, out byte value), value),
        static (node, key, value) => node.SetByte(key, value),
        static (node, key, value) => node.GetByte(key, value),
        None);

    public static readonly SettingType<int> Integer = new(
        static (node, key) => Callbacks.Found(node.TryGetInteger(key, out int value), value),
        static (node, key, value) => node.SetInteger(key, value),
        static (node, key, value) => node.GetInteger(key, value),
        None);

    public static readonly SettingType<uint> UnsignedInteger = new(
        static (node, key) => Callbacks.Found(node.TryGetUnsignedInteger(key, out uint value), value),
        static (node, key, value) => node.SetUnsignedInteger(key, value),
        static (node, key, value) => node.GetUnsignedInteger(key, value),
        None);

    public static readonly SettingType<double> Double = new(
        static (node, key) => Callbacks.Found(node.TryGetDouble(key, out double value), value),
        static (node, key, value) => node.SetDouble(key, value),
        static (node, key, value) => node.GetDouble(key, value),
        None);

    public static readonly SettingType<char> Char = new(
        static (node, key) => Callbacks.Found(node.TryGetChar(key, out char value), value),
        static (node, key, value) => node.SetChar(key, value),
        static (node, key, value) => node.GetChar(key, value),
        None);

    public static readonly SettingType<string> String = new(
        static (node, key) => node.TryGetString(key, out string? value) ? Conversions.Present(value) : None,
        static (node, key, value) => node.SetString(key, value),
        static (node, key, value) => node.GetString(key, value),
        Some<Action<PersistentSettings, string, Func<string, bool>>>(static (node, key, refused) =>
            node.RegisterSettingsValidator<string>(key, (_, args) => args.Cancel = refused(args.NewValue))));

    public static readonly SettingType<string[]> StringList = new(
        static (node, key) => node.TryGetStringList(key, out string[]? value) ? Optional(value) : None,
        static (node, key, value) => node.SetStringList(key, value),
        static (node, key, value) => node.GetStringList(key, value),
        None);

    public static readonly SettingType<KeyValuePair<string, string>[]> StringDictionary = new(
        static (node, key) => Callbacks.Found(node.TryGetStringDictionary(key, out KeyValuePair<string, string>[] value), value),
        static (node, key, value) => node.SetStringDictionary(key, value),
        static (node, key, value) => node.GetStringDictionary(key, value),
        None);

    public static readonly SettingType<DateTime> Date = new(
        static (node, key) => Callbacks.Found(node.TryGetDate(key, out DateTime value), value),
        static (node, key, value) => node.SetDate(key, value),
        static (node, key, value) => node.GetDate(key, value),
        None);

    public static readonly SettingType<System.Drawing.Color> Color = new(
        static (node, key) => Callbacks.Found(node.TryGetColor(key, out System.Drawing.Color value), value),
        static (node, key, value) => node.SetColor(key, value),
        static (node, key, value) => node.GetColor(key, value),
        None);

    public static readonly SettingType<System.Drawing.Point> Point = new(
        static (node, key) => Callbacks.Found(node.TryGetPoint(key, out System.Drawing.Point value), value),
        static (node, key, value) => node.SetPoint(key, value),
        static (node, key, value) => node.GetPoint(key, value),
        None);

    public static readonly SettingType<Point3d> Point3d = new(
        static (node, key) => Callbacks.Found(node.TryGetPoint3d(key, out Point3d value), value),
        static (node, key, value) => node.SetPoint3d(key, value),
        static (node, key, value) => node.GetPoint3d(key, value),
        None);

    public static readonly SettingType<System.Drawing.Size> Size = new(
        static (node, key) => Callbacks.Found(node.TryGetSize(key, out System.Drawing.Size value), value),
        static (node, key, value) => node.SetSize(key, value),
        static (node, key, value) => node.GetSize(key, value),
        None);

    public static readonly SettingType<System.Drawing.Rectangle> Rectangle = new(
        static (node, key) => Callbacks.Found(node.TryGetRectangle(key, out System.Drawing.Rectangle value), value),
        static (node, key, value) => node.SetRectangle(key, value),
        static (node, key, value) => node.GetRectangle(key, value),
        None);

    public static SettingType<T> Enumeration<T>() where T : struct, Enum =>
        new(
            static (node, key) => Callbacks.Found(node.TryGetEnumValue(key, out T value), value),
            static (node, key, value) => node.SetEnumValue(key, value),
            static (node, key, value) => node.GetEnumValue(key, value),
            None);
}
