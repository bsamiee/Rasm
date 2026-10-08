using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Display;
using Rhino.Resources;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Viewers;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record StageInput(IO<Option<PixelFrame>> Read, Gamut Working, Imaging.Tone.Formations.Display Display, OutputDepth Depth, Option<FramePicker> Pick);

public sealed record TileSource(IO<Option<PixelExtent>> Frame, Func<PixelExtent, IO<Option<PixelFrame>>> Read, Imaging.ColorManagement.Transfer Encoding, IO<Option<string>> File);

public sealed record FramePane(
    FrameSide Current,
    Func<RowScope, IO<Option<FrameSide>>> Reference,
    Seq<Func<FrameView, RowScope, CallbackSite, Func<Option<(FrameOverlay Case, FrameEdit Value)>, IO<Unit>>, IO<bool>, IO<(IO<Unit> Shown, IDisposable Release)>>> Targets,
    Seq<ControlRow> Tools,
    FramePicker Picker,
    Option<FrameLink> Link,
    Func<RowScope, IO<Seq<FrameMask>>> Masks);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class FramePicker(Option<RowSource<ScopeState>> scope) {
    private readonly Atom<(HashMap<uint, Shown> Before, HashMap<uint, Shown> After)> views = Atom((Before: HashMap<uint, Shown>(), After: HashMap<uint, Shown>()));

    public IO<SampleSquare> Sample(RowScope rows) =>
        scope.Match(Some: source => rows.Read(source).Map(static state => state.Sample), None: static () => IO.pure(ScopeState.Default.Sample));

    public IO<Unit> Start(RhinoDoc document, PickKind kind, Func<System.Drawing.Point, IO<Unit>> picked) =>
        from swapped in PreviewRows.Swapped(views, held => held.TrySetItem(document.RuntimeSerialNumber, shown => shown with { Pending = Some(picked) }))
        from shown in IO.lift(swapped.After.Find(document.RuntimeSerialNumber).ToFin(new Missing(nameof(Start))))
        from started in shown.View.Pick(kind)
        select started;

    internal IO<IDisposable> Hold(uint serial, FrameView view) =>
        PreviewRows.Swapped(views, held => held.AddOrUpdate(serial, new Shown(view, None)))
            .Map<IDisposable>(_ => new Disposal<uint>(serial, released => _ = PreviewRows.Swapped(views, held => held.Remove(released)).Run()));

    internal IO<Unit> Picked(uint serial, System.Drawing.Point pixel) =>
        from swapped in PreviewRows.Swapped(views, held => held.TrySetItem(serial, static shown => shown with { Pending = None }))
        from ran in swapped.Before.Find(serial).Bind(static shown => shown.Pending).Map(run => run(pixel)).IfNone(IO.pure(unit))
        select ran;

    private sealed record Shown(FrameView View, Option<Func<System.Drawing.Point, IO<Unit>>> Pending);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class PreviewRows {
    // --- [TILES]
    public static ControlRow Tile(string caption, string help, TileSource source, RowRules rules) =>
        new ControlRow.Readout(RowShape.Block, scope => Tiled(source, scope), caption, help, rules);

    private static IO<RowCells> Tiled(TileSource source, RowScope scope) =>
        from view in IO.lift(static () => new ImageView())
        from held in IO.lift(static () => Atom((Before: TileState.Initial, After: TileState.Initial)))
        let site = new CallbackSite(scope.Sink, typeof(PreviewRows), nameof(Tile))
        from acquired in DisposalOps.AcquireAll(
            Seq(IO.pure<IDisposable>(new Disposal<Atom<(TileState Before, TileState After)>>(held, cell => _ = Callbacks.Answer(DisposalOps.Release(cell.Value.After.Owned), static () => unit, site))),
                Subscriptions.Attach(
                    h => view.MouseDown += h, h => view.MouseDown -= h,
                    Callbacks.Handler<MouseEventArgs>(e => Swapped(held, state => state with { Press = Callbacks.Found(e.Buttons == MouseButtons.Primary, e.Location) }).Map(static _ => unit), site)),
                Subscriptions.Attach(
                    h => view.MouseMove += h, h => view.MouseMove -= h,
                    Callbacks.Handler<MouseEventArgs>(e => Dragged(view, source, held, e), site)),
                Subscriptions.Attach(
                    h => view.MouseUp += h, h => view.MouseUp -= h,
                    Callbacks.Handler<MouseEventArgs>(_ => Swapped(held, static state => state with { Press = None }).Map(static _ => unit), site)),
                FrameClocks.Sample(view, Stepped(view, source, held), site)),
            DisposalOps.Release)
        select new RowCells(view, None, None, None, [], [], new RowHelp(None, None), Restaged(view, source, held), None, None, DisposalOps.Composite(acquired, site));

    private static IO<Unit> Stepped(ImageView view, TileSource source, Atom<(TileState Before, TileState After)> held) =>
        from frame in source.Frame
        from window in IO.lift(() => Optional(view.ParentWindow).Map(window => (Scale: window.LogicalPixelSize, view.Width)))
        let asked =
            from at in window
            from whole in frame
            from bound in Callbacks.Found(PixelExtent.Validate((int)MathF.Ceiling(at.Width * at.Scale), whole.Height, out PixelExtent extent) is null, extent)
            select (at.Scale, Extent: whole.Fitted(bound))
        from read in asked.Match(
            Some: at => source.Read(at.Extent).Map(tile => tile.Map(next => (at.Scale, Tile: next))).Catch(error => Failed(held, error)).As(),
            None: static () => IO.pure(Option<(float Scale, PixelFrame Tile)>.None))
        from state in held.ValueIO
        from shown in read
            .Filter(next => !state.After.Shown.Exists(prior => ReferenceEquals(prior.Tile, next.Tile)))
            .Match(Some: next => Showing(view, source, held, next.Tile, next.Scale), None: static () => IO.pure(unit))
        select shown;

    private static IO<Option<(float Scale, PixelFrame Tile)>> Failed(Atom<(TileState Before, TileState After)> held, Error error) =>
        from swapped in Swapped(held, state => state with { Read = new TileRead.Failed(error) })
        from raised in when(swapped.Before.Read != swapped.After.Read, IO.fail<Unit>(error)).As()
        select Option<(float Scale, PixelFrame Tile)>.None;

    private static IO<Unit> Restaged(ImageView view, TileSource source, Atom<(TileState Before, TileState After)> held) =>
        from state in held.ValueIO
        from scale in IO.lift(() => Optional(view.ParentWindow).Map(static window => window.LogicalPixelSize))
        from shown in (from prior in state.After.Shown from at in scale select Showing(view, source, held, prior.Tile, at)).IfNone(IO.pure(unit))
        select shown;

    private static IO<Unit> Showing(
        ImageView view, TileSource source, Atom<(TileState Before, TileState After)> held, PixelFrame tile, float scale) =>
        from staged in Staged(view, source, tile, scale)
        from swapped in Swapped(held, state => state with { Shown = Some(staged), Read = new TileRead.Formed() })
        from shown in IO.lift(() => view.Image = staged.Icon)
        from released in DisposalOps.Release(swapped.Before.Owned)
        select released;

    private static IO<(PixelFrame Tile, Bitmap Staged, Icon Icon)> Staged(ImageView view, TileSource source, PixelFrame tile, float scale) =>
        (from raster in use(FrameView.Raster(view, tile, source.Encoding))
         from staged in IO.lift(() => new Bitmap(tile.Size.Width, tile.Size.Height, PixelFormat.Format32bppRgba))
         from icon in DisposalOps.OnFailure(
             (from graphics in use(() => new Graphics(staged))
              from canvas in PlotCanvas.Of(graphics, 1f, EtoFonts.SmallFont, enabled: true, focused: false)
              let bounds = new RectangleF(0f, 0f, tile.Size.Width, tile.Size.Height)
              from board in Thumbnails.Checker(canvas, bounds, Checkerboard.Content)
              from drawn in Plots.Raster(canvas, bounds, raster)
              select drawn)
             .Bracket()
             .Bind(_ => IO.lift(() => new Icon(scale, staged))),
             IO.lift(staged.Dispose))
         select (tile, staged, icon)).Bracket();

    private static IO<Unit> Dragged(ImageView view, TileSource source, Atom<(TileState Before, TileState After)> held, MouseEventArgs e) =>
        from swapped in Swapped(held, state => state with {
            Press = state.Press.Filter(press => e.Buttons != MouseButtons.Primary || PointF.Distance(press, e.Location) <= ComponentControl.DragThreshold),
        })
        let icon = swapped.Before.Shown.Filter(_ => swapped.Before.Press.IsSome && swapped.After.Press.IsNone).Map(static shown => shown.Icon)
        from file in icon.Traverse(_ => source.File).As()
        from dragged in (from shown in icon from found in file from path in found select Rows.Transfer.Drag(view, Seq(path), Some<Image>(shown))).IfNone(IO.pure(unit))
        select dragged;

    private sealed record TileState(Option<(PixelFrame Tile, Bitmap Staged, Icon Icon)> Shown, TileRead Read, Option<PointF> Press) {
        public static TileState Initial { get; } = new(None, new TileRead.Formed(), None);

        public Seq<IDisposable> Owned => Shown.ToSeq().Bind(static held => Seq<IDisposable>(held.Staged, held.Icon));
    }

    [Union]
    private abstract partial record TileRead {
        public sealed record Formed : TileRead;

        public sealed record Failed(Error Error) : TileRead;
    }

    // --- [FRAMES]
    public static ControlRow Frame(string caption, string help, FramePane pane, RowRules rules) =>
        new ControlRow.Readout(RowShape.Filled, scope => Framed(pane, scope), caption, help, rules);

    public static Func<FrameView, RowScope, CallbackSite, Func<Option<(FrameOverlay Case, FrameEdit Value)>, IO<Unit>>, IO<bool>, IO<(IO<Unit> Shown, IDisposable Release)>> Area<TRecord>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, FrameRegion> lens, Option<PixelExtent> aspect, Func<FrameRegion, string> readout, Func<TRecord, bool> shown)
        where TRecord : notnull =>
        (view, scope, site, show, current) => Bound(
            source, field, lens, shown, new FrameOverlay.AreaPick(aspect, readout), static area => new FrameEdit.AreaEdited(area),
            static edit => edit.Switch(areaEdited: static held => Some(held.Area), quadEdited: static _ => Option<FrameRegion>.None, pixelPicked: static _ => Option<FrameRegion>.None),
            view, scope, site, show, current);

    public static Func<FrameView, RowScope, CallbackSite, Func<Option<(FrameOverlay Case, FrameEdit Value)>, IO<Unit>>, IO<bool>, IO<(IO<Unit> Shown, IDisposable Release)>> Quad<TRecord>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, FrameQuad> lens, Func<FrameQuad, Seq<QuadPatch>> patches, Gamut working, OverlayOpacity opacity, Func<TRecord, bool> shown)
        where TRecord : notnull =>
        (view, scope, site, show, current) => Bound(
            source, field, lens, shown, new FrameOverlay.QuadCage(patches, working, opacity), static quad => new FrameEdit.QuadEdited(quad),
            static edit => edit.Switch(areaEdited: static _ => Option<FrameQuad>.None, quadEdited: static held => Some(held.Quad), pixelPicked: static _ => Option<FrameQuad>.None),
            view, scope, site, show, current);

    private static IO<(IO<Unit> Shown, IDisposable Release)> Bound<TRecord, TValue>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, Func<TRecord, bool> shown, FrameOverlay @case, Func<TValue, FrameEdit> value,
        Func<FrameEdit, Option<TValue>> take, FrameView view, RowScope scope, CallbackSite site, Func<Option<(FrameOverlay Case, FrameEdit Value)>, IO<Unit>> show, IO<bool> current)
        where TRecord : notnull =>
        RowEdit.Bind<TRecord, EventHandler<Edit<FrameEdit>>>(
                source, IterableNE.create(field),
                handler => view.Edited += handler, handler => view.Edited -= handler,
                edit => Callbacks.Handler<Edit<FrameEdit>>(
                    next =>
                        from on in current
                        from taken in when(on && Owned(next, take), edit.Take(next, Into(take, lens))).As()
                        select taken,
                    site),
                held => show(held.Filter(shown).Map(static record => (@case, value(lens.Get(record))))),
                scope, site)
            .Map(static bound => (bound.Edit.Shown, bound.Release));

    private static bool Owned<TValue>(Edit<FrameEdit> edit, Func<FrameEdit, Option<TValue>> take) =>
        edit.Switch(
            take,
            preview: static (of, held) => of(held.Value).IsSome,
            step: static (of, held) => of(held.Value).IsSome,
            commit: static (of, held) => of(held.Value).IsSome,
            cancel: static (_, _) => true);

    private static Func<FrameEdit, TRecord, Fin<TRecord>> Into<TRecord, TValue>(Func<FrameEdit, Option<TValue>> take, Lens<TRecord, TValue> lens) =>
        (held, record) => take(held).Map(focus => lens.Set(focus, record)).IfNone(record);

    private static IO<RowCells> Framed(FramePane pane, RowScope scope) =>
        from view in IO.lift(() => new FrameView(scope.Sink, pane.Current, pane.Reference(scope)))
        from readout in IO.lift(static () => new Label { Font = Themes.Digits(EtoFonts.SmallFont), TextAlignment = TextAlignment.Right })
        from cells in IO.lift(() => Atom(toList(pane.Targets.Map(static _ => Option<(FrameOverlay Case, FrameEdit Value)>.None))))
        from ticks in IO.lift(static () => Atom((Before: PaneTick.Initial, After: PaneTick.Initial)))
        let site = new CallbackSite(scope.Sink, typeof(PreviewRows), nameof(Frame))
        from tools in DisposalOps.AcquireAll(pane.Tools.Choose(row => Realized(row, scope)), static held => DisposalOps.Release(held.Map(static cell => cell.Release)))
        let tooled = tools.Map(static cell => cell.Release)
        from compare in DisposalOps.OnFailure(
            ButtonRows.Split(view.Compare, view.Modes.Map(static mode => (mode.Command, (IGlyph)mode.Mode.Glyph)), IconSlot.PanelButton, scope.Sink), DisposalOps.Release(tooled))
        from swap in DisposalOps.OnFailure(ButtonRows.Image(view.Swap, GlyphRole.Swap, IconSlot.PanelButton, scope.Sink), DisposalOps.Release(tooled.Add(compare.Release)))
        let toggled = tooled + Seq(compare.Release, swap.Release)
        from actions in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(
                Seq(ButtonRows.Image(view.Fit, GlyphRole.Fit, IconSlot.PanelButton, scope.Sink), ButtonRows.Image(view.ActualSize, GlyphRole.ActualSize, IconSlot.PanelButton, scope.Sink)),
                static held => DisposalOps.Release(held.Map(static button => button.Release))),
            DisposalOps.Release(toggled))
        let toolbar = toggled + actions.Map(static button => button.Release)
        from targets in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(
                pane.Targets.Map((target, index) => Targeted(target, index, view, scope, site, cells)),
                static held => DisposalOps.Release(held.Map(static bound => bound.Release))),
            DisposalOps.Release(toolbar))
        let bound = toolbar + targets.Map(static target => target.Release)
        from tail in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(
                scope.Document.ToSeq()
                    .Bind(document => Picking(pane.Picker, document.RuntimeSerialNumber, view, site))
                    .Add(FrameClocks.Sample(view, Ticked(view, readout, pane.Link, pane.Masks(scope), cells, ticks), site)),
                DisposalOps.Release),
            DisposalOps.Release(bound))
        let held = bound + tail
        from body in DisposalOps.OnFailure(
            from leading in ButtonRows.Strip(tools.Map(static cell => cell.Value), Seq<Control>(compare.Control, swap.Control), None)
            from trailing in ButtonRows.Strip(actions.Map(static button => (Control)button.Control), [], None)
            from spacing in IO.lift(static () => RhinoLayout.Spacing(RhinoLayout.SpacingType.Table))
            select new StackLayout(
                new StackLayoutItem(new TableLayout(new TableRow(new TableCell(leading), new TableCell { ScaleWidth = true }, new TableCell(readout), new TableCell(trailing))) {
                    Spacing = spacing,
                    Padding = 0,
                }),
                new StackLayoutItem(view, expand: true)) {
                Orientation = Orientation.Vertical,
                Spacing = spacing.Height,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            },
            DisposalOps.Release(held))
        select new RowCells(
            body, None, None, None, [], [], new RowHelp(None, None),
            (targets.Map(static target => target.Shown) + tools.Map(static cell => cell.Show)).TraverseM(static show => show).As().Map(static _ => unit),
            None, None, DisposalOps.Composite(held, site));

    private static Option<IO<RowCells>> Realized(ControlRow row, RowScope scope) =>
        row.Switch<RowScope, Option<IO<RowCells>>>(
            scope,
            field: static (held, field) => field.Realize(held),
            readout: static (held, readout) => readout.Realize(held),
            command: static (held, command) => command.Realize(held),
            group: static (_, _) => None);

    private static IO<(IO<Unit> Shown, IDisposable Release)> Targeted(
        Func<FrameView, RowScope, CallbackSite, Func<Option<(FrameOverlay Case, FrameEdit Value)>, IO<Unit>>, IO<bool>, IO<(IO<Unit> Shown, IDisposable Release)>> target,
        int index, FrameView view, RowScope scope, CallbackSite site, Atom<Lst<Option<(FrameOverlay Case, FrameEdit Value)>>> cells) =>
        target(
            view, scope, site,
            shown => cells.SwapIO(held => held.SetItem(index, shown)).Map(static _ => unit),
            cells.ValueIO.Map(held => toSeq(held).Map(static (shown, at) => (Shown: shown, At: at)).Find(static cell => cell.Shown.IsSome).Exists(cell => cell.At == index)));

    private static Seq<IO<IDisposable>> Picking(FramePicker picker, uint serial, FrameView view, CallbackSite site) =>
        Seq(picker.Hold(serial, view),
            Subscriptions.Attach(
                h => view.Edited += h, h => view.Edited -= h,
                Callbacks.Handler<Edit<FrameEdit>>(edit => PickedPixel(edit).Map(pixel => picker.Picked(serial, pixel)).IfNone(IO.pure(unit)), site)));

    private static Option<System.Drawing.Point> PickedPixel(Edit<FrameEdit> edit) =>
        edit.Switch(
            preview: static _ => None,
            step: static _ => None,
            commit: static commit => commit.Value.Switch(
                areaEdited: static _ => None, quadEdited: static _ => None, pixelPicked: static picked => Some(picked.Pixel)),
            cancel: static _ => None);

    private static IO<Unit> Ticked(
        FrameView view, Label readout, Option<FrameLink> link, IO<Seq<FrameMask>> masks, Atom<Lst<Option<(FrameOverlay Case, FrameEdit Value)>>> cells,
        Atom<(PaneTick Before, PaneTick After)> ticks) =>
        from hovered in view.Hovered
        from zoom in view.Zoom
        from own in masks
        from linked in link.Match(Some: static held => held.Masks.ValueIO, None: static () => IO.pure(Seq<FrameMask>()))
        from shown in cells.ValueIO
        from tick in Swapped(ticks, _ => new PaneTick(
            hovered,
            zoom,
            toSeq(shown).Somes().Head.Map(static cell => (cell.Case, Some(cell.Value)))
            | Seq(own, linked).Find(static set => !set.IsEmpty).Map(static set => ((FrameOverlay)new FrameOverlay.MaskSet(set), Option<FrameEdit>.None))))
        from pointed in when(
            tick.After.Hovered != tick.Before.Hovered,
            link.Match(Some: static held => held.Pointer.SwapIO(static _ => tick.After.Hovered).Map(static _ => unit), None: static () => IO.pure(unit))).As()
        from zoomed in when(
            tick.After.Zoom != tick.Before.Zoom,
            IO.lift(() => readout.Text = tick.After.Zoom.Match(Some: static held => ((float)held).ToString("P0", RowText.Culture), None: static () => "")).Map(static _ => unit)).As()
        from overlaid in when(
            tick.After.Overlay != tick.Before.Overlay,
            view.Overlay(tick.After.Overlay.Map(static held => held.Case))
                .Bind(_ => tick.After.Overlay.Bind(static held => held.Value).Map(value => view.Receive(Some(value))).IfNone(IO.pure(unit)))).As()
        select unit;

    private sealed record PaneTick(Option<System.Drawing.Point> Hovered, Option<FrameZoom> Zoom, Option<(FrameOverlay Case, Option<FrameEdit> Value)> Overlay) {
        public static PaneTick Initial { get; } = new(None, None, None);
    }

    // --- [VIEWPORTS]
    public static ControlRow LiveViewport(string caption, string help, Option<Guid> mode, Option<CameraPose> pose, RowRules rules) =>
        new ControlRow.Readout(RowShape.Filled, scope => Viewed(caption, mode, pose, scope), caption, help, rules);

    private static IO<RowCells> Viewed(string caption, Option<Guid> mode, Option<CameraPose> pose, RowScope scope) =>
        from control in IO.lift(() => new ViewportControl(Wording.English.Shown(caption, scope.Sink)))
        from moded in mode.Traverse(id =>
            from display in IO.lift(() => Missing.Unless(DisplayModeDescription.GetDisplayMode(id), nameof(DisplayModeDescription.GetDisplayMode)))
            from set in IO.lift(() => control.Viewport.DisplayMode = display)
            select unit).As()
        from posed in pose.Traverse(held => IO.lift(() => Cameras.WritePose(control.Viewport, held))).As()
        from refreshed in IO.lift(control.Refresh)
        select new RowCells(control, None, None, None, [], [], new RowHelp(None, None), IO.lift(control.Refresh), None, None, control);

    // --- [TRANSITIONS]
    internal static IO<(T Before, T After)> Swapped<T>(Atom<(T Before, T After)> held, Func<T, T> change) =>
        held.SwapIO(state => (state.After, change(state.After)));
}
