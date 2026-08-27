# [APPUI_EDITING_GRAPH]

The graph canvas is the typed-edit plane's node surface: NodeEditorAvalonia `IDrawingNode`/`DrawingNodeEditor` realize the parametric/dependency-graph canvas on ReactiveUI — node/pin/connector editing over a typed graph model, QuikGraph owning the connection-admission cycle gate and graph algebra, `LoroTree` as the co-edit data boundary under ONE bidirectional projection with `EventTriggerKind` echo suppression, and canvas snapshots exporting through the capture encode fold. The page owns the graph model rows at the package's full node and connector concept, the template palette and drop ingress, the pressure-aware ink plane, the selection-layout and camera verb projections, the overview-strip feed with its viewport state, the variant-keyed canvas skin, the admission gate, the co-edit bridge, the typed `CanvasFault` family, and the notebook dependency read projection. Recompute stays the AppHost `RecomputeGraph`'s — this canvas renders and edits structure, never re-solves.

## [01]-[INDEX]

- [02]-[GRAPH_MODEL]: Typed node/pin/connector rows at the package's whole concept on the ReactiveUI drawing model.
- [03]-[ADMISSION_GATE]: QuikGraph cycle gate over wiring and containment; typed `CanvasFault` on the kernel fault floor.
- [04]-[PALETTE_INGRESS]: Template registry, drop targets, and the pressure-aware ink plane.
- [05]-[CANVAS_VERBS]: Selection-layout rows and the typed camera verb union over the pinned viewport.
- [06]-[NAVIGATION_SURFACE]: Overview feed, jump lift, find walk, and viewport state.
- [07]-[CANVAS_SKIN]: Variant-keyed package slots, host-key closure, and the grid sizing rows.
- [08]-[COEDIT_BRIDGE]: One bidirectional `LoroTree` projection, one reconcile gate per binding.
- [09]-[PROJECTIONS]: Notebook dependency read projection; capture snapshot export.

## [02]-[GRAPH_MODEL]

- Owner: `GraphNodeRow` and `GraphPinRow` are the package-neutral model rows; `GraphEndpoint` preserves node and pin identity and `GraphLink` is the endpoint pair an edge is IDENTIFIED by; `GraphWire` is the per-edge presentation row; `GraphEdge` pairs the two; `GraphRouting` is the resolved placement policy row; `GraphModelAdapter` binds the package's mint surface and the graph serializer; `GraphCanvas` owns two-phase materialization over one `DrawingNodeEditor` through the ONE mode-discriminated `Commit`.
- Entry: `Commit(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges, CommitMode mode)` — the one gate → stage → land spine; `CommitMode.Append` adds staged structure, `Replace` swaps the whole drawing atomically inside the success arm, `Cloned` round-trips every staged node through the installed serializer and re-wires the clones so a pasted subgraph carries clone identity end to end; `Placed(string nodeKey, double x, double y)` mints the position op through the routing grid; `Reparented(string nodeKey, Option<string> parent, uint index)` mints the containment op the tree commits; `Seq<GraphNodeRow>.Extent()` — the one model-extent fold the camera fit and the overview frame both read.
- Auto: the node row carries every column `INode` declares — containment parent, extent, rotation, lock, and visibility — so the declared hierarchy-move op has a model that expresses it and a co-edited group, frame, or collapsed subgraph round-trips whole; the edge row splits identity from presentation, so waypoints, per-edge routing mode, arrow styles, offset, and label travel beside a `GraphLink` the duplicate and fanout folds key on unchanged. `IDrawingNodeSettings` IS the one connection-policy authority and every column this page consumes is read once at `GraphRouting.Of`: `GraphCanvas.Wired` reads direction and bus width and delegates final connectability to `DrawingNodeEditor.CanConnectPin`, `GraphAdmission` reads the connection-enable, self-connection, duplicate, and per-pin-fanout columns and imposes the stronger dependency-DAG invariant over BOTH the wiring graph and the containment forest, and the snap and grid columns lift into the routing row every position write, connector path, and grid decorator reads — so the batch gate, the interactive drag, and the painted grid cannot answer differently. Guide and nudge behaviour stays the settings row's own, consumed by the package's interactive behaviours directly — the resolved row re-carries nothing this page's writes never read. Clone, paste, and duplication ride `INodeSerializer` through the editor's own `Clone<T>`, and node minting rides the palette's `INodeTemplate` rows through that same clone.
- Result: a committed structural edit projects a typed edit-intent op onto the `Collab/sync.md` durable stream through the `[08]` binding, and the deck row returns its `DeckOutcome`.
- Packages: NodeEditorAvalonia (+`.Model` transitive-floor pin), PanAndZoom, ReactiveUI, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new node kind is one palette template row; a new pin shape is one `GraphPinRow` value; a new connector presentation is one `GraphWire` column off the package's own enums; a retuned grid is one `GraphRouting` column off the settings row; a new commit posture is one `CommitMode` row; zero new surface.
- Boundary: connector routing and hit testing stay the package's `OrthogonalRouter`/`RTree`/`HitTestIndex` — `GraphRouting` carries the `ConnectorRoutingAlgorithm` and the default `ConnectorStyle` a render binds and re-implements neither, and a per-edge `ConnectorRoutingMode.Manual` hands that edge's path to its own `Waypoints` exactly as the package intends. Pan and zoom ride the PINNED `PanAndZoom` `ZoomBorder` hosting a bare `DrawingNode` canvas, never the package's `Editor` control and never `NodeZoomBorder`: the transitive `Avalonia.Controls.PanAndZoom` assembly and the pinned `PanAndZoom` assembly BOTH publish `Avalonia.Controls.PanAndZoom.ZoomBorder`, the collision is a package-id rename rather than a namespace clash, `NodeZoomBorder` derives the LEGACY type and adds seven parameterless command shims and nothing else, and every saved-view, view-history, discrete-zoom, grid, rotation, and state-export member lives on the pinned type alone — so `Editor`, whose template fills `ZoomControl` from `PART_ZoomBorder` with the legacy base, is the deleted host and `[05]-[CANVAS_VERBS]` binds the pinned control directly; the manifest posture is `Aliases` metadata on the TRANSITIVE reference (`ExcludeAssets` is not viable because `NodeZoomBorder` inherits the type it removes), placed on the legacy package rather than the pinned one, since aliasing the pinned package lifts its whole type set out of global scope and every existing plain `ZoomBorder` mention across the corpus then binds the legacy type silently while only the absent members fault — and the alias must be manifest metadata rather than a source `extern alias`, because the Avalonia name generator emits its own partial naming the type unqualified and no source directive reaches generated code. Ursa `ImageViewer` is the third pan-zoom owner in the package closure and stays scoped to image presentation. The editor's `IUndoRedoHost` binds to the one `Editing/history.md` `EditHistory` — `Undo`/`Redo` delegate to the `history.undo`/`history.redo` intents and `BeginUndoBatch`/`EndUndoBatch` open and seal one `RevertDelta.Composite` op so a multi-op canvas gesture undoes as one unit; the canvas renders structure and routes recompute through the AppHost `RecomputeGraph` port exactly as the notebook does — a canvas-local topo/dirty engine is the deleted form.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

public enum Rung { Next = 1, Prev = -1 }

// --- [MODELS] --------------------------------------------------------------------------

public sealed record GraphPinRow(string Key, string Name, PinAlignment Alignment, PinDirection Direction, int BusWidth);

public sealed record GraphNodeRow(
    string Key,
    string TemplateKey,
    string Title,
    Option<string> Parent,
    double X,
    double Y,
    double Width,
    double Height,
    double Rotation,
    bool Locked,
    bool Visible,
    Seq<GraphPinRow> Pins);

public static class GraphRows {
    extension(Seq<GraphNodeRow> rows) {
        public Option<Rect> Extent() =>
            rows.Filter(static row => row.Visible)
                .Fold(Option<Rect>.None, static (held, row) => new Rect(row.X, row.Y, row.Width, row.Height) switch {
                    var box => held.Match(Some: union => Some(union.Union(box)), None: () => Some(box)),
                });
    }
}

public readonly record struct GraphEndpoint(string NodeKey, Option<string> PinKey);

public readonly record struct GraphLink(GraphEndpoint From, GraphEndpoint To);

public sealed record GraphWire(
    ConnectorRoutingMode Routing,
    ConnectorStyle Style,
    ConnectorOrientation Orientation,
    ConnectorArrowStyle StartArrow,
    ConnectorArrowStyle EndArrow,
    double Offset,
    Option<string> Label,
    Seq<(double X, double Y)> Waypoints) {
    public static GraphWire Seed(ConnectorStyle style) =>
        new(ConnectorRoutingMode.Auto, style, ConnectorOrientation.Auto,
            ConnectorArrowStyle.None, ConnectorArrowStyle.Arrow, Offset: 0d, None, Seq<(double, double)>());
}

public sealed record GraphEdge(GraphEndpoint From, GraphEndpoint To, GraphWire Wire) {
    public GraphLink Ends => new(From, To);
}

public sealed record GraphModelAdapter(
    GraphPalette Palette,
    Func<GraphPinRow, Fin<IPin>> Pin,
    Func<GraphWire, Fin<IConnector>> Wire,
    INodeSerializer Serializer) {
    public Fin<INode> Node(DrawingNodeEditor editor, GraphNodeRow row) =>
        from seed in Palette.Mint(editor, row.TemplateKey)
        from pins in row.Pins.TraverseM(Pin).As()
        select Dressed(seed, row, pins.Strict());

    public Fin<IConnector> Connect(IPin start, IPin end, GraphWire wire) =>
        Wire(wire).Map(connector => {
            connector.Start = start;
            connector.End = end;
            connector.Name = wire.Label.IfNone(string.Empty);
            connector.Style = wire.Style;
            connector.Orientation = wire.Orientation;
            connector.RoutingMode = wire.Routing;
            connector.StartArrow = wire.StartArrow;
            connector.EndArrow = wire.EndArrow;
            connector.Offset = wire.Offset;
            wire.Waypoints.Iter(point => connector.Waypoints.Add(new ConnectorPoint(point.X, point.Y)));
            return connector;
        });

    public static Seq<IPin> Pins(INode node) => toSeq(node.Pins ?? []);

    public static string PinKey(IPin pin) => pin.Name ?? string.Empty;

    public static PinDirection Direction(IPin pin) =>
        pin is IConnectablePin typed ? typed.Direction : PinDirection.Bidirectional;

    public static int BusWidth(IPin pin) => pin is IConnectablePin typed ? typed.BusWidth : 1;

    static INode Dressed(INode node, GraphNodeRow row, Seq<IPin> pins) {
        node.Name = row.Key;
        node.X = row.X;
        node.Y = row.Y;
        node.Width = row.Width;
        node.Height = row.Height;
        node.Rotation = row.Rotation;
        node.IsLocked = row.Locked;
        node.IsVisible = row.Visible;
        node.Pins?.Clear();
        pins.Iter(pin => { pin.Parent = node; node.Pins?.Add(pin); });
        return node;
    }
}

public sealed record GraphRouting(
    ConnectorRoutingAlgorithm Algorithm,
    ConnectorStyle Style,
    Option<(double X, double Y)> Snap,
    Option<(double Width, double Height)> Grid) {
    public static GraphRouting Of(IDrawingNodeSettings policy, ResolvedTheme resolved, ConnectorRoutingAlgorithm algorithm, ConnectorStyle style) =>
        resolved.Metric(MetricFamily.Space, 3).IfNone(GraphSkin.GridFallback) switch {
            var cell => new(algorithm, style,
                policy.EnableSnap ? Some((policy.SnapX, policy.SnapY)) : None,
                policy.EnableGrid ? Some((Sized(policy.GridCellWidth, cell), Sized(policy.GridCellHeight, cell))) : None),
        };

    public (double Width, double Height) Cell => Grid.IfNone((GraphSkin.GridFallback, GraphSkin.GridFallback));

    public (double X, double Y) Place(double x, double y) =>
        Snap.Match(
            Some: pitch => (Quantized(x, pitch.X), Quantized(y, pitch.Y)),
            None: () => (x, y));

    static double Quantized(double value, double pitch) =>
        Math.Abs(pitch) switch {
            var step when step > 0d => Math.Round(value / step, MidpointRounding.AwayFromZero) * step,
            _ => value,
        };

    static double Sized(double declared, double resolved) =>
        double.IsFinite(declared) && declared > 0d ? declared : resolved;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CommitMode {
    public static readonly CommitMode Append = new("append");
    public static readonly CommitMode Replace = new("replace");
    public static readonly CommitMode Cloned = new("cloned");
}

public sealed record GraphCanvas(
    DrawingNodeEditor Editor,
    IDrawingNode Drawing,
    IDrawingNodeSettings Policy,
    GraphAdmission Gate,
    GraphModelAdapter Model,
    GraphRouting Routing) {
    public Fin<IDrawingNode> Commit(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges, CommitMode mode) =>
        Gate.Admitted(nodes, edges)
            .Bind(_ => Staged(nodes, edges))
            .Bind(staged => mode.Switch(
                state: (Canvas: this, Staged: staged, Edges: edges),
                append: static s => Fin.Succ(s.Staged),
                replace: static s => {
                    s.Canvas.Drawing.Nodes?.Clear();
                    s.Canvas.Drawing.Connectors?.Clear();
                    return Fin.Succ(s.Staged);
                },
                cloned: static s => s.Canvas.Cloned(s.Staged, s.Edges)))
            .Map(Landed);

    public GraphOp Placed(string nodeKey, double x, double y) =>
        Routing.Place(x, y) switch { var at => new GraphOp.NodeAt(nodeKey, at.X, at.Y) };

    public GraphOp Reparented(string nodeKey, Option<string> parent, uint index) =>
        new GraphOp.NodeMove(nodeKey, parent, index);

    sealed record Staging(Seq<(GraphNodeRow Row, INode Node)> Rows, Map<string, INode> Index, Seq<IConnector> Wires);

    Fin<Staging> Staged(Seq<GraphNodeRow> rows, Seq<GraphEdge> edges) =>
        from materialized in rows.TraverseM(row => Model.Node(Editor, row).Map(node => (row, Node: node))).As()
        let index = materialized.Fold(Map<string, INode>(), static (held, entry) => held.Add(entry.row.Key, entry.Node))
        from wires in edges.TraverseM(edge => Wired(index, edge)).As()
        select new Staging(materialized.Strict(), index, wires.Strict());

    Fin<Staging> Cloned(Staging staged, Seq<GraphEdge> edges) =>
        from clones in staged.Rows.TraverseM(entry => Optional(Editor.Clone(entry.Node))
            .ToFin(Fail: (Error)new CanvasFault.ModelRejected("serializer round-trip refused a staged node"))
            .Map(clone => (entry.Row, Node: clone))).As()
        let index = clones.Fold(Map<string, INode>(), static (held, entry) => held.Add(entry.Row.Key, entry.Node))
        from wires in edges.TraverseM(edge => Wired(index, edge)).As()
        select new Staging(clones.Strict(), index, wires.Strict());

    IDrawingNode Landed(Staging staged) {
        Drawing.SetSerializer(Model.Serializer);
        staged.Rows.Iter(entry => entry.Row.Parent.Iter(parent =>
            staged.Index.Find(parent).Iter(owner => entry.Node.Parent = owner)));
        staged.Rows.Iter(entry => Drawing.Nodes?.Add(entry.Node));
        staged.Wires.Iter(wire => Drawing.Connectors?.Add(wire));
        return Drawing;
    }

    Fin<IConnector> Wired(Map<string, INode> index, GraphEdge edge) =>
        from start in Endpoint(index, edge.From, RequiredDirection(PinDirection.Output)).ToFin(Fail: new CanvasFault.EndpointUnknown(edge.From.ToString()))
        from end in Endpoint(index, edge.To, RequiredDirection(PinDirection.Input)).ToFin(Fail: new CanvasFault.EndpointUnknown(edge.To.ToString()))
        from _bus in !Policy.RequireMatchingBusWidth || GraphModelAdapter.BusWidth(start) == GraphModelAdapter.BusWidth(end)
            ? Fin.Succ(unit) : Fin.Fail<Unit>(new CanvasFault.PolicyRejected($"bus width {edge.From} -> {edge.To}"))
        from _gate in Editor.CanConnectPin(start) && Editor.CanConnectPin(end)
            ? Fin.Succ(unit) : Fin.Fail<Unit>(new CanvasFault.PolicyRejected($"{edge.From} -> {edge.To}"))
        from wire in Model.Connect(start, end, edge.Wire)
        select wire;

    Option<IPin> Endpoint(Map<string, INode> index, GraphEndpoint endpoint, Option<PinDirection> direction) =>
        index.Find(endpoint.NodeKey).Bind(node => GraphModelAdapter.Pins(node).Find(pin =>
            direction.Match(Some: admitted => GraphModelAdapter.Direction(pin) == admitted, None: static () => true)
            && endpoint.PinKey.Match(
                Some: key => GraphModelAdapter.PinKey(pin) == key,
                None: () => true)));

    Option<PinDirection> RequiredDirection(PinDirection direction) =>
        Policy.RequireDirectionalConnections ? Some(direction) : Option<PinDirection>.None;
}
```

## [03]-[ADMISSION_GATE]

- Owner: `CanvasFault` — the direct generated `[Union]` with one `[FaultCase]` leaf per canvas failure; `GraphAdmission` — the QuikGraph-backed connection-admission gate whose policy column IS the editor `IDrawingNodeSettings` row, never a parallel policy source.
- Entry: `Admitted(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges)` — the accumulating admission returning the ADMITTED graph value, so a caller wanting only the verdict maps it away and a caller wanting the graph never re-derives it; `Order` returns topological node keys from the same admitted value and is the `[09]` dependency projection's row order; `Graph(Seq<string> vertices, Seq<(string From, string To)> edges, bool allowParallelEdges)` — the ONE container-construction fold (`GraphExtensions.ToAdjacencyGraph` over a grouped edge fold), which `Editing/forms.md`'s field-dependency gate composes rather than hand-building a third container.
- Auto: admission ACCUMULATES — identity, extent, containment, endpoint, and policy claims ride the shared `AdmissionSlots` applicative, so a batch carrying five defects reports five named refusals rather than the first, and every fault constructs on its failing arm alone; the wiring cycle oracle builds the sole `AdjacencyGraph` through the one fold and `TopologicalSort` reads off the SAME graph value through `Order`; containment folds through that identical oracle over a child-to-parent edge graph, so a group nested inside its own descendant refuses on the same mechanism a feedback wire does; the policy clauses read `EnableConnections`, `AllowSelfConnections`, `AllowDuplicateConnections`, and `EnableMultiplePinConnections` directly, so the gate and the interactive connector-drag answer from one settings row.
- Packages: QuikGraph (shared tier), Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (kernel fault floor), Rasm.Element (boundary `AdmissionSlots`)
- Growth: a new admission rule is one accumulating claim; a new fault case is one `[FaultCase]` leaf; zero new surface.
- Boundary: the gate guards STRUCTURE only — recompute scheduling, dirty propagation, and evaluation stay the AppHost `RecomputeGraph`'s; `CanvasFault` leaves each refusal distinct through its direct generated union case.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------



// --- [ERRORS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CanvasFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Canvas;
    private CanvasFault() { }
    [FaultCase(0)]
    public sealed partial record CycleRejected(int Nodes, int Edges) : CanvasFault() {
        public override string Message => $"graph/cycle: {Nodes} nodes and {Edges} edges are not acyclic";
    }
    [FaultCase(1)]
    public sealed partial record EndpointUnknown(string Key) : CanvasFault() {
        public override string Message => $"graph/endpoint: {Key} is not admitted";
    }
    [FaultCase(2)]
    public sealed partial record PolicyRejected(string Detail) : CanvasFault() {
        public override string Message => $"graph/policy: {Detail}";
    }
    [FaultCase(3)]
    public sealed partial record TriggerUnsupported(string Trigger) : CanvasFault() {
        public override string Message => $"graph/trigger: {Trigger} is not an admitted diff trigger";
    }
    [FaultCase(4)]
    public sealed partial record ModelRejected(string Detail) : CanvasFault() {
        public override string Message => $"graph/model: {Detail}";
    }
    [FaultCase(5)]
    public sealed partial record TemplateUnknown(string Key) : CanvasFault() {
        public override string Message => $"graph/template: {Key} resolves no palette row";
    }
    [FaultCase(6)]
    public sealed partial record CameraRejected(string Detail) : CanvasFault() {
        public override string Message => $"graph/camera: {Detail}";
    }
    [FaultCase(7)]
    public sealed partial record DropRejected(string Detail) : CanvasFault() {
        public override string Message => $"graph/drop: {Detail}";
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed record GraphAdmission(IDrawingNodeSettings Policy) {
    public Fin<AdjacencyGraph<string, SEdge<string>>> Admitted(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        AdmissionSlots.Accumulate(Seq(
                Identities(nodes),
                Containment(nodes),
                Endpoints(nodes, edges),
                AdmissionSlots.Gate(Policy.EnableConnections || edges.IsEmpty, "connections", "disabled",
                    static (concern, detail) => (Error)new CanvasFault.PolicyRejected($"{concern} are {detail}")),
                AdmissionSlots.Gate(Policy.AllowSelfConnections || !edges.Exists(static edge => edge.From.NodeKey == edge.To.NodeKey),
                    "self", "connection", static (concern, detail) => (Error)new CanvasFault.PolicyRejected($"{concern} {detail}")),
                Duplicates(edges),
                Fanout(edges)))
            .ToFin()
            .Bind(_ => Graph(nodes.Map(static node => node.Key),
                    edges.Map(static edge => (edge.From.NodeKey, edge.To.NodeKey)), allowParallelEdges: true) switch {
                var graph when graph.IsDirectedAcyclicGraph() => Fin.Succ(graph),
                _ => Fin.Fail<AdjacencyGraph<string, SEdge<string>>>(new CanvasFault.CycleRejected(nodes.Count, edges.Count)),
            });

    public Fin<Seq<string>> Order(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        Admitted(nodes, edges).Map(static graph => toSeq(graph.TopologicalSort()));

    public static AdjacencyGraph<string, SEdge<string>> Graph(
        Seq<string> vertices, Seq<(string From, string To)> edges, bool allowParallelEdges) =>
        toSeq(edges.Map(static edge => new SEdge<string>(edge.From, edge.To))
            .GroupBy(static edge => edge.Source))
            .Fold(HashMap<string, Seq<SEdge<string>>>(), static (held, group) => held.Add(group.Key, toSeq(group)))
            switch {
            var outgoing => vertices.AsIterable().ToAdjacencyGraph(
                vertex => outgoing.Find(vertex).IfNone(Seq<SEdge<string>>()).AsIterable(),
                allowParallelEdges),
        };

    static Validation<Error, Unit> Identities(Seq<GraphNodeRow> nodes) =>
        AdmissionSlots.Accumulate(
            AdmissionSlots.Gate(Repeated(nodes.Map(static node => node.Key)).IsNone, "node", "keys must be distinct",
                static (concern, detail) => (Error)new CanvasFault.ModelRejected($"{concern} {detail}"))
            .Cons(nodes.Map(static node => AdmissionSlots.Gate(
                !string.IsNullOrWhiteSpace(node.Key)
                && !string.IsNullOrWhiteSpace(node.TemplateKey)
                && !string.IsNullOrWhiteSpace(node.Title)
                && double.IsFinite(node.X) && double.IsFinite(node.Y) && double.IsFinite(node.Rotation)
                && node.Width >= 0d && node.Height >= 0d
                && Repeated(node.Pins.Map(static pin => pin.Key)).IsNone
                && node.Pins.ForAll(static pin => !string.IsNullOrWhiteSpace(pin.Key)
                    && !string.IsNullOrWhiteSpace(pin.Name)
                    && pin.BusWidth > 0), "key, extent, rotation, pin keys, and bus widths must be admitted",
                static (concern, detail) => (Error)new CanvasFault.ModelRejected($"{concern}: {detail}")))));

    static Validation<Error, Unit> Containment(Seq<GraphNodeRow> nodes) =>
        toHashSet(nodes.Map(static node => node.Key)) switch {
            var keys => AdmissionSlots.Accumulate(Seq(
                nodes.Find(node => node.Parent.Exists(parent => !keys.Contains(parent))).Match(
                    Some: node => AdmissionSlots.Gate(false, node.Key, "parent",
                        static (concern, detail) => (Error)new CanvasFault.EndpointUnknown($"{concern} {detail}")),
                    None: static () => AdmissionSlots.Gate(true, string.Empty, string.Empty, static (_, _) => Error.Empty)),
                AdmissionSlots.Gate(
                    Graph(nodes.Map(static node => node.Key),
                        nodes.Choose(static node => node.Parent.Map(parent => (node.Key, parent))),
                        allowParallelEdges: false).IsDirectedAcyclicGraph(),
                    nodes.Count, nodes.Count(static node => node.Parent.IsSome),
                    static (count, contained) => (Error)new CanvasFault.CycleRejected(count, contained)))),
        };

    Validation<Error, Unit> Endpoints(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        AdmissionSlots.Accumulate(edges.Map(edge =>
            AdmissionSlots.Gate(EndpointKnown(nodes, edge.From, PinDirection.Output) && EndpointKnown(nodes, edge.To, PinDirection.Input),
                edge, unit,
                (offending, _) => (Error)new CanvasFault.EndpointUnknown(
                    !EndpointKnown(nodes, offending.From, PinDirection.Output) ? offending.From.ToString() : offending.To.ToString()))));

    Validation<Error, Unit> Duplicates(Seq<GraphEdge> edges) =>
        AdmissionSlots.Gate(Policy.AllowDuplicateConnections || Repeated(edges.Map(static edge => edge.Ends)).IsNone,
            edges, unit,
            static (offending, _) => (Error)new CanvasFault.PolicyRejected(
                Repeated(offending.Map(static edge => edge.Ends)).Map(static dup => $"duplicate edge {dup.From} -> {dup.To}").IfNone("duplicate edge")));

    Validation<Error, Unit> Fanout(Seq<GraphEdge> edges) =>
        AdmissionSlots.Gate(Policy.EnableMultiplePinConnections
                || Repeated(edges.Map(static edge => edge.From) + edges.Map(static edge => edge.To)).IsNone,
            edges, unit,
            static (offending, _) => (Error)new CanvasFault.PolicyRejected(
                Repeated(offending.Map(static edge => edge.From) + offending.Map(static edge => edge.To))
                    .Map(static pin => $"pin fanout {pin}").IfNone("pin fanout")));

    bool EndpointKnown(Seq<GraphNodeRow> nodes, GraphEndpoint endpoint, PinDirection expected) =>
        nodes.Find(node => StringComparer.Ordinal.Equals(node.Key, endpoint.NodeKey)).Exists(node =>
            endpoint.PinKey.Match(
                Some: pinKey => node.Pins.Exists(pin => StringComparer.Ordinal.Equals(pin.Key, pinKey)
                    && (!Policy.RequireDirectionalConnections || pin.Direction == expected)),
                None: () => node.Pins.Exists(pin => !Policy.RequireDirectionalConnections || pin.Direction == expected)));

    static Option<TKey> Repeated<TKey>(Seq<TKey> keys) where TKey : notnull =>
        toSeq(keys.GroupBy(static key => key)).Filter(static group => group.Count() > 1).Head.Map(static group => group.Key);
}
```

## [04]-[PALETTE_INGRESS]

- Owner: `GraphTemplate` the palette row over the package's own `INodeTemplate`; `GraphPalette` the frozen template registry every `TemplateKey` resolves through; `GraphDropTarget` the package drop contract over the one transfer-admission path; `GraphInk` the pressure-aware markup arm over the landed pen rows with `InkLadder` as its quantization policy row.
- Entry: `GraphPalette.Freeze(params ReadOnlySpan<GraphTemplate> rows)` — one freeze per mounted canvas, its two claims accumulating; `Mint(DrawingNodeEditor editor, string templateKey)` — the one node mint; `GraphInk.Strokes(Seq<PenSample> samples, InkPen pen, InkLadder ladder)` — the pen fold to committed strokes; `GraphInk.Route(Seq<PenSample> samples, InkLadder ladder)` — the paint-versus-erase routing verdict.
- Auto: the palette is the seat every `GraphNodeRow.TemplateKey` resolves against, so a key that resolves nothing is a typed refusal at admission instead of a node the mint silently skipped, and the same frozen row set feeds the package `Toolbox` through `TemplatesSource` — palette browsing, double-tap insert, and toolbox drag are the package's own behaviours over one registry. Drop ingress implements `IDrawingDropTarget` so the package's own drop behaviours deliver, files route through `DragPayload.Admit` before a node exists, and BOTH drop arms fold through one result into the composition sink, so a refused drop is evidence at the sink rather than a silent no-op. Ink strokes mint from `PointerTrack.Pen`, so pressure, tilt, twist, barrel, and eraser arrive as normalized `DeviceAxis` levels on the one channel grammar, and a gesture crossing the eraser rung routes whole to removal.
- Result: an admitted drop and a committed stroke each land through the same `Commit` a typed edit crosses.
- Packages: NodeEditorAvalonia, Avalonia, LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm.Element (boundary `AdmissionSlots`)
- Growth: a new node kind is one `GraphTemplate` row; a new drop shape is one `DragPayload` case already carried by the transfer path; a new pen behaviour is one `PenAxis` row at its own owner; a retuned quantization is one `InkLadder` value; zero new surface.
- Boundary: the palette holds the package's `INodeTemplate` VALUE rather than re-declaring its three members; template instantiation is `DrawingNodeEditor.Clone<T>` over the row's own `Template`, so a minted node and a pasted node come off one round-trip. The package's own ink CAPTURE is refused — `IsInkMode` stays false so `InkLayer` never installs its pointer handlers — because that capture writes a CONSTANT unit pressure and discards the coalesced burst; `InkLayer` remains the RENDERER. Its render is one immutable pen at one constant width per stroke, so a pressure-varying gesture lands as a RUN SET: the fold quantizes pressure onto the ladder and emits one `InkStroke` per level run, which renders as a varying-width stroke through the package's own renderer instead of forking it. Strokes enter `Drawing.InkStrokes` inside one `BeginUndoBatch`/`EndUndoBatch` pair so a whole gesture reverts as one op on the `Editing/history.md` ledger; the pen tool's pointer glyph is the `Theme/assets#CURSOR_ROWS` `CursorRow` the interaction root already inherits.

```csharp
// --- [MODELS] --------------------------------------------------------------------------

public sealed record GraphTemplate(string Key, INodeTemplate Row) {
    public string Title => Row.Title ?? Key;
}

public sealed record GraphPalette(FrozenDictionary<string, GraphTemplate> Templates) {
    public static Fin<GraphPalette> Freeze(params ReadOnlySpan<GraphTemplate> rows) =>
        toSeq(rows.ToArray()) switch {
            var authored => AdmissionSlots.Accumulate(Seq(
                    AdmissionSlots.Gate(authored.Map(static row => row.Key).Distinct().Count == authored.Count,
                        "palette", "keys must be distinct", static (c, d) => (Error)new CanvasFault.ModelRejected($"{c} {d}")),
                    AdmissionSlots.Gate(authored.ForAll(static row => !string.IsNullOrWhiteSpace(row.Key) && row.Row.Template is not null),
                        "palette", "every row must carry a template", static (c, d) => (Error)new CanvasFault.ModelRejected($"{c} {d}"))))
                .ToFin()
                .Map(_ => new GraphPalette(authored.ToFrozenDictionary(static row => row.Key, static row => row, StringComparer.Ordinal))),
        };

    public Fin<INode> Mint(DrawingNodeEditor editor, string templateKey) =>
        Templates.TryGetValue(templateKey, out GraphTemplate? row)
            ? Optional(row.Row.Template).Bind(seed => Optional(editor.Clone(seed)))
                .ToFin(Fail: (Error)new CanvasFault.ModelRejected($"template '{templateKey}' round-tripped to nothing"))
            : Fin.Fail<INode>(new CanvasFault.TemplateUnknown(templateKey));

    public IList<INodeTemplate> Host => [.. toSeq(Templates.Values).Map(static row => row.Row)];
}

// --- [BOUNDARIES] ----------------------------------------------------------------------

public sealed record GraphDropTarget(
    GraphRouting Routing,
    Func<string, bool> Admitted,
    string FileTemplate,
    string NoteTemplate,
    Func<Fin<Seq<GraphNodeRow>>, IO<Unit>> Commit,
    Func<double, double, string> Mint) : IDrawingDropTarget {
    public bool CanDropText(string text, Point point) => !string.IsNullOrWhiteSpace(text);

    public bool CanDropFiles(IReadOnlyList<IStorageItem> files, Point point) => Paths(files).Exists(Admitted);

    public void DropText(string text, Point point) =>
        ignore(Commit(Fin.Succ(Rows(Seq(text), NoteTemplate, point))).Run());

    public void DropFiles(IReadOnlyList<IStorageItem> files, Point point) =>
        ignore(Commit(DragPayload.Admit(Paths(files), Admitted).ToFin()
            .Map(payload => payload is DragPayload.Files admitted
                ? Rows(admitted.Paths, FileTemplate, point)
                : Seq<GraphNodeRow>())).Run());

    Seq<GraphNodeRow> Rows(Seq<string> subjects, string templateKey, Point point) =>
        subjects.Map((subject, ordinal) => Routing.Place(point.X, point.Y + (ordinal * Routing.Cell.Height)) switch {
            var at => new GraphNodeRow(Mint(at.X, at.Y), templateKey, Title(subject), None,
                at.X, at.Y, Width: 0d, Height: 0d, Rotation: 0d, Locked: false, Visible: true, Seq<GraphPinRow>()),
        }).Strict();

    static string Title(string subject) => Path.GetFileName(subject) is { Length: > 0 } name ? name : subject;

    static Seq<string> Paths(IReadOnlyList<IStorageItem> files) =>
        toSeq(files).Choose(static file => Optional(file.TryGetLocalPath()));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed record InkLadder(int Levels, double MinimumScale, double EraserRung) {
    public static readonly InkLadder Standard = new(Levels: 8, MinimumScale: 0.25d, EraserRung: 0.5d);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PenRoute {
    public static readonly PenRoute Paint = new("paint");
    public static readonly PenRoute Erase = new("erase");
}

public static class GraphInk {
    public static PenRoute Route(Seq<PenSample> samples, InkLadder ladder) =>
        samples.Exists(sample => sample.Level(PenAxis.Eraser).Exists(level => level.Value > ladder.EraserRung))
            ? PenRoute.Erase
            : PenRoute.Paint;

    public static Seq<InkStroke> Strokes(Seq<PenSample> samples, InkPen pen, InkLadder ladder) =>
        samples.Map(sample => (sample, Level: Quantized(sample, ladder)))
            .Fold(Seq<(int Level, Seq<PenSample> Run)>(), static (runs, entry) => runs.Last
                .Filter(last => last.Level == entry.Level)
                .Match(
                    Some: last => runs.Init.Add((last.Level, last.Run.Add(entry.sample))),
                    None: () => runs.Add((entry.Level, runs.Last.Map(last => last.Run.Last).Match(
                        Some: joint => Seq(joint, entry.sample),
                        None: () => Seq(entry.sample))))))
            .Filter(static run => run.Run.Count > 1)
            .Map(run => Stroke(run.Run, pen, run.Level, ladder));

    static int Quantized(PenSample sample, InkLadder ladder) =>
        sample.Level(PenAxis.Pressure).Match(
            Some: level => Math.Clamp((int)Math.Round(Math.Abs(level.Value) * (ladder.Levels - 1)), 0, ladder.Levels - 1),
            None: () => ladder.Levels - 1);

    static InkStroke Stroke(Seq<PenSample> run, InkPen pen, int level, InkLadder ladder) =>
        new() {
            Color = pen.Color,
            Opacity = pen.Opacity,
            Thickness = pen.Thickness * (ladder.MinimumScale + ((1d - ladder.MinimumScale) * (level / (double)(ladder.Levels - 1)))),
            Name = pen.Name,
            Points = [.. run.Map(sample => new InkPoint(
                sample.Position.X, sample.Position.Y,
                sample.Level(PenAxis.Pressure).Map(static value => Math.Abs(value.Value)).IfNone(1d),
                sample.At.ToUnixTimeMilliseconds()))],
        };
}
```

## [05]-[CANVAS_VERBS]

- Owner: `GraphVerbs` — the selection-layout and camera command-table projection; `GraphNav` `[Union]` — the typed camera verb vocabulary; `ViewVerb` — the named-view verb rows; `GraphCamera` — the pinned viewport every camera verb dispatches through, holding the named-view roster it seats.
- Cases: `GraphNav` = Fit | FitTo | Step | ZoomBy | Travel | Locate | Named | Reset — the direction-bearing rows carry a `Rung`, so one case spans a forward and a backward verb with no open integer to guard, and the ONE `Named` case carries `ViewVerb` = Write | Restore | Drop, landing the ruled `NamedView`-family collapse: writing, returning to, and dropping a named view are three verb rows on one case, one admission, one roster.
- Entry: `GraphVerbs.Rows(IDrawingNode drawing, GraphCamera camera, GraphFind find, Func<Seq<GraphNodeRow>> selected)` — the whole graph verb projection; `GraphVerbs.Jump(GraphCamera camera)` and `Jumped(CommandDeck deck)` — the overview strip's point-carrying verb and the arrow lifting a published `Point` onto it; `GraphCamera.Navigate(GraphNav verb)` — every camera move and named-view write on the one `Fin`.
- Auto: the layout rows GENERATE off the package's own bounded vocabularies — one row per `NodeAlignment`, `NodeDistribution`, and `NodeOrder` case — so a package enum gaining a case gains its verb, its chord slot, its palette entry, and its journal replay with no row edit; every row raises the `IDrawingNode` operation the package already implements, all through the ONE `Raise` minter whose availability predicate is the row's only variation. Camera rows admit their own argument before dispatch and answer the same `Fin` every sibling entry answers, and all four `IO<Fin<Unit>>` lowerings ride one `Lowered` fold.
- Result: each row returns its own `DeckOutcome`; a verb that only moved the camera fires no edit fact.
- Packages: NodeEditorAvalonia, PanAndZoom, ReactiveUI, LanguageExt.Core, Avalonia, Rasm (kernel `Cell`/`Transition`)
- Growth: a new selection-layout verb is one package enum case its generator already covers; a new camera move is one `GraphNav` case; a new named-view verb is one `ViewVerb` row; zero new surface.
- Boundary: the verb rows land on the one `Shell/commands#INTENT_TABLE` table under the `graph.` prefix and mint no second registry; the content-space point codec is `Editing/history.md`'s `ScrubPoint`, taken as a value — a second encoder over the same keyed payload case drifts from the decoder the moment either gains a column; the package's own bound `ICommand` twins stay unbound at the canvas, because binding both gives one gesture two paths. `GraphCamera` holds the PINNED `ZoomBorder` — the alias posture at `[02]` is what makes the plain type name that control — and the whole camera capability rides its members: discrete rungs through `EnableDiscreteZoomLevels`/`DiscreteZoomLevels`, traversal through `EnableViewHistory`/`ViewHistorySize` with `NavigateBack`/`NavigateForward`, fit through `Uniform`, focus through `ZoomToRectangle`, named views through `ExportState`/`ImportState` under this owner's own keyed roster, and the grid through `ShowGrid`/`EnableSnapToGrid`/`GridSize` seeded from the same `GraphRouting` row the decorator and the position write read. The control's `SaveView`/`RestoreView` family is NOT the named-view seat: it captures whatever view is live under a name and publishes no member that seats a saved view carrying a matrix, so a roster written through it can never be RESTORED across sessions — which is why the roster lives here, why the `Drop` verb exists (a roster a user can only grow is the capability the swap would have silently dropped), and why all three named-view verbs cross the deck through the payload union's text case. The rectangle a selection-fit frames comes from the model extent fold, so a fit over a virtualized or scrolled canvas frames the same rectangle a realized one would.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ViewVerb {
    public static readonly ViewVerb Write = new("write");
    public static readonly ViewVerb Restore = new("restore");
    public static readonly ViewVerb Drop = new("drop");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphNav {
    private GraphNav() { }
    public sealed record Fit : GraphNav;
    public sealed record FitTo(Rect Content) : GraphNav;
    public sealed record Step(Rung Direction) : GraphNav;
    public sealed record ZoomBy(double Ratio, Point At) : GraphNav;
    public sealed record Travel(Rung Direction) : GraphNav;
    public sealed record Locate(Point At) : GraphNav;
    public sealed record Named(string Name, ViewVerb Verb) : GraphNav;
    public sealed record Reset : GraphNav;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class GraphCamera(ZoomBorder border) {
    public const string VerbPrefix = "graph.";

    readonly Atom<Map<string, ZoomBorderState>> bookmarks = Atom(Map<string, ZoomBorderState>());

    public ZoomBorder Border { get; } = border;

    public Map<string, ZoomBorderState> Bookmarks => bookmarks.Value;

    public Map<string, ZoomBorderState> Seat(Map<string, ZoomBorderState> roster) {
        Map<string, ZoomBorderState> retired = default;
        bookmarks.Swap(held => { retired = held; return roster; });
        return retired;
    }

    public GraphCamera Seated(GraphRouting routing, Seq<double> ladder, int history) {
        Border.EnableViewHistory = history > 0;
        Border.ViewHistorySize = history;
        Border.EnableDiscreteZoomLevels = !ladder.IsEmpty;
        Border.DiscreteZoomLevels = ladder.IsEmpty ? null : [.. ladder];
        Border.ShowGrid = routing.Grid.IsSome;
        Border.EnableSnapToGrid = routing.Snap.IsSome;
        Border.GridSize = routing.Cell.Width;
        return this;
    }

    public IO<Fin<Unit>> Navigate(GraphNav verb) =>
        IO.lift<Fin<Unit>>(() => Admit(verb).Bind(admitted => Try.lift(() => Fin.Succ(ignore(admitted.Switch(
                state: this,
                fit: static (camera, _) => fun(() => camera.Border.Uniform(false))(),
                fitTo: static (camera, v) => fun(() => camera.Border.ZoomToRectangle(v.Content, null, true))(),
                step: static (camera, v) => fun(() => camera.Border.ZoomToLevel(
                    v.Direction is Rung.Next ? camera.Border.GetNextDiscreteZoomLevel() : camera.Border.GetPreviousDiscreteZoomLevel(),
                    camera.Border.Bounds.Center.X, camera.Border.Bounds.Center.Y))(),
                zoomBy: static (camera, v) => fun(() => camera.Border.ZoomTo(v.Ratio, v.At.X, v.At.Y))(),
                travel: static (camera, v) => fun(() => {
                    if (v.Direction is Rung.Next) { camera.Border.NavigateForward(true); } else { camera.Border.NavigateBack(true); }
                })(),
                locate: static (camera, v) => fun(() => camera.Border.CenterOn(v.At, true))(),
                named: static (camera, v) => v.Verb.Switch(
                    state: (Camera: camera, v.Name),
                    write: static s => fun(() => s.Camera.Border.ExportState() switch {
                        var captured => ignore(s.Camera.bookmarks.Swap(roster => roster.AddOrUpdate(s.Name, captured))),
                    })(),
                    restore: static s => fun(() => ignore(s.Camera.bookmarks.Value.Find(s.Name)
                        .Iter(state => s.Camera.Border.ImportState(state, animate: true))))(),
                    drop: static s => fun(() => ignore(s.Camera.bookmarks.Swap(roster => roster.Remove(s.Name))))()),
                reset: static (camera, _) => fun(() => camera.Border.ResetMatrix())())))).Run().Bind(static inner => inner)));

    Fin<GraphNav> Admit(GraphNav verb) => verb.Switch(
        state: (Row: verb, Roster: bookmarks.Value),
        fit: static (s, _) => Fin.Succ(s.Row),
        fitTo: static (s, v) => v.Content.Width > 0d && v.Content.Height > 0d ? Fin.Succ(s.Row) : Refused(s.Row),
        step: static (s, _) => Fin.Succ(s.Row),
        zoomBy: static (s, v) => double.IsFinite(v.Ratio) && v.Ratio > 0d && Finite(v.At) ? Fin.Succ(s.Row) : Refused(s.Row),
        travel: static (s, _) => Fin.Succ(s.Row),
        locate: static (s, v) => Finite(v.At) ? Fin.Succ(s.Row) : Refused(s.Row),
        named: static (s, v) => !string.IsNullOrWhiteSpace(v.Name)
            && (v.Verb.Equals(ViewVerb.Write) || s.Roster.ContainsKey(v.Name))
                ? Fin.Succ(s.Row) : Refused(s.Row),
        reset: static (s, _) => Fin.Succ(s.Row));

    static bool Finite(Point at) => double.IsFinite(at.X) && double.IsFinite(at.Y);

    static Fin<GraphNav> Refused(GraphNav row) =>
        Fin.Fail<GraphNav>(new CanvasFault.CameraRejected($"{row}: argument outside its admitted domain"));
}

public static class GraphVerbs {
    public const string JumpVerb = "overview-jump";

    static readonly Func<CommandRow.Availability, bool> Anyone = static _ => true;
    static readonly Func<CommandRow.Availability, bool> WithSelection = static input => input.Selection.Count > 0;

    public static Seq<CommandRow> Rows(
        IDrawingNode drawing, GraphCamera camera, GraphFind find, Func<Seq<GraphNodeRow>> selected) =>
        toSeq(Enum.GetValues<NodeAlignment>()).Map(value =>
            Raise($"align.{Key(value)}", () => drawing.AlignSelectedNodes(value), WithSelection))
        + toSeq(Enum.GetValues<NodeDistribution>()).Map(value =>
            Raise($"distribute.{Key(value)}", () => drawing.DistributeSelectedNodes(value), WithSelection))
        + toSeq(Enum.GetValues<NodeOrder>()).Map(value =>
            Raise($"order.{Key(value)}", () => drawing.OrderSelectedNodes(value), WithSelection))
        + Seq(
            Raise("lock", drawing.LockSelection, WithSelection),
            Raise("unlock", drawing.UnlockSelection, WithSelection),
            Raise("hide", drawing.HideSelection, WithSelection),
            Raise("show", drawing.ShowSelection, WithSelection),
            Raise("show-all", drawing.ShowAll, Anyone),
            Raise("select-all", drawing.SelectAllNodes, Anyone),
            Raise("deselect-all", drawing.DeselectAllNodes, Anyone))
        + Seq(
            Camera("zoom-fit", camera, static _ => new GraphNav.Fit()),
            Camera("zoom-in", camera, static _ => new GraphNav.Step(Rung.Next)),
            Camera("zoom-out", camera, static _ => new GraphNav.Step(Rung.Prev)),
            Camera("zoom-reset", camera, static _ => new GraphNav.Reset()),
            Camera("navigate-back", camera, static _ => new GraphNav.Travel(Rung.Prev)),
            Camera("navigate-forward", camera, static _ => new GraphNav.Travel(Rung.Next)))
        + Seq(
            Named("view-save", camera, ViewVerb.Write),
            Named("view-recall", camera, ViewVerb.Restore),
            Named("view-forget", camera, ViewVerb.Drop))
        + Seq(
            Framed("zoom-selection", camera, selected),
            Walked("find-next", find, Rung.Next),
            Walked("find-previous", find, Rung.Prev));

    public static CommandRow Jump(GraphCamera camera) =>
        Row(JumpVerb, RowShape.Fielded, Anyone, (payload, _) =>
            ScrubPoint.Read(payload).Match(
                Succ: at => camera.Navigate(new GraphNav.Locate(at)).Map(static _ => unit),
                Fail: static error => IO.fail<Unit>(error)));

    public static Fin<ICommand> Jumped(CommandDeck deck) =>
        deck.Rows.TryGetValue($"{GraphCamera.VerbPrefix}{JumpVerb}", out CommandRow? row)
            ? Fin<ICommand>.Succ(ReactiveCommand.CreateFromTask<Point, DeckOutcome>(
                (at, token) => row.Run(
                    ScrubPoint.Of(at), deck, CallerModality.Operator, token)
                    .RunAsync(EnvIO.New(token: token)).AsTask(),
                outputScheduler: deck.Scheduler))
            : Fin<ICommand>.Fail(new CanvasFault.CameraRejected($"{JumpVerb} is absent from the frozen deck"));

    static CommandRow Raise(string verb, Action run, Func<CommandRow.Availability, bool> when) =>
        Row(verb, RowShape.Bare, when, (_, _) => IO.lift(() => { run(); return unit; }));

    static IO<Unit> Lowered(IO<Fin<Unit>> outcome) =>
        outcome.Bind(static settled => settled.Match(Succ: static _ => IO.pure(unit), Fail: IO.fail<Unit>));

    static CommandRow Camera(string verb, GraphCamera camera, Func<Unit, GraphNav> move) =>
        Row(verb, RowShape.Bare, Anyone, (_, _) => Lowered(camera.Navigate(move(unit))));

    static CommandRow Named(string verb, GraphCamera camera, ViewVerb view) =>
        Row(verb, RowShape.Named, Anyone, (payload, _) => payload is CommandPayload.Text named
            ? Lowered(camera.Navigate(new GraphNav.Named(named.Value, view)))
            : IO.fail<Unit>(new CanvasFault.CameraRejected($"{verb} carries no view name")));

    static CommandRow Framed(string verb, GraphCamera camera, Func<Seq<GraphNodeRow>> selected) =>
        Row(verb, RowShape.Bare, WithSelection, (_, _) => Lowered(
            camera.Navigate(selected().Extent().Match(
                Some: box => (GraphNav)new GraphNav.FitTo(box),
                None: static () => new GraphNav.Fit()))));

    static CommandRow Walked(string verb, GraphFind find, Rung direction) =>
        Row(verb, RowShape.Bare, _ => !find.Matches.IsEmpty, (_, _) =>
            find.Walk(direction).Match(
                Succ: _ => Lowered(find.Frame()),
                Fail: static error => IO.fail<Unit>(error)));

    static CommandRow Row(
        string verb, RowShape shape, Func<CommandRow.Availability, bool> when,
        Func<CommandPayload, CancellationToken, IO<Unit>> execute) =>
        new FamilyRow($"{GraphCamera.VerbPrefix}{verb}", CommandScope.Screen, shape,
            When: Some(when)).Mint(execute);

    static string Key<TCase>(TCase value) where TCase : struct, Enum =>
        string.Concat(value.ToString().Select(static (glyph, index) =>
            char.IsUpper(glyph) && index > 0 ? $"-{char.ToLowerInvariant(glyph)}" : $"{char.ToLowerInvariant(glyph)}"));
}
```

## [06]-[NAVIGATION_SURFACE]

- Owner: `GraphOverview` — the `OverviewFrame` producer the minimap renders; `GraphFind` — the match set, its cursor, and its walk; `CameraState` with `GraphView` and the generated `ViewportMap` — the viewport-state column, its wire shape, and its round-trip.
- Entry: `Frames(IObservable<Unit> ticks)` — the strip feed; `GraphFind.Walk(Rung direction)` — the match cursor step on the kernel transition type; `GraphView.Export()` and `Import(Option<string> state)` — the snapshot round-trip through the one package wire options; `GraphFind.Reveal(SearchOpen.GraphCanvas request, Func<string, Fin<Unit>> select)` — the far end of `Document/search#HIGHLIGHT_NAV`'s navigation request.
- Auto: the minimap is the `Shell/controls` `Overview` intent over the `Shell/virtualization` `OverviewFrame` model, so the graph publishes a CONTENT-SPACE frame under a source key and computes no geometry: the content rectangle is the model extent fold, the viewport rectangle is the pinned control's own `GetVisibleContentBounds()`, and the decoration bands fold over the settled `OverviewLane` roster through ONE lane-keyed marks arrow — selection, search, error, and change are four answers of one function, not four columns. The strip re-emits on the control's own `ZoomChanged`/`MatrixChanged` signals and on model change. Find composes the one `Document/search.md` plane: the graph projects its nodes as search candidates and consumes the ranked results, while the WALK and the HIGHLIGHT are this page's — the cursor steps the ranked set through a kernel `Cell.Step` transition whose declined arm IS the empty-set refusal, and the camera centres the current match. Viewport state round-trips through the control's own `ExportState`/`ImportState` over `ZoomBorderState`, and the camera's bookmark roster rides the same payload so a named view outlives the session that captured it — the domain-to-wire correspondence is the generated `ViewportMap`, whose two `[UserMapping]` converters are the one `Map`↔`Dictionary` crossing.
- Result: the screen-state snapshot carries the exported viewport column; a camera move is not an edit and fires nothing.
- Packages: PanAndZoom, System.Reactive, Avalonia, Riok.Mapperly, LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm (kernel `Cell`/`Transition`), BCL inbox
- Growth: a new decoration lane is one `OverviewLane` row the marks arrow already answers; a new navigation verb is one `GraphNav` case; one wire column per new viewport axis; zero new surface.
- Boundary: the graph publishes an `OverviewFrame` and renders nothing — a graph-local minimap control is the `Shell/virtualization#OVERVIEW_PROJECTION` rejected form, and the strip's drag publishes a content-space point back through the jump verb so the canvas moves its OWN camera; the frame's axis is `OverviewAxis.Plane`, because a graph summarized under an independently-scaled fit renders a distorted map of the thing it exists to make navigable. ZOOM HUD ownership is RULED to the package: the pinned control's own indicator (`ShowZoomIndicator`, `ZoomIndicatorPosition`, `ZoomIndicatorFormat`, `ZoomIndicatorAutoHideDuration`, `IsZoomIndicatorVisible`) reads the live matrix inside the viewport with no subscription, where a chrome chip mirroring the same number needs a subscription, a second formatter, and a placement that tracks a viewport it sits outside. The viewport column is the SCREEN-STATE snapshot's, not the co-edit document's, because a camera is per-viewer and committing it drags every peer's view along with one peer's pan; the snapshot payload rides the one composition-seated `EvidenceOps.Wire` options and `GraphViewport` registers on the package wire context, so a stale or foreign snapshot refuses through the same admission every durable AppUi payload crosses.

```csharp
// --- [MODELS] --------------------------------------------------------------------------

public sealed record GraphOverview(
    GraphCamera Camera,
    Func<Seq<GraphNodeRow>> Nodes,
    Func<OverviewLane, Set<string>> Marks) {
    public const string SourceKey = "graph.overview";
    public const string IntentKey = "graph.minimap";

    public IObservable<OverviewFrame> Frames(IObservable<Unit> ticks) =>
        ticks.StartWith(unit).Select(_ => Frame()).DistinctUntilChanged().Replay(1).RefCount();

    public static ControlIntent Intent(IntentBinding binding) =>
        new ControlIntent.Overview(IntentKey, OverviewAxis.Plane, SourceKey,
            $"{GraphCamera.VerbPrefix}{GraphVerbs.JumpVerb}", binding);

    OverviewFrame Frame() =>
        Nodes().Filter(static row => row.Visible) switch {
            var visible => new OverviewFrame(
                visible.Extent().Filter(static box => box.Width > 0d && box.Height > 0d).IfNone(new Rect(0d, 0d, 1d, 1d)),
                Camera.Border.GetVisibleContentBounds(),
                toSeq(OverviewLane.Items).Map(lane => Band(visible, lane, Marks(lane))).Strict()),
        };

    static OverviewBand Band(Seq<GraphNodeRow> rows, OverviewLane lane, Set<string> keys) =>
        new(lane, rows.Filter(row => keys.Contains(row.Key))
            .Map(static row => new Rect(row.X, row.Y, row.Width, row.Height)).Strict());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class GraphFind(GraphCamera camera, Func<Seq<GraphNodeRow>> nodes) {
    readonly Atom<(Seq<SearchResult> Hits, int Cursor)> state = Atom((Seq<SearchResult>(), -1));

    GraphCamera Camera { get; } = camera;

    Func<Seq<GraphNodeRow>> Nodes { get; } = nodes;

    public Set<string> Matches => toSet(state.Value.Hits.Choose(static hit => hit.Member));

    public Seq<SearchDocument> Candidates(string docKey) =>
        Nodes().Map(row => new SearchDocument(
            SearchSource.Node, docKey, Some(row.Key), row.Title, $"{row.Title} {row.TemplateKey}"));

    public Unit Seat(Seq<SearchResult> hits) =>
        ignore(state.Swap(_ => (hits, hits.IsEmpty ? -1 : 0)));

    public Fin<int> Walk(Rung direction) =>
        Cell.Step(
            cell: state,
            step: held => held.Hits.IsEmpty
                ? Option<(Seq<SearchResult>, int)>.None
                : Some((held.Hits, (((held.Cursor + (int)direction) % held.Hits.Count) + held.Hits.Count) % held.Hits.Count)),
            declined: new CanvasFault.ModelRejected("find carries no matches to walk")) switch {
            Transition<(Seq<SearchResult> Hits, int Cursor)>.Committed committed => Fin.Succ(committed.State.Cursor),
            Transition<(Seq<SearchResult> Hits, int Cursor)> declined => Fin.Fail<int>(
                declined is Transition<(Seq<SearchResult> Hits, int Cursor)>.Refused refused
                    ? refused.Cause
                    : new CanvasFault.ModelRejected("find carries no matches to walk")),
        };

    public IO<Fin<Unit>> Reveal(SearchOpen.GraphCanvas request, Func<string, Fin<Unit>> select) =>
        Nodes().Find(row => StringComparer.Ordinal.Equals(row.Key, request.NodeKey))
            .ToFin(Fail: new CanvasFault.ModelRejected($"search/node:{request.NodeKey}"))
            .Bind(row => select(row.Key).Map(_ => row))
            .Match(
                Succ: row => Camera.Navigate(new GraphNav.Locate(
                    new Point(row.X + (row.Width / 2d), row.Y + (row.Height / 2d)))),
                Fail: error => IO.pure(Fin.Fail<Unit>(error)));

    public IO<Fin<Unit>> Frame() =>
        state.Value switch {
            var held when held.Cursor >= 0 => held.Hits[held.Cursor].Member
                .Bind(key => Nodes().Find(row => StringComparer.Ordinal.Equals()))
                .Match(
                    Some: row => Camera.Navigate(new GraphNav.Locate(new Point(row.X + (row.Width / 2d), row.Y + (row.Height / 2d)))),
                    None: () => IO.pure(Fin.Succ(unit))),
            _ => IO.pure(Fin.Succ(unit)),
        };
}

public sealed record CameraState(ZoomBorderState State, Map<string, ZoomBorderState> Views);

public sealed record GraphViewport(ZoomBorderState State, Dictionary<string, ZoomBorderState> Views);

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both,
    EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
public static partial class ViewportMap {
    public static partial GraphViewport ToWire(CameraState camera);
    public static partial CameraState FromWire(GraphViewport wire);

    [UserMapping]
    private static Dictionary<string, ZoomBorderState> Wired(Map<string, ZoomBorderState> views) =>
        views.AsIterable().ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal);

    [UserMapping]
    private static Map<string, ZoomBorderState> Seated(Dictionary<string, ZoomBorderState> views) =>
        toSeq(views).Fold(Map<string, ZoomBorderState>(), static (roster, entry) => roster.AddOrUpdate(entry.Key, entry.Value));
}

public sealed record GraphView(GraphCamera Camera) {
    public Option<string> Export() =>
        Optional(Camera.Border.ExportState()).Map(state =>
            JsonSerializer.Serialize(ViewportMap.ToWire(new CameraState(state, Camera.Bookmarks)), EvidenceOps.Wire));

    public Fin<Unit> Import(Option<string> state) =>
        state.Filter(static payload => !string.IsNullOrWhiteSpace(payload)).Match(
            Some: payload => Try.lift(() => Fin.Succ(JsonSerializer.Deserialize<GraphViewport>(payload, EvidenceOps.Wire))).Run().Bind(static inner => inner)
                .Bind(decoded => Optional(decoded)
                    .ToFin(Fail: (Error)new CanvasFault.CameraRejected("viewport state decoded to nothing"))
                    .Map(admitted => ViewportMap.FromWire(admitted) switch {
                        var camera => fun(() => {
                            Camera.Seat(camera.Views);
                            Camera.Border.ImportState(camera.State, animate: false);
                            return unit;
                        })(),
                    })),
            None: static () => Fin.Succ(unit));
}
```

## [07]-[CANVAS_SKIN]

- Owner: `GraphSkin` — the node-editor shipped-key correspondence, its host-key closure, and the icon rows the package templates consume.
- Entry: `Slots` — the role-to-shipped-key roster the token emission folds; `Icons` — the chrome glyph rows the editor templates resolve, each a typed `GlyphSlot` pairing the package key with the asset key it resolves through; `Glyphs(AssetRuntime runtime, ResolvedTheme resolved, int step, double scale, FlowDirection flow)` — the resolved carriers the swap re-materializes.
- Auto: every key the package's own theme defines per variant re-seeds from the resolved role ladder, so the pin, connector, crossing, rejection, node-handle, and rotation-readout families follow a variant swap through the one emission and this page writes no brush. The package resolves each key as a dynamic resource against the application resources before its own dictionaries compose, so a slot the emission mints WINS over the package's shipped value, and a key the package consumes yet defines nowhere — the two editor background keys and the four chrome icon keys — closes here rather than rendering blank chrome: the backgrounds land on the surface ladder and the icons resolve through the asset registry, riding the swap's `Rematerialize.TintedAsset` roster because a tinted glyph holds its pigment in its own bitmap. The guide key the package binds to BOTH the guide overlay and the selected-connector overlay splits at this roster.
- Packages: Avalonia, NodeEditorAvalonia, LanguageExt.Core
- Growth: a new package key is one slot row naming the role it re-seeds from; a new chrome glyph is one `GlyphSlot` row at the asset catalog; zero new surface.
- Boundary: the roster mints VALUES onto the one emission and never a second dictionary — a graph-local `ResourceDictionary` merged beside the emitted one goes stale on the next re-seed; the slot cases are the token catalogue's shipped-key correspondence vocabulary, and the roster's key strings are FOREIGN package keys, not `TokenKey`s — the one place a raw key string is the value itself. The grid decorator's cell columns default to zero, so the sizing lands on `GraphRouting`, and the decorator, the position snap, and the viewport's own grid all read that one row. A code-level colour property on a node, pin, or connector does not exist and `Connector` derives `Shape`, so stroke and thickness reach it through a theme setter alone; a paint written onto a control is the `Theme/tokens#CONTROL_THEMES` deleted form.

```csharp
// --- [TABLES] --------------------------------------------------------------------------

public readonly record struct GlyphSlot(string Slot, AssetKey Key);

public static class GraphSkin {
    public const double GridFallback = 16d;

    public static readonly Seq<SemiSlot> Slots = Seq<SemiSlot>(
        new SemiSlot.Pigment(PaintRole.Raised, 0, "PinBackgroundBrush"),
        new SemiSlot.Pigment(PaintRole.Accent, 1, "PinPointerOverBackgroundBrush"),
        new SemiSlot.Pigment(PaintRole.Text, 0, "PinForegroundBrush"),
        new SemiSlot.Pigment(PaintRole.AccentText, 0, "PinPointerOverForegroundBrush"),
        new SemiSlot.Pigment(PaintRole.Separator, 0, "ConnectorBackgroundBrush"),
        new SemiSlot.Pigment(PaintRole.Panel, 0, "ConnectorLabelBackgroundBrush"),
        new SemiSlot.Pigment(PaintRole.Border, 0, "ConnectorLabelBorderBrush"),
        new SemiSlot.Pigment(PaintRole.Separator, 1, "ConnectorCrossingStrokeBrush"),
        new SemiSlot.Pigment(PaintRole.Surface, 0, "ConnectorCrossingBackgroundBrush"),
        new SemiSlot.Extent(MetricFamily.Stroke, 0, "ConnectorCrossingStrokeThickness"),
        new SemiSlot.Extent(MetricFamily.Radius, 1, "ConnectorCrossingArcRadius"),
        new SemiSlot.Pigment(PaintRole.Error, 0, "ConnectionRejectedBrush"),
        new SemiSlot.Pigment(PaintRole.Panel, 0, "ConnectionRejectedLabelBackgroundBrush"),
        new SemiSlot.Pigment(PaintRole.Error, 1, "ConnectionRejectedLabelBorderBrush"),
        new SemiSlot.Pigment(PaintRole.ErrorText, 0, "ConnectionRejectedLabelForegroundBrush"),
        new SemiSlot.Pigment(PaintRole.Accent, 0, "NodeResizeHandleFillBrush"),
        new SemiSlot.Pigment(PaintRole.Border, 0, "NodeResizeHandleBorderBrush"),
        new SemiSlot.Pigment(PaintRole.Panel, 0, "RotationSnapReadoutBackgroundBrush"),
        new SemiSlot.Pigment(PaintRole.Border, 0, "RotationSnapReadoutBorderBrush"),
        new SemiSlot.Pigment(PaintRole.Text, 0, "RotationSnapReadoutForegroundBrush"),
        new SemiSlot.Pigment(PaintRole.Focus, 0, "GuideLineBrush"),
        new SemiSlot.Pigment(PaintRole.Selection, 0, "ConnectorSelectedStrokeBrush"),
        new SemiSlot.Pigment(PaintRole.Workbench, 0, "EditorBackground"),
        new SemiSlot.Pigment(PaintRole.Well, 0, "DrawingBackground"));

    public static readonly Seq<GlyphSlot> Icons = Seq(
        new GlyphSlot("EditorCutIcon", AssetDeclaration.EditorCut.Asset),
        new GlyphSlot("EditorCopyIcon", AssetDeclaration.EditorCopy.Asset),
        new GlyphSlot("EditorPasteIcon", AssetDeclaration.EditorPaste.Asset),
        new GlyphSlot("DeleteIcon", AssetDeclaration.EditorDelete.Asset));

    public static Fin<Seq<(string Slot, IImage Image)>> Glyphs(
        AssetRuntime runtime, ResolvedTheme resolved, int step, double scale, FlowDirection flow) =>
        Icons.TraverseM(row => IconSurface
                .Resolve(runtime, new AssetRequest(row.Key, step, scale, flow, new GlyphForm.Image()), resolved)
                .Bind(static product => product.Image)
                .Map(image => (row.Slot, Image: image)))
            .As()
            .Map(static rows => rows.Strict());
}
```

## [08]-[COEDIT_BRIDGE]

- Owner: `GraphCoEdit` — the composition record that mints bindings; `GraphBinding` — the ONE bidirectional projection between one ReactiveUI graph canvas and the `Collab/sync.md` `LoroTree` node register beside its `LoroMap` edge register, carrying BOTH directions; `ReconcileGate` — the serialized whole-state reconcile gate that TYPES the folder latch law.
- Entry: `Bind(CollabDoc doc, GraphCanvas canvas)` — one binding per canvas (the document identity reads off `doc.Key`, so a second key parameter the value reconstructs is gone), seating one scoped subscription per graph container with rollback custody; `CommitLocal(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges, GraphOp op, string origin)` on the binding — the outbound direction re-admits the post-op topology before the intent rides `IntentLedger.Commit`; the binding's own subscription sink is the inbound direction.
- Auto: the graph structure maps onto `LoroTree` — a node is a tree node whose meta map carries the `GraphNodeRow` columns and whose position under its parent IS the containment column, an edge is a child row on the connection register carrying its `GraphWire` — and subscriber diffs discriminate `EventTriggerKind.Local`/`Import`/`Checkout` for ECHO SUPPRESSION: a local commit arrives back as its own `Local` diff and is dropped, a mutation raised BY a remote apply commits nothing back, and a remote `Import` diff applies to the graph model WITHOUT re-emitting. Suppression is a DROP on both sides and never a fault; the one diff-side fault is `CanvasFault.TriggerUnsupported`. A hierarchy move rides the sync-owned `GraphOp.NodeMove` onto the tree's identity-preserving `MovTo` and a position write commits as `GraphOp.NodeAt` minted through `GraphCanvas.Placed`, both on the same commit leg.
- Result: durable truth rides `Collab/sync.md`'s typed edit-intent stream; the live half rides the session-ephemeral Loro wire — this page persists nothing.
- Packages: LoroCs, ReactiveUI, LanguageExt.Core, Rasm (kernel `Custody`)
- Growth: a new co-edited column is one meta-map key; a new structural verb is one `GraphOp` case landed at the sync owner; zero new surface.
- Boundary: the binding is the ONE projection per canvas and covers both directions — a second inbound sink, a canvas-local `LoroTree` mutation beside `IntentApply.Apply`, a model-poll loop, or a per-node sync channel is the deleted form; remote-applied diffs re-run the `GraphAdmission` gate, and a cycle-closing edit surfaces as the typed conflict row for the presence UI. The gate and the conflict cell live on the BINDING rather than the composition record, because `Bind` mints one binding per canvas while the record composes once: a gate on the record made two canvases over one document cross-suppress, dropping a peer's edit with no fault and no divergence the merge can repair. `Open`, `Subscribe`, `ReadNodes`, and `ReadEdges` are the composition adapters over the verified container values; `Subscribe` is ADDRESSED and the binding seats one per graph container, because the node tree and the edge register are separate roots and a tree-only subscription made every remote edge change a silent no-render — a root-feed subscription beside them is equally deleted, since it re-reconciles this canvas on every unrelated document edit. The multi-seat mount carries CUSTODY across its roster through the kernel rollback fold, so a refusal on the second container releases the seat the first took and a partial bind never leaves a live sink feeding a binding the caller never received.

```csharp
// --- [COMPOSITION] ---------------------------------------------------------------------

public sealed record GraphCoEdit(
    GraphAdmission Gate,
    IntentLedger Ledger,
    Func<CollabDoc, Fin<(LoroTree Tree, LoroMap Edges)>> Open,
    Func<CollabDoc, CollabAddress, Subscriber, Fin<Subscription>> Subscribe,
    Func<LoroTree, Fin<Seq<GraphNodeRow>>> ReadNodes,
    Func<LoroMap, Fin<Seq<GraphEdge>>> ReadEdges) {
    public IO<Fin<GraphBinding>> Bind(CollabDoc doc, GraphCanvas canvas) =>
        IO.lift<Fin<GraphBinding>>(() =>
            from containers in Open(doc)
            let binding = new GraphBinding(this, doc, canvas, containers.Tree, containers.Edges)
            from live in Seated(doc, binding)
            select binding.Seated(live));

    Fin<Seq<Subscription>> Seated(CollabDoc doc, GraphBinding binding) =>
        Subscribe(doc, CollabAddress.Of(CollabRoot.Graph), binding).Bind(tree =>
            Subscribe(doc, CollabAddress.Of(CollabRoot.Edges), binding)
                .Rollback(tree)
                .Map(edges => Seq(tree, edges)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ReconcileGate {
    int applying;
    int restated;

    public bool Held => Volatile.Read(ref applying) == 1;

    public Unit Signal(Action reconcile) {
        Interlocked.Exchange(ref restated, 1);
        if (Interlocked.CompareExchange(ref applying, 1, 0) == 1) { return unit; }
        do {
            while (Interlocked.Exchange(ref restated, 0) == 1) { reconcile(); }
            Interlocked.Exchange(ref applying, 0);
        } while (Volatile.Read(ref restated) == 1 && Interlocked.CompareExchange(ref applying, 1, 0) == 0);
        return unit;
    }
}

public sealed class GraphBinding(
    GraphCoEdit owner, CollabDoc doc, GraphCanvas canvas, LoroTree tree, LoroMap edges)
    : Subscriber, IDisposable {
    readonly ReconcileGate gate = new();
    Seq<Subscription> live = Seq<Subscription>();

    public Atom<Option<Error>> Conflict { get; } = Atom(Option<Error>.None);

    public GraphBinding Seated(Seq<Subscription> subscriptions) { live = subscriptions; return this; }

    public IO<Fin<Unit>> CommitLocal(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges, GraphOp op, string origin) =>
        gate.Held
            ? IO.pure(Fin.Succ(unit))
            : string.IsNullOrWhiteSpace(origin)
                ? IO.pure(Fin.Fail<Unit>(new CanvasFault.ModelRejected("origin is required")))
                : owner.Gate.Admitted(nodes, edges).Match(
                    Succ: _ => owner.Ledger.Commit(doc, new EditIntent.GraphStructure(), origin),
                    Fail: error => IO.pure(Fin.Fail<Unit>(error)));

    public void OnDiff(DiffEvent diff) {
        using (diff) {
            EventTriggerKind trigger = diff.TriggeredBy;
            ignore(trigger switch {
                EventTriggerKind.Local => unit,
                EventTriggerKind.Import or EventTriggerKind.Checkout => gate.Signal(Reconcile),
                _ => fun(() => Conflict.Swap(_ => Some<Error>(new CanvasFault.TriggerUnsupported($"{trigger}"))))(),
            });
        }
    }

    public void Dispose() { live.Iter(static subscription => subscription.Dispose()); live = Seq<Subscription>(); }

    void Reconcile() =>
        ignore((from rows in owner.ReadNodes(tree)
                from pairs in owner.ReadEdges(edges)
                from _ in canvas.Commit(rows, pairs, CommitMode.Replace)
                select unit)
            .Match(
                Succ: _ => Conflict.Swap(_ => None),
                Fail: error => Conflict.Swap(_ => Some(error))));
}
```

## [09]-[PROJECTIONS]

- Owner: `DependencyRead` — the read-projection fold family (renamed from the three-folder `GraphProjection` name collision).
- Entry: `FromDependencies(GraphAdmission gate, RecomputeGraph.Graph graph, Map<string, (double X, double Y)> layout, string templateKey, Size extent, ConnectorStyle style)` — the notebook cell-dependency graph renders as a READ projection onto canvas rows in topological order; edits on this projection are disabled by SHAPE (a dependency edge derives from a node's own recorded inputs, never a hand-drawn connector).
- Auto: the fold reads the port's own node map — each node's `Hex` is the canvas key, its `Descriptor` the title, its `Inputs` the incoming edges — and composes `GraphAdmission.Order`, so the rendered row order and any solve-order consumer read ONE topological fold off the same acyclicity oracle the edit gate reads. A projected row is flat and locked at the caller's declared extent; a projected wire carries no authored decoration, seeding from the caller's style through the one `GraphWire.Seed`. Canvas snapshot export rides the capture capsule — the editor surface renders through the capture in-tree lane and encodes through `VisualCodec` as kind graph; PNG/SVG/PDF export composes `Document/export.md`'s destination union.
- Packages: NodeEditorAvalonia, Rasm.AppHost (project), QuikGraph (shared tier), SkiaSharp, LanguageExt.Core
- Growth: a new read projection (Compute solve graph, Fabrication posting chain) is one `From*` fold returning rows; zero new surface.
- Boundary: the dependency projection READS the AppHost `RecomputeGraph` vocabulary through the declared port (decode-only); a projection-local dependency model, a second topological sort beside `GraphAdmission.Order`, and a layout fallback that DROPS an unplaced node are the deleted forms — an omitted dependency reads as a graph that is not there. The projection's gate carries a settings row admitting pin fanout, since one recompute output legitimately feeds many nodes; a gate configured for single-pin connections refuses the whole projection by name rather than rendering a partial graph.

```csharp
public static class DependencyRead {
    public const string InPin = "in";
    public const string OutPin = "out";

    public static Fin<Seq<GraphNodeRow>> FromDependencies(
        GraphAdmission gate, RecomputeGraph.Graph graph, Map<string, (double X, double Y)> layout,
        string templateKey, Size extent, ConnectorStyle style) {
        Seq<GraphNodeRow> rows = toSeq(graph.Nodes.Values).Map(node => Row(node, layout, templateKey, extent)).Strict();
        Seq<GraphEdge> edges = toSeq(graph.Nodes.Values).Bind(node => node.Inputs.Map(input => new GraphEdge(
            new GraphEndpoint(input.Hex, Some(OutPin)),
            new GraphEndpoint(node.Hash.Hex, Some(InPin)),
            GraphWire.Seed(style)))).Strict();
        return gate.Order(rows, edges).Map(order => Ranked(rows, order));
    }

    static GraphNodeRow Row(
        RecomputeNode node, Map<string, (double X, double Y)> layout, string templateKey, Size extent) =>
        layout.Find(node.Hash.Hex).IfNone((X: 0d, Y: 0d)) switch {
            var at => new GraphNodeRow(node.Hash.Hex, templateKey, node.Descriptor, Parent: None,
                at.X, at.Y, extent.Width, extent.Height, Rotation: 0d, Locked: true, Visible: true, Seq(
                    new GraphPinRow(InPin, InPin, PinAlignment.Left, PinDirection.Input, BusWidth: 1),
                    new GraphPinRow(OutPin, OutPin, PinAlignment.Right, PinDirection.Output, BusWidth: 1))),
        };

    static Seq<GraphNodeRow> Ranked(Seq<GraphNodeRow> rows, Seq<string> order) =>
        rows.Fold(Map<string, GraphNodeRow>(), static (index, row) => index.Add(row.Key, row)) switch {
            var index => order.Choose(index.Find),
        };
}
```
