using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Annotation.Styles;
using Rasm.Rhino.Display;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.Viewport;
using Rhino.Resources;
using Rhino.UI;
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

    public IO<Unit> Release => IO.lift(() => Switch(raster: static raster => raster.Pixels.Dispose(), linework: static _ => { }, hatching: static _ => { }));

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
    public static readonly ImageZoom Fit = new(new CommandFace("image-fit", "Fit") { Icon = Some(new IconResource(Assets.Rhino.Assembly, ResourceIds.Svg_ZoomExtentsSvg)) }, Keys.Application | Keys.D0,
        static (image, viewport) => image.Width > 0 && image.Height > 0
            ? float.Min((float)viewport.Width / image.Width, (float)viewport.Height / image.Height) switch { var scale => Size.Round(new SizeF(image.Width * scale, image.Height * scale)) }
            : image);

    public static readonly ImageZoom ActualSize = new(new CommandFace("image-actual", "Actual Size") { Icon = Some(new IconResource(Assets.Rhino.Assembly, ResourceIds.Svg_Zoom1To1SvgSvg_Zoom1To1Svg)) }, Keys.Application | Keys.D1,
        static (image, _) => image);

    public CommandFace Face { get; }
    public Keys Shortcut { get; }

    [UseDelegateFromConstructor]
    internal partial Size Sized(Size image, Size viewport);
}

public sealed record ImageRow(ReadoutSource<Loading<Bitmap>> Image) : IRow<ImageRow> {
    public RowShape Shape { get; init; } = RowShape.Filled;

    public static IO<RowCells> Realize(IO<ImageRow> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from held in atomIO((Image: (Loading<Bitmap>)new Loading<Bitmap>.Pending(None), Zoom: ImageZoom.Fit))
        from readout in use(static () => new Label { TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center })
        from scroll in Containers.Frame(static () => new Scrollable { Border = BorderType.None, ExpandContentWidth = false, ExpandContentHeight = false })
        from view in first.Image.Live(
            loaded =>
                from made in use(static () => new ImageView())
                from seeded in held.SwapIO(state => state with { Image = loaded })
                select made,
            (made, content) => held.SwapIO(state => state with { Image = content }).Bind(_ => Shown(made, scroll, readout, held, label)), scope)
        let shown = Shown(view, scroll, readout, held, label)
        let groups = Seq(toSeq(ImageZoom.Items).Map(zoom => Zoomed(held, shown, zoom)))
        from strip in CommandRows.Strip(groups, IconSlot.PanelButton, scope)
        from keyed in CommandRows.Keyed(groups, scope)
        from keys in Subscriptions.Inline<KeyEventArgs>(h => scroll.KeyDown += h, h => scroll.KeyDown -= h).Attach(args => CommandRows.Pressed(keyed, args))
        from sized in Subscriptions.On<EventArgs>(h => scroll.SizeChanged += h, h => scroll.SizeChanged -= h).Attach(_ => shown)
        from seated in IO.lift(() => { scroll.Content = view; })
        from heading in RowTable.Lined([new TableCell(strip.Control), new TableCell { ScaleWidth = true }, new TableCell(readout)])
        from body in Containers.Frame(() => new StackLayout(new StackLayoutItem(heading), new StackLayoutItem(scroll, expand: true)) {
            Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Table),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        })
        select new RowCells(body, shown.Bind(_ => strip.Refresh)) { Menu = groups };

    private static IO<Unit> Shown(ImageView view, Scrollable scroll, Label readout, Atom<(Loading<Bitmap> Image, ImageZoom Zoom)> held, LocalizeStringPair label) =>
        from state in held.ValueIO
        from drawn in IO.lift(() => state.Image.Held.Match(
            Some: image => {
                (view.Image, view.Size, view.ToolTip) = (image, state.Zoom.Sized(image.Size, scroll.ClientSize), null);
                readout.Text = LocalText.Of("{0}%", (int)double.Round(100d * view.Width / image.Width, MidpointRounding.AwayFromZero)).Local;
            },
            None: () => (view.Image, view.ToolTip, readout.Text) = (null, LocalText.Of("{0} unavailable", label.Local).Local, "")))
        select unit;

    private static Command<Invoke> Zoomed(Atom<(Loading<Bitmap> Image, ImageZoom Zoom)> held, IO<Unit> shown, ImageZoom zoom) =>
        new Command<Invoke>.Run(
            zoom.Face,
            Some(new RowRule(_ => held.ValueIO.Map(static state => state.Image.Held.IsSome))),
            new Invoke.Effect(_ => held.SwapIO(state => state with { Zoom = zoom }).Bind(_ => shown)) { Shortcut = Some(zoom.Shortcut) }) {
            Marked = new RowRule(_ => held.ValueIO.Map(state => state.Zoom == zoom)),
        };
}

public sealed record TileRow(ReadoutSource<Loading<Bitmap>> Image, Func<RowScope, IO<Option<string>>> File) : IRow<TileRow> {
    public RowShape Shape { get; init; } = RowShape.Block;

    public static IO<RowCells> Realize(IO<TileRow> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from view in first.Image.Live(
            static _ => use(static () => new ImageView()),
            (made, content) => IO.lift(() => {
                (made.Image, made.ToolTip) = content.Held.Match(
                    Some: static image => ((Image?)image, (string?)null),
                    None: () => (null, LocalText.Of("{0} unavailable", label.Local).Local));
            }), scope)
        from dragged in Subscriptions.On<MouseEventArgs>(h => view.MouseMove += h, h => view.MouseMove -= h).Attach(args =>
            when(args.Buttons is MouseButtons.Primary && Optional(view.Image).IsSome,
                latest.Bind(current => current.File(scope)).Bind(path => path.Traverse(held => DataTransfer.Drag(view, [held], Optional(view.Image))).As()).Map(unit)).As())
        select new RowCells(view, IO.pure(unit));
}

public sealed record PreviewRow(Thumbnail Thumbnail, Size Extent) : IRow<PreviewRow> {
    public RowShape Shape { get; init; } = RowShape.Inline;

    public static IO<RowCells> Realize(IO<PreviewRow> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from view in Surfaces.Of(first.Thumbnail, static (graphics, bounds, thumbnail, zone) => thumbnail.Paint(graphics, bounds, zone.Text.Enabled))
        from released in use(IO.pure(view), static held => held.State.Release)
        select new RowCells(view,
            from current in latest
            from prior in IO.lift(() => view.State)
            from shown in view.Show(current.Thumbnail)
            from dropped in unless(ReferenceEquals(prior, current.Thumbnail), prior.Release).As()
            from sized in IO.lift(() => { view.Size = current.Extent; })
            select unit);
}

public sealed record ViewportRow(Option<Guid> Mode, Option<CameraPose> Pose) : IRow<ViewportRow> {
    public RowShape Shape { get; init; } = RowShape.Filled;

    public static IO<RowCells> Realize(IO<ViewportRow> latest, LocalizeStringPair label, RowScope scope) =>
        from control in use(() => new ViewportControl(label.Local))
        select new RowCells(control,
            from current in latest
            from moded in current.Mode.Traverse(id => DisplayModes.With(id, display => IO.lift(() => { control.Viewport.DisplayMode = display; }))).As()
            from posed in current.Pose.Traverse(held => IO.lift(() => Cameras.Write(control.Viewport, held))).As()
            from refreshed in IO.lift(control.Refresh)
            select unit);
}
