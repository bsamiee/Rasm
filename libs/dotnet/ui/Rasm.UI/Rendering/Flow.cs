using System.Globalization;
using System.Numerics.Tensors;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Layouts {
    // --- [ARRANGE]
    internal sealed record Pass(HashMap<Seq<Key>, LayoutNode> Prior, HashMap<Seq<Key>, float> Offsets, Shaping Shaping, Slot<OutlineKey, Outline> Outlines, CultureInfo Culture);

    internal static LayoutNode Arrange(Pass pass, Cascade root, Node node, SKSize size) => WithCorners(Framed(pass, Built(pass, root, [], node, pass.Prior.Find([])), SKRect.Create(size)), None);

    internal static LayoutNode Framed(Pass pass, LayoutNode layout, SKRect frame) =>
        Placed(pass, Placed(pass, layout, PhysicalAxis.Horizontal, frame.Left, frame.Width), PhysicalAxis.Vertical, frame.Top, frame.Height);

    internal static Fin<LayoutNode> Unique(LayoutNode layout, HashMap<Seq<Key>, LayoutNode> prior) =>
        Repeated(layout, prior) switch {
            var repeated when repeated.IsEmpty => layout,
            var repeated => Error.Many(repeated.Map(static path => (Error)new UIFault(string.Join('/', path.Map(static key => key.Identifier)), UIReason.Repeated))),
        };

    internal static SKSize Preferred(Pass pass, LayoutNode layout, SKSize offered) {
        LayoutNode wide = Placed(pass, layout, PhysicalAxis.Horizontal, 0f, float.Min(Basis(layout, PhysicalAxis.Horizontal, offered.Width), offered.Width));
        return new SKSize(wide.Frame.Width, Basis(wide, PhysicalAxis.Vertical, offered.Height));
    }

    private static bool Reused(LayoutNode layout, HashMap<Seq<Key>, LayoutNode> prior) => prior.Find(layout.Path).Exists(held => ReferenceEquals(held, layout));

    private static Seq<Seq<Key>> Repeated(LayoutNode layout, HashMap<Seq<Key>, LayoutNode> prior) =>
        Reused(layout, prior)
            ? []
            : (layout.Children + layout.Layers).Fold((Seen: HashSet<Seq<Key>>(), Repeated: Seq<Seq<Key>>()), static (held, member) => held.Seen.Contains(member.Path)
                    ? (held.Seen, held.Repeated.Add(member.Path))
                    : (held.Seen.Add(member.Path), held.Repeated))
                .Repeated
                .Concat((layout.Members + layout.Layers).Bind(member => Repeated(member, prior)));

    // --- [QUERY]
    internal sealed record Overflow(float Content, float Viewport) {
        internal float Max => float.Max(0f, Content - Viewport);

        internal float Clamped(float offset) => float.Clamp(offset, 0f, Max);
    }

    internal static Option<PlotFrame> Plotted(LayoutNode layout) => layout.Node.Plotted.Map(plane => plane.In(layout.Data));

    internal static bool IsHandle(LayoutNode layout) => layout.Path.Last.Exists(static last => last is Key.Handle);

    internal static Seq<Key> Stored(LayoutNode scroll, Node.Scroll node) => node.Follows.IfNone(scroll.Path);

    internal static Option<Overflow> Scrollable(LayoutNode scroll, Node.Scroll node) =>
        Panes(scroll).Head.Map(content => new Overflow(node.Axis.Of(content.Frame.Size), node.Axis.Of(scroll.ContentBox.Size)));

    internal static float Rescaled(float value, float span, float onto) => span > 0f ? value * onto / span : 0f;

    internal static Seq<LayoutNode> Panes(LayoutNode owner) => owner.Children.Filter(static child => !IsHandle(child));

    internal static Sizing Sized(Node node, PhysicalAxis along) => (along.Map(horizontal: node.Width, vertical: node.Height) | node.Inner.Map(inner => Sized(inner, along))).IfNone(Sizing.Fit.Content);

    private static Option<Factor> Aspect(Node node) =>
        node.Aspect | node.Plotted.Bind(static plane => plane.Aspect)
        | (node is Node.Image image ? Factor.Validated(image.Source.Info.Width / (float)image.Source.Info.Height).ToOption() : None);

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
        SKRect span = node.Axis.Spanned(box, node.Axis.Of(box.Location) + (line * (node.Extent + Gap(lazy))), node.Extent);
        return Definite(lazy, node, perLine).Map(tracks => node.Axis.Cross.Spanned(span, node.Axis.Cross.Of(box.Location) + tracks[track].Start, tracks[track].Extent)).IfNone(span);
    }

    private static (float From, float To) Window(PhysicalAxis axis, LayoutNode lazy, SKRect frame, SKRect shown) =>
        (Offset: axis.Of(shown.Location - frame.Location) - axis.Of(lazy.ContentBox.Location), Viewport: axis.Of(shown.Size)) switch {
            var held => (held.Offset - held.Viewport, held.Offset + (2f * held.Viewport)),
        };

    private static Option<Seq<(float Start, float Extent)>> Definite(LayoutNode lazy, Node.Lazy node, int perLine) =>
        Sizes(node.Across, perLine).Traverse(static size => size.Definite).As().Map(extents => Started(lazy, extents));

    private static int Line(LayoutNode lazy, Node.Lazy node, int perLine, float at) =>
        (int)float.Clamp(MathF.Floor(at / (node.Extent + Gap(lazy))), 0f, float.Max(0f, Lines(node, perLine) - 1));

    private static int Lines(Node.Lazy lazy, int perLine) => (lazy.Items.Count + perLine - 1) / perLine;

    // --- [BUILD]
    internal static LayoutNode Built(Pass pass, Cascade cascade, Seq<Key> path, Node node, Option<LayoutNode> held) =>
        held.Filter(kept => kept.Node.Equals(node) && kept.Cascade.Equals(cascade)).IfNone(() => Created(pass, cascade, path, node, held));

    internal static Option<LayoutNode> Held(Pass pass, Seq<LayoutNode> earlier, Seq<Key> path) => pass.Prior.Find(path) | earlier.Find(member => member.Path == path);

    private static LayoutNode Created(Pass pass, Cascade cascade, Seq<Key> path, Node node, Option<LayoutNode> prior) {
        Cascade inner = Styles.Enter(cascade, node);
        Seq<LayoutNode> earlier = prior.Map(static held => held.Members + held.Layers).IfNone([]);
        Seq<LayoutNode> members = Members(node).Map(member => path.Add(member.Segment) switch {
            var at => Built(pass, inner, at, member.Node, Held(pass, earlier, at)),
        }).Strict();
        LayoutNode.Box box = new() { Path = path, Node = node, Cascade = cascade, Inner = inner, Style = inner.Enclosing, Members = members.Filter(static member => member.Node is not Node.Layer) };
        LayoutNode built = node.SwitchPartially(
            (Pass: pass, Box: box with { Children = box.Members }),
            @default: static (held, _) => held.Box,
            choice: static (held, _) => held.Box with { Children = held.Box.Members.Take(1) },
            mark: static (held, mark) => mark.Figure is Figure.Label label ? TextNode(held.Pass, held.Box, new Node.Text(label.Spans)) : held.Box,
            text: static (held, text) => (LayoutNode)TextNode(held.Pass, held.Box, text)) with {
            Layers = members.Filter(static member => member.Node is Node.Layer),
        };
        float width = IntrinsicWidth(built);
        return built with { Intrinsic = new SKSize(width, IntrinsicHeight(built, width)) };
    }

    private static LayoutNode.Text TextNode(Pass pass, LayoutNode box, Node.Text text) {
        Typeset set = Typesetting.Set(pass.Shaping, text, box.Inner, Typesetting.Resolve(pass.Shaping, box.Inner, box.Style), float.PositiveInfinity, pass.Culture);
        return new LayoutNode.Text(set) { Path = box.Path, Node = box.Node, Cascade = box.Cascade, Inner = box.Inner, Style = box.Style, MinContent = Typesetting.MinContent(pass.Shaping, set, pass.Culture) };
    }

    private static Seq<(Key Segment, Node Node)> Members(Node node) =>
        node.SwitchPartially(
            @default: static held => Segments(held.Inner.ToSeq()),
            flex: static flex => Segments(flex.Children),
            grid: static grid => Segments(grid.Rows.Flatten()),
            lazy: static lazy => lazy.Items.Choose(key => lazy.Realized.Find(key).Map(realized => (Segment: key, Node: realized))),
            overlay: static overlay => Segments(overlay.Base.Cons(overlay.Above)),
            layer: static layer => (layer.Modal ? Seq<(Key Segment, Node Node)>((LayerStack.Scrim, new Node.Box { Role = Role.Scrim })) : []) + Segments([layer.Content]),
            choice: static choice => choice.Alternatives.Map(static alternative => (Segment: alternative.Key.IfNone(static () => new Key.Ordinal(0)), Node: alternative)),
            scroll: static scroll => Seq<(Key Segment, Node Node)>(
                (scroll.Content.Key.IfNone(static () => new Key.Ordinal(0)), scroll.Content),
                (new Key.Handle(), new Node.Box { Key = new Key.Handle(), Role = Role.Scrollbar })),
            plot: static plot => Segments(plot.Children));

    private static Seq<(Key Segment, Node Node)> Segments(Seq<Node> nodes) => nodes.Map(static (node, index) => (Segment: node.Key.IfNone(() => new Key.Ordinal((uint)index)), Node: node));

    // --- [FIT]
    private static float IntrinsicWidth(LayoutNode layout) =>
        (from aspect in Aspect(layout.Node)
         from height in Sized(layout.Node, PhysicalAxis.Vertical).Definite
         select height * (float)aspect).IfNone(() => Boxed(layout, PhysicalAxis.Horizontal));

    private static float IntrinsicHeight(LayoutNode layout, float width) => Aspect(layout.Node).Map(aspect => width / (float)aspect).IfNone(() => Boxed(layout, PhysicalAxis.Vertical));

    private static float Boxed(LayoutNode layout, PhysicalAxis along) => float.Max(Content(layout, along) + (2f * layout.Inset), layout.Node.Role.IsSome ? layout.Style.Metrics[Metric.Extent] : 0f);

    private static float Content(LayoutNode layout, PhysicalAxis along) =>
        layout.Switch(along, box: static (held, box) => Contained(box, held), text: static (held, text) => MathF.Ceiling(held.Of(text.Typeset.Size)));

    private static float Contained(LayoutNode.Box layout, PhysicalAxis along) =>
        layout.Node.SwitchPartially(
            (Layout: layout, Along: along),
            @default: static (held, _) => Widest(held.Layout.Children, held.Along),
            flex: static (held, flex) => flex.Axis == held.Along
                ? Summed(held.Layout.Children.Map(child => Basis(child, held.Along, float.PositiveInfinity)), Gap(held.Layout))
                : Widest(held.Layout.Children, held.Along),
            grid: static (held, grid) => Lattice.Of(held.Layout, grid).Extent(held.Layout, held.Along),
            lazy: static (held, lazy) => Lattice.Of(held.Layout, lazy).Extent(held.Layout, held.Along),
            overlay: static (held, _) => Widest(held.Layout.Children.Take(1), held.Along),
            choice: static (held, choice) => Widest((held.Along == choice.Axis ? held.Layout.Members : held.Along.Map(horizontal: held.Layout.Members, vertical: held.Layout.Children)).Take(1), held.Along),
            scroll: static (held, _) => Widest(Panes(held.Layout), held.Along),
            plot: static (held, _) => Widest(held.Layout.Children.Filter(static child => child.Node.Pinned.IsNone), held.Along),
            glyph: static (held, _) => held.Layout.Style.Metrics[Metric.Icon],
            image: static (held, image) => held.Along.Of(image.Source.Info.Size) / (float)image.Density);

    private static float Least(LayoutNode layout) =>
        Sized(layout.Node, PhysicalAxis.Horizontal).Definite.IfNone(() => float.Max(layout.Node.Role.IsSome ? layout.Style.Metrics[Metric.Extent] : 0f, (2f * layout.Inset) + layout.Switch(
            box: static box => box.Node.SwitchPartially(
                box,
                @default: static (held, _) => Floors(held.Children).Fold(0f, float.Max),
                flex: static (held, flex) => flex.Axis == PhysicalAxis.Horizontal ? Summed(Floors(held.Children), Gap(held)) : Floors(held.Children).Fold(0f, float.Max),
                grid: static (_, _) => 0f,
                lazy: static (_, _) => 0f,
                overlay: static (held, _) => Floors(held.Children.Take(1)).Fold(0f, float.Max),
                choice: static (held, _) => Floors(held.Members.Last.ToSeq()).Fold(0f, float.Max),
                scroll: static (_, _) => 0f,
                plot: static (_, _) => 0f,
                glyph: static (held, _) => held.Style.Metrics[Metric.Icon],
                image: static (_, _) => 0f),
            text: static text => text.MinContent)));

    private static Seq<float> Floors(Seq<LayoutNode> children) => children.Map(static child => Sized(child.Node, PhysicalAxis.Horizontal).Floor(Least(child)));

    private static float Summed(Seq<float> extents, float gap) => TensorPrimitives.Sum<float>([.. extents]) + (gap * float.Max(0, extents.Count - 1));

    private static float Widest(Seq<LayoutNode> children, PhysicalAxis along) => children.Map(child => Basis(child, along, float.PositiveInfinity)).Fold(0f, float.Max);

    private static float Basis(LayoutNode layout, PhysicalAxis along, float space) => Based(Sized(layout.Node, along), along.Of(layout.Intrinsic), space);

    internal static float Based(Sizing sizing, float fit, float space) =>
        sizing.Limits.Clamped(sizing.Switch(
            (Fit: fit, Space: space),
            @fixed: static (_, held) => held.Extent,
            fit: static (state, _) => state.Fit,
            grow: static (state, _) => state.Fit,
            percent: static (state, held) => float.IsFinite(state.Space) ? (float)held.Fraction * state.Space : state.Fit));

    internal static float Cross(LayoutNode layout, PhysicalAxis along, float space) {
        Sizing sizing = Sized(layout.Node, along);
        return sizing.Limits.Clamped(sizing.Stretches ? space : float.Min(Basis(layout, along, space), space));
    }

    private static float Gap(LayoutNode layout) => layout.Style.Metrics[Metric.Gap];

    // --- [PLACE]
    internal sealed record AvailableSpace(Pass Pass, LayoutNode Layout, PhysicalAxis Along, float Start, float Space);

    internal static LayoutNode Placed(Pass pass, LayoutNode layout, PhysicalAxis along, float start, float extent) {
        if (Reused(layout, pass.Prior) && along.Of(layout.Frame.Size).CompareTo(extent) is 0) return Moved(layout, along, start);
        LayoutNode framed = layout with { Frame = along.Spanned(layout.Frame, start, extent) };
        SKRect box = framed.ContentBox;
        AvailableSpace available = new(pass, framed, along, along.Of(box.Location), along.Of(box.Size));
        return along.Switch(
            (Room: available, Layout: Arranged(available)),
            horizontal: static held => Wrapped(held.Room, held.Layout) switch {
                var set => set with { Intrinsic = new SKSize(set.Intrinsic.Width, IntrinsicHeight(set, set.Frame.Width)) },
            },
            vertical: static held => held.Layout.Switch(
                box: static box => (LayoutNode)box,
                text: static text => text.ContentBox switch {
                    var box => text with { Origin = new SKPoint(box.Left, box.Top + (float.Max(0f, box.Height - text.Typeset.Size.Height) * (float)text.Typeset.Text.Align.Y)) },
                }));
    }

    private static LayoutNode Wrapped(AvailableSpace available, LayoutNode layout) =>
        layout.Switch(
            available,
            box: static (_, box) => (LayoutNode)box,
            text: static (available, wrapped) => wrapped.Typeset.Width.CompareTo(available.Space) is 0
                ? wrapped
                : wrapped with {
                    Typeset = available.Pass.Prior.Find(wrapped.Path)
                        .Bind(static held => held.TextNode)
                        .Map(static held => held.Typeset)
                        .Filter(held => held.Width.CompareTo(available.Space) is 0 && held.Text.Equals(wrapped.Typeset.Text) && held.Style.Equals(wrapped.Typeset.Style))
                        .IfNone(() => Typesetting.Set(available.Pass.Shaping, wrapped.Typeset.Text, wrapped.Inner, wrapped.Typeset.Style, available.Space, available.Pass.Culture)),
                });

    private static Option<float> Baseline(LayoutNode layout, Baseline baseline) =>
        layout.Switch(
            baseline,
            box: static (line, box) => box.Children.Choose(child => Baseline(child, line).Map(at => child.Frame.Top + at)) switch {
                var lines => line.Line.IsFromEnd ? lines.Last : lines.Head,
            },
            text: static (line, text) => text.Typeset.At(line).Map(at => text.Origin.Y + at));

    private static LayoutNode Moved(LayoutNode layout, PhysicalAxis along, float start) =>
        along.Of(layout.Frame.Location).CompareTo(start) is 0
            ? layout
            : layout with { Frame = along.Spanned(layout.Frame, start, along.Of(layout.Frame.Size)) };

    private static LayoutNode Shown(LayoutNode layout, Seq<LayoutNode> placed) => layout with { Members = placed, Children = placed };

    internal static LayoutNode Filled(AvailableSpace available, LayoutNode child) => Placed(available.Pass, child, available.Along, available.Start, available.Space);

    private static LayoutNode Layered(AvailableSpace available, LayoutNode child) => Aligned(available, child, available.Start, available.Space, child.Node.Align.Of(available.Along));

    private static LayoutNode Aligned(AvailableSpace available, LayoutNode child, float start, float space, float align) {
        float extent = Cross(child, available.Along, space);
        return Placed(available.Pass, child, available.Along, start + ((space - extent) * align), extent);
    }

    private static LayoutNode Arranged(AvailableSpace available) =>
        available.Layout.Node.SwitchPartially(
            available,
            @default: static (held, _) => Shown(held.Layout, held.Layout.Members.Map(member => Filled(held, member))),
            flex: static (held, flex) => Shown(held.Layout, Guides.Shared(held, flex.Axis == held.Along ? Along(held) : Across(held, flex.Baseline), flex.Axis.Cross, static _ => 0)),
            grid: static (held, grid) => Lattice.Of(held.Layout, grid) switch {
                var lattice => Shown(held.Layout, Guides.Shared(held, Guides.Shared(held, lattice.Arranged(held), PhysicalAxis.Vertical, index => lattice.Cells[index].Line), PhysicalAxis.Horizontal, index => lattice.Cells[index].Track)),
            },
            lazy: static (held, lazy) => Shown(held.Layout, Lattice.Of(held.Layout, lazy).Arranged(held)),
            overlay: static (held, _) => Shown(held.Layout, Overlaid(held)),
            choice: static (held, choice) => Chosen(held, choice),
            scroll: static (held, scroll) => Scrolled(held, scroll),
            plot: static (held, plot) => Guides.Plot(held, plot, Gutter.Empty));

    private static Seq<LayoutNode> Along(AvailableSpace available) {
        Seq<LayoutNode> children = available.Layout.Members;
        float gap = Gap(available.Layout);
        float space = available.Space - (gap * float.Max(0, children.Count - 1));
        Seq<float> extents = Distribute(children.Map(child => Sized(child.Node, available.Along) switch {
            var sizing => (Sizing: sizing, Basis: Basis(child, available.Along, space), Floor: sizing.Floor(available.Along.Map(horizontal: Least(child), vertical: 0f))),
        }), space);
        return children.Zip(extents.Zip(extents.Scan(available.Start, (start, extent) => start + extent + gap)))
            .Map(placed => Placed(available.Pass, placed.First, available.Along, placed.Second.Second, placed.Second.First));
    }

    private static Seq<LayoutNode> Across(AvailableSpace available, Option<Baseline> baseline) {
        Seq<LayoutNode> layered = available.Layout.Members.Map(member => Layered(available, member));
        return baseline.Filter(_ => available.Along == PhysicalAxis.Vertical).Map(line => {
            Seq<(LayoutNode Child, Option<float> Line)> lines = layered.Map(child => (Child: child, Line: Baseline(child, line))).Strict();
            float most = lines.Choose(static held => held.Line).Fold(0f, float.Max);
            return lines.Map(held => held.Line.Match(Some: at => Moved(held.Child, available.Along, available.Start + most - at), None: () => held.Child));
        }).IfNone(layered);
    }

    private static LayoutNode Chosen(AvailableSpace available, Node.Choice choice) =>
        available.Along == choice.Axis
            ? (available.Layout.Members.Find(alternative => Basis(alternative, available.Along, float.PositiveInfinity) <= available.Space) | available.Layout.Members.Last)
                .Map(chosen => available.Layout with { Children = [Filled(available, chosen)] })
                .IfNone(available.Layout)
            : available.Along.Switch(
                available,
                horizontal: static held => Shown(held.Layout, held.Layout.Members.Map(member => Filled(held, member))),
                vertical: static held => held.Layout with { Children = held.Layout.Children.Map(child => Filled(held, child)) });

    private static Seq<LayoutNode> Overlaid(AvailableSpace available) =>
        available.Layout.Members.Match(
            Empty: static () => Seq<LayoutNode>(),
            Tail: (baseNode, above) => available.Layout.ContentBox switch {
                var box => SKMatrix.CreateScaleTranslation(box.Width, box.Height, box.Left, box.Top) switch {
                    var placed => Filled(available, baseNode).Cons(Guides.Pins(available, box, above, at => placed.MapPoint(at), Layered)),
                },
            });

    // --- [TRACKS]
    private sealed record Lattice(PhysicalAxis Axis, int Count, Option<float> Line, Seq<Sizing> Tracks, Seq<(int Line, int Track, bool Spans, LayoutNode Layout)> Cells) {
        internal static Lattice Of(LayoutNode layout, Node.Grid grid) {
            int longest = grid.Rows.Map(static row => row.Count).Fold(0, int.Max);
            Seq<Sizing> columns = Sizes(grid.Columns, PerLine(layout, grid.Columns, PhysicalAxis.Horizontal, longest, longest));
            Seq<(int Line, int Track, bool Spans, LayoutNode Layout)> cells =
                from row in grid.Rows.Zip(grid.Rows.Scan(0, static (start, cells) => start + cells.Count))
                    .Bind(row => toSeq(Enumerable.Range(row.Second, row.First.Count).Chunk(columns.Count)))
                    .Map(static (members, index) => (Members: members, Index: index))
                from cell in toSeq(row.Members).Map(static (member, column) => (Member: member, Column: column))
                select (Line: row.Index, Track: cell.Column, Spans: cell.Column == row.Members.Length - 1 && row.Members.Length < columns.Count, Layout: layout.Members[cell.Member]);
            return new(PhysicalAxis.Vertical, cells.Map(static cell => cell.Line + 1).Fold(0, int.Max), None, columns, cells);
        }

        internal static Lattice Of(LayoutNode layout, Node.Lazy lazy) {
            int perLine = PerLine(layout, lazy);
            Seq<(int Line, int Track, bool Spans, LayoutNode Layout)> cells = layout.Members
                .Zip(lazy.Items.Map(static (key, index) => (Key: key, Index: index)).Filter(held => lazy.Realized.ContainsKey(held.Key)))
                .Map(held => (Line: held.Second.Index / perLine, Track: held.Second.Index % perLine, Spans: false, Layout: held.First));
            return new(lazy.Axis, Lines(lazy, perLine), Some(lazy.Extent), Sizes(lazy.Across, perLine), cells);
        }

        internal Seq<(float Start, float Extent)> Tracked(LayoutNode layout, PhysicalAxis along, float space) {
            if (along != Axis) {
                HashMap<int, float> widths = toHashMap(Cells.Filter(static cell => !cell.Spans).GroupBy(static cell => cell.Track).Select(track => (track.Key, Widest(toSeq(track).Map(static cell => cell.Layout), along))));
                return Started(layout, Sizing.Shared(Tracks, Tracks.Map((_, track) => widths.Find(track).IfNone(0f)), space - (Gap(layout) * float.Max(0, Tracks.Count - 1))));
            }
            HashMap<int, float> tallest = toHashMap(Cells.GroupBy(static cell => cell.Line).Select(line => (line.Key, Widest(toSeq(line).Map(static cell => cell.Layout), along))));
            return Started(layout, toSeq(Range(0, Count)).Map(line => Line.IfNone(() => tallest.Find(line).IfNone(0f))));
        }

        internal float Extent(LayoutNode layout, PhysicalAxis along) {
            Seq<(float Start, float Extent)> tracks = Tracked(layout, along, float.PositiveInfinity);
            float extent = Summed(tracks.Map(static track => track.Extent), Gap(layout));
            return along == Axis ? extent : Cells.Filter(static cell => cell.Spans).Fold(extent, (widest, cell) => float.Max(widest, tracks[cell.Track].Start + Basis(cell.Layout, along, float.PositiveInfinity)));
        }

        internal Seq<LayoutNode> Arranged(AvailableSpace available) {
            Seq<(float Start, float Extent)> tracks = Tracked(available.Layout, available.Along, available.Space);
            bool lines = available.Along == Axis;
            return Cells.Map(cell => tracks[lines ? cell.Line : cell.Track] switch {
                var track => Aligned(available, cell.Layout, available.Start + track.Start, !lines && cell.Spans ? available.Space - track.Start : track.Extent, cell.Layout.Node.Align.Of(available.Along)),
            });
        }
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

    // --- [SCROLL]
    private static LayoutNode Scrolled(AvailableSpace available, Node.Scroll scroll) {
        Seq<LayoutNode> panes = available.Layout.Members.Filter(static member => !IsHandle(member)).Map(content => scroll.Axis == available.Along
            ? Placed(available.Pass, content, available.Along, available.Start, float.Max(Basis(content, available.Along, float.PositiveInfinity), available.Space))
            : Filled(available, content));
        Overflow overflow = new(panes.Map(pane => scroll.Axis.Of(pane.Frame.Size)).Fold(0f, float.Max), scroll.Axis.Of(available.Layout.ContentBox.Size));
        Seq<LayoutNode> thumbs = available.Layout.Members.Filter(IsHandle).Map(thumb => Thumb(available, scroll, thumb, overflow));
        bool shown = available.Along.Map(horizontal: scroll.Axis.Map(horizontal: overflow.Max > 0f, vertical: true), vertical: overflow.Max > 0f);
        return available.Layout with { Members = panes + thumbs, Children = shown ? panes + thumbs : panes };
    }

    private static LayoutNode Thumb(AvailableSpace available, Node.Scroll scroll, LayoutNode thumb, Overflow overflow) {
        float least = Basis(thumb, available.Along, float.PositiveInfinity);
        if (scroll.Axis != available.Along) return Placed(available.Pass, thumb, available.Along, available.Start + available.Space - least, least);
        float length = overflow.Max > 0f ? float.Min(overflow.Viewport, float.Max(least, overflow.Viewport * overflow.Viewport / overflow.Content)) : 0f;
        float offset = overflow.Clamped(available.Pass.Offsets.Find(Stored(available.Layout, scroll)).IfNone(0f));
        return Placed(available.Pass, thumb, available.Along, available.Start + Rescaled(offset, overflow.Max, overflow.Viewport - length), length);
    }

    // --- [CORNER]
    internal static LayoutNode WithCorners(LayoutNode layout, Option<(Corners Corners, SKRect Frame)> background) {
        Corners stated = layout.Node.Role.IsNone
            ? Corners.Square
            : layout.Style.Rounded.Match(
                Some: Corners.Round,
                None: () => background.Map(outer => outer.Corners.Within(outer.Frame, layout.Frame)).IfNone(() => Corners.Round(layout.Style.Metrics[Metric.Radius])));
        Corners corners = layout.Smoothing > 0f ? stated.Fitted(layout.Frame.Size) : stated;
        Option<(Corners Corners, SKRect Frame)> inner = (layout.HasBackground ? Some<(Corners Corners, SKRect Frame)>((corners, layout.Frame)) : background)
            .Map(held => (held.Corners, Frame: SKRect.Create(held.Frame.Location - layout.Frame.Location, held.Frame.Size)));
        Seq<(LayoutNode Was, LayoutNode Now)> children = layout.Children.Map(child => (Was: child, Now: WithCorners(child, inner))).Strict();
        return layout.Corners == corners && children.ForAll(static held => ReferenceEquals(held.Was, held.Now))
            ? layout
            : layout with {
                Corners = corners,
                Children = children.Map(static held => held.Now),
                Members = layout.Members.Map(member => children.Find(held => ReferenceEquals(held.Was, member)).Map(static held => held.Now).IfNone(member)),
            };
    }

    // --- [DISTRIBUTE]
    internal static Seq<float> Distribute(Seq<(Sizing Sizing, float Basis, float Floor)> items, float space) {
        float[] bases = [.. items.Map(static item => item.Basis)];
        float free = space - TensorPrimitives.Sum<float>(bases);
        return toSeq(!float.IsFinite(space)
            ? bases
            : free >= 0f
                ? Shared(items, bases, free)
                : toSeq(items.Map(static item => item.Sizing.ShrinkTier).Somes().Distinct().Order()).Fold(bases, (extents, tier) => Shrunk(items, extents, TensorPrimitives.Sum<float>(extents) - space, tier)));
    }

    private static float[] Shrunk(Seq<(Sizing Sizing, float Basis, float Floor)> items, float[] extents, float deficit, int tier) {
        float[] room = [.. items.Map((item, index) => item.Sizing.ShrinkTier == Some(tier) ? float.Max(0f, extents[index] - item.Floor) : 0f)];
        float total = TensorPrimitives.Sum<float>(room);
        if (total <= 0f || deficit <= 0f) return extents;
        float[] shrunk = new float[extents.Length];
        TensorPrimitives.MultiplyAdd(room, -float.Min(deficit, total) / total, extents, shrunk);
        return shrunk;
    }

    private static float[] Shared(Seq<(Sizing Sizing, float Basis, float Floor)> items, float[] extents, float free) {
        float[] open = [.. items.Map((item, index) => extents[index] < item.Sizing.Limits.Max ? item.Sizing.GrowWeight : 0f)];
        float weight = TensorPrimitives.Sum<float>(open);
        if (weight <= 0f || free <= 0f) return extents;
        float[] capped = new float[extents.Length];
        TensorPrimitives.MultiplyAdd(open, free / weight, extents, capped);
        float proposed = TensorPrimitives.Sum<float>(capped);
        TensorPrimitives.Min(capped, [.. items.Map(static item => item.Sizing.Limits.Max)], capped);
        float spill = proposed - TensorPrimitives.Sum<float>(capped);
        return spill > 0f ? Shared(items, capped, spill) : capped;
    }
}
