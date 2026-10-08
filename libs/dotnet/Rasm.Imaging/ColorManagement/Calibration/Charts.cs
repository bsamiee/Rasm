using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using Rasm.Imaging.Pixels;
using Wacton.Unicolour;

namespace Rasm.Imaging.ColorManagement.Calibration;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError<InvalidColor>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChartPatch {
    public static readonly ChartPatch DarkSkin = new("dark-skin");
    public static readonly ChartPatch LightSkin = new("light-skin");
    public static readonly ChartPatch BlueSky = new("blue-sky");
    public static readonly ChartPatch Foliage = new("foliage");
    public static readonly ChartPatch BlueFlower = new("blue-flower");
    public static readonly ChartPatch BluishGreen = new("bluish-green");
    public static readonly ChartPatch Orange = new("orange");
    public static readonly ChartPatch PurplishBlue = new("purplish-blue");
    public static readonly ChartPatch ModerateRed = new("moderate-red");
    public static readonly ChartPatch Purple = new("purple");
    public static readonly ChartPatch YellowGreen = new("yellow-green");
    public static readonly ChartPatch OrangeYellow = new("orange-yellow");
    public static readonly ChartPatch Blue = new("blue");
    public static readonly ChartPatch Green = new("green");
    public static readonly ChartPatch Red = new("red");
    public static readonly ChartPatch Yellow = new("yellow");
    public static readonly ChartPatch Magenta = new("magenta");
    public static readonly ChartPatch Cyan = new("cyan");
    public static readonly ChartPatch White95 = new("white-9-5");
    public static readonly ChartPatch Neutral8 = new("neutral-8");
    public static readonly ChartPatch Neutral65 = new("neutral-6-5");
    public static readonly ChartPatch Neutral5 = new("neutral-5");
    public static readonly ChartPatch Neutral35 = new("neutral-3-5");
    public static readonly ChartPatch Black2 = new("black-2");
}

[SmartEnum<string>]
[ValidationError<InvalidColor>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColorChart {
    public const int Columns = 6;

    private static readonly Configuration D50 = new(xyzConfig: XyzConfiguration.D50);

    public static readonly ColorChart ColorCheckerClassic = new("color-checker-classic", Seq(
        (ChartPatch.DarkSkin, (0.1136398927, 0.09832436105, 0.047793811)),
        (ChartPatch.LightSkin, (0.3811104477, 0.336202304, 0.1852590702)),
        (ChartPatch.BlueSky, (0.1652470004, 0.1785519348, 0.2546024121)),
        (ChartPatch.Foliage, (0.1114392339, 0.1346792679, 0.05239320311)),
        (ChartPatch.BlueFlower, (0.2419823988, 0.2287175998, 0.3282104382)),
        (ChartPatch.BluishGreen, (0.30451114, 0.4143554688, 0.344352688)),
        (ChartPatch.Orange, (0.4073691399, 0.3126416159, 0.05130591012)),
        (ChartPatch.PurplishBlue, (0.1200518326, 0.1091090233, 0.2874447494)),
        (ChartPatch.ModerateRed, (0.2915036416, 0.188999956, 0.09736350318)),
        (ChartPatch.Purple, (0.08353888545, 0.06276662955, 0.1042075686)),
        (ChartPatch.YellowGreen, (0.3427379502, 0.4331759409, 0.08330791241)),
        (ChartPatch.OrangeYellow, (0.4769723742, 0.4293377578, 0.06005041429)),
        (ChartPatch.Blue, (0.06809095613, 0.05596214063, 0.2077405936)),
        (ChartPatch.Green, (0.1413517689, 0.2233437582, 0.07287461742)),
        (ChartPatch.Red, (0.2143728424, 0.127800835, 0.03868150726)),
        (ChartPatch.Yellow, (0.5888922356, 0.5992976803, 0.07077420003)),
        (ChartPatch.Magenta, (0.299122798, 0.1895114577, 0.2213469194)),
        (ChartPatch.Cyan, (0.1247966941, 0.180609913, 0.2913392383)),
        (ChartPatch.White95, (0.8436985288, 0.8806903203, 0.6936778752)),
        (ChartPatch.Neutral8, (0.5665335579, 0.5899709702, 0.4828473821)),
        (ChartPatch.Neutral65, (0.3495921991, 0.3648652066, 0.3013565492)),
        (ChartPatch.Neutral5, (0.1835495863, 0.1906228754, 0.1566717383)),
        (ChartPatch.Neutral35, (0.08448968042, 0.08817234828, 0.07391630753)),
        (ChartPatch.Black2, (0.03042544265, 0.03151319431, 0.02656724434))));
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

    public static (Vector2 TopLeft, Vector2 TopRight, Vector2 BottomRight, Vector2 BottomLeft) Centered(RectangleF bounds) =>
        (float.Sqrt(0.1f * bounds.Width * bounds.Height * Columns / Rows), float.Sqrt(0.1f * bounds.Width * bounds.Height * Rows / Columns)) switch {
            var (width, height) => (new Vector2(width, height) * float.Min(1f, float.Min(bounds.Width / width, bounds.Height / height)) / 2f, new Vector2(bounds.X + (bounds.Width / 2f), bounds.Y + (bounds.Height / 2f))) switch {
                var (half, centre) => (centre - half, centre + new Vector2(half.X, -half.Y), centre + half, centre + new Vector2(-half.X, half.Y)),
            },
        };
}

[ComplexValueObject]
[ValidationError<InvalidColor>]
public readonly partial struct PatchSample {
    public double R { get; }
    public double G { get; }
    public double B { get; }

    static partial void ValidateFactoryArguments(ref InvalidColor? validationError, ref double r, ref double g, ref double b) =>
        validationError = double.IsFinite(r) && double.IsFinite(g) && double.IsFinite(b) ? null : new InvalidColor();
}

[ComplexValueObject]
[ValidationError<InvalidColor>]
public sealed partial class ChartSamples {
    public ColorChart Chart { get; }
    public Gamut Gamut { get; }
    public HashMap<ChartPatch, PatchSample> Patches { get; }

    internal HashMap<ChartPatch, (double R, double G, double B)> Colors => Patches.Map(static sample => (sample.R, sample.G, sample.B));

    static partial void ValidateFactoryArguments(ref InvalidColor? validationError, ref ColorChart chart, ref Gamut gamut, ref HashMap<ChartPatch, PatchSample> patches) =>
        validationError = patches.Count == ChartPatch.Items.Count ? null : new InvalidColor();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None,
    MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidColor>]
public readonly partial struct FootprintShare : IMinMaxValue<FootprintShare> {
    public static FootprintShare MinValue { get; } = new(float.BitIncrement(0f));
    public static FootprintShare MaxValue { get; } = new(1f);
    public static FootprintShare Central { get; } = new(0.8f);

    static partial void ValidateFactoryArguments(ref InvalidColor? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidColor();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChartPlacement {
    private ChartPlacement(ColorChart chart, FrameQuad quad, FootprintShare footprint) => (Chart, Quad, Footprint) = (chart, quad, footprint);

    public ColorChart Chart { get; }
    public FrameQuad Quad { get; }
    public FootprintShare Footprint { get; }

    public Seq<(ChartPatch Patch, Vector2 TopLeft, Vector2 TopRight, Vector2 BottomRight, Vector2 BottomLeft)> Footprints =>
        (Quad.Homography, new Vector2(Footprint / 2f / ColorChart.Columns, Footprint / 2f / ColorChart.Rows)) switch {
            var (map, half) => Chart.Cells.Map((cell, index) => Centre(index) switch {
                var centre => Each((centre - half, centre + new Vector2(half.X, -half.Y), centre + half, centre + new Vector2(-half.X, half.Y)), map.Apply)
                    .Map(corners => (cell.Patch, corners.TopLeft, corners.TopRight, corners.BottomRight, corners.BottomLeft)),
            }).Somes(),
        };

    public Seq<ChartFinding> Findings(PixelExtent frame) =>
        (double.Pi / 3d, 2d * double.Pi / 3d) switch {
            var (narrowest, widest) => Seq(
                Quad.Mirrored ? Some<ChartFinding>(new ChartFinding.Reversed()) : None,
                Seq((Quad.BottomLeft, Quad.TopLeft, Quad.TopRight), (Quad.TopLeft, Quad.TopRight, Quad.BottomRight), (Quad.TopRight, Quad.BottomRight, Quad.BottomLeft), (Quad.BottomRight, Quad.BottomLeft, Quad.TopLeft))
                    .Map(static turn => turn switch { var (previous, corner, next) => Between(new Vector3(previous - corner, 0f), new Vector3(next - corner, 0f)) })
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
                    latLong: static (state, latLong) =>
                        from left in latLong.Quad.Homography.Apply(new Vector2(0f, 0.5f)).Bind(latLong.Lens.Ray)
                        from right in latLong.Quad.Homography.Apply(new Vector2(1f, 0.5f)).Bind(latLong.Lens.Ray)
                        where latLong.Field > state.Widest
                        select (ChartFinding)new ChartFinding.WideView(latLong.Field, Between(left, right)))).Somes(),
        };

    public Fin<ChartSamples> Sample(PixelFrame frame, Gamut gamut) {
        static double Length(Vector2 from, Vector2 to) => double.Hypot((double)to.X - from.X, (double)to.Y - from.Y);
        static Validation<Error, (ChartPatch Patch, PatchSample Sample)> Mean(ChartPatch patch, Vector256<double> sum) =>
            (sum[3] > 0d ? sum / sum[3] : sum) switch {
                var mean => PatchSample.Validate(mean[0], mean[1], mean[2], out PatchSample sample) is { } error ? error : (patch, sample),
            };
        Memory2D<Vector4> texels = frame.View;
        (Func<Vector2, Option<Vector2>> image, (WrapMode Across, WrapMode Down) wrap) = ToImage(frame.Extent);
        Func<Vector256<double>, Vector2, Vector256<double>> add = (sum, point) => Bilinear(texels, wrap, sum, point);
        (double width, double height) = (
            (Length(Quad.TopLeft, Quad.TopRight) + Length(Quad.BottomLeft, Quad.BottomRight)) / 2d,
            (Length(Quad.TopLeft, Quad.BottomLeft) + Length(Quad.TopRight, Quad.BottomRight)) / 2d);
        (int across, int down) = (
            int.Max(1, (int)Math.Round(Footprint * width / ColorChart.Columns, MidpointRounding.ToEven)),
            int.Max(1, (int)Math.Round(Footprint * height / ColorChart.Rows, MidpointRounding.ToEven)));
        return Chart.Cells.Map((cell, index) => (cell.Patch, Sum: Enumerable.Range(0, across * down).Aggregate(
                Vector256<double>.Zero,
                (sum, point) => image(Centre(index) + new Vector2(
                    (float)(((point % across) + 0.5d - (across / 2d)) / width),
                    (float)(((point / across) + 0.5d - (down / 2d)) / height))).Fold(sum, add))))
            .Traverse(static cell => Mean(cell.Patch, cell.Sum))
            .As()
            .ToFin()
            .Bind(Fin<ChartSamples> (patches) => ChartSamples.Validate(Chart, gamut, toHashMap(patches), out ChartSamples? samples) is { } error ? error : samples!);
    }

    internal abstract (Func<Vector2, Option<Vector2>> Map, (WrapMode Across, WrapMode Down) Wrap) ToImage(PixelExtent image);

    private static Vector256<double> Bilinear(Memory2D<Vector4> view, (WrapMode Across, WrapMode Down) wrap, Vector256<double> sum, Vector2 point) {
        (double x, double y) = (point.X - 0.5d, point.Y - 0.5d);
        (int left, int top) = ((int)Math.Floor(x), (int)Math.Floor(y));
        (double across, double down) = (x - left, y - top);
        Vector256<double> Add(Vector256<double> total, int row, int column, double weight) =>
            (wrap.Down.Index(row, view.Height), wrap.Across.Index(column, view.Width)) switch {
                ( { IsSome: true } wrappedRow, { IsSome: true } wrappedColumn) => view.Span[(int)wrappedRow, (int)wrappedColumn] switch {
                    var texel => total + (Vector256.Create(texel.X, texel.Y, texel.Z, 1d) * weight),
                },
                _ => total,
            };
        return Add(Add(Add(Add(sum, top, left, (1d - across) * (1d - down)), top, left + 1, across * (1d - down)), top + 1, left, (1d - across) * down), top + 1, left + 1, across * down);
    }

    private static Vector2 Centre(int cell) => new(((cell % ColorChart.Columns) + 0.5f) / ColorChart.Columns, ((cell / ColorChart.Columns) + 0.5f) / ColorChart.Rows);
    private static double Between(Vector3 first, Vector3 second) => double.Atan2(Vector3.Cross(first, second).Length(), Vector3.Dot(first, second));

    private static Option<(B TopLeft, B TopRight, B BottomRight, B BottomLeft)> Each<A, B>((A TopLeft, A TopRight, A BottomRight, A BottomLeft) corners, Func<A, Option<B>> map) =>
        from topLeft in map(corners.TopLeft)
        from topRight in map(corners.TopRight)
        from bottomRight in map(corners.BottomRight)
        from bottomLeft in map(corners.BottomLeft)
        select (topLeft, topRight, bottomRight, bottomLeft);

    private static Fin<FrameQuad> Placed((Vector2 TopLeft, Vector2 TopRight, Vector2 BottomRight, Vector2 BottomLeft) corners) =>
        FrameQuad.Validate(corners.TopLeft, corners.TopRight, corners.BottomRight, corners.BottomLeft, out FrameQuad? quad) is { } error ? error : quad!;

    public sealed record Flat(ColorChart Chart, FrameQuad Quad, FootprintShare Footprint) : ChartPlacement(Chart, Quad, Footprint) {
        public Fin<LatLong> ToLatLong(PixelExtent source) {
            static Validation<Error, T> Crossed<T>(float key) where T : IObjectFactory<T, float, InvalidPixelValue> =>
                T.Validate(key, provider: null, out T? item) is { } error ? error : item!;
            static Validation<Error, PixelExtent> Sized(int width, int height) => PixelExtent.Validate(width, height, out PixelExtent extent) is { } error ? error : extent;
            static Vector2 Slope(Vector3 ray) => new Vector2(ray.X, ray.Y) / -ray.Z;
            static float Spread(float first, float second, float third, float fourth) =>
                float.Max(float.Max(first, second), float.Max(third, fourth)) - float.Min(float.Min(first, second), float.Min(third, fourth));
            Projection.Equirectangular panorama = new(source);
            Homography map = Quad.Homography;
            return
                from keys in (from left in map.Apply(new Vector2(0f, 0.5f)).Bind(panorama.Ray)
                              from right in map.Apply(new Vector2(1f, 0.5f)).Bind(panorama.Ray)
                              from facing in PanoramaView.Facing(left, right)
                              select facing).ToFin(new ViewUndefined(Quad))
                from view in (Crossed<SignedAngle>(keys.Heading), Crossed<Elevation>(keys.Elevation), Crossed<SignedAngle>(keys.Roll))
                    .Apply(static (heading, elevation, roll) => new PanoramaView(heading, elevation, roll)).As().ToFin()
                let level = Quaternion.Conjugate(view.Rotation)
                from rays in Each((Quad.TopLeft, Quad.TopRight, Quad.BottomRight, Quad.BottomLeft), corner =>
                    panorama.Ray(corner).Map(ray => Vector3.Transform(ray, level)).Filter(static ray => -ray.Z > 0f)).ToFin(new ViewUndefined(Quad))
                let slopes = (TopLeft: Slope(rays.TopLeft), TopRight: Slope(rays.TopRight), BottomRight: Slope(rays.BottomRight), BottomLeft: Slope(rays.BottomLeft))
                let reach = float.Max(
                    Spread(slopes.TopLeft.X, slopes.TopRight.X, slopes.BottomRight.X, slopes.BottomLeft.X),
                    (float)ColorChart.Columns / ColorChart.Rows * Spread(slopes.TopLeft.Y, slopes.TopRight.Y, slopes.BottomRight.Y, slopes.BottomLeft.Y))
                let width = int.Max(1, (int)float.Round(source.Width * reach / float.Pi, MidpointRounding.ToEven))
                from lens in (Crossed<FieldOfView>(2f * float.Atan(reach)), Sized(width, int.Max(1, (int)float.Round(width * (float)ColorChart.Rows / ColorChart.Columns, MidpointRounding.ToEven))))
                    .Apply(static (field, extent) => new Projection.Rectilinear(extent, new Frustum.Perspective(ViewWindow.Centered(field, extent)))).As().ToFin()
                from corners in Each(rays, lens.Pixel).ToFin(new ViewUndefined(Quad))
                from quad in Placed(corners)
                select new LatLong(Chart, quad, Footprint, view, lens);
        }

        internal override (Func<Vector2, Option<Vector2>> Map, (WrapMode Across, WrapMode Down) Wrap) ToImage(PixelExtent image) =>
            (Quad.Homography.Apply, (WrapMode.Black, WrapMode.Black));
    }

    public sealed record LatLong(ColorChart Chart, FrameQuad Quad, FootprintShare Footprint, PanoramaView View, Projection.Rectilinear Lens)
        : ChartPlacement(Chart, Quad, Footprint) {
        public double Field => 2d * double.Atan(Lens.Frustum.Window.Right);

        public Fin<Flat> ToFlat(PixelExtent source) {
            (Projection.Equirectangular panorama, Quaternion turn) = (new(source), View.Rotation);
            return
                from column in panorama.Pixel(Vector3.Transform(-Vector3.UnitZ, turn)).Map(static forward => forward.X).ToFin(new ViewUndefined(Quad))
                from corners in Each((Quad.TopLeft, Quad.TopRight, Quad.BottomRight, Quad.BottomLeft), corner =>
                    Lens.Reprojected(panorama, turn, corner)
                        .Map(point => point with { X = point.X + (source.Width * float.Round((column - point.X) / source.Width, MidpointRounding.ToEven)) })).ToFin(new ViewUndefined(Quad))
                from quad in Placed(corners)
                select new Flat(Chart, quad, Footprint);
        }

        internal override (Func<Vector2, Option<Vector2>> Map, (WrapMode Across, WrapMode Down) Wrap) ToImage(PixelExtent image) {
            (Projection.Equirectangular panorama, Homography view, Quaternion turn) = (new(image), Quad.Homography, View.Rotation);
            Func<Vector2, Option<Vector2>> source = frame => Lens.Reprojected(panorama, turn, frame);
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
