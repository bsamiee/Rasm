using System.Globalization;
using System.Numerics;
using System.Runtime.Intrinsics;
using MathNet.Numerics;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Warping;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class LensFraming {
    public static readonly LensFraming Edges = new("edges", static _ => 1d);
    public static readonly LensFraming Corners = new("corners", static extent => (double.Pow(extent.Width, 2d) + double.Pow(extent.Height, 2d)) / double.Pow(int.Max(extent.Width, extent.Height), 2d));

    public double Scale(PixelExtent extent, double coefficient) => 1d / (1d + ((coefficient > 0d ? Reach(extent) : 1d) * coefficient));

    [UseDelegateFromConstructor]
    private partial double Reach(PixelExtent extent);
}

[SmartEnum<string>]
public sealed partial class LensDirection {
    public static readonly LensDirection Distort = new("distort", static (coefficient, scale, radius) =>
        (4d * coefficient * scale * scale * radius) switch { var bound => (2d * scale / (1d + double.Sqrt(double.Max(0d, 1d - bound))), bound > 1d ? 0f : 1f) });
    public static readonly LensDirection Undistort = new("undistort", static (coefficient, scale, radius) =>
        (coefficient * radius) switch { var bound => bound > -1d ? (1d / ((1d + bound) * scale), bound > 1d ? 0f : 1f) : (0d, 0f) });

    [UseDelegateFromConstructor]
    internal partial (double Factor, float Weight) Radial(double coefficient, double scale, double radius);
}

[ComplexValueObject]
[ValidationError<ImagingFault>]
public sealed partial class RadialDistortion {
    public static (double Low, double High) Bounds { get; } = (-0.999d, 1d);

    public double Coefficient { get; }
    public double Dispersion { get; }
    public LensFraming Framing { get; }
    public LensDirection Direction { get; }

    private (double Red, double Green, double Blue) Channels =>
        (double.Min(Coefficient + (Dispersion / 4d), Bounds.High), Coefficient, double.Max(Coefficient - (Dispersion / 4d), Bounds.Low));

    public CoordinateMap.Analytic Map(WrapMode wrap) =>
        new((points, coverage, extent) => {
            double scale = Framing.Scale(extent, Channels.Red);
            for (int i = 0; i < points.Length; i++)
                (points[i], coverage[i]) = Mapped(Coefficient, points[i], extent, scale, coverage[i]);
        }, Sampling.Linear, (wrap, wrap));

    public PixelPass.Frame Pass(WrapMode wrap) =>
        Dispersion is 0d ? new(Map(wrap).Apply)
            : Channels switch {
                var (red, green, blue) => new(CoordinateMap.IntegratedLinear(static integral => ContourIntegrate.GaussKronrod(integral, 0d, 1d), wrap,
                    new MapPath(static t => new Vector4((float)(1d - t), (float)t, 0f, 1f), (point, t, extent) => Mapped(double.Lerp(red, green, t), point, extent, Framing.Scale(extent, red), 1f)),
                    new MapPath(static t => new Vector4(0f, (float)(1d - t), (float)t, 1f), (point, t, extent) => Mapped(double.Lerp(green, blue, t), point, extent, Framing.Scale(extent, red), 1f)))),
            };

    private ((double X, double Y) Point, float Coverage) Mapped(double coefficient, (double X, double Y) point, PixelExtent extent, double scale, float coverage) {
        (double x, double y) = (extent.Width / 2d, extent.Height / 2d);
        double half = double.Max(x, y);
        double radius = double.Pow((point.X - x) / half, 2d) + double.Pow((point.Y - y) / half, 2d);
        (double factor, float weight) = Direction.Radial(coefficient, scale, radius);
        return ((x + (factor * (point.X - x)), y + (factor * (point.Y - y))),
            (Dispersion is 0d ? weight * weight : Channels switch { var (red, _, blue) => Direction.Radial(red, scale, radius).Weight * Direction.Radial(blue, scale, radius).Weight }) * coverage);
    }

    public static Fin<RadialDistortion> Validated(double coefficient, double dispersion, LensFraming framing, LensDirection direction) =>
        TryCreate(coefficient, dispersion, framing, direction, out RadialDistortion? distortion, out ImagingFault? fault) ? distortion : fault;

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref double coefficient, ref double dispersion, ref LensFraming framing, ref LensDirection direction) =>
        validationError = coefficient >= Bounds.Low && coefficient <= Bounds.High && dispersion >= 0d ? null
            : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"radial coefficient {coefficient} in [{Bounds.Low}, {Bounds.High}] with dispersion {dispersion}"), ImagingReason.Range);
}

public sealed record AxialDefocus(Iris Iris, ShortSideLength Radius) {
    public PixelPass.Frame Pass =>
        Bokeh.Gathered(Iris, 0d, (light, _, frame) => Bokeh.Correlate(light, Iris, 0d, Vector256.Create(0.5d, 0d, 1d, 0d) * Radius.Pixels(frame.Extent)));
}
