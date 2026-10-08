using System.Globalization;
using System.Numerics;

namespace Rasm.Imaging.Pixels;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct FieldOfView : IMinMaxValue<FieldOfView> {
    public static FieldOfView MinValue { get; } = new(float.BitIncrement(0f));
    public static FieldOfView MaxValue { get; } = new(float.BitDecrement(float.Pi));
    public static FieldOfView SixthTurn { get; } = new(float.Pi / 3f);

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct SweepAngle : IMinMaxValue<SweepAngle> {
    public static SweepAngle MinValue { get; } = new(float.BitIncrement(0f));
    public static SweepAngle MaxValue { get; } = new(float.BitDecrement(float.Tau));
    public static SweepAngle HalfTurn { get; } = new(float.Pi);

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None,
    SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct SignedAngle : IMinMaxValue<SignedAngle> {
    public static SignedAngle MinValue { get; } = new(-float.Pi);
    public static SignedAngle MaxValue { get; } = new(float.Pi);
    public static SignedAngle Down { get; } = new(-float.Pi / 2f);
    public static SignedAngle Diagonal { get; } = new(float.Pi / 4f);
    public static SignedAngle Up { get; } = new(float.Pi / 2f);

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Neutral", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None,
    SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct Elevation : IMinMaxValue<Elevation> {
    public static Elevation MinValue { get; } = new(-float.Pi / 2f);
    public static Elevation MaxValue { get; } = new(float.Pi / 2f);

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Rectilinear", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None,
    SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct PaniniDistance : IMinMaxValue<PaniniDistance> {
    public static PaniniDistance MinValue { get; } = new(0f);
    public static PaniniDistance MaxValue { get; } = new(float.MaxValue);
    public static PaniniDistance Panini { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

[ValueObject<float>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None,
    SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct VerticalCompression : IMinMaxValue<VerticalCompression> {
    public static VerticalCompression MinValue { get; } = new(0f);
    public static VerticalCompression MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

[ComplexValueObject]
[ValidationError<InvalidPixelValue>]
public readonly partial struct ViewWindow {
    public float Left { get; }
    public float Right { get; }
    public float Bottom { get; }
    public float Top { get; }

    public static ViewWindow Centered(FieldOfView field, PixelExtent extent) => Centered(float.Tan(field / 2f), extent);
    public static ViewWindow Centered(SweepAngle sweep, PixelExtent extent) => Centered(sweep / 2f, extent);

    public Vector2 WindowPoint(Vector2 point, PixelExtent extent) =>
        new(Left + (point.X / extent.Width * (Right - Left)), Top - (point.Y / extent.Height * (Top - Bottom)));

    public Vector2 FramePoint(Vector2 window, PixelExtent extent) =>
        new((window.X - Left) / (Right - Left) * extent.Width, (Top - window.Y) / (Top - Bottom) * extent.Height);

    private static ViewWindow Centered(float half, PixelExtent extent) =>
        new(-half, half, -half * extent.Height / extent.Width, half * extent.Height / extent.Width);

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref float left, ref float right, ref float bottom, ref float top) =>
        validationError = (right - left, top - bottom) is ( > 0f and < float.PositiveInfinity, > 0f and < float.PositiveInfinity) ? null : new InvalidPixelValue();
}

public sealed class Homography {
    private readonly double a;
    private readonly double b;
    private readonly double c;
    private readonly double d;
    private readonly double e;
    private readonly double f;
    private readonly double g;
    private readonly double h;
    private readonly double i;

    internal Homography(double a, double b, double c, double d, double e, double f, double g, double h, double i) =>
        (this.a, this.b, this.c, this.d, this.e, this.f, this.g, this.h, this.i) = (a, b, c, d, e, f, g, h, i);

    public Homography Inverse =>
        ((a * ((e * i) - (f * h))) - (b * ((d * i) - (f * g))) + (c * ((d * h) - (e * g)))) switch {
            var det => new(
                ((e * i) - (f * h)) / det, ((c * h) - (b * i)) / det, ((b * f) - (c * e)) / det,
                ((f * g) - (d * i)) / det, ((a * i) - (c * g)) / det, ((c * d) - (a * f)) / det,
                ((d * h) - (e * g)) / det, ((b * g) - (a * h)) / det, ((a * e) - (b * d)) / det),
        };

    public Option<Vector2> Apply(Vector2 point) =>
        ((g * point.X) + (h * point.Y) + i) switch {
            > 0d and var w => Some(new Vector2((float)(((a * point.X) + (b * point.Y) + c) / w), (float)(((d * point.X) + (e * point.Y) + f) / w))),
            _ => None,
        };
}

[ComplexValueObject]
[ValidationError<InvalidPixelValue>]
[ObjectFactory<string>]
public sealed partial class FrameQuad : IConvertible<string> {
    public Vector2 TopLeft { get; }
    public Vector2 TopRight { get; }
    public Vector2 BottomRight { get; }
    public Vector2 BottomLeft { get; }

    public bool Mirrored => Turn(BottomLeft, TopLeft, TopRight) < 0d;

    public FrameQuad FlippedHorizontal => new(TopRight, TopLeft, BottomLeft, BottomRight);
    public FrameQuad FlippedVertical => new(BottomLeft, BottomRight, TopRight, TopLeft);
    public FrameQuad Turned => new(BottomLeft, TopLeft, TopRight, BottomRight);

    public Homography Homography {
        get {
            (double x0, double y0, double x1, double y1) = (TopLeft.X, TopLeft.Y, TopRight.X, TopRight.Y);
            (double x2, double y2, double x3, double y3) = (BottomRight.X, BottomRight.Y, BottomLeft.X, BottomLeft.Y);
            (double sx, double sy, double turn) = (x0 - x1 + x2 - x3, y0 - y1 + y2 - y3, ((x1 - x2) * (y3 - y2)) - ((x3 - x2) * (y1 - y2)));
            (double g, double h) = (((sx * (y3 - y2)) - ((x3 - x2) * sy)) / turn, (((x1 - x2) * sy) - (sx * (y1 - y2))) / turn);
            return new(x1 - x0 + (g * x1), x3 - x0 + (h * x3), x0, y1 - y0 + (g * y1), y3 - y0 + (h * y3), y0, g, h, 1d);
        }
    }

    public string ToValue() =>
        string.Create(CultureInfo.InvariantCulture, $"{TopLeft.X},{TopLeft.Y},{TopRight.X},{TopRight.Y},{BottomRight.X},{BottomRight.Y},{BottomLeft.X},{BottomLeft.Y}");

    public static InvalidPixelValue? Validate(string? value, IFormatProvider? provider, out FrameQuad? item) {
        item = null;
        return value is null
            ? null
            : toSeq(value.Split(',')).Traverse(static field => Invariant.Number<float>(field)).As().Case switch {
                Seq<float> and [var x0, var y0, var x1, var y1, var x2, var y2, var x3, var y3] => Validate(new(x0, y0), new(x1, y1), new(x2, y2), new(x3, y3), out item),
                _ => new InvalidPixelValue(),
            };
    }

    private static double Turn(Vector2 previous, Vector2 corner, Vector2 next) =>
        (((double)corner.X - previous.X) * ((double)next.Y - corner.Y)) - (((double)corner.Y - previous.Y) * ((double)next.X - corner.X));

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref Vector2 topLeft, ref Vector2 topRight, ref Vector2 bottomRight, ref Vector2 bottomLeft) =>
        validationError = (Turn(bottomLeft, topLeft, topRight), Turn(topLeft, topRight, bottomRight), Turn(topRight, bottomRight, bottomLeft), Turn(bottomRight, bottomLeft, topLeft)) switch {
            var turns when turns is ( > 0d, > 0d, > 0d, > 0d) or ( < 0d, < 0d, < 0d, < 0d) && double.IsFinite(turns.Item1 + turns.Item2 + turns.Item3 + turns.Item4) => null,
            _ => new InvalidPixelValue(),
        };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Frustum {
    private Frustum(ViewWindow window) => Window = window;

    public ViewWindow Window { get; }

    internal abstract Vector3 At(Vector2 window, float depth);
    internal abstract Vector2 WindowOf(Vector3 view);

    internal Option<(Vector2 Point, float Depth)> Locate(Vector3 view, PixelExtent extent) =>
        -view.Z > 0f ? Some((Window.FramePoint(WindowOf(view), extent), -view.Z)) : None;
    internal (Vector3 Origin, Vector3 Direction) Ray(Vector2 window) =>
        At(window, 0f) switch {
            var origin => (origin, Vector3.Normalize(At(window, 1f) - origin)),
        };

    public sealed record Perspective(ViewWindow Window) : Frustum(Window) {
        internal override Vector3 At(Vector2 window, float depth) => new(window * depth, -depth);
        internal override Vector2 WindowOf(Vector3 view) => new Vector2(view.X, view.Y) / -view.Z;
    }

    public sealed record Parallel(ViewWindow Window) : Frustum(Window) {
        internal override Vector3 At(Vector2 window, float depth) => new(window, -depth);
        internal override Vector2 WindowOf(Vector3 view) => new(view.X, view.Y);
    }
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct FocalLength : IMinMaxValue<FocalLength> {
    public static FocalLength MinValue { get; } = new(float.BitIncrement(0f));
    public static FocalLength MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct FocusDistance : IMinMaxValue<FocusDistance> {
    public static FocusDistance MinValue { get; } = new(float.BitIncrement(0f));
    public static FocusDistance MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct SensorSize : IMinMaxValue<SensorSize> {
    public static SensorSize MinValue { get; } = new(float.BitIncrement(0f));
    public static SensorSize MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidPixelValue>]
public readonly partial struct FNumber : IMinMaxValue<FNumber> {
    public static FNumber MinValue { get; } = new(float.BitIncrement(0f));
    public static FNumber MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidPixelValue();
}

[ComplexValueObject]
[ValidationError<InvalidPixelValue>]
public readonly partial struct LensFocus {
    public FocalLength FocalLength { get; }
    public FocusDistance FocusDistance { get; }

    static partial void ValidateFactoryArguments(ref InvalidPixelValue? validationError, ref FocalLength focalLength, ref FocusDistance focusDistance) =>
        validationError = (float)focusDistance > (float)focalLength ? null : new InvalidPixelValue();
}

public sealed record Camera {
    public Camera(Matrix4x4 viewToWorld, Frustum frustum, PixelExtent extent, Option<LensFocus> lens) =>
        (Location, Orientation, Frustum, Extent, Lens) = (viewToWorld.Translation, Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(viewToWorld)), frustum, extent, lens);

    public Vector3 Location { get; }
    public Quaternion Orientation { get; }
    public Frustum Frustum { get; }
    public PixelExtent Extent { get; }
    public Option<LensFocus> Lens { get; }

    public Matrix4x4 ViewToWorld => Matrix4x4.CreateFromQuaternion(Orientation) with { Translation = Location };
    public Matrix4x4 WorldToView => Matrix4x4.CreateTranslation(-Location) * Matrix4x4.CreateFromQuaternion(Quaternion.Conjugate(Orientation));

    public Vector3 WorldPoint(Vector2 point, float depth) =>
        Location + Vector3.Transform(Frustum.At(Frustum.Window.WindowPoint(point, Extent), depth), Orientation);

    public (Vector3 Origin, Vector3 Direction) Ray(Vector2 point) =>
        Frustum.Ray(Frustum.Window.WindowPoint(point, Extent)) switch {
            var (origin, direction) => (Location + Vector3.Transform(origin, Orientation), Vector3.Transform(direction, Orientation)),
        };

    public Option<(Vector2 Point, float Depth)> Pixel(Vector3 world) =>
        Frustum.Locate(Vector3.Transform(world - Location, Quaternion.Conjugate(Orientation)), Extent);
}

[SmartEnum]
public sealed partial class WrapMode {
    public static readonly WrapMode Black = new(static (index, size) => index >= 0 && index < size ? Some(index) : None);
    public static readonly WrapMode Clamp = new(static (index, size) => Some(int.Clamp(index, 0, size - 1)));
    public static readonly WrapMode Periodic = new(static (index, size) => Some(((index % size) + size) % size));

    [UseDelegateFromConstructor]
    public partial Option<int> Index(int index, int size);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Projection {
    private Projection(PixelExtent extent) => Extent = extent;

    public PixelExtent Extent { get; }
    public virtual bool Upright => false;
    public virtual (WrapMode Across, WrapMode Down) Wrap => (WrapMode.Black, WrapMode.Black);

    public abstract Option<Vector3> Ray(Vector2 point);
    public abstract Option<Vector2> Pixel(Vector3 ray);

    public Option<Vector2> Reprojected(Projection target, Quaternion turn, Vector2 point) =>
        Ray(point) switch {
            { IsSome: true } ray => target.Pixel(Vector3.Transform((Vector3)ray, turn)),
            _ => None,
        };

    private float Circle => (float)int.Min(Extent.Width, Extent.Height) / Extent.Width;

    private Vector2 Centered(Vector2 point) => new((2f * point.X / Extent.Width) - 1f, (Extent.Height - (2f * point.Y)) / Extent.Width);
    private Vector2 Uncentered(Vector2 centered) => new((centered.X + 1f) * Extent.Width / 2f, (Extent.Height - (centered.Y * Extent.Width)) / 2f);

    private Option<Vector3> Radial(Vector2 point, float cut, float scale, Func<float, float, float> angle) =>
        Centered(point) switch {
            var centered => angle(centered.Length(), scale) switch {
                var theta when theta <= cut => Some(new Vector3(Direction(centered) * float.Sin(theta), -float.Cos(theta))),
                _ => None,
            },
        };
    private Option<Vector2> RadialPoint(Vector3 ray, float cut, float scale, Func<float, float, float> radius) =>
        float.Atan2(float.Hypot(ray.X, ray.Y), -ray.Z) switch {
            var theta when theta <= cut => Some(Uncentered(Direction(new Vector2(ray.X, ray.Y)) * radius(theta, scale))),
            _ => None,
        };

    private static Vector2 Direction(Vector2 planar) =>
        float.Atan2(planar.Y, planar.X) switch {
            var azimuth => new(float.Cos(azimuth), float.Sin(azimuth)),
        };
    private static float SolidAngle(float radius, float reach) => 2f * float.Asin(radius / reach);
    private static float SolidRadius(float angle, float reach) => reach * float.Sin(angle / 2f);

    public sealed record Rectilinear(PixelExtent Extent, Frustum.Perspective Frustum) : Projection(Extent) {
        public override Option<Vector3> Ray(Vector2 point) => Some(Frustum.Ray(Frustum.Window.WindowPoint(point, Extent)).Direction);
        public override Option<Vector2> Pixel(Vector3 ray) => Frustum.Locate(ray, Extent).Map(static located => located.Point);
    }

    public sealed record Equirectangular(PixelExtent Extent) : Projection(Extent) {
        public override bool Upright => true;
        public override (WrapMode Across, WrapMode Down) Wrap => (WrapMode.Periodic, WrapMode.Clamp);

        public override Option<Vector3> Ray(Vector2 point) =>
            (((point.X / Extent.Width) - 0.5f) * float.Tau, (0.5f - (point.Y / Extent.Height)) * float.Pi) switch {
                var (longitude, latitude) =>
                    Some(new Vector3(float.Sin(longitude) * float.Cos(latitude), float.Sin(latitude), -float.Cos(longitude) * float.Cos(latitude))),
            };

        public override Option<Vector2> Pixel(Vector3 ray) =>
            Some(new Vector2(
                ((float.Atan2(ray.X, -ray.Z) / float.Tau) + 0.5f) * Extent.Width,
                (0.5f - (float.Atan2(ray.Y, float.Hypot(ray.X, ray.Z)) / float.Pi)) * Extent.Height));
    }

    public sealed record Cylindrical(PixelExtent Extent, ViewWindow Window) : Projection(Extent) {
        public override bool Upright => true;

        public override Option<Vector3> Ray(Vector2 point) =>
            Window.WindowPoint(point, Extent) switch {
                var window => Some(Vector3.Normalize(new Vector3(float.Sin(window.X), window.Y, -float.Cos(window.X)))),
            };

        public override Option<Vector2> Pixel(Vector3 ray) =>
            float.Hypot(ray.X, ray.Z) switch {
                > 0f and var radius => Some(Window.FramePoint(new Vector2(float.Atan2(ray.X, -ray.Z), ray.Y / radius), Extent)),
                _ => None,
            };
    }

    public sealed record Fisheye(PixelExtent Extent, SweepAngle Sweep) : Projection(Extent) {
        public override Option<Vector3> Ray(Vector2 point) => Radial(point, Sweep / 2f, Sweep / (2f * Circle), static (radius, scale) => radius * scale);
        public override Option<Vector2> Pixel(Vector3 ray) => RadialPoint(ray, Sweep / 2f, Sweep / (2f * Circle), static (angle, scale) => angle / scale);
    }

    public sealed record Equisolid(PixelExtent Extent, SweepAngle Sweep, FocalLength Lens, SensorSize Sensor) : Projection(Extent) {
        public override Option<Vector3> Ray(Vector2 point) => Radial(point, Sweep / 2f, Reach, SolidAngle);
        public override Option<Vector2> Pixel(Vector3 ray) => RadialPoint(ray, Sweep / 2f, Reach, SolidRadius);

        private float Reach => 4f * Lens * int.Max(Extent.Width, Extent.Height) / (Sensor * Extent.Width);
    }

    public sealed record MirrorBall(PixelExtent Extent) : Projection(Extent) {
        public override Option<Vector3> Ray(Vector2 point) => Radial(point, float.Pi, Circle, SolidAngle);
        public override Option<Vector2> Pixel(Vector3 ray) => RadialPoint(ray, float.Pi, Circle, SolidRadius);
    }

    public sealed record Stereographic(PixelExtent Extent, SweepAngle Sweep) : Projection(Extent) {
        public override Option<Vector3> Ray(Vector2 point) =>
            (Centered(point) * float.Tan(Sweep / 4f)) switch {
                var q => Some(new Vector3(2f * q, q.LengthSquared() - 1f) / (q.LengthSquared() + 1f)),
            };

        public override Option<Vector2> Pixel(Vector3 ray) =>
            (ray.Length() - ray.Z) switch {
                > 0f and var lift => Some(Uncentered(new Vector2(ray.X, ray.Y) / lift / float.Tan(Sweep / 4f))),
                _ => None,
            };
    }

    public sealed record Panini(PixelExtent Extent, FieldOfView Field, PaniniDistance Distance, VerticalCompression Compression) : Projection(Extent) {
        public override bool Upright => true;

        public override Option<Vector3> Ray(Vector2 point) =>
            (Centered(point) * Edge / (Distance + 1f)) switch {
                var plane => (1f + (plane.X * plane.X * (1f - (Distance * Distance)))) switch {
                    >= 0f and var root => ((float.Sqrt(root) - (plane.X * plane.X * Distance)) / (1f + (plane.X * plane.X))) switch {
                        > 0f and var cos => Some(Vector3.Normalize(new Vector3(plane.X * (Distance + cos), plane.Y * (Distance + cos) / float.Lerp(1f, 1f / cos, Compression), -cos))),
                        _ => None,
                    },
                    _ => None,
                },
            };

        public override Option<Vector2> Pixel(Vector3 ray) =>
            float.Hypot(ray.X, ray.Z) switch {
                var radius when -ray.Z > 0f =>
                    Some(Uncentered(new Vector2(ray.X, ray.Y * float.Lerp(1f, radius / -ray.Z, Compression)) * (Distance + 1f) / ((Distance * radius) - ray.Z) / Edge)),
                _ => None,
            };

        private float Edge => (Distance + 1f) * float.Sin(Field / 2f) / (Distance + float.Cos(Field / 2f));
    }
}

public readonly record struct PanoramaView(SignedAngle Heading, Elevation Elevation, SignedAngle Roll) {
    public Quaternion Rotation => Turn(Heading, Elevation, Roll);

    public static Option<(float Heading, float Elevation, float Roll)> Facing(Vector3 left, Vector3 right) =>
        from forward in Some(left + right).Filter(static sum => sum != Vector3.Zero)
        let across = right - left - (Vector3.Dot(right - left, forward) / forward.LengthSquared() * forward)
        where across != Vector3.Zero
        let heading = float.Atan2(forward.X, -forward.Z)
        let elevation = float.Atan2(forward.Y, float.Hypot(forward.X, forward.Z))
        let level = Vector3.Transform(across, Quaternion.Conjugate(Turn(heading, elevation, 0f)))
        select (heading, elevation, float.Atan2(level.Y, level.X));

    private static Quaternion Turn(float heading, float elevation, float roll) => Quaternion.CreateFromYawPitchRoll(-heading, elevation, roll);
}
