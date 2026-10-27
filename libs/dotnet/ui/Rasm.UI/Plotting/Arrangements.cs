using System.Numerics;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate;
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

    public sealed record Leaf(Key Key, float Weight) : Hierarchy(Key, Weight, []);

    public sealed record Group(Key Key, Seq<Hierarchy> Members) : Hierarchy(Key, Members.Fold(0f, static (sum, member) => sum + member.Weight), Members);
}

public sealed record Flow(Key Key, Key Source, Key Target, float Weight);

public sealed record Arranged(Key Key, Vector2 At, float Weight, Figure Figure);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Arrangements {
    private const float Slack = 1e-5f;

    // --- [HIERARCHIES]
    public static Fin<Seq<Arranged>> Treemap(Hierarchy root, AxisRange across, AxisRange down) =>
        toSeq(SuperEnumerable.TraverseDepthFirst(root, static node => node.Members))
            .Traverse(static node => UIFault.Unless(float.IsFinite(node.Weight) && node.Weight >= 0f, UIReason.Range, node.Weight)).As()
            .Map(_ => Tiled(root, new Tile(new Vector2(across.From, down.From), new Vector2(across.Span, down.Span))));

    public static Seq<Arranged> Tree(Hierarchy root, AxisRange across, AxisRange down) {
        Shape shape = Shaped(root);
        (float low, float high) = (shape.Contour.Fold(float.PositiveInfinity, static (least, depth) => float.Min(least, depth.Low)), shape.Contour.Fold(float.NegativeInfinity, static (most, depth) => float.Max(most, depth.High)));
        int deepest = shape.Contour.Count - 1;
        Vector2 Placed(Vector2 at) => new(
            across.From + ((high > low ? (at.X - low) / (high - low) : 0.5f) * across.Span),
            down.From + ((deepest > 0 ? at.Y / deepest : 0f) * down.Span));
        return shape.Spots.Map(spot => new Arranged(spot.Node.Key, Placed(spot.At), spot.Node.Weight, spot.From.Match(
            Some: from => (Figure)new Figure.Path([Placed(from), Placed(spot.At)]) { Interpolation = Interpolation.BumpY },
            None: () => new Figure.Points([Placed(spot.At)]))));
    }

    // --- [FLOWS]
    public static Fin<Seq<Arranged>> Sankey(Seq<Flow> flows, AxisRange across, AxisRange down, Fraction width, Fraction gap) {
        Seq<Key> keys = toSeq(flows.Bind(static flow => Seq(flow.Source, flow.Target)).Distinct());
        HashMap<Key, int> Relaxed(HashMap<Key, int> depths) => flows.Fold(depths, static (held, flow) => held.SetItem(flow.Target, int.Max(held[flow.Target], held[flow.Source] + 1)));
        HashMap<Key, int> depths = Range(0, keys.Count).Fold(toHashMap(keys.Map(static key => (key, 0))), (held, _) => Relaxed(held));
        return flows.Traverse(static flow => UIFault.Unless(float.IsFinite(flow.Weight) && flow.Weight >= 0f, UIReason.Range, flow.Weight)).As()
            .Bind(_ => UIFault.Unless(Relaxed(depths) == depths, UIReason.Order, flows.Count))
            .Map(_ => {
                HashMap<Key, float> leaving = flows.Fold(HashMap<Key, float>(), static (held, flow) => held.AddOrUpdate(flow.Source, weight => weight + flow.Weight, () => flow.Weight));
                HashMap<Key, float> entering = flows.Fold(HashMap<Key, float>(), static (held, flow) => held.AddOrUpdate(flow.Target, weight => weight + flow.Weight, () => flow.Weight));
                HashMap<Key, float> values = toHashMap(keys.Map(key => (key, float.Max(leaving.Find(key).IfNone(0f), entering.Find(key).IfNone(0f)))));
                Seq<Seq<Key>> columns = toSeq(keys.GroupBy(key => depths[key]).OrderBy(static column => column.Key).Select(static column => toSeq(column)));
                float node = (float)width * across.Span / columns.Count;
                float spacing = (float)gap * MathF.Abs(down.Span) / float.Max(1f, columns.Fold(0, static (most, column) => int.Max(most, column.Count)) - 1);
                float scale = columns.Fold(float.PositiveInfinity, (least, column) => float.Min(least, (MathF.Abs(down.Span) - ((column.Count - 1) * spacing)) / Total(column.Map(key => values[key])))) * MathF.Sign(down.Span);
                HashMap<Key, Vector2> corners = toHashMap(columns.Bind(column => column.Zip(column.Scan(0f, (top, key) => top + (values[key] * scale) + (spacing * MathF.Sign(down.Span))), (key, top) =>
                    (key, new Vector2(across.From + (columns.Count > 1 ? depths[key] * (across.Span - node) / (columns.Count - 1) : (across.Span - node) / 2f), down.From + top)))));
                Seq<Arranged> nodes = keys.Map(key => (AxisRange.Validated(corners[key].X, corners[key].X + node), AxisRange.Validated(corners[key].Y, corners[key].Y + (values[key] * scale)))
                    .Apply((x, y) => new Arranged(key, corners[key] + new Vector2(node / 2f, values[key] * scale / 2f), values[key], new Figure.Region { First = Some(x), Second = Some(y) })).As().ToOption()).Somes();
                Seq<Arranged> links = flows.Fold((Out: HashMap<Key, float>(), In: HashMap<Key, float>(), Links: Seq<Arranged>()), (held, flow) => {
                    (float leaving, float entering) = (held.Out.Find(flow.Source).IfNone(0f), held.In.Find(flow.Target).IfNone(0f));
                    (Vector2 from, Vector2 to) = (corners[flow.Source] + new Vector2(node, leaving * scale), corners[flow.Target] + new Vector2(0f, entering * scale));
                    Vector2 thickness = new(0f, flow.Weight * scale);
                    return (held.Out.AddOrUpdate(flow.Source, leaving + flow.Weight), held.In.AddOrUpdate(flow.Target, entering + flow.Weight),
                        held.Links.Add(new Arranged(flow.Key, Vector2.Lerp(from, to, 0.5f) + (thickness / 2f), flow.Weight, new Figure.Path([from, to, to + thickness, from + thickness]) { Closed = true, Interpolation = Interpolation.BumpX })));
                }).Links;
                return nodes + links;
            });
    }

    public static Fin<Seq<Arranged>> Chord(Seq<Key> groups, Seq<Seq<float>> matrix, Fraction gap, Fraction inner) {
        int count = groups.Count;
        float total = Total(matrix.Bind(static row => row));
        return UIFault.Unless(matrix.Count == count && matrix.ForAll(row => row.Count == count && row.ForAll(static cell => float.IsFinite(cell) && cell >= 0f)) && total > 0f && count * (float)gap < 1f, UIReason.Range, (count, total, gap))
            .Map(_ => {
                float share = (1f - (count * (float)gap)) / total;
                Seq<float> starts = matrix.Scan(0f, (start, row) => start + (Total(row) * share) + (float)gap);
                FractionRange Slice(int row, int column) => (starts[row] + (Total(matrix[row].Take(column)) * share)) switch { var start => new FractionRange(start, start + (matrix[row][column] * share)) };
                Seq<Arranged> arcs = groups.Map((key, index) => (AxisRange.Validated(starts[index], starts[index] + (Total(matrix[index]) * share)), AxisRange.Validated((float)inner, 1f))
                    .Apply((angle, radius) => new Arranged(key, new Vector2((angle.From + angle.To) / 2f, 1f), Total(matrix[index]), new Figure.Region { First = Some(angle), Second = Some(radius) })).As().ToOption()).Somes();
                Seq<Arranged> ribbons = from row in toSeq(Range(0, count))
                                        from column in toSeq(Range(row, count - row))
                                        let weight = matrix[row][column] + matrix[column][row]
                                        where weight > 0f
                                        let source = Slice(row, column)
                                        let target = Slice(column, row)
                                        select new Arranged(new Key.Ordinal((uint)((row * count) + column)), new Vector2(source.Low, (float)inner), weight, new Figure.Path(
                                            Arc(source.Low, source.High, (float)inner) + Bend(source.High, target.Low, (float)inner) + Arc(target.Low, target.High, (float)inner) + Bend(target.High, source.Low, (float)inner)) { Closed = true });
                return arcs + ribbons;
            });
    }

    // --- [FIELDS]
    public static Seq<Arranged> Contours(PlotPlane plane, Figure.Field field, Seq<float> thresholds) {
        (int columns, int rows) = (field.Columns, field.Rows);
        float Height(int column, int row) => column < 0 || row < 0 || column >= columns || row >= rows ? float.NegativeInfinity : field.Values.Values.Span[(row * columns) + column];
        long Node(int column, int row) => ((row + 1L) * (columns + 2L)) + column + 1L;
        Vector2 Center(long node) => new(((int)(node % (columns + 2)) - 0.5f) / columns, ((int)(node / (columns + 2)) - 0.5f) / rows);
        return thresholds.Map((threshold, level) => {
            Vector2 Crossed(long edge) {
                long low = edge / 2;
                long high = edge % 2 is 0 ? low + 1 : low + columns + 2;
                (float from, float to) = (Height((int)(low % (columns + 2)) - 1, (int)(low / (columns + 2)) - 1), Height((int)(high % (columns + 2)) - 1, (int)(high / (columns + 2)) - 1));
                return Vector2.Lerp(Center(low), Center(high), float.IsFinite(from) ? (float.IsFinite(to) ? (threshold - from) / (to - from) : 0f) : 1f);
            }
            Seq<Cut> segments = from row in toSeq(Range(-1, rows + 1))
                                from column in toSeq(Range(-1, columns + 1))
                                let corners = Seq(Height(column, row), Height(column + 1, row), Height(column + 1, row + 1), Height(column, row + 1))
                                let shape = corners.Map((height, bit) => height >= threshold ? 1 << bit : 0).Fold(0, static (bits, bit) => bits | bit)
                                let joined = Total(corners) / 4f >= threshold
                                let origin = Node(column, row)
                                let edges = Seq(2 * origin, (2 * Node(column + 1, row)) + 1, 2 * Node(column, row + 1), (2 * origin) + 1)
                                from cut in (joined ? Joined.Find(shape) : None).IfNone(() => Cuts[shape])
                                select new Cut(edges[(int)cut.From], edges[(int)cut.To]);
            return Rings(segments).Map(ring => new Arranged(new Key.Ordinal((uint)level), plane.Valued(Crossed(ring[0])), threshold, new Figure.Path(ring.Map(edge => plane.Valued(Crossed(edge)))) { Closed = true }));
        }).Bind(static rings => rings);
    }

    public static Seq<Arranged> Hexbins(PlotPlane plane, Seq<Vector2> points, Factor radius) {
        (float size, float across, float down) = ((float)radius, (float)radius * MathF.Sqrt(3f), (float)radius * 1.5f);
        Seq<Vector2> Corners(Vector2 center) => toSeq(Range(0, 6)).Map(corner => MathF.SinCos((MathF.PI / 6f) + (corner * MathF.PI / 3f)) switch { var (sin, cos) => center + (new Vector2(cos, sin) * size) });
        return toSeq(points.Map(at => (Vector2)plane.Unit(at)).Filter(static at => float.IsFinite(at.X) && float.IsFinite(at.Y)).GroupBy(at => Binned(at, across, down)))
            .Map((bin, index) => new Arranged(new Key.Ordinal((uint)index), plane.Value(bin.Key), bin.Count(), new Figure.Path(Corners(bin.Key).Map(corner => plane.Value(corner))) { Closed = true }));
    }

    public static Figure.Points Dodged(PlotPlane plane, PhysicalAxis along, Seq<float> values, Factor radius) {
        float reach = 2f * (float)radius;
        Seq<float> squared = values.Map(value => along.Of(plane.Unit(along.With(Vector2.Zero, value))));
        HashMap<int, float> offsets = toSeq(squared.Map(static (at, index) => (At: at, Index: index)).OrderBy(static held => held.At)).Fold(HashMap<int, float>(), (placed, held) => {
            Seq<(float Offset, float Clear)> near = toSeq<(int Key, float Value)>(placed).Filter(other => MathF.Abs(squared[other.Key] - held.At) < reach)
                .Map(other => (Offset: other.Value, Clear: MathF.Sqrt((reach * reach) - ((squared[other.Key] - held.At) * (squared[other.Key] - held.At)))));
            return placed.Add(held.Index, toSeq(Seq(0f) + near.Bind(static other => Seq(other.Offset + other.Clear, other.Offset - other.Clear)))
                .Filter(candidate => near.ForAll(other => MathF.Abs(candidate - other.Offset) >= other.Clear - (Slack * reach)))
                .MinBy(static candidate => MathF.Abs(candidate)));
        });
        return new Figure.Points(squared.Map((at, index) => plane.Value(along.With(along.Cross.With(Vector2.Zero, 0.5f + offsets[index]), at))));
    }

    // --- [MESHES]
    public static Seq<Arranged> Triangulated(PlotPlane plane, Seq<Vector2> sites) {
        DelaunayTriangulationBuilder builder = new();
        builder.SetSites(Squared(plane, sites));
        return Cells(plane, builder.GetTriangles(GeometryFactory.Default), static (_, index) => (uint)index);
    }

    public static Seq<Arranged> Voronoi(PlotPlane plane, Seq<Vector2> sites) {
        Coordinate[] squared = Squared(plane, sites);
        FrozenDictionary<Coordinate, int> owners = squared.Select(static (site, index) => (Site: site, Index: index)).DistinctBy(static held => held.Site).ToFrozenDictionary(static held => held.Site, static held => held.Index);
        VoronoiDiagramBuilder builder = new() { ClipEnvelope = new Envelope(0d, 1d, 0d, 1d) };
        builder.SetSites(squared);
        return Cells(plane, builder.GetDiagram(GeometryFactory.Default), (cell, _) => (uint)owners[(Coordinate)cell.UserData]);
    }

    private static Coordinate[] Squared(PlotPlane plane, Seq<Vector2> sites) => [.. sites.Map(plane.Unit).Map(static at => new Coordinate(at.X, at.Y))];

    private static Seq<Arranged> Cells(PlotPlane plane, Geometry cells, Func<Geometry, int, uint> owner) =>
        toSeq(Enumerable.Range(0, cells.NumGeometries).Select(cells.GetGeometryN)).Map((cell, index) => new Arranged(
            new Key.Ordinal(owner(cell, index)), Valued(plane, cell.Centroid.Coordinate), (float)cell.Area,
            new Figure.Path(toSeq(((Polygon)cell).ExteriorRing.Coordinates[..^1]).Map(corner => Valued(plane, corner))) { Closed = true }));

    private static Vector2 Valued(PlotPlane plane, Coordinate at) => plane.Value(new SKPoint((float)at.X, (float)at.Y));

    private static float Total(Seq<float> values) => values.Fold(0f, static (sum, value) => sum + value);

    // --- [STEPS]
    private static readonly Seq<Seq<Cut>> Cuts = Seq(
        Seq<Cut>(), Seq(new Cut(3, 0)), Seq(new Cut(0, 1)), Seq(new Cut(3, 1)), Seq(new Cut(1, 2)), Seq(new Cut(3, 0), new Cut(1, 2)), Seq(new Cut(0, 2)), Seq(new Cut(3, 2)),
        Seq(new Cut(2, 3)), Seq(new Cut(0, 2)), Seq(new Cut(0, 1), new Cut(2, 3)), Seq(new Cut(1, 2)), Seq(new Cut(3, 1)), Seq(new Cut(0, 1)), Seq(new Cut(3, 0)), Seq<Cut>());

    private static readonly HashMap<int, Seq<Cut>> Joined = HashMap((5, Seq(new Cut(3, 2), new Cut(0, 1))), (10, Seq(new Cut(3, 0), new Cut(1, 2))));

    private static Seq<Seq<long>> Rings(Seq<Cut> segments) {
        HashMap<long, Seq<long>> links = segments.Fold(HashMap<long, Seq<long>>(), static (held, cut) => held.AddOrUpdate(cut.From, ends => ends.Add(cut.To), () => Seq(cut.To)).AddOrUpdate(cut.To, ends => ends.Add(cut.From), () => Seq(cut.From)));
        HashSet<long> seen = [];
        Seq<Seq<long>> rings = [];
        foreach (long start in links.Keys) {
            if (seen.Contains(start)) continue;
            Seq<long> ring = [start];
            (long previous, long current) = (start, links[start][0]);
            seen = seen.Add(start);
            while (current != start && !seen.Contains(current)) {
                seen = seen.Add(current);
                ring = ring.Add(current);
                (previous, current) = (current, links[current].Find(next => next != previous).IfNone(start));
            }
            rings = rings.Add(ring);
        }
        return rings;
    }

    private static Vector2 Binned(Vector2 at, float across, float down) {
        float row = at.Y / down;
        int j = (int)MathF.Round(row, MidpointRounding.ToEven);
        float column = (at.X / across) - ((j & 1) / 2f);
        int i = (int)MathF.Round(column, MidpointRounding.ToEven);
        float rowOffset = row - j;
        (float otherI, int otherJ) = (i + ((column < i ? -1f : 1f) / 2f), j + (row < j ? -1 : 1));
        bool farther = MathF.Abs(rowOffset) * 3f > 1f && ((column - i) * (column - i)) + (rowOffset * rowOffset) > ((column - otherI) * (column - otherI)) + ((row - otherJ) * (row - otherJ));
        (float binI, int binJ) = farther ? (otherI + ((otherJ & 1) is 1 ? 0.5f : -0.5f), otherJ) : (i, j);
        return new Vector2((binI + ((binJ & 1) / 2f)) * across, binJ * down);
    }

    private static Seq<Arranged> Tiled(Hierarchy node, Tile tile) =>
        (AxisRange.Validated(tile.Origin.X, tile.Origin.X + tile.Size.X), AxisRange.Validated(tile.Origin.Y, tile.Origin.Y + tile.Size.Y))
            .Apply((x, y) => new Arranged(node.Key, tile.Origin + (tile.Size / 2f), node.Weight, new Figure.Region { First = Some(x), Second = Some(y) })).As().ToOption().ToSeq()
        + Squarified(toSeq(node.Members.Filter(static member => member.Weight > 0f).OrderByDescending(static member => member.Weight)), tile).Bind(static placed => Tiled(placed.Node, placed.Tile));

    private static Seq<Placed> Squarified(Seq<Hierarchy> items, Tile tile) {
        Seq<Placed> Row() {
            float total = Total(items.Map(static item => item.Weight));
            Seq<float> areas = items.Map(item => item.Weight * MathF.Abs(tile.Size.X * tile.Size.Y) / total);
            PhysicalAxis along = MathF.Abs(tile.Size.X) >= MathF.Abs(tile.Size.Y) ? PhysicalAxis.Horizontal : PhysicalAxis.Vertical;
            float side = MathF.Abs(along.Cross.Of(tile.Size));
            int taken = toSeq(Range(1, areas.Count)).TakeWhile(count => count is 1 || Worst(areas.Take(count), side) <= Worst(areas.Take(count - 1), side)).Last.IfNone(1);
            float thickness = MathF.CopySign(Total(areas.Take(taken)) / side, along.Of(tile.Size));
            float stride = MathF.Sign(along.Cross.Of(tile.Size)) / MathF.Abs(thickness);
            Seq<float> starts = areas.Scan(0f, static (sum, area) => sum + area);
            Seq<Placed> row = toSeq(Range(0, taken)).Map(index =>
                new Placed(items[index], new Tile(tile.Origin + along.Cross.With(Vector2.Zero, stride * starts[index]), along.With(along.Cross.With(Vector2.Zero, stride * areas[index]), thickness))));
            return row + Squarified(items.Skip(taken), new Tile(tile.Origin + along.With(Vector2.Zero, thickness), along.With(tile.Size, along.Of(tile.Size) - thickness)));
        }
        return items.IsEmpty ? [] : Row();
    }

    private static float Worst(Seq<float> row, float side) => (Total(row), row.Fold(float.NegativeInfinity, float.Max), row.Fold(float.PositiveInfinity, float.Min)) switch { var (sum, most, least) => float.Max(side * side * most / (sum * sum), sum * sum / (side * side * least)) };

    private static Shape Shaped(Hierarchy node) {
        Shape Joined(Shape left, Shape right) {
            float offset = left.Contour.Zip(right.Contour, static (one, two) => one.High - two.Low + 1f).Fold(0f, float.Max);
            return new Shape(left.Spots + right.Spots.Map(spot => spot.Moved(new Vector2(offset, 0f))),
                toSeq(Range(0, int.Max(left.Contour.Count, right.Contour.Count))).Map(depth => (left.Contour.At(depth), right.Contour.At(depth).Map(held => new FractionRange(held.Low + offset, held.High + offset))) switch {
                    var (one, two) => (one, two).Apply(static (first, second) => new FractionRange(float.Min(first.Low, second.Low), float.Max(first.High, second.High))).As() | one | two,
                }).Somes());
        }
        Shape Grouped(Shape children) {
            Seq<float> roots = children.Spots.Filter(static spot => spot.From.IsNone).Map(static spot => spot.At.X);
            Vector2 shift = new(-(roots.Head.IfNone(0f) + roots.Last.IfNone(0f)) / 2f, 1f);
            return new Shape(
                new Spot(node, Vector2.Zero, None).Cons(children.Spots.Map(spot => spot.Moved(shift)).Map(static spot => spot with { From = spot.From | Some(Vector2.Zero) })),
                new FractionRange(0f, 0f).Cons(children.Contour.Map(depth => new FractionRange(depth.Low + shift.X, depth.High + shift.X))));
        }
        return node.Members.IsEmpty ? new Shape([new Spot(node, Vector2.Zero, None)], [new FractionRange(0f, 0f)]) : Grouped(node.Members.Map(Shaped).Reduce(Joined));
    }

    private static Seq<Vector2> Arc(float from, float to, float radius) =>
        toSeq(Range(0, int.Max(1, (int)MathF.Ceiling(MathF.Abs(to - from) * 360f)) + 1)).Map(step => new Vector2(float.Lerp(from, to, step / MathF.Max(1f, MathF.Ceiling(MathF.Abs(to - from) * 360f))), radius));

    private static Seq<Vector2> Bend(float from, float to, float radius) {
        (Vector2 start, Vector2 end) = (Circled(from, radius), Circled(to, radius));
        int steps = int.Max(2, (int)MathF.Ceiling(Vector2.Distance(start, end) * 360f / MathF.Tau));
        return toSeq(Range(1, steps - 1)).Map(step => (step / (float)steps) switch {
            var along => (((1f - along) * (1f - along) * start) + (along * along * end)) switch { var at => new Vector2(((MathF.Atan2(at.Y, at.X) / MathF.Tau % 1f) + 1f) % 1f, at.Length()) },
        });
    }

    private static Vector2 Circled(float turn, float radius) => MathF.SinCos(MathF.Tau * turn) switch { var (sin, cos) => new Vector2(cos, sin) * radius };

    private sealed record Tile(Vector2 Origin, Vector2 Size);

    private sealed record Placed(Hierarchy Node, Tile Tile);

    private sealed record Spot(Hierarchy Node, Vector2 At, Option<Vector2> From) {
        public Spot Moved(Vector2 by) => this with { At = At + by, From = From.Map(from => from + by) };
    }

    private sealed record Shape(Seq<Spot> Spots, Seq<FractionRange> Contour);

    private sealed record Cut(long From, long To);
}
