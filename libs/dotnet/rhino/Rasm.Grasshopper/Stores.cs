using Grasshopper2;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;

namespace Rasm.Grasshopper;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record KeyedText(
    string Owner,
    Func<string, Fin<Option<string>>> Read,
    Func<Seq<(string Key, Option<string> Text)>, IO<Unit>> Write,
    Option<HostEvent<KeyValueEventArgs>> Changed) : ISettingReader {
    // --- [KEYS]
    public string Pattern => Key("*");

    public string Key(string name) => EntryKey.Name(Seq(Owner, name));

    public Fin<TValue> Current<TValue, TRaw>(PlugInSetting<TValue, TRaw> row)
        where TValue : notnull
        where TRaw : notnull =>
        Current(row.Value);

    public Fin<TValue> Current<TValue, TRaw>(ValueKey<TValue, TRaw> key)
        where TValue : notnull
        where TRaw : notnull =>
        Read(Key(key.Name)).Bind(stored => stored.Traverse(value => EntryKey.Named(Owner, key.Name).Read(value, key.Read)).As()).Map(held => held.IfNone(key.Default));

    // --- [HOSTS]
    public static KeyedText Of(KeyedValues values, string owner) =>
        new(owner,
            key => Optional(values.Get(key, @default: null)),
            entries => IO.lift(() => entries.Iter(entry => entry.Text.Match(
                Some: text => values.Set(entry.Key, text),
                None: () => values.Delete(entry.Key)))),
            Some(GrasshopperEvents.Stored(values)));

    public static KeyedText Of(SettingsFile file, string owner) =>
        new(owner,
            key => Optional(file.Node.FindItem(key)).Traverse(item => Conversions.Typed<string>(item.RawData, key)).As(),
            entries =>
                from set in IO.lift(() => entries.Iter(entry => entry.Text.Match(
                    Some: text => file.Set(entry.Key, text),
                    None: () => file.Node.RemoveItem(entry.Key))))
                from saved in GrasshopperStores.Saved(file)
                select saved,
            None);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GrasshopperStores {
    // --- [SETTINGS]
    public static ValueStore<T> Of<T>(Settings.Setting<T> setting) where T : notnull {
        Func<T, T, bool> same = EqualityComparer<T>.Default.Equals;
        Func<T, Option<T>> stored = value => Some(value).Filter(held => !same(held, setting.Default));
        return new(
            IO.lift(() => stored(setting.Value)),
            value =>
                from before in IO.lift(() => stored(setting.Value))
                from after in IO.lift(() => {
                    setting.Value = value.IfNone(setting.Default);
                    return stored(setting.Value);
                })
                select ValueStore.Between(before, after, same),
            same,
            Applied.Live,
            Some(ValueStore.Signal(GrasshopperEvents.SettingChanged(setting), static _ => true)));
    }

    public static IO<Unit> Saved(SettingsFile file) =>
        IO.lift(() => Refused.Unless(Settings.SandBox || file.TrySaveSettingsToFile(), nameof(SettingsFile.TrySaveSettingsToFile)));

    // --- [KEYS]
    public static ValueStore<TValue> Of<TValue, TRaw>(KeyedText text, ValueKey<TValue, TRaw> key)
        where TValue : notnull
        where TRaw : notnull {
        string host = text.Key(key.Name);
        string factory = key.Text(key.Default);
        return ValueStore.Defaulted(
                IO.lift(() => text.Current(key)),
                IO.pure(key.Default),
                value => text.Write(Seq((host, Some(key.Text(value)).Filter(stored => !string.Equals(stored, factory, StringComparison.Ordinal))))),
                EqualityComparer<TValue>.Default.Equals,
                Applied.Live,
                text.Changed.Map(row => ValueStore.Signal(row, args => args.Key.WildcardMatch(host, StringComparison.InvariantCultureIgnoreCase))));
    }

    // --- [RECORDS]
    public static ValueStore<TRecord> Of<TRecord>(KeyedText text, TRecord fallback) where TRecord : IStateRecord<TRecord> =>
        ValueStore.Defaulted(
            FieldTexts.Recalled(text.Owner, TRecord.Parameters, path => IO.lift(() => text.Read(text.Key(EntryKey.Name(path)))), fallback),
            IO.pure(fallback),
            value => text.Write(
                from item in FieldTexts.Items<TRecord>()
                let stored = item.Capture(value)
                select (text.Key(EntryKey.Name(item.Path)), stored == item.Capture(fallback) ? None : stored)),
            EqualityComparer<TRecord>.Default.Equals,
            Applied.Live,
            text.Changed.Map(row => ValueStore.Signal(row, args => args.Key.WildcardMatch(text.Pattern, StringComparison.InvariantCultureIgnoreCase))));
}
