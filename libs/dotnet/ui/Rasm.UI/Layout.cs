using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class Orientation {
    public static readonly Orientation Horizontal = new(
        new Vector2(1f, 0f),
        Cursor.ColumnResize,
        static value => value.X,
        static (value, along) => value with { X = along },
        static (frame, start, extent) => SKRect.Create(start, frame.Top, extent, frame.Height),
        static node => node.Width);

    public static readonly Orientation Vertical = new(
        new Vector2(0f, -1f),
        Cursor.RowResize,
        static value => value.Y,
        static (value, along) => value with { Y = along },
        static (frame, start, extent) => SKRect.Create(frame.Left, start, frame.Width, extent),
        static node => node.Height.Switch(sizing: static sizing => sizing, ratio: static ratio => ratio.Bounds));

    public Vector2 Forward { get; }
    public Cursor Resize { get; }

    public float Of(SKSize size) => Of(size.ToPoint());

    [UseDelegateFromConstructor]
    public partial float Of(Vector2 value);

    [UseDelegateFromConstructor]
    public partial Vector2 With(Vector2 value, float along);

    [UseDelegateFromConstructor]
    public partial SKRect Spanned(SKRect frame, float start, float extent);

    [UseDelegateFromConstructor]
    public partial Sizing Sized(Node node);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Sizing {
    public float Min { get; init; }
    public float Max { get; init; } = float.PositiveInfinity;

    public float Clamped(float value) => float.Max(Min, float.Min(Max, value));

    public sealed record Fixed(float Extent) : Sizing;

    public sealed record Fit : Sizing {
        public static Fit Content { get; } = new();
    }

    public sealed record Grow(float Weight) : Sizing;

    public sealed record Percent(float Fraction) : Sizing;
}

public sealed record Ratio(float PerWidth) {
    public Sizing.Fit Bounds { get; init; } = Sizing.Fit.Content;
}

[Union<Sizing, Ratio>]
public sealed partial class Height;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record DuplicateKey(Seq<Key> Path) : Expected($"sibling key /{string.Join('/', Path.Map(static key => key.ToValue()))} repeats", 0);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Layouts {
    private sealed record Room(Laid Laid, Orientation Along, float Inset, float Space, HashMap<Seq<Key>, Laid> Prior, HashMap<Seq<Key>, float> Adjusted);

    // --- [ARRANGE]
    internal static (Laid Laid, HashMap<Style, TextStyle> Texts) Arrange(Cascade root, Node node, SKSize size, HashMap<Seq<Key>, float> adjusted, HashMap<Seq<Key>, Laid> prior, HashMap<Style, TextStyle> texts) =>
        Built(root, [], node, prior.Find([]), prior, texts) switch {
            var (built, held) => (Placed(Placed(built, Orientation.Horizontal, 0f, size.Width, prior, adjusted), Orientation.Vertical, 0f, size.Height, prior, adjusted), held),
        };

    internal static Fin<Laid> Unique(Laid laid, HashMap<Seq<Key>, Laid> prior) =>
        Repeated(laid, prior).Head.Match<Fin<Laid>>(Some: static path => new DuplicateKey(path), None: () => laid);

    internal static SKSize Preferred(Laid laid, SKSize offered) =>
        new(Basis(laid, Orientation.Horizontal, offered.Width), Basis(laid, Orientation.Vertical, offered.Height));

    internal static float Inset(Laid laid) => laid.Node.Role.IsSome ? laid.Style.Metrics[Metric.Inset] : 0f;

    internal static SKRect Content(Laid laid) =>
        Inset(laid) switch {
            var inset => SKRect.Create(inset, inset, float.Max(0f, laid.Frame.Width - (2f * inset)), float.Max(0f, laid.Frame.Height - (2f * inset))),
        };

    internal static Option<(PlotPlane Plane, SKMatrix Frame)> Plotted(Laid laid) => laid.Node is Node.Plot plot ? Some((plot.Plane, plot.Plane.Frame(Content(laid)))) : None;

    internal static Option<Node.Pin> Pinned(Node node) => node is Node.Pin pin ? Some(pin) : node is Node.Styled styled ? Pinned(styled.Content) : None;

    internal static (float Least, float Most) Travel(Node.Split split, float available) => (Sized(split.First, split.Axis).Min, available - Sized(split.Second, split.Axis).Min);

    internal static float Pane(Laid laid, Node.Split split) => split.Axis.Of(laid.Children[0].Frame.Size);

    internal static (float Content, float Viewport) Scrollable(Laid scroll, Laid content, Orientation axis) => (axis.Of(content.Frame.Size), axis.Of(scroll.Frame.Size) - (2f * Inset(scroll)));

    internal static float Scrolling(float offset, float total, float viewport) => float.Clamp(offset, 0f, float.Max(0f, total - viewport));

    internal static float Rescaled(float value, float span, float onto) => span > 0f ? value * onto / span : 0f;

    private static Sizing Sized(Node node, Orientation along) => node is Node.Styled styled ? Sized(styled.Content, along) : along.Sized(node);

    private static bool Reused(Laid laid, HashMap<Seq<Key>, Laid> prior) => prior.Find(laid.Path).Exists(held => ReferenceEquals(held, laid));

    private static Seq<Laid> Members(Laid laid) => laid is Laid.Choice choice ? choice.Alternatives : laid.Children;

    private static Seq<Seq<Key>> Repeated(Laid laid, HashMap<Seq<Key>, Laid> prior) =>
        Reused(laid, prior)
            ? []
            : Duplicates(laid.Children).Concat(Members(laid).Bind(member => Repeated(member, prior)));

    private static Seq<Seq<Key>> Duplicates(Seq<Laid> members) =>
        members.Fold((Seen: LanguageExt.HashSet<Seq<Key>>.Empty, Repeated: Seq<Seq<Key>>()), static (held, member) => held.Seen.Contains(member.Path)
                ? (held.Seen, held.Repeated.Add(member.Path))
                : (held.Seen.Add(member.Path), held.Repeated))
            .Repeated;

    // --- [BUILD]
    private static (Laid Laid, HashMap<Style, TextStyle> Texts) Built(Cascade cascade, Seq<Key> path, Node node, Option<Laid> held, HashMap<Seq<Key>, Laid> prior, HashMap<Style, TextStyle> texts) =>
        held.Filter(kept => kept.Node.Equals(node) && kept.Cascade.Equals(cascade)).Match(
            Some: kept => (kept, texts),
            None: () => Fresh(cascade, path, node, held, prior, texts));

    private static Option<Laid> Held(HashMap<Seq<Key>, Laid> prior, Option<Laid> parent, Seq<Key> path) =>
        prior.Find(path) | parent.Filter(held => held is Laid.Choice || !Reused(held, prior)).Bind(held => Members(held).Find(member => member.Path == path));

    private static (Laid Laid, HashMap<Style, TextStyle> Texts) Fresh(Cascade cascade, Seq<Key> path, Node node, Option<Laid> prior, HashMap<Seq<Key>, Laid> index, HashMap<Style, TextStyle> texts) {
        (Style style, Cascade inner) = Styles.Enter(cascade, node);
        (Seq<Laid> members, HashMap<Style, TextStyle> held) = Members(node).Fold(
            (Laid: Seq<Laid>(), Texts: texts),
            (state, member) => path.Add(member.Segment) switch {
                var at => Built(inner, at, member.Node, Held(index, prior, at), index, state.Texts) switch {
                    var (laid, next) => (state.Laid.Add(laid), next),
                },
            });
        (Laid built, HashMap<Style, TextStyle> kept) = node is Node.Text text
            ? held.Find(style).IfNone(() => Typesetting.Resolve(cascade.Theme, style)) switch {
                var face => ((Laid)new Laid.Text(Typesetting.Set(text, style, face, float.PositiveInfinity), face) { Path = path, Node = node, Cascade = cascade, Style = style }, held.TryAdd(style, face)),
            }
            : (node is Node.Choice
                ? new Laid.Choice(members) { Path = path, Node = node, Cascade = cascade, Style = style }
                : new Laid.Box { Path = path, Node = node, Cascade = cascade, Style = style, Children = members }, held);
        return (built with { Fit = new SKSize(Fitted(built, Orientation.Horizontal), 0f) }, kept);
    }

    private static Seq<(Key Segment, Node Node)> Members(Node node) =>
        node.SwitchPartially(
            @default: static _ => Seq<(Key, Node)>(),
            flex: static flex => Segments(flex.Children),
            grid: static grid => Segments(grid.Rows.Flatten()),
            wrap: static wrap => Segments(wrap.Children),
            overlay: static overlay => Segments(overlay.Children),
            choice: static choice => choice.Alternatives.Map(static alternative => (alternative.Key.IfNone(static () => new Key.Ordinal(0)), alternative)),
            scroll: static scroll => Seq(
                (scroll.Content.Key.IfNone(static () => new Key.Ordinal(0)), scroll.Content),
                (new Key.Thumb(), new Node.Flex(scroll.Axis, []) { Key = new Key.Thumb(), Role = Role.Scrollbar })),
            split: static split => Seq(
                (split.First.Key.IfNone(static () => new Key.Ordinal(0)), split.First),
                (new Key.Divider(), new Node.Flex(split.Axis, []) { Key = new Key.Divider(), Role = Role.Divider }),
                (split.Second.Key.IfNone(static () => new Key.Ordinal(1)), split.Second)),
            plot: static plot => Segments(plot.Children),
            pin: static pin => Segments(pin.Content.ToSeq()),
            styled: static styled => Segments(Seq(styled.Content)));

    private static Seq<(Key Segment, Node Node)> Segments(Seq<Node> nodes) => nodes.Map(static (node, index) => (node.Key.IfNone(() => new Key.Ordinal((uint)index)), node));

    // --- [FIT]
    private static float Fitted(Laid laid, Orientation along) {
        float content = laid.Node.Switch(
            (Laid: laid, Along: along),
            flex: static (held, flex) => Summed(held.Laid.Children, held.Along, flex.Axis == held.Along, Gap(held.Laid)),
            grid: static (held, grid) => Spanned(held.Laid, grid, held.Along),
            wrap: static (held, wrap) => Spanned(held.Laid, Wrapped(held.Laid, wrap), held.Along),
            overlay: static (held, _) => Widest(Grounded(held.Laid), held.Along),
            choice: static (held, choice) => Widest((held.Along == choice.Axis || held.Along == Orientation.Horizontal ? Members(held.Laid) : held.Laid.Children).Take(1), held.Along),
            scroll: static (held, _) => Widest(held.Laid.Children.Take(1), held.Along),
            split: static (held, split) => Summed(held.Laid.Children, held.Along, split.Axis == held.Along, 0f),
            plot: static (held, _) => Widest(Grounded(held.Laid), held.Along),
            pin: static (held, _) => Widest(held.Laid.Children, held.Along),
            mark: static (_, _) => 0f,
            text: static (held, _) => held.Laid is Laid.Text text ? MathF.Ceiling(held.Along.Of(text.Typeset.Size)) : 0f,
            glyph: static (held, _) => held.Laid.Style.Metrics[Metric.Icon],
            image: static (held, image) => held.Along.Of(new SKSize(image.Source.Width, image.Source.Height)),
            slot: static (_, _) => 0f,
            styled: static (held, _) => Widest(held.Laid.Children, held.Along));
        float fit = float.Max(content + (2f * Inset(laid)), laid.Node.Role.IsSome ? laid.Style.Metrics[Metric.Extent] : 0f);
        return along == Orientation.Horizontal
            ? fit
            : laid.Node.Height.Switch((Laid: laid, Fit: fit), sizing: static (state, _) => state.Fit, ratio: static (state, ratio) => ratio.PerWidth * state.Laid.Frame.Width);
    }

    private static float Spanned(Laid laid, Node.Grid grid, Orientation along) =>
        Tracks(laid, grid, along, float.PositiveInfinity) switch {
            var tracks => Enumerable.Sum(tracks) + (Gap(laid) * float.Max(0, tracks.Count - 1)),
        };

    private static Seq<Laid> Grounded(Laid laid) => laid.Children.Filter(static child => Pinned(child.Node).IsNone);

    private static float Summed(Seq<Laid> children, Orientation along, bool sum, float gap) =>
        sum ? Enumerable.Sum(children.Map(child => Basis(child, along, float.PositiveInfinity))) + (gap * float.Max(0, children.Count - 1)) : Widest(children, along);

    private static float Widest(Seq<Laid> children, Orientation along) => children.Map(child => Basis(child, along, float.PositiveInfinity)).Fold(0f, float.Max);

    private static float Basis(Laid laid, Orientation along, float space) => Based(Sized(laid.Node, along), along.Of(laid.Fit), space);

    private static float Based(Sizing sizing, float fit, float space) =>
        sizing.Clamped(sizing.Switch(
            (Fit: fit, Space: space),
            @fixed: static (_, held) => held.Extent,
            fit: static (state, _) => state.Fit,
            grow: static (state, _) => state.Fit,
            percent: static (state, held) => float.IsFinite(state.Space) ? held.Fraction * state.Space : state.Fit));

    private static float Cross(Laid laid, Orientation along, float space) =>
        Sized(laid.Node, along) switch {
            var sizing => sizing.Clamped(sizing is Sizing.Grow ? space : float.Min(Basis(laid, along, space), space)),
        };

    private static float Gap(Laid laid) => laid.Style.Metrics[Metric.Gap];

    // --- [PLACE]
    private static Laid Placed(Laid laid, Orientation along, float start, float extent, HashMap<Seq<Key>, Laid> prior, HashMap<Seq<Key>, float> adjusted) {
        if (Reused(laid, prior) && along.Of(laid.Frame.Size).CompareTo(extent) is 0) return Moved(laid, along, start);
        float inset = Inset(laid);
        Laid framed = laid with { Frame = along.Spanned(laid.Frame, start, extent) };
        Room room = new(framed, along, inset, float.Max(0f, extent - (2f * inset)), prior, adjusted);
        Laid arranged = framed is Laid.Choice { Node: Node.Choice { Axis: var axis } } choice && axis != along && along == Orientation.Horizontal
            ? choice with { Alternatives = choice.Alternatives.Map(alternative => Filled(room, alternative)) }
            : framed with { Children = Arranged(room) };
        return along == Orientation.Horizontal
            ? (arranged is Laid.Text { Typeset: Typeset.Block, Node: Node.Text block } wrapped
                ? wrapped with {
                    Typeset = prior.Find(wrapped.Path)
                        .Bind(static held => Optional(held as Laid.Text))
                        .Filter(held => held.Node.Equals(wrapped.Node) && held.Style.Equals(wrapped.Style) && held.Frame.Width.CompareTo(wrapped.Frame.Width) is 0)
                        .Map(static held => held.Typeset)
                        .IfNone(() => Typesetting.Set(block, wrapped.Style, wrapped.TextStyle, room.Space)),
                }
                : arranged) switch {
                    var set => set with { Fit = new SKSize(set.Fit.Width, Fitted(set, Orientation.Vertical)) },
                }
            : arranged is Laid.Text { Node: Node.Text content } text ? text with { Origin = Origin(text, content, inset) } : arranged;
    }

    private static SKPoint Origin(Laid.Text text, Node.Text content, float inset) =>
        new(inset + (text.Typeset is Typeset.Run run
                ? (text.Frame.Width - (2f * inset) - run.Size.Width) * content.Align switch {
                    Topten.RichTextKit.TextAlignment.Right => 1f,
                    Topten.RichTextKit.TextAlignment.Center => 0.5f,
                    Topten.RichTextKit.TextAlignment.Auto or Topten.RichTextKit.TextAlignment.Left => 0f,
                }
                : 0f),
            inset + float.Max(0f, (text.Frame.Height - (2f * inset) - text.Typeset.Size.Height) / 2f));

    private static Laid Moved(Laid laid, Orientation along, float start) =>
        along.Of(laid.Frame.Location).CompareTo(start) is 0
            ? laid
            : laid with { Frame = along.Spanned(laid.Frame, start, along.Of(laid.Frame.Size)) };

    private static Laid Filled(Room room, Laid child) => Placed(child, room.Along, room.Inset, room.Space, room.Prior, room.Adjusted);

    private static Seq<Laid> Arranged(Room room) =>
        room.Laid.Node.SwitchPartially(
            room,
            @default: static (held, _) => held.Laid.Children.Map(child => Filled(held, child)),
            flex: static (held, flex) => flex.Axis == held.Along ? Along(held, flex.Align) : Across(held, flex.Align),
            grid: static (held, grid) => Gridded(held, grid),
            wrap: static (held, wrap) => Gridded(held, Wrapped(held.Laid, wrap)),
            overlay: static (held, _) => Content(held.Laid) switch {
                var box => Pins(held, SKMatrix.CreateScaleTranslation(box.Width, box.Height, box.Left, box.Top), static at => at),
            },
            choice: static (held, choice) => held.Along == choice.Axis
                ? Members(held.Laid) switch {
                    var alternatives => (alternatives.Find(alternative => Basis(alternative, held.Along, float.PositiveInfinity) <= held.Space) | alternatives.Last).ToSeq().Map(chosen => Filled(held, chosen)),
                }
                : held.Laid.Children.Map(child => Filled(held, child)),
            scroll: static (held, scroll) => Scrolled(held, scroll),
            split: static (held, split) => split.Axis == held.Along ? Panes(held, split) : held.Laid.Children.Map(child => Filled(held, child)),
            plot: static (held, plot) => Pins(held, plot.Plane.Frame(Content(held.Laid)), plot.Plane.Unit)
                .Fold(Seq<Laid>(), static (kept, child) => Label(child) && kept.Exists(prior => Label(prior) && prior.Frame.IntersectsWith(child.Frame)) ? kept : kept.Add(child)));

    private static bool Label(Laid laid) => laid.Node.Key.IsNone && Pinned(laid.Node).Exists(static pin => pin.Key.IsNone);

    private static Seq<Laid> Along(Room room, Vector2 align) {
        Seq<Laid> children = room.Laid.Children;
        float gap = Gap(room.Laid);
        float gaps = gap * float.Max(0, children.Count - 1);
        Seq<float> extents = Distribute(children.Map(child => (Sized(child.Node, room.Along), Basis(child, room.Along, room.Space))), room.Space - gaps);
        float free = room.Space - gaps - Enumerable.Sum(extents);
        Seq<float> starts = extents.Scan(room.Inset + (free > 0f ? free * room.Along.Of(align) : 0f), (start, extent) => start + extent + gap);
        return children.Zip(extents.Zip(starts)).Map(placed => Placed(placed.First, room.Along, placed.Second.Second, placed.Second.First, room.Prior, room.Adjusted));
    }

    private static Seq<Laid> Across(Room room, Vector2 align) => room.Laid.Children.Map(child => Aligned(room, child, room.Inset, room.Space, room.Along.Of(align)));

    private static Laid Aligned(Room room, Laid child, float start, float space, float align) =>
        Cross(child, room.Along, space) switch {
            var extent => Placed(child, room.Along, start + ((space - extent) * align), extent, room.Prior, room.Adjusted),
        };

    private static Seq<Laid> Pins(Room room, SKMatrix frame, Func<Vector2, SKPoint> square) =>
        room.Laid.Children.Map(child => Pinned(child.Node).Match(
            Some: pin => room.Along == Orientation.Horizontal
                ? Aligned(room, child, child.Frame.Left, room.Space, 0f)
                : Pinned(room, child, pin, value => frame.MapPoint(square(value))),
            None: () => Filled(room, child)));

    private static Laid Pinned(Room room, Laid child, Node.Pin pin, Func<Vector2, SKPoint> at) =>
        pin.Span.Match(
            Some: span => (at(pin.At), at(pin.At + span)) switch {
                var (from, to) => new SKRect(from.X, from.Y, to.X, to.Y).Standardized switch {
                    var rect => Placed(Placed(child, Orientation.Horizontal, rect.Left, rect.Width, room.Prior, room.Adjusted), Orientation.Vertical, rect.Top, rect.Height, room.Prior, room.Adjusted),
                },
            },
            None: () => (At: at(pin.At), Tall: Cross(child, Orientation.Vertical, room.Space)) switch {
                var (point, tall) => Placed(
                    Moved(child, Orientation.Horizontal, Sized(child.Node, Orientation.Horizontal) is Sizing.Grow ? room.Inset : point.X - (pin.Align.X * child.Frame.Width)),
                    Orientation.Vertical, Sized(child.Node, Orientation.Vertical) is Sizing.Grow ? room.Inset : point.Y - (pin.Align.Y * tall), tall, room.Prior, room.Adjusted),
            });

    private static Seq<Laid> Gridded(Room room, Node.Grid grid) {
        float gap = Gap(room.Laid);
        Seq<float> tracks = room.Along == Orientation.Horizontal
            ? Distribute(Columns(grid).Zip(Tracks(room.Laid, grid, room.Along, room.Space)), room.Space - (gap * float.Max(0, Columns(grid).Count - 1)))
            : Tracks(room.Laid, grid, room.Along, room.Space);
        Seq<float> starts = tracks.Scan(room.Inset, (start, extent) => start + extent + gap);
        float end = starts.Zip(tracks).Last.Map(static last => last.First + last.Second).IfNone(room.Inset);
        return Cells(room.Laid, grid).Map(cell => (room.Along == Orientation.Horizontal ? (Track: cell.Column, cell.Spans) : (Track: cell.Row, Spans: false)) switch {
            var (track, spans) => Aligned(room, cell.Laid, starts[track], spans ? end - starts[track] : tracks[track], room.Along.Of(grid.Align)),
        });
    }

    private static Node.Grid Wrapped(Laid laid, Node.Wrap wrap) =>
        (Space: Content(laid).Width, Gap: Gap(laid)) switch {
            var (space, gap) => (int)float.Clamp(MathF.Floor((space + gap) / float.Max(Based(wrap.Column, Widest(laid.Children, Orientation.Horizontal), space) + gap, float.Epsilon)), 1f, float.Max(1f, wrap.Children.Count)) switch {
                var columns => new Node.Grid(LanguageExt.Seq.repeat(wrap.Column, columns), toSeq(wrap.Children.Chunk(columns)).Map(static row => toSeq(row))) { Align = wrap.Align },
            },
        };

    private static Seq<Sizing> Columns(Node.Grid grid) =>
        toSeq(Range(0, int.Max(grid.Columns.Count, grid.Rows.Map(static row => row.Count).Fold(0, int.Max)))).Map(column => grid.Columns.At(column).IfNone(Sizing.Fit.Content));

    private static Seq<(int Row, int Column, bool Spans, Laid Laid)> Cells(Laid laid, Node.Grid grid) =>
        from row in grid.Rows.Zip(grid.Rows.Scan(0, static (start, cells) => start + cells.Count)).Map(static (pair, index) => (Cells: pair.First, Start: pair.Second, Index: index))
        from column in row.Cells.Map(static (_, column) => column)
        select (row.Index, column, column == row.Cells.Count - 1 && row.Cells.Count < Columns(grid).Count, laid.Children[row.Start + column]);

    private static Seq<float> Tracks(Laid laid, Node.Grid grid, Orientation along, float space) =>
        Cells(laid, grid) switch {
            var cells => along == Orientation.Horizontal
                ? Columns(grid).Map((sizing, column) => Based(sizing, Widest(cells.Filter(cell => cell.Column == column && !cell.Spans).Map(static cell => cell.Laid), along), space))
                : grid.Rows.Map((_, row) => Widest(cells.Filter(cell => cell.Row == row).Map(static cell => cell.Laid), along)),
        };

    private static Seq<Laid> Scrolled(Room room, Node.Scroll scroll) {
        Laid content = room.Laid.Children[0];
        Laid thumb = room.Laid.Children[1];
        float least = Basis(thumb, room.Along, float.PositiveInfinity);
        if (scroll.Axis != room.Along) return Seq(Filled(room, content), Placed(thumb, room.Along, room.Inset + room.Space - least, least, room.Prior, room.Adjusted));
        Laid moved = Placed(content, room.Along, room.Inset, float.Max(Basis(content, room.Along, float.PositiveInfinity), room.Space), room.Prior, room.Adjusted);
        (float total, float viewport) = Scrollable(room.Laid, moved, room.Along);
        float length = total > viewport ? float.Min(viewport, float.Max(least, viewport * viewport / total)) : 0f;
        float offset = Scrolling(room.Adjusted.Find(room.Laid.Path).IfNone(0f), total, viewport);
        return Seq(moved, Placed(thumb, room.Along, room.Inset + Rescaled(offset, total - viewport, viewport - length), length, room.Prior, room.Adjusted));
    }

    private static Seq<Laid> Panes(Room room, Node.Split split) {
        Laid first = room.Laid.Children[0];
        Laid divider = room.Laid.Children[1];
        Laid second = room.Laid.Children[2];
        float rule = Basis(divider, room.Along, float.PositiveInfinity);
        float available = float.Max(0f, room.Space - rule);
        (float least, float most) = Travel(split, available);
        float extent = float.Max(least, float.Min(most, room.Adjusted.Find(room.Laid.Path).IfNone(() => Basis(first, room.Along, available))));
        return Seq(
            Placed(first, room.Along, room.Inset, extent, room.Prior, room.Adjusted),
            Placed(divider, room.Along, room.Inset + extent, rule, room.Prior, room.Adjusted),
            Placed(second, room.Along, room.Inset + extent + rule, float.Max(0f, available - extent), room.Prior, room.Adjusted));
    }

    // --- [DISTRIBUTE]
    private static Seq<float> Distribute(Seq<(Sizing Sizing, float Basis)> items, float space) {
        Seq<float> bases = items.Map(static item => item.Basis);
        float free = space - Enumerable.Sum(bases);
        if (!float.IsFinite(space)) return bases;
        if (free >= 0f) return Shared(items, bases, free);
        Seq<float> grown = Shrunk(items, bases, -free, static sizing => sizing is Sizing.Grow);
        return Shrunk(items, grown, Enumerable.Sum(grown) - space, static sizing => sizing is Sizing.Fit);
    }

    private static Seq<float> Shrunk(Seq<(Sizing Sizing, float Basis)> items, Seq<float> extents, float deficit, Func<Sizing, bool> yields) =>
        items.Zip(extents).Map(pair => yields(pair.First.Sizing) ? float.Max(0f, pair.Second - pair.First.Sizing.Min) : 0f) switch {
            var room => Enumerable.Sum(room) switch {
                var total when total > 0f && deficit > 0f => extents.Zip(room).Map(pair => pair.First - (pair.Second * float.Min(deficit, total) / total)),
                _ => extents,
            },
        };

    private static Seq<float> Shared(Seq<(Sizing Sizing, float Basis)> items, Seq<float> extents, float free) {
        Seq<float> open = items.Zip(extents).Map(static pair => pair.First.Sizing is Sizing.Grow grow && pair.Second < grow.Max ? grow.Weight : 0f);
        float weight = Enumerable.Sum(open);
        if (weight <= 0f || free <= 0f) return extents;
        Seq<float> proposed = extents.Zip(open).Map(pair => pair.First + (free * pair.Second / weight));
        Seq<float> capped = items.Zip(proposed).Map(static pair => float.Min(pair.First.Sizing.Max, pair.Second));
        float spill = Enumerable.Sum(proposed) - Enumerable.Sum(capped);
        return spill > 0f ? Shared(items, capped, spill) : capped;
    }
}
