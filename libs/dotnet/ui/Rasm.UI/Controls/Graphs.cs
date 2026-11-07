using System.Numerics;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.UI.Automation;
using Rasm.UI.Controls.Primitives;
using Rasm.UI.Input;
using Rasm.UI.Layout;
using Rasm.UI.Media;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;
using Topten.RichTextKit;

namespace Rasm.UI.Controls;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record Port(Key Key, DataSeries Type) {
    public Option<string> Label { get; init; }
    public bool Many { get; init; }
}

public sealed record Socket(Key Node, Key Port);

public sealed record Wire(Socket From, Socket To);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Group {
    private Group(Key key, string title) => (Key, Title) = (key, title);

    public Key Key { get; init; }
    public string Title { get; init; }

    internal abstract Option<SKRect> Framed(HashMap<Key, SKRect> boxes, float margin);

    internal static Option<SKRect> Enclosing(Seq<SKRect> boxes, float margin) => boxes.Head.Map(first => SKRect.Inflate(boxes.Tail.Fold(first, SKRect.Union), margin, margin));

    public sealed record Members(Key Key, string Title, Seq<Key> Nodes) : Group(Key, Title) {
        internal override Option<SKRect> Framed(HashMap<Key, SKRect> boxes, float margin) => Enclosing(Nodes.Choose(boxes.Find), margin);
    }

    public sealed record Bounds(Key Key, string Title, SKRect Frame) : Group(Key, Title) {
        internal override Option<SKRect> Framed(HashMap<Key, SKRect> boxes, float margin) => Some(Frame.Standardized);
    }
}

public sealed record Graph<TNode>(Seq<TNode> Nodes, Func<TNode, Key> Key, Func<TNode, string> Title, Vector2 Row, PlotPlane View, Part.Viewport Navigation)
    where TNode : notnull {

    // --- [STATE]

    private static readonly Seq<View> Gridlines = toSeq(PhysicalAxis.Items).Bind(static along => Seq<View>(
        new View.Mark(new Figure.Grid(along) { Major = false }) { Role = Roles.GridFine },
        new View.Mark(new Figure.Grid(along)) { Role = Roles.Grid }));

    public Option<Func<TNode, Vector2>> Position { get; init; }
    public Func<TNode, Seq<Port>> Inputs { get; init; } = static _ => [];
    public Func<TNode, Seq<Port>> Outputs { get; init; } = static _ => [];
    public Option<Func<TNode, View>> Face { get; init; }
    public Option<Func<TNode, GlyphSource>> Glyph { get; init; }
    public Func<TNode, HashSet<Condition>> Conditions { get; init; } = static _ => [];
    public Seq<Wire> Wires { get; init; }
    public Seq<Group> Groups { get; init; }
    public PhysicalAxis Flow { get; init; } = PhysicalAxis.Horizontal;
    public Selection Selection { get; init; } = Selection.Empty;
    public Option<Func<Edit<Selection>, IO<Unit>>> Selected { get; init; }
    public Option<Socket> Hovered { get; init; }
    public Option<Func<Option<Socket>, IO<Unit>>> Hover { get; init; }
    public Option<Func<Edit<HashMap<Key, Vector2>>, IO<Unit>>> Moved { get; init; }
    public Option<Func<Edit<Seq<Wire>>, IO<Unit>>> Wired { get; init; }
    public Func<Port, Port, bool> Accepts { get; init; } = static (output, input) => output.Type.Ordinal == input.Type.Ordinal;
    public Option<Marquee> Marquee { get; init; }
    public bool Snaps { get; init; }
    public bool Minimap { get; init; }
    public Option<Interpolation> Curve { get; init; }
    public Key Role { get; init; } = Roles.Well;

    // --- [DRAWING]

    public View Drawn {
        get {
            Seq<Node> nodes = Nodes.Map(node => new Node(node, Key(node), Title(node), Inputs(node), Outputs(node))).Strict();
            Seq<Key> keys = nodes.Map(static node => node.Key);
            HashMap<Key, Vector2> positions = Position.Match(Some: at => toHashMap(nodes.Map(node => (node.Key, at(node.Item)))), None: () => Layered(nodes, keys));
            HashMap<Key, SKRect> boxes = toHashMap(nodes.Map(node => (node.Key, SKRect.Create(positions[node.Key], new SKSize(Size(node))))));
            HashMap<Key, Seq<Slot>> ports = toHashMap(nodes.Map(node => (node.Key, Slots(node, boxes[node.Key]).Strict())));
            HashMap<Socket, Slot> slots = toHashMap(toSeq(ports.Values).Flatten().Map(static slot => (slot.Socket, slot)));
            HashMap<Key, View.Pin> faces = toHashMap(nodes.Map(node => (node.Key, Faced(node, boxes[node.Key], ports[node.Key], slots, keys))));
            Seq<(Group Group, SKRect Box)> groups = Groups.Choose(group => group.Framed(boxes, Row.Y).Map(box => (group, box))).Strict();
            Part.Viewport navigation = Navigation with { Rest = Fitted(toSeq(boxes.Values) + groups.Map(static group => group.Box)).IfNone(Navigation.Rest) };
            View canvas = new View.Plot(View, [
                new Handles<HashMap<Key, Vector2>>(positions, Positions(keys), Moved.IfNone(static _ => unitIO)) {
                    Selection = Selection,
                    Selected = Selected,
                    Marquee = Marquee,
                    Snaps = Snaps,
                    Handle = (key, at) => faces[key] with { At = at },
                }.Drawn(View, Gridlines + groups.Map<View>(static group => Framed(group.Group, group.Box)) + Wires.Choose(wire => Connection(wire, slots))) with {
                    Drop = Wired.Map(wired => new Drop((transfer, _, _) => Detached(transfer, slots, wired))),
                },
            ]) {
                Role = Role,
                Part = navigation,
                Accessible = new Accessible { Role = AccessRole.Group },
            }.Grown;
            return new View.Overlay(canvas, Seq(navigation.Zooms(View) with { Align = new UnitPoint(Fraction.MaxValue, Fraction.MinValue) }) + (Minimap ? Seq<View>(Mapped(navigation, boxes, keys)) : [])).Grown;
        }
    }

    private View.Flex Mapped(Part.Viewport navigation, HashMap<Key, SKRect> boxes, Seq<Key> keys) =>
        new(PhysicalAxis.Horizontal, [
            navigation.Overview(View, keys.Choose(key => boxes.Find(key).Bind(static box => Figure.Region.Spanning(box.Left, box.Right, box.Top, box.Bottom))
                .Map(region => (View)new View.Mark(region) { Role = Roles.Card, Conditions = Selection.Holds(key) ? [Condition.Selected] : [] }))),
        ]) {
            Role = Roles.Hud,
            Accessible = new Accessible { Role = AccessRole.Group },
            Align = new UnitPoint(Fraction.MaxValue, Fraction.MaxValue),
        };

    private static View.Pin Framed(Group group, SKRect box) =>
        new(new Vector2(box.Left, box.Top),
            new View.Flex(PhysicalAxis.Vertical, [new View.Text([group.Title]) { Role = Roles.Caption, MaxLines = 1 }]) {
                Role = Roles.Group,
                Accessible = new Accessible { Role = AccessRole.Group, Label = group.Title },
            }) {
            Key = group.Key,
            Span = new Vector2(box.Width, box.Height),
        };

    // --- [FACES]

    private View.Pin Faced(Node node, SKRect box, Seq<Slot> members, HashMap<Socket, Slot> slots, Seq<Key> keys) {
        HashSet<Condition> marks = Conditions(node.Item);
        Seq<View> ports = members.Map(slot => (View)Ported(slot, slots, keys));
        return new View.Pin(box.Location, new View.Choice(PhysicalAxis.Vertical, [
            new View.Overlay(Face.Map(face => face(node.Item)).IfNone(() => Labeled(node, marks, new Sizing.Percent(new Fraction(1f / Rows(node))))), ports),
            new View.Overlay(new View.Flex(PhysicalAxis.Vertical, [Header(node, marks) with { Height = Sizing.Grow.Unit }]), ports),
            new View.Box()])) {
            Span = Size(node),
            Role = Roles.Card,
            Conditions = Selection.Holds(node.Key) ? marks.TryAdd(Condition.Selected) : marks,
            Accessible = new Accessible { Label = node.Title },
        };
    }

    private View.Flex Labeled(Node node, HashSet<Condition> marks, Sizing share) =>
        new(PhysicalAxis.Vertical, Seq<View>(Header(node, marks) with { Height = share }) + toSeq(Range(0, Rows(node) - 1)).Map(index => (View)new View.Flex(PhysicalAxis.Horizontal, [
                PortLabel(node.Inputs.At(index), TextAlignment.Left),
                PortLabel(node.Outputs.At(index), TextAlignment.Right),
            ]) {
            Role = Roles.ListRow,
            Height = share,
        }));

    private View.Flex Header(Node node, HashSet<Condition> marks) =>
        Panel.Header(Panel.Icon(Glyph.Map(glyph => glyph(node.Item))), node.Title, None, Panel.StatusGlyph(marks).ToSeq());

    private static View PortLabel(Option<Port> port, TextAlignment alignment) =>
        port.Bind(static held => held.Label)
            .Map(label => (View)new View.Text([label]) { MaxLines = 1, Alignment = alignment, Width = Sizing.Grow.Unit })
            .IfNone(static () => new View.Box { Width = Sizing.Grow.Unit });

    private View.Pin Ported(Slot slot, HashMap<Socket, Slot> slots, Seq<Key> keys) =>
        new(slot.Unit, new Part.Choice([slot.Socket.Port], new Selection.One(None), edit => Focused(edit, slot.Socket.Node, keys)) {
            Hover = hovered => Hover.Map(report => report(hovered.Map(_ => slot.Socket))),
        }.Drawn(
            _ => new View.Box {
                Role = Roles.Port,
                Series = slot.Port.Type,
                Conditions = Hovered == Some(slot.Socket) ? [Condition.Highlighted] : [],
                Accessible = new Accessible { Label = slot.Port.Label },
            },
            members => new View.Flex(PhysicalAxis.Horizontal, members) {
                Key = slot.Socket.Node,
                Lift = Wired.Map(_ => Lifted(slot, slots)),
                Drop = Wired.Map(wired => new Drop((transfer, _, _) =>
                    from source in Source(transfer)
                    from wires in Rewired(source, slot.Socket, slots)
                    select wired(Edit.Of(EditPhase.Commit, wires)))),
            })) {
            Align = UnitPoint.Center,
        };

    private IO<Unit> Focused(Edit<Selection> edit, Key node, Seq<Key> keys) =>
        Selected.Map(selected => selected(edit.Map(_ => Selection.Navigated(keys, node, extend: false)).As())).IfNone(unitIO);

    // --- [WIRES]

    private Option<View> Connection(Wire wire, HashMap<Socket, Slot> slots) =>
        from start in slots.Find(wire.From).Filter(static slot => slot.Output)
        from end in slots.Find(wire.To)
        select (View)Line(start.Anchor, end.Anchor, start.Port.Type, Seq(wire.From, wire.To).Exists(socket => Selection.Holds(socket.Node) || Hovered == Some(socket)) ? [Condition.Highlighted] : []);

    private View.Mark Line(Vector2 start, Vector2 end, DataSeries type, HashSet<Condition> marks) =>
        new(new Figure.Path([start, end]) { Interpolation = Curve.IfNone(() => Flow.Map(horizontal: Interpolation.BumpX, vertical: Interpolation.BumpY)) }) {
            Role = Roles.Wire,
            Series = type,
            Conditions = marks,
        };

    private Lift Lifted(Slot slot, HashMap<Socket, Slot> slots) =>
        Lift.Of(ports => new Transfer.Keys(ports.Map(port => Seq(slot.Socket.Node, port)))) with {
            Preview = slots.Find(Attached(slot.Socket, slots).Head.Map(static wire => wire.From).IfNone(slot.Socket))
                .Map(start => (Func<Vector2, View>)(pointer => Line(start.Anchor, pointer, slot.Port.Type, []))),
        };

    private Seq<Wire> Attached(Socket socket, HashMap<Socket, Slot> slots) => slots.Find(socket).Exists(static slot => !slot.Output) ? Wires.Filter(wire => wire.To == socket) : [];

    private Option<Seq<Wire>> Rewired(Socket source, Socket target, HashMap<Socket, Slot> slots) {
        Seq<Wire> attached = Attached(source, slots);
        return from wire in attached.Head.Filter(_ => slots.Find(target).Exists(static slot => !slot.Output)).Map(held => new Wire(held.From, target))
                   || Some(slots.Find(source).Exists(static slot => slot.Output) ? new Wire(source, target) : new Wire(target, source))
               from output in slots.Find(wire.From).Filter(static slot => slot.Output)
               from input in slots.Find(wire.To).Filter(static slot => !slot.Output)
               where Accepts(output.Port, input.Port) && !Wires.Exists(held => held == wire)
               select Wires.Filter(held => !attached.Exists(moved => moved == held) && (input.Port.Many || held.To != wire.To)).Add(wire);
    }

    private Option<IO<Unit>> Detached(Transfer transfer, HashMap<Socket, Slot> slots, Func<Edit<Seq<Wire>>, IO<Unit>> wired) =>
        from source in Source(transfer)
        let attached = Attached(source, slots)
        where !attached.IsEmpty
        select wired(Edit.Of(EditPhase.Commit, Wires.Filter(wire => !attached.Exists(held => held == wire))));

    private static Option<Socket> Source(Transfer transfer) =>
        from keys in Optional(transfer as Transfer.Keys)
        from path in keys.Paths.Head
        from node in path.Init.Last
        from port in path.Last
        select new Socket(node, port);

    // --- [LAYOUT]

    private Points<HashMap<Key, Vector2>> Positions(Seq<Key> keys) =>
        new(positions => keys.Choose(key => positions.Find(key).Map(at => new HandlePoint(key, at))),
            (positions, targets, _) => (
                Moved.IsSome ? targets.Fold(positions, static (held, target) => held.AddOrUpdate(target.Key, target.At)) : positions,
                []));

    private HashMap<Key, Vector2> Layered(Seq<Node> nodes, Seq<Key> keys) {
        HashMap<Key, int> ranks = Ranks(keys);
        return nodes.Map(node => (node.Key, Rank: ranks[node.Key], Size: Size(node))).Fold((Ends: HashMap<int, float>(), At: HashMap<Key, Vector2>()), (held, placed) =>
            held.Ends.Find(placed.Rank).IfNone(0f) switch {
                var start => (
                    held.Ends.AddOrUpdate(placed.Rank, start + Flow.Cross.Of(placed.Size) + Row.Y),
                    held.At.AddOrUpdate(placed.Key, Flow.Cross.With(
                        Flow.With(Vector2.Zero, Directed(Flow, 2f * placed.Rank * Flow.Of(placed.Size), Flow.Of(placed.Size))),
                        Directed(Flow.Cross, start, Flow.Cross.Of(placed.Size))))),
            }).At;
    }

    private HashMap<Key, int> Ranks(Seq<Key> keys) {
        HashSet<Key> known = toHashSet(keys);
        ILookup<Key, Wire> outgoing = Wires.Filter(wire => known.Contains(wire.From.Node) && known.Contains(wire.To.Node)).ToLookup(static wire => wire.From.Node);
        return Arrangements.Ranked(keys.ToBidirectionalGraph(key => outgoing[key].Select(wire => new SEquatableEdge<Key>(key, wire.To.Node)), allowParallelEdges: false)
                .CondensateStronglyConnected<Key, SEquatableEdge<Key>, AdjacencyGraph<Key, SEquatableEdge<Key>>>())
            .AsIterable()
            .Fold(HashMap<Key, int>(), static (ranks, entry) => toSeq(entry.Key.Vertices).Fold(ranks, (all, vertex) => all.Add(vertex, entry.Value)));
    }

    private Seq<Slot> Slots(Node node, SKRect box) =>
        node.Inputs.Map((port, index) => Slotted(node.Key, box, port, output: false, index, node.Inputs.Count, Rows(node)))
        + node.Outputs.Map((port, index) => Slotted(node.Key, box, port, output: true, index, node.Outputs.Count, Rows(node)));

    private Slot Slotted(Key node, SKRect box, Port port, bool output, int index, int count, int rows) {
        Vector2 placed = Flow.Cross.With(Flow.With(Vector2.Zero, output ? 1f : 0f), Flow == PhysicalAxis.Horizontal ? (index + 1.5f) / rows : (index + 0.5f) / count);
        return new Slot(new Socket(node, port.Key), port, output, placed, new Vector2(Along(PhysicalAxis.Horizontal, box.Left, box.Right, placed.X), Along(PhysicalAxis.Vertical, box.Top, box.Bottom, 1f - placed.Y)));
    }

    private int Rows(Node node) => Flow == PhysicalAxis.Horizontal ? 1 + int.Max(node.Inputs.Count, node.Outputs.Count) : 1;

    private Vector2 Size(Node node) => Row with { Y = Row.Y * Rows(node) };

    private Option<PlotPlane> Fitted(Seq<SKRect> boxes) =>
        from content in Group.Enclosing(boxes, Row.Y)
        from across in View.Dimension(PhysicalAxis.Horizontal)
        from up in View.Dimension(PhysicalAxis.Vertical)
        from ratio in View.Proportion.Map(static held => (float)held)
        let width = float.Max(content.Width, content.Height * ratio)
        from x in Spanned(across.Domain, content.MidX, width)
        from y in Spanned(up.Domain, content.MidY, width / ratio)
        from fitted in View.WithDomain(PhysicalAxis.Horizontal, x)
        from plane in fitted.WithDomain(PhysicalAxis.Vertical, y)
        select plane;

    private static Option<AxisRange> Spanned(AxisRange domain, float middle, float span) =>
        (MathF.CopySign(span, domain.Span) / 2f) switch { var half => AxisRange.Validated(middle - half, middle + half).ToOption() };

    private float Along(PhysicalAxis along, float low, float high, float fraction) => Rising(along) ? float.Lerp(low, high, fraction) : float.Lerp(high, low, fraction);

    private float Directed(PhysicalAxis along, float start, float extent) => Rising(along) == (along == PhysicalAxis.Horizontal) ? start : -(start + extent);

    private bool Rising(PhysicalAxis along) => View.Dimension(along).Exists(static axis => axis.Domain.Span > 0f);

    private sealed record Node(TNode Item, Key Key, string Title, Seq<Port> Inputs, Seq<Port> Outputs);

    private sealed record Slot(Socket Socket, Port Port, bool Output, Vector2 Unit, Vector2 Anchor);
}
