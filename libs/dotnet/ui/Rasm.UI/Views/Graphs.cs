using System.Numerics;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.UI.Components;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;
using Topten.RichTextKit;

namespace Rasm.UI.Views;

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

    public Key Key { get; }
    public string Title { get; }

    internal abstract Option<SKRect> Framed(HashMap<Key, SKRect> boxes, float margin);

    internal static Option<SKRect> Union(Seq<SKRect> boxes) => boxes.Fold(Option<SKRect>.None, static (held, box) => held.Map(union => SKRect.Union(union, box)) | Some(box));

    public sealed record Members : Group {
        public Members(Key key, string title, Seq<Key> nodes) : base(key, title) => Nodes = nodes;

        public Seq<Key> Nodes { get; }

        internal override Option<SKRect> Framed(HashMap<Key, SKRect> boxes, float margin) =>
            Union(Nodes.Choose(boxes.Find)).Map(union => SKRect.Inflate(union, margin, margin));
    }

    public sealed record Bounds : Group {
        public Bounds(Key key, string title, Vector2 from, Vector2 to) : base(key, title) => (From, To) = (from, to);

        public Vector2 From { get; }
        public Vector2 To { get; }

        internal override Option<SKRect> Framed(HashMap<Key, SKRect> boxes, float margin) =>
            Some(new SKRect(float.Min(From.X, To.X), float.Min(From.Y, To.Y), float.Max(From.X, To.X), float.Max(From.Y, To.Y)));
    }
}

public sealed record Graph<TNode>(Seq<TNode> Nodes, Func<TNode, Key> Key, Func<TNode, string> Title, Vector2 Row, PlotPlane View, Func<Edit<PlotPlane>, IO<Unit>> Viewed)
    where TNode : notnull {
    // --- [STATE]
    private static readonly HashSet<Condition> Signals = [Condition.Success, Condition.Warning, Condition.Faulted];
    private static readonly Seq<Node> Gridlines = toSeq(PhysicalAxis.Items).Bind(static along => Seq<Node>(
        new Node.Mark(new Figure.Grid(along) { Major = false }) { Role = Role.GridFine },
        new Node.Mark(new Figure.Grid(along)) { Role = Role.Grid }));

    public Option<Func<TNode, Vector2>> Position { get; init; }
    public Func<TNode, Seq<Port>> Inputs { get; init; } = static _ => [];
    public Func<TNode, Seq<Port>> Outputs { get; init; } = static _ => [];
    public Option<Func<TNode, Node>> Face { get; init; }
    public Option<Func<TNode, GlyphSource>> Glyph { get; init; }
    public Func<TNode, HashSet<Condition>> Conditions { get; init; } = static _ => [];
    public Seq<Wire> Wires { get; init; }
    public Seq<Group> Groups { get; init; }
    public PhysicalAxis Flow { get; init; } = PhysicalAxis.Horizontal;
    public Selection Selection { get; init; } = new Selection.Many([]);
    public Option<Func<Edit<Selection>, IO<Unit>>> Selected { get; init; }
    public Option<Socket> Hovered { get; init; }
    public Option<Func<Option<Socket>, IO<Unit>>> Hover { get; init; }
    public Option<Func<Edit<HashMap<Key, Vector2>>, IO<Unit>>> Moved { get; init; }
    public Option<Func<Edit<Seq<Wire>>, IO<Unit>>> Wired { get; init; }
    public Func<Port, Port, bool> Accepts { get; init; } = static (output, input) => output.Type.Ordinal == input.Type.Ordinal;
    public Option<Marquee> Marquee { get; init; }
    public bool Snaps { get; init; }
    public Limits Zoom { get; init; } = Limits.Open;
    public bool Minimap { get; init; }
    public Option<Interpolation> Curve { get; init; }
    public Role Role { get; init; } = Role.Well;

    // --- [DRAW]
    public Node Drawn {
        get {
            Seq<Key> keys = Nodes.Map(Key).Strict();
            HashMap<Key, Vector2> positions = Position.Match(Some: at => toHashMap(Nodes.Map(node => (Key(node), at(node)))), None: () => Layered(keys));
            Laid laid = Placed(positions);
            HashMap<Key, Node.Pin> faces = toHashMap(Nodes.Choose(node => positions.Find(Key(node)).Map(at => (Key(node), Faced(node, at, laid, keys)))));
            PlotPlane rest = Fitted(toSeq(laid.Boxes.Values) + Groups.Choose(group => group.Framed(laid.Boxes, Row.Y))).IfNone(View);
            Part.Viewport navigation = new(rest, Viewed) { Magnification = Zoom };
            Node canvas = new Node.Plot(View, [
                new Handles<HashMap<Key, Vector2>>(positions, Positions(keys), Moved.IfNone(static _ => unitIO)) {
                    Selection = Selection,
                    Selected = Selected,
                    Marquee = Marquee,
                    Snaps = Snaps,
                    Handle = (key, at) => faces[key] with { At = at },
                }.Drawn(View, Gridlines + Groups.Choose(group => Framed(group, laid)) + Wires.Choose(wire => Connection(wire, laid))) with {
                    Drop = Wired.Map(wired => new Drop((transfer, _, _) => Detached(transfer, laid, wired))),
                },
            ]) {
                Role = Role,
                Part = navigation,
                Width = Sizing.Grow.Unit,
                Height = Sizing.Grow.Unit,
                Accessible = new Accessible { Role = AccessRole.Group },
            };
            return new Node.Overlay(canvas, Seq(Viewers.Zooms(navigation, View) with { Align = new UnitPoint(Fraction.MaxValue, Fraction.MinValue) }) + (Minimap ? Seq<Node>(Mapped(rest, laid, keys)) : [])) {
                Width = Sizing.Grow.Unit,
                Height = Sizing.Grow.Unit,
            };
        }
    }

    private Node.Flex Mapped(PlotPlane overview, Laid laid, Seq<Key> keys) =>
        new(PhysicalAxis.Horizontal, [
            new Node.Plot(overview, keys.Choose(key => laid.Boxes.Find(key).Map(box => (Node)new Node.Mark(new Figure.Region {
                    First = AxisRange.Validated(box.Left, box.Right).ToOption(),
                    Second = AxisRange.Validated(box.Top, box.Bottom).ToOption(),
                }) {
                    Role = Role.Card,
                    Conditions = Selection.Holds(key) ? [Condition.Selected] : [],
                }))
                .Add(new Brush(View, Viewed).Drawn(overview))) {
                Role = Role.Well,
                Aspect = from across in overview.Dimension(PhysicalAxis.Horizontal)
                         from up in overview.Dimension(PhysicalAxis.Vertical)
                         from ratio in Factor.Validated(MathF.Abs(across.Domain.Span / up.Domain.Span)).ToOption()
                         select ratio,
            },
        ]) {
            Role = Role.Hud,
            Accessible = new Accessible { Role = AccessRole.Group },
            Align = new UnitPoint(Fraction.MaxValue, Fraction.MaxValue),
        };

    private Option<Node> Framed(Group group, Laid laid) =>
        group.Framed(laid.Boxes, Row.Y).Map(box => (Node)new Node.Pin(new Vector2(box.Left, box.Top),
            new Node.Flex(PhysicalAxis.Vertical, [new Node.Text([group.Title]) { Role = Role.Caption, MaxLines = 1 }]) {
                Role = Role.Group,
                Accessible = new Accessible { Role = AccessRole.Group, Label = group.Title },
            }) {
            Key = group.Key,
            Span = new Vector2(box.Width, box.Height),
        });

    // --- [FACES]
    private Node.Pin Faced(TNode node, Vector2 at, Laid laid, Seq<Key> keys) {
        HashSet<Condition> marks = Conditions(node);
        Seq<Node> ports = Slots(node).Map(slot => (Node)Ported(slot, laid, keys));
        Seq<Node> full = Fraction.Validated(1f / Rows(node)).ToOption()
            .Map(share => (Node)new Node.Overlay(Face.Map(face => face(node)).IfNone(() => Labeled(node, marks, new Sizing.Percent(share))), ports)).ToSeq();
        return new Node.Pin(at, new Node.Choice(PhysicalAxis.Vertical, full + Seq<Node>(
            new Node.Overlay(new Node.Flex(PhysicalAxis.Vertical, [Header(node, marks) with { Height = Sizing.Grow.Unit }]), ports),
            new Node.Box()))) {
            Span = Size(node),
            Role = Role.Card,
            Conditions = Selection.Holds(Key(node)) ? marks.TryAdd(Condition.Selected) : marks,
            Accessible = new Accessible { Label = Title(node) },
        };
    }

    private Node.Flex Labeled(TNode node, HashSet<Condition> marks, Sizing share) =>
        (Inputs: Inputs(node), Outputs: Outputs(node)) switch {
            var held => new(PhysicalAxis.Vertical, Seq<Node>(Header(node, marks) with { Height = share }) + toSeq(Range(0, Rows(node) - 1)).Map(index => (Node)new Node.Flex(PhysicalAxis.Horizontal, [
                Said(held.Inputs.At(index), TextAlignment.Left),
                Said(held.Outputs.At(index), TextAlignment.Right),
            ]) {
                Role = Role.ListRow,
                Height = share,
            })),
        };

    private Node.Flex Header(TNode node, HashSet<Condition> marks) =>
        new(PhysicalAxis.Horizontal,
            Glyph.Map(glyph => (Node)new Node.Glyph(glyph(node))).ToSeq()
            + Seq<Node>(new Node.Text([Title(node)]) { MaxLines = 1, Width = Sizing.Grow.Unit })
            + (marks.Contains(Condition.Busy) ? Seq<Node>(new Node.Box { Role = Role.Spinner, Conditions = [Condition.Busy] }) : [])
            + (marks.Intersect(Signals) switch { var shown => shown.IsEmpty ? [] : Seq<Node>(new Node.Box { Role = Role.Status, Conditions = shown }) })) {
            Role = Role.Header,
            Width = Sizing.Grow.Unit,
        };

    private static Node Said(Option<Port> port, TextAlignment alignment) =>
        port.Bind(static held => held.Label)
            .Map(label => (Node)new Node.Text([label]) { MaxLines = 1, Alignment = alignment, Width = Sizing.Grow.Unit })
            .IfNone(static () => new Node.Box { Width = Sizing.Grow.Unit });

    private Node.Pin Ported(Slot slot, Laid laid, Seq<Key> keys) =>
        new(slot.Unit, new Part.Choice([slot.Socket.Port], new Selection.One(None), edit => Focused(edit, slot.Socket.Node, keys)) {
            Hover = hovered => Hover.Map(report => report(hovered.Map(_ => slot.Socket))),
        }.Drawn(
            _ => new Node.Box {
                Role = Role.Port,
                Series = slot.Port.Type,
                Conditions = Hovered == Some(slot.Socket) ? [Condition.Highlighted] : [],
                Accessible = new Accessible { Label = slot.Port.Label },
            },
            members => new Node.Flex(PhysicalAxis.Horizontal, members) {
                Key = slot.Socket.Node,
                Lift = Wired.Map(_ => Lifted(slot, laid)),
                Drop = Wired.Map(wired => new Drop((transfer, _, _) =>
                    from source in Source(transfer)
                    from wires in Rewired(source, slot.Socket, laid)
                    select wired(Edit.Of(EditPhase.Commit, wires)))),
            })) {
            Align = UnitPoint.Center,
        };

    private IO<Unit> Focused(Edit<Selection> edit, Key node, Seq<Key> keys) =>
        Selected.Map(selected => selected(edit.Map(_ => Selection.Navigated(keys, node, extend: false)).As())).IfNone(unitIO);

    // --- [WIRING]
    private Option<Node> Connection(Wire wire, Laid laid) =>
        from start in laid.Anchors.Find(wire.From)
        from end in laid.Anchors.Find(wire.To)
        from port in laid.Outputs.Find(wire.From)
        select (Node)Line(start, end, port.Type, Seq(wire.From, wire.To).Exists(socket => Selection.Holds(socket.Node) || Hovered == Some(socket)) ? [Condition.Highlighted] : []);

    private Node.Mark Line(Vector2 start, Vector2 end, DataSeries type, HashSet<Condition> marks) =>
        new(new Figure.Path([start, end]) { Interpolation = Curve.IfNone(() => Flow.Map(horizontal: Interpolation.BumpX, vertical: Interpolation.BumpY)) }) {
            Role = Role.Wire,
            Series = type,
            Conditions = marks,
        };

    private Lift Lifted(Slot slot, Laid laid) =>
        new(ports => IO.pure<Transfer>(new Transfer.Keys(ports.Map(port => Seq(slot.Socket.Node, port))))) {
            Preview = laid.Anchors.Find(Carried(slot.Socket, laid).Head.Map(static wire => wire.From).IfNone(slot.Socket))
                .Map(start => (Func<Vector2, Node>)(pointer => Line(start, pointer, slot.Port.Type, []))),
        };

    private Seq<Wire> Carried(Socket socket, Laid laid) => laid.Inputs.ContainsKey(socket) ? Wires.Filter(wire => wire.To == socket) : [];

    private Option<Seq<Wire>> Rewired(Socket source, Socket target, Laid laid) {
        Seq<Wire> carried = Carried(source, laid);
        return from wire in carried.Head.Filter(_ => laid.Inputs.ContainsKey(target)).Map(held => new Wire(held.From, target))
                   | Some(laid.Outputs.ContainsKey(source) ? new Wire(source, target) : new Wire(target, source))
               from output in laid.Outputs.Find(wire.From)
               from input in laid.Inputs.Find(wire.To)
               where Accepts(output, input) && !Wires.Exists(held => held == wire)
               select Wires.Filter(held => !carried.Exists(moved => moved == held) && (input.Many || held.To != wire.To)).Add(wire);
    }

    private Option<IO<Unit>> Detached(Transfer transfer, Laid laid, Func<Edit<Seq<Wire>>, IO<Unit>> wired) =>
        from source in Source(transfer)
        let carried = Carried(source, laid)
        where !carried.IsEmpty
        select wired(Edit.Of(EditPhase.Commit, Wires.Filter(wire => !carried.Exists(held => held == wire))));

    private static Option<Socket> Source(Transfer transfer) =>
        from keys in Optional(transfer as Transfer.Keys)
        from path in keys.Paths.Head
        from node in path.Init.Last
        from port in path.Last
        select new Socket(node, port);

    // --- [LAYOUT]
    private Points<HashMap<Key, Vector2>> Positions(Seq<Key> keys) =>
        new(positions => keys.Choose(key => positions.Find(key).Map(at => new HandlePoint(key, at))),
            (positions, targets, _) => new Dragged<HashMap<Key, Vector2>>(
                Moved.IsSome ? targets.Fold(positions, static (held, target) => held.AddOrUpdate(target.Key, target.At)) : positions,
                targets.Map(static target => target.Key)));

    private HashMap<Key, Vector2> Layered(Seq<Key> keys) {
        HashMap<Key, int> ranks = Ranks(keys);
        HashMap<Key, Vector2> sizes = toHashMap(Nodes.Map(node => (Key(node), Size(node))));
        return keys.Map(key => (Key: key, Rank: ranks[key], Size: sizes[key])).Fold((Ends: HashMap<int, float>(), At: HashMap<Key, Vector2>()), (held, placed) =>
            held.Ends.Find(placed.Rank).IfNone(0f) switch {
                var start => (
                    held.Ends.AddOrUpdate(placed.Rank, start + Flow.Cross.Of(placed.Size) + Row.Y),
                    held.At.AddOrUpdate(placed.Key, Flow.Cross.With(
                        Flow.With(Vector2.Zero, Advanced(Flow, 2f * placed.Rank * Flow.Of(placed.Size), Flow.Of(placed.Size))),
                        Advanced(Flow.Cross, start, Flow.Cross.Of(placed.Size))))),
            }).At;
    }

    private HashMap<Key, int> Ranks(Seq<Key> keys) {
        HashSet<Key> known = toHashSet(keys);
        Seq<Wire> inner = Wires.Filter(wire => known.Contains(wire.From.Node) && known.Contains(wire.To.Node)).Strict();
        return keys.ToBidirectionalGraph(key => inner.Filter(wire => wire.From.Node == key).Map(wire => new SEquatableEdge<Key>(key, wire.To.Node)), allowParallelEdges: false).CondensateStronglyConnected<Key, SEquatableEdge<Key>, AdjacencyGraph<Key, SEquatableEdge<Key>>>() switch {
            var components => toSeq(components.SourceFirstBidirectionalTopologicalSort())
                .Fold(HashMap<AdjacencyGraph<Key, SEquatableEdge<Key>>, int>(), (ranks, component) =>
                    ranks.Add(component, toSeq(components.InEdges(component)).Fold(0, (rank, edge) => int.Max(rank, ranks[edge.Source] + 1))))
                .AsIterable()
                .Fold(HashMap<Key, int>(), static (ranks, entry) => toSeq(entry.Key.Vertices).Fold(ranks, (all, vertex) => all.Add(vertex, entry.Value))),
        };
    }

    private Laid Placed(HashMap<Key, Vector2> positions) =>
        Nodes.Fold(new Laid(HashMap<Key, SKRect>(), HashMap<Socket, Port>(), HashMap<Socket, Port>(), HashMap<Socket, Vector2>()), (laid, node) =>
            positions.Find(Key(node)).Map(at => Size(node) switch { var size => SKRect.Create(at.X, at.Y, size.X, size.Y) }).Match(
                Some: box => Slots(node).Fold(laid with { Boxes = laid.Boxes.AddOrUpdate(Key(node), box) }, (held, slot) => held with {
                    Inputs = slot.Output ? held.Inputs : held.Inputs.AddOrUpdate(slot.Socket, slot.Port),
                    Outputs = slot.Output ? held.Outputs.AddOrUpdate(slot.Socket, slot.Port) : held.Outputs,
                    Anchors = held.Anchors.AddOrUpdate(slot.Socket, Unplaced(box, slot.Unit)),
                }),
                None: () => laid));

    private Seq<Slot> Slots(TNode node) =>
        (Node: Key(node), Inputs: Inputs(node), Outputs: Outputs(node), Rows: Rows(node)) switch {
            var held => held.Inputs.Map((port, index) => Slotted(held.Node, port, output: false, index, held.Inputs.Count, held.Rows))
                + held.Outputs.Map((port, index) => Slotted(held.Node, port, output: true, index, held.Outputs.Count, held.Rows)),
        };

    private Slot Slotted(Key node, Port port, bool output, int index, int count, int rows) =>
        new(new Socket(node, port.Key), port, output, Flow.Cross.With(
            Flow.With(Vector2.Zero, output ? 1f : 0f),
            Flow == PhysicalAxis.Horizontal ? (index + 1.5f) / rows : (index + 0.5f) / count));

    private int Rows(TNode node) => Flow == PhysicalAxis.Horizontal ? 1 + int.Max(Inputs(node).Count, Outputs(node).Count) : 1;

    private Vector2 Size(TNode node) => Row with { Y = Row.Y * Rows(node) };

    private Option<PlotPlane> Fitted(Seq<SKRect> boxes) =>
        from content in Group.Union(boxes).Map(union => SKRect.Inflate(union, Row.Y, Row.Y))
        from across in View.Dimension(PhysicalAxis.Horizontal)
        from up in View.Dimension(PhysicalAxis.Vertical)
        let ratio = MathF.Abs(across.Domain.Span / up.Domain.Span)
        let width = float.Max(content.Width, content.Height * ratio)
        from x in Spanned(across.Domain, content.MidX, width)
        from y in Spanned(up.Domain, content.MidY, width / ratio)
        from fitted in View.WithDomain(PhysicalAxis.Horizontal, x)
        from plane in fitted.WithDomain(PhysicalAxis.Vertical, y)
        select plane;

    private static Option<AxisRange> Spanned(AxisRange domain, float middle, float span) =>
        (MathF.CopySign(span, domain.Span) / 2f) switch { var half => AxisRange.Validated(middle - half, middle + half).ToOption() };

    private Vector2 Unplaced(SKRect box, Vector2 fraction) =>
        new(Along(PhysicalAxis.Horizontal, box.Left, box.Right, fraction.X), Along(PhysicalAxis.Vertical, box.Top, box.Bottom, 1f - fraction.Y));

    private float Along(PhysicalAxis along, float low, float high, float fraction) => Rising(along) ? float.Lerp(low, high, fraction) : float.Lerp(high, low, fraction);

    private float Advanced(PhysicalAxis along, float start, float extent) => Rising(along) == (along == PhysicalAxis.Horizontal) ? start : -(start + extent);

    private bool Rising(PhysicalAxis along) => View.Dimension(along).Exists(static axis => axis.Domain.Span > 0f);

    private sealed record Slot(Socket Socket, Port Port, bool Output, Vector2 Unit);

    private sealed record Laid(HashMap<Key, SKRect> Boxes, HashMap<Socket, Port> Inputs, HashMap<Socket, Port> Outputs, HashMap<Socket, Vector2> Anchors);
}
