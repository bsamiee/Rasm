using Rasm.Imaging.Pixels;
using Rasm.Rhino.Persistence.Stores;
using Rhino;
using Rhino.PlugIns;

namespace Rasm.Rhino.Persistence.Settings;

// --- [TYPES] ---------------------------------------------------------------------------
public interface ISettingReader {
    public Fin<TValue> Current<TValue, TRaw>(PlugInSetting<TValue, TRaw> row)
        where TValue : notnull
        where TRaw : notnull;
}

// --- [MODELS] --------------------------------------------------------------------------
public abstract record PlugInSetting {
    internal abstract IO<Unit> Register(PersistentSettings settings);
}

public sealed record PlugInSetting<TValue, TRaw>(ValueKey<TValue, TRaw> Value, SettingType<TRaw> Kind) : PlugInSetting where TRaw : notnull {
    public Applied Applied { get; init; } = Applied.Live;
    public Seq<string> LegacyNames { get; init; }
    public bool Hidden { get; init; }

    internal override IO<Unit> Register(PersistentSettings settings) =>
        from registered in IO.lift(() => Kind.Register(settings, Value.Name, Value.ToRaw(Value.Default), LegacyNames))
            | @catch(static error => error.HasException<NotSupportedException>(), error => IO.fail<TRaw>(SettingType.String.TryGet(settings, Value.Name).Match(
                Some: text => Error.New(ErrorCodes.ParseError, $"{text} reads as no {typeof(TRaw).Name}", error),
                None: () => error)))
        from admitted in IO.lift(() => Value.From(registered))
        from hidden in when(Hidden, IO.lift(() => settings.HideSettingFromUserInterface(Value.Name))).As()
        let refused = fun((TRaw raw) => Value.From(raw).IsFail)
        from validated in IO.lift(() => Kind.Validator.Iter(validator => validator(settings, Value.Name, refused)))
        select unit;
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
        where TRaw : notnull =>
        ValueStore.Of(
            IO.lift(() => node.Current(row)).Map(value => Some(value).Filter(held => !EqualityComparer<TValue>.Default.Equals(held, row.Value.Default))),
            value =>
                from settings in SettingRoots.AddChild(node)
                from writable in IO.lift(() => Writable(settings, row.Value.Name))
                from written in IO.lift(() => row.Kind.Set(settings, row.Value.Name, row.Value.ToRaw(value.IfNone(row.Value.Default))))
                select written,
            row.Applied,
            SettingRoots.Saved(node));

    public static ValueStore<TRecord> Record<TRecord>(SettingsNode node, Applied applied) where TRecord : IStateRecord<TRecord> =>
        ValueStore.Of(
            from texts in Callbacks.Each(
                from leaf in Leaves<TRecord>(node)
                select from found in SettingRoots.TryGetChild(leaf.Parent)
                       select from settings in found
                              from text in SettingType.String.TryGet(settings, leaf.Key)
                              select (leaf.Field.Path, text))
            let stored = toHashMap(texts.Somes())
            from record in FieldTexts.Recalled(typeof(TRecord).Name, TRecord.Parameters, path => IO.pure(stored.Find(path)), TRecord.Default)
            select Some(record).Filter(static held => !EqualityComparer<TRecord>.Default.Equals(held, TRecord.Default)),
            value =>
                from targets in Callbacks.Each(
                    from leaf in Leaves<TRecord>(node)
                    select from found in SettingRoots.TryGetChild(leaf.Parent)
                           select (leaf.Parent, leaf.Key, Found: found, Text: value.Bind(leaf.Field.Capture)))
                let existing = from target in targets from settings in target.Found.ToSeq() select (Settings: settings, target.Key, target.Text)
                from writable in IO.lift(() => Callbacks.Each(existing, static (target, _) => Writable(target.Settings, target.Key)))
                from written in Callbacks.Each(
                    from target in targets
                    where target.Text.IsSome || target.Found.IsSome
                    select
                        from settings in target.Found.Match(Some: IO.pure, None: () => SettingRoots.AddChild(target.Parent))
                        from set in IO.lift(() => SettingType.String.Write(settings, target.Key, target.Text))
                        select set)
                select unit,
            applied,
            SettingRoots.Saved(node));

    private static Seq<(FieldText<TRecord> Field, SettingsNode Parent, string Key)> Leaves<TRecord>(SettingsNode node) where TRecord : IStateRecord<TRecord> =>
        FieldTexts.Items<TRecord>().Choose(field => field.Path.Last.Map(key => (field, node with { Path = node.Path + field.Path.Init }, key)));

    internal static Fin<Unit> Writable(PersistentSettings settings, string key) =>
        settings.TryGetSettingIsReadOnly(key, out bool readOnly) && readOnly ? Error.New($"key {key} is read-only") : unit;

    // --- [SAVES]
    public static IO<Unit> SavePluginSettings(Guid id) =>
        from loaded in IO.lift(() => id == RhinoApp.CurrentRhinoId || (PlugIn.PlugInExists(id, out bool isLoaded, out _) && isLoaded) ? Fin.Succ(unit) : Error.New($"plug-in {id} is not loaded"))
        from saved in IO.lift(() => PlugIn.SavePluginSettings(id))
        select saved;
}
