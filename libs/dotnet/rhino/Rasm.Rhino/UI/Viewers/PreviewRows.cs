using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
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
public sealed record StageInput(IO<Option<PixelFrame>> Read, PassContext Context, Option<FramePicker> Pick);

public sealed record TileSource(IO<Option<PixelExtent>> Frame, Func<PixelExtent, IO<Option<PixelFrame>>> Read, Imaging.ColorManagement.Transfer Encoding, IO<Option<string>> File);

public sealed record FramePane(
    Func<RowScope, FrameSampling> Sample,
    Seq<Func<FrameView, RowScope, CallbackSite, Func<Option<(FrameOverlay Case, FrameEdit Value)>, IO<Unit>>, IO<bool>, Action<IDisposable>, IO<IO<Unit>>>> Targets,
    Seq<ControlRow> Tools,
    FramePicker Picker,
    Option<FrameLink> Link);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class FramePicker(Option<RowSource<ScopeState>> scope) {
    private readonly Atom<(HashMap<uint, Shown> Before, HashMap<uint, Shown> After)> views = Atom((Before: HashMap<uint, Shown>(), After: HashMap<uint, Shown>()));

    public IO<SampleSquare> Sample(RowScope rows) =>
        scope.Match(Some: source => rows.Read(source).Map(static state => state.Sample), None: static () => IO.pure(ScopeState.Default.Sample));

    public IO<Unit> Start(RhinoDoc document, PickKind kind, Func<(double X, double Y), IO<Unit>> picked, Action<IDisposable> retain) =>
        from lease in DisposalOps.Acquire(
            IO.lift(() => new Disposal<Func<(double X, double Y), IO<Unit>>>(picked, released =>
                _ = views.Swap(held => (held.After, held.After.TrySetItem(document.RuntimeSerialNumber,
                    shown => shown with { Pending = shown.Pending.Filter(next => !ReferenceEquals(next, released)) }))))),
            static lease => IO.lift(lease.Dispose), retain)
        from started in DisposalOps.OnFailure(
            from swapped in PreviewRows.Swapped(views, held => held.TrySetItem(document.RuntimeSerialNumber, shown => shown with { Pending = Some(picked) }))
            from shown in IO.lift(swapped.After.Find(document.RuntimeSerialNumber).ToFin(new Missing(nameof(Start))))
            from pickedView in shown.View.Pick(kind)
            select pickedView,
            IO.lift(lease.Dispose))
        select started;

    internal IO<Unit> Hold(uint serial, FrameView view, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => {
            IDisposable lease = new Disposal<(uint Serial, FrameView View)>((serial, view), released =>
                _ = views.Swap(held => (held.After, held.After.Find(released.Serial).Exists(shown => ReferenceEquals(shown.View, released.View))
                    ? held.After.Remove(released.Serial)
                    : held.After)));
            _ = views.Swap(held => (held.After, held.After.AddOrUpdate(serial, new Shown(view, None))));
            return lease;
        }), static lease => IO.lift(lease.Dispose), retain).Map(static _ => unit);

    internal IO<Unit> Picked(uint serial, (double X, double Y) pixel) =>
        from swapped in PreviewRows.Swapped(views, held => held.TrySetItem(serial, static shown => shown with { Pending = None }))
        from ran in swapped.Before.Find(serial).Bind(static shown => shown.Pending).Map(run => run(pixel)).IfNone(IO.pure(unit))
        select ran;

    private sealed record Shown(FrameView View, Option<Func<(double X, double Y), IO<Unit>>> Pending);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class PreviewRows {
    // --- [CONTEXT]
    public static ControlRow Contextual<TRecord>(
        RowSource<TRecord> source, string caption, string help, IO<Option<StageInput>> read, Seq<HostEvent<Unit>> changes,
        Func<Option<StageInput>, Seq<ControlRow>> rows, RowRules rules,
        Option<Func<RowGrid, Action<IDisposable>, IO<Unit>>> attached = default) where TRecord : notnull {
        Seq<EntryKey> keys = source.Fields.Bind(field => source.Keys(field.Parameter));
        return new ControlRow.Field(source, keys, RowShape.Nested,
            (scope, retain) => Contextualized(read, changes, rows, keys, attached, scope, retain), caption, help, rules);
    }

    private static IO<RowCells> Contextualized(
        IO<Option<StageInput>> read, Seq<HostEvent<Unit>> changes, Func<Option<StageInput>, Seq<ControlRow>> rows, Seq<EntryKey> keys,
        Option<Func<RowGrid, Action<IDisposable>, IO<Unit>>> attached, RowScope scope, Action<IDisposable> retain) =>
        from held in IO.lift(static () => Ref((
            Shown: Option<(Option<(PassContext Context, Option<FramePicker> Pick)> Input, RowGrid Grid)>.None,
            Releases: Seq<IDisposable>())))
        from panel in DisposalOps.Acquire(IO.lift(() => new Panel { Tag = held }), value =>
            IO.lift(() => {
                held.Value = (None, held.Value.Releases);
                value.Content = null;
                value.Dispose();
            }).Bind(_ => atomicIO(() => {
                Seq<IDisposable> prior = held.Value.Releases;
                held.Value = (None, []);
                return prior;
            })).Bind(DisposalOps.Release), retain)
        let refresh =
            from input in read
            let context = input.Map(static value => (value.Context, value.Pick))
            from prior in held.ValueIO
            from editing in prior.Shown.Traverse(static shown => shown.Grid.Editing).As()
            from replaced in editing.Contains(true) || prior.Shown.Exists(shown => shown.Input == context)
                ? prior.Shown.Traverse(shown => shown.Grid.Refresh(keys)).As().Map(static _ => unit)
                : IO.lift(() => Children(read, rows, input)).Bind(children => Replaced(held, panel, prior, context, children, attached, scope))
            select replaced
        from post in Subscriptions.Idle<Unit, Unit>(_ => refresh)(new CallbackSite(scope.Sink, typeof(PreviewRows), nameof(Contextual)), retain)
        from subscribed in changes.TraverseM(change => change.Inline(raised => post((raised, raised)), scope.Sink, retain)).As()
        from heard in scope.Listen(_ => post((unit, unit)), retain)
        from initial in refresh
        select new RowCells(panel, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None) {
            NestedEditing = held.ValueIO.Bind(static current => current.Shown.Traverse(static shown => shown.Grid.Editing).As()).Map(static editing => editing.IfNone(noneValue: false)),
        };

    private static Seq<ControlRow> Children(IO<Option<StageInput>> read, Func<Option<StageInput>, Seq<ControlRow>> rows, Option<StageInput> input) =>
        rows(input.Map(value => value with {
            Read = read.Bind(current => current.Filter(next => next.Context == value.Context && next.Pick == value.Pick)
                .Traverse(static next => next.Read).As()).Map(static frame => frame.Bind(identity)),
        }));

    private static IO<Unit> Replaced(
        Ref<(Option<(Option<(PassContext Context, Option<FramePicker> Pick)> Input, RowGrid Grid)> Shown, Seq<IDisposable> Releases)> held, Panel panel,
        (Option<(Option<(PassContext Context, Option<FramePicker> Pick)> Input, RowGrid Grid)> Shown, Seq<IDisposable> Releases) prior,
        Option<(PassContext Context, Option<FramePicker> Pick)> context, Seq<ControlRow> children, Option<Func<RowGrid, Action<IDisposable>, IO<Unit>>> attached, RowScope scope) =>
        DisposalOps.Replace(
            keep => from grid in RowGrid.Realize(children, children, scope, keep)
                    from linked in attached.Traverse(attach => attach(grid, keep)).As()
                    select grid,
            (grid, release) => {
                Control? priorContent = panel.Content;
                held.Value = (prior.Shown, prior.Releases.Add(release));
                return DisposalOps.OnFailure(
                    IO.lift(() => panel.Content = grid).Map(static _ => unit),
                    IO.lift(() => panel.Content = priorContent)
                        .Bind(_ => IO.lift(() => held.Value = prior))
                        .Bind(_ => DisposalOps.Release(Seq(release))))
                    .Bind(_ => IO.lift(() => held.Value = (Some((context, grid)), Seq(release))))
                    .Bind(_ => DisposalOps.Release(prior.Releases));
            }).Map(static _ => unit);

    // --- [TILES]
    public static ControlRow Tile(string caption, string help, TileSource source, RowRules rules) =>
        new ControlRow.Readout(RowShape.Block, (scope, retain) => Tiled(source, scope, retain), caption, help, rules);

    private static IO<RowCells> Tiled(TileSource source, RowScope scope, Action<IDisposable> retain) =>
        from held in IO.lift(static () => Atom((Before: TileState.Initial, After: TileState.Initial)))
        from view in DisposalOps.Acquire(IO.lift(() => new ImageView { Tag = held }), value =>
            IO.lift(() => { value.Image = null; value.Dispose(); })
                .Bind(_ => Swapped(held, static state => state with { Shown = None, Owned = [] }))
                .Bind(static prior => DisposalOps.Release(prior.Before.Owned)), retain)
        let site = new CallbackSite(scope.Sink, typeof(PreviewRows), nameof(Tile))
        from down in Subscriptions.Attach(
                    h => view.MouseDown += h, h => view.MouseDown -= h,
                    Callbacks.Handler<MouseEventArgs>(e => Swapped(held, state => state with { Press = Callbacks.Found(e.Buttons == MouseButtons.Primary, e.Location) }).Map(static _ => unit), site), retain)
        from move in Subscriptions.Attach(
                    h => view.MouseMove += h, h => view.MouseMove -= h,
                    Callbacks.Handler<MouseEventArgs>(e => Dragged(view, source, held, e), site), retain)
        from up in Subscriptions.Attach(
                    h => view.MouseUp += h, h => view.MouseUp -= h,
                    Callbacks.Handler<MouseEventArgs>(_ => Swapped(held, static state => state with { Press = None }).Map(static _ => unit), site), retain)
        from clock in FrameClocks.Sample(view, Stepped(scope.Sink, view, source, held), site, retain)
        select new RowCells(view, None, None, None, [], [], new RowHelp(None, None), Restaged(scope.Sink, view, source, held), None, None);

    private static IO<Unit> Stepped(IPlugInSink sink, ImageView view, TileSource source, Atom<(TileState Before, TileState After)> held) =>
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
            .Filter(next => !state.After.Shown.Exists(prior => ReferenceEquals(prior.Tile, next.Tile) && prior.Scale == next.Scale))
            .Match(Some: next => Showing(sink, view, source, held, next.Tile, next.Scale), None: static () => IO.pure(unit))
        select shown;

    private static IO<Option<(float Scale, PixelFrame Tile)>> Failed(Atom<(TileState Before, TileState After)> held, Error error) =>
        from swapped in Swapped(held, state => state with { Read = new TileRead.Failed(error) })
        from raised in when(swapped.Before.Read != swapped.After.Read, IO.fail<Unit>(error)).As()
        select Option<(float Scale, PixelFrame Tile)>.None;

    private static IO<Unit> Restaged(IPlugInSink sink, ImageView view, TileSource source, Atom<(TileState Before, TileState After)> held) =>
        from state in held.ValueIO
        from scale in IO.lift(() => Optional(view.ParentWindow).Map(static window => window.LogicalPixelSize))
        from shown in (from prior in state.After.Shown from at in scale select Showing(sink, view, source, held, prior.Tile, at)).IfNone(IO.pure(unit))
        select shown;

    private static IO<Unit> Showing(
        IPlugInSink sink, ImageView view, TileSource source, Atom<(TileState Before, TileState After)> held, PixelFrame tile, float scale) =>
        Staged(sink, view, source, tile, scale, (staged, release) => {
            Image? priorImage = view.Image;
            (TileState Before, TileState After) = held.Swap(state =>
                (state.After, state.After with { Owned = state.After.Owned.Add(release) }));
            return DisposalOps.OnFailure(
                IO.lift(() => view.Image = staged.Icon).Map(static _ => unit),
                IO.lift(() => view.Image = priorImage)
                    .Bind(_ => Swapped(held, state => state with { Owned = Before.Owned }))
                    .Bind(_ => DisposalOps.Release(Seq(release))))
                .Bind(_ => Swapped(held, state => state with {
                    Shown = Some((staged.Tile, staged.Scale, staged.Staged, staged.Icon)),
                    Read = new TileRead.Formed(),
                    Owned = Seq(release),
                }))
                .Bind(_ => DisposalOps.Release(Before.Owned));
        }).Map(static _ => unit);

    private static IO<(PixelFrame Tile, float Scale, Bitmap Staged, Icon Icon)> Staged(
        IPlugInSink sink, ImageView view, TileSource source, PixelFrame tile, float scale,
        Func<(PixelFrame Tile, float Scale, Bitmap Staged, Icon Icon), IDisposable, IO<Unit>> publish) =>
        DisposalOps.Replace(acquired => DisposalOps.Within((Action<IDisposable> temporary) =>
            from raster in FrameView.Raster(sink, view, tile, source.Encoding, temporary)
            from staged in DisposalOps.Acquire(IO.lift(() => new Bitmap(tile.Size.Width, tile.Size.Height, PixelFormat.Format32bppRgba)), static bitmap => IO.lift(bitmap.Dispose), acquired)
            from drawn in DisposalOps.Within(() => new Graphics(staged), graphics => Drawn(graphics, tile, raster), static graphics => IO.lift(graphics.Dispose))
            from icon in DisposalOps.Acquire(IO.lift(() => new Icon(scale, staged)), static icon => IO.lift(icon.Dispose), acquired)
            select (Tile: tile, Scale: scale, Staged: staged, Icon: icon), DisposalOps.Release),
            publish);

    private static IO<Unit> Drawn(Graphics graphics, PixelFrame tile, Bitmap raster) =>
        from canvas in PlotCanvas.Of(graphics, 1f, EtoFonts.SmallFont, enabled: true, focused: false)
        let bounds = new RectangleF(0f, 0f, tile.Size.Width, tile.Size.Height)
        from board in Thumbnails.Checker(canvas, bounds, Checkerboard.Content)
        from drawn in Plots.Raster(canvas, bounds, raster)
        select drawn;

    private static IO<Unit> Dragged(ImageView view, TileSource source, Atom<(TileState Before, TileState After)> held, MouseEventArgs e) =>
        from swapped in Swapped(held, state => state with {
            Press = state.Press.Filter(press => e.Buttons != MouseButtons.Primary || PointF.Distance(press, e.Location) <= ComponentControl.DragThreshold),
        })
        let icon = swapped.Before.Shown.Filter(_ => swapped.Before.Press.IsSome && swapped.After.Press.IsNone).Map(static shown => shown.Icon)
        from file in icon.Traverse(_ => source.File).As()
        from dragged in (from shown in icon from found in file from path in found select Rows.Transfer.Drag(view, Seq(path), Some<Image>(shown))).IfNone(IO.pure(unit))
        select dragged;

    private sealed record TileState(Option<(PixelFrame Tile, float Scale, Bitmap Staged, Icon Icon)> Shown, TileRead Read, Option<PointF> Press, Seq<IDisposable> Owned) {
        public static TileState Initial { get; } = new(None, new TileRead.Formed(), None, []);
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
                        from taken in when(active && Owned(args.Edit, take), edit.Take(args.Edit, Into(take, lens)).Map(static _ => unit)).As()
                        select taken,
                    site),
                held => show(held.Filter(shown).Map(record => (@case, value(lens.Get(record))))),
                scope, site, retain)
            .Map(static bound => bound.Shown);

    private static bool Owned<TValue>(Edit<FrameEdit> edit, Func<FrameEdit, Option<TValue>> take) =>
        edit.Switch(
            take,
            preview: static (of, held) => of(held.Value).IsSome,
            step: static (of, held) => of(held.Value).IsSome,
            commit: static (of, held) => of(held.Value).IsSome,
            cancel: static (_, _) => true);

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
        from view in DisposalOps.Acquire(IO.lift(() => new FrameView(scope.Sink, initial, linked)), static view => IO.lift(view.Dispose), retain)
        from readout in DisposalOps.Acquire(IO.lift(static () => new Label { Font = HostTheme.Digits(EtoFonts.SmallFont), TextAlignment = TextAlignment.Right }), static label => IO.lift(label.Dispose), retain)
        from cells in IO.lift(() => Atom(toList(pane.Targets.Map(static _ => Option<(FrameOverlay Case, FrameEdit Value)>.None))))
        from ticks in IO.lift(static () => Atom((Before: PaneTick.Initial, After: PaneTick.Initial)))
        let site = new CallbackSite(scope.Sink, typeof(PreviewRows), nameof(Frame))
        from tools in pane.Tools.Choose(row => Realized(row, scope, retain)).TraverseM(static row => row).As()
        from compare in ButtonRows.Split(view.Compare, view.Modes.Map(static mode => (mode.Command, ((IGlyph)mode.Mode.Glyph, IconSlot.PanelButton.Master))), IconSlot.PanelButton, scope.Sink, retain)
        from swap in ButtonRows.Image(view.Swap, (GlyphRole.Swap, IconSlot.PanelButton.Master), IconSlot.PanelButton, scope.Sink, retain)
        from actions in Seq((Command: view.Fit, Glyph: GlyphRole.Fit), (Command: view.ActualSize, Glyph: GlyphRole.ActualSize))
            .TraverseM(action => ButtonRows.Image(action.Command, (action.Glyph, IconSlot.PanelButton.Master), IconSlot.PanelButton, scope.Sink, retain)).As()
        from targets in pane.Targets.Map((target, index) => Targeted(target, index, view, scope, site, cells, retain)).TraverseM(static target => target).As()
        from picking in scope.Document.ToSeq().TraverseM(document => Picking(pane.Picker, document.RuntimeSerialNumber, view, site, retain)).As()
        from clock in FrameClocks.Sample(view, Ticked(view, readout, pane.Link, cells, ticks), site, retain)
        from unloading in Subscriptions.Attach(
            handler => view.UnLoad += handler, handler => view.UnLoad -= handler,
            Callbacks.Handler<EventArgs>(_ => Callbacks.Each(
                IO.lift(sampling.Stop.Dispose).Cons(pane.Link.ToSeq().Map(link =>
                    link.Pointer.SwapIO(static _ => None).Bind(_ => ticks.SwapIO(static _ => (PaneTick.Initial, PaneTick.Initial))).Map(static _ => unit))))
                .Map(static _ => unit), site), retain)
        from leading in ButtonRows.Strip(tools.Map(static cell => cell.Value), Seq<Control>(compare, swap), None, retain)
        from trailing in ButtonRows.Strip(actions.Map(static button => (Control)button), [], None, retain)
        from spacing in IO.lift(static () => RhinoLayout.Spacing(RhinoLayout.SpacingType.Table))
        from heading in DisposalOps.Acquire(IO.lift(() => new TableLayout(
                new TableRow(new TableCell(leading), new TableCell { ScaleWidth = true }, new TableCell(readout), new TableCell(trailing))) {
            Spacing = spacing,
            Padding = 0,
        }), static heading => IO.lift(heading.Dispose), retain)
        from body in DisposalOps.Acquire(IO.lift(() => new StackLayout(
                new StackLayoutItem(heading),
                new StackLayoutItem(view, expand: true)) {
            Orientation = Orientation.Vertical,
            Spacing = spacing.Height,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        }), static body => IO.lift(body.Dispose), retain)
        select new RowCells(
            body, None, None, None, [], [], new RowHelp(None, None),
            (targets + tools.Map(static cell => cell.Show)).TraverseM(static show => show).As().Map(static _ => unit),
            None, None);

    private static Option<IO<RowCells>> Realized(ControlRow row, RowScope scope, Action<IDisposable> retain) =>
        row.Switch<(RowScope Scope, Action<IDisposable> Retain), Option<IO<RowCells>>>(
            (scope, retain),
            field: static (held, field) => field.Realize(held.Scope, held.Retain),
            readout: static (held, readout) => readout.Realize(held.Scope, held.Retain),
            command: static (held, command) => command.Realize(held.Scope, held.Retain),
            group: static (_, _) => None);

    private static IO<IO<Unit>> Targeted(
        Func<FrameView, RowScope, CallbackSite, Func<Option<(FrameOverlay Case, FrameEdit Value)>, IO<Unit>>, IO<bool>, Action<IDisposable>, IO<IO<Unit>>> target,
        int index, FrameView view, RowScope scope, CallbackSite site, Atom<Lst<Option<(FrameOverlay Case, FrameEdit Value)>>> cells, Action<IDisposable> retain) =>
        target(
            view, scope, site,
            shown => cells.SwapIO(held => held.SetItem(index, shown)).Map(static _ => unit),
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
        Atom<(PaneTick Before, PaneTick After)> ticks) =>
        from hovered in view.Hovered
        from zoom in view.Zoom
        from shown in cells.ValueIO
        from tick in Swapped(ticks, _ => new PaneTick(
            hovered,
            zoom,
            toSeq(shown).Somes().Head))
        from pointed in when(
            tick.After.Hovered != tick.Before.Hovered,
            link.Match(Some: held => held.Pointer.SwapIO(_ => tick.After.Hovered).Map(static _ => unit), None: static () => IO.pure(unit))).As()
        from zoomed in when(
            tick.After.Zoom != tick.Before.Zoom,
            IO.lift(() => readout.Text = tick.After.Zoom.Match(Some: static held => ((float)held).ToString("P0", RowText.Culture), None: static () => "")).Map(static _ => unit)).As()
        from overlaid in when(
            tick.After.Overlay != tick.Before.Overlay,
            view.Overlay(tick.After.Overlay.Map(static held => held.Case))
                .Bind(_ => tick.After.Overlay.Map(held => view.Receive(Some(held.Value))).IfNone(IO.pure(unit)))).As()
        select unit;

    private sealed record PaneTick(Option<(double X, double Y)> Hovered, Option<FrameZoom> Zoom, Option<(FrameOverlay Case, FrameEdit Value)> Overlay) {
        public static PaneTick Initial { get; } = new(None, None, None);
    }

    // --- [VIEWPORTS]
    public static ControlRow LiveViewport(string caption, string help, Option<Guid> mode, Option<CameraPose> pose, RowRules rules) =>
        new ControlRow.Readout(RowShape.Filled, (scope, retain) => Viewed(caption, mode, pose, scope, retain), caption, help, rules);

    private static IO<RowCells> Viewed(string caption, Option<Guid> mode, Option<CameraPose> pose, RowScope scope, Action<IDisposable> retain) =>
        from control in DisposalOps.Acquire(IO.lift(() => new ViewportControl(Wording.English.Shown(caption, scope.Sink))), static control => IO.lift(control.Dispose), retain)
        from moded in mode.Traverse(id =>
            from display in IO.lift(() => Missing.Unless(DisplayModeDescription.GetDisplayMode(id), nameof(DisplayModeDescription.GetDisplayMode)))
            from set in IO.lift(() => control.Viewport.DisplayMode = display)
            select unit).As()
        from posed in pose.Traverse(held => IO.lift(() => Cameras.WritePose(control.Viewport, held))).As()
        from refreshed in IO.lift(control.Refresh)
        select new RowCells(control, None, None, None, [], [], new RowHelp(None, None), IO.lift(control.Refresh), None, None);

    // --- [TRANSITIONS]
    internal static IO<(T Before, T After)> Swapped<T>(Atom<(T Before, T After)> held, Func<T, T> change) =>
        held.SwapIO(state => (state.After, change(state.After)));
}
