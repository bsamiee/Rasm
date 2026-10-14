using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.FileIO;
using Rhino.Render;
using Rhino.Render.DataSources;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render.Settings;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IRenderSettingsRecord<TSelf> : IStateRecord<TSelf> where TSelf : IRenderSettingsRecord<TSelf> {
    public static abstract string Owner { get; }
    public static abstract Option<DocumentEvent<RenderPropertyChangedEvent>> Changed { get; }
    public static abstract IO<TSelf> Read(RenderSettingsScope scope);
    public static abstract IO<TSelf> Write(RenderSettingsScope scope, TSelf record);
}

public interface IRenderSettingsRecord<TSelf, TParameter> : IRenderSettingsRecord<TSelf>, IStateRecord<TSelf, TParameter, InvalidRhinoValue>
    where TSelf : IRenderSettingsRecord<TSelf, TParameter>
    where TParameter : class, IStateParameter<TSelf>, ISmartEnum<string, TParameter, InvalidRhinoValue> {
    public static abstract ParameterText Text(TParameter parameter);
    public static abstract RowRules Rules(RowSource<TSelf> source, TParameter parameter);
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
                DisposalOps.Within(live.Settings.Duplicate,
                    staged =>
                        from edited in body(live with { Settings = staged })
                        from committed in IO.lift(() => { held.Doc.RenderSettings = staged; })
                        select edited,
                    static staged => IO.lift(staged.Dispose))),
            section: static (body, held) => SectionModels.Write(held.Host, Provider.RhinoSettings, data =>
                DisposalOps.Within(data.GetRenderSettings,
                    settings =>
                        from lent in IO.lift(() => Missing.Unless(settings, nameof(RhinoSettings.GetRenderSettings)))
                        from edited in body(new RenderSettingsScope(lent, held.Doc.ModelUnits))
                        from committed in IO.lift(() => data.SetRenderSettings(lent))
                        select edited,
                    static settings => IO.lift(() => Optional(settings).Iter(static lent => lent.Dispose())))),
            archive: static (body, held) => Read(held, body),
            detached: static (body, held) => Read(held, body));

    private static IO<T> Opened<T>(RhinoDoc doc, Func<RenderSettingsScope, IO<T>> body) =>
        DisposalOps.Within(() => doc.RenderSettings, settings => body(new RenderSettingsScope(settings, doc.ModelUnits)), static settings => IO.lift(settings.Dispose));

    // --- [SUB_OWNERS]
    public static IO<T> SubOwner<THost, T>(RenderSettingsScope scope, Func<RenderSettings, THost> select, Func<THost, IO<T>> body) where THost : IDisposable =>
        DisposalOps.Within(() => select(scope.Settings), body, static held => IO.lift(held.Dispose));

    // --- [STORES]
    public static ValueStore<TRecord> Store<TRecord>(RenderSettingsSource source) where TRecord : IRenderSettingsRecord<TRecord> =>
        Read(source, static scope => TRecord.Read(scope)).Map(Held) switch {
            var read => new(read,
                value =>
                    from before in read
                    from after in before == value.Bind(Held) ? IO.pure(before) : Edit(source, scope => TRecord.Write(scope, value.IfNone(TRecord.Default))).Map(Held)
                    select ValueStore.Between(before, after),
                EqualityComparer<TRecord>.Default.Equals, Applied.Live, Signal(source, TRecord.Changed)),
        };

    private static Option<TRecord> Held<TRecord>(TRecord record) where TRecord : IRenderSettingsRecord<TRecord> =>
        Some(record).Filter(static held => !EqualityComparer<TRecord>.Default.Equals(held, TRecord.Default));

    public static Option<HostEvent<Unit>> Signal(RenderSettingsSource source, Option<DocumentEvent<RenderPropertyChangedEvent>> changed) =>
        from row in changed
        from serial in source.Switch(
            live: static held => Some(held.Doc.RuntimeSerialNumber),
            section: static held => Some(held.Doc.RuntimeSerialNumber),
            archive: static _ => Option<uint>.None,
            detached: static _ => Option<uint>.None)
        select ValueStore.Signal(row.In(serial), static _ => true);

    public static DictionaryOwner Dictionary(RenderSettingsSource source) =>
        new(Read(source, static scope => ArchivableDictionaries.UserDictionary(scope.Settings).Map(static held => held.Clone())),
            edit => Edit(source, scope => ArchivableDictionaries.UserDictionary(scope.Settings).Bind(target => IO.lift(() => edit(target)))));

    // --- [ARRIVALS]
    public static Func<IPlugInSink, Action<IDisposable>, IO<Unit>> Register<TRecord>(Func<TRecord, TRecord> next) where TRecord : IRenderSettingsRecord<TRecord> =>
        (sink, retain) => EventKind.DocumentArrived.Choose(static serial => Some((serial, unit))).Through(Subscriptions.Idle<uint, Unit>(arrived => Arrived(arrived, next)), sink, retain);

    private static IO<Unit> Arrived<TRecord>(HashMap<uint, Unit> arrived, Func<TRecord, TRecord> next) where TRecord : IRenderSettingsRecord<TRecord> =>
        from stores in IO.lift(() => toSeq(arrived.Keys).Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial))).Map(static doc => Store<TRecord>(doc)).Strict())
        from written in Callbacks.Each(stores.Map(store => store.Read.Bind(held => store.Write(next(held.IfNone(TRecord.Default))))))
        select unit;

    // --- [BINDINGS]
    public static BindingGroup Bindings(RenderSettingsSource source) =>
        new(Rows(source).Map(static row => row.Binding));

    public static IO<ValueDiff> Copy(RenderSettingsSource source, RenderSettingsSource target) =>
        from values in Bindings(source).Capture
        from diff in Edit(target, staged =>
            from bindings in IO.pure(Bindings(staged))
            from applied in bindings.Apply(values, toHashSet(bindings.Keys().Filter(static key => string.Equals(key.Owner, RenderSourceState.Owner, StringComparison.Ordinal))))
            select applied)
        select diff;

    public static IO<Unit> Reset(RenderSettingsSource source) =>
        Edit(source, static staged => Callbacks.Each(Rows(staged).Map(static row => row.Reset)).Map(static _ => unit));

    private static Seq<(ValueBinding Binding, IO<Unit> Reset)> Rows(RenderSettingsSource source) =>
        Seq(Row<ImageOutputState, ImageOutputParameter>(source), Row<RenderSourceState, RenderSourceParameter>(source), Row<FrameState, FrameParameter>(source),
                Row<LinearWorkflowState, LinearWorkflowParameter>(source), Row<DitheringState, DitheringParameter>(source), Row<RenderChannelsState, RenderChannelsParameter>(source),
                Row<BackgroundState, BackgroundParameter>(source), Row<LightingState, LightingParameter>(source), Row<SafeFrameState, SafeFrameParameter>(source),
                Row<GroundPlaneState, GroundPlaneParameter>(source), Row<SunState, SunParameter>(source))
            .Concat(Dictionary(source) switch {
                var dictionary => toSeq(CyclesKey.Items).Map(item => item.Entry(dictionary)),
            });

    private static (ValueBinding Binding, IO<Unit> Reset) Row<TRecord, TParameter>(RenderSettingsSource source)
        where TRecord : IRenderSettingsRecord<TRecord, TParameter>
        where TParameter : class, IStateParameter<TRecord>, ISmartEnum<string, TParameter, InvalidRhinoValue> =>
        Store<TRecord>(source) switch {
            var store => (
                ValueBinding.Fields(TRecord.Owner, store, TRecord.Default, ParameterText.Join<TRecord, TParameter, InvalidRhinoValue>(TRecord.Text)
                    .Map<(IStateParameter<TRecord> Parameter, string Caption)>(static pair => (pair.Parameter, pair.Text.Caption))),
                store.Write(None).Map(static _ => unit)),
        };

    // --- [ROWS]
    public static Seq<ControlRow> Fields<TRecord, TParameter>(Seq<TParameter> items)
        where TRecord : IRenderSettingsRecord<TRecord, TParameter>
        where TParameter : class, IStateParameter<TRecord>, ISmartEnum<string, TParameter, InvalidRhinoValue> =>
        items.Map(static item => (Parameter: item, Text: TRecord.Text(item))).Strict() switch {
            var texts => SectionRows.Source(TRecord.Owner, texts, static scope => IO.pure(scope.Document.ToSeq().Map(static doc => Store<TRecord>(doc)))) switch {
                var source => SectionRows.Of(source, texts, None, item => TRecord.Rules(source, item)),
            },
        };
}
