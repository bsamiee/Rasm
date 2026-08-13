# [TS_UI_API_ELKJS]

`elkjs` solves graph layout as data: an `ElkNode` tree with compound children, ports, and edges goes in, and the same tree comes back carrying absolute coordinates, node sizes, and `ElkEdgeSection` bend points routed orthogonally around every obstacle. Eleven algorithms and a string-keyed option table parameterize the solve, so hierarchy, force, tree, radial, stress, and packing layouts are option rows rather than sibling engines.

Reach it only through `elkjs/lib/elk-api` with a worker: the constructor demands `workerUrl` or `workerFactory`, and the worker path is the one that leaves the caller's graph untouched.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `elkjs`
- package: `elkjs` (EPL-2.0 OR GPL-3.0-or-later)
- module: CJS with hand-written `.d.ts` and no `exports` map, so every deep path resolves — `elkjs/lib/elk-api` is the admitted entry (the thin promise client) and `elkjs/lib/elk-worker.min.js` the solver payload the worker loads
- runtime: the client is DOM-free and isomorphic; the solver is GWT-compiled JavaScript running inside a `Worker`, reached through `postMessage` and structured clone
- rail: view canvas plane — the layout solver whose coordinate output crosses into `view/canvas` as data

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the graph model — one shape family carrying both the request and the solved answer

`ElkShape` seats `x`/`y`/`width`/`height` on every positioned element, so the caller writes sizes and the solver writes coordinates into the same fields. Nesting `children` makes a node compound and the solver lays out each level under its own algorithm.

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :-------------------------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `ElkGraphElement`                             | interface     | `id` `labels` `layoutOptions` — the per-element option carrier |
|  [02]   | `ElkShape extends ElkGraphElement`            | interface     | `x` `y` `width` `height` — sizes in, solved positions out      |
|  [03]   | `ElkNode extends ElkShape`                    | interface     | `id` `children` (compound nesting) `ports` `edges`             |
|  [04]   | `ElkPort extends ElkShape`                    | interface     | a fixed anchor the solver places on the node border            |
|  [05]   | `ElkLabel extends ElkShape`                   | interface     | `text` and its own solved box, placed by the label options     |
|  [06]   | `ElkExtendedEdge`                             | interface     | `sources`/`targets` id arrays (hyperedge-capable), `sections`  |
|  [07]   | `ElkEdgeSection`                              | interface     | `startPoint` `endPoint` `bendPoints` `incomingShape`           |
|  [08]   | `ElkPoint` (`{ x, y }`)                       | struct        | the coordinate atom every geometry field carries               |
|  [09]   | `LayoutOptions` (`{ [key: string]: string }`) | struct        | the option table — every key carries a string value            |

[PUBLIC_TYPE_SCOPE]: the call and construction contracts

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------------ | :------------ | :--------------------------------------------------------------- |
|  [01]   | `ELKConstructorArguments`       | struct        | `workerUrl` `workerFactory` `defaultLayoutOptions` `algorithms`  |
|  [02]   | `ElkLayoutArguments`            | struct        | `layoutOptions` `logging` `measureExecutionTime` per call        |
|  [03]   | `ELK`                           | interface     | the promise client the constructor yields                        |
|  [04]   | `ElkLayoutAlgorithmDescription` | interface     | `id` `name` `category` `knownOptions` `supportedFeatures`        |
|  [05]   | `ElkLayoutOptionDescription`    | interface     | `id` `group` `type` `targets` — the introspected option registry |
|  [06]   | `ElkLayoutCategoryDescription`  | interface     | `id` `name` `knownLayouters` — the algorithm grouping            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and the solve call

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                                                       |
| :-----: | :------------------------------------------------------ | :------- | :----------------------------------------------------------------- |
|  [01]   | `new ELK(ELKConstructorArguments)`                      | ctor     | spawns the solver worker and registers its algorithm set           |
|  [02]   | `elk.layout(graph, args?) -> Promise<ElkNode>`          | instance | solves one graph; `args.layoutOptions` overrides the ctor defaults |
|  [03]   | `elk.knownLayoutAlgorithms() -> Promise<Description[]>` | instance | the registered algorithms with their supported features            |
|  [04]   | `elk.knownLayoutOptions() -> Promise<Description[]>`    | instance | the option registry with group, value type, and targets            |
|  [05]   | `elk.knownLayoutCategories() -> Promise<Description[]>` | instance | the algorithm categories                                           |
|  [06]   | `elk.terminateWorker()`                                 | instance | tears the worker down; the client dies with the call               |

- `new ELK`: THROWS `Cannot construct an ELK without both 'workerUrl' and 'workerFactory'.` when neither field arrives — `workerFactory` alone satisfies it, `workerUrl` alone falls back to a bare `new Worker(url)`.
- `elk.layout`: `args.logging` and `args.measureExecutionTime` fold solver diagnostics into the answer; every option value crosses as a string, so a numeric spacing spells `'20'`.

[ENTRYPOINT_SCOPE]: the algorithms — the `elk.algorithm` option's value set, keyed by the id suffix

| [INDEX] | [SURFACE]                                | [SHAPE] | [CAPABILITY]                                                       |
| :-----: | :--------------------------------------- | :------ | :----------------------------------------------------------------- |
|  [01]   | `elk.algorithm: 'layered'`               | fold    | Sugiyama hierarchy with ports and orthogonal routing — the default |
|  [02]   | `elk.algorithm: 'mrtree'`                | fold    | tree layout for a strict parent-child hierarchy                    |
|  [03]   | `elk.algorithm: 'radial'`                | fold    | concentric rings around one root                                   |
|  [04]   | `elk.algorithm: 'force'`                 | fold    | force-directed placement for an unrooted relation graph            |
|  [05]   | `elk.algorithm: 'stress'`                | fold    | stress-majorization placement honoring desired edge lengths        |
|  [06]   | `elk.algorithm: 'rectpacking'` / `'box'` | fold    | area packing for unconnected children — dashboards and legends     |
|  [07]   | `elk.algorithm: 'sporeOverlap'`          | fold    | overlap removal over an existing placement                         |
|  [08]   | `elk.algorithm: 'sporeCompaction'`       | fold    | compaction over an existing placement                              |
|  [09]   | `elk.algorithm: 'fixed'` / `'random'`    | fold    | honors caller-supplied positions; scatters for a baseline          |

[ENTRYPOINT_SCOPE]: the option groups a canvas spends — every key spells `elk.<group>.<name>` and carries a string value

| [INDEX] | [SURFACE]                                                    | [SHAPE] | [CAPABILITY]                                                    |
| :-----: | :----------------------------------------------------------- | :------ | :-------------------------------------------------------------- |
|  [01]   | `elk.direction` (`RIGHT`/`DOWN`/`LEFT`/`UP`)                 | fold    | the flow axis every hierarchy solver reads                      |
|  [02]   | `elk.aspectRatio`                                            | fold    | the target bounding-box ratio                                   |
|  [03]   | `elk.spacing.*` / `elk.padding`                              | fold    | node, edge, label, and port spacing with container insets       |
|  [04]   | `elk.edgeRouting` (`ORTHOGONAL`/`POLYLINE`/`SPLINES`)        | fold    | selects the routing family `ElkEdgeSection` reports             |
|  [05]   | `elk.portConstraints` / `elk.portAlignment.*` / `elk.port.*` | fold    | how far the solver moves a port and how it distributes them     |
|  [06]   | `elk.hierarchyHandling` / `elk.partitioning.*`               | fold    | cross-hierarchy edges and forced layer partitions               |
|  [07]   | `elk.layered.*`                                              | fold    | crossing minimization, layering, node placement, cycle breaking |
|  [08]   | `elk.nodeSize.*` / `elk.nodeLabels.*` / `elk.edgeLabels.*`   | fold    | size derivation and label placement policy                      |
|  [09]   | `elk.separateConnectedComponents` / `elk.randomSeed`         | fold    | component splitting and deterministic reruns                    |
|  [10]   | `elk.interactive` / `elk.layered.considerModelOrder.*`       | fold    | biases the solve toward existing positions and input order      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Two execution paths ship, differing in purity rather than answer: an in-process solve MUTATES the caller's graph and returns the same object reference, writing `x`/`y` onto the nodes handed in and stamping a GWT hash field on each, while a worker solve structured-clones the request across `postMessage` and resolves a fresh tree, leaving the caller's graph pristine — the purity the estate's controlled-state fold requires.
- Each client wraps one worker in promises: every method posts a `{ cmd, id }` message and resolves the matching answer, so calls pipeline over a single worker instance and a client lives as a long-lived resource whose disposal is `terminateWorker()`.
- Options resolve in three tiers — `defaultLayoutOptions` at construction, `args.layoutOptions` per call, and per-element `layoutOptions` on any `ElkGraphElement` — each narrower tier overriding the wider one. Every value is a string at every key, so the option table is data a policy row emits rather than a typed argument object.
- Compound nesting is the structural axis: a node carrying `children` is laid out as its own graph under its own `elk.algorithm`, and `edges` seat at the lowest node containing both endpoints. `elk.hierarchyHandling: INCLUDE_CHILDREN` collapses the levels into one solve when edges cross hierarchy boundaries.
- Ports make edge attachment explicit: an `ElkPort` on a node's `ports` array receives a solved border position, and `elk.portConstraints` decides whether the solver may reorder or relocate it. Edges naming `sourcePort`/`targetPort` through their `sources`/`targets` ids route to the exact anchor.
- `ElkEdgeSection` answers geometry as data rather than markup: `bendPoints` carries the routed polyline between `startPoint` and `endPoint`, so the consumer owns every stroke, marker, and curve it draws through those points.
- `algorithms` at construction registers a subset in the worker, so a canvas that only ever solves hierarchy registers `['layered']` alone.

[STACKING]:
- `@xyflow/react` (`.api/xyflow-react.md`): the canvas seam — each node's measured `width`/`height` and the id graph build the `ElkNode` request, `elk.layout` answers absolute `x`/`y`, and the result lands as one `NodeReplaceChange[]` batch through `applyNodeChanges`, followed by `fitView` from `useReactFlow`; `ElkEdgeSection.bendPoints` rides an edge's `data` for a custom edge component to path through `BaseEdge`, so the canvas never learns the solver and the solver never learns React.
- `effect` (`libs/typescript/.api/effect.md`): `Effect.tryPromise` lifts `elk.layout` onto a typed failure, `Layer.scoped` over `Effect.acquireRelease` holds the client with `terminateWorker()` as its release, and `Effect.timeout` bounds a runaway solve — so a leaked worker and an unbounded wait both become rail faults.
- `@effect/platform-browser` (`libs/typescript/.api/effect-platform-browser.md`): the worker spawn is the seam — `BrowserWorker.layer(spawn)`'s spawn factory shape is exactly the `workerFactory` this constructor takes, so the estate mints its solver worker with the same `new Worker(new URL('elkjs/lib/elk-worker.min.js', import.meta.url), { type: 'module' })` bundler-resolved URL its other workers use. This worker speaks its own `{ cmd, id }` protocol, so it holds its own handle and never joins a serialized `Worker.makePool`.
- `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): layout is a derived atom over the graph cell — `Atom.fn` runs the solve as an `Effect`, `Atom.debounce` collapses rapid graph edits, and the `Result` carrier drives the canvas's pending state, so a solve in flight is readable rather than a hidden mutation.
- `@lume/kiwi` (`.api/lume-kiwi.md`): the sibling solver at the other grain — kiwi's `Solver` re-solves a `Constraint` tableau per frame under `suggestValue`/`updateVariables()` for panel and annotation placement, while this solves whole-graph topology in one batched worker call; a graph solve never enters the tableau and a per-frame constraint never crosses `postMessage`.

[LOCAL_ADMISSION]:
- Import `elkjs/lib/elk-api` and pass a `workerFactory`; the bare `elkjs` entry pulls the full 1.5MB CJS solver into the main chunk and takes the mutating in-process path.
- Hold one client per canvas as a scoped resource whose release calls `terminateWorker()`.
- Emit the option table from a policy row keyed by layout intent, and spell every value as a string.
- Read solved geometry back as data — positions into a change batch, bend points onto edge data — and let the consuming renderer own every stroke.
- Register the narrowest `algorithms` set the surface solves.

[RAIL_LAW]:
- Package: `elkjs`
- Owns: whole-graph layout as a promise over a worker — the `ElkNode` compound model with ports and labels, the eleven algorithm folds, the 235-row string option table across three override tiers, solved absolute coordinates, and `ElkEdgeSection` orthogonal routing geometry
- Accept: the `elkjs/lib/elk-api` entry with a `workerFactory`, a scoped client released through `terminateWorker()`, policy-row option tables, compound nesting for containers and ports for fixed anchors, solved geometry crossing into a canvas as a change batch and edge data, the solve wrapped as a debounced `Effect`
- Reject: the bare `elkjs` entry, the in-process path that mutates the caller's graph, a hand-rolled hierarchy or force pass where an algorithm row answers, a hand-drawn polyline where `bendPoints` answers, a per-frame solve during a drag, a client leaked past its scope, and the solver joining a shared serialized worker pool

