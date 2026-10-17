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
    Func<string, IO<Option<string>>> Read,
    Func<Seq<(string Key, Option<string> Text)>, IO<Unit>> Write,
    Option<HostEvent<KeyValueEventArgs>> Changed) : ISettingReader {
    // --- [KEYS]
    public string Pattern => Key("*");

    public string Key(string name) => EntryKey.Name(Seq(Owner, name));

    public IO<TValue> Current<TValue, TRaw>(PlugInSetting<TValue, TRaw> row)
        where TValue : notnull
        where TRaw : notnull =>
        GrasshopperStores.Of(this, row.Value).Read.Map(held => held.IfNone(row.Value.Default));

    // --- [HOSTS]
    public static KeyedText Of(KeyedValues values, string owner) =>
        new(owner,
            key => IO.lift(() => Optional(values.Get(key, @default: null))),
            entries => IO.lift(() => entries.Iter(entry => entry.Text.Match(
                Some: text => values.Set(entry.Key, text),
                None: () => values.Delete(entry.Key)))),
            Some(GrasshopperEvents.Stored(values)));

    public static KeyedText Of(SettingsFile file, string owner) =>
        new(owner,
            key => IO.lift(() => Optional(file.Node.FindItem(key)).Traverse(item => Conversions.Typed<string>(item.RawData, key)).As()),
            entries =>
                from set in IO.lift(() => entries.Iter(entry => entry.Text.Match(
                    Some: text => file.Set(entry.Key, text),
                    None: () => file.Node.RemoveItem(entry.Key))))
                from saved in IO.lift(() => Refused.Unless(file.TrySaveSettingsToFile(), nameof(SettingsFile.TrySaveSettingsToFile)))
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

    // --- [KEYS]
    public static ValueStore<TValue> Of<TValue, TRaw>(KeyedText text, ValueKey<TValue, TRaw> key)
        where TValue : notnull
        where TRaw : notnull {
        string host = text.Key(key.Name);
        EntryKey entry = EntryKey.Named(text.Owner, key.Name);
        string factory = key.Text(key.Default);
        return ValueStore.Defaulted(
                from stored in text.Read(host)
                from held in IO.lift(() => stored.Traverse(value => entry.Read(value, key.Read)).As())
                select held.IfNone(key.Default),
                IO.pure(key.Default),
                value => text.Write(Seq((host, Some(key.Text(value)).Filter(stored => !string.Equals(stored, factory, StringComparison.Ordinal))))),
                EqualityComparer<TValue>.Default.Equals,
                Applied.Live,
                text.Changed.Map(row => ValueStore.Signal(row, args => args.Key.WildcardMatch(host, StringComparison.InvariantCultureIgnoreCase))));
    }

    // --- [RECORDS]
    public static ValueStore<TRecord> Of<TRecord>(KeyedText text, TRecord fallback) where TRecord : IStateRecord<TRecord> =>
        ValueStore.Defaulted(
            FieldTexts.Recalled(text.Owner, TRecord.Parameters, path => text.Read(text.Key(EntryKey.Name(path))), fallback),
            IO.pure(fallback),
            value => text.Write(
                from item in FieldTexts.Items<TRecord>()
                let stored = item.Capture(value)
                select (text.Key(EntryKey.Name(item.Path)), stored == item.Capture(fallback) ? None : stored)),
            EqualityComparer<TRecord>.Default.Equals,
            Applied.Live,
            text.Changed.Map(row => ValueStore.Signal(row, args => args.Key.WildcardMatch(text.Pattern, StringComparison.InvariantCultureIgnoreCase))));
}
