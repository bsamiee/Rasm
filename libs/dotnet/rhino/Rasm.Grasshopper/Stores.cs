using Grasshopper2;
using Grasshopper2.Doc;
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
    public string Pattern => Key(["*"]);

    public string Key(Seq<string> path) => EntryKey.Name(Owner.Cons(path));

    // --- [STORES]
    public ValueStore<TValue> Store<TValue>(FieldCodec<TValue> codec) where TValue : notnull =>
        codec.Capture(codec.Default) switch {
            var factory => ValueStore.Defaulted(
                codec.Recalled(Owner, path => IO.lift(() => Read(Key(path)))),
                IO.pure(codec.Default),
                value => Write(codec.Slots(Some(value)).Map(slot =>
                    (Key(slot.Path), slot.Text.Filter(stored => !factory.Find(slot.Path).Exists(held => string.Equals(held, stored, StringComparison.Ordinal)))))),
                EqualityComparer<TValue>.Default.Equals,
                Applied.Live,
                Changed.Map(row => ValueStore.Signal(row, args => codec.Entries.Exists(entry => string.Equals(Key(entry.Path), args.Key, StringComparison.OrdinalIgnoreCase))))),
        };

    public IO<TValue> Current<TValue, TRaw>(PlugInSetting<TValue, TRaw> row)
        where TValue : notnull
        where TRaw : notnull, ISpanParsable<TRaw> =>
        Store(row.Key.Codec).Read.Map(held => held.IfNone(row.Key.Default));

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
}
