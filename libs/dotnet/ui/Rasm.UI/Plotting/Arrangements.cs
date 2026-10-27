using System.Numerics;
using System.Numerics.Tensors;
using MathNet.Numerics;
using MathNet.Numerics.Random;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Triangulate;
using RBush;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Plotting;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Hierarchy {
    private Hierarchy(Key key, float weight, Seq<Hierarchy> members) => (Key, Weight, Members) = (key, weight, members);

    public Key Key { get; }
    public float Weight { get; }
    public Seq<Hierarchy> Members { get; }

    public sealed record Leaf : Hierarchy {
        private Leaf(Key key, float weight) : base(key, weight, []) { }

        public static Fin<Leaf> Validated(Key key, float weight) => UIFault.Unless(float.IsFinite(weight) && weight >= 0f, UIReason.Range, weight).Map(_ => new Leaf(key, weight));
    }

    public sealed record Group(Key Key, Seq<Hierarchy> Members) : Hierarchy(Key, Members.Sum(static member => member.Weight), Members);
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

public sealed record Network(Seq<Arranged> Nodes, Seq<Arranged> Links);

public sealed record Triangulation(Seq<Arranged> Triangles, Seq<Arranged> Edges, Option<Figure.Path> Hull);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Arrangements {
    // --- [HIERARCHIES]
    private sealed record Section(Hierarchy Node, SKRect Rect);

    private sealed record Spot(Hierarchy Node, Vector2 At, Option<Vector2> From) {
        public Spot Moved(Vector2 by) => this with { At = At + by, From = From.Map(from => from + by) };
    }

    private sealed record Breadth(float Left, float Right) {
        public Breadth Moved(float by) => new(Left + by, Right + by);
    }

    private sealed record Subtree(Seq<Spot> Spots, Seq<Breadth> Contour);

    public static Fin<Seq<Arranged>> Treemap(PlotFrame frame, Hierarchy root, float inner = 0f, float outer = 0f) =>
        UIFault.Unless(float.IsFinite(inner) && float.IsFinite(outer) && inner >= 0f && outer >= 0f, UIReason.Range, (inner, outer)).Map(_ => Tiled(frame, root, Box(frame), inner / 2f, outer));

    public static Network Tree(PlotFrame frame, Hierarchy root) {
        Subtree shape = Shaped(root);
        SKRect box = Box(frame);
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

    private static Seq<Section> Squarified(Seq<Hierarchy> items, SKRect rect) {
        Seq<Section> Row() {
            Seq<float> areas = items.Map(item => item.Weight * rect.Width * rect.Height / items.Sum(static held => held.Weight));
            PhysicalAxis along = rect.Width >= rect.Height ? PhysicalAxis.Horizontal : PhysicalAxis.Vertical;
            float side = along.Cross.Of(rect.Size);
            int taken = toSeq(Range(1, areas.Count)).TakeWhile(count => count is 1 || Worst(areas.Take(count), side) <= Worst(areas.Take(count - 1), side)).Last.IfNone(1);
            float thickness = TensorPrimitives.Sum<float>([.. areas.Take(taken)]) / side;
            Seq<float> starts = areas.Scan(0f, static (sum, area) => sum + area);
            Seq<Section> row = toSeq(Range(0, taken)).Map(index => new Section(items[index], along.Spanned(along.Cross.Spanned(rect, along.Cross.Of(rect.Location) + (starts[index] / thickness), areas[index] / thickness), along.Of(rect.Location), thickness)));
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
                toSeq(Range(0, int.Max(left.Contour.Count, right.Contour.Count))).Map(depth => (left.Contour.At(depth), right.Contour.At(depth).Map(held => held.Moved(offset))) switch {
                    var (one, two) => (one, two).Apply(static (first, second) => new Breadth(float.Min(first.Left, second.Left), float.Max(first.Right, second.Right))).As() | one | two,
                }).Somes());
        }
        Subtree Grouped(Subtree children) {
            Seq<float> roots = children.Spots.Filter(static spot => spot.From.IsNone).Map(static spot => spot.At.X);
            Vector2 shift = new(-(roots.Head.IfNone(0f) + roots.Last.IfNone(0f)) / 2f, 1f);
            return new Subtree(
                new Spot(node, Vector2.Zero, None).Cons(children.Spots.Map(spot => spot.Moved(shift)).Map(static spot => spot with { From = spot.From | Some(Vector2.Zero) })),
                new Breadth(0f, 0f).Cons(children.Contour.Map(depth => depth.Moved(shift.X))));
        }
        return node.Members.IsEmpty ? new Subtree([new Spot(node, Vector2.Zero, None)], [new Breadth(0f, 0f)]) : Grouped(node.Members.Map(Shaped).Reduce(Joined));
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
                SKRect box = Box(frame);
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

    public static Fin<Network> Chord(PlotPlane.Polar plane, Seq<Key> groups, Seq<Seq<float>> matrix, Fraction gap, Fraction inner) {
        int count = groups.Count;
        Seq<float> totals = matrix.Map(static row => TensorPrimitives.Sum<float>([.. row]));
        float total = TensorPrimitives.Sum<float>([.. totals]);
        return UIFault.Unless(matrix.Count == count && matrix.ForAll(row => row.Count == count && row.ForAll(static cell => float.IsFinite(cell) && cell >= 0f)) && total > 0f && count * (float)gap < 1f, UIReason.Range, (count, total, gap))
            .Map(_ => {
                float share = (1f - (count * (float)gap)) / total;
                Seq<float> starts = totals.Scan(0f, (start, sum) => start + (sum * share) + (float)gap);
                FractionRange Slice(int row, int column) => (starts[row] + (TensorPrimitives.Sum<float>([.. matrix[row].Take(column)]) * share)) switch { var start => new FractionRange(start, start + (matrix[row][column] * share)) };
                Vector2 Valued(float angle, float radius) => plane.Valued(new Vector2(angle, radius));
                Seq<Arranged> arcs = groups.Map((key, index) => (AxisRange.Validated(plane.Value(PhysicalAxis.Horizontal, starts[index]), plane.Value(PhysicalAxis.Horizontal, starts[index] + (totals[index] * share))),
                        AxisRange.Validated(plane.Value(PhysicalAxis.Vertical, (float)inner), plane.Value(PhysicalAxis.Vertical, 1f)))
                    .Apply((angle, radius) => new Arranged(key, Valued(starts[index] + (totals[index] * share / 2f), 1f), totals[index], new Figure.Region { First = Some(angle), Second = Some(radius) })).As().ToOption()).Somes();
                Seq<Arranged> ribbons = from row in toSeq(Range(0, count))
                                        from column in toSeq(Range(row, count - row))
                                        let weight = matrix[row][column] + matrix[column][row]
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
            var along => (((1f - along) * (1f - along) * start) + (along * along * end)) switch { var at => new Vector2(((MathF.Atan2(at.Y, at.X) / MathF.Tau % 1f) + 1f) % 1f, at.Length()) },
        });
    }

    private static Vector2 Circled(float turn, float radius) => MathF.SinCos(MathF.Tau * turn) switch { var (sin, cos) => new Vector2(cos, sin) * radius };

    // --- [FIELDS]
    private sealed record Cut(long From, long To);

    private sealed record Hexagon(int Column, int Row) {
        public Key Key => new Key.Ordinal(unchecked((ushort)Column | ((uint)(ushort)Row << 16)));

        public SKPoint Center(float across, float down) => new((Column + ((Row & 1) / 2f)) * across, Row * down);
    }

    private static readonly Seq<Seq<Cut>> Cuts = Seq(
        Seq<Cut>(), Seq(new Cut(3, 0)), Seq(new Cut(0, 1)), Seq(new Cut(3, 1)), Seq(new Cut(1, 2)), Seq(new Cut(3, 0), new Cut(1, 2)), Seq(new Cut(0, 2)), Seq(new Cut(3, 2)),
        Seq(new Cut(2, 3)), Seq(new Cut(0, 2)), Seq(new Cut(0, 1), new Cut(2, 3)), Seq(new Cut(1, 2)), Seq(new Cut(3, 1)), Seq(new Cut(0, 1)), Seq(new Cut(3, 0)), Seq<Cut>());

    private static readonly HashMap<int, Seq<Cut>> Joined = HashMap((5, Seq(new Cut(3, 2), new Cut(0, 1))), (10, Seq(new Cut(3, 0), new Cut(1, 2))));

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
                       select (Geometry)GeometryFactory.Default.CreateLineString([Crossed(edges[(int)cut.From]), Crossed(edges[(int)cut.To])]));
            Seq<Seq<Vector2>> rings = toSeq(merger.GetMergedLineStrings()).Map(ring => toSeq(ring.Coordinates[..^1]).Map(at => plane.Valued(new Vector2((float)at.X, (float)at.Y))));
            return rings.Head.Map(outer => new Arranged(new Key.Ordinal((uint)level), outer[0], threshold, new Figure.Path(outer) { Closed = true, Holes = rings.Tail }));
        }).Somes();
    }

    public static Seq<Arranged> Hexbins(PlotFrame frame, Seq<Vector2> points, Factor radius) {
        (float size, float across, float down) = ((float)radius, (float)radius * MathF.Sqrt(3f), (float)radius * 1.5f);
        SKPoint origin = Box(frame).Location;
        Seq<SKPoint> Corners(SKPoint center) => toSeq(Range(0, 6)).Map(corner => MathF.SinCos((MathF.PI / 6f) + (corner * MathF.PI / 3f)) switch { var (sin, cos) => center + new SKPoint(cos * size, sin * size) });
        return toSeq(points.Map(frame.Point).Filter(static at => float.IsFinite(at.X) && float.IsFinite(at.Y)).GroupBy(at => Binned(at - origin, across, down)))
            .Choose(bin => (origin + bin.Key.Center(across, down)) switch {
                var middle => (frame.Valued(middle), Unframed(frame, Corners(middle))).Apply((center, corners) => new Arranged(bin.Key.Key, center, bin.Count(), new Figure.Path(corners) { Closed = true })).As(),
            });
    }

    private static Hexagon Binned(SKPoint at, float across, float down) {
        float row = at.Y / down;
        int j = (int)MathF.Round(row, MidpointRounding.ToEven);
        float column = (at.X / across) - ((j & 1) / 2f);
        int i = (int)MathF.Round(column, MidpointRounding.ToEven);
        float rowOffset = row - j;
        (float otherI, int otherJ) = (i + ((column < i ? -1f : 1f) / 2f), j + (row < j ? -1 : 1));
        bool farther = MathF.Abs(rowOffset) * 3f > 1f && ((column - i) * (column - i)) + (rowOffset * rowOffset) > ((column - otherI) * (column - otherI)) + ((row - otherJ) * (row - otherJ));
        return farther ? new Hexagon((int)MathF.Round(otherI + ((j & 1) is 1 ? 0.5f : -0.5f), MidpointRounding.ToEven), otherJ) : new Hexagon(i, j);
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
        float middle = Box(frame) switch { var box => along.Cross.Of(new Vector2(box.MidX, box.MidY)) };
        return new Figure.Points(placed.Map((at, index) => (SKPoint)along.With(along.Cross.With(Vector2.Zero, middle + offset(index)), at)).Choose(frame.Valued));
    }

    // --- [MESHES]
    public static Triangulation Triangulated(PlotFrame frame, Seq<Vector2> sites) {
        Coordinate[] framed = Framed(frame, sites);
        DelaunayTriangulationBuilder builder = new();
        builder.SetSites(framed);
        Geometry hull = GeometryFactory.Default.CreateMultiPointFromCoords(framed).ConvexHull();
        return new Triangulation(
            Cells(frame, builder.GetTriangles(GeometryFactory.Default), static (_, index) => (uint)index),
            toSeq(builder.GetEdges(GeometryFactory.Default).Geometries).Map(static (edge, index) => (Edge: edge, Index: index)).Choose(held => Unframed(frame, toSeq(held.Edge.Coordinates).Map(Figure.Point))
                .Map(line => new Arranged(new Key.Ordinal((uint)held.Index), Vector2.Lerp(line[0], line[1], 0.5f), (float)held.Edge.Length, new Figure.Path(line)))),
            Unframed(frame, toSeq(hull is Polygon polygon ? polygon.ExteriorRing.Coordinates[..^1] : hull.Coordinates).Map(Figure.Point)).Filter(static corners => !corners.IsEmpty)
                .Map(corners => new Figure.Path(corners) { Closed = hull is Polygon }));
    }

    public static Seq<Arranged> Voronoi(PlotFrame frame, Seq<Vector2> sites) {
        Coordinate[] framed = Framed(frame, sites);
        FrozenDictionary<Coordinate, int> owners = framed.Select(static (site, index) => (Site: site, Index: index)).DistinctBy(static held => held.Site).ToFrozenDictionary(static held => held.Site, static held => held.Index);
        SKRect box = Box(frame);
        VoronoiDiagramBuilder builder = new() { ClipEnvelope = new(box.Left, box.Right, box.Top, box.Bottom) };
        builder.SetSites(framed);
        return Cells(frame, builder.GetDiagram(GeometryFactory.Default), (cell, _) => (uint)owners[(Coordinate)cell.UserData]);
    }

    private static Coordinate[] Framed(PlotFrame frame, Seq<Vector2> sites) => Figure.Coordinates(sites.Map(frame.Point));

    private static Seq<Arranged> Cells(PlotFrame frame, Geometry cells, Func<Geometry, int, uint> owner) =>
        toSeq(Enumerable.Range(0, cells.NumGeometries).Select(cells.GetGeometryN)).Map(static (cell, index) => (Cell: cell, Index: index)).Choose(held =>
            (frame.Valued(Figure.Point(held.Cell.Centroid.Coordinate)), Unframed(frame, toSeq(((Polygon)held.Cell).ExteriorRing.Coordinates[..^1]).Map(Figure.Point)))
                .Apply((center, corners) => new Arranged(new Key.Ordinal(owner(held.Cell, held.Index)), center, (float)held.Cell.Area, new Figure.Path(corners) { Closed = true })).As());

    // --- [GEOGRAPHY]
    public static Seq<Figure.Path> Paths(Geometry geometry) =>
        geometry switch {
            Polygon polygon => [new Figure.Path(Ring(polygon.ExteriorRing)) { Closed = true, Holes = toSeq(polygon.InteriorRings).Map(Ring) }],
            LinearRing ring => [new Figure.Path(Ring(ring)) { Closed = true }],
            LineString line => [new Figure.Path(toSeq(line.Coordinates).Map(Vertex))],
            GeometryCollection collection => toSeq(collection.Geometries).Bind(Paths),
            _ => [],
        };

    private static Seq<Vector2> Ring(LineString ring) => toSeq(ring.Coordinates[..^1]).Map(Vertex);

    private static Vector2 Vertex(Coordinate at) => new((float)at.X, (float)at.Y);

    // --- [FRAMES]
    private static SKRect Box(PlotFrame frame) => frame.Matrix.MapRect(SKRect.Create(1f, 1f));

    private static Option<Seq<Vector2>> Unframed(PlotFrame frame, Seq<SKPoint> pixels) => pixels.Traverse(pixel => frame.Valued(pixel)).As();

    private static Option<Arranged> Rectangle(PlotFrame frame, Key key, float weight, SKRect rect) =>
        from low in frame.Valued(new SKPoint(rect.Left, rect.Bottom))
        from high in frame.Valued(new SKPoint(rect.Right, rect.Top))
        from across in AxisRange.Validated(low.X, high.X).ToOption()
        from up in AxisRange.Validated(low.Y, high.Y).ToOption()
        select new Arranged(key, Vector2.Lerp(low, high, 0.5f), weight, new Figure.Region { First = Some(across), Second = Some(up) });

}
