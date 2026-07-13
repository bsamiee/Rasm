# [APPUI_EDITING_GRAPH]

The graph canvas is the typed-edit plane's node surface: NodeEditorAvalonia `IDrawingNode`/`DrawingNodeEditor` realize the parametric/dependency-graph canvas on ReactiveUI — node/pin/connector editing over a typed graph model, QuikGraph owning the connection-admission cycle gate and graph algebra, `LoroTree` as the co-edit data seam under ONE bidirectional projection with `EventTriggerKind` echo suppression, and canvas snapshots exporting through the capture encode fold. The page owns the graph model rows, the admission gate, the co-edit bridge, the typed `CanvasFault` rail, and the notebook dependency read projection. Recompute stays the AppHost `RecomputeGraph`'s — this canvas renders and edits structure, never re-solves.

## [01]-[INDEX]

- [02]-[GRAPH_MODEL]: Typed node/pin/connector rows on the ReactiveUI drawing model.
- [03]-[ADMISSION_GATE]: QuikGraph cycle gate and graph algebra; typed `CanvasFault`.
- [04]-[COEDIT_BRIDGE]: One bidirectional `LoroTree` projection with echo suppression.
- [05]-[PROJECTIONS]: Notebook dependency read projection; capture snapshot export.

## [02]-[GRAPH_MODEL]

- Owner: `GraphNodeRow` — the typed node row the drawing-node view-model materializes from; `GraphPinRow` — the typed pin row; `GraphCanvas` — the one canvas owner over `DrawingNodeEditor` carrying the `IDrawingNodeFactory` every pin and connector mints through.
- Entry: `public Fin<IDrawingNode> Materialize(Seq<GraphNodeRow> nodes, Seq<(string From, string To)> edges)` — one two-phase fold from typed rows to the mounted drawing: stage detached through the factory, validate every edge against the settings policy, commit only in the success arm; edge endpoints carry the gate-owned grammar `nodeKey` or `nodeKey/pinKey` so pin identity survives from row to connector; every edit verb (add node, connect, move, delete) discriminates through the editor's own operation surface — `Drawing.Nodes`/`Connectors` mutate only from the staged commit, never mid-validation.
- Auto: product view-models implement `IDrawingNode`/`INode`/`IConnector`/`IPin`/`IConnectablePin` on ReactiveUI so activation, command, and validation ride the suite's one reactive rail; `IDrawingNodeSettings` is the ONE connection-policy authority — `RequireDirectionalConnections`, `RequireMatchingBusWidth`, `AllowSelfConnections`, `AllowDuplicateConnections` are its data columns, the interactive connector-drag honors them through `DrawingNodeEditor.CanConnectPin`/`ConnectionValidationContext`, and the programmatic gate reads the same row so no second policy source exists; pins and connectors mint through `IDrawingNodeFactory.CreatePin()`/`CreateConnector()` so the engine sees every construction; node templates are `INodeTemplate` rows on the editor's `Templates` host so a new node kind is one template row.
- Receipt: every committed structural edit seals an Edit-case `EvidenceReceipt` and projects a typed edit-intent op onto the `Collab/sync.md` durable stream — the graph mints no parallel op union.
- Packages: NodeEditorAvalonia (+`.Model` transitive-floor pin), ReactiveUI, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new node kind is one `GraphNodeRow` template value; a new pin shape is one `GraphPinRow` value; zero new surface.
- Boundary: pan/zoom rides the package's OWN `NodeZoomBorder` — a distinct asset, NOT the admitted `PanAndZoom` package (which keeps its five page consumers); the disambiguation is RULED, no dup exists; the editor's `IUndoRedoHost` binds to the one `Editing/history.md` `EditHistory` — `Undo`/`Redo` delegate to the `history.undo`/`history.redo` intents and `BeginUndoBatch`/`EndUndoBatch` open and seal one `RevertDelta.Composite` op so a multi-op canvas gesture undoes as one unit, and the package's own coalesced-history surface therefore rides the one revert vocabulary, never a second undo stack; the canvas renders structure and routes recompute through the AppHost `RecomputeGraph` port exactly as the notebook does — a canvas-local topo/dirty engine is the deleted form.

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
    Func<IPin, IPin, Fin<IConnector>> Connect);

public sealed record GraphCanvas(DrawingNodeEditor Editor, IDrawingNode Drawing, IDrawingNodeSettings Policy, GraphAdmission Gate, GraphModelAdapter Model) {
    public Fin<IDrawingNode> Materialize(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        Gate.Admit(nodes, edges)
            .Bind(_ => Staged(nodes, edges))
            .Map(Commit);

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
        rows.TraverseM(row => Model.Node(row).Map(node => (row.Key, Node: node))).As()
            .Bind(materialized => materialized.Fold(Map<string, INode>(), static (index, row) => index.Add(row.Key, row.Node)) switch {
                Map<string, INode> byKey => edges.TraverseM(edge => Wired(byKey, edge)).As()
                    .Map(wires => (toSeq(byKey.Values), wires.Strict())),
            });

    IDrawingNode Commit((Seq<INode> Nodes, Seq<IConnector> Wires) staged) {
        staged.Nodes.Iter(node => Drawing.Nodes?.Add(node));
        staged.Wires.Iter(wire => Drawing.Connectors?.Add(wire));
        return Drawing;
    }

    // The settings row is the one policy authority: self-connection and bus-width checks read its columns,
    // and CanConnectPin is the editor's own connectability gate over the same settings.
    Fin<IConnector> Wired(Map<string, INode> byKey, GraphEdge edge) =>
        from start in Endpoint(byKey, edge.From, PinDirection.Output).ToFin(new CanvasFault.EndpointUnknown(edge.From.NodeKey))
        from end in Endpoint(byKey, edge.To, PinDirection.Input).ToFin(new CanvasFault.EndpointUnknown(edge.To.NodeKey))
        from _bus in !Policy.RequireMatchingBusWidth || Model.BusWidth(start) == Model.BusWidth(end)
            ? Fin.Succ(unit) : Fin.Fail<Unit>(new CanvasFault.PolicyRejected($"bus width {edge.From} -> {edge.To}"))
        from _gate in Editor.CanConnectPin(start) && Editor.CanConnectPin(end)
            ? Fin.Succ(unit) : Fin.Fail<Unit>(new CanvasFault.PolicyRejected($"{edge.From} -> {edge.To}"))
        from wire in Model.Connect(start, end)
        select wire;

    // Endpoint grammar (GraphAdmission owns it): `nodeKey` or `nodeKey/pinKey` — a pin-qualified endpoint
    // routes to its named pin so pin identity survives end-to-end; an unqualified endpoint routes by
    // direction (first Output on source, first Input on target).
    Option<IPin> Endpoint(Map<string, INode> byKey, GraphEndpoint endpoint, PinDirection direction) =>
        byKey.Find(endpoint.NodeKey).Bind(node => Model.Pins(node).Find(pin =>
            Model.Direction(pin) == direction && endpoint.PinKey.Match(
                Some: key => Model.PinKey(pin) == key,
                None: () => true)));
}
```

## [03]-[ADMISSION_GATE]

- Owner: `CanvasFault` — the typed canvas rail; `GraphAdmission` — the QuikGraph-backed connection-admission gate whose policy column IS the editor `IDrawingNodeSettings` row, never a parallel policy source.
- Entry: `public Fin<Unit> Admit(Seq<string> nodes, Seq<(string From, string To)> edges)` — the gate rejects a cycle, a dangling endpoint, or a policy-violating duplicate before the editor mutates; `public static Fin<Seq<string>> Order(Seq<string> nodes, Seq<(string From, string To)> edges)` — the evaluation-order projection downstream reads.
- Auto: the edge set folds into a QuikGraph `AdjacencyGraph<string, SEdge<string>>` and `IsDirectedAcyclicGraph` is the cycle oracle; topological order reads `TopologicalSort` off the SAME graph value through `Order`, so the notebook dependency projection and any solve-order consumer read one composed fold — a hand-rolled adjacency list or DFS beside QuikGraph is the deleted form; duplicate admission reads `IDrawingNodeSettings.AllowDuplicateConnections` so the gate and the interactive connector-drag answer from one settings row.
- Packages: QuikGraph (shared tier), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new admission rule is one gate clause folding on the same graph value; one `CanvasFault` case is one `detail` ordinal under the `AppUiFaultBand.Canvas` row (6330); zero new surface.
- Boundary: the gate guards STRUCTURE only — recompute scheduling, dirty propagation, and evaluation stay the AppHost `RecomputeGraph`'s (a second incremental-recompute owner is the deleted form, the same law `Document/notebook.md` lands under `[V11]`); every fault derives through the `Diagnostics/evidence.md#FAULT_TABLES` registry.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CanvasFault : Expected {
    private CanvasFault(string detail, int code) : base(detail, code) { }
    public sealed record CycleRejected(int Nodes, int Edges)
        : CanvasFault($"graph/cycle: {Nodes} nodes and {Edges} edges are not acyclic", AppUiFaultBand.Canvas.Code(0));
    public sealed record EndpointUnknown(string Key)
        : CanvasFault($"graph/endpoint: {Key} is not a node", AppUiFaultBand.Canvas.Code(1));
    public sealed record PolicyRejected(string Detail)
        : CanvasFault($"graph/policy: {Detail}", AppUiFaultBand.Canvas.Code(2));
    public sealed record EchoRejected(string OpId)
        : CanvasFault($"graph/echo: {OpId} re-applied its own mutation", AppUiFaultBand.Canvas.Code(3));
    public sealed record ModelRejected(string Detail)
        : CanvasFault($"graph/model: {Detail}", AppUiFaultBand.Canvas.Code(4));
}

public sealed record GraphAdmission(IDrawingNodeSettings Policy) {
    public Fin<Unit> Admit(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) {
        LanguageExt.HashSet<string> known = toHashSet(nodes.Map(static node => node.Key));
        bool uniqueNodes = known.Count == nodes.Count;
        bool validPins = nodes.ForAll(node =>
            node.Pins.Map(static pin => pin.Key).Distinct().Count == node.Pins.Count
            && node.Pins.ForAll(static pin => pin.BusWidth > 0));
        if (!uniqueNodes || !validPins) { return Fin.Fail<Unit>(new CanvasFault.ModelRejected("node keys, pin keys, and bus widths must be admitted")); }
        return edges.Find(edge => !known.Contains(edge.From.NodeKey) || !known.Contains(edge.To.NodeKey))
            .Match(
                Some: edge => Fin.Fail<Unit>(new CanvasFault.EndpointUnknown(known.Contains(edge.From.NodeKey) ? edge.To.NodeKey : edge.From.NodeKey)),
                None: () => (Policy.AllowDuplicateConnections ? None : Duplicate(edges)).Match(
                    Some: dup => Fin.Fail<Unit>(new CanvasFault.PolicyRejected($"duplicate edge {dup.From} -> {dup.To}")),
                    None: () => Acyclic(nodes.Map(static node => node.Key), edges)));
    }

    // Policy column honored as data: the duplicate gate fires only when the settings row disallows repeats;
    // duplicates key on the full pin-qualified endpoint pair, so parallel pins stay distinct edges.
    static Option<GraphEdge> Duplicate(Seq<GraphEdge> edges) =>
        Optional(edges.GroupBy(static edge => edge).FirstOrDefault(static group => group.Count() > 1))
            .Map(static group => group.Key);

    // Evaluation order off the SAME graph value the cycle oracle reads — one fold, two projections.
    public Fin<Seq<string>> Order(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        Admit(nodes, edges)
            .Bind(_ => Graph(nodes.Map(static node => node.Key), edges) is { } graph && graph.IsDirectedAcyclicGraph()
                ? Fin.Succ(toSeq(graph.TopologicalSort()))
                : Fin.Fail<Seq<string>>(new CanvasFault.PolicyRejected("order requested on a cyclic graph")));

    static Fin<Unit> Acyclic(Seq<string> nodes, Seq<GraphEdge> edges) =>
        Graph(nodes, edges).IsDirectedAcyclicGraph()
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new CanvasFault.CycleRejected(nodes.Count, edges.Count));

    static AdjacencyGraph<string, SEdge<string>> Graph(Seq<string> nodes, Seq<GraphEdge> edges) {
        AdjacencyGraph<string, SEdge<string>> graph = new();
        nodes.Iter(node => graph.AddVertex(node));
        edges.Iter(edge => graph.AddEdge(new SEdge<string>(edge.From.NodeKey, edge.To.NodeKey)));
        return graph;
    }
}
```

## [04]-[COEDIT_BRIDGE]

- Owner: `GraphCoEdit` — the ONE bidirectional projection between the ReactiveUI graph model and the `Collab/sync.md` `LoroTree` container, carrying BOTH directions: `CommitLocal` outbound, the subscription sink inbound.
- Entry: `public IO<Fin<Unit>> CommitLocal(CollabDoc doc, string docKey, Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges, GraphOp op, string origin)` — the outbound direction re-admits the post-op topology before the intent rides `IntentLedger.Commit`; `public IO<Fin<Subscription>> Bind(CollabDoc doc, GraphCanvas canvas)` — the inbound direction holds one subscription per canvas and applies remote diffs through the same typed rows.
- Auto: the graph structure maps onto `LoroTree` — a node is a tree node whose meta map carries the `GraphNodeRow` columns, an edge is a child row on the connection register — and subscriber diffs discriminate `EventTriggerKind.Local`/`Import`/`Checkout` for ECHO SUPPRESSION: a local `CommitLocal` mutation arrives back as its own `Local` diff and is dropped, and a remote `Import` diff applies to the ReactiveUI graph model WITHOUT re-emitting — the feedback loop is the named deleted form, its structural fault `CanvasFault.EchoRejected`; a hierarchy move rides the sync-owned `GraphOp.NodeMove(NodeId, Parent, Index)` case onto the tree's identity-preserving `MovTo`, and a canvas x/y position write is a meta-map column edit riding the same commit leg through the graph arm — its verb row is one `GraphOp` case at the sync owner, never a side channel.
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
    public IO<Fin<Unit>> CommitLocal(CollabDoc doc, string docKey, Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges, GraphOp op, string origin) =>
        Gate.Admit(nodes, edges).Match(
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
            _ => fun(() => Owner.Conflict.Swap(_ => Some<Error>(new CanvasFault.ModelRejected($"unsupported diff trigger {diff.TriggeredBy}"))))(),
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
- Entry: `public static Seq<GraphNodeRow> FromDependencies(RecomputeGraph graph, Map<string, (double X, double Y)> layout)` — the notebook cell-dependency graph renders as a READ projection onto canvas rows; edits on this projection are disabled (a dependency edge derives from cell references, never a hand-drawn connector).
- Auto: canvas snapshot export rides the capture capsule — the editor surface renders through the capture in-tree lane and encodes through `VisualCodec` as kind graph, so a canvas baseline joins the render-hash proof lanes; PNG/SVG/PDF export of the canvas composes `Document/export.md`'s destination union with the capture raster or the package `ExportRenderer` vector arm as the source.
- Packages: NodeEditorAvalonia, Rasm.AppHost (project), SkiaSharp, LanguageExt.Core
- Growth: a new read projection (Compute solve graph, Fabrication posting chain) is one `From*` fold returning rows; zero new surface.
- Boundary: the dependency projection READS the AppHost `RecomputeGraph` vocabulary through the declared port (decode-only) — the same `[V11]` law the notebook lands; a projection-local dependency model is the deleted form.
