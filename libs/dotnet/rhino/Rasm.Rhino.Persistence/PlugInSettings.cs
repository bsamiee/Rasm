using System.Diagnostics.CodeAnalysis;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.PlugIns;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
public interface ISettingRead<TSelf> where TSelf : SettingValue {
    public static abstract Option<TSelf> Read(PersistentSettings node, string key, Seq<string> legacy);
}

public interface ISettingDefaultWrite<TSelf> where TSelf : SettingValue {
    public static abstract IO<Unit> SetDefault(PersistentSettings node, string key, TSelf value);
}

public interface ISettingDefault<TSelf> : ISettingRead<TSelf>, ISettingDefaultWrite<TSelf> where TSelf : SettingValue {
    public static abstract Option<TSelf> ReadDefault(PersistentSettings node, string key);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingValue {
    public sealed record Bool(bool Value) : SettingValue, ISettingDefault<Bool> {
        public static Option<Bool> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetBool(key, out bool stored, legacy), stored).Map(static found => new Bool(found));

        public static Option<Bool> ReadDefault(PersistentSettings node, string key) =>
            Answers.Found(node.TryGetDefault(key, out bool stored), stored).Map(static found => new Bool(found));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, Bool value) =>
            PlugInSettings.Defaulted(node, key, () => node.SetDefault(key, value.Value), value);
    }

    public sealed record Byte(byte Value) : SettingValue, ISettingDefault<Byte> {
        public static Option<Byte> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetByte(key, out byte stored, legacy), stored).Map(static found => new Byte(found));

        public static Option<Byte> ReadDefault(PersistentSettings node, string key) =>
            Answers.Found(node.TryGetDefault(key, out byte stored), stored).Map(static found => new Byte(found));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, Byte value) =>
            PlugInSettings.Defaulted(node, key, () => node.SetDefault(key, value.Value), value);
    }

    public sealed record Integer(int Value) : SettingValue, ISettingDefault<Integer> {
        public static Option<Integer> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetInteger(key, out int stored, legacy), stored).Map(static found => new Integer(found));

        public static Option<Integer> ReadDefault(PersistentSettings node, string key) =>
            Answers.Found(node.TryGetDefault(key, out int stored), stored).Map(static found => new Integer(found));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, Integer value) =>
            PlugInSettings.Defaulted(node, key, () => node.SetDefault(key, value.Value), value);
    }

    public sealed record UnsignedInteger(uint Value) : SettingValue, ISettingRead<UnsignedInteger> {
        public static Option<UnsignedInteger> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetUnsignedInteger(key, out uint stored, legacy), stored).Map(static found => new UnsignedInteger(found));
    }

    public sealed record Double(double Value) : SettingValue, ISettingDefault<Double> {
        public static Option<Double> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetDouble(key, out double stored, legacy), stored).Map(static found => new Double(found));

        public static Option<Double> ReadDefault(PersistentSettings node, string key) =>
            Answers.Found(node.TryGetDefault(key, out double stored), stored).Map(static found => new Double(found));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, Double value) =>
            PlugInSettings.Defaulted(node, key, () => node.SetDefault(key, value.Value), value);
    }

    public sealed record Char(char Value) : SettingValue, ISettingDefault<Char> {
        public static Option<Char> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetChar(key, out char stored, legacy), stored).Map(static found => new Char(found));

        public static Option<Char> ReadDefault(PersistentSettings node, string key) =>
            Answers.Found(node.TryGetDefault(key, out char stored), stored).Map(static found => new Char(found));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, Char value) =>
            PlugInSettings.Defaulted(node, key, () => node.SetDefault(key, value.Value), value);
    }

    public sealed record String(string Value) : SettingValue, ISettingDefault<String> {
        public static Option<String> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetString(key, out string stored, legacy), stored).Map(static found => new String(found));

        public static Option<String> ReadDefault(PersistentSettings node, string key) =>
            Answers.Found(node.TryGetDefault(key, out string stored), stored).Map(static found => new String(found));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, String value) =>
            PlugInSettings.Defaulted(node, key, () => node.SetDefault(key, value.Value), value);
    }

    public sealed record StringList(Seq<string> Values) : SettingValue, ISettingDefault<StringList> {
        public static Option<StringList> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetStringList(key, out string[] stored, legacy), stored).Map(static found => new StringList(toSeq(found)));

        public static Option<StringList> ReadDefault(PersistentSettings node, string key) =>
            Answers.Found(node.TryGetDefault(key, out string[] stored), stored).Map(static found => new StringList(toSeq(found)));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, StringList value) =>
            PlugInSettings.Defaulted(node, key, () => node.SetDefault(key, [.. value.Values]), value);
    }

    public sealed record StringDictionary(Seq<(string Key, string Value)> Pairs) : SettingValue, ISettingRead<StringDictionary>, ISettingDefaultWrite<StringDictionary> {
        public static Option<StringDictionary> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetStringDictionary(key, out KeyValuePair<string, string>[] stored, legacy), stored).Map(static found => new StringDictionary(toSeq(found).Map(static pair => (pair.Key, pair.Value))));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, StringDictionary value) =>
            IO.lift(() => node.SetDefault(key, PlugInSettings.Pairs(value.Pairs)));
    }

    public sealed record Date(DateTime Value) : SettingValue, ISettingDefault<Date> {
        public static Option<Date> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetDate(key, out DateTime stored, legacy), stored).Map(static found => new Date(found));

        public static Option<Date> ReadDefault(PersistentSettings node, string key) =>
            Answers.Found(node.TryGetDefault(key, out DateTime stored), stored).Map(static found => new Date(found));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, Date value) =>
            PlugInSettings.Defaulted(node, key, () => node.SetDefault(key, value.Value), value);
    }

    public sealed record Color(System.Drawing.Color Value) : SettingValue, ISettingDefault<Color> {
        public static Option<Color> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetColor(key, out System.Drawing.Color stored, legacy), stored).Map(static found => new Color(found));

        public static Option<Color> ReadDefault(PersistentSettings node, string key) =>
            Answers.Found(node.TryGetDefault(key, out System.Drawing.Color stored), stored).Map(static found => new Color(found));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, Color value) =>
            PlugInSettings.Defaulted(node, key, () => node.SetDefault(key, value.Value), value);

        public bool Equals([NotNullWhen(true)] Color? other) => other is not null && (other.Value.ToArgb() == Value.ToArgb());

        public override int GetHashCode() => Value.ToArgb();
    }

    public sealed record OptionalColor(Option<System.Drawing.Color> Value) : SettingValue, ISettingRead<OptionalColor>, ISettingDefaultWrite<OptionalColor> {
        public static Option<OptionalColor> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetColor(key, out System.Drawing.Color? stored, legacy), stored).Map(static found => new OptionalColor(Optional(found)));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, OptionalColor value) =>
            IO.lift(() => node.SetDefault(key, value.Value.ToNullable()));

        public bool Equals([NotNullWhen(true)] OptionalColor? other) => other is not null && (other.Value.Map(static color => color.ToArgb()) == Value.Map(static color => color.ToArgb()));

        public override int GetHashCode() => Value.Map(static color => color.ToArgb()).GetHashCode();
    }

    public sealed record Guid(System.Guid Value) : SettingValue, ISettingRead<Guid>, ISettingDefaultWrite<Guid> {
        public static Option<Guid> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetGuid(key, out System.Guid stored, legacy), stored).Map(static found => new Guid(found));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, Guid value) =>
            IO.lift(() => node.SetDefault(key, value.Value));
    }

    public sealed record Point(System.Drawing.Point Value) : SettingValue, ISettingRead<Point>, ISettingDefaultWrite<Point> {
        public static Option<Point> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetPoint(key, out System.Drawing.Point stored, legacy), stored).Map(static found => new Point(found));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, Point value) =>
            IO.lift(() => node.SetDefault(key, value.Value));
    }

    public sealed record Point3d(global::Rhino.Geometry.Point3d Value) : SettingValue, ISettingDefault<Point3d> {
        public static Option<Point3d> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetPoint3d(key, out global::Rhino.Geometry.Point3d stored, legacy), stored).Map(static found => new Point3d(found));

        public static Option<Point3d> ReadDefault(PersistentSettings node, string key) =>
            Answers.Found(node.TryGetDefault(key, out global::Rhino.Geometry.Point3d stored), stored).Map(static found => new Point3d(found));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, Point3d value) =>
            PlugInSettings.Defaulted(node, key, () => node.SetDefault(key, value.Value), value);
    }

    public sealed record Size(System.Drawing.Size Value) : SettingValue, ISettingDefault<Size> {
        public static Option<Size> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetSize(key, out System.Drawing.Size stored, legacy), stored).Map(static found => new Size(found));

        public static Option<Size> ReadDefault(PersistentSettings node, string key) =>
            Answers.Found(node.TryGetDefault(key, out System.Drawing.Size stored), stored).Map(static found => new Size(found));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, Size value) =>
            PlugInSettings.Defaulted(node, key, () => node.SetDefault(key, value.Value), value);
    }

    public sealed record Rectangle(System.Drawing.Rectangle Value) : SettingValue, ISettingDefault<Rectangle> {
        public static Option<Rectangle> Read(PersistentSettings node, string key, Seq<string> legacy) =>
            Answers.Found(node.TryGetRectangle(key, out System.Drawing.Rectangle stored, legacy), stored).Map(static found => new Rectangle(found));

        public static Option<Rectangle> ReadDefault(PersistentSettings node, string key) =>
            Answers.Found(node.TryGetDefault(key, out System.Drawing.Rectangle stored), stored).Map(static found => new Rectangle(found));

        public static IO<Unit> SetDefault(PersistentSettings node, string key, Rectangle value) =>
            PlugInSettings.Defaulted(node, key, () => node.SetDefault(key, value.Value), value);
    }
}

public sealed record SettingState(Type SettingType, bool ReadOnly, bool HiddenFromUserInterface);

public sealed record SettingsState(Seq<string> Keys, Seq<string> ChildKeys, bool HiddenFromUserInterface);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PlugInSettings {
    // --- [NODES]
    public static IO<PersistentSettings> FromPlugInId(Guid plugInId) =>
        IO.lift(() => Answers.NonEmpty(plugInId, nameof(PersistentSettings.FromPlugInId)).Map(PersistentSettings.FromPlugInId));

    public static IO<Option<PersistentSettings>> TryGetChild(PersistentSettings root, Seq<string> path) =>
        IO.lift(() => path.Fold(Some(root), static (node, key) => node.Bind(found => Answers.Found(found.TryGetChild(key, out PersistentSettings child), child))));

    public static IO<PersistentSettings> AddChild(PersistentSettings root, Seq<string> path) =>
        IO.lift(() => path.Fold(root, static (node, key) => node.AddChild(key)));

    public static IO<SettingsState> Describe(PersistentSettings node) =>
        IO.lift(() => SettingsMapper.ToState(node));

    // --- [READS]
    public static IO<Option<TValue>> Find<TValue>(PersistentSettings node, string key, Seq<string> legacy) where TValue : SettingValue, ISettingRead<TValue> =>
        IO.lift(() => Present(TValue.Read(node, key, legacy), node, key, typeof(TValue)));

    public static IO<Option<T>> FindEnum<T>(PersistentSettings node, string key) where T : struct, Enum =>
        from keyed in IO.lift(() => Invalid.Unless(key.Length > 0, nameof(PersistentSettings.TryGetEnumValue)))
        from present in IO.lift(() => Present(Answers.Found(node.TryGetEnumValue(key, out T value), value), node, key, typeof(T)))
        select present;

    public static IO<Option<TValue>> FindDefault<TValue>(PersistentSettings node, string key) where TValue : SettingValue, ISettingDefault<TValue> =>
        IO.lift(() => TValue.ReadDefault(node, key));

    public static IO<int> GetInteger(PersistentSettings node, string key, int fallback, Limits<int> limits) =>
        IO.lift(() => limits.Inclusive(nameof(PersistentSettings.GetInteger)).Map(inclusive => inclusive.Fold(
            both: (lower, upper) => node.GetInteger(key, fallback, lower.Value, upper.Value),
            lower: lower => node.GetInteger(key, fallback, lower.Value, boundIsLower: true),
            upper: upper => node.GetInteger(key, fallback, upper.Value, boundIsLower: false),
            none: () => node.GetInteger(key, fallback))));

    public static IO<Option<SettingState>> Inspect(PersistentSettings node, string key) =>
        IO.lift(() => node.TryGetSettingType(key, out Type stored) && node.TryGetSettingIsReadOnly(key, out bool readOnly) && node.TryGetSettingIsHiddenFromUserInterface(key, out bool hidden)
            ? Some(new SettingState(stored, readOnly, hidden))
            : Option<SettingState>.None);

    private static Fin<Option<A>> Present<A>(Option<A> found, PersistentSettings node, string key, Type requested) =>
        found.IsSome || !node.TryGetSettingType(key, out Type stored) ? found : new TypeMismatch(key, stored, requested);

    // --- [WRITES]
    public static IO<Unit> Set(PersistentSettings node, string key, SettingValue value) =>
        IO.lift(() => Writable(node, key)).Bind(_ => value.Switch(
            new Setting(node, key),
            @bool: static (setting, item) => setting.Stored(() => setting.Node.SetBool(setting.Key, item.Value), SettingValue.Bool.Read, item),
            @byte: static (setting, item) => setting.Stored(() => setting.Node.SetByte(setting.Key, item.Value), SettingValue.Byte.Read, item),
            integer: static (setting, item) => setting.Stored(() => setting.Node.SetInteger(setting.Key, item.Value), SettingValue.Integer.Read, item),
            unsignedInteger: static (setting, item) => setting.Stored(() => setting.Node.SetUnsignedInteger(setting.Key, item.Value), SettingValue.UnsignedInteger.Read, item),
            @double: static (setting, item) => setting.Stored(() => setting.Node.SetDouble(setting.Key, item.Value), SettingValue.Double.Read, item),
            @char: static (setting, item) => setting.Stored(() => setting.Node.SetChar(setting.Key, item.Value), SettingValue.Char.Read, item),
            @string: static (setting, item) => setting.Stored(() => setting.Node.SetString(setting.Key, item.Value), SettingValue.String.Read, item),
            stringList: static (setting, item) => item.Values.Exists(static row => string.Equals(row, PersistentSettings.StringListRootKey, StringComparison.Ordinal))
                ? IO.lift(() => setting.Node.SetStringList(setting.Key, [.. item.Values]))
                : setting.Stored(() => setting.Node.SetStringList(setting.Key, [.. item.Values]), SettingValue.StringList.Read, item),
            stringDictionary: static (setting, item) => setting.Stored(() => setting.Node.SetStringDictionary(setting.Key, Pairs(item.Pairs)), SettingValue.StringDictionary.Read, item),
            date: static (setting, item) => setting.Stored(() => setting.Node.SetDate(setting.Key, item.Value), SettingValue.Date.Read, item),
            color: static (setting, item) => setting.Stored(() => setting.Node.SetColor(setting.Key, item.Value), SettingValue.Color.Read, item),
            optionalColor: static (setting, item) => setting.Stored(() => setting.Node.SetColor(setting.Key, item.Value.ToNullable()), SettingValue.OptionalColor.Read, item),
            guid: static (setting, item) => setting.Stored(() => setting.Node.SetGuid(setting.Key, item.Value), SettingValue.Guid.Read, item),
            point: static (setting, item) => setting.Stored(() => setting.Node.SetPoint(setting.Key, item.Value), SettingValue.Point.Read, item),
            point3d: static (setting, item) => setting.Stored(() => setting.Node.SetPoint3d(setting.Key, item.Value), SettingValue.Point3d.Read, item),
            size: static (setting, item) => setting.Stored(() => setting.Node.SetSize(setting.Key, item.Value), SettingValue.Size.Read, item),
            rectangle: static (setting, item) => setting.Stored(() => setting.Node.SetRectangle(setting.Key, item.Value), SettingValue.Rectangle.Read, item)));

    public static IO<Unit> SetEnum<T>(PersistentSettings node, string key, T value) where T : struct, Enum =>
        from keyed in IO.lift(() => Invalid.Unless(key.Length > 0, nameof(PersistentSettings.SetEnumValue)))
        from writable in IO.lift(() => Writable(node, key))
        from written in IO.lift(() => node.SetEnumValue(key, value))
        from stored in FindEnum<T>(node, key)
        from same in IO.lift(Mismatch.Unless(stored == Some(value), nameof(PersistentSettings.TryGetEnumValue)))
        select same;

    public static IO<Unit> Hide(PersistentSettings node, string key) =>
        from present in IO.lift(() => Answers.Found(node.TryGetSettingIsHiddenFromUserInterface(key, out bool hidden), hidden))
        from flagged in IO.lift(present.ToFin(new Missing(nameof(PersistentSettings.HideSettingFromUserInterface))))
        from same in unless(
            flagged,
            IO.lift(() => node.HideSettingFromUserInterface(key))
                .Bind(_ => IO.lift(() => Mismatch.Unless(node.TryGetSettingIsHiddenFromUserInterface(key, out bool now) && now, nameof(PersistentSettings.TryGetSettingIsHiddenFromUserInterface))))).As()
        select same;

    private static Fin<Unit> Writable(PersistentSettings node, string key) =>
        ReadOnlyKey.Unless(!(node.TryGetSettingIsReadOnly(key, out bool readOnly) && readOnly), key);

    internal static KeyValuePair<string, string>[] Pairs(Seq<(string Key, string Value)> pairs) =>
        [.. pairs.Map(static pair => new KeyValuePair<string, string>(pair.Key, pair.Value))];

    internal static IO<Unit> Defaulted<TValue>(PersistentSettings node, string key, Action write, TValue value) where TValue : SettingValue, ISettingDefault<TValue> =>
        Verified(write, IO.lift(() => TValue.ReadDefault(node, key)), value, nameof(PersistentSettings.TryGetDefault));

    private static IO<Unit> Verified<TValue>(Action write, IO<Option<TValue>> read, TValue value, string member) where TValue : SettingValue =>
        from written in IO.lift(write)
        from stored in read
        from same in IO.lift(Mismatch.Unless(stored == Some(value), member))
        select same;

    private sealed record Setting(PersistentSettings Node, string Key) {
        public IO<Unit> Stored<TValue>(Action write, Func<PersistentSettings, string, Seq<string>, Option<TValue>> read, TValue value) where TValue : SettingValue =>
            Verified(write, IO.lift(() => Present(read(Node, Key, Seq<string>()), Node, Key, typeof(TValue))), value, nameof(PersistentSettings.TryGetSettingType));
    }

    // --- [VALIDATORS]
    public static IO<Unit> RegisterSettingsValidator<T>(PersistentSettings node, string key, Func<T, T, bool> accept) =>
        IO.lift(() => node.RegisterSettingsValidator<T>(key, (_, args) => args.Cancel = !accept(args.CurrentValue, args.NewValue)));

    // --- [EVENTS]
    public static IO<IDisposable> OnSaved(PlugIn plugIn, Func<PersistentSettingsSavedEventArgs, IO<Unit>> deliver, Action<Error> reject) =>
        Events.Attach(
            h => plugIn.SettingsSaved += h,
            h => plugIn.SettingsSaved -= h,
            Answers.Handler(deliver, reject));
}
