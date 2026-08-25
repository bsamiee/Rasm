# [RASM_FABRICATION_SCAN_PATH]

`Scan.Plan` owns powder-bed vector planning from admitted layers to source-assigned machine events. One `ScanPolicy` controls zone classification, the hatch partition algebra, exposure scaling, source election, thermal wave election, plume separation, remelt, delays, recoating, and canonical egress. The chartered strategy set — meander, stripe, island, hexagon — falls out of ONE partitioned band-and-tile law rather than four generators, so a fifth strategy is a partition case and a tessellation row.

Wire posture: HOST-LOCAL. `SliceStack`, `ProcessBudget.Powder`, and optional `SupportPlan` enter once; `Audit.Preflight` gates commit; `ScanPlan` leaves through `ContentKey.Of(EgressKind.ScanVectors)`. Zone geometry composes `SliceRegion` from `slicing`; support duty composes `SupportPlan.PlanarRows` from `support`; every broad-phase and neighbour question folds the kernel `Rasm.Spatial` owner, so this page mints no spatial index and no bucket grid. Every tuning factor is a declared policy row shipping its landed values as one named preset a caller replaces whole.

## [01]-[INDEX]

- [02]-[EXPOSURE_VOCABULARY]: `ExposureClass` identity, the `ExposureScaling` factor table and its named preset, `ExposureProfile`, and `LaserSource`.
- [03]-[HATCH_ALGEBRA]: `HatchPartition`, `TileForm`, `CellOrder`, `HatchLaw` with the four chartered presets, `HatchProgram`, and serpentine ray emission.
- [04]-[ZONING]: N-layer lookback zoning of `SliceRegion` into disjoint `ExposureRegion` rows and bounded candidate generation.
- [05]-[SOURCE_FIELDS]: calibrated laser fields, the pooled election plane, and stitch peers.
- [06]-[THERMAL_SCHEDULE]: `ScanPlane` locality, kernel-served conflict pairs, bounded wave election, and `ScanOrder`.
- [07]-[EVENTS]: `ScanEvent`, discontinuity-gated jumps, remelt passes, and sampled field compensation.
- [08]-[EGRESS]: the canonical codec over `FabricationCanon`, `ScanEvidence`, and the `Scan.Plan` fold.

## [02]-[EXPOSURE_VOCABULARY]

- Owner: `ExposureClass` owns the zone identity vocabulary alone; `ExposureScaling` owns every per-class factor as a caller-supplied table; `ExposureProfile` owns one admitted physical exposure; `LaserSource` owns one calibrated scan field and the operating envelope clamping every command into it.
- Cases: core · down-skin · up-skin · contour · support-sparse · support-interface · remelt.
- Law: a class is an IDENTITY, never a factor carrier. Power, speed, spacing, focus, spot, contour-pass, and remelt-pass factors are shop tuning that varies per machine and per alloy, so they ride `ExposureScaling` rows and `ExposureScaling.Baseline` ships the landed values as one named preset. A factor spelled on the vocabulary freezes shop policy into the type system and strands every machine that disagrees.
- Entry: `ExposureScaling.For(ExposureClass)` is the one factor read; an absent row answers unity, so a partially-stated table scales only what it names.
- Auto: `ExposureProfile` and `LaserSource` admit through their generated `Validate` and the one `Admitted` bridge, so no site re-spells the refusal lift.
- Packages: Thinktecture.Runtime.Extensions, `UnitsNet`, LanguageExt.Core, BCL inbox.
- Growth: a zone is one `ExposureClass` row plus one `ExposureScaling.Baseline` entry; a shop tuning set is one named `ExposureScaling` value and no edit here.
- Boundary: scaling is dimensionless against the profile it multiplies except `FocusOffset`, which is an additive length because focus is measured from a datum and has no meaningful zero to scale.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Additive;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ExposureClass {
    public static readonly ExposureClass Core = new("core");
    public static readonly ExposureClass DownSkin = new("down-skin");
    public static readonly ExposureClass UpSkin = new("up-skin");
    public static readonly ExposureClass Contour = new("contour");
    public static readonly ExposureClass SupportSparse = new("support-sparse");
    public static readonly ExposureClass SupportInterface = new("support-interface");
    public static readonly ExposureClass Remelt = new("remelt");
}

[SmartEnum<string>]
public sealed partial class CellOrder {
    public static readonly CellOrder Serpentine = new("serpentine");
    public static readonly CellOrder Locality = new("locality");
    public static readonly CellOrder Sequential = new("sequential");
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ExposureScale(
    Ratio Power,
    Ratio Speed,
    Ratio Spacing,
    Length FocusOffset,
    Ratio Spot,
    int ContourPasses,
    int RemeltPasses) {
    public static readonly ExposureScale Unity = new(
        Ratio.FromDecimalFractions(1.0), Ratio.FromDecimalFractions(1.0), Ratio.FromDecimalFractions(1.0),
        Length.Zero, Ratio.FromDecimalFractions(1.0), ContourPasses: 0, RemeltPasses: 0);

    public bool Admitted =>
        Seq(Power.DecimalFractions, Speed.DecimalFractions, Spacing.DecimalFractions,
            FocusOffset.Millimeters, Spot.DecimalFractions).ForAll(double.IsFinite)
        && Power > Ratio.Zero && Speed > Ratio.Zero && Spacing > Ratio.Zero && Spot > Ratio.Zero
        && ContourPasses >= 0 && RemeltPasses >= 0;
}

public sealed record ExposureScaling(FrozenDictionary<ExposureClass, ExposureScale> Rows) {
    public static readonly ExposureScaling Baseline = new(new Dictionary<ExposureClass, ExposureScale> {
        [ExposureClass.Core] = ExposureScale.Unity,
        [ExposureClass.DownSkin] = new(
            Ratio.FromDecimalFractions(0.82), Ratio.FromDecimalFractions(0.76), Ratio.FromDecimalFractions(0.82),
            Length.FromMillimeters(-0.15), Ratio.FromDecimalFractions(1.08), ContourPasses: 1, RemeltPasses: 0),
        [ExposureClass.UpSkin] = new(
            Ratio.FromDecimalFractions(0.88), Ratio.FromDecimalFractions(0.82), Ratio.FromDecimalFractions(0.86),
            Length.FromMillimeters(-0.08), Ratio.FromDecimalFractions(1.04), ContourPasses: 1, RemeltPasses: 1),
        [ExposureClass.Contour] = new(
            Ratio.FromDecimalFractions(0.72), Ratio.FromDecimalFractions(0.62), Ratio.FromDecimalFractions(0.70),
            Length.FromMillimeters(-0.10), Ratio.FromDecimalFractions(0.78), ContourPasses: 2, RemeltPasses: 0),
        [ExposureClass.SupportSparse] = new(
            Ratio.FromDecimalFractions(0.65), Ratio.FromDecimalFractions(1.20), Ratio.FromDecimalFractions(1.55),
            Length.FromMillimeters(0.10), Ratio.FromDecimalFractions(1.12), ContourPasses: 0, RemeltPasses: 0),
        [ExposureClass.SupportInterface] = new(
            Ratio.FromDecimalFractions(0.80), Ratio.FromDecimalFractions(0.92), Ratio.FromDecimalFractions(0.92),
            Length.FromMillimeters(0.02), Ratio.FromDecimalFractions(1.04), ContourPasses: 1, RemeltPasses: 0),
        [ExposureClass.Remelt] = new(
            Ratio.FromDecimalFractions(0.58), Ratio.FromDecimalFractions(0.54), Ratio.FromDecimalFractions(1.00),
            Length.FromMillimeters(-0.18), Ratio.FromDecimalFractions(0.90), ContourPasses: 0, RemeltPasses: 1),
    }.ToFrozenDictionary());

    public ExposureScale For(ExposureClass row) =>
        Rows.TryGetValue(row, out ExposureScale scale) ? scale : ExposureScale.Unity;

    public bool Admitted => Rows.Values.All(static row => row.Admitted);
}

[ValueObject<int>]
public readonly partial struct LaserId {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        if (value < 0)
            validationError = new ValidationError("laser-id");
    }
}

[ComplexValueObject]
public sealed partial class LaserSource {
    public LaserId Id { get; }
    public BoundingBox Field { get; }
    public Power MaximumPower { get; }
    public Length SpotDiameter { get; }
    public Length StitchWidth { get; }
    public Length FocusMinimum { get; }
    public Length FocusMaximum { get; }
    public Ratio Drift { get; }
    public ContentKey Calibration { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref LaserId id,
        ref BoundingBox field,
        ref Power maximumPower,
        ref Length spotDiameter,
        ref Length stitchWidth,
        ref Length focusMinimum,
        ref Length focusMaximum,
        ref Ratio drift,
        ref ContentKey calibration) {
        Seq<double> values = Seq(maximumPower.Watts, spotDiameter.Millimeters, stitchWidth.Millimeters,
            focusMinimum.Millimeters, focusMaximum.Millimeters, drift.DecimalFractions);
        if (!field.IsValid || values.Exists(static value => !double.IsFinite(value))
            || maximumPower <= Power.Zero || spotDiameter <= Length.Zero || stitchWidth < Length.Zero
            || focusMinimum > focusMaximum || drift < Ratio.Zero || drift >= Ratio.FromDecimalFractions(1.0)
            || calibration.Kind != EgressKind.Plan)
            validationError = new ValidationError("laser-source");
    }

    public static Fin<LaserSource> Admit(
        LaserId id,
        BoundingBox field,
        Power maximumPower,
        Length spotDiameter,
        Length stitchWidth,
        Length focusMinimum,
        Length focusMaximum,
        Ratio drift,
        ContentKey calibration) =>
        Validate(id, field, maximumPower, spotDiameter, stitchWidth, focusMinimum, focusMaximum, drift,
            calibration, out LaserSource source).Admitted(source);

    public Power Derated(Power commanded, Ratio scale) => Power.FromWatts(Math.Min(
        commanded.Watts * scale.DecimalFractions * (1.0 - Drift.DecimalFractions),
        MaximumPower.Watts));

    public Length Focused(Length commanded, Length offset) => Length.FromMillimeters(Math.Clamp(
        (commanded + offset).Millimeters, FocusMinimum.Millimeters, FocusMaximum.Millimeters));

    public Length Spotted(Length commanded, Ratio scale) => Length.FromMillimeters(Math.Max(
        commanded.Millimeters * scale.DecimalFractions, SpotDiameter.Millimeters));
}

[ComplexValueObject]
public sealed partial class ExposureProfile {
    public Power Power { get; }
    public Speed Speed { get; }
    public Length Spacing { get; }
    public Duration Dwell { get; }
    public Length Spot { get; }
    public Length Focus { get; }
    public Duration PulseOn { get; }
    public Duration PulseOff { get; }
    public Length SkywritingLead { get; }
    public Length SkywritingLag { get; }

    public Ratio Duty => PulseOn + PulseOff == Duration.Zero
        ? Ratio.FromDecimalFractions(1.0)
        : Ratio.FromDecimalFractions(PulseOn.Seconds / (PulseOn + PulseOff).Seconds);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Power power,
        ref Speed speed,
        ref Length spacing,
        ref Duration dwell,
        ref Length spot,
        ref Length focus,
        ref Duration pulseOn,
        ref Duration pulseOff,
        ref Length skywritingLead,
        ref Length skywritingLag) {
        Seq<double> values = Seq(power.Watts, speed.MillimetersPerSecond, spacing.Millimeters, dwell.Seconds,
            spot.Millimeters, focus.Millimeters, pulseOn.Seconds, pulseOff.Seconds,
            skywritingLead.Millimeters, skywritingLag.Millimeters);
        if (values.Exists(static value => !double.IsFinite(value))
            || power <= Power.Zero || speed <= Speed.Zero || spacing <= Length.Zero || dwell < Duration.Zero
            || spot <= Length.Zero || pulseOn < Duration.Zero || pulseOff < Duration.Zero
            || skywritingLead < Length.Zero || skywritingLag < Length.Zero)
            validationError = new ValidationError("exposure-profile");
    }

    public static Fin<ExposureProfile> Admit(
        Power power,
        Speed speed,
        Length spacing,
        Duration dwell,
        Length spot,
        Length focus,
        Duration pulseOn,
        Duration pulseOff,
        Length skywritingLead,
        Length skywritingLag) =>
        Validate(power, speed, spacing, dwell, spot, focus, pulseOn, pulseOff, skywritingLead, skywritingLag,
            out ExposureProfile profile).Admitted(profile);
}
```

## [03]-[HATCH_ALGEBRA]

- Owner: `HatchPartition` owns the cell lattice a strategy fills; `TileForm` owns the tessellating polygon; `HatchLaw` owns the whole strategy as columns; `ScanGeometry` owns serpentine ray emission inside one cell.
- Cases: `HatchPartition.Whole` · `HatchPartition.Bands` · `HatchPartition.Tiles`; `TileForm.Square` · `TileForm.Hexagon`.
- Law: the chartered set is ONE law under four column settings — meander is `Whole`, stripe is `Bands`, island is `Tiles(Square)`, hexagon is `Tiles(Hexagon)`. Four hand-written generators for one parameterization is the deleted form, and a fifth strategy is a `HatchPartition` case or a `TileForm` row, never an injected generator: this page charters the fill algebra, so a caller-supplied fill callback exits the algebra it exists to own and takes its content key with it.
- Law: cells tessellate in the BEARING frame and rotate back, so a rotated stripe set stays a lattice rather than a clipped approximation of one, and per-layer rotation moves the whole lattice rather than only the rays inside it.
- Law: the cell boundary IS the strategy, so it is also the clip — rays span the cell's own extent and clip against the cell intersected with the region. Hatching a cell's bounding box spills every run into its neighbours, exposes the shared band twice, and never puts a cell boundary in front of the scanner.
- Entry: `HatchLaw.Meander`, `.Stripe`, `.Island`, and `.Hexagon` are the named presets carrying the landed spans and increments; every column is caller-replaceable.
- Auto: `ScanGeometry.Rays` emits alternate rays reversed, so consecutive rays inside a cell share an endpoint by construction and the event fold finds no discontinuity to jump across. Serpentine is a property of EMISSION; a post-hoc index-parity reversal after sorting pairs whichever rays the sort happened to adjoin and is the deleted form.
- Packages: `Rasm.Fabrication.Geometry2D` (`SliceRegion` composition), `Rhino.Geometry`, `UnitsNet`, LanguageExt.Core.
- Growth: a strategy is one `HatchPartition` case with its `Cells` arm, or one `TileForm` row with its tessellation column.
- Boundary: tessellation here is a deterministic closed-form lattice, never a relaxed point-site diagram — a checkerboard is regular by definition, and a seeded Voronoi models a different object with a seed this page would then have to key.

```csharp
[SmartEnum<string>]
public sealed partial class TileForm {
    public static readonly TileForm Square = new("square", TileLattice.Square);
    public static readonly TileForm Hexagon = new("hexagon", TileLattice.Hexagon);

    public Func<BoundingBox, double, Context, Fin<Seq<Loop>>> Tessellate { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchPartition {
    private HatchPartition() { }

    public sealed record Whole : HatchPartition;
    public sealed record Bands(Length Width) : HatchPartition;
    public sealed record Tiles(TileForm Form, Length Span) : HatchPartition;

    public bool Admitted => Switch(
        whole: static _ => true,
        bands: static row => row.Width > Length.Zero && double.IsFinite(row.Width.Millimeters),
        tiles: static row => row.Span > Length.Zero && double.IsFinite(row.Span.Millimeters));
}

public sealed record HatchLaw(
    Angle Bearing,
    HatchPartition Partition,
    Angle CellIncrement,
    Angle LayerIncrement,
    int Cycle,
    CellOrder Order) {
    public static readonly HatchLaw Meander = new(
        Angle.Zero, new HatchPartition.Whole(), Angle.Zero, Angle.FromDegrees(67.0), Cycle: 360, CellOrder.Serpentine);

    public static readonly HatchLaw Stripe = new(
        Angle.Zero, new HatchPartition.Bands(Length.FromMillimeters(10.0)),
        Angle.Zero, Angle.FromDegrees(67.0), Cycle: 360, CellOrder.Serpentine);

    public static readonly HatchLaw Island = new(
        Angle.Zero, new HatchPartition.Tiles(TileForm.Square, Length.FromMillimeters(5.0)),
        Angle.FromDegrees(90.0), Angle.FromDegrees(67.0), Cycle: 360, CellOrder.Locality);

    public static readonly HatchLaw Hexagon = new(
        Angle.Zero, new HatchPartition.Tiles(TileForm.Hexagon, Length.FromMillimeters(5.0)),
        Angle.FromDegrees(60.0), Angle.FromDegrees(67.0), Cycle: 360, CellOrder.Locality);

    public bool Admitted =>
        Partition.Admitted && Cycle > 0
        && Seq(Bearing.Radians, CellIncrement.Radians, LayerIncrement.Radians).ForAll(double.IsFinite);

    public Angle At(int layer) => Bearing + Angle.FromDegrees((layer % Cycle) * LayerIncrement.Degrees);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchProgram {
    private HatchProgram() { }

    public sealed record Filled(HatchLaw Law) : HatchProgram;
    public sealed record Contours(int Passes, Length Offset) : HatchProgram;

    public bool Admitted => Switch(
        filled: static row => row.Law.Admitted,
        contours: static row => row.Passes > 0 && row.Offset > Length.Zero
            && double.IsFinite(row.Offset.Millimeters));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TileLattice {
    public static Fin<Seq<Loop>> Square(BoundingBox bound, double span, Context tolerance) =>
        Steps(bound, span, span).Traverse(cell => Rectangle(cell, tolerance)).As();

    public static Fin<Seq<Loop>> Hexagon(BoundingBox bound, double span, Context tolerance) {
        double radius = span * 0.5;
        double columnPitch = radius * 1.5;
        double rowPitch = radius * Math.Sqrt(3.0);
        return Steps(bound, columnPitch, rowPitch)
            .Map((cell, index) => Cell(cell, index, radius, rowPitch, bound))
            .Traverse(centre => Loop.Admit(
                toSeq(Enumerable.Range(0, 6))
                    .Map(corner => new Point3d(
                        centre.X + (radius * Math.Cos(corner * Math.PI / 3.0)),
                        centre.Y + (radius * Math.Sin(corner * Math.PI / 3.0)),
                        bound.Min.Z))
                    .ToArr(),
                closed: true,
                Arr<double>(),
                tolerance))
            .As();
    }

    internal static Fin<Loop> Rectangle(BoundingBox box, Context tolerance) => Loop.Admit(
        Arr(new Point3d(box.Min.X, box.Min.Y, box.Min.Z),
            new Point3d(box.Max.X, box.Min.Y, box.Min.Z),
            new Point3d(box.Max.X, box.Max.Y, box.Min.Z),
            new Point3d(box.Min.X, box.Max.Y, box.Min.Z)),
        closed: true,
        Arr<double>(),
        tolerance);

    private static Point3d Cell(BoundingBox cell, int index, double radius, double rowPitch, BoundingBox bound) {
        int columns = Math.Max(1, (int)Math.Ceiling(bound.Diagonal.X / (radius * 1.5)));
        double stagger = (index / columns) % 2 == 0 ? 0.0 : rowPitch * 0.5;
        return new Point3d(cell.Min.X, cell.Min.Y + stagger, bound.Min.Z);
    }

    private static Seq<BoundingBox> Steps(BoundingBox bound, double columnPitch, double rowPitch) {
        int columns = Math.Max(1, (int)Math.Ceiling(bound.Diagonal.X / columnPitch));
        int rows = Math.Max(1, (int)Math.Ceiling(bound.Diagonal.Y / rowPitch));
        return toSeq(
            from row in Enumerable.Range(0, rows)
            from column in Enumerable.Range(0, columns)
            select new BoundingBox(
                new Point3d(bound.Min.X + (column * columnPitch), bound.Min.Y + (row * rowPitch), bound.Min.Z),
                new Point3d(bound.Min.X + ((column + 1) * columnPitch), bound.Min.Y + ((row + 1) * rowPitch), bound.Min.Z)));
    }
}

public static class ScanGeometry {
    public static Fin<Seq<Loop>> Cells(HatchLaw law, BoundingBox bound, Angle bearing, Context tolerance) {
        Transform into = Transform.Rotation(-bearing.Radians, Vector3d.ZAxis, bound.Center);
        Transform back = Transform.Rotation(bearing.Radians, Vector3d.ZAxis, bound.Center);
        BoundingBox framed = Framed(bound, into);
        return law.Partition.Switch(
            state: (Framed: framed, Tolerance: tolerance),
            whole: static (state, _) => TileLattice.Rectangle(state.Framed, state.Tolerance).Map(Seq),
            bands: static (state, row) => TileLattice.Square(
                    Spanning(state.Framed, row.Width.Millimeters), row.Width.Millimeters, state.Tolerance),
            tiles: static (state, row) => row.Form.Tessellate(state.Framed, row.Span.Millimeters, state.Tolerance))
            .Bind(cells => Ordered(cells, law.Order).Traverse(cell => Placed(cell, back)).As());
    }

    public static Seq<Edge3> Rays(BoundingBox cell, Angle bearing, Length spacing) {
        Vector3d direction = new(Math.Cos(bearing.Radians), Math.Sin(bearing.Radians), 0.0);
        Vector3d normal = new(-direction.Y, direction.X, 0.0);
        Point3d centre = cell.Center;
        double span = cell.Diagonal.Length;
        int count = Math.Max(1, (int)Math.Ceiling(span / spacing.Millimeters));
        return toSeq(Enumerable.Range(-count, (2 * count) + 1)).Map((index, ordinal) => {
            Point3d offset = centre + (index * spacing.Millimeters * normal);
            Edge3 ray = new(offset - (span * direction), offset + (span * direction));
            return ordinal % 2 == 0 ? ray : new Edge3(ray.B, ray.A);
        });
    }

    public static Seq<Edge3> Ring(Loop loop, Angle phase) {
        Point3d centre = loop.Bound().Center;
        int start = toSeq(Enumerable.Range(0, loop.Count)).Fold(0, (best, index) =>
            Wrapped(Math.Atan2(loop.At(index).Y - centre.Y, loop.At(index).X - centre.X) - phase.Radians)
                < Wrapped(Math.Atan2(loop.At(best).Y - centre.Y, loop.At(best).X - centre.X) - phase.Radians)
                ? index
                : best);
        return toSeq(Enumerable.Range(0, loop.Count))
            .Map(step => new Edge3(loop.At(start + step), loop.At(start + step + 1)));
    }

    internal static double Wrapped(double radians) => radians - (Math.Tau * Math.Floor(radians / Math.Tau));

    private static Seq<Loop> Ordered(Seq<Loop> cells, CellOrder order) => order.Switch(
        state: cells,
        sequential: static rows => rows,
        locality: static rows => toSeq(rows.OrderBy(static cell => ScanPlane.Morton(cell.Bound().Center))),
        serpentine: static rows => toSeq(rows
            .Select((cell, index) => (Cell: cell, Index: index, Row: Band(rows, cell)))
            .OrderBy(static row => row.Row)
            .ThenBy(static row => row.Row % 2 == 0 ? row.Index : -row.Index)
            .Select(static row => row.Cell)));

    private static int Band(Seq<Loop> cells, Loop cell) {
        double floor = cells.Fold(cells.Head.Map(static row => row.Bound().Min.Y).IfNone(0.0),
            static (least, row) => Math.Min(least, row.Bound().Min.Y));
        double height = cells.Fold(0.0, static (tallest, row) => Math.Max(tallest, row.Bound().Diagonal.Y));
        return height > 0.0 ? (int)Math.Floor((cell.Bound().Min.Y - floor) / height) : 0;
    }

    private static BoundingBox Framed(BoundingBox bound, Transform into) {
        BoundingBox framed = bound;
        framed.Transform(into);
        return framed;
    }

    private static BoundingBox Spanning(BoundingBox framed, double width) => new(
        framed.Min,
        new Point3d(framed.Min.X + Math.Max(framed.Diagonal.X, width), framed.Max.Y, framed.Min.Z));

    private static Fin<Loop> Placed(Loop cell, Transform back) =>
        Loop.Admit(cell.Vertices.Map(point => back * point), cell.Closed, cell.Bulges, cell.Tolerance);
}
```

## [04]-[ZONING]

- Owner: `Scan.Zoned` owns the layer classification; `ExposureRegion` owns one classified area with the duty it carries.
- Law: down-skin is the material within `DownSkinLayers` of an underside, so the zone is the UNION over each lookback depth of the region minus that slice — a point uncovered at any depth in range is down-skin, and a single-slice difference misses exactly the overhangs a multi-layer skin exists to catch. Up-skin mirrors it upward.
- Law: zones are DISJOINT by construction — up-skin subtracts down-skin and core subtracts both — so a thin wall classified in both directions is exposed once. Overlapping zones double the energy the wall receives and the defect is invisible in the vector count.
- Law: a solid zone's density is unity by construction; only support rows carry a planner-realized duty, and that duty comes off `SupportLayer` rather than a model-derived default.
- Law: an empty zone leaves the row set entirely, so a class with no area never reaches candidate generation as a degenerate region.
- Entry: `SliceRegion.Of(stack, layer)` is the one region source; a layer outside the stack contributes nothing beneath or above and its whole region is skin.
- Result: `ExposureRegion` carries layer, elevation, class, region, and density, so spacing resolves from the row alone.
- Packages: `Rasm.Fabrication.Additive` (`SliceRegion` from `slicing`, `SupportPlan`/`SupportLayer` from `support`), LanguageExt.Core.
- Growth: a zone is one `ExposureClass` row and one row on the zoning fold.
- Boundary: the contour class covers the whole region boundary rather than a subtracted area, because a boundary pass is a perimeter and carries no area to double-expose.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record ExposureRegion(int Layer, Length Elevation, ExposureClass Class, SliceRegion Region, Ratio Density);

public sealed record CandidateVector(int Layer, Length Elevation, ExposureClass Class, Edge3 Geometry);
```

## [05]-[SOURCE_FIELDS]

- Owner: `LaserSource` owns one calibrated field; `SourcePolicy` owns the multi-source law; `FieldCell` owns one source's tessellated territory; `SourcePartition` owns election and stitching.
- Law: a vector no calibrated field admits leaves `Elect` as source `-1` and converts on the rail; no election throws and no unadmitted vector reaches an emission arm.
- Law: exclusive vectors stay inside one source field; overlap vectors stitch under one policy and retain both adjacent source identities, so a stitched seam is addressable evidence rather than an inferred boundary.
- Auto: `SourcePartition.Build` issues one `PolygonOp.Cells` request per PLAN because calibrated source fields are invariant across layers; `SiteCell.Site` addresses the calibrated source directly and `CellDiagram.Adjacency` carries overlap, so no nearest-site probe and no page-local tessellator stands between them. This is the one legitimate point-site diagram on the page — laser fields are physically sited, unlike the hatch lattice, which is regular by definition.
- Auto: `MemoryOwner<double>` stages the vector-to-source score plane and `Elect` walks it as one pooled span kernel; `TensorPrimitives.MultiplyAdd` and `IndexOfMin` derive the load-balanced election.
- Exemption: `Elect` carries a running load cell between elections, so it stays a span loop inside one kernel, and the `Span<double>` score plane encodes an unreachable source as infinite cost because a span of doubles carries no `Option` — the absence the DOMAIN reads is the `-1` source discriminant, which converts on the rail.
- Packages: `Rasm.Fabrication.Geometry2D` (`PolygonAlgebra`/`PolygonOp.Cells`/`SitePolicy`/`CellDiagram`), CommunityToolkit.HighPerformance, `System.Numerics.Tensors`.
- Growth: a source is one `LaserSource` value on the policy; the cell census gate proves the tessellation answered one cell per source.
- Boundary: field scoring is a plain Euclidean distance between two points; a tensor call at length three allocates two arrays per cell to compute what the point atom computes with none, so the atom's own member is the entry.

```csharp
public sealed record SourcePolicy(
    Arr<LaserSource> Sources,
    Ratio BalanceWeight,
    Length PlumeClearance,
    Length Overlap,
    int FieldRelaxations,
    Ratio FieldRelaxationStrength,
    Vector3d GasBearing);

public sealed record FieldCell(LaserId Source, Seq<Point2d> Boundary, Seq<LaserId> Neighbours, Point2d Centroid, Length Perimeter);

public sealed record SourceAssignment(CandidateVector Vector, LaserSource Source, Seq<LaserSource> StitchPeers, double Score);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SourcePartition {
    public static Fin<Seq<FieldCell>> Build(SliceStack stack, SourcePolicy policy) =>
        stack.LayerCount == 0 || policy.Sources.IsEmpty
        || stack.X.Count == 0 || stack.X.Count != stack.Y.Count || stack.X.Count != stack.Z.Count
            ? Fin.Fail<Seq<FieldCell>>(new KernelFault.InvalidValue("scanpath", "scan:source-partition"))
            : policy.Sources.Map(static source => (source.Field.Center.X, source.Field.Center.Y)).Distinct().Count != policy.Sources.Count
            ? Fin.Fail<Seq<FieldCell>>(new KernelFault.InvalidValue("scanpath", "scan:duplicate-source-sites"))
            : from tolerance in Context.Millimeters().ToFin()
              from boundary in TileLattice.Rectangle(Bound(stack), tolerance)
              from trace in PolygonAlgebra.Apply(
                  new PolygonOp.Cells(
                      policy.Sources.Map(static source => new Point3d(source.Field.Center.X, source.Field.Center.Y, 0.0)).ToArr(),
                      boundary,
                      SitePolicy.Create(policy.FieldRelaxations, policy.FieldRelaxationStrength.DecimalFractions, merge: None)),
                  Op.Of(name: nameof(Build)))
              from diagram in trace.Diagram(
                  new KernelFault.InvalidValue("scanpath", "scan:cell-trace"))
              from _census in diagram.Cells.Count == policy.Sources.Count
                  ? Fin.Succ(unit)
                  : Fin.Fail<Unit>(new KernelFault.InvalidValue("scanpath", $"scan:source-cell-census:{diagram.Cells.Count}"))
              from cells in diagram.Cells.ToSeq().Traverse(cell => Measure(Seq(cell.Ring)).Map(measured => new FieldCell(
                  policy.Sources[cell.Site].Id,
                  toSeq(cell.Ring.Vertices).Map(static point => new Point2d(point.X, point.Y)),
                  diagram.Adjacency
                      .Filter(edge => edge.A == cell.Site || edge.B == cell.Site)
                      .Map(edge => policy.Sources[edge.A == cell.Site ? edge.B : edge.A].Id)
                      .ToSeq()
                      .Distinct(),
                  new Point2d(cell.Centroid.X, cell.Centroid.Y),
                  Length.FromMillimeters(measured.BoundaryLength)))).As()
              select cells;

    public static Fin<Seq<SourceAssignment>> Assign(
        Seq<CandidateVector> vectors,
        Seq<FieldCell> fields,
        SourcePolicy policy,
        int maximumVectors) =>
        vectors.IsEmpty ? Fin.Succ(Seq<SourceAssignment>())
        : vectors.Count > maximumVectors
            ? Fin.Fail<Seq<SourceAssignment>>(new KernelFault.InvalidValue("scanpath", $"scan:vector-cap:{vectors.Count}"))
        : Elected(vectors, policy).Bind(elected => vectors
            .Map((vector, row) => (Vector: vector, Index: row, Election: elected[row]))
            .Find(static row => row.Election.Source < 0)
            .Match(
                Some: row => Fin.Fail<Seq<SourceAssignment>>(new GeometryFault.DegenerateInput(
                    Kind.Mesh, row.Index, $"scan:source-field-miss:{row.Vector.Layer}")),
                None: () => Fin.Succ(vectors.Map((vector, row) => {
                    LaserSource source = policy.Sources[elected[row].Source];
                    return new SourceAssignment(vector, source, Peers(vector, source, fields, policy), elected[row].Score);
                }))));

    private static BoundingBox Bound(SliceStack stack) => new(
        new Point3d(stack.X.Min(), stack.Y.Min(), 0.0),
        new Point3d(stack.X.Max(), stack.Y.Max(), 0.0));

    private static Fin<PolygonMeasure> Measure(Seq<Loop> paths) =>
        PolygonAlgebra.Apply(new PolygonOp.Measure(paths, PolygonFill.NonZero), Op.Of(name: nameof(Measure)))
            .Bind(static trace => trace.Measure(
                new KernelFault.InvalidValue("scanpath", "scan:measure-trace")));

    private static Fin<Arr<(int Source, double Score)>> Elected(Seq<CandidateVector> vectors, SourcePolicy policy) =>
        Op.Of(name: "scan:score-plane").Catch(() => {
            int capacity = checked(vectors.Count * policy.Sources.Count);
            using MemoryOwner<double> scores = MemoryOwner<double>.Allocate(capacity, AllocationMode.Clear);
            ScoreAction action = new(scores.Memory, policy.Sources.Count, vectors.ToArr(), policy.Sources);
            ParallelHelper.For2D(0, vectors.Count, 0, policy.Sources.Count, in action);
            return Fin.Succ(Elect(scores.Span, vectors.Count, policy.Sources.Count, policy.BalanceWeight.DecimalFractions));
        });

    private static Seq<LaserSource> Peers(CandidateVector vector, LaserSource source, Seq<FieldCell> fields, SourcePolicy policy) =>
        fields.Find(field => field.Source == source.Id)
            .Map(field => field.Neighbours.Bind(id => policy.Sources
                .Find(candidate => candidate.Id == id)
                .Filter(candidate => candidate.Field.Center.DistanceTo(Midpoint(vector.Geometry))
                    <= candidate.StitchWidth.Millimeters + policy.Overlap.Millimeters)
                .ToSeq()))
            .IfNone(Seq<LaserSource>());

    internal static Point3d Midpoint(Edge3 edge) => 0.5 * (edge.A + edge.B);

    private static Arr<(int Source, double Score)> Elect(Span<double> plane, int rows, int width, double balanceWeight) {
        (int Source, double Score)[] elected = new (int, double)[rows];
        using SpanOwner<double> load = SpanOwner<double>.Allocate(width, AllocationMode.Clear);
        using SpanOwner<double> balanced = SpanOwner<double>.Allocate(width, AllocationMode.Clear);
        for (int row = 0; row < rows; row++) {
            ReadOnlySpan<double> score = plane.Slice(row * width, width);
            TensorPrimitives.MultiplyAdd(load.Span, balanceWeight, score, balanced.Span);
            int index = TensorPrimitives.IndexOfMin(balanced.Span);
            bool admitted = index >= 0 && double.IsFinite(score[index]);
            elected[row] = admitted ? (index, score[index]) : (-1, 0.0);
            if (admitted)
                load.Span[index]++;
        }
        return elected;
    }

    private readonly struct ScoreAction(Memory<double> scores, int width, Arr<CandidateVector> vectors, Arr<LaserSource> sources)
        : IAction2D {
        public void Invoke(int row, int column) {
            Point3d midpoint = Midpoint(vectors[row].Geometry);
            LaserSource source = sources[column];
            scores.Span[(row * width) + column] = source.Field.Contains(midpoint)
                ? midpoint.DistanceTo(source.Field.Center)
                : double.PositiveInfinity;
        }
    }
}
```

## [06]-[THERMAL_SCHEDULE]

- Owner: `ScanPlane` owns every derived spatial quantity — Morton locality and gas bearing — and is the ONE Morton owner on the page; `Scan.Waves` owns wave election; `ScanOrder` owns the sort key law.
- Law: Morton order is a deterministic ORDERING key, never an index. Neighbour and overlap questions fold the kernel `Rasm.Spatial` owner, so this page holds no bucket grid, no cell hash, and no neighbourhood stencil — three byte-identical grids across this folder collapse onto one kernel index.
- Law: contention is decided ONCE per layer. `SpatialQuery.SelfOverlap` enumerates every unordered pair inside one index whose bounds — each vector's segment box inflated by half the separation — overlap, and the exact `EdgeSeparation.Gap` predicate narrows that broad phase. A second quadratic re-test at result time re-derives what the election already settled and is the deleted form.
- Law: a wave identifier stays inside `[0, ThermalWindow)`. A vector whose whole window is blocked is UNRESOLVED — it takes its seed wave, the result counts it, and the plan refuses on that count. Growing the identifier past the window escapes the modal vocabulary the machine schedules against and turns an unschedulable vector into a silently valid one.
- Auto: `ScanOrder` rows carry one `Project` column, so `ScanSort.Order` is one sort over one comparable key and no caller re-tests order identity. No row rewrites geometry: serpentine orientation is owned by ray emission, so an ordering that reverses alternate rows after sorting pairs whichever vectors the sort adjoined and never survives a re-sort.
- Packages: `Rasm.Spatial` (`Spatial.Apply`, `SpatialOp.Build`/`Query`, `SpatialKind.Bvh`, `BuildPolicy.Canonical`, `SpatialQuery.SelfOverlap`, `SpatialAnswer.Result`, `QueryResult.Pairs`), `Rasm.Fabrication.Geometry2D` (`EdgeSeparation.Gap`), LanguageExt.Core.
- Growth: an ordering law is one `ScanOrder` row with its projection column.
- Boundary: the plume gate and the thermal gate share one separation — the greater of the two policy lengths — so one index answers both and no second broad phase exists to disagree with the first.

```csharp
public sealed record ThermalPolicy(Length Separation, Length PlumeClearance, Length IntersectionTolerance, int Window) {
    public static readonly ThermalPolicy Baseline = new(
        Separation: Length.FromMillimeters(3.0),
        PlumeClearance: Length.FromMillimeters(5.0),
        IntersectionTolerance: Length.FromMillimeters(0.001),
        Window: 8);

    public Length Contention => UnitMath.Max(Separation, PlumeClearance);

    public bool Admitted =>
        Window > 0 && Separation >= Length.Zero && PlumeClearance >= Length.Zero
        && IntersectionTolerance > Length.Zero
        && Seq(Separation.Millimeters, PlumeClearance.Millimeters, IntersectionTolerance.Millimeters)
            .ForAll(double.IsFinite);
}

public sealed record ScanPlane(Vector3d Gas, ThermalPolicy Thermal) {
    public ulong Locality(SourceAssignment row) => Morton(SourcePartition.Midpoint(row.Vector.Geometry));

    public double Bearing(SourceAssignment row) =>
        Vector3d.Multiply(row.Vector.Geometry.B - row.Vector.Geometry.A, Gas);

    public static ulong Morton(Point3d point) {
        long x = (long)Math.Floor(point.X);
        long y = (long)Math.Floor(point.Y);
        return Spread(ZigZag(x)) | (Spread(ZigZag(y)) << 1);
    }

    private static uint ZigZag(long value) => unchecked((uint)((value << 1) ^ (value >> 63)));

    private static ulong Spread(uint value) {
        ulong bits = value;
        bits = (bits | (bits << 16)) & 0x0000FFFF0000FFFF;
        bits = (bits | (bits << 8)) & 0x00FF00FF00FF00FF;
        bits = (bits | (bits << 4)) & 0x0F0F0F0F0F0F0F0F;
        bits = (bits | (bits << 2)) & 0x3333333333333333;
        return (bits | (bits << 1)) & 0x5555555555555555;
    }
}

[SmartEnum<string>]
public sealed partial class ScanOrder {
    public static readonly ScanOrder Spatial = new("spatial",
        static (row, plane, wave) => new ScanSortKey(0, 0, 0.0, plane.Locality(row), row.Score));
    public static readonly ScanOrder ThermalColored = new("thermal-colored",
        static (row, plane, wave) => new ScanSortKey(wave, 0, 0.0,
            wave % 2 == 0 ? plane.Locality(row) : ulong.MaxValue - plane.Locality(row), row.Score));
    public static readonly ScanOrder AgainstGas = new("against-gas",
        static (row, plane, wave) => new ScanSortKey(0, 0, plane.Bearing(row), plane.Locality(row), row.Score));
    public static readonly ScanOrder SourceBalanced = new("source-balanced",
        static (row, plane, wave) => new ScanSortKey(0, row.Source.Id.ToValue(), 0.0, plane.Locality(row), row.Score));

    public Func<SourceAssignment, ScanPlane, int, ScanSortKey> Project { get; }
}

public readonly record struct ScanSortKey(int Wave, int Source, double Bearing, ulong Locality, double Score)
    : IComparable<ScanSortKey> {
    public int CompareTo(ScanSortKey other) =>
        Wave != other.Wave ? Wave.CompareTo(other.Wave)
        : Source != other.Source ? Source.CompareTo(other.Source)
        : Bearing != other.Bearing ? Bearing.CompareTo(other.Bearing)
        : Locality != other.Locality ? Locality.CompareTo(other.Locality)
        : Score.CompareTo(other.Score);
}

public readonly record struct ScheduledVector(SourceAssignment Assignment, int Wave);

public sealed record WaveElection(Seq<ScheduledVector> Scheduled, int Unresolved);

public static class ScanSort {
    public static Seq<ScheduledVector> Order(Seq<ScheduledVector> rows, ScanOrder order, ScanPlane plane) =>
        rows.IsEmpty
            ? rows
            : toSeq(rows.OrderBy(row => order.Project(row.Assignment, plane, row.Wave)));
}
```

## [07]-[EVENTS]

- Owner: `ScanEvent` is the executable vocabulary; `SourceLane` and `ScanLayer` own the per-source and per-layer programs; `DistortionCompensation` owns the field correction.
- Law: a `Jump` is emitted only on a real DISCONTINUITY — the prior vector's end is not this vector's start within the link tolerance. Serpentine emission makes consecutive rays inside a cell contiguous by construction, so an unconditional jump per vector both doubles the event count and makes the `Jumps` counter measure the vector count instead of the dark travel it exists to measure.
- Law: field correction is calibration DATA, never an injected callback — a sampled correction grid replays byte-for-byte, keys canonically through its own bytes, and is what a scanner vendor actually ships. A caller-supplied correction function has no canonical form, so a plan built with one carries a content key that attests nothing about the geometry the machine received.
- Cases: exposure with dwell · jump · synchronization barrier · recoat · layer delay.
- Auto: each remelt pass re-reads the scaling table under `ExposureClass.Remelt`, so a shop that disables remelt sets one row rather than editing an emission arm; the source clamps power, focus, and spot through its own operating-envelope members, so the derate and the clamp exist once.
- Packages: `Rasm.Fabrication.Process` (`FabricationCanon`, `ContentKey`), `Rhino.Geometry`, `UnitsNet`, LanguageExt.Core.
- Growth: a machine semantic is one `ScanEvent` case consumed by the existing folds; a correction family is one `DistortionCompensation` case.
- Boundary: the barrier carries the wave it closes and the sources it holds, so the controller schedules against the same bounded wave vocabulary the election produced.

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DistortionCompensation {
    private DistortionCompensation() { }

    public sealed record None : DistortionCompensation;
    public sealed record Affine(Transform BuildToCommand, ContentKey Calibration) : DistortionCompensation;
    public sealed record Sampled(
        ContentKey Calibration,
        BoundingBox Field,
        int Columns,
        int Rows,
        ReadOnlyMemory<double> OffsetX,
        ReadOnlyMemory<double> OffsetY) : DistortionCompensation;

    public bool Admitted => Switch(
        none: static _ => true,
        affine: static row => Finite(row.BuildToCommand) && row.Calibration.Digest != UInt128.Zero,
        sampled: static row => row.Field.IsValid && row.Columns > 1 && row.Rows > 1
            && row.OffsetX.Count == row.Columns * row.Rows
            && row.OffsetY.Count == row.Columns * row.Rows
            && row.Calibration.Digest != UInt128.Zero);

    public Point3d Apply(Point3d point) => Switch(
        state: point,
        none: static (value, _) => value,
        affine: static (value, row) => row.BuildToCommand * value,
        sampled: static (value, row) => row.Sample(value));

    private static bool Finite(Transform value) => Seq(
        value.M00, value.M01, value.M02, value.M03,
        value.M10, value.M11, value.M12, value.M13,
        value.M20, value.M21, value.M22, value.M23,
        value.M30, value.M31, value.M32, value.M33).ForAll(double.IsFinite);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScanEvent {
    private ScanEvent() { }

    public sealed record Expose(
        LaserId Source,
        ExposureClass Class,
        Point3d From,
        Point3d To,
        Power Power,
        Speed Speed,
        Duration Dwell,
        Length Focus,
        Length Spot,
        Duration PulseOn,
        Duration PulseOff,
        Length SkywritingLead,
        Length SkywritingLag,
        Seq<LaserId> StitchPeers,
        int Wave,
        int Pass) : ScanEvent;
    public sealed record Jump(LaserId Source, Point3d From, Point3d To, Speed Speed, int Wave) : ScanEvent;
    public sealed record Synchronize(Seq<LaserId> Sources, int Wave, Duration Duration, string Reason) : ScanEvent;
    public sealed record Recoat(int Layer, Length Travel, Speed Speed, Duration Delay) : ScanEvent;
    public sealed record LayerDelay(int Layer, Duration Duration) : ScanEvent;
}

public sealed record SourceLane(LaserId Source, Seq<ScanEvent> Events);

public sealed record ScanLayer(int Layer, Length Elevation, Seq<SourceLane> Sources, Seq<ScanEvent> Events);

public sealed record TimingPolicy(
    Speed JumpSpeed,
    Duration LayerDelay,
    Duration RecoatDelay,
    Duration SourceDelay,
    Speed RecoatSpeed,
    Length RecoatTravel,
    Length LinkTolerance) {
    public static readonly TimingPolicy Baseline = new(
        JumpSpeed: new Speed(5000.0, SpeedUnit.MillimeterPerSecond),
        LayerDelay: Duration.Zero,
        RecoatDelay: Duration.FromSeconds(2.0),
        SourceDelay: Duration.Zero,
        RecoatSpeed: new Speed(100.0, SpeedUnit.MillimeterPerSecond),
        RecoatTravel: Length.FromMillimeters(300.0),
        LinkTolerance: Length.FromMillimeters(0.001));

    public bool Admitted =>
        JumpSpeed > Speed.Zero && RecoatSpeed > Speed.Zero && RecoatTravel > Length.Zero
        && LinkTolerance > Length.Zero && LayerDelay >= Duration.Zero
        && RecoatDelay >= Duration.Zero && SourceDelay >= Duration.Zero;
}
```

## [08]-[EGRESS]

- Owner: `ScanCodec.Write` is the sole canonical octet projection; `ScanEvidence` owns what the plan measured; `ScanPlan` owns the layers, bytes, evidence, and key.
- Law: the codec composes `FabricationCanon` alone — `Coords`, `Basis`, `Maybe`, `Rows`, and `Discriminant` — so a point, a transform, an optional, a row set, and a vocabulary key have one framing package-wide. A page-local point or transform writer beside them is the deleted duplicate, and a sixteen-double transform beside the twelve affine reads is a second convention over one fact.
- Law: this page owns exposure-count, path-length, and energy evidence. The executed machine clock — recoat scheduling, wave barriers, and inter-source waits — belongs to `Verify/simulate`, so no build-time column lands here and no second clock disagrees with it. Per-vector NOMINAL exposure time survives because energy is power times time and cannot be derived without it.
- Law: an unmeasured thermal quantity is ABSENT. A layer set with no inter-vector transition has no separation to average, so the columns carry `None` rather than a zero a consumer reads as perfect locality. `Unresolved` and `Stitches` are measured counts whose zero is a real reading.
- Entry: `Scan.Plan` runs policy admission, audit, physics agreement, zoning, field build, candidate generation, election, wave election, ordering, event projection, canonicalization, and result construction in one flat query inside the `FabricationEngine.Scan` bracket the supplied `SpanBand` opens, so a long derivation traces and a headless caller passing no band runs the identical query untraced.
- Result: `ScanEvidence` retains source loads, field cells, thermal moments, unresolved contention, exposure, jump, remelt, and stitch counts, path, energy, and canonical size. The producer writes its engine steps through the caller-supplied instrument set, defaulting absent for headless callers.
- Output: `ContentKey.Of(EgressKind.ScanVectors, bytes)` mints exactly once over the canonical stored bytes.
- Packages: `Rasm.Element.Projection` (`CanonicalWriter`), `Rasm.Fabrication.Process` (`FabricationCanon`, `FabricationInstruments`, `FabricationTrace`), LanguageExt.Core.
- Boundary: `ScanEvidence.Exposures`, `.Jumps`, `.Remelts`, and `.Stitches` are the four columns `Process/telemetry#OBSERVE` writes as `FabricationEngine.Scan` phases; renaming one silently strands its instrument, and the site reads them through the artifact's `Evidence`.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record ScanPolicy(
    AuditPolicy Audit,
    ExposureProfile Base,
    ExposureScaling Scaling,
    HatchProgram Hatch,
    ScanOrder Order,
    SourcePolicy Sources,
    TimingPolicy Timing,
    ThermalPolicy Thermal,
    DistortionCompensation Compensation,
    OffsetPolicy Offset,
    int MaximumVectors,
    int DownSkinLayers,
    int UpSkinLayers);

public sealed record SourceLoad(LaserId Source, int Vectors, Length Path, Duration Nominal, Energy Energy);

public sealed record ThermalEvidence(
    Option<double> AverageSeparation,
    Option<double> StandardDeviation,
    Option<double> SumOfSquares,
    int Unresolved);

public sealed record ScanEvidence(
    Seq<SourceLoad> Sources,
    Seq<FieldCell> Fields,
    ThermalEvidence Thermal,
    int Exposures,
    int Jumps,
    int Remelts,
    int Stitches,
    Length Path,
    Energy Energy,
    int CanonicalBytes);

public sealed record ScanPlan(
    Seq<ScanLayer> Layers,
    ReadOnlyMemory<byte> Bytes,
    ScanEvidence Evidence,
    ContentKey Key);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Scan {
    public static Fin<ScanPlan> Plan(
        SliceStack stack,
        ScanPolicy policy,
        ProcessBudget.Powder budget,
        Option<SupportPlan> support,
        Option<InstrumentSet> set = default,
        Option<SpanBand> band = default) =>
        band.Traced(FabricationEngine.Scan, Op.Of(), _ =>
        from _policy in (
            AdmissionSlots.Gate(policy.DownSkinLayers > 0 && policy.UpSkinLayers > 0
                && policy.MaximumVectors > 0 && policy.MaximumVectors < int.MaxValue,
                    FabConcern.Additive, "scan:layer-policy", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(policy.Thermal.Admitted, FabConcern.Additive, "scan:thermal-policy", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(policy.Scaling.Admitted, FabConcern.Additive, "scan:exposure-scaling", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(policy.Sources.FieldRelaxations >= 0
                && policy.Sources.FieldRelaxationStrength >= Ratio.Zero
                && policy.Sources.Sources.Count > 0
                && policy.Sources.Sources.Map(static source => source.Id).Distinct().Count == policy.Sources.Sources.Count
                && policy.Sources.GasBearing.IsValid && !policy.Sources.GasBearing.IsZero
                && policy.Sources.BalanceWeight >= Ratio.Zero
                && policy.Sources.PlumeClearance >= Length.Zero
                && policy.Sources.Overlap >= Length.Zero, FabConcern.Additive, "scan:source-policy", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(policy.Timing.Admitted, FabConcern.Additive, "scan:timing-policy", FabricationFault.Inadmissible),
            AdmissionSlots.Gate(policy.Hatch.Admitted && policy.Compensation.Admitted,
                FabConcern.Additive, "scan:hatch-policy", FabricationFault.Inadmissible))
            .Apply(static (_, _, _, _, _, _) => unit)
            .As()
            .ToFin()
        from audit in Audit.Preflight(stack, policy.Audit)
        from _clean in audit.Clean
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidValue("scanpath", $"scan:audit:{audit.Defects.Count}"))
        from physics in Physics(budget, policy)
        from _physics in physics.Power == policy.Base.Power
                && physics.Speed == policy.Base.Speed
                && physics.Spacing == policy.Base.Spacing
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidValue("scanpath", "scan:physics-policy"))
        from regions in Regions(stack, support, policy)
        from fields in SourcePartition.Build(stack, policy.Sources)
        from vectors in Candidates(regions, policy)
        from assigned in SourcePartition.Assign(vectors, fields, policy.Sources, policy.MaximumVectors)
        from elections in Schedule(assigned, policy)
        let contention = elections.Fold(0, static (total, row) => total + row.Unresolved)
        from _contention in contention == 0
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidValue("scanpath", $"scan:thermal-contention:{contention}"))
        from layers in Events(elections, policy)
        from bytes in ScanCodec.Write(policy, layers, Op.Of(name: nameof(Plan)))
        let evidence = Measured(fields, elections, layers, bytes.Length)
        let key = ContentKey.Of(EgressKind.ScanVectors, bytes.Span)
        from _steps in set.Steps(
            (EnginePhase.Exposures, evidence.Exposures),
            (EnginePhase.Jumps, evidence.Jumps),
            (EnginePhase.Remelts, evidence.Remelts),
            (EnginePhase.Stitches, evidence.Stitches))
        select new ScanPlan(layers, bytes, evidence, key));

    // --- [ZONING]
    private static Fin<Seq<ExposureRegion>> Regions(SliceStack stack, Option<SupportPlan> support, ScanPolicy policy) =>
        toSeq(Enumerable.Range(0, stack.LayerCount))
            .Traverse(layer => SliceRegion.Of(stack, layer))
            .As()
            .Bind(slices => Zoned(stack, slices, support, policy));

    private static Fin<Seq<ExposureRegion>> Zoned(
        SliceStack stack,
        Seq<SliceRegion> slices,
        Option<SupportPlan> support,
        ScanPolicy policy) =>
        toSeq(Enumerable.Range(0, stack.LayerCount)).Traverse(layer => {
            SliceRegion region = slices[layer];
            Length elevation = Length.FromMillimeters(stack.Elevations[layer]);
            Seq<SupportLayer> supports = support
                .Map(plan => plan.PlanarRows.Filter(row => row.Layer == layer))
                .IfNone(Seq<SupportLayer>());
            return from down in Skin(region, slices, layer, -1, policy.DownSkinLayers)
                   from above in Skin(region, slices, layer, 1, policy.UpSkinLayers)
                   from up in above.Difference(down)
                   from skin in down.Union(up)
                   from core in region.Difference(skin)
                   select Seq(
                       new ExposureRegion(layer, elevation, ExposureClass.Core, core, Solid),
                       new ExposureRegion(layer, elevation, ExposureClass.Contour, region, Solid),
                       new ExposureRegion(layer, elevation, ExposureClass.DownSkin, down, Solid),
                       new ExposureRegion(layer, elevation, ExposureClass.UpSkin, up, Solid))
                       .Concat(supports.Bind(row => Seq(
                           new ExposureRegion(layer, elevation, ExposureClass.SupportSparse, row.Sparse, row.Density),
                           new ExposureRegion(layer, elevation, ExposureClass.SupportInterface, row.Interface, row.ContactDuty))));
        }).As()
            .Map(static rows => rows.Bind(static row => row).Filter(static row => !row.Region.IsEmpty));

    private static Fin<SliceRegion> Skin(SliceRegion region, Seq<SliceRegion> slices, int layer, int direction, int depth) =>
        toSeq(Enumerable.Range(1, depth))
            .Traverse(step => At(slices, layer + (direction * step))
                .Map(region.Difference)
                .IfNone(Fin.Succ(region)))
            .As()
            .Bind(rows => rows.Head.Match(
                Some: head => rows.Tail.Fold(Fin.Succ(head), static (state, row) => state.Bind(held => held.Union(row))),
                None: () => Fin.Succ(region)));

    private static Option<SliceRegion> At(Seq<SliceRegion> slices, int layer) =>
        layer >= 0 && layer < slices.Count ? Some(slices[layer]) : None;

    // --- [CANDIDATES]
    private static Fin<Seq<CandidateVector>> Candidates(Seq<ExposureRegion> regions, ScanPolicy policy) =>
        regions.Fold(
            Fin.Succ(Seq<CandidateVector>()),
            (state, region) => state.Bind(held => Candidates(region, policy, policy.MaximumVectors - held.Count)
                .Map(held.Concat)));

    private static Fin<Seq<CandidateVector>> Candidates(ExposureRegion region, ScanPolicy policy, int remaining) {
        ExposureScale scale = policy.Scaling.For(region.Class);
        Length spacing = policy.Base.Spacing * scale.Spacing.DecimalFractions / region.Density.DecimalFractions;
        HatchProgram program = region.Class == ExposureClass.Contour
            ? new HatchProgram.Contours(scale.ContourPasses, spacing)
            : policy.Hatch;
        return spacing <= Length.Zero || !double.IsFinite(spacing.Millimeters)
            ? Fin.Fail<Seq<CandidateVector>>(new KernelFault.InvalidValue("scanpath", "scan:hatch-spacing"))
            : program.Switch(
                state: (Region: region, Spacing: spacing),
                filled: static (state, row) => Filled(state.Region, row.Law, state.Spacing),
                contours: (state, row) => Rings(state.Region, row.Passes, row.Offset, policy.Offset))
                .Bind(edges => Bounded(edges, remaining))
                .Map(edges => edges.Map(edge => new CandidateVector(region.Layer, region.Elevation, region.Class, edge)));
    }

    private static Fin<Seq<Edge3>> Filled(ExposureRegion region, HatchLaw law, Length spacing) {
        Angle bearing = law.At(region.Layer);
        return from tolerance in Context.Millimeters().ToFin()
               from cells in ScanGeometry.Cells(law, region.Region.Bound(), bearing, tolerance)
               from runs in cells.Map((cell, index) => (Cell: cell, Bearing: bearing + (index * law.CellIncrement)))
                   .Traverse(row =>
                       from island in SliceRegion.Of(Seq(row.Cell))
                       from clipped in island.Intersect(region.Region)
                       from rays in clipped.Rays(ScanGeometry.Rays(row.Cell.Bound(), row.Bearing, spacing))
                       select rays)
                   .As()
               select runs.Bind(static row => row);
    }

    private static Fin<Seq<Edge3>> Rings(ExposureRegion region, int passes, Length offset, OffsetPolicy policy) =>
        passes <= 0 || offset <= Length.Zero
            ? Fin.Fail<Seq<Edge3>>(new KernelFault.InvalidValue("scanpath", "scan:contour-program"))
            : toSeq(Enumerable.Range(0, passes))
                .Traverse(pass => region.Region.Grow(offset * -(pass + 1), policy))
                .As()
                .Map(static rows => rows.Bind(static row => row.Outers.Bind(loop => ScanGeometry.Ring(loop, Angle.Zero))));

    private static Fin<Seq<Edge3>> Bounded(Seq<Edge3> edges, int maximum) {
        Seq<Edge3> bounded = edges.Take(maximum + 1).Strict();
        return bounded.Count <= maximum
            ? Fin.Succ(bounded)
            : Fin.Fail<Seq<Edge3>>(new KernelFault.InvalidValue("scanpath", $"scan:vector-cap:{bounded.Count}"));
    }

    // --- [WAVE_ELECTION]
    private static Fin<Seq<WaveElection>> Schedule(Seq<SourceAssignment> assigned, ScanPolicy policy) {
        ScanPlane plane = new(policy.Sources.GasBearing, policy.Thermal);
        return toSeq(assigned.GroupBy(static row => row.Vector.Layer))
            .Traverse(group => Waves(toSeq(group), plane)
                .Map(election => election with { Scheduled = ScanSort.Order(election.Scheduled, policy.Order, plane) }))
            .As();
    }

    private static Fin<WaveElection> Waves(Seq<SourceAssignment> rows, ScanPlane plane) {
        double separation = plane.Thermal.Contention.Millimeters;
        BoundingBox[] boxes = rows.Map(row => Inflated(row.Vector.Geometry, separation * 0.5)).ToArray();
        return from index in Spatial
                   .Apply(new SpatialOp.Build(SpatialKind.Bvh, boxes, BuildPolicy.Canonical), Op.Of(name: nameof(Waves)))
                   .Bind(static answer => answer is SpatialAnswer.Index built
                       ? Fin.Succ(built.Value)
                       : Fin.Fail<SpatialIndex>(new KernelFault.InvalidValue("scanpath", "scan:contention-index")))
               from pairs in Spatial
                   .Apply(new SpatialOp.Query(index, new SpatialQuery.SelfOverlap(separation)), Op.Of(name: nameof(Waves)))
                   .Bind(static answer => answer is SpatialAnswer.Result { Value: QueryResult.Pairs overlaps }
                       ? Fin.Succ(overlaps.Overlaps)
                       : Fin.Fail<Seq<(int Left, int Right)>>(new KernelFault.InvalidValue("scanpath", "scan:contention-pairs")))
               let adjacency = Adjacency(rows, pairs, plane)
               select Coloured(rows, adjacency, plane.Thermal.Window);
    }

    private static HashMap<int, Set<int>> Adjacency(Seq<SourceAssignment> rows, Seq<(int Left, int Right)> pairs, ScanPlane plane) =>
        pairs.Filter(pair => rows[pair.Left].Source.Id != rows[pair.Right].Source.Id
                && rows[pair.Left].Vector.Geometry.Gap(
                    rows[pair.Right].Vector.Geometry, plane.Thermal.IntersectionTolerance.Millimeters)
                    < plane.Thermal.Contention.Millimeters)
            .Fold(HashMap<int, Set<int>>(), static (index, pair) => index
                .AddOrUpdate(pair.Left, held => held.Add(pair.Right), Set(pair.Right))
                .AddOrUpdate(pair.Right, held => held.Add(pair.Left), Set(pair.Left)));

    private static WaveElection Coloured(Seq<SourceAssignment> rows, HashMap<int, Set<int>> adjacency, int window) {
        (Seq<ScheduledVector> Scheduled, HashMap<int, int> Waves, int Unresolved) settled = rows.Fold(
            (Scheduled: Seq<ScheduledVector>(), Waves: HashMap<int, int>(), Unresolved: 0),
            (state, row) => {
                int ordinal = state.Scheduled.Count;
                int seed = (int)(ScanPlane.Morton(SourcePartition.Midpoint(row.Vector.Geometry)) % (ulong)window);
                Set<int> blocked = adjacency.Find(ordinal).IfNone(Set<int>())
                    .Fold(Set<int>(), (held, peer) => state.Waves.Find(peer).Match(Some: held.Add, None: () => held));
                Option<int> free = toSeq(Enumerable.Range(0, window))
                    .Map(offset => (seed + offset) % window)
                    .Find(candidate => !blocked.Contains(candidate));
                return (
                    state.Scheduled.Add(new ScheduledVector(row, free.IfNone(seed))),
                    state.Waves.Add(ordinal, free.IfNone(seed)),
                    state.Unresolved + (free.IsSome ? 0 : 1));
            });
        return new WaveElection(settled.Scheduled, settled.Unresolved);
    }

    private static BoundingBox Inflated(Edge3 segment, double margin) {
        BoundingBox box = new(
            new Point3d(Math.Min(segment.A.X, segment.B.X), Math.Min(segment.A.Y, segment.B.Y), Math.Min(segment.A.Z, segment.B.Z)),
            new Point3d(Math.Max(segment.A.X, segment.B.X), Math.Max(segment.A.Y, segment.B.Y), Math.Max(segment.A.Z, segment.B.Z)));
        box.Inflate(margin);
        return box;
    }

    // --- [EVENTS]
    private static Fin<Seq<ScanLayer>> Events(Seq<WaveElection> elections, ScanPolicy policy) =>
        elections.Filter(static election => !election.Scheduled.IsEmpty).Traverse(election => {
            Seq<ScheduledVector> rows = election.Scheduled;
            int layer = rows.Head.Map(static row => row.Assignment.Vector.Layer).IfNone(0);
            Seq<SourceLane> lanes = toSeq(rows.GroupBy(static row => row.Assignment.Source.Id)).Map(source => {
                Seq<ScheduledVector> lane = toSeq(source.OrderBy(static row => row.Wave));
                Seq<ScanEvent> events = lane
                    .Map((row, index) => Exposure(row, policy, index > 0 ? Some(lane[index - 1]) : None))
                    .Bind(static row => row);
                return new SourceLane(source.Key, events.Map(Compensated(policy.Compensation)));
            });
            Seq<ScanEvent> global = toSeq(rows.Map(static row => row.Wave).Distinct().OrderBy(static wave => wave))
                .Map(wave => (ScanEvent)new ScanEvent.Synchronize(
                    policy.Sources.Sources.Map(static source => source.Id).ToSeq(),
                    wave,
                    policy.Timing.SourceDelay,
                    "wave-barrier"))
                .Add(new ScanEvent.Recoat(layer, policy.Timing.RecoatTravel, policy.Timing.RecoatSpeed, policy.Timing.RecoatDelay))
                .Add(new ScanEvent.LayerDelay(layer, policy.Timing.LayerDelay));
            return Fin.Succ(new ScanLayer(
                layer,
                rows.Head.Map(static row => row.Assignment.Vector.Elevation).IfNone(Length.Zero),
                lanes,
                global.Map(Compensated(policy.Compensation))));
        }).As();

    private static Seq<ScanEvent> Exposure(ScheduledVector row, ScanPolicy policy, Option<ScheduledVector> prior) {
        SourceAssignment assignment = row.Assignment;
        Point3d resume = prior
            .Map(static previous => previous.Assignment.Vector.Geometry.B)
            .IfNone(assignment.Source.Field.Center);
        Seq<ScanEvent> approach =
            resume.DistanceTo(assignment.Vector.Geometry.A) <= policy.Timing.LinkTolerance.Millimeters
                ? Seq<ScanEvent>()
                : Seq<ScanEvent>(new ScanEvent.Jump(
                    assignment.Source.Id, resume, assignment.Vector.Geometry.A, policy.Timing.JumpSpeed, row.Wave));
        ExposureScale scale = policy.Scaling.For(assignment.Vector.Class);
        return approach.Concat(toSeq(Enumerable.Range(0, scale.RemeltPasses + 1)).Map(pass => {
            ExposureClass active = pass == 0 ? assignment.Vector.Class : ExposureClass.Remelt;
            ExposureScale passScale = pass == 0 ? scale : policy.Scaling.For(ExposureClass.Remelt);
            return (ScanEvent)new ScanEvent.Expose(
                assignment.Source.Id,
                active,
                assignment.Vector.Geometry.A,
                assignment.Vector.Geometry.B,
                assignment.Source.Derated(policy.Base.Power, passScale.Power),
                policy.Base.Speed * passScale.Speed.DecimalFractions,
                policy.Base.Dwell,
                assignment.Source.Focused(policy.Base.Focus, passScale.FocusOffset),
                assignment.Source.Spotted(policy.Base.Spot, passScale.Spot),
                policy.Base.PulseOn,
                policy.Base.PulseOff,
                policy.Base.SkywritingLead,
                policy.Base.SkywritingLag,
                assignment.StitchPeers.Map(static source => source.Id),
                row.Wave,
                pass);
        }));
    }

    private static Func<ScanEvent, ScanEvent> Compensated(DistortionCompensation compensation) => scanEvent =>
        scanEvent.Switch(
            state: compensation,
            expose: static (map, row) => (ScanEvent)(row with { From = map.Apply(row.From), To = map.Apply(row.To) }),
            jump: static (map, row) => row with { From = map.Apply(row.From), To = map.Apply(row.To) },
            synchronize: static (_, row) => row,
            recoat: static (_, row) => row,
            layerDelay: static (_, row) => row);

    private static Fin<ExposureProfile> Physics(ProcessBudget.Powder budget, ScanPolicy policy) =>
        ExposureProfile.Admit(
            new Power(budget.LaserPower, PowerUnit.Watt),
            new Speed(budget.ScanSpeed, SpeedUnit.MillimeterPerSecond),
            Length.FromMillimeters(budget.HatchSpacing),
            policy.Base.Dwell,
            Length.FromMillimeters(budget.HatchSpacing) * PowderSeed.SpotPerSpacing.DecimalFractions,
            policy.Base.Focus,
            policy.Base.PulseOn,
            policy.Base.PulseOff,
            policy.Base.SkywritingLead,
            policy.Base.SkywritingLag);

    private static ScanEvidence Measured(
        Seq<FieldCell> fields,
        Seq<WaveElection> elections,
        Seq<ScanLayer> layers,
        int bytes) {
        Seq<ScanEvent> events = layers.Bind(static layer =>
            layer.Sources.Bind(static source => source.Events).Concat(layer.Events));
        Seq<ScanEvent.Expose> exposure = events.Choose(static value => value is ScanEvent.Expose row ? Some(row) : None);
        Seq<double> separations = elections.Bind(static election => election.Scheduled.Skip(1)
            .Map((row, index) => election.Scheduled[index].Assignment.Vector.Geometry.B
                .DistanceTo(row.Assignment.Vector.Geometry.A)));
        double[] samples = separations.ToArray();
        ThermalEvidence thermal = new(
            samples.Length == 0 ? None : Some(TensorPrimitives.Average(samples)),
            samples.Length == 0 ? None : Some(TensorPrimitives.StdDev(samples)),
            samples.Length == 0 ? None : Some(TensorPrimitives.SumOfSquares(samples)),
            elections.Fold(0, static (total, row) => total + row.Unresolved));
        Seq<SourceLoad> loads = toSeq(exposure.GroupBy(static row => row.Source)).Map(static group => {
            Seq<ScanEvent.Expose> rows = toSeq(group);
            return new SourceLoad(
                group.Key,
                rows.Count,
                rows.Fold(Length.Zero, static (sum, row) => sum + Travel(row)),
                rows.Fold(Duration.Zero, static (sum, row) => sum + Nominal(row)),
                rows.Fold(Energy.Zero, static (sum, row) => sum + (row.Power * Nominal(row) * Duty(row))));
        });
        return new ScanEvidence(
            loads,
            fields,
            thermal,
            exposure.Count,
            events.Count(static value => value is ScanEvent.Jump),
            exposure.Count(static value => value.Pass > 0),
            exposure.Fold(0, static (total, row) => total + row.StitchPeers.Count),
            events.Fold(Length.Zero, static (sum, value) => sum + Path(value)),
            loads.Fold(Energy.Zero, static (sum, load) => sum + load.Energy),
            bytes);
    }

    private static Length Travel(ScanEvent.Expose row) => Length.FromMillimeters(
        row.From.DistanceTo(row.To) + row.SkywritingLead.Millimeters + row.SkywritingLag.Millimeters);

    private static Duration Nominal(ScanEvent.Expose row) =>
        Duration.FromSeconds(Travel(row).Millimeters / row.Speed.MillimetersPerSecond) + row.Dwell;

    private static double Duty(ScanEvent.Expose row) => row.PulseOn + row.PulseOff == Duration.Zero
        ? 1.0
        : row.PulseOn.Seconds / (row.PulseOn + row.PulseOff).Seconds;

    private static Length Path(ScanEvent value) => value.Switch(
        expose: static row => Travel(row),
        jump: static row => Length.FromMillimeters(row.From.DistanceTo(row.To)),
        synchronize: static _ => Length.Zero,
        recoat: static row => row.Travel,
        layerDelay: static _ => Length.Zero);

    private static Ratio Solid => Ratio.FromDecimalFractions(1.0);

}

public static class PowderSeed {
    public static readonly Ratio SpotPerSpacing = Ratio.FromDecimalFractions(0.5);
}

// --- [CANONICAL_EGRESS] ----------------------------------------------------------------
public static class ScanCodec {
    public static Fin<ReadOnlyMemory<byte>> Write(ScanPolicy policy, Seq<ScanLayer> layers, Op key) {
        CanonicalWriter writer = CanonicalWriter.Retaining(0.0);
        Identity(writer, policy);
        writer.Rows(layers, static (sink, layer) => sink
            .Ordinal(layer.Layer)
            .Double(layer.Elevation.Millimeters)
            .Rows(layer.Sources, static (lane, source) => lane
                .Ordinal(source.Source.ToValue())
                .Rows(source.Events, Event))
            .Rows(layer.Events, Event));
        return writer.ToBytes(key);
    }

    private static CanonicalWriter Event(CanonicalWriter writer, ScanEvent value) => value.Switch(
        state: writer,
        expose: static (sink, row) => sink
            .Ordinal(1).Ordinal(row.Source.ToValue()).Discriminant(row.Class)
            .Coords(row.From).Coords(row.To)
            .Double(row.Power.Watts).Double(row.Speed.MillimetersPerSecond).Double(row.Dwell.Seconds)
            .Double(row.Focus.Millimeters).Double(row.Spot.Millimeters)
            .Double(row.PulseOn.Seconds).Double(row.PulseOff.Seconds)
            .Double(row.SkywritingLead.Millimeters).Double(row.SkywritingLag.Millimeters)
            .Rows(row.StitchPeers, static (peers, peer) => peers.Ordinal(peer.ToValue()))
            .Ordinal(row.Wave).Ordinal(row.Pass),
        jump: static (sink, row) => sink
            .Ordinal(2).Ordinal(row.Source.ToValue()).Coords(row.From).Coords(row.To)
            .Double(row.Speed.MillimetersPerSecond).Ordinal(row.Wave),
        synchronize: static (sink, row) => sink
            .Ordinal(3)
            .Rows(row.Sources, static (sources, id) => sources.Ordinal(id.ToValue()))
            .Ordinal(row.Wave).Double(row.Duration.Seconds).String(row.Reason),
        recoat: static (sink, row) => sink
            .Ordinal(4).Ordinal(row.Layer).Double(row.Travel.Millimeters)
            .Double(row.Speed.MillimetersPerSecond).Double(row.Delay.Seconds),
        layerDelay: static (sink, row) => sink.Ordinal(5).Ordinal(row.Layer).Double(row.Duration.Seconds));

    private static CanonicalWriter Identity(CanonicalWriter writer, ScanPolicy policy) => Profile(writer, policy.Base)
        .Rows(toSeq(policy.Scaling.Rows.OrderBy(static row => row.Key.Key)), static (sink, row) => sink
            .Discriminant(row.Key)
            .Double(row.Value.Power.DecimalFractions).Double(row.Value.Speed.DecimalFractions)
            .Double(row.Value.Spacing.DecimalFractions).Double(row.Value.FocusOffset.Millimeters)
            .Double(row.Value.Spot.DecimalFractions)
            .Ordinal(row.Value.ContourPasses).Ordinal(row.Value.RemeltPasses))
        .Apply(policy.Hatch)
        .Discriminant(policy.Order)
        .Apply(policy.Sources)
        .Apply(policy.Timing)
        .Apply(policy.Thermal)
        .Apply(policy.Compensation)
        .Apply(policy.Offset)
        .Ordinal(policy.MaximumVectors).Ordinal(policy.DownSkinLayers).Ordinal(policy.UpSkinLayers);

    private static CanonicalWriter Profile(CanonicalWriter writer, ExposureProfile value) => writer
        .Double(value.Power.Watts).Double(value.Speed.MillimetersPerSecond)
        .Double(value.Spacing.Millimeters).Double(value.Dwell.Seconds)
        .Double(value.Spot.Millimeters).Double(value.Focus.Millimeters)
        .Double(value.PulseOn.Seconds).Double(value.PulseOff.Seconds)
        .Double(value.SkywritingLead.Millimeters).Double(value.SkywritingLag.Millimeters);

    private static CanonicalWriter Apply(this CanonicalWriter writer, HatchProgram program) => program.Switch(
        state: writer,
        filled: static (sink, row) => sink.Ordinal(1)
            .Double(row.Law.Bearing.Radians).Double(row.Law.CellIncrement.Radians)
            .Double(row.Law.LayerIncrement.Radians).Ordinal(row.Law.Cycle)
            .Discriminant(row.Law.Order)
            .Apply(row.Law.Partition),
        contours: static (sink, row) => sink.Ordinal(2).Ordinal(row.Passes).Double(row.Offset.Millimeters));

    private static CanonicalWriter Apply(this CanonicalWriter writer, HatchPartition partition) => partition.Switch(
        state: writer,
        whole: static (sink, _) => sink.Ordinal(1),
        bands: static (sink, row) => sink.Ordinal(2).Double(row.Width.Millimeters),
        tiles: static (sink, row) => sink.Ordinal(3).Discriminant(row.Form).Double(row.Span.Millimeters));

    private static CanonicalWriter Apply(this CanonicalWriter writer, SourcePolicy value) => writer
        .Rows(value.Sources.ToSeq(), static (sink, source) => sink
            .Ordinal(source.Id.ToValue()).Coords(source.Field.Min).Coords(source.Field.Max)
            .Double(source.MaximumPower.Watts).Double(source.SpotDiameter.Millimeters)
            .Double(source.StitchWidth.Millimeters).Double(source.FocusMinimum.Millimeters)
            .Double(source.FocusMaximum.Millimeters).Double(source.Drift.DecimalFractions)
            .U128(source.Calibration.Digest))
        .Double(value.BalanceWeight.DecimalFractions).Double(value.PlumeClearance.Millimeters)
        .Double(value.Overlap.Millimeters).Ordinal(value.FieldRelaxations)
        .Double(value.FieldRelaxationStrength.DecimalFractions)
        .Coords(value.GasBearing);

    private static CanonicalWriter Apply(this CanonicalWriter writer, TimingPolicy value) => writer
        .Double(value.JumpSpeed.MillimetersPerSecond).Double(value.LayerDelay.Seconds)
        .Double(value.RecoatDelay.Seconds).Double(value.SourceDelay.Seconds)
        .Double(value.RecoatSpeed.MillimetersPerSecond).Double(value.RecoatTravel.Millimeters)
        .Double(value.LinkTolerance.Millimeters);

    private static CanonicalWriter Apply(this CanonicalWriter writer, ThermalPolicy value) => writer
        .Double(value.Separation.Millimeters).Double(value.PlumeClearance.Millimeters)
        .Double(value.IntersectionTolerance.Millimeters).Ordinal(value.Window);

    private static CanonicalWriter Apply(this CanonicalWriter writer, OffsetPolicy value) => writer
        .Double(value.CollapseTolerance).Double(value.MiterLimit).Double(value.ArcTolerance)
        .Rows(toSeq(value.EdgeSpeed), static (row, speed) => row.Double(speed));

    private static CanonicalWriter Apply(this CanonicalWriter writer, DistortionCompensation value) => value.Switch(
        state: writer,
        none: static (sink, _) => sink.Ordinal(1),
        affine: static (sink, row) => sink.Ordinal(2).Basis(row.BuildToCommand).U128(row.Calibration.Digest),
        sampled: static (sink, row) => sink.Ordinal(3).U128(row.Calibration.Digest)
            .Coords(row.Field.Min).Coords(row.Field.Max).Ordinal(row.Columns).Ordinal(row.Rows)
            .Rows(toSeq(row.OffsetX.ToArray()), static (grid, offset) => grid.Double(offset))
            .Rows(toSeq(row.OffsetY.ToArray()), static (grid, offset) => grid.Double(offset)));
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Additive scanpath planning flow
    accDescr: Admitted layers become disjoint exposure zones, cell-partitioned serpentine vectors, source elections, kernel-served thermal waves, discontinuity-gated events, and canonical plan evidence.
    Stack["SliceStack"] --> Audit["Audit.Preflight"]
    Audit --> Zones["disjoint ExposureRegion zones"]
    Zones --> Cells["HatchLaw partition + serpentine rays"]
    Cells --> Fields["calibrated source cells"]
    Fields --> Assign["pooled election plane"]
    Assign --> Waves["Spatial.SelfOverlap → bounded wave election"]
    Waves --> Order["ScanOrder sort key"]
    Order --> Events["discontinuity-gated ScanEvent program"]
    Events --> Codec["FabricationCanon preimage"]
    Codec --> Key["ContentKey.Of ScanVectors"]
    Key --> Plan["ScanPlan"]
```

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
