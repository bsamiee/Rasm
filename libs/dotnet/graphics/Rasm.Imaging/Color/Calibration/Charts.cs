using System.Drawing;
using System.Numerics;
using Rasm.Imaging.Pixels;
using UnitsNet;
using Wacton.Unicolour;

namespace Rasm.Imaging.Color.Calibration;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class ColorChart {
    public static ColorChart ColorCheckerClassic { get; } = new(6, 21, [
        new(0.1136398927f, 0.09832436105f, 0.047793811f), new(0.3811104477f, 0.336202304f, 0.1852590702f), new(0.1652470004f, 0.1785519348f, 0.2546024121f),
        new(0.1114392339f, 0.1346792679f, 0.05239320311f), new(0.2419823988f, 0.2287175998f, 0.3282104382f), new(0.30451114f, 0.4143554688f, 0.344352688f),
        new(0.4073691399f, 0.3126416159f, 0.05130591012f), new(0.1200518326f, 0.1091090233f, 0.2874447494f), new(0.2915036416f, 0.188999956f, 0.09736350318f),
        new(0.08353888545f, 0.06276662955f, 0.1042075686f), new(0.3427379502f, 0.4331759409f, 0.08330791241f), new(0.4769723742f, 0.4293377578f, 0.06005041429f),
        new(0.06809095613f, 0.05596214063f, 0.2077405936f), new(0.1413517689f, 0.2233437582f, 0.07287461742f), new(0.2143728424f, 0.127800835f, 0.03868150726f),
        new(0.5888922356f, 0.5992976803f, 0.07077420003f), new(0.299122798f, 0.1895114577f, 0.2213469194f), new(0.1247966941f, 0.180609913f, 0.2913392383f),
        new(0.8436985288f, 0.8806903203f, 0.6936778752f), new(0.5665335579f, 0.5899709702f, 0.4828473821f), new(0.3495921991f, 0.3648652066f, 0.3013565492f),
        new(0.1835495863f, 0.1906228754f, 0.1566717383f), new(0.08448968042f, 0.08817234828f, 0.07391630753f), new(0.03042544265f, 0.03151319431f, 0.02656724434f),
    ]);
    public static ColorChart SpyderCheckr24 { get; } = new(6, 21, [
        new(0.3076822512f, 0.4234772396f, 0.3374520529f), new(0.2452835592f, 0.2348654227f, 0.3416765010f), new(0.1037198781f, 0.1279977883f, 0.0523456534f),
        new(0.1692686551f, 0.1858660205f, 0.2666809013f), new(0.3952982368f, 0.3553099128f, 0.1928659515f), new(0.1113998964f, 0.0966807861f, 0.0490883941f),
        new(0.3888763433f, 0.2946518384f, 0.0431458486f), new(0.1172593609f, 0.1098191865f, 0.2771796138f), new(0.2996723782f, 0.1951990129f, 0.1039804505f),
        new(0.0917247784f, 0.0693130081f, 0.1219016829f), new(0.3562702838f, 0.4446769162f, 0.0880911399f), new(0.5038320114f, 0.4391253963f, 0.0603927928f),
        new(0.1208589631f, 0.1795378021f, 0.2854043683f), new(0.2990686680f, 0.1930314394f, 0.2271268085f), new(0.6242483660f, 0.6326192430f, 0.0712084288f),
        new(0.2194131115f, 0.1230703602f, 0.0384412857f), new(0.1438849866f, 0.2259240844f, 0.0741006071f), new(0.0669023563f, 0.0544993983f, 0.2116807502f),
        new(0.8886310787f, 0.9226861820f, 0.7490419119f), new(0.5669540573f, 0.5894253744f, 0.4798162851f), new(0.3432548507f, 0.3572595422f, 0.2897947815f),
        new(0.1838367508f, 0.1913948585f, 0.1556906989f), new(0.0807065939f, 0.0838392766f, 0.0677486804f), new(0.0248551698f, 0.0258980980f, 0.0222499522f),
    ]);

    public int Columns { get; }
    public int Neutral { get; }
    public Seq<Vector3> D50 { get; }

    public int Rows => D50.Count / Columns;

    public static Fin<ColorChart> Validated(int columns, int neutral, Seq<Vector3> d50) =>
        ImagingFault.Unless(
            columns > 0 && !d50.IsEmpty && d50.Count % columns is 0 && neutral >= 0 && neutral < d50.Count && d50.ForAll(static xyz => Vector3.AllWhereAllBitsSet(Vector3.IsFinite(xyz))),
            ImagingReason.Degenerate, (columns, neutral, d50.Count)).Map(_ => new ColorChart(columns, neutral, d50));
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class ChartSamples {
    public ColorChart Chart { get; }
    public Gamut Gamut { get; }
    public Seq<Vector3> Patches { get; }

    public static ChartSamples Reference(ColorChart chart, Gamut gamut) =>
        Gamut.Columns(new(xyzConfig: XyzConfiguration.D50), ColourSpace.Xyz, gamut.Configuration, ColourSpace.RgbLinear) switch {
            var matrix => new(chart, gamut, chart.D50.Map(xyz => Vector3.Transform(xyz, matrix))),
        };

    public static Fin<ChartSamples> Validated(ColorChart chart, Gamut gamut, Seq<Vector3> patches) =>
        ImagingFault.Unless(patches.Count == chart.D50.Count, ImagingReason.Miscounted, patches.Count).Map(_ => new ChartSamples(chart, gamut, patches));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChartPlacement {
    private ChartPlacement(ColorChart chart, FrameQuad quad, float footprint) => (Chart, Quad, Footprint) = (chart, quad, footprint);

    public ColorChart Chart { get; }
    public FrameQuad Quad { get; }
    public float Footprint { get; }

    public Seq<(int Patch, FrameQuad Footprint)> Footprints =>
        (Quad.Mapping(), new Vector2(Footprint / 2f) / new Vector2(Chart.Columns, Chart.Rows)) switch {
            var (map, half) => Patches.Map(patch => Center(patch) switch {
                var center => Mapped([center - half, center + new Vector2(half.X, -half.Y), center + half, center + new Vector2(-half.X, half.Y)], map).Map(quad => (patch, quad)),
            }).Somes(),
        };

    private Seq<int> Patches => toSeq(Enumerable.Range(0, Chart.D50.Count));

    private ImagingFault Unaligned => ImagingFault.Of(ImagingReason.Degenerate, Quad);

    public Seq<ChartFinding> Findings(PixelExtent frame) {
        (double narrowest, double widest) = (double.Pi / 3d, 2d * double.Pi / 3d);
        Seq<Vector2> corners = Corners(Quad);
        (double smallest, double largest) = corners
            .Map((corner, index) => (corners[(index + 3) % 4] - corner, corners[(index + 1) % 4] - corner) switch {
                var (previous, next) => double.Atan2(float.Abs(Vector2.Cross(previous, next)), Vector2.Dot(previous, next)),
            })
            .Fold((Smallest: double.PositiveInfinity, Largest: 0d), static (range, angle) => (double.Min(range.Smallest, angle), double.Max(range.Largest, angle)));
        return Seq(
            Quad.Mirrored ? Some<ChartFinding>(new ChartFinding.Reversed()) : None,
            smallest < narrowest || largest > widest ? Some<ChartFinding>(new ChartFinding.Distorted(Angle.FromRadians(smallest), Angle.FromRadians(largest))) : None,
            Switch(
                (Bounds: frame.Point(Vector2.One), Widest: Angle.FromRadians(widest)),
                flat: static (state, flat) =>
                    Some(flat.Footprints.Filter(footprint => Corners(footprint.Footprint).Exists(point => Vector2.Clamp(point, Vector2.Zero, state.Bounds) != point)).Map(static footprint => footprint.Patch))
                        .Filter(static patches => !patches.IsEmpty).Map(static patches => (ChartFinding)new ChartFinding.PatchesOutside(patches)),
                latLong: static (state, latLong) => latLong.Quad.Mapping() switch {
                    var map =>
                        from left in map(new Vector2(0f, 0.5f)).Bind(latLong.Lens.Ray)
                        from right in map(new Vector2(1f, 0.5f)).Bind(latLong.Lens.Ray)
                        where latLong.Field > state.Widest
                        select (ChartFinding)new ChartFinding.WideView(latLong.Field,
                            Angle.FromRadians(double.Atan2(Vector3.Cross(left.Direction, right.Direction).Length(), Vector3.Dot(left.Direction, right.Direction)))),
                })).Somes();
    }

    public Fin<ChartSamples> Sample(PixelFrame frame, Gamut gamut) {
        (Func<Vector2, Option<Vector2>> image, (WrapMode Across, WrapMode Down) wrap) = ToImage(frame.Extent);
        Seq<Vector2> taps =
            from y in Taps(Side(Quad.TopLeft, Quad.BottomLeft, Quad.TopRight, Quad.BottomRight), Chart.Rows)
            from x in Taps(Side(Quad.TopLeft, Quad.TopRight, Quad.BottomLeft, Quad.BottomRight), Chart.Columns)
            select new Vector2(x, y);
        return Patches.Traverse(patch =>
            taps.Choose(tap => image(Center(patch) + tap).Bind(point => frame.Texel(new Point((int)float.Floor(point.X), (int)float.Floor(point.Y)), wrap)))
                .Fold((Sum: Vector3.Zero, Count: 0), static (mean, texel) => (mean.Sum + texel.AsVector3(), mean.Count + 1)) switch {
                    (_, 0) => Fin.Fail<Vector3>(ImagingFault.Of(ImagingReason.Empty, patch, patch)),
                    var (sum, count) => sum / count,
                }).As().Bind(patches => ChartSamples.Validated(Chart, gamut, patches));
    }

    private protected abstract (Func<Vector2, Option<Vector2>> Map, (WrapMode Across, WrapMode Down) Wrap) ToImage(PixelExtent image);

    private Seq<float> Taps(float length, int cells) =>
        int.Max(1, (int)float.Round(Footprint * length / cells, MidpointRounding.ToEven)) switch {
            var count => toSeq(Enumerable.Range(0, count)).Map(index => (index + 0.5f - (count / 2f)) / length),
        };

    private Vector2 Center(int patch) => (new Vector2(patch % Chart.Columns, patch / Chart.Columns) + new Vector2(0.5f)) / new Vector2(Chart.Columns, Chart.Rows);

    private static float Side(Vector2 from, Vector2 to, Vector2 otherFrom, Vector2 otherTo) => (Vector2.Distance(from, to) + Vector2.Distance(otherFrom, otherTo)) / 2f;

    private static Seq<Vector2> Corners(FrameQuad quad) => [quad.TopLeft, quad.TopRight, quad.BottomRight, quad.BottomLeft];

    private static Option<FrameQuad> Mapped(Seq<Vector2> corners, Func<Vector2, Option<Vector2>> map) =>
        corners.Traverse(corner => map(corner)).As().Bind(static points => Quadrilateral(points).ToOption());

    private static Fin<FrameQuad> Quadrilateral(Seq<Vector2> points) => FrameQuad.Validated(points[0], points[1], points[2], points[3]);

    public sealed record Flat(ColorChart Chart, FrameQuad Quad, float Footprint) : ChartPlacement(Chart, Quad, Footprint) {
        public Fin<LatLong> ToLatLong(PixelExtent source) {
            Projection.Equirectangular panorama = new(source);
            Func<Vector2, Option<Vector2>> map = Quad.Mapping();
            return
                from view in (from left in map(new Vector2(0f, 0.5f)).Bind(panorama.Ray)
                              from right in map(new Vector2(1f, 0.5f)).Bind(panorama.Ray)
                              from facing in PanoramaView.Facing(left.Direction, right.Direction)
                              select facing).ToFin(Unaligned)
                let level = Quaternion.Conjugate(view.Rotation)
                from rays in Corners(Quad).Traverse(corner => panorama.Ray(corner).Map(ray => Vector3.Transform(ray.Direction, level)).Filter(static ray => -ray.Z > 0f)).As().ToFin(Unaligned)
                let slopes = rays.Map(static ray => new Vector2(ray.X, ray.Y) / -ray.Z)
                let reach = float.Max(Spread(slopes.Map(static slope => slope.X)), (float)Chart.Columns / Chart.Rows * Spread(slopes.Map(static slope => slope.Y)))
                let width = float.Max(1f, float.Round(source.Width * reach / float.Pi, MidpointRounding.ToEven))
                from extent in PixelExtent.Validated(int.CreateSaturating(width), int.CreateSaturating(float.Max(1f, float.Round(width * Chart.Rows / Chart.Columns, MidpointRounding.ToEven))))
                from window in ViewWindow.Centered(2f * reach, extent)
                let lens = new Projection.Rectilinear(extent, window)
                from points in rays.Traverse(ray => lens.Pixel(ray).Map(static located => located.Point)).As().ToFin(Unaligned)
                from quad in Quadrilateral(points)
                select new LatLong(Chart, quad, Footprint, view, lens);

            static float Spread(Seq<float> values) => values.Fold(float.NegativeInfinity, float.Max) - values.Fold(float.PositiveInfinity, float.Min);
        }

        public static Fin<Flat> Centered(ColorChart chart, RectangleF bounds, float share, float footprint) {
            Vector2 size = new(bounds.Width, bounds.Height);
            Vector2 chartSize = float.Sqrt(share * size.X * size.Y) * new Vector2(float.Sqrt((float)chart.Columns / chart.Rows), float.Sqrt((float)chart.Rows / chart.Columns));
            Vector2 half = chartSize * float.Min(1f, float.Min(size.X / chartSize.X, size.Y / chartSize.Y)) / 2f;
            Vector2 center = new Vector2(bounds.X, bounds.Y) + (size / 2f);
            return FrameQuad.Validated(center - half, center + new Vector2(half.X, -half.Y), center + half, center + new Vector2(-half.X, half.Y)).Map(quad => new Flat(chart, quad, footprint));
        }

        private protected override (Func<Vector2, Option<Vector2>> Map, (WrapMode Across, WrapMode Down) Wrap) ToImage(PixelExtent image) => (Quad.Mapping(), (WrapMode.Black, WrapMode.Black));
    }

    public sealed record LatLong(ColorChart Chart, FrameQuad Quad, float Footprint, PanoramaView View, Projection.Rectilinear Lens) : ChartPlacement(Chart, Quad, Footprint) {
        public Angle Field => Angle.FromRadians(2d * double.Atan(Lens.Window.Size.X / 2d));

        public Fin<Flat> ToFlat(PixelExtent source) {
            (Projection.Equirectangular panorama, Quaternion turn) = (new(source), View.Rotation);
            return
                from column in panorama.Pixel(Vector3.Transform(-Vector3.UnitZ, turn)).Map(static forward => forward.Point.X).ToFin(Unaligned)
                from points in Corners(Quad).Traverse(corner => Lens.Reprojected(panorama, turn, corner)
                    .Map(point => point with { X = point.X + (source.Width * float.Round((column - point.X) / source.Width, MidpointRounding.ToEven)) })).As().ToFin(Unaligned)
                from quad in Quadrilateral(points)
                select new Flat(Chart, quad, Footprint);
        }

        private protected override (Func<Vector2, Option<Vector2>> Map, (WrapMode Across, WrapMode Down) Wrap) ToImage(PixelExtent image) {
            (Projection.Equirectangular panorama, Func<Vector2, Option<Vector2>> view, Quaternion turn) = (new(image), Quad.Mapping(), View.Rotation);
            return (point => view(point).Bind(frame => Lens.Reprojected(panorama, turn, frame)), panorama.Wrap);
        }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChartFinding {
    public sealed record Reversed : ChartFinding;
    public sealed record Distorted(Angle Smallest, Angle Largest) : ChartFinding;
    public sealed record WideView(Angle Field, Angle Span) : ChartFinding;
    public sealed record PatchesOutside(Seq<int> Patches) : ChartFinding;
}
