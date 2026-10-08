using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone.Formations;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Editors;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino.Resources;

namespace Rasm.Rhino.UI.Viewers;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ScopeSource(string Caption, IO<Option<ScopeFrames>> Frames);

public sealed record ScopeState(ScopeOptions Options, PlotAspect Aspect, SampleSquare Sample) : IStateRecord<ScopeState, ScopeParameter, InvalidRhinoValue> {
    public static ScopeState Default { get; } = new(ScopeOptions.Default, PlotAspect.Proportional, SampleSquare.One);
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScopeParameter : IStateParameter<ScopeState> {
    private static readonly Lens<ScopeState, ScopeOptions> Options =
        Lens<ScopeState, ScopeOptions>.New(static state => state.Options, static options => state => state with { Options = options });

    public static readonly ScopeParameter Chart = new("kind", new StateParameter<ScopeState>.Choice<ScopeKind, InvalidRhinoValue>(
        lens(Options, Lens<ScopeOptions, ScopeKind>.New(static options => options.Kind, static kind => options => options with { Kind = kind }))));
    public static readonly ScopeParameter Counts = new("counts", new StateParameter<ScopeState>.Choice<CountScale, InvalidRhinoValue>(
        lens(Options, Lens<ScopeOptions, CountScale>.New(static options => options.Counts, static counts => options => options with { Counts = counts }))));
    public static readonly ScopeParameter Range = new("range", new StateParameter<ScopeState>.Choice<HistogramRange, InvalidRhinoValue>(
        lens(Options, Lens<ScopeOptions, HistogramRange>.New(static options => options.Range, static range => options => options with { Range = range }))));
    public static readonly ScopeParameter Channels = new("channels", new StateParameter<ScopeState>.Choice<TraceMix, InvalidRhinoValue>(
        lens(Options, Lens<ScopeOptions, TraceMix>.New(static options => options.Channels, static channels => options => options with { Channels = channels }))));
    public static readonly ScopeParameter Brightness = new("brightness", new StateParameter<ScopeState>.Choice<TraceBrightness, InvalidRhinoValue>(
        lens(Options, Lens<ScopeOptions, TraceBrightness>.New(static options => options.Brightness, static brightness => options => options with { Brightness = brightness }))));
    public static readonly ScopeParameter Aspect = new("aspect", new StateParameter<ScopeState>.Keyed<PlotAspect, float, InvalidRhinoValue>(
        Lens<ScopeState, PlotAspect>.New(static state => state.Aspect, static aspect => state => state with { Aspect = aspect })));
    public static readonly ScopeParameter Sample = new("sample", new StateParameter<ScopeState>.Keyed<SampleSquare, int, InvalidPixelValue>(
        Lens<ScopeState, SampleSquare>.New(static state => state.Sample, static sample => state => state with { Sample = sample })));

    public StateParameter<ScopeState> Kind { get; }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class FrameLink {
    public Atom<Option<System.Drawing.Point>> Pointer { get; } = Atom(Option<System.Drawing.Point>.None);
    public Atom<Seq<FrameMask>> Masks { get; } = Atom(Seq<FrameMask>());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ChartRows {
    // --- [SCOPE]
    public static (ControlRow Row, RowSource<ScopeState> Source) Scope(
        string caption, string help, IterableNE<ScopeSource> sources, string owner, Func<RowScope, ValueStore<ScopeState>> store, Option<FrameLink> link, RowRules rules) =>
        IterableNE.create(ScopeParameter.Items[0], ScopeParameter.Items.Skip(1)).Map(item => new RowField<ScopeState>(item, caption, help)) switch {
            var fields => RowSource.Fields(owner, toSeq(fields).Map(static field => (field.Parameter, field.Caption)), store) switch {
                var source => (ControlRow.Of(source, fields, caption, help, RowShape.Block, scope => Realized(caption, sources, source, fields, link, scope), rules), source),
            },
        };

    private sealed record ScopeHeld(ScopeSource Source, Option<System.Drawing.Point> Pointed, Option<System.Drawing.Point> Frozen);

    private static IO<RowCells> Realized(
        string caption, IterableNE<ScopeSource> sources, RowSource<ScopeState> source, IterableNE<RowField<ScopeState>> fields, Option<FrameLink> link, RowScope scope) =>
        from held in IO.lift(() => Atom(new ScopeHeld(sources.Head, None, None)))
        from view in IO.lift(() => new ScopeView(
            scope.Sink, Wording.English.Shown(caption, scope.Sink), new ScopeEdit.Chart(source.Default.Options, source.Default.Aspect), held.ValueIO.Bind(static shown => shown.Source.Frames)))
        from label in IO.lift(static () => new Label { Wrap = Eto.Forms.WrapMode.None, Font = HostTheme.Digits(EtoFonts.SmallFont) })
        from popup in ChoiceRows.Popup(
            toSeq(ScopeKind.Items).Map(static kind => new ChoiceItem<ScopeKind>(kind, ScopeView.Caption(kind), None)), kind => Chosen(view, options => options with { Kind = kind }), scope.Sink)
        let site = new CallbackSite(scope.Sink, typeof(ScopeView), nameof(ScopeView.Edited))
        from bound in DisposalOps.OnFailure(
            RowEdit.Bind(
                source, fields, h => view.Edited += h, h => view.Edited -= h,
                edit => Callbacks.Handler<EditEventArgs<ScopeEdit>>(args => edit.Take(args.Edit, Into), site),
                value =>
                    from received in view.Receive(value.Map(static state => (ScopeEdit)new ScopeEdit.Chart(state.Options, state.Aspect)))
                    from picked in popup.Show(value.Map(static state => state.Options.Kind))
                    from inked in IO.lift(() => label.TextColor = new Color(PaintSlot.ControlText.Read(), MarkStyle.Label.Alpha))
                    select unit,
                scope, site),
            IO.lift(popup.Release.Dispose))
        let bracketed = Seq(popup.Release, bound.Release)
        from menu in DisposalOps.OnFailure(ChoiceRows.Menu([], [], Some(Listing(toSeq(sources), source, bound.Edit, view, held, scope)), scope.Sink), DisposalOps.Release(bracketed))
        from menuButton in DisposalOps.OnFailure(
            ButtonRows.Menu(menu.Menu, GlyphRole.Options, IconSlot.PanelButton, RowText.Localize("Scope Options").Local, scope.Sink), DisposalOps.Release(bracketed.Add(menu.Release)))
        let buttoned = bracketed + Seq(menu.Release, menuButton.Release)
        from padlock in DisposalOps.OnFailure(link.Traverse(_ => Locked(held, scope.Sink)).As(), DisposalOps.Release(buttoned))
        let locked = buttoned + padlock.Map(static toggle => toggle.Release).ToSeq()
        from tick in DisposalOps.OnFailure(
            FrameClocks.Sample(
                label, Ticked(view, label, padlock.Map(static toggle => toggle.Command), link, held, source, scope), new CallbackSite(scope.Sink, typeof(FrameClocks), nameof(FrameClocks.Sample))),
            DisposalOps.Release(locked))
        select new RowCells(
            view, None, None, None,
            Seq(new RowLine(None, popup.Control, Some<Control>(menuButton.Control), None)),
            Seq(new RowLine(None, label, padlock.Map(static toggle => (Control)toggle.Control), None)),
            new RowHelp(None, None), bound.Edit.Shown, Some<RowEdit>(bound.Edit), None,
            DisposalOps.Composite(locked.Add(tick), new CallbackSite(scope.Sink, typeof(ChartRows), nameof(Scope))));

    private static IO<Seq<MenuPick>> Listing(
        Seq<ScopeSource> sources, RowSource<ScopeState> source, RowEdit<ScopeState> edit, ScopeView view, Atom<ScopeHeld> held, RowScope scope) =>
        from state in scope.Read(source)
        from shown in held.ValueIO
        let brightness = Picks(
            toSeq(TraceBrightness.Items), static level => RowText.Localize(level.Map(bright: "Bright", normal: "Normal", dimmed: "Dimmed")).Local,
            state.Options.Brightness, level => Chosen(view, options => options with { Brightness = level }))
        let histogram = Picks(
                toSeq(CountScale.Items), static scale => RowText.Localize(scale.Map(linear: "Linear Count", logarithmic: "Logarithmic Count")).Local,
                state.Options.Counts, scale => Chosen(view, options => options with { Counts = scale }))
            .Add(new MenuPick.Item(
                RowText.Localize("SDR Only").Local, state.Options.Range == HistogramRange.Sdr,
                Chosen(view, static options => options with { Range = options.Range.Map(full: HistogramRange.Sdr, sdr: HistogramRange.Full) })))
            .Add(new MenuPick.Group(RowText.Localize("Channels").Local, Picks(
                toSeq(TraceMix.Items),
                static mix => RowText.Localize(mix.Map(rgbLuma: "RGB and Luma", rgb: "RGB", luma: "Luma", red: "Red", green: "Green", blue: "Blue")).Local,
                state.Options.Channels, mix => Chosen(view, options => options with { Channels = mix }))))
        select state.Options.Kind.Map(histogram: histogram, waveformLuma: brightness, waveformRgb: brightness, parade: brightness, vectorscope: brightness, chromaticity: Seq<MenuPick>())
            + Seq<MenuPick>(new MenuPick.Group(RowText.Localize("Source").Local, Picks(
                    sources, entry => Wording.English.Shown(entry.Caption, scope.Sink), shown.Source,
                    picked => held.SwapIO(current => current with { Source = picked }).Map(static _ => unit))))
                .Filter(_ => sources.Count > 1)
            + Seq<MenuPick>(new MenuPick.Group(RowText.Localize("Sample").Local, Picks(
                toSeq(SampleSquare.Items), static square => RowText.Localize("{0} × {0}", arguments: [square.Key]).Local,
                state.Sample, square => edit.Take(new Edit<SampleSquare>.Commit(square), static (picked, record) => record with { Sample = picked }))));

    private static Seq<MenuPick> Picks<TValue>(Seq<TValue> items, Func<TValue, string> caption, TValue marked, Func<TValue, IO<Unit>> run) =>
        items.Map(item => (MenuPick)new MenuPick.Item(caption(item), EqualityComparer<TValue>.Default.Equals(item, marked), run(item)));

    private static IO<Unit> Chosen(ScopeView view, Func<ScopeOptions, ScopeOptions> change) =>
        view.Advance(state => new Transition<ScopeViewState, ScopeEdit>(
            state, Some<Edit<ScopeEdit>>(new Edit<ScopeEdit>.Commit(new ScopeEdit.Chart(change(state.Value.Chosen), state.Value.WellAspect)))));

    private static Fin<ScopeState> Into(ScopeEdit edit, ScopeState held) =>
        edit.Switch(held,
            chart: static (state, chart) => state with { Options = chart.Options, Aspect = chart.Aspect },
            points: static (state, _) => state);

    // --- [READOUT]
    private static IO<(ToggleButton Control, CheckCommand Command, IDisposable Release)> Locked(Atom<ScopeHeld> held, IPlugInSink sink) =>
        from command in IO.lift(static () => new CheckCommand { ToolTip = RowText.Localize("Lock Pixel Readout").Local, Enabled = false })
        let site = new CallbackSite(sink, typeof(CheckCommand), nameof(CheckCommand.CheckedChanged))
        from frozen in Subscriptions.Attach(
            h => command.CheckedChanged += h, h => command.CheckedChanged -= h,
            Callbacks.Handler<EventArgs>(_ =>
                from ticked in IO.lift(() => command.Checked)
                from swapped in held.SwapIO(state => state with { Frozen = ticked ? state.Pointed : None })
                select unit, site))
        from toggle in DisposalOps.OnFailure(ToggleRows.Command(command, GlyphRole.LockClosed, IconSlot.PanelButton, sink), IO.lift(frozen.Dispose))
        select (toggle.Control, command, DisposalOps.Composite(Seq(frozen, toggle.Release), site));

    private static IO<Unit> Ticked(
        ScopeView view, Label label, Option<CheckCommand> padlock, Option<FrameLink> link, Atom<ScopeHeld> held, RowSource<ScopeState> source, RowScope scope) =>
        from marks in view.Marks
        from pointer in link.Match(Some: static linked => linked.Pointer.ValueIO, None: static () => IO.pure(Option<System.Drawing.Point>.None))
        from shown in held.SwapMaybeIO(prior => pointer.Map(at => prior with { Pointed = Some(at) }))
        from frames in shown.Source.Frames
        from state in scope.Read(source)
        let text = Reading(marks, frames, shown.Frozen | pointer, state.Sample)
        from labelled in IO.lift(() => label.Text)
        from written in when(!string.Equals(labelled, text, StringComparison.Ordinal), IO.lift(() => { label.Text = text; })).As()
        from enabled in IO.lift(() => padlock.Iter(command => command.Enabled = shown.Pointed.IsSome))
        from masked in link.Match(
            Some: linked => frames.Map(formed => Masks(marks, formed)).IfNone(Seq<FrameMask>()) switch {
                var next => linked.Masks.SwapMaybeIO(current => Some(next).Filter(fresh => fresh != current)).Map(static _ => unit),
            },
            None: static () => IO.pure(unit))
        select unit;

    private static string Reading(ScopeMarks marks, Option<ScopeFrames> frames, Option<System.Drawing.Point> pixel, SampleSquare square) =>
        marks.Hovered.Match(
                Some: static hovered => Binned(hovered.Bin, hovered.Reading),
                None: () => from shown in frames from at in pixel from text in Sampled(shown, at, square) select text)
            .IfNone("");

    private static Option<string> Binned(int bin, ScopeReading.Histogram reading) =>
        toSeq(MemoryMarshal.ToEnumerable(reading.Bins.Luma.Counts))
            .Map((count, at) => (Below: at <= bin ? count : 0L, Covered: (long)count))
            .Fold((Below: 0L, Covered: 0L), static (sum, entry) => (sum.Below + entry.Below, sum.Covered + entry.Covered)) switch {
                (_, 0L) => None,
                var (below, covered) => Some(RowText.Localize(
                        "{0:F3} · bin {1} · {2} · {3:F3} %", arguments: [HistogramAxis.LowerBound(bin), bin, reading.Bins.Luma.Row(0)[bin], 100d * below / covered]).Local),
            };

    private static Option<string> Sampled(ScopeFrames frames, System.Drawing.Point pixel, SampleSquare square) =>
        frames.Scene.Match(
            Some: scene =>
                from linear in square.Mean(scene.Frame, pixel)
                from formed in square.Mean(frames.Frame, pixel)
                select RowText.Localize(
                    "{0:F3} {1:F3} {2:F3} → {3:F3} {4:F3} {5:F3} · {6} × {6}", arguments: [linear.X, linear.Y, linear.Z, formed.X, formed.Y, formed.Z, square.Key]).Local,
            None: () => square.Mean(frames.Frame, pixel).Map(formed =>
                RowText.Localize("{0:F3} {1:F3} {2:F3} · {3} × {3}", arguments: [formed.X, formed.Y, formed.Z, square.Key]).Local));

    private static Seq<FrameMask> Masks(ScopeMarks marks, ScopeFrames frames) =>
        marks.Selection.Map(range => new FrameMask(new Coverage.Luma(frames.Gamut, range), MarkStyle.Selection)).ToSeq()
            .Concat(
                from end in marks.Clips.ToSeq()
                from levels in frames.Levels.Map(static quantizer => quantizer.Clipping).ToSeq()
                from clip in Seq((Shown: end.Below, End: ClipEnd.Low, Channel: TraceChannel.Blue), (Shown: end.Above, End: ClipEnd.High, Channel: TraceChannel.Red))
                where clip.Shown
                select new FrameMask(new Coverage.Clipped(levels, clip.End), new MarkStyle.Fill(new MarkColor.Trace(clip.Channel), 1f)));

    // --- [RAMP_KEY]
    public static ControlRow RampKey(string caption, string help, Func<RowScope, IO<Option<AgXLook>>> look, RowRules rules) =>
        new ControlRow.Readout(RowShape.Block, scope =>
            from shown in look(scope)
            from ramp in IO.lift(() => new GradientRamp(scope.Sink, Wording.English.Shown(caption, scope.Sink), new GradientRampState.Key(shown)))
            select new RowCells(
                ramp, None, None, None, [], [], new RowHelp(None, None),
                look(scope).Bind(current => ramp.Advance(_ => new(new GradientRampState.Key(current), None))),
                None, None, ramp),
            caption, help, rules);
}
