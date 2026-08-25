# [RASM_FABRICATION_ATOMS]

`Process` atoms are the acyclic vocabulary floor every fabrication plane reads and that read nothing above themselves: arc-native profile geometry, admitted motion, decoded equipment, and the routed plan carriers. Each family admits at construction through its own generated `Validate`, so a plane holding an atom re-proves nothing and a provider type never crosses the floor.

`Loop.Apply` closes arc-native profile operations over one case family, reading ONE held polyline and ONE held index per loop. `Loop.CanonicalBytes` is the ONE loop preimage, rotation-canonical and tolerance-quantized, so two loops describing the same closed region under different vertex origins and windings mint one key, and `Loop.CanonicalOrder` is the ONE rank over that same normal form; a second loop preimage or sibling comparer anywhere in the package is the deleted form.

`Move` carries its endpoint once, projects its circular geometry once, admits through sealed case factories, and carries its continuous tool frame where the cut is oriented. `MotionDirective` preserves spindle law, dwell, channel synchronization, oriented stop, channel barriers, and admitted `SpecializedToolpathEnvelope` payloads beside atom-safe moves. `ToolEvidence` and `CutterIngress` carry already-decoded scalars, so no provider type reaches this floor.

## [01]-[INDEX]

- [02]-[GEOMETRY]: `BoolKind`, `LoopView`, `Loop`, `ProfileOp`, `ProfileResult`, `Edge3`, `RotationSense`, `ArcCenter`, `PartTransform`, `ProjectionDir`.
- [03]-[MOTION]: `SpindleControl`, `DwellBasis`, the specialized-row vocabularies, `SpecializedToolpathRow`, `SpecializedToolpathEnvelope`, `MotionDirective`, `MoveOrientation`, `Move`, `MotionEvidence`, `RunWarning`.
- [04]-[EQUIPMENT]: `ConsumableKey`, `CornerRule`, `TaperRule`, `TaperSource`, `CutterFamily`, `CutterMetric`, `ToolState`, `ToolLifeBasis`, `ToolLifeEvidence`, `FeedEnvelope`, `SpindleEnvelope`, `ToolEvidence`, `CutterIngress`, `CutterForm`.
- [05]-[PLAN]: `MachineInstanceKey`, `ComponentLayer`, `ComponentConnection`, `AdmittedComponent`, `ResidualStock`, `StockSnapshot`, `PlannedStep`, `CamPassPolicy`, `BendOrientation`, `BendStep`, `CapabilityAttestation`, `CapabilityVerdict`, `GougeWitness`, `InspectionMethod`, `InspectionFeature`.

## [02]-[GEOMETRY]

- Owner: `Loop` owns the arc-native closed or open chain and every profile query over it; `BoolKind` owns the Boolean posture with its provider code and truth function; `PartTransform` owns the nest placement map; `ProjectionDir` owns the orthonormal screen basis.
- Cases: `ProfileOp` carries each arc-native operation's evidence — measure, bound, containment, closest point, arc-length sample, single-loop offset, island-preserving shape offset, Boolean, intersection census, and containment relation.
- Entry: `Loop.Apply` is the sole profile-operation surface; input shape selects behavior. `Loop.Admit` is the sole construction, `Loop.Canonical` the sole identity normalization, and `Loop.CanonicalOrder` the sole sibling rank over that normal form.
- Law: the Boolean posture crosses every seam as `BoolKind`, so the provider ordinal stays a private column on the row and a plane above reaches a set operation without naming a `CavalierContours` type; the owned key is also what a preimage frames, under the folder ruling that a provider ordinal never enters one.
- Auto: one `Polyline<double>` and one `StaticAABB2DIndex<double>` are built per `Loop` and HELD, so a fold running measure, winding, offset, and Boolean over one loop pays one build rather than one per query; the held view is ignored by equality because it is derived from the admitted members.
- Law: island-preserving offset rides `Shape<double>.FromPlines(...).ParallelOffset(...)`, which offsets CCW outer and CW hole loops together; a per-loop `PlineOffset.ParallelOffset` over a forest loses the hole nesting and is the deleted form. A single loop with no islands keeps the single-polyline path, where the two agree.
- Output: `ProfileResult.Loops` carries rebuilt loops re-admitted through `Loop.Admit`, so a provider result that degenerated fails at the boundary rather than downstream.
- Packages: `CavalierContours` (`Polyline<double>`, `PlineOffset`, `PlineBoolean`, `PlineContains`, `Shape<double>`, `StaticAABB2DIndex<double>`), RhinoCommon value geometry, `UnitsNet` at the measure boundary.
- Boundary: containment, area, and winding are defined only over a CLOSED loop; an open chain has no interior and answers `Sign.Zero`, zero area, and false containment consistently. Provider geometry never leaves this cluster.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using CavalierContours.Core;
using CavalierContours.Polyline;
using CavalierContours.Shape;
using CavalierContours.Spatial;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Fabrication.Toolpath;
using Rasm.Meshing;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using BooleanOp = CavalierContours.Polyline.BooleanOp;
using TimeDuration = NodaTime.Duration;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Process;

// --- [MODELS] --------------------------------------------------------------------------
// --- [GEOMETRY]

[SmartEnum<string>]
public sealed partial class BoolKind {
    public static readonly BoolKind Or = new("or", BooleanOp.Or, static (subject, clip) => subject || clip);
    public static readonly BoolKind And = new("and", BooleanOp.And, static (subject, clip) => subject && clip);
    public static readonly BoolKind Not = new("not", BooleanOp.Not, static (subject, clip) => subject && !clip);
    public static readonly BoolKind Xor = new("xor", BooleanOp.Xor, static (subject, clip) => subject ^ clip);

    internal BooleanOp Native { get; }
    private Func<bool, bool, bool> Rule { get; }

    internal bool Includes(bool subject, bool clip) => Rule(subject, clip);
}

public readonly record struct LoopView(Polyline<double> Pline, StaticAABB2DIndex<double> Index);

[ComplexValueObject]
public sealed partial class Loop {
    public Arr<Point3d> Vertices { get; }
    public bool Closed { get; }
    public Arr<double> Bulges { get; }
    public Context Tolerance { get; }

    [IgnoreMember]
    private LoopView? view;

    public int Count => Vertices.Count;
    public int Spans => Closed ? Count : Count - 1;
    public double Plane => Vertices[0].Z;
    public Point3d At(int i) => Vertices[((i % Count) + Count) % Count];
    public double BulgeAt(int i) => Bulges.IsEmpty ? 0.0 : Bulges[((i % Count) + Count) % Count];

    internal LoopView View => view ??= Built(Vertices, Bulges, Closed);

    private static LoopView Built(Arr<Point3d> vertices, Arr<double> bulges, bool closed) {
        Polyline<double> pline = PlineOf(vertices, bulges, closed);
        return new LoopView(pline, pline.CreateAabbIndex());
    }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Arr<Point3d> vertices,
        ref bool closed,
        ref Arr<double> bulges,
        ref Context tolerance) {
        bulges = bulges.IsEmpty ? Range(0, vertices.Count).ToSeq().Map(static _ => 0.0).ToArr() : bulges;
        if (!Valid(vertices, closed, bulges, tolerance))
            validationError = new ValidationError(string.Join(" | ", new object?[] { Kind.Polyline, None, "loop:degenerate" }));
    }

    public static Fin<Loop> Admit(Arr<Point3d> vertices, bool closed, Arr<double> bulges, Context tolerance) =>
        Validate(vertices, closed, bulges, tolerance, out Loop loop).Admitted(loop);

    public Sign Winding() => !Closed ? Sign.Zero : View.Pline.Area() switch {
        > 0.0 => Sign.Positive,
        < 0.0 => Sign.Negative,
        _ => Sign.Zero,
    };

    public double Area() => Closed ? View.Pline.Area() : 0.0;
    public double Length() => View.Pline.PathLength();

    public Loop AsCcw() {
        if (Winding() != Sign.Negative) return this;
        Polyline<double> reversed = new(View.Pline.IterVertexes(), View.Pline.IsClosed);
        reversed.InvertDirection();
        Seq<PlineVertex<double>> vertices = toSeq(reversed.IterVertexes());
        return new Loop(
            vertices.Map(vertex => new Point3d(vertex.X, vertex.Y, Plane)).ToArr(),
            reversed.IsClosed,
            vertices.Map(static vertex => vertex.Bulge).ToArr(),
            Tolerance);
    }

    public Loop Canonical() {
        Loop oriented = Closed ? AsCcw() : Directed();
        return oriented.Closed ? oriented.RotatedTo(oriented.LeastVertex()) : oriented;
    }

    public static IComparer<Loop> CanonicalOrder { get; } = Comparer<Loop>.Create(static (left, right) =>
        left.Closed != right.Closed ? left.Closed.CompareTo(right.Closed)
        : left.Count != right.Count ? left.Count.CompareTo(right.Count)
        : toSeq(Range(0, left.Count))
            .Map(index => Compare(Quantized(left.At(index), left.Tolerance), Quantized(right.At(index), right.Tolerance))
                is int order and not 0
                    ? order
                    : left.BulgeAt(index).CompareTo(right.BulgeAt(index)))
            .Find(static order => order != 0)
            .IfNone(0));

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) {
        Loop canon = Canonical();
        return writer.Bool(canon.Closed).Double(canon.Tolerance.Absolute.Value).Ordinal(canon.Count)
            .Rows(toSeq(Range(0, canon.Count)), (row, index) =>
                row.Coords(Quantized(canon.At(index), canon.Tolerance)).Double(canon.BulgeAt(index)));
    }

    public BoundingBox Bound() => View.Pline.Extents() is { } bounds
        ? new BoundingBox(
            new Point3d(bounds.MinX, bounds.MinY, Vertices.Min(static point => point.Z)),
            new Point3d(bounds.MaxX, bounds.MaxY, Vertices.Max(static point => point.Z)))
        : BoundingBox.Empty;

    public bool Covers(Point3d point) =>
        Closed && View.Pline.WindingNumber(new Vector2<double>(point.X, point.Y)) != 0;

    public Fin<Loop> RotateStart(int segment, Point3d point) =>
        View.Pline.RotateStart(segment, new Vector2<double>(point.X, point.Y), Tolerance.Absolute.Value) is { } rotated
            ? Rebuilt(rotated, this)
            : Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "loop:rotate-start"));

    public Fin<ProfileResult> Apply(ProfileOp operation) => operation.Switch(
        state: this,
        measure: static loop => Fin.Succ<ProfileResult>(new ProfileResult.Measure(
            UnitsNet.Area.FromSquareMillimeters(loop.Area()),
            UnitsNet.Length.FromMillimeters(loop.Length()),
            loop.Winding())),
        bound: static loop => Fin.Succ<ProfileResult>(new ProfileResult.Bound(loop.Bound())),
        contains: static (loop, op) => Fin.Succ<ProfileResult>(new ProfileResult.Contains(loop.Covers(op.Point))),
        closest: static (loop, op) => loop.View.Pline.ClosestPoint(
            new Vector2<double>(op.Point.X, op.Point.Y),
            loop.Tolerance.Absolute.Value) is { } closest
                ? Fin.Succ<ProfileResult>(new ProfileResult.Closest(closest))
                : Fin.Fail<ProfileResult>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "loop:closest")),
        sample: static (loop, op) => loop.View.Pline.FindPointAtPathLength(op.At.Millimeters) switch {
            (true, int segment, Vector2<double> point, double accumulated) => Fin.Succ<ProfileResult>(new ProfileResult.Sampled(
                segment,
                new Point3d(point.X, point.Y, loop.Plane),
                UnitsNet.Length.FromMillimeters(accumulated))),
            _ => Fin.Fail<ProfileResult>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "loop:sample")),
        },
        offset: static (loop, op) => loop.Offset(op.Distance.Millimeters),
        offsetShape: static (loop, op) => loop.OffsetShape(op.Islands, op.Distance.Millimeters),
        boolean: static (loop, op) => loop.Boolean(op.Other, op.Kind),
        intersections: static (loop, op) => Fin.Succ<ProfileResult>(loop.Intersections(op.Other)),
        relation: static (loop, op) => Fin.Succ<ProfileResult>(new ProfileResult.Relation(PlineContains.PolylineContains(
            loop.View.Pline, op.Other.View.Pline, new PlineContainsOptions<double> { PosEqualEps = loop.Tolerance.Absolute.Value }))));

    private Loop Directed() =>
        Compare(Quantized(Vertices[Count - 1], Tolerance), Quantized(Vertices[0], Tolerance)) < 0 ? Reversed() : this;

    private Loop Reversed() => new(
        toSeq(Vertices).Rev().ToArr(),
        Closed,
        toSeq(Range(0, Count)).Map(index => -BulgeAt(Count - 1 - index)).ToArr(),
        Tolerance);

    private int LeastVertex() => toSeq(Range(0, Count))
        .Fold(0, (best, index) =>
            Compare(Quantized(At(index), Tolerance), Quantized(At(best), Tolerance)) < 0 ? index : best);

    private Loop RotatedTo(int start) => new(
        toSeq(Range(0, Count)).Map(index => At(start + index)).ToArr(),
        Closed,
        toSeq(Range(0, Count)).Map(index => BulgeAt(start + index)).ToArr(),
        Tolerance);

    private static (double X, double Y, double Z) Quantized(Point3d point, Context tolerance) {
        double grid = tolerance.Absolute.Value;
        return (Math.Round(point.X / grid) * grid, Math.Round(point.Y / grid) * grid, Math.Round(point.Z / grid) * grid);
    }

    private static int Compare((double X, double Y, double Z) first, (double X, double Y, double Z) second) =>
        first.X != second.X ? first.X.CompareTo(second.X)
        : first.Y != second.Y ? first.Y.CompareTo(second.Y)
        : first.Z.CompareTo(second.Z);

    private static bool Valid(Arr<Point3d> vertices, bool closed, Arr<double> bulges, Context tolerance) =>
        vertices.Count >= (closed ? 3 : 2)
        && bulges.Count == vertices.Count
        && (closed || bulges[vertices.Count - 1] == 0.0)
        && vertices.ForAll(static point => point.IsValid)
        && bulges.ForAll(double.IsFinite)
        && vertices.ForAll(point => Math.Abs(point.Z - vertices[0].Z) <= tolerance.Absolute.Value)
        && Range(0, closed ? vertices.Count : vertices.Count - 1).ForAll(index =>
            vertices[index].DistanceTo(vertices[(index + 1) % vertices.Count]) > tolerance.Absolute.Value)
        && (!closed || Math.Abs(PlineOf(vertices, bulges, closed).Area()) > tolerance.Absolute.Value * tolerance.Absolute.Value);

    private static Polyline<double> PlineOf(Arr<Point3d> vertices, Arr<double> bulges, bool closed) =>
        new(toSeq(vertices).Map((point, index) => PlineVertex<double>.FromVector2(new Vector2<double>(point.X, point.Y), bulges[index])), closed);

    private Fin<ProfileResult> Offset(double millimeters) => FromPlines(
        PlineOffset.ParallelOffset<Polyline<double>, double>(View.Pline, millimeters, OffsetOptions()), this);

    private Fin<ProfileResult> OffsetShape(Arr<Loop> islands, double millimeters) {
        double eps = Tolerance.Absolute.Value;
        Shape<double> offset = Shape<double>
            .FromPlines(toSeq(islands).Map(static island => island.View.Pline).Prepend(View.Pline))
            .ParallelOffset(millimeters, new ShapeOffsetOptions<double>(
                posEqualEps: eps, offsetDistEps: eps, sliceJoinEps: eps));
        return FromPlines(
            toSeq(offset.CcwPlines).Concat(toSeq(offset.CwPlines)).Map(static indexed => indexed.Polyline), this);
    }

    private PlineOffsetOptions<double> OffsetOptions() => new() {
        AabbIndex = View.Index,
        HandleSelfIntersects = true,
        OffsetDistEps = Tolerance.Absolute.Value,
        PosEqualEps = Tolerance.Absolute.Value,
        SliceJoinEps = Tolerance.Absolute.Value,
    };

    private Fin<ProfileResult> Boolean(Loop other, BoolKind kind) =>
        Math.Abs(Plane - other.Plane) > Tolerance.Absolute.Value || Tolerance != other.Tolerance
            ? Fin.Fail<ProfileResult>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "loop:boolean-context"))
            : FromPlines(
                toSeq(PlineBoolean.PolylineBoolean<Polyline<double>, double>(
                        View.Pline,
                        other.View.Pline,
                        kind.Native,
                        new PlineBooleanOptions<double> {
                            PosEqualEps = Tolerance.Absolute.Value,
                            CollapsedAreaEps = Tolerance.Absolute.Value * Tolerance.Absolute.Value,
                            Pline1AabbIndex = View.Index,
                        }).PosPlines)
                    .Map(static row => row.Pline),
                this);

    private ProfileResult Intersections(Loop other) {
        PlineIntersectsCollection<double> intersections = PlineBoolean.FindIntersects(
            View.Pline,
            other.View.Pline,
            new FindIntersectsOptions<double> { Pline1AabbIndex = View.Index, PosEqualEps = Tolerance.Absolute.Value });
        return new ProfileResult.Intersections(intersections.BasicIntersects.Count, intersections.OverlappingIntersects.Count);
    }

    private static Fin<ProfileResult> FromPlines(IEnumerable<Polyline<double>> sources, Loop basis) =>
        toSeq(sources)
            .Traverse(source => Rebuilt(source, basis))
            .As()
            .Map(static loops => (ProfileResult)new ProfileResult.Loops(loops));

    private static Fin<Loop> Rebuilt(Polyline<double> source, Loop basis) {
        Seq<PlineVertex<double>> vertices = toSeq(source.IterVertexes());
        return Admit(
            vertices.Map(vertex => new Point3d(vertex.X, vertex.Y, basis.Plane)).ToArr(),
            source.IsClosed,
            vertices.Map(static vertex => vertex.Bulge).ToArr(),
            basis.Tolerance);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProfileOp {
    private ProfileOp() { }

    public sealed record Measure : ProfileOp;
    public sealed record Bound : ProfileOp;
    public sealed record Contains(Point3d Point) : ProfileOp;
    public sealed record Closest(Point3d Point) : ProfileOp;
    public sealed record Sample(UnitsNet.Length At) : ProfileOp;
    public sealed record Offset(UnitsNet.Length Distance) : ProfileOp;
    public sealed record OffsetShape(Arr<Loop> Islands, UnitsNet.Length Distance) : ProfileOp;
    public sealed record Boolean(Loop Other, BoolKind Kind) : ProfileOp;
    public sealed record Intersections(Loop Other) : ProfileOp;
    public sealed record Relation(Loop Other) : ProfileOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProfileResult {
    private ProfileResult() { }

    public sealed record Measure(UnitsNet.Area SignedArea, UnitsNet.Length Path, Sign Winding) : ProfileResult;
    public sealed record Bound(BoundingBox Box) : ProfileResult;
    public sealed record Contains(bool Value) : ProfileResult;
    public sealed record Closest(ClosestPointResult<double> Value) : ProfileResult;
    public sealed record Sampled(int Segment, Point3d Point, UnitsNet.Length Accumulated) : ProfileResult;
    public sealed record Loops(Seq<Loop> Values) : ProfileResult;
    public sealed record Intersections(int Points, int Overlaps) : ProfileResult;
    public sealed record Relation(PlineContainsResult Value) : ProfileResult;
}

public readonly record struct Edge3(Point3d A, Point3d B);

[SmartEnum<string>]
public sealed partial class RotationSense {
    public static readonly RotationSense Clockwise = new("clockwise");
    public static readonly RotationSense Counterclockwise = new("counterclockwise");

    public RotationSense Flipped => Switch(
        clockwise: static _ => Counterclockwise,
        counterclockwise: static _ => Clockwise);
}

public readonly record struct ArcCenter(Point3d Center, RotationSense Sense);

[ComplexValueObject]
public sealed partial class PartTransform {
    public int PartId { get; }
    public int Instance { get; }
    public double Tx { get; }
    public double Ty { get; }
    public double RotationRadians { get; }
    public int SheetIndex { get; }

    public bool Mirrored { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int partId,
        ref int instance,
        ref double tx,
        ref double ty,
        ref double rotationRadians,
        ref int sheetIndex,
        ref bool mirrored) {
        if (partId < 0 || instance < 0 || sheetIndex < 0
            || !double.IsFinite(tx) || !double.IsFinite(ty) || !double.IsFinite(rotationRadians))
            validationError = new ValidationError("part-transform");
    }

    public static Fin<PartTransform> Admit(
        int partId, int instance, double tx, double ty, double rotationRadians, int sheetIndex, bool mirrored) =>
        Validate(partId, instance, tx, ty, rotationRadians, sheetIndex, mirrored, out PartTransform transform)
            .Admitted(transform);

    public Point3d Apply(Point3d point) {
        double x = Mirrored ? -point.X : point.X;
        return new Point3d(
            (x * Math.Cos(RotationRadians)) - (point.Y * Math.Sin(RotationRadians)) + Tx,
            (x * Math.Sin(RotationRadians)) + (point.Y * Math.Cos(RotationRadians)) + Ty,
            point.Z);
    }

    public Fin<Loop> Apply(Loop source) =>
        Loop.Admit(
            source.Vertices.Map(Apply),
            source.Closed,
            Mirrored ? source.Bulges.Map(static bulge => -bulge) : source.Bulges,
            source.Tolerance);

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) =>
        writer.Ordinal(PartId).Ordinal(Instance).Ordinal(SheetIndex)
            .Double(Tx).Double(Ty).Double(RotationRadians).Bool(Mirrored);
}

[ComplexValueObject]
public sealed partial class ProjectionDir {
    public Vector3d Forward { get; }
    public Vector3d ScreenU { get; }
    public Vector3d ScreenV { get; }

    private const double Orthogonal = 1e-9;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Vector3d forward,
        ref Vector3d screenU,
        ref Vector3d screenV) {
        if (!(forward.IsValid && screenU.IsValid && screenV.IsValid
            && Math.Abs(forward.Length - 1.0) <= Orthogonal
            && Math.Abs(screenU.Length - 1.0) <= Orthogonal
            && Math.Abs(screenV.Length - 1.0) <= Orthogonal
            && Math.Abs(forward * screenU) <= Orthogonal
            && Math.Abs(forward * screenV) <= Orthogonal
            && Math.Abs(screenU * screenV) <= Orthogonal
            && Math.Abs((Vector3d.CrossProduct(screenU, screenV) * forward) - 1.0) <= Orthogonal))
            validationError = new ValidationError(string.Join(" | ", new object?[] { Kind.Plane, None, "projection-dir:basis" }));
    }

    public static Fin<ProjectionDir> Of(Vector3d forward) =>
        Basis(forward).Match(
            Some: basis => Validate(basis.Forward, basis.ScreenU, basis.ScreenV, out ProjectionDir view).Admitted(view),
            None: () => Fin.Fail<ProjectionDir>(
                new GeometryFault.DegenerateInput(Kind.Plane, None, "projection-dir:forward")));

    public Point3d Project(Point3d point) {
        Vector3d radius = point - Point3d.Origin;
        return new Point3d(radius * ScreenU, radius * ScreenV, radius * Forward);
    }

    public Point3d Unproject(Point3d screen) =>
        Point3d.Origin + (ScreenU * screen.X) + (ScreenV * screen.Y) + (Forward * screen.Z);

    private static Option<(Vector3d Forward, Vector3d ScreenU, Vector3d ScreenV)> Basis(Vector3d forward) {
        Vector3d normal = forward;
        if (!normal.Unitize()) return None;

        Vector3d reference = Math.Abs(normal.Z) < 0.9 ? Vector3d.ZAxis : Vector3d.XAxis;
        Vector3d screenU = Vector3d.CrossProduct(reference, normal);
        return screenU.Unitize()
            ? Some((normal, screenU, Vector3d.CrossProduct(normal, screenU)))
            : None;
    }
}
```

## [03]-[MOTION]

- Owner: `Move` owns the admitted endpoint, its intrinsic circular geometry, and the continuous tool frame an oriented cut carries; `MotionDirective` owns executable non-Cartesian semantics; `SpecializedToolpathEnvelope` owns the admitted specialized-row payload; `MotionEvidence` owns one joint row and duration per motion target.
- Cases: every `Move` case inherits `Target` and `Orientation`, and `Move.Circular` carries feed, centre, sense, and intrinsic signed sweep; `MotionDirective` carries spindle law with its direction and ceiling, a basis-carrying dwell, a synchronized channel pair, an oriented stop with its orient angle, a channel barrier, and an admitted `SpecializedToolpathEnvelope`; `SpecializedToolpathRow` preserves wire, bevel, link, inspection, and turning evidence through one case-owned toolpath-kind column.
- Entry: `Move.Rapid.Of`, `Move.Linear.Of`, and `Move.Circular.Of` are the ONLY constructions — every case constructor is private, so admission runs BEFORE the value exists and no caller holds an unvalidated move. `Move.Transformed` re-seats an admitted move under a placement without re-admission, because an affine placement preserves every admitted invariant and mirrors the sweep sign with the point map. `Move.Admit` is the ONE re-proof a plane receiving a move across a seam runs — each case re-enters its own factory — so a consumer never spells a per-case admission ladder.
- Law: an admitted `Move` with no `Orientation` is AXIS-FREE, so its planar swept solid is exact; a consumer computing a planar sweep over an oriented move refuses with `FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "swept-solid:oriented-move")` rather than approximating the tilt silently. Indexed three-plus-two work carries no orientation here — its frame belongs to the setup, not the move.
- Auto: `SpecializedToolpathEnvelope.Admit` folds payload validity ONCE — kind correspondence across every row, non-empty rows, finite non-negative duration — so a consumer holding the admitted payload revalidates nothing.
- Output: `MotionEvidence.Warnings` carries typed `RunWarning` rows naming the raising plane and its locus, so the `rasm.fabrication.run.warnings` instrument partitions by concern instead of counting opaque text.
- Growth: a new specialized lane is one `SpecializedToolpathKind` row and one `SpecializedToolpathRow` case; a new controller semantic is one `MotionDirective` case, and the dialect owns its spelling.
- Boundary: closed row vocabularies are `[SmartEnum<string>]` rows; a unit-suffixed bare double stays bare where `CanonicalWriter` digests it, under the folder ruling that lifting a digested scalar to a typed quantity forks every key already minted.

```csharp
// --- [MOTION]
[SmartEnum<string>]
public sealed partial class SpindleControl {
    public static readonly SpindleControl ConstantRpm = new("constant-rpm");
    public static readonly SpindleControl ConstantSurface = new("constant-surface");
}

[SmartEnum<string>]
public sealed partial class DwellBasis {
    public static readonly DwellBasis Seconds = new("seconds");
    public static readonly DwellBasis Revolutions = new("revolutions");
}

[SmartEnum<string>]
public sealed partial class WireActionKind {
    public static readonly WireActionKind Access = new("access");
    public static readonly WireActionKind Cut = new("cut");
    public static readonly WireActionKind Bridge = new("bridge");
    public static readonly WireActionKind Handoff = new("handoff");
}

[SmartEnum<string>]
public sealed partial class LinkTransition {
    public static readonly LinkTransition Direct = new("direct");
    public static readonly LinkTransition Retract = new("retract");
    public static readonly LinkTransition Clearance = new("clearance");
    public static readonly LinkTransition ToolChange = new("tool-change");
    public static readonly LinkTransition SetupChange = new("setup-change");
}

[SmartEnum<string>]
public sealed partial class ThreadForm {
    public static readonly ThreadForm Metric = new("metric");
    public static readonly ThreadForm Unified = new("unified");
    public static readonly ThreadForm Trapezoidal = new("trapezoidal");
    public static readonly ThreadForm Acme = new("acme");
    public static readonly ThreadForm Buttress = new("buttress");
    public static readonly ThreadForm Round = new("round");
    public static readonly ThreadForm Pipe = new("pipe");
}

[SmartEnum<string>]
public sealed partial class ThreadHand {
    public static readonly ThreadHand Right = new("right");
    public static readonly ThreadHand Left = new("left");
}

[SmartEnum<string>]
public sealed partial class AxialKind {
    public static readonly AxialKind Drill = new("drill");
    public static readonly AxialKind Peck = new("peck");
    public static readonly AxialKind Bore = new("bore");
    public static readonly AxialKind Ream = new("ream");
    public static readonly AxialKind Countersink = new("countersink");
    public static readonly AxialKind Counterbore = new("counterbore");
}

[SmartEnum<string>]
public sealed partial class KnurlPattern {
    public static readonly KnurlPattern Straight = new("straight");
    public static readonly KnurlPattern Diamond = new("diamond");
    public static readonly KnurlPattern Diagonal = new("diagonal");
}

[SmartEnum<string>]
public sealed partial class HandoffKind {
    public static readonly HandoffKind Transfer = new("transfer");
    public static readonly HandoffKind CutoffTransfer = new("cutoff-transfer");
    public static readonly HandoffKind Handoff = new("handoff");
    public static readonly HandoffKind CutoffHandoff = new("cutoff-handoff");
}

[SmartEnum<string>]
public sealed partial class SpecializedToolpathKind {
    public static readonly SpecializedToolpathKind Wire = new("wire");
    public static readonly SpecializedToolpathKind Bevel = new("bevel");
    public static readonly SpecializedToolpathKind Link = new("link");
    public static readonly SpecializedToolpathKind Inspection = new("inspection");
    public static readonly SpecializedToolpathKind Turning = new("turning");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpecializedToolpathRow(SpecializedToolpathKind ToolpathKind) {
    public sealed record Wire(
        int Pass, double Station, double Progress, double TraversedMm,
        Point3d Lower, Point3d Upper, WireActionKind Action, double LagMm,
        double UpperCornerRadiusMm, Option<double> RotaryDeg) : SpecializedToolpathRow(SpecializedToolpathKind.Wire);
    public sealed record Bevel(
        int Move, int Pass, double Station, int SourceSpan, double SourceBulge,
        Point3d Point, Vector3d ToolAxis, Point3d Pivot, double AngleDeg,
        double CrossTiltDeg, double FeedMmPerMin, double CompensationMm) : SpecializedToolpathRow(SpecializedToolpathKind.Bevel);
    public sealed record Link(
        string From, string To, LinkTransition Transition, double DistanceMm,
        double DurationSeconds, double LiftMm, double ThermalExposure,
        double RotationPenalty, int Retracts, int Pierces,
        int ToolChanges, int SetupChanges) : SpecializedToolpathRow(SpecializedToolpathKind.Link);
    public sealed record Inspection(
        int Pass, int FromBlock, int ToBlockExclusive,
        double NominalAngleDeg, double NominalOffsetMm,
        double AngleDeviationDeg, double OffsetDeviationMm,
        bool Conforming) : SpecializedToolpathRow(SpecializedToolpathKind.Inspection);
    public sealed record TurningThread(
        ThreadForm Form, double LoadFlankDeg, double ClearanceFlankDeg,
        double CrestFlat, double RootFlat, double CrestRadius,
        double RootRadius, CutSide Side) : SpecializedToolpathRow(SpecializedToolpathKind.Turning);
    public sealed record TurningAxial(
        int FromMove, int ToMove, AxialKind Kind,
        double Diameter, double Depth, double TipAngleDeg) : SpecializedToolpathRow(SpecializedToolpathKind.Turning);
    public sealed record TurningTap(
        int FromMove, int ToMove, double Diameter, double Depth,
        double Pitch, ThreadForm Form, ThreadHand Hand) : SpecializedToolpathRow(SpecializedToolpathKind.Turning);
    public sealed record TurningKnurl(
        int FromMove, int ToMove, KnurlPattern Pattern, double Pressure) : SpecializedToolpathRow(SpecializedToolpathKind.Turning);
    public sealed record TurningHandoff(
        HandoffKind Kind, string From, string To,
        double GripPlane, double GripLength, double PullDistance) : SpecializedToolpathRow(SpecializedToolpathKind.Turning);
}

public sealed record SpecializedToolpathEnvelope {
    private SpecializedToolpathEnvelope(
        SpecializedToolpathKind kind,
        Seq<SpecializedToolpathRow> rows,
        double durationSeconds) => (Kind, Rows, DurationSeconds) = (kind, rows, durationSeconds);

    public SpecializedToolpathKind Kind { get; }
    public Seq<SpecializedToolpathRow> Rows { get; }
    public double DurationSeconds { get; }

    public static Fin<SpecializedToolpathEnvelope> Admit(
        SpecializedToolpathKind kind,
        Seq<SpecializedToolpathRow> rows,
        double durationSeconds) =>
        (AdmissionSlots.Gate(!rows.IsEmpty, kind, "rows", Refusal),
         AdmissionSlots.Gate(double.IsFinite(durationSeconds) && durationSeconds >= 0.0, kind, "duration", Refusal),
         AdmissionSlots.Gate(rows.ForAll(row => row.ToolpathKind == kind), kind, "kind", Refusal))
            .Apply(static (_, _, _) => unit)
            .As()
            .ToFin()
            .Map(_ => new SpecializedToolpathEnvelope(kind, rows, durationSeconds));

    private static FabricationFault Refusal(SpecializedToolpathKind kind, string slot) =>
        FabricationFault.Inadmissible(FabConcern.Toolpath, $"specialized-envelope:{kind.Key}:{slot}");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MotionDirective {
    private MotionDirective() { }

    public sealed record Spindle(
        SpindleControl Control,
        RotationSense Hand,
        double SurfaceMetersPerMinute,
        double ResolvedRpm,
        Option<double> CeilingRpm) : MotionDirective;
    public sealed record Dwell(int AfterMove, DwellBasis Basis, double Amount) : MotionDirective;
    public sealed record Synchronize(int FromMove, int ToMove, double Rpm, double Lead, RotationSense Hand) : MotionDirective;
    public sealed record OrientedStop(int AfterMove, double OrientDeg, Vector3d Retract) : MotionDirective;
    public sealed record ChannelBarrier(int Step, string Channel, Seq<string> WaitFor, Option<string> Signal) : MotionDirective;
    public sealed record Specialized(int AfterMove, SpecializedToolpathEnvelope Payload) : MotionDirective;

    public int AfterMove => Switch(
        spindle: static _ => -1,
        dwell: static row => row.AfterMove,
        synchronize: static row => row.ToMove,
        orientedStop: static row => row.AfterMove,
        channelBarrier: static row => row.Step,
        specialized: static row => row.AfterMove);
}

public sealed record MoveOrientation(Vector3d AxisAtStart, Vector3d AxisAtEnd, Option<Point3d> Contact) {
    public bool Valid =>
        AxisAtStart.IsValid && AxisAtEnd.IsValid
        && Math.Abs(AxisAtStart.Length - 1.0) <= 1e-9 && Math.Abs(AxisAtEnd.Length - 1.0) <= 1e-9
        && Contact.Map(static point => point.IsValid).IfNone(true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Move {
    private Move(Point3d target, Option<MoveOrientation> orientation) =>
        (Target, Orientation) = (target, orientation);

    public Point3d Target { get; }
    public Option<MoveOrientation> Orientation { get; }

    public bool AxisFree => Orientation.IsNone;

    public sealed record Rapid : Move {
        private Rapid(Point3d target, Option<MoveOrientation> orientation) : base(target, orientation) { }

        public static Fin<Move> Of(Point3d target, Option<MoveOrientation> orientation = default) =>
            Admitted(target, orientation, "move:rapid", Kind.Point, static (at, frame) => new Rapid(at, frame));
    }

    public sealed record Linear : Move {
        private Linear(Point3d target, Option<MoveOrientation> orientation, double feed)
            : base(target, orientation) => Feed = feed;

        public double Feed { get; }

        public static Fin<Move> Of(Point3d target, double feed, Option<MoveOrientation> orientation = default) =>
            double.IsFinite(feed) && feed > 0.0
                ? Admitted(target, orientation, "move:linear", Kind.Point, (at, frame) => new Linear(at, frame, feed))
                : Degenerate(Kind.Point, "move:linear");
    }

    public sealed record Circular : Move {
        private Circular(Point3d target, Option<MoveOrientation> orientation, double feed, ArcCenter arc, double sweepRadians)
            : base(target, orientation) => (Feed, Arc, SweepRadians) = (feed, arc, sweepRadians);

        public double Feed { get; }
        public ArcCenter Arc { get; }
        public double SweepRadians { get; }
        public double Radius => Arc.Center.DistanceTo(Target);

        public static Fin<Move> Of(
            Point3d target, double feed, ArcCenter arc, double sweepRadians, Option<MoveOrientation> orientation = default) =>
            arc.Center.IsValid && arc.Center.DistanceTo(target) > 0.0
            && double.IsFinite(feed) && feed > 0.0
            && double.IsFinite(sweepRadians) && Math.Abs(sweepRadians) is > 0.0 and <= Math.Tau
            && (arc.Sense == RotationSense.Clockwise ? sweepRadians < 0.0 : sweepRadians > 0.0)
                ? Admitted(target, orientation, "move:circular", Kind.Arc,
                    (at, frame) => new Circular(at, frame, feed, arc, sweepRadians))
                : Degenerate(Kind.Arc, "move:circular");
    }

    public Option<Circular> CircularGeometry => Switch(
        rapid: static _ => Option<Circular>.None,
        linear: static _ => Option<Circular>.None,
        circular: static move => Some(move));

    public Move Transformed(PartTransform placement) => Switch(
        state: placement,
        rapid: static (at, move) => (Move)new Rapid(at.Apply(move.Target), move.Orientation),
        linear: static (at, move) => new Linear(at.Apply(move.Target), move.Orientation, move.Feed),
        circular: static (at, move) => new Circular(
            at.Apply(move.Target),
            move.Orientation,
            move.Feed,
            new ArcCenter(at.Apply(move.Arc.Center), at.Mirrored ? move.Arc.Sense.Flipped : move.Arc.Sense),
            at.Mirrored ? -move.SweepRadians : move.SweepRadians));

    public static Fin<Move> Admit(Move move) => move.Switch(
        rapid: static row => Rapid.Of(row.Target, row.Orientation),
        linear: static row => Linear.Of(row.Target, row.Feed, row.Orientation),
        circular: static row => Circular.Of(row.Target, row.Feed, row.Arc, row.SweepRadians, row.Orientation));

    private static Fin<Move> Admitted(
        Point3d target,
        Option<MoveOrientation> orientation,
        string locus,
        Kind kind,
        Func<Point3d, Option<MoveOrientation>, Move> seat) =>
        target.IsValid && orientation.Map(static frame => frame.Valid).IfNone(true)
            ? Fin.Succ(seat(target, orientation))
            : Degenerate(kind, locus);

    private static Fin<Move> Degenerate(Kind kind, string locus) =>
        Fin.Fail<Move>(new GeometryFault.DegenerateInput(kind, None, locus));
}

public sealed record RunWarning(FabConcern Raised, string Locus, string Detail);

[ComplexValueObject]
public sealed partial class MotionEvidence {
    public Seq<Arr<double>> Joints { get; }
    public Seq<TimeDuration> SegmentDurations { get; }
    public TimeDuration Cycle { get; }
    public Seq<string> ControllerCode { get; }
    public Seq<RunWarning> Warnings { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<Arr<double>> joints,
        ref Seq<TimeDuration> segmentDurations,
        ref TimeDuration cycle,
        ref Seq<string> controllerCode,
        ref Seq<RunWarning> warnings) {
        if (joints.IsEmpty
            || !joints.ForAll(static row => !row.IsEmpty && row.ForAll(double.IsFinite))
            || joints.Count != segmentDurations.Count
            || !segmentDurations.ForAll(static duration => duration >= TimeDuration.Zero)
            || cycle < TimeDuration.Zero)
            validationError = new ValidationError("motion-evidence");
    }

    public static Fin<MotionEvidence> Admit(
        Seq<Arr<double>> joints,
        Seq<TimeDuration> segmentDurations,
        TimeDuration cycle,
        Seq<string> controllerCode,
        Seq<RunWarning> warnings) =>
        Validate(joints, segmentDurations, cycle, controllerCode, warnings, out MotionEvidence evidence)
            .Admitted(evidence);
}
```

## [04]-[EQUIPMENT]

- Owner: `ConsumableKey` owns the shop's consumable catalogue identity; `CutterFamily` owns the geometric rule columns every cutter shape admits against; `CutterForm` owns the admitted cutter geometry; `ToolEvidence` owns the decoded asset lifecycle a magazine read produced.
- Law: `ConsumableKey` seats at S0 because `Tooling/wear` at S2 budgets a consumable and `Joining/weld` at S3 names one, and a stratum never composes the stratum above it. Declaring the identity at either consumer splits a contact tip a procedure names from a contact tip a wear budget spends into two incomparable values, stranding every maintenance action from the procedure that consumed the part. `FabConcern.Tooling` names the refusal because the wear registry owns the roster, matching `MachineInstanceKey`, which names the fleet registry owning the station census.
- Cases: `CutterMetric` is the one keyed vocabulary for every optional cutter length, angle, and mass — a metric is a ROW, so a new ISO-13399 dimension needs no column, no constructor slot, and no validation clause. `CutterFamily.Compound` is the composite form whose two profile sections ride `MajorLength` and `SecondaryAngle` on that stream, so a cutter pairing a body form with a tip form is one family row rather than one family per pairing.
- Entry: `CutterForm.Admit` consumes one `CutterIngress` record of decoded scalars; `ToolEvidence.Admit` consumes already-decoded lifecycle scalars.
- Auto: the metric map validates under ONE clause — every admitted metric is finite and positive — replacing a per-column predicate ladder whose arity grew with the catalog.
- Output: the named projections (`ShankDiameterMm`, `OverallLengthMm`, and their peers) read the same map, so a consumer keeps its member spelling while the carrier stays one fact stream.
- Packages: `Thinktecture.Runtime.Extensions` closes construction; `UnitsNet` seats the quantity projections.
- Boundary: provider assets, mutable tool state, and unit parsing terminate at `Tooling/magazine`; this cluster admits decoded scalars and keyed catalogue identities only.

```csharp
// --- [EQUIPMENT]
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct ConsumableKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        if (!Witness.Keyed(value))
            validationError = new ValidationError("consumable-key");
    }

    public static Fin<ConsumableKey> Admit(string value) => Admission.OfValue<ConsumableKey, string>(value);
}

[SmartEnum<string>]
public sealed partial class CornerRule {
    public static readonly CornerRule Sharp = new("sharp");
    public static readonly CornerRule Full = new("full");
    public static readonly CornerRule Partial = new("partial");
    public static readonly CornerRule Any = new("any");

    private const double Relative = 1e-6;

    public bool Admits(double cornerRadius, double diameter) => Switch(
        state: (Corner: cornerRadius, Half: diameter * 0.5),
        sharp: static state => state.Corner <= Relative * state.Half,
        full: static state => Math.Abs(state.Corner - state.Half) <= Relative * state.Half,
        partial: static state => state.Corner > Relative * state.Half && state.Corner < state.Half * (1.0 - Relative),
        any: static state => state.Corner >= 0.0 && state.Corner <= state.Half);
}

[SmartEnum<string>]
public sealed partial class TaperRule {
    public static readonly TaperRule Straight = new("straight");
    public static readonly TaperRule Tapered = new("tapered");
    public static readonly TaperRule Any = new("any");

    private const double DegreeEpsilon = 1e-9;

    public bool Admits(double taperAngleDeg) => Switch(
        state: taperAngleDeg,
        straight: static angle => angle <= DegreeEpsilon,
        tapered: static angle => angle > DegreeEpsilon,
        any: static angle => angle >= 0.0);
}

[SmartEnum<string>]
public sealed partial class TaperSource {
    public static readonly TaperSource Flat = new("flat");
    public static readonly TaperSource EdgeAngle = new("edge-angle");
    public static readonly TaperSource HalfPointAngle = new("half-point-angle");
}

[SmartEnum<string>]
public sealed partial class CutterFamily {
    public static readonly CutterFamily Flat = new("flat", CornerRule.Sharp, TaperRule.Straight, TaperSource.Flat);
    public static readonly CutterFamily Ball = new("ball", CornerRule.Full, TaperRule.Straight, TaperSource.Flat);
    public static readonly CutterFamily Bull = new("bull", CornerRule.Partial, TaperRule.Straight, TaperSource.Flat);
    public static readonly CutterFamily Barrel = new("barrel", CornerRule.Partial, TaperRule.Straight, TaperSource.Flat);
    public static readonly CutterFamily Lollipop = new("lollipop", CornerRule.Full, TaperRule.Straight, TaperSource.Flat);
    public static readonly CutterFamily Taper = new("taper", CornerRule.Any, TaperRule.Tapered, TaperSource.EdgeAngle);
    public static readonly CutterFamily Dovetail = new("dovetail", CornerRule.Sharp, TaperRule.Tapered, TaperSource.EdgeAngle);
    public static readonly CutterFamily Drill = new("drill", CornerRule.Sharp, TaperRule.Tapered, TaperSource.HalfPointAngle);
    public static readonly CutterFamily Chamfer = new("chamfer", CornerRule.Sharp, TaperRule.Tapered, TaperSource.EdgeAngle);
    public static readonly CutterFamily Engraver = new("engraver", CornerRule.Sharp, TaperRule.Tapered, TaperSource.HalfPointAngle);
    public static readonly CutterFamily ThreadMill = new("thread-mill", CornerRule.Sharp, TaperRule.Straight, TaperSource.Flat);
    public static readonly CutterFamily Tap = new("tap", CornerRule.Sharp, TaperRule.Straight, TaperSource.Flat);
    public static readonly CutterFamily Reamer = new("reamer", CornerRule.Sharp, TaperRule.Straight, TaperSource.Flat);
    public static readonly CutterFamily BoringBar = new("boring-bar", CornerRule.Any, TaperRule.Straight, TaperSource.Flat);
    public static readonly CutterFamily FaceMill = new("face-mill", CornerRule.Any, TaperRule.Straight, TaperSource.Flat);
    public static readonly CutterFamily SlittingSaw = new("slitting-saw", CornerRule.Sharp, TaperRule.Straight, TaperSource.Flat);

    public static readonly CutterFamily Compound = new("compound", CornerRule.Any, TaperRule.Any, TaperSource.EdgeAngle);

    public CornerRule Corner { get; }
    public TaperRule Taper { get; }
    public TaperSource TaperFrom { get; }

    public bool Fits(double diameter, double cornerRadius, double taperAngle) =>
        Corner.Admits(cornerRadius, diameter) && Taper.Admits(taperAngle);
}

[SmartEnum<string>]
public sealed partial class CutterMetric {
    public static readonly CutterMetric UsableLength = new("usable-length");
    public static readonly CutterMetric FunctionalLength = new("functional-length");
    public static readonly CutterMetric OverallLength = new("overall-length");
    public static readonly CutterMetric ShankDiameter = new("shank-diameter");
    public static readonly CutterMetric MaxDepth = new("max-depth");
    public static readonly CutterMetric LeadAngle = new("lead-angle");
    public static readonly CutterMetric PointAngle = new("point-angle");
    public static readonly CutterMetric Orientation = new("orientation");
    public static readonly CutterMetric Mass = new("mass");
    public static readonly CutterMetric ProtrudingLength = new("protruding-length");
    public static readonly CutterMetric BodyDiameter = new("body-diameter");

    public static readonly CutterMetric MajorLength = new("major-length");
    public static readonly CutterMetric SecondaryAngle = new("secondary-angle");
}

[SmartEnum<string>]
public sealed partial class ToolState {
    public static readonly ToolState New = new("new");
    public static readonly ToolState Available = new("available");
    public static readonly ToolState Used = new("used");
    public static readonly ToolState Measured = new("measured");
    public static readonly ToolState Reconditioned = new("reconditioned");
    public static readonly ToolState Expired = new("expired");
    public static readonly ToolState Broken = new("broken");
    public static readonly ToolState Allocated = new("allocated");
    public static readonly ToolState Unallocated = new("unallocated");
    public static readonly ToolState NotRegistered = new("not-registered");
    public static readonly ToolState Unavailable = new("unavailable");
    public static readonly ToolState Unknown = new("unknown");
}

[SmartEnum<string>]
public sealed partial class ToolLifeBasis {
    public static readonly ToolLifeBasis Minutes = new("minutes");
    public static readonly ToolLifeBasis PartCount = new("part-count");
    public static readonly ToolLifeBasis Wear = new("wear");
}

public readonly record struct ToolLifeEvidence(
    ToolLifeBasis Basis,
    double Value,
    Option<double> Initial,
    Option<double> Limit,
    Option<double> Warning,
    bool CountsUp);

public readonly record struct FeedEnvelope(Option<Speed> Minimum, Option<Speed> Maximum, Option<Speed> Nominal);

public readonly record struct SpindleEnvelope(
    Option<RotationalSpeed> Minimum,
    Option<RotationalSpeed> Maximum,
    Option<RotationalSpeed> Nominal);

[ComplexValueObject]
public sealed partial class ToolEvidence {
    public string ToolId { get; }
    public string SerialNumber { get; }
    public string StructuralDigest { get; }
    public Set<ToolState> States { get; }
    public Seq<ToolLifeEvidence> Life { get; }
    public Option<FeedEnvelope> Feed { get; }
    public Option<SpindleEnvelope> Spindle { get; }
    public Option<string> ProgramNumber { get; }
    public Option<string> ProgramGroup { get; }
    public Option<int> Reconditions { get; }
    public Option<int> ReconditionLimit { get; }
    public Seq<string> InsertIds { get; }
    public Seq<string> InsertGrades { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string toolId,
        ref string serialNumber,
        ref string structuralDigest,
        ref Set<ToolState> states,
        ref Seq<ToolLifeEvidence> life,
        ref Option<FeedEnvelope> feed,
        ref Option<SpindleEnvelope> spindle,
        ref Option<string> programNumber,
        ref Option<string> programGroup,
        ref Option<int> reconditions,
        ref Option<int> reconditionLimit,
        ref Seq<string> insertIds,
        ref Seq<string> insertGrades) {
        if (!(Witness.Keyed(toolId)
            && Witness.Keyed(structuralDigest)
            && !states.IsEmpty
            && life.ForAll(static value => double.IsFinite(value.Value) && value.Value >= 0.0
                && value.Initial.Map(Bounded).IfNone(true)
                && value.Limit.Map(Bounded).IfNone(true)
                && value.Warning.Map(Bounded).IfNone(true))
            && life.Map(static value => value.Basis).Distinct().Count == life.Count
            && reconditions.Map(static value => value >= 0).IfNone(true)
            && reconditionLimit.Map(static value => value >= 0).IfNone(true)
            && reconditions.Map(count => reconditionLimit.Map(limit => count <= limit).IfNone(true)).IfNone(true)
            && insertIds.ForAll(Witness.Keyed)))
            validationError = new ValidationError("tool-evidence");
    }

    public static Fin<ToolEvidence> Admit(
        string toolId,
        string serialNumber,
        string structuralDigest,
        Set<ToolState> states,
        Seq<ToolLifeEvidence> life,
        Option<FeedEnvelope> feed,
        Option<SpindleEnvelope> spindle,
        Option<string> programNumber,
        Option<string> programGroup,
        Option<int> reconditions,
        Option<int> reconditionLimit,
        Seq<string> insertIds,
        Seq<string> insertGrades) =>
        Validate(toolId, serialNumber, structuralDigest, states, life, feed, spindle, programNumber, programGroup,
            reconditions, reconditionLimit, insertIds, insertGrades, out ToolEvidence evidence).Admitted(evidence);

    private static bool Bounded(double amount) => double.IsFinite(amount) && amount >= 0.0;
}

public sealed record CutterIngress(
    CutterFamily Family,
    double Diameter,
    double CornerRadius,
    double TaperAngle,
    double FluteLength,
    Map<CutterMetric, double> Metrics,
    Option<int> FluteCount = default,
    Option<ToolEvidence> Evidence = default);

[ComplexValueObject]
public sealed partial class CutterForm {
    public CutterFamily Family { get; }
    public double Diameter { get; }
    public double CornerRadius { get; }
    public double TaperAngle { get; }
    public double FluteLength { get; }
    public Map<CutterMetric, double> Metrics { get; }
    public Option<int> FluteCount { get; }
    public Option<ToolEvidence> Evidence { get; }

    public UnitsNet.Length DiameterLength => UnitsNet.Length.FromMillimeters(Diameter);
    public UnitsNet.Length CornerRadiusLength => UnitsNet.Length.FromMillimeters(CornerRadius);
    public UnitsNet.Angle Taper => UnitsNet.Angle.FromDegrees(TaperAngle);
    public UnitsNet.Length CuttingLength => UnitsNet.Length.FromMillimeters(FluteLength);

    public Option<double> UsableLengthMm => Metrics.Find(CutterMetric.UsableLength);
    public Option<double> FunctionalLengthMm => Metrics.Find(CutterMetric.FunctionalLength);
    public Option<double> OverallLengthMm => Metrics.Find(CutterMetric.OverallLength);
    public Option<double> ShankDiameterMm => Metrics.Find(CutterMetric.ShankDiameter);
    public Option<double> MaxDepthMm => Metrics.Find(CutterMetric.MaxDepth);
    public Option<double> LeadAngleDeg => Metrics.Find(CutterMetric.LeadAngle);
    public Option<double> PointAngleDeg => Metrics.Find(CutterMetric.PointAngle);
    public Option<double> OrientationDeg => Metrics.Find(CutterMetric.Orientation);
    public Option<double> MassKg => Metrics.Find(CutterMetric.Mass);
    public Option<double> ProtrudingLengthMm => Metrics.Find(CutterMetric.ProtrudingLength);
    public Option<double> BodyDiameterMm => Metrics.Find(CutterMetric.BodyDiameter);
    public Option<double> MajorLengthMm => Metrics.Find(CutterMetric.MajorLength);
    public Option<double> SecondaryAngleDeg => Metrics.Find(CutterMetric.SecondaryAngle);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CutterFamily family,
        ref double diameter,
        ref double cornerRadius,
        ref double taperAngle,
        ref double fluteLength,
        ref Map<CutterMetric, double> metrics,
        ref Option<int> fluteCount,
        ref Option<ToolEvidence> evidence) {
        if (!(ValidityClaim.All(
            ValidityClaim.Positive(diameter), double.IsFinite(cornerRadius), cornerRadius >= 0.0, cornerRadius <= diameter * 0.5,
            double.IsFinite(taperAngle), taperAngle is >= 0.0 and < 90.0, ValidityClaim.Positive(fluteLength),
            metrics.ForAll(static row => double.IsFinite(row.Value) && row.Value > 0.0), fluteCount.Map(static value => value > 0).IfNone(true),
            family.Fits(diameter, cornerRadius, taperAngle))))
            validationError = new ValidationError("cutter-form");
    }

    public static Fin<CutterForm> Admit(CutterIngress ingress) =>
        Validate(
            ingress.Family,
            ingress.Diameter,
            ingress.CornerRadius,
            ingress.TaperAngle,
            ingress.FluteLength,
            ingress.Metrics,
            ingress.FluteCount,
            ingress.Evidence,
            out CutterForm cutter).Admitted(cutter);

    public CanonicalWriter CanonicalBytes(CanonicalWriter writer) => writer
        .Discriminant(Family)
        .Double(Diameter).Double(CornerRadius).Double(TaperAngle).Double(FluteLength)
        .Rows(toSeq(Metrics).OrderBy(static row => row.Key.Key).ToSeq(),
            static (row, metric) => row.Discriminant(metric.Key).Double(metric.Value));
}
```

## [05]-[PLAN]

- Owner: `AdmittedComponent` owns the geometry-and-property carrier every plane derives from; `PlannedStep` owns one routed operation group with its assigned machine INSTANCE; `CapabilityAttestation` owns the attestation vocabulary and `CapabilityVerdict` the fail-closed capability gate over it.
- Law: component quantities and properties key on `PropertyName` minted through `PropertyCategory.Fabrication.Row`, the seam's own custody scope — a bare string key at S0 forks the vocabulary the derivation plane already blesses, and a `PropertyName.Create` at a write site is the deleted form.
- Cases: `PlannedStep.Instance` names the physical machine the schedule reserved, so a lot fold seats work on a specific station rather than on a machine CLASS with unbounded parallelism; a step whose plane owns no instance census carries `None` and the schedule treats the class as uncapped.
- Auto: every atom here admits through its generated `Validate` and the one `Admitted` bridge, so no site re-spells the refusal lift.
- Boundary: `CapabilityVerdict` fails closed by ABSENCE — an attestation the study never earned is unheld, so it fails `Pass` on its own evidence rather than masquerading as a zero-Cpk process, and a consumer states the attestations it demands as one `CapabilitySet` value instead of a bool per axis.

```csharp
// --- [PLAN]
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct MachineInstanceKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        if (!Witness.Keyed(value))
            validationError = new ValidationError("machine-instance-key");
    }

    public static Fin<MachineInstanceKey> Admit(string value) => Admission.OfValue<MachineInstanceKey, string>(value);
}

public sealed record ComponentLayer(string Function, double ThicknessMm, PropertyName MaterialKey);

public sealed record ComponentConnection(PropertyName DetailKey, PropertyName RealizingKey, Option<Edge3> At);

[ComplexValueObject]
public sealed partial class AdmittedComponent {
    public UInt128 RepresentationKey { get; }
    public Option<MeshSpace> Mesh { get; }
    public Arr<Loop> Profiles { get; }
    public Option<double> SheetThicknessMm { get; }
    public Arr<ComponentLayer> Layers { get; }
    public Arr<ComponentConnection> Connections { get; }
    public Map<PropertyName, double> Quantities { get; }
    public Map<PropertyName, string> Properties { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref UInt128 representationKey,
        ref Option<MeshSpace> mesh,
        ref Arr<Loop> profiles,
        ref Option<double> sheetThicknessMm,
        ref Arr<ComponentLayer> layers,
        ref Arr<ComponentConnection> connections,
        ref Map<PropertyName, double> quantities,
        ref Map<PropertyName, string> properties) {
        if (mesh.IsNone && profiles.IsEmpty)
            validationError = new ValidationError("component:geometry");
    }

    public static Fin<AdmittedComponent> Admit(
        UInt128 representationKey,
        Option<MeshSpace> mesh,
        Arr<Loop> profiles,
        Option<double> sheetThicknessMm,
        Arr<ComponentLayer> layers,
        Arr<ComponentConnection> connections,
        Map<PropertyName, double> quantities,
        Map<PropertyName, string> properties) =>
        (AdmissionSlots.Gate(mesh.IsSome || !profiles.IsEmpty, FabConcern.Ingress, "component:geometry", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(sheetThicknessMm.Map(static value => ValidityClaim.Positive(value).Holds).IfNone(true),
             FabConcern.Ingress, "component:thickness", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(layers.ForAll(static layer => ValidityClaim.All(
             Witness.Keyed(layer.Function), ValidityClaim.Positive(layer.ThicknessMm))),
             FabConcern.Ingress, "component:layers", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(connections.ForAll(static connection =>
                connection.At.Map(static edge => edge.A.IsValid && edge.B.IsValid && edge.A != edge.B).IfNone(true)),
                    FabConcern.Ingress, "component:connections", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(quantities.ForAll(static row => double.IsFinite(row.Value)),
             FabConcern.Ingress, "component:quantities", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(properties.ForAll(static row => Witness.Keyed(row.Value)),
             FabConcern.Ingress, "component:properties", FabricationFault.Inadmissible))
            .Apply(static (_, _, _, _, _, _) => unit)
            .As()
            .ToFin()
            .Bind(_ => Validate(representationKey, mesh, profiles, sheetThicknessMm, layers, connections,
                quantities, properties, out AdmittedComponent component).Admitted(component));

}

[ComplexValueObject]
public sealed partial class ResidualStock {
    public ContentKey Key { get; }
    public Arr<Loop> Uncut { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ContentKey key,
        ref Arr<Loop> uncut) {
        if (key.Kind != EgressKind.Remnant || !uncut.ForAll(static loop => loop.Closed))
            validationError = new ValidationError("residual-stock");
    }

    public static Fin<ResidualStock> Admit(ContentKey key, Arr<Loop> uncut) =>
        Validate(key, uncut, out ResidualStock stock).Admitted(stock);
}

[ComplexValueObject]
public sealed partial class StockSnapshot {
    public int Setup { get; }
    public ContentKey Key { get; }
    public Arr<Loop> Machined { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int setup,
        ref ContentKey key,
        ref Arr<Loop> machined) {
        if (setup < 0 || key.Kind != EgressKind.StockSnapshot || !machined.ForAll(static loop => loop.Closed))
            validationError = new ValidationError("stock-snapshot");
    }

    public static Fin<StockSnapshot> Admit(int setup, ContentKey key, Arr<Loop> machined) =>
        Validate(setup, key, machined, out StockSnapshot snapshot).Admitted(snapshot);
}

[ComplexValueObject]
public sealed partial class PlannedStep {
    public int Order { get; }
    public ProcessKind Process { get; }
    public Machine Machine { get; }

    public Option<MachineInstanceKey> Instance { get; }

    public int Setup { get; }
    public Arr<int> Operations { get; }
    public Option<ContentKey> Program { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int order,
        ref ProcessKind process,
        ref Machine machine,
        ref Option<MachineInstanceKey> instance,
        ref int setup,
        ref Arr<int> operations,
        ref Option<ContentKey> program) {
        if (order < 0 || setup < 0
            || operations.IsEmpty
            || !operations.ForAll(static value => ValidityClaim.Nonnegative(value).Holds)
            || operations.Distinct().Count != operations.Count
            || !machine.Admits(process)
            || !program.Map(static key => key.Kind == EgressKind.CutProgram).IfNone(true))
            validationError = new ValidationError("planned-step");
    }

    public static Fin<PlannedStep> Admit(
        int order,
        ProcessKind process,
        Machine machine,
        Option<MachineInstanceKey> instance,
        int setup,
        Arr<int> operations,
        Option<ContentKey> program) =>
        Validate(order, process, machine, instance, setup, operations, program, out PlannedStep step).Admitted(step);
}

[ComplexValueObject]
public sealed partial class CamPassPolicy {
    public double StepOver { get; }
    public int Passes { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double stepOver,
        ref int passes) {
        if (!ValidityClaim.Positive(stepOver).Holds || passes < 1)
            validationError = new ValidationError("cam-pass-policy");
    }

    public static Fin<CamPassPolicy> Admit(double stepOver, int passes) =>
        Validate(stepOver, passes, out CamPassPolicy policy).Admitted(policy);
}

[SmartEnum<string>]
public sealed partial class BendOrientation {
    public static readonly BendOrientation AsIs = new("as-is");
    public static readonly BendOrientation Flipped = new("flipped");
}

public readonly record struct BendStep(
    int Order,
    Edge3 Line,
    double AngleDeg,
    double RadiusMm,
    double KFactor,
    double OverbendDeg,
    double TonnageKn,
    BendOrientation Orientation);

[SmartEnum<string>]
public sealed partial class CapabilityAttestation : ICapability<CapabilityAttestation> {
    public static readonly CapabilityAttestation Procedure = new("procedure-qualified");
    public static readonly CapabilityAttestation Measurement = new("measurement-system-suitable");
}

[ComplexValueObject]
public sealed partial class CapabilityVerdict {
    public double Cpk { get; }
    public double DemandedCpk { get; }

    public CapabilitySet<CapabilityAttestation> Attested { get; }
    public bool Pass => Cpk >= DemandedCpk && Attested.AdmitsAll(CapabilitySet<CapabilityAttestation>.All);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double cpk,
        ref double demandedCpk,
        ref CapabilitySet<CapabilityAttestation> attested) {
        if (!(ValidityClaim.All(double.IsFinite(cpk), cpk >= 0.0, ValidityClaim.Positive(demandedCpk))))
            validationError = new ValidationError("capability-verdict");
    }

    public static Fin<CapabilityVerdict> Admit(
        double cpk, double demandedCpk, CapabilitySet<CapabilityAttestation> attested) =>
        Validate(cpk, demandedCpk, attested, out CapabilityVerdict verdict).Admitted(verdict);
}

public readonly record struct GougeWitness(int Setup, int Move, Point3d Point, double DepthMm);

[SmartEnum<string>]
public sealed partial class InspectionMethod {
    public static readonly InspectionMethod Probe = new("probe");
    public static readonly InspectionMethod Scan = new("scan");
    public static readonly InspectionMethod Gauge = new("gauge");
    public static readonly InspectionMethod Vision = new("vision");
    public static readonly InspectionMethod Manual = new("manual");
}

[ComplexValueObject]
public sealed partial class InspectionFeature {
    public PropertyName Key { get; }
    public Point3d Nominal { get; }
    public Point3d Measured { get; }
    public Option<double> ToleranceMm { get; }
    public double UncertaintyMm { get; }
    public InspectionMethod Method { get; }
    public double DeviationMm => Nominal.DistanceTo(Measured);
    public Option<bool> Pass => ToleranceMm.Map(tolerance => DeviationMm + UncertaintyMm <= tolerance);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref PropertyName key,
        ref Point3d nominal,
        ref Point3d measured,
        ref Option<double> toleranceMm,
        ref double uncertaintyMm,
        ref InspectionMethod method) {
        if (!(nominal.IsValid && measured.IsValid
            && toleranceMm.Map(static value => ValidityClaim.Positive(value).Holds).IfNone(true)
            && double.IsFinite(uncertaintyMm) && uncertaintyMm >= 0.0))
            validationError = new ValidationError("inspection-feature");
    }

    public static Fin<InspectionFeature> Admit(
        PropertyName key,
        Point3d nominal,
        Point3d measured,
        Option<double> toleranceMm,
        double uncertaintyMm,
        InspectionMethod method) =>
        Validate(key, nominal, measured, toleranceMm, uncertaintyMm, method, out InspectionFeature feature)
            .Admitted(feature);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
