using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using Rasm.UI.Media;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Layout;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record LayoutNode {
    public virtual bool Equals([NotNullWhen(true)] LayoutNode? other) => ReferenceEquals(this, other);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    internal required Seq<Key> Path { get; init; }
    internal required View View { get; init; }
    internal required Cascade Cascade { get; init; }
    internal required Cascade Inner { get; init; }
    internal required RoleStyle Style { get; init; }
    internal SKSize Intrinsic { get; init; }
    internal SKRect Frame { get; init; }
    internal SKRect Data { get; init; }
    internal Corners Corners { get; init; } = Corners.Square;
    internal Seq<LayoutNode> Children { get; init; }
    internal Seq<LayoutNode> Members { get; init; }
    internal Seq<LayoutNode> Layers { get; init; }

    internal SKRect ContentBox {
        get {
            float inset = Inset;
            return SKRect.Create(inset, inset, float.Max(0f, Frame.Width - (2f * inset)), float.Max(0f, Frame.Height - (2f * inset)));
        }
    }

    internal virtual SKRect Painted => SKRect.Inflate(Frame, Overhang, Overhang);

    internal SKRect SubtreeBounds =>
        Painted.Cons(Children.Filter(child => !View.Clips(child.View)).Map(child => Place.Absolute(child.SubtreeBounds, Frame.Location))).Filter(static reach => !reach.IsEmpty) switch {
            var reached => reached.Head.Map(first => reached.Tail.Fold(first, SKRect.Union)).IfNone(Painted),
        };

    internal SKRect LocalSubtreeBounds {
        get {
            SKRect reach = SubtreeBounds;
            return SKRect.Create(reach.Location - Frame.Location, reach.Size);
        }
    }

    internal Seq<LayoutNode> Preorder => toSeq(SuperEnumerable.TraverseDepthFirst(this, static held => held.Children));

    internal HashMap<Seq<Key>, LayoutNode> Indexed => toHashMap(Preorder.Map(static held => (held.Path, held)));

    internal bool Timed => (View.Role.IsSome && Style.Timed) || View is View.Glyph { Art.Animated: true } or View.Image { Source: ImageSource.Encoded { FrameCount: > 1 } };

    internal Option<Matrix4x4> Transform => View.Transform.Map(turn => Turned(turn, Vector2.Zero));

    internal float Inset => View.Role.IsSome ? Style.Metrics[Metric.Inset] : 0f;

    internal float Smoothing => View.Role.IsSome ? Style.Metrics[Metric.Smoothing] : 0f;

    internal bool HasBackground => View.Role.IsSome && View.Painting.AcceptsBackground && (!Style.Fills.IsEmpty || !Style.Strokes.IsEmpty);

    internal Option<Text> TextNode => Switch(box: static _ => Option<Text>.None, text: static text => Some(text));

    internal float Alpha(SKColorSpace target) => (Style.Dimmed && !Cascade.Enclosing.Dimmed ? Cascade.Theme.DisabledOpacity(target) : 1f) * (float)View.Opacity;

    internal Option<Seq<Key>> Lead => View.Part.Bind(part => part.Revealed(this));

    internal Option<PlotFrame> Plotted(SKPoint origin) => View.Plotted.Map(plane => plane.In(Data).Translated(origin));

    internal RoleStyle Styled(Key role) => Cascade.Resolve(role, Style.Conditions);

    internal RoleStyle Within(Key role) => Inner.Resolve(role, Style.Conditions);

    private float Overhang => View.Role.IsSome || View.Traced.IsSome ? Style.Overhang : 0f;

    internal Matrix4x4 Turned(Matrix4x4 turn, Vector2 origin) {
        Vector3 pivot = new(origin + new Vector2(Frame.Width * (float)View.Align.X, Frame.Height * (float)View.Align.Y), 0f);
        return Matrix4x4.CreateTranslation(-pivot) * turn * Matrix4x4.CreateTranslation(pivot);
    }

    internal sealed record Box : LayoutNode;

    internal sealed record Text(Typeset Typeset) : LayoutNode {
        internal SKPoint Origin { get; init; }

        internal override SKRect Painted {
            get {
                SKRect set = SKRect.Create(Frame.Location + Origin, Typeset.Size);
                SKRect painted = Typeset.Overhang;
                return SKRect.Union(base.Painted, new SKRect(set.Left - float.Max(Overhang, painted.Left), set.Top - float.Max(Overhang, painted.Top),
                    set.Right + float.Max(Overhang, painted.Right), set.Bottom + float.Max(Overhang, painted.Bottom)));
            }
        }
    }
}

internal sealed record Place(SKPoint Origin, SKRect Clip, Option<PlotFrame> Plot) {
    private readonly Option<SKMatrix> inverse = Some(SKMatrix.Identity);

    internal SKMatrix Transform { get; init { field = value; inverse = value.Inverse; } } = SKMatrix.Identity;

    internal static Place Root(LayoutNode layout, SKRect window) => new Place(SKPoint.Empty, window, None).Turned(layout);

    internal static HashMap<Seq<Key>, Place> Placing(LayoutNode layout, Place place, HashMap<Seq<Key>, float> offsets, HashMap<Seq<Key>, Place> placed) =>
        SuperEnumerable.TraverseDepthFirst((Layout: layout, Place: place), held => held.Layout.Children.Map(child => (Layout: child, Place: held.Place.Within(held.Layout, child, offsets))))
            .Aggregate(placed, static (index, held) => index.AddOrUpdate(held.Layout.Path, held.Place));

    internal static SKRect Absolute(SKRect frame, SKPoint origin) => SKRect.Create(frame.Location + origin, frame.Size);

    internal static SKPoint Scrolled(LayoutNode parent, LayoutNode child, HashMap<Seq<Key>, float> offsets) =>
        (from scroller in Layouts.Scroller(parent, None, offsets)
         where !Layouts.IsHandle(child)
         select (SKPoint)scroller.Axis.With(Vector2.Zero, -scroller.Value)).IfNone(SKPoint.Empty);

    internal Option<SKPoint> Local(SKPoint at) => inverse.Map(held => held.MapPoint(at));

    internal Option<PlotFrame> Plotted(LayoutNode layout) => layout.Plotted(Absolute(layout.Frame, Origin).Location) | Plot;

    internal Place Within(LayoutNode parent, LayoutNode child, HashMap<Seq<Key>, float> offsets) {
        SKRect frame = Absolute(parent.Frame, Origin);
        SKPoint origin = frame.Location + Scrolled(parent, child, offsets);
        return new Place(origin + Stuck(parent, child, Clip, origin), parent.View.Clips(child.View) ? SKRect.Intersect(Clip, frame) : Clip, Plotted(parent)) { Transform = Transform }.Turned(child);
    }

    private Place Turned(LayoutNode layout) =>
        layout.View.Transform.Map(turn => this with { Transform = Transform.PreConcat(((SKMatrix44)layout.Turned(turn, Absolute(layout.Frame, Origin).Location)).Matrix) }).IfNone(this);

    private static SKPoint Stuck(LayoutNode parent, LayoutNode child, SKRect clip, SKPoint origin) =>
        child.View.Sticks.Map(axis => {
            float start = axis.Of(origin + child.Frame.Location);
            float limit = toSeq(parent.Children.SkipWhile(sibling => !ReferenceEquals(sibling, child)).Skip(1)).Find(sibling => sibling.View.Sticks == Some(axis))
                .Map(next => axis.Of(origin + next.Frame.Location)).IfNone(() => axis.Of(origin) + axis.End(parent.ContentBox));
            return (SKPoint)axis.With(Vector2.Zero, float.Clamp(axis.Of(clip.Location), start, float.Max(start, limit - axis.Of(child.Frame.Size))) - start);
        }).IfNone(SKPoint.Empty);
}

internal sealed record PlacedNode(LayoutNode Layout, Place Place) {
    internal RoleStyle Style { get; init; } = Layout.Style;

    internal SKRect Frame { get; init; } = Place.Absolute(Layout.Frame, Place.Origin);
}
