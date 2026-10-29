using System.Runtime.CompilerServices;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document.Units;
using Rhino.Commands;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class SetOperation {
    public static readonly SetOperation Union = new(static (first, second, options) =>
        (Mesh.CreateBooleanUnion(first + second, options, out Result result, out int[][] map), result, map));
    public static readonly SetOperation Intersection = new(static (first, second, options) =>
        (Mesh.CreateBooleanIntersection(first, second, options, out Result result, out int[][] map), result, map));
    public static readonly SetOperation Difference = new(static (first, second, options) =>
        (Mesh.CreateBooleanDifference(first, second, options, out Result result, out int[][] map), result, map));

    [UseDelegateFromConstructor]
    internal partial (Mesh[]? Meshes, Result Result, int[][]? Map) Meshed(Seq<Mesh> first, Seq<Mesh> second, MeshBooleanOptions options);
}

public sealed record Boolean<T>(SetOperation Operation, Seq<T> First, Seq<T> Second) where T : GeometryBase;

public sealed record Split<T>(Seq<T> Pieces, Seq<T> Cutters) where T : GeometryBase;

public sealed record Grouped<T, TFrom>(T Value, Seq<TFrom> From);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Booleans {
    // --- [SOLIDS]
    public static IO<Seq<Grouped<Brep, int>>> Solid(Tolerances tolerances, Boolean<Brep> operation, bool manifoldOnly) =>
        operation.Operation.Switch(
            (Operation: operation, Tolerance: tolerances.Absolute, ManifoldOnly: manifoldOnly),
            union: static at => Mapped(() => (Brep.CreateBooleanUnionWithIndexMap(at.Operation.First + at.Operation.Second, at.Tolerance, at.ManifoldOnly, out int[][] map), map)),
            intersection: static at => PerFirst(at.Operation.First, first =>
                Products.Many(() => Brep.CreateBooleanIntersection([first], at.Operation.Second, at.Tolerance, at.ManifoldOnly), required: false)),
            difference: static at => Mapped(() => (Brep.CreateBooleanDifferenceWithIndexMap(at.Operation.First, at.Operation.Second, at.Tolerance, at.ManifoldOnly, out int[] keys), keys) switch {
                var (pieces, owners) => (pieces, Optional(owners).Map(static held => held.Select(static int[] (input) => [input]).ToArray()).ValueUnsafe()),
            }));

    public static IO<Seq<Grouped<Brep, int>>> Planar(Tolerances tolerances, Boolean<Brep> operation, Plane plane) =>
        operation.Operation.Switch(
            (Operation: operation, Plane: plane, Tolerance: tolerances.Absolute),
            union: static at => (at.Operation.First + at.Operation.Second) switch {
                var operands => Products.Many(() => Brep.CreatePlanarUnion(operands, at.Plane, at.Tolerance), required: false)
                    .Map(regions => regions.Map(region => new Grouped<Brep, int>(region, operands.Map(static (_, index) => index))).Strict()),
            },
            intersection: static at => Meet(
                at.Operation.First,
                at.Operation.Second,
                () => Products.Many(() => Brep.CreatePlanarUnion(at.Operation.Second, at.Plane, at.Tolerance), required: false),
                (first, region) => Products.Many(() => Brep.CreatePlanarIntersection(first, region, at.Plane, at.Tolerance), required: false)),
            difference: static at => PerFirst(at.Operation.First, first => at.Operation.Second.IsEmpty
                ? Products.Of(first.DuplicateBrep).Map(static copy => Seq(copy))
                : at.Operation.Second.FoldM(Seq(first), (pieces, cutter) => pieces
                        .TraverseM(piece => Products.Many(() => Brep.CreatePlanarDifference(piece, cutter, at.Plane, at.Tolerance), required: false))
                        .As()
                        .Map(static cut => cut.Bind(static held => held)))
                    .As()));

    public static IO<Seq<Brep>> Solid(Tolerances tolerances, Split<Brep> split) =>
        Products.Many(() => Brep.CreateBooleanSplit(split.Pieces, split.Cutters, tolerances.Absolute), required: false);

    // --- [MESHES]
    public static IO<(Seq<Grouped<Mesh, int>> Groups, MeshBooleanOptions Report)> Meshes(Tolerances tolerances, Boolean<Mesh> operation, Func<MeshBooleanOptions> options) =>
        from report in Running(tolerances, options)
        from groups in Settled(() => operation.Operation.Meshed(operation.First, operation.Second, report))
        select (groups, report);

    public static IO<(Seq<Grouped<Mesh, int>> Groups, MeshBooleanOptions Report)> Meshes(Tolerances tolerances, Split<Mesh> split, Func<MeshBooleanOptions> options) =>
        from report in Running(tolerances, options)
        from groups in Settled(() => (Mesh.CreateBooleanSplit(split.Pieces, split.Cutters, report, out Result result, out int[][] map), result, map))
        select (groups, report);

    private static IO<MeshBooleanOptions> Running(Tolerances tolerances, Func<MeshBooleanOptions> options) =>
        from token in cancelToken
        from fresh in IO.lift(options)
        from running in IO.lift(() => (fresh.Tolerance, fresh.CancellationToken) = (tolerances.MeshIntersection, token))
        select fresh;

    private static IO<Seq<Grouped<Mesh, int>>> Settled(Func<(Mesh[]? Meshes, Result Result, int[][]? Map)> call, [CallerArgumentExpression(nameof(call))] string member = "") =>
        from answered in IO.lift(call)
        from ended in IO.lift(() => Results.Ended(answered.Result, member))
        from groups in Mapped(() => (answered.Meshes, answered.Map), member)
        select groups;

    // --- [CURVES]
    public static IO<Seq<Grouped<Curve, int>>> Curves(Tolerances tolerances, Boolean<Curve> operation) =>
        operation.Operation.Switch(
            (Operation: operation, tolerances.Absolute),
            union: static at => Keyed(() => (Curve.CreateBooleanUnion(at.Operation.First + at.Operation.Second, at.Absolute, out int[] key), key)),
            intersection: static at => Meet(
                at.Operation.First,
                at.Operation.Second,
                () => Keyed(() => (Curve.CreateBooleanUnion(at.Operation.Second, at.Absolute, out int[] key), key))
                    .Bind(static regions => IO.lift(() => Products.Each(regions, static (region, index) => RhinoFault.Unless(!region.From.IsEmpty, RhinoReason.Missing, region.From, index).Map(_ => region.Value)))),
                (first, outer) => Products.Many(() => Curve.CreateBooleanIntersection(first, outer, at.Absolute), required: false)),
            difference: static at => PerFirst(at.Operation.First, first => Products.Many(() => Curve.CreateBooleanDifference(first, at.Operation.Second, at.Absolute), required: false)));

    public static IO<Seq<Grouped<Seq<Curve>, int>>> Regions(Tolerances tolerances, Seq<Curve> curves, Plane plane, Seq<Point3d> points, bool combineRegions) =>
        from regions in Products.Of(() => Curve.CreateBooleanRegions(curves, plane, points, combineRegions, tolerances.Absolute))
        from shapes in toSeq(Range(0, regions.RegionCount)).TraverseM(index => Products.Many(() => regions.RegionCurves(index), required: false)).As()
        let owners = toSeq(Range(0, regions.PointCount)).Map(point => (Point: point, Region: regions.RegionPointIndex(point))).Strict()
        select shapes.Map((shape, index) => new Grouped<Seq<Curve>, int>(shape, owners.Filter(owner => owner.Region == index).Map(static owner => owner.Point))).Strict();

    // --- [PROVENANCE]
    public static IO<Seq<Grouped<T, int>>> Keyed<T>(Func<(T[]? Rows, int[]? Key)> call, [CallerArgumentExpression(nameof(call))] string member = "") where T : GeometryBase =>
        from answered in IO.lift(call)
        from rows in Products.Many(() => answered.Rows, required: false, member)
        from grouped in IO.lift(() =>
            from key in Optional(answered.Key).ToFin(new RhinoFault(member, RhinoReason.Missing))
            from entries in Products.Each(toSeq(key).Map(static (owner, input) => (Owner: owner, Input: input)).Filter(static entry => entry.Owner is not -1), (entry, index) =>
                RhinoFault.Unless(entry.Owner < rows.Count, RhinoReason.Outside, entry.Owner, index).Map(_ => entry))
            select rows.Map((row, index) => new Grouped<T, int>(row, entries.Filter(entry => entry.Owner == index).Map(static entry => entry.Input))).Strict())
        select grouped;

    public static IO<Seq<Grouped<T, int>>> Mapped<T>(Func<(T[]? Rows, int[][]? Map)> call, [CallerArgumentExpression(nameof(call))] string member = "") where T : GeometryBase =>
        from answered in IO.lift(call)
        from rows in Products.Many(() => answered.Rows, required: false, member)
        from grouped in IO.lift(() =>
            from map in Optional(answered.Map).ToFin(new RhinoFault(member, RhinoReason.Missing))
            from paired in RhinoFault.Unless(map.Length == rows.Count, RhinoReason.Mismatched, map.Length)
            select rows.Zip(toSeq(map), static (row, inputs) => new Grouped<T, int>(row, toSeq(inputs))).Strict())
        select grouped;

    // --- [OPERANDS]
    private static IO<Seq<Grouped<T, int>>> PerFirst<T>(Seq<T> first, Func<T, IO<Seq<T>>> step) =>
        first.Map(static (operand, index) => (Operand: operand, Index: index))
            .TraverseM(at => step(at.Operand).Map(pieces => pieces.Map(piece => new Grouped<T, int>(piece, [at.Index]))))
            .As()
            .Map(static groups => groups.Bind(static group => group));

    private static IO<Seq<Grouped<T, int>>> Meet<T>(Seq<T> first, Seq<T> second, Func<IO<Seq<T>>> union, Func<T, T, IO<Seq<T>>> binary) =>
        from pieces in second.Count switch {
            0 => IO.pure(Seq<T>()),
            1 => IO.pure(second),
            _ => union(),
        }
        from met in PerFirst(first, operand => pieces.TraverseM(piece => binary(operand, piece)).As().Map(static met => met.Bind(static held => held)))
        select met;
}
