using Rasm.Rhino.Persistence.Stores;
using Rasm.UI;
using Rhino;
using Rhino.PlugIns;

namespace Rasm.Rhino.Persistence.Settings;

// --- [TYPES] ---------------------------------------------------------------------------
public interface ISettingReader {
    public IO<TValue> Current<TValue, TRaw>(PlugInSetting<TValue, TRaw> row)
        where TValue : notnull
        where TRaw : notnull, ISpanParsable<TRaw>;
}

// --- [MODELS] --------------------------------------------------------------------------
public abstract record PlugInSetting {
    internal abstract IO<Unit> Register(PersistentSettings settings);
}

public sealed record PlugInSetting<TValue, TRaw>(ValueKey<TValue> Key, SettingType<TRaw> Kind) : PlugInSetting
    where TValue : notnull
    where TRaw : notnull, ISpanParsable<TRaw> {
    public Applied Applied { get; init; } = Applied.Live;
    public Seq<string> LegacyNames { get; init; }
    public bool Hidden { get; init; }

    internal override IO<Unit> Register(PersistentSettings settings) =>
        Key.Codec.Entries.Choose(entry => entry.Capture(Key.Default).Map(text => (Entry: entry, Name: EntryKey.Name(entry.Path), Text: text)))
            .TraverseM(slot =>
                from factory in IO.lift(StoredText.Parse<TRaw>(slot.Text))
                from registered in IO.lift(() => Kind.Register(settings, slot.Name, factory, LegacyNames))
                    | @catch(static error => error.HasException<NotSupportedException>(), error => IO.fail<TRaw>(SettingType.String.TryGet(settings, slot.Name).Match(
                        Some: text => Error.New(ErrorCodes.ParseError, $"{text} is no {typeof(TRaw).Name}", error),
                        None: () => error)))
                from accepted in IO.lift(slot.Entry.Recall(StoredText.Format(registered)))
                from hidden in when(Hidden, IO.lift(() => settings.HideSettingFromUserInterface(slot.Name))).As()
                from validated in IO.lift(() => Kind.Validator.Iter(validator => validator(settings, slot.Name, raw => slot.Entry.Recall(StoredText.Format(raw)).IsFail)))
                select unit)
            .As()
            .Map(static _ => unit);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PlugInSettings {
    // --- [REGISTRATION]
    public static IO<Unit> RegisterAll(SettingsNode node, Seq<PlugInSetting> rows) =>
        from settings in SettingRoots.AddChild(node)
        from registered in Callbacks.Each(rows.Map(row => row.Register(settings)))
        select unit;

    // --- [STORES]
    public static ValueStore<TValue> Store<TValue, TRaw>(SettingsNode node, PlugInSetting<TValue, TRaw> row)
        where TValue : notnull
        where TRaw : notnull, ISpanParsable<TRaw> =>
        ValueStore.Of(
            row.Key.Codec.Stored(row.Key.Key, path => IO.lift(() => SettingRoots.Resolved(node)
                    .Traverse(settings => row.Kind.Read(settings, EntryKey.Name(path))).As()
                    .Map(static raw => raw.Flatten().Map(static held => StoredText.Format(held)))))
                .Map(value => value.Filter(held => !EqualityComparer<TValue>.Default.Equals(held, row.Key.Default))),
            value =>
                from settings in SettingRoots.AddChild(node)
                from written in IO.lift(() => Callbacks.Each(row.Key.Codec.Slots(Some(value.IfNone(row.Key.Default))), (slot, _) =>
                    from writable in Writable(settings, EntryKey.Name(slot.Path))
                    from raw in slot.Text.Traverse(StoredText.Parse<TRaw>).As()
                    select row.Kind.Write(settings, EntryKey.Name(slot.Path), raw)))
                select unit,
            row.Applied,
            SettingRoots.Saved(node));

    public static ValueStore<TRecord> Record<TRecord>(SettingsNode node, Applied applied) where TRecord : IStateRecord<TRecord> =>
        new FieldCodec<TRecord>(TRecord.Parameters, TRecord.Default) switch {
            var codec => ValueStore.Of(
                codec.Stored(typeof(TRecord).Name, path => Slot(node, path).Match(
                        Some: static slot => SettingRoots.TryGetChild(slot.Parent).Map(found => found.Bind(settings => SettingType.String.TryGet(settings, slot.Key))),
                        None: static () => IO.pure(Option<string>.None)))
                    .Map(static value => value.Filter(static held => !EqualityComparer<TRecord>.Default.Equals(held, TRecord.Default))),
                value => Callbacks.Each(codec.Slots(value).Choose(slot => Slot(node, slot.Path).Map(held => (held.Parent, held.Key, slot.Text))).Map(slot =>
                    from found in SettingRoots.TryGetChild(slot.Parent)
                    from written in (found.Case, slot.Text.Case) switch {
                        (PersistentSettings settings, _) => IO.lift(() => Writable(settings, slot.Key).Map(_ => SettingType.String.Write(settings, slot.Key, slot.Text))),
                        (_, string) => SettingRoots.AddChild(slot.Parent).Bind(settings => IO.lift(() => SettingType.String.Write(settings, slot.Key, slot.Text))),
                        _ => IO.pure(unit),
                    }
                    select written)).Map(static _ => unit),
                applied,
                SettingRoots.Saved(node)),
        };

    private static Option<(SettingsNode Parent, string Key)> Slot(SettingsNode node, Seq<string> path) =>
        path.Last.Map(key => (node with { Path = node.Path + path.Init }, key));

    internal static Fin<Unit> Writable(PersistentSettings settings, string key) =>
        settings.TryGetSettingIsReadOnly(key, out bool readOnly) && readOnly ? Error.New($"key {key} is read-only") : unit;

    // --- [SAVES]
    public static IO<Unit> SavePluginSettings(Guid id) =>
        from loaded in IO.lift(() => id == RhinoApp.CurrentRhinoId || (PlugIn.PlugInExists(id, out bool isLoaded, out _) && isLoaded) ? Fin.Succ(unit) : Error.New($"plug-in {id} is not loaded"))
        from saved in IO.lift(() => PlugIn.SavePluginSettings(id))
        select saved;
}
