using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Viewers;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.PlugIns;
using Rhino.Render.PostEffects;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Rhino.Render.Slots;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CompareSide {
    private CompareSide(RenderSlot slot) => Slot = slot;

    public RenderSlot Slot { get; }

    public TileRequest Request(Seq<EffectKind> kinds) =>
        Switch(
            kinds,
            before: static (_, side) => new TileRequest(side.Slot, ValueSet.Empty, []),
            recorded: static (running, side) => new TileRequest(side.Slot, side.State.Grading, side.State.Running(running)));

    public sealed record Before(RenderSlot Slot) : CompareSide(Slot);

    public sealed record Recorded(RenderSlot Slot, SlotState State) : CompareSide(Slot);
}

public sealed record SettingsRow(EntryKey Key, string Setting, Option<string> A, Option<string> B) {
    public static Seq<SettingsRow> Between(ValueSet a, ValueSet b, BindingGroup bindings, IPlugInSink sink) =>
        toSeq(a.Diff(b).Changes.Keys.Order())
            .Choose(key => bindings.Text(key).Map(text => new SettingsRow(
                key, RowText.Localize(text.Caption, table: Some<object>(sink)).Local, a.Entries.Find(key).Map(text.Shown), b.Entries.Find(key).Map(text.Shown))))
            .Strict();
}

public sealed record ComparisonSettingRows(
    PlugInSetting<BarLevel, double, InvalidPixelValue> PsnrAdvisory,
    PlugInSetting<BarLevel, double, InvalidPixelValue> PsnrRefuse,
    PlugInSetting<BarLevel, double, InvalidPixelValue> NrmseAdvisory,
    PlugInSetting<BarLevel, double, InvalidPixelValue> NrmseRefuse) {
    private static readonly Presentation<BarLevel, double> Decibels = new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(LevelUnit.Decibel), Decimals = 1 };
    private static readonly Presentation<BarLevel, double> Unitless = new() { Form = NumberForm.Field, Decimals = 2 };

    public Seq<PlugInSetting> Rows => Presented.Map(static shown => (PlugInSetting)shown.Row);

    public Seq<Child> Children => Presented.Bind(static shown => SectionRows.Setting(shown.Row, shown.Presentation, RowRules.Always)).Map(static row => (Child)row);

    public IO<ComparisonBars> Bars(SettingsNode node) =>
        (PlugInSettings.Current(node, PsnrAdvisory), PlugInSettings.Current(node, PsnrRefuse), PlugInSettings.Current(node, NrmseAdvisory), PlugInSettings.Current(node, NrmseRefuse))
            .Apply(static (psnrAdvisory, psnrRefuse, nrmseAdvisory, nrmseRefuse) => new ComparisonBars(new Bar(psnrAdvisory, psnrRefuse), new Bar(nrmseAdvisory, nrmseRefuse)))
            .As();

    private Seq<(PlugInSetting<BarLevel, double, InvalidPixelValue> Row, Presentation<BarLevel, double> Presentation)> Presented =>
        [(PsnrAdvisory, Decibels), (PsnrRefuse, Decibels), (NrmseAdvisory, Unitless), (NrmseRefuse, Unitless)];
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SlotComparison {
    // --- [STATE]
    private static readonly Atom<Option<Preview>> previewed = Atom(Option<Preview>.None);
    private static readonly (RowSource<bool> Source, RowField<bool> Field) difference = RowSource.Opaque(nameof(SlotComparison), nameof(Difference), @default: false);
    private static readonly Derivations derivations = new();

    private readonly Atom<Option<Chosen>> held = Atom(Option<Chosen>.None);
    private readonly RenderHistory history;
    private readonly IPlugInRendering rendering;

    private SlotComparison(RenderHistory history, IPlugInRendering rendering) => (this.history, this.rendering) = (history, rendering);

    private sealed record Chosen(CompareSide Side, FrameSide View, Option<PixelFrame> Frame, Option<Measurement> Measurement);

    private sealed record Measurement(PixelFrame Test, Option<FrameComparison> Result);

    private sealed record Preview(TileRequest Request, Option<Fin<PixelFrame>> Frame);

    // --- [ACQUISITION]
    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(RenderHistory history) =>
        (_, sink) =>
            from comparison in IO.lift(() => new SlotComparison(history, (IPlugInRendering)sink))
            from filled in comparison.rendering.Comparison.SwapIO(_ => Some(comparison))
            select (IDisposable)new Disposal<Atom<Option<SlotComparison>>>(comparison.rendering.Comparison, static cell => cell.Swap(static _ => None));

    // --- [READS]
    public IO<Option<CompareSide>> Held => held.ValueIO.Map(static current => current.Map(static chosen => chosen.Side));

    private IO<Seq<SettingsRow>> Listed(Option<RhinoDoc> document) =>
        from current in held.ValueIO
        from rows in (
            from chosen in current
            from saved in chosen.Side.Slot.State.Bind(static state => state.Settings)
            from doc in document
            select (Saved: saved, Document: doc)).Match(
                Some: listed =>
                    from bindings in IO.lift(history.Bindings(listed.Document).ToFin())
                    from captured in bindings.Capture
                    select SettingsRow.Between(listed.Saved, captured, bindings, rendering),
                None: static () => IO.pure(Seq<SettingsRow>()))
        select rows;

    private HostEvent<Unit> Changed =>
        Subscriptions.Host<AtomChangedEvent<Option<Chosen>>, Option<Chosen>>(
                typeof(SlotComparison), handler => held.Change += handler, handler => held.Change -= handler, static handler => value => handler(sender: null, value), nameof(Held))
            .Choose(static _ => Some(unit));

    // --- [WRITES]
    public IO<Unit> Choose(CompareSide side, string caption) =>
        from request in IO.pure(side.Request(rendering.Effects))
        let read = held.ValueIO.Map(state => state.Filter(chosen => chosen.Side == side).Bind(static chosen => chosen.Frame))
        let feed = request.Chain.Exists(static kind => kind.Stage == PostEffectType.ToneMapping)
            ? RenderRuns.Formed.Feed with { Read = read }
            : RenderRuns.Formed.Feed with { Read = read, Encoding = TransferCurve.Linear, Peak = None }
        let next = new Chosen(side, new FrameSide(feed, caption), None, None)
        from current in held.SwapIO(state => state.Filter(chosen => chosen.Side == side) | Some(next))
        from graded in when(current.Exists(chosen => ReferenceEquals(chosen, next)), Grade(side, request).Fork().Map(static _ => unit)).As()
        select graded;

    public IO<Unit> Clear => held.SwapIO(static _ => Option<Chosen>.None).Map(static _ => unit);

    // --- [GRADES]
    private IO<Unit> Grade(CompareSide side, TileRequest request) =>
        Formation(request)
            .Bind(frame => held.SwapIO(state => state.Map(chosen => chosen.Side == side ? chosen with { Frame = Some(frame) } : chosen)))
            .Map(static _ => unit)
            .Catch(error => held.SwapIO(state => state.Filter(chosen => chosen.Side != side)).Bind(_ => IO.lift(() => rendering.Report(error, typeof(SlotComparison), nameof(Choose)))))
            .As();

    private static IO<PixelFrame> Formation(TileRequest request) =>
        request.Slot.Frame.Bind(frame => EffectPipeline.Form(frame, request.Set, request.Chain, derivations));

    // --- [MEASURES]
    private IO<Option<FrameComparison>> Measure =>
        from formed in IO.lift(static () => RenderRuns.Rendered.Map(static rendered => rendered.Formed))
        from current in held.ValueIO
        from result in (
            from chosen in current
            from reference in chosen.Frame
            from test in formed
            where !chosen.Measurement.Exists(taken => ReferenceEquals(taken.Test, test))
            select (chosen.Side, Reference: reference, Pending: new Measurement(test, None))).Match(
                Some: due => held.SwapIO(state => state.Map(chosen => chosen.Side == due.Side ? chosen with { Measurement = Some(due.Pending) } : chosen))
                    .Bind(_ => Measured(due.Pending, due.Reference).Fork())
                    .Map(static _ => Option<FrameComparison>.None),
                None: () => IO.pure(current.Bind(static chosen => chosen.Measurement).Bind(static measurement => measurement.Result)))
        select result;

    private IO<Unit> Measured(Measurement pending, PixelFrame reference) =>
        IO.lift(() => FrameComparison.Of(reference, pending.Test))
            .Bind(measured => held.SwapIO(state => state.Map(chosen =>
                chosen.Measurement == Some(pending) ? chosen with { Measurement = Some(pending with { Result = Some(measured) }) } : chosen)))
            .Map(static _ => unit)
            .Catch(error => IO.lift(() => rendering.Report(error, typeof(SlotComparison), nameof(Measures))))
            .As();

    // --- [PREVIEW]
    public static FrameSide Live { get; } =
        RenderRuns.Formed with {
            Feed = RenderRuns.Formed.Feed with {
                Read =
                    from shown in previewed.ValueIO
                    from read in (from preview in shown from formed in preview.Frame select (Preview: preview, Formed: formed)).Match(
                        Some: static pending => pending.Formed.Match(Succ: static frame => IO.pure(Some(frame)), Fail: error => Dropped(pending.Preview, error)),
                        None: static () => RenderRuns.Formed.Feed.Read)
                    select read,
            },
        };

    public static IO<Unit> Previewed(Option<TileRequest> request) =>
        from shown in previewed.SwapIO(_ => request.Map(static asked => new Preview(asked, None)))
        from formed in shown.Match(
            Some: static preview => Formation(preview.Request)
                .Bind(frame => Landed(preview, frame))
                .Catch(error => Landed(preview, error))
                .As()
                .Fork()
                .Map(static _ => unit),
            None: static () => IO.pure(unit))
        select formed;

    private static IO<Unit> Landed(Preview asked, Fin<PixelFrame> formed) =>
        previewed.SwapIO(state => state.Map(preview => ReferenceEquals(preview, asked) ? preview with { Frame = Some(formed) } : preview)).Map(static _ => unit);

    private static IO<Option<PixelFrame>> Dropped(Preview failed, Error error) =>
        previewed.SwapIO(state => state.Filter(preview => !ReferenceEquals(preview, failed))).Bind(_ => IO.fail<Option<PixelFrame>>(error));

    // --- [ROWS]
    public static IO<Option<FrameSide>> Reference(RowScope scope) =>
        Compared(scope, static comparison => comparison.held.ValueIO, Option<Chosen>.None).Map(static current => current.Map(static chosen => chosen.View));

    public static ControlRow Difference { get; } =
        ToggleRows.Check(
            difference.Source,
            difference.Field with {
                Caption = RowText.Localize("Difference").Local,
                Help = RowText.Localize("Tints the frame where the compared slot differs from the newest rendering").Local,
            },
            Lens.identity<bool>(),
            RowRules.Always) with {
            Wording = Wording.Localized,
        };

    public static IO<Seq<FrameMask>> Masks(RowScope scope) =>
        from shown in scope.Read(difference.Source)
        from current in Compared(scope, static comparison => comparison.held.ValueIO, Option<Chosen>.None)
        select (
            from chosen in current
            from measurement in chosen.Measurement
            from measured in measurement.Result
            where shown
            select new FrameMask(
                new Coverage.Matte(MatteChannel.Lightness, measured.Difference, EffectPipeline.Display.Encoding.Gamut, Inverted: false),
                new MarkStyle.Fill(new MarkColor.Trace(TraceChannel.Red), 1f))).ToSeq();

    public static ControlRow Measures(ComparisonSettingRows bars) =>
        TextRows.Readout(
            RowText.Localize("Measure").Local,
            RowText.Localize("PSNR and NRMSE of the compared slot against the newest rendering, graded by the comparison bars").Local,
            new ReadoutSource<Option<(FrameComparison, Verdict)>>.Scoped(scope =>
                from measured in Compared(scope, static comparison => comparison.Measure, Option<FrameComparison>.None)
                from graded in measured.Traverse(result => bars.Bars(((IPlugInViews)scope.Sink).Settings).Map(levels => (result, result.Graded(levels)))).As()
                select graded),
            static graded => graded.Match(Some: Shown, None: static () => ""),
            RowRules.Always) with {
            Wording = Wording.Localized,
        };

    public static ControlRow Settings { get; } =
        ListRows.List(
            RowText.Localize("Render Settings").Local,
            RowText.Localize("The compared slot's saved render settings against the document's current values").Local,
            new ListRow<SettingsRow, EntryKey>(
                IterableNE.create<ListColumn<SettingsRow>>(
                    new ListColumn<SettingsRow>.Label(Some(RowText.Localize("Setting").Local), static row => row.Setting, None, None),
                    new ListColumn<SettingsRow>.Label(Some(RowText.Localize("Saved").Local), static row => row.A.IfNone(""), None, None),
                    new ListColumn<SettingsRow>.Label(Some(RowText.Localize("Current").Local), static row => row.B.IfNone(""), None, None)),
                static row => row.Key,
                RowSource.Opaque(nameof(SlotComparison), nameof(ListRow<,>.Items), Seq<SettingsRow>(), Items),
                RowSource.Opaque(nameof(SlotComparison), nameof(ListRow<,>.Selected), LanguageExt.HashSet<EntryKey>.Empty)) {
                Wording = Wording.Localized,
            },
            RowRules.Always);

    private static IO<A> Compared<A>(RowScope scope, Func<SlotComparison, IO<A>> read, A absent) =>
        ((IPlugInRendering)scope.Sink).Comparison.ValueIO.Bind(cell => cell.Match(Some: read, None: () => IO.pure(absent)));

    private static string Shown((FrameComparison Measured, Verdict Verdict) graded) =>
        RowText.Localize("{0}, {1}, {2}", arguments: [
            RowText.Localize("PSNR {0:F1} dB", arguments: [graded.Measured.Psnr]).Local,
            graded.Measured.Nrmse.Match(Some: static nrmse => RowText.Localize("NRMSE {0:F3}", arguments: [nrmse]).Local, None: static () => RowText.Localize("NRMSE unmeasured").Local),
            graded.Verdict.Map(pass: RowText.Localize("Pass"), advisory: RowText.Localize("Advisory"), unmeasured: RowText.Localize("Unmeasured"), refuse: RowText.Localize("Refused")).Local,
        ]).Local;

    private static ValueStore<Seq<SettingsRow>> Items(RowScope scope) =>
        ValueStore.Of(
            Compared(scope, comparison => comparison.Listed(scope.Document), Seq<SettingsRow>()).Map(static rows => Some(rows)),
            static _ => IO.pure(unit),
            Applied.Live,
            Some(Marked(scope)));

    private static HostEvent<Unit> Marked(RowScope scope) =>
        new(typeof(SlotComparison), nameof(Settings), (deliver, site) =>
            from cell in ((IPlugInRendering)scope.Sink).Comparison.ValueIO
            let raises = scope.Document.ToSeq().Bind(Edited) + cell.ToSeq().Map(static comparison => comparison.Changed)
            from attached in DisposalOps.AcquireAll(raises.Map(raise => raise.Attach(deliver, site)), DisposalOps.Release)
            select DisposalOps.Composite(attached, site));

    private static Seq<HostEvent<Unit>> Edited(RhinoDoc document) =>
        Seq(EventKind.DocumentPropertiesChanged.In(document.RuntimeSerialNumber).Choose(static _ => Some(unit)),
            EventKind.UndoRedo.In(document.RuntimeSerialNumber).Choose(static _ => Some(unit)));
}
