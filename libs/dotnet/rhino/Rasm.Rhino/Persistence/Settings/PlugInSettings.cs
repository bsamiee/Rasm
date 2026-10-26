using System.Text.Json;
using Rasm.Rhino.Persistence.Stores;
using Rhino;
using Rhino.PlugIns;

namespace Rasm.Rhino.Persistence.Settings;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate bool SettingRead<T>(PersistentSettings settings, string key, out T value, IEnumerable<string> legacyKeyList);

public interface ISettingReader {
    public IO<T> Current<T>(PlugInSetting<T> row) where T : notnull;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PlugInSetting<T>(string Key, T Default, SettingRead<T> TryGet, Action<PersistentSettings, string, T> Set) where T : notnull {
    public Applied Applied { get; init; } = Applied.Live;
    public Seq<string> LegacyNames { get; init; }
    public bool Hidden { get; init; }

    public Option<T> Read(PersistentSettings settings) =>
        Callbacks.Found(TryGet(settings, Key, out T value, LegacyNames), value);

    internal IO<Unit> Register(PersistentSettings settings) =>
        when(Hidden, IO.lift(() => settings.HideSettingFromUserInterface(Key))).As();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PlugInSettings {
    // --- [REGISTRATION]
    public static IO<Unit> RegisterAll<T>(SettingsNode node, Seq<PlugInSetting<T>> rows) where T : notnull =>
        from settings in SettingRoots.AddChild(node)
        from registered in Callbacks.Each(rows.Map(row => row.Register(settings)))
        select unit;

    // --- [STORES]
    public static ValueStore<T> Store<T>(SettingsNode node, PlugInSetting<T> row) where T : notnull =>
        ValueStores.Of(
            IO.lift(() => SettingRoots.Resolved(node).Bind(row.Read).Filter(held => !EqualityComparer<T>.Default.Equals(held, row.Default))),
            value =>
                from settings in SettingRoots.AddChild(node)
                from writable in IO.lift(() => Writable(settings, row.Key))
                from written in IO.lift(() => row.Set(settings, row.Key, value.IfNone(row.Default)))
                select written,
            row.Applied,
            SettingRoots.Saved(node));

    public static ValueStore<TRecord> Record<TRecord>(SettingsNode node, TRecord @default, Applied applied) where TRecord : notnull =>
        typeof(TRecord).Name switch {
            var key => ValueStores.Of(
                from found in SettingRoots.TryGetChild(node)
                from text in IO.lift(() => found.Bind(settings => settings.TryGetString(key, out string? held) ? Optional(held) : None))
                from value in IO.lift(() => text.Traverse(held => ArchivableDictionaries.Decoded(key, () => JsonSerializer.Deserialize<TRecord>(held, ArchivableDictionaries.Options))).As())
                select value.Filter(held => !EqualityComparer<TRecord>.Default.Equals(held, @default)),
                value =>
                    from settings in SettingRoots.AddChild(node)
                    from writable in IO.lift(() => Writable(settings, key))
                    from written in IO.lift(() => value.Match(Some: held => settings.SetString(key, JsonSerializer.Serialize(held, ArchivableDictionaries.Options)), None: () => settings.DeleteItem(key)))
                    select written,
                applied,
                SettingRoots.Saved(node)),
        };

    internal static Fin<Unit> Writable(PersistentSettings settings, string key) =>
        settings.TryGetSettingIsReadOnly(key, out bool readOnly) && readOnly ? Error.New($"key {key} is read-only") : unit;

    // --- [SAVES]
    public static IO<Unit> SavePluginSettings(Guid id) =>
        from loaded in IO.lift(() => id == RhinoApp.CurrentRhinoId || (PlugIn.PlugInExists(id, out bool isLoaded, out _) && isLoaded) ? Fin.Succ(unit) : Error.New($"plug-in {id} is not loaded"))
        from saved in IO.lift(() => PlugIn.SavePluginSettings(id))
        select saved;
}
