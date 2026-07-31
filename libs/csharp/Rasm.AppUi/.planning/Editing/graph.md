# [APPUI_EDITING_GRAPH]

The graph canvas is the typed-edit plane's node surface: NodeEditorAvalonia `IDrawingNode`/`DrawingNodeEditor` realize the parametric/dependency-graph canvas on ReactiveUI — node/pin/connector editing over a typed graph model, QuikGraph owning the connection-admission cycle gate and graph algebra, `LoroTree` as the co-edit data seam under ONE bidirectional projection with `EventTriggerKind` echo suppression, and canvas snapshots exporting through the capture encode fold. The page owns the graph model rows, the admission gate, the co-edit bridge, the typed `CanvasFault` rail, and the notebook dependency read projection. Recompute stays the AppHost `RecomputeGraph`'s — this canvas renders and edits structure, never re-solves.

## [01]-[INDEX]

- [02]-[GRAPH_MODEL]: Typed node/pin/connector rows on the ReactiveUI drawing model.
- [03]-[ADMISSION_GATE]: QuikGraph cycle gate and graph algebra; typed `CanvasFault`.
- [04]-[COEDIT_BRIDGE]: One bidirectional `LoroTree` projection with echo suppression.
- [05]-[PROJECTIONS]: Notebook dependency read projection; capture snapshot export.

## [02]-[GRAPH_MODEL]

- Owner: `GraphNodeRow` and `GraphPinRow` are the package-neutral model rows; `GraphEndpoint` and `GraphEdge` preserve node and pin identity; `GraphRouting` is the resolved placement-and-path policy row; `GraphModelAdapter` binds complete NodeEditorAvalonia model implementations and the graph serializer; `GraphCanvas` owns two-phase materialization over one `DrawingNodeEditor`.
- Entry: `Materialize(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges)` admits structure, stages every node and connector through `GraphModelAdapter`, and mutates the live drawing only in the success arm; `Reset` performs the same gate and staging before an atomic replacement; `Paste` stages, round-trips each node through the installed serializer's `Clone<T>`, and commits the clones; `Placed(string nodeKey, double x, double y)` mints the position op through the routing lattice.
- Auto: the composition adapter supplies complete `INode`, `IPin`, and `IConnector` implementations, including the package's permission and event surface, so this page never publishes a hollow interface implementation. `IDrawingNodeSettings` IS the one connection-policy authority and every column it owns is read: `GraphCanvas.Wired` reads direction and bus width and delegates final connectability to `DrawingNodeEditor.CanConnectPin`, `GraphAdmission` reads the connection-enable, self-connection, duplicate, and per-pin-fanout columns and imposes the stronger dependency-DAG invariant, and `GraphRouting.Of` lifts the snap, grid, guide, and nudge columns into the one row every position write and connector path reads — so the batch gate and the interactive drag cannot answer differently. Clone, paste, and duplication ride `INodeSerializer` through the editor's own `Clone<T>`, and node templates remain `INodeTemplate` rows on the editor host.
- Receipt: every committed structural edit seals an Edit-case `EvidenceReceipt` and projects a typed edit-intent op onto the `Collab/sync.md` durable stream — the graph mints no parallel op union.
- Packages: NodeEditorAvalonia (+`.Model` transitive-floor pin), ReactiveUI, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new node kind is one `GraphNodeRow` template value; a new pin shape is one `GraphPinRow` value; a retuned lattice or connector path is one `GraphRouting` column off the settings row; zero new surface.
- Boundary: connector routing and hit testing stay the package's `OrthogonalRouter`/`RTree`/`HitTestIndex` — `GraphRouting` carries the `ConnectorRoutingAlgorithm` and `ConnectorStyle` a render binds and re-implements neither; pan/zoom rides the package's OWN `NodeZoomBorder` — a distinct asset, NOT the admitted `PanAndZoom` package (which keeps its five page consumers); the disambiguation is RULED, no dup exists; the editor's `IUndoRedoHost` binds to the one `Editing/history.md` `EditHistory` — `Undo`/`Redo` delegate to the `history.undo`/`history.redo` intents and `BeginUndoBatch`/`EndUndoBatch` open and seal one `RevertDelta.Composite` op so a multi-op canvas gesture undoes as one unit, and the package's own coalesced-history surface therefore rides the one revert vocabulary, never a second undo stack; the canvas renders structure and routes recompute through the AppHost `RecomputeGraph` port exactly as the notebook does — a canvas-local topo/dirty engine is the deleted form.

```csharp signature
public sealed record GraphPinRow(string Key, string Name, PinAlignment Alignment, PinDirection Direction, int BusWidth);

public sealed record GraphNodeRow(
    string Key,
    string TemplateKey,
    string Title,
    double X,
    double Y,
    Seq<GraphPinRow> Pins);

public readonly record struct GraphEndpoint(string NodeKey, Option<string> PinKey);

public readonly record struct GraphEdge(GraphEndpoint From, GraphEndpoint To);

// The composition adapter owns complete NodeEditorAvalonia contract implementations. The graph page never
// publishes a partial INode, IPin, or IConnector implementation that omits package permissions or events.
public sealed record GraphModelAdapter(
    Func<GraphNodeRow, Fin<INode>> Node,
    Func<INode, Seq<IPin>> Pins,
    Func<IPin, string> PinKey,
    Func<IPin, PinDirection> Direction,
    Func<IPin, int> BusWidth,
    Func<IPin, IPin, Fin<IConnector>> Connect,
    INodeSerializer Serializer);

// Placement and connector-path policy as DATA off the one settings authority: the nine snap, grid, guide,
// and nudge columns travel beside the two routing enums as one resolved row, so a position write commits a
// SNAPPED coordinate and a connector renders under a declared algorithm and style rather than each write
// and each render inventing its own. `Of` reads the settings row instead of taking loose knobs, so the
// admission gate, the interactive drag, and the batch position write answer from one authority — a
// caller-supplied grid pitch beside that row is the deleted form, and so is a raw coordinate reaching the
// position op, which lands a canvas on a lattice the drag would have quantized away and makes two peers
// converge to two positions for one gesture.
public sealed record GraphRouting(
    ConnectorRoutingAlgorithm Algorithm,
    ConnectorStyle Style,
    bool Snap,
    double SnapX,
    double SnapY,
    bool Grid,
    double GridCellWidth,
    double GridCellHeight,
    bool Guides,
    double NudgeStep,
    double NudgeMultiplier) {
    public static GraphRouting Of(IDrawingNodeSettings policy, ConnectorRoutingAlgorithm algorithm, ConnectorStyle style) =>
        new(algorithm, style,
            policy.EnableSnap, policy.SnapX, policy.SnapY,
            policy.EnableGrid, policy.GridCellWidth, policy.GridCellHeight,
            policy.EnableGuides, policy.NudgeStep, policy.NudgeMultiplier);

    // The ONE position projection every write crosses, composing the package's own lattice —
    // `NodeEditor.SnapHelper.Snap(double, double)` (decompile-proven) — so the write quantizes on the
    // identical rounding the interactive drag uses and the two cannot converge to two positions for one
    // gesture. A snap-disabled or degenerate-pitch policy answers the raw coordinate, so the lattice is
    // policy data rather than a branch at each write site.
    public (double X, double Y) Place(double x, double y) =>
        Snap && SnapX > 0d && SnapY > 0d
            ? (SnapHelper.Snap(x, SnapX), SnapHelper.Snap(y, SnapY))
            : (x, y);
}

public sealed record GraphCanvas(
    DrawingNodeEditor Editor,
    IDrawingNode Drawing,
    IDrawingNodeSettings Policy,
    GraphAdmission Gate,
    GraphModelAdapter Model,
    GraphRouting Routing) {
    public Fin<IDrawingNode> Materialize(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        Gate.Admit(nodes, edges)
            .Bind(_ => Staged(nodes, edges))
            .Map(Commit);

    // Duplication and clipboard paste ride the package's OWN round-trip: `DrawingNodeEditor.Clone<T>` runs
    // each staged node through the installed `INodeSerializer`, so a pasted subgraph carries every
    // permission and event the adapter minted where a re-minted row copy would fork node identity from the
    // peer receiving the op. The clone re-enters the SAME gate, so a paste that would close a cycle refuses
    // exactly as an interactive drag does.
    public Fin<IDrawingNode> Paste(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        Gate.Admit(nodes, edges)
            .Bind(_ => Staged(nodes, edges))
            .Bind(staged => staged.Nodes
                .TraverseM(node => Optional(Editor.Clone(node))
                    .ToFin((Error)new CanvasFault.ModelRejected("serializer round-trip refused a staged node")))
                .As()
                .Map(cloned => Commit((cloned.Strict(), staged.Wires))));

    // Every position write crosses the routing lattice, so a co-edited coordinate converges to the same
    // grid on every peer and the batch write cannot land a position the drag would have quantized.
    public GraphOp Placed(string nodeKey, double x, double y) =>
        Routing.Place(x, y) switch { var at => new GraphOp.NodeAt(nodeKey, at.X, at.Y) };

    // Reconcile entry for remote applies: the swap is atomic — the gate and the staged validation admit the
    // replacement FIRST, the live canvas clears only inside the success arm, so a rejected apply leaves the
    // graph intact.
    public Fin<IDrawingNode> Reset(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        Gate.Admit(nodes, edges)
            .Bind(_ => Staged(nodes, edges))
            .Map(staged => {
                Drawing.Nodes?.Clear();
                Drawing.Connectors?.Clear();
                return Commit(staged);
            });

    // Two-phase apply: every node, pin, and connector mints DETACHED through the factory and validates
    // against the settings policy BEFORE the first Drawing mutation.
    Fin<(Seq<INode> Nodes, Seq<IConnector> Wires)> Staged(Seq<GraphNodeRow> rows, Seq<GraphEdge> edges) =>
        from materialized in rows.TraverseM(row => Model.Node(row).Map(node => (row.Key, Node: node))).As()
        let byKey = materialized.Fold(Map<string, INode>(), static (index, row) => index.Add(row.Key, row.Node))
        from wires in edges.TraverseM(edge => Wired(byKey, edge)).As()
        select (toSeq(byKey.Values), wires.Strict());

    // The serializer installs at the one mutation edge, so clipboard, duplication, and the editor's own
    // graph persistence all round-trip through the adapter's instance rather than whichever the host last
    // set — `Clone<T>` is silently null-returning against an unset serializer.
    IDrawingNode Commit((Seq<INode> Nodes, Seq<IConnector> Wires) staged) {
        Drawing.SetSerializer(Model.Serializer);
        staged.Nodes.Iter(node => Drawing.Nodes?.Add(node));
        staged.Wires.Iter(wire => Drawing.Connectors?.Add(wire));
        return Drawing;
    }

    // The settings row is the one policy authority: self-connection and bus-width checks read its columns,
    // and CanConnectPin is the editor's own connectability gate over the same settings.
    Fin<IConnector> Wired(Map<string, INode> byKey, GraphEdge edge) =>
        from start in Endpoint(byKey, edge.From, RequiredDirection(PinDirection.Output)).ToFin(new CanvasFault.EndpointUnknown(edge.From.ToString()))
        from end in Endpoint(byKey, edge.To, RequiredDirection(PinDirection.Input)).ToFin(new CanvasFault.EndpointUnknown(edge.To.ToString()))
        from _bus in !Policy.RequireMatchingBusWidth || Model.BusWidth(start) == Model.BusWidth(end)
            ? Fin.Succ(unit) : Fin.Fail<Unit>(new CanvasFault.PolicyRejected($"bus width {edge.From} -> {edge.To}"))
        from _gate in Editor.CanConnectPin(start) && Editor.CanConnectPin(end)
            ? Fin.Succ(unit) : Fin.Fail<Unit>(new CanvasFault.PolicyRejected($"{edge.From} -> {edge.To}"))
        from wire in Model.Connect(start, end)
        select wire;

    // Endpoint grammar (GraphAdmission owns it): `nodeKey` or `nodeKey/pinKey` — a pin-qualified endpoint
    // routes to its named pin so pin identity survives end-to-end; an unqualified endpoint routes by
    // direction (first Output on source, first Input on target).
    Option<IPin> Endpoint(Map<string, INode> byKey, GraphEndpoint endpoint, Option<PinDirection> direction) =>
        byKey.Find(endpoint.NodeKey).Bind(node => Model.Pins(node).Find(pin =>
            direction.Match(Some: admitted => Model.Direction(pin) == admitted, None: static () => true) && endpoint.PinKey.Match(
                Some: key => Model.PinKey(pin) == key,
                None: () => true)));

    Option<PinDirection> RequiredDirection(PinDirection direction) =>
        Policy.RequireDirectionalConnections ? Some(direction) : Option<PinDirection>.None;
}
```

## [03]-[ADMISSION_GATE]

- Owner: `CanvasFault` — the typed canvas rail; `GraphAdmission` — the QuikGraph-backed connection-admission gate whose policy column IS the editor `IDrawingNodeSettings` row, never a parallel policy source.
- Entry: `Admit(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges)` rejects invalid node or pin identities, non-finite positions, non-positive bus widths, dangling endpoints, disabled connections, self connections, policy-disallowed duplicate edges, policy-disallowed pin fanout, and cycles before the editor mutates; `Order` returns topological node keys from the same admitted graph value and is the `[05]` dependency projection's row order.
- Auto: every structural and policy clause is one row on an ordered guard seq folded to first refusal, so a new rule is a row rather than a deeper arm and each refusal names itself; the edge set then folds into a QuikGraph `AdjacencyGraph<string, SEdge<string>>` where `IsDirectedAcyclicGraph` is the cycle oracle and `TopologicalSort` reads off the SAME graph value through `Order`, so the notebook dependency projection and any solve-order consumer read one composed fold — a hand-rolled adjacency list or DFS beside QuikGraph is the deleted form. The policy clauses read `EnableConnections`, `AllowSelfConnections`, `AllowDuplicateConnections`, and `EnableMultiplePinConnections` directly, so the gate and the interactive connector-drag answer from one settings row instead of the batch path admitting a wiring the drag rejects.
- Packages: QuikGraph (shared tier), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new admission rule is one gate clause folding on the same graph value; one `CanvasFault` case is one `detail` ordinal under the `AppUiFaultBand.Canvas` row (6330); zero new surface.
- Boundary: the gate guards STRUCTURE only — recompute scheduling, dirty propagation, and evaluation stay the AppHost `RecomputeGraph`'s (a second incremental-recompute owner is the deleted form); every fault derives through the `AppUiFaultBand.Canvas` registry row.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CanvasFault : Expected {
    private CanvasFault(string detail, int code) : base(detail, code) { }
    public sealed record CycleRejected(int Nodes, int Edges)
        : CanvasFault($"graph/cycle: {Nodes} nodes and {Edges} edges are not acyclic", AppUiFaultBand.Canvas.Code(0));
    public sealed record EndpointUnknown(string Key)
        : CanvasFault($"graph/endpoint: {Key} is not admitted", AppUiFaultBand.Canvas.Code(1));
    public sealed record PolicyRejected(string Detail)
        : CanvasFault($"graph/policy: {Detail}", AppUiFaultBand.Canvas.Code(2));
    // Ordinal 3 was the echo refusal. Echo suppression is a DROP, not a fault — the latch fires on every
    // correct remote apply — so the ordinal now carries the one diff-side case that IS a fault: a trigger
    // the bridge does not admit, which previously flattened into the generic model refusal.
    public sealed record TriggerUnsupported(string Trigger)
        : CanvasFault($"graph/trigger: {Trigger} is not an admitted diff trigger", AppUiFaultBand.Canvas.Code(3));
    public sealed record ModelRejected(string Detail)
        : CanvasFault($"graph/model: {Detail}", AppUiFaultBand.Canvas.Code(4));
}

public sealed record GraphAdmission(IDrawingNodeSettings Policy) {
    public Fin<Unit> Admit(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        Admitted(nodes, edges).Map(static _ => unit);

    private Fin<AdjacencyGraph<string, SEdge<string>>> Admitted(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        Refused(nodes, edges).Match(
            Some: Fin.Fail<AdjacencyGraph<string, SEdge<string>>>,
            None: () => Graph(nodes.Map(static node => node.Key), edges) is { } graph && graph.IsDirectedAcyclicGraph()
                ? Fin.Succ(graph)
                : Fin.Fail<AdjacencyGraph<string, SEdge<string>>>(new CanvasFault.CycleRejected(nodes.Count, edges.Count)));

    // Every structural and policy clause is a ROW on one ordered guard seq evaluated to first refusal, so
    // a new admission rule is a row rather than another arm on a nesting ladder that already ran four
    // levels deep. The cycle oracle stays below the seq because it alone PRODUCES the admitted graph value
    // every other clause only inspects. Every connection column the settings authority declares is read on
    // this seq or in `Wired`: with `EnableConnections` and `EnableMultiplePinConnections` unread, a batch
    // `Materialize` admitted a wired canvas and a pin fanout the interactive drag rejects at the SAME
    // settings row — the one-authority claim falsified by the gate that makes it.
    Option<Error> Refused(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        Seq<Func<Option<Error>>>(
            () => Identities(nodes),
            () => Endpoints(nodes, edges),
            () => !Policy.EnableConnections && !edges.IsEmpty
                ? Some<Error>(new CanvasFault.PolicyRejected("connections are disabled"))
                : None,
            () => !Policy.AllowSelfConnections && edges.Exists(static edge => edge.From.NodeKey == edge.To.NodeKey)
                ? Some<Error>(new CanvasFault.PolicyRejected("self connection"))
                : None,
            () => Policy.AllowDuplicateConnections
                ? None
                : Duplicate(edges).Map(static dup => (Error)new CanvasFault.PolicyRejected($"duplicate edge {dup.From} -> {dup.To}")),
            () => Policy.EnableMultiplePinConnections
                ? None
                : Fanned(edges).Map(static pin => (Error)new CanvasFault.PolicyRejected($"pin fanout {pin}")))
        .Fold(Option<Error>.None, static (held, clause) => held.IsSome ? held : clause());

    static Option<Error> Identities(Seq<GraphNodeRow> nodes) =>
        toHashSet(nodes.Map(static node => node.Key)).Count == nodes.Count
        && nodes.ForAll(static node =>
            !string.IsNullOrWhiteSpace(node.Key)
            && !string.IsNullOrWhiteSpace(node.TemplateKey)
            && !string.IsNullOrWhiteSpace(node.Title)
            && double.IsFinite(node.X)
            && double.IsFinite(node.Y)
            && node.Pins.Map(static pin => pin.Key).Distinct().Count == node.Pins.Count
            && node.Pins.ForAll(static pin => !string.IsNullOrWhiteSpace(pin.Key)
                && !string.IsNullOrWhiteSpace(pin.Name)
                && pin.BusWidth > 0))
            ? None
            : Some<Error>(new CanvasFault.ModelRejected("node keys, pin keys, and bus widths must be admitted"));

    Option<Error> Endpoints(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        edges.Find(edge => !EndpointKnown(nodes, edge.From, PinDirection.Output) || !EndpointKnown(nodes, edge.To, PinDirection.Input))
            .Map(edge => (Error)new CanvasFault.EndpointUnknown(
                !EndpointKnown(nodes, edge.From, PinDirection.Output) ? edge.From.ToString() : edge.To.ToString()));

    // Fanout counts BOTH endpoints, because a pin is as multiply-connected on the source side as on the
    // target side, and keys on the pin-qualified endpoint so two parallel pins on one node stay distinct.
    static Option<GraphEndpoint> Fanned(Seq<GraphEdge> edges) =>
        Optional((edges.Map(static edge => edge.From) + edges.Map(static edge => edge.To))
                .GroupBy(static endpoint => endpoint)
                .FirstOrDefault(static group => group.Count() > 1))
            .Map(static group => group.Key);

    bool EndpointKnown(Seq<GraphNodeRow> nodes, GraphEndpoint endpoint, PinDirection expected) =>
        nodes.Find(node => StringComparer.Ordinal.Equals(node.Key, endpoint.NodeKey)).Exists(node =>
            endpoint.PinKey.Match(
                Some: pinKey => node.Pins.Exists(pin => StringComparer.Ordinal.Equals(pin.Key, pinKey)
                    && (!Policy.RequireDirectionalConnections || pin.Direction == expected)),
                None: () => node.Pins.Exists(pin => !Policy.RequireDirectionalConnections || pin.Direction == expected)));

    // Policy column honored as data: the duplicate gate fires only when the settings row disallows repeats;
    // duplicates key on the full pin-qualified endpoint pair, so parallel pins stay distinct edges.
    static Option<GraphEdge> Duplicate(Seq<GraphEdge> edges) =>
        Optional(edges.GroupBy(static edge => edge).FirstOrDefault(static group => group.Count() > 1))
            .Map(static group => group.Key);

    // Evaluation order off the SAME graph value the cycle oracle reads — one fold, two projections.
    public Fin<Seq<string>> Order(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        Admitted(nodes, edges).Map(static graph => toSeq(graph.TopologicalSort()));

    static AdjacencyGraph<string, SEdge<string>> Graph(Seq<string> nodes, Seq<GraphEdge> edges) {
        AdjacencyGraph<string, SEdge<string>> graph = new(allowParallelEdges: true);
        nodes.Iter(node => graph.AddVertex(node));
        edges.Iter(edge => graph.AddEdge(new SEdge<string>(edge.From.NodeKey, edge.To.NodeKey)));
        return graph;
    }
}
```

## [04]-[COEDIT_BRIDGE]

- Owner: `GraphCoEdit` — the ONE bidirectional projection between the ReactiveUI graph model and the `Collab/sync.md` `LoroTree` container, carrying BOTH directions: `CommitLocal` outbound, the subscription sink inbound.
- Entry: `public IO<Fin<Unit>> CommitLocal(CollabDoc doc, string docKey, Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges, GraphOp op, string origin)` — the outbound direction re-admits the post-op topology before the intent rides `IntentLedger.Commit`; `public IO<Fin<Subscription>> Bind(CollabDoc doc, GraphCanvas canvas)` — the inbound direction holds one subscription per canvas and applies remote diffs through the same typed rows.
- Auto: the graph structure maps onto `LoroTree` — a node is a tree node whose meta map carries the `GraphNodeRow` columns, an edge is a child row on the connection register — and subscriber diffs discriminate `EventTriggerKind.Local`/`Import`/`Checkout` for ECHO SUPPRESSION: a local `CommitLocal` mutation arrives back as its own `Local` diff and is dropped, a mutation raised BY a remote apply commits nothing back, and a remote `Import` diff applies to the ReactiveUI graph model WITHOUT re-emitting — the feedback loop is the named deleted form. Suppression is a DROP on both sides and never a fault, because a fault on the routine convergence path surfaces correct behaviour to the presence UI as an error; the one diff-side fault is `CanvasFault.TriggerUnsupported`, raised for a trigger the bridge does not admit. A hierarchy move rides the sync-owned `GraphOp.NodeMove(NodeId, Parent, Index)` case onto the tree's identity-preserving `MovTo`, and a canvas x/y position write commits as the sync-owned `GraphOp.NodeAt(NodeId, X, Y)` meta-column write minted through `GraphCanvas.Placed` so the committed coordinate is the routing lattice's, riding the same commit leg through the graph arm and never a side channel.
- Receipt: durable truth rides `Collab/sync.md`'s typed edit-intent stream (a graph structural op is one row on the single edit-intent union); the live half rides the session-ephemeral Loro wire — this page persists nothing.
- Packages: LoroCs, ReactiveUI, LanguageExt.Core
- Growth: a new co-edited column is one meta-map key; a new structural verb is one `GraphOp` case landed at the sync owner; zero new surface.
- Boundary: the bridge is the ONE projection and this owner covers both declared directions — a second subscription path, a canvas-local `LoroTree` mutation beside `IntentApply.Apply`, a model-poll loop, or a per-node sync channel is the deleted form; remote-applied diffs re-run the `GraphAdmission` gate, and a cycle-closing edit surfaces as a typed conflict row for the presence UI; `Open`, `Subscribe`, `ReadNodes`, and `ReadEdges` are the composition adapters over the verified container values, so no unverified internal attach or read member enters the page.

```csharp signature
public sealed record GraphCoEdit(
    GraphAdmission Gate,
    Func<CollabDoc, Fin<(LoroTree Tree, LoroMap Edges)>> Open,
    Func<LoroTree, Subscriber, Fin<Subscription>> Subscribe,
    Func<LoroTree, Fin<Seq<GraphNodeRow>>> ReadNodes,
    Func<LoroMap, Fin<Seq<GraphEdge>>> ReadEdges) {
    int applying; // re-entrancy latch: a model mutation raised BY a remote apply emits no tree op back

    // The typed conflict row the presence UI observes: a remote apply the gate rejects lands HERE.
    public Atom<Option<Error>> Conflict { get; } = Atom(Option<Error>.None);

    // Outbound: gate the POST-op topology, then ride the ONE transaction rail — durable first, live tree
    // apply through the same IntentApply.Apply arm replay uses; the resulting Local diff is echo-dropped.
    // Under the latch the commit is a NO-OP, not a refusal: a model mutation raised by a remote apply is
    // the expected, correct case the suppression law names a DROP, and returning a fault surfaced routine
    // convergence to the presence UI as an error. The genuine diff-side fault is the unsupported trigger,
    // which the sink routes to `Conflict` on its own arm.
    public IO<Fin<Unit>> CommitLocal(CollabDoc doc, string docKey, Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges, GraphOp op, string origin) =>
        Volatile.Read(ref applying) == 1
            ? IO.pure(Fin.Succ(unit))
            : string.IsNullOrWhiteSpace(docKey) || string.IsNullOrWhiteSpace(origin)
                ? IO.pure(Fin.Fail<Unit>(new CanvasFault.ModelRejected("document key and origin are required")))
                : Gate.Admit(nodes, edges).Match(
                Succ: _ => IntentLedger.Commit(doc, new EditIntent.GraphStructure(docKey, op), origin),
                Fail: error => IO.pure(Fin.Fail<Unit>(error)));

    public IO<Fin<Subscription>> Bind(CollabDoc doc, GraphCanvas canvas) =>
        IO.lift(() =>
            from containers in Open(doc)
            from live in Subscribe(containers.Tree, new TreeSink(this, canvas, containers.Tree, containers.Edges))
            select live);

    sealed record TreeSink(GraphCoEdit Owner, GraphCanvas Canvas, LoroTree Tree, LoroMap Edges) : Subscriber {
        public void OnDiff(DiffEvent diff) => ignore(diff.TriggeredBy switch {
            EventTriggerKind.Local => unit,
            EventTriggerKind.Import or EventTriggerKind.Checkout => Owner.ApplyRemote(Canvas, Tree, Edges),
            _ => fun(() => Owner.Conflict.Swap(_ => Some<Error>(new CanvasFault.TriggerUnsupported($"{diff.TriggeredBy}"))))(),
        });
    }

    // Remote apply is a state reconcile over the verified LoroTree/LoroMap read surface: tree nodes + meta
    // columns re-project to rows, the edge register re-projects to pairs, and the canvas rebuilds through
    // the ONE gate-checked Materialize fold — a remote edit that would close a cycle surfaces as the typed
    // CanvasFault conflict row for the presence UI, never a silent apply.
    Unit ApplyRemote(GraphCanvas canvas, LoroTree tree, LoroMap edges) {
        if (Interlocked.CompareExchange(ref applying, 1, 0) == 1) return unit;
        try {
            Fin<Unit> applied =
                from rows in ReadNodes(tree)
                from pairs in ReadEdges(edges)
                from _ in canvas.Reset(rows, pairs)
                select unit;
            applied.Match(
                Succ: _ => Conflict.Swap(_ => None),
                Fail: error => Conflict.Swap(_ => Some(error)));
            return unit;
        }
        finally { Interlocked.Exchange(ref applying, 0); }
    }
}
```

## [05]-[PROJECTIONS]

- Owner: `GraphProjection` — the read-projection fold family.
- Entry: `public static Fin<Seq<GraphNodeRow>> FromDependencies(GraphAdmission gate, RecomputeGraph.Graph graph, Map<string, (double X, double Y)> layout, string templateKey)` — the notebook cell-dependency graph renders as a READ projection onto canvas rows in topological order; edits on this projection are disabled by SHAPE (a dependency edge derives from a node's own recorded inputs, never a hand-drawn connector).
- Auto: the fold reads the port's own node map — each node's `Hex` is the canvas key, its `Descriptor` the title, its `Inputs` the incoming edges — and composes `GraphAdmission.Order`, so the rendered row order and any solve-order consumer read ONE topological fold off the same acyclicity oracle the edit gate reads and `Order` finally has the reader its declaration names. Canvas snapshot export rides the capture capsule — the editor surface renders through the capture in-tree lane and encodes through `VisualCodec` as kind graph, so a canvas baseline joins the render-hash proof lanes; PNG/SVG/PDF export of the canvas composes `Document/export.md`'s destination union with the capture raster or the package `ExportRenderer` vector arm as the source.
- Packages: NodeEditorAvalonia, Rasm.AppHost (project), QuikGraph (shared tier), SkiaSharp, LanguageExt.Core
- Growth: a new read projection (Compute solve graph, Fabrication posting chain) is one `From*` fold returning rows; zero new surface.
- Boundary: the dependency projection READS the AppHost `RecomputeGraph` vocabulary through the declared port (decode-only) — the same `[V11]` law the notebook lands; a projection-local dependency model, a second topological sort beside `GraphAdmission.Order`, and a layout fallback that DROPS an unplaced node are the deleted forms, the last because an omitted dependency reads as a graph that is not there. The projection's gate carries a settings row admitting pin fanout, since one recompute output legitimately feeds many nodes; a gate configured for single-pin connections refuses the whole projection by name rather than rendering a partial graph.

```csharp signature
public static class GraphProjection {
    // Canonical pin pair for a dependency node: all inputs converge on one port and the output fans out,
    // which is the shape a recompute node HAS — a pinless row would fail the admission gate's own endpoint
    // check, and per-input pins would invent structure the port never recorded.
    public const string InPin = "in";
    public const string OutPin = "out";

    public static Fin<Seq<GraphNodeRow>> FromDependencies(
        GraphAdmission gate, RecomputeGraph.Graph graph, Map<string, (double X, double Y)> layout, string templateKey) =>
        Ordered(gate, toSeq(graph.Nodes.Values), layout, templateKey);

    static Fin<Seq<GraphNodeRow>> Ordered(
        GraphAdmission gate, Seq<RecomputeNode> nodes, Map<string, (double X, double Y)> layout, string templateKey) {
        Seq<GraphNodeRow> rows = nodes.Map(node => Row(node, layout, templateKey)).Strict();
        Seq<GraphEdge> edges = nodes.Bind(static node => node.Inputs.Map(input => new GraphEdge(
            new GraphEndpoint(input.Hex, Some(OutPin)), new GraphEndpoint(node.Hash.Hex, Some(InPin))))).Strict();
        return gate.Order(rows, edges).Map(order => Ranked(rows, order));
    }

    // A node the layout does not place lands at the origin rather than dropping: an unplaced dependency is
    // still structure the reader must see.
    static GraphNodeRow Row(RecomputeNode node, Map<string, (double X, double Y)> layout, string templateKey) =>
        layout.Find(node.Hash.Hex).IfNone((X: 0d, Y: 0d)) switch {
            var at => new GraphNodeRow(node.Hash.Hex, templateKey, node.Descriptor, at.X, at.Y, Seq(
                new GraphPinRow(InPin, InPin, PinAlignment.Left, PinDirection.Input, BusWidth: 1),
                new GraphPinRow(OutPin, OutPin, PinAlignment.Right, PinDirection.Output, BusWidth: 1))),
        };

    // The topological order IS the projection's row order, so a reader scans dependencies in solve order
    // and no consumer re-sorts; a key the order omits cannot exist, because both come from one gate call.
    static Seq<GraphNodeRow> Ranked(Seq<GraphNodeRow> rows, Seq<string> order) =>
        rows.Fold(Map<string, GraphNodeRow>(), static (index, row) => index.Add(row.Key, row)) switch {
            var index => order.Choose(index.Find),
        };
}
```

## [06]-[RESEARCH]

(none)
