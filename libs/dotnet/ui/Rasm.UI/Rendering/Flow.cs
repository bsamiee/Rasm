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
    internal sealed record Pass(HashMap<Seq<Key>, LayoutNode> Prior, HashMap<Seq<Key>, float> Adjusted, Shaping Shaping, ColorCache ColorCache, CultureInfo Culture);

    internal static LayoutNode Arrange(Pass pass, Cascade root, Node node, SKSize size) => WithCorners(Framed(pass, Built(pass, root, [], node, pass.Prior.Find([])), SKRect.Create(size)), None);

    internal static LayoutNode Framed(Pass pass, LayoutNode layout, SKRect frame) =>
        Placed(pass, Placed(pass, layout, PhysicalAxis.Horizontal, frame.Left, frame.Width), PhysicalAxis.Vertical, frame.Top, frame.Height);

    internal static Fin<LayoutNode> Unique(LayoutNode layout, HashMap<Seq<Key>, LayoutNode> prior) =>
        Repeated(layout, prior).Head.Match<Fin<LayoutNode>>(Some: static path => new UIFault(string.Join('/', path.Map(static key => key.Identifier)), UIReason.Repeated), None: () => layout);

    internal static SKSize Preferred(Pass pass, LayoutNode layout, SKSize offered) {
        LayoutNode wide = Placed(pass, layout, PhysicalAxis.Horizontal, 0f, float.Min(Basis(layout, PhysicalAxis.Horizontal, offered.Width), offered.Width));
        return new SKSize(wide.Frame.Width, Basis(wide, PhysicalAxis.Vertical, offered.Height));
    }

    private static bool Reused(LayoutNode layout, HashMap<Seq<Key>, LayoutNode> prior) => prior.Find(layout.Path).Exists(held => ReferenceEquals(held, layout));

    private static Seq<Seq<Key>> Repeated(LayoutNode layout, HashMap<Seq<Key>, LayoutNode> prior) =>
        Reused(layout, prior)
            ? []
            : Duplicates(layout.Children + layout.Layers).Concat((layout.Members + layout.Layers).Bind(member => Repeated(member, prior)));

    private static Seq<Seq<Key>> Duplicates(Seq<LayoutNode> members) =>
        members.Fold((Seen: HashSet<Seq<Key>>(), Repeated: Seq<Seq<Key>>()), static (held, member) => held.Seen.Contains(member.Path)
                ? (held.Seen, held.Repeated.Add(member.Path))
                : (held.Seen.Add(member.Path), held.Repeated))
            .Repeated;

    // --- [QUERY]
    internal sealed record Overflow(float Content, float Viewport) {
        internal float Max => float.Max(0f, Content - Viewport);

        internal float Clamped(float offset) => float.Clamp(offset, 0f, Max);
    }

    internal static Option<PlotFrame> Plotted(LayoutNode layout) => layout.Node.Plotted.Map(plane => plane.In(layout.Data));

    internal static bool IsHandle(LayoutNode layout) => layout.Path.Last.Exists(static last => last.Map(named: false, ordinal: false, handle: true));

    internal static Seq<Key> Stored(LayoutNode scroll, Node.Scroll node) => node.Follows.IfNone(scroll.Path);

    internal static Option<Overflow> Scrollable(LayoutNode scroll, Node.Scroll node) =>
        Panes(scroll).Head.Map(content => new Overflow(node.Axis.Of(content.Frame.Size), node.Axis.Of(scroll.ContentBox.Size)));

    internal static float Rescaled(float value, float span, float onto) => span > 0f ? value * onto / span : 0f;

    internal static Seq<LayoutNode> Panes(LayoutNode owner) => owner.Children.Filter(static child => !IsHandle(child));

    internal static Sizing Sized(Node node, PhysicalAxis along) => (along.Map(horizontal: node.Width, vertical: node.Height) | node.Inner.Map(inner => Sized(inner, along))).IfNone(Sizing.Fit.Content);

    private static Option<Factor> Aspect(Node node) =>
        node.Aspect | node.Plotted.Bind(static plane => plane.Aspect)
        | (node is Node.Image image ? Factor.Validated(image.Source.Info.Width / (float)image.Source.Info.Height).ToOption() : None);

    private static float MinContent(LayoutNode layout) => layout.TextNode.Map(static text => text.MinContent).IfNone(0f);

    // --- [WINDOW]
    private sealed record Window(float From, float To) {
        internal static Window Of(PhysicalAxis axis, LayoutNode lazy, SKRect frame, SKRect shown) {
            float offset = axis.Of(shown.Location - frame.Location) - axis.Of(lazy.ContentBox.Location);
            float viewport = axis.Of(shown.Size);
            return new(offset - viewport, offset + (2f * viewport));
        }

        internal bool Meets(GridTrack track) => track.Start + track.Extent >= From && track.Start <= To;
    }

    internal static Seq<Key> Visible(Scene scene, LayoutNode lazy, Node.Lazy node) =>
        (from frame in scene.Frame(lazy.Path)
         from shown in scene.Visible(lazy.Path)
         let perLine = PerLine(lazy, node)
         let lines = Window.Of(node.Axis, lazy, frame, shown)
         let first = Line(lazy, node, perLine, lines.From)
         let across = Window.Of(node.Axis.Cross, lazy, frame, shown)
         let tracks = Definite(lazy, node, perLine)
             .Map(definite => definite.Map(static (track, index) => (Track: track, Index: index)).Filter(held => across.Meets(held.Track)).Map(static held => held.Index))
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

    private static Option<Seq<GridTrack>> Definite(LayoutNode lazy, Node.Lazy node, int perLine) =>
        Sizes(node.Across, perLine).Traverse(static size => size.Definite).As().Map(extents => Started(lazy, extents));

    private static int Line(LayoutNode lazy, Node.Lazy node, int perLine, float at) =>
        (int)float.Clamp(MathF.Floor(at / (node.Extent + Gap(lazy))), 0f, float.Max(0f, Lines(node, perLine) - 1));

    private static int Lines(Node.Lazy lazy, int perLine) => (lazy.Items.Count + perLine - 1) / perLine;

    // --- [BUILD]
    internal static LayoutNode Built(Pass pass, Cascade cascade, Seq<Key> path, Node node, Option<LayoutNode> held) =>
        held.Filter(kept => kept.Node.Equals(node) && kept.Cascade.Equals(cascade)).IfNone(() => Created(pass, cascade, path, node, held));

    private static Option<LayoutNode> Held(HashMap<Seq<Key>, LayoutNode> prior, Option<LayoutNode> parent, Seq<Key> path) =>
        prior.Find(path) | parent.Bind(held => (held.Members + held.Layers).Find(member => member.Path == path));

    private sealed record Building(Pass Pass, Seq<Key> Path, Node Node, Cascade Cascade, RoleStyle Style, Cascade Inner, Seq<LayoutNode> Members);

    private sealed record Member(Key Segment, Node Node);

    private static LayoutNode Created(Pass pass, Cascade cascade, Seq<Key> path, Node node, Option<LayoutNode> prior) {
        (RoleStyle style, Cascade inner) = Styles.Enter(cascade, node);
        Seq<LayoutNode> members = Members(node).Map(member => {
            Seq<Key> at = path.Add(member.Segment);
            return Built(pass, inner, at, member.Node, Held(pass.Prior, prior, at));
        }).Strict();
        LayoutNode built = node.SwitchPartially(new Building(pass, path, node, cascade, style, inner, members.Filter(static member => member.Node is not Node.Layer)),
            @default: BoxNode,
            choice: static (held, choice) => BoxNode(held, choice) with { Children = held.Members.Take(1) },
            mark: static (held, mark) => mark.Figure is Figure.Label label ? TextNode(held, new Node.Text(label.Spans)) : BoxNode(held, mark),
            text: TextNode) with {
            Layers = members.Filter(static member => member.Node is Node.Layer),
        };
        float width = IntrinsicWidth(built);
        return built with { Intrinsic = new SKSize(width, IntrinsicHeight(built, width)) };
    }

    private static LayoutNode BoxNode(Building held, Node _) =>
        new LayoutNode.Box { Path = held.Path, Node = held.Node, Cascade = held.Cascade, Inner = held.Inner, Style = held.Style, Members = held.Members, Children = held.Members };

    private static LayoutNode TextNode(Building held, Node.Text text) {
        TextStyle face = Typesetting.Resolve(held.Pass.Shaping, held.Pass.ColorCache, held.Inner, held.Style);
        Typeset set = Typesetting.Set(held.Pass.Shaping, held.Pass.ColorCache, text, held.Inner, face, float.PositiveInfinity, held.Pass.Culture);
        return new LayoutNode.Text(text, set, face) {
            Path = held.Path, Node = held.Node, Cascade = held.Cascade, Inner = held.Inner, Style = held.Style, MinContent = Typesetting.MinContent(held.Pass.Shaping, set),
        };
    }

    private static Seq<Member> Members(Node node) =>
        node.SwitchPartially(
            @default: static held => Segments(held.Inner.ToSeq()),
            flex: static flex => Segments(flex.Children),
            grid: static grid => Segments(grid.Rows.Flatten()),
            lazy: static lazy => lazy.Items.Choose(key => lazy.Realized.Find(key).Map(realized => new Member(key, realized))),
            overlay: static overlay => Segments(overlay.Base.Cons(overlay.Above)),
            layer: static layer => (layer.Modal ? Seq(new Member(LayerStack.Veil, new Node.Box { Role = Role.Veil })) : []) + Segments([layer.Content]),
            choice: static choice => choice.Alternatives.Map(static alternative => new Member(alternative.Key.IfNone(static () => new Key.Ordinal(0)), alternative)),
            scroll: static scroll => Seq(new Member(scroll.Content.Key.IfNone(static () => new Key.Ordinal(0)), scroll.Content), Handle(Role.Scrollbar)),
            plot: static plot => Segments(plot.Children));

    private static Member Handle(Role role) => new(new Key.Handle(), new Node.Box { Key = new Key.Handle(), Role = role });

    private static Seq<Member> Segments(Seq<Node> nodes) => nodes.Map(static (node, index) => new Member(node.Key.IfNone(() => new Key.Ordinal((uint)index)), node));

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
            flex: static (held, flex) => Summed(held.Layout.Children, flex.Axis, held.Along, Gap(held.Layout)),
            grid: static (held, grid) => Lattice.Of(held.Layout, grid).Extent(held.Layout, held.Along),
            lazy: static (held, lazy) => Lattice.Of(held.Layout, lazy).Extent(held.Layout, held.Along),
            overlay: static (held, _) => Widest(held.Layout.Children.Head.ToSeq(), held.Along),
            choice: static (held, choice) => Widest((held.Along == choice.Axis ? held.Layout.Members : held.Along.Map(horizontal: held.Layout.Members, vertical: held.Layout.Children)).Take(1), held.Along),
            scroll: static (held, _) => Widest(Panes(held.Layout), held.Along),
            plot: static (held, _) => Widest(held.Layout.Children.Filter(static child => child.Node.Pinned.IsNone), held.Along),
            glyph: static (held, _) => held.Layout.Style.Metrics[Metric.Icon],
            image: static (held, image) => held.Along.Of(new SKSize(image.Source.Info.Width / image.Density, image.Source.Info.Height / image.Density)));

    private static float Summed(Seq<LayoutNode> children, PhysicalAxis axis, PhysicalAxis along, float gap) =>
        axis == along ? Summed(children.Map(child => Basis(child, along, float.PositiveInfinity)), gap) : Widest(children, along);

    private static float Summed(Seq<float> extents, float gap) => TensorPrimitives.Sum<float>([.. extents]) + (gap * float.Max(0, extents.Count - 1));

    private static float Widest(Seq<LayoutNode> children, PhysicalAxis along) => children.Map(child => Basis(child, along, float.PositiveInfinity)).Fold(0f, float.Max);

    private static float Basis(LayoutNode layout, PhysicalAxis along, float space) => Based(Sized(layout.Node, along), along.Of(layout.Intrinsic), space);

    private static float Based(Sizing sizing, float fit, float space) =>
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
            horizontal: static held => {
                LayoutNode set = Wrapped(held.Room, held.Layout);
                return set with { Intrinsic = new SKSize(set.Intrinsic.Width, IntrinsicHeight(set, set.Frame.Width)) };
            },
            vertical: static held => held.Layout.Switch(
                box: static box => (LayoutNode)box,
                text: static text => {
                    SKRect box = text.ContentBox;
                    return text with { Origin = new SKPoint(box.Left, box.Top + (float.Max(0f, box.Height - text.Typeset.Size.Height) * (float)text.Source.Align.Y)) };
                }));
    }

    private static LayoutNode Wrapped(AvailableSpace available, LayoutNode layout) =>
        layout.Switch(
            available,
            box: static (_, box) => (LayoutNode)box,
            text: static (available, wrapped) => wrapped.Typeset.Setting.Width.CompareTo(available.Space) is 0
                ? wrapped
                : wrapped with {
                    Typeset = available.Pass.Prior.Find(wrapped.Path)
                        .Bind(static held => held.TextNode)
                        .Filter(held => held.Source.Equals(wrapped.Source) && held.Style.Equals(wrapped.Style) && held.Frame.Width.CompareTo(wrapped.Frame.Width) is 0)
                        .Map(static held => held.Typeset)
                        .IfNone(() => Typesetting.Set(available.Pass.Shaping, available.Pass.ColorCache, wrapped.Source, wrapped.Inner, wrapped.TextStyle, available.Space, available.Pass.Culture)),
                });

    private static Option<float> Baseline(LayoutNode layout, Baseline baseline) =>
        layout.Switch(
            baseline,
            box: static (line, box) => box.Children.Choose(child => Baseline(child, line).Map(at => child.Frame.Top + at)) switch {
                var lines => line.Line.IsFromEnd ? lines.Last : lines.Head,
            },
            text: static (line, text) => text.Typeset.At(line).Map(at => text.Origin.Y + at));

    private static Seq<LayoutNode> Baselined(AvailableSpace available, Seq<LayoutNode> placed, Baseline baseline) {
        Seq<(LayoutNode Child, Option<float> Line)> lines = placed.Map(child => (Child: child, Line: Baseline(child, baseline))).Strict();
        float most = lines.Choose(static held => held.Line).Fold(0f, float.Max);
        return lines.Map(held => held.Line.Match(Some: line => Moved(held.Child, available.Along, available.Start + most - line), None: () => held.Child));
    }

    private static LayoutNode Moved(LayoutNode layout, PhysicalAxis along, float start) =>
        along.Of(layout.Frame.Location).CompareTo(start) is 0
            ? layout
            : layout with { Frame = along.Spanned(layout.Frame, start, along.Of(layout.Frame.Size)) };

    private static LayoutNode Shown(LayoutNode layout, Seq<LayoutNode> placed) => layout with { Members = placed, Children = placed };

    internal static LayoutNode Filled(AvailableSpace available, LayoutNode child) => Placed(available.Pass, child, available.Along, available.Start, available.Space);

    internal static LayoutNode Layered(AvailableSpace available, LayoutNode child) => Aligned(available, child, available.Start, available.Space, child.Node.Align.Of(available.Along));

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
        Seq<float> extents = Distribute(children.Map(child => {
            Sizing sizing = Sized(child.Node, available.Along);
            return new FlexItem(sizing, Basis(child, available.Along, space), sizing.Floor(available.Along.Map(horizontal: MinContent(child), vertical: 0f)));
        }), space);
        Seq<float> starts = extents.Scan(available.Start, (start, extent) => start + extent + gap);
        return children.Zip(extents.Zip(starts)).Map(placed => Placed(available.Pass, placed.First, available.Along, placed.Second.Second, placed.Second.First));
    }

    private static Seq<LayoutNode> Across(AvailableSpace available, Option<Baseline> baseline) {
        Seq<LayoutNode> layered = available.Layout.Members.Map(member => Layered(available, member));
        return baseline.Filter(_ => available.Along == PhysicalAxis.Vertical).Map(line => Baselined(available, layered, line)).IfNone(layered);
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
            Tail: (baseNode, above) => {
                SKRect box = available.Layout.ContentBox;
                SKMatrix placed = SKMatrix.CreateScaleTranslation(box.Width, box.Height, box.Left, box.Top);
                return Filled(available, baseNode).Cons(Guides.Pins(available, box, above, at => placed.MapPoint(at), Layered));
            });

    // --- [TRACKS]
    private sealed record Cell(int Line, int Track, bool Spans, LayoutNode Layout);

    internal sealed record GridTrack(float Start, float Extent);

    private sealed record Lattice(PhysicalAxis Axis, int Count, Option<float> Line, Seq<Sizing> Tracks, Seq<Cell> Cells) {
        internal static Lattice Of(LayoutNode layout, Node.Grid grid) {
            Seq<Sizing> columns = Columns(layout, grid);
            Seq<Cell> cells = Celled(layout, grid, columns.Count);
            return new(PhysicalAxis.Vertical, cells.Map(static cell => cell.Line + 1).Fold(0, int.Max), None, columns, cells);
        }

        internal static Lattice Of(LayoutNode layout, Node.Lazy lazy) {
            int perLine = PerLine(layout, lazy);
            Seq<Cell> cells = layout.Members.Zip(Slots(lazy)).Map(held => new Cell(held.Second / perLine, held.Second % perLine, Spans: false, held.First));
            return new(lazy.Axis, Lines(lazy, perLine), Some(lazy.Extent), Sizes(lazy.Across, perLine), cells);
        }

        internal Seq<GridTrack> Tracked(LayoutNode layout, PhysicalAxis along, float space) {
            if (along != Axis) {
                float available = space - (Gap(layout) * float.Max(0, Tracks.Count - 1));
                return Started(layout, Distributed(Tracks, Columned(along, available), available));
            }
            HashMap<int, float> tallest = toHashMap(Cells.GroupBy(static cell => cell.Line).Select(line => (line.Key, Widest(toSeq(line).Map(static cell => cell.Layout), along))));
            return Started(layout, Line.Match(Some: extent => LanguageExt.Seq.repeat(extent, Count), None: () => toSeq(Range(0, Count)).Map(line => tallest.Find(line).IfNone(0f))));
        }

        internal float Extent(LayoutNode layout, PhysicalAxis along) {
            Seq<GridTrack> tracks = Tracked(layout, along, float.PositiveInfinity);
            float extent = Summed(tracks.Map(static track => track.Extent), Gap(layout));
            return along == Axis ? extent : Cells.Filter(static cell => cell.Spans).Fold(extent, (widest, cell) => float.Max(widest, tracks[cell.Track].Start + Basis(cell.Layout, along, float.PositiveInfinity)));
        }

        internal Seq<LayoutNode> Arranged(AvailableSpace available) {
            Seq<GridTrack> tracks = Tracked(available.Layout, available.Along, available.Space);
            bool lines = available.Along == Axis;
            return Cells.Map(cell => tracks[lines ? cell.Line : cell.Track] switch {
                var track => Aligned(available, cell.Layout, available.Start + track.Start, !lines && cell.Spans ? available.Space - track.Start : track.Extent, cell.Layout.Node.Align.Of(available.Along)),
            });
        }

        private Seq<float> Columned(PhysicalAxis across, float space) {
            HashMap<int, float> widths = toHashMap(Cells.Filter(static cell => !cell.Spans).GroupBy(static cell => cell.Track).Select(track => (track.Key, Widest(toSeq(track).Map(static cell => cell.Layout), across))));
            return Tracks.Map((sizing, track) => Based(sizing, widths.Find(track).IfNone(0f), space));
        }
    }

    private static Seq<Sizing> Columns(LayoutNode layout, Node.Grid grid) {
        int longest = grid.Rows.Map(static row => row.Count).Fold(0, int.Max);
        return Sizes(grid.Columns, grid.Columns.Switch(
            (Layout: layout, Longest: longest),
            @explicit: static (held, tracks) => int.Max(tracks.Sizes.Count, held.Longest),
            autoFill: static (held, tracks) => Fill(tracks.Size, held.Layout.ContentBox.Width, Gap(held.Layout), Widest(held.Layout.Members, PhysicalAxis.Horizontal), held.Longest)));
    }

    private static Seq<Sizing> Sizes(Tracks tracks, int count) =>
        tracks.Switch(
            count,
            @explicit: static (perLine, held) => toSeq(Range(0, perLine)).Map(column => held.Sizes.At(column).IfNone(Sizing.Fit.Content)),
            autoFill: static (perLine, held) => LanguageExt.Seq.repeat(held.Size, perLine));

    private static int Fill(Sizing size, float space, float gap, float widest, int most) =>
        (int)float.Clamp(MathF.Floor((space + gap) / float.Max(Based(size, widest, space) + gap, float.Epsilon)), 1f, float.Max(1f, most));

    private static Seq<Cell> Celled(LayoutNode layout, Node.Grid grid, int columns) =>
        from row in grid.Rows.Zip(grid.Rows.Scan(0, static (start, cells) => start + cells.Count))
            .Bind(row => toSeq(Enumerable.Range(row.Second, row.First.Count).Chunk(columns)))
            .Map(static (members, index) => (Members: members, Index: index))
        from cell in toSeq(row.Members).Map(static (member, column) => (Member: member, Column: column))
        select new Cell(row.Index, cell.Column, cell.Column == row.Members.Length - 1 && row.Members.Length < columns, layout.Members[cell.Member]);

    private static Seq<GridTrack> Started(LayoutNode layout, Seq<float> extents) {
        float gap = Gap(layout);
        return extents.Zip(extents.Scan(0f, (start, extent) => start + extent + gap)).Map(static track => new GridTrack(track.Second, track.First));
    }

    private static int PerLine(LayoutNode layout, Node.Lazy lazy) =>
        lazy.Across.Switch(
            (Layout: layout, Lazy: lazy),
            @explicit: static (_, tracks) => int.Max(1, tracks.Sizes.Count),
            autoFill: static (held, tracks) => Fill(tracks.Size, held.Lazy.Axis.Cross.Of(held.Layout.ContentBox.Size), Gap(held.Layout), Widest(held.Layout.Members, held.Lazy.Axis.Cross), held.Lazy.Items.Count));

    private static Seq<int> Slots(Node.Lazy lazy) =>
        lazy.Items.Map(static (key, index) => (Key: key, Index: index)).Filter(held => lazy.Realized.ContainsKey(held.Key)).Map(static held => held.Index);

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
        float offset = overflow.Clamped(available.Pass.Adjusted.Find(Stored(available.Layout, scroll)).IfNone(0f));
        return Placed(available.Pass, thumb, available.Along, available.Start + Rescaled(offset, overflow.Max, overflow.Viewport - length), length);
    }

    // --- [CORNER]
    internal sealed record CornerBackground(Corners Corners, SKRect Frame);

    internal static LayoutNode WithCorners(LayoutNode layout, Option<CornerBackground> background) {
        Corners stated = layout.Node.Role.IsNone
            ? Corners.Square
            : layout.Style.Rounded.Match(
                Some: Corners.Round,
                None: () => background.Map(outer => outer.Corners.Within(outer.Frame, layout.Frame)).IfNone(() => Corners.Round(layout.Style.Metrics[Metric.Radius])));
        Corners corners = layout.Smoothing > 0f ? stated.Fitted(layout.Frame.Size) : stated;
        Option<CornerBackground> inner = (layout.HasBackground ? Some(new CornerBackground(corners, layout.Frame)) : background)
            .Map(held => held with { Frame = SKRect.Create(held.Frame.Location - layout.Frame.Location, held.Frame.Size) });
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
    internal sealed record FlexItem(Sizing Sizing, float Basis, float Floor);

    private static Seq<float> Distributed(Seq<Sizing> tracks, Seq<float> bases, float space) =>
        Distribute(tracks.Zip(bases).Map(static track => new FlexItem(track.First, track.Second, track.First.Floor(0f))), space);

    internal static Seq<float> Distribute(Seq<FlexItem> items, float space) {
        float[] bases = [.. items.Map(static item => item.Basis)];
        float free = space - TensorPrimitives.Sum<float>(bases);
        return toSeq(!float.IsFinite(space)
            ? bases
            : free >= 0f
                ? Shared(items, bases, free)
                : toSeq(items.Map(static item => item.Sizing.ShrinkTier).Somes().Distinct().Order()).Fold(bases, (extents, tier) => Shrunk(items, extents, TensorPrimitives.Sum<float>(extents) - space, tier)));
    }

    private static float[] Shrunk(Seq<FlexItem> items, float[] extents, float deficit, int tier) {
        float[] room = [.. items.Map((item, index) => item.Sizing.ShrinkTier == Some(tier) ? float.Max(0f, extents[index] - item.Floor) : 0f)];
        float total = TensorPrimitives.Sum<float>(room);
        if (total <= 0f || deficit <= 0f) return extents;
        float[] shrunk = new float[extents.Length];
        TensorPrimitives.MultiplyAdd(room, -float.Min(deficit, total) / total, extents, shrunk);
        return shrunk;
    }

    private static float[] Shared(Seq<FlexItem> items, float[] extents, float free) {
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
