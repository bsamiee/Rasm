using System.Numerics;
using UnitsNet;

namespace Rasm.Imaging.Pixels;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class ViewWindow {
    public double CenterX { get; }
    public double CenterY { get; }
    public double Width { get; }
    public double Height { get; }

    public static ViewWindow Centered(Angle field, PixelExtent extent) => Centered(2d * double.Tan(field.Radians.ToDouble() / 2d), extent);
    public static ViewWindow Centered(double width, PixelExtent extent) => new(0d, 0d, width, width * extent.Height / extent.Width);

    public ViewWindow Raised(float height) => new(CenterX, CenterY + height, Width, Height);

    public (double X, double Y) WindowPoint(double x, double y, PixelExtent extent) =>
        (CenterX + ((x - (extent.Width / 2d)) * Width / extent.Width), CenterY + (((extent.Height / 2d) - y) * Height / extent.Height));

    public Option<(double X, double Y)> FramePoint((double X, double Y) window, PixelExtent extent) =>
        from input in double.IsFinite(window.X) && double.IsFinite(window.Y) ? Some(window) : None
        let x = (input.X - CenterX + (Width / 2d)) * extent.Width / Width
        let y = (CenterY - input.Y + (Height / 2d)) * extent.Height / Height
        from result in double.IsFinite(x) && double.IsFinite(y) ? Some((x, y)) : None
        select result;

    public static Fin<ViewWindow> Validated(double centerX, double centerY, double width, double height) =>
        ImagingFault.Unless(width > 0d && height > 0d, ImagingReason.Range, (width, height)).Map(_ => new ViewWindow(centerX, centerY, width, height));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Frustum {
    private Frustum(ViewWindow window) => Window = window;

    public ViewWindow Window { get; }

    internal abstract Vector3 At((double X, double Y) window, float depth);
    internal abstract (double X, double Y) WindowOf(Vector3 view);
    internal abstract (Vector3 Origin, Vector3 Direction) Ray((double X, double Y) point, PixelExtent extent);

    internal Option<((double X, double Y) Point, float Depth)> Locate(Vector3 view, PixelExtent extent) =>
        -view.Z > 0f ? Window.FramePoint(WindowOf(view), extent).Map(point => (point, -view.Z)) : None;

    public sealed record Perspective(ViewWindow Window) : Frustum(Window) {
        internal override Vector3 At((double X, double Y) window, float depth) => new((float)(window.X * depth), (float)(window.Y * depth), -depth);
        internal override (double X, double Y) WindowOf(Vector3 view) => ((double)view.X / -view.Z, (double)view.Y / -view.Z);
        internal override (Vector3 Origin, Vector3 Direction) Ray((double X, double Y) point, PixelExtent extent) =>
            Window.WindowPoint(point.X, point.Y, extent) switch {
                var (x, y) => double.Hypot(double.Hypot(x, y), 1d) switch {
                    var length => (Vector3.Zero, new Vector3((float)(x / length), (float)(y / length), (float)(-1d / length))),
                },
            };
    }

    public sealed record Parallel(ViewWindow Window) : Frustum(Window) {
        internal override Vector3 At((double X, double Y) window, float depth) => new((float)window.X, (float)window.Y, -depth);
        internal override (double X, double Y) WindowOf(Vector3 view) => (view.X, view.Y);
        internal override (Vector3 Origin, Vector3 Direction) Ray((double X, double Y) point, PixelExtent extent) => (At(Window.WindowPoint(point.X, point.Y, extent), 0f), -Vector3.UnitZ);
    }
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class LensFocus {
    public Length FocalLength { get; }
    public Length FocusDistance { get; }

    public static Fin<LensFocus> Validated(Length focalLength, Length focusDistance) =>
        ImagingFault.Unless(focalLength > Length.Zero && focusDistance > focalLength, ImagingReason.Range, (focalLength, focusDistance)).Map(_ => new LensFocus(focalLength, focusDistance));
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

    public Vector3 WorldPoint((double X, double Y) point, float depth) =>
        Location + Vector3.Transform(Frustum.At(Frustum.Window.WindowPoint(point.X, point.Y, Extent), depth), Orientation);

    public (Vector3 Origin, Vector3 Direction) Ray((double X, double Y) point) =>
        Frustum.Ray(point, Extent) switch {
            var (origin, direction) => (Location + Vector3.Transform(origin, Orientation), Vector3.Transform(direction, Orientation)),
        };

    public Option<((double X, double Y) Point, float Depth)> Pixel(Vector3 world) =>
        Frustum.Locate(Vector3.Transform(world - Location, Quaternion.Conjugate(Orientation)), Extent);

    public Option<((double X, double Y) Point, float Depth)> Distant(Vector3 direction) =>
        Frustum.Switch((Camera: this, Direction: direction),
            perspective: static (at, _) => at.Camera.Pixel(at.Camera.Location + at.Direction).Map(static located => (located.Point, Depth: float.PositiveInfinity)),
            parallel: static (_, _) => Option<((double X, double Y) Point, float Depth)>.None);

    public Option<float> Illumination((double X, double Y) point) =>
        Frustum.Switch((Point: point, Extent),
            perspective: static (at, frustum) => frustum.Window.WindowPoint(at.Point.X, at.Point.Y, at.Extent) switch {
                var tangent => (1d + (tangent.X * tangent.X) + (tangent.Y * tangent.Y)) switch { var spread => Some((float)(1d / (spread * spread))) },
            },
            parallel: static (_, _) => Option<float>.None);
}

public sealed record Occluder(PixelFrame Depth, float Source) {
    public bool Hides((double X, double Y) point) =>
        Depth.Sample((double.Floor(point.X) + 0.5d, double.Floor(point.Y) + 0.5d), (WrapMode.Black, WrapMode.Black),
            float.PositiveInfinity, static (_, sample, _) => sample.X) < Source;

    public float Visible((double X, double Y) source, double reach) {
        (Occluder occluder, PixelExtent extent) = (this, Depth.Extent);
        Seq<(double X, double Y)> disc = toSeq(
            from y in Window(source.Y, extent.Height)
            from x in Window(source.X, extent.Width)
            let center = (X: x + 0.5d, Y: y + 0.5d)
            where double.Hypot(center.X - source.X, center.Y - source.Y) <= reach
            select center);
        return reach is 0d || disc.IsEmpty ? 1f : disc.Filter(point => !occluder.Hides(point)).Count / (float)disc.Count;

        IEnumerable<int> Window(double at, int size) =>
            (int)double.Clamp(double.Floor(at - reach), 0d, size) switch { var low => Enumerable.Range(low, (int)double.Clamp(double.Ceiling(at + reach), 0d, size) - low) };
    }
}
