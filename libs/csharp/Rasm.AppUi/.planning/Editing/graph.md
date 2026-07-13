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
- Boundary: pan/zoom rides the package's OWN `NodeZoomBorder` — a distinct asset, NOT the admitted `PanAndZoom` package (which keeps its five page consumers); the disambiguation is RULED, no dup exists; the canvas renders structure and routes recompute through the AppHost `RecomputeGraph` port exactly as the notebook does — a canvas-local topo/dirty engine is the deleted form.

```csharp signature
public sealed record GraphPinRow(string Key, string Name, PinAlignment Alignment, bool Input);

public sealed record GraphNodeRow(
    string Key,
    string TemplateKey,
    string Title,
    double X,
    double Y,
    Seq<GraphPinRow> Pins);

// Product ReactiveUI contract concretions — the ONLY INode/IConnectablePin/IConnector implementations this
// plane mints; the engine constructs them through CanvasGraphFactory, never a bare new at a call site.
public sealed class CanvasPin : ReactiveObject, IConnectablePin {
    public string? Key { get; set; }
    public PinAlignment Alignment { get; set; }
    public PinDirection Direction { get; set; }
    public int BusWidth { get; set; } = 1;
}

public sealed class CanvasNode : ReactiveObject, INode {
    public string? Key { get; init; }
    public string? Title { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public IList<IPin> Pins { get; } = [];
}

public sealed class CanvasConnector : ReactiveObject, IConnector {
    public IPin? Start { get; set; }
    public IPin? End { get; set; }
}

// The one IDrawingNodeFactory row: the engine and the staged fold mint every pin, connector, and list here.
public sealed class CanvasGraphFactory : IDrawingNodeFactory {
    public IPin CreatePin() => new CanvasPin();
    public IConnector CreateConnector() => new CanvasConnector();
    public IList<T> CreateList<T>() => [];
}

public sealed record GraphCanvas(DrawingNodeEditor Editor, IDrawingNode Drawing, IDrawingNodeFactory Factory, IDrawingNodeSettings Policy, GraphAdmission Gate) {
    public Fin<IDrawingNode> Materialize(Seq<GraphNodeRow> nodes, Seq<(string From, string To)> edges) =>
        Gate.Admit(nodes.Map(static n => n.Key), edges)
            .Bind(_ => Staged(nodes, edges))
            .Map(Commit);

    // Reconcile entry for remote applies: the swap is atomic — the gate and the staged validation admit the
    // replacement FIRST, the live canvas clears only inside the success arm, so a rejected apply leaves the
    // graph intact.
    public Fin<IDrawingNode> Reset(Seq<GraphNodeRow> nodes, Seq<(string From, string To)> edges) =>
        Gate.Admit(nodes.Map(static n => n.Key), edges)
            .Bind(_ => Staged(nodes, edges))
            .Map(staged => {
                Drawing.Nodes?.Clear();
                Drawing.Connectors?.Clear();
                return Commit(staged);
            });

    // Two-phase apply: every node, pin, and connector mints DETACHED through the factory and validates
    // against the settings policy BEFORE the first Drawing mutation.
    Fin<(Seq<CanvasNode> Nodes, Seq<IConnector> Wires)> Staged(Seq<GraphNodeRow> rows, Seq<(string From, string To)> edges) {
        Map<string, CanvasNode> byKey = rows.Fold(Map<string, CanvasNode>(), (acc, row) => acc.Add(row.Key, NodeOf(Factory, row)));
        return edges.TraverseM(edge => Wired(byKey, edge)).As()
            .Map(wires => (toSeq(byKey.Values), wires.Strict()));
    }

    IDrawingNode Commit((Seq<CanvasNode> Nodes, Seq<IConnector> Wires) staged) {
        staged.Nodes.Iter(node => Drawing.Nodes?.Add(node));
        staged.Wires.Iter(wire => Drawing.Connectors?.Add(wire));
        return Drawing;
    }

    // The settings row is the one policy authority: self-connection and bus-width checks read its columns,
    // and CanConnectPin is the editor's own connectability gate over the same settings.
    Fin<IConnector> Wired(Map<string, CanvasNode> byKey, (string From, string To) edge) =>
        from start in Endpoint(byKey, edge.From, PinDirection.Output).ToFin(new CanvasFault.EndpointUnknown(edge.From))
        from end in Endpoint(byKey, edge.To, PinDirection.Input).ToFin(new CanvasFault.EndpointUnknown(edge.To))
        from _self in Policy.AllowSelfConnections || GraphAdmission.NodeKey(edge.From) != GraphAdmission.NodeKey(edge.To)
            ? Fin.Succ(unit) : Fin.Fail<Unit>(new CanvasFault.PolicyRejected($"self connection {edge.From}"))
        from _bus in !Policy.RequireMatchingBusWidth || (start, end) is (CanvasPin a, CanvasPin b) && a.BusWidth == b.BusWidth
            ? Fin.Succ(unit) : Fin.Fail<Unit>(new CanvasFault.PolicyRejected($"bus width {edge.From} -> {edge.To}"))
        from _gate in Editor.CanConnectPin(start) && Editor.CanConnectPin(end)
            ? Fin.Succ(unit) : Fin.Fail<Unit>(new CanvasFault.PolicyRejected($"{edge.From} -> {edge.To}"))
        select Wire(start, end);

    IConnector Wire(IPin start, IPin end) {
        IConnector wire = Factory.CreateConnector();
        wire.Start = start;
        wire.End = end;
        return wire;
    }

    static CanvasNode NodeOf(IDrawingNodeFactory factory, GraphNodeRow row) {
        CanvasNode node = new() { Key = row.Key, Title = row.Title, X = row.X, Y = row.Y };
        row.Pins.Iter(pin => node.Pins.Add(PinOf(factory, pin)));
        return node;
    }

    static IPin PinOf(IDrawingNodeFactory factory, GraphPinRow row) {
        IPin pin = factory.CreatePin();
        if (pin is CanvasPin typed) {
            typed.Key = row.Key;
            typed.Alignment = row.Alignment;
            typed.Direction = row.Input ? PinDirection.Input : PinDirection.Output;
        }
        return pin;
    }

    // Endpoint grammar (GraphAdmission owns it): `nodeKey` or `nodeKey/pinKey` — a pin-qualified endpoint
    // routes to its named pin so pin identity survives end-to-end; an unqualified endpoint routes by
    // direction (first Output on source, first Input on target).
    static Option<IPin> Endpoint(Map<string, CanvasNode> byKey, string endpoint, PinDirection direction) {
        string[] parts = endpoint.Split('/', 2);
        return byKey.Find(parts[0]).Bind(node => Optional(node.Pins.FirstOrDefault(pin =>
            pin is CanvasPin candidate && candidate.Direction == direction && (parts.Length == 1 || candidate.Key == parts[1]))));
    }
}
```

## [03]-[ADMISSION_GATE]

- Owner: `CanvasFault` — the typed canvas rail; `GraphAdmission` — the QuikGraph-backed connection-admission gate whose policy column IS the editor `IDrawingNodeSettings` row, never a parallel policy source.
- Entry: `public Fin<Unit> Admit(Seq<string> nodes, Seq<(string From, string To)> edges)` — the gate rejects a cycle, a dangling endpoint, or a policy-violating duplicate before the editor mutates.
- Auto: the edge set folds into a QuikGraph `AdjacencyGraph<string, SEdge<string>>` and `IsDirectedAcyclicGraph` is the cycle oracle; topological order for downstream projections reads `TopologicalSort` off the same graph — a hand-rolled adjacency list or DFS beside QuikGraph is the deleted form; duplicate admission reads `IDrawingNodeSettings.AllowDuplicateConnections` so the gate and the interactive connector-drag answer from one settings row.
- Packages: QuikGraph (shared tier), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new admission rule is one gate clause folding on the same graph value; one `CanvasFault` case is one `detail` ordinal under the `AppUiFaultBand.Canvas` row (6330); zero new surface.
- Boundary: the gate guards STRUCTURE only — recompute scheduling, dirty propagation, and evaluation stay the AppHost `RecomputeGraph`'s (a second incremental-recompute owner is the deleted form, the same law `Document/notebook.md` lands under `[V11]`); every fault derives through the `Diagnostics/evidence.md#FAULT_TABLES` registry.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CanvasFault : Expected {
    private CanvasFault(string detail, int code) : base(detail, code) { }
    public sealed record CycleRejected(string From, string To)
        : CanvasFault($"graph/cycle: {From} -> {To} closes a cycle", AppUiFaultBand.Canvas.Code(0));
    public sealed record EndpointUnknown(string Key)
        : CanvasFault($"graph/endpoint: {Key} is not a node", AppUiFaultBand.Canvas.Code(1));
    public sealed record PolicyRejected(string Detail)
        : CanvasFault($"graph/policy: {Detail}", AppUiFaultBand.Canvas.Code(2));
    public sealed record EchoRejected(string OpId)
        : CanvasFault($"graph/echo: {OpId} re-applied its own mutation", AppUiFaultBand.Canvas.Code(3));
}

public sealed record GraphAdmission(IDrawingNodeSettings Policy) {
    // The gate owns the endpoint grammar: `nodeKey` or `nodeKey/pinKey`; node-level checks strip the pin.
    public static string NodeKey(string endpoint) => endpoint.Split('/', 2)[0];

    public Fin<Unit> Admit(Seq<string> nodes, Seq<(string From, string To)> edges) {
        LanguageExt.HashSet<string> known = toHashSet(nodes);
        return edges.Find(edge => !known.Contains(NodeKey(edge.From)) || !known.Contains(NodeKey(edge.To)))
            .Match(
                Some: edge => Fin.Fail<Unit>(new CanvasFault.EndpointUnknown(known.Contains(NodeKey(edge.From)) ? edge.To : edge.From)),
                None: () => (Policy.AllowDuplicateConnections ? None : Duplicate(edges)).Match(
                    Some: dup => Fin.Fail<Unit>(new CanvasFault.PolicyRejected($"duplicate edge {dup.From} -> {dup.To}")),
                    None: () => Acyclic(nodes, edges)));
    }

    // Policy column honored as data: the duplicate gate fires only when the settings row disallows repeats;
    // duplicates key on the full pin-qualified endpoint pair, so parallel pins stay distinct edges.
    static Option<(string From, string To)> Duplicate(Seq<(string From, string To)> edges) =>
        Optional(edges.GroupBy(static edge => edge).FirstOrDefault(static group => group.Count() > 1))
            .Map(static group => group.Key);

    static Fin<Unit> Acyclic(Seq<string> nodes, Seq<(string From, string To)> edges) {
        AdjacencyGraph<string, SEdge<string>> graph = new();
        nodes.Iter(node => graph.AddVertex(node));
        edges.Iter(edge => graph.AddEdge(new SEdge<string>(NodeKey(edge.From), NodeKey(edge.To))));
        return graph.IsDirectedAcyclicGraph()
            ? Fin.Succ(unit)
            : edges.Last().Match(
                Some: last => Fin.Fail<Unit>(new CanvasFault.CycleRejected(last.From, last.To)),
                None: () => Fin.Fail<Unit>(new CanvasFault.PolicyRejected("empty edge set reported cyclic")));
    }
}
```

## [04]-[COEDIT_BRIDGE]

- Owner: `GraphCoEdit` — the ONE bidirectional projection between the ReactiveUI graph model and the `Collab/sync.md` `LoroTree` container.
- Entry: `public IO<Fin<Subscription>> Bind(CollabDoc doc, GraphCanvas canvas)` — one held subscription per canvas (dispose detaches); local structural commits emit tree ops, remote diffs apply to the model.
- Auto: the graph structure maps onto `LoroTree` — a node is a tree node whose meta map carries the `GraphNodeRow` columns, an edge is a child row on the connection register — and subscriber diffs discriminate `EventTriggerKind.Local`/`Import`/`Checkout` for ECHO SUPPRESSION: a local UI mutation commits as tree ops WITHOUT re-applying its own `Local` echo, and a remote `Import` diff applies to the ReactiveUI graph model WITHOUT re-emitting — the feedback loop is the named deleted form, its structural fault `CanvasFault.EchoRejected`.
- Receipt: durable truth rides `Collab/sync.md`'s typed edit-intent stream (a graph structural op is one row on the single edit-intent union); the live half rides the session-ephemeral Loro wire — this page persists nothing.
- Packages: LoroCs, ReactiveUI, LanguageExt.Core
- Growth: a new co-edited column is one meta-map key; zero new surface.
- Boundary: the bridge is the ONE projection — a second subscription path, a model-poll loop, or a per-node sync channel is the deleted form; remote-applied diffs re-run the `GraphAdmission` gate so a remote edit that would close a cycle surfaces as a typed conflict row for the presence UI, never a silent apply.

```csharp signature
public sealed record GraphCoEdit(GraphAdmission Gate) {
    int applying; // re-entrancy latch: a model mutation raised BY a remote apply emits no tree op back

    // The typed conflict row the presence UI observes: a remote apply the gate rejects lands HERE.
    public Atom<Option<Error>> Conflict { get; } = Atom(Option<Error>.None);

    public IO<Fin<Subscription>> Bind(CollabDoc doc, GraphCanvas canvas) =>
        IO.lift(() =>
            from tree in IntentApply.As<LoroTree>(doc, CollabContainer.Tree, "graph")
            from edges in IntentApply.As<LoroMap>(doc, CollabContainer.Map, "graph/edges")
            from sub in CollabDoc.Lift(() => tree.Subscribe(new TreeSink(this, canvas, tree, edges)))
            from live in Optional(sub).ToFin(new CollabFault.Detached("graph"))
            select live);

    sealed record TreeSink(GraphCoEdit Owner, GraphCanvas Canvas, LoroTree Tree, LoroMap Edges) : Subscriber {
        public void OnDiff(DiffEvent diff) => ignore(diff.TriggeredBy switch {
            EventTriggerKind.Local => unit,                                       // echo-suppressed: the UI already holds this state
            EventTriggerKind.Import or EventTriggerKind.Checkout => Owner.ApplyRemote(Canvas, Tree, Edges), // remote -> model, no re-emit
            _ => unit,
        });
    }

    // Remote apply is a state reconcile over the verified LoroTree/LoroMap read surface: tree nodes + meta
    // columns re-project to rows, the edge register re-projects to pairs, and the canvas rebuilds through
    // the ONE gate-checked Materialize fold — a remote edit that would close a cycle surfaces as the typed
    // CanvasFault conflict row for the presence UI, never a silent apply.
    Unit ApplyRemote(GraphCanvas canvas, LoroTree tree, LoroMap edges) {
        if (Interlocked.CompareExchange(ref applying, 1, 0) == 1) return unit;
        try {
            Seq<GraphNodeRow> rows = tree.Nodes().AsIterable().Map(id => RowOf(tree, id)).Somes().ToSeq();
            Seq<(string From, string To)> pairs = edges.Keys().AsIterable()
                .Map(static key => key.Split("->", 2))
                .Filter(static parts => parts.Length == 2)
                .Map(static parts => (parts[0], parts[1]))
                .ToSeq();
            canvas.Reset(rows, pairs).IfFail(fault => ignore(Conflict.Swap(_ => Some(fault))));
            return unit;
        }
        finally { Interlocked.Exchange(ref applying, 0); }
    }

    static Option<GraphNodeRow> RowOf(LoroTree tree, TreeId id) {
        LoroMap meta = tree.GetMeta(id);
        return (Str(meta, "key"), Str(meta, "template"), Str(meta, "title")).Apply((key, template, title) =>
            new GraphNodeRow(key, template, title, Num(meta, "x"), Num(meta, "y"), PinsOf(meta)));
    }

    static Seq<GraphPinRow> PinsOf(LoroMap meta) =>
        Str(meta, "pins")
            .Bind(static json => Optional(JsonSerializer.Deserialize<GraphPinRow[]>(json)))
            .Map(static pins => pins.ToSeq())
            .IfNone(Seq<GraphPinRow>());

    static Option<string> Str(LoroMap meta, string key) =>
        meta.Get(key)?.AsValue() is LoroValue.String s ? Some(s.Value) : None;

    static double Num(LoroMap meta, string key) =>
        meta.Get(key)?.AsValue() is LoroValue.Double d ? d.Value : 0d;
}
```

## [05]-[PROJECTIONS]

- Owner: `GraphProjection` — the read-projection fold family.
- Entry: `public static Seq<GraphNodeRow> FromDependencies(RecomputeGraph graph, Map<string, (double X, double Y)> layout)` — the notebook cell-dependency graph renders as a READ projection onto canvas rows; edits on this projection are disabled (a dependency edge derives from cell references, never a hand-drawn connector).
- Auto: canvas snapshot export rides the capture capsule — the editor surface renders through the capture in-tree lane and encodes through `VisualCodec` as kind graph, so a canvas baseline joins the render-hash proof lanes; PNG/SVG/PDF export of the canvas composes `Document/export.md`'s destination union with the capture raster or the package `ExportRenderer` vector arm as the source.
- Packages: NodeEditorAvalonia, Rasm.AppHost (project), SkiaSharp, LanguageExt.Core
- Growth: a new read projection (Compute solve graph, Fabrication posting chain) is one `From*` fold returning rows; zero new surface.
- Boundary: the dependency projection READS the AppHost `RecomputeGraph` vocabulary through the declared port (decode-only) — the same `[V11]` law the notebook lands; a projection-local dependency model is the deleted form.
