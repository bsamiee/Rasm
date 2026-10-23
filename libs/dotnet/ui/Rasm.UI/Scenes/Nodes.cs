using System.Diagnostics.CodeAnalysis;
using Rasm.UI.Access;
using Rasm.UI.Geometry;
using Rasm.UI.Glyphs;
using Rasm.UI.Inputs;
using Rasm.UI.Layout;
using Rasm.UI.Styling;
using SkiaSharp;
using Topten.RichTextKit;

namespace Rasm.UI.Scenes;

// --- [TYPES] ---------------------------------------------------------------------------
public enum Edge {
    Left,
    Top,
    Right,
    Bottom,
}

public enum ImageFit {
    Contain,
    Cover,
    Fill,
    Actual,
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Padding(Space Horizontal, Space Vertical) {
    public static Padding Zero { get; } = new(Space.Zero, Space.Zero);
}

public readonly record struct Alignment(float X, float Y) {
    public static Alignment Start { get; } = new(0f, 0f);
    public static Alignment Center { get; } = new(0.5f, 0.5f);
    public static Alignment End { get; } = new(1f, 1f);
}

public abstract record Component {
    public abstract Seq<(MarkShape Shape, MarkStyle Style)> Marks(SKRect frame);

    public virtual Option<float> Height(float width) => None;

    public virtual Seq<(SKRect Bounds, Facet Facet)> Facets(SKRect frame) => [];
}

public abstract record Component<TPart> : Component where TPart : notnull {
    public abstract Seq<PlotMark<TPart>> Parts(SKRect frame);

    public sealed override Seq<(MarkShape Shape, MarkStyle Style)> Marks(SKRect frame) =>
        Parts(frame).Choose(static mark => mark.Style.Map(style => (mark.Shape, style)));

    public Option<TPart> Hit(SKRect frame, SKPoint at) => Plots.Hit(Parts(frame), at);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Node {
    public Option<string> Key { get; init; }
    public Sizing Width { get; init; } = Sizing.Content;
    public Sizing Height { get; init; } = Sizing.Content;
    public Padding Padding { get; init; } = Padding.Zero;
    public Option<Role> Role { get; init; }
    public InteractionState State { get; init; } = InteractionState.Rest;
    public Option<Func<Input, Laid, IO<Response>>> Handler { get; init; }
    public CursorKind Cursor { get; init; }
    public bool Focusable { get; init; }
    public Option<Facet> Facet { get; init; }

    public bool Leaf => this is Text or Glyph or Shape or Image or Widget or Slot or Layer;

    public virtual bool Equals([NotNullWhen(true)] Node? other) =>
        other is not null && EqualityContract == other.EqualityContract
        && Key == other.Key && Width == other.Width && Height == other.Height && Padding == other.Padding
        && Role == other.Role && State == other.State && Cursor == other.Cursor && Focusable == other.Focusable && Facet == other.Facet;

    public override int GetHashCode() => HashCode.Combine(EqualityContract, Key, Width, Height, Padding, Role, State, HashCode.Combine(Cursor, Focusable, Facet));

    public sealed record Flex(Orientation Axis, Seq<Node> Children) : Node {
        public Space Gap { get; init; } = Space.Zero;
        public Alignment Align { get; init; }
        public bool Wraps { get; init; }
    }

    public sealed record Grid(Seq<Sizing> Columns, Seq<Node> Children) : Node {
        public Seq<Sizing> Rows { get; init; }
        public Space Gap { get; init; } = Space.Zero;
    }

    public sealed record Overlay(Seq<Node> Children) : Node {
        public Alignment Align { get; init; }
    }

    public sealed record Scroll(Orientation Axis, float Offset, Node Content) : Node;

    public sealed record Split(Orientation Axis, float Fraction, Node First, Node Second) : Node;

    public sealed record Dock(Seq<(Edge Edge, Node Node)> Edges, Node Fill) : Node;

    public sealed record Text(string Content) : Node {
        public Option<TypeRole> Type { get; init; }
        public Option<Ink> Ink { get; init; }
        public Option<int> MaxLines { get; init; }
        public TextAlignment Align { get; init; } = TextAlignment.Auto;
        public Option<TextRange> Selection { get; init; }
    }

    public sealed record Glyph(GlyphRole Affordance) : Node {
        public Option<Ink> Ink { get; init; }
    }

    public sealed record Shape(MarkShape Figure, MarkStyle Style) : Node;

    public sealed record Image(SKImage Source, ImageFit Fit) : Node;

    public sealed record Layer(Node Content) : Node;

    public sealed record Widget(Component Component) : Node;

    public sealed record Slot(string Id) : Node;

    public sealed record Styled(Node Content) : Node {
        public Option<Appearance> Appearance { get; init; }
        public Option<GlyphSet> Glyphs { get; init; }
    }
}
