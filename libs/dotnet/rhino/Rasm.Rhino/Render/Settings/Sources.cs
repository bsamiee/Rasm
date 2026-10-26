using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rasm.UI;
using Rhino;
using Rhino.FileIO;
using Rhino.Render;
using Rhino.Render.DataSources;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render.Settings;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IRenderSettingsRecord<TSelf, TParameter> : IStateRecord<TSelf, TParameter>
    where TSelf : IRenderSettingsRecord<TSelf, TParameter>
    where TParameter : class, IStateParameter<TSelf>, ISmartEnum<string, TParameter, ValidationError> {
    public static abstract string Owner { get; }
    public static virtual Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => None;
    public static abstract IO<TSelf> Read(RenderSettingsScope scope);
    public static abstract IO<TSelf> Write(RenderSettingsScope scope, TSelf state);
    public static virtual RowRules Rules(RowSource<TSelf> source, TParameter parameter) => RowRules.Always;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RenderSettingsScope(RenderSettings Settings, LengthUnit Units);

[Union]
public abstract partial record RenderSettingsSource {
    public sealed record Live(RhinoDoc Doc) : RenderSettingsSource;

    public sealed record Section(RhinoDoc Doc, ICollapsibleSection Host) : RenderSettingsSource;

    public sealed record Archive(File3dm File) : RenderSettingsSource;

    public sealed record Detached(RenderSettingsScope Scope) : RenderSettingsSource;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RenderSettingsSources {
    // --- [SCOPES]
    public static IO<T> Read<T>(RenderSettingsSource source, Func<RenderSettingsScope, IO<T>> read) =>
        source.Switch(
            read,
            live: static (body, held) => Opened(held.Doc, body),
            section: static (body, held) => Opened(held.Doc, body),
            archive: static (body, held) => IO.lift(() => new RenderSettingsScope(held.File.Settings.RenderSettings, held.File.Settings.ModelUnits)).Bind(body),
            detached: static (body, held) => body(held.Scope));

    public static IO<T> Edit<T>(RenderSettingsSource source, Func<RenderSettingsScope, IO<T>> edit) =>
        source.Switch(
            edit,
            live: static (body, held) => Opened(held.Doc, live =>
                DisposalOps.Within(live.Settings.Duplicate, staged =>
                    from edited in body(live with { Settings = staged })
                    from recorded in Recorded(held.Doc, live.Settings, staged)
                    from committed in IO.lift(() => { held.Doc.RenderSettings = staged; })
                    select edited)),
            section: static (body, held) => SectionModels.Write(held.Host, Provider.RhinoSettings, data =>
                DisposalOps.Found(data.GetRenderSettings, nameof(RhinoSettings.GetRenderSettings), lent =>
                    from edited in body(new RenderSettingsScope(lent, held.Doc.ModelUnits))
                    from committed in IO.lift(() => data.SetRenderSettings(lent))
                    select edited)),
            archive: static (body, held) => Read(held, body),
            detached: static (body, held) => Read(held, body));

    private static IO<T> Opened<T>(RhinoDoc doc, Func<RenderSettingsScope, IO<T>> body) =>
        DisposalOps.Within(() => doc.RenderSettings, settings => body(new RenderSettingsScope(settings, doc.ModelUnits)));

    private static IO<Unit> Recorded(RhinoDoc doc, RenderSettings prior, RenderSettings next) =>
        from open in IO.lift(() => doc.UndoRecordingIsActive)
        from recorded in when(open, IO.lift(() => (Prior: prior.Duplicate(), Next: next.Duplicate())).Bind(held => Undoable(doc, held.Prior, held.Next))).As()
        select recorded;

    private static IO<Unit> Undoable(RhinoDoc doc, RenderSettings restored, RenderSettings replaced) =>
        IO.lift(() => Refused.Unless(
            doc.AddCustomUndoEvent(nameof(RhinoDoc.RenderSettings), (_, args) => Restored(args.Document, restored, replaced).Run()),
            nameof(RhinoDoc.AddCustomUndoEvent)));

    private static IO<Unit> Restored(RhinoDoc doc, RenderSettings restored, RenderSettings replaced) =>
        from registered in Undoable(doc, replaced, restored)
        from assigned in IO.lift(() => { doc.RenderSettings = restored; })
        select assigned;

    // --- [STORES]
    public static ValueStore<TRecord> Store<TRecord, TParameter>(RenderSettingsSource source)
        where TRecord : IRenderSettingsRecord<TRecord, TParameter>
        where TParameter : class, IStateParameter<TRecord>, ISmartEnum<string, TParameter, ValidationError> =>
        Read(source, static scope => TRecord.Read(scope)).Map(Held) switch {
            var read => new(read,
                value =>
                    from before in read
                    from after in before == value.Bind(Held) ? IO.pure(before) : Edit(source, scope => TRecord.Write(scope, value.IfNone(TRecord.Default))).Map(Held)
                    select ValueStore.Between(before, after),
                EqualityComparer<TRecord>.Default.Equals, Applied.Live, Signal(source, TRecord.Changed)),
        };

    private static Option<TRecord> Held<TRecord>(TRecord record) where TRecord : IStateRecord<TRecord> =>
        Some(record).Filter(static held => !EqualityComparer<TRecord>.Default.Equals(held, TRecord.Default));

    public static Option<HostEvent<Unit>> Signal(RenderSettingsSource source, Option<DocumentEvent<RenderPropertyChangedEvent>> changed) =>
        from row in changed
        from serial in source.Switch(
            live: static held => Some(held.Doc.RuntimeSerialNumber),
            section: static held => Some(held.Doc.RuntimeSerialNumber),
            archive: static _ => Option<uint>.None,
            detached: static _ => Option<uint>.None)
        select ValueStore.Signal(row.In(serial), static _ => true);

    public static Option<HostEvent<Unit>> DictionaryChanged(RowScope scope) =>
        scope.Document.Map(static doc => ValueStore.Signal(EventKind.DocumentPropertiesChanged.In(doc.RuntimeSerialNumber), static _ => true));

    public static DictionaryOwner Dictionary(RenderSettingsSource source) =>
        new(Read(source, static scope => ArchivableDictionaries.UserDictionary(scope.Settings).Map(static held => held.Clone())),
            edit => Edit(source, scope => ArchivableDictionaries.UserDictionary(scope.Settings).Bind(target => IO.lift(() => edit(target))))) {
            Document = source.Switch(
                live: static held => Some(held.Doc),
                section: static _ => Option<RhinoDoc>.None,
                archive: static _ => Option<RhinoDoc>.None,
                detached: static _ => Option<RhinoDoc>.None),
        };

    // --- [BINDINGS]
    public static BindingGroup Bindings(RenderSettingsSource source) =>
        new(Rows(source).Map(static row => row.Binding));

    public static IO<ValueDiff> Copy(RenderSettingsSource source, RenderSettingsSource target) =>
        from values in Bindings(source).Capture
        from diff in Edit(target, staged =>
            from bindings in IO.pure(Bindings(staged))
            from applied in bindings.Apply(values, [.. bindings.Keys().Filter(static key => string.Equals(key.Owner, RenderSourceState.Owner, StringComparison.Ordinal))])
            select applied)
        select diff;

    public static IO<Unit> Reset(RenderSettingsSource source) =>
        Edit(source, static staged => Callbacks.Each(Rows(staged).Map(static row => row.Reset)).Map(unit));

    private static Seq<(ValueBinding Binding, IO<Unit> Reset)> Rows(RenderSettingsSource source) =>
        [
            Row<ImageOutputState, ImageOutputParameter>(source), Row<RenderSourceState, RenderSourceParameter>(source), Row<FrameState, FrameParameter>(source),
            Row<LinearWorkflowState, LinearWorkflowParameter>(source), Row<DitheringState, DitheringParameter>(source), Row<RenderChannelsState, RenderChannelsParameter>(source),
            Row<BackgroundState, BackgroundParameter>(source), Row<LightingState, LightingParameter>(source), Row<SafeFrameState, SafeFrameParameter>(source),
            Row<GroundPlaneState, GroundPlaneParameter>(source), Row<SunState, SunParameter>(source),
            .. Dictionary(source) switch { var dictionary => toSeq(CyclesKey.Items).Map(item => item.Entry(dictionary)) },
        ];

    private static (ValueBinding Binding, IO<Unit> Reset) Row<TRecord, TParameter>(RenderSettingsSource source)
        where TRecord : IRenderSettingsRecord<TRecord, TParameter>
        where TParameter : class, IStateParameter<TRecord>, ISmartEnum<string, TParameter, ValidationError> =>
        Store<TRecord, TParameter>(source) switch {
            var store => (ValueBinding.Of(TRecord.Owner, new FieldCodec<TRecord>(TRecord.Parameters, TRecord.Default), store), store.Write(None).Map(unit)),
        };

    // --- [ROWS]
    public static Seq<ControlRow> Fields<TRecord, TParameter>(Seq<TParameter> items)
        where TRecord : IRenderSettingsRecord<TRecord, TParameter>
        where TParameter : class, IStateParameter<TRecord>, ISmartEnum<string, TParameter, ValidationError> =>
        RowSource.Fields(TRecord.Owner, [.. items], static scope => IO.pure(scope.Document.ToSeq().Map(static doc => Store<TRecord, TParameter>(doc)))) switch {
            var source => new ParameterRows<TRecord>(source, parameter => TRecord.Rules(source, TParameter.Get(parameter.Key))).Of(items),
        };
}
