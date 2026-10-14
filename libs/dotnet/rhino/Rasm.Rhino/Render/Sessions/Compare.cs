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
                    row.Key, RowText.Localize(text.Caption, table: Some<object>(sink)).Local, row.A.Map(text.Shown), row.B.Map(text.Shown)),
                None: () => new SettingsRow(row.Key, EntryKey.Name(row.Key.Path), row.A, row.B)))
            .Strict();
}

public sealed record ComparisonSettingRows(
    PlugInSetting<BarLevel, double> PsnrAdvisory,
    PlugInSetting<BarLevel, double> PsnrRefuse,
    PlugInSetting<BarLevel, double> NrmseAdvisory,
    PlugInSetting<BarLevel, double> NrmseRefuse) {
    public Seq<PlugInSetting<BarLevel, double>> Rows => [PsnrAdvisory, PsnrRefuse, NrmseAdvisory, NrmseRefuse];

    public Seq<Child> Children => Rows.Bind(static row => SectionRows.Setting(row, RowRules.Always)).Map(static row => (Child)row);

    public IO<ComparisonBars> Bars(ISettingReader node) =>
        (node.Current(PsnrAdvisory), node.Current(PsnrRefuse), node.Current(NrmseAdvisory), node.Current(NrmseRefuse))
            .Apply(static (psnrAdvisory, psnrRefuse, nrmseAdvisory, nrmseRefuse) => new ComparisonBars(new Bar(psnrAdvisory, psnrRefuse), new Bar(nrmseAdvisory, nrmseRefuse)))
            .As();

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
            started = new(new(() => {
                CancellationTokenSource cancellation = new();
                Task<Unit> work = Task.Factory.StartNew(async () => {
                    using EnvIO environment = EnvIO.New(token: cancellation.Token, source: cancellation);
                    return await IO.lift(() => FrameComparison.Of(reference, test))
                        .Bind(measured => Result.SwapIO(_ => Some(measured)))
                        .Map(static _ => unit)
                        .Catch(error => IO.lift(() => sink.Report(error, typeof(RenderingComparison), nameof(Measures))))
                        .As().RunAsync(environment).ConfigureAwait(false);
                }, cancellation.Token, TaskCreationOptions.None, scheduler).Unwrap();
                return new Disposal<CancellationTokenSource>(cancellation, released => {
                    released.Cancel();
                    _ = work.ContinueWith(_ => released.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                });
            }), static acquired => {
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
            from comparison in DisposalOps.Acquire(IO.lift(() => new RenderingComparison(history, sink)), static held => IO.lift(held.Dispose), owned)
            from published in Current.Publish(sink, comparison)
            select unit, (_, lease) => retain(lease));

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
                    from bindings in IO.lift(() => history.Bindings(listed.Document))
                    from captured in bindings.Capture
                    select SettingsRow.Between(listed.Saved, captured, bindings, sink),
                None: static () => IO.pure(Seq<SettingsRow>()))
        select rows;

    private HostEvent<Unit> Changed =>
        Subscriptions.Host<EventArgs>(typeof(RenderingComparison), handler => SelectionChanged += handler, handler => SelectionChanged -= handler, nameof(Held)).Choose(static _ => Some(unit));

    // --- [WRITES]
    public IO<Unit> Choose(CompareSide side, string caption) =>
        IO.lift(static environment => environment).Bind(environment => (
        from request in IO.pure(side.Request(history.Kinds))
        let output = Output(request)
        let next = new Chosen(side, new FrameSide(output.Feed, caption), request, None, None, None)
        from current in Updated(state => state.Filter(chosen => chosen.Side == side) | Some(next))
        from graded in when(current.Exists(chosen => ReferenceEquals(chosen, next)),
            IO.liftVAsync(() => DisposalOps.Fork(Grade(request), lease => Updated(state => state.Map(chosen => ReferenceEquals(chosen.Request, request) ? chosen with { Work = Some(lease) } : chosen))
                .Map(chosen => { if (!chosen.Exists(held => ReferenceEquals(held.Request, request))) lease.Dispose(); return unit; }).Run()).RunAsync(environment)).Map(static _ => unit)).As()
        select graded).Uninterruptible());

    public IO<Unit> Clear => Updated(static _ => Option<Chosen>.None).Map(static _ => unit);

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
            .Map(static _ => unit)
            .Catch(error => Updated(state => state.Filter(chosen => !ReferenceEquals(chosen.Request, request))).Bind(_ => IO.lift(() => sink.Report(error, typeof(RenderingComparison), nameof(Choose)))))
            .As();

    private IO<(PixelFrame Frame, PixelFrame Scene)> Formation(TileRequest request) =>
        from scene in request.Rendering.Frame
        from formed in EffectPipeline.Form(scene, request.Set, request.Chain, derivations)
        select (formed, scene);

    private static (FrameFeed Feed, Gamut Gamut) Output(TileRequest request) =>
        request.Chain.Exists(static kind => kind.Stage == PostEffectType.ToneMapping)
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
                    new MarkStyle.Fill(new MarkColor.Trace(TraceChannel.Red), 1f))).ToSeq());

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
            from release in DisposalOps.Acquire(IO.lift(() => new Disposal<TileRequest>(request, asked => Withdrawn(asked).Run())),
                static lease => IO.lift(lease.Dispose), owned)
            from published in Previewed(Some(request))
            select unit, retain);

    private IO<Unit> Previewed(Option<TileRequest> request) =>
        IO.lift(static environment => environment).Bind(environment => (
        from shown in (from changed in atomicIO(() => {
            Option<Preview> before = previewed.Value;
            Option<Preview> after = request.Map(static asked => new Preview(asked, None, None));
            previewed.Value = after;
            return (Current: after, Released: before.Bind(static shown => shown.Work));
        })
                       from released in DisposalOps.Release(changed.Released.ToSeq())
                       select changed.Current).Uninterruptible()
        from formed in shown.Match(
            Some: preview => IO.liftVAsync(() => DisposalOps.Fork(IO.lift(() => PreviewTiles.Current.Of(sink))
                .Bind(tiles => tiles.Formed(preview.Request, preview.Request.Rendering.Extent, TileRank.Current))
                .Bind(frame => Landed(preview, (frame.Frame, frame.Proxy)))
                .Catch(error => Landed(preview, error))
                .As(), lease => atomicIO(() => {
                    bool current = previewed.Value.Exists(shown => ReferenceEquals(shown.Request, preview.Request));
                    previewed.Value = previewed.Value.Map(shown => ReferenceEquals(shown.Request, preview.Request) ? shown with { Work = Some(lease) } : shown);
                    return current;
                }).Map(current => { if (!current) lease.Dispose(); return unit; }).Run()).RunAsync(environment))
                .Map(static _ => unit),
            None: static () => IO.pure(unit))
        select formed).Uninterruptible());

    private IO<Unit> Landed(Preview asked, Fin<(PixelFrame Frame, PixelFrame Scene)> formed) =>
        atomicIO(() => previewed.Value = previewed.Value.Map(preview => ReferenceEquals(preview.Request, asked.Request) ? preview with { Frame = Some(formed) } : preview)).Map(static _ => unit);

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
            from comparison in IO.lift(() => Current.Of(scope.Sink))
            from source in comparison.displayed.ValueIO.Bind(static held => IO.lift(held.ToFin(new Missing(nameof(FrameShown.Frame)))))
            from document in IO.lift(scope.Document.ToFin(new Missing(nameof(RowScope.Document))))
            from book in TargetRows.Book(document)
            from origin in FileOrigin.At(scope.Sink)
            from written in Saves.Write(document, origin, book, source)
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
                chosen.Measurement.Exists(measurement => ReferenceEquals(measurement.Work.Owner, owner)) ? chosen with { Measurement = None } : chosen)).Map(static _ => unit), IO.pure(unit)));
    }

    public static ControlRow Difference { get; } =
        ToggleRows.Check(
            difference.Source,
            difference.Field with {
                Caption = RowText.Localize("Difference").Local,
                Help = RowText.Localize("Tints the frame where the compared rendering differs from the newest rendering").Local,
            },
            Lens.identity<bool>(),
            RowRules.Always) with {
            Wording = Wording.Localized,
        };

    public static ControlRow Measures(ComparisonSettingRows bars) =>
        TextRows.Readout(
            RowText.Localize("Measure").Local,
            RowText.Localize("PSNR and NRMSE of the compared rendering against the newest rendering, graded by the comparison bars").Local,
            new ReadoutSource<Option<(FrameComparison, Verdict)>>.Scoped(scope =>
                from measured in Compared(scope, static comparison => comparison.Measure, IO.pure(Option<FrameComparison>.None))
                from graded in measured.Traverse(result => bars.Bars(((IPlugInViews)scope.Sink).Settings).Map(levels => (result, result.Graded(levels)))).As()
                select graded),
            static graded => graded.Match(Some: Shown, None: static () => ""),
            RowRules.Always) with {
            Wording = Wording.Localized,
        };

    public static ControlRow Settings { get; } =
        ListRows.List(
            RowText.Localize("Render Settings").Local,
            RowText.Localize("The compared rendering's saved render settings against the document's current values").Local,
            new ListRow<SettingsRow, EntryKey>(
                IterableNE.create<ListColumn<SettingsRow>>(
                    new ListColumn<SettingsRow>.Label(Some(RowText.Localize("Setting").Local), static row => row.Setting, None, None),
                    new ListColumn<SettingsRow>.Label(Some(RowText.Localize("Saved").Local), static row => row.A.IfNone(""), None, None),
                    new ListColumn<SettingsRow>.Label(Some(RowText.Localize("Current").Local), static row => row.B.IfNone(""), None, None)),
                static row => row.Key,
                RowSource.Opaque(nameof(RenderingComparison), nameof(ListRow<,>.Items), Seq<SettingsRow>(), Items),
                RowSource.Opaque(nameof(RenderingComparison), nameof(ListRow<,>.Selected), LanguageExt.HashSet<EntryKey>.Empty)) {
                Wording = Wording.Localized,
            },
            RowRules.Always);

    private static IO<A> Compared<A>(RowScope scope, Func<RenderingComparison, IO<A>> read, IO<A> absent) =>
        IO.lift(() => Current.Of(scope.Sink).ToOption()).Bind(cell => cell.Match(Some: read, None: () => absent));

    private static string Shown((FrameComparison Measured, Verdict Verdict) graded) =>
        RowText.Localize("{0}, {1}, {2}", arguments: [
            RowText.Localize("PSNR {0:F1} dB", arguments: [graded.Measured.Psnr]).Local,
            graded.Measured.Nrmse.Match(Some: static nrmse => RowText.Localize("NRMSE {0:F3}", arguments: [nrmse]).Local, None: static () => RowText.Localize("NRMSE unmeasured").Local),
            graded.Verdict.Map(pass: RowText.Localize("Pass"), advisory: RowText.Localize("Advisory"), unmeasured: RowText.Localize("Unmeasured"), refuse: RowText.Localize("Refused")).Local,
        ]).Local;

    private static ValueStore<Seq<SettingsRow>> Items(RowScope scope) =>
        new(
            Compared(scope, comparison => comparison.Listed(scope.Document), IO.pure(Seq<SettingsRow>())).Map(static rows => Some(rows)),
            static _ => IO.pure(Change<Seq<SettingsRow>>.None),
            EqualityComparer<Seq<SettingsRow>>.Default.Equals,
            Applied.Live,
            Some(Marked(scope)));

    private static HostEvent<Unit> Marked(RowScope scope) =>
        new(typeof(RenderingComparison), nameof(Settings), (deliver, site, retain) =>
            from cell in IO.lift(() => Current.Of(scope.Sink).ToOption())
            let raises = scope.Document.ToSeq().Bind(Edited) + cell.ToSeq().Map(static comparison => comparison.Changed)
            from attached in DisposalOps.AcquireAll(raises.Map<Func<Action<IDisposable>, IO<Unit>>>(raise => keep => raise.Attach(deliver, site, keep)),
                DisposalOps.Release, (_, lease) => retain(lease))
            select unit);

    private static Seq<HostEvent<Unit>> Edited(RhinoDoc document) =>
        Seq(EventKind.DocumentPropertiesChanged.In(document.RuntimeSerialNumber).Choose(static _ => Some(unit)),
            EventKind.UndoRedo.In(document.RuntimeSerialNumber).Choose(static _ => Some(unit)));
}
