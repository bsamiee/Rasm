using System.Text.Json;
using Grasshopper2;
using Grasshopper2.Doc;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;

namespace Rasm.Grasshopper;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record KeyedText(
    string Owner,
    Func<string, Fin<Option<string>>> Read,
    Func<Seq<(string Key, Option<string> Text)>, IO<Unit>> Write,
    Option<HostEvent<KeyValueEventArgs>> Changed) {
    // --- [STORES]
    public ValueStore<TValue> Store<TValue>(TValue @default) where TValue : notnull =>
        ValueStores.Of(
            from text in IO.lift(() => Read(Owner))
            from value in IO.lift(() => text.Traverse(held => ArchivableDictionaries.Decoded(Owner, () => JsonSerializer.Deserialize<TValue>(held, ArchivableDictionaries.Options))).As())
            select value.Filter(held => !EqualityComparer<TValue>.Default.Equals(held, @default)),
            value => Write([(Owner, value.Map(static held => JsonSerializer.Serialize(held, ArchivableDictionaries.Options)))]),
            Applied.Live,
            Changed.Map(row => ValueStores.Signal(row, args => string.Equals(args.Key, Owner, StringComparison.OrdinalIgnoreCase))));

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
                select ValueStores.Between(before, after, same),
            same,
            Applied.Live,
            Some(ValueStores.Signal(GrasshopperEvents.SettingChanged(setting), static _ => true)));
    }

    public static IO<Unit> Saved(SettingsFile file) =>
        IO.lift(() => Refused.Unless(Settings.SandBox || file.TrySaveSettingsToFile(), nameof(SettingsFile.TrySaveSettingsToFile)));
}
