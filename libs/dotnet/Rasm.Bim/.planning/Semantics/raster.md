# [BIM_GEORASTER]

`GeoRaster` is the GDAL raster ingest owner over `MaxRev.Gdal.Core`: a windowed multi-band `Dataset.ReadRaster` stack placed in georeferenced space by the six-coefficient affine RE-ANCHORED to the pixel window and resample ratio, its NTS extent folded off that affine's own corners, every band's full schema — storage type, display role, nodata sentinel, linear decode, statistics, histogram, validity mask, and palette legend — lowered ONCE at ingest, and the GDAL overview pyramid a COG or tiled DEM carries read as the level run it is. `GeoRaster.ToCoverage` lowers the placed MULTI-RESOLUTION raster onto a `Rasm.Element/Geospatial/coverage#COVERAGE_NODE` `CoverageGrid` by content-key reference, never a stored pixel blob on the node and never a single-resolution descriptor that strands the pyramid.

Every measurement a band carries rides the `Rasm/Domain/validation#VERDICT_CARRIERS` `Evidence<T>` carrier, so a statistic GDAL refused to compute, a histogram the source never cached, and a mask an all-valid raster genuinely has none of read as three distinct facts rather than one absence. The placement is the kernel `Rasm.Numerics` `CellLattice` and the storage vocabulary the kernel `Rasm.Drawing` `ChannelDtype` roster; the `Contour`, `Cog`, `Warp`, and `DemProcess` derive legs all run inside the ONE `Semantics/feature#GEO_BOUNDARY` `GeoGdal` bracket.

## [01]-[INDEX]

- [02]-[RASTER_TILE]: `RasterBand` the typed pixel stack, `RasterTile` the placed windowed carrier with its validity mask, `RasterOverview` the level row, `RasterBandInfo` the per-band schema with its evidence columns, `DemMode` the DEM-derivation roster.
- [03]-[RASTER_INGEST]: `GeoRaster` the ingest, the shared coverage projection, and the contour/COG/warp/DEM derive legs.

## [02]-[RASTER_TILE]

- Owner: `RasterBand` the `[Union]` pixel buffer typed by the source `Band.DataType`; `RasterTile` the windowed placed carrier — the band stack, the full six-coefficient geo-transform, the NTS extent, the per-band schema, the base tile dims, the overview pyramid, the dataset's own CRS, and the validity mask; `RasterOverview` one pyramid level's schema; `RasterBandInfo` the per-band GDAL schema; `BandStat`/`BandHistogram`/`MaskSource` the three measurement carriers; `DemMode` the bounded DEM-derivation vocabulary.
- Cases: `RasterBand` arms `Floats` (Float16/32 DEM), `Doubles` (Float64 survey-grade DEM and the 64-bit integer widths), `Bytes` (ortho), `Ints` (the 16/32-bit classification widths); `MaskSource` rows `AllValid`, `PerDataset`, `Alpha`, `NoData` — GDAL's mask flags are a SET, so a mask states every source that produced it.
- Law: a band is FULLY self-describing, so a `Range`-less band and a `Palette`-role band with an EMPTY colour table are the deleted forms the shared `CoverageBand` contract forbids; `Doubles` carries the 64-bit integer widths because GDAL's `ReadRaster` overload family is `byte[]`/`short[]`/`int[]`/`float[]`/`double[]` with NO `long[]` member, so double is the widest EXACT carrier available and an Int64/UInt64 raster reads integer-true up to 2^53 — stated, because past that the read is lossy and a consumer of a full-range 64-bit coverage must know which side of that bound it is on.
- Entry: `RasterTile.Plane<T>(band, floats, doubles, bytes, ints)` is the ONLY pixel read — zero-copy per band at the TRUE pixel type; `RasterTile.Valid(col, row)` reads the validity mask; `DemMode.Key` IS the gdal mode token.
- Auto: `Tessellates`-style derivation has no place here, but `Valid` DERIVES from the mask evidence so an all-valid raster answers true without a plane to read; `MaskSource.Of(flags)` decodes GDAL's raw flag word through the roster rather than four hand tests.
- Output: `RasterTile` is the placed pixel evidence a terrain-mesh tessellation reads; each `RasterBandInfo` carries its statistics, histogram, and mask as `Evidence`, so a display-normalization consumer can tell "the source declares no statistics" from "the statistics scan refused" and never reads a fabricated zero for either.
- Packages: `MaxRev.Gdal.Core`, `CommunityToolkit.HighPerformance`, `NetTopologySuite`, `Rasm.Element`, `Rasm`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`
- Growth: a new pixel width is one `SampleRows` row over a kernel `ChannelDtype` the roster already carries; a new resolution tier is one `RasterOverview` row off the existing `GetOverviewCount` fold; a new band attribute is one `RasterBandInfo` column lowered to one `CoverageBand` column; a new DEM derivation is one `DemMode` row carrying its own gdal token; never a per-format raster reader and never a package-local sample-type enum beside the kernel roster.
- Boundary: `SampleAt` was an erased-to-float convenience with no reader that silently narrowed a survey-grade `Float64` DEM and an `Int64` classification raster alike — the typed `Plane` continuation is the pixel read and an erased one beside it is the deleted form; `OSGeo.GDAL.*` types stay confined to `RasterBandInfo` and the `GeoRaster` owner and never cross to the graph node; every `ColorTable`/`RasterAttributeTable`/`ColorEntry` SWIG handle is read under `using` and only the lowered `ColorBin` rows cross; the DEM mode roster carries its own gdal token as the row KEY, so the lowering is a key read rather than a `ToString().ToLowerInvariant()` that couples the wire token to a C# identifier's casing; colour relief carries no row because its `wrapper_GDALDEMProcessing` arm also takes a colour-file argument no row can hold.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using CommunityToolkit.HighPerformance;
using LanguageExt;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Rasm;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Numerics;
using Rasm.Element.Geospatial;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Thinktecture;
using static LanguageExt.Prelude;
using LatticeAxis = Rasm.Numerics.Dimension;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------
public sealed record BandStat(double Min, double Max, double Mean, double StdDev);

public sealed record BandHistogram(double Min, double Max, Seq<int> Buckets);

[SmartEnum<int>]
public sealed partial class MaskSource {
    public static readonly MaskSource AllValid = new(0x01);
    public static readonly MaskSource PerDataset = new(0x02);
    public static readonly MaskSource Alpha = new(0x04);
    public static readonly MaskSource NoData = new(0x08);

    public static Seq<MaskSource> Of(int flags) => toSeq(Items).Filter(row => (flags & row.Key) != 0);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class DemMode {
    public static readonly DemMode Hillshade = new("hillshade");
    public static readonly DemMode Slope = new("slope");
    public static readonly DemMode Aspect = new("aspect");
}

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public partial record RasterBand {
    partial record Floats(float[] Samples);
    partial record Doubles(double[] Samples);
    partial record Bytes(byte[] Samples);
    partial record Ints(int[] Samples);

    public int Length => Switch(
        floats:  static b => b.Samples.Length,
        doubles: static b => b.Samples.Length,
        bytes:   static b => b.Samples.Length,
        ints:    static b => b.Samples.Length);
}

public sealed record RasterTile(
    RasterBand Band,
    int Width,
    int Height,
    double[] GeoTransform,
    Envelope Extent,
    Seq<RasterBandInfo> Bands,
    Seq<RasterOverview> Overviews,
    int BaseBlockX,
    int BaseBlockY,
    Option<ProjectedCrs> SourceCrs,
    Evidence<RasterBand> Mask) {

    public T Plane<T>(int band, Func<ReadOnlyMemory2D<float>, T> floats, Func<ReadOnlyMemory2D<double>, T> doubles, Func<ReadOnlyMemory2D<byte>, T> bytes, Func<ReadOnlyMemory2D<int>, T> ints) =>
        Band.Switch(
            floats:  s => floats(s.Samples.AsMemory().AsMemory2D(band * Width * Height, Height, Width, 0)),
            doubles: s => doubles(s.Samples.AsMemory().AsMemory2D(band * Width * Height, Height, Width, 0)),
            bytes:   s => bytes(s.Samples.AsMemory().AsMemory2D(band * Width * Height, Height, Width, 0)),
            ints:    s => ints(s.Samples.AsMemory().AsMemory2D(band * Width * Height, Height, Width, 0)));

    public bool Valid(int col, int row) =>
        Mask.Switch(
            measured: plane => plane.Value.Switch(
                floats:  s => s.Samples[(row * Width) + col] != 0f,
                doubles: s => s.Samples[(row * Width) + col] != 0d,
                bytes:   s => s.Samples[(row * Width) + col] != 0,
                ints:    s => s.Samples[(row * Width) + col] != 0),
            refused: static _ => false,
            absent: static _ => true);
}

public sealed record RasterOverview(
    int Level,
    int Width,
    int Height,
    double CellSize,
    int BlockX,
    int BlockY);

public sealed record RasterBandInfo(
    int Index,
    OSGeo.GDAL.DataType DataType,
    OSGeo.GDAL.ColorInterp ColorInterp,
    Option<double> NoData,
    string Units,
    double Offset,
    double Scale,
    Evidence<BandStat> Range,
    Evidence<BandHistogram> Histogram,
    Seq<MaskSource> Mask,
    Seq<ColorBin> Palette);
```

## [03]-[RASTER_INGEST]

- Owner: `GeoRaster` the GDAL raster ingest, the shared `Coverage`-node projection, and the contour/COG/warp/DEM derive legs; `BufferWidth` the closed read-width roster the stack read dispatches through; `SampleRows` the ONE GDAL-`DataType` roster carrying both the kernel storage row and the buffer width.
- Law: pixels move through the PER-TYPE `Dataset.ReadRaster` overload family into a managed array matching the `Band.DataType` — a generic `<T>` call binds no overload on that SWIG surface — and the returned `CPLErr` gates the buffer before it becomes a band, because GDAL populates a read buffer BEFORE it reports failure and a discarded status publishes a zero-filled stack as pixel evidence; the abort grain is DECLARED, not assumed — GDAL publishes no interrupt across `Gdal.Open` or a windowed `ReadRaster`, so the token gates the ONE managed boundary this leg owns.
- Entry: `Read(bytes, window, targetWidth, targetHeight, token)` opens the raster and reads the windowed band stack, every per-band schema, the mask, and the pyramid ONCE; `ToCoverage(tile, reference, field, overviewKey, ctx)` lands the shared `Node.Coverage`; `Contour(demBytes, interval)` vectorizes, `Cog(bytes)` transcodes, `Warp(bytes, target)` reprojects, and `DemProcess(demBytes, mode)` derives hillshade/slope/aspect.
- Auto: `Read` re-anchors the `GetGeoTransform` affine to the pixel window and resample ratio and folds the NTS extent off THIS affine's four corners (rotation honored), because stamping the SOURCE affine on a windowed or resampled buffer silently mislocates the tile; `ToCoverage` lowers the geo-transform onto the kernel `CellLattice` through `Placement.Build`'s `PointBasisMap` (the two rotation terms riding the affine's off-diagonal and the SIGNED pixel height preserved, so a north-up raster's negative Y scale is ordinary), derives every pyramid level as the base grid's `Coarsen` step, and lands a NON-ROOTED `Node.Coverage` whose `NodeId` is CONTENT-hashed over its own canonical bytes.
- Output: the shared `Coverage` node is the by-reference field the terrain consumer and the `Exchange/export` 3D-Tiles terrain leg read — its `OverviewLevel` run letting a `Rasm.Compute` working-resolution route pick a level by `LevelFor`, size the fetch by `ByteLength(level)`, and read that level's bytes by its own `BlobKey` rather than the full base raster; the contour `GeoFeature` lines are the vectorized terrain the site model indexes.
- Packages: `MaxRev.Gdal.Core`, `MaxRev.Gdal.MacosRuntime.Minimal.arm64`, `NetTopologySuite`, `ProjNET`, `CommunityToolkit.HighPerformance`, `Rasm.Element`, `Rasm`, `LanguageExt.Core`
- Growth: a new raster format is enumerable through the one `Gdal.Open` universal driver path with zero new code; a new resample kernel is one `RasterIOExtraArg`; a new derive leg is one `GeoGdal.Derive` call over the existing bracket; never a per-format raster reader, never an inlined pixel blob on the node, and never a `Palette`-role band with no colour table behind it.
- Boundary: the placement is the kernel `Rasm.Numerics` `CellLattice` and nothing else — a package-local six-coefficient descriptor, a north-up sign assumption, or a forward-only map with no inverse is the deleted form, and the storage vocabulary is the kernel `Rasm.Drawing` `ChannelDtype` roster, so a package-local sample-type enum beside it is the deleted form; building that placement is the ONE site where the kernel's host-typed affine primitives cross into this owner, spelled qualified and confined to `Grid` so the crossing stays countable — host GEOMETRY remains the banned form, and the distinction is that the kernel grid IS the contract's admitted placement; grid degeneracy is NOT gated here because `CellLattice.Of` owns invertibility and the cell budget behind a PRIVATE constructor, so a zero-determinant re-check is a second admission authority for one fact; a coverage is MULTI-RESOLUTION so `ToCoverage` reads the pyramid and content-keys each level, the run's HEAD being the base, and the pyramid is the base grid's `Coarsen` CHAIN the projector derives rather than transcribes — a source pyramid off the `Coarsen` chain — odd axes ceiling-half, terminal axes remain unchanged, and a three-dimensional chain retains at least two layers — faults at the level that broke the chain instead of seating an affine its bytes do not match, and a ZERO-EXTENT level faults at its own index because substituting a unit decimation ratio hands that level the BASE cell size and dropping the row gaps the chain the same gate walks; a caller window DISJOINT from the raster refuses by name, the retired one-pixel clamp having published a tile with a real affine, a real extent, and one arbitrary pixel the window never covered; a tile that states a CRS and a reference that states a DIFFERENT one is a placement contradiction and refuses, because admitting it content-keys a wrong `Coverage` as a valid distinct node — a genuine frame difference is the caller's `Warp` beside `Cog` and `DemProcess`, never a reprojection inside this projection; the DEM-to-vector legs carry the DEM's own `GetProjectionRef` frame onto every derived feature through the ONE `SourceFrame` read the ingest also takes, because a blank `SourceCrs` makes the datum leg short-circuit and every contour lands unshifted on a target the caller believes it reprojected onto; reprojection inside a GDAL pipeline uses OSR while managed-geometry reprojection stays the `ProjNET` leg; the tile-pyramid PARTITIONING stays at `Rasm.Compute` — `Rasm.Bim` AUTHORS the COG/contour and READS the existing GDAL overview pyramid.

```csharp
// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
sealed partial class BufferWidth {
    public static readonly BufferWidth Bytes = new("bytes");
    public static readonly BufferWidth Ints = new("ints");
    public static readonly BufferWidth Floats = new("floats");
    public static readonly BufferWidth Doubles = new("doubles");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GeoRaster {
    static readonly Map<OSGeo.GDAL.DataType, (ChannelDtype Storage, BufferWidth Width)> SampleRows = Map(
        (OSGeo.GDAL.DataType.GDT_Byte,     (ChannelDtype.UInt8,    BufferWidth.Bytes)),
        (OSGeo.GDAL.DataType.GDT_Int8,     (ChannelDtype.Int8,     BufferWidth.Bytes)),
        (OSGeo.GDAL.DataType.GDT_UInt16,   (ChannelDtype.UInt16,   BufferWidth.Ints)),
        (OSGeo.GDAL.DataType.GDT_Int16,    (ChannelDtype.Int16,    BufferWidth.Ints)),
        (OSGeo.GDAL.DataType.GDT_UInt32,   (ChannelDtype.UInt32,   BufferWidth.Ints)),
        (OSGeo.GDAL.DataType.GDT_Int32,    (ChannelDtype.Int32,    BufferWidth.Ints)),
        (OSGeo.GDAL.DataType.GDT_UInt64,   (ChannelDtype.UInt64,   BufferWidth.Doubles)),
        (OSGeo.GDAL.DataType.GDT_Int64,    (ChannelDtype.Int64,    BufferWidth.Doubles)),
        (OSGeo.GDAL.DataType.GDT_Float16,  (ChannelDtype.Float16,  BufferWidth.Floats)),
        (OSGeo.GDAL.DataType.GDT_Float32,  (ChannelDtype.Float32,  BufferWidth.Floats)),
        (OSGeo.GDAL.DataType.GDT_Float64,  (ChannelDtype.Float64,  BufferWidth.Doubles)),
        (OSGeo.GDAL.DataType.GDT_CInt16,   (ChannelDtype.CInt16,   BufferWidth.Ints)),
        (OSGeo.GDAL.DataType.GDT_CInt32,   (ChannelDtype.CInt32,   BufferWidth.Ints)),
        (OSGeo.GDAL.DataType.GDT_CFloat32, (ChannelDtype.CFloat32, BufferWidth.Floats)),
        (OSGeo.GDAL.DataType.GDT_CFloat64, (ChannelDtype.CFloat64, BufferWidth.Doubles)));

    public static Fin<RasterTile> Read(ReadOnlyMemory<byte> bytes, Option<Envelope> window, int targetWidth, int targetHeight, CancellationToken token) =>
        token.IsCancellationRequested
        ? Fin.Fail<RasterTile>(Errors.Cancelled)
        : GeoGdal.Raster(bytes, ".tif", dataset => {
            var transform = new double[6];
            dataset.GetGeoTransform(transform);
            var (xOff, yOff, xSize, ySize) = Pixels(window, transform, dataset.RasterXSize, dataset.RasterYSize);
            if (xSize <= 0 || ySize <= 0) {
                return Fin.Fail<RasterTile>(new BimFault.Refused(BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "raster-window-disjoint", xOff.ToString(CultureInfo.InvariantCulture), yOff.ToString(CultureInfo.InvariantCulture) })));
            }
            var (rx, ry) = ((double)xSize / targetWidth, (double)ySize / targetHeight);
            double[] gt = [
                transform[0] + (xOff * transform[1]) + (yOff * transform[2]), transform[1] * rx, transform[2] * ry,
                transform[3] + (xOff * transform[4]) + (yOff * transform[5]), transform[4] * rx, transform[5] * ry];
            (double X, double Y) Corner(double c, double r) => (gt[0] + (c * gt[1]) + (r * gt[2]), gt[3] + (c * gt[4]) + (r * gt[5]));
            var extent = new Envelope();
            Span<(double X, double Y)> corners = [Corner(0, 0), Corner(targetWidth, 0), Corner(0, targetHeight), Corner(targetWidth, targetHeight)];
            foreach (var (cx, cy) in corners) { extent.ExpandToInclude(cx, cy); }
            int bands = dataset.RasterCount;
            var bandMap = Enumerable.Range(1, bands).ToArray();
            using var first = dataset.GetRasterBand(1);
            Seq<RasterBandInfo> schema = Enumerable.Range(1, bands).AsIterable().Map(b => BandInfo(dataset.GetRasterBand(b), b - 1)).ToSeq();
            first.GetBlockSize(out int baseBlockX, out int baseBlockY);
            double baseCell = Math.Sqrt(Math.Abs((transform[1] * transform[5]) - (transform[2] * transform[4])));
            Option<ProjectedCrs> sourceCrs = SourceFrame(dataset.GetProjectionRef());
            return from band in Materialize(dataset, first.DataType, xOff, yOff, xSize, ySize, targetWidth, targetHeight, bands, bandMap)
                   from overviews in Overviews(first, dataset.RasterXSize, baseCell)
                   select new RasterTile(band, targetWidth, targetHeight, gt, extent, schema, overviews,
                       baseBlockX, baseBlockY, sourceCrs, Mask(first, xOff, yOff, xSize, ySize, targetWidth, targetHeight));
        }, "raster-read");

    static Option<ProjectedCrs> SourceFrame(string wkt) =>
        wkt.Length == 0
            ? Option<ProjectedCrs>.None
            : ProjectedCrs.Of("", "", "", wkt).ToOption();

    // --- [COVERAGE_PROJECTION]
    public static Fin<Node.Coverage> ToCoverage(
        RasterTile tile, GeoReference reference, ArtifactContent raster,
        Func<int, ArtifactContent> overview, ProjectionContext ctx) =>
        tile.SourceCrs.Match(
            None: () => Fin.Succ(unit),
            Some: source => reference.Crs.Match(
                None: () => Fin.Succ(unit),
                Some: target => source == target
                    ? Fin.Succ(unit)
                    : Fin.Fail<Unit>(new BimFault.Refused(ctx.Key, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "raster-frame-mismatch", source.Name, target.Name })))))
        .Bind(_ => Grid(tile, ctx.Key))
        .Bind(basis =>
            from bands in tile.Bands.Traverse(info => Sampled(info, ctx.Key)).As()
            from levels in Pyramid(basis, tile, raster, overview, ctx.Key)
            from grid in CoverageGrid.Of(CoverageKind.Raster, levels, bands, reference, ctx.Key)
            let draft = new Node.Coverage(NodeId.Of(new NodeSeed.Placement()), grid)
            select (Node.Coverage)draft.Relabel(NodeId.Of(new NodeSeed.Content(draft, ctx.Header.Tolerance))));

    static Fin<CellLattice> Grid(RasterTile tile) =>
        from map in Placement.Build(
            spec: new TransformSpec.PointBasisMap(
                Rhino.Geometry.Point3d.Origin,
                Rhino.Geometry.Vector3d.XAxis, Rhino.Geometry.Vector3d.YAxis, Rhino.Geometry.Vector3d.ZAxis,
                new Rhino.Geometry.Point3d(tile.GeoTransform[0], tile.GeoTransform[3], 0.0),
                new Rhino.Geometry.Vector3d(tile.GeoTransform[1], tile.GeoTransform[4], 0.0),
                new Rhino.Geometry.Vector3d(tile.GeoTransform[2], tile.GeoTransform[5], 0.0),
                Rhino.Geometry.Vector3d.ZAxis))
        from columns in FactoryBridge.Accept<LatticeAxis>(candidate: tile.Width)
        from rows in FactoryBridge.Accept<LatticeAxis>(candidate: tile.Height)
        from layers in FactoryBridge.Accept<LatticeAxis>(candidate: 1)
        from grid in CellLattice.Of(
            indexToWorld: map, columns: columns, rows: rows, layers: layers,
            ceiling: (long)tile.Width * tile.Height)
        select grid;

    static Fin<Seq<OverviewLevel>> Pyramid(
        CellLattice basis, RasterTile tile, ArtifactContent raster,
        Func<int, ArtifactContent> overview) =>
        tile.Overviews.FoldM(
            (Grid: basis, Levels: Seq(new OverviewLevel(basis, raster, Blocked(tile.BaseBlockX, tile.BaseBlockY)))),
            (carried, level) => carried.Grid.Coarsen().Bind(next =>
                next.Columns.Value == level.Width && next.Rows.Value == level.Height
                    ? Fin.Succ((Grid: next, Levels: carried.Levels.Add(
                        new OverviewLevel(next, overview(level.Level), Blocked(level.BlockX, level.BlockY)))))
                    : Fin.Fail<(CellLattice, Seq<OverviewLevel>)>(new BimFault.Refused(BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "geo-raster-pyramid-offchain", level.Level.ToString(CultureInfo.InvariantCulture), string.Create(provider: CultureInfo.InvariantCulture, $"{level.Width}x{level.Height}") })))))
            .As()
            .Map(static carried => carried.Levels);

    static Option<(int X, int Y)> Blocked(int x, int y) => x > 0 && y > 0 ? Some((x, y)) : None;

    static Fin<CoverageBand> Sampled(RasterBandInfo info) =>
        SampleRows.Find(info.DataType).Match(
            Some: row => CoverageBand.Of(
                index: info.Index, name: $"band{info.Index}", sampleType: row.Storage,
                role: Role(info.ColorInterp, info.Palette),
                noData: info.NoData, units: info.Units, offset: info.Offset, scale: info.Scale,
                range: info.Range.Value().Map(static stat => (stat.Min, stat.Max)), palette: info.Palette),
            None: () => Fin.Fail<CoverageBand>(new BimFault.Refused(BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "geo-raster-sample-unrepresentable", info.DataType.ToString() }))));

    static readonly Map<OSGeo.GDAL.ColorInterp, BandRole> Roles = Map(
        (OSGeo.GDAL.ColorInterp.GCI_GrayIndex,    BandRole.Gray),
        (OSGeo.GDAL.ColorInterp.GCI_PaletteIndex, BandRole.Palette),
        (OSGeo.GDAL.ColorInterp.GCI_RedBand,      BandRole.Red),
        (OSGeo.GDAL.ColorInterp.GCI_GreenBand,    BandRole.Green),
        (OSGeo.GDAL.ColorInterp.GCI_BlueBand,     BandRole.Blue),
        (OSGeo.GDAL.ColorInterp.GCI_AlphaBand,    BandRole.Alpha));

    static BandRole Role(OSGeo.GDAL.ColorInterp colorInterp, Seq<ColorBin> palette) =>
        Roles.Find(colorInterp)
            .Filter(role => role != BandRole.Palette || !palette.IsEmpty)
            .IfNone(BandRole.Undefined);

    // --- [PIXEL_READ]
    static (int XOff, int YOff, int XSize, int YSize) Pixels(Option<Envelope> window, double[] gt, int rasterX, int rasterY) =>
        window.Filter(_ => (gt[1] * gt[5]) - (gt[2] * gt[4]) != 0.0).Match(
            None: () => (0, 0, rasterX, rasterY),
            Some: env => {
                double det = (gt[1] * gt[5]) - (gt[2] * gt[4]);
                (double Col, double Row) Invert(double x, double y) =>
                    (((gt[5] * (x - gt[0])) - (gt[2] * (y - gt[3]))) / det,
                     ((gt[1] * (y - gt[3])) - (gt[4] * (x - gt[0]))) / det);
                Span<(double Col, double Row)> corners =
                    [Invert(env.MinX, env.MinY), Invert(env.MinX, env.MaxY), Invert(env.MaxX, env.MinY), Invert(env.MaxX, env.MaxY)];
                var (c0, c1, r0, r1) = (double.MaxValue, double.MinValue, double.MaxValue, double.MinValue);
                foreach (var (col, row) in corners) {
                    (c0, c1, r0, r1) = (Math.Min(c0, col), Math.Max(c1, col), Math.Min(r0, row), Math.Max(r1, row));
                }
                int x0 = Math.Clamp((int)Math.Floor(c0), 0, rasterX);
                int y0 = Math.Clamp((int)Math.Floor(r0), 0, rasterY);
                int x1 = Math.Clamp((int)Math.Ceiling(c1), 0, rasterX);
                int y1 = Math.Clamp((int)Math.Ceiling(r1), 0, rasterY);
                return (x0, y0, x1 - x0, y1 - y0);
            });

    static Fin<RasterBand> Materialize(
        OSGeo.GDAL.Dataset dataset, OSGeo.GDAL.DataType dataType,
        int xOff, int yOff, int xSize, int ySize, int width, int height, int bands, int[] bandMap) {
        int cells = width * height * bands;
        return SampleRows.Find(dataType).Match(
            None: () => Fin.Fail<RasterBand>(new BimFault.Refused(BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "geo-raster-sample-unrepresentable", dataType.ToString() }))),
            Some: row => row.Width.Switch(
                bytes: _ => Stacked(
                    new byte[cells], b => dataset.ReadRaster(xOff, yOff, xSize, ySize, b, width, height, bands, bandMap, 0, 0, 0),
                    static b => new RasterBand.Bytes(b)),
                ints: _ => Stacked(
                    new int[cells], b => dataset.ReadRaster(xOff, yOff, xSize, ySize, b, width, height, bands, bandMap, 0, 0, 0),
                    static b => new RasterBand.Ints(b)),
                floats: _ => Stacked(
                    new float[cells], b => dataset.ReadRaster(xOff, yOff, xSize, ySize, b, width, height, bands, bandMap, 0, 0, 0),
                    static b => new RasterBand.Floats(b)),
                doubles: _ => Stacked(
                    new double[cells], b => dataset.ReadRaster(xOff, yOff, xSize, ySize, b, width, height, bands, bandMap, 0, 0, 0),
                    static b => new RasterBand.Doubles(b))));
    }

    static Fin<RasterBand> Stacked<T>(T[] buffer, Func<T[], OSGeo.GDAL.CPLErr> read, Func<T[], RasterBand> band)
        where T : struct {
        OSGeo.GDAL.CPLErr status = read(buffer);
        return status is OSGeo.GDAL.CPLErr.CE_Failure or OSGeo.GDAL.CPLErr.CE_Fatal
            ? Fin.Fail<RasterBand>(new BimFault.Refused(BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "raster-read-rejected", status.ToString(), buffer.Length.ToString(CultureInfo.InvariantCulture) })))
            : Fin.Succ(band(buffer));
    }

    static Evidence<RasterBand> Mask(OSGeo.GDAL.Band band, int xOff, int yOff, int xSize, int ySize, int width, int height) =>
        MaskSource.Of(band.GetMaskFlags()).Exists(static row => row == MaskSource.AllValid)
            ? new Evidence<RasterBand>.Absent()
            : Evidence.Of(Try.lift(() => {
                using OSGeo.GDAL.Band mask = band.GetMaskBand();
                return Stacked(new byte[width * height],
                    b => mask.ReadRaster(xOff, yOff, xSize, ySize, b, width, height, 0, 0),
                    static b => (RasterBand)new RasterBand.Bytes(b));
            }).Run().Bind(static inner => inner).Bind(static read => read));

    // --- [BAND_SCHEMA]
    static RasterBandInfo BandInfo(OSGeo.GDAL.Band band, int index) {
        band.GetNoDataValue(out double noData, out int hasNoData);
        band.GetOffset(out double offset, out int _);
        band.GetScale(out double scale, out int _);
        return new RasterBandInfo(
            Index:       index,
            DataType:    band.DataType,
            ColorInterp: band.GetColorInterpretation(),
            NoData:      hasNoData != 0 ? Some(noData) : Option<double>.None,
            Units:       band.GetUnitType() ?? "",
            Offset:      offset,
            Scale:       scale,
            Range:       Statistics(band),
            Histogram:   Histogram(band),
            Mask:        MaskSource.Of(band.GetMaskFlags()),
            Palette:     PaletteOf(band));
    }

    static Evidence<BandStat> Statistics(OSGeo.GDAL.Band band) =>
        band.GetStatistics(1, 0, out double min, out double max, out double mean, out double stdDev) is OSGeo.GDAL.CPLErr.CE_None
            ? new Evidence<BandStat>.Measured(new BandStat(min, max, mean, stdDev))
            : Evidence.Of(Try.lift(() =>
                band.ComputeStatistics(true, out double cMin, out double cMax, out double cMean, out double cDev, null, null) is OSGeo.GDAL.CPLErr.CE_None
                    ? Fin.Succ(new BandStat(cMin, cMax, cMean, cDev))
                    : Fin.Fail<BandStat>(new BimFault.Refused(BimScope.Semantics, BimReason.Codec, "raster-statistics-unavailable"))).Run().Bind(static inner => inner).Bind(static stat => stat));

    static Evidence<BandHistogram> Histogram(OSGeo.GDAL.Band band) =>
        band.GetDefaultHistogram(out double min, out double max, out int _, out int[] buckets, 0, null, null) is OSGeo.GDAL.CPLErr.CE_None
            && buckets is { Length: > 0 }
            ? new Evidence<BandHistogram>.Measured(new BandHistogram(min, max, toSeq(buckets)))
            : new Evidence<BandHistogram>.Absent();

    static Seq<ColorBin> PaletteOf(OSGeo.GDAL.Band band) {
        using var table = band.GetRasterColorTable();
        if (table is null) { return Seq<ColorBin>(); }
        using var rat = band.GetDefaultRAT();
        int nameColumn = rat is not null ? rat.GetColOfUsage(OSGeo.GDAL.RATFieldUsage.GFU_Name) : -1;
        string[] categories = band.GetCategoryNames() ?? [];
        return Enumerable.Range(0, table.GetCount()).AsIterable().Map(i => {
            using OSGeo.GDAL.ColorEntry entry = table.GetColorEntry(i);
            string category =
                rat is not null && nameColumn >= 0 && rat.GetRowOfValue(i) is int row and >= 0 ? rat.GetValueAsString(row, nameColumn)
                : i < categories.Length                                                       ? categories[i]
                : "";
            return new ColorBin(i, Clamp(entry.c1), Clamp(entry.c2), Clamp(entry.c3), Clamp(entry.c4), category);
        }).ToSeq();
    }

    static Fin<Seq<RasterOverview>> Overviews(OSGeo.GDAL.Band band, int baseWidth, double baseCell) =>
        Enumerable.Range(0, band.GetOverviewCount()).AsIterable().ToSeq().Traverse(i => {
            using OSGeo.GDAL.Band level = band.GetOverview(i);
            level.GetBlockSize(out int blockX, out int blockY);
            return level.XSize > 0 && level.YSize > 0
                ? Fin.Succ(new RasterOverview(i, level.XSize, level.YSize, baseCell * baseWidth / level.XSize, blockX, blockY))
                : Fin.Fail<RasterOverview>(new BimFault.Refused(BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "raster-overview-degenerate", i.ToString(CultureInfo.InvariantCulture), level.XSize.ToString(CultureInfo.InvariantCulture), level.YSize.ToString(CultureInfo.InvariantCulture) })));
        }).As();

    static byte Clamp(short channel) => (byte)Math.Clamp((int)channel, 0, 255);

    // --- [DERIVE_LEGS]
    public static Fin<Seq<GeoFeature>> Contour(ReadOnlyMemory<byte> demBytes, double interval) =>
        GeoGdal.Derive(demBytes, GdalSink.Memory, ".shp", (dem, sink) => Try.lift(() => {
            Option<ProjectedCrs> demCrs = SourceFrame(dem.GetProjectionRef());
            var options = new OSGeo.GDAL.GDALContourOptions(["-i", interval.ToString(CultureInfo.InvariantCulture), "-a", "elev"]);
            using var contoured = OSGeo.GDAL.Gdal.wrapper_GDALContourDestName(sink, dem, options, null, null);
            return Enumerable.Range(0, contoured.GetLayerCount()).AsIterable()
                .Bind(l => {
                    var layer = contoured.GetLayerByIndex(l);
                    layer.ResetReading();
                    return Traced(layer, demCrs);
                })
                .ToSeq();
        }).Run().Bind(static inner => inner), "contour");

    static IEnumerable<GeoFeature> Traced(OSGeo.OGR.Layer layer, Option<ProjectedCrs> crs) {
        for (var feature = layer.GetNextFeature(); feature is not null; feature = layer.GetNextFeature()) {
            yield return new GeoFeature(
                GeoWkb.ToNts(feature.GetGeometryRef()),
                new AttributesTable { ["type"] = "contour", ["elev"] = feature.GetFieldAsDouble("elev") },
                crs);
        }
    }

    public static Fin<byte[]> Cog(ReadOnlyMemory<byte> bytes) =>
        Translated(bytes, ["-of", "COG", "-co", "COMPRESS=DEFLATE", "-co", "OVERVIEWS=AUTO"], "cog");

    public static Fin<byte[]> Warp(ReadOnlyMemory<byte> bytes, ProjectedCrs target) =>
        GeoGdal.Derive(bytes, GdalSink.Temp, ".tif", (src, sink) => Try.lift(() => {
            var options = new OSGeo.GDAL.GDALWarpAppOptions(
                ["-t_srs", target.Wkt.Length > 0 ? target.Wkt : target.Name, "-r", "bilinear"]);
            using (OSGeo.GDAL.Gdal.Warp(sink, [src], options, null, null)) { }
            return File.ReadAllBytes(sink);
        }).Run(), "warp");

    public static Fin<byte[]> DemProcess(ReadOnlyMemory<byte> demBytes, DemMode mode) =>
        GeoGdal.Derive(demBytes, GdalSink.Temp, ".tif", (dem, sink) => Try.lift(() => {
            var options = new OSGeo.GDAL.GDALDEMProcessingOptions(["-of", "GTiff", "-co", "COMPRESS=DEFLATE"]);
            using (OSGeo.GDAL.Gdal.wrapper_GDALDEMProcessing(sink, dem, mode.Key, null, options, null, null)) { }
            return File.ReadAllBytes(sink);
        }).Run(), "dem");

    static Fin<byte[]> Translated(ReadOnlyMemory<byte> bytes, string[] arguments, string lane) =>
        GeoGdal.Derive(bytes, GdalSink.Temp, ".tif", (src, sink) => Try.lift(() => {
            using (OSGeo.GDAL.Gdal.wrapper_GDALTranslate(sink, src, new OSGeo.GDAL.GDALTranslateOptions(arguments), null, null)) { }
            return File.ReadAllBytes(sink);
        }).Run(), lane);
}
```

## [04]-[RESEARCH]

(none)
