using Rasm.Imaging.Pixels;
using Rasm.Rhino.Persistence.Stores;
using Rhino;
using Rhino.PlugIns;

namespace Rasm.Rhino.Persistence.Settings;

// --- [MODELS] --------------------------------------------------------------------------
public abstract record PlugInSetting {
    internal abstract IO<Unit> Register(SettingsNode node);
}

public sealed record PlugInSetting<TValue, TRaw, TError>(
    ValueKey<TValue, TRaw, TError> Value,
    SettingType<TRaw> Kind,
    Applied Applied,
    Seq<string> LegacyNames,
    bool Hidden) : PlugInSetting
    where TRaw : notnull {
    internal override IO<Unit> Register(SettingsNode node) =>
        from settings in SettingRoots.AddChild(node)
        from registered in IO.lift(() => Kind.Register(settings, Value.Name, Value.ToRaw(Value.Default), LegacyNames))
            | @catch(static error => error.HasException<NotSupportedException>(), error => IO.lift(() => Fin.Fail<Unit>(new UnreadText(settings.GetString(Value.Name), typeof(TRaw), Some(error)))))
        from hidden in when(Hidden, IO.lift(() => settings.HideSettingFromUserInterface(Value.Name))).As()
        from validated in IO.lift(() => Kind.Validator.Iter(validator => validator(settings, Value.Name, raw => Value.From(raw).IsFail)))
        select unit;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PlugInSettings {
    // --- [REGISTRATION]
    public static IO<Unit> RegisterAll(SettingsNode node, Seq<PlugInSetting> rows) =>
        Callbacks.Each(rows.Map(row => row.Register(node))).Map(static _ => unit);

    // --- [STORES]
    public static ValueStore<TValue> Store<TValue, TRaw, TError>(SettingsNode node, PlugInSetting<TValue, TRaw, TError> row)
        where TValue : notnull
        where TRaw : notnull =>
        ValueStore.Of(
            from found in SettingRoots.TryGetChild(node)
            from held in IO.lift(() => found.Traverse(settings => row.Kind.Read(settings, row.Value.Name).Bind(raw => raw.Traverse(row.Value.From).As())).As())
            select held.Flatten().Filter(value => !EqualityComparer<TValue>.Default.Equals(value, row.Value.Default)),
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
            from texts in Leaves<TRecord>(node)
                .TraverseM(static leaf => SettingRoots.TryGetChild(leaf.Parent).Map(found => found.Bind(settings => SettingType.String.TryGet(settings, leaf.Key)).Map(text => (leaf.Field.Path, text))))
                .As()
            from record in FieldTexts.Recalled<TRecord>(typeof(TRecord).Name, toHashMap(texts.Somes()).Find)
            select Some(record).Filter(static held => !EqualityComparer<TRecord>.Default.Equals(held, TRecord.Default)),
            value =>
                from targets in Leaves<TRecord>(node)
                    .TraverseM(leaf => SettingRoots.TryGetChild(leaf.Parent).Map(found => (leaf.Parent, leaf.Key, Found: found, Text: value.Bind(leaf.Field.Capture))))
                    .As()
                from writable in IO.lift(() => Callbacks.Each(targets, static (target, _) => target.Found.Traverse(settings => Writable(settings, target.Key)).As()))
                from written in targets
                    .Choose(static target => target.Text.Map(text => (target.Parent, target.Key, Text: text)))
                    .TraverseM(static target =>
                        from settings in SettingRoots.AddChild(target.Parent)
                        from set in IO.lift(() => SettingType.String.Set(settings, target.Key, target.Text))
                        select set)
                    .As()
                from deleted in IO.lift(() => targets.Filter(static target => target.Text.IsNone).Iter(static target => target.Found.Iter(settings => settings.DeleteItem(target.Key))))
                select unit,
            applied,
            SettingRoots.Saved(node));

    private static Seq<(FieldText<TRecord> Field, SettingsNode Parent, string Key)> Leaves<TRecord>(SettingsNode node) where TRecord : IStateRecord<TRecord> =>
        FieldTexts<TRecord>.Items.Choose(field => field.Path.Last.Map(key => (field, node with { Path = node.Path + field.Path.Init }, key)));
    private static Fin<Unit> Writable(PersistentSettings settings, string key) =>
        ReadOnlyKey.Unless(!(settings.TryGetSettingIsReadOnly(key, out bool readOnly) && readOnly), key);

    // --- [SAVES]
    public static IO<Unit> SavePluginSettings(Guid id) =>
        from loaded in IO.lift(() => Unloaded.Unless(id == RhinoApp.CurrentRhinoId || (PlugIn.PlugInExists(id, out bool isLoaded, out _) && isLoaded), id))
        from saved in IO.lift(() => PlugIn.SavePluginSettings(id))
        select saved;
}
