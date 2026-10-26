using System.Globalization;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Pixels;
using UnitsNet;
using Wacton.Unicolour;

namespace Rasm.Imaging.Color.Calibration;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class ChartPatch {
    public static readonly ChartPatch DarkSkin = new((0.1136398927, 0.09832436105, 0.047793811));
    public static readonly ChartPatch LightSkin = new((0.3811104477, 0.336202304, 0.1852590702));
    public static readonly ChartPatch BlueSky = new((0.1652470004, 0.1785519348, 0.2546024121));
    public static readonly ChartPatch Foliage = new((0.1114392339, 0.1346792679, 0.05239320311));
    public static readonly ChartPatch BlueFlower = new((0.2419823988, 0.2287175998, 0.3282104382));
    public static readonly ChartPatch BluishGreen = new((0.30451114, 0.4143554688, 0.344352688));
    public static readonly ChartPatch Orange = new((0.4073691399, 0.3126416159, 0.05130591012));
    public static readonly ChartPatch PurplishBlue = new((0.1200518326, 0.1091090233, 0.2874447494));
    public static readonly ChartPatch ModerateRed = new((0.2915036416, 0.188999956, 0.09736350318));
    public static readonly ChartPatch Purple = new((0.08353888545, 0.06276662955, 0.1042075686));
    public static readonly ChartPatch YellowGreen = new((0.3427379502, 0.4331759409, 0.08330791241));
    public static readonly ChartPatch OrangeYellow = new((0.4769723742, 0.4293377578, 0.06005041429));
    public static readonly ChartPatch Blue = new((0.06809095613, 0.05596214063, 0.2077405936));
    public static readonly ChartPatch Green = new((0.1413517689, 0.2233437582, 0.07287461742));
    public static readonly ChartPatch Red = new((0.2143728424, 0.127800835, 0.03868150726));
    public static readonly ChartPatch Yellow = new((0.5888922356, 0.5992976803, 0.07077420003));
    public static readonly ChartPatch Magenta = new((0.299122798, 0.1895114577, 0.2213469194));
    public static readonly ChartPatch Cyan = new((0.1247966941, 0.180609913, 0.2913392383));
    public static readonly ChartPatch White95 = new((0.8436985288, 0.8806903203, 0.6936778752));
    public static readonly ChartPatch Neutral8 = new((0.5665335579, 0.5899709702, 0.4828473821));
    public static readonly ChartPatch Neutral65 = new((0.3495921991, 0.3648652066, 0.3013565492));
    public static readonly ChartPatch Neutral5 = new((0.1835495863, 0.1906228754, 0.1566717383));
    public static readonly ChartPatch Neutral35 = new((0.08448968042, 0.08817234828, 0.07391630753));
    public static readonly ChartPatch Black2 = new((0.03042544265, 0.03151319431, 0.02656724434));

    public (double X, double Y, double Z) ClassicD50 { get; }
}

[SmartEnum<string>]
public sealed partial class ColorChart {
    public const int Columns = 6;

    private static readonly Configuration D50 = new(xyzConfig: XyzConfiguration.D50);

    public static readonly ColorChart ColorCheckerClassic = new("color-checker-classic", toSeq(ChartPatch.Items).Map(static patch => (patch, patch.ClassicD50)));
    public static readonly ColorChart SpyderCheckr24 = new("spyder-checkr-24", Seq(
        (ChartPatch.BluishGreen, (0.3076822512, 0.4234772396, 0.3374520529)),
        (ChartPatch.BlueFlower, (0.2452835592, 0.2348654227, 0.3416765010)),
        (ChartPatch.Foliage, (0.1037198781, 0.1279977883, 0.0523456534)),
        (ChartPatch.BlueSky, (0.1692686551, 0.1858660205, 0.2666809013)),
        (ChartPatch.LightSkin, (0.3952982368, 0.3553099128, 0.1928659515)),
        (ChartPatch.DarkSkin, (0.1113998964, 0.0966807861, 0.0490883941)),
        (ChartPatch.Orange, (0.3888763433, 0.2946518384, 0.0431458486)),
        (ChartPatch.PurplishBlue, (0.1172593609, 0.1098191865, 0.2771796138)),
        (ChartPatch.ModerateRed, (0.2996723782, 0.1951990129, 0.1039804505)),
        (ChartPatch.Purple, (0.0917247784, 0.0693130081, 0.1219016829)),
        (ChartPatch.YellowGreen, (0.3562702838, 0.4446769162, 0.0880911399)),
        (ChartPatch.OrangeYellow, (0.5038320114, 0.4391253963, 0.0603927928)),
        (ChartPatch.Cyan, (0.1208589631, 0.1795378021, 0.2854043683)),
        (ChartPatch.Magenta, (0.2990686680, 0.1930314394, 0.2271268085)),
        (ChartPatch.Yellow, (0.6242483660, 0.6326192430, 0.0712084288)),
        (ChartPatch.Red, (0.2194131115, 0.1230703602, 0.0384412857)),
        (ChartPatch.Green, (0.1438849866, 0.2259240844, 0.0741006071)),
        (ChartPatch.Blue, (0.0669023563, 0.0544993983, 0.2116807502)),
        (ChartPatch.White95, (0.8886310787, 0.9226861820, 0.7490419119)),
        (ChartPatch.Neutral8, (0.5669540573, 0.5894253744, 0.4798162851)),
        (ChartPatch.Neutral65, (0.3432548507, 0.3572595422, 0.2897947815)),
        (ChartPatch.Neutral5, (0.1838367508, 0.1913948585, 0.1556906989)),
        (ChartPatch.Neutral35, (0.0807065939, 0.0838392766, 0.0677486804)),
        (ChartPatch.Black2, (0.0248551698, 0.0258980980, 0.0222499522))));

    public Seq<(ChartPatch Patch, (double X, double Y, double Z) D50)> Cells { get; }
    public static int Rows => ChartPatch.Items.Count / Columns;

    public HashMap<ChartPatch, (double R, double G, double B)> References(Gamut gamut) =>
        toHashMap(Cells.Map(cell => (cell.Patch, new Unicolour(D50, ColourSpace.Xyz, cell.D50).ConvertToConfiguration(gamut.Configuration).RgbLinear.Tuple)));

    public static ((double X, double Y) TopLeft, (double X, double Y) TopRight, (double X, double Y) BottomRight, (double X, double Y) BottomLeft) Centered((double X, double Y, double Width, double Height) bounds) =>
        (double.Sqrt(0.1d * bounds.Width * bounds.Height * Columns / Rows), double.Sqrt(0.1d * bounds.Width * bounds.Height * Rows / Columns)) switch {
            var (width, height) => (double.Min(1d, double.Min(bounds.Width / width, bounds.Height / height)) / 2d, X: bounds.X + (bounds.Width / 2d), Y: bounds.Y + (bounds.Height / 2d)) switch {
                var (scale, x, y) => ((x - (scale * width), y - (scale * height)), (x + (scale * width), y - (scale * height)),
                    (x + (scale * width), y + (scale * height)), (x - (scale * width), y + (scale * height))),
            },
        };
}

[ComplexValueObject]
[ValidationError<ImagingFault>]
public sealed partial class ChartSamples {
    public ColorChart Chart { get; }
    public Gamut Gamut { get; }
    public HashMap<ChartPatch, (double R, double G, double B)> Patches { get; }

    public static Fin<ChartSamples> Validated(ColorChart chart, Gamut gamut, HashMap<ChartPatch, (double R, double G, double B)> patches) =>
        TryCreate(chart, gamut, patches, out ChartSamples? samples, out ImagingFault? fault) ? samples : fault;

    static partial void ValidateFactoryArguments(ref ImagingFault? validationError, ref ColorChart chart, ref Gamut gamut, ref HashMap<ChartPatch, (double R, double G, double B)> patches) =>
        validationError = patches.Count == ChartPatch.Items.Count ? null
            : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"chart samples of {patches.Count} for {ChartPatch.Items.Count} patches"), ImagingReason.Miscounted);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChartPlacement {
    private ChartPlacement(ColorChart chart, FrameQuad quad, float footprint) => (Chart, Quad, Footprint) = (chart, quad, footprint);

    public static float CentralFootprint => 0.8f;

    public ColorChart Chart { get; }
    public FrameQuad Quad { get; }
    public float Footprint { get; }

    public Seq<(ChartPatch Patch, (double X, double Y) TopLeft, (double X, double Y) TopRight, (double X, double Y) BottomRight, (double X, double Y) BottomLeft)> Footprints =>
        (Quad.Homography, (X: Footprint / 2d / ColorChart.Columns, Y: Footprint / 2d / ColorChart.Rows)) switch {
            var (map, half) => Chart.Cells.Map((cell, index) => Center(index) switch {
                var center => Each(((center.X - half.X, center.Y - half.Y), (center.X + half.X, center.Y - half.Y),
                        (center.X + half.X, center.Y + half.Y), (center.X - half.X, center.Y + half.Y)), map.Apply)
                    .Map(corners => (cell.Patch, corners.TopLeft, corners.TopRight, corners.BottomRight, corners.BottomLeft)),
            }).Somes(),
        };

    public Seq<ChartFinding> Findings(PixelExtent frame) {
        static double Angle(((double X, double Y) Previous, (double X, double Y) Corner, (double X, double Y) Next) turn) {
            (double ax, double ay, double bx, double by) = (turn.Previous.X - turn.Corner.X, turn.Previous.Y - turn.Corner.Y, turn.Next.X - turn.Corner.X, turn.Next.Y - turn.Corner.Y);
            return double.Atan2(double.Abs((ax * by) - (ay * bx)), (ax * bx) + (ay * by));
        }
        return (double.Pi / 3d, 2d * double.Pi / 3d) switch {
            var (narrowest, widest) => Seq(
                Quad.Mirrored ? Some<ChartFinding>(new ChartFinding.Reversed()) : None,
                Seq((Quad.BottomLeft, Quad.TopLeft, Quad.TopRight), (Quad.TopLeft, Quad.TopRight, Quad.BottomRight), (Quad.TopRight, Quad.BottomRight, Quad.BottomLeft), (Quad.BottomRight, Quad.BottomLeft, Quad.TopLeft))
                    .Map(Angle)
                    .Fold((Smallest: double.PositiveInfinity, Largest: 0d), static (range, angle) => (double.Min(range.Smallest, angle), double.Max(range.Largest, angle))) switch {
                        var (smallest, largest) when smallest < narrowest || largest > widest => Some<ChartFinding>(new ChartFinding.Distorted(smallest, largest)),
                        _ => None,
                    },
                Switch(
                    (Frame: frame, Widest: widest),
                    flat: static (state, flat) =>
                        Some(flat.Footprints.Filter(footprint => Seq(footprint.TopLeft, footprint.TopRight, footprint.BottomRight, footprint.BottomLeft)
                            .Exists(point => point.X < 0f || point.X > state.Frame.Width || point.Y < 0f || point.Y > state.Frame.Height)).Map(static footprint => footprint.Patch))
                            .Filter(static patches => !patches.IsEmpty).Map(static patches => (ChartFinding)new ChartFinding.PatchesOutside(patches)),
                    latLong: static (state, latLong) => latLong.Quad.Homography switch {
                        var map =>
                            from left in map.Apply((0d, 0.5d)).Bind(latLong.Lens.Ray)
                            from right in map.Apply((1d, 0.5d)).Bind(latLong.Lens.Ray)
                            where latLong.Field > state.Widest
                            select (ChartFinding)new ChartFinding.WideView(latLong.Field, Between(left, right)),
                    })).Somes(),
        };
    }

    public Fin<ChartSamples> Sample(PixelFrame frame, Gamut gamut) {
        static double Length((double X, double Y) from, (double X, double Y) to) => double.Hypot(to.X - from.X, to.Y - from.Y);
        static double MeanLength((double X, double Y) from, (double X, double Y) to, (double X, double Y) otherFrom, (double X, double Y) otherTo) =>
            (Length(from, to) + Length(otherFrom, otherTo)) / 2d;
        int Count(double length, int cells) => int.Max(1, (int)double.Round(Footprint * length / cells, MidpointRounding.ToEven));
        static double Offset(int index, int count, double length) => (index + 0.5d - (count / 2d)) / length;
        static double Ratio(double part, double weight) => weight is 0d ? 0d : part / weight;
        static (double R, double G, double B) Mean((double R, double G, double B, double Weight) sum) => (Ratio(sum.R, sum.Weight), Ratio(sum.G, sum.Weight), Ratio(sum.B, sum.Weight));
        (Func<(double X, double Y), Option<(double X, double Y)>> image, (WrapMode Across, WrapMode Down) wrap) = ToImage(frame.Extent);
        Func<(double R, double G, double B, double Weight), (double X, double Y), (double R, double G, double B, double Weight)> add = (sum, point) =>
            frame.Sample(point, wrap, sum, static (total, texel, weight) => (total.R + (texel.X * weight), total.G + (texel.Y * weight), total.B + (texel.Z * weight), total.Weight + weight));
        (double width, double height) = (
            MeanLength(Quad.TopLeft, Quad.TopRight, Quad.BottomLeft, Quad.BottomRight),
            MeanLength(Quad.TopLeft, Quad.BottomLeft, Quad.TopRight, Quad.BottomRight));
        (int across, int down) = (Count(width, ColorChart.Columns), Count(height, ColorChart.Rows));
        return ChartSamples.Validated(Chart, gamut, toHashMap(Chart.Cells.Map((cell, index) => (cell.Patch, Mean(Sum(index))))));

        (double R, double G, double B, double Weight) Sum(int cell) {
            (double x, double y) = Center(cell);
            (double R, double G, double B, double Weight) sum = (0d, 0d, 0d, 0d);
            for (int row = 0; row < down; row++)
                for (int column = 0; column < across; column++)
                    sum = image((x + Offset(column, across, width), y + Offset(row, down, height))).Fold(sum, add);
            return sum;
        }
    }

    internal abstract (Func<(double X, double Y), Option<(double X, double Y)>> Map, (WrapMode Across, WrapMode Down) Wrap) ToImage(PixelExtent image);
    private static double Between(Vector3 first, Vector3 second) => double.Atan2(Vector3.Cross(first, second).Length(), Vector3.Dot(first, second));

    private static (double X, double Y) Center(int cell) => (((cell % ColorChart.Columns) + 0.5d) / ColorChart.Columns, ((cell / ColorChart.Columns) + 0.5d) / ColorChart.Rows);

    private static Option<(B TopLeft, B TopRight, B BottomRight, B BottomLeft)> Each<A, B>((A TopLeft, A TopRight, A BottomRight, A BottomLeft) corners, Func<A, Option<B>> map) =>
        from topLeft in map(corners.TopLeft)
        from topRight in map(corners.TopRight)
        from bottomRight in map(corners.BottomRight)
        from bottomLeft in map(corners.BottomLeft)
        select (topLeft, topRight, bottomRight, bottomLeft);

    private ImagingFault Unaligned => new($"chart alignment {Quad} as a rectilinear view", ImagingReason.Degenerate);

    public sealed record Flat(ColorChart Chart, FrameQuad Quad, float Footprint) : ChartPlacement(Chart, Quad, Footprint) {
        public Fin<LatLong> ToLatLong(PixelExtent source) {
            static Fin<PixelExtent> Sized(double width, double height) =>
                PixelExtent.Validated(int.CreateSaturating(width), int.CreateSaturating(height));
            static (double X, double Y) Slope(Vector3 ray) => (ray.X / -(double)ray.Z, ray.Y / -(double)ray.Z);
            static double Spread(double first, double second, double third, double fourth) =>
                double.Max(double.Max(first, second), double.Max(third, fourth)) - double.Min(double.Min(first, second), double.Min(third, fourth));
            Projection.Equirectangular panorama = new(source);
            Matrix<double> map = Quad.Homography;
            return
                from keys in (from left in map.Apply((0d, 0.5d)).Bind(panorama.Ray)
                              from right in map.Apply((1d, 0.5d)).Bind(panorama.Ray)
                              from facing in PanoramaView.Facing(left, right)
                              select facing).ToFin(Unaligned)
                let view = new PanoramaView(Angle.FromRadians(keys.Heading), Angle.FromRadians(keys.Elevation), Angle.FromRadians(keys.Roll))
                let level = Quaternion.Conjugate(view.Rotation)
                from rays in Each((Quad.TopLeft, Quad.TopRight, Quad.BottomRight, Quad.BottomLeft), corner =>
                    panorama.Ray(corner).Map(ray => Vector3.Transform(ray, level)).Filter(static ray => -ray.Z > 0f)).ToFin(Unaligned)
                let slopes = (TopLeft: Slope(rays.TopLeft), TopRight: Slope(rays.TopRight), BottomRight: Slope(rays.BottomRight), BottomLeft: Slope(rays.BottomLeft))
                let reach = double.Max(
                    Spread(slopes.TopLeft.X, slopes.TopRight.X, slopes.BottomRight.X, slopes.BottomLeft.X),
                    (double)ColorChart.Columns / ColorChart.Rows * Spread(slopes.TopLeft.Y, slopes.TopRight.Y, slopes.BottomRight.Y, slopes.BottomLeft.Y))
                let width = double.Max(1d, double.Round(source.Width * reach / double.Pi, MidpointRounding.ToEven))
                from extent in Sized(width, double.Max(1d, double.Round(width * ColorChart.Rows / ColorChart.Columns, MidpointRounding.ToEven)))
                let lens = new Projection.Rectilinear(extent, new Frustum.Perspective(ViewWindow.Centered(2d * reach, extent)))
                from corners in Each(rays, lens.Pixel).ToFin(Unaligned)
                from quad in FrameQuad.Validated(corners.TopLeft, corners.TopRight, corners.BottomRight, corners.BottomLeft)
                select new LatLong(Chart, quad, Footprint, view, lens);
        }

        internal override (Func<(double X, double Y), Option<(double X, double Y)>> Map, (WrapMode Across, WrapMode Down) Wrap) ToImage(PixelExtent image) =>
            (Quad.Homography.Apply, (WrapMode.Black, WrapMode.Black));
    }

    public sealed record LatLong(ColorChart Chart, FrameQuad Quad, float Footprint, PanoramaView View, Projection.Rectilinear Lens)
        : ChartPlacement(Chart, Quad, Footprint) {
        public double Field => 2d * double.Atan(Lens.Frustum.Window.Width / 2d);

        public Fin<Flat> ToFlat(PixelExtent source) {
            (Projection.Equirectangular panorama, Quaternion turn) = (new(source), View.Rotation);
            return
                from column in panorama.Pixel(Vector3.Transform(-Vector3.UnitZ, turn)).Map(static forward => forward.X).ToFin(Unaligned)
                from corners in Each((Quad.TopLeft, Quad.TopRight, Quad.BottomRight, Quad.BottomLeft), corner =>
                    Lens.Reprojected(panorama, turn, corner)
                        .Map(point => (point.X + (source.Width * double.Round((column - point.X) / source.Width, MidpointRounding.ToEven)), point.Y))).ToFin(Unaligned)
                from quad in FrameQuad.Validated(corners.TopLeft, corners.TopRight, corners.BottomRight, corners.BottomLeft)
                select new Flat(Chart, quad, Footprint);
        }

        internal override (Func<(double X, double Y), Option<(double X, double Y)>> Map, (WrapMode Across, WrapMode Down) Wrap) ToImage(PixelExtent image) {
            (Projection.Equirectangular panorama, Matrix<double> view, Quaternion turn) = (new(image), Quad.Homography, View.Rotation);
            Func<(double X, double Y), Option<(double X, double Y)>> source = frame => Lens.Reprojected(panorama, turn, frame);
            return (point => view.Apply(point).Bind(source), panorama.Wrap);
        }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChartFinding {
    public sealed record Reversed : ChartFinding;
    public sealed record Distorted(double Smallest, double Largest) : ChartFinding;
    public sealed record WideView(double Field, double Span) : ChartFinding;
    public sealed record PatchesOutside(Seq<ChartPatch> Patches) : ChartFinding;
}
