using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Annotation.Styles;
using Rasm.Rhino.Display;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.Viewport;
using Rhino.Resources;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Thumbnail {
    public void Paint(Graphics graphics, RectangleF bounds, Color color) =>
        Switch(
            (Graphics: graphics, Bounds: bounds, Color: color),
            raster: static (at, raster) => Fitted(at.Graphics, raster.Pixels, at.Bounds),
            linework: static (at, linework) => Plotted(at.Graphics, at.Bounds, at.Color, linework.Extent,
                linework.Lines.Dashes.Map(static points => (Closed: true, Points: points))
                + linework.Lines.CurveShapes.Filter(static points => points.Count > 1).Map(static points => (Closed: false, Points: points))
                + linework.Lines.TextSurfaceShapes.Filter(static points => points.Count > 1).Map(static points => (Closed: true, Points: points))),
            hatching: static (at, hatching) => {
                if (hatching.Lines.IsEmpty) at.Graphics.FillRectangle(at.Color, at.Bounds);
                else Plotted(at.Graphics, at.Bounds, at.Color, hatching.Extent, hatching.Lines.Map(static line => (Closed: false, Points: Seq(line.From, line.To))));
            });

    public static void Release(Thumbnail thumbnail) =>
        thumbnail.Switch(raster: static raster => raster.Pixels.Dispose(), linework: static _ => { }, hatching: static _ => { });

    private static void Fitted(Graphics graphics, Image image, RectangleF bounds) {
        float scale = float.Min(bounds.Width / image.Width, bounds.Height / image.Height);
        SizeF size = new(image.Width * scale, image.Height * scale);
        graphics.DrawImage(image, new RectangleF(bounds.Center - (size / 2f), size));
    }

    private static void Plotted(Graphics graphics, RectangleF bounds, Color color, SizeF extent, Seq<(bool Closed, Seq<Vector2> Points)> shapes) {
        float scale = float.Min(bounds.Width / extent.Width, bounds.Height / extent.Height);
        PointF origin = bounds.Center - (new SizeF(extent.Width, extent.Height) * scale / 2f);
        using Pen pen = new(color);
        _ = shapes.Iter(shape => {
            PointF[] points = [.. shape.Points.Map(point => new PointF(origin.X + (point.X * scale), origin.Y + ((extent.Height - point.Y) * scale)))];
            if (shape.Closed) graphics.DrawPolygon(pen, points);
            else graphics.DrawLines(pen, points);
        });
    }

    public sealed record Raster(Bitmap Pixels) : Thumbnail;

    public sealed record Linework(LinetypePreview Lines, SizeF Extent) : Thumbnail;

    public sealed record Hatching(Seq<(Vector2 From, Vector2 To)> Lines, SizeF Extent) : Thumbnail;
}

[SmartEnum]
public sealed partial class ImageZoom {
    public static readonly ImageZoom Fit = new("image-fit", "Fit", Keys.Application | Keys.D0, new IconResource(Assets.Rhino.Assembly, ResourceIds.Svg_ZoomExtentsSvg));
    public static readonly ImageZoom ActualSize = new("image-actual", "Actual Size", Keys.Application | Keys.D1, new IconResource(Assets.Rhino.Assembly, ResourceIds.Svg_Zoom1To1SvgSvg_Zoom1To1Svg));

    public string Name { get; }
    public string Caption { get; }
    public Keys Shortcut { get; }
    public IconResource Icon { get; }

    internal Size Sized(Size image, Size viewport) =>
        this == Fit && image.Width > 0 && image.Height > 0
            ? float.Min((float)viewport.Width / image.Width, (float)viewport.Height / image.Height) switch { var scale => Size.Round(new SizeF(image.Width * scale, image.Height * scale)) }
            : image;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ViewRows {
    // --- [IMAGES]
    public static ControlRow Image(string label, ReadoutSource<Loading<Bitmap>> image) =>
        new ControlRow.Bound<ReadoutSource<Loading<Bitmap>>>(label, RowShape.Filled, image, static (latest, scope) =>
            from row in latest
            from held in atomIO((Image: (Loading<Bitmap>)new Loading<Bitmap>.Pending(None), Zoom: ImageZoom.Fit))
            from readout in use(static () => new Label { TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center })
            from scroll in Containers.Frame(static () => new Scrollable { Border = BorderType.None, ExpandContentWidth = false, ExpandContentHeight = false })
            from view in TextRows.Live(row.Row,
                first =>
                    from made in use(static () => new ImageView())
                    from seeded in held.SwapIO(state => state with { Image = first })
                    select made,
                (made, content) => held.SwapIO(state => state with { Image = content }).Bind(_ => Shown(made, scroll, readout, held, latest, scope)), scope)
            let shown = Shown(view, scroll, readout, held, latest, scope)
            let groups = Seq(toSeq(ImageZoom.Items).Map(zoom => Zoomed(held, shown, zoom)))
            from strip in ButtonRows.Strip(groups, IconSlot.PanelButton, scope)
            from keyed in CommandRows.Keyed(groups, scope)
            from keys in Subscriptions.During<KeyEventArgs>(h => scroll.KeyDown += h, h => scroll.KeyDown -= h).Inline(args => CommandRows.Pressed(keyed, args), scope.Owner)
            from sized in Subscriptions.Host<EventArgs>(h => scroll.SizeChanged += h, h => scroll.SizeChanged -= h).Inline(_ => shown, scope.Owner)
            from seated in IO.lift(() => { scroll.Content = view; })
            from heading in RowGrid.Lined([new TableCell(strip.Control), new TableCell { ScaleWidth = true }, new TableCell(readout)])
            from body in Containers.Frame(() => new StackLayout(new StackLayoutItem(heading), new StackLayoutItem(scroll, expand: true)) {
                Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Table),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            })
            select new RowCells(body, shown.Bind(_ => strip.Refresh)) { Menu = groups });

    private static IO<Unit> Shown(
        ImageView view, Scrollable scroll, Label readout, Atom<(Loading<Bitmap> Image, ImageZoom Zoom)> held, IO<ControlRow.Bound<ReadoutSource<Loading<Bitmap>>>> latest, RowScope scope) =>
        from row in latest
        from state in held.ValueIO
        from drawn in IO.lift(() => state.Image.Held.Match(
            Some: image => {
                (view.Image, view.Size, view.ToolTip) = (image, state.Zoom.Sized(image.Size, scroll.ClientSize), null);
                readout.Text = RowText.Localize("{0}%", arguments: [(int)double.Round(100d * view.Width / image.Width, MidpointRounding.AwayFromZero)]).Local;
            },
            None: () => {
                (view.Image, view.ToolTip) = (null, RowText.Localize("{0} unavailable", scope.Owner, arguments: [RowText.Localize(row.Label, scope.Owner).Local]).Local);
                readout.Text = "";
            }))
        select unit;

    private static Command<Invoke> Zoomed(Atom<(Loading<Bitmap> Image, ImageZoom Zoom)> held, IO<Unit> shown, ImageZoom zoom) =>
        new Command<Invoke>.Run(
            new CommandFace(zoom.Name, zoom.Caption) { Icon = Some(zoom.Icon) },
            Some(new RowRule(_ => held.ValueIO.Map(static state => state.Image.Held.IsSome))),
            new Invoke.Effect(_ => held.SwapIO(state => state with { Zoom = zoom }).Bind(_ => shown)) { Shortcut = Some(zoom.Shortcut) }) {
            Marked = new RowRule(_ => held.ValueIO.Map(state => state.Zoom == zoom)),
        };

    // --- [TILES]
    public static ControlRow Tile(string label, ReadoutSource<Loading<Bitmap>> image, Func<RowScope, IO<Option<string>>> file) =>
        new ControlRow.Bound<(ReadoutSource<Loading<Bitmap>> Image, Func<RowScope, IO<Option<string>>> File)>(label, RowShape.Block, (image, file), static (latest, scope) =>
            from row in latest
            from view in TextRows.Live(row.Row.Image,
                static _ => use(static () => new ImageView()),
                (made, content) => latest.Bind(current => IO.lift(() => {
                    (made.Image, made.ToolTip) = content.Held.Match(
                        Some: static image => ((Image?)image, (string?)null),
                        None: () => (null, RowText.Localize("{0} unavailable", scope.Owner, arguments: [RowText.Localize(current.Label, scope.Owner).Local]).Local));
                })), scope)
            from dragged in Subscriptions.Host<MouseEventArgs>(h => view.MouseMove += h, h => view.MouseMove -= h).Inline(args =>
                when(args.Buttons is MouseButtons.Primary && Optional(view.Image).IsSome,
                    latest.Bind(current => current.Row.File(scope)).Bind(path => path.Traverse(held => DataTransfer.Drag(view, Seq(held), Optional(view.Image))).As()).Map(unit)).As(),
                scope.Owner)
            select new RowCells(view, IO.pure(unit)));

    // --- [PREVIEWS]
    public static ControlRow Preview(string label, Thumbnail thumbnail, Size extent) =>
        new ControlRow.Bound<(Thumbnail Thumbnail, Size Extent)>(label, RowShape.Inline, (thumbnail, extent), static (latest, scope) =>
            from first in latest
            from view in use(() => new Drawable { Size = first.Row.Extent })
            from painted in Subscriptions.During<PaintEventArgs>(h => view.Paint += h, h => view.Paint -= h).Inline(args =>
                latest.Bind(current => IO.lift(() => current.Row.Thumbnail.Paint(args.Graphics, new RectangleF(PointF.Empty, view.Size), view.Enabled ? SystemColors.ControlText : SystemColors.DisabledText))),
                scope.Owner)
            select new RowCells(view, latest.Bind(current => IO.lift(() => {
                view.Size = current.Row.Extent;
                view.Invalidate();
            }))));

    // --- [VIEWPORTS]
    public static ControlRow Viewport(string label, Option<Guid> mode, Option<CameraPose> pose) =>
        new ControlRow.Bound<(Option<Guid> Mode, Option<CameraPose> Pose)>(label, RowShape.Filled, (mode, pose), static (latest, scope) =>
            from row in latest
            from control in use(() => new ViewportControl(RowText.Localize(row.Label, scope.Owner).Local))
            select new RowCells(control,
                from current in latest
                from moded in current.Row.Mode.Traverse(id => DisplayModes.With(id, display => IO.lift(() => { control.Viewport.DisplayMode = display; }))).As()
                from posed in current.Row.Pose.Traverse(held => IO.lift(() => Cameras.Write(control.Viewport, held))).As()
                from refreshed in IO.lift(control.Refresh)
                select unit));
}
