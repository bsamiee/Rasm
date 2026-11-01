using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using Rasm.UI.Media;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Layout;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record LayoutPass(HashMap<Seq<Key>, LayoutNode> Prior, HashMap<Seq<Key>, float> Offsets, Shaping Shaping, CultureInfo Culture);

internal sealed record AvailableSpace(LayoutPass Pass, LayoutNode Layout, PhysicalAxis Along, float Start, float Space);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class Layouts {
    // --- [ARRANGE]
    internal static LayoutNode Arrange(LayoutPass pass, Cascade root, View node, SKSize size) => WithCorners(Framed(pass, Built(pass, root, [], node, pass.Prior.Find([])), SKRect.Create(size)), None);

    internal static LayoutNode Framed(LayoutPass pass, LayoutNode layout, SKRect frame) =>
        Placed(pass, Placed(pass, layout, PhysicalAxis.Horizontal, frame.Left, frame.Width), PhysicalAxis.Vertical, frame.Top, frame.Height);

    internal static Fin<LayoutNode> Unique(LayoutNode layout, HashMap<Seq<Key>, LayoutNode> prior) =>
        Repeated(layout, prior) switch {
            var repeated when repeated.IsEmpty => layout,
            var repeated => Error.Many(repeated.Map(static path => (Error)new UIFault(string.Join('/', path.Map(static key => key.Identifier)), UIReason.Repeated))),
        };

    internal static SKSize Preferred(LayoutPass pass, LayoutNode layout, SKSize offered) {
        LayoutNode wide = Placed(pass, layout, PhysicalAxis.Horizontal, 0f, float.Min(Basis(layout, PhysicalAxis.Horizontal, offered.Width), offered.Width));
        return new SKSize(wide.Frame.Width, float.Min(Basis(wide, PhysicalAxis.Vertical, offered.Height), offered.Height));
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
    private static Sizing Sized(View node, PhysicalAxis along) => (along.Map(horizontal: node.Width, vertical: node.Height) | node.Inner.Map(inner => Sized(inner, along))).IfNone(Sizing.Fit.Content);

    private static Option<Factor> Aspect(View node) =>
        node.Aspect | node.Plotted.Bind(static plane => plane.Aspect)
        | (node is View.Image image ? Factor.Validated(image.Source.Info.Width / (float)image.Source.Info.Height).ToOption() : None);

    // --- [BUILD]
    internal static LayoutNode Built(LayoutPass pass, Cascade cascade, Seq<Key> path, View node, Option<LayoutNode> held) =>
        held.Filter(kept => kept.View.Equals(node) && kept.Cascade.Equals(cascade)).IfNone(() => Created(pass, cascade, path, node, held));

    internal static Option<LayoutNode> Held(LayoutPass pass, Seq<LayoutNode> earlier, Seq<Key> path) => pass.Prior.Find(path) | earlier.Find(member => member.Path == path);

    private static LayoutNode Created(LayoutPass pass, Cascade cascade, Seq<Key> path, View node, Option<LayoutNode> prior) {
        Cascade inner = cascade.Enter(node);
        Seq<LayoutNode> earlier = prior.Map(static held => held.Members + held.Layers).IfNone([]);
        Seq<LayoutNode> members = Members(node).Map(member => path.Add(member.Segment) switch {
            var at => Built(pass, inner, at, member.View, Held(pass, earlier, at)),
        }).Strict();
        LayoutNode.Box box = new() { Path = path, View = node, Cascade = cascade, Inner = inner, Style = inner.Enclosing, Members = members.Filter(static member => member.View is not View.Layer) };
        LayoutNode built = node.SwitchPartially(
            (Pass: pass, Box: box with { Children = box.Members }),
            @default: static (held, _) => held.Box,
            choice: static (held, _) => held.Box with { Children = held.Box.Members.Take(1) },
            mark: static (held, mark) => mark.Figure is Figure.Label label ? TextNode(held.Pass, held.Box, new View.Text(label.Spans)) : held.Box,
            text: static (held, text) => (LayoutNode)TextNode(held.Pass, held.Box, text)) with {
            Layers = members.Filter(static member => member.View is View.Layer),
        };
        float width = IntrinsicWidth(built);
        return built with { Intrinsic = new SKSize(width, IntrinsicHeight(built, width)) };
    }

    private static LayoutNode.Text TextNode(LayoutPass pass, LayoutNode box, View.Text text) {
        Typeset set = Typesetting.Set(pass.Shaping, text, box.Inner, Typesetting.Resolve(pass.Shaping, box.Inner, box.Style), float.PositiveInfinity, pass.Culture);
        return new LayoutNode.Text(set) { Path = box.Path, View = box.View, Cascade = box.Cascade, Inner = box.Inner, Style = box.Style, MinContent = Typesetting.MinContent(pass.Shaping, set, pass.Culture) };
    }

    private static Seq<(Key Segment, View View)> Members(View node) =>
        node.SwitchPartially(
            @default: static held => Segments(held.Inner.ToSeq()),
            flex: static flex => Segments(flex.Children),
            grid: static grid => Segments(grid.Rows.Flatten()),
            lazy: static lazy => lazy.Items.Choose(key => lazy.Realized.Find(key).Map(realized => (Segment: key, View: realized with { Role = realized.Role | Some(lazy.Rows) }))),
            overlay: static overlay => Segments(overlay.Base.Cons(overlay.Above)),
            layer: static layer => (layer.Modal ? Seq<(Key Segment, View View)>((LayerStack.Scrim, new View.Box { Role = Role.Scrim })) : []) + Segments([layer.Content]),
            choice: static choice => Segments(choice.Alternatives),
            scroll: static scroll => Segments([scroll.Content]).Add((new Key.Handle(), new View.Box { Key = new Key.Handle(), Role = Role.Scrollbar })),
            plot: static plot => Segments(plot.Children));

    private static Seq<(Key Segment, View View)> Segments(Seq<View> nodes) => nodes.Map(static (node, index) => (Segment: node.Key.IfNone(() => new Key.Ordinal((uint)index)), View: node));

    // --- [FIT]
    private static float IntrinsicWidth(LayoutNode layout) =>
        (from aspect in Aspect(layout.View)
         from height in Sized(layout.View, PhysicalAxis.Vertical).Definite
         select height * (float)aspect).IfNone(() => Boxed(layout, PhysicalAxis.Horizontal, float.PositiveInfinity, least: false));

    private static float IntrinsicHeight(LayoutNode layout, float width) => Aspect(layout.View).Map(aspect => width / (float)aspect).IfNone(() => Boxed(layout, PhysicalAxis.Vertical, width, least: false));

    private static float Least(LayoutNode layout) => Sized(layout.View, PhysicalAxis.Horizontal).Definite.IfNone(() => Boxed(layout, PhysicalAxis.Horizontal, float.PositiveInfinity, least: true));

    private static float Boxed(LayoutNode layout, PhysicalAxis along, float measure, bool least) {
        float boxed = (2f * layout.Inset) + layout.Switch(
            (Along: along, Measure: measure, Least: least),
            box: static (held, box) => Contained(box, held.Along, held.Measure, held.Least),
            text: static (held, text) => held.Least ? text.MinContent : MathF.Ceiling(held.Along.Of(text.Typeset.Size)));
        return layout.View.Role.IsNone
            ? boxed
            : float.Max(along == PhysicalAxis.Horizontal && !least ? float.Min(boxed, layout.Style.Metrics[Metric.Measure]) : boxed, layout.Style.Metrics[Metric.Extent]);
    }

    private static float Contained(LayoutNode.Box layout, PhysicalAxis along, float measure, bool least) =>
        layout.View.SwitchPartially(
            (Layout: layout, Along: along, Measure: measure, Least: least),
            @default: static (held, _) => Widest(held.Layout.Children, held.Along, held.Least),
            flex: static (held, flex) => Gap(held.Layout) switch {
                var gap when flex.Axis == held.Along && !(held.Least && flex.Wraps) => Summed(held.Layout.Children.Map(child => Reach(child, held.Along, held.Least)), gap),
                var gap => Summed((flex.Wraps ? Broken(held.Layout.Children, flex.Axis, held.Measure, 2f * held.Layout.Inset, gap) : [held.Layout.Children]).Map(line => Widest(line, held.Along, held.Least)), gap),
            },
            grid: static (held, grid) => held.Least ? 0f : Lattice.Of(held.Layout, grid).Extent(held.Layout, held.Along),
            lazy: static (held, lazy) => held.Least ? 0f : Lattice.Of(held.Layout, lazy).Extent(held.Layout, held.Along),
            overlay: static (held, _) => Widest(held.Layout.Children.Take(1), held.Along, held.Least),
            choice: static (held, choice) => Widest(
                held.Least ? held.Layout.Members.Last.ToSeq() : (held.Along == choice.Axis ? held.Layout.Members : held.Along.Map(horizontal: held.Layout.Members, vertical: held.Layout.Children)).Take(1),
                held.Along, held.Least),
            scroll: static (held, _) => held.Least ? 0f : Widest(Panes(held.Layout), held.Along),
            plot: static (held, _) => held.Least ? 0f : Widest(held.Layout.Children.Filter(static child => child.View.Pinned.IsNone), held.Along),
            glyph: static (held, _) => held.Layout.Style.Metrics[Metric.Icon],
            image: static (held, image) => held.Least ? 0f : held.Along.Of(image.Source.Info.Size) / (float)image.Density);

    private static float Reach(LayoutNode child, PhysicalAxis along, bool least) => least ? Sized(child.View, PhysicalAxis.Horizontal).Floor(Least(child)) : Basis(child, along, float.PositiveInfinity);

    private static float Widest(Seq<LayoutNode> children, PhysicalAxis along, bool least = false) => children.Map(child => Reach(child, along, least)).Max(0f);

    private static float Summed(Seq<float> extents, float gap) => TensorPrimitives.Sum<float>([.. extents]) + (gap * float.Max(0, extents.Count - 1));

    private static float Basis(LayoutNode layout, PhysicalAxis along, float space) => Based(Sized(layout.View, along), along.Of(layout.Intrinsic), space);

    internal static float Based(Sizing sizing, float fit, float space) =>
        sizing.Limits.Clamped(sizing.Switch(
            (Fit: fit, Space: space),
            @fixed: static (_, held) => held.Extent,
            fit: static (state, _) => state.Fit,
            grow: static (state, _) => state.Fit,
            percent: static (state, held) => float.IsFinite(state.Space) ? (float)held.Fraction * state.Space : state.Fit));

    private static float Cross(LayoutNode layout, PhysicalAxis along, float space) {
        Sizing sizing = Sized(layout.View, along);
        return sizing.Limits.Clamped(sizing.Stretches ? space : float.Min(Basis(layout, along, space), space));
    }

    private static float Gap(LayoutNode layout) => layout.Style.Metrics[Metric.Gap];

    // --- [PLACE]
    private static LayoutNode Placed(LayoutPass pass, LayoutNode layout, PhysicalAxis along, float start, float extent) {
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

    private static LayoutNode Layered(AvailableSpace available, LayoutNode child) => Aligned(available, child, available.Start, available.Space, child.View.Align.Of(available.Along));

    private static LayoutNode Aligned(AvailableSpace available, LayoutNode child, float start, float space, float align) {
        float extent = Cross(child, available.Along, space);
        return Placed(available.Pass, child, available.Along, start + ((space - extent) * align), extent);
    }

    private static LayoutNode Arranged(AvailableSpace available) =>
        available.Layout.View.SwitchPartially(
            available,
            @default: static (held, _) => Shown(held.Layout, held.Layout.Members.Map(member => Filled(held, member))),
            flex: static (held, flex) => Flexed(held, flex) switch {
                var placed => Shown(held.Layout, Gutter.Shared(held, placed.Map(static item => item.Child), flex.Axis.Cross, index => placed[index].Line)),
            },
            grid: static (held, grid) => Lattice.Of(held.Layout, grid) switch {
                var lattice => Shown(held.Layout, Gutter.Shared(held, Gutter.Shared(held, lattice.Arranged(held), PhysicalAxis.Vertical, index => lattice.Cells[index].Line), PhysicalAxis.Horizontal, index => lattice.Cells[index].Track)),
            },
            lazy: static (held, lazy) => Shown(held.Layout, Lattice.Of(held.Layout, lazy).Arranged(held)),
            overlay: static (held, _) => Shown(held.Layout, Overlaid(held)),
            choice: static (held, choice) => Chosen(held, choice),
            scroll: static (held, scroll) => Scrolled(held, scroll),
            plot: static (held, plot) => Gutter.Empty.Plot(held, plot));

    private static Seq<(LayoutNode Child, int Line)> Flexed(AvailableSpace available, View.Flex flex) {
        LayoutNode layout = available.Layout;
        (PhysicalAxis across, bool main) = (flex.Axis.Cross, available.Along == flex.Axis);
        if (!flex.Wraps || (!main && across == PhysicalAxis.Horizontal))
            return (main ? Along(available, layout.Members) : Across(available, layout.Members, flex.Baseline)).Map(static child => (Child: child, Line: 0));
        Seq<Seq<LayoutNode>> lines = Broken(layout.Members, flex.Axis, flex.Axis.Of(layout.Frame.Size), 2f * layout.Inset, Gap(layout));
        float origin = across.Of(layout.ContentBox.Location);
        return lines.Zip(Stacked(layout, lines, across)).Map((held, line) => (main
                ? Along(available, across == PhysicalAxis.Horizontal
                    ? held.First.Map(child => Aligned(available with { Along = across }, child, origin + held.Second.Start, held.Second.Extent, child.View.Align.Of(across)))
                    : held.First)
                : Across(available with { Start = origin + held.Second.Start, Space = held.Second.Extent }, held.First, flex.Baseline))
            .Map(child => (Child: child, Line: line))).Flatten();
    }

    private static Seq<Seq<LayoutNode>> Broken(Seq<LayoutNode> children, PhysicalAxis along, float extent, float inset, float gap) =>
        children.Fold(Seq<(Seq<LayoutNode> Line, Seq<float> Bases)>(), (lines, child) => Basis(child, along, extent - inset) switch {
            var basis => lines.Last.Filter(last => Summed(last.Bases.Add(basis), gap) + inset <= extent).Match(
                Some: last => lines.Init.Add((last.Line.Add(child), last.Bases.Add(basis))),
                None: () => lines.Add((Seq(child), Seq(basis)))),
        }).Map(static held => held.Line);

    private static Seq<(float Start, float Extent)> Stacked(LayoutNode layout, Seq<Seq<LayoutNode>> lines, PhysicalAxis across) {
        Seq<float> extents = lines.Map(line => Widest(line, across)).Strict();
        float share = float.Max(0f, across.Of(layout.ContentBox.Size) - Summed(extents, Gap(layout))) / int.Max(1, lines.Count);
        return Started(layout, extents.Map(extent => extent + share));
    }

    private static Seq<LayoutNode> Along(AvailableSpace available, Seq<LayoutNode> children) {
        float gap = Gap(available.Layout);
        float space = available.Space - (gap * float.Max(0, children.Count - 1));
        Seq<float> extents = Distribute(children.Map(child => Sized(child.View, available.Along) switch {
            var sizing => (Sizing: sizing, Basis: Basis(child, available.Along, space), Floor: sizing.Floor(available.Along.Map(horizontal: Least(child), vertical: 0f))),
        }), space);
        return children.Zip(extents.Zip(extents.Scan(available.Start, (start, extent) => start + extent + gap)))
            .Map(placed => Placed(available.Pass, placed.First, available.Along, placed.Second.Second, placed.Second.First));
    }

    private static Seq<LayoutNode> Across(AvailableSpace available, Seq<LayoutNode> children, Option<Baseline> baseline) {
        Seq<LayoutNode> layered = children.Map(member => Layered(available, member));
        return baseline.Filter(_ => available.Along == PhysicalAxis.Vertical).Map(line => {
            Seq<(LayoutNode Child, Option<float> Line)> lines = layered.Map(child => (Child: child, Line: Baseline(child, line))).Strict();
            float most = lines.Choose(static held => held.Line).Max(0f);
            return lines.Map(held => held.Line.Match(Some: at => Moved(held.Child, available.Along, available.Start + most - at), None: () => held.Child));
        }).IfNone(layered);
    }

    private static LayoutNode Chosen(AvailableSpace available, View.Choice choice) =>
        available.Along == choice.Axis
            ? (available.Layout.Members.Find(alternative => Sized(alternative.View, available.Along) switch {
                var sizing => (sizing.Stretches && available.Along == PhysicalAxis.Horizontal ? sizing.Floor(Least(alternative)) : Basis(alternative, available.Along, float.PositiveInfinity)) <= available.Space,
            }) | available.Layout.Members.Last)
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
                    var placed => Filled(available, baseNode).Cons(Pins(available, box, above, at => placed.MapPoint(at), Layered)),
                },
            });

    internal static Seq<LayoutNode> Pins(AvailableSpace available, SKRect box, Seq<LayoutNode> members, Func<Vector2, SKPoint> at, Func<AvailableSpace, LayoutNode, LayoutNode> unpinned) =>
        members.Map(member => member.View.Pinned.Match(
            Some: pin => available.Along == PhysicalAxis.Horizontal ? member : Pinned(available, box, member, pin, at),
            None: () => unpinned(available, member)));

    private static LayoutNode Pinned(AvailableSpace available, SKRect box, LayoutNode member, View.Pin pin, Func<Vector2, SKPoint> at) {
        SKPoint point = at(pin.At);
        Option<SKPoint> end = pin.Span.Map(span => at(pin.At + span));
        return toSeq(PhysicalAxis.Items).Fold(member, (held, axis) => end.Match(
            Some: to => Placed(available.Pass, held, axis, float.Min(axis.Of(point), axis.Of(to)), MathF.Abs(axis.Of(to) - axis.Of(point))),
            None: () => Cross(held, axis, axis.Of(box.Size)) switch {
                var extent => Placed(available.Pass, held, axis, Sized(held.View, axis).Stretches ? axis.Of(box.Location) : axis.Of(point) - (pin.Align.Of(axis) * extent), extent),
            }));
    }

    // --- [CORNER]
    internal static LayoutNode WithCorners(LayoutNode layout, Option<(Corners Corners, SKRect Frame)> background) {
        Corners corners = layout.View.Role.IsNone ? Corners.Square
            : layout.Style.Shape is Shape.Capsule ? Corners.Round(float.Min(layout.Frame.Width, layout.Frame.Height) / 2f)
            : layout.Style.Rounded.Match(
                Some: Corners.Round,
                None: () => background.Map(outer => outer.Corners.Within(outer.Frame, layout.Frame)).IfNone(() => Corners.Round(layout.Style.Metrics[Metric.Radius]))).Fitted(layout.Frame.Size);
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
                ? Grown(items, bases, free)
                : toSeq(items.Map(static item => item.Sizing.ShrinkTier).Somes().Distinct().Order()).Fold(bases, (extents, tier) => Shrunk(items, extents, TensorPrimitives.Sum<float>(extents) - space, tier)));
    }

    private static float[] Shrunk(Seq<(Sizing Sizing, float Basis, float Floor)> items, float[] extents, float deficit, int tier) {
        float[] room = [.. items.Map((item, index) => item.Sizing.ShrinkTier == Some(tier) ? float.Max(0f, extents[index] - item.Floor) : 0f)];
        return Spread(room, -float.Clamp(deficit, 0f, TensorPrimitives.Sum<float>(room)), extents);
    }

    private static float[] Grown(Seq<(Sizing Sizing, float Basis, float Floor)> items, float[] extents, float free) {
        float[] capped = Spread([.. items.Map((item, index) => extents[index] < item.Sizing.Limits.Max ? item.Sizing.GrowWeight : 0f)], free, extents);
        float proposed = TensorPrimitives.Sum<float>(capped);
        TensorPrimitives.Min(capped, [.. items.Map(static item => item.Sizing.Limits.Max)], capped);
        float spill = proposed - TensorPrimitives.Sum<float>(capped);
        return spill > 0f ? Grown(items, capped, spill) : capped;
    }

    private static float[] Spread(float[] weights, float amount, float[] extents) {
        float total = TensorPrimitives.Sum<float>(weights);
        if (total <= 0f || amount is 0f) return extents;
        float[] spread = new float[extents.Length];
        TensorPrimitives.MultiplyAdd(weights, amount / total, extents, spread);
        return spread;
    }
}
