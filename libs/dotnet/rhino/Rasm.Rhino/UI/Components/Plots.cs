using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino.UI.Assets;
using Wacton.Unicolour;

namespace Rasm.Rhino.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct AxisRange {
    public float Low { get; }
    public float High { get; }

    public static AxisRange Unit { get; } = new(0f, 1f);
    public static AxisRange Radians { get; } = new(0f, MathF.Tau);

    public float Span => High - Low;

    public static AxisRange Of(CurveAxis axis) => new(float.CreateSaturating(axis.Span.Low), float.CreateSaturating(axis.Span.High));

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref float low, ref float high) =>
        validationError = low < high && float.IsFinite(high - low) ? null : new InvalidRhinoValue();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Axis {
    private Axis(AxisRange range) => Range = range;

    public AxisRange Range { get; }

    public abstract float Unit(float value);
    public abstract float Value(float fraction);

    public virtual Seq<float> Grid(float least) =>
        Plots.Nice(least)
            .Map(step => ((int)MathF.Ceiling(Range.Low / step), (int)MathF.Floor(Range.High / step)) switch {
                var (first, last) => toSeq(Prelude.Range(first, last - first + 1)).Map(multiple => multiple * step),
            })
            .IfNone(Seq<float>());

    public virtual float Snap(float value, int steps, float least) =>
        Plots.Nice(least).Map(step => Stepped(0f, step, value, steps)).IfNone(value);

    private static float Stepped(float origin, float step, float value, int steps) =>
        origin + ((MathF.Round((value - origin) / step) + steps) * step);

    public sealed record Linear(AxisRange Range) : Axis(Range) {
        public override float Unit(float value) => (value - Range.Low) / Range.Span;
        public override float Value(float fraction) => Range.Low + (fraction * Range.Span);
    }

    public sealed record Logarithmic(AxisRange Range) : Axis(Range) {
        public override float Unit(float value) => float.LogP1(value - Range.Low) / float.LogP1(Range.Span);
        public override float Value(float fraction) => Range.Low + float.ExpM1(fraction * float.LogP1(Range.Span));
    }

    public sealed record Periodic(AxisRange Range) : Axis(Range) {
        public override float Unit(float value) => (((value - Range.Low) / Range.Span % 1f) + 1f) % 1f;
        public override float Value(float fraction) => Range.Low + (fraction * Range.Span);
        public override Seq<float> Grid(float least) => toSeq(Prelude.Range(0, 6)).Map(index => Range.Low + (index * Range.Span / 6f));
        public override float Snap(float value, int steps, float least) => Stepped(Range.Low, Range.Span / 12f, value, steps);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlotPlane {
    private PlotPlane(RectangleF plot) => Plot = plot;

    public RectangleF Plot { get; }

    public abstract PointF Point(float first, float second);
    public abstract (float First, float Second) Value(PointF point);

    public sealed record Cartesian(RectangleF Plot, Axis X, Axis Y) : PlotPlane(Plot) {
        public override PointF Point(float first, float second) =>
            new(Plot.Left + (X.Unit(first) * Plot.Width), Plot.Bottom - (Y.Unit(second) * Plot.Height));

        public override (float First, float Second) Value(PointF point) =>
            (X.Value((point.X - Plot.Left) / Plot.Width), Y.Value((Plot.Bottom - point.Y) / Plot.Height));
    }

    public sealed record Polar(RectangleF Plot, Axis Angle, Axis Radius) : PlotPlane(Plot) {
        private float Half => float.Min(Plot.Width, Plot.Height) / 2f;

        public override PointF Point(float first, float second) =>
            MathF.SinCos(MathF.Tau * Angle.Unit(first)) switch {
                var (sin, cos) => Plot.Center + (new PointF(cos, -sin) * (Radius.Unit(second) * Half)),
            };

        public override (float First, float Second) Value(PointF point) =>
            (point - Plot.Center) switch {
                var offset => (Angle.Value(((MathF.Atan2(-offset.Y, offset.X) / MathF.Tau % 1f) + 1f) % 1f), Radius.Value(offset.Length / Half)),
            };
    }
}

[Union]
public abstract partial record MarkColor {
    public sealed record Themed(PaintSlot Slot) : MarkColor;
    public sealed record Fixed(Color Value) : MarkColor;
    public sealed record Trace(TraceChannel Channel) : MarkColor;
}

[SmartEnum]
public sealed partial class TraceChannel {
    public static readonly TraceChannel Luma = new(
        PaintSlot.ControlText, DashStyles.DashDot, CurveChannel.Master, Option<Lens<Vector3, float>>.None, static traces => traces.Luma);
    public static readonly TraceChannel Red = new(
        Color.FromArgb(0xFF, 0x00, 0x00), DashStyles.Solid, CurveChannel.Red,
        Some(Lens<Vector3, float>.New(static rgb => rgb.X, static x => rgb => rgb with { X = x })), static traces => traces.Red);
    public static readonly TraceChannel Green = new(
        Color.FromArgb(0x00, 0xFF, 0x00), DashStyles.Dash, CurveChannel.Green,
        Some(Lens<Vector3, float>.New(static rgb => rgb.Y, static y => rgb => rgb with { Y = y })), static traces => traces.Green);
    public static readonly TraceChannel Blue = new(
        Color.FromArgb(0x26, 0x63, 0xFF), DashStyles.Dot, CurveChannel.Blue,
        Some(Lens<Vector3, float>.New(static rgb => rgb.Z, static z => rgb => rgb with { Z = z })), static traces => traces.Blue);

    public MarkColor Paint { get; }
    public DashStyle Dash { get; }
    public CurveChannel Curve { get; }
    public Option<Lens<Vector3, float>> Lane { get; }

    [UseDelegateFromConstructor]
    public partial BinGrid Bins(Traces traces);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MarkStyle {
    private MarkStyle(MarkColor paint, float alpha) => (Paint, Alpha) = (paint, alpha);

    public MarkColor Paint { get; }
    public float Alpha { get; }

    public static MarkStyle Grid { get; } = new Stroke(PaintSlot.ControlText, 0.1f, 1f);
    public static MarkStyle Label { get; } = new Fill(PaintSlot.ControlText, 0.7f);
    public static MarkStyle Selection { get; } = new Fill(PaintSlot.Highlight, 0.39f);
    public static MarkStyle Ring { get; } = new Stroke(PaintSlot.ControlText, 0.7f, 1f);

    public sealed record Fill(MarkColor Paint, float Alpha) : MarkStyle(Paint, Alpha);
    public sealed record Stroke(MarkColor Paint, float Alpha, float Width) : MarkStyle(Paint, Alpha);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MarkShape {
    public abstract bool Closed { get; }

    public IGraphicsPath Figure() {
        IGraphicsPath path = GraphicsPath.Create();
        AddTo(path);
        return path;
    }

    public virtual MarkShape Snapped(float scale, float stroke) => this;

    public bool Hits(PointF pointer, float reach) {
        using IGraphicsPath path = Figure();
        using Pen pen = new(Colors.Transparent, 2f * reach) { LineCap = PenLineCap.Round };
        return (Closed && path.FillContains(pointer)) || path.StrokeContains(pen, pointer);
    }

    public RectangleF Region(float reach) {
        using IGraphicsPath path = Figure();
        return RectangleF.Inflate(path.Bounds, new SizeF(reach, reach));
    }

    protected abstract void AddTo(IGraphicsPath path);

    private static float Snap(float coordinate, float scale, float device) =>
        (MathF.Round((coordinate * scale) - (device / 2f)) + (device / 2f)) / scale;

    public sealed record Rule(PointF Start, PointF End) : MarkShape {
        public override bool Closed => false;

        public override MarkShape Snapped(float scale, float stroke) =>
            (Start.X == End.X, Start.Y == End.Y, MathF.Round(stroke * scale)) switch {
                (true, _, var device) => this with { Start = Start with { X = Snap(Start.X, scale, device) }, End = End with { X = Snap(End.X, scale, device) } },
                (_, true, var device) => this with { Start = Start with { Y = Snap(Start.Y, scale, device) }, End = End with { Y = Snap(End.Y, scale, device) } },
                _ => this,
            };

        protected override void AddTo(IGraphicsPath path) => path.AddLine(Start, End);
    }

    public sealed record Polyline(IReadOnlyList<PointF> Points) : MarkShape {
        public override bool Closed => false;

        protected override void AddTo(IGraphicsPath path) => path.AddLines(Points);
    }

    public sealed record Polygon(IReadOnlyList<PointF> Points) : MarkShape {
        public override bool Closed => true;

        protected override void AddTo(IGraphicsPath path) {
            path.AddLines(Points);
            path.CloseFigure();
        }
    }

    public sealed record Band(RectangleF Bounds) : MarkShape {
        public override bool Closed => true;

        public override MarkShape Snapped(float scale, float stroke) =>
            MathF.Round(stroke * scale) switch {
                var device => new Band(RectangleF.FromSides(Snap(Bounds.Left, scale, device), Snap(Bounds.Top, scale, device), Snap(Bounds.Right, scale, device), Snap(Bounds.Bottom, scale, device))),
            };

        protected override void AddTo(IGraphicsPath path) => path.AddRectangle(Bounds);
    }

    public sealed record Disc(PointF Centre, float Diameter) : MarkShape {
        public override bool Closed => true;

        protected override void AddTo(IGraphicsPath path) => path.AddEllipse(RectangleF.FromCenter(Centre, new SizeF(Diameter, Diameter)));
    }
}

public sealed record MarkPart<T>(T Key, float Reach) where T : notnull;

public sealed record PlotMark<T>(MarkShape Shape, Option<MarkStyle> Style, Option<MarkPart<T>> Part) where T : notnull;

public sealed record AxisLabel(float Value, string Text);

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class LabelSide {
    public static readonly LabelSide Bottom = new(Orientation.Horizontal, far: true);
    public static readonly LabelSide Left = new(Orientation.Vertical, far: false);
    public static readonly LabelSide Right = new(Orientation.Vertical, far: true);

    public Orientation Along { get; }
    public bool Far { get; }

    public float Position(PointF point) => Along == Orientation.Horizontal ? point.X : point.Y;
    public float Span(SizeF size) => Along == Orientation.Horizontal ? size.Width : size.Height;

    public PointF Tick(PlotPlane.Cartesian plane, float value) =>
        (plane.Point(value, value), Edge(plane.Plot)) switch {
            var (at, edge) => Along == Orientation.Horizontal ? new(at.X, edge) : new(edge, at.Y),
        };

    public RectangleF Box(RectangleF plot, PointF tick, SizeF size) =>
        Position(tick) switch {
            var place => (place - ((place - Position(plot.Location)) / Span(plot.Size) * Span(size)), Edge(plot) - (Far ? Cross(size) : 0f)) switch {
                var (along, across) => Along == Orientation.Horizontal ? new(along, across, size.Width, size.Height) : new(across, along, size.Width, size.Height),
            },
        };

    private float Cross(SizeF size) => Along == Orientation.Horizontal ? size.Height : size.Width;

    private float Edge(RectangleF plot) =>
        Along == Orientation.Horizontal ? (Far ? plot.Bottom : plot.Top) : (Far ? plot.Right : plot.Left);
}

public sealed record PlotCanvas {
    private PlotCanvas(Graphics graphics, float scale, Font font, bool enabled, bool focused, DisplayOptions accessibility, HashMap<PaintSlot, Color> slots) =>
        (Graphics, Scale, Font, Enabled, Focused, Accessibility, Slots) = (graphics, scale, font, enabled, focused, accessibility, slots);

    public Graphics Graphics { get; }
    public float Scale { get; }
    public Font Font { get; }
    public bool Enabled { get; }
    public bool Focused { get; }
    public DisplayOptions Accessibility { get; }
    public HashMap<PaintSlot, Color> Slots { get; }

    public static IO<PlotCanvas> Of(Graphics graphics, float scale, Font font, bool enabled, bool focused) =>
        IO.lift(() => new PlotCanvas(graphics, scale, font, enabled, focused, Themes.Accessibility, toHashMap(PaintSlot.Items.Select(static slot => (slot, slot.Read())))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Plots {
    // --- [PLACEMENT]
    public const float Inset = 4f;

    public static RectangleF Plot(RectangleF well) => RectangleF.Inset(well, new PaddingF(Inset));

    public static Fin<PixelExtent> Device(PlotPlane plane, float scale) => Extent(plane.Plot.Size * scale);

    public static Option<float> Nice(float least) =>
        float.IsFinite(least) && least > 0f
            ? MathF.Pow(10f, MathF.Floor(MathF.Log10(least))) switch {
                var decade => Seq(1f, 2f, 5f, 10f).Map(step => step * decade).Find(step => step >= least),
            }
            : None;

    public static Seq<MarkShape> Rules(PlotPlane.Cartesian plane, Orientation across, Seq<float> values) =>
        values.Map(MarkShape (value) => plane.Point(value, value) switch {
            var at when across == Orientation.Vertical => new MarkShape.Rule(new(at.X, plane.Plot.Top), new(at.X, plane.Plot.Bottom)),
            var at => new MarkShape.Rule(new(plane.Plot.Left, at.Y), new(plane.Plot.Right, at.Y)),
        });

    public static MarkShape Sampled(PlotPlane.Cartesian plane, float scale, Func<float, float> function) =>
        new MarkShape.Polyline([.. Columns(plane, scale, function)]);

    public static MarkShape Area(PlotPlane.Cartesian plane, float scale, Func<float, float> function) =>
        new MarkShape.Polygon([.. Columns(plane, scale, function), new(plane.Plot.Right, plane.Plot.Bottom), new(plane.Plot.Left, plane.Plot.Bottom)]);

    private static Seq<PointF> Columns(PlotPlane.Cartesian plane, float scale, Func<float, float> function) =>
        toSeq(Prelude.Range(0, (int)MathF.Ceiling(plane.Plot.Width * scale)))
            .Map(column => plane.X.Value((column + 0.5f) / (plane.Plot.Width * scale)))
            .Map(input => plane.Point(input, function(input)));

    private static Fin<PixelExtent> Extent(SizeF device) =>
        Size.Ceiling(device) switch {
            var whole => PixelExtent.Validate(whole.Width, whole.Height, out PixelExtent extent) is { } error ? error : extent,
        };

    // --- [COLOR]
    public static Color Encoded(Gamut gamut, Vector3 linear) =>
        new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, linear.X, linear.Y, linear.Z)
            .ConvertToConfiguration(Configuration.Default)
            .MapToRgbGamut()
            .Rgb switch {
                var rgb => new Color((float)rgb.R, (float)rgb.G, (float)rgb.B),
            };

    public static Vector4 HueRamp(float hue) => new Hsy(hue, 1f, Exposure.MiddleGrey).ToRgb(1f);

    // --- [PAINT]
    public static Color Resolve(PlotCanvas canvas, MarkStyle style) =>
        new(canvas.Enabled ? Ink(canvas, style.Paint) : canvas.Slots[PaintSlot.DisabledText],
            style.Switch(
                canvas,
                fill: static (_, fill) => (float?)fill.Alpha,
                stroke: static (held, stroke) => held.Accessibility.IncreaseContrast ? null : (float?)stroke.Alpha));

    public static IO<Unit> Well(PlotCanvas canvas, RectangleF well) =>
        IO.lift(() => canvas.Graphics.FillRectangle(canvas.Slots[PaintSlot.ControlBackground], well));

    public static IO<Unit> Paint<T>(PlotCanvas canvas, Seq<PlotMark<T>> marks) where T : notnull =>
        IO.lift(() => marks.Iter(mark => mark.Style.Iter(style => Draw(canvas, mark.Shape, style))));

    public static IO<Unit> Raster(PlotCanvas canvas, RectangleF target, Image image) =>
        IO.lift(() => {
            canvas.Graphics.ImageInterpolation = ImageInterpolation.None;
            canvas.Graphics.DrawImage(image, target);
        });

    public static IO<Unit> Strip(PlotCanvas canvas, PlotPlane.Cartesian plane, Func<float, Color> color) =>
        IO.lift(Extent(new SizeF(plane.Plot.Width * canvas.Scale, 1f))).Bind(extent =>
            use(() => Pixels(extent, toSeq(Prelude.Range(0, extent.Width)).Map(column => color(plane.X.Value((column + 0.5f) / extent.Width)).ToArgb())))
                .Bind(bitmap => Raster(canvas, plane.Plot, bitmap))
                .Bracket());

    public static Bitmap Pixels(PixelExtent extent, IEnumerable<int> argb) => new(extent.Width, extent.Height, PixelFormat.Format32bppRgba, argb);

    public static IO<Unit> Labels(PlotCanvas canvas, PlotPlane.Cartesian plane, LabelSide side, Seq<AxisLabel> labels) =>
        IO.lift(() => Shown(side, labels.Map(label => (Tick: side.Tick(plane, label.Value), Size: canvas.Graphics.MeasureString(canvas.Font, label.Text), label.Text)))
            .Iter(label => canvas.Graphics.DrawText(canvas.Font, Resolve(canvas, MarkStyle.Label), side.Box(plane.Plot, label.Tick, label.Size).Location, label.Text)));

    private static Color Ink(PlotCanvas canvas, MarkColor paint) =>
        paint.Switch(
            canvas,
            themed: static (held, themed) => held.Slots[themed.Slot],
            @fixed: static (_, value) => value.Value,
            trace: static (held, trace) => Ink(held, trace.Channel.Paint));

    private static void Draw(PlotCanvas canvas, MarkShape shape, MarkStyle style) =>
        style.Switch(
            (Canvas: canvas, Shape: shape),
            fill: static (held, fill) => Filled(held.Canvas, held.Shape, fill),
            stroke: static (held, stroke) => Stroked(held.Canvas, held.Shape, stroke));

    private static void Filled(PlotCanvas canvas, MarkShape shape, MarkStyle.Fill fill) {
        using IGraphicsPath path = shape.Snapped(canvas.Scale, 0f).Figure();
        canvas.Graphics.FillPath(Resolve(canvas, fill), path);
    }

    private static void Stroked(PlotCanvas canvas, MarkShape shape, MarkStyle.Stroke stroke) {
        using IGraphicsPath path = shape.Snapped(canvas.Scale, stroke.Width).Figure();
        using Pen pen = new(Resolve(canvas, stroke), stroke.Width) {
            DashStyle = stroke.Paint.Switch(
                canvas,
                themed: static (_, _) => DashStyles.Solid,
                @fixed: static (_, _) => DashStyles.Solid,
                trace: static (held, trace) => held.Accessibility.DifferentiateWithoutColor ? trace.Channel.Dash : DashStyles.Solid),
        };
        canvas.Graphics.DrawPath(pen, path);
    }

    private static Seq<(PointF Tick, SizeF Size, string Text)> Shown(LabelSide side, Seq<(PointF Tick, SizeF Size, string Text)> labels) =>
        toSeq(Prelude.Range(1, labels.Count))
            .Filter(stride => stride < labels.Count && Nice(stride).Exists(step => step == stride))
            .Map(stride => labels.Map(static (label, index) => (Label: label, Index: index)).Filter(held => held.Index % stride == 0).Map(static held => held.Label))
            .Find(kept => kept.Zip(kept.Tail).ForAll(pair =>
                MathF.Abs(side.Position(pair.Second.Tick) - side.Position(pair.First.Tick)) >= (side.Span(pair.First.Size) + side.Span(pair.Second.Size)) / 2f))
            .IfNone(() => labels.Take(1));

    // --- [HIT]
    public const float Reach = 5f;

    public static Option<T> Hit<T>(Seq<PlotMark<T>> marks, PointF pointer) where T : notnull =>
        marks.Rev().Choose(mark => mark.Part.Filter(part => mark.Shape.Hits(pointer, part.Reach)).Map(static part => part.Key)).Head;
}
