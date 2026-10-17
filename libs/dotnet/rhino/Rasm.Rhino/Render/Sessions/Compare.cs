using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.UI;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Render.PostEffects;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CompareSide {
    private CompareSide(Rendering rendering) => Rendering = rendering;

    public Rendering Rendering { get; }

    public TileRequest Request(Seq<EffectKind> kinds) =>
        Switch(
            kinds,
            before: static (_, side) => new TileRequest(side.Rendering, ValueSet.Empty, []),
            recorded: static (running, side) => TileRequest.Recorded(side.Rendering, running));

    public sealed record Before(Rendering Rendering) : CompareSide(Rendering);

    public sealed record Recorded(Rendering Rendering) : CompareSide(Rendering);
}

public sealed record SettingsRow(EntryKey Key, string Setting, Option<string> A, Option<string> B) {
    public static Seq<SettingsRow> Between(ValueSet a, ValueSet b, BindingGroup bindings, IPlugInSink sink) =>
        toSeq(a.Diff(b).Changes.AsIterable().OrderBy(static entry => entry.Key))
            .Map(static entry => (entry.Key, A: ValueStore.Before(entry.Value), B: entry.Value.ToOption()))
            .Map(row => bindings.Text(row.Key).Match(
                Some: text => new SettingsRow(
                    row.Key, RowText.Localize(text.Caption, sink).Local, row.A.Map(text.Shown), row.B.Map(text.Shown)),
                None: () => new SettingsRow(row.Key, EntryKey.Name(row.Key.Path), row.A, row.B)))
            .Strict();
}

public sealed record ComparisonSettingRows(
    PlugInSetting<BarLevel, double> PsnrAdvisory,
    PlugInSetting<BarLevel, double> PsnrRefuse,
    PlugInSetting<BarLevel, double> NrmseAdvisory,
    PlugInSetting<BarLevel, double> NrmseRefuse) {
    public static ComparisonSettingRows Default { get; } = new(
        PsnrAdvisory: new(ValueKey.Numeric("ComparePsnrAdvisory", "PSNR advisory", "PSNR in decibels below which a compared render reads advisory", BarLevel.PsnrAdvisory,
            NumericDomain.Bounded<BarLevel, double>(new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(LevelUnit.Decibel), Decimals = 1 })), SettingType.Double),
        PsnrRefuse: new(ValueKey.Numeric("ComparePsnrRefuse", "PSNR refuse", "PSNR in decibels below which a compared render is refused", BarLevel.PsnrRefuse,
            NumericDomain.Bounded<BarLevel, double>(new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(LevelUnit.Decibel), Decimals = 1 })), SettingType.Double),
        NrmseAdvisory: new(ValueKey.Numeric("CompareNrmseAdvisory", "NRMSE advisory", "Normalized error above which a compared render reads advisory", BarLevel.NrmseAdvisory,
            NumericDomain.Bounded<BarLevel, double>(new() { Form = NumberForm.Field, Decimals = 2 })), SettingType.Double),
        NrmseRefuse: new(ValueKey.Numeric("CompareNrmseRefuse", "NRMSE refuse", "Normalized error above which a compared render is refused", BarLevel.NrmseRefuse,
            NumericDomain.Bounded<BarLevel, double>(new() { Form = NumberForm.Field, Decimals = 2 })), SettingType.Double));

    public Seq<PlugInSetting> Rows => [.. Levels];

    public Seq<Child> Children => [new ControlRow.Group("Render comparison", RowRules.Always) { Wording = Wording.Library }, .. Levels.Bind(static row => SectionRows.Setting(row, RowRules.Always))];

    public IO<ComparisonBars> Bars(ISettingReader node) =>
        (node.Current(PsnrAdvisory), node.Current(PsnrRefuse), node.Current(NrmseAdvisory), node.Current(NrmseRefuse))
            .Apply(static (psnrAdvisory, psnrRefuse, nrmseAdvisory, nrmseRefuse) => new ComparisonBars(new Bar(psnrAdvisory, psnrRefuse), new Bar(nrmseAdvisory, nrmseRefuse)))
            .As();

    private Seq<PlugInSetting<BarLevel, double>> Levels => [PsnrAdvisory, PsnrRefuse, NrmseAdvisory, NrmseRefuse];
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RenderingComparison : IDisposable {
    // --- [STATE]
    private readonly Ref<Option<Preview>> previewed = Ref(Option<Preview>.None);
    private readonly Ref<Option<Either<(Guid Rendering, RenderRun Run), (PixelFrame Scene, TileRequest Request)>>> displayed =
        Ref(Option<Either<(Guid Rendering, RenderRun Run), (PixelFrame Scene, TileRequest Request)>>.None);
    private static readonly (RowSource<bool> Source, RowField<bool> Field) difference = RowSource.Opaque(nameof(RenderingComparison), nameof(Difference), @default: false);
    private readonly Derivations derivations = new();

    private readonly Ref<Option<Chosen>> held = Ref(Option<Chosen>.None);
    private readonly ConcurrentExclusiveSchedulerPair measurements = new();
    private readonly RenderHistory history;
    private readonly IPlugInSink sink;
    private event EventHandler<EventArgs>? SelectionChanged;

    private RenderingComparison(RenderHistory history, IPlugInSink sink) => (this.history, this.sink) = (history, sink);

    private sealed record Chosen(CompareSide Side, FrameSide View, TileRequest Request, Option<PixelFrame> Frame, Option<(Measurement Work, Option<FrameComparison> Result)> Measurement, Option<IDisposable> Work);

    private sealed class Measurement : IDisposable {
        private readonly Lock lifetime = new();
        private readonly Disposal<Lazy<IDisposable>> started;

        public Measurement(object owner, PixelFrame reference, PixelFrame test, IPlugInSink sink, TaskScheduler scheduler) {
            Owner = owner;
            Test = test;
            started = new(
                new(() => new Disposal<ForkIO<Unit>>(
                    IO.liftAsync(environment => Task.Factory.StartNew(() => FrameComparison.Of(reference, test), environment.Token, TaskCreationOptions.None, scheduler))
                        .Bind(static measured => IO.lift(() => measured))
                        .Bind(measured => Result.SwapIO(_ => Some(measured)))
                        .Map(unit)
                        .Catch(error => IO.lift(() => sink.Report(error, typeof(RenderingComparison), nameof(Measures))))
                        .As().Fork().Run(),
                    static fork => fork.Cancel.Run())),
                static acquired => {
                    if (acquired.IsValueCreated) acquired.Value.Dispose();
                });
        }

        public PixelFrame Test { get; }

        public object Owner { get; }

        public Atom<Option<FrameComparison>> Result { get; } = Atom(Option<FrameComparison>.None);

        public IO<Unit> Start => IO.lift(() => {
            lock (lifetime) return started.Held.Iter(static pending => _ = pending.Value);
        });

        public void Dispose() {
            lock (lifetime) started.Dispose();
        }
    }

    private sealed record Preview(TileRequest Request, Option<Fin<(PixelFrame Frame, PixelFrame Scene)>> Frame, Option<IDisposable> Work);

    // --- [ACQUISITION]
    public static PlugInSlot<RenderingComparison> Current { get; } = new();

    public static Func<IPlugInSink, Action<IDisposable>, IO<Unit>> Register(RenderHistory history) =>
        (sink, retain) => DisposalOps.Acquire(owned =>
            from comparison in DisposalOps.Acquire(IO.lift(() => new RenderingComparison(history, sink)), owned)
            from published in Current.Publish(sink, comparison)
            select unit, retain);

    public void Dispose() {
        try {
            _ = Callbacks.Each(Seq(Clear, Previewed(None), IO.lift(measurements.Complete))).Run();
        } finally {
            derivations.Dispose();
        }
    }

    // --- [READS]
    public IO<Option<CompareSide>> Held => held.ValueIO.Map(static current => current.Map(static chosen => chosen.Side));

    private IO<Seq<SettingsRow>> Listed(Option<RhinoDoc> document) =>
        from current in held.ValueIO
        from rows in (
            from chosen in current
            from saved in chosen.Side.Rendering.State.Bind(static state => state.Settings)
            from doc in document
            select (Saved: saved, Document: doc)).Match(
                Some: listed =>
                    from bindings in history.Bindings(listed.Document)
                    from captured in bindings.Capture
                    select SettingsRow.Between(listed.Saved, captured, bindings, sink),
                None: static () => IO.pure(Seq<SettingsRow>()))
        select rows;

    private HostEvent<Unit> Changed =>
        Subscriptions.Host<EventArgs>(typeof(RenderingComparison), handler => SelectionChanged += handler, handler => SelectionChanged -= handler, nameof(Held)).Choose(static _ => Some(unit));

    // --- [WRITES]
    public IO<Unit> Choose(CompareSide side, string caption) =>
        from request in EffectRegistry.Kinds.Of(sink).Map(side.Request)
        let output = Output(request)
        let next = new Chosen(side, new FrameSide(output.Feed, caption), request, None, None, None)
        from current in Updated(state => state.Filter(chosen => chosen.Side == side) | Some(next))
        from graded in when(current.Exists(chosen => ReferenceEquals(chosen, next)),
            DisposalOps.Fork(Grade(request), lease => Updated(state => state.Map(chosen => ReferenceEquals(chosen.Request, request) ? chosen with { Work = Some(lease) } : chosen))
                .Map(chosen => { if (!chosen.Exists(held => ReferenceEquals(held.Request, request))) lease.Dispose(); return unit; }).Run()).Map(unit)).As()
        select graded;

    public IO<Unit> Clear => Updated(static _ => Option<Chosen>.None).Map(unit);

    private IO<Option<Chosen>> Updated(Func<Option<Chosen>, Option<Chosen>> change) =>
        (from changed in atomicIO(() => {
            Option<Chosen> before = held.Value;
            Option<Chosen> after = change(before);
            held.Value = after;
            return (Current: after, SelectionChanged: before.Map(static chosen => chosen.Side) != after.Map(static chosen => chosen.Side),
                Released: before.Bind(static chosen => chosen.Measurement).Map<IDisposable>(static measurement => measurement.Work)
                    .Filter(work => !after.Bind(static chosen => chosen.Measurement).Exists(measurement => ReferenceEquals(measurement.Work, work))).ToSeq()
                    + before.Bind(static chosen => chosen.Work).Filter(work => !after.Bind(static chosen => chosen.Work).Exists(kept => ReferenceEquals(kept, work))).ToSeq());
        })
         from released in DisposalOps.Release(changed.Released)
         from raised in when(changed.SelectionChanged, IO.lift(() => SelectionChanged?.Invoke(this, EventArgs.Empty))).As()
         select changed.Current).Uninterruptible();

    // --- [GRADES]
    private IO<Unit> Grade(TileRequest request) =>
        Formation(request)
            .Bind(formed => Updated(state => state.Map(chosen => ReferenceEquals(chosen.Request, request) ? chosen with { Frame = Some(formed.Frame) } : chosen)))
            .Map(unit)
            .Catch(error => Updated(state => state.Filter(chosen => !ReferenceEquals(chosen.Request, request))).Bind(_ => IO.lift(() => sink.Report(error, typeof(RenderingComparison), nameof(Choose)))))
            .As();

    private IO<(PixelFrame Frame, PixelFrame Scene)> Formation(TileRequest request) =>
        from scene in request.Rendering.Frame
        from formed in EffectPipeline.Form(scene, request.Set, request.Chain, derivations)
        select (formed, scene);

    private static (FrameFeed Feed, Gamut Gamut) Output(TileRequest request) =>
        request.Chain.Exists(static kind => kind.Stage is PostEffectType.ToneMapping)
            ? (RenderRuns.Formed.Feed, EffectPipeline.Display.Encoding.Gamut)
            : (RenderRuns.Formed.Feed with { Encoding = TransferCurve.Linear, Peak = None }, EffectPipeline.Working);

    // --- [MEASURES]
    private IO<Option<FrameComparison>> Measure =>
        held.ValueIO.Map(static current => current.Bind(static chosen => chosen.Measurement).Bind(static measurement => measurement.Result));

    private IO<(Option<FrameShown> Reference, Seq<FrameMask> Masks)> Sampled(object owner, Option<PixelFrame> frame, bool differenceShown) =>
        from current in Updated(state => state.Map(chosen => chosen.Frame.Match(
            Some: reference => chosen with {
                Measurement = frame.Map(test => {
                    Measurement work = chosen.Measurement.Map(static measurement => measurement.Work)
                        .Filter(prior => ReferenceEquals(prior.Owner, owner) && ReferenceEquals(prior.Test, test))
                        .IfNone(() => new Measurement(owner, reference, test, sink, measurements.ExclusiveScheduler));
                    return (Work: work, Result: work.Result.Value);
                }),
            },
            None: () => chosen)))
        from started in current.Bind(static chosen => chosen.Measurement).Map(static measurement => measurement.Work.Start).IfNone(IO.pure(unit))
        select (current.Map(static chosen => new FrameShown(chosen.View, chosen.Frame)),
            current.Bind(static chosen => chosen.Measurement).Bind(static measurement => measurement.Result).Filter(_ => differenceShown)
                .Map(static measured => new FrameMask(
                    new Coverage.Matte(MatteChannel.Lightness, measured.Difference, EffectPipeline.Display.Encoding.Gamut, Inverted: false),
                    new MarkStyle.Fill(TraceChannel.Red.Mark, 1f))).ToSeq());

    // --- [PREVIEW]
    private IO<(FrameShown Current, Option<ScopeFrames> Scope, Option<Either<(Guid Rendering, RenderRun Run), (PixelFrame Scene, TileRequest Request)>> Source)> Live =>
        from shown in previewed.ValueIO
        from read in (from preview in shown from formed in preview.Frame select (Preview: preview, Formed: formed)).Match(
            Some: pending => pending.Formed.Match(
                Succ: formed => IO.pure((Shown: Shown(Some((Formed: formed.Frame, Scene: Some(formed.Scene))), Some(pending.Preview.Request)),
                    Source: Some(Right<(Guid Rendering, RenderRun Run), (PixelFrame Scene, TileRequest Request)>((formed.Scene, pending.Preview.Request))))),
                Fail: error => Dropped(pending.Preview, error)
                    .Map(static shown => (Shown: shown, Source: Option<Either<(Guid Rendering, RenderRun Run), (PixelFrame Scene, TileRequest Request)>>.None))),
            None: static () =>
                from captured in IO.lift(static () => RenderRuns.Rendered)
                select (Shown: Shown(from held in captured from formed in held.Run.Formed select (formed, held.Run.Scene), None),
                    Source: captured.Map(Left<(Guid Rendering, RenderRun Run), (PixelFrame Scene, TileRequest Request)>)))
        select (read.Shown.Current, read.Shown.Scope, read.Source);

    private static IO<(FrameShown Current, Option<ScopeFrames> Scope)> Rendered => IO.lift(static () => Shown(
        from held in RenderRuns.Rendered from formed in held.Run.Formed select (formed, held.Run.Scene), None));

    private static (FrameShown Current, Option<ScopeFrames> Scope) Shown(Option<(PixelFrame Formed, Option<PixelFrame> Scene)> rendered, Option<TileRequest> request) =>
        (Output: request.Map(Output).IfNone((RenderRuns.Formed.Feed, EffectPipeline.Display.Encoding.Gamut)), EffectPipeline.Depth.Levels) switch {
            var (output, levels) => (new FrameShown(RenderRuns.Formed with { Feed = output.Feed }, rendered.Map(static frames => frames.Formed)),
                rendered.Map(frames => new ScopeFrames(frames.Formed, output.Gamut, output.Feed.Encoding,
                    levels.Map(static held => (float)held.MaxCode), levels.Map(static held => held.Clipping), frames.Scene.Map(static scene => (scene, EffectPipeline.Working))))),
        };

    public static IO<Option<ScopeFrames>> Scoped(RowScope scope) =>
        Compared(scope, static comparison => comparison.Live.Map(static live => (live.Current, live.Scope)), Rendered).Map(static shown => shown.Scope);

    public IO<Unit> Previewed(TileRequest request, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from release in DisposalOps.Acquire(IO.lift(() => new Disposal<TileRequest>(request, asked => Withdrawn(asked).Run())), owned)
            from published in Previewed(Some(request))
            select unit, retain);

    private IO<Unit> Previewed(Option<TileRequest> request) =>
        from shown in (from changed in atomicIO(() => {
            Option<Preview> before = previewed.Value;
            Option<Preview> after = request.Map(static asked => new Preview(asked, None, None));
            previewed.Value = after;
            return (Current: after, Released: before.Bind(static shown => shown.Work));
        })
                       from released in DisposalOps.Release(changed.Released.ToSeq())
                       select changed.Current).Uninterruptible()
        from formed in shown.Match(
            Some: preview => DisposalOps.Fork(PreviewTiles.Current.Of(sink)
                .Bind(tiles => tiles.Formed(preview.Request, preview.Request.Rendering.Extent, TileRank.Current))
                .Bind(frame => Landed(preview, (frame.Frame, frame.Proxy)))
                .Catch(error => Landed(preview, error))
                .As(), lease => atomicIO(() => {
                    bool current = previewed.Value.Exists(shown => ReferenceEquals(shown.Request, preview.Request));
                    previewed.Value = previewed.Value.Map(shown => ReferenceEquals(shown.Request, preview.Request) ? shown with { Work = Some(lease) } : shown);
                    return current;
                }).Map(current => { if (!current) lease.Dispose(); return unit; }).Run())
                .Map(unit),
            None: static () => IO.pure(unit))
        select formed;

    private IO<Unit> Landed(Preview asked, Fin<(PixelFrame Frame, PixelFrame Scene)> formed) =>
        atomicIO(() => previewed.Value = previewed.Value.Map(preview => ReferenceEquals(preview.Request, asked.Request) ? preview with { Frame = Some(formed) } : preview)).Map(unit);

    private IO<Unit> Withdrawn(TileRequest request) =>
        atomicIO(() => {
            Option<Preview> removed = previewed.Value.Filter(preview => ReferenceEquals(preview.Request, request));
            previewed.Value = previewed.Value.Filter(preview => !ReferenceEquals(preview.Request, request));
            return removed.Bind(static preview => preview.Work);
        }).Bind(static work => DisposalOps.Release(work.ToSeq())).Uninterruptible();

    private IO<(FrameShown Current, Option<ScopeFrames> Scope)> Dropped(Preview failed, Error error) =>
        Withdrawn(failed.Request).Bind(_ => IO.fail<(FrameShown Current, Option<ScopeFrames> Scope)>(error));

    // --- [ROWS]
    public static ControlRow Export { get; } = SectionRows.Pushed(new CommandRow.Run(
        new CommandFace("comparison-export", "Export displayed frame", "Write the displayed callback or history frame through the output targets", None, None), None,
        static scope =>
            from comparison in Current.Of(scope.Sink)
            from source in comparison.displayed.ValueIO.Bind(static held => IO.lift(held.ToFin(new Refused(nameof(FrameShown.Frame), Refused.Absent))))
            from document in scope.DocumentIO
            from written in Saves.Write(document, scope.Sink, source)
            select unit));

    public static FrameSampling Sample(RowScope scope) {
        object owner = new();
        return new(
            Compared(scope, comparison =>
                from live in comparison.Live
                from shown in scope.Read(difference.Source)
                from sampled in comparison.Sampled(owner, live.Current.Frame, shown)
                from retained in comparison.displayed.SwapIO(_ => live.Source)
                select (live.Current, sampled.Reference, sampled.Masks),
                Rendered.Map(static live => (live.Current, Reference: Option<FrameShown>.None, Masks: Seq<FrameMask>()))),
            Compared(scope, comparison => comparison.Updated(state => state.Map(chosen =>
                chosen.Measurement.Exists(measurement => ReferenceEquals(measurement.Work.Owner, owner)) ? chosen with { Measurement = None } : chosen)).Map(unit), IO.pure(unit)));
    }

    public static ControlRow Difference { get; } =
        ToggleRows.Check(
            difference.Source,
            difference.Field with {
                Caption = "Difference",
                Help = "Tints the frame where the compared rendering differs from the newest rendering",
            },
            Lens.identity<bool>(),
            RowRules.Always) with {
            Wording = Wording.Library,
        };

    public static ControlRow Measures(ComparisonSettingRows bars) =>
        TextRows.Readout(
            "Measure",
            "PSNR and NRMSE of the compared rendering against the newest rendering, graded by the comparison bars",
            new ReadoutSource<Option<(FrameComparison, Verdict)>>.Scoped(scope =>
                from measured in Compared(scope, static comparison => comparison.Measure, IO.pure(Option<FrameComparison>.None))
                from graded in measured.Traverse(result => bars.Bars(((IPlugInViews)scope.Sink).Settings).Map(levels => (result, result.Graded(levels)))).As()
                select graded),
            static graded => graded.Match(Some: Shown, None: static () => ""),
            RowRules.Always) with {
            Wording = Wording.Library,
        };

    public static ControlRow Settings { get; } =
        ListRows.List(
            "Render Settings",
            "The compared rendering's saved render settings against the document's current values",
            new ListRow<SettingsRow, EntryKey>(
                IterableNE.create<ListColumn<SettingsRow>>(
                    new ListColumn<SettingsRow>.Label(Some("Setting"), static row => row.Setting, None, None),
                    new ListColumn<SettingsRow>.Label(Some("Saved"), static row => row.A.IfNone(""), None, None),
                    new ListColumn<SettingsRow>.Label(Some("Current"), static row => row.B.IfNone(""), None, None)),
                static row => row.Key,
                RowSource.Opaque(nameof(RenderingComparison), nameof(ListRow<,>.Items), Seq<SettingsRow>(), Items),
                RowSource.Opaque(nameof(RenderingComparison), nameof(ListRow<,>.Selected), LanguageExt.HashSet<EntryKey>.Empty)) {
                Wording = Wording.Library,
            },
            RowRules.Always);

    private static IO<A> Compared<A>(RowScope scope, Func<RenderingComparison, IO<A>> read, IO<A> absent) =>
        (Current.Of(scope.Sink).Map(static comparison => Some(comparison)) | IO.pure(Option<RenderingComparison>.None)).Bind(cell => cell.Match(Some: read, None: () => absent));

    private static string Shown((FrameComparison Measured, Verdict Verdict) graded) =>
        RowText.Localize("{0}, {1}, {2}", arguments: [
            RowText.Localize("PSNR {0:F1} dB", arguments: [graded.Measured.Psnr]).Local,
            graded.Measured.Nrmse.Match(Some: static nrmse => RowText.Localize("NRMSE {0:F3}", arguments: [nrmse]).Local, None: static () => RowText.Localize("NRMSE unmeasured").Local),
            graded.Verdict.Map(pass: RowText.Localize("Pass"), advisory: RowText.Localize("Advisory"), unmeasured: RowText.Localize("Unmeasured"), refuse: RowText.Localize("Refused")).Local,
        ]).Local;

    private static ValueStore<Seq<SettingsRow>> Items(RowScope scope) =>
        ValueStore.Readout(Compared(scope, comparison => comparison.Listed(scope.Document), IO.pure(Seq<SettingsRow>())), Some(Marked(scope)));

    private static HostEvent<Unit> Marked(RowScope scope) =>
        new(typeof(RenderingComparison), nameof(Settings), (deliver, site, retain) =>
            from cell in Current.Of(scope.Sink).Map(static comparison => Some(comparison)) | IO.pure(Option<RenderingComparison>.None)
            let raises = scope.Document.ToSeq().Bind(Edited) + cell.ToSeq().Map(static comparison => comparison.Changed)
            from attached in DisposalOps.Acquire(keep => raises.TraverseM(raise => raise.Attach(deliver, site, keep)).As(), retain)
            select unit);

    private static Seq<HostEvent<Unit>> Edited(RhinoDoc document) =>
        Seq(EventKind.DocumentPropertiesChanged.In(document.RuntimeSerialNumber).Choose(static _ => Some(unit)),
            EventKind.UndoRedo.In(document.RuntimeSerialNumber).Choose(static _ => Some(unit)));
}
