using System.Numerics;
using System.Runtime.InteropServices;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Display;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.UI.Surfaces;
using Rasm.Rhino.Viewport;
using Rasm.UI;
using Rasm.UI.Imaging;
using Rhino;
using Rhino.Resources;
using Rhino.UI.Controls;
using SkiaSharp;

namespace Rasm.Rhino.Render.UI;

// --- [TYPES] ---------------------------------------------------------------------------
internal delegate IO<float> DisplayHeadroom(Control view);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record StageInput(IO<Option<PixelFrame>> Read, PassContext Context, Option<FramePicker> Pick);

public sealed record TileSource(IO<Option<PixelExtent>> Frame, Func<PixelExtent, IO<Option<PixelFrame>>> Read, Imaging.Tone.Display Display, IO<Option<string>> File, HostEvent<Unit> Changed);

public sealed record FramePixels(FrameSide Side, Option<PixelFrame> Frame, Imaging.Color.Transfer Encoding);

public sealed record FrameSampling(IO<(FramePixels Current, Option<FramePixels> Reference, Seq<FrameMask> Masks)> Read, Seq<HostEvent<Unit>> Changes, IO<Unit> Stop);

public sealed record FrameAction(Option<RowRule> Enabled, Option<RowRule> Marked, Invoke.Effect Effect) {
    public Command<RowRule, Invoke>.Run Faced(CommandFace face, Option<Keys> shortcut) => new(face, Enabled, Effect with { Shortcut = shortcut }) { Marked = Marked };
}

public sealed record FrameCommands(
    FrameAction Compare, Func<CompareMode, FrameAction> Mode, FrameAction Swap, FrameAction Fit, FrameAction ActualSize, FrameAction FlipHorizontal, FrameAction FlipVertical);

public sealed record FramePane(
    Func<RowScope, FrameSampling> Sample,
    Seq<Func<RowScope, CallbackSite, Action<IDisposable>, IO<IO<Option<(FrameTool Tool, Option<string> Readout)>>>>> Targets,
    Func<FrameCommands, Seq<Seq<Command<RowRule, Invoke>>>> Commands,
    FramePicker Picker,
    string Empty);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class FramePicker(Option<RowSource<ScopeState>> scope) {
    private readonly Ref<HashMap<uint, Shown>> views = Ref(HashMap<uint, Shown>());

    public IO<SampleSquare> Sample(RowScope rows) =>
        scope.Match(Some: source => rows.Read(source).Map(static state => state.Sample), None: static () => IO.pure(ScopeState.Default.Sample));

    public IO<Unit> Start(RhinoDoc document, Func<(double X, double Y), IO<Unit>> picked, Action<IDisposable> retain) =>
        from lease in DisposalOps.Acquire(IO.lift(() => new Disposal<Func<(double X, double Y), IO<Unit>>>(picked, released =>
            _ = atomic(() => views.Swap(held => held.TrySetItem(document.RuntimeSerialNumber,
                shown => shown with { Pending = shown.Pending.Filter(next => !ReferenceEquals(next, released)) }))))), retain)
        from started in DisposalOps.OnFailure(
            from swapped in PreviewRows.Swapped(views, held => held.TrySetItem(document.RuntimeSerialNumber, shown => shown with { Pending = Some(picked) }))
            from shown in IO.lift(swapped.After.Find(document.RuntimeSerialNumber).ToFin(new Refused(nameof(Start), Refused.Absent)))
            from armed in shown.Arm
            select armed,
            IO.lift(lease.Dispose))
        select started;

    internal IO<Unit> Hold(uint serial, IO<Unit> arm, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => {
            IDisposable lease = new Disposal<(uint Serial, IO<Unit> Arm)>((serial, arm), released =>
                _ = atomic(() => views.Swap(held => held.Find(released.Serial).Exists(shown => ReferenceEquals(shown.Arm, released.Arm))
                    ? held.Remove(released.Serial)
                    : held)));
            _ = atomic(() => views.Swap(held => held.AddOrUpdate(serial, new Shown(arm, None))));
            return lease;
        }), retain).Map(unit);

    internal IO<Unit> Picked(uint serial, (double X, double Y) pixel) =>
        from swapped in PreviewRows.Swapped(views, held => held.TrySetItem(serial, static shown => shown with { Pending = None }))
        from ran in swapped.Before.Find(serial).Bind(static shown => shown.Pending).Map(run => run(pixel)).IfNone(IO.pure(unit))
        select ran;

    internal IO<Unit> Availability(uint serial, Action<bool> show, IPlugInSink sink, Action<IDisposable> retain) =>
        from heard in PreviewRows.Changes(views).Inline(held => IO.lift(() => show(held.ContainsKey(serial))), sink, retain)
        from current in IO.lift(() => show(views.Value.ContainsKey(serial)))
        select unit;

    // --- [PUBLICATION]
    public static PlugInSlot<FramePicker> Current { get; } = new();

    public Func<IPlugInViews, Action<IDisposable>, IO<Unit>> Published => (owner, _) => Current.Publish(owner, this);

    private sealed record Shown(IO<Unit> Arm, Option<Func<(double X, double Y), IO<Unit>>> Pending);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PreviewRows {
    // --- [CONTEXT]
    public static ControlRow Contextual<TRecord>(
        RowSource<TRecord> source, string caption, string help, IO<Option<StageInput>> read, Seq<HostEvent<Unit>> changes,
        Func<Option<StageInput>, Seq<ControlRow>> rows, RowRules rules,
        Option<Func<RowGrid, Action<IDisposable>, IO<Unit>>> attached = default) where TRecord : notnull {
        Seq<EntryKey> keys = source.Fields.Bind(source.Keys);
        return new ControlRow.Field(source, keys, RowShape.Nested,
            (scope, retain) => Contextualized(read, changes, rows, keys, attached, scope, retain), caption, help, rules);
    }

    private static IO<RowCells> Contextualized(
        IO<Option<StageInput>> read, Seq<HostEvent<Unit>> changes, Func<Option<StageInput>, Seq<ControlRow>> rows, Seq<EntryKey> keys,
        Option<Func<RowGrid, Action<IDisposable>, IO<Unit>>> attached, RowScope scope, Action<IDisposable> retain) =>
        from held in atomIO(Option<(Option<(PassContext Context, Option<FramePicker> Pick)> Input, RowGrid Grid)>.None)
        from owned in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain)
        from panel in Containers.Frame(static () => new Panel(), retain)
        let refresh =
            from input in read
            let context = input.Map(static value => (value.Context, value.Pick))
            from prior in held.ValueIO
            from editing in prior.Traverse(static shown => shown.Grid.Editing).As()
            from replaced in editing.Contains(true) || prior.Exists(shown => shown.Input == context)
                ? prior.Traverse(shown => shown.Grid.Refresh(keys)).As().Map(unit)
                : IO.lift(() => Children(read, rows, input)).Bind(children => Replaced(held, owned, panel, context, children, attached, scope))
            select replaced
        from post in Subscriptions.Idle<Unit, Unit>(_ => refresh)(new CallbackSite(scope.Sink, typeof(PreviewRows), nameof(Contextual)), retain)
        from subscribed in changes.TraverseM(change => change.Inline(raised => post((raised, raised)), scope.Sink, retain)).As()
        from heard in scope.Listen(_ => post((unit, unit)), retain)
        from initial in refresh
        select new RowCells(panel, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None) {
            NestedEditing = held.ValueIO.Bind(static current => current.Traverse(static shown => shown.Grid.Editing).As()).Map(static editing => editing.IfNone(noneValue: false)),
        };

    private static Seq<ControlRow> Children(IO<Option<StageInput>> read, Func<Option<StageInput>, Seq<ControlRow>> rows, Option<StageInput> input) =>
        rows(input.Map(value => value with {
            Read = read.Bind(current => current.Filter(next => next.Context == value.Context && next.Pick == value.Pick)
                .Traverse(static next => next.Read).As()).Map(static frame => frame.Flatten()),
        }));

    private static IO<Unit> Replaced(
        Atom<Option<(Option<(PassContext Context, Option<FramePicker> Pick)> Input, RowGrid Grid)>> held, Retained<IDisposable> owned, Panel panel,
        Option<(PassContext Context, Option<FramePicker> Pick)> context, Seq<ControlRow> children, Option<Func<RowGrid, Action<IDisposable>, IO<Unit>>> attached, RowScope scope) =>
        DisposalOps.Acquire(
            keep => from grid in RowGrid.Realize(children, children, scope, keep)
                    from linked in attached.Traverse(attach => attach(grid, keep)).As()
                    select grid,
            (grid, release) =>
                from published in DisposalOps.Published(owned, (Control?)grid, release, () => panel.Content, content => panel.Content = content)
                from shown in writeIO(held, Some((context, grid)))
                select unit)
            .Map(static _ => unit);

    // --- [IMAGES]
    private static readonly Sizing Grown = new Sizing.Grow(Factor.Create(1f));

    public static IO<SKImage> Image(PixelFrame frame, Imaging.Color.Transfer encoding, Imaging.Tone.Display display, float headroom) =>
        IO.lift(() => encoding == display.Encoding.Transfer ? frame : Encoded(frame, encoding, display.Encoding.Transfer))
            .Bind(shown => Frames.Image(shown, display, headroom));

    private static PixelFrame Encoded(PixelFrame frame, Imaging.Color.Transfer from, Imaging.Color.Transfer to) =>
        new(frame.Origin, frame.Size, frame.Extent, block => {
            frame.Block.CopyTo(block);
            Span<Vector4> pixels = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
            from.Decode(pixels, Imaging.Color.Nits.ReferenceWhite);
            to.Encode(pixels, Imaging.Color.Nits.ReferenceWhite);
        });

    private static IO<float> Headroom(Control view) =>
        IO.lift(static () => Refused.UnlessFound(Platform.Instance.Find<DisplayHeadroom>(), nameof(DisplayHeadroom)).Map(static row => row())).Bind(row => row(view));

    private static IO<Content<SKImage>> Arrived(IO<SKImage> image) =>
        image.Map(static ready => (Content<SKImage>)new Content<SKImage>.Ready(ready))
            .Catch(static error => !error.Is(Errors.Cancelled), static error => IO.pure<Content<SKImage>>(new Content<SKImage>.Failed(error)));

    private static IO<Unit> Forked(IO<Unit> work, Retained<IDisposable> forming, IPlugInSink sink) =>
        from drained in forming.Drain
        from fork in DisposalOps.Fork(
            work.Catch(static error => !error.Is(Errors.Cancelled), error => IO.lift(() => sink.Report(error, typeof(PreviewRows), nameof(Image)))),
            forming.Retain)
        select unit;

    // --- [TILES]
    public static ControlRow Tile(string caption, string help, TileSource source, RowRules rules) =>
        new ControlRow.Readout(RowShape.Block, (scope, retain) => Tiled(source, scope, retain), caption, help, rules);

    private static IO<RowCells> Tiled(TileSource source, RowScope scope, Action<IDisposable> retain) =>
        from held in IO.lift(static () => Ref(TileHeld.Initial))
        from forming in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain)
        let unexpected = RowText.Localize("Preview failed", scope.Sink).Local
        from view in DisposalOps.Acquire(IO.lift(() => new SceneView(scope, Tiling(TileHeld.Initial, unexpected))), retain)
        let site = new CallbackSite(scope.Sink, typeof(PreviewRows), nameof(Tile))
        let refresh = Stepped(view, source, held, forming, scope.Sink)
        from asked in Subscriptions.Idle<Unit, Unit>(_ => refresh)(site, retain)
        from drawn in Subscriptions.Idle<Unit, Unit>(_ => held.ValueIO.Bind(state => view.Show(Tiling(state, unexpected))))(site, retain)
        from heard in Changes(held).Inline(_ => drawn((unit, unit)), scope.Sink, retain)
        from landed in source.Changed.Inline(_ => asked((unit, unit)), scope.Sink, retain)
        from sized in Subscriptions.Attach(h => view.SizeChanged += h, h => view.SizeChanged -= h, Callbacks.Handler<EventArgs>(_ => asked((unit, unit)), site), retain)
        from down in Subscriptions.Attach(
            h => view.MouseDown += h, h => view.MouseDown -= h,
            Callbacks.Handler<MouseEventArgs>(e => Swapped(held, state => state with { Press = Callbacks.Found(e.Buttons is MouseButtons.Primary, e.Location) }).Map(static _ => unit), site), retain)
        from move in Subscriptions.Attach(
            h => view.MouseMove += h, h => view.MouseMove -= h,
            Callbacks.Handler<MouseEventArgs>(e => Dragged(view, source, held, e), site), retain)
        from up in Subscriptions.Attach(
            h => view.MouseUp += h, h => view.MouseUp -= h,
            Callbacks.Handler<MouseEventArgs>(_ => Swapped(held, static state => state with { Press = None }).Map(static _ => unit), site), retain)
        from initial in refresh
        select new RowCells(view, None, None, None, [], [], new RowHelp(None, None), refresh, None, None);

    private static IO<Unit> Stepped(SceneView view, TileSource source, Ref<TileHeld> held, Retained<IDisposable> forming, IPlugInSink sink) =>
        from frame in source.Frame
        from scale in IO.lift(() => Optional(view.ParentWindow).Map(static window => window.LogicalPixelSize))
        let asked =
            from at in scale
            from whole in frame
            from bound in PixelExtent.Validated((int)MathF.Ceiling(view.Width * at), whole.Height).ToOption()
            select whole.Fitted(bound)
        from read in asked.Match(Some: source.Read, None: static () => IO.pure(Option<PixelFrame>.None))
            .Catch(static error => !error.Is(Errors.Cancelled), error => Swapped(held, state => state with { Image = new Content<SKImage>.Failed(error) }).Map(static _ => Option<PixelFrame>.None))
        from swapped in Swapped(held, state => read.Filter(tile => !state.Tile.Exists(shown => ReferenceEquals(shown, tile))).Match(
            Some: tile => state with { Tile = Some(tile), Image = state.Image.Requested(None) },
            None: () => state))
        from formed in swapped.After.Tile.Filter(tile => !swapped.Before.Tile.Exists(prior => ReferenceEquals(prior, tile))).Match(
            Some: tile => Forked(
                from headroom in Headroom(view)
                from image in Arrived(Image(tile, source.Display.Encoding.Transfer, source.Display, headroom))
                from stored in Swapped(held, state => state.Tile.Exists(shown => ReferenceEquals(shown, tile)) ? state with { Image = image } : state)
                select unit, forming, sink),
            None: static () => IO.pure(unit))
        select formed;

    private static Node Tiling(TileHeld held, string unexpected) =>
        held.Image.Drawn(static image => new Node.Overlay(
            new Node.Box { Role = ImagingRoles.Checker, Width = Grown, Aspect = Factor.Create((float)image.Width / image.Height) },
            [new Node.Image(image) { Width = Grown, Height = Grown }]) { Width = Grown }, unexpected);

    private static IO<Unit> Dragged(SceneView view, TileSource source, Ref<TileHeld> held, MouseEventArgs e) =>
        from swapped in Swapped(held, state => state with {
            Press = state.Press.Filter(press => e.Buttons is not MouseButtons.Primary || PointF.Distance(press, e.Location) <= Rhino.UI.Drawing.ComponentControl.DragThreshold),
        })
        from file in swapped.Before.Tile.IsSome && swapped.Before.Press.IsSome && swapped.After.Press.IsNone ? source.File : IO.pure(Option<string>.None)
        from dragged in file.Match(Some: path => Rhino.UI.Rows.Transfer.Drag(view, Seq(path), None), None: static () => IO.pure(unit))
        select dragged;

    private sealed record TileHeld(Option<PixelFrame> Tile, Content<SKImage> Image, Option<PointF> Press) {
        public static TileHeld Initial { get; } = new(None, new Content<SKImage>.Pending(None, None), None);
    }

    // --- [FRAMES]
    public static ControlRow Frame(string caption, string help, FramePane pane, RowRules rules) =>
        new ControlRow.Readout(RowShape.Filled, (scope, retain) => Framed(pane, scope, retain), caption, help, rules);

    public static Func<RowScope, CallbackSite, Action<IDisposable>, IO<IO<Option<(FrameTool Tool, Option<string> Readout)>>>> Area<TRecord>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, FrameRegion> lens, Option<PixelExtent> aspect, Func<FrameRegion, string> readout, Func<TRecord, bool> shown)
        where TRecord : notnull =>
        (scope, site, retain) => Bound(source, field, lens, shown, (area, edited) => (new FrameTool.Region(area, aspect, edited), Some(readout(area))), scope, site, retain);

    public static Func<RowScope, CallbackSite, Action<IDisposable>, IO<IO<Option<(FrameTool Tool, Option<string> Readout)>>>> Quad<TRecord>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, FrameQuad> lens, Func<FrameQuad, Seq<Node>> patches, Func<TRecord, bool> shown)
        where TRecord : notnull =>
        (scope, site, retain) => Bound(source, field, lens, shown, (quad, edited) => (new FrameTool.Cage(quad, patches, edited), Option<string>.None), scope, site, retain);

    private static IO<IO<Option<(FrameTool Tool, Option<string> Readout)>>> Bound<TRecord, TValue>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, Func<TRecord, bool> shown,
        Func<TValue, Func<Edit<TValue>, IO<Unit>>, (FrameTool Tool, Option<string> Readout)> tool, RowScope scope, CallbackSite site, Action<IDisposable> retain)
        where TRecord : notnull where TValue : notnull =>
        from current in atomIO(Option<TRecord>.None)
        from listener in atomIO(Option<Action<Edit<TValue>>>.None)
        from edit in RowEdit.Bind(
            source, IterableNE.create(field),
            (Action<Edit<TValue>> handler) => _ = listener.Swap(_ => Some(handler)), handler => _ = listener.Swap(static _ => None),
            edit => taken => Callbacks.Run(edit.Take<TValue>(taken, (value, record) => lens.Set(value, record)), site),
            held => current.SwapIO(_ => held).Map(static _ => unit), scope, site, retain)
        select current.ValueIO.Map(held => held.Filter(shown).Map(record => tool(lens.Get(record), taken => listener.ValueIO.Map(deliver => deliver.Iter(handle => handle(taken))))));

    private static IO<RowCells> Framed(FramePane pane, RowScope scope, Action<IDisposable> retain) =>
        from sampling in DisposalOps.Acquire(IO.lift(() => pane.Sample(scope)), static held => held.Stop, retain)
        from held in IO.lift(static () => Ref(FrameHeld.Initial))
        from forming in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain)
        let site = new CallbackSite(scope.Sink, typeof(PreviewRows), nameof(Frame))
        let empty = RowText.Localize(pane.Empty, scope.Sink).Local
        let change = fun((Func<FrameHeld, FrameHeld> step) => Swapped(held, step).Map(static _ => unit))
        from targets in pane.Targets.TraverseM(target => target(scope, site, retain)).As()
        let tooled = targets.TraverseM(static read => read).As().Map(static tools => tools.Somes().Head)
        from view in DisposalOps.Acquire(IO.lift(() => new SceneView(scope, Viewed(FrameHeld.Initial, None, empty, change))), retain)
        from readout in DisposalOps.Acquire(IO.lift(static () => new Label { Font = HostTheme.Digits(EtoFonts.SmallFont), TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center }), retain)
        let groups = pane.Commands(Commanded(view, held, tooled, change))
        from strip in ButtonRows.Strip(groups, IconSlot.PanelButton, scope, retain)
        from keyed in CommandRows.Keyed(groups, scope, retain)
        let shown = Shown(view, readout, strip.Refresh, held, tooled, empty, change)
        from drawn in Subscriptions.Idle<Unit, Unit>(_ => shown)(site, retain)
        from heard in Changes(held).Inline(_ => drawn((unit, unit)), scope.Sink, retain)
        let refresh = Sampled(view, sampling, held, forming, scope.Sink)
        from loaded in ViewOps.WhileLoaded(view, keep =>
            from sampled in Subscriptions.Idle<Unit, Unit>(_ => refresh)(site, keep)
            from changed in sampling.Changes.TraverseM(source => source.Inline(_ => sampled((unit, unit)), scope.Sink, keep)).As()
            from listened in scope.Listen(_ => sampled((unit, unit)).Bind(_ => drawn((unit, unit))), keep)
            from initial in refresh
            select unit, site, retain)
        from keys in Subscriptions.Attach(
            h => view.KeyDown += h, h => view.KeyDown -= h,
            Callbacks.Handler<KeyEventArgs>(e => keyed(e.KeyData).Map(handled => {
                e.Handled = handled;
                return unit;
            }), site), retain)
        from picking in scope.Document.ToSeq().TraverseM(document => pane.Picker.Hold(document.RuntimeSerialNumber, Armed(held, pane.Picker, document.RuntimeSerialNumber), retain)).As()
        from spacing in IO.lift(static () => RhinoLayout.Spacing(RhinoLayout.SpacingType.Table))
        from heading in Containers.Frame(() => new TableLayout(
                new TableRow(new TableCell(strip.Control), new TableCell { ScaleWidth = true }, new TableCell(readout))) {
            Spacing = spacing,
            Padding = 0,
        }, retain)
        from body in Containers.Frame(() => new StackLayout(
                new StackLayoutItem(heading),
                new StackLayoutItem(view, expand: true)) {
            Orientation = Eto.Forms.Orientation.Vertical,
            Spacing = spacing.Height,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        }, retain)
        select new RowCells(body, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None);

    private static IO<Unit> Sampled(SceneView view, FrameSampling sampling, Ref<FrameHeld> held, Retained<IDisposable> forming, IPlugInSink sink) =>
        from read in sampling.Read
        from headroom in Headroom(view)
        from swapped in Swapped(held, prior => Merged(prior, read, headroom))
        from formed in when(swapped.After != swapped.Before, Forming(swapped.After, held, forming, sink)).As()
        select formed;

    private static FrameHeld Merged(FrameHeld held, (FramePixels Current, Option<FramePixels> Reference, Seq<FrameMask> Masks) read, float headroom) {
        Option<Formed> current = Kept(held.Current, read.Current, headroom);
        Option<Formed> reference = read.Reference.Bind(pixels => Kept(held.Reference, pixels, headroom));
        return held with {
            Current = current,
            Reference = reference,
            Masks = current.ToSeq().Bind(shown => read.Masks.Map(mask => held.Masks.Find(prior => prior.Mask == mask && ReferenceEquals(prior.Frame, shown.Frame)).Match(
                Some: static prior => prior,
                None: () => new Masked(mask, shown.Frame, new Content<SKImage>.Pending(None, None))))),
            View = current.Map(static shown => shown.Frame.Size) == held.Current.Map(static shown => shown.Frame.Size) ? held.View : None,
            Comparing = reference.IsSome && (held.Comparing || reference.Map(static shown => shown.Pixels.Side) != held.Reference.Map(static shown => shown.Pixels.Side)),
        };
    }

    private static Option<Formed> Kept(Option<Formed> prior, FramePixels pixels, float headroom) =>
        pixels.Frame.Map(frame => prior.Filter(held => held.Holds(pixels, frame, headroom)).Match(
            Some: static held => held,
            None: () => new Formed(pixels, frame, headroom, new Content<SKImage>.Pending(prior.Bind(static held => held.Image.Held), None))));

    private static IO<Unit> Forming(FrameHeld held, Ref<FrameHeld> cell, Retained<IDisposable> forming, IPlugInSink sink) =>
        (held.Current.ToSeq().Filter(static formed => formed.Waiting).Map(static formed => Arrived(formed.Imaged).Map(image => fun((FrameHeld next) =>
                next with { Current = next.Current.Map(kept => ReferenceEquals(kept, formed) ? kept with { Image = image } : kept) })))
         + held.Reference.ToSeq().Filter(static formed => formed.Waiting).Map(static formed => Arrived(formed.Imaged).Map(image => fun((FrameHeld next) =>
                next with { Reference = next.Reference.Map(kept => ReferenceEquals(kept, formed) ? kept with { Image = image } : kept) })))
         + held.Masks.Filter(static masked => masked.Plane is Content<SKImage>.Pending).Map(static masked => Arrived(Frames.Mask(masked.Frame, masked.Mask.Coverage)).Map(plane => fun((FrameHeld next) =>
                next with { Masks = next.Masks.Map(kept => ReferenceEquals(kept, masked) ? kept with { Plane = plane } : kept) }))))
        switch {
            var steps => Forked(
                steps.TraverseM(static step => step).As()
                    .Bind(changes => Swapped(cell, prior => changes.Fold(prior, static (next, apply) => apply(next))))
                    .Map(static _ => unit),
                forming, sink),
        };

    private static IO<Unit> Shown(
        SceneView view, Label readout, IO<Unit> strip, Ref<FrameHeld> held, IO<Option<(FrameTool Tool, Option<string> Readout)>> tooled, string empty,
        Func<Func<FrameHeld, FrameHeld>, IO<Unit>> change) =>
        from state in held.ValueIO
        from tool in tooled
        from pixels in Pixels(view, state)
        from shown in view.Show(Viewed(state, tool.Map(static held => held.Tool), empty, change))
        from text in IO.lift(() => readout.Text = (tool.Bind(static held => held.Readout)
            | (from frame in state.State from across in pixels select (across / MathF.Abs(frame.View.X.Range.Span)).ToString("P0", RowText.Culture))).IfNone(""))
        from refreshed in strip
        select unit;

    private static Node Viewed(FrameHeld held, Option<FrameTool> tool, string empty, Func<Func<FrameHeld, FrameHeld>, IO<Unit>> change) =>
        held.State.Match(
            Some: state => Frames.View(
                state, held.Masks.Map(static masked => (masked.Mask, masked.Plane)), held.Pick | tool,
                edit => edit.Carried.Match(Some: next => change(prior => prior with { View = Some(next.View), Split = next.Split }), None: static () => IO.pure(unit)),
                static _ => IO.pure(unit)),
            None: () => new Node.Text([empty]) { Role = ImagingRoles.Well, Width = Grown, Align = UnitPoint.Center });

    private static IO<Option<float>> Pixels(SceneView view, FrameHeld held) =>
        IO.lift(() => Optional(view.ParentWindow).Map(window => view.Width * window.LogicalPixelSize / (held.State.Exists(static state => state.Reference.IsSome && state.Mode.Rule.IsNone) ? 2f : 1f)));

    private static FrameCommands Commanded(SceneView view, Ref<FrameHeld> held, IO<Option<(FrameTool Tool, Option<string> Readout)>> tooled, Func<Func<FrameHeld, FrameHeld>, IO<Unit>> change) {
        RowRule Holds(Func<FrameHeld, bool> holds) => new([], [], _ => held.ValueIO.Map(holds));
        IO<Option<FrameTool.Cage>> caged = tooled.Map(static tool => tool.Bind(static held => held.Tool.Switch(
            region: static _ => Option<FrameTool.Cage>.None,
            cage: static cage => Some(cage),
            pick: static _ => Option<FrameTool.Cage>.None)));
        FrameAction Act(Option<RowRule> enabled, Option<RowRule> marked, Func<FrameHeld, FrameHeld> step) => new(enabled, marked, new Invoke.Effect(_ => change(step)));
        FrameAction Flip(Func<FrameQuad, FrameQuad> flipped) =>
            new(Some(new RowRule([], [], _ => caged.Map(static cage => cage.IsSome))), None, new Invoke.Effect(_ => caged.Bind(cage => cage.Match(
                Some: held => held.Edited(new Edit<FrameQuad>.Commit(flipped(held.Value))),
                None: static () => IO.pure(unit)))));
        return new(
            Compare: Act(Some(Holds(static state => state.Reference.IsSome)), Some(Holds(static state => state.Comparing)), static state => state with { Comparing = !state.Comparing }),
            Mode: mode => Act(None, Some(Holds(state => state.Mode == mode)), state => state with { Comparing = !(state.Comparing && state.Mode == mode), Mode = mode }),
            Swap: Act(Some(Holds(static state => state.Reference.IsSome)), Some(Holds(static state => state.Swapped)), static state => state with { Swapped = !state.Swapped }),
            Fit: Act(Some(Holds(static state => state.View.IsSome)), None, static state => state with { View = None }),
            ActualSize: new FrameAction(Some(Holds(static state => state.Current.IsSome)), None, new Invoke.Effect(_ =>
                from state in held.ValueIO
                from pixels in Pixels(view, state)
                from changed in change(prior => prior with {
                    View = (from frame in prior.State from across in pixels from actual in Actual(frame.View, across) select actual) | prior.View,
                })
                select changed)),
            FlipHorizontal: Flip(static quad => quad.FlippedHorizontal),
            FlipVertical: Flip(static quad => quad.FlippedVertical));
    }

    private static Option<PlotPlane.Cartesian> Actual(PlotPlane.Cartesian view, float pixels) =>
        (pixels / MathF.Abs(view.X.Range.Span)) switch {
            var factor => from across in view.X.Range.Scaled((view.X.Range.From + view.X.Range.To) / 2f, factor)
                          from down in view.Y.Range.Scaled((view.Y.Range.From + view.Y.Range.To) / 2f, factor)
                          select new PlotPlane.Cartesian(view.X with { Range = across }, view.Y with { Range = down }),
        };

    private static IO<Unit> Armed(Ref<FrameHeld> held, FramePicker picker, uint serial) =>
        Swapped(held, prior => prior with {
            Pick = Some<FrameTool>(new FrameTool.Pick(at =>
                from taken in Swapped(held, static next => next with { Pick = None })
                from ran in taken.Before.Current
                    .Filter(shown => at.X >= 0f && at.Y >= 0f && at.X < shown.Frame.Size.Width && at.Y < shown.Frame.Size.Height)
                    .Map(shown => picker.Picked(serial, (shown.Frame.Origin.X + Math.Floor(at.X) + 0.5d, shown.Frame.Origin.Y + Math.Floor(at.Y) + 0.5d)))
                    .IfNone(IO.pure(unit))
                select ran)),
        }).Map(static _ => unit);

    private sealed record Formed(FramePixels Pixels, PixelFrame Frame, float Headroom, Content<SKImage> Image) {
        public FrameShown Shown => new(Pixels.Side, Image, Frame.Size);

        public bool Waiting => Image is Content<SKImage>.Pending;

        public IO<SKImage> Imaged => PreviewRows.Image(Frame, Pixels.Encoding, Pixels.Side.Display, Headroom);

        public bool Holds(FramePixels pixels, PixelFrame frame, float headroom) =>
            ReferenceEquals(Frame, frame) && Pixels.Side == pixels.Side && Pixels.Encoding == pixels.Encoding && Headroom.Equals(headroom);
    }

    private sealed record Masked(FrameMask Mask, PixelFrame Frame, Content<SKImage> Plane);

    private sealed record FrameHeld(
        Option<Formed> Current, Option<Formed> Reference, Seq<Masked> Masks, Option<PlotPlane.Cartesian> View, CompareMode Mode, Fraction Split, bool Swapped, bool Comparing,
        Option<FrameTool> Pick) {
        public static FrameHeld Initial { get; } = new(None, None, [], None, CompareMode.VerticalWipe, Fraction.Create(0.5f), Swapped: false, Comparing: false, None);

        public Option<FrameState> State =>
            Current.Map(current => new FrameState(
                current.Shown, Reference.Filter(_ => Comparing && Pick.IsNone).Map(static held => held.Shown),
                View.Match(Some: static view => view, None: () => FrameState.Fitted(current.Frame.Size)), Mode, Split, Swapped));
    }

    // --- [PICKS]
    public static IO<Control> Pick(StageInput stage, string caption, Func<Vector4, IO<Unit>> picked, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from available in atomIO(value: false)
        from button in CommandRows.Button(
            new Command<RowRule, Invoke>.Run(
                new CommandFace("frame-pick", caption, None, Some<GlyphSource>(Icons.Embedded(Assets.Rhino.Assembly, ResourceIds.SVGEditorEyeDropperSvg))),
                new RowRule([], [], _ => available.ValueIO),
                new Invoke.Effect(held => stage.Pick.Traverse(picker => held.DocumentIO
                    .Bind(document => picker.Start(document, pixel =>
                        from square in picker.Sample(held)
                        from frame in stage.Read
                        from done in frame.Bind(shown => square.Mean(shown, pixel)).Match(Some: picked, None: static () => IO.pure(unit))
                        select done, owned))).As().Map(unit))),
            IconSlot.Small, scope, owned)
        let site = new CallbackSite(scope.Sink, typeof(FramePicker), nameof(FramePicker.Availability))
        from shown in (from picker in stage.Pick from document in scope.Document
                       select picker.Availability(document.RuntimeSerialNumber, holds => Callbacks.Run(available.SwapIO(_ => holds).Bind(_ => button.Refresh), site), scope.Sink, owned))
            .IfNone(IO.pure(unit))
        select button.Control, retain);

    // --- [VIEWPORTS]
    public static ControlRow LiveViewport(string caption, string help, Option<Guid> mode, Option<CameraPose> pose, RowRules rules) =>
        new ControlRow.Readout(RowShape.Filled, (scope, retain) => Viewed(caption, mode, pose, scope, retain), caption, help, rules);

    private static IO<RowCells> Viewed(string caption, Option<Guid> mode, Option<CameraPose> pose, RowScope scope, Action<IDisposable> retain) =>
        from control in DisposalOps.Acquire(IO.lift(() => new ViewportControl(RowText.Localize(caption, scope.Sink).Local)), retain)
        from moded in mode.Traverse(id => DisplayModes.With(id, display => IO.lift(() => { control.Viewport.DisplayMode = display; }))).As()
        from posed in pose.Traverse(held => IO.lift(() => Cameras.Write(control.Viewport, held))).As()
        from refreshed in IO.lift(control.Refresh)
        select new RowCells(control, None, None, None, [], [], new RowHelp(None, None), IO.lift(control.Refresh), None, None);

    // --- [TRANSITIONS]
    internal static IO<(T Before, T After)> Swapped<T>(Ref<T> held, Func<T, T> change) =>
        atomicIO(() => (Before: held.Value, After: held.Value = change(held.Value)));

    internal static HostEvent<T> Changes<T>(Ref<T> held) =>
        Subscriptions.Host<AtomChangedEvent<T>, T>(
            typeof(PreviewRows), handler => held.Change += handler, handler => held.Change -= handler, static handler => value => handler(sender: null, value), nameof(held.Change));
}
