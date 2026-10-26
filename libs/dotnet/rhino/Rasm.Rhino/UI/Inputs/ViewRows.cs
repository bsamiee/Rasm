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
    public static ControlRow Image(string label, ReadoutSource<Content<SKImage>> image, Option<ImageTool> tool) =>
        new ControlRow.Bound<(ReadoutSource<Content<SKImage>> Image, Option<ImageTool> Tool)>(label, RowShape.Filled, (image, tool), static (latest, scope, retain) =>
            from row in latest
            from held in atomIO((Image: (Content<SKImage>)new Content<SKImage>.Pending(None, None), View: Option<PlotPlane>.None))
            from readout in DisposalOps.Acquire(IO.lift(static () => new Label { TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center }), retain)
            from view in TextRows.Live(row.Row.Image,
                (first, keep) => DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), keep).Bind(made => held.SwapIO(state => state with { Image = first }).Map(_ => made)),
                (made, content) => held.SwapIO(state => state with { Image = content }).Bind(_ => Shown(made, readout, held, latest, scope)), scope, retain)
            let shown = Shown(view, readout, held, latest, scope)
            let groups = Seq(toSeq(ImageOperation.Items).Map(operation => Operated(view, held, latest, shown, operation)))
            from strip in ButtonRows.Strip(groups, IconSlot.PanelButton, scope, retain)
            from keyed in CommandRows.Keyed(groups, scope, retain)
            let site = new CallbackSite(scope.Sink, typeof(SceneView), nameof(Image))
            from keys in Subscriptions.Attach(h => view.KeyDown += h, h => view.KeyDown -= h,
                Callbacks.Synchronous<KeyEventArgs>(args => keyed(args.KeyData).Bind(handled => IO.lift(() => { args.Handled = handled; })), site with { Member = nameof(Control.KeyDown) }), retain)
            from sized in Subscriptions.Attach(h => view.SizeChanged += h, h => view.SizeChanged -= h,
                Callbacks.Handler<EventArgs>(_ => shown, site with { Member = nameof(Control.SizeChanged) }), retain)
            from heading in RowGrid.Lined([new TableCell(strip.Control), new TableCell { ScaleWidth = true }, new TableCell(readout)], retain)
            from body in Containers.Frame(() => new StackLayout(new StackLayoutItem(heading), new StackLayoutItem(view, expand: true)) {
                Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Table),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            }, retain)
            select new RowCells(body, shown.Bind(_ => strip.Refresh)) { Menu = groups });

    private static IO<Option<float>> Across(SceneView view) =>
        IO.lift(() => Optional(view.ParentWindow).Map(window => view.Width * window.LogicalPixelSize));

    private static IO<Option<ImagePlot>> Viewed(
        Atom<(Content<SKImage> Image, Option<PlotPlane> View)> held, IO<ControlRow.Bound<(ReadoutSource<Content<SKImage>> Image, Option<ImageTool> Tool)>> latest, IO<Unit> shown) =>
        from state in held.ValueIO
        from row in latest
        select state.Image.Held.Map(image => new ImagePlot(state.Image, new SKSizeI(image.Width, image.Height), PlotPlane.Track.Unit(PhysicalAxis.Horizontal),
            edit => edit.Shown.Traverse(plane => held.SwapIO(current => current with { View = Some(plane) })).As().Bind(_ => shown)) switch {
                var made => made with { View = state.View.IfNone(made.Fit), Tool = row.Row.Tool },
            });

    private static IO<Unit> Shown(
        SceneView view, Label readout, Atom<(Content<SKImage> Image, Option<PlotPlane> View)> held,
        IO<ControlRow.Bound<(ReadoutSource<Content<SKImage>> Image, Option<ImageTool> Tool)>> latest, RowScope scope) =>
        from row in latest
        let caption = RowText.Localize(row.Label, scope.Sink).Local
        from imaged in Viewed(held, latest, unitIO.Bind(_ => Shown(view, readout, held, latest, scope)))
        from state in held.ValueIO
        from across in Across(view)
        from drawn in view.Show(imaged.Match(
            Some: shown => (shown with { Caption = Some(caption) }).Drawn,
            None: () => state.Image.Drawn(static _ => new Node.Box(), RowText.Localize("{0} unavailable", scope.Sink, arguments: [caption]).Local)))
        from zoomed in IO.lift(() => readout.Text = (from shown in imaged from width in across from zoom in shown.Zoom(width, RowText.Culture) select zoom).IfNone(""))
        select unit;

    private static Command<RowRule, Invoke> Operated(
        SceneView view, Atom<(Content<SKImage> Image, Option<PlotPlane> View)> held, IO<ControlRow.Bound<(ReadoutSource<Content<SKImage>> Image, Option<ImageTool> Tool)>> latest,
        IO<Unit> shown, ImageOperation operation) =>
        (from imaged in Viewed(held, latest, shown)
         from across in Across(view)
         select (from viewed in imaged from width in across from applied in operation.Applied(viewed, width) select applied)) switch {
             var applied => operation.Commanded<RowRule, Invoke>(
                     operation.Map(fit: "Fit", actualSize: "Actual Size"),
                     _ => new RowRule(_ => applied.Map(static run => run.IsSome)),
                     _ => new Invoke.Effect(_ => applied.Bind(static run => run.IfNone(IO.pure(unit)))) {
                         Shortcut = operation.Map(fit: Keys.Application | Keys.D0, actualSize: Keys.Application | Keys.D1),
                     })
                 .Faced((face, _) => face with {
                     Glyph = Some<GlyphSource>(operation.Map(
                         fit: Icons.Embedded(Assets.Rhino.Assembly, ResourceIds.Svg_ZoomExtentsSvg),
                         actualSize: Icons.Embedded(Assets.Rhino.Assembly, ResourceIds.Svg_Zoom1To1SvgSvg_Zoom1To1Svg))),
                 }),
         };

    // --- [TILES]
    public static ControlRow Tile(string label, ReadoutSource<Content<SKImage>> image, Func<RowScope, IO<Option<string>>> file) =>
        new ControlRow.Bound<(ReadoutSource<Content<SKImage>> Image, Func<RowScope, IO<Option<string>>> File)>(label, RowShape.Block, (image, file), static (latest, scope, retain) =>
            from row in latest
            from view in TextRows.Live(row.Row.Image,
                (_, keep) => DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), keep),
                (made, content) => latest.Bind(current => made.Show(Tiled(made, content, current.Row.File(scope),
                    RowText.Localize("{0} unavailable", scope.Sink, arguments: [RowText.Localize(current.Label, scope.Sink).Local]).Local))), scope, retain)
            select new RowCells(view, IO.pure(unit)));

    private static Node Tiled(SceneView view, Content<SKImage> content, IO<Option<string>> file, string unexpected) =>
        content.Held.Match(
            Some: image => new Part.Choice([new Key.Named(nameof(Tile))], new Selection.One(None), static _ => IO.pure(unit)) {
                Exported = Some<Func<Seq<Key>, IO<Unit>>>(_ => file.Bind(path => path.Traverse(held => DataTransfer.Drag(view, Seq(held), None)).As()).Map(unit)),
            }.Drawn(_ => new ImageTile(content).Drawn, static items => new Node.Flex(PhysicalAxis.Vertical, items) { Width = Sizing.Grow.Unit }) with {
                Aspect = Factor.TryCreate((float)image.Width / image.Height, out Factor? aspect) ? Some(aspect) : None,
            },
            None: () => content.Drawn(static _ => new Node.Box(), unexpected));

    // --- [VIEWPORTS]
    public static ControlRow Viewport(string label, Option<Guid> mode, Option<CameraPose> pose) =>
        new ControlRow.Bound<(Option<Guid> Mode, Option<CameraPose> Pose)>(label, RowShape.Filled, (mode, pose), static (latest, scope, retain) =>
            from row in latest
            from control in DisposalOps.Acquire(IO.lift(() => new ViewportControl(RowText.Localize(row.Label, scope.Sink).Local)), retain)
            select new RowCells(control, latest.Bind(current =>
                from moded in current.Row.Mode.Traverse(id => DisplayModes.With(id, display => IO.lift(() => { control.Viewport.DisplayMode = display; }))).As()
                from posed in current.Row.Pose.Traverse(held => IO.lift(() => Cameras.Write(control.Viewport, held))).As()
                from refreshed in IO.lift(control.Refresh)
                select unit)));
}
