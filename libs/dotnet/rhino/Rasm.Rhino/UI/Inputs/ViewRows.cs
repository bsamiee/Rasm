using System.Numerics;
using Eto.Forms;
using Rasm.Rhino.Annotation.Styles;
using Rasm.Rhino.Display;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.UI.Surfaces;
using Rasm.Rhino.Viewport;
using Rasm.UI;
using Rasm.UI.Components;
using Rasm.UI.Styling;
using Rhino.Resources;
using Rhino.UI.Controls;
using SkiaSharp;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Thumbnail {
    public Node Drawn(Role ink) =>
        Switch(
            ink,
            raster: static (_, raster) => new Node.Image(raster.Pixels) { Fit = ImageFit.Contain },
            linework: static (role, linework) => Plotted(linework.Extent, role,
                linework.Lines.Dashes.Map(static points => (Figure)new Figure.Polygon(points))
                + linework.Lines.CurveShapes.Filter(static points => points.Count > 1).Map(static points => (Figure)new Figure.Polyline(points))
                + linework.Lines.TextSurfaceShapes.Filter(static points => points.Count > 1).Map(static points => (Figure)new Figure.Polygon(points))),
            hatching: static (role, hatching) => Plotted(hatching.Extent, role, hatching.Lines.IsEmpty
                ? [new Figure.Region(None, None)]
                : hatching.Lines.Map(static line => (Figure)new Figure.Rule(line.From, line.To))));

    public static void Release(Thumbnail thumbnail) =>
        thumbnail.Switch(raster: static raster => raster.Pixels.Dispose(), linework: static _ => { }, hatching: static _ => { });

    private static Node Plotted(SKSize extent, Role ink, Seq<Figure> figures) =>
        AxisRange.TryCreate(0f, extent.Width, out AxisRange? across) && AxisRange.TryCreate(extent.Height, 0f, out AxisRange? down)
            ? new Node.Plot(new PlotPlane.Cartesian(new Axis.Linear(across), new Axis.Linear(down)), figures.Map(figure => (Node)new Node.Mark(figure) { Role = ink }))
            : new Node.Box();

    public sealed record Raster(SKImage Pixels) : Thumbnail;

    public sealed record Linework(LinetypePreview Lines, SKSize Extent) : Thumbnail;

    public sealed record Hatching(Seq<(Vector2 From, Vector2 To)> Lines, SKSize Extent) : Thumbnail;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ViewRows {
    // --- [IMAGES]
    public static ControlRow Image(
        ParameterText text, ReadoutSource<Content<SKImage>> image, Seq<Func<RowScope, IO<Unit>, Action<IDisposable>, IO<IO<Option<ImageTool>>>>> tools, RowRules rules) =>
        new ControlRow.Readout(RowShape.Filled, (scope, retain) => DisposalOps.Acquire(owned =>
            from held in atomIO((Image: (Content<SKImage>)new Content<SKImage>.Pending(None, None), View: Option<PlotPlane>.None))
            from tooled in atomIO(Seq<IO<Option<ImageTool>>>())
            from readout in DisposalOps.Acquire(IO.lift(static () => new Label { TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center }), owned)
            let caption = RowText.Localize(text.Caption, scope.Sink).Local
            let unexpected = RowText.Localize("{0} unavailable", scope.Sink, arguments: [caption]).Local
            from view in TextRows.Live(image,
                (first, keep) => DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), keep).Bind(made => held.SwapIO(state => state with { Image = first }).Map(_ => made)),
                (made, content) => held.SwapIO(state => state with { Image = content }).Bind(_ => Shown(made, readout, held, tooled, caption, unexpected)), scope, owned)
            let shown = Shown(view, readout, held, tooled, caption, unexpected)
            from bound in tools.TraverseM(tool => tool(scope, shown, owned)).As()
            from tied in writeIO(tooled, bound)
            let groups = Seq(toSeq(ImageOperation.Items).Map(operation => Operated(view, held, tooled, shown, operation)))
            from strip in ButtonRows.Strip(groups, IconSlot.PanelButton, scope, owned)
            from keyed in CommandRows.Keyed(groups, scope, owned)
            let site = new CallbackSite(scope.Sink, typeof(SceneView), nameof(Image))
            from keys in Subscriptions.Attach(h => view.KeyDown += h, h => view.KeyDown -= h,
                Callbacks.Synchronous<KeyEventArgs>(args => keyed(args.KeyData).Bind(handled => IO.lift(() => { args.Handled = handled; })), site with { Member = nameof(Control.KeyDown) }), owned)
            from sized in Subscriptions.Attach(h => view.SizeChanged += h, h => view.SizeChanged -= h,
                Callbacks.Handler<EventArgs>(_ => shown, site with { Member = nameof(Control.SizeChanged) }), owned)
            from heard in scope.Listen(_ => shown.Bind(_ => strip.Refresh), owned)
            from heading in RowGrid.Lined([new TableCell(strip.Control), new TableCell { ScaleWidth = true }, new TableCell(readout)], owned)
            from body in Containers.Frame(() => new StackLayout(new StackLayoutItem(heading), new StackLayoutItem(view, expand: true)) {
                Orientation = Eto.Forms.Orientation.Vertical,
                Spacing = RhinoLayout.StackedSpacing(Eto.Forms.Orientation.Vertical, RhinoLayout.SpacingType.Table),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            }, owned)
            select new RowCells(body, None, None, None, [], [], new RowHelp(None, None), shown.Bind(_ => strip.Refresh), None, Some(ChoiceRows.Context(view, groups, [strip.Refresh]))), retain),
            text, rules);

    public static Func<RowScope, IO<Unit>, Action<IDisposable>, IO<IO<Option<ImageTool>>>> Region<TRecord>(
        RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, ImageRegion> lens, Option<float> aspect = default)
        where TRecord : notnull =>
        (scope, shown, retain) => Bound(source, parameter, lens, (region, edited) => new ImageTool.Region(region, aspect, edited), scope, shown, retain);

    public static Func<RowScope, IO<Unit>, Action<IDisposable>, IO<IO<Option<ImageTool>>>> Cage<TRecord>(RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, ImageQuad> lens)
        where TRecord : notnull =>
        (scope, shown, retain) => Bound(source, parameter, lens, static (quad, edited) => new ImageTool.Cage(quad, edited), scope, shown, retain);

    private static IO<IO<Option<ImageTool>>> Bound<TRecord, TValue>(
        RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, TValue> lens, Func<TValue, Func<Edit<TValue>, IO<Unit>>, ImageTool> tool,
        RowScope scope, IO<Unit> shown, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : notnull =>
        from agreed in atomIO(Option<TRecord>.None)
        from edit in RowEdit.Bind<TRecord, Action>(source, IterableNE.create(parameter), static _ => { }, static _ => { }, static _ => static () => { },
            held => agreed.SwapIO(_ => held).Bind(_ => shown), scope, new CallbackSite(scope.Sink, typeof(ImageTool), nameof(Bound)), retain)
        select from latest in edit.Latest
               from held in agreed.ValueIO
               select (latest | held).Map(record => tool(lens.Get(record), change => edit.Take(change, (value, current) => lens.Set(value, current)).Bind(_ => shown)));

    private static IO<Option<float>> Across(SceneView view) =>
        IO.lift(() => Optional(view.ParentWindow).Map(window => view.Width * window.LogicalPixelSize));

    private static IO<Option<Rasm.UI.Components.ImageView>> Viewed(Atom<(Content<SKImage> Image, Option<PlotPlane> View)> held, Atom<Seq<IO<Option<ImageTool>>>> tooled, IO<Unit> shown) =>
        from state in held.ValueIO
        from reads in tooled.ValueIO
        from tools in reads.TraverseM(static read => read).As()
        select state.Image.Held.Map(image => new Rasm.UI.Components.ImageView(state.Image, new SKSizeI(image.Width, image.Height), PlotPlane.Track.Unit(Rasm.UI.Orientation.Horizontal),
            edit => edit.Carried.Traverse(plane => held.SwapIO(current => current with { View = Some(plane) })).As().Bind(_ => shown)) switch {
                var made => made with { View = state.View.IfNone(made.Fit), Tool = tools.Somes().Head },
            });

    private static IO<Unit> Shown(
        SceneView view, Label readout, Atom<(Content<SKImage> Image, Option<PlotPlane> View)> held, Atom<Seq<IO<Option<ImageTool>>>> tooled, string caption, string unexpected) =>
        from imaged in Viewed(held, tooled, unitIO.Bind(_ => Shown(view, readout, held, tooled, caption, unexpected)))
        from state in held.ValueIO
        from across in Across(view)
        from drawn in view.Show(imaged.Match(
            Some: shown => (shown with { Caption = Some(caption) }).Drawn,
            None: () => state.Image.Drawn(static _ => new Node.Box(), unexpected)))
        from zoomed in IO.lift(() => readout.Text = (from shown in imaged from width in across from zoom in shown.Zoom(width, RowText.Culture) select zoom).IfNone(""))
        select unit;

    private static Command<RowRule, Invoke> Operated(
        SceneView view, Atom<(Content<SKImage> Image, Option<PlotPlane> View)> held, Atom<Seq<IO<Option<ImageTool>>>> tooled, IO<Unit> shown, ImageOperation operation) =>
        (from imaged in Viewed(held, tooled, shown)
         from across in Across(view)
         select (from shown in imaged from width in across from applied in operation.Applied(shown, width) select applied)) switch {
             var applied => operation.Commanded<RowRule, Invoke>(
                     _ => new RowRule([], [], _ => applied.Map(static run => run.IsSome)),
                     _ => new Invoke.Effect(_ => applied.Bind(static run => run.IfNone(IO.pure(unit)))) {
                         Shortcut = operation.Map(fit: Some(Keys.Application | Keys.D0), actualSize: Some(Keys.Application | Keys.D1), flipHorizontal: Option<Keys>.None, flipVertical: Option<Keys>.None),
                     })
                 .Faced((face, _) => face with {
                     Glyph = operation.Map(
                         fit: Some<GlyphSource>(Icons.Embedded(Assets.Rhino.Assembly, ResourceIds.Svg_ZoomExtentsSvg)),
                         actualSize: Some<GlyphSource>(Icons.Embedded(Assets.Rhino.Assembly, ResourceIds.Svg_Zoom1To1SvgSvg_Zoom1To1Svg)),
                         flipHorizontal: Option<GlyphSource>.None,
                         flipVertical: Option<GlyphSource>.None),
                 }),
         };

    // --- [TILES]
    public static ControlRow Tile(ParameterText text, ReadoutSource<Content<SKImage>> image, Func<RowScope, IO<Option<string>>> file, RowRules rules) =>
        new ControlRow.Readout(RowShape.Block, (scope, retain) =>
            from unexpected in IO.pure(RowText.Localize("{0} unavailable", scope.Sink, arguments: [RowText.Localize(text.Caption, scope.Sink).Local]).Local)
            from view in TextRows.Live(image,
                (_, keep) => DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), keep),
                (made, content) => made.Show(Tiled(made, content, file(scope), unexpected)), scope, retain)
            select new RowCells(view, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None),
            text, rules);

    private static Node Tiled(SceneView view, Content<SKImage> content, IO<Option<string>> file, string unexpected) =>
        content.Held.Match(
            Some: image => new Part.Choice([new Key.Named(nameof(Tile))], new Selection.One(None), static _ => IO.pure(unit)) {
                Exported = Some<Func<Seq<Key>, IO<Unit>>>(_ => file.Bind(path => path.Traverse(held => DataTransfer.Drag(view, Seq(held), None)).As()).Map(unit)),
            }.Drawn(_ => new ImageTile(content).Drawn, static items => new Node.Flex(Rasm.UI.Orientation.Vertical, items) { Width = Sizing.Grow.Unit }) with {
                Aspect = Factor.Validated((float)image.Width / image.Height).ToOption(),
            },
            None: () => content.Drawn(static _ => new Node.Box(), unexpected));

    // --- [VIEWPORTS]
    public static ControlRow Viewport(ParameterText text, Option<Guid> mode, Option<CameraPose> pose, RowRules rules) =>
        new ControlRow.Readout(RowShape.Filled, (scope, retain) =>
            from control in DisposalOps.Acquire(IO.lift(() => new ViewportControl(RowText.Localize(text.Caption, scope.Sink).Local)), retain)
            from moded in mode.Traverse(id => DisplayModes.With(id, display => IO.lift(() => { control.Viewport.DisplayMode = display; }))).As()
            from posed in pose.Traverse(held => IO.lift(() => Cameras.Write(control.Viewport, held))).As()
            from refreshed in IO.lift(control.Refresh)
            select new RowCells(control, None, None, None, [], [], new RowHelp(None, None), IO.lift(control.Refresh), None, None),
            text, rules);
}
