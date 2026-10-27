using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Layouts {
    // --- [ARRANGE]
    internal sealed record Pass(HashMap<Seq<Key>, LayoutNode> Prior, HashMap<Seq<Key>, float> Adjusted, Shaping Shaping);

    internal static LayoutNode Arrange(Cascade root, Node node, SKSize size, HashMap<Seq<Key>, float> adjusted, HashMap<Seq<Key>, LayoutNode> prior, Shaping shaping) {
        Pass pass = new(prior, adjusted, shaping);
        return WithCorners(Placed(pass, Placed(pass, Built(pass, root, [], node, prior.Find([])), PhysicalAxis.Horizontal, 0f, size.Width), PhysicalAxis.Vertical, 0f, size.Height), None);
    }

    internal static Fin<LayoutNode> Unique(LayoutNode layout, HashMap<Seq<Key>, LayoutNode> prior) =>
        Repeated(layout, prior).Head.Match<Fin<LayoutNode>>(Some: static path => new UIFault(string.Join('/', path.Map(static key => key.Identifier)), UIReason.Repeated), None: () => layout);

    internal static SKSize Preferred(LayoutNode layout, SKSize offered, HashMap<Seq<Key>, float> adjusted, Shaping shaping) {
        LayoutNode wide = Placed(new Pass([], adjusted, shaping), layout, PhysicalAxis.Horizontal, 0f, float.Min(Basis(layout, PhysicalAxis.Horizontal, offered.Width), offered.Width));
        return new SKSize(wide.Frame.Width, Basis(wide, PhysicalAxis.Vertical, offered.Height));
    }

    private static bool Reused(LayoutNode layout, HashMap<Seq<Key>, LayoutNode> prior) => prior.Find(layout.Path).Exists(held => ReferenceEquals(held, layout));

    private static Seq<Seq<Key>> Repeated(LayoutNode layout, HashMap<Seq<Key>, LayoutNode> prior) =>
        Reused(layout, prior)
            ? []
            : Duplicates(layout.Children).Concat(layout.Members.Bind(member => Repeated(member, prior)));

    private static Seq<Seq<Key>> Duplicates(Seq<LayoutNode> members) =>
        members.Fold((Seen: HashSet<Seq<Key>>(), Repeated: Seq<Seq<Key>>()), static (held, member) => held.Seen.Contains(member.Path)
                ? (held.Seen, held.Repeated.Add(member.Path))
                : (held.Seen.Add(member.Path), held.Repeated))
            .Repeated;

    // --- [QUERY]
    internal sealed record Overflow(float Content, float Viewport);

    internal static Option<PlotFrame> Plotted(LayoutNode layout) => layout.Node.Plotted.Map(plane => plane.In(layout.ContentBox));

    internal static bool IsHandle(LayoutNode layout) => layout.Path.Last.Exists(static last => last.Map(named: false, ordinal: false, handle: true));

    internal static Option<Overflow> Scrollable(LayoutNode scroll, Node.Scroll node) =>
        Panes(scroll).Head.Map(content => new Overflow(node.Axis.Of(content.Frame.Size), node.Axis.Of(scroll.ContentBox.Size)));

    internal static float Scrolling(float offset, float total, float viewport) => float.Clamp(offset, 0f, float.Max(0f, total - viewport));

    internal static float Rescaled(float value, float span, float onto) => span > 0f ? value * onto / span : 0f;

    internal static Seq<Key> Visible(LayoutNode lazy, Node.Lazy node, float offset, float viewport) {
        int perLine = PerLine(lazy, node);
        int first = Line(lazy, node, perLine, offset - viewport);
        return node.Items.Skip(first * perLine).Take((Line(lazy, node, perLine, offset + (2f * viewport)) - first + 1) * perLine);
    }

    internal static Option<SKRect> Slot(LayoutNode lazy, Node.Lazy node, Key key) =>
        Selection.Position(node.Items, key).Map(index => node.Axis.Spanned(lazy.ContentBox, node.Axis.Of(lazy.ContentBox.Location) + (index / PerLine(lazy, node) * (node.Extent + Gap(lazy))), node.Extent));

    private static int Line(LayoutNode lazy, Node.Lazy node, int perLine, float at) =>
        (int)float.Max(0f, float.Min(Lines(node, perLine) - 1, MathF.Floor((at - node.Axis.Of(lazy.ContentBox.Location)) / (node.Extent + Gap(lazy)))));

    private static int Lines(Node.Lazy lazy, int perLine) => (lazy.Items.Count + perLine - 1) / perLine;

    internal static Sizing Sized(Node node, PhysicalAxis along) => (along.Map(horizontal: node.Width, vertical: node.Height) | node.Inner.Map(inner => Sized(inner, along))).IfNone(Sizing.Fit.Content);

    private static Option<Factor> Aspect(Node node) =>
        node.Aspect | node.Plotted.Bind(static plane => plane.Aspect) | node.SourceImage.Bind(static image => Factor.Validated(image.Width / (float)image.Height).ToOption());

    private static Seq<LayoutNode> Panes(LayoutNode owner) => owner.Children.Filter(static child => !IsHandle(child));

    private static float MinContent(LayoutNode layout) => layout.TextNode.Map(static text => text.MinContent).IfNone(0f);

    // --- [BUILD]
    private static LayoutNode Built(Pass pass, Cascade cascade, Seq<Key> path, Node node, Option<LayoutNode> held) =>
        held.Filter(kept => kept.Node.Equals(node) && kept.Cascade.Equals(cascade)).IfNone(() => Created(pass, cascade, path, node, held));

    private static Option<LayoutNode> Held(HashMap<Seq<Key>, LayoutNode> prior, Option<LayoutNode> parent, Seq<Key> path) =>
        prior.Find(path) | parent.Bind(held => held.Members.Find(member => member.Path == path));

    private sealed record Building(Pass Pass, Seq<Key> Path, Node Node, Cascade Cascade, RoleStyle Style, Cascade Inner, Seq<LayoutNode> Members);

    private sealed record Member(Key Segment, Node Node);

    private static LayoutNode Created(Pass pass, Cascade cascade, Seq<Key> path, Node node, Option<LayoutNode> prior) {
        (RoleStyle style, Cascade inner) = Styles.Enter(cascade, node);
        Seq<LayoutNode> members = Members(node).Map(member => {
            Seq<Key> at = path.Add(member.Segment);
            return Built(pass, inner, at, member.Node, Held(pass.Prior, prior, at));
        });
        LayoutNode built = node.SwitchPartially(new Building(pass, path, node, cascade, style, inner, members), @default: BoxNode, choice: static (held, choice) => BoxNode(held, choice) with { Children = held.Members.Take(1) }, text: TextNode);
        float width = IntrinsicWidth(built);
        return built with { Intrinsic = new SKSize(width, IntrinsicHeight(built, width)) };
    }

    private static LayoutNode BoxNode(Building held, Node _) =>
        new LayoutNode.Box { Path = held.Path, Node = held.Node, Cascade = held.Cascade, Style = held.Style, Members = held.Members, Children = held.Members };

    private static LayoutNode TextNode(Building held, Node.Text text) {
        TextStyle face = Typesetting.Resolve(held.Pass.Shaping, held.Inner, held.Style);
        Typeset set = Typesetting.Set(held.Pass.Shaping, text, held.Inner, face, float.PositiveInfinity);
        return new LayoutNode.Text(text, set, face) { Path = held.Path, Node = held.Node, Cascade = held.Cascade, Style = held.Style, MinContent = Typesetting.MinContent(set) };
    }

    private static Seq<Member> Members(Node node) =>
        node.SwitchPartially(
            @default: static held => Segments(held.Inner.ToSeq()),
            flex: static flex => Segments(flex.Children),
            grid: static grid => Segments(grid.Rows.Flatten()),
            lazy: static lazy => lazy.Items.Choose(key => lazy.Realized.Find(key).Map(realized => new Member(key, realized))),
            overlay: static overlay => Segments(overlay.Base.Cons(overlay.Above)),
            choice: static choice => choice.Alternatives.Map(static alternative => new Member(alternative.Key.IfNone(static () => new Key.Ordinal(0)), alternative)),
            scroll: static scroll => Seq(new Member(scroll.Content.Key.IfNone(static () => new Key.Ordinal(0)), scroll.Content), Handle(Role.Scrollbar)),
            plot: static plot => Segments(plot.Children + plot.Labels.Map(static label => (Node)label)));

    private static Member Handle(Role role) => new(new Key.Handle(), new Node.Box { Key = new Key.Handle(), Role = role });

    private static Seq<Member> Segments(Seq<Node> nodes) => nodes.Map(static (node, index) => new Member(node.Key.IfNone(() => new Key.Ordinal((uint)index)), node));

    private static Seq<int> Slots(Node.Lazy lazy) =>
        lazy.Items.Map(static (key, index) => (Key: key, Index: index)).Filter(held => lazy.Realized.ContainsKey(held.Key)).Map(static held => held.Index);

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
            grid: static (held, grid) => GridExtent(held.Layout, grid, held.Along),
            lazy: static (held, lazy) => LazyExtent(held.Layout, lazy, held.Along),
            overlay: static (held, _) => Widest(held.Layout.Children.Head.ToSeq(), held.Along),
            choice: static (held, choice) => Widest((held.Along == choice.Axis ? held.Layout.Members : held.Along.Map(horizontal: held.Layout.Members, vertical: held.Layout.Children)).Take(1), held.Along),
            scroll: static (held, _) => Widest(Panes(held.Layout), held.Along),
            plot: static (held, _) => Widest(held.Layout.Children.Filter(static child => child.Node.Pinned.IsNone), held.Along),
            glyph: static (held, _) => held.Layout.Style.Metrics[Metric.Icon],
            image: static (held, image) => held.Along.Of(new SKSize(image.Source.Width / image.Density, image.Source.Height / image.Density)));

    private static float GridExtent(LayoutNode layout, Node.Grid grid, PhysicalAxis along) {
        Seq<Sizing> columns = Columns(layout, grid);
        Seq<Cell> cells = Cells(layout, grid, columns.Count);
        Seq<GridTrack> gridTracks = Tracks(layout, cells, columns, along, float.PositiveInfinity);
        return along.Switch(
            (Cells: cells, Lanes: gridTracks, Extent: Extent(gridTracks, Gap(layout))),
            horizontal: static held => held.Cells.Filter(static cell => cell.Spans).Fold(held.Extent, (widest, cell) => float.Max(widest, held.Lanes[cell.Column].Start + Basis(cell.Layout, PhysicalAxis.Horizontal, float.PositiveInfinity))),
            vertical: static held => held.Extent);
    }

    private static float LazyExtent(LayoutNode layout, Node.Lazy lazy, PhysicalAxis along) {
        float gap = Gap(layout);
        int perLine = PerLine(layout, lazy);
        int lines = Lines(lazy, perLine);
        return along == lazy.Axis
            ? (lazy.Extent * lines) + (gap * float.Max(0, lines - 1))
            : Extent(Tracked(layout, Celled(layout, lazy, perLine), Sizes(lazy.Across, perLine), along, float.PositiveInfinity), gap);
    }

    private static float Summed(Seq<LayoutNode> children, PhysicalAxis axis, PhysicalAxis along, float gap) =>
        axis == along ? Summed(children.Map(child => Basis(child, along, float.PositiveInfinity)), gap) : Widest(children, along);

    private static float Summed(Seq<float> extents, float gap) => Enumerable.Sum(extents) + (gap * float.Max(0, extents.Count - 1));

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

    private sealed record Cell(int Row, int Column, bool Spans, LayoutNode Layout);

    private sealed record GridTrack(float Start, float Extent);

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
            text: static (available, wrapped) => (wrapped.Typeset.Lines.MaxWidth ?? float.PositiveInfinity).CompareTo(available.Space) is 0
                ? wrapped
                : wrapped with {
                    Typeset = available.Pass.Prior.Find(wrapped.Path)
                        .Bind(static held => held.TextNode)
                        .Filter(held => held.Source.Equals(wrapped.Source) && held.Style.Equals(wrapped.Style) && held.Frame.Width.CompareTo(wrapped.Frame.Width) is 0)
                        .Map(static held => held.Typeset)
                        .IfNone(() => Typesetting.Set(available.Pass.Shaping, wrapped.Source, Styles.Enter(wrapped.Cascade, wrapped.Source).Inner, wrapped.TextStyle, available.Space)),
                });

    private static Option<float> Baseline(LayoutNode layout) =>
        layout.Switch(
            box: static box => box.Children.Choose(static child => Baseline(child).Map(line => child.Frame.Top + line)).Head,
            text: static text => text.Typeset.Baseline.Map(line => text.Origin.Y + line));

    private static Seq<LayoutNode> Baselined(AvailableSpace available, Seq<LayoutNode> placed) {
        Seq<(LayoutNode Child, Option<float> Line)> lines = placed.Map(static child => (Child: child, Line: Baseline(child))).Strict();
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
            flex: static (held, flex) => Shown(held.Layout, flex.Axis == held.Along ? Along(held) : Across(held, flex.Baseline)),
            grid: static (held, grid) => Shown(held.Layout, Gridded(held, grid)),
            lazy: static (held, lazy) => Shown(held.Layout, Windowed(held, lazy)),
            overlay: static (held, _) => Shown(held.Layout, Layers(held)),
            choice: static (held, choice) => Chosen(held, choice),
            scroll: static (held, scroll) => Scrolled(held, scroll),
            plot: static (held, plot) => Guides.Plot(held, plot));

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

    private static Seq<LayoutNode> Across(AvailableSpace available, bool baseline) {
        Seq<LayoutNode> layered = available.Layout.Members.Map(member => Layered(available, member));
        return baseline && available.Along == PhysicalAxis.Vertical ? Baselined(available, layered) : layered;
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

    private static Seq<LayoutNode> Layers(AvailableSpace available) =>
        available.Layout.Members.Match(
            Empty: static () => Seq<LayoutNode>(),
            Tail: (baseNode, above) => {
                SKRect box = available.Layout.ContentBox;
                SKMatrix placed = SKMatrix.CreateScaleTranslation(box.Width, box.Height, box.Left, box.Top);
                return Filled(available, baseNode).Cons(Guides.Pins(available, above, at => placed.MapPoint(at), Layered));
            });

    private static Seq<LayoutNode> Gridded(AvailableSpace available, Node.Grid grid) {
        Seq<Sizing> columns = Columns(available.Layout, grid);
        Seq<Cell> cells = Cells(available.Layout, grid, columns.Count);
        Seq<GridTrack> gridTracks = Tracks(available.Layout, cells, columns, available.Along, available.Space);
        return cells.Map(cell => {
            (int track, bool spans) = available.Along.Map(horizontal: (cell.Column, cell.Spans), vertical: (cell.Row, false));
            return Aligned(available, cell.Layout, available.Start + gridTracks[track].Start, spans ? available.Space - gridTracks[track].Start : gridTracks[track].Extent, cell.Layout.Node.Align.Of(available.Along));
        });
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

    private static Seq<Cell> Cells(LayoutNode layout, Node.Grid grid, int columns) =>
        from row in grid.Rows.Zip(grid.Rows.Scan(0, static (start, cells) => start + cells.Count))
            .Bind(row => toSeq(Enumerable.Range(row.Second, row.First.Count).Chunk(columns)))
            .Map(static (members, index) => (Members: members, Index: index))
        from cell in toSeq(row.Members).Map(static (member, column) => (Member: member, Column: column))
        select new Cell(row.Index, cell.Column, cell.Column == row.Members.Length - 1 && row.Members.Length < columns, layout.Members[cell.Member]);

    private static Seq<GridTrack> Tracks(LayoutNode layout, Seq<Cell> cells, Seq<Sizing> columns, PhysicalAxis along, float space) =>
        along.Switch(
            (Layout: layout, Cells: cells, Columns: columns, Space: space),
            horizontal: static held => Tracked(held.Layout, held.Cells, held.Columns, PhysicalAxis.Horizontal, held.Space),
            vertical: static held => Started(held.Layout, toSeq(held.Cells.GroupBy(static cell => cell.Row).Select(static row => Widest(toSeq(row).Map(static cell => cell.Layout), PhysicalAxis.Vertical)))));

    private static Seq<GridTrack> Tracked(LayoutNode layout, Seq<Cell> cells, Seq<Sizing> columns, PhysicalAxis along, float space) {
        float available = space - (Gap(layout) * float.Max(0, columns.Count - 1));
        return Started(layout, Distributed(columns, Columned(cells, columns, along, available), available));
    }

    private static Seq<GridTrack> Started(LayoutNode layout, Seq<float> extents) {
        float gap = Gap(layout);
        return extents.Zip(extents.Scan(0f, (start, extent) => start + extent + gap)).Map(static gridTrack => new GridTrack(gridTrack.Second, gridTrack.First));
    }

    private static float Extent(Seq<GridTrack> gridTracks, float gap) => Summed(gridTracks.Map(static gridTrack => gridTrack.Extent), gap);

    private static Seq<float> Columned(Seq<Cell> cells, Seq<Sizing> columns, PhysicalAxis across, float space) {
        HashMap<int, float> widths = toHashMap(cells.Filter(static cell => !cell.Spans).GroupBy(static cell => cell.Column).Select(column => (column.Key, Widest(toSeq(column).Map(static cell => cell.Layout), across))));
        return columns.Map((sizing, column) => Based(sizing, widths.Find(column).IfNone(0f), space));
    }

    private static Seq<LayoutNode> Windowed(AvailableSpace available, Node.Lazy lazy) {
        float gap = Gap(available.Layout);
        int perLine = PerLine(available.Layout, lazy);
        Seq<Cell> cells = Celled(available.Layout, lazy, perLine);
        if (available.Along == lazy.Axis) return cells.Map(cell => Aligned(available, cell.Layout, available.Start + (cell.Row * (lazy.Extent + gap)), lazy.Extent, cell.Layout.Node.Align.Of(available.Along)));
        Seq<GridTrack> gridTracks = Tracked(available.Layout, cells, Sizes(lazy.Across, perLine), available.Along, available.Space);
        return cells.Map(cell => Aligned(available, cell.Layout, available.Start + gridTracks[cell.Column].Start, gridTracks[cell.Column].Extent, cell.Layout.Node.Align.Of(available.Along)));
    }

    private static int PerLine(LayoutNode layout, Node.Lazy lazy) =>
        lazy.Across.Switch(
            (Layout: layout, Lazy: lazy),
            @explicit: static (_, tracks) => int.Max(1, tracks.Sizes.Count),
            autoFill: static (held, tracks) => Fill(tracks.Size, held.Lazy.Axis.Cross.Of(held.Layout.ContentBox.Size), Gap(held.Layout), Widest(held.Layout.Members, held.Lazy.Axis.Cross), held.Lazy.Items.Count));

    private static Seq<Cell> Celled(LayoutNode layout, Node.Lazy lazy, int perLine) =>
        layout.Members.Zip(Slots(lazy)).Map(held => new Cell(held.Second / perLine, held.Second % perLine, Spans: false, held.First));

    private static LayoutNode Scrolled(AvailableSpace available, Node.Scroll scroll) {
        Seq<LayoutNode> panes = available.Layout.Members.Filter(static member => !IsHandle(member)).Map(content => scroll.Axis == available.Along
            ? Placed(available.Pass, content, available.Along, available.Start, float.Max(Basis(content, available.Along, float.PositiveInfinity), available.Space))
            : Filled(available, content));
        float total = panes.Map(pane => scroll.Axis.Of(pane.Frame.Size)).Fold(0f, float.Max);
        float viewport = scroll.Axis.Of(available.Layout.ContentBox.Size);
        Seq<LayoutNode> thumbs = available.Layout.Members.Filter(IsHandle).Map(thumb => Thumb(available, scroll, thumb, total, viewport));
        bool shown = available.Along.Map(horizontal: scroll.Axis.Map(horizontal: total > viewport, vertical: true), vertical: total > viewport);
        return available.Layout with { Members = panes + thumbs, Children = shown ? panes + thumbs : panes };
    }

    private static LayoutNode Thumb(AvailableSpace available, Node.Scroll scroll, LayoutNode thumb, float total, float viewport) {
        float least = Basis(thumb, available.Along, float.PositiveInfinity);
        if (scroll.Axis != available.Along) return Placed(available.Pass, thumb, available.Along, available.Start + available.Space - least, least);
        float length = total > viewport ? float.Min(viewport, float.Max(least, viewport * viewport / total)) : 0f;
        float offset = Scrolling(available.Pass.Adjusted.Find(available.Layout.Path).IfNone(0f), total, viewport);
        return Placed(available.Pass, thumb, available.Along, available.Start + Rescaled(offset, total - viewport, viewport - length), length);
    }

    // --- [CORNER]
    private sealed record CornerBackground(Corners Corners, SKRect Frame);

    private static LayoutNode WithCorners(LayoutNode layout, Option<CornerBackground> background) {
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
        Seq<float> bases = items.Map(static item => item.Basis);
        float free = space - Enumerable.Sum(bases);
        return !float.IsFinite(space)
            ? bases
            : free >= 0f
                ? Shared(items, bases, free)
                : toSeq(items.Map(static item => item.Sizing.ShrinkTier).Somes().Distinct().Order()).Fold(bases, (extents, tier) => Shrunk(items, extents, Enumerable.Sum(extents) - space, tier));
    }

    private static Seq<float> Shrunk(Seq<FlexItem> items, Seq<float> extents, float deficit, int tier) {
        Seq<float> available = items.Zip(extents).Map(pair => pair.First.Sizing.ShrinkTier == Some(tier) ? float.Max(0f, pair.Second - pair.First.Floor) : 0f).Strict();
        float total = Enumerable.Sum(available);
        return total > 0f && deficit > 0f ? extents.Zip(available).Map(pair => pair.First - (pair.Second * float.Min(deficit, total) / total)) : extents;
    }

    private static Seq<float> Shared(Seq<FlexItem> items, Seq<float> extents, float free) {
        Seq<float> open = items.Zip(extents).Map(static pair => pair.Second < pair.First.Sizing.Limits.Max ? pair.First.Sizing.GrowWeight : 0f);
        float weight = Enumerable.Sum(open);
        if (weight <= 0f || free <= 0f) return extents;
        Seq<float> proposed = extents.Zip(open).Map(pair => pair.First + (free * pair.Second / weight));
        Seq<float> capped = items.Zip(proposed).Map(static pair => float.Min(pair.First.Sizing.Limits.Max, pair.Second));
        float spill = Enumerable.Sum(proposed) - Enumerable.Sum(capped);
        return spill > 0f ? Shared(items, capped, spill) : capped;
    }
}
