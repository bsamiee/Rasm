# [RASM_FABRICATION_ALGEBRA]

`PolygonAlgebra` owns line-space fabrication geometry over `Clipper2`: one operation family admits line-only planar material, returns topology for region results and grouping for open runs, executes offset, Boolean, window, hygiene, morphology, inspection, and field projection, then emits one evidence-bearing result family. `Loop`, `Edge3`, and `Context` remain the boundary atoms, and `ArcAlgebra.Densify` remains the only bulge-to-line bridge.

`PolygonAlgebra.Apply` mirrors `Parametric.Apply`: one request, one `Op?` resolved through `OrDefault()`, and one `Fin<PolygonTrace>` rail. Each arm names its case for provenance. Malformed policy routes `key.InvalidInput()`, degenerate geometry routes indexed `GeometryFault.DegenerateInput`, and provider throws route `key.InvalidResult(detail)`. Requests carry policy values, foreign carriers terminate inside the owner, and results re-enter at the admitted context and elevation.

## [01]-[INDEX]

- [02]-[OPERATION_ALGEBRA]: `PolygonOp`, its policy families, one `Apply` dispatch, and the typed `PolygonTrace` egress.
- [03]-[FIELD_PLANE]: `FieldGrid` and `FieldMetric` project occupancy, signed clearance, cutter engagement, cutter reachability, and local inscribed diameter into one finite-gated plane receipt.

## [02]-[OPERATION_ALGEBRA]

- Owner: `PolygonOp` is the complete request family, and `PolygonAlgebra.Apply(PolygonOp?, Op?)` is its only public execution surface.
- Cases: `PolygonOp` and its input-shape policies jointly discriminate uniform and per-vertex offset, closed and open clipping, closed and open windowing, four hygiene algorithms, morphology, measurement, containment, topology, and raster projection. Boolean, fill, join, and end rows carry their native `Clipper2` values.
- Entry: `OffsetField` survives on scalar-versus-matrix arity, `PolygonSource` on region-versus-run admission, and `HygieneRule` on algorithm payload timing; every collection owns singular and plural arity, and each case carries only the evidence its arm consumes.
- Auto: `ClipperD` owns precision-bearing Boolean, rectangle, containment, and topology work; one `ClipperOffset` engine owns both constant and callback offsets so `PreserveCollinear`, `ReverseSolution`, and `MergeGroups` bind identically on either arity; `Minkowski` owns morphology; one `Try` boundary lowers package exceptions onto `Fin<T>`.
- Law: admission owns its vocabulary — `HygieneRule.Admit` and `FieldMetric.Admit` gate their own scalars on the case that declares them, so no arm dispatches the same discriminant twice, and `Op.Need`/`Finite`/`Positive` carry presence and scalar gates rather than page-local re-derivations.
- Receipt: `PolygonTrace` distinguishes flat paths, region forests, grouped runs, split runs, measures, point relations, and sampled fields by evidence timing; `RegionNode.Parent` carries the pre-order ordinal the tree walk assigns, never a re-scanned reference match.
- Packages: `Clipper2` supplies the line-space kernel; `Thinktecture` supplies generated owners and exhaustive dispatch; `LanguageExt` supplies admission, traversal, immutable carriers, and the exception rail; `Rasm.Domain` supplies the `Op` key rail and the `Kind` fault taxonomy.
- Growth: a new operation is one `PolygonOp` case, one `PolygonTrace` case when its evidence differs, and one generated dispatch arm naming its own `Op`.
- Boundary: `ClipperD`, `ClipperOffset`, region measurement, point relation, and `IAction2D` sampling are the statement-bearing native and numeric kernels. Inputs share one `Context` and elevation before XY projection; bulges, mixed contexts, mixed elevations, invalid open edges, and closure-policy conflicts fail before package execution, each naming the index of the first offending path.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Linq;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Clipper2Lib;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Helpers;
using Foundation.CSharp.Analyzers.Contracts;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using Rasm.Domain;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Geometry2D;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class PolygonBoolean {
    public static readonly PolygonBoolean Union = new("union", ClipType.Union);
    public static readonly PolygonBoolean Intersection = new("intersection", ClipType.Intersection);
    public static readonly PolygonBoolean Difference = new("difference", ClipType.Difference);
    public static readonly PolygonBoolean Xor = new("xor", ClipType.Xor);

    internal ClipType Native { get; }
}

[SmartEnum<string>]
public sealed partial class PolygonFill {
    public static readonly PolygonFill EvenOdd = new("even-odd", FillRule.EvenOdd);
    public static readonly PolygonFill NonZero = new("non-zero", FillRule.NonZero);
    public static readonly PolygonFill Positive = new("positive", FillRule.Positive);
    public static readonly PolygonFill Negative = new("negative", FillRule.Negative);

    internal FillRule Native { get; }
}

[SmartEnum<string>]
public sealed partial class OffsetJoin {
    public static readonly OffsetJoin Miter = new("miter", JoinType.Miter);
    public static readonly OffsetJoin Square = new("square", JoinType.Square);
    public static readonly OffsetJoin Bevel = new("bevel", JoinType.Bevel);
    public static readonly OffsetJoin Round = new("round", JoinType.Round);

    internal JoinType Native { get; }
}

[SmartEnum<string>]
public sealed partial class OffsetEnd {
    public static readonly OffsetEnd Polygon = new("polygon", EndType.Polygon);
    public static readonly OffsetEnd Joined = new("joined", EndType.Joined);
    public static readonly OffsetEnd Butt = new("butt", EndType.Butt);
    public static readonly OffsetEnd Square = new("square", EndType.Square);
    public static readonly OffsetEnd Round = new("round", EndType.Round);

    internal EndType Native { get; }
}

[SmartEnum<string>]
public sealed partial class MorphologyKind {
    public static readonly MorphologyKind Sum = new("sum");
    public static readonly MorphologyKind Difference = new("difference");
}

[SmartEnum<string>]
public sealed partial class PointRelation {
    public static readonly PointRelation Outside = new("outside");
    public static readonly PointRelation Boundary = new("boundary");
    public static readonly PointRelation Inside = new("inside");
}

[ComplexValueObject]
public sealed partial class OffsetPolicy {
    public OffsetJoin Join { get; }
    public OffsetEnd End { get; }
    public double MiterLimit { get; }
    public double ArcTolerance { get; }
    public bool PreserveCollinear { get; }
    public bool ReverseSolution { get; }
    public bool MergeGroups { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref OffsetJoin join,
        ref OffsetEnd end,
        ref double miterLimit,
        ref double arcTolerance,
        ref bool preserveCollinear,
        ref bool reverseSolution,
        ref bool mergeGroups) =>
        validationError = double.IsFinite(miterLimit) && miterLimit >= 1.0
            && double.IsFinite(arcTolerance) && arcTolerance > 0.0
                ? null
                : new ValidationError(message: "Offset fidelity must carry a finite miter limit and positive arc tolerance.");

    public static Fin<OffsetPolicy> Admit(
        OffsetJoin join,
        OffsetEnd end,
        double miterLimit,
        double arcTolerance,
        bool preserveCollinear = false,
        bool reverseSolution = false,
        bool mergeGroups = true,
        Op? key = null) =>
        Validate(join, end, miterLimit, arcTolerance, preserveCollinear, reverseSolution, mergeGroups, out OffsetPolicy? policy) is null
            ? Fin.Succ(policy!)
            : Fin.Fail<OffsetPolicy>(key.OrDefault().InvalidInput());
}

[ComplexValueObject]
public sealed partial class FieldGrid {
    public BoundingBox Bounds { get; }
    public double Cell { get; }
    public int MaximumCells { get; }

    public int Columns => (int)Math.Ceiling(Bounds.Diagonal.X / Cell);
    public int Rows => (int)Math.Ceiling(Bounds.Diagonal.Y / Cell);

    public Point3d Center(int row, int column) =>
        new(Bounds.Min.X + (column + 0.5) * Cell, Bounds.Min.Y + (row + 0.5) * Cell, Bounds.Min.Z);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref BoundingBox bounds,
        ref double cell,
        ref int maximumCells) {
        double columns = bounds.IsValid && double.IsFinite(cell) && cell > 0.0
            ? Math.Ceiling(bounds.Diagonal.X / cell)
            : double.PositiveInfinity;
        double rows = bounds.IsValid && double.IsFinite(cell) && cell > 0.0
            ? Math.Ceiling(bounds.Diagonal.Y / cell)
            : double.PositiveInfinity;
        validationError = columns is >= 1.0 and <= int.MaxValue
            && rows is >= 1.0 and <= int.MaxValue
            && maximumCells is >= 1 and <= Array.MaxLength
            && columns * rows <= maximumCells
            ? null
            : new ValidationError(message: "Field bounds and cell size must resolve to a finite non-empty plane.");
    }

    public static Fin<FieldGrid> Admit(BoundingBox bounds, double cell, int maximumCells, Op? key = null) =>
        Validate(bounds, cell, maximumCells, out FieldGrid? grid) is null
            ? Fin.Succ(grid!)
            : Fin.Fail<FieldGrid>(key.OrDefault().InvalidInput());
}

[Union]
public abstract partial record OffsetField {
    public sealed record Uniform(double Distance) : OffsetField;
    public sealed record Variable(Arr<Arr<double>> Distances) : OffsetField;
}

[Union]
public abstract partial record HygieneRule {
    public sealed record Simplify(double Epsilon) : HygieneRule;
    public sealed record RamerDouglasPeucker(double Epsilon) : HygieneRule;
    public sealed record Collinear : HygieneRule;
    public sealed record Duplicates : HygieneRule;

    internal Fin<Unit> Admit(Op key) => Switch(
        state: key,
        simplify: static (op, rule) => op.Positive(rule.Epsilon).Map(static _ => unit),
        ramerDouglasPeucker: static (op, rule) => op.Positive(rule.Epsilon).Map(static _ => unit),
        collinear: static (_, _) => Fin.Succ(unit),
        duplicates: static (_, _) => Fin.Succ(unit));
}

[Union]
public abstract partial record FieldMetric {
    public sealed record Occupancy : FieldMetric;
    public sealed record SignedClearance : FieldMetric;
    public sealed record Engagement(double CutterRadius) : FieldMetric;
    public sealed record Reachable(double ToolRadius) : FieldMetric;
    public sealed record InscribedDiameter : FieldMetric;

    internal Fin<Unit> Admit(Op key) => Switch(
        state: key,
        occupancy: static (_, _) => Fin.Succ(unit),
        signedClearance: static (_, _) => Fin.Succ(unit),
        engagement: static (op, metric) => op.Positive(metric.CutterRadius).Map(static _ => unit),
        reachable: static (op, metric) => op.Positive(metric.ToolRadius).Map(static _ => unit),
        inscribedDiameter: static (_, _) => Fin.Succ(unit));

    // Occupancy reads only the sign, so its cells skip the per-cell nearest-segment fold entirely.
    internal bool NeedsClearance => Switch(
        occupancy: static _ => false,
        signedClearance: static _ => true,
        engagement: static _ => true,
        reachable: static _ => true,
        inscribedDiameter: static _ => true);

    internal double Sample(double clearance) => Switch(
        state: clearance,
        occupancy: static (value, _) => value <= 0.0 ? 1.0 : 0.0,
        signedClearance: static (value, _) => value,
        engagement: static (value, field) => double.Clamp((field.CutterRadius - Math.Abs(value)) / field.CutterRadius, 0.0, 1.0),
        reachable: static (value, field) => value <= -field.ToolRadius ? 1.0 : 0.0,
        inscribedDiameter: static (value, _) => value < 0.0 ? -2.0 * value : 0.0);
}

[Union]
public abstract partial record PolygonSource {
    public sealed record Regions(Seq<Loop> Paths, PolygonFill Fill) : PolygonSource;
    public sealed record Edges(Seq<Seq<Edge3>> Paths, Context Tolerance, double Plane) : PolygonSource;
}

[Union]
public abstract partial record PolygonOp {
    public sealed record Offset(Seq<Loop> Paths, OffsetField Field, OffsetPolicy Policy) : PolygonOp;
    public sealed record Boolean(Seq<Loop> Subject, Seq<Loop> Clip, PolygonBoolean Kind, PolygonFill Fill) : PolygonOp;
    public sealed record ClipOpen(Seq<Seq<Edge3>> Subject, Seq<Loop> Clip, PolygonFill Fill) : PolygonOp;
    public sealed record Window(PolygonSource Source, BoundingBox Bounds) : PolygonOp;
    public sealed record Hygiene(Seq<Loop> Paths, HygieneRule Rule) : PolygonOp;
    public sealed record Morphology(Loop Pattern, Loop Path, MorphologyKind Kind) : PolygonOp;
    public sealed record Measure(Seq<Loop> Paths, PolygonFill Fill) : PolygonOp;
    public sealed record Contains(Seq<Loop> Paths, Arr<Point3d> Points, PolygonFill Fill) : PolygonOp;
    public sealed record Topology(Seq<Loop> Paths, PolygonFill Fill) : PolygonOp;
    public sealed record Raster(Seq<Loop> Paths, PolygonFill Fill, FieldGrid Grid, FieldMetric Metric) : PolygonOp;
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record RegionNode(
    int Index,
    Option<int> Parent,
    int Depth,
    bool IsHole,
    Loop Boundary,
    double SignedArea,
    BoundingBox Bounds);

public sealed record TopologyReceipt(Seq<RegionNode> Nodes, PolygonFill Fill, Context Tolerance, double Plane);

public sealed record PolygonMeasure(
    double SignedArea,
    double FilledArea,
    double BoundaryLength,
    Point3d Centroid,
    BoundingBox Bounds,
    int Outers,
    int Holes);

public sealed record FieldReceipt(
    ReadOnlyMemory2D<double> Samples,
    FieldGrid Grid,
    FieldMetric Metric,
    Context Tolerance,
    double Plane,
    double Minimum,
    double Maximum,
    double Average,
    double Deviation,
    Point3d MinimumAt,
    Point3d MaximumAt);

[Union]
public abstract partial record PolygonTrace {
    public sealed record Paths(Seq<Loop> Result) : PolygonTrace;
    public sealed record Regions(TopologyReceipt Result) : PolygonTrace;
    public sealed record Runs(Seq<Seq<Edge3>> Result) : PolygonTrace;
    public sealed record SplitRuns(Seq<Seq<Edge3>> Inside, Seq<Seq<Edge3>> Outside) : PolygonTrace;
    public sealed record Measured(PolygonMeasure Result) : PolygonTrace;
    public sealed record Related(Arr<PointRelation> Result) : PolygonTrace;
    public sealed record Field(FieldReceipt Result) : PolygonTrace;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class PolygonAlgebra {
    public static Fin<PolygonTrace> Apply(PolygonOp? operation, Op? key = null) =>
        from admitted in key.OrDefault().Need(operation)
        let resolved = Resolve(admitted, key)
        from result in Try.lift<Fin<PolygonTrace>>(f: () => admitted.Switch(
                state: resolved,
                offset: static (op, request) => OffsetOf(request, op),
                boolean: static (op, request) => BooleanOf(request, op),
                clipOpen: static (op, request) => OpenClipOf(request, op),
                window: static (op, request) => WindowOf(request, op),
                hygiene: static (op, request) => HygieneOf(request, op),
                morphology: static (op, request) => MorphologyOf(request, op),
                measure: static (op, request) => MeasureOf(request, op),
                contains: static (op, request) => ContainsOf(request, op),
                topology: static (op, request) => TopologyOf(request, op),
                raster: static (op, request) => RasterOf(request, op)))
            .Run()
            .MapFail(error => resolved.InvalidResult(detail: error.Message))
            .Bind(static value => value)
        select result;

    private static Op Resolve(PolygonOp operation, Op? key) => key ?? operation.Switch(
        offset: static _ => Op.Of(name: nameof(PolygonOp.Offset)),
        boolean: static _ => Op.Of(name: nameof(PolygonOp.Boolean)),
        clipOpen: static _ => Op.Of(name: nameof(PolygonOp.ClipOpen)),
        window: static _ => Op.Of(name: nameof(PolygonOp.Window)),
        hygiene: static _ => Op.Of(name: nameof(PolygonOp.Hygiene)),
        morphology: static _ => Op.Of(name: nameof(PolygonOp.Morphology)),
        measure: static _ => Op.Of(name: nameof(PolygonOp.Measure)),
        contains: static _ => Op.Of(name: nameof(PolygonOp.Contains)),
        topology: static _ => Op.Of(name: nameof(PolygonOp.Topology)),
        raster: static _ => Op.Of(name: nameof(PolygonOp.Raster)));

    private static Fin<PolygonTrace> OffsetOf(PolygonOp.Offset request, Op op) =>
        from paths in Lines(request.Paths, Kind.Polyline)
        from field in op.Need(request.Field)
        from policy in op.Need(request.Policy)
        from _ in OffsetClosure(paths, policy)
        from result in field.Switch(
            state: (Paths: paths, Policy: policy, Op: op),
            uniform: static (state, admitted) => UniformOffset(state.Paths, admitted.Distance, state.Policy, state.Op),
            variable: static (state, admitted) => VariableOffset(state.Paths, admitted.Distances, state.Policy, state.Op))
        from tree in TreeOf(result, PolygonFill.NonZero, paths[0].Tolerance, op)
        from topology in TopologyOf(tree, paths[0].Tolerance, paths[0].Plane, PolygonFill.NonZero, op)
        select (PolygonTrace)new PolygonTrace.Regions(topology);

    private static Fin<PolygonTrace> BooleanOf(PolygonOp.Boolean request, Op op) =>
        from subject in Regions(request.Subject)
        from clip in Regions(request.Clip, admitEmpty: true)
        from kind in op.Need(request.Kind)
        from fill in op.Need(request.Fill)
        from operands in Lines(subject.Concat(clip), Kind.Polyline)
        from tree in TreeOf(ToPaths(subject), clip.IsEmpty ? null : ToPaths(clip), kind, fill, operands[0].Tolerance, op)
        from topology in TopologyOf(tree, operands[0].Tolerance, operands[0].Plane, fill, op)
        select (PolygonTrace)new PolygonTrace.Regions(topology);

    private static Fin<PolygonTrace> OpenClipOf(PolygonOp.ClipOpen request, Op op) =>
        from clip in Regions(request.Clip)
        from fill in op.Need(request.Fill)
        from subject in Edges(request.Subject, clip[0].Tolerance, clip[0].Plane)
        from result in ClipRuns(subject, clip, fill, op)
        select (PolygonTrace)new PolygonTrace.SplitRuns(result.Inside, result.Outside);

    private static Fin<PolygonTrace> WindowOf(PolygonOp.Window request, Op op) =>
        op.Need(request.Source).Bind(source => source.Switch(
            state: (Bounds: request.Bounds, Op: op),
            regions: static (state, admitted) =>
                from paths in Regions(admitted.Paths)
                from fill in state.Op.Need(admitted.Fill)
                from _ in Window(state.Bounds, paths[0].Tolerance)
                from clipped in FromPaths(
                    Clipper.RectClip(RectOf(state.Bounds), ToPaths(paths), Precision(paths[0].Tolerance)),
                    closed: true,
                    paths[0].Tolerance,
                    paths[0].Plane)
                from tree in TreeOf(ToPaths(clipped), fill, paths[0].Tolerance, state.Op)
                from topology in TopologyOf(tree, paths[0].Tolerance, paths[0].Plane, fill, state.Op)
                select (PolygonTrace)new PolygonTrace.Regions(topology),
            edges: static (state, admitted) =>
                from tolerance in state.Op.Need(admitted.Tolerance)
                from paths in Edges(admitted.Paths, tolerance, admitted.Plane)
                from _ in Window(state.Bounds, tolerance)
                let clipped = Clipper.RectClipLines(RectOf(state.Bounds), ToOpenPaths(paths), Precision(tolerance))
                select (PolygonTrace)new PolygonTrace.Runs(Runs(clipped, admitted.Plane))));

    private static Fin<PolygonTrace> HygieneOf(PolygonOp.Hygiene request, Op op) =>
        from paths in Lines(request.Paths, Kind.Polyline)
        from rule in op.Need(request.Rule)
        from _ in rule.Admit(op)
        from result in paths.TraverseM(path => rule.Switch(
            state: path,
            simplify: static (loop, admitted) => FromPath(
                Clipper.SimplifyPath(ToPath(loop), admitted.Epsilon, loop.Closed), loop.Closed, loop.Tolerance, loop.Plane),
            ramerDouglasPeucker: static (loop, admitted) => FromPath(
                Clipper.RamerDouglasPeucker(ToPath(loop), admitted.Epsilon), loop.Closed, loop.Tolerance, loop.Plane),
            collinear: static (loop, _) => FromPath(
                Clipper.TrimCollinear(ToPath(loop), Precision(loop.Tolerance), isOpen: !loop.Closed),
                loop.Closed,
                loop.Tolerance,
                loop.Plane),
            duplicates: static (loop, _) => StripDuplicates(loop))).As()
        select (PolygonTrace)new PolygonTrace.Paths(result);

    private static Fin<PolygonTrace> MorphologyOf(PolygonOp.Morphology request, Op op) =>
        from kind in op.Need(request.Kind)
        from operands in Lines(Seq(request.Pattern, request.Path), Kind.Polyline)
        from _ in guard(operands[0].Closed, new GeometryFault.DegenerateInput(Kind.Polyline, 0, "morphology:open-pattern").ToError()).ToFin()
        let result = kind.Switch(
            state: (Pattern: ToPath(operands[0]), Path: ToPath(operands[1]), Closed: operands[1].Closed, Precision: Precision(operands[0].Tolerance)),
            sum: static state => Clipper2Lib.Minkowski.Sum(state.Pattern, state.Path, state.Closed, state.Precision),
            difference: static state => Clipper2Lib.Minkowski.Diff(state.Pattern, state.Path, state.Closed, state.Precision))
        from tree in TreeOf(result, PolygonFill.NonZero, operands[0].Tolerance, op)
        from topology in TopologyOf(tree, operands[0].Tolerance, operands[0].Plane, PolygonFill.NonZero, op)
        select (PolygonTrace)new PolygonTrace.Regions(topology);

    private static Fin<PolygonTrace> MeasureOf(PolygonOp.Measure request, Op op) =>
        from paths in Regions(request.Paths)
        from fill in op.Need(request.Fill)
        from tree in TreeOf(ToPaths(paths), null, PolygonBoolean.Union, fill, paths[0].Tolerance, op)
        from topology in TopologyOf(tree, paths[0].Tolerance, paths[0].Plane, fill, op)
        from _ in guard(!topology.Nodes.IsEmpty, new GeometryFault.DegenerateInput(Kind.Polyline, -1, "measure:empty-fill").ToError()).ToFin()
        select (PolygonTrace)new PolygonTrace.Measured(MeasureOf(topology));

    private static Fin<PolygonTrace> ContainsOf(PolygonOp.Contains request, Op op) =>
        from paths in Regions(request.Paths)
        from fill in op.Need(request.Fill)
        from trace in PointsOf(paths, request.Points, fill)
        select trace;

    private static Fin<PolygonTrace> TopologyOf(PolygonOp.Topology request, Op op) =>
        from paths in Regions(request.Paths)
        from fill in op.Need(request.Fill)
        from tree in TreeOf(ToPaths(paths), null, PolygonBoolean.Union, fill, paths[0].Tolerance, op)
        from topology in TopologyOf(tree, paths[0].Tolerance, paths[0].Plane, fill, op)
        select (PolygonTrace)new PolygonTrace.Regions(topology);

    private static Fin<PolygonTrace> RasterOf(PolygonOp.Raster request, Op op) =>
        from paths in Regions(request.Paths)
        from fill in op.Need(request.Fill)
        from grid in op.Need(request.Grid)
        from metric in op.Need(request.Metric)
        from _ in Check(
                Math.Abs(grid.Bounds.Min.Z - paths[0].Plane) <= paths[0].Tolerance.Absolute.Value
                && Math.Abs(grid.Bounds.Max.Z - paths[0].Plane) <= paths[0].Tolerance.Absolute.Value,
                Kind.Plane,
                -1,
                "raster:grid-elevation")
            .As()
            .ToFin()
            .Bind(_ => metric.Admit(op))
        let values = new double[grid.Rows, grid.Columns]
        let kernel = new RasterKernel(values, ToPaths(paths), Segments(paths), fill, grid, Precision(paths[0].Tolerance), metric)
        from receipt in Raster(kernel, values, grid, metric, paths[0].Tolerance, paths[0].Plane)
        select (PolygonTrace)new PolygonTrace.Field(receipt);

    // --- [BOUNDARIES] -------------------------------------------------------------------------------------------------------------------------------
    private static K<Validation<Error>, Unit> Check(bool condition, Kind kind, int index, string witness) =>
        guard(condition, new GeometryFault.DegenerateInput(kind, index, witness).ToError()).ToFin().ToValidation();

    private static K<Validation<Error>, Unit> Defect<T>(Seq<T> values, Func<T, bool> offends, Kind kind, string witness) {
        Option<int> at = values
            .Map(static (value, index) => (Value: value, Index: index))
            .Filter(row => offends(row.Value))
            .Map(static row => row.Index)
            .Head;
        return Check(at.IsNone, kind, at.IfNone(-1), witness);
    }

    private static Fin<Seq<Loop>> Lines(Seq<Loop> paths, Kind kind, bool admitEmpty = false) =>
        (Check(admitEmpty || !paths.IsEmpty, kind, -1, "empty"),
         Defect(paths, static path => path is null, kind, "null"),
         Defect(paths, static path => path is not null && path.Bulges.Exists(static bulge => bulge != 0.0), kind, "bulged"),
         Defect(paths, path => path is not null && paths[0] is not null && path.Tolerance != paths[0].Tolerance, kind, "mixed-context"),
         Defect(paths, path => path is not null && paths[0] is not null
             && Math.Abs(path.Plane - paths[0].Plane) > path.Tolerance.Absolute.Value, kind, "mixed-elevation"))
            .Apply(static (_, _, _, _, _) => paths)
            .As()
            .ToFin();

    private static Fin<Seq<Loop>> Regions(Seq<Loop> paths, bool admitEmpty = false) =>
        from admitted in Lines(paths, Kind.Polyline, admitEmpty)
        from _ in Defect(admitted, static path => !path.Closed, Kind.Polyline, "open").As().ToFin()
        select admitted;

    private static Fin<Seq<Seq<Edge3>>> Edges(Seq<Seq<Edge3>> paths, Context tolerance, double plane) =>
        (Check(double.IsFinite(plane), Kind.Plane, -1, "non-finite-plane"),
         Check(!paths.IsEmpty, Kind.Line, -1, "empty"),
         Defect(paths, static path => path.IsEmpty, Kind.Line, "empty-run"),
         Defect(paths, static path => path.Exists(static edge => !edge.A.IsValid || !edge.B.IsValid), Kind.Line, "non-finite"),
         Defect(paths, path => path.Exists(edge => edge.A.DistanceTo(edge.B) <= tolerance.Absolute.Value), Kind.Line, "zero-length"),
         Defect(paths, path => path.Exists(edge => Math.Abs(edge.A.Z - plane) > tolerance.Absolute.Value
             || Math.Abs(edge.B.Z - plane) > tolerance.Absolute.Value), Kind.Line, "off-plane"),
         Defect(paths, path => Range(0, Math.Max(0, path.Count - 1))
             .Exists(index => path[index].B.DistanceTo(path[index + 1].A) > tolerance.Absolute.Value), Kind.Line, "discontinuous"))
            .Apply(static (_, _, _, _, _, _, _) => paths)
            .As()
            .ToFin();

    private static Fin<Unit> OffsetClosure(Seq<Loop> paths, OffsetPolicy policy) =>
        Defect(paths, path => policy.End == OffsetEnd.Polygon ? !path.Closed : path.Closed, Kind.Polyline, "offset:closure-conflict")
            .As()
            .ToFin();

    private static Fin<Unit> Window(BoundingBox bounds, Context tolerance) =>
        guard(
                bounds.IsValid
                && bounds.Diagonal.X > tolerance.Absolute.Value
                && bounds.Diagonal.Y > tolerance.Absolute.Value,
                new GeometryFault.DegenerateInput(Kind.BoundingBox, -1, "window:degenerate").ToError())
            .ToFin();

    private static ClipperOffset Engine(OffsetPolicy policy, double scale) =>
        new(policy.MiterLimit, policy.ArcTolerance * scale, policy.PreserveCollinear, policy.ReverseSolution) {
            MergeGroups = policy.MergeGroups,
        };

    private static Fin<PathsD> UniformOffset(Seq<Loop> paths, double distance, OffsetPolicy policy, Op op) =>
        op.Finite(distance).Map(delta => Inflated(paths, delta, policy, Scale(paths[0].Tolerance)));

    private static Fin<PathsD> VariableOffset(Seq<Loop> paths, Arr<Arr<double>> distances, OffsetPolicy policy, Op op) =>
        from _ in guard(
                distances.Count == paths.Count
                && distances.Zip(paths, static (row, path) => row.Count == path.Count).ForAll(identity)
                && TensorPrimitives.IsFiniteAll(distances.Bind(static row => row).ToArray()),
                op.InvalidInput())
            .ToFin()
        let scale = Scale(paths[0].Tolerance)
        let parts = toSeq(ToPaths64(paths, scale))
            .Zip(distances.ToSeq())
            .Map(item => Inflated(item.First, item.Second, policy, scale))
        let merged = Clipper.Union(new Paths64(parts.Bind(static part => toSeq(part))), FillRule.NonZero)
        select Clipper.ScalePathsD(merged, 1.0 / scale);

    private static PathsD Inflated(Seq<Loop> paths, double delta, OffsetPolicy policy, double scale) {
        ClipperOffset engine = Engine(policy, scale);
        Paths64 result = [];
        engine.AddPaths(ToPaths64(paths, scale), policy.Join.Native, policy.End.Native);
        engine.Execute(delta * scale, result);
        return Clipper.ScalePathsD(result, 1.0 / scale);
    }

    // One registered path binds the callback row, so `current` stays the vertex index into that path's own delta row.
    private static Paths64 Inflated(Path64 path, Arr<double> distances, OffsetPolicy policy, double scale) {
        ClipperOffset engine = Engine(policy, scale);
        Paths64 result = [];
        engine.AddPath(path, policy.Join.Native, policy.End.Native);
        engine.Execute((_, _, current, _) => distances[current] * scale, result);
        return result;
    }

    private static Fin<PolyTreeD> TreeOf(PathsD paths, PolygonFill fill, Context tolerance, Op op) =>
        TreeOf(paths, null, PolygonBoolean.Union, fill, tolerance, op);

    private static Fin<PolyTreeD> TreeOf(PathsD subject, PathsD? clip, PolygonBoolean kind, PolygonFill fill, Context tolerance, Op op) {
        ClipperD engine = new(Precision(tolerance));
        PolyTreeD result = new();
        engine.AddSubject(subject);
        if (clip is not null) engine.AddClip(clip);
        return engine.Execute(kind.Native, fill.Native, result)
            ? Fin.Succ(result)
            : Fin.Fail<PolyTreeD>(op.InvalidResult(detail: "clipper:tree-execute"));
    }

    private static Fin<TopologyReceipt> TopologyOf(PolyTreeD tree, Context tolerance, double plane, PolygonFill fill, Op op) =>
        Descendants(tree, None, Seq<(PolyPathD Node, Option<int> Parent)>.Empty)
            .Map(static (row, index) => (row.Node, row.Parent, Index: index))
            .TraverseM(row => op.Need(row.Node.Polygon)
                .Bind(path => FromPath(path, closed: true, tolerance, plane)
                    .Map(loop => new RegionNode(
                        row.Index,
                        row.Parent,
                        row.Node.Level - 1,
                        row.Node.IsHole,
                        loop,
                        Clipper.Area(path),
                        loop.Bound()))))
            .As()
            .Map(nodes => new TopologyReceipt(nodes, fill, tolerance, plane));

    // Pre-order emission assigns each child its ordinal before descending, so a parent index needs no reference re-scan.
    private static Seq<(PolyPathD Node, Option<int> Parent)> Descendants(
        PolyPathD node,
        Option<int> parent,
        Seq<(PolyPathD Node, Option<int> Parent)> emitted) =>
        toSeq(node.Cast<PolyPathD>()).Fold(
            emitted,
            (rows, child) => Descendants(child, Some(rows.Count), rows.Add((Node: child, Parent: parent))));

    private static Fin<(Seq<Seq<Edge3>> Inside, Seq<Seq<Edge3>> Outside)> ClipRuns(
        Seq<Seq<Edge3>> subject,
        Seq<Loop> clip,
        PolygonFill fill,
        Op op) {
        ClipperD engine = new(Precision(clip[0].Tolerance));
        PathsD insideClosed = [];
        PathsD outsideClosed = [];
        PathsD inside = [];
        PathsD outside = [];
        engine.AddOpenSubject(ToOpenPaths(subject));
        engine.AddClip(ToPaths(clip));
        return engine.Execute(ClipType.Intersection, fill.Native, insideClosed, inside)
            && engine.Execute(ClipType.Difference, fill.Native, outsideClosed, outside)
            && insideClosed.Count == 0
            && outsideClosed.Count == 0
                ? Fin.Succ((Runs(inside, clip[0].Plane), Runs(outside, clip[0].Plane)))
                : Fin.Fail<(Seq<Seq<Edge3>>, Seq<Seq<Edge3>>)>(op.InvalidResult(detail: "clipper:open-partition"));
    }

    private static PolygonMeasure MeasureOf(TopologyReceipt topology) {
        Seq<Loop> paths = topology.Nodes.Map(static node => node.Boundary);
        PathsD native = ToPaths(paths);
        RectD bounds = Clipper.GetBounds(native);
        double signedArea = topology.Nodes.Fold(0.0, static (area, node) => area + node.SignedArea);
        double filledArea = topology.Nodes.Fold(
            0.0,
            static (area, node) => area + (node.IsHole ? -Math.Abs(node.SignedArea) : Math.Abs(node.SignedArea)));
        Point3d centroid = CentroidOf(paths, signedArea);
        return new PolygonMeasure(
            signedArea,
            filledArea,
            paths.Fold(0.0, static (length, path) => length + path.Length()),
            centroid,
            new BoundingBox(
                new Point3d(bounds.left, bounds.top, paths[0].Plane),
                new Point3d(bounds.right, bounds.bottom, paths[0].Plane)),
            topology.Nodes.Count(static node => !node.IsHole),
            topology.Nodes.Count(static node => node.IsHole));
    }

    private static Point3d CentroidOf(Seq<Loop> paths, double signedArea) {
        (double X, double Y, double Cross) moment = paths
            .Bind(path => Range(0, path.Spans).Map(index => (A: path.At(index), B: path.At(index + 1))).ToSeq())
            .Fold(
                (X: 0.0, Y: 0.0, Cross: 0.0),
                static (state, edge) => {
                    double cross = edge.A.X * edge.B.Y - edge.B.X * edge.A.Y;
                    return (state.X + (edge.A.X + edge.B.X) * cross, state.Y + (edge.A.Y + edge.B.Y) * cross, state.Cross + cross);
                });
        return Math.Abs(signedArea) > paths[0].Tolerance.Absolute.Value * paths[0].Tolerance.Absolute.Value
            ? new Point3d(moment.X / (3.0 * moment.Cross), moment.Y / (3.0 * moment.Cross), paths[0].Plane)
            : paths[0].Bound().Center;
    }

    private static Fin<PolygonTrace> PointsOf(Seq<Loop> paths, Arr<Point3d> points, PolygonFill fill) =>
        Defect(
                points.ToSeq(),
                point => !point.IsValid || Math.Abs(point.Z - paths[0].Plane) > paths[0].Tolerance.Absolute.Value,
                Kind.Point,
                "contains:off-plane")
            .As()
            .ToFin()
            .Map(_ => (PolygonTrace)new PolygonTrace.Related(
                points.Map(point => RelationOf(new PointD(point.X, point.Y), ToPaths(paths), fill, Precision(paths[0].Tolerance)))));

    private static PointRelation RelationOf(PointD point, PathsD paths, PolygonFill fill, int precision) {
        Seq<PointInPolygonResult> relations = toSeq(paths).Map(path => Clipper.PointInPolygon(point, path, precision));
        if (relations.Exists(static relation => relation == PointInPolygonResult.IsOn)) return PointRelation.Boundary;
        (int Crossings, int Winding) coverage = relations.Zip(toSeq(paths), static (relation, path) =>
                relation == PointInPolygonResult.IsInside
                    ? (Crossings: 1, Winding: Clipper.IsPositive(path) ? 1 : -1)
                    : (Crossings: 0, Winding: 0))
            .Fold((Crossings: 0, Winding: 0), static (state, item) =>
                (state.Crossings + item.Crossings, state.Winding + item.Winding));
        bool inside = fill.Switch(
            state: coverage,
            evenOdd: static value => value.Crossings % 2 == 1,
            nonZero: static value => value.Winding != 0,
            positive: static value => value.Winding > 0,
            negative: static value => value.Winding < 0);
        return inside ? PointRelation.Inside : PointRelation.Outside;
    }

    private static Fin<FieldReceipt> Raster(
        RasterKernel kernel,
        double[,] values,
        FieldGrid grid,
        FieldMetric metric,
        Context tolerance,
        double plane) {
        ParallelHelper.For2D(0, values.GetLength(0), 0, values.GetLength(1), in kernel);
        ReadOnlySpan<double> samples = MemoryMarshal.CreateReadOnlySpan(ref values[0, 0], values.Length);
        return TensorPrimitives.IsFiniteAll(samples)
            ? Fin.Succ(new FieldReceipt(
                values.AsMemory2D(),
                grid,
                metric,
                tolerance,
                plane,
                TensorPrimitives.Min(samples),
                TensorPrimitives.Max(samples),
                TensorPrimitives.Average(samples),
                TensorPrimitives.StdDev(samples),
                CellOf(grid, TensorPrimitives.IndexOfMin(samples)),
                CellOf(grid, TensorPrimitives.IndexOfMax(samples))))
            : Fin.Fail<FieldReceipt>(new GeometryFault.DegenerateInput(Kind.Plane, -1, "raster:non-finite-cell").ToError());
    }

    private static Point3d CellOf(FieldGrid grid, int index) => grid.Center(index / grid.Columns, index % grid.Columns);

    private readonly struct RasterKernel(
        double[,] values,
        PathsD paths,
        Arr<(PointD A, PointD B)> segments,
        PolygonFill fill,
        FieldGrid grid,
        int precision,
        FieldMetric metric) : IAction2D {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(int row, int column) {
            PointD point = ToPoint(grid.Center(row, column));
            PointRelation relation = RelationOf(point, paths, fill, precision);
            double magnitude = metric.NeedsClearance
                ? Math.Sqrt(segments.Fold(double.PositiveInfinity, (least, segment) => Math.Min(least, SquaredDistance(point, segment))))
                : 1.0;
            double signed = relation == PointRelation.Boundary ? 0.0 : relation == PointRelation.Inside ? -magnitude : magnitude;
            values[row, column] = metric.Sample(signed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double SquaredDistance(PointD point, (PointD A, PointD B) segment) {
            double dx = segment.B.x - segment.A.x;
            double dy = segment.B.y - segment.A.y;
            double length = dx * dx + dy * dy;
            double t = double.Clamp(((point.x - segment.A.x) * dx + (point.y - segment.A.y) * dy) / length, 0.0, 1.0);
            double x = segment.A.x + t * dx - point.x;
            double y = segment.A.y + t * dy - point.y;
            return x * x + y * y;
        }
    }

    private static Arr<(PointD A, PointD B)> Segments(Seq<Loop> paths) =>
        paths.Bind(path => Range(0, path.Spans).Map(index => (ToPoint(path.At(index)), ToPoint(path.At(index + 1)))).ToSeq()).ToArr();

    private static Fin<Loop> StripDuplicates(Loop path) {
        double scale = Scale(path.Tolerance);
        Path64 stripped = Clipper.StripDuplicates(Clipper.ScalePath64(ToPath(path), scale), path.Closed);
        return FromPath(Clipper.ScalePathD(stripped, 1.0 / scale), path.Closed, path.Tolerance, path.Plane);
    }

    private static int Precision(Context tolerance) =>
        int.Clamp((int)Math.Ceiling(-Math.Log10(tolerance.Absolute.Value)), -8, 8);

    private static double Scale(Context tolerance) => Math.Pow(10.0, Precision(tolerance));

    private static RectD RectOf(BoundingBox bounds) => new(bounds.Min.X, bounds.Min.Y, bounds.Max.X, bounds.Max.Y);

    private static Paths64 ToPaths64(Seq<Loop> paths, double scale) => Clipper.ScalePaths64(ToPaths(paths), scale);

    private static PathsD ToPaths(Seq<Loop> paths) => new(paths.Map(ToPath));

    private static PathsD ToOpenPaths(Seq<Seq<Edge3>> paths) => new(paths.Map(path =>
        new PathD(path.Map(edge => ToPoint(edge.A)).Concat(Seq(ToPoint(path.Last.B))))));

    private static PathD ToPath(Loop path) => new(path.Vertices.Map(ToPoint));

    private static PointD ToPoint(Point3d point) => new(point.X, point.Y);

    private static Fin<Seq<Loop>> FromPaths(PathsD paths, bool closed, Context tolerance, double plane) =>
        toSeq(paths).TraverseM(path => FromPath(path, closed, tolerance, plane)).As();

    private static Fin<Loop> FromPath(PathD path, bool closed, Context tolerance, double plane) =>
        Loop.Admit(
            toSeq(path).Map(point => new Point3d(point.x, point.y, plane)).ToArr(),
            closed,
            [],
            tolerance);

    private static Seq<Seq<Edge3>> Runs(PathsD paths, double plane) =>
        toSeq(paths).Filter(static path => path.Count >= 2).Map(path => Range(0, path.Count - 1)
            .Map(index => new Edge3(
                new Point3d(path[index].x, path[index].y, plane),
                new Point3d(path[index + 1].x, path[index + 1].y, plane)))
            .ToSeq());
}
```

## [03]-[FIELD_PLANE]

- Owner: `FieldGrid` admits the sampled window and explicit cell budget once, then derives `Rows`, `Columns`, and `Center`; `FieldMetric` owns each cell's scalar interpretation.
- Cases: occupancy derives fill membership; signed clearance derives boundary distance and sign; engagement derives cutter-radius overlap; reachability derives cutter-center admissibility at a tool radius; inscribed diameter doubles the nearest-boundary radius at interior cells — the last four from the same clearance field.
- Entry: `PolygonOp.Raster` consumes one admitted region set, fill rule, grid, and metric through `PolygonAlgebra.Apply`.
- Auto: `ParallelHelper.For2D` partitions cells over a package-required `IAction2D` kernel; `Memory2D<double>` materializes the plane, `ReadOnlyMemory2D<double>` publishes it, and `TensorPrimitives.IsFiniteAll`, `Min`, `Max`, `Average`, `StdDev`, `IndexOfMin`, and `IndexOfMax` derive the receipt statistics.
- Law: `FieldMetric.NeedsClearance` gates the per-cell nearest-segment fold, so an occupancy plane costs one containment test per cell while a clearance-derived plane pays the distance reduction only where its interpretation reads it.
- Receipt: `FieldReceipt` keeps the plane, grid, metric, finite extrema, dispersion, and the model-space cells holding those extrema together, so engagement, additive masks, and layer audits consume one substrate value; `MinimumAt` over a signed-clearance plane is the deepest interior cell, the largest inscribed disc a cutter can occupy.
- Growth: a new field interpretation is one `FieldMetric` case over the existing sampled clearance kernel, its admission row, and its clearance-need row.
- Boundary: field storage remains owned by the receipt, provider paths remain private, and a non-finite cell fails the whole projection.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
