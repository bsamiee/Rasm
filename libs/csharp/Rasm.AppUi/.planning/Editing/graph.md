# [APPUI_EDITING_GRAPH]

The graph canvas is the typed-edit plane's node surface: NodeEditorAvalonia `IDrawingNode`/`DrawingNodeEditor` realize the parametric/dependency-graph canvas on ReactiveUI — node/pin/connector editing over a typed graph model, QuikGraph owning the connection-admission cycle gate and graph algebra, `LoroTree` as the co-edit data seam under ONE bidirectional projection with `EventTriggerKind` echo suppression, and canvas snapshots exporting through the capture encode fold. The page owns the graph model rows at the package's full node and connector concept, the template palette and drop ingress, the pressure-aware ink plane, the selection-layout and camera verb projections, the overview-strip feed with its viewport state, the variant-keyed canvas skin, the admission gate, the co-edit bridge, the typed `CanvasFault` rail, and the notebook dependency read projection. Recompute stays the AppHost `RecomputeGraph`'s — this canvas renders and edits structure, never re-solves.

## [01]-[INDEX]

- [02]-[GRAPH_MODEL]: Typed node/pin/connector rows at the package's whole concept on the ReactiveUI drawing model.
- [03]-[ADMISSION_GATE]: QuikGraph cycle gate over wiring and containment; typed `CanvasFault`.
- [04]-[PALETTE_INGRESS]: Template registry, drop targets, and the pressure-aware ink plane.
- [05]-[CANVAS_VERBS]: Selection-layout rows and the typed camera verb union over the pinned viewport.
- [06]-[NAVIGATION_SURFACE]: Overview feed, jump lift, find walk, guides, and viewport state.
- [07]-[CANVAS_SKIN]: Variant-keyed package slots, host-key closure, and the grid sizing rows.
- [08]-[COEDIT_BRIDGE]: One bidirectional `LoroTree` projection, one latch per binding.
- [09]-[PROJECTIONS]: Notebook dependency read projection; capture snapshot export.

## [02]-[GRAPH_MODEL]

- Owner: `GraphNodeRow` and `GraphPinRow` are the package-neutral model rows; `GraphEndpoint` preserves node and pin identity and `GraphLink` is the endpoint pair an edge is IDENTIFIED by; `GraphWire` is the per-edge presentation row; `GraphEdge` pairs the two; `GraphRouting` is the resolved placement-and-path policy row; `GraphModelAdapter` binds the package's mint surface and the graph serializer; `GraphCanvas` owns two-phase materialization over one `DrawingNodeEditor`.
- Entry: `Materialize(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges)` admits structure, stages every node and connector through `GraphModelAdapter`, and mutates the live drawing only in the success arm; `Reset` performs the same gate and staging before an atomic replacement; `Paste` stages, round-trips each node through the installed serializer's `Clone<T>`, and commits the clones; `Placed(string nodeKey, double x, double y)` mints the position op through the routing lattice; `Reparented(string nodeKey, Option<string> parent, uint index)` mints the containment op the tree commits.
- Auto: the node row carries every column `INode` declares — containment parent, extent, rotation, lock, and visibility — so the declared hierarchy-move op has a model that expresses it and a co-edited group, frame, or collapsed subgraph round-trips whole; the edge row splits identity from presentation, so waypoints, per-edge routing mode, arrow styles, offset, and label travel beside a `GraphLink` the duplicate and fanout folds key on unchanged. `IDrawingNodeSettings` IS the one connection-policy authority and every column it owns is read: `GraphCanvas.Wired` reads direction and bus width and delegates final connectability to `DrawingNodeEditor.CanConnectPin`, `GraphAdmission` reads the connection-enable, self-connection, duplicate, and per-pin-fanout columns and imposes the stronger dependency-DAG invariant over BOTH the wiring graph and the containment forest, and `GraphRouting.Of` lifts the snap, grid, guide, and nudge columns into the one row every position write, connector path, and grid decorator reads — so the batch gate, the interactive drag, and the painted lattice cannot answer differently. Clone, paste, and duplication ride `INodeSerializer` through the editor's own `Clone<T>`, and node minting rides the palette's `INodeTemplate` rows through that same clone.
- Receipt: every committed structural edit seals an Edit-case `EvidenceReceipt` and projects a typed edit-intent op onto the `Collab/sync.md` durable stream — the graph mints no parallel op union.
- Packages: NodeEditorAvalonia (+`.Model` transitive-floor pin), PanAndZoom, ReactiveUI, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new node kind is one palette template row; a new pin shape is one `GraphPinRow` value; a new connector presentation is one `GraphWire` column off the package's own enums; a retuned lattice is one `GraphRouting` column off the settings row; zero new surface.
- Boundary: connector routing and hit testing stay the package's `OrthogonalRouter`/`RTree`/`HitTestIndex` — `GraphRouting` carries the `ConnectorRoutingAlgorithm` and the default `ConnectorStyle` a render binds and re-implements neither, and a per-edge `ConnectorRoutingMode.Manual` hands that edge's path to its own `Waypoints` exactly as the package intends. Pan and zoom ride the PINNED `PanAndZoom` `ZoomBorder` hosting a bare `DrawingNode` canvas, never the package's `Editor` control and never `NodeZoomBorder`: the transitive `Avalonia.Controls.PanAndZoom` assembly and the pinned `PanAndZoom` assembly BOTH publish `Avalonia.Controls.PanAndZoom.ZoomBorder`, the collision is a package-id rename rather than a namespace clash, `NodeZoomBorder` derives the LEGACY type and adds seven parameterless command shims and nothing else, and every saved-view, view-history, discrete-zoom, grid, rotation, and state-export member lives on the pinned type alone — so `Editor`, whose template fills `ZoomControl` from `PART_ZoomBorder` with the legacy base, is the deleted host and `[05]-[CANVAS_VERBS]` binds the pinned control directly; the manifest posture is `Aliases` metadata on the TRANSITIVE reference (`ExcludeAssets` is not viable because `NodeZoomBorder` inherits the type it removes), placed on the legacy package rather than the pinned one, since aliasing the pinned package lifts its whole type set out of global scope and every existing plain `ZoomBorder` mention across the corpus then binds the legacy type silently while only the absent members fault — and the alias must be manifest metadata rather than a source `extern alias`, because the Avalonia name generator emits its own partial naming the type unqualified and no source directive reaches generated code. Ursa `ImageViewer` is the third pan-zoom owner in the package closure and stays scoped to image presentation, so the viewport closure is exactly three owners with three disjoint seats. The editor's `IUndoRedoHost` binds to the one `Editing/history.md` `EditHistory` — `Undo`/`Redo` delegate to the `history.undo`/`history.redo` intents and `BeginUndoBatch`/`EndUndoBatch` open and seal one `RevertDelta.Composite` op so a multi-op canvas gesture undoes as one unit, and the package's own coalesced-history surface therefore rides the one revert vocabulary, never a second undo stack; the canvas renders structure and routes recompute through the AppHost `RecomputeGraph` port exactly as the notebook does — a canvas-local topo/dirty engine is the deleted form.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------

public sealed record GraphPinRow(string Key, string Name, PinAlignment Alignment, PinDirection Direction, int BusWidth);

// The node row at the concept `INode` spells. Containment, extent, rotation, lock, and visibility are model
// columns rather than live-control state, because the hierarchy-move op commits a parent and an index onto
// the co-edit tree and a model that could not express a parent left that op addressing a shape no peer could
// rehydrate — a group, a frame, and a collapsed subgraph were all durable-but-unreadable.
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

public readonly record struct GraphEndpoint(string NodeKey, Option<string> PinKey);

// Edge IDENTITY is the pin-qualified endpoint pair alone. Duplicate detection, fanout counting, and the
// co-edit register key all read this value, so adding presentation columns to the edge cannot make two
// renderings of one wiring read as two distinct edges.
public readonly record struct GraphLink(GraphEndpoint From, GraphEndpoint To);

// Per-edge presentation as DATA off the package's own bounded vocabularies: a manual route carries its own
// waypoints, an automatic route defers to the lattice the settings row selects, and the arrow pair, offset,
// and label are columns rather than a second connector model. `Of` seeds a new wire from the resolved
// routing row, so a default-styled edge and the settings authority cannot disagree at the first render.
public sealed record GraphWire(
    ConnectorRoutingMode Routing,
    ConnectorStyle Style,
    ConnectorOrientation Orientation,
    ConnectorArrowStyle StartArrow,
    ConnectorArrowStyle EndArrow,
    double Offset,
    Option<string> Label,
    Seq<(double X, double Y)> Waypoints) {
    public static GraphWire Of(GraphRouting routing) =>
        new(ConnectorRoutingMode.Auto, routing.Style, ConnectorOrientation.Auto,
            ConnectorArrowStyle.None, ConnectorArrowStyle.Arrow, Offset: 0d, None, Seq<(double, double)>());

    // A READ projection's wire carries no authored decoration at all: waypoints, a label, and an offset are
    // things an editor placed, and a derived edge that carried them would be presenting as authored a shape
    // no user ever drew. The style is the settings row's own default at composition, so a projected canvas
    // and an edited one still render one connector vocabulary.
    public static GraphWire Read(ConnectorStyle style) =>
        new(ConnectorRoutingMode.Auto, style, ConnectorOrientation.Auto,
            ConnectorArrowStyle.None, ConnectorArrowStyle.Arrow, Offset: 0d, None, Seq<(double, double)>());
}

public sealed record GraphEdge(GraphEndpoint From, GraphEndpoint To, GraphWire Wire) {
    public GraphLink Ends => new(From, To);
}

// The composition adapter is the package's MINT surface, not a projection surface: reading a pin's key,
// direction, bus width, or a node's pin list are total reads off the package contracts, so they are static
// folds here rather than delegate columns a composition root supplies and could supply inconsistently. Only
// the two constructions the package cannot perform without the product's own types — a pin and a connector —
// remain columns, beside the palette that mints nodes and the serializer that clones them.
public sealed record GraphModelAdapter(
    GraphPalette Palette,
    Func<GraphPinRow, Fin<IPin>> Pin,
    Func<GraphWire, Fin<IConnector>> Wire,
    INodeSerializer Serializer) {
    // A minted node is the template's clone DRESSED with the row's own columns, so a template supplies the
    // content, chrome, and behaviour while the row supplies identity, placement, extent, posture, and ports.
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

    // The pin key is the pin's own `Name`, so identity crosses the package boundary on the member the package
    // already round-trips through its serializer rather than on a side table keyed by reference.
    public static Seq<IPin> Pins(INode node) => toSeq(node.Pins ?? []);

    public static string PinKey(IPin pin) => pin.Name ?? string.Empty;

    // Direction and bus width are the OPTIONAL typing contract: a pin that declines it is bidirectional at
    // unit width, which is exactly the policy the settings row then evaluates rather than a refusal, because
    // an untyped port is a legal port and only a MISMATCHED one is a fault.
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

// Placement and connector-path policy as DATA off the one settings authority: every snap, grid, guide, and
// nudge column the settings row declares travels beside the two routing enums as one resolved row, so a
// position write commits a SNAPPED coordinate, a connector renders under a declared algorithm and style, the
// guide adorner tests against the tolerance the policy declared, and the grid decorator paints the same
// lattice the drag quantizes to, rather than each write, each render, and each overlay inventing its own.
// `Of` reads the settings row instead of taking loose knobs — a caller-supplied grid pitch beside that row is
// the deleted form, and so is a raw coordinate reaching the position op, which lands a canvas on a lattice
// the drag would have quantized away and makes two peers converge to two positions for one gesture.
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
    double GuideTolerance,
    double NudgeStep,
    double NudgeMultiplier) {
    // A grid cell is lifted onto the resolved space scale when the settings row leaves it non-positive,
    // because the decorator's own cell columns default to zero and a zero-sized grid paints nothing at all —
    // the overlay mounts, resolves its brush, measures, and draws an empty surface no symptom points at.
    public static GraphRouting Of(IDrawingNodeSettings policy, ResolvedTheme resolved, ConnectorRoutingAlgorithm algorithm, ConnectorStyle style) =>
        resolved.Metric(MetricFamily.Space, 3).IfNone(GraphSkin.GridFallback) switch {
            var cell => new(algorithm, style,
                policy.EnableSnap, policy.SnapX, policy.SnapY,
                policy.EnableGrid, Sized(policy.GridCellWidth, cell), Sized(policy.GridCellHeight, cell),
                policy.EnableGuides, policy.GuideSnapTolerance, policy.NudgeStep, policy.NudgeMultiplier),
        };

    // The ONE position projection every write crosses. The package's own lattice is `NodeEditor.SnapHelper`,
    // which is INTERNAL (decompile-proven), so the rounding is TRANSCRIBED rather than called — and the
    // transcription is the convergence guarantee precisely because it is that helper's exact body:
    // away-from-zero rounding of the quotient, scaled back by the pitch. A snap-disabled policy answers the
    // raw coordinate, so the lattice is policy data rather than a branch at each write site.
    public (double X, double Y) Place(double x, double y) =>
        Snap ? (Quantized(x, SnapX), Quantized(y, SnapY)) : (x, y);

    // The pitch guard rides per AXIS, exactly as the package's own point overload does: a policy declaring one
    // live axis and one degenerate axis still quantizes the axis it declared, where a paired guard dropped
    // both to raw coordinates and put every such canvas off the lattice its drag was already snapping to.
    static double Quantized(double value, double pitch) =>
        Math.Abs(pitch) switch {
            var step when step > 0d => Math.Round(value / step, MidpointRounding.AwayFromZero) * step,
            _ => value,
        };

    static double Sized(double declared, double resolved) =>
        double.IsFinite(declared) && declared > 0d ? declared : resolved;
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

    // Containment commits as the identity-preserving tree move, so a node entering a group keeps the key
    // every edge endpoint, receipt, and remote cursor already addresses it by; a remove-then-add pair would
    // strand every one of them on a key no peer holds any more.
    public GraphOp Reparented(string nodeKey, Option<string> parent, uint index) =>
        new GraphOp.NodeMove(nodeKey, parent, index);

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

    // Two-phase apply: every node, pin, and connector mints DETACHED through the adapter and validates
    // against the settings policy BEFORE the first Drawing mutation. Containment is re-seated in the same
    // pass, off the index the staging fold already holds, so a parent that materialized after its child is
    // still bound — an in-order write would drop exactly the forward references a re-parented group carries.
    Fin<(Seq<INode> Nodes, Seq<IConnector> Wires)> Staged(Seq<GraphNodeRow> rows, Seq<GraphEdge> edges) =>
        from materialized in rows.TraverseM(row => Model.Node(Editor, row).Map(node => (row, Node: node))).As()
        let byKey = materialized.Fold(Map<string, INode>(), static (index, entry) => index.Add(entry.row.Key, entry.Node))
        let _seated = materialized.Iter(entry => entry.row.Parent.Iter(parent =>
            byKey.Find(parent).Iter(owner => entry.Node.Parent = owner)))
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
        from _bus in !Policy.RequireMatchingBusWidth || GraphModelAdapter.BusWidth(start) == GraphModelAdapter.BusWidth(end)
            ? Fin.Succ(unit) : Fin.Fail<Unit>(new CanvasFault.PolicyRejected($"bus width {edge.From} -> {edge.To}"))
        from _gate in Editor.CanConnectPin(start) && Editor.CanConnectPin(end)
            ? Fin.Succ(unit) : Fin.Fail<Unit>(new CanvasFault.PolicyRejected($"{edge.From} -> {edge.To}"))
        from wire in Model.Connect(start, end, edge.Wire)
        select wire;

    // Endpoint grammar (GraphAdmission owns it): `nodeKey` or `nodeKey/pinKey` — a pin-qualified endpoint
    // routes to its named pin so pin identity survives end-to-end; an unqualified endpoint routes by
    // direction (first Output on source, first Input on target).
    Option<IPin> Endpoint(Map<string, INode> byKey, GraphEndpoint endpoint, Option<PinDirection> direction) =>
        byKey.Find(endpoint.NodeKey).Bind(node => GraphModelAdapter.Pins(node).Find(pin =>
            direction.Match(Some: admitted => GraphModelAdapter.Direction(pin) == admitted, None: static () => true)
            && endpoint.PinKey.Match(
                Some: key => GraphModelAdapter.PinKey(pin) == key,
                None: () => true)));

    Option<PinDirection> RequiredDirection(PinDirection direction) =>
        Policy.RequireDirectionalConnections ? Some(direction) : Option<PinDirection>.None;
}
```

## [03]-[ADMISSION_GATE]

- Owner: `CanvasFault` — the typed canvas rail; `GraphAdmission` — the QuikGraph-backed connection-admission gate whose policy column IS the editor `IDrawingNodeSettings` row, never a parallel policy source.
- Entry: `Admit(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges)` rejects invalid node or pin identities, non-finite positions, extents, or rotations, non-positive bus widths, unknown containment parents, containment cycles, dangling endpoints, disabled connections, self connections, policy-disallowed duplicate edges, policy-disallowed pin fanout, and wiring cycles before the editor mutates; `Order` returns topological node keys from the same admitted graph value and is the `[09]` dependency projection's row order.
- Auto: every structural and policy clause is one row on an ordered guard seq folded to first refusal, so a new rule is a row rather than a deeper arm and each refusal names itself; the edge set then folds into a QuikGraph `AdjacencyGraph<string, SEdge<string>>` where `IsDirectedAcyclicGraph` is the cycle oracle and `TopologicalSort` reads off the SAME graph value through `Order`, so the notebook dependency projection and any solve-order consumer read one composed fold — a hand-rolled adjacency list or DFS beside QuikGraph is the deleted form. Containment folds through that identical oracle over a parent-edge graph rather than a walk of its own, so a group nested inside its own descendant refuses on the same mechanism a feedback wire does. The policy clauses read `EnableConnections`, `AllowSelfConnections`, `AllowDuplicateConnections`, and `EnableMultiplePinConnections` directly, so the gate and the interactive connector-drag answer from one settings row instead of the batch path admitting a wiring the drag rejects.
- Packages: QuikGraph (shared tier), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new admission rule is one gate clause folding on the same graph value; one `CanvasFault` case is one `detail` ordinal under the `AppUiFaultBand.Canvas` row (6330); zero new surface.
- Boundary: the gate guards STRUCTURE only — recompute scheduling, dirty propagation, and evaluation stay the AppHost `RecomputeGraph`'s (a second incremental-recompute owner is the deleted form); every fault derives through the `AppUiFaultBand.Canvas` registry row, and the band's span bounds the family, so a refusal class exceeding it collapses onto an existing case's detail rather than minting a code the reverse index cannot own.

```csharp signature
// --- [ERRORS] ---------------------------------------------------------------------------

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
    // A template key with no palette row is its OWN refusal rather than a model rejection, because the row
    // set is authored data a host froze and the repair is a palette edit, where a model rejection points at
    // the adapter; a drop and a paste that name a missing template therefore read identically.
    public sealed record TemplateUnknown(string Key)
        : CanvasFault($"graph/template: {Key} resolves no palette row", AppUiFaultBand.Canvas.Code(5));
    public sealed record CameraRejected(string Detail)
        : CanvasFault($"graph/camera: {Detail}", AppUiFaultBand.Canvas.Code(6));
    public sealed record DropRejected(string Detail)
        : CanvasFault($"graph/drop: {Detail}", AppUiFaultBand.Canvas.Code(7));
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public sealed record GraphAdmission(IDrawingNodeSettings Policy) {
    public Fin<Unit> Admit(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        Admitted(nodes, edges).Map(static _ => unit);

    private Fin<AdjacencyGraph<string, SEdge<string>>> Admitted(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        Refused(nodes, edges).Match(
            Some: Fin.Fail<AdjacencyGraph<string, SEdge<string>>>,
            None: () => Graph(nodes.Map(static node => node.Key), edges.Map(static edge => edge.Ends)) is { } graph && graph.IsDirectedAcyclicGraph()
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
            () => Containment(nodes),
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
            && double.IsFinite(node.Rotation)
            && node.Width >= 0d
            && node.Height >= 0d
            && node.Pins.Map(static pin => pin.Key).Distinct().Count == node.Pins.Count
            && node.Pins.ForAll(static pin => !string.IsNullOrWhiteSpace(pin.Key)
                && !string.IsNullOrWhiteSpace(pin.Name)
                && pin.BusWidth > 0))
            ? None
            : Some<Error>(new CanvasFault.ModelRejected("node keys, extents, rotations, pin keys, and bus widths must be admitted"));

    // Containment rides the SAME acyclicity oracle the wiring does, over a graph whose edges are child-to-
    // parent: a parent naming a key the batch never carried is an unknown endpoint, and a group nested inside
    // its own descendant is a cycle. A bespoke ancestor walk beside the oracle is the deleted form, because
    // the two would then disagree the first time one gained a rule the other did not.
    static Option<Error> Containment(Seq<GraphNodeRow> nodes) =>
        toHashSet(nodes.Map(static node => node.Key)) switch {
            var keys => nodes.Find(node => node.Parent.Exists(parent => !keys.Contains(parent)))
                .Map(static node => (Error)new CanvasFault.EndpointUnknown($"{node.Key} parent"))
                .IfNone(() => Graph(nodes.Map(static node => node.Key),
                        nodes.Choose(static node => node.Parent.Map(parent =>
                            new GraphLink(new GraphEndpoint(node.Key, None), new GraphEndpoint(parent, None)))))
                    .IsDirectedAcyclicGraph()
                    ? Option<Error>.None
                    : Some<Error>(new CanvasFault.CycleRejected(nodes.Count, nodes.Count(static node => node.Parent.IsSome)))),
        };

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
    // duplicates key on the edge's IDENTITY pair alone, so two renderings of one wiring never read as two
    // edges and parallel pins still stay distinct.
    static Option<GraphLink> Duplicate(Seq<GraphEdge> edges) =>
        Optional(edges.Map(static edge => edge.Ends).GroupBy(static link => link).FirstOrDefault(static group => group.Count() > 1))
            .Map(static group => group.Key);

    // Evaluation order off the SAME graph value the cycle oracle reads — one fold, two projections.
    public Fin<Seq<string>> Order(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges) =>
        Admitted(nodes, edges).Map(static graph => toSeq(graph.TopologicalSort()));

    static AdjacencyGraph<string, SEdge<string>> Graph(Seq<string> nodes, Seq<GraphLink> links) {
        AdjacencyGraph<string, SEdge<string>> graph = new(allowParallelEdges: true);
        nodes.Iter(node => graph.AddVertex(node));
        links.Iter(link => graph.AddEdge(new SEdge<string>(link.From.NodeKey, link.To.NodeKey)));
        return graph;
    }
}
```

## [04]-[PALETTE_INGRESS]

- Owner: `GraphTemplate` the palette row over the package's own `INodeTemplate`; `GraphPalette` the frozen template registry every `TemplateKey` resolves through; `GraphDropTarget` the package drop contract over the one transfer-admission rail; `GraphInk` the pressure-aware markup arm over the landed pen rows.
- Entry: `public static Fin<GraphPalette> Freeze(params ReadOnlySpan<GraphTemplate> rows)` — one freeze per mounted canvas, refusing a duplicate key; `public Fin<INode> Mint(DrawingNodeEditor editor, string templateKey)` — the one node mint; `public Seq<InkStroke> Strokes(Seq<PenSample> samples, InkPen pen)` on `GraphInk` — the pen fold to committed strokes.
- Auto: the palette is the seat every `GraphNodeRow.TemplateKey` resolves against, so a key that resolved nothing is now a typed refusal at admission instead of a node the mint silently skipped, and the same frozen row set feeds the package `Toolbox` through `TemplatesSource` — palette browsing, double-tap insert, and toolbox drag are the package's own behaviours over one registry rather than three surfaces with three rosters. Drop ingress implements `IDrawingDropTarget` so the package's own drop behaviours deliver, and files route through `DragPayload.Admit` before a node exists, so a canvas cannot mint a node for a path the transfer vocabulary refuses; each admitted file mints one node at the snapped drop point through the file template and dropped text mints one note node through the note template, both re-entering the same `Materialize` gate a typed edit does. Ink strokes are minted from `PointerTrack.Pen`, so pressure, tilt, twist, barrel, and eraser arrive as normalized `DeviceAxis` levels on the one channel grammar, and a stroke crossing the eraser channel routes to removal rather than paint.
- Receipt: an admitted drop and a committed stroke each seal the Edit-case `EvidenceReceipt` their `Materialize` commit already seals — the ingress arm mints no receipt of its own.
- Packages: NodeEditorAvalonia, Avalonia, LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new node kind is one `GraphTemplate` row; a new drop shape is one `DragPayload` case already carried by the transfer rail; a new pen behaviour is one `PenAxis` row at its own owner; zero new surface.
- Boundary: the palette holds the package's `INodeTemplate` VALUE rather than re-declaring its three members, because a row re-spelling title, template, and preview is a rename shell over a contract the `Toolbox` already binds; template instantiation is `DrawingNodeEditor.Clone<T>` over the row's own `Template`, so a minted node and a pasted node come off one round-trip and a hand-built node beside the clone is the deleted form. The package's own ink CAPTURE is refused — `IsInkMode` stays false so `InkLayer` never installs its pointer handlers — because that capture writes a CONSTANT unit pressure on every sample and reads only `GetCurrentPoint`, discarding the coalesced burst the stroke is drawn from; `InkLayer` remains the RENDERER, which draws `IDrawingNode.InkStrokes` unconditionally. Its render is one immutable pen at one constant width for a whole stroke and it reads `InkPoint.Pressure` nowhere, so a pressure-varying stroke lands as a RUN SET: the fold quantizes pressure onto a bounded level ladder and emits one `InkStroke` per level run, which renders as a varying-width stroke through the package's own renderer instead of forking it — a per-sample stroke emits one geometry per point and a single stroke ignores pressure entirely. Strokes enter `Drawing.InkStrokes` inside one `BeginUndoBatch`/`EndUndoBatch` pair so a whole gesture reverts as one op on the `Editing/history.md` rail, and the pen tool's pointer glyph is the `Theme/assets#CURSOR_ROWS` `CursorRow` the interaction root already inherits — this arm mints none.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------

// The palette row carries the package's own template VALUE beside the key the model addresses it by, so the
// `Toolbox` binds the roster verbatim and no member of `INodeTemplate` is re-spelled here.
public sealed record GraphTemplate(string Key, INodeTemplate Row) {
    public string Title => Row.Title ?? Key;
}

public sealed record GraphPalette(FrozenDictionary<string, GraphTemplate> Templates) {
    public static Fin<GraphPalette> Freeze(params ReadOnlySpan<GraphTemplate> rows) =>
        toSeq(rows.ToArray()) switch {
            var authored => authored.Map(static row => row.Key).Distinct().Count == authored.Count
                && authored.ForAll(static row => !string.IsNullOrWhiteSpace(row.Key) && row.Row.Template is not null)
                ? Fin.Succ(new GraphPalette(authored.ToFrozenDictionary(static row => row.Key, static row => row, StringComparer.Ordinal)))
                : Fin.Fail<GraphPalette>(new CanvasFault.ModelRejected("palette keys must be distinct and every row must carry a template")),
        };

    // The one node mint. A missing key refuses BY NAME rather than folding into a generic model rejection,
    // because the repair is a palette row and the caller — a paste, a drop, a remote apply — cannot tell the
    // two apart from a rejection that names neither.
    public Fin<INode> Mint(DrawingNodeEditor editor, string templateKey) =>
        Templates.TryGetValue(templateKey, out GraphTemplate? row)
            ? Optional(row.Row.Template).Bind(seed => Optional(editor.Clone(seed)))
                .ToFin((Error)new CanvasFault.ModelRejected($"template '{templateKey}' round-tripped to nothing"))
            : Fin.Fail<INode>(new CanvasFault.TemplateUnknown(templateKey));

    // The roster the package `Toolbox` binds through `TemplatesSource`, so browsing, double-tap insert, and
    // toolbox drag read the frozen registry the model resolves against. The frozen map's `Values` is an
    // ordinary enumerable rather than a carrier, so it re-enters through `toSeq` before a carrier fold reads
    // it — a carrier member spelled straight off the enumerable binds nothing.
    public IList<INodeTemplate> Host => [.. toSeq(Templates.Values).Map(static row => row.Row)];
}

// --- [BOUNDARIES] -----------------------------------------------------------------------

// The package's own drop contract over the one transfer-admission rail. Delivery is the package's drop
// behaviours; admission is `DragPayload`; minting is the palette; commit is the same gate a typed edit
// crosses — so no leg of a drop is a second implementation of a leg the corpus already owns.
public sealed record GraphDropTarget(
    GraphCanvas Canvas,
    GraphRouting Routing,
    Func<string, bool> Admitted,
    string FileTemplate,
    string NoteTemplate,
    Func<Seq<GraphNodeRow>, IO<Unit>> Commit,
    Func<double, double, string> Mint) : IDrawingDropTarget {
    public bool CanDropText(string text, Point point) => !string.IsNullOrWhiteSpace(text);

    public bool CanDropFiles(IReadOnlyList<IStorageItem> files, Point point) => Paths(files).Exists(Admitted);

    public void DropText(string text, Point point) => ignore(Rows(Seq(text), NoteTemplate, point));

    // Admission precedes the mint: `Admit` accumulates one refusal per unadmitted path, so a mixed drop
    // reports every rejected file at once instead of minting nodes up to the first refusal and leaving a
    // half-populated canvas the user then has to unpick.
    public void DropFiles(IReadOnlyList<IStorageItem> files, Point point) =>
        ignore(DragPayload.Admit(Paths(files), Admitted).ToFin()
            .MapFail(static error => (Error)new CanvasFault.DropRejected(error.Message))
            .Match(
                Succ: payload => payload is DragPayload.Files admitted
                    ? Rows(admitted.Paths, FileTemplate, point)
                    : unit,
                Fail: static _ => unit));

    // Each dropped subject becomes one node at a SNAPPED point walked along the lattice, so a multi-file drop
    // lands a readable column rather than a stack of coincident nodes one gesture then cannot separate.
    Unit Rows(Seq<string> subjects, string templateKey, Point point) =>
        subjects.Map((subject, ordinal) => Routing.Place(point.X, point.Y + (ordinal * Routing.GridCellHeight)) switch {
            var at => new GraphNodeRow(Mint(at.X, at.Y), templateKey, Title(subject), None,
                at.X, at.Y, Width: 0d, Height: 0d, Rotation: 0d, Locked: false, Visible: true, Seq<GraphPinRow>()),
        }).Strict() switch {
            var rows => ignore(Commit(rows).Run()),
        };

    static string Title(string subject) => Path.GetFileName(subject) is { Length: > 0 } name ? name : subject;

    static Seq<string> Paths(IReadOnlyList<IStorageItem> files) =>
        toSeq(files).Choose(static file => Optional(file.TryGetLocalPath()));
}

// --- [OPERATIONS] -----------------------------------------------------------------------

// The markup arm. Pressure arrives as a normalized level off the one axis grammar the device fabric mints,
// and the RUN LADDER is what makes it visible: the package renderer takes one width per stroke and reads the
// per-point pressure nowhere, so a gesture becomes a short sequence of constant-width runs whose widths track
// the quantized level. The ladder is bounded so a long stroke stays a handful of strokes rather than one per
// sample, and the quantization is the same value both the width scale and the run split read.
public static class GraphInk {
    public const int Levels = 8;
    public const double MinimumScale = 0.25d;

    public static Seq<InkStroke> Strokes(Seq<PenSample> samples, InkPen pen) =>
        samples.Map(sample => (sample, Level: Quantized(sample)))
            .Fold(Seq<(int Level, Seq<PenSample> Run)>(), static (runs, entry) => runs.Last
                .Filter(last => last.Level == entry.Level)
                .Match(
                    // The joining sample repeats into the next run so consecutive runs share an endpoint and
                    // the rendered path has no gap where the width changes.
                    Some: last => runs.Init.Add((last.Level, last.Run.Add(entry.sample))),
                    None: () => runs.Add((entry.Level, runs.Last.Map(last => last.Run.Last).Match(
                        Some: joint => Seq(joint, entry.sample),
                        None: () => Seq(entry.sample))))))
            .Filter(static run => run.Run.Count > 1)
            .Map(run => Stroke(run.Run, pen, run.Level));

    // The eraser channel is a TOOL routing read, never a stroke column: a gesture whose eraser level crosses
    // the half rung removes the strokes it touches instead of painting a stroke the renderer would then have
    // to erase by drawing the background over it.
    public static bool Erasing(Seq<PenSample> samples) =>
        samples.Exists(static sample => sample.Level(PenAxis.Eraser).Exists(static level => level.Value > 0.5d));

    static int Quantized(PenSample sample) =>
        sample.Level(PenAxis.Pressure).Match(
            Some: level => Math.Clamp((int)Math.Round(Math.Abs(level.Value) * (Levels - 1)), 0, Levels - 1),
            None: () => Levels - 1);

    static InkStroke Stroke(Seq<PenSample> run, InkPen pen, int level) =>
        new() {
            Color = pen.Color,
            Opacity = pen.Opacity,
            Thickness = pen.Thickness * (MinimumScale + ((1d - MinimumScale) * (level / (double)(Levels - 1)))),
            Name = pen.Name,
            Points = [.. run.Map(sample => new InkPoint(
                sample.Position.X, sample.Position.Y,
                sample.Level(PenAxis.Pressure).Map(static value => Math.Abs(value.Value)).IfNone(1d),
                sample.At.ToUnixTimeMilliseconds()))],
        };
}
```

## [05]-[CANVAS_VERBS]

- Owner: `GraphVerbs` — the selection-layout and camera command-table projection; `GraphNav` `[Union]` — the typed camera verb vocabulary; `GraphCamera` — the pinned viewport every camera verb dispatches through, holding the named-view roster it seats.
- Cases: `GraphNav` = Fit | FitTo | Step | ZoomBy | Travel | Locate | Bookmark | Recall | Forget | Reset, the direction-bearing rows carrying a signed unit step so one case spans a forward and a backward verb, and the three named-view rows carrying the key the camera's own bookmark roster seats.
- Entry: `public static Seq<CommandIntent> Rows(IDrawingNode drawing, GraphCamera camera, GraphFind find, Func<Seq<GraphNodeRow>> selected)` — the whole graph verb projection, one row per package alignment, distribution, and order case beside the lock, visibility, selection, find-walk, and camera rows; `public static CommandIntent Jump(GraphCamera camera)` and `public static Fin<ICommand> Jumped(CommandDeck deck)` — the overview strip's point-carrying verb and the arrow lifting a published `Point` onto it; `public IO<Fin<Unit>> Navigate(GraphNav verb)` — every camera move and named-view write on the one `Fin` rail.
- Auto: the layout rows GENERATE off the package's own bounded vocabularies — one row per `NodeAlignment`, `NodeDistribution`, and `NodeOrder` case — so a package enum gaining a case gains its verb, its chord slot, its palette entry, and its journal replay with no row edit, exactly as the viewport visibility verbs derive from their own fold. Each row raises the `IDrawingNode` operation the package already implements against `DrawingNodeEditor`, so the deck owns reachability and the package owns the operation; the selection-bearing rows gate on a non-empty selection through the availability input, and the reveal-all and deselect rows stay always available because their folds are total over an empty selection. Camera rows admit their own argument before dispatch and answer the same `Fin` rail every sibling entry answers, so a degenerate rectangle, a non-finite ratio, or an out-of-band direction refuses by name rather than folding into a silent no-op.
- Receipt: each row seals its own `CommandReceipt` through the deck's sink; the canvas seals its Edit-case `EvidenceReceipt` for the structural rows through the commit their execute reaches — a verb that only moved the camera seals no edit.
- Packages: NodeEditorAvalonia, PanAndZoom, ReactiveUI, LanguageExt.Core, Avalonia
- Growth: a new selection-layout verb is one package enum case its generator already covers; a new camera move is one `GraphNav` case; zero new surface.
- Boundary: the verb rows land on the one `Shell/commands#INTENT_TABLE` table under the `graph.` prefix and mint no second registry, so a chord, a palette hit, a toolbar button, and a replayed journal entry raise one row; the content-space point codec is `Editing/history.md`'s `ScrubPoint`, taken as a value rather than re-spelled, because a second encoder over the same keyed payload case drifts from the decoder the moment either gains a column — the same-folder history owner this page already binds for undo is the seat; the package's own bound `ICommand` twins stay unbound at the canvas, because binding both gives one gesture two paths and only one of them a receipt. `GraphCamera` holds the PINNED `ZoomBorder` — the alias posture at `[02]` is what makes the plain type name that control — and the whole camera capability rides its members: discrete rungs through `EnableDiscreteZoomLevels`/`DiscreteZoomLevels` with `GetNextDiscreteZoomLevel`/`GetPreviousDiscreteZoomLevel`, traversal through `EnableViewHistory`/`ViewHistorySize` with `NavigateBack`/`NavigateForward` and their `CanNavigateBack`/`CanNavigateForward` gates, fit through `Uniform`, focus through `ZoomToRectangle`, named views through `ExportState`/`ImportState` under this owner's own keyed roster, and the lattice through `ShowGrid`/`EnableSnapToGrid`/`GridSize` seeded from the same `GraphRouting` row the decorator and the position write read — a hand-built matrix, a second history ring, or a canvas-local zoom ladder is the deleted form. The control's `SaveView`/`RestoreView` family is NOT the named-view seat: it captures whatever view is live under a name and publishes no member that seats a saved view carrying a matrix, so its roster can be written and read yet never RESTORED, and a bookmark set persisted through it would vanish with the session that made it; the replacement therefore carries that family's deletion verb too, because a roster a user can only grow is the capability the swap would have silently dropped, and all three named-view rows land on the deck through the payload union's text case so a view name reaches them the way every other argument reaches a verb. The rectangle a selection-fit frames comes from the selected nodes' own extents, which the widened node row now carries, so the fit reads the model rather than measuring realized controls that a virtualized or scrolled canvas may not have realized.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

// The camera vocabulary. The two direction-bearing rows collapse four verbs into two cases carrying a signed
// unit step, because a forward and a backward traversal differ in one integer and never in their dispatch;
// the admission below is what keeps that integer bounded rather than an open parameter.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GraphNav {
    private GraphNav() { }
    public sealed record Fit : GraphNav;
    public sealed record FitTo(Rect Content) : GraphNav;
    public sealed record Step(int Direction) : GraphNav;
    public sealed record ZoomBy(double Ratio, Point At) : GraphNav;
    public sealed record Travel(int Direction) : GraphNav;
    public sealed record Locate(Point At) : GraphNav;
    // A named view is a camera verb, not a second surface: writing one, returning to one, and dropping one
    // are three rows on this vocabulary, so a bookmark crosses the same admission, the same `Fin` rail, and
    // the same dispatch every pan and zoom crosses. The roster this owner holds replaced a package family
    // that carried its own deletion, so the drop row is what keeps the replacement total rather than a
    // bookmark set a user can only ever grow.
    public sealed record Bookmark(string Name) : GraphNav;
    public sealed record Recall(string Name) : GraphNav;
    public sealed record Forget(string Name) : GraphNav;
    public sealed record Reset : GraphNav;
}

// --- [OPERATIONS] -----------------------------------------------------------------------

// The camera is a CLASS holding a live cell, never a record: the bookmark roster is one atomic cell and a
// record copy would share it by reference while forking every column beside it, so two cameras would drive
// one roster; value equality over a live viewport control answers nothing a caller can use either. The
// control is the only column, because the drawing every verb operates on arrives at the verb projection that
// raises it rather than being held twice.
public sealed class GraphCamera(ZoomBorder border) {
    public const string VerbPrefix = "graph.";

    // The named-view registry is THIS owner's rather than the control's, because the control's own roster
    // cannot be rehydrated: `SaveView` captures whatever view is live under a name and no member seats a
    // saved view carrying a matrix, so a bookmark set written into the control outlives nothing. Holding
    // `ZoomBorderState` values here makes the roster the same serializable shape `[06]`'s snapshot column
    // already round-trips, so a bookmark survives a session on the one persistence path the viewport takes.
    readonly Atom<Map<string, ZoomBorderState>> bookmarks = Atom(Map<string, ZoomBorderState>());

    public ZoomBorder Border { get; } = border;

    public Map<string, ZoomBorderState> Bookmarks => bookmarks.Value;

    public Unit Seat(Map<string, ZoomBorderState> roster) => ignore(bookmarks.Swap(_ => roster));

    // The composition edge: the routing lattice the position write and the grid decorator read is the SAME
    // lattice the viewport snaps and paints, and the ladder and history depths are policy values rather than
    // knobs each mount re-decides.
    public GraphCamera Seated(GraphRouting routing, Seq<double> ladder, int history) {
        Border.EnableViewHistory = history > 0;
        Border.ViewHistorySize = history;
        Border.EnableDiscreteZoomLevels = !ladder.IsEmpty;
        Border.DiscreteZoomLevels = ladder.IsEmpty ? null : [.. ladder];
        Border.ShowGrid = routing.Grid;
        Border.EnableSnapToGrid = routing.Snap;
        Border.GridSize = routing.GridCellWidth;
        return this;
    }

    // Every camera move admits its own argument before it dispatches and answers the SAME `Fin` rail every
    // sibling entry on this page answers; the capture converts a control throw into the typed refusal on that
    // rail, so one `Bind` chain carries a structural commit and the camera move that follows it.
    public IO<Fin<Unit>> Navigate(GraphNav verb) =>
        IO.lift(() => Admit(verb).Bind(admitted => Try.lift(() => ignore(admitted.Switch(
                state: this,
                fit: static (camera, _) => fun(() => camera.Border.Uniform(false))(),
                fitTo: static (camera, v) => fun(() => camera.Border.ZoomToRectangle(v.Content, null, true))(),
                step: static (camera, v) => fun(() => camera.Border.ZoomToLevel(
                    v.Direction > 0 ? camera.Border.GetNextDiscreteZoomLevel() : camera.Border.GetPreviousDiscreteZoomLevel(),
                    camera.Border.Bounds.Center.X, camera.Border.Bounds.Center.Y))(),
                zoomBy: static (camera, v) => fun(() => camera.Border.ZoomTo(v.Ratio, v.At.X, v.At.Y))(),
                travel: static (camera, v) => fun(() => {
                    if (v.Direction > 0) { camera.Border.NavigateForward(true); } else { camera.Border.NavigateBack(true); }
                })(),
                locate: static (camera, v) => fun(() => camera.Border.CenterOn(v.At, true))(),
                // A bookmark captures the control's OWN exported state, so a recall restores the matrix,
                // stretch, rotation, and clamp posture the view actually had rather than a zoom ratio and an
                // offset that lost every other axis. The capture runs AHEAD of the exchange because a CAS
                // body re-runs on every losing attempt: a control read inside it re-samples the live viewport
                // per retry and seats whichever sample the winning attempt happened to be holding.
                bookmark: static (camera, v) => fun(() => camera.Border.ExportState() switch {
                    var captured => ignore(camera.bookmarks.Swap(roster => roster.AddOrUpdate(v.Name, captured))),
                })(),
                recall: static (camera, v) => fun(() => camera.bookmarks.Value.Find(v.Name)
                    .Iter(state => camera.Border.ImportState(state, animate: true)))(),
                forget: static (camera, v) => fun(() => ignore(camera.bookmarks.Swap(
                    roster => roster.Remove(v.Name))))(),
                reset: static (camera, _) => fun(() => camera.Border.ResetMatrix())())))
            .Run()
            .MapFail(error => (Error)new CanvasFault.CameraRejected($"{admitted.Key}: {error.Message}"))));

    // One admission over the verb union, total by construction: a ratio must be finite and positive because
    // zero collapses the viewport scale, a focus rectangle must have extent, a point must be finite, a
    // direction is exactly one rung either way — a wider integer would silently walk the whole history ring —
    // a bookmark needs a name to be addressed by, and a recall or a drop needs one the roster actually holds,
    // so an unknown key refuses by name instead of leaving the viewport standing where it was and reading as
    // a verb that did nothing.
    Fin<GraphNav> Admit(GraphNav verb) => verb.Switch(
        state: (Row: verb, Roster: bookmarks.Value),
        fit: static (s, _) => Fin.Succ(s.Row),
        fitTo: static (s, v) => v.Content.Width > 0d && v.Content.Height > 0d ? Fin.Succ(s.Row) : Refused(s.Row),
        step: static (s, v) => v.Direction is 1 or -1 ? Fin.Succ(s.Row) : Refused(s.Row),
        zoomBy: static (s, v) => double.IsFinite(v.Ratio) && v.Ratio > 0d && Finite(v.At) ? Fin.Succ(s.Row) : Refused(s.Row),
        travel: static (s, v) => v.Direction is 1 or -1 ? Fin.Succ(s.Row) : Refused(s.Row),
        locate: static (s, v) => Finite(v.At) ? Fin.Succ(s.Row) : Refused(s.Row),
        bookmark: static (s, v) => !string.IsNullOrWhiteSpace(v.Name) ? Fin.Succ(s.Row) : Refused(s.Row),
        recall: static (s, v) => s.Roster.ContainsKey(v.Name) ? Fin.Succ(s.Row) : Refused(s.Row),
        forget: static (s, v) => s.Roster.ContainsKey(v.Name) ? Fin.Succ(s.Row) : Refused(s.Row),
        reset: static (s, _) => Fin.Succ(s.Row));

    // The selection's own extent, read off the MODEL columns rather than off realized controls, so a fit over
    // a selection scrolled out of view frames the same rectangle a fully realized canvas would.
    public Option<Rect> Extent(Seq<GraphNodeRow> selected) =>
        selected.Filter(static row => row.Visible)
            .Fold(Option<Rect>.None, static (held, row) => new Rect(row.X, row.Y, row.Width, row.Height) switch {
                var box => held.Match(Some: union => Some(union.Union(box)), None: () => Some(box)),
            });

    static bool Finite(Point at) => double.IsFinite(at.X) && double.IsFinite(at.Y);

    static Fin<GraphNav> Refused(GraphNav row) =>
        Fin.Fail<GraphNav>(new CanvasFault.CameraRejected($"{row.Key}: argument outside its admitted domain"));
}

// The whole graph verb projection. The layout rows GENERATE off the package's own case sets, so the table
// carries one row per alignment, distribution, and z-order case without an authored roster that would drift
// the first time the package widened one of them; the lock, visibility, selection, and camera rows follow the
// same shape, and every one of them raises an operation the package already implements.
public static class GraphVerbs {
    public static Seq<CommandIntent> Rows(
        IDrawingNode drawing, GraphCamera camera, GraphFind find, Func<Seq<GraphNodeRow>> selected) =>
        Enum.GetValues<NodeAlignment>().AsIterable().ToSeq().Map(value =>
            Selected($"align.{Key(value)}", () => drawing.AlignSelectedNodes(value)))
        + Enum.GetValues<NodeDistribution>().AsIterable().ToSeq().Map(value =>
            Selected($"distribute.{Key(value)}", () => drawing.DistributeSelectedNodes(value)))
        + Enum.GetValues<NodeOrder>().AsIterable().ToSeq().Map(value =>
            Selected($"order.{Key(value)}", () => drawing.OrderSelectedNodes(value)))
        + Seq(
            Selected("lock", drawing.LockSelection),
            Selected("unlock", drawing.UnlockSelection),
            Selected("hide", drawing.HideSelection),
            Selected("show", drawing.ShowSelection),
            Always("show-all", drawing.ShowAll),
            Always("select-all", drawing.SelectAllNodes),
            Always("deselect-all", drawing.DeselectAllNodes))
        + Seq(
            Camera("zoom-fit", camera, static _ => new GraphNav.Fit()),
            Camera("zoom-in", camera, static _ => new GraphNav.Step(1)),
            Camera("zoom-out", camera, static _ => new GraphNav.Step(-1)),
            Camera("zoom-reset", camera, static _ => new GraphNav.Reset()),
            Camera("navigate-back", camera, static _ => new GraphNav.Travel(-1)),
            Camera("navigate-forward", camera, static _ => new GraphNav.Travel(1)))
        + Seq(
            Named("view-save", camera, static name => new GraphNav.Bookmark(name)),
            Named("view-recall", camera, static name => new GraphNav.Recall(name)),
            Named("view-forget", camera, static name => new GraphNav.Forget(name)))
        + Seq(
            Framed("zoom-selection", camera, selected),
            Walked("find-next", find, 1),
            Walked("find-previous", find, -1));

    // The overview strip's jump verb: the strip publishes a CONTENT-SPACE point and the lift below lowers it
    // onto the existing keyed payload case, so the verb is an ordinary deck row a chord and a palette hit can
    // also raise.
    public static CommandIntent Jump(GraphCamera camera) =>
        Row("overview-jump", ["fields"], static _ => true, (payload, _) =>
            ScrubPoint.Read(payload).Match(
                Succ: at => camera.Navigate(new GraphNav.Locate(at)).Map(static _ => unit),
                Fail: static error => IO.fail<Unit>(error)));

    // The point-lifting arrow the strip binds. Handing the row's own materialized command to a control that
    // publishes a `Point` throws on the first drag, because the command's parameter type is the payload.
    public static Fin<ICommand> Jumped(CommandDeck deck) =>
        deck.Rows.TryGetValue($"{GraphCamera.VerbPrefix}overview-jump", out CommandIntent? row)
            ? Fin<ICommand>.Succ(ReactiveCommand.CreateFromTask<Point, CommandReceipt>(
                (at, token) => row.Run(ScrubPoint.Of(at), deck, token).RunAsync(EnvIO.New(token: token)).AsTask(),
                outputScheduler: deck.Scheduler))
            : Fin<ICommand>.Fail(new CanvasFault.CameraRejected("overview-jump is absent from the frozen deck"));

    static CommandIntent Selected(string verb, Action run) =>
        Row(verb, ["none"], static input => input.Selection.Count > 0, (_, _) => IO.lift(() => { run(); return unit; }));

    static CommandIntent Always(string verb, Action run) =>
        Row(verb, ["none"], static _ => true, (_, _) => IO.lift(() => { run(); return unit; }));

    static CommandIntent Camera(string verb, GraphCamera camera, Func<Unit, GraphNav> move) =>
        Row(verb, ["none"], static _ => true, (_, _) => camera.Navigate(move(unit)).Bind(static outcome => outcome.Match(
            Succ: static _ => IO.pure(unit),
            Fail: static error => IO.fail<Unit>(error))));

    // The named-view rows ride the closed payload union's TEXT case, so a bookmark, a recall, and a drop are
    // ordinary deck rows a chord, a palette hit, and a replayed journal entry each raise — a camera verb the
    // table never carried would be reachable from composition alone, which is the second registry this page's
    // one-table law deletes. The name crosses as the payload rather than as a row per view, because the
    // roster is live state and a frozen deck cannot carry a row per entry a user mints.
    static CommandIntent Named(string verb, GraphCamera camera, Func<string, GraphNav> move) =>
        Row(verb, ["text"], static _ => true, (payload, _) => payload is CommandPayload.Text named
            ? camera.Navigate(move(named.Value)).Bind(static outcome => outcome.Match(
                Succ: static _ => IO.pure(unit),
                Fail: static error => IO.fail<Unit>(error)))
            : IO.fail<Unit>(new CanvasFault.CameraRejected($"{verb} carries no view name")));

    // A selection fit with nothing selected frames the whole canvas rather than refusing, because the verb's
    // own availability already reads a non-empty selection and a race between the read and the fold must not
    // surface as a fault a user cannot act on.
    static CommandIntent Framed(string verb, GraphCamera camera, Func<Seq<GraphNodeRow>> selected) =>
        Row(verb, ["none"], static input => input.Selection.Count > 0, (_, _) =>
            camera.Navigate(camera.Extent(selected()).Match(
                Some: box => (GraphNav)new GraphNav.FitTo(box),
                None: static () => new GraphNav.Fit())).Bind(static outcome => outcome.Match(
                Succ: static _ => IO.pure(unit),
                Fail: static error => IO.fail<Unit>(error))));

    // The find walk steps the cursor and FRAMES in one row, because a walk that moved a cursor no camera
    // followed leaves a user reading an unchanged canvas and pressing the verb again. Availability reads the
    // live match set exactly as the inbox verbs read their live unread count.
    static CommandIntent Walked(string verb, GraphFind find, int direction) =>
        Row(verb, ["none"], _ => !find.Matches.IsEmpty, (_, _) =>
            find.Walk(direction).Match(
                Succ: _ => find.Frame().Bind(static outcome => outcome.Match(
                    Succ: static _ => IO.pure(unit),
                    Fail: static error => IO.fail<Unit>(error))),
                Fail: static error => IO.fail<Unit>(error)));

    static CommandIntent Row(
        string verb, string[] accepts, Func<CommandIntent.Availability, bool> when,
        Func<CommandPayload, CancellationToken, IO<Unit>> execute) =>
        new($"{GraphCamera.VerbPrefix}{verb}", CommandScope.Screen, [], accepts.ToFrozenSet(StringComparer.Ordinal),
            when, None, static (_, _) => true, FrozenSet<string>.Empty, None, execute);

    static string Key<TCase>(TCase value) where TCase : struct, Enum =>
        string.Concat(value.ToString().Select(static (glyph, index) =>
            char.IsUpper(glyph) && index > 0 ? $"-{char.ToLowerInvariant(glyph)}" : $"{char.ToLowerInvariant(glyph)}"));
}
```

## [06]-[NAVIGATION_SURFACE]

- Owner: `GraphOverview` — the `OverviewFrame` producer the minimap renders; `GraphFind` — the match set, its cursor, and its walk; `GraphView` — the viewport-state column and its round-trip; `GraphViewport` — that column's wire shape, the live state beside the named-view roster.
- Entry: `public IObservable<OverviewFrame> Frames(IObservable<Unit> ticks)` — the strip feed; `public Fin<int> Walk(int direction)` on `GraphFind` — the match cursor step; `public Option<string> Export()` and `public Fin<Unit> Import(Option<string> state)` on `GraphView` — the snapshot round-trip; `public IO<Fin<Unit>> Reveal(SearchOpen.GraphCanvas request, Func<string, Fin<Unit>> select)` on `GraphFind` — the far end of `Document/search#HIGHLIGHT_NAV`'s navigation request, selecting the addressed node and framing it.
- Auto: the minimap is the `Shell/controls` `Overview` intent over the `Shell/virtualization` `OverviewFrame` model, so the graph publishes a CONTENT-SPACE frame under a source key and computes no geometry: the content rectangle is the union of the node extents the widened row carries, the viewport rectangle is the pinned control's own `GetVisibleContentBounds()`, and the decoration lanes are the settled `OverviewLane` rows — selection marks the selected nodes, search marks the live find matches, error marks the nodes a refused apply or a rejected connection touched, and change marks the nodes a remote apply moved. The strip re-emits on the control's own `ZoomChanged`/`MatrixChanged` signals and on model change, so a pan and a structural edit each move the frame with no polling loop. Find composes the one `Document/search.md` plane: the graph projects its nodes as search candidates and consumes the ranked results, so a match set, its ordering, and its snippet come off the corpus fold every other surface reads, while the WALK and the HIGHLIGHT are this page's — the cursor steps the ranked set, the camera centres the current match, and the match keys feed the search lane the strip already paints. Viewport state round-trips through the control's own `ExportState`/`ImportState` over the `ZoomBorderState` value, so a restored canvas returns to the matrix, stretch, rotation, and clamp posture it was left at rather than to a reconstructed approximation, and the camera's bookmark roster rides that same payload so a named view outlives the session that captured it.
- Receipt: the screen-state snapshot carries the exported viewport column; the navigation arm seals none of its own, because a camera move is not an edit.
- Packages: PanAndZoom, System.Reactive, Avalonia, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: a new decoration lane is one `OverviewBand` off an existing `OverviewLane` row; a new navigation verb is one `GraphNav` case; zero new surface.
- Boundary: the graph publishes an `OverviewFrame` and renders nothing — a graph-local minimap control is the `Shell/virtualization#OVERVIEW_PROJECTION` rejected form, and the strip's drag publishes a content-space point back through the jump verb so the canvas moves its OWN camera and the strip never owns a viewport; the frame's axis is `OverviewAxis.Plane`, which fits uniformly and centres the remainder, because a graph summarized under an independently-scaled fit renders a distorted map of the thing it exists to make navigable. The alignment GUIDES the settings row has always carried finally land their consumer: `GuidesAdorner` mounts on the canvas adorner layer with the drag's live `GuideLine` set, gated on `GraphRouting.Guides` and matched within `GraphRouting.GuideTolerance`, so the tolerance the settings row declares is the tolerance a user sees rather than a second threshold the overlay picked. ZOOM HUD ownership is RULED to the package: the pinned control's own indicator (`ShowZoomIndicator`, `ZoomIndicatorPosition`, `ZoomIndicatorFormat`, `ZoomIndicatorAutoHideDuration`, `IsZoomIndicatorVisible`) reads the live matrix inside the viewport with no subscription and auto-hides on its own schedule, where a chrome chip mirroring the same number needs a `ZoomChanged` subscription, a second numeric formatter, and a placement that tracks a viewport it sits outside — so the chrome chip family stays for status text and the canvas zoom readout is the control's. The viewport column is the SCREEN-STATE snapshot's, not the co-edit document's, because a camera is per-viewer and committing it drags every peer's view along with one peer's pan.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------

// The minimap producer. Content and viewport are read at emission from the model and the control, so the
// strip re-projects on resize with no producer re-emission and no pixel ever leaves this fold.
public sealed record GraphOverview(
    GraphCamera Camera,
    Func<Seq<GraphNodeRow>> Nodes,
    Func<Set<string>> Selected,
    Func<Set<string>> Matched,
    Func<Set<string>> Refused,
    Func<Set<string>> Changed) {
    public const string SourceKey = "graph.overview";
    public const string IntentKey = "graph.minimap";

    public IObservable<OverviewFrame> Frames(IObservable<Unit> ticks) =>
        ticks.StartWith(unit).Select(_ => Frame()).DistinctUntilChanged().Replay(1).RefCount();

    // The strip's own intent, naming its frame producer and its jump verb by KEY: the intent is a
    // serializable shape that crosses the control wire, so it carries neither a stream nor a command.
    public static ControlIntent Intent(IntentBinding binding) =>
        new ControlIntent.Overview(IntentKey, OverviewAxis.Plane, SourceKey,
            $"{GraphCamera.VerbPrefix}overview-jump", binding);

    OverviewFrame Frame() =>
        Nodes().Filter(static row => row.Visible) switch {
            var visible => new OverviewFrame(
                Content(visible),
                Camera.Border.GetVisibleContentBounds(),
                Seq(Band(visible, OverviewLane.Selection, Selected()),
                    Band(visible, OverviewLane.Search, Matched()),
                    Band(visible, OverviewLane.Error, Refused()),
                    Band(visible, OverviewLane.Change, Changed()))),
        };

    // A degenerate content extent yields a unit rectangle rather than an empty one, so the downsample's own
    // ratio guard sees a measurable surface and an unpopulated canvas renders an empty strip rather than a
    // strip whose every mark projects onto one pixel.
    static Rect Content(Seq<GraphNodeRow> rows) =>
        rows.Fold(Option<Rect>.None, static (held, row) => new Rect(row.X, row.Y, row.Width, row.Height) switch {
            var box => held.Match(Some: union => Some(union.Union(box)), None: () => Some(box)),
        }).Filter(static box => box.Width > 0d && box.Height > 0d).IfNone(new Rect(0d, 0d, 1d, 1d));

    static OverviewBand Band(Seq<GraphNodeRow> rows, OverviewLane lane, Set<string> keys) =>
        new(lane, rows.Filter(row => keys.Contains(row.Key))
            .Map(static row => new Rect(row.X, row.Y, row.Width, row.Height)).Strict());
}

// --- [OPERATIONS] -----------------------------------------------------------------------

// Find over the one search plane. The graph contributes CANDIDATES and consumes RANKED RESULTS, so match
// semantics, case posture, whole-word posture, and the grammar all stay the corpus fold's; the cursor, the
// highlight set, and the camera walk are what this surface owns, because none of them is a search concern.
public sealed class GraphFind(GraphCamera camera, Func<Seq<GraphNodeRow>> nodes) {
    readonly Atom<(Seq<SearchResult> Hits, int Cursor)> state = Atom((Seq<SearchResult>(), -1));

    // Cursor custody makes this a class exactly as it makes the camera one: a record copy would fork the
    // match set's owner while sharing the one cell every walk exchanges through.
    GraphCamera Camera { get; } = camera;

    Func<Seq<GraphNodeRow>> Nodes { get; } = nodes;

    public Set<string> Matches => toSet(state.Value.Hits.Choose(static hit => hit.Member));

    // Every node is one candidate whose searchable text is its title beside its template key, so a find over
    // a kind name reaches both a node a user named and every node of that kind; the node key rides the
    // candidate's MEMBER slot, which is the identity a result attributes through and the walk resolves back.
    public Seq<SearchDocument> Candidates(string docKey) =>
        Nodes().Map(row => new SearchDocument(
            SearchSource.Node, docKey, Some(row.Key), row.Title, $"{row.Title} {row.TemplateKey}"));

    public Unit Seat(Seq<SearchResult> hits) =>
        ignore(state.Swap(_ => (hits, hits.IsEmpty ? -1 : 0)));

    // The walk WRAPS, because a find over a canvas has no beginning and no end a user can see, and a walk
    // that stopped at the last match would read as a broken verb rather than as an exhausted set.
    public Fin<int> Walk(int direction) =>
        direction is 1 or -1
            ? state.Swap(held => held.Hits.IsEmpty
                    ? held
                    : (held.Hits, ((held.Cursor + direction) % held.Hits.Count + held.Hits.Count) % held.Hits.Count))
                switch {
                    var moved => moved.Cursor >= 0
                        ? Fin.Succ(moved.Cursor)
                        : Fin.Fail<int>(new CanvasFault.ModelRejected("find carries no matches to walk")),
                }
            : Fin.Fail<int>(new CanvasFault.CameraRejected($"find walk direction {direction} is not one rung"));

    // The SEARCH far end. A result the plane resolved to this canvas arrives as its own typed request, and
    // landing it is the walk's two steps against one addressed key: SELECT the node, then FRAME it. Selection
    // rides a composition-bound arrow because membership is the canvas's own state and this owner holds only
    // the match set; the fit is the same `Locate` every walk takes, so a search landing and a find step move
    // the viewport through one dispatch. A key this canvas does not carry refuses by name rather than
    // centring on nothing — the plane's corpus and this canvas differ by exactly one remote delete.
    public IO<Fin<Unit>> Reveal(SearchOpen.GraphCanvas request, Func<string, Fin<Unit>> select) =>
        Nodes().Find(row => StringComparer.Ordinal.Equals(row.Key, request.NodeKey))
            .ToFin(new CanvasFault.ModelRejected($"search/node:{request.NodeKey}"))
            .Bind(row => select(row.Key).Map(_ => row))
            .Match(
                Succ: row => Camera.Navigate(new GraphNav.Locate(
                    new Point(row.X + (row.Width / 2d), row.Y + (row.Height / 2d)))),
                Fail: error => IO.pure(Fin.Fail<Unit>(error)));

    // Centring the current match is a camera verb, so the walk and an overview jump move the viewport through
    // one dispatch and a find that framed the canvas its own way is unrepresentable.
    public IO<Fin<Unit>> Frame() =>
        state.Value switch {
            var held when held.Cursor >= 0 => held.Hits[held.Cursor].Member
                .Bind(key => Nodes().Find(row => StringComparer.Ordinal.Equals(row.Key, key)))
                .Match(
                    Some: row => Camera.Navigate(new GraphNav.Locate(new Point(row.X + (row.Width / 2d), row.Y + (row.Height / 2d)))),
                    None: () => IO.pure(Fin.Succ(unit))),
            _ => IO.pure(Fin.Succ(unit)),
        };
}

// The snapshot's WIRE shape: the live state beside the named-view roster, both plain-setter values the shared
// JSON rail serializes with no converter. The roster is a BCL dictionary at this boundary alone — the camera
// holds it as a `Map` and the two conversions sit at the one edge that crosses the wire.
public sealed record GraphViewport(ZoomBorderState State, Dictionary<string, ZoomBorderState> Views);

// The viewport snapshot column. The control exports its own whole state — matrix, stretch, rotation, clamps,
// animation posture — so a restore returns the exact view rather than a matrix reconstructed from a scroll
// offset and a zoom ratio that never carried the rotation or the stretch mode at all. The bookmark roster
// rides the SAME payload, because a named view is exactly a viewport a user asked to keep.
public sealed record GraphView(GraphCamera Camera, JsonSerializerOptions Wire) {
    public Option<string> Export() =>
        Optional(Camera.Border.ExportState()).Map(state => JsonSerializer.Serialize(
            new GraphViewport(state, Camera.Bookmarks.AsIterable().ToDictionary(
                static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal)),
            Wire));

    // A refused decode leaves the live view UNTOUCHED and typed, because a canvas that reset itself on a
    // stale snapshot loses the position a user is working from to repair a persistence fault they cannot see.
    // The roster seats BEFORE the state import, so a recall raised on the same frame reads the restored set.
    public Fin<Unit> Import(Option<string> state) =>
        state.Filter(static payload => !string.IsNullOrWhiteSpace(payload)).Match(
            Some: payload => Try.lift(() => JsonSerializer.Deserialize<GraphViewport>(payload, Wire)).Run()
                .MapFail(static error => (Error)new CanvasFault.CameraRejected($"viewport state: {error.Message}"))
                .Bind(decoded => Optional(decoded)
                    .ToFin((Error)new CanvasFault.CameraRejected("viewport state decoded to nothing"))
                    .Map(admitted => {
                        Camera.Seat(toSeq(admitted.Views).Fold(Map<string, ZoomBorderState>(),
                            static (roster, entry) => roster.AddOrUpdate(entry.Key, entry.Value)));
                        Camera.Border.ImportState(admitted.State, animate: false);
                        return unit;
                    })),
            None: static () => Fin.Succ(unit));
}
```

## [07]-[CANVAS_SKIN]

- Owner: `GraphSkin` — the node-editor shipped-key correspondence, its host-key closure, and the icon rows the package templates consume.
- Entry: `public static readonly Seq<SemiSlot> Slots` — the role-to-shipped-key roster the token emission folds; `public static readonly Seq<(string Slot, AssetKey Key)> Icons` — the chrome glyph slots the editor templates resolve, each paired with the asset key it resolves through; `public static Fin<Seq<(string Slot, IImage Image)>> Glyphs(AssetRuntime runtime, ResolvedTheme resolved, int step, double scale, FlowDirection flow)` — the resolved carriers the swap re-materializes.
- Auto: every key the package's own theme defines per variant re-seeds from the resolved role ladder, so the pin, connector, crossing, rejection, node-handle, and rotation-readout families follow a variant swap through the one emission every other surface follows and this page writes no brush. The package resolves each key as a dynamic resource against the application resources before its own dictionaries compose, so a slot the emission mints WINS over the package's shipped value without replacing a control theme, and a key the package consumes yet defines nowhere — the two editor background keys and the four chrome icon keys — closes here rather than rendering blank chrome: the backgrounds land on the surface ladder and the icons resolve through the asset rail, riding the swap's `Rematerialize.TintedAsset` roster because a tinted glyph holds its pigment in its own bitmap and cannot observe a dictionary. The guide key the package binds to BOTH the guide overlay and the selected-connector overlay splits at this roster, so recolouring an alignment guide no longer recolours a selected wire.
- Packages: Avalonia, NodeEditorAvalonia, LanguageExt.Core
- Growth: a new package key is one slot row naming the role it re-seeds from; a new chrome glyph is one icon row at the asset catalog; zero new surface.
- Boundary: the roster mints VALUES onto the one emission and never a second dictionary — a graph-local `ResourceDictionary` merged beside the emitted one goes stale on the next re-seed, because the swap rewrites the emission's own partition alone; the slot cases are the token catalogue's shipped-key correspondence vocabulary, so a graph slot and a Semi slot are one axis and the roster proves against the package's own key set exactly as the Semi correspondence proves against the shipped theme's. The grid decorator's cell columns default to zero, so an unsized grid mounts, resolves, measures, and paints an empty surface — the sizing therefore lands on `GraphRouting`, which lifts a non-positive cell onto the resolved space step, and the decorator, the position snap, and the viewport's own grid all read that one row. A code-level colour property on a node, pin, or connector does not exist and `Connector` derives `Shape`, so stroke and thickness reach it through a theme setter alone; a paint written onto a control is the `Theme/tokens#CONTROL_THEMES` deleted form.

```csharp signature
// --- [TABLES] ---------------------------------------------------------------------------

// The node-editor correspondence. Every row names the resolved role and rung a shipped key re-seeds from, so
// a seed move carries the whole canvas with it and no row carries a colour of its own. The host-key rows at
// the tail are the closure: the package's templates resolve them and its dictionaries define them nowhere.
public static class GraphSkin {
    // The lattice fallback when the resolve carries no space rung — a canvas with an unsized grid paints
    // nothing at all, so the fallback exists to make that state unreachable rather than to be authored over.
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
        // The shipped theme binds ONE key to the alignment-guide overlay and the selected-connector overlay
        // alike, so the two are inseparable until the theme rows split them: the guide keeps the shipped key
        // and the selection stroke takes its own, which the selected-connector overlay's `Stroke` binds.
        new SemiSlot.Pigment(PaintRole.Focus, 0, "GuideLineBrush"),
        new SemiSlot.Pigment(PaintRole.Selection, 0, "ConnectorSelectedStrokeBrush"),
        // The two backgrounds the package's templates resolve and no shipped dictionary defines. An
        // unresolved dynamic resource renders that chrome blank rather than faulting, which is why the gap
        // reads as a design choice on first mount and as a bug on every mount after.
        new SemiSlot.Pigment(PaintRole.Workbench, 0, "EditorBackground"),
        new SemiSlot.Pigment(PaintRole.Well, 0, "DrawingBackground"));

    // The chrome glyphs the editor templates resolve by key. Each rides the asset rail's own resolve, so the
    // tint follows the theme through the re-materialization roster rather than through a second subscription.
    public static readonly Seq<(string Slot, AssetKey Key)> Icons = Seq(
        ("EditorCutIcon", AssetKeys.EditorCut),
        ("EditorCopyIcon", AssetKeys.EditorCopy),
        ("EditorPasteIcon", AssetKeys.EditorPaste),
        ("DeleteIcon", AssetKeys.EditorDelete));

    // The icon half of the emission: a resolved product is an IMAGE rather than a paint, so it lands beside
    // the slot values as a materialized carrier the swap rebuilds under `Rematerialize.TintedAsset`.
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

- Owner: `GraphCoEdit` — the composition record that mints bindings; `GraphBinding` — the ONE bidirectional projection between one ReactiveUI graph canvas and the `Collab/sync.md` `LoroTree` node register beside its `LoroMap` edge register, carrying BOTH directions and OWNING the re-entrancy latch.
- Entry: `public IO<Fin<GraphBinding>> Bind(CollabDoc doc, string docKey, GraphCanvas canvas)` — one binding per canvas, seating one scoped subscription per graph container onto the one sink; `public IO<Fin<Unit>> CommitLocal(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges, GraphOp op, string origin)` on the binding — the outbound direction re-admits the post-op topology before the intent rides `IntentLedger.Commit`; the binding's own subscription sink is the inbound direction.
- Auto: the graph structure maps onto `LoroTree` — a node is a tree node whose meta map carries the `GraphNodeRow` columns and whose position under its parent IS the containment column, an edge is a child row on the connection register carrying its `GraphWire` — and subscriber diffs discriminate `EventTriggerKind.Local`/`Import`/`Checkout` for ECHO SUPPRESSION: a local `CommitLocal` mutation arrives back as its own `Local` diff and is dropped, a mutation raised BY a remote apply commits nothing back, and a remote `Import` diff applies to the ReactiveUI graph model WITHOUT re-emitting — the feedback loop is the named deleted form. Suppression is a DROP on both sides and never a fault, because a fault on the routine convergence path surfaces correct behaviour to the presence UI as an error; the one diff-side fault is `CanvasFault.TriggerUnsupported`, raised for a trigger the bridge does not admit. A hierarchy move rides the sync-owned `GraphOp.NodeMove(NodeId, Parent, Index)` case onto the tree's identity-preserving `MovTo` and a canvas x/y position write commits as the sync-owned `GraphOp.NodeAt(NodeId, X, Y)` meta-column write minted through `GraphCanvas.Placed`, both riding the same commit leg through the graph arm and never a side channel.
- Receipt: durable truth rides `Collab/sync.md`'s typed edit-intent stream (a graph structural op is one row on the single edit-intent union); the live half rides the session-ephemeral Loro wire — this page persists nothing.
- Packages: LoroCs, ReactiveUI, LanguageExt.Core
- Growth: a new co-edited column is one meta-map key; a new structural verb is one `GraphOp` case landed at the sync owner; zero new surface.
- Boundary: the binding is the ONE projection per canvas and it covers both declared directions — a second inbound sink beside this one, a canvas-local `LoroTree` mutation beside `IntentApply.Apply`, a model-poll loop, or a per-node sync channel is the deleted form; remote-applied diffs re-run the `GraphAdmission` gate, and a cycle-closing edit surfaces as a typed conflict row for the presence UI. The re-entrancy latch and the conflict cell live on the BINDING rather than on the composition record, because `Bind` mints one binding per canvas while the record composes once: a latch on the record made two canvases over one document cross-suppress, so one canvas applying a remote diff silently dropped the other canvas's unrelated local commit and that peer's edit was never sent at all — a data loss the convergence protocol cannot detect and no receipt records. `Open`, `Subscribe`, `ReadNodes`, and `ReadEdges` are the composition adapters over the verified container values, so no unverified internal attach or read member enters the page; `Subscribe` is ADDRESSED and the binding seats one per graph container, because the node tree and the edge register are separate roots and a tree-only subscription made every remote edge add and remove a silent no-render until an unrelated node edit happened to fire the tree — a root-feed subscription beside them is equally the deleted form, since it re-reconciles this canvas on every unrelated document edit. That multi-seat mount carries CUSTODY across its roster rather than short-circuiting: a refusal on the second container releases the seat the first took, so a partial bind never leaves a live sink feeding a binding the caller never received. A diff arriving while an apply holds the latch is HANDED to the holder rather than dropped, because the reconcile reads whole container state and a diff landing after that read describes a topology the pass could not have seen.

```csharp signature
// --- [COMPOSITION] ----------------------------------------------------------------------

public sealed record GraphCoEdit(
    GraphAdmission Gate,
    // The one transaction rail. `IntentLedger.Commit` is an INSTANCE member at its own owner, so the ledger
    // rides here as a value rather than being reached statically — a static call would bind a second ledger
    // identity in any process that composes two documents, and the durable append would land on the wrong one.
    IntentLedger Ledger,
    Func<CollabDoc, Fin<(LoroTree Tree, LoroMap Edges)>> Open,
    // The container-SCOPED subscribe, the `Collab/sync#PRESENCE` `Scoped` shape taken as a column: a canvas
    // subscribes the levels it projects rather than the root feed, so a busy document costs this surface
    // nothing per unrelated edit and each addressed container still raises its own diff.
    Func<CollabDoc, CollabAddress, Subscriber, Fin<Subscription>> Subscribe,
    // The read columns are `Collab/sync#DURABLE_INTENT` `GraphRegister.ReadNodes`/`ReadEdges` verbatim: the
    // register owns its write arm and the projections that read those columns back, so a column written by
    // one and read by a locally-derived inverse could exist at one end alone. Binding them here rather than
    // re-deriving is what keeps the correspondence one declaration.
    Func<LoroTree, Fin<Seq<GraphNodeRow>>> ReadNodes,
    Func<LoroMap, Fin<Seq<GraphEdge>>> ReadEdges) {
    // One binding per canvas over BOTH graph containers, because the node tree and the edge register are
    // separate roots: a tree-only subscription left every remote edge add and remove raising no diff at all,
    // so a peer's rewiring stayed invisible until an unrelated node edit happened to fire the tree. The
    // subscriptions seat on the binding after it exists, so a sink can never observe a diff before the object
    // that suppresses it, and both land on the SAME sink because either container's diff reconciles the whole
    // canvas through one `Reset`.
    public IO<Fin<GraphBinding>> Bind(CollabDoc doc, string docKey, GraphCanvas canvas) =>
        IO.lift(() =>
            from containers in Open(doc)
            let binding = new GraphBinding(this, doc, docKey, canvas, containers.Tree, containers.Edges)
            from live in Seated(doc, binding)
            select binding.Seated(live));

    // Custody spans the roster: a traverse short-circuiting on the second container strands the subscription
    // the first already took — a live sink feeding a binding no caller ever receives, which no teardown
    // reaches, no receipt records, and no later bind repairs, because the document keeps delivering into it.
    // The fold therefore threads the seats it has taken and releases them on the refusal that ends it, so a
    // partial mount leaves the document exactly as it found it.
    Fin<Seq<Subscription>> Seated(CollabDoc doc, GraphBinding binding) =>
        Seq(CollabAddress.Of(CollabRoot.Graph), CollabAddress.Of(CollabRoot.Edges))
            .Fold(Fin.Succ(Seq<Subscription>()), (held, address) => held.Bind(taken =>
                Subscribe(doc, address, binding).Match(
                    Succ: seat => Fin.Succ(taken.Add(seat)),
                    Fail: error => taken.Iter(static seat => seat.Dispose()) switch {
                        _ => Fin.Fail<Seq<Subscription>>(error),
                    })));
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public sealed class GraphBinding(
    GraphCoEdit owner, CollabDoc doc, string docKey, GraphCanvas canvas, LoroTree tree, LoroMap edges)
    : Subscriber, IDisposable {
    // The latch is PER BINDING because `Bind` is per canvas: a latch shared across bindings makes one
    // canvas's remote apply suppress a sibling canvas's unrelated local commit, which drops that peer's edit
    // with no fault, no receipt, and no divergence the merge can later repair.
    int applying;
    // A diff that arrived while an apply held the latch. The reconcile reads WHOLE container state, so a
    // diff landing after this pass took its read describes a state the pass cannot have seen; dropping it
    // leaves the canvas on a superseded topology until an unrelated edit happens to fire a container, which
    // is a divergence no receipt records and the merge cannot repair. Recording the arrival and re-running
    // once for it collapses any burst into exactly one extra pass and costs nothing under serial delivery.
    int restated;
    Seq<Subscription> live = Seq<Subscription>();

    // The typed conflict row the presence UI observes: a remote apply the gate rejects lands HERE.
    public Atom<Option<Error>> Conflict { get; } = Atom(Option<Error>.None);

    public GraphBinding Seated(Seq<Subscription> subscriptions) { live = subscriptions; return this; }

    // Outbound: gate the POST-op topology, then ride the ONE transaction rail — durable first, live tree
    // apply through the same IntentApply.Apply arm replay uses; the resulting Local diff is echo-dropped.
    // Under the latch the commit is a NO-OP, not a refusal: a model mutation raised by a remote apply is
    // the expected, correct case the suppression law names a DROP, and returning a fault surfaced routine
    // convergence to the presence UI as an error. The genuine diff-side fault is the unsupported trigger,
    // which the sink routes to `Conflict` on its own arm.
    public IO<Fin<Unit>> CommitLocal(Seq<GraphNodeRow> nodes, Seq<GraphEdge> edges, GraphOp op, string origin) =>
        Volatile.Read(ref applying) == 1
            ? IO.pure(Fin.Succ(unit))
            : string.IsNullOrWhiteSpace(docKey) || string.IsNullOrWhiteSpace(origin)
                ? IO.pure(Fin.Fail<Unit>(new CanvasFault.ModelRejected("document key and origin are required")))
                : owner.Gate.Admit(nodes, edges).Match(
                    Succ: _ => owner.Ledger.Commit(doc, new EditIntent.GraphStructure(docKey, op), origin),
                    Fail: error => IO.pure(Fin.Fail<Unit>(error)));

    // `DiffEvent` is DISPOSABLE and its `Dispose` frees the trigger, the origin, the target, and every
    // container event, so the callback reads its projection inside this frame and releases at the end of it.
    // The trigger is read into a local BEFORE the dispatch for the same reason: the unsupported arm spells the
    // value into its fault text, and reading it off a disposed event would print freed memory.
    public void OnDiff(DiffEvent diff) {
        using (diff) {
            EventTriggerKind trigger = diff.TriggeredBy;
            ignore(trigger switch {
                EventTriggerKind.Local => unit,
                EventTriggerKind.Import or EventTriggerKind.Checkout => ApplyRemote(),
                _ => fun(() => Conflict.Swap(_ => Some<Error>(new CanvasFault.TriggerUnsupported($"{trigger}"))))(),
            });
        }
    }

    public void Dispose() { live.Iter(static subscription => subscription.Dispose()); live = Seq<Subscription>(); }

    // Remote apply is a state reconcile over the verified LoroTree/LoroMap read surface. The latch holder
    // DRAINS rather than returning after one pass: it clears the arrival mark before each reconcile and
    // re-checks it after releasing, so a diff seen at any point during a pass — including the window between
    // the last read and the release — still reaches the canvas, and a caller that found the latch held has
    // handed its arrival to the holder rather than dropped it.
    Unit ApplyRemote() {
        Interlocked.Exchange(ref restated, 1);
        if (Interlocked.CompareExchange(ref applying, 1, 0) == 1) { return unit; }
        do {
            while (Interlocked.Exchange(ref restated, 0) == 1) { Reconciled(); }
            Interlocked.Exchange(ref applying, 0);
        } while (Volatile.Read(ref restated) == 1 && Interlocked.CompareExchange(ref applying, 1, 0) == 0);
        return unit;
    }

    // Tree nodes + meta columns re-project to rows, the edge register re-projects to pairs, and the canvas
    // rebuilds through the ONE gate-checked Materialize fold — a remote edit that would close a cycle
    // surfaces as the typed CanvasFault conflict row for the presence UI, never a silent apply.
    Unit Reconciled() =>
        ignore((from rows in owner.ReadNodes(tree)
                from pairs in owner.ReadEdges(edges)
                from _ in canvas.Reset(rows, pairs)
                select unit)
            .Match(
                Succ: _ => Conflict.Swap(_ => None),
                Fail: error => Conflict.Swap(_ => Some(error))));
}
```

## [09]-[PROJECTIONS]

- Owner: `GraphProjection` — the read-projection fold family.
- Entry: `public static Fin<Seq<GraphNodeRow>> FromDependencies(GraphAdmission gate, RecomputeGraph.Graph graph, Map<string, (double X, double Y)> layout, string templateKey, Size extent, ConnectorStyle style)` — the notebook cell-dependency graph renders as a READ projection onto canvas rows in topological order; edits on this projection are disabled by SHAPE (a dependency edge derives from a node's own recorded inputs, never a hand-drawn connector).
- Auto: the fold reads the port's own node map — each node's `Hex` is the canvas key, its `Descriptor` the title, its `Inputs` the incoming edges — and composes `GraphAdmission.Order`, so the rendered row order and any solve-order consumer read ONE topological fold off the same acyclicity oracle the edit gate reads and `Order` finally has the reader its declaration names. A projected row is flat and unlocked at the caller's declared extent, because a dependency read has no containment of its own and a zero-extent row gives the minimap and the selection fit an empty rectangle to frame. Canvas snapshot export rides the capture capsule — the editor surface renders through the capture in-tree lane and encodes through `VisualCodec` as kind graph, so a canvas baseline joins the render-hash proof lanes; PNG/SVG/PDF export of the canvas composes `Document/export.md`'s destination union with the capture raster or the package `ExportRenderer` vector arm as the source.
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
        GraphAdmission gate, RecomputeGraph.Graph graph, Map<string, (double X, double Y)> layout,
        string templateKey, Size extent, ConnectorStyle style) =>
        Ordered(gate, toSeq(graph.Nodes.Values), layout, templateKey, extent, style);

    static Fin<Seq<GraphNodeRow>> Ordered(
        GraphAdmission gate, Seq<RecomputeNode> nodes, Map<string, (double X, double Y)> layout,
        string templateKey, Size extent, ConnectorStyle style) {
        Seq<GraphNodeRow> rows = nodes.Map(node => Row(node, layout, templateKey, extent)).Strict();
        Seq<GraphEdge> edges = nodes.Bind(node => node.Inputs.Map(input => new GraphEdge(
            new GraphEndpoint(input.Hex, Some(OutPin)),
            new GraphEndpoint(node.Hash.Hex, Some(InPin)),
            GraphWire.Read(style)))).Strict();
        return gate.Order(rows, edges).Map(order => Ranked(rows, order));
    }

    // A node the layout does not place lands at the origin rather than dropping: an unplaced dependency is
    // still structure the reader must see. Containment is absent by SHAPE — a recompute node has inputs, not
    // a parent — so the projection can never fabricate a hierarchy the port never recorded.
    static GraphNodeRow Row(
        RecomputeNode node, Map<string, (double X, double Y)> layout, string templateKey, Size extent) =>
        layout.Find(node.Hash.Hex).IfNone((X: 0d, Y: 0d)) switch {
            var at => new GraphNodeRow(node.Hash.Hex, templateKey, node.Descriptor, Parent: None,
                at.X, at.Y, extent.Width, extent.Height, Rotation: 0d, Locked: true, Visible: true, Seq(
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

## [10]-[RESEARCH]

(none)
