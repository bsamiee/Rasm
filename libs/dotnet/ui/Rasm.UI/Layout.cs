using System.Globalization;
using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<int>(SkipIComparable = true)]
public sealed partial class Orientation {
    public static readonly Orientation Horizontal = new(0);
    public static readonly Orientation Vertical = new(1);

    public Orientation Cross => Map(horizontal: Vertical, vertical: Horizontal);

    public float Of(Vector2 value) => value[Key];

    public float Of(SKSize size) => Of(size.ToPoint());

    public Vector2 With(Vector2 value, float along) => value.WithElement(Key, along);

    public SKRect Spanned(SKRect frame, float start, float extent) => SKRect.Create(With(frame.Location, start), new SKSize(With(frame.Size.ToPoint(), extent)));
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Fraction : IMinMaxValue<Fraction> {
    public static Fraction MinValue { get; } = new(0f);
    public static Fraction MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Factor : IMinMaxValue<Factor> {
    public static Factor MinValue { get; } = new(float.BitIncrement(0f));
    public static Factor MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Aspect : IMinMaxValue<Aspect> {
    public static Aspect MinValue { get; } = new(float.BitIncrement(0f));
    public static Aspect MaxValue { get; } = new(float.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ComplexValueObject(AllowDefaultStructs = true, DefaultInstancePropertyName = "Start")]
public readonly partial struct UnitPoint {
    public static readonly UnitPoint Center = new(0.5f, 0.5f);

    public float X { get; }
    public float Y { get; }

    public float Of(Orientation along) => along.Of(new Vector2(X, Y));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float x, ref float y) =>
        validationError = ValidationResults.OutsideRange(x, Fraction.MinValue, Fraction.MaxValue) ?? ValidationResults.OutsideRange(y, Fraction.MinValue, Fraction.MaxValue);
}

[ComplexValueObject]
public sealed partial class Limits {
    public static Limits Open { get; } = Create(0f, float.PositiveInfinity);

    public float Min { get; }
    public float Max { get; }

    public float Clamped(float value) => float.Clamp(value, Min, Max);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float min, ref float max) =>
        validationError = float.IsFinite(min) && min >= 0f && max >= min
            ? null
            : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"limits ({min}, {max}) are not a non-negative ordered span"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Sizing {
    public Limits Limits { get; init; } = Limits.Open;

    internal float Growth => Switch(@fixed: static _ => 0f, fit: static _ => 0f, grow: static grow => (float)grow.Weight, percent: static _ => 0f);
    internal Option<int> Yields => Map(@fixed: Option<int>.None, fit: Some(1), grow: Some(0), percent: Some(1));
    internal bool Stretches => Map(@fixed: false, fit: false, grow: true, percent: false);
    internal Option<float> Definite => Switch(@fixed: static held => Some(held.Limits.Clamped(held.Extent)), fit: static _ => Option<float>.None, grow: static _ => Option<float>.None, percent: static _ => Option<float>.None);

    internal float Floor(float content) =>
        Switch(content, @fixed: static (_, held) => held.Limits.Min, fit: static (least, held) => float.Max(held.Limits.Min, least), grow: static (_, held) => held.Limits.Min, percent: static (_, held) => held.Limits.Min);

    public sealed record Fixed(float Extent) : Sizing;

    public sealed record Fit : Sizing {
        public static Fit Content { get; } = new();
    }

    public sealed record Grow(Factor Weight) : Sizing;

    public sealed record Percent(Fraction Fraction) : Sizing;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Tracks {
    public sealed record Explicit(Seq<Sizing> Sizes) : Tracks;

    public sealed record AutoFill(Sizing Size) : Tracks;
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record DuplicateKey(Seq<Key> Path) : Expected($"sibling key /{string.Join('/', Path.Map(static key => key.ToValue()))} repeats", 0);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Layouts {
    // --- [ARRANGE]
    private sealed record Pass(HashMap<Seq<Key>, Laid> Prior, HashMap<Seq<Key>, float> Adjusted);

    internal static Laid Arrange(Cascade root, Node node, SKSize size, HashMap<Seq<Key>, float> adjusted, HashMap<Seq<Key>, Laid> prior) =>
        new Pass(prior, adjusted) switch {
            var pass => Placed(pass, Placed(pass, Built(root, [], node, prior.Find([]), prior), Orientation.Horizontal, 0f, size.Width), Orientation.Vertical, 0f, size.Height),
        };

    internal static Fin<Laid> Unique(Laid laid, HashMap<Seq<Key>, Laid> prior) =>
        Repeated(laid, prior).Head.Match<Fin<Laid>>(Some: static path => new DuplicateKey(path), None: () => laid);

    internal static SKSize Preferred(Laid laid, SKSize offered, HashMap<Seq<Key>, float> adjusted) =>
        Placed(new Pass([], adjusted), laid, Orientation.Horizontal, 0f, float.Min(Basis(laid, Orientation.Horizontal, offered.Width), offered.Width)) switch {
            var wide => new SKSize(wide.Frame.Width, Basis(wide, Orientation.Vertical, offered.Height)),
        };

    private static bool Reused(Laid laid, HashMap<Seq<Key>, Laid> prior) => prior.Find(laid.Path).Exists(held => ReferenceEquals(held, laid));

    private static Seq<Seq<Key>> Repeated(Laid laid, HashMap<Seq<Key>, Laid> prior) =>
        Reused(laid, prior)
            ? []
            : Duplicates(laid.Children).Concat(laid.Members.Bind(member => Repeated(member, prior)));

    private static Seq<Seq<Key>> Duplicates(Seq<Laid> members) =>
        members.Fold((Seen: LanguageExt.HashSet<Seq<Key>>.Empty, Repeated: Seq<Seq<Key>>()), static (held, member) => held.Seen.Contains(member.Path)
                ? (held.Seen, held.Repeated.Add(member.Path))
                : (held.Seen.Add(member.Path), held.Repeated))
            .Repeated;

    // --- [QUERY]
    internal static Option<PlotFrame> Plotted(Laid laid) => laid.Node.Plotted.Map(plane => plane.In(laid.ContentBox));

    internal static bool IsHandle(Laid laid) => laid.Path.Last.Exists(static last => last.Map(named: false, ordinal: false, handle: true));

    internal static Option<Laid> Handle(Laid owner) => owner.Children.Find(IsHandle);

    internal static Option<(float Least, float Most, float Extent)> Travel(Laid split, Node.Split node) =>
        from first in Panes(split).Head
        let range = Bounded(split, node)
        select (range.Least, range.Most, node.Axis.Of(first.Frame.Size));

    internal static Option<(float Content, float Viewport)> Scrollable(Laid scroll, Node.Scroll node) =>
        Panes(scroll).Head.Map(content => (node.Axis.Of(content.Frame.Size), node.Axis.Of(scroll.ContentBox.Size)));

    internal static float Scrolling(float offset, float total, float viewport) => float.Clamp(offset, 0f, float.Max(0f, total - viewport));

    internal static float Rescaled(float value, float span, float onto) => span > 0f ? value * onto / span : 0f;

    internal static Seq<Key> Visible(Laid lazy, Node.Lazy node, float offset, float viewport) =>
        Lanes(lazy, node) switch {
            var lanes => (First: Line(lazy, node, lanes, offset - viewport), Last: Line(lazy, node, lanes, offset + (2f * viewport))) switch {
                var span => node.Items.Skip(span.First * lanes).Take((span.Last - span.First + 1) * lanes),
            },
        };

    internal static Option<SKRect> Slot(Laid lazy, Node.Lazy node, Key key) =>
        Selection.Position(node.Items, key).Map(index => node.Axis.Spanned(lazy.ContentBox, node.Axis.Of(lazy.ContentBox.Location) + (index / Lanes(lazy, node) * (node.Extent + Gap(lazy))), node.Extent));

    private static int Line(Laid lazy, Node.Lazy node, int lanes, float at) =>
        (int)float.Max(0f, float.Min(Lines(node, lanes) - 1, MathF.Floor((at - node.Axis.Of(lazy.ContentBox.Location)) / (node.Extent + Gap(lazy)))));

    private static int Lines(Node.Lazy lazy, int lanes) => (lazy.Items.Count + lanes - 1) / lanes;

    private static Option<Node> Inner(Node node) =>
        node.Switch(
            box: static _ => Option<Node>.None,
            flex: static _ => Option<Node>.None,
            grid: static _ => Option<Node>.None,
            lazy: static _ => Option<Node>.None,
            overlay: static overlay => Some(overlay.Base),
            choice: static _ => Option<Node>.None,
            scroll: static _ => Option<Node>.None,
            split: static _ => Option<Node>.None,
            plot: static _ => Option<Node>.None,
            pin: static pin => Some(pin.Content),
            mark: static _ => Option<Node>.None,
            text: static _ => Option<Node>.None,
            glyph: static _ => Option<Node>.None,
            image: static _ => Option<Node>.None,
            slot: static _ => Option<Node>.None,
            styled: static styled => Some(styled.Content));

    private static Sizing Sized(Node node, Orientation along) => (along.Map(horizontal: node.Width, vertical: node.Height) | Inner(node).Map(inner => Sized(inner, along))).IfNone(Sizing.Fit.Content);

    private static Option<Aspect> Aspect(Node node) =>
        node.Aspect | node.Switch(
            box: static _ => Option<Aspect>.None,
            flex: static _ => Option<Aspect>.None,
            grid: static _ => Option<Aspect>.None,
            lazy: static _ => Option<Aspect>.None,
            overlay: static _ => Option<Aspect>.None,
            choice: static _ => Option<Aspect>.None,
            scroll: static _ => Option<Aspect>.None,
            split: static _ => Option<Aspect>.None,
            plot: static plot => plot.Plane.Aspect,
            pin: static _ => Option<Aspect>.None,
            mark: static _ => Option<Aspect>.None,
            text: static _ => Option<Aspect>.None,
            glyph: static _ => Option<Aspect>.None,
            image: static image => UI.Aspect.Validated(image.Source.Width / (float)image.Source.Height).ToOption(),
            slot: static _ => Option<Aspect>.None,
            styled: static _ => Option<Aspect>.None);

    private static Seq<Laid> Panes(Laid owner) => owner.Children.Filter(static child => !IsHandle(child));

    private static float Rule(Laid split, Orientation axis) => Handle(split).Map(handle => Basis(handle, axis, float.PositiveInfinity)).IfNone(0f);

    private static (float Least, float Most) Bounded(Laid split, Node.Split node) =>
        Sized(node.First, node.Axis).Limits.Min switch {
            var least => (least, float.Max(least, float.Max(0f, node.Axis.Of(split.ContentBox.Size) - Rule(split, node.Axis)) - Sized(node.Second, node.Axis).Limits.Min)),
        };

    private static float MinContent(Laid laid) => laid.Switch(box: static _ => 0f, text: static text => text.MinContent);

    // --- [BUILD]
    private static Laid Built(Cascade cascade, Seq<Key> path, Node node, Option<Laid> held, HashMap<Seq<Key>, Laid> prior) =>
        held.Filter(kept => kept.Node.Equals(node) && kept.Cascade.Equals(cascade)).IfNone(() => Fresh(cascade, path, node, held, prior));

    private static Option<Laid> Held(HashMap<Seq<Key>, Laid> prior, Option<Laid> parent, Seq<Key> path) =>
        prior.Find(path) | parent.Bind(held => held.Members.Find(member => member.Path == path));

    private static Laid Fresh(Cascade cascade, Seq<Key> path, Node node, Option<Laid> prior, HashMap<Seq<Key>, Laid> index) {
        (Style style, Cascade inner) = Styles.Enter(cascade, node);
        Seq<Laid> members = Members(node).Map(member => path.Add(member.Segment) switch {
            var at => Built(inner, at, member.Node, Held(index, prior, at), index),
        });
        Laid built = node.Switch(
            (Path: path, Node: node, Cascade: cascade, Style: style, Inner: inner, Members: members),
            box: Boxing, flex: Boxing, grid: Boxing, lazy: Boxing, overlay: Boxing, choice: Choosing, scroll: Boxing, split: Boxing, plot: Boxing, pin: Boxing, mark: Boxing,
            text: Setting, glyph: Boxing, image: Boxing, slot: Boxing, styled: Boxing);
        float width = Wide(built);
        return built with { Intrinsic = new SKSize(width, Tall(built, width)) };
    }

    private static Laid Boxing((Seq<Key> Path, Node Node, Cascade Cascade, Style Style, Cascade Inner, Seq<Laid> Members) held, Node _) =>
        new Laid.Box { Path = held.Path, Node = held.Node, Cascade = held.Cascade, Style = held.Style, Members = held.Members, Children = held.Members };

    private static Laid Choosing((Seq<Key> Path, Node Node, Cascade Cascade, Style Style, Cascade Inner, Seq<Laid> Members) held, Node.Choice _) =>
        new Laid.Box { Path = held.Path, Node = held.Node, Cascade = held.Cascade, Style = held.Style, Members = held.Members, Children = held.Members.Take(1) };

    private static Laid Setting((Seq<Key> Path, Node Node, Cascade Cascade, Style Style, Cascade Inner, Seq<Laid> Members) held, Node.Text text) =>
        Typesetting.Resolve(held.Cascade.Theme, held.Style) switch {
            var face => Typesetting.Set(text, held.Inner, face, float.PositiveInfinity) switch {
                var set => new Laid.Text(text, set, face) { Path = held.Path, Node = held.Node, Cascade = held.Cascade, Style = held.Style, MinContent = Typesetting.MinContent(set) },
            },
        };

    private static Seq<(Key Segment, Node Node)> Members(Node node) =>
        node.Switch(
            box: static _ => Seq<(Key, Node)>(),
            flex: static flex => Segments(flex.Children),
            grid: static grid => Segments(grid.Rows.Flatten()),
            lazy: static lazy => lazy.Items.Choose(key => lazy.Realized.Find(key).Map(realized => (key, realized))),
            overlay: static overlay => Segments(overlay.Base.Cons(overlay.Above)),
            choice: static choice => choice.Alternatives.Map(static alternative => (alternative.Key.IfNone(static () => new Key.Ordinal(0)), alternative)),
            scroll: static scroll => Seq((scroll.Content.Key.IfNone(static () => new Key.Ordinal(0)), scroll.Content), Handle(Role.Scrollbar)),
            split: static split => Seq(
                (split.First.Key.IfNone(static () => new Key.Ordinal(0)), split.First),
                Handle(Role.Divider),
                (split.Second.Key.IfNone(static () => new Key.Ordinal(1)), split.Second)),
            plot: static plot => Segments(plot.Children + plot.Labels.Map(static label => (Node)label)),
            pin: static pin => Segments(Seq(pin.Content)),
            mark: static _ => Seq<(Key, Node)>(),
            text: static _ => Seq<(Key, Node)>(),
            glyph: static _ => Seq<(Key, Node)>(),
            image: static _ => Seq<(Key, Node)>(),
            slot: static _ => Seq<(Key, Node)>(),
            styled: static styled => Segments(Seq(styled.Content)));

    private static (Key Segment, Node Node) Handle(Role role) => (new Key.Handle(), new Node.Box { Key = new Key.Handle(), Role = role });

    private static Seq<(Key Segment, Node Node)> Segments(Seq<Node> nodes) => nodes.Map(static (node, index) => (node.Key.IfNone(() => new Key.Ordinal((uint)index)), node));

    private static Seq<int> Slots(Node.Lazy lazy) =>
        lazy.Items.Map(static (key, index) => (Key: key, Index: index)).Filter(held => lazy.Realized.ContainsKey(held.Key)).Map(static held => held.Index);

    // --- [FIT]
    private static float Wide(Laid laid) =>
        (from aspect in Aspect(laid.Node)
         from height in Sized(laid.Node, Orientation.Vertical).Definite
         select height * (float)aspect).IfNone(() => Boxed(laid, Orientation.Horizontal));

    private static float Tall(Laid laid, float width) => Aspect(laid.Node).Map(aspect => width / (float)aspect).IfNone(() => Boxed(laid, Orientation.Vertical));

    private static float Boxed(Laid laid, Orientation along) => float.Max(Content(laid, along) + (2f * laid.Inset), laid.Node.Role.IsSome ? laid.Style.Metrics[Metric.Extent] : 0f);

    private static float Content(Laid laid, Orientation along) =>
        laid.Switch(along, box: static (held, box) => Contained(box, held), text: static (held, text) => MathF.Ceiling(held.Of(text.Typeset.Size)));

    private static float Contained(Laid.Box laid, Orientation along) =>
        laid.Node.Switch(
            (Laid: laid, Along: along),
            box: static (_, _) => 0f,
            flex: static (held, flex) => Summed(held.Laid.Children, held.Along, flex.Axis == held.Along, Gap(held.Laid)),
            grid: static (held, grid) => Spanned(held.Laid, grid, held.Along),
            lazy: static (held, lazy) => Spread(held.Laid, lazy, held.Along),
            overlay: static (held, _) => Widest(held.Laid.Children.Head.ToSeq(), held.Along),
            choice: static (held, choice) => Widest((held.Along == choice.Axis ? held.Laid.Members : held.Along.Map(horizontal: held.Laid.Members, vertical: held.Laid.Children)).Take(1), held.Along),
            scroll: static (held, _) => Widest(Panes(held.Laid), held.Along),
            split: static (held, split) => Summed(held.Laid.Children, held.Along, split.Axis == held.Along, 0f),
            plot: static (held, _) => Widest(held.Laid.Children.Filter(static child => child.Node.Pinned.IsNone), held.Along),
            pin: static (held, _) => Widest(held.Laid.Children, held.Along),
            mark: static (_, _) => 0f,
            text: static (_, _) => 0f,
            glyph: static (held, _) => held.Laid.Style.Metrics[Metric.Icon],
            image: static (held, image) => held.Along.Of(new SKSize(image.Source.Width / image.Density, image.Source.Height / image.Density)),
            slot: static (_, _) => 0f,
            styled: static (held, _) => Widest(held.Laid.Children, held.Along));

    private static float Spanned(Laid laid, Node.Grid grid, Orientation along) {
        float gap = Gap(laid);
        Seq<Sizing> columns = Columns(laid, grid);
        Seq<Cell> cells = Cells(laid, grid, columns.Count);
        Seq<float> tracks = Tracks(cells, columns, along, float.PositiveInfinity);
        Seq<float> starts = tracks.Scan(0f, (start, extent) => start + extent + gap);
        return along.Switch(
            (Cells: cells, Starts: starts, Summed: Summed(tracks, gap)),
            horizontal: static held => held.Cells.Filter(static cell => cell.Spans).Fold(held.Summed, (widest, cell) => float.Max(widest, held.Starts[cell.Column] + Basis(cell.Laid, Orientation.Horizontal, float.PositiveInfinity))),
            vertical: static held => held.Summed);
    }

    private static float Spread(Laid laid, Node.Lazy lazy, Orientation along) {
        float gap = Gap(laid);
        int lanes = Lanes(laid, lazy);
        int lines = Lines(lazy, lanes);
        return along == lazy.Axis
            ? (lazy.Extent * lines) + (gap * float.Max(0, lines - 1))
            : Summed(Columned(Celled(laid, lazy, lanes), Tracked(lazy.Across, lanes), along, float.PositiveInfinity), gap);
    }

    private static float Summed(Seq<Laid> children, Orientation along, bool sum, float gap) =>
        sum ? Summed(children.Map(child => Basis(child, along, float.PositiveInfinity)), gap) : Widest(children, along);

    private static float Summed(Seq<float> extents, float gap) => Enumerable.Sum(extents) + (gap * float.Max(0, extents.Count - 1));

    private static float Widest(Seq<Laid> children, Orientation along) => children.Map(child => Basis(child, along, float.PositiveInfinity)).Fold(0f, float.Max);

    private static float Basis(Laid laid, Orientation along, float space) => Based(Sized(laid.Node, along), along.Of(laid.Intrinsic), space);

    private static float Based(Sizing sizing, float fit, float space) =>
        sizing.Limits.Clamped(sizing.Switch(
            (Fit: fit, Space: space),
            @fixed: static (_, held) => held.Extent,
            fit: static (state, _) => state.Fit,
            grow: static (state, _) => state.Fit,
            percent: static (state, held) => float.IsFinite(state.Space) ? (float)held.Fraction * state.Space : state.Fit));

    private static float Cross(Laid laid, Orientation along, float space) =>
        Sized(laid.Node, along) switch {
            var sizing => sizing.Limits.Clamped(sizing.Stretches ? space : float.Min(Basis(laid, along, space), space)),
        };

    private static float Gap(Laid laid) => laid.Style.Metrics[Metric.Gap];

    // --- [PLACE]
    private sealed record Room(Pass Pass, Laid Laid, Orientation Along, float Start, float Space);

    private sealed record Cell(int Row, int Column, bool Spans, Laid Laid);

    private static Laid Placed(Pass pass, Laid laid, Orientation along, float start, float extent) {
        if (Reused(laid, pass.Prior) && along.Of(laid.Frame.Size).CompareTo(extent) is 0) return Moved(laid, along, start);
        Laid framed = laid with { Frame = along.Spanned(laid.Frame, start, extent) };
        Room room = framed.ContentBox switch {
            var box => new Room(pass, framed, along, along.Of(box.Location), along.Of(box.Size)),
        };
        return along.Switch(
            (Room: room, Laid: Arranged(room)),
            horizontal: static held => Wrapped(held.Room, held.Laid) switch {
                var set => set with { Intrinsic = new SKSize(set.Intrinsic.Width, Tall(set, set.Frame.Width)) },
            },
            vertical: static held => held.Laid.Switch(
                box: static box => (Laid)box,
                text: static text => text.ContentBox switch {
                    var box => text with { Origin = new SKPoint(box.Left, box.Top + (float.Max(0f, box.Height - text.Typeset.Size.Height) * text.Source.Align.Y)) },
                }));
    }

    private static Laid Wrapped(Room room, Laid laid) =>
        laid.Switch(
            room,
            box: static (_, box) => (Laid)box,
            text: static (room, wrapped) => wrapped with {
                Typeset = room.Pass.Prior.Find(wrapped.Path)
                    .Bind(static held => held.Switch(box: static _ => Option<Laid.Text>.None, text: static text => Some(text)))
                    .Filter(held => held.Source.Equals(wrapped.Source) && held.Style.Equals(wrapped.Style) && held.Frame.Width.CompareTo(wrapped.Frame.Width) is 0)
                    .Map(static held => held.Typeset)
                    .IfNone(() => Typesetting.Set(wrapped.Source, Styles.Enter(wrapped.Cascade, wrapped.Source).Inner, wrapped.TextStyle, room.Space)),
            });

    private static Option<float> Baseline(Laid laid) =>
        laid.Switch(
            box: static box => box.Children.Choose(static child => Baseline(child).Map(line => child.Frame.Top + line)).Head,
            text: static text => text.Typeset.Baseline.Map(line => text.Origin.Y + line));

    private static Seq<Laid> Baselined(Room room, Seq<Laid> placed) =>
        placed.Map(static child => (Child: child, Line: Baseline(child))) switch {
            var lines => lines.Choose(static held => held.Line).Fold(0f, float.Max) switch {
                var most => lines.Map(held => held.Line.Match(Some: line => Moved(held.Child, room.Along, room.Start + most - line), None: () => held.Child)),
            },
        };

    private static Laid Moved(Laid laid, Orientation along, float start) =>
        along.Of(laid.Frame.Location).CompareTo(start) is 0
            ? laid
            : laid with { Frame = along.Spanned(laid.Frame, start, along.Of(laid.Frame.Size)) };

    private static Laid Shown(Laid laid, Seq<Laid> placed) => laid with { Members = placed, Children = placed };

    private static Laid Filled(Room room, Laid child) => Placed(room.Pass, child, room.Along, room.Start, room.Space);

    private static Laid Layered(Room room, Laid child) => Aligned(room, child, room.Start, room.Space, child.Node.Align.Of(room.Along));

    private static Laid Aligned(Room room, Laid child, float start, float space, float align) =>
        Cross(child, room.Along, space) switch {
            var extent => Placed(room.Pass, child, room.Along, start + ((space - extent) * align), extent),
        };

    private static Laid Arranged(Room room) =>
        room.Laid.Node.Switch(
            room,
            box: static (held, _) => held.Laid,
            flex: static (held, flex) => Shown(held.Laid, flex.Axis == held.Along ? Along(held) : held.Laid.Members.Map(member => Layered(held, member)) switch {
                var layered => flex.Baseline && held.Along == Orientation.Vertical ? Baselined(held, layered) : layered,
            }),
            grid: static (held, grid) => Shown(held.Laid, Gridded(held, grid)),
            lazy: static (held, lazy) => Shown(held.Laid, Windowed(held, lazy)),
            overlay: static (held, _) => Shown(held.Laid, Layers(held)),
            choice: static (held, choice) => Chosen(held, choice),
            scroll: static (held, scroll) => Scrolled(held, scroll),
            split: static (held, split) => Shown(held.Laid, split.Axis == held.Along ? Divided(held, split) : held.Laid.Members.Map(member => Filled(held, member))),
            plot: static (held, plot) => Charted(held, plot),
            pin: static (held, _) => Shown(held.Laid, held.Laid.Members.Map(member => Filled(held, member))),
            mark: static (held, _) => held.Laid,
            text: static (held, _) => held.Laid,
            glyph: static (held, _) => held.Laid,
            image: static (held, _) => held.Laid,
            slot: static (held, _) => held.Laid,
            styled: static (held, _) => Shown(held.Laid, held.Laid.Members.Map(member => Filled(held, member))));

    private static Seq<Laid> Along(Room room) {
        Seq<Laid> children = room.Laid.Members;
        float gap = Gap(room.Laid);
        float space = room.Space - (gap * float.Max(0, children.Count - 1));
        Seq<float> extents = Distribute(children.Map(child => Sized(child.Node, room.Along) switch {
            var sizing => (sizing, Basis(child, room.Along, space), sizing.Floor(room.Along.Map(horizontal: MinContent(child), vertical: 0f))),
        }), space);
        Seq<float> starts = extents.Scan(room.Start, (start, extent) => start + extent + gap);
        return children.Zip(extents.Zip(starts)).Map(placed => Placed(room.Pass, placed.First, room.Along, placed.Second.Second, placed.Second.First));
    }

    private static Laid Chosen(Room room, Node.Choice choice) =>
        room.Along == choice.Axis
            ? (room.Laid.Members.Find(alternative => Basis(alternative, room.Along, float.PositiveInfinity) <= room.Space) | room.Laid.Members.Last)
                .Map(chosen => room.Laid with { Children = [Filled(room, chosen)] })
                .IfNone(room.Laid)
            : room.Along.Switch(
                room,
                horizontal: static held => Shown(held.Laid, held.Laid.Members.Map(member => Filled(held, member))),
                vertical: static held => held.Laid with { Children = held.Laid.Children.Map(child => Filled(held, child)) });

    private static Seq<Laid> Layers(Room room) =>
        room.Laid.Members.Match(
            Empty: static () => Seq<Laid>(),
            Tail: (ground, above) => room.Laid.ContentBox switch {
                var box => Filled(room, ground).Cons(Pins(room, above, at => SKMatrix.CreateScaleTranslation(box.Width, box.Height, box.Left, box.Top).MapPoint(at), Layered)),
            });

    private static Laid Charted(Room room, Node.Plot plot) =>
        Pins(room, room.Laid.Members, plot.Plane.In(room.Laid.ContentBox).Point, Filled) switch {
            var placed => room.Laid with {
                Members = placed,
                Children = room.Along.Switch(
                    (Placed: placed, Grounded: plot.Children.Count),
                    horizontal: static held => held.Placed,
                    vertical: static held => held.Placed.Take(held.Grounded) + held.Placed.Skip(held.Grounded).Fold(Seq<Laid>(), static (kept, label) => kept.Exists(prior => prior.Frame.IntersectsWith(label.Frame)) ? kept : kept.Add(label))),
            },
        };

    private static Seq<Laid> Pins(Room room, Seq<Laid> members, Func<Vector2, SKPoint> at, Func<Room, Laid, Laid> unpinned) =>
        members.Map(member => member.Node.Pinned.Match(
            Some: pin => room.Along.Switch((Room: room, Member: member, Pin: pin, At: at), horizontal: static held => held.Member, vertical: static held => Pinned(held.Room, held.Member, held.Pin, held.At)),
            None: () => unpinned(room, member)));

    private static Laid Pinned(Room room, Laid member, Node.Pin pin, Func<Vector2, SKPoint> at) {
        SKRect box = room.Laid.ContentBox;
        SKPoint point = at(pin.At);
        Option<SKPoint> end = pin.Span.Map(span => at(pin.At + span));
        return toSeq(Orientation.Items).Fold(member, (held, axis) => end.Match(
            Some: to => Placed(room.Pass, held, axis, float.Min(axis.Of(point), axis.Of(to)), MathF.Abs(axis.Of(to) - axis.Of(point))),
            None: () => Cross(held, axis, axis.Of(box.Size)) switch {
                var extent => Placed(room.Pass, held, axis, Sized(held.Node, axis).Stretches ? axis.Of(box.Location) : axis.Of(point) - (pin.Align.Of(axis) * extent), extent),
            }));
    }

    private static Seq<Laid> Gridded(Room room, Node.Grid grid) {
        float gap = Gap(room.Laid);
        Seq<Sizing> columns = Columns(room.Laid, grid);
        Seq<Cell> cells = Cells(room.Laid, grid, columns.Count);
        Seq<float> tracks = room.Along.Switch(
            (Room: room, Columns: columns, Cells: cells, Gap: gap),
            horizontal: static held => (held.Room.Space - (held.Gap * float.Max(0, held.Columns.Count - 1))) switch {
                var space => Distributed(held.Columns, Tracks(held.Cells, held.Columns, Orientation.Horizontal, space), space),
            },
            vertical: static held => Tracks(held.Cells, held.Columns, Orientation.Vertical, held.Room.Space));
        Seq<float> starts = tracks.Scan(room.Start, (start, extent) => start + extent + gap);
        float end = room.Start + room.Space;
        return cells.Map(cell => room.Along.Map(horizontal: (Track: cell.Column, cell.Spans), vertical: (Track: cell.Row, Spans: false)) switch {
            var (track, spans) => Aligned(room, cell.Laid, starts[track], spans ? end - starts[track] : tracks[track], cell.Laid.Node.Align.Of(room.Along)),
        });
    }

    private static Seq<Sizing> Columns(Laid laid, Node.Grid grid) =>
        grid.Rows.Map(static row => row.Count).Fold(0, int.Max) switch {
            var longest => Tracked(grid.Columns, grid.Columns.Switch(
                (Laid: laid, Longest: longest),
                @explicit: static (held, tracks) => int.Max(tracks.Sizes.Count, held.Longest),
                autoFill: static (held, tracks) => Fill(tracks.Size, held.Laid.ContentBox.Width, Gap(held.Laid), Widest(held.Laid.Members, Orientation.Horizontal), held.Longest))),
        };

    private static Seq<Sizing> Tracked(Tracks tracks, int count) =>
        tracks.Switch(
            count,
            @explicit: static (lanes, held) => toSeq(Range(0, lanes)).Map(column => held.Sizes.At(column).IfNone(Sizing.Fit.Content)),
            autoFill: static (lanes, held) => LanguageExt.Seq.repeat(held.Size, lanes));

    private static int Fill(Sizing size, float space, float gap, float widest, int most) =>
        (int)float.Clamp(MathF.Floor((space + gap) / float.Max(Based(size, widest, space) + gap, float.Epsilon)), 1f, float.Max(1f, most));

    private static Seq<Cell> Cells(Laid laid, Node.Grid grid, int columns) =>
        from row in grid.Rows.Zip(grid.Rows.Scan(0, static (start, cells) => start + cells.Count))
            .Bind(row => toSeq(Enumerable.Range(row.Second, row.First.Count).Chunk(columns)))
            .Map(static (members, index) => (Members: members, Index: index))
        from cell in toSeq(row.Members).Map(static (member, column) => (Member: member, Column: column))
        select new Cell(row.Index, cell.Column, cell.Column == row.Members.Length - 1 && row.Members.Length < columns, laid.Members[cell.Member]);

    private static Seq<float> Tracks(Seq<Cell> cells, Seq<Sizing> columns, Orientation along, float space) =>
        along.Switch(
            (Cells: cells, Columns: columns, Space: space),
            horizontal: static held => Columned(held.Cells, held.Columns, Orientation.Horizontal, held.Space),
            vertical: static held => toSeq(held.Cells.GroupBy(static cell => cell.Row).Select(static row => Widest(toSeq(row).Map(static cell => cell.Laid), Orientation.Vertical))));

    private static Seq<float> Columned(Seq<Cell> cells, Seq<Sizing> columns, Orientation across, float space) =>
        toHashMap(cells.Filter(static cell => !cell.Spans).GroupBy(static cell => cell.Column).Select(column => (column.Key, Widest(toSeq(column).Map(static cell => cell.Laid), across)))) switch {
            var widths => columns.Map((sizing, column) => Based(sizing, widths.Find(column).IfNone(0f), space)),
        };

    private static Seq<Laid> Windowed(Room room, Node.Lazy lazy) {
        float gap = Gap(room.Laid);
        int lanes = Lanes(room.Laid, lazy);
        Seq<Cell> cells = Celled(room.Laid, lazy, lanes);
        if (room.Along == lazy.Axis) return cells.Map(cell => Aligned(room, cell.Laid, room.Start + (cell.Row * (lazy.Extent + gap)), lazy.Extent, cell.Laid.Node.Align.Of(room.Along)));
        float space = room.Space - (gap * float.Max(0, lanes - 1));
        Seq<Sizing> sizes = Tracked(lazy.Across, lanes);
        Seq<float> tracks = Distributed(sizes, Columned(cells, sizes, room.Along, space), space);
        Seq<float> starts = tracks.Scan(room.Start, (start, extent) => start + extent + gap);
        return cells.Map(cell => Aligned(room, cell.Laid, starts[cell.Column], tracks[cell.Column], cell.Laid.Node.Align.Of(room.Along)));
    }

    private static int Lanes(Laid laid, Node.Lazy lazy) =>
        lazy.Across.Switch(
            (Laid: laid, Lazy: lazy),
            @explicit: static (_, tracks) => int.Max(1, tracks.Sizes.Count),
            autoFill: static (held, tracks) => Fill(tracks.Size, held.Lazy.Axis.Cross.Of(held.Laid.ContentBox.Size), Gap(held.Laid), Widest(held.Laid.Members, held.Lazy.Axis.Cross), held.Lazy.Items.Count));

    private static Seq<Cell> Celled(Laid laid, Node.Lazy lazy, int lanes) =>
        laid.Members.Zip(Slots(lazy)).Map(held => new Cell(held.Second / lanes, held.Second % lanes, Spans: false, held.First));

    private static Laid Scrolled(Room room, Node.Scroll scroll) {
        Seq<Laid> panes = room.Laid.Members.Filter(static member => !IsHandle(member)).Map(content => scroll.Axis == room.Along
            ? Placed(room.Pass, content, room.Along, room.Start, float.Max(Basis(content, room.Along, float.PositiveInfinity), room.Space))
            : Filled(room, content));
        float total = panes.Map(pane => scroll.Axis.Of(pane.Frame.Size)).Fold(0f, float.Max);
        float viewport = scroll.Axis.Of(room.Laid.ContentBox.Size);
        Seq<Laid> thumbs = room.Laid.Members.Filter(IsHandle).Map(thumb => Thumb(room, scroll, thumb, total, viewport));
        bool shown = room.Along.Map(horizontal: scroll.Axis.Map(horizontal: total > viewport, vertical: true), vertical: total > viewport);
        return room.Laid with { Members = panes + thumbs, Children = shown ? panes + thumbs : panes };
    }

    private static Laid Thumb(Room room, Node.Scroll scroll, Laid thumb, float total, float viewport) {
        float least = Basis(thumb, room.Along, float.PositiveInfinity);
        if (scroll.Axis != room.Along) return Placed(room.Pass, thumb, room.Along, room.Start + room.Space - least, least);
        float length = total > viewport ? float.Min(viewport, float.Max(least, viewport * viewport / total)) : 0f;
        float offset = Scrolling(room.Pass.Adjusted.Find(room.Laid.Path).IfNone(0f), total, viewport);
        return Placed(room.Pass, thumb, room.Along, room.Start + Rescaled(offset, total - viewport, viewport - length), length);
    }

    private static Seq<Laid> Divided(Room room, Node.Split split) {
        float rule = Rule(room.Laid, room.Along);
        float available = float.Max(0f, room.Space - rule);
        (float least, float most) = Bounded(room.Laid, split);
        Option<Laid> first = room.Laid.Members.Find(static member => !IsHandle(member));
        float extent = float.Clamp((room.Pass.Adjusted.Find(room.Laid.Path) | first.Map(pane => Basis(pane, room.Along, available))).IfNone(0f), least, most);
        Seq<float> extents = room.Laid.Members.Map(member => IsHandle(member) ? rule : first.Exists(pane => pane.Path == member.Path) ? extent : float.Max(0f, available - extent));
        Seq<float> starts = extents.Scan(room.Start, static (start, span) => start + span);
        return room.Laid.Members.Zip(extents.Zip(starts)).Map(placed => Placed(room.Pass, placed.First, room.Along, placed.Second.Second, placed.Second.First));
    }

    // --- [DISTRIBUTE]
    private static Seq<float> Distributed(Seq<Sizing> tracks, Seq<float> bases, float space) =>
        Distribute(tracks.Zip(bases).Map(static track => (track.First, track.Second, track.First.Floor(0f))), space);

    private static Seq<float> Distribute(Seq<(Sizing Sizing, float Basis, float Floor)> items, float space) {
        Seq<float> bases = items.Map(static item => item.Basis);
        float free = space - Enumerable.Sum(bases);
        return !float.IsFinite(space)
            ? bases
            : free >= 0f
                ? Shared(items, bases, free)
                : toSeq(items.Map(static item => item.Sizing.Yields).Somes().Distinct().Order()).Fold(bases, (extents, tier) => Shrunk(items, extents, Enumerable.Sum(extents) - space, tier));
    }

    private static Seq<float> Shrunk(Seq<(Sizing Sizing, float Basis, float Floor)> items, Seq<float> extents, float deficit, int tier) =>
        items.Zip(extents).Map(pair => pair.First.Sizing.Yields == Some(tier) ? float.Max(0f, pair.Second - pair.First.Floor) : 0f) switch {
            var room => Enumerable.Sum(room) switch {
                var total when total > 0f && deficit > 0f => extents.Zip(room).Map(pair => pair.First - (pair.Second * float.Min(deficit, total) / total)),
                _ => extents,
            },
        };

    private static Seq<float> Shared(Seq<(Sizing Sizing, float Basis, float Floor)> items, Seq<float> extents, float free) {
        Seq<float> open = items.Zip(extents).Map(static pair => pair.Second < pair.First.Sizing.Limits.Max ? pair.First.Sizing.Growth : 0f);
        float weight = Enumerable.Sum(open);
        if (weight <= 0f || free <= 0f) return extents;
        Seq<float> proposed = extents.Zip(open).Map(pair => pair.First + (free * pair.Second / weight));
        Seq<float> capped = items.Zip(proposed).Map(static pair => float.Min(pair.First.Sizing.Limits.Max, pair.Second));
        float spill = Enumerable.Sum(proposed) - Enumerable.Sum(capped);
        return spill > 0f ? Shared(items, capped, spill) : capped;
    }
}
