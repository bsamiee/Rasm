using Rasm.Imaging.Pixels;
using Rasm.Rhino.Persistence.Stores;
using Rhino;
using Rhino.PlugIns;

namespace Rasm.Rhino.Persistence.Settings;

// --- [MODELS] --------------------------------------------------------------------------
public abstract record PlugInSetting {
    internal abstract IO<Unit> Register(PersistentSettings settings);
}

public sealed record PlugInSetting<TValue, TRaw, TError>(
    ValueKey<TValue, TRaw, TError> Value,
    SettingType<TRaw> Kind,
    Applied Applied,
    Seq<string> LegacyNames,
    bool Hidden) : PlugInSetting
    where TRaw : notnull {
    internal override IO<Unit> Register(PersistentSettings settings) =>
        from registered in IO.lift(() => Kind.Register(settings, Value.Name, Value.ToRaw(Value.Default), LegacyNames))
            | @catch(static error => error.HasException<NotSupportedException>(), error => IO.lift(() => Fin.Fail<TRaw>(new UnreadText(settings.GetString(Value.Name), typeof(TRaw), Some(error)))))
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
    public static ValueStore<TValue> Store<TValue, TRaw, TError>(SettingsNode node, PlugInSetting<TValue, TRaw, TError> row)
        where TValue : notnull
        where TRaw : notnull =>
        ValueStore.Of(
            from found in SettingRoots.TryGetChild(node)
            from raw in IO.lift(() => found.Traverse(settings => row.Kind.Read(settings, row.Value.Name)).As())
            from held in IO.lift(() => raw.Flatten().Traverse(row.Value.From).As())
            select held.Filter(value => !EqualityComparer<TValue>.Default.Equals(value, row.Value.Default)),
            value =>
                from settings in SettingRoots.AddChild(node)
                from writable in IO.lift(() => Writable(settings, row.Value.Name))
                from written in IO.lift(() => row.Kind.Set(settings, row.Value.Name, row.Value.ToRaw(value.IfNone(row.Value.Default))))
                select written,
            row.Applied,
            SettingRoots.Saved(node));

    public static IO<TValue> Current<TValue, TRaw, TError>(SettingsNode node, PlugInSetting<TValue, TRaw, TError> row)
        where TValue : notnull
        where TRaw : notnull =>
        Store(node, row).Read.Map(held => held.IfNone(row.Value.Default));

    public static ValueStore<TRecord> Record<TRecord>(SettingsNode node, Applied applied) where TRecord : IStateRecord<TRecord> =>
        ValueStore.Of(
            from texts in Callbacks.Each(
                from leaf in Leaves<TRecord>(node)
                select from found in SettingRoots.TryGetChild(leaf.Parent)
                       select from settings in found
                              from text in SettingType.String.TryGet(settings, leaf.Key)
                              select (leaf.Field.Path, text))
            from record in FieldTexts.Recalled<TRecord>(typeof(TRecord).Name, toHashMap(texts.Somes()).Find)
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
        FieldTexts<TRecord>.Items.Choose(field => field.Path.Last.Map(key => (field, node with { Path = node.Path + field.Path.Init }, key)));
    internal static Fin<Unit> Writable(PersistentSettings settings, string key) =>
        ReadOnlyKey.Unless(!(settings.TryGetSettingIsReadOnly(key, out bool readOnly) && readOnly), key);

    // --- [SAVES]
    public static IO<Unit> SavePluginSettings(Guid id) =>
        from loaded in IO.lift(() => Unloaded.Unless(id == RhinoApp.CurrentRhinoId || (PlugIn.PlugInExists(id, out bool isLoaded, out _) && isLoaded), id))
        from saved in IO.lift(() => PlugIn.SavePluginSettings(id))
        select saved;
}
