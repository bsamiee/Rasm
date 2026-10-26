using System.Numerics;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.UI.Styling;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Imaging;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Ramps {
    private static readonly PlotPlane.Track Unit = new(Orientation.Horizontal, new Axis.Linear(AxisRange.Create(0f, 1f)));
    private static readonly Sizing Grown = new Sizing.Grow(Factor.Create(1f));

    public static Node.Plot Key(Ramp ramp, Gamut gamut, Seq<(double At, string Text)> labels) =>
        (ramp.Interpolation == RampInterpolation.Constant ? Bands(ramp, gamut) : new Node.Plot(Unit, Checked(ramp, [Field(Unit, x => ramp.Tabulate(gamut).Sample(x.X), gamut)])) { Width = Grown }) with {
            Labels = labels.Map(static label => new Node.Pin(new Vector2((float)label.At, 0f), new Node.Text([label.Text]) { Role = Role.Label }) { Align = UnitPoint.Create(0.5f, 0f) }),
        };

    public static Node.Image Field(PlotPlane plane, Func<Vector2, Vector4> color, Gamut gamut) =>
        Tabulated(Lattice(plane, Orientation.Horizontal), Lattice(plane, Orientation.Vertical), plane.Aspect, gamut, (column, row, columns, rows) =>
            plane.Value(new SKPoint((column + 0.5f) / columns, 1f - ((row + 0.5f) / rows))) switch {
                var value => plane.Dimensions.ForAll(held => held.Axis.Contains(held.Axis.Range, held.Along.Of(value))) ? color(value) : Vector4.Zero,
            });

    public static Node.Plot Bands(Ramp ramp, Gamut gamut) =>
        new(Unit, Checked(ramp, ramp.Stops
            .Map((stop, index) => (Index: index, stop.Color, From: index is 0 ? 0f : (float)(double)stop.Position, To: ramp.Stops.At(index + 1).Map(static next => (float)(double)next.Position).IfNone(1f)))
            .Filter(static band => band.To > band.From)
            .Map(band => (Node)new Node.Mark(new Figure.Region(Some(AxisRange.Create(band.From, band.To)), None)) {
                Role = ImagingRoles.Series,
                Series = new Series((uint)band.Index) { Color = Shown(band.Color, gamut) },
            }))) { Width = Grown };

    internal static Node.Image Tabulated(int columns, int rows, Option<Factor> aspect, Gamut gamut, Func<int, int, int, int, Vector4> sample) {
        Matrix<double> into = gamut.MatrixTo(Gamut.StandardRgb);
        (Vector3 red, Vector3 green, Vector3 blue) = (Column(into, 0), Column(into, 1), Column(into, 2));
        Vector4[] texels = new Vector4[columns * rows];
        for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
                texels[(row * columns) + column] = sample(column, row, columns, rows) switch {
                    var held => new Vector4((red * held.X) + (green * held.Y) + (blue * held.Z), held.W),
                };
        return new Node.Image(SKImage.FromPixelCopy(
            new SKImageInfo(columns, rows, SKColorType.RgbaF32, SKAlphaType.Unpremul, SKColorSpace.CreateSrgbLinear()), MemoryMarshal.AsBytes(texels.AsSpan()))) {
            Fit = aspect.IsSome ? ImageFit.Contain : ImageFit.Stretch,
            Width = Grown,
            Height = Grown,
        };
    }

    internal static SKColorF Shown(Vector4 linear, Gamut gamut) =>
        new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, linear.X, linear.Y, linear.Z).ConvertToConfiguration(Configuration.Default).Rgb.Clipped switch {
            var encoded => new SKColorF((float)encoded.R, (float)encoded.G, (float)encoded.B, linear.W),
        };

    private static Seq<Node> Checked(Ramp ramp, Seq<Node> ground) =>
        ramp.Stops.Exists(static stop => stop.Color.W < 1f) ? new Node.Box { Role = ImagingRoles.Checker, Width = Grown, Height = Grown }.Cons(ground) : ground;

    private static int Lattice(PlotPlane plane, Orientation along) => plane.Dimension(along).IsSome ? RampTable.Texels : 1;

    private static Vector3 Column(Matrix<double> matrix, int column) => new((float)matrix[0, column], (float)matrix[1, column], (float)matrix[2, column]);
}
