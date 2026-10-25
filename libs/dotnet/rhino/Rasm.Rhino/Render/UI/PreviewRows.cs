using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Display;
using Rhino.Resources;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record StageInput(IO<Option<PixelFrame>> Read, PassContext Context, Option<FramePicker> Pick);

public sealed record TileSource(IO<Option<PixelExtent>> Frame, Func<PixelExtent, IO<Option<PixelFrame>>> Read, Imaging.Color.Transfer Encoding, IO<Option<string>> File);

public sealed record FramePane(
    Func<RowScope, FrameSampling> Sample,
    Seq<Func<FrameView, RowScope, CallbackSite, Func<Option<(FrameOverlay Case, FrameEdit Value)>, IO<Unit>>, IO<bool>, Action<IDisposable>, IO<IO<Unit>>>> Targets,
    Seq<CommandRow.Check> Tools,
    FramePicker Picker,
    Option<FrameLink> Link,
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
            from pickedView in shown.View.Pick
            select pickedView,
            IO.lift(lease.Dispose))
        select started;

    internal IO<Unit> Hold(uint serial, FrameView view, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => {
            IDisposable lease = new Disposal<(uint Serial, FrameView View)>((serial, view), released =>
                _ = atomic(() => views.Swap(held => held.Find(released.Serial).Exists(shown => ReferenceEquals(shown.View, released.View))
                    ? held.Remove(released.Serial)
                    : held)));
            _ = atomic(() => views.Swap(held => held.AddOrUpdate(serial, new Shown(view, None))));
            return lease;
        }), retain).Map(unit);

    internal IO<Unit> Picked(uint serial, (double X, double Y) pixel) =>
        from swapped in PreviewRows.Swapped(views, held => held.TrySetItem(serial, static shown => shown with { Pending = None }))
        from ran in swapped.Before.Find(serial).Bind(static shown => shown.Pending).Map(run => run(pixel)).IfNone(IO.pure(unit))
        select ran;

    internal IO<Unit> Availability(uint serial, Action<bool> show, IPlugInSink sink, Action<IDisposable> retain) =>
        from heard in Subscriptions.Host<AtomChangedEvent<HashMap<uint, Shown>>, HashMap<uint, Shown>>(
                typeof(FramePicker), handler => views.Change += handler, handler => views.Change -= handler, static handler => held => handler(sender: null, held), nameof(views.Change))
            .Inline(held => IO.lift(() => show(held.ContainsKey(serial))), sink, retain)
        from current in IO.lift(() => show(views.Value.ContainsKey(serial)))
        select unit;

    // --- [PUBLICATION]
    public static PlugInSlot<FramePicker> Current { get; } = new();

    public Func<IPlugInViews, Action<IDisposable>, IO<Unit>> Published => (owner, _) => Current.Publish(owner, this);

    private sealed record Shown(FrameView View, Option<Func<(double X, double Y), IO<Unit>>> Pending);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class PreviewRows {
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
        DisposalOps.Replace(
            keep => from grid in RowGrid.Realize(children, children, scope, keep)
                    from linked in attached.Traverse(attach => attach(grid, keep)).As()
                    select grid,
            (grid, release) =>
                from published in DisposalOps.Published(owned, (Control?)grid, release, () => panel.Content, content => panel.Content = content)
                from shown in writeIO(held, Some((context, grid)))
                select unit)
            .Map(static _ => unit);

    // --- [TILES]
    public static ControlRow Tile(string caption, string help, TileSource source, RowRules rules) =>
        new ControlRow.Readout(RowShape.Block, (scope, retain) => Tiled(source, scope, retain), caption, help, rules);

    private static IO<RowCells> Tiled(TileSource source, RowScope scope, Action<IDisposable> retain) =>
        from held in IO.lift(static () => Ref(TileState.Initial))
        from owned in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain)
        from view in DisposalOps.Acquire(IO.lift(static () => new ImageView()), retain)
        let site = new CallbackSite(scope.Sink, typeof(PreviewRows), nameof(Tile))
        from down in Subscriptions.Attach(
                    h => view.MouseDown += h, h => view.MouseDown -= h,
                    Callbacks.Handler<MouseEventArgs>(e => Swapped(held, state => state with { Press = Callbacks.Found(e.Buttons is MouseButtons.Primary, e.Location) }).Map(static _ => unit), site), retain)
        from move in Subscriptions.Attach(
                    h => view.MouseMove += h, h => view.MouseMove -= h,
                    Callbacks.Handler<MouseEventArgs>(e => Dragged(view, source, held, e), site), retain)
        from up in Subscriptions.Attach(
                    h => view.MouseUp += h, h => view.MouseUp -= h,
                    Callbacks.Handler<MouseEventArgs>(_ => Swapped(held, static state => state with { Press = None }).Map(static _ => unit), site), retain)
        from clock in FrameClocks.Sample(view, Stepped(view, source, held, owned), site, retain)
        select new RowCells(view, None, None, None, [], [], new RowHelp(None, None), Restaged(view, source, held, owned), None, None);

    private static IO<Unit> Stepped(ImageView view, TileSource source, Ref<TileState> held, Retained<IDisposable> owned) =>
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
            .Filter(next => !state.Shown.Exists(prior => ReferenceEquals(prior.Tile, next.Tile) && prior.Scale == next.Scale))
            .Match(Some: next => Showing(view, source, held, owned, next.Tile, next.Scale), None: static () => IO.pure(unit))
        select shown;

    private static IO<Option<(float Scale, PixelFrame Tile)>> Failed(Ref<TileState> held, Error error) =>
        from swapped in Swapped(held, state => state with { Read = new TileRead.Failed(error) })
        from raised in when(swapped.Before.Read != swapped.After.Read, IO.fail<Unit>(error)).As()
        select Option<(float Scale, PixelFrame Tile)>.None;

    private static IO<Unit> Restaged(ImageView view, TileSource source, Ref<TileState> held, Retained<IDisposable> owned) =>
        from state in held.ValueIO
        from scale in IO.lift(() => Optional(view.ParentWindow).Map(static window => window.LogicalPixelSize))
        from shown in (from prior in state.Shown from at in scale select Showing(view, source, held, owned, prior.Tile, at)).IfNone(IO.pure(unit))
        select shown;

    private static IO<Unit> Showing(
        ImageView view, TileSource source, Ref<TileState> held, Retained<IDisposable> owned, PixelFrame tile, float scale) =>
        Staged(view, source, tile, scale, (staged, release) =>
            from published in DisposalOps.Published(owned, (Image?)staged.Icon, release, () => view.Image, image => view.Image = image)
            from shown in Swapped(held, state => state with { Shown = Some(staged), Read = new TileRead.Formed() })
            select unit)
            .Map(static _ => unit);

    private static IO<(PixelFrame Tile, float Scale, Bitmap Staged, Icon Icon)> Staged(
        ImageView view, TileSource source, PixelFrame tile, float scale,
        Func<(PixelFrame Tile, float Scale, Bitmap Staged, Icon Icon), IDisposable, IO<Unit>> publish) =>
        DisposalOps.Replace(acquired => DisposalOps.Within((Action<IDisposable> temporary) =>
            from raster in FrameView.Raster(view, tile, source.Encoding, temporary)
            from staged in DisposalOps.Acquire(IO.lift(() => new Bitmap(tile.Size.Width, tile.Size.Height, PixelFormat.Format32bppRgba)), acquired)
            from drawn in DisposalOps.Within(() => new Graphics(staged), graphics => Drawn(graphics, tile, raster))
            from icon in DisposalOps.Acquire(IO.lift(() => new Icon(scale, staged)), acquired)
            select (Tile: tile, Scale: scale, Staged: staged, Icon: icon), DisposalOps.Release),
            publish);

    private static IO<Unit> Drawn(Graphics graphics, PixelFrame tile, Bitmap raster) =>
        from canvas in PlotCanvas.Of(graphics, 1f, EtoFonts.SmallFont, enabled: true, focused: false)
        let bounds = new RectangleF(0f, 0f, tile.Size.Width, tile.Size.Height)
        from board in Thumbnails.Checker(canvas, bounds, Checkerboard.Content)
        from drawn in Plots.Raster(canvas, bounds, raster)
        select drawn;

    private static IO<Unit> Dragged(ImageView view, TileSource source, Ref<TileState> held, MouseEventArgs e) =>
        from swapped in Swapped(held, state => state with {
            Press = state.Press.Filter(press => e.Buttons is not MouseButtons.Primary || PointF.Distance(press, e.Location) <= ComponentControl.DragThreshold),
        })
        let icon = swapped.Before.Shown.Filter(_ => swapped.Before.Press.IsSome && swapped.After.Press.IsNone).Map(static shown => shown.Icon)
        from file in icon.Traverse(_ => source.File).As()
        from dragged in (from shown in icon from found in file from path in found select Rhino.UI.Rows.Transfer.Drag(view, Seq(path), Some<Image>(shown))).IfNone(IO.pure(unit))
        select dragged;

    private sealed record TileState(Option<(PixelFrame Tile, float Scale, Bitmap Staged, Icon Icon)> Shown, TileRead Read, Option<PointF> Press) {
        public static TileState Initial { get; } = new(None, new TileRead.Formed(), None);
    }

    [Union]
    private abstract partial record TileRead {
        public sealed record Formed : TileRead;

        public sealed record Failed(Error Error) : TileRead;
    }

    // --- [FRAMES]
    public static ControlRow Frame(string caption, string help, FramePane pane, RowRules rules) =>
        new ControlRow.Readout(RowShape.Filled, (scope, retain) => Framed(pane, scope, retain), caption, help, rules);

    public static Func<FrameView, RowScope, CallbackSite, Func<Option<(FrameOverlay Case, FrameEdit Value)>, IO<Unit>>, IO<bool>, Action<IDisposable>, IO<IO<Unit>>> Area<TRecord>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, FrameRegion> lens, Option<PixelExtent> aspect, Func<FrameRegion, string> readout, Func<TRecord, bool> shown)
        where TRecord : notnull =>
        (view, scope, site, show, current, retain) => Bound(
            source, field, lens, shown, new FrameOverlay.AreaPick(aspect, readout), static area => new FrameEdit.AreaEdited(area),
            static edit => edit.Switch(areaEdited: static held => Some(held.Area), quadEdited: static _ => Option<FrameRegion>.None, pixelPicked: static _ => Option<FrameRegion>.None),
            view, scope, site, show, current, retain);

    public static Func<FrameView, RowScope, CallbackSite, Func<Option<(FrameOverlay Case, FrameEdit Value)>, IO<Unit>>, IO<bool>, Action<IDisposable>, IO<IO<Unit>>> Quad<TRecord>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, FrameQuad> lens, Func<FrameQuad, Seq<QuadPatch>> patches, Gamut working, OverlayOpacity opacity, Func<TRecord, bool> shown)
        where TRecord : notnull =>
        (view, scope, site, show, current, retain) => Bound(
            source, field, lens, shown, new FrameOverlay.QuadCage(patches, working, opacity), static quad => new FrameEdit.QuadEdited(quad),
            static edit => edit.Switch(areaEdited: static _ => Option<FrameQuad>.None, quadEdited: static held => Some(held.Quad), pixelPicked: static _ => Option<FrameQuad>.None),
            view, scope, site, show, current, retain);

    private static IO<IO<Unit>> Bound<TRecord, TValue>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, Func<TRecord, bool> shown, FrameOverlay @case, Func<TValue, FrameEdit> value,
        Func<FrameEdit, Option<TValue>> take, FrameView view, RowScope scope, CallbackSite site, Func<Option<(FrameOverlay Case, FrameEdit Value)>, IO<Unit>> show, IO<bool> current, Action<IDisposable> retain)
        where TRecord : notnull =>
        RowEdit.Bind(
                source, IterableNE.create(field),
                handler => view.Edited += handler, handler => view.Edited -= handler,
                edit => Callbacks.Handler<EditEventArgs<FrameEdit>>(
                    args =>
                        from active in current
                        from taken in when(active && args.Edit.Carried.ForAll(held => take(held).IsSome), edit.Take(args.Edit, Into(take, lens)).Map(unit)).As()
                        select taken,
                    site),
                held => show(held.Filter(shown).Map(record => (@case, value(lens.Get(record))))),
                scope, site, retain)
            .Map(static bound => bound.Shown);

    private static Func<FrameEdit, TRecord, Fin<TRecord>> Into<TRecord, TValue>(Func<FrameEdit, Option<TValue>> take, Lens<TRecord, TValue> lens) =>
        (held, record) => take(held).Map(focus => lens.Set(focus, record)).IfNone(record);

    private static IO<RowCells> Framed(FramePane pane, RowScope scope, Action<IDisposable> retain) =>
        from sampling in DisposalOps.Acquire(IO.lift(() => {
            FrameSampling value = pane.Sample(scope);
            return (Value: value, Stop: new Disposal<FrameSampling>(value, static held => held.Stop.Run()));
        }), static held => IO.lift(held.Stop.Dispose), held => retain(held.Stop))
        from linked in IO.pure(sampling.Value with {
            Read = (pane.Link.Match(Some: static link => link.Masks.ValueIO, None: static () => IO.pure(Seq<FrameMask>())), sampling.Value.Read)
                .Apply(static (masks, own) => (own.Current, own.Reference, Masks: masks + own.Masks)).As(),
        })
        from initial in linked.Read
        from view in DisposalOps.Acquire(IO.lift(() => new FrameView(scope.Sink, Wording.English.Shown(pane.Empty, scope.Sink), initial, linked)), retain)
        from readout in DisposalOps.Acquire(IO.lift(static () => new Label { Font = HostTheme.Digits(EtoFonts.SmallFont), TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center }), retain)
        from cells in atomIO(toList(pane.Targets.Map(static _ => Option<(FrameOverlay Case, FrameEdit Value)>.None)))
        from ticks in IO.lift(static () => Ref(PaneTick.Initial))
        let site = new CallbackSite(scope.Sink, typeof(PreviewRows), nameof(Frame))
        from tools in pane.Tools.TraverseM(tool =>
            from realized in tool.Realize(scope, retain)
            from toggle in ToggleRows.Command(realized.Commands, tool.Face.Glyph, IconSlot.PanelButton, scope.Sink, retain)
            select (Toggle: toggle, realized.Refresh)).As()
        from compare in ButtonRows.Split(view.Compare, view.Modes.Map(static mode => (mode.Command, (IGlyph)mode.Mode.Glyph)), IconSlot.PanelButton, scope.Sink, retain)
        from swap in ButtonRows.Image(view.Swap, GlyphRole.Swap, IconSlot.PanelButton, scope.Sink, retain)
        from actions in Seq((Command: view.Fit, Glyph: GlyphRole.Fit), (Command: view.ActualSize, Glyph: GlyphRole.ActualSize))
            .TraverseM(action => ButtonRows.Image(action.Command, action.Glyph, IconSlot.PanelButton, scope.Sink, retain)).As()
        from targets in pane.Targets.Map((target, index) => Targeted(target, index, view, scope, site, cells, retain)).TraverseM(static target => target).As()
        from picking in scope.Document.ToSeq().TraverseM(document => Picking(pane.Picker, document.RuntimeSerialNumber, view, site, retain)).As()
        from clock in FrameClocks.Sample(view, Ticked(view, readout, pane.Link, cells, ticks), site, retain)
        from unloading in Subscriptions.Attach(
            handler => view.UnLoad += handler, handler => view.UnLoad -= handler,
            Callbacks.Handler<EventArgs>(_ => Callbacks.Each(
                IO.lift(sampling.Stop.Dispose).Cons(pane.Link.ToSeq().Map(link =>
                    writeIO(link.Pointer, None).Bind(_ => atomicIO(() => ticks.Value = PaneTick.Initial)).Map(unit))))
                .Map(unit), site), retain)
        from leading in ButtonRows.Strip([[.. tools.Map(static tool => tool.Toggle)], [compare, swap]], retain)
        from trailing in ButtonRows.Strip([[.. actions]], retain)
        from spacing in IO.lift(static () => RhinoLayout.Spacing(RhinoLayout.SpacingType.Table))
        from heading in Containers.Frame(() => new TableLayout(
                new TableRow(new TableCell(leading), new TableCell { ScaleWidth = true }, new TableCell(readout), new TableCell(trailing))) {
            Spacing = spacing,
            Padding = 0,
        }, retain)
        from body in Containers.Frame(() => new StackLayout(
                new StackLayoutItem(heading),
                new StackLayoutItem(view, expand: true)) {
            Orientation = Orientation.Vertical,
            Spacing = spacing.Height,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        }, retain)
        select new RowCells(
            body, None, None, None, [], [], new RowHelp(None, None),
            (targets + tools.Map(static tool => tool.Refresh)).TraverseM(static show => show).As().Map(unit),
            None, None);

    private static IO<IO<Unit>> Targeted(
        Func<FrameView, RowScope, CallbackSite, Func<Option<(FrameOverlay Case, FrameEdit Value)>, IO<Unit>>, IO<bool>, Action<IDisposable>, IO<IO<Unit>>> target,
        int index, FrameView view, RowScope scope, CallbackSite site, Atom<Lst<Option<(FrameOverlay Case, FrameEdit Value)>>> cells, Action<IDisposable> retain) =>
        target(
            view, scope, site,
            shown => cells.SwapIO(held => held.SetItem(index, shown)).Map(unit),
            cells.ValueIO.Map(held => toSeq(held).Map(static (shown, at) => (Shown: shown, At: at)).Find(static cell => cell.Shown.IsSome).Exists(cell => cell.At == index)), retain);

    private static IO<Unit> Picking(FramePicker picker, uint serial, FrameView view, CallbackSite site, Action<IDisposable> retain) =>
        from held in picker.Hold(serial, view, retain)
        from heard in Subscriptions.Attach(
                h => view.Edited += h, h => view.Edited -= h,
                Callbacks.Handler<EditEventArgs<FrameEdit>>(args => PickedPixel(args.Edit).Map(pixel => picker.Picked(serial, pixel)).IfNone(IO.pure(unit)), site), retain)
        select heard;

    private static Option<(double X, double Y)> PickedPixel(Edit<FrameEdit> edit) =>
        edit.Switch(
            preview: static _ => None,
            step: static _ => None,
            commit: static commit => commit.Value.Switch(
                areaEdited: static _ => None, quadEdited: static _ => None, pixelPicked: static picked => Some(picked.Pixel)),
            cancel: static _ => None);

    private static IO<Unit> Ticked(
        FrameView view, Label readout, Option<FrameLink> link, Atom<Lst<Option<(FrameOverlay Case, FrameEdit Value)>>> cells,
        Ref<PaneTick> ticks) =>
        from hovered in view.Hovered
        from zoom in view.Zoom
        from shown in cells.ValueIO
        from tick in Swapped(ticks, _ => new PaneTick(
            hovered,
            zoom,
            toSeq(shown).Somes().Head))
        from pointed in when(
            tick.After.Hovered != tick.Before.Hovered,
            link.Match(Some: held => held.Pointer.SwapIO(_ => tick.After.Hovered).Map(unit), None: static () => IO.pure(unit))).As()
        from zoomed in when(
            tick.After.Zoom != tick.Before.Zoom,
            IO.lift(() => readout.Text = tick.After.Zoom.Match(Some: static held => ((float)held).ToString("P0", RowText.Culture), None: static () => "")).Map(unit)).As()
        from overlaid in when(
            tick.After.Overlay != tick.Before.Overlay,
            view.Overlay(tick.After.Overlay.Map(static held => held.Case))
                .Bind(_ => tick.After.Overlay.Map(held => view.Receive(Some(held.Value))).IfNone(IO.pure(unit)))).As()
        select unit;

    private sealed record PaneTick(Option<(double X, double Y)> Hovered, Option<FrameZoom> Zoom, Option<(FrameOverlay Case, FrameEdit Value)> Overlay) {
        public static PaneTick Initial { get; } = new(None, None, None);
    }

    // --- [PICKS]
    public static IO<Button> Pick(StageInput stage, string caption, Func<System.Numerics.Vector4, IO<Unit>> picked, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from command in IO.lift(static () => new Command())
        from faced in IO.lift(() => { command.ToolTip = RowText.Localize(caption).Local; })
        from shown in (from picker in stage.Pick from document in scope.Document select picker.Availability(document.RuntimeSerialNumber, available => command.Enabled = available, scope.Sink, owned))
            .IfNone(IO.lift(() => { command.Enabled = false; }))
        let site = new CallbackSite(scope.Sink, typeof(Command), nameof(Command.Executed))
        from executed in stage.Pick.Traverse(picker => Subscriptions.Attach(
            h => command.Executed += h, h => command.Executed -= h,
            Callbacks.Handler<EventArgs>(
                _ => scope.DocumentIO
                    .Bind(document => picker.Start(document, pixel =>
                        from square in picker.Sample(scope)
                        from frame in stage.Read
                        from done in frame.Bind(held => square.Mean(held, pixel)).Match(Some: picked, None: static () => IO.pure(unit))
                        select done, owned)), site), owned)).As()
        from button in ButtonRows.Pick(command, GlyphRole.Pick, scope.Sink, owned)
        select button, retain);

    // --- [VIEWPORTS]
    public static ControlRow LiveViewport(string caption, string help, Option<Guid> mode, Option<CameraPose> pose, RowRules rules) =>
        new ControlRow.Readout(RowShape.Filled, (scope, retain) => Viewed(caption, mode, pose, scope, retain), caption, help, rules);

    private static IO<RowCells> Viewed(string caption, Option<Guid> mode, Option<CameraPose> pose, RowScope scope, Action<IDisposable> retain) =>
        from control in DisposalOps.Acquire(IO.lift(() => new ViewportControl(Wording.English.Shown(caption, scope.Sink))), retain)
        from moded in mode.Traverse(id => DisposalOps.Found(() => DisplayModeDescription.GetDisplayMode(id), nameof(DisplayModeDescription.GetDisplayMode),
            display => IO.lift(() => { control.Viewport.DisplayMode = display; }))).As()
        from posed in pose.Traverse(held => IO.lift(() => Cameras.WritePose(control.Viewport, held))).As()
        from refreshed in IO.lift(control.Refresh)
        select new RowCells(control, None, None, None, [], [], new RowHelp(None, None), IO.lift(control.Refresh), None, None);

    // --- [TRANSITIONS]
    internal static IO<(T Before, T After)> Swapped<T>(Ref<T> held, Func<T, T> change) =>
        atomicIO(() => (Before: held.Value, After: held.Value = change(held.Value)));
}
