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
            difference: static at => Products.Many(() =>
                    (Brep.CreateBooleanDifferenceWithIndexMap(at.Operation.First, at.Operation.Second, at.Tolerance, at.ManifoldOnly, out int[] keys), keys), required: false)
                .Map(static answer => answer.Values.Zip(toSeq(answer.Out), static (piece, owner) => new Grouped<Brep, int>(piece, [owner])).Strict()));

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
                : at.Operation.Second.FoldM(Seq(first), (pieces, cutter) => Products.Each(pieces
                        .Map(piece => Products.Many(() => Brep.CreatePlanarDifference(piece, cutter, at.Plane, at.Tolerance), required: false)))
                        .Map(static cut => cut.Bind(static held => held)))
                    .As()));

    public static IO<Seq<Brep>> Solid(Tolerances tolerances, Split<Brep> split) =>
        Products.Many(() => Brep.CreateBooleanSplit(split.Pieces, split.Cutters, tolerances.Absolute), required: false);

    // --- [MESHES]

    public static IO<(Seq<Grouped<Mesh, int>> Groups, MeshBooleanOptions Report)> Meshes(Tolerances tolerances, Boolean<Mesh> operation, Func<MeshBooleanOptions> options) =>
        from report in Running(tolerances, options)
        from groups in Checked(() => operation.Operation.Meshed(operation.First, operation.Second, report))
        select (groups, report);

    public static IO<(Seq<Grouped<Mesh, int>> Groups, MeshBooleanOptions Report)> Meshes(Tolerances tolerances, Split<Mesh> split, Func<MeshBooleanOptions> options) =>
        from report in Running(tolerances, options)
        from groups in Checked(() => (Mesh.CreateBooleanSplit(split.Pieces, split.Cutters, report, out Result result, out int[][] map), result, map))
        select (groups, report);

    private static IO<MeshBooleanOptions> Running(Tolerances tolerances, Func<MeshBooleanOptions> options) =>
        from token in cancelToken
        from fresh in IO.lift(options)
        from running in IO.lift(() => (fresh.Tolerance, fresh.CancellationToken) = (tolerances.MeshIntersection, token))
        select fresh;

    private static IO<Seq<Grouped<Mesh, int>>> Checked(Func<(Mesh[]? Meshes, Result Result, int[][]? Map)> call, [CallerArgumentExpression(nameof(call))] string member = "") =>
        from answered in IO.lift(call)
        from ended in IO.lift(Results.Ended(answered.Result, member))
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
                    .Bind(static regions => IO.lift(Products.Each(regions, static (region, index) => RhinoFault.Unless(!region.From.IsEmpty, RhinoReason.Missing, region.From, index).Map(_ => region.Value)))),
                (first, outer) => Products.Many(() => Curve.CreateBooleanIntersection(first, outer, at.Absolute), required: false)),
            difference: static at => PerFirst(at.Operation.First, first => Products.Many(() => Curve.CreateBooleanDifference(first, at.Operation.Second, at.Absolute), required: false)));

    public static IO<Seq<Grouped<Seq<Curve>, int>>> Regions(Tolerances tolerances, Seq<Curve> curves, Plane plane, Seq<Point3d> points, bool combineRegions) =>
        from regions in Products.Of(() => Curve.CreateBooleanRegions(curves, plane, points, combineRegions, tolerances.Absolute))
        from shapes in Products.Each(toSeq(Range(0, regions.RegionCount)).Map(index => Products.Many(() => regions.RegionCurves(index), required: false)))
        let owners = Enumerable.Range(0, regions.PointCount).ToLookup(regions.RegionPointIndex)
        select shapes.Map((shape, index) => new Grouped<Seq<Curve>, int>(shape, toSeq(owners[index]))).Strict();

    // --- [PROVENANCE]

    public static IO<Seq<Grouped<T, int>>> Keyed<T>(Func<(T[]? Rows, int[]? Key)> call, [CallerArgumentExpression(nameof(call))] string member = "") where T : GeometryBase =>
        from answered in Products.Many(() => call(), required: false, member)
        from key in IO.lift(Conversions.Required(answered.Out, call: member))
        let owners = key.Select(static (owner, input) => (Owner: owner, Input: input)).ToLookup(static entry => entry.Owner, static entry => entry.Input)
        select answered.Values.Map((row, index) => new Grouped<T, int>(row, toSeq(owners[index]))).Strict();

    public static IO<Seq<Grouped<T, int>>> Mapped<T>(Func<(T[]? Rows, int[][]? Map)> call, [CallerArgumentExpression(nameof(call))] string member = "") where T : GeometryBase =>
        from answered in Products.Many(() => call(), required: false, member)
        from map in IO.lift(Conversions.Required(answered.Out, call: member))
        select answered.Values.Zip(toSeq(map), static (row, inputs) => new Grouped<T, int>(row, toSeq(inputs))).Strict();

    // --- [OPERANDS]

    private static IO<Seq<Grouped<T, int>>> PerFirst<T>(Seq<T> first, Func<T, IO<Seq<T>>> step) =>
        Products.Each(first.Map((operand, index) => step(operand).Map(pieces => pieces.Map(piece => new Grouped<T, int>(piece, [index])))))
            .Map(static groups => groups.Bind(static group => group));

    private static IO<Seq<Grouped<T, int>>> Meet<T>(Seq<T> first, Seq<T> second, Func<IO<Seq<T>>> union, Func<T, T, IO<Seq<T>>> binary) =>
        from pieces in second.Count switch {
            0 => IO.pure(Seq<T>()),
            1 => IO.pure(second),
            _ => union(),
        }
        from met in PerFirst(first, operand => Products.Each(pieces.Map(piece => binary(operand, piece))).Map(static met => met.Bind(static held => held)))
        select met;
}
