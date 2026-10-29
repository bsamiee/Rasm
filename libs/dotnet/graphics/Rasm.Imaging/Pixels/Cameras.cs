using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.RootFinding;
using UnitsNet;

namespace Rasm.Imaging.Pixels;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class ViewWindow {
    public Vector2 Center { get; }
    public Vector2 Size { get; }

    public static Fin<ViewWindow> Centered(Angle field, PixelExtent extent) => Centered(2f * float.Tan((float)field.Radians.ToDouble() / 2f), extent);
    public static Fin<ViewWindow> Centered(float width, PixelExtent extent) => Validated(Vector2.Zero, new(width, width * extent.Height / extent.Width));

    public ViewWindow Raised(float height) => new(Center + new Vector2(0f, height), Size);

    public Matrix3x2 ToWindow(PixelExtent extent) =>
        Matrix3x2.CreateTranslation(-0.5f * extent.Point(Vector2.One)) * Matrix3x2.CreateScale(Size / extent.Point(new Vector2(1f, -1f))) * Matrix3x2.CreateTranslation(Center);

    public Matrix3x2 FromWindow(PixelExtent extent) =>
        Matrix3x2.CreateTranslation(-Center) * Matrix3x2.CreateScale(extent.Point(new Vector2(1f, -1f)) / Size) * Matrix3x2.CreateTranslation(0.5f * extent.Point(Vector2.One));

    public static Fin<ViewWindow> Validated(Vector2 center, Vector2 size) =>
        ImagingFault.Unless(Vector2.AllWhereAllBitsSet(Vector2.IsFinite(center)) && size.X > 0f && size.Y > 0f && float.IsFinite(size.X) && float.IsFinite(size.Y), ImagingReason.Range, (center, size))
            .Map(_ => new ViewWindow(center, size));
}

public sealed record LensDistortion {
    private readonly double fold;

    public LensDistortion(double k1, double k2, double p1, double p2, double k3) =>
        (K1, K2, P1, P2, K3, fold) = (k1, k2, p1, p2, k3,
            FindRoots.Polynomial([1d, 3d * k1, 5d * k2, 7d * k3]).Where(static root => root.Imaginary is 0d && root.Real > 0d).Select(static root => root.Real).DefaultIfEmpty(double.PositiveInfinity).Min());

    public double K1 { get; }
    public double K2 { get; }
    public double P1 { get; }
    public double P2 { get; }
    public double K3 { get; }

    public Option<Vector2> Distorted(Vector2 ideal) =>
        ideal.LengthSquared() <= fold ? Some(Applied(ideal.X, ideal.Y) switch { var (x, y) => new Vector2((float)x, (float)y) }) : None;

    public Option<Vector2> Ideal(Vector2 distorted) {
        double reach = distorted.Length();
        if (Unbent(reach) is not { IsSome: true } radius)
            return None;
        Vector2 seed = reach > 0d ? distorted * (float)((double)radius / reach) : distorted;
        return P1 is 0d && P2 is 0d ? seed
            : Broyden.TryFindRoot(guess => Applied(guess[0], guess[1]) switch { var (x, y) => [x - distorted.X, y - distorted.Y] }, [seed.X, seed.Y], Precision.SinglePrecision, 100, out double[] root)
                && (root[0] * root[0]) + (root[1] * root[1]) <= fold ? new Vector2((float)root[0], (float)root[1]) : None;
    }

    private Option<double> Unbent(double reach) {
        Func<double, double> excess = radius => (radius * Radial(radius * radius)) - reach;
        (bool bounded, double low, double high) = double.IsFinite(fold) ? (true, 0d, double.Sqrt(fold)) : (false, 0d, double.Max(2d * reach, double.Epsilon));
        return reach is 0d ? 0d
            : (bounded ? excess(high) >= 0d : ZeroCrossingBracketing.Expand(excess, ref low, ref high, 1.6d, 50)) && Brent.TryFindRoot(excess, low, high, Precision.SinglePrecision, 100, out double radius) ? radius : None;
    }

    private double Radial(double r2) => 1d + (r2 * (K1 + (r2 * (K2 + (r2 * K3)))));

    private (double X, double Y) Applied(double x, double y) =>
        ((x * x) + (y * y)) switch {
            var r2 => Radial(r2) switch {
                var radial => ((x * radial) + (2d * P1 * x * y) + (P2 * (r2 + (2d * x * x))), (y * radial) + (P1 * (r2 + (2d * y * y))) + (2d * P2 * x * y)),
            },
        };
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class LensFocus {
    public Length FocalLength { get; }
    public Length FocusDistance { get; }
    public double FNumber { get; }

    public static Fin<LensFocus> Validated(Length focalLength, Length focusDistance, double fNumber) =>
        ImagingFault.Unless(focalLength > Length.Zero && focusDistance > focalLength && fNumber > 0d && double.IsFinite(fNumber), ImagingReason.Range, (focalLength, focusDistance, fNumber))
            .Map(_ => new LensFocus(focalLength, focusDistance, fNumber));
}

public sealed record Camera {
    public Camera(Matrix4x4 viewToWorld, Projection lens, Option<LensFocus> focus) =>
        (Location, Orientation, Lens, Focus) = (viewToWorld.Translation, Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(viewToWorld)), lens, focus);

    public Vector3 Location { get; }
    public Quaternion Orientation { get; }
    public Projection Lens { get; }
    public Option<LensFocus> Focus { get; }

    public Matrix4x4 ViewToWorld => Matrix4x4.CreateFromQuaternion(Orientation) with { Translation = Location };
    public Matrix4x4 WorldToView => Matrix4x4.CreateTranslation(-Location) * Matrix4x4.CreateFromQuaternion(Quaternion.Conjugate(Orientation));

    public Option<Vector3> WorldPoint(Vector2 pixel, float depth) => Lens.Point(pixel, depth).Map(view => Location + Vector3.Transform(view, Orientation));

    public Option<(Vector3 Origin, Vector3 Direction)> Ray(Vector2 pixel) =>
        Lens.Ray(pixel).Map(ray => (Location + Vector3.Transform(ray.Origin, Orientation), Vector3.Transform(ray.Direction, Orientation)));

    public Option<(Vector2 Point, float Depth)> Pixel(Vector3 world) => Lens.Pixel(Vector3.Transform(world - Location, Quaternion.Conjugate(Orientation)));

    public Option<(Vector2 Point, float Depth)> Distant(Vector3 direction) =>
        Lens is Projection.Orthographic
            ? None
            : Lens.Pixel(Vector3.Transform(direction, Quaternion.Conjugate(Orientation))).Map(static located => (located.Point, float.PositiveInfinity));
}

public sealed record Occluder(PixelFrame Depth, float Source) {
    public bool Hides(Vector2 point) =>
        Depth.Texel(new((int)float.Floor(point.X), (int)float.Floor(point.Y)), (WrapMode.Black, WrapMode.Black)) is { IsSome: true } texel && ((Vector4)texel).X < Source;

    public float Visible(Vector2 source, float reach) {
        ((int Low, int High) across, (int Low, int High) down) = (Window(source.X, Depth.Extent.Width), Window(source.Y, Depth.Extent.Height));
        Vector2 seen = (from y in Enumerable.Range(down.Low, down.High - down.Low)
                        from x in Enumerable.Range(across.Low, across.High - across.Low)
                        let center = new Vector2(x + 0.5f, y + 0.5f)
                        where Vector2.Distance(center, source) <= reach
                        select new Vector2(1f, Hides(center) ? 0f : 1f)).Aggregate(Vector2.Zero, static (sum, cell) => sum + cell);
        return seen.X is 0f ? Hides(source) ? 0f : 1f : seen.Y / seen.X;

        (int Low, int High) Window(float at, int size) => ((int)float.Clamp(float.Floor(at - reach), 0f, size), (int)float.Clamp(float.Ceiling(at + reach), 0f, size));
    }
}
