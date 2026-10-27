using System.Numerics;
using System.Numerics.Tensors;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Random;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Triangulate;
using RBush;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Plotting;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Hierarchy {
    private Hierarchy(Key key, float weight, Seq<Hierarchy> members) => (Key, Weight, Members) = (key, weight, members);

    public Key Key { get; }
    public float Weight { get; }
    public Seq<Hierarchy> Members { get; }

    public static Fin<Hierarchy> Leaf(Key key, float weight) => UIFault.Unless(float.IsFinite(weight) && weight >= 0f, UIReason.Range, weight).Map(_ => new Hierarchy(key, weight, []));

    public static Hierarchy Group(Key key, Seq<Hierarchy> members) => new(key, members.Sum(static member => member.Weight), members);
}

public sealed record Flow {
    private Flow(Key key, Key source, Key target, float weight) => (Key, Source, Target, Weight) = (key, source, target, weight);

    public Key Key { get; }
    public Key Source { get; }
    public Key Target { get; }
    public float Weight { get; }

    public static Fin<Flow> Validated(Key key, Key source, Key target, float weight) =>
        UIFault.Unless(float.IsFinite(weight) && weight >= 0f && source != target, UIReason.Range, (source, target, weight)).Map(_ => new Flow(key, source, target, weight));
}

public sealed record Arranged(Key Key, Vector2 At, float Weight, Figure Figure);

public sealed record Network(Seq<Arranged> Nodes, Seq<Arranged> Links) {
    public Option<Figure.Path> Hull { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Arrangements {
    // --- [HIERARCHIES]
    private sealed record Spot(Hierarchy Node, Vector2 At, Option<Vector2> From) {
        public Spot Moved(Vector2 by) => this with { At = At + by, From = From.Map(from => from + by) };
    }

    private sealed record Subtree(Seq<Spot> Spots, Seq<(float Left, float Right)> Contour);

    public static Fin<Seq<Arranged>> Treemap(PlotFrame frame, Hierarchy root, float inner = 0f, float outer = 0f) =>
        UIFault.Unless(float.IsFinite(inner) && float.IsFinite(outer) && inner >= 0f && outer >= 0f, UIReason.Range, (inner, outer)).Map(_ => Tiled(frame, root, frame.Box, inner / 2f, outer));

    public static Network Tree(PlotFrame frame, Hierarchy root) {
        Subtree shape = Shaped(root);
        SKRect box = frame.Box;
        (float low, float high) = (shape.Contour.Min(static depth => depth.Left), shape.Contour.Max(static depth => depth.Right));
        int deepest = shape.Contour.Count - 1;
        SKPoint Placed(Vector2 at) => new(box.Left + ((high > low ? (at.X - low) / (high - low) : 0.5f) * box.Width), box.Top + ((deepest > 0 ? at.Y / deepest : 0f) * box.Height));
        return new Network(
            shape.Spots.Choose(spot => frame.Valued(Placed(spot.At)).Map(at => new Arranged(spot.Node.Key, at, spot.Node.Weight, new Figure.Points([at])))),
            shape.Spots.Choose(spot => spot.From.Bind(from => Unframed(frame, [Placed(from), Placed(spot.At)]))
                .Map(link => new Arranged(spot.Node.Key, link[1], spot.Node.Weight, new Figure.Path(link) { Interpolation = Interpolation.BumpY }))));
    }

    private static Seq<Arranged> Tiled(PlotFrame frame, Hierarchy node, SKRect rect, float half, float outer) {
        SKRect content = Deflated(rect, outer - half);
        return Rectangle(frame, node.Key, node.Weight, rect).ToSeq()
            + Squarified(toSeq(node.Members.Filter(static member => member.Weight > 0f).OrderByDescending(static member => member.Weight)), content)
                .Bind(section => Tiled(frame, section.Node, Deflated(section.Rect, half), half, outer));
    }

    private static Seq<(Hierarchy Node, SKRect Rect)> Squarified(Seq<Hierarchy> items, SKRect rect) {
        Seq<(Hierarchy Node, SKRect Rect)> Row() {
            Seq<float> areas = items.Map(item => item.Weight * rect.Width * rect.Height / items.Sum(static held => held.Weight));
            PhysicalAxis along = rect.Width >= rect.Height ? PhysicalAxis.Horizontal : PhysicalAxis.Vertical;
            float side = along.Cross.Of(rect.Size);
            int taken = toSeq(Range(1, areas.Count)).TakeWhile(count => count is 1 || Worst(areas.Take(count), side) <= Worst(areas.Take(count - 1), side)).Last.IfNone(1);
            float thickness = TensorPrimitives.Sum<float>([.. areas.Take(taken)]) / side;
            Seq<float> starts = areas.Scan(0f, static (sum, area) => sum + area);
            Seq<(Hierarchy Node, SKRect Rect)> row = toSeq(Range(0, taken)).Map(index => (items[index], along.Spanned(along.Cross.Spanned(rect, along.Cross.Of(rect.Location) + (starts[index] / thickness), areas[index] / thickness), along.Of(rect.Location), thickness)));
            return row + Squarified(items.Skip(taken), along.Spanned(rect, along.Of(rect.Location) + thickness, along.Of(rect.Size) - thickness));
        }
        return items.IsEmpty || rect.Width <= 0f || rect.Height <= 0f ? [] : Row();
    }

    private static float Worst(Seq<float> row, float side) {
        float[] areas = [.. row];
        (float sum, float most, float least) = (TensorPrimitives.Sum<float>(areas), TensorPrimitives.Max<float>(areas), TensorPrimitives.Min<float>(areas));
        return float.Max(side * side * most / (sum * sum), sum * sum / (side * side * least));
    }

    private static Subtree Shaped(Hierarchy node) {
        Subtree Joined(Subtree left, Subtree right) {
            float offset = left.Contour.Zip(right.Contour, static (one, two) => one.Right - two.Left + 1f).DefaultIfEmpty(0f).Max();
            return new Subtree(left.Spots + right.Spots.Map(spot => spot.Moved(new Vector2(offset, 0f))),
                toSeq(Range(0, int.Max(left.Contour.Count, right.Contour.Count))).Map(depth => (left.Contour.At(depth), right.Contour.At(depth).Map(held => (Left: held.Left + offset, Right: held.Right + offset))) switch {
                    var (one, two) => (one, two).Apply(static (first, second) => (Left: float.Min(first.Left, second.Left), Right: float.Max(first.Right, second.Right))).As() | one | two,
                }).Somes());
        }
        Subtree Grouped(Subtree children) {
            Seq<float> roots = children.Spots.Filter(static spot => spot.From.IsNone).Map(static spot => spot.At.X);
            Vector2 shift = new(-(roots.Head.IfNone(0f) + roots.Last.IfNone(0f)) / 2f, 1f);
            return new Subtree(
                new Spot(node, Vector2.Zero, None).Cons(children.Spots.Map(spot => spot.Moved(shift)).Map(static spot => spot with { From = spot.From | Some(Vector2.Zero) })),
                (Left: 0f, Right: 0f).Cons(children.Contour.Map(depth => (Left: depth.Left + shift.X, Right: depth.Right + shift.X))));
        }
        return node.Members.IsEmpty ? new Subtree([new Spot(node, Vector2.Zero, None)], [(0f, 0f)]) : Grouped(node.Members.Map(Shaped).Reduce(Joined));
    }

    private static SKRect Deflated(SKRect rect, float by) =>
        SKRect.Inflate(rect, -by, -by) switch { var inset => new SKRect(float.Min(inset.Left, rect.MidX), float.Min(inset.Top, rect.MidY), float.Max(inset.Right, rect.MidX), float.Max(inset.Bottom, rect.MidY)) };

    // --- [FLOWS]
    private const int Relaxations = 6;

    public static Fin<Network> Sankey(PlotFrame frame, Seq<Flow> flows, Fraction width, Fraction gap) {
        Seq<Key> keys = toSeq(flows.Bind(static flow => Seq(flow.Source, flow.Target)).Distinct());
        HashMap<Key, int> Deepened(HashMap<Key, int> depths) => flows.Fold(depths, static (held, flow) => held.SetItem(flow.Target, int.Max(held[flow.Target], held[flow.Source] + 1)));
        HashMap<Key, int> depths = Range(0, keys.Count).Fold(toHashMap(keys.Map(static key => (key, 0))), (held, _) => Deepened(held));
        HashMap<Key, Seq<Flow>> outgoing = toHashMap(flows.GroupBy(static flow => flow.Source).Select(static group => (group.Key, toSeq(group))));
        HashMap<Key, Seq<Flow>> incoming = toHashMap(flows.GroupBy(static flow => flow.Target).Select(static group => (group.Key, toSeq(group))));
        return (UIFault.Unless(Deepened(depths) == depths, UIReason.Order, flows.Count), UIFault.Unless(toHashSet(flows.Map(static flow => flow.Key)).Count == flows.Count, UIReason.Repeated, flows.Count))
            .Apply(static (_, _) => unit).As().Map(_ => {
                SKRect box = frame.Box;
                float Total(HashMap<Key, Seq<Flow>> linked, Key key) => linked.Find(key).Map(static held => held.Sum(static flow => flow.Weight)).IfNone(0f);
                HashMap<Key, float> values = toHashMap(keys.Map(key => (key, float.Max(Total(outgoing, key), Total(incoming, key)))));
                Seq<Seq<Key>> columns = toSeq(keys.GroupBy(key => depths[key]).OrderBy(static column => column.Key).Select(static column => toSeq(column)));
                float node = (float)width * box.Width / columns.Count;
                float spacing = (float)gap * box.Height / float.Max(1f, columns.Max(static column => column.Count) - 1);
                float scale = columns.Min(column => (box.Height - ((column.Count - 1) * spacing)) / column.Sum(key => values[key]));
                float Left(Key key) => box.Left + (columns.Count > 1 ? depths[key] * (box.Width - node) / (columns.Count - 1) : (box.Width - node) / 2f);
                float Middle(HashMap<Key, float> held, Key key) => held[key] + (values[key] * scale / 2f);
                HashMap<Key, float> Collided(HashMap<Key, float> held, Seq<Key> column) {
                    Seq<Key> ordered = toSeq(column.OrderBy(key => held[key]));
                    HashMap<Key, float> pushed = ordered.Fold((Tops: held, Floor: box.Top), (state, key) => float.Max(state.Tops[key], state.Floor) switch {
                        var top => (state.Tops.SetItem(key, top), top + (values[key] * scale) + spacing),
                    }).Tops;
                    return ordered.Rev().Fold((Tops: pushed, Ceiling: box.Bottom), (state, key) => float.Min(state.Tops[key], state.Ceiling - (values[key] * scale)) switch {
                        var top => (state.Tops.SetItem(key, top), top - spacing),
                    }).Tops;
                }
                HashMap<Key, float> Toward(HashMap<Key, float> held, Seq<Key> column, HashMap<Key, Seq<Flow>> linked, Func<Flow, Key> other, float alpha) =>
                    Collided(column.Fold(held, (moved, key) => linked.Find(key).Map(near =>
                        moved.SetItem(key, moved[key] + (alpha * ((near.Sum(flow => Middle(moved, other(flow)) * flow.Weight) / near.Sum(static flow => flow.Weight)) - Middle(moved, key))))).IfNone(moved)), column);
                HashMap<Key, float> tops = Range(0, Relaxations).Fold(
                    toHashMap(columns.Bind(column => column.Zip(column.Scan(box.Top, (top, key) => top + (values[key] * scale) + spacing), static (key, top) => (key, top)))),
                    (held, iteration) => MathF.Pow(0.99f, iteration) switch {
                        var alpha => columns.Fold(columns.Rev().Fold(held, (state, column) => Toward(state, column, outgoing, static flow => flow.Target, alpha)), (state, column) => Toward(state, column, incoming, static flow => flow.Source, alpha)),
                    });
                HashMap<Key, float> Offsets(HashMap<Key, Seq<Flow>> linked, Func<Flow, Key> other) =>
                    toHashMap(linked.Values.Bind(group => toSeq(group.OrderBy(flow => Middle(tops, other(flow)))) switch {
                        var sorted => toSeq(sorted.Zip(sorted.Scan(0f, static (sum, flow) => sum + flow.Weight), static (flow, offset) => (flow.Key, offset))),
                    }));
                HashMap<Key, float> leaving = Offsets(outgoing, static flow => flow.Target);
                HashMap<Key, float> entering = Offsets(incoming, static flow => flow.Source);
                return new Network(
                    keys.Choose(key => Rectangle(frame, key, values[key], SKRect.Create(Left(key), tops[key], node, values[key] * scale))),
                    flows.Choose(flow => {
                        SKPoint from = new(Left(flow.Source) + node, tops[flow.Source] + (leaving[flow.Key] * scale));
                        SKPoint to = new(Left(flow.Target), tops[flow.Target] + (entering[flow.Key] * scale));
                        SKPoint thickness = new(0f, flow.Weight * scale);
                        return Unframed(frame, [from, to, to + thickness, from + thickness]).Map(link => new Arranged(flow.Key, Vector2.Lerp(link[0], link[3], 0.5f), flow.Weight, new Figure.Path(link) { Closed = true, Interpolation = Interpolation.BumpX }));
                    }));
            });
    }

    public static Fin<Network> Chord(PlotPlane.Polar plane, Seq<Key> groups, Matrix<float> matrix, Fraction gap, Fraction inner) {
        int count = groups.Count;
        Seq<float> totals = toSeq(matrix.RowSums().Enumerate());
        float total = matrix.Enumerate().Sum();
        return UIFault.Unless(matrix.RowCount == count && matrix.ColumnCount == count && matrix.ForAll(static cell => float.IsFinite(cell) && cell >= 0f) && total > 0f && count * (float)gap < 1f, UIReason.Range, (count, total, gap))
            .Map(_ => {
                float share = (1f - (count * (float)gap)) / total;
                Seq<float> starts = totals.Scan(0f, (start, sum) => start + (sum * share) + (float)gap);
                Seq<Seq<float>> prefixes = toSeq(matrix.EnumerateRows()).Map(static row => toSeq(row.Enumerate()).Scan(0f, static (sum, cell) => sum + cell));
                Matrix<float> weights = matrix + matrix.Transpose();
                (float Low, float High) Slice(int row, int column) => (starts[row] + (prefixes[row][column] * share)) switch { var start => (start, start + (matrix[row, column] * share)) };
                Vector2 Valued(float angle, float radius) => plane.Valued(new Vector2(angle, radius));
                Seq<Arranged> arcs = groups.Map((key, index) =>
                    Figure.Region.Spanning(plane.Value(PhysicalAxis.Horizontal, starts[index]), plane.Value(PhysicalAxis.Horizontal, starts[index] + (totals[index] * share)), plane.Value(PhysicalAxis.Vertical, (float)inner), plane.Value(PhysicalAxis.Vertical, 1f))
                        .Map(region => new Arranged(key, Valued(starts[index] + (totals[index] * share / 2f), 1f), totals[index], region))).Somes();
                Seq<Arranged> ribbons = from row in toSeq(Range(0, count))
                                        from column in toSeq(Range(row, count - row))
                                        let weight = weights[row, column]
                                        where weight > 0f
                                        let source = Slice(row, column)
                                        let target = Slice(column, row)
                                        select new Arranged(new Key.Ordinal((uint)((row * count) + column)), Valued(source.Low, (float)inner), weight, new Figure.Path(
                                            (Seq(new Vector2(source.Low, (float)inner), new Vector2(source.High, (float)inner)) + Bend(source.High, target.Low, (float)inner)
                                                + Seq(new Vector2(target.Low, (float)inner), new Vector2(target.High, (float)inner)) + Bend(target.High, source.Low, (float)inner))
                                            .Map(at => Valued(at.X, at.Y))) { Closed = true });
                return new Network(arcs, ribbons);
            });
    }

    private static Seq<Vector2> Bend(float from, float to, float radius) {
        (Vector2 start, Vector2 end) = (Circled(from, radius), Circled(to, radius));
        return toSeq(Generate.LinearSpaced(int.Max(3, (int)MathF.Ceiling(Vector2.Distance(start, end) * 360f / MathF.Tau) + 1), 0d, 1d)[1..^1]).Map(step => (float)step switch {
            var along => (((1f - along) * (1f - along) * start) + (along * along * end)) switch { var at => new Vector2(Euclid.Modulus(MathF.Atan2(at.Y, at.X) / MathF.Tau, 1f), at.Length()) },
        });
    }

    private static Vector2 Circled(float turn, float radius) => MathF.SinCos(MathF.Tau * turn) switch { var (sin, cos) => new Vector2(cos, sin) * radius };

    // --- [FIELDS]
    private static readonly Seq<Seq<(int From, int To)>> Cuts = Seq(
        Seq<(int From, int To)>(), Seq((3, 0)), Seq((0, 1)), Seq((3, 1)), Seq((1, 2)), Seq((3, 0), (1, 2)), Seq((0, 2)), Seq((3, 2)),
        Seq((2, 3)), Seq((0, 2)), Seq((0, 1), (2, 3)), Seq((1, 2)), Seq((3, 1)), Seq((0, 1)), Seq((3, 0)), Seq<(int From, int To)>());

    private static readonly HashMap<int, Seq<(int From, int To)>> Joined = HashMap((5, Seq((3, 2), (0, 1))), (10, Seq((3, 0), (1, 2))));

    public static Seq<Arranged> Contours(PlotPlane plane, Figure.Field field, Seq<float> thresholds) {
        (int columns, int rows) = (field.Columns, field.Rows);
        float Height(int column, int row) => column < 0 || row < 0 || column >= columns || row >= rows ? float.NegativeInfinity : field.Values.Values.Span[(row * columns) + column];
        long Node(int column, int row) => ((row + 1L) * (columns + 2L)) + column + 1L;
        Vector2 Center(long node) => new(((int)(node % (columns + 2)) - 0.5f) / columns, ((int)(node / (columns + 2)) - 0.5f) / rows);
        return thresholds.Map((threshold, level) => {
            Coordinate Crossed(long edge) {
                long low = edge / 2;
                long high = edge % 2 is 0 ? low + 1 : low + columns + 2;
                (float from, float to) = (Height((int)(low % (columns + 2)) - 1, (int)(low / (columns + 2)) - 1), Height((int)(high % (columns + 2)) - 1, (int)(high / (columns + 2)) - 1));
                Vector2 at = Vector2.Lerp(Center(low), Center(high), float.IsFinite(from) ? (float.IsFinite(to) ? (threshold - from) / (to - from) : 0f) : 1f);
                return new Coordinate(at.X, at.Y);
            }
            LineMerger merger = new();
            merger.Add(from row in toSeq(Range(-1, rows + 1))
                       from column in toSeq(Range(-1, columns + 1))
                       let corners = Seq(Height(column, row), Height(column + 1, row), Height(column + 1, row + 1), Height(column, row + 1))
                       let shape = corners.Map((height, bit) => height >= threshold ? 1 << bit : 0).Fold(0, static (mask, bit) => mask | bit)
                       let origin = Node(column, row)
                       let edges = Seq(2 * origin, (2 * Node(column + 1, row)) + 1, 2 * Node(column, row + 1), (2 * origin) + 1)
                       from cut in (TensorPrimitives.Sum<float>([.. corners]) / 4f >= threshold ? Joined.Find(shape) : None).IfNone(() => Cuts[shape])
                       select (Geometry)GeometryFactory.Default.CreateLineString([Crossed(edges[cut.From]), Crossed(edges[cut.To])]));
            Seq<Seq<Vector2>> rings = toSeq(merger.GetMergedLineStrings()).Map(ring => Figure.Ring((LineString)ring).Map(at => plane.Valued(at)));
            return rings.Head.Map(outer => new Arranged(new Key.Ordinal((uint)level), outer[0], threshold, new Figure.Path(outer) { Closed = true, Holes = rings.Tail }));
        }).Somes();
    }

    public static Seq<Arranged> Hexbins(PlotFrame frame, Seq<Vector2> points, Factor radius) {
        (float size, float across, float down) = ((float)radius, (float)radius * MathF.Sqrt(3f), (float)radius * 1.5f);
        SKPoint origin = frame.Box.Location;
        Seq<SKPoint> Corners(SKPoint center) => toSeq(Range(0, 6)).Map(corner => MathF.SinCos((MathF.PI / 6f) + (corner * MathF.PI / 3f)) switch { var (sin, cos) => center + new SKPoint(cos * size, sin * size) });
        return toSeq(points.Map(frame.Point).Filter(static at => float.IsFinite(at.X) && float.IsFinite(at.Y)).GroupBy(at => Binned(at - origin, across, down)))
            .Choose(bin => (origin + new SKPoint((bin.Key.Column + ((bin.Key.Row & 1) / 2f)) * across, bin.Key.Row * down)) switch {
                var middle => (frame.Valued(middle), Unframed(frame, Corners(middle)))
                    .Apply((center, corners) => new Arranged(new Key.Ordinal(unchecked((ushort)bin.Key.Column | ((uint)(ushort)bin.Key.Row << 16))), center, bin.Count(), new Figure.Path(corners) { Closed = true })).As(),
            });
    }

    private static (int Column, int Row) Binned(SKPoint at, float across, float down) {
        float row = at.Y / down;
        int j = (int)MathF.Round(row, MidpointRounding.ToEven);
        float column = (at.X / across) - ((j & 1) / 2f);
        int i = (int)MathF.Round(column, MidpointRounding.ToEven);
        float rowOffset = row - j;
        (float otherI, int otherJ) = (i + ((column < i ? -1f : 1f) / 2f), j + (row < j ? -1 : 1));
        bool farther = MathF.Abs(rowOffset) * 3f > 1f && ((column - i) * (column - i)) + (rowOffset * rowOffset) > ((column - otherI) * (column - otherI)) + ((row - otherJ) * (row - otherJ));
        return farther ? ((int)MathF.Round(otherI + ((j & 1) is 1 ? 0.5f : -0.5f), MidpointRounding.ToEven), otherJ) : (i, j);
    }

    // --- [DISTRIBUTIONS]
    private const float Slack = 1e-5f;

    public static Figure.Points Dodged(PlotFrame frame, PhysicalAxis along, Seq<float> values, Factor radius) {
        float reach = 2f * (float)radius;
        Seq<float> placed = values.Map(value => along.Of(frame.Point(along.With(Vector2.Zero, value))));
        RBush<Indexed> tree = new();
        HashMap<int, float> offsets = toSeq(placed.Map(static (at, index) => (At: at, Index: index)).OrderBy(static held => held.At)).Fold(HashMap<int, float>(), (held, point) => {
            Seq<(float Offset, float Clear)> near = toSeq(tree.Search(new(point.At - reach, double.NegativeInfinity, point.At + reach, double.PositiveInfinity)))
                .Map(other => (Offset: held[other.Index], Clear: MathF.Sqrt((reach * reach) - ((placed[other.Index] - point.At) * (placed[other.Index] - point.At)))))
                .Filter(static other => float.IsFinite(other.Clear));
            float offset = toSeq(Seq(0f) + near.Bind(static other => Seq(other.Offset + other.Clear, other.Offset - other.Clear)))
                .Filter(candidate => near.ForAll(other => MathF.Abs(candidate - other.Offset) >= other.Clear - (Slack * reach)))
                .MinBy(static candidate => MathF.Abs(candidate));
            tree.Insert(new Indexed(point.Index, new(point.At, offset, point.At, offset)));
            return held.Add(point.Index, offset);
        });
        return Spread(frame, along, placed, index => offsets[index]);
    }

    public static Figure.Points Jittered(PlotFrame frame, PhysicalAxis along, Seq<float> values, Factor width, int seed) {
        double[] uniform = Xoshiro256StarStar.Doubles(values.Count, seed);
        return Spread(frame, along, values.Map(value => along.Of(frame.Point(along.With(Vector2.Zero, value)))), index => ((float)uniform[index] - 0.5f) * (float)width);
    }

    private static Figure.Points Spread(PlotFrame frame, PhysicalAxis along, Seq<float> placed, Func<int, float> offset) {
        float middle = frame.Box switch { var box => along.Cross.Of(new Vector2(box.MidX, box.MidY)) };
        return new Figure.Points(placed.Map((at, index) => (SKPoint)along.With(along.Cross.With(Vector2.Zero, middle + offset(index)), at)).Choose(frame.Valued));
    }

    // --- [MESHES]
    public static Network Triangulated(PlotFrame frame, Seq<Vector2> sites) {
        Coordinate[] framed = Framed(frame, sites);
        DelaunayTriangulationBuilder builder = new();
        builder.SetSites(framed);
        Geometry hull = GeometryFactory.Default.CreateMultiPointFromCoords(framed).ConvexHull();
        return new Network(
            Cells(frame, builder.GetTriangles(GeometryFactory.Default), static (_, index) => (uint)index),
            toSeq(builder.GetEdges(GeometryFactory.Default).Geometries).Map(static (edge, index) => (Edge: edge, Index: index)).Choose(held => Unframed(frame, toSeq(held.Edge.Coordinates).Map(Figure.Point))
                .Map(line => new Arranged(new Key.Ordinal((uint)held.Index), Vector2.Lerp(line[0], line[1], 0.5f), (float)held.Edge.Length, new Figure.Path(line))))) {
            Hull = Unframed(frame, hull is Polygon polygon ? Figure.Ring(polygon.ExteriorRing) : toSeq(hull.Coordinates).Map(Figure.Point)).Filter(static corners => !corners.IsEmpty)
                .Map(corners => new Figure.Path(corners) { Closed = hull is Polygon }),
        };
    }

    public static Seq<Arranged> Voronoi(PlotFrame frame, Seq<Vector2> sites) {
        Coordinate[] framed = Framed(frame, sites);
        FrozenDictionary<Coordinate, int> owners = framed.Select(static (site, index) => (Site: site, Index: index)).DistinctBy(static held => held.Site).ToFrozenDictionary(static held => held.Site, static held => held.Index);
        SKRect box = frame.Box;
        VoronoiDiagramBuilder builder = new() { ClipEnvelope = new(box.Left, box.Right, box.Top, box.Bottom) };
        builder.SetSites(framed);
        return Cells(frame, builder.GetDiagram(GeometryFactory.Default), (cell, _) => (uint)owners[(Coordinate)cell.UserData]);
    }

    private static Coordinate[] Framed(PlotFrame frame, Seq<Vector2> sites) => Figure.Coordinates(sites.Map(frame.Point));

    private static Seq<Arranged> Cells(PlotFrame frame, Geometry cells, Func<Geometry, int, uint> owner) =>
        toSeq(Enumerable.Range(0, cells.NumGeometries).Select(cells.GetGeometryN)).Map(static (cell, index) => (Cell: cell, Index: index)).Choose(held =>
            (frame.Valued(Figure.Point(held.Cell.Centroid.Coordinate)), Unframed(frame, Figure.Ring(((Polygon)held.Cell).ExteriorRing)))
                .Apply((center, corners) => new Arranged(new Key.Ordinal(owner(held.Cell, held.Index)), center, (float)held.Cell.Area, new Figure.Path(corners) { Closed = true })).As());

    // --- [GEOGRAPHY]
    public static Seq<Figure.Path> Paths(Geometry geometry) =>
        geometry switch {
            Polygon polygon => [new Figure.Path(Vertices(polygon.ExteriorRing)) { Closed = true, Holes = toSeq(polygon.InteriorRings).Map(Vertices) }],
            LineString line => [new Figure.Path(Vertices(line)) { Closed = line.IsClosed }],
            GeometryCollection collection => toSeq(collection.Geometries).Bind(Paths),
            _ => [],
        };

    private static Seq<Vector2> Vertices(LineString line) => Figure.Ring(line).Map(static at => (Vector2)at);

    // --- [FRAMES]
    private static Option<Seq<Vector2>> Unframed(PlotFrame frame, Seq<SKPoint> pixels) => pixels.Traverse(pixel => frame.Valued(pixel)).As();

    private static Option<Arranged> Rectangle(PlotFrame frame, Key key, float weight, SKRect rect) =>
        from low in frame.Valued(new SKPoint(rect.Left, rect.Bottom))
        from high in frame.Valued(new SKPoint(rect.Right, rect.Top))
        from region in Figure.Region.Spanning(low.X, high.X, low.Y, high.Y)
        select new Arranged(key, Vector2.Lerp(low, high, 0.5f), weight, region);
}
