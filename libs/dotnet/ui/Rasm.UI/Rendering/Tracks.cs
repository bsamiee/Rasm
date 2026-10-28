using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class Layouts {
    // --- [TRACKS]
    private sealed record Lattice(PhysicalAxis Axis, int Count, Option<float> Line, Seq<Sizing> Tracks, Seq<(int Line, int Track, int Down, int Across, LayoutNode Layout)> Cells) {
        internal static Lattice Of(LayoutNode layout, Node.Grid grid) {
            int longest = grid.Rows.Map(static row => row.Count).Fold(0, int.Max);
            Seq<Sizing> columns = Sizes(grid.Columns, PerLine(layout, grid.Columns, PhysicalAxis.Horizontal, longest, longest));
            int count = int.Max(1, columns.Count);
            Seq<(int Line, int Track, int Down, int Across, LayoutNode Layout)> cells = grid.Rows.Zip(grid.Rows.Scan(0, static (start, cells) => start + cells.Count)).Fold(
                (Taken: HashSet<(int Line, int Track)>(), Cursor: (Line: 0, Track: 0), Cells: Seq<(int Line, int Track, int Down, int Across, LayoutNode Layout)>()),
                (held, row) => toSeq(Enumerable.Range(row.Second, row.First.Count)).Fold(held, (state, member) => {
                    LayoutNode cell = layout.Members[member];
                    (int down, int across) = cell.Node.Spanning.Map(spanning => (int.Max(1, spanning.Rows), int.Clamp(spanning.Columns, 1, count))).IfNone((1, 1));
                    (int line, int track) = (from candidate in Enumerable.Range(state.Cursor.Line, int.MaxValue - state.Cursor.Line)
                                             let first = candidate == state.Cursor.Line ? state.Cursor.Track : 0
                                             from column in Enumerable.Range(first, int.Max(0, count - across + 1 - first))
                                             where !Covered(candidate, column, down, across).Exists(state.Taken.Contains)
                                             select (candidate, column)).First();
                    int wide = member == row.Second + row.First.Count - 1 && cell.Node.Spanning.IsNone
                        ? Enumerable.Range(track, count - track).TakeWhile(column => !state.Taken.Contains((line, column))).Count()
                        : across;
                    return (state.Taken.Union(Covered(line, track, down, wide)), (line, track + wide), state.Cells.Add((line, track, down, wide, cell)));
                }) switch {
                    var placed => placed with { Cursor = placed.Cursor.Track > 0 ? (placed.Cursor.Line + 1, 0) : placed.Cursor },
                }).Cells;
            return new(PhysicalAxis.Vertical, cells.Map(static cell => cell.Line + cell.Down).Fold(0, int.Max), None, columns, cells);
        }

        internal static Lattice Of(LayoutNode layout, Node.Lazy lazy) {
            int perLine = PerLine(layout, lazy);
            Seq<(int Line, int Track, int Down, int Across, LayoutNode Layout)> cells = layout.Members
                .Zip(lazy.Items.Map(static (key, index) => (Key: key, Index: index)).Filter(held => lazy.Realized.ContainsKey(held.Key)))
                .Map(held => (Line: held.Second.Index / perLine, Track: held.Second.Index % perLine, Down: 1, Across: 1, Layout: held.First));
            return new(lazy.Axis, Lines(lazy, perLine), Some(LineExtent(layout, lazy)), Sizes(lazy.Across, perLine), cells);
        }

        internal Seq<(float Start, float Extent)> Tracked(LayoutNode layout, PhysicalAxis along, float space) {
            if (along != Axis) {
                HashMap<int, float> widths = toHashMap(Cells.Filter(static cell => cell.Across is 1).GroupBy(static cell => cell.Track).Select(track => (track.Key, Widest(toSeq(track).Map(static cell => cell.Layout), along))));
                return Started(layout, Sizing.Shared(Tracks, Tracks.Map((_, track) => widths.Find(track).IfNone(0f)), space - (Gap(layout) * float.Max(0, Tracks.Count - 1))));
            }
            HashMap<int, float> tallest = toHashMap(Cells.Filter(static cell => cell.Down is 1).GroupBy(static cell => cell.Line).Select(line => (line.Key, Widest(toSeq(line).Map(static cell => cell.Layout), along))));
            float gap = Gap(layout);
            return Started(layout, Cells.Filter(static cell => cell.Down > 1).Fold(toSeq(Range(0, Count)).Map(line => Line.IfNone(() => tallest.Find(line).IfNone(0f))).Strict(), (lines, cell) =>
                (Basis(cell.Layout, along, float.PositiveInfinity) - Summed(lines.Skip(cell.Line).Take(cell.Down), gap)) switch {
                    var deficit when deficit > 0f => lines.Map((extent, line) => line == cell.Line + cell.Down - 1 ? extent + deficit : extent).Strict(),
                    _ => lines,
                }));
        }

        internal float Extent(LayoutNode layout, PhysicalAxis along) {
            Seq<(float Start, float Extent)> tracks = Tracked(layout, along, float.PositiveInfinity);
            float extent = Summed(tracks.Map(static track => track.Extent), Gap(layout));
            return along == Axis ? extent : Cells.Filter(static cell => cell.Across > 1).Fold(extent, (widest, cell) => float.Max(widest, tracks[cell.Track].Start + Basis(cell.Layout, along, float.PositiveInfinity)));
        }

        internal Seq<LayoutNode> Arranged(AvailableSpace available) {
            Seq<(float Start, float Extent)> tracks = Tracked(available.Layout, available.Along, available.Space);
            bool lines = available.Along == Axis;
            return Cells.Map(cell => (First: lines ? cell.Line : cell.Track, Count: lines ? cell.Down : cell.Across) switch {
                var span => (tracks[span.First], tracks[span.First + span.Count - 1]) switch {
                    var (head, last) => Aligned(available, cell.Layout, available.Start + head.Start,
                        !lines && span.Count > 1 && span.First + span.Count == tracks.Count ? available.Space - head.Start : last.Start + last.Extent - head.Start,
                        cell.Layout.Node.Align.Of(available.Along)),
                },
            });
        }

        private static Seq<(int Line, int Track)> Covered(int line, int track, int down, int across) =>
            toSeq(from row in Enumerable.Range(line, down) from column in Enumerable.Range(track, across) select (row, column));
    }

    private static Seq<Sizing> Sizes(Tracks tracks, int count) =>
        toSeq(Range(0, count)).Map(index => tracks.Sizes.At(tracks.AutoFill ? index % int.Max(1, tracks.Sizes.Count) : index).IfNone(Sizing.Fit.Content));

    private static int PerLine(LayoutNode layout, Tracks tracks, PhysicalAxis across, int most, int least) {
        if (!tracks.AutoFill) return int.Max(least, tracks.Sizes.Count);
        int period = int.Max(1, tracks.Sizes.Count);
        (float space, float gap, float widest) = (across.Of(layout.ContentBox.Size), Gap(layout), Widest(layout.Members, across));
        float pattern = Sizes(tracks, period).Fold(0f, (sum, size) => sum + Based(size, widest, space) + gap);
        return period * (int)float.Clamp(MathF.Floor((space + gap) / float.Max(pattern, float.Epsilon)), 1f, float.Max(1f, MathF.Ceiling(most / (float)period)));
    }

    private static int PerLine(LayoutNode layout, Node.Lazy lazy) => PerLine(layout, lazy.Across, lazy.Axis.Cross, lazy.Items.Count, 1);

    private static Seq<(float Start, float Extent)> Started(LayoutNode layout, Seq<float> extents) =>
        Gap(layout) switch {
            var gap => extents.Zip(extents.Scan(0f, (start, extent) => start + extent + gap)).Map(static track => (Start: track.Second, Extent: track.First)),
        };

    // --- [WINDOW]
    internal static Seq<Key> Visible(Scene scene, LayoutNode lazy, Node.Lazy node) =>
        (from frame in scene.Frame(lazy.Path)
         from shown in scene.Visible(lazy.Path)
         let perLine = PerLine(lazy, node)
         let lines = Window(node.Axis, lazy, frame, shown)
         let across = Window(node.Axis.Cross, lazy, frame, shown)
         let first = Line(lazy, node, perLine, lines.From)
         let tracks = Definite(lazy, node, perLine)
             .Map(definite => definite.Map(static (track, index) => (Track: track, Index: index)).Filter(held => held.Track.Start + held.Track.Extent >= across.From && held.Track.Start <= across.To).Map(static held => held.Index))
             .IfNone(() => toSeq(Range(0, perLine)))
         select toSeq(from line in toSeq(Range(first, Line(lazy, node, perLine, lines.To) - first + 1)) from track in tracks select (line * perLine) + track).Choose(index => node.Items.At(index))).IfNone([]);

    internal static Option<SKRect> Slot(LayoutNode lazy, Node.Lazy node, Key key) => Selection.Position(node.Items, key).Map(index => Slotted(lazy, node, index));

    private static SKRect Slotted(LayoutNode lazy, Node.Lazy node, int index) {
        int perLine = PerLine(lazy, node);
        (int line, int track) = int.DivRem(index, perLine);
        SKRect box = lazy.ContentBox;
        float extent = LineExtent(lazy, node);
        SKRect span = node.Axis.Spanned(box, node.Axis.Of(box.Location) + (line * (extent + Gap(lazy))), extent);
        return Definite(lazy, node, perLine).Map(tracks => node.Axis.Cross.Spanned(span, node.Axis.Cross.Of(box.Location) + tracks[track].Start, tracks[track].Extent)).IfNone(span);
    }

    private static (float From, float To) Window(PhysicalAxis axis, LayoutNode lazy, SKRect frame, SKRect shown) =>
        (Offset: axis.Of(shown.Location - frame.Location) - axis.Of(lazy.ContentBox.Location), Viewport: axis.Of(shown.Size)) switch {
            var held => (held.Offset - held.Viewport, held.Offset + (2f * held.Viewport)),
        };

    private static Option<Seq<(float Start, float Extent)>> Definite(LayoutNode lazy, Node.Lazy node, int perLine) =>
        Sizes(node.Across, perLine).Traverse(static size => size.Definite).As().Map(extents => Started(lazy, extents));

    private static int Line(LayoutNode lazy, Node.Lazy node, int perLine, float at) =>
        (int)float.Clamp(MathF.Floor(at / (LineExtent(lazy, node) + Gap(lazy))), 0f, float.Max(0f, Lines(node, perLine) - 1));

    private static int Lines(Node.Lazy lazy, int perLine) => (lazy.Items.Count + perLine - 1) / perLine;

    private static float LineExtent(LayoutNode lazy, Node.Lazy node) =>
        node.Extent.IfNone(() => Styles.Resolve(lazy.Inner, node.Rows, lazy.Style.Conditions).Metrics[Metric.Extent]);
}
