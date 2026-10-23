using System.Globalization;
using Rasm.UI.Geometry;
using Rasm.UI.Glyphs;
using Rasm.UI.Scenes;
using Rasm.UI.Styling;
using Rasm.UI.Text;
using SkiaSharp;

namespace Rasm.UI.Layout;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Sizing {
    public float Min { get; init; }
    public float Max { get; init; } = float.PositiveInfinity;

    public static Sizing Content { get; } = new Fit();
    public static Sizing Fill { get; } = new Grow(1f);

    public float Clamped(float value) => float.Clamp(value, Min, Max);

    public sealed record Fixed(float Value) : Sizing;
    public sealed record Fit : Sizing;
    public sealed record Grow(float Weight) : Sizing;
    public sealed record Percent(float Fraction) : Sizing;
}

public sealed record Cascade(Seq<Appearance> Chain, GlyphSet Glyphs, Family Family, Ink Ink, TypeRole Type) {
    public static Cascade Root { get; } = new([], GlyphSet.Empty, Family.Chrome, new Ink.Themed(ColorRole.Text, 1f), TypeRole.Body);
}

public sealed record Signature(Option<Node> Leaf, SKRect Frame, Option<Look> Look, GlyphSet Glyphs, Family Family, Ink Ink, TypeRole Type, Seq<SKRect> Children, Seq<Signature> Rasterized);

public sealed record Laid(string Id, Node Node, SKRect Frame, SKRect Content, Option<Look> Look, Cascade Cascade, Seq<Laid> Children) {
    public Signature Signature =>
        new(Node.Leaf ? Some(Node) : None, Frame, Look, Cascade.Glyphs, Cascade.Family, Cascade.Ink, Cascade.Type, Children.Map(static child => child.Frame),
            Node is Node.Layer ? Flattened.Tail.Map(static held => held.Signature) : []);

    public Seq<Laid> Flattened => this.Cons(Children.Bind(static child => child.Flattened));

    public SKRect Bounds(Theme theme) =>
        Look.Match(
            Some: look => (look.Elevation.Extent(theme.Metrics) + look.Border.Map(static border => border.Width).IfNone(0f)) switch {
                var reach => SKRect.Inflate(Frame, reach, reach),
            },
            None: () => Frame);

    public Seq<Laid> Chain(SKPoint at) =>
        Node is Node.Scroll && !Frame.Contains(at)
            ? []
            : Children.Rev().Map(child => child.Chain(at)).Find(static chain => !chain.IsEmpty).Match(
                Some: chain => chain.Add(this),
                None: () => Frame.Contains(at) ? Seq(this) : []);

    public Option<Seq<Laid>> Path(string id) =>
        string.Equals(Id, id, StringComparison.Ordinal) ? Some(Seq(this)) : Children.Choose(child => child.Path(id)).Head.Map(path => path.Add(this));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Layouts {
    // --- [ARRANGE]
    public static Laid Arrange(Theme theme, Node root, SKSize size) => Place(theme, Cascade.Root, Identity("", root, 0), root, SKRect.Create(size));

    public static (Option<Look> Look, Cascade Cascade) Enter(Cascade cascade, Node node) =>
        (node is Node.Styled styled
            ? cascade with {
                Chain = styled.Appearance.Map(appearance => appearance.Cons(cascade.Chain)).IfNone(cascade.Chain),
                Glyphs = styled.Glyphs.Map(glyphs => glyphs.Over(cascade.Glyphs)).IfNone(cascade.Glyphs),
            }
            : cascade) switch {
                var inner => node.Role.Match(
                    Some: role => Appearances.Resolve(inner.Chain, role, node.State) switch {
                        var look => (Some(look), inner with { Family = role.Family, Ink = look.Ink, Type = look.Type }),
                    },
                    None: () => (Option<Look>.None, inner)),
            };

    public static float Divider(Theme theme) => Space.Small.Of(theme.Metrics);

    private static Laid Place(Theme theme, Cascade cascade, string id, Node node, SKRect frame) =>
        (Enter(cascade, node), Inner(theme, node, frame)) switch {
            var ((look, inner), content) => new Laid(id, node, frame, content, look, inner,
                Arranged(theme, inner, node, content).Map((placed, index) => Place(theme, inner, Identity(id, placed.Node, index), placed.Node, placed.Frame))),
        };

    private static string Identity(string parent, Node node, int index) =>
        $"{parent}/{node.Key.IfNone(() => index.ToString(CultureInfo.InvariantCulture))}";

    private static SKRect Inner(Theme theme, Node node, SKRect frame) =>
        SKRect.Inflate(frame, -node.Padding.Horizontal.Of(theme.Metrics), -node.Padding.Vertical.Of(theme.Metrics)) switch {
            var inner => inner.Width < 0f || inner.Height < 0f ? SKRect.Create(frame.MidX, frame.MidY, 0f, 0f) : inner,
        };

    private static Seq<(Node Node, SKRect Frame)> Arranged(Theme theme, Cascade inner, Node node, SKRect box) =>
        node.Switch(
            (Theme: theme, Cascade: inner, Box: box),
            flex: static (held, flex) => flex.Wraps ? Flowed(held.Theme, held.Cascade, flex, held.Box) : Stacked(held.Theme, held.Cascade, flex, held.Box),
            grid: static (held, grid) => Gridded(held.Theme, held.Cascade, grid, held.Box),
            overlay: static (held, overlay) => overlay.Children.Map(child => (child, Within(held.Theme, held.Cascade, child, held.Box, overlay.Align))),
            scroll: static (held, scroll) => Seq((scroll.Content, Scrolled(held.Theme, held.Cascade, scroll, held.Box))),
            split: static (held, split) => Panes(held.Theme, split, held.Box) switch {
                var (first, second) => Seq((split.First, first), (split.Second, second)),
            },
            dock: static (held, dock) => Docked(held.Theme, held.Cascade, dock, held.Box),
            text: static (_, _) => Seq<(Node, SKRect)>(),
            glyph: static (_, _) => Seq<(Node, SKRect)>(),
            shape: static (_, _) => Seq<(Node, SKRect)>(),
            image: static (_, _) => Seq<(Node, SKRect)>(),
            layer: static (held, layer) => Seq((layer.Content, held.Box)),
            widget: static (_, _) => Seq<(Node, SKRect)>(),
            slot: static (_, _) => Seq<(Node, SKRect)>(),
            styled: static (held, styled) => Seq((styled.Content, held.Box)));

    // --- [MEASURE]
    public static float FitWidth(Theme theme, Cascade cascade, Node node, float max) =>
        (Enter(cascade, node).Cascade, 2f * node.Padding.Horizontal.Of(theme.Metrics)) switch {
            var (inner, pad) => pad + node.Switch(
                (Theme: theme, Cascade: inner, Max: max - pad),
                flex: static (held, flex) => flex.Axis is Orientation.Horizontal && !flex.Wraps
                    ? Total(Widths(held.Theme, held.Cascade, flex.Children, held.Max)) + Gaps(held.Theme, flex.Gap, flex.Children.Count)
                    : Widths(held.Theme, held.Cascade, flex.Children, held.Max).Fold(0f, float.Max),
                grid: static (held, grid) => Total(Columns(held.Theme, held.Cascade, grid, held.Max)) + Gaps(held.Theme, grid.Gap, grid.Columns.Count),
                overlay: static (held, overlay) => Widths(held.Theme, held.Cascade, overlay.Children, held.Max).Fold(0f, float.Max),
                scroll: static (held, scroll) => Width(held.Theme, held.Cascade, scroll.Content, held.Max),
                split: static (held, split) => SplitWidth(held.Theme, held.Cascade, split, held.Max),
                dock: static (held, dock) => DockWidth(held.Theme, held.Cascade, dock, held.Max),
                text: static (held, text) => MathF.Ceiling(Paragraphs.Measure(held.Theme, held.Cascade, text, Bounded(held.Max)).Width),
                glyph: static (held, _) => held.Theme.Metrics.Icon,
                shape: static (held, _) => held.Theme.Metrics.Stroke,
                image: static (held, image) => image.Fit is ImageFit.Actual ? image.Source.Width : float.Min(image.Source.Width, held.Max),
                layer: static (held, layer) => FitWidth(held.Theme, held.Cascade, layer.Content, held.Max),
                widget: static (held, _) => float.IsFinite(held.Max) ? held.Max : held.Theme.Metrics.Hit,
                slot: static (held, _) => held.Theme.Metrics.Hit,
                styled: static (held, styled) => FitWidth(held.Theme, held.Cascade, styled.Content, held.Max)),
        };

    public static float FitHeight(Theme theme, Cascade cascade, Node node, float width) =>
        (Enter(cascade, node).Cascade, 2f * node.Padding.Vertical.Of(theme.Metrics), SKRect.Create(float.Max(0f, width - (2f * node.Padding.Horizontal.Of(theme.Metrics))), float.PositiveInfinity)) switch {
            var (inner, pad, box) => pad + node.Switch(
                (Theme: theme, Cascade: inner, Box: box),
                flex: static (held, flex) => Extent(Arranged(held.Theme, held.Cascade, flex, held.Box), held.Box),
                grid: static (held, grid) => Extent(Gridded(held.Theme, held.Cascade, grid, held.Box), held.Box),
                overlay: static (held, overlay) => overlay.Children.Map(child => Height(held.Theme, held.Cascade, child, held.Box.Width)).Fold(0f, float.Max),
                scroll: static (held, scroll) => Height(held.Theme, held.Cascade, scroll.Content, held.Box.Width),
                split: static (held, split) => SplitHeight(held.Theme, held.Cascade, split, held.Box.Width),
                dock: static (held, dock) => DockHeight(held.Theme, held.Cascade, dock, held.Box.Width),
                text: static (held, text) => MathF.Ceiling(Paragraphs.Measure(held.Theme, held.Cascade, text, held.Box.Width).Height),
                glyph: static (held, _) => held.Theme.Metrics.Icon,
                shape: static (held, _) => held.Theme.Metrics.Stroke,
                image: static (held, image) => image.Fit is ImageFit.Actual ? image.Source.Height : held.Box.Width * image.Source.Height / image.Source.Width,
                layer: static (held, layer) => FitHeight(held.Theme, held.Cascade, layer.Content, held.Box.Width),
                widget: static (held, widget) => widget.Component.Height(held.Box.Width).IfNone(held.Theme.Metrics.Hit),
                slot: static (held, _) => held.Theme.Metrics.Hit,
                styled: static (held, styled) => FitHeight(held.Theme, held.Cascade, styled.Content, held.Box.Width)),
        };

    private static float SplitWidth(Theme theme, Cascade cascade, Node.Split split, float max) =>
        (Width(theme, cascade, split.First, max), Width(theme, cascade, split.Second, max)) switch {
            var (first, second) => split.Axis switch {
                Orientation.Horizontal => first + second + Divider(theme),
                Orientation.Vertical => float.Max(first, second),
            },
        };

    private static float SplitHeight(Theme theme, Cascade cascade, Node.Split split, float width) =>
        Panes(theme, split, SKRect.Create(width, 0f)) switch {
            var (first, second) => split.Axis switch {
                Orientation.Horizontal => float.Max(Height(theme, cascade, split.First, first.Width), Height(theme, cascade, split.Second, second.Width)),
                Orientation.Vertical => Height(theme, cascade, split.First, width) + Height(theme, cascade, split.Second, width) + Divider(theme),
            },
        };

    private static float DockWidth(Theme theme, Cascade cascade, Node.Dock dock, float max) =>
        Total(Sides(dock).Map(docked => Width(theme, cascade, docked.Node, max)))
        + Ends(dock).Map(docked => Width(theme, cascade, docked.Node, max)).Add(Width(theme, cascade, dock.Fill, max)).Fold(0f, float.Max);

    private static float DockHeight(Theme theme, Cascade cascade, Node.Dock dock, float width) =>
        Total(Sides(dock).Map(docked => Width(theme, cascade, docked.Node, width))) switch {
            var across => Total(Ends(dock).Map(docked => Height(theme, cascade, docked.Node, width)))
                + Sides(dock).Map(docked => Height(theme, cascade, docked.Node, Width(theme, cascade, docked.Node, width)))
                    .Add(Height(theme, cascade, dock.Fill, float.Max(0f, width - across))).Fold(0f, float.Max),
        };

    private static Seq<(Edge Edge, Node Node)> Sides(Node.Dock dock) => dock.Edges.Filter(static docked => docked.Edge is Edge.Left or Edge.Right);

    private static Seq<(Edge Edge, Node Node)> Ends(Node.Dock dock) => dock.Edges.Filter(static docked => docked.Edge is Edge.Top or Edge.Bottom);

    private static float Width(Theme theme, Cascade cascade, Node node, float max) => Basis(node.Width, max, () => FitWidth(theme, cascade, node, max));

    private static float Height(Theme theme, Cascade cascade, Node node, float width) => Basis(node.Height, float.PositiveInfinity, () => FitHeight(theme, cascade, node, width));

    private static Seq<float> Widths(Theme theme, Cascade cascade, Seq<Node> children, float max) => children.Map(child => Width(theme, cascade, child, max));

    private static float Gaps(Theme theme, Space gap, int count) => gap.Of(theme.Metrics) * float.Max(0, count - 1);

    private static float Extent(Seq<(Node Node, SKRect Frame)> placed, SKRect box) => placed.Map(held => held.Frame.Bottom - box.Top).Fold(0f, float.Max);

    private static float Bounded(float max) => float.IsFinite(max) ? float.Max(0f, max) : float.PositiveInfinity;

    // --- [DISTRIBUTE]
    private static Seq<float> Distribute(Seq<(Sizing Sizing, float Basis)> items, float available) =>
        (Free: available - Total(items.Map(static item => item.Basis)), Weight: Total(items.Map(static item => item.Sizing is Sizing.Grow grow ? grow.Weight : 0f))) switch {
            var share => items.Map(item => item.Sizing is Sizing.Grow grow && share.Weight > 0f && float.IsFinite(share.Free)
                ? grow.Clamped(item.Basis + (share.Free * grow.Weight / share.Weight))
                : item.Basis),
        };

    private static float Basis(Sizing sizing, float available, Func<float> fit) =>
        sizing.Clamped(sizing.Switch(
            (Available: available, Fit: fit),
            @fixed: static (_, @fixed) => @fixed.Value,
            fit: static (held, _) => held.Fit(),
            grow: static (held, _) => held.Fit(),
            percent: static (held, percent) => float.IsFinite(held.Available) ? percent.Fraction * held.Available : held.Fit()));

    private static float Cross(Sizing sizing, float available, Func<float> fit) =>
        sizing is Sizing.Grow && float.IsFinite(available) ? sizing.Clamped(available) : Basis(sizing, available, fit);

    private static SKRect Within(Theme theme, Cascade cascade, Node child, SKRect box, Alignment align) =>
        Cross(child.Width, box.Width, () => FitWidth(theme, cascade, child, box.Width)) switch {
            var width => Cross(child.Height, box.Height, () => FitHeight(theme, cascade, child, width)) switch {
                var height => SKRect.Create(box.Left + ((box.Width - width) * align.X), box.Top + (float.IsFinite(box.Height) ? (box.Height - height) * align.Y : 0f), width, height),
            },
        };

    // --- [CONTAINERS]
    private static Seq<(Node Node, SKRect Frame)> Stacked(Theme theme, Cascade cascade, Node.Flex flex, SKRect box) {
        float gap = flex.Gap.Of(theme.Metrics);
        float gaps = Gaps(theme, flex.Gap, flex.Children.Count);
        bool horizontal = flex.Axis is Orientation.Horizontal;
        Seq<float> widths = horizontal
            ? Distribute(flex.Children.Map(child => (child.Width, Width(theme, cascade, child, box.Width))), box.Width - gaps)
            : flex.Children.Map(child => Cross(child.Width, box.Width, () => FitWidth(theme, cascade, child, box.Width)));
        Seq<float> heights = horizontal
            ? flex.Children.Zip(widths).Map(pair => Cross(pair.First.Height, box.Height, () => FitHeight(theme, cascade, pair.First, pair.Second)))
            : Distribute(flex.Children.Zip(widths).Map(pair => (pair.First.Height, Basis(pair.First.Height, box.Height, () => FitHeight(theme, cascade, pair.First, pair.Second)))), box.Height - gaps);
        Seq<SKSize> sizes = widths.Zip(heights).Map(static pair => new SKSize(pair.First, pair.Second));
        float used = Total(sizes.Map(size => Main(size, horizontal))) + gaps;
        float free = float.Max(0f, (horizontal ? box.Width : box.Height) - used);
        return flex.Children.Zip(sizes).Fold(
            (Along: (horizontal ? box.Left : box.Top) + (float.IsFinite(free) ? free * (horizontal ? flex.Align.X : flex.Align.Y) : 0f), Placed: Seq<(Node, SKRect)>()),
            (state, pair) => (state.Along + (horizontal ? pair.Second.Width : pair.Second.Height) + gap,
                state.Placed.Add((pair.First, horizontal
                    ? SKRect.Create(state.Along, box.Top + Offset(box.Height, pair.Second.Height, flex.Align.Y), pair.Second.Width, pair.Second.Height)
                    : SKRect.Create(box.Left + Offset(box.Width, pair.Second.Width, flex.Align.X), state.Along, pair.Second.Width, pair.Second.Height))))).Placed;
    }

    private static Seq<(Node Node, SKRect Frame)> Flowed(Theme theme, Cascade cascade, Node.Flex flex, SKRect box) {
        float gap = flex.Gap.Of(theme.Metrics);
        bool horizontal = flex.Axis is Orientation.Horizontal;
        float limit = horizontal ? box.Width : box.Height;
        Seq<(Node Node, SKSize Size)> sized = flex.Children.Map(child => Width(theme, cascade, child, box.Width) switch {
            var width => (child, new SKSize(width, Height(theme, cascade, child, width))),
        });
        Seq<Seq<(Node Node, SKSize Size)>> lines = sized.Fold(Seq<Seq<(Node Node, SKSize Size)>>(), (built, item) =>
            built.Last.Filter(line => Total(line.Map(held => Main(held.Size, horizontal) + gap)) + Main(item.Size, horizontal) <= limit)
                .Match(Some: line => built.Init.Add(line.Add(item)), None: () => built.Add(Seq(item))));
        return lines.Fold((Across: horizontal ? box.Top : box.Left, Placed: Seq<(Node, SKRect)>()), (state, line) =>
            (state.Across + line.Map(held => Main(held.Size, !horizontal)).Fold(0f, float.Max) + gap,
                state.Placed.Concat(line.Fold((Along: horizontal ? box.Left : box.Top, Placed: Seq<(Node, SKRect)>()), (inner, held) =>
                    (inner.Along + Main(held.Size, horizontal) + gap, inner.Placed.Add((held.Node, horizontal
                        ? SKRect.Create(inner.Along, state.Across, held.Size.Width, held.Size.Height)
                        : SKRect.Create(state.Across, inner.Along, held.Size.Width, held.Size.Height))))).Placed))).Placed;
    }

    private static Seq<(Node Node, SKRect Frame)> Gridded(Theme theme, Cascade cascade, Node.Grid grid, SKRect box) {
        float gap = grid.Gap.Of(theme.Metrics);
        int count = int.Max(1, grid.Columns.Count);
        Seq<Seq<Node>> rows = toSeq(grid.Children.Chunk(count)).Map(static row => toSeq(row));
        Seq<float> widths = Distribute(Columns(theme, cascade, grid, box.Width).Zip(grid.Columns).Map(static pair => (pair.Second, pair.First)), box.Width - Gaps(theme, grid.Gap, count));
        Seq<Sizing> rowSizing = rows.Map((_, index) => grid.Rows.At(index).IfNone(Sizing.Content));
        Seq<float> heights = Distribute(rows.Zip(rowSizing).Map(pair => (pair.Second, Basis(pair.Second, box.Height,
            () => pair.First.Zip(widths).Map(cell => Height(theme, cascade, cell.First, cell.Second)).Fold(0f, float.Max)))), box.Height - Gaps(theme, grid.Gap, rows.Count));
        Seq<float> lefts = widths.Scan(box.Left, (left, width) => left + width + gap).Init;
        Seq<float> tops = heights.Scan(box.Top, (top, height) => top + height + gap).Init;
        return from row in rows.Zip(tops.Zip(heights))
               from cell in row.First.Zip(lefts.Zip(widths))
               select (cell.First, Within(theme, cascade, cell.First, SKRect.Create(cell.Second.First, row.Second.First, cell.Second.Second, row.Second.Second), Alignment.Start));
    }

    private static Seq<float> Columns(Theme theme, Cascade cascade, Node.Grid grid, float max) =>
        grid.Columns.Map((sizing, index) => Basis(sizing, max,
            () => grid.Children.Map(static (cell, at) => (Cell: cell, At: at)).Filter(held => held.At % grid.Columns.Count == index).Map(held => Width(theme, cascade, held.Cell, max)).Fold(0f, float.Max)));

    private static SKRect Scrolled(Theme theme, Cascade cascade, Node.Scroll scroll, SKRect box) =>
        scroll.Axis is Orientation.Vertical
            ? FitHeight(theme, cascade, scroll.Content, box.Width) switch {
                var height => SKRect.Create(box.Left, box.Top - float.Clamp(scroll.Offset, 0f, float.Max(0f, height - box.Height)), box.Width, float.Max(height, box.Height)),
            }
            : FitWidth(theme, cascade, scroll.Content, float.PositiveInfinity) switch {
                var width => SKRect.Create(box.Left - float.Clamp(scroll.Offset, 0f, float.Max(0f, width - box.Width)), box.Top, float.Max(width, box.Width), box.Height),
            };

    private static (SKRect First, SKRect Second) Panes(Theme theme, Node.Split split, SKRect box) =>
        (Divider(theme), float.Clamp(split.Fraction, 0f, 1f)) switch {
            var (divider, fraction) when split.Axis is Orientation.Horizontal => ((box.Width - divider) * fraction) switch {
                var first => (SKRect.Create(box.Left, box.Top, first, box.Height), new SKRect(box.Left + first + divider, box.Top, box.Right, box.Bottom)),
            },
            var (divider, fraction) => ((box.Height - divider) * fraction) switch {
                var first => (SKRect.Create(box.Left, box.Top, box.Width, first), new SKRect(box.Left, box.Top + first + divider, box.Right, box.Bottom)),
            },
        };

    private static Seq<(Node Node, SKRect Frame)> Docked(Theme theme, Cascade cascade, Node.Dock dock, SKRect box) =>
        dock.Edges.Fold((Remaining: box, Placed: Seq<(Node, SKRect)>()), (state, docked) => docked.Edge switch {
            Edge.Top or Edge.Bottom => Cross(docked.Node.Height, state.Remaining.Height, () => FitHeight(theme, cascade, docked.Node, state.Remaining.Width)) switch {
                var height when docked.Edge is Edge.Top => (new SKRect(state.Remaining.Left, state.Remaining.Top + height, state.Remaining.Right, state.Remaining.Bottom), state.Placed.Add((docked.Node, SKRect.Create(state.Remaining.Left, state.Remaining.Top, state.Remaining.Width, height)))),
                var height => (new SKRect(state.Remaining.Left, state.Remaining.Top, state.Remaining.Right, state.Remaining.Bottom - height), state.Placed.Add((docked.Node, new SKRect(state.Remaining.Left, state.Remaining.Bottom - height, state.Remaining.Right, state.Remaining.Bottom)))),
            },
            Edge.Left or Edge.Right => Cross(docked.Node.Width, state.Remaining.Width, () => FitWidth(theme, cascade, docked.Node, state.Remaining.Width)) switch {
                var width when docked.Edge is Edge.Left => (new SKRect(state.Remaining.Left + width, state.Remaining.Top, state.Remaining.Right, state.Remaining.Bottom), state.Placed.Add((docked.Node, SKRect.Create(state.Remaining.Left, state.Remaining.Top, width, state.Remaining.Height)))),
                var width => (new SKRect(state.Remaining.Left, state.Remaining.Top, state.Remaining.Right - width, state.Remaining.Bottom), state.Placed.Add((docked.Node, new SKRect(state.Remaining.Right - width, state.Remaining.Top, state.Remaining.Right, state.Remaining.Bottom)))),
            },
        }) switch {
            var (rest, placed) => placed.Add((dock.Fill, rest)),
        };

    private static float Main(SKSize size, bool horizontal) => horizontal ? size.Width : size.Height;

    private static float Total(Seq<float> values) => values.Fold(0f, static (sum, value) => sum + value);

    private static float Offset(float space, float size, float align) => float.IsFinite(space) ? (space - size) * align : 0f;
}
