# [UI_CANVAS]

Canvas owns the interactive-canvas engine class — node/flow editing, graph layout, and the temporal band plane — distinct from `view/chart`'s marks and the viewer's spatial render. `@xyflow/react` is the one flow engine, driven as controlled state: ONE atom cell holds the revision-stamped graph, every interaction folds through `applyNodeChanges`/`applyEdgeChanges` inside the one writer, and the engine keeps its own recognizers under the third-party-physics law. Layout is worker data admitted at revision equality alone. Module: `ui/src/view/canvas.ts`.

Composed facts: the adapter-atom pattern is `view/table#STATE_FOLD`'s (`Grid.edge`): the cell owns and the engine binds through controlled props; persisted-grain keys ride `system/atom#STORE_ROOT`'s seal-versioned form; the proposal-admission law transcribes `viewer/scene`'s identity-keyed commit precedent onto async solves; hook facts ride `system/hook`'s open `Points`; faults derive through core `Fault.Class.family`; the document-embedding and anchor-space rows cross as VALUES under `view/content#BLOCK_ROSTER`'s and `view/presence#ANCHOR_PLANE`'s admission gates, never as lateral imports.

## [01]-[INDEX]

- [02]-[GRAPH_EDGE]: `Canvas.Graph` holds the revision-stamped cell — controlled-prop edge, persistence, recognizer law; `Canvas`.
- [03]-[NODE_PLANE]: closed node/edge kind vocabulary, render-arm records, selection and command seams, the exported space and block rows; `Canvas`.
- [04]-[LAYOUT_SOLVE]: `Canvas.solve` drives the scoped elk client — proposal-admission fold, solver rows, the solve hook fact; `Canvas`.
- [05]-[TIMELINE_PLANE]: temporal bands over `scaleUtc` × `scaleBand`, span rows, windowed lanes, arrow routing; `Canvas`.

## [02]-[GRAPH_EDGE]

[GRAPH_EDGE]:
- Owner: `Canvas.Graph` — the ONE cell: `revision` (a monotonic ordinal every write bumps), `nodes`/`edges` in the engine's own vocabulary, and the controlled `viewport`; `Canvas.seed` mints the rest state, `Canvas.apply` folds a change batch through the engine's pure folds INSIDE the writer, and `Canvas.useEdge(cell)` returns the controlled prop record (`nodes`, `edges`, `viewport`, `onNodesChange`, `onEdgesChange`, `onViewportChange`) a `ReactFlow` root spreads — so drag, select, measure, resize, connect, and delete all reach state through one seam and undo, persistence, and remote authority ride the same rail.
- Packages: `@xyflow/react` (`applyNodeChanges`, `applyEdgeChanges`, `addEdge`, the `NodeChange`/`EdgeChange` closed families, `Viewport`, `ReactFlowProvider`); `@effect-atom/atom-react` (`Atom`, `useAtomValue`, `useAtomSet`, `Atom.kvs`); `effect` (`Schema`, `Record`).
- Entry: one `Canvas.useEdge` per canvas instance under its own `ReactFlowProvider`; a second write path into `nodes`/`edges` beside it is the two-writer fork `view/table#STATE_FOLD` names.
- Law: the cell HOLDS the engine vocabulary — storing `Node[]`/`Edge[]` directly (domain payloads ride `node.data`) means `applyNodeChanges` preserves unchanged nodes' object identity by construction, which is the reference-identity contract the engine's `nodeLookup` reconciliation demands; a projection that rebuilds the whole array from a foreign domain collection re-derives every node's measured internals per render and is the named defect — such a projection memoizes per node id and returns prior references for unchanged rows, or the domain collection stays in `node.data` and never forks into a parallel model.
- Law: every write bumps `revision` inside the one writer — the ordinal is the admission coordinate `[04]`'s proposal fold reads, so no mutation can race a solve invisibly; the bump lives in `Canvas.apply`, never at a call site.
- Law: the engine keeps its OWN recognizers — `panOnDrag`, `zoomOnScroll`, `zoomOnPinch` stay on their defaults and no `Gesture.useCanvas` binds the canvas element; the folder ruling and `system/act#CLASS_DIVISION`'s third-party-physics row both name layering a second recognizer over the engine's d3-zoom as the double-bind defect. `viewport` + `onViewportChange` still mirror the camera into the cell, so a viewpoint restore or a control intent is one viewport write.
- Law: persistence is the seal-versioned parcel — `Canvas.Persisted` carries nodes, edges, and viewport with MUTABLE leaves (`Schema.mutable`, the `view/table` precedent: the engine's types are mutable arrays, and a readonly parcel forces the defensive copy where a restored graph silently forks); the key spells the folder's `rasm.ui.<domain>.<grain>.v<N>` seal-versioned form, so a parcel-shape change bumps the ordinal and an old key reads as absence onto the seeded default, never as a mis-decode. Measured `width`/`height` and `selected` never persist — measurement re-derives at mount and selection is session state.
- Law: `nodeTypes`/`edgeTypes` mount at module scope — a record rebuilt per render remounts every node (the catalogue trap); `[03]` owns the records.
- Boundary: registry lifecycle and write modality are `system/atom`'s; `defaultNodes`/`defaultEdges` and `useNodesState` never appear — an uncontrolled canvas holds graph truth the estate cannot read; the `@xyflow/react/dist/base.css` structural import is app stylesheet data and `style.css` never loads (the token plane owns every visual rule).
- Growth: a new interaction class is one change-variant arm the engine already emits through the same fold; a new persisted fact is one parcel field; a second canvas on one page is a second cell under a second provider — never a shared store.

```typescript signature
import type { Edge, Node, NodeChange, EdgeChange, Viewport } from "@xyflow/react"
import { addEdge, applyEdgeChanges, applyNodeChanges } from "@xyflow/react"
import { Atom } from "@effect-atom/atom-react"
import type { KeyValueStore } from "@effect/platform"
import { Match, Schema } from "effect"
import { Store } from "../system/atom.ts"

declare namespace Canvas {
  type Graph = {
    readonly revision: number
    readonly nodes: Array<Node>
    readonly edges: Array<Edge>
    readonly viewport: Viewport
  }
  type Change =
    | { readonly _tag: "nodes"; readonly changes: Array<NodeChange> }
    | { readonly _tag: "edges"; readonly changes: Array<EdgeChange> }
    | { readonly _tag: "viewport"; readonly viewport: Viewport }
    | { readonly _tag: "connect"; readonly edge: Edge }
  type Persisted = typeof _Persisted.Type
}

const _seed: Canvas.Graph = { revision: 0, nodes: [], edges: [], viewport: { x: 0, y: 0, zoom: 1 } }

// one writer: every arm folds through the engine's own pure fold and bumps the revision the solve lane keys on
const _apply = (graph: Canvas.Graph, change: Canvas.Change): Canvas.Graph => {
  const revision = graph.revision + 1
  return Match.value(change).pipe(
    Match.tagsExhaustive({
      nodes: ({ changes }) => ({ ...graph, revision, nodes: applyNodeChanges(changes, graph.nodes) }),
      edges: ({ changes }) => ({ ...graph, revision, edges: applyEdgeChanges(changes, graph.edges) }),
      viewport: ({ viewport }) => ({ ...graph, revision, viewport }),
      connect: ({ edge }) => ({ ...graph, revision, edges: addEdge(edge, graph.edges) }),
    }),
  )
}

// engine slice types are mutable arrays and records, so every leaf decodes mutable per the grid-parcel law; measured
// dimensions and selection never enter — measurement re-derives at mount and selection is session state
const _Persisted = Schema.Struct({
  nodes: Schema.mutable(Schema.Array(Schema.Struct({
    id: Schema.String,
    type: Schema.String,
    position: Schema.Struct({ x: Schema.Number, y: Schema.Number }),
    data: Schema.mutable(Schema.Record({ key: Schema.String, value: Schema.Unknown })),
    parentId: Schema.optionalWith(Schema.String, { as: "Option" }),
  }))),
  edges: Schema.mutable(Schema.Array(Schema.Struct({
    id: Schema.String,
    type: Schema.String,
    source: Schema.String,
    target: Schema.String,
    data: Schema.mutable(Schema.Record({ key: Schema.String, value: Schema.Unknown })),
  }))),
  viewport: Schema.Struct({ x: Schema.Number, y: Schema.Number, zoom: Schema.Number }),
})

// `Store.key` mints this key at the one store member: a parcel-shape change bumps the declared ordinal, and the
// stale key reads as absence onto the seeded default rather than mis-decoding yesterday's shape
const _GRAPH = Store.key({ domain: "canvas", grain: "graph", seal: { posture: "versioned", version: 1 } })

const _persisted = (
  runtime: Atom.AtomRuntime<KeyValueStore.KeyValueStore, never>,
  seed: Canvas.Persisted,
): Atom.Writable<Canvas.Persisted> => Atom.kvs({ runtime, key: _GRAPH, schema: _Persisted, defaultValue: () => seed })

declare namespace Canvas {
  type EdgeProps = {
    readonly nodes: Array<Node>
    readonly edges: Array<Edge>
    readonly viewport: Viewport
    readonly onNodesChange: (changes: Array<NodeChange>) => void
    readonly onEdgesChange: (changes: Array<EdgeChange>) => void
    readonly onViewportChange: (viewport: Viewport) => void
  }
}

// reads project the cell and writes fold through the one writer — the engine binds on this prop edge and nowhere
// else, so a `defaultNodes` seed or a second change handler beside it is the two-writer fork
declare const _useEdge: (cell: Atom.Writable<Canvas.Graph, Canvas.Change>) => Canvas.EdgeProps
```

## [03]-[NODE_PLANE]

[NODE_PLANE]:
- Owner: the kind vocabulary and its render records — `Canvas.kinds` is the closed node-kind tuple and `Canvas.edgeKinds` the edge sibling; `_NODE_ARMS`/`_EDGE_ARMS` are the annotated mapped records (`{ readonly [K in Kind]: ComponentType<NodeProps> }`) the engine's `nodeTypes`/`edgeTypes` props take, mounted at module scope so no node remounts per render; `Canvas.space` and `Canvas.block` are the two composition-value exports — the anchor space row `view/presence#ANCHOR_PLANE`'s registry admits and the document-embedding row `view/content#BLOCK_ROSTER`'s gate admits.
- Packages: `@xyflow/react` (`Handle`, `Position`, `NodeProps`, `EdgeProps`, `BaseEdge`, `getSmoothStepPath`, `useReactFlow` — `screenToFlowPosition`/`flowToScreenPosition` are the coordinate map the space row rides); `system/primitive` (`Primitive.styled` recipes — node shells style off `selected`/`dragging` state through the one `cn` rail); `system/token` (tone slots; a node never carries a raw color).
- Law: a node kind is one vocabulary member, one render arm, and one `data` schema — the record is annotated with its mapped contract per the dispatch doctrine, so a new kind is one row in each and the canvas root never branches on kind; a kind branched at the root or an inline `nodeTypes` object is the named defect.
- Law: selection lives IN the cell — the engine's `select` change variants fold through `Canvas.apply` like every other mutation, `Canvas.selected` is the derived read (`Atom.map` over the cell), and node commands register as `Overlay.Command` rows on the EXISTING `node` scope (`view/overlay#PALETTE`'s `_scopes` already carries it), so invocation stays select-then-run and no second selection plane appears.
- Law: interactive content inside a node carves the drag — every pressable in a node body carries `noDragClassName`, so a RAC button press never starts a node drag; node bodies are ordinary React trees composing `system/primitive` recipes.
- Law: the space row crosses as a VALUE — `Canvas.space(surface, cell, read, project)` closes over the cell's read and the instance's `flowToScreenPosition`, answering `locator` from its own codec, `resolve` from a node's live position, `carry` from node identity (a node id survives every mutation short of removal, so the carrier answers `Option.none` exactly when the graph no longer holds it), and `epoch` from the cell's own value stream (any revision bump re-resolves); the composing root hands the row to the presence overlay's registry, and no import edge exists between the two view pages.
- Law: the block row crosses as a VALUE the same way — `Canvas.block` names the embedded-graph document kind, its `attrs` schema IS `_Persisted` (one parcel serves storage and embedding, so a doc-embedded canvas and a persisted canvas are one shape), its render arm is a nodeView portal mounting this plane read-only, and its `steps` policy is decode-only — collab never authors graph mutations through prose steps; graph editing stays this owner's.
- Boundary: `Handle` placement, connection validity (`isValidConnection`), and `Position` sides are per-kind facts the render arm owns; the shipped `input`/`output`/`default`/`group` builtins never render — every kind is an estate row.
- Growth: a new node kind is one tuple member + one render arm + one `data` schema; a new command is one `Overlay.Command` row on the `node` scope; a new embedding fact is one `attrs` field on the one parcel.

```typescript signature
import type { ComponentType } from "react"
import type { NodeProps, EdgeProps, XYPosition } from "@xyflow/react"
import { Atom } from "@effect-atom/atom-react"
import { Option, type Stream } from "effect"

const _kinds = ["card", "group", "port"] as const
const _edgeKinds = ["flow", "reference"] as const

declare namespace Canvas {
  type Kind = (typeof _kinds)[number]
  type EdgeKind = (typeof _edgeKinds)[number]
  // this anchor-plane contract spells structurally field-for-field against view/presence#ANCHOR_PLANE's
  // Anchor.Space<L, C>, so the registry admits the value unchanged and no view-lateral import exists
  type Locator = typeof _Locator.Type
  type Space = {
    readonly kind: "canvas"
    readonly surface: string // the mounted instance: two canvases register two lanes sharing this one codec family
    readonly locator: typeof _Locator
    readonly resolve: (locator: Canvas.Locator) => Option.Option<{ readonly x: number; readonly y: number; readonly width: number; readonly height: number }>
    readonly carry: Option.Option<(locator: Canvas.Locator, change: Canvas.Graph) => Option.Option<Canvas.Locator>>
    readonly epoch: Stream.Stream<Canvas.Graph>
  }
}

const _Locator = Schema.Struct({ node: Schema.String })

// annotated with the mapped contract, never `satisfies`: the record IS the dispatch surface nodeTypes takes,
// and the annotation keeps a missing kind a compile break at this declaration
declare const _NODE_ARMS: { readonly [K in Canvas.Kind]: ComponentType<NodeProps> }
declare const _EDGE_ARMS: { readonly [K in Canvas.EdgeKind]: ComponentType<EdgeProps> }

const _space = (
  surface: string,
  cell: Atom.Writable<Canvas.Graph, Canvas.Change>,
  read: (locator: Canvas.Locator) => Option.Option<{ readonly position: XYPosition; readonly width: number; readonly height: number }>,
  project: (position: XYPosition) => { readonly x: number; readonly y: number },
): Canvas.Space => ({
  kind: "canvas",
  surface,
  locator: _Locator,
  resolve: (locator) =>
    Option.map(read(locator), (node) => {
      const screen = project(node.position)
      return { x: screen.x, y: screen.y, width: node.width, height: node.height }
    }),
  // node identity survives every mutation short of removal, so the carrier answers None exactly when the graph
  // no longer holds the id and the anchor parks instead of pointing at a ghost
  carry: Option.some((locator, graph) =>
    graph.nodes.some((node) => node.id === locator.node) ? Option.some(locator) : Option.none()),
  // any revision bump re-resolves this space's anchors; the cell's own value stream is the invalidation source
  epoch: Atom.toStream(cell),
})

declare namespace Canvas {
  // this document-embedding contract spells structurally field-for-field against view/content#BLOCK_ROSTER's
  // Node arm, so the roster gate admits the value unchanged: one parcel serves storage and embedding, so a
  // doc-embedded canvas and a persisted canvas are one shape
  type Block = {
    readonly _tag: "Node"
    readonly kind: "canvas"
    readonly attrs: typeof _Persisted
    readonly content: Option.Option<string>
    readonly group: Option.Option<string>
    readonly inline: false
    readonly atom: true
    readonly parse: ReadonlyArray<never>
    readonly render: { readonly _tag: "Live"; readonly view: "canvas" }
    readonly version: 1
    readonly ups: ReadonlyArray<never>
    readonly steps: "decoded"
  }
}

// steps stays decoded-only: collab never authors graph mutations through prose steps — graph editing is this
// owner's, and an embedded canvas inside a live document renders read-only from its attrs parcel; the Live view
// key names the nodeViews entry the editor host binds, and the portal constructor is that host's material
const _block: Canvas.Block = {
  _tag: "Node",
  kind: "canvas",
  attrs: _Persisted,
  content: Option.none(),
  group: Option.some("block"),
  inline: false,
  atom: true,
  parse: [],
  render: { _tag: "Live", view: "canvas" },
  version: 1,
  ups: [],
  steps: "decoded",
}
```

## [04]-[LAYOUT_SOLVE]

[LAYOUT_SOLVE]:
- Owner: the solve lane — `Canvas.solver` is the scoped elk client (acquired through `elkjs/lib/elk-api` with the app-supplied `workerFactory`, released by `terminateWorker`), `Canvas.solve(cell, intent)` runs one layout as a debounced Effect whose REQUEST captures the cell's revision, and `Canvas.admit` is the proposal fold: a `Canvas.Proposal` applies only when its revision equals the cell's current revision — equality lands the solved positions as a `nodes` change batch beside an `edges` replace batch carrying the routed bends onto each edge's `data`, inequality drops the proposal as SUPERSEDED work, counted on the solve fact and never applied, never a fault. `_SOLVERS` is the solver row table: the elk algorithms ride `elk.algorithm` option rows and the d3 `tree`/`force` rows answer the same `Canvas.Proposal` shape, so the canvas stays engine-blind and a new solver is one row.
- Packages: `elkjs` (`elkjs/lib/elk-api` — `ELK`, `ElkNode`, `ElkExtendedEdge`, `ElkEdgeSection`, `LayoutOptions`; the constructor THROWS without a worker, which makes the purity path structural — the in-process entry mutates the caller's graph and never loads); `d3` (`tree`, `stratify`, `forceSimulation` — the sibling solver rows); `effect` (`Effect`, `Layer`, `Duration`, `Option`); `@rasm/core` (`Fault`, `Convention`).
- Entry: `Canvas.solve` per layout intent; a solve mid-drag never fires — the debounce window is a `Canvas.Solving` policy row the composing panel supplies, never a module literal.
- Law: the request is built FROM the cell at capture — each node's `measured` box (the engine's post-mount write, never the optional style `width`/`height`) and the id graph become the `ElkNode` tree (compound `children` for `group` kinds, `ports` where a kind fixes anchors), the captured revision rides the request, and a node the engine has not measured yet refuses the whole request as `solve-refused` — a zero-size stand-in solves a layout no real node fits; the worker structured-clones the request, so the cell's graph is never the solver's operand.
- Law: solved geometry is DATA — positions land as a `position`-variant change batch through the one writer (which bumps revision, so a solve's own landing supersedes any sibling solve in flight — exactly the serialization the identity-keyed dome commit precedent buys), and `ElkEdgeSection.bendPoints` land in the SAME admission as an `edges` replace batch onto each routed edge's `data`, where the edge arm paths through `BaseEdge`; a proposal whose bends never landed routes arrows nowhere, the solver never learns React, and the canvas never learns the solver.
- Law: option tables are policy rows — `_SOLVERS` emits `elk.algorithm`, `elk.direction`, `elk.spacing.*`, and `elk.edgeRouting` as the string-valued rows the option contract takes; interactive re-solves bias toward held positions through `elk.interactive` and the `elk.layered.considerModelOrder.*` group (the `layered`-prefixed spelling — the root prefix does not exist).
- Law: supersession is MEASURED, never silent — every solve settles one fact on `rasm.ui.canvas.solve` carrying the bounded `applied | superseded` stage beside the solver row name, and the same stage value tags `_SOLVES` through `Effect.withMetric`, so solver health reads as data and a test asserts no-stale-application without touching the engine; a dropped proposal that vanishes unaccounted is the forged-zero's cousin.
- Law: the client is one scoped resource — `Layer.scoped` over `Effect.acquireRelease` with `terminateWorker()` as release; the elk worker speaks its own `{ cmd, id }` protocol, so it holds its own handle and never joins a serialized pool; `algorithms` registers only the rows this table names; `Effect.timeout` bounds a runaway solve into the fault family.
- Boundary: the `workerFactory` value is app composition (the same bundler-resolved `new Worker(new URL(...))` mint the estate's other workers use); which intent fires when is the consuming surface's; the timeline's solves ride this same admission fold per `[05]`.
- Growth: this fold IS the general shape for any worker-computed derivation beside a mutable source — a media decode product or an export octet stream admitted against the revision its request captured cites this cluster as a row, never a re-derived guard.

```typescript signature
import ELK from "elkjs/lib/elk-api"
import type { ElkNode, LayoutOptions } from "elkjs/lib/elk-api"
import type { NodeChange } from "@xyflow/react"
import { Convention, Fault } from "@rasm/core"
import { Array, Context, Duration, Effect, Either, Layer, Metric, Option, Schema } from "effect"
import { Hook } from "../system/hook.ts"

declare module "../system/hook.ts" {
  interface Points {
    readonly "rasm.ui.canvas.solve": { readonly modality: "observe"; readonly payload: Solve.Fact }
  }
}

declare namespace Solve {
  type Stage = "applied" | "superseded"
  type Fact = { readonly canvas: string; readonly solver: Canvas.Solver; readonly stage: Solve.Stage; readonly revision: number }
}

declare namespace Canvas {
  type Solver = keyof typeof _SOLVERS
  type Solving = { readonly window: Duration.Duration; readonly budget: Duration.Duration }
  type Proposal = {
    readonly revision: number
    readonly changes: Array<NodeChange>
    readonly routed: Array<{ readonly edge: string; readonly bends: Array<{ readonly x: number; readonly y: number }> }>
  }
}

// Three legs partition the layout plane and each reason renders its OWN subject. The engine name was a column
// whose only value was the engine, so it moved into the renderers; the node the graph could not measure is what
// actually varies, and it rides absence-shaped because the engine's own throw names no node at all.
const _family = Fault.Class.family(["solver-lost", "solve-refused", "solve-overrun"] as const, {
  "solver-lost": Fault.Class.row({
    class: "unavailable",
    leg: "worker",
    detail: Schema.Struct({ cause: Schema.String }),
    render: ({ cause }) => `elk worker did not start: ${cause}`,
  }),
  "solve-refused": Fault.Class.row({
    class: "invalid",
    leg: "graph",
    detail: Schema.Struct({
      node: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
      cause: Schema.String,
    }),
    render: ({ cause, node }) =>
      `elk refused the graph${Option.getOrElse(Option.map(node, (id) => ` at node ${id}`), () => "")}: ${cause}`,
  }),
  "solve-overrun": Fault.Class.row({
    class: "exhausted",
    leg: "budget",
    detail: Schema.Struct({ budget: Schema.DurationFromSelf }),
    render: ({ budget }) => `elk solve outlasted its ${Duration.toMillis(budget)}ms budget`,
  }),
})

declare namespace CanvasFault {
  type Case = typeof _family.payload.Type
  type Reason = (typeof _family.kinds)[number]
}

class CanvasFault extends Schema.TaggedError<CanvasFault>()("CanvasFault", {
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  get leg(): string {
    return _family.legOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

// Nodes measure INDEPENDENTLY, so the capture censuses every unmeasured one in a single refusal: a freshly mounted
// graph reports its whole gap on the first solve attempt rather than one node per round trip.
const CanvasCensus = _family.census("CanvasCensus")
type CanvasCensus = InstanceType<typeof CanvasCensus>

// each row emits the option table as data — every elk value is a string at every key, and the layered
// model-order group carries its `elk.layered.` prefix (the root spelling resolves to nothing)
const _SOLVERS = {
  layered: { "elk.algorithm": "layered", "elk.direction": "RIGHT", "elk.edgeRouting": "ORTHOGONAL" },
  tree: { "elk.algorithm": "mrtree" },
  force: { "elk.algorithm": "force" },
  packed: { "elk.algorithm": "rectpacking" },
} as const satisfies Record<string, LayoutOptions>

class Solver extends Context.Tag("ui/CanvasSolver")<Solver, ELK>() {}

// composition hands the worker mint down — the same bundler-resolved Worker(new URL(...)) form the estate's
// other workers use; the constructor's own refusal without one is what pins the purity path
const _client = (spawn: () => Worker): Layer.Layer<Solver, CanvasFault> =>
  Layer.scoped(
    Solver,
    Effect.acquireRelease(
      Effect.try({
        try: () => new ELK({ workerFactory: spawn, algorithms: Object.values(_SOLVERS).map((row) => row["elk.algorithm"]) }),
        catch: (cause) => new CanvasFault({ case: { reason: "solver-lost", cause: String(cause) } }),
      }),
      (elk) => Effect.sync(() => elk.terminateWorker()),
    ),
  )

const _SOLVES = Convention.mount(Convention.metric.canvasSolve)

// node.measured carries the measured box (the engine's post-mount write); a node the engine has not measured yet
// REFUSES the whole request — a zero-size stand-in solves a layout no real node fits — and ONE pass over the graph
// names every such node, where a short-circuiting walk surrendered the rest of the damage to the next attempt.
type _Measured = {
  readonly id: string
  readonly parent: string | undefined
  readonly width: number
  readonly height: number
}

const _measured = (graph: Canvas.Graph): Either.Either<ReadonlyArray<_Measured>, CanvasCensus> => {
  const [absent, present] = Array.partitionMap(graph.nodes, (node): Either.Either<_Measured, CanvasFault.Case> =>
    node.measured?.width !== undefined && node.measured.height !== undefined
      ? Either.right({ id: node.id, parent: node.parentId, width: node.measured.width, height: node.measured.height })
      : Either.left({ reason: "solve-refused", node: Option.some(node.id), cause: "engine has not measured it yet" }))
  return Array.isNonEmptyReadonlyArray(absent)
    ? Either.left(new CanvasCensus({ issues: absent }))
    : Either.right(present)
}

// capture builds the request FROM the cell: the proven box table and the id graph become the ElkNode tree —
// compound children RECURSE to the graph's own depth, because a group inside a group is one more row of the same
// parent fact and a two-level build drops its interior silently — and the captured revision rides the request
// as the admission coordinate. The tree walk is total by construction: the gate above already proved every box.
const _request = (
  graph: Canvas.Graph,
  solver: Canvas.Solver,
): Either.Either<{ readonly revision: number; readonly root: ElkNode }, CanvasCensus> =>
  Either.map(_measured(graph), (nodes) => {
    const under = (parent: string | undefined): Array<ElkNode> =>
      nodes.filter((node) => node.parent === parent).map(({ height, id, width }) => ({
        id,
        width,
        height,
        children: under(id),
      }))
    return {
      revision: graph.revision,
      root: {
        id: "root",
        layoutOptions: { ..._SOLVERS[solver] },
        children: under(undefined),
        edges: graph.edges.map((edge) => ({ id: edge.id, sources: [edge.source], targets: [edge.target] })),
      },
    }
  })

// one solve is one bounded Effect on the scoped client: the worker structured-clones the request, the budget
// bounds a runaway solve into the fault family, and the answer folds into the proposal the admission fold reads
const _solve = (
  solving: Canvas.Solving,
  request: { readonly revision: number; readonly root: ElkNode },
): Effect.Effect<Canvas.Proposal, CanvasFault, Solver> =>
  Effect.gen(function* () {
    const elk = yield* Solver
    const solved = yield* Effect.tryPromise({
      try: () => elk.layout(request.root),
      catch: (cause) => new CanvasFault({ case: { reason: "solve-refused", node: Option.none(), cause: String(cause) } }),
    }).pipe(
      Effect.timeoutFail({
        duration: solving.budget,
        onTimeout: () => new CanvasFault({ case: { reason: "solve-overrun", budget: solving.budget } }),
      }),
    )
    // positions harvest to the tree's own depth — elk child coordinates are parent-relative, exactly the frame the
    // engine's parentId nodes render in, so the walk carries no offset math
    const positions = (nodes: ReadonlyArray<ElkNode>): Array<NodeChange> =>
      nodes.flatMap((node) => [
        { type: "position" as const, id: node.id, position: { x: node.x ?? 0, y: node.y ?? 0 } },
        ...positions(node.children ?? []),
      ])
    return {
      revision: request.revision,
      changes: positions(solved.children ?? []),
      routed: (solved.edges ?? []).map((edge) => ({
        edge: edge.id,
        bends: (edge.sections ?? []).flatMap((section) => section.bendPoints ?? []),
      })),
    }
  })

// admission at revision equality alone: an equal proposal lands as TWO batches through the one writer — the node
// positions and the routed bends (an edges replace batch stamping each routed edge's data, so the edge arms path
// through BaseEdge off the same admission; a proposal whose bends never land would route arrows nowhere) — and an
// unequal one drops counted; superseded is normal operation
const _admit = (registry: Hook.Registry, canvas: string, solver: Canvas.Solver) =>
  (graph: Canvas.Graph, proposal: Canvas.Proposal): Effect.Effect<ReadonlyArray<Canvas.Change>> =>
    Effect.gen(function* () {
      const stage: Solve.Stage = proposal.revision === graph.revision ? "applied" : "superseded"
      yield* Effect.asVoid(Hook.publish(registry, "rasm.ui.canvas.solve", { canvas, solver, stage, revision: proposal.revision }))
      yield* Effect.asVoid(Effect.withMetric(Effect.succeed(1), Metric.tagged(_SOLVES, Convention.rasm.canvasSolveStage, stage)))
      if (stage !== "applied") return []
      const bends = new Map(proposal.routed.map((route) => [route.edge, route.bends]))
      return [
        { _tag: "nodes", changes: proposal.changes },
        {
          _tag: "edges",
          changes: graph.edges.filter((edge) => bends.has(edge.id)).map((edge) => ({
            type: "replace" as const,
            id: edge.id,
            item: { ...edge, data: { ...edge.data, bends: bends.get(edge.id) } },
          })),
        },
      ]
    })
```

## [05]-[TIMELINE_PLANE]

[TIMELINE_PLANE]:
- Owner: the temporal band plane — `Canvas.Lane` rows (the categorical bands) and `Canvas.Span` items (`key`, `lane`, `start`/`end` as `DateTime.Utc`), rendered as visx marks under two scales: `scaleUtc` on x because it INVERTS (drag and hit-test map a pointer back to an instant; `scaleBand` carries no `.invert`, which is why the axes never swap roles) and `scaleBand` on y for the lane ladder; `Canvas.useWindow` folds the lane set through `useVirtualizer` so a thousand-lane schedule renders its viewport slice; `_ARROWS` is the dependency-routing policy row — `direct` paths an elbow between span endpoints, `solved` routes congested arrow sets through the `[04]` lane as `bendPoints` data on the same proposal admission, so a re-route racing a span edit drops superseded like any other solve.
- Packages: `@visx/scale` (`scaleUtc`, `scaleBand` — d3 scale methods reach through for `invert` and ticks); `@visx/shape` + `@visx/group` (the marks); `@tanstack/react-virtual` (`useVirtualizer` — lane windowing); `system/intl` (`Format.date`/`Format.instant` — every rendered instant crosses the one epoch seam); `effect` (`DateTime`, `Order`).
- Law: a span's temporal edit is the same one-writer discipline — drag folds a pointer position through `x.invert` into a `DateTime.Utc` pair and writes the span row through the cell; the widget never holds a second copy of a span, and an uncommitted drag ride is presentation state at the motion-value grain, not atom state.
- Law: measurement is the chart panel's — the timeline takes a `Chart.Frame` parameter from `view/chart#DECLARED_SURFACE`'s one `useFrame` producer and divides it into scale ranges; a resize observer or a bare `width`/`height` pair here re-derives the one measurement owner.
- Law: lane order is data — a `rank` value on the lane row, sorted through one `Order` fold; a consumer's order never imports as a keyed comparator (the grid's registry-blind ruling).
- Law: span appearance rides the token roster — a span's status axis maps through a `_tone` table keying `Theme.Tone`, so a schedule, a gantt, and an availability board differ by lane rows, span rows, and tone mappings alone.
- Boundary: recurring-schedule expansion, dependency SEMANTICS (what an arrow means), and working-calendar math are consumer domain logic arriving as span rows; this plane owns coordinates, bands, windows, and routing alone. Zoom on the time axis is a domain-window write (two instants), never a second recognizer.
- Growth: a new band surface (audio regions, resource allocation, log swim-lanes) is lane rows + span rows + one tone mapping; a new arrow policy is one `_ARROWS` row; a denser schedule is the same window fold at a different `estimateSize`.

```typescript signature
import { scaleBand, scaleUtc } from "@visx/scale"
import { useVirtualizer } from "@tanstack/react-virtual"
import { DateTime, Order } from "effect"
import type { Theme } from "../system/token.ts"

declare namespace Canvas {
  type Lane = { readonly key: string; readonly label: string; readonly rank: number }
  type Span = {
    readonly key: string
    readonly lane: string
    readonly start: DateTime.Utc
    readonly end: DateTime.Utc
    readonly tone: Theme.Tone
  }
  type Window = { readonly from: DateTime.Utc; readonly until: DateTime.Utc }
}

const _byLane = Order.mapInput(Order.number, (lane: Canvas.Lane) => lane.rank)

// x must invert — a pointer maps back to an instant for drag and hit-test — so time is always x and the band
// ladder always y; the band scale carries no invert and never takes the temporal axis
const _scales = (window: Canvas.Window, lanes: ReadonlyArray<Canvas.Lane>, frame: { readonly width: number; readonly height: number }) => ({
  x: scaleUtc({ domain: [DateTime.toDate(window.from), DateTime.toDate(window.until)], range: [0, frame.width] }),
  y: scaleBand({ domain: lanes.map((lane) => lane.key), range: [0, frame.height], padding: 0.2 }),
})

// arrow routing is a policy row: direct elbows serve sparse graphs, and a congested set routes through the
// [04] solve lane as bendPoints data under the same revision admission — a re-route racing a span edit drops superseded
const _ARROWS = { direct: { solved: false }, solved: { solved: true } } as const

// lane windowing: the virtualizer owns which bands mount, estimateSize reads the band scale's step, and a
// thousand-lane schedule renders its viewport slice; span hit-testing stays inside the mounted window — the
// `use` prefix is load-bearing: the member composes a hook, so rules-of-hooks and the compiler key on it
const _useWindow = (lanes: ReadonlyArray<Canvas.Lane>, scroll: () => HTMLElement | null, step: number) =>
  useVirtualizer({ count: lanes.length, getScrollElement: scroll, estimateSize: () => step, overscan: 4 })

declare namespace Canvas {
  type Shape = {
    readonly Solver: typeof Solver
    readonly kinds: typeof _kinds
    readonly edgeKinds: typeof _edgeKinds
    readonly solvers: typeof _SOLVERS
    readonly arrows: typeof _ARROWS
    readonly seed: typeof _seed
    readonly apply: typeof _apply
    readonly useEdge: typeof _useEdge
    readonly persisted: typeof _persisted
    readonly space: typeof _space
    readonly block: typeof _block
    readonly client: typeof _client
    readonly request: typeof _request
    readonly solve: typeof _solve
    readonly admit: typeof _admit
    readonly scales: typeof _scales
    readonly byLane: typeof _byLane
    readonly useWindow: typeof _useWindow
  }
}

const Canvas: Canvas.Shape = {
  Solver,
  kinds: _kinds,
  edgeKinds: _edgeKinds,
  solvers: _SOLVERS,
  arrows: _ARROWS,
  seed: _seed,
  apply: _apply,
  useEdge: _useEdge,
  persisted: _persisted,
  space: _space,
  block: _block,
  client: _client,
  request: _request,
  solve: _solve,
  admit: _admit,
  scales: _scales,
  byLane: _byLane,
  useWindow: _useWindow,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Canvas, CanvasCensus, CanvasFault }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
