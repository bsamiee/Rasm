using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Render.DataSources;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render.Scenes;

// --- [TYPES] ---------------------------------------------------------------------------
public interface ISceneRecord<TSelf> : IStateRecord<TSelf> where TSelf : ISceneRecord<TSelf> {
    public static abstract string Owner { get; }
    public static abstract Option<DocumentEvent<RenderPropertyChangedEvent>> Changed { get; }
    public static abstract IO<TSelf> Read(SceneWindow window);
    public static abstract IO<Unit> Write(SceneWindow window, TSelf record);
}

public interface ISceneRecord<TSelf, TParameter> : ISceneRecord<TSelf>, IStateRecord<TSelf, TParameter, InvalidRhinoValue>
    where TSelf : ISceneRecord<TSelf, TParameter>
    where TParameter : class, IStateParameter<TSelf>, ISmartEnum<string, TParameter, InvalidRhinoValue> {
    public static abstract ParameterText Text(TParameter parameter);
    public static abstract RowRules Rules(RowSource<TSelf> source, TParameter parameter);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SceneWindow(RenderSettings Settings, LengthUnit Units);

[Union]
public abstract partial record SceneSource {
    public sealed record Live(RhinoDoc Doc) : SceneSource;

    public sealed record Section(RhinoDoc Doc, ICollapsibleSection Host) : SceneSource;

    public sealed record Archive(File3dm File) : SceneSource;

    public sealed record Detached(SceneWindow Window) : SceneSource;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SceneSources {
    // --- [WINDOWS]
    public static IO<T> Read<T>(SceneSource source, Func<SceneWindow, IO<T>> read) =>
        source.Switch(
            read,
            live: static (body, held) => Opened(held.Doc, body),
            section: static (body, held) => Opened(held.Doc, body),
            archive: static (body, held) => IO.lift(() => new SceneWindow(held.File.Settings.RenderSettings, held.File.Settings.ModelUnits)).Bind(body),
            detached: static (body, held) => body(held.Window));

    public static IO<T> Edit<T>(SceneSource source, Func<SceneWindow, IO<T>> edit) =>
        source.Switch(
            edit,
            live: static (body, held) => Opened(held.Doc, live =>
                (from staged in use(live.Settings.Duplicate)
                 from edited in body(live with { Settings = staged })
                 from committed in IO.lift(() => { held.Doc.RenderSettings = staged; })
                 select edited).Bracket()),
            section: static (body, held) => SectionModels.Write(held.Host, Provider.RhinoSettings, data =>
                (from lent in use(IO.lift(() => Missing.Unless(data.GetRenderSettings(), nameof(RhinoSettings.GetRenderSettings))))
                 from edited in body(new SceneWindow(lent, held.Doc.ModelUnits))
                 from committed in IO.lift(() => data.SetRenderSettings(lent))
                 select edited).Bracket()),
            archive: static (body, held) => Read(held, body),
            detached: static (body, held) => Read(held, body));

    private static IO<T> Opened<T>(RhinoDoc doc, Func<SceneWindow, IO<T>> body) =>
        use(() => doc.RenderSettings).Bind(settings => body(new SceneWindow(settings, doc.ModelUnits))).Bracket();

    // --- [SUB_OWNERS]
    public static IO<T> SubOwner<THost, T>(SceneWindow window, Func<RenderSettings, THost> select, Func<THost, IO<T>> body) where THost : IDisposable =>
        use(() => select(window.Settings)).Bind(body).Bracket();

    // --- [STORES]
    public static ValueStore<TRecord> Store<TRecord>(SceneSource source) where TRecord : ISceneRecord<TRecord> =>
        ValueStore.Of(
            Read(source, static window => TRecord.Read(window)).Map(static record => Some(record).Filter(static held => !EqualityComparer<TRecord>.Default.Equals(held, TRecord.Default))),
            value => Edit(source, window => TRecord.Write(window, value.IfNone(TRecord.Default))),
            Applied.Live,
            Signal(source, TRecord.Changed));

    public static Option<HostEvent<Unit>> Signal(SceneSource source, Option<DocumentEvent<RenderPropertyChangedEvent>> changed) =>
        from row in changed
        from serial in source.Switch(
            live: static held => Some(held.Doc.RuntimeSerialNumber),
            section: static held => Some(held.Doc.RuntimeSerialNumber),
            archive: static _ => Option<uint>.None,
            detached: static _ => Option<uint>.None)
        select ValueStore.Signal(row.In(serial), static _ => true);

    public static DictionaryOwner Dictionary(SceneSource source) =>
        new(Read(source, static window => ArchivableDictionaries.UserDictionary(window.Settings).Map(static held => held.Clone())),
            edit => Edit(source, window => ArchivableDictionaries.UserDictionary(window.Settings).Bind(target => IO.lift(() => edit(target)))));

    // --- [ARRIVALS]
    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register<TRecord>(Func<TRecord, TRecord> next) where TRecord : ISceneRecord<TRecord> =>
        (_, sink) => EventKind.DocumentArrived.Choose(static serial => Some((serial, unit))).Through(Subscriptions.Idle<uint, Unit>(arrived => Arrived(arrived, next)), sink);

    private static IO<Unit> Arrived<TRecord>(HashMap<uint, Unit> arrived, Func<TRecord, TRecord> next) where TRecord : ISceneRecord<TRecord> =>
        from stores in IO.lift(() => toSeq(arrived.Keys).Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial))).Map(static doc => Store<TRecord>(doc)).Strict())
        from written in Callbacks.Each(stores.Map(store => store.Read
            .Map(held => next(held.IfNone(TRecord.Default)))
            .Bind(record => store.Write(Callbacks.Found(!EqualityComparer<TRecord>.Default.Equals(record, TRecord.Default), record)))))
        select unit;

    // --- [BINDINGS]
    public static Validation<Error, BindingGroup> Bindings(SceneSource source) =>
        BindingGroup.Of(Rows(source).Map(static row => row.Binding));

    public static IO<ValueDiff> Copy(SceneSource source, SceneSource target) =>
        from values in IO.lift(Bindings(source).ToFin()).Bind(static captured => captured.Capture)
        from diff in Edit(target, staged =>
            from bindings in IO.lift(Bindings(staged).ToFin())
            from applied in bindings.Apply(values, toHashSet(bindings.Keys().Filter(static key => string.Equals(key.Owner, RenderSourceState.Owner, StringComparison.Ordinal))))
            select applied)
        select diff;

    public static IO<Unit> Reset(SceneSource source) =>
        Edit(source, static staged => Rows(staged).TraverseM(static row => row.Reset).As().Map(static _ => unit));

    private static Seq<(ValueBinding Binding, IO<Unit> Reset)> Rows(SceneSource source) =>
        Seq(Row<ImageOutputState, ImageOutputParameter>(source), Row<RenderSourceState, RenderSourceParameter>(source), Row<FrameState, FrameParameter>(source),
                Row<LinearWorkflowState, LinearWorkflowParameter>(source), Row<DitheringState, DitheringParameter>(source), Row<RenderChannelsState, RenderChannelsParameter>(source),
                Row<BackgroundState, BackgroundParameter>(source), Row<LightingState, LightingParameter>(source), Row<SafeFrameState, SafeFrameParameter>(source),
                Row<GroundPlaneState, GroundPlaneParameter>(source), Row<SunState, SunParameter>(source))
            .Concat(Dictionary(source) switch {
                var dictionary => toSeq(CyclesKey.Items).Map(item => item.Entry(dictionary)),
            });

    private static (ValueBinding Binding, IO<Unit> Reset) Row<TRecord, TParameter>(SceneSource source)
        where TRecord : ISceneRecord<TRecord, TParameter>
        where TParameter : class, IStateParameter<TRecord>, ISmartEnum<string, TParameter, InvalidRhinoValue> =>
        Store<TRecord>(source) switch {
            var store => (
                ValueBinding.Fields(TRecord.Owner, store, TRecord.Default, ParameterText.Join<TRecord, TParameter, InvalidRhinoValue>(TRecord.Text)
                    .Map<(IStateParameter<TRecord> Parameter, string Caption)>(static pair => (pair.Parameter, pair.Text.Caption))),
                store.Write(None).Map(static _ => unit)),
        };

    // --- [ROWS]
    public static Seq<ControlRow> Fields<TRecord, TParameter>(Seq<TParameter> items)
        where TRecord : ISceneRecord<TRecord, TParameter>
        where TParameter : class, IStateParameter<TRecord>, ISmartEnum<string, TParameter, InvalidRhinoValue> =>
        items.Map(static item => (Parameter: item, Text: TRecord.Text(item))).Strict() switch {
            var texts => SectionRows.Source(TRecord.Owner, texts, static scope => IO.pure(scope.Document.ToSeq().Map(static doc => Store<TRecord>(doc)))) switch {
                var source => SectionRows.Of(source, texts, SectionRows.Unstaged, item => TRecord.Rules(source, item)),
            },
        };
}
