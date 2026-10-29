using System.Collections.Generic;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Intersections;

// --- [MODELS] --------------------------------------------------------------------------
[Union<BoundingBox, Sphere>(SkipToString = true, SkipEqualityComparison = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class SearchRegion;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Search {
    // --- [BOXES]
    public static IO<Seq<(int A, int B)>> Overlaps(Sets<BoundingBox> boxes, double tolerance) =>
        boxes.Switch(
            tolerance,
            within: static (distance, within) => Planted(within.Items, tree =>
                Collected(found => tree.SearchOverlaps(distance, found, tag: null), static found => (A: found.Id, B: found.IdB))),
            between: static (distance, between) => Planted(between.A, treeA => Planted(between.B, treeB =>
                Collected(found => RTree.SearchOverlaps(treeA, treeB, distance, found), static found => (A: found.Id, B: found.IdB)))));

    public static IO<Seq<Seq<int>>> Within(Seq<BoundingBox> items, Seq<SearchRegion> queries) =>
        Planted(items, tree => queries.TraverseM(query => query.Switch(
            tree,
            boundingBox: static (held, box) => Collected(found => held.Search(box, found), static found => found.Id),
            sphere: static (held, sphere) => Collected(found => held.Search(sphere, found), static found => found.Id))).As());

    private static IO<T> Planted<T>(Seq<BoundingBox> boxes, Func<RTree, IO<T>> body) =>
        (from tree in use(static () => new RTree())
         from planted in Products.Each(boxes.Map((box, index) => Products.Accepted(() => tree.Insert(box, index))))
         from found in body(tree)
         select found).Bracket();

    private static IO<Seq<T>> Collected<T>(Func<EventHandler<RTreeEventArgs>, bool> search, Func<RTreeEventArgs, T> row) =>
        from rows in IO.lift(static () => new List<T>())
        from searched in Products.Accepted(() => search((_, found) => rows.Add(row(found))))
        select toSeq(rows).Strict();

    // --- [CURVES]
    public static IO<Seq<(int Object, bool Entire, Seq<ComponentIndex> Removed, Option<Curve> Replacement)>> Overlapping(
        Tolerances tolerances, Seq<GeometryBase> objects, Plane plane, bool wholeCurves, Seq<GeometryBase> locked, bool lockedExclusive) =>
        from answered in IO.lift(() => (
            Indices: GeometryBase.GetOverlaps(objects, plane, tolerances.Absolute, wholeCurves, locked, lockedExclusive, out bool[] entire, out ComponentIndex[][] removed, out Curve[] replacements),
            Entire: entire, Removed: removed, Replacements: replacements))
        from indices in IO.lift(() => Optional(answered.Indices).ToFin(new RhinoFault(nameof(GeometryBase.GetOverlaps), RhinoReason.Missing)))
        from rows in Products.Each(toSeq(indices).Map((index, at) => Products.Maybe(() => answered.Replacements[at])
            .Map(replacement => (Object: index, Entire: answered.Entire[at], Removed: toSeq(answered.Removed[at]), Replacement: replacement))).Strict())
        select rows;
}
