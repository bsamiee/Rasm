using System.Globalization;
using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Geometry;

// --- [TYPES] ---------------------------------------------------------------------------
public enum Orientation {
    Horizontal,
    Vertical,
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public readonly partial struct AxisRange {
    public float Low { get; }
    public float High { get; }

    public static AxisRange Unit { get; } = new(0f, 1f);
    public static AxisRange Radians { get; } = new(0f, MathF.Tau);

    public float Span => High - Low;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float low, ref float high) =>
        validationError = low < high && float.IsFinite(high - low) ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"axis range [{low}, {high}] is empty or unbounded"));
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
        origin + ((MathF.Round((value - origin) / step, MidpointRounding.ToEven) + steps) * step);

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
    private PlotPlane(SKRect plot) => Plot = plot;

    public SKRect Plot { get; }

    public abstract SKPoint Point(float first, float second);
    public abstract (float First, float Second) Value(SKPoint point);

    public sealed record Cartesian(SKRect Plot, Axis X, Axis Y) : PlotPlane(Plot) {
        public static Cartesian Unit(SKRect plot) => new(plot, new Axis.Linear(AxisRange.Unit), new Axis.Linear(AxisRange.Unit));

        public override SKPoint Point(float first, float second) =>
            new(Plot.Left + (X.Unit(first) * Plot.Width), Plot.Bottom - (Y.Unit(second) * Plot.Height));

        public override (float First, float Second) Value(SKPoint point) =>
            (X.Value((point.X - Plot.Left) / Plot.Width), Y.Value((Plot.Bottom - point.Y) / Plot.Height));
    }

    public sealed record Polar(SKRect Plot, Axis Angle, Axis Radius) : PlotPlane(Plot) {
        private float Half => float.Min(Plot.Width, Plot.Height) / 2f;
        private Vector2 Center => new(Plot.MidX, Plot.MidY);

        public override SKPoint Point(float first, float second) =>
            MathF.SinCos(MathF.Tau * Angle.Unit(first)) switch {
                var (sin, cos) => Center + (new Vector2(cos, -sin) * (Radius.Unit(second) * Half)),
            };

        public override (float First, float Second) Value(SKPoint point) =>
            ((Vector2)point - Center) switch {
                var offset => (Angle.Value(((MathF.Atan2(-offset.Y, offset.X) / MathF.Tau % 1f) + 1f) % 1f), Radius.Value(offset.Length() / Half)),
            };
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MarkStyle {
    private MarkStyle(Ink ink) => Ink = ink;

    public Ink Ink { get; }

    public static MarkStyle Grid { get; } = new Stroke(new Ink.Themed(ColorRole.Grid, 1f), 1f, []);
    public static MarkStyle Label { get; } = new Fill(new Ink.Themed(ColorRole.Label, 1f));
    public static MarkStyle Selection { get; } = new Fill(new Ink.Themed(ColorRole.Selection, 1f));
    public static MarkStyle Ring { get; } = new Stroke(new Ink.Themed(ColorRole.Label, 1f), 1f, []);
    public static MarkStyle Lifted { get; } = new Stroke(new Ink.Themed(ColorRole.Text, 1f), 1f, []);

    public sealed record Fill(Ink Ink) : MarkStyle(Ink);
    public sealed record Stroke(Ink Ink, float Width, Seq<float> Dash) : MarkStyle(Ink);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MarkShape {
    public abstract bool Closed { get; }

    public SKPath Figure() {
        using SKPathBuilder builder = new();
        AddTo(builder);
        return builder.Detach();
    }

    public virtual MarkShape Snapped(float scale, float stroke) => this;

    public abstract MarkShape Mapped(SKRect frame);

    public bool Hits(SKPoint pointer, float reach) {
        using SKPath path = Figure();
        using SKPaint pen = new() { Style = SKPaintStyle.Stroke, StrokeWidth = 2f * reach, StrokeCap = SKStrokeCap.Round };
        using SKPath? outline = pen.GetFillPath(path);
        return (Closed && path.Contains(pointer.X, pointer.Y)) || Optional(outline).Exists(held => held.Contains(pointer.X, pointer.Y));
    }

    public SKRect Region(float reach) {
        using SKPath path = Figure();
        return SKRect.Inflate(path.Bounds, reach, reach);
    }

    protected abstract void AddTo(SKPathBuilder builder);

    private static float Snap(float coordinate, float scale, float device) =>
        (MathF.Round((coordinate * scale) - (device / 2f), MidpointRounding.ToEven) + (device / 2f)) / scale;

    private static SKPoint Map(SKRect frame, SKPoint fraction) => new(frame.Left + (fraction.X * frame.Width), frame.Top + (fraction.Y * frame.Height));

    public sealed record Rule(SKPoint Start, SKPoint End) : MarkShape {
        public override bool Closed => false;

        public override MarkShape Snapped(float scale, float stroke) =>
            (Start.X == End.X, Start.Y == End.Y, MathF.Round(stroke * scale, MidpointRounding.ToEven)) switch {
                (true, _, var device) => this with { Start = new(Snap(Start.X, scale, device), Start.Y), End = new(Snap(End.X, scale, device), End.Y) },
                (_, true, var device) => this with { Start = new(Start.X, Snap(Start.Y, scale, device)), End = new(End.X, Snap(End.Y, scale, device)) },
                _ => this,
            };

        public override MarkShape Mapped(SKRect frame) => new Rule(Map(frame, Start), Map(frame, End));

        protected override void AddTo(SKPathBuilder builder) {
            builder.MoveTo(Start);
            builder.LineTo(End);
        }
    }

    public sealed record Polyline(Seq<SKPoint> Points) : MarkShape {
        public override bool Closed => false;

        public override MarkShape Mapped(SKRect frame) => new Polyline(Points.Map(point => Map(frame, point)));

        protected override void AddTo(SKPathBuilder builder) => builder.AddPoly([.. Points], close: false);
    }

    public sealed record Polygon(Seq<SKPoint> Points) : MarkShape {
        public override bool Closed => true;

        public override MarkShape Mapped(SKRect frame) => new Polygon(Points.Map(point => Map(frame, point)));

        protected override void AddTo(SKPathBuilder builder) => builder.AddPoly([.. Points], close: true);
    }

    public sealed record Band(SKRect Bounds, float Radius = 0f) : MarkShape {
        public override bool Closed => true;

        public override MarkShape Snapped(float scale, float stroke) =>
            MathF.Round(stroke * scale, MidpointRounding.ToEven) switch {
                var device => this with { Bounds = new SKRect(Snap(Bounds.Left, scale, device), Snap(Bounds.Top, scale, device), Snap(Bounds.Right, scale, device), Snap(Bounds.Bottom, scale, device)) },
            };

        public override MarkShape Mapped(SKRect frame) =>
            this with { Bounds = new SKRect(frame.Left + (Bounds.Left * frame.Width), frame.Top + (Bounds.Top * frame.Height), frame.Left + (Bounds.Right * frame.Width), frame.Top + (Bounds.Bottom * frame.Height)) };

        protected override void AddTo(SKPathBuilder builder) => builder.AddRoundRect(Bounds, Radius, Radius);
    }

    public sealed record Disc(SKPoint Center, float Diameter) : MarkShape {
        public override bool Closed => true;

        public override MarkShape Mapped(SKRect frame) => new Disc(Map(frame, Center), Diameter * float.Min(frame.Width, frame.Height));

        protected override void AddTo(SKPathBuilder builder) => builder.AddCircle(Center.X, Center.Y, Diameter / 2f);
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

    public float Position(SKPoint point) => Along switch { Orientation.Horizontal => point.X, Orientation.Vertical => point.Y };
    public float Span(SKSize size) => Along switch { Orientation.Horizontal => size.Width, Orientation.Vertical => size.Height };

    public SKPoint Tick(PlotPlane.Cartesian plane, float value) =>
        (plane.Point(value, value), Edge(plane.Plot)) switch {
            var (at, edge) => Along switch { Orientation.Horizontal => new SKPoint(at.X, edge), Orientation.Vertical => new SKPoint(edge, at.Y) },
        };

    public SKRect Box(SKRect plot, SKPoint tick, SKSize size) =>
        Position(tick) switch {
            var place => (place - ((place - Position(plot.Location)) / Span(plot.Size) * Span(size)), Edge(plot) - (Far ? Span(new SKSize(size.Height, size.Width)) : 0f)) switch {
                var (along, across) => Along switch {
                    Orientation.Horizontal => SKRect.Create(along, across, size.Width, size.Height),
                    Orientation.Vertical => SKRect.Create(across, along, size.Width, size.Height),
                },
            },
        };

    private float Edge(SKRect plot) =>
        Along switch {
            Orientation.Horizontal => Far ? plot.Bottom : plot.Top,
            Orientation.Vertical => Far ? plot.Right : plot.Left,
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Plots {
    // --- [PLACEMENT]
    public const float Inset = 4f;

    public static SKRect Plot(SKRect well) => SKRect.Inflate(well, -Inset, -Inset);

    public static Option<float> Nice(float least) =>
        float.IsFinite(least) && least > 0f
            ? float.Exp10(MathF.Floor(MathF.Log10(least))) switch {
                var decade => Seq(1f, 2f, 5f, 10f).Map(step => step * decade).Find(step => step >= least),
            }
            : None;

    public static Seq<MarkShape> Rules(PlotPlane.Cartesian plane, Orientation across, Seq<float> values) =>
        values.Map(MarkShape (value) => plane.Point(value, value) switch {
            var at when across is Orientation.Vertical => new MarkShape.Rule(new(at.X, plane.Plot.Top), new(at.X, plane.Plot.Bottom)),
            var at => new MarkShape.Rule(new(plane.Plot.Left, at.Y), new(plane.Plot.Right, at.Y)),
        });

    public static MarkShape Sampled(PlotPlane.Cartesian plane, float scale, Func<float, float> function) =>
        new MarkShape.Polyline(Columns(plane, scale, function));

    public static MarkShape Area(PlotPlane.Cartesian plane, float scale, Func<float, float> function) =>
        new MarkShape.Polygon(Columns(plane, scale, function).Add(new(plane.Plot.Right, plane.Plot.Bottom)).Add(new(plane.Plot.Left, plane.Plot.Bottom)));

    private static Seq<SKPoint> Columns(PlotPlane.Cartesian plane, float scale, Func<float, float> function) =>
        toSeq(Range(0, (int)MathF.Ceiling(plane.Plot.Width * scale)))
            .Map(column => plane.X.Value((column + 0.5f) / (plane.Plot.Width * scale)))
            .Map(input => plane.Point(input, function(input)));

    // --- [LABELS]
    public static Seq<(SKPoint Tick, SKSize Size, string Text)> Thinned(LabelSide side, Seq<(SKPoint Tick, SKSize Size, string Text)> labels) =>
        toSeq(Range(1, labels.Count))
            .Filter(stride => stride < labels.Count && Nice(stride).Exists(step => step == stride))
            .Map(stride => labels.Map(static (label, index) => (Label: label, Index: index)).Filter(held => held.Index % stride is 0).Map(static held => held.Label))
            .Find(kept => kept.Zip(kept.Tail).ForAll(pair =>
                MathF.Abs(side.Position(pair.Second.Tick) - side.Position(pair.First.Tick)) >= (side.Span(pair.First.Size) + side.Span(pair.Second.Size)) / 2f))
            .IfNone(() => labels.Take(1));

    // --- [HIT]
    public const float Reach = 5f;

    public static Option<T> Hit<T>(Seq<PlotMark<T>> marks, SKPoint pointer) where T : notnull =>
        marks.Rev().Choose(mark => mark.Part.Filter(part => mark.Shape.Hits(pointer, part.Reach)).Map(static part => part.Key)).Head;
}
