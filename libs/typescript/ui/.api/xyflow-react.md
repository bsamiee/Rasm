# [TS_UI_API_XYFLOW_REACT]

`@xyflow/react` owns the node-flow canvas: a pan-zoom viewport, handle-anchored connection machine, and a node/edge render tree the consumer drives as controlled state through pure change folds. Node and edge appearance is a component record keyed by type string, and the recognizer knobs surrender pan, wheel-zoom, and pinch individually, so the canvas hosts whatever geometry, gesture, and layout owners the estate already carries.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@xyflow/react`
- package: `@xyflow/react` (MIT)
- module: ESM (`dist/esm/index.js`) + UMD, `sideEffects` on CSS alone; `.` re-exports the whole `@xyflow/system` type algebra, and `./dist/base.css` / `./dist/style.css` are the only other subpaths
- runtime: React DOM in the browser — DOM measurement, `ResizeObserver`, and pointer capture; peer `react`/`react-dom` through the folder React spine
- depends: `@xyflow/system` (framework-free engine — pan/zoom, drag, handle, minimap, resizer kernels), `zustand` (per-provider store), `classcat`
- rail: view canvas plane — the node-flow engine `view/canvas`'s `Canvas.edge` adapter atom drives

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the graph value model — the shapes a consumer stores, renders, and folds

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :------------------------------------------------ | :------------ | :---------------------------------------------------------- |
|  [01]   | `Node<Data, Type>` / `Edge<Data, Type>`           | struct        | the stored graph element; `data` carries the domain payload |
|  [02]   | `NodeProps<NodeType>` / `EdgeProps<EdgeType>`     | struct        | the props a custom node or edge renderer receives           |
|  [03]   | `NodeTypes` / `EdgeTypes`                         | struct        | `Record<string, ComponentType>` — type string to renderer   |
|  [04]   | `InternalNode<NodeType>`                          | struct        | measured node with handle bounds and `internals.userNode`   |
|  [05]   | `BuiltInNode` / `BuiltInEdge`                     | union         | the shipped `input`/`output`/`default`/`group` variants     |
|  [06]   | `Connection` / `ConnectionState`                  | union         | the requested link and the live in-drag state               |
|  [07]   | `FinalConnectionState`                            | union         | `ConnectionInProgress` or `NoConnection` at the drop        |
|  [08]   | `Viewport` / `Transform` / `Rect` / `Box`         | struct        | camera `{ x, y, zoom }` and the bounds algebra              |
|  [09]   | `Position` / `ConnectionMode` / `MarkerType`      | enum          | handle side, loose or strict connect, arrow marker          |
|  [10]   | `SelectionMode` / `PanOnScrollMode` / `ColorMode` | enum          | partial or full lasso, scroll axis, light or dark           |

[PUBLIC_TYPE_SCOPE]: the change algebra — the closed variant set the controlled fold consumes

`NodeChange<NodeType>` unions six variants and `EdgeChange<EdgeType>` four; every variant carries `type` as its discriminant, so a reducer matches on `type` and never on field presence.

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------------ | :------------ | :------------------------------------------------------------- |
|  [01]   | `NodeDimensionChange` (`'dimensions'`)                  | struct        | measured `dimensions`, `resizing`, `setAttributes`             |
|  [02]   | `NodePositionChange` (`'position'`)                     | struct        | `position`, `positionAbsolute`, `dragging`                     |
|  [03]   | `NodeSelectionChange` (`'select'`)                      | struct        | `id` + `selected`; `EdgeSelectionChange` aliases it            |
|  [04]   | `NodeRemoveChange` (`'remove'`)                         | struct        | `id` alone; `EdgeRemoveChange` aliases it                      |
|  [05]   | `NodeAddChange` / `EdgeAddChange` (`'add'`)             | struct        | `item` + optional `index` for ordered insertion                |
|  [06]   | `NodeReplaceChange` / `EdgeReplaceChange` (`'replace'`) | struct        | `id` + whole `item` — the full-node swap arm                   |
|  [07]   | `NodeChange<NodeType>` / `EdgeChange<EdgeType>`         | union         | the six/four-variant closed families the change props emit     |
|  [08]   | `OnNodesChange<NodeType>` / `OnEdgesChange<EdgeType>`   | delegate      | `(changes: Change[]) => void` — the controlled-state callbacks |

[PUBLIC_TYPE_SCOPE]: the layout and geometry contracts a solver, resizer, or adapter fills

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :----------------------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `XYPosition` / `Dimensions` / `CoordinateExtent` | struct        | flow-space point, size, and the `[[x,y],[x,y]]` clamp box    |
|  [02]   | `NodeHandle` / `HandleType` / `HandleConnection` | struct        | handle geometry, `'source'`/`'target'`, and a live link      |
|  [03]   | `ProOptions` (`{ account?, hideAttribution }`)   | struct        | `hideAttribution` reads as a plain boolean, gated by nothing |
|  [04]   | `ResizeParams` / `ShouldResize` / `OnResize*`    | delegate      | the `NodeResizer` callback family with drag direction        |
|  [05]   | `FitViewOptions<NodeType>` / `FitBoundsOptions`  | struct        | padding, duration, easing, and node filters per camera move  |
|  [06]   | `AriaLabelConfig` / `ZIndexMode`                 | struct        | the canvas string table and the stacking policy              |
|  [07]   | `ReactFlowState<NodeType, EdgeType>`             | struct        | the internal store snapshot `useStore` selects over          |
|  [08]   | `ReactFlowInstance<NodeType, EdgeType>`          | interface     | the imperative handle `useReactFlow` and `onInit` return     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the canvas root, its store provider, and the in-viewport slots

| [INDEX] | [SURFACE]                                          | [SHAPE] | [CAPABILITY]                                                            |
| :-----: | :------------------------------------------------- | :------ | :---------------------------------------------------------------------- |
|  [01]   | `ReactFlow(props)`                                 | ctor    | the canvas — viewport, node/edge tree, connection and selection machine |
|  [02]   | `ReactFlowProvider({ children })`                  | ctor    | seals one store above the canvas so siblings reach the instance         |
|  [03]   | `Handle(HandleProps)`                              | ctor    | a connection anchor inside a custom node; `type` + `position` + `id`    |
|  [04]   | `Panel({ position, children })`                    | ctor    | screen-fixed overlay at one of eight `PanelPosition` corners            |
|  [05]   | `ViewportPortal({ children })`                     | ctor    | renders children in flow coordinates, panning and zooming with nodes    |
|  [06]   | `EdgeLabelRenderer({ children })`                  | ctor    | hoists edge labels into one DOM layer above the SVG edge paths          |
|  [07]   | `NodeToolbar({ nodeId, position, align, offset })` | ctor    | zoom-invariant toolbar anchored to a node                               |
|  [08]   | `EdgeToolbar({ edgeId, children })`                | ctor    | the same anchoring for an edge                                          |
|  [09]   | `NodeResizer(props)` / `NodeResizeControl(props)`  | ctor    | drag-resize with min/max, aspect ratio, and `shouldResize` gate         |

- [01]-[ROOT_STATE]: `nodes` `edges` `onNodesChange` `onEdgesChange` `onConnect` `defaultNodes` `defaultEdges` `defaultEdgeOptions` `nodeTypes` `edgeTypes`.
- [01]-[ROOT_VIEWPORT]: `viewport` `onViewportChange` `defaultViewport` `fitView` `fitViewOptions` `minZoom` `maxZoom` `translateExtent` `nodeExtent` `snapToGrid` `snapGrid`.
- [01]-[ROOT_RECOGNIZER]: `panOnDrag` `panOnScroll` `panOnScrollMode` `panOnScrollSpeed` `zoomOnScroll` `zoomOnPinch` `zoomOnDoubleClick` `preventScrolling` `selectionOnDrag` `selectionMode` `nodeDragThreshold` `connectionDragThreshold`.
- [01]-[ROOT_KEYS]: `deleteKeyCode` `selectionKeyCode` `multiSelectionKeyCode` `panActivationKeyCode` `zoomActivationKeyCode` `noDragClassName` `noWheelClassName` `noPanClassName`.
- [01]-[ROOT_POLICY]: `connectionMode` `connectionRadius` `connectOnClick` `isValidConnection` `onBeforeDelete` `nodesDraggable` `nodesConnectable` `nodesFocusable` `edgesFocusable` `edgesReconnectable` `elementsSelectable` `onlyRenderVisibleElements` `proOptions` `colorMode` `ariaLabelConfig` `onError`.
- [03]-[HANDLE]: `type: 'source' | 'target'` `position` `id` `isConnectable` `isConnectableStart` `isConnectableEnd` `onConnect` and `<div>` attributes.

[ENTRYPOINT_SCOPE]: the pure change folds and edge constructors — the controlled-state operators

| [INDEX] | [SURFACE]                                           | [SHAPE] | [CAPABILITY]                                                       |
| :-----: | :-------------------------------------------------- | :------ | :----------------------------------------------------------------- |
|  [01]   | `applyNodeChanges(NodeChange[], Node[]) -> Node[]`  | static  | folds the six variants into a fresh array, unchanged nodes intact  |
|  [02]   | `applyEdgeChanges(EdgeChange[], Edge[]) -> Edge[]`  | static  | the edge half of the same fold                                     |
|  [03]   | `addEdge(Connection \| Edge, Edge[]) -> Edge[]`     | static  | appends a connection as an edge, rejecting duplicates              |
|  [04]   | `reconnectEdge(Edge, Connection, Edge[]) -> Edge[]` | static  | re-points an existing edge onto a new source/target pair           |
|  [05]   | `useNodesState(Node[])` / `useEdgesState(Edge[])`   | static  | `[state, setState, onChange]` — the local-state convenience triple |
|  [06]   | `isNode(element)` / `isEdge(element)`               | static  | discriminates the two element families                             |

[ENTRYPOINT_SCOPE]: hooks — the instance handle, store selectors, and derived reads

| [INDEX] | [SURFACE]                                           | [SHAPE] | [CAPABILITY]                                                     |
| :-----: | :-------------------------------------------------- | :------ | :--------------------------------------------------------------- |
|  [01]   | `useReactFlow<NodeType, EdgeType>()`                | static  | the `ReactFlowInstance` — element CRUD, camera, coordinate map   |
|  [02]   | `useStore(selector, equalityFn?)` / `useStoreApi()` | static  | selects `ReactFlowState`; the hatch for state no hook exposes    |
|  [03]   | `useViewport()` / `useOnViewportChange(handlers)`   | static  | camera read and the `onStart`/`onChange`/`onEnd` phase callbacks |
|  [04]   | `useNodes()` / `useEdges()` / `useNodesData(ids)`   | static  | whole-collection reads and a `data`-only projection by id        |
|  [05]   | `useInternalNode(id)` / `useNodeId()`               | static  | measured node record; the ambient id inside a custom node        |
|  [06]   | `useNodeConnections(params)`                        | static  | live links into or out of one node                               |
|  [07]   | `useHandleConnections(params)`                      | static  | live links on one handle of one node                             |
|  [08]   | `useConnection(selector?)`                          | static  | the in-flight `ConnectionState` while a drag is live             |
|  [09]   | `useNodesInitialized(options?)`                     | static  | resolves once every node reports measured dimensions             |
|  [10]   | `useUpdateNodeInternals()`                          | static  | forces handle-bounds re-measure after a node changes its handles |
|  [11]   | `useOnSelectionChange({ onChange })`                | static  | selection deltas without re-rendering the canvas root            |
|  [12]   | `useKeyPress(keyCode, options?)`                    | static  | boolean key or chord state on the canvas target                  |
|  [13]   | `experimental_useOnNodesChangeMiddleware(fn)`       | static  | intercepts a node-change batch before the fold                   |
|  [14]   | `experimental_useOnEdgesChangeMiddleware(fn)`       | static  | the edge half of the same interception                           |

- `useReactFlow`: `getNodes` `setNodes` `addNodes` `getNode` `getInternalNode` `getEdges` `setEdges` `addEdges` `getEdge` `updateNode` `updateNodeData` `updateEdge` `updateEdgeData` `deleteElements` `toObject` `getNodesBounds` `getIntersectingNodes` `isNodeIntersecting` `getHandleConnections` `getNodeConnections` `fitView` `fitBounds` `zoomIn` `zoomOut` `zoomTo` `getZoom` `setViewport` `getViewport` `setCenter` `screenToFlowPosition` `flowToScreenPosition` `viewportInitialized`.

[ENTRYPOINT_SCOPE]: chrome components and the geometry functions re-exported from `@xyflow/system`

| [INDEX] | [SURFACE]                                                     | [SHAPE] | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------------------ | :------ | :------------------------------------------------------------- |
|  [01]   | `Background(props)` / `BackgroundVariant`                     | ctor    | dots, lines, or cross grid at `gap`/`size`/`offset`            |
|  [02]   | `Controls(props)` / `ControlButton(props)`                    | ctor    | zoom, fit-view, and lock buttons; `children` adds rows         |
|  [03]   | `MiniMap(props)` / `MiniMapNode(props)` / `PanOnScrollMode`   | ctor    | pannable, zoomable overview; `nodeComponent` swaps the mark    |
|  [04]   | `BaseEdge(props)` / `EdgeText(props)`                         | ctor    | the path, marker, and stripe primitive a custom edge builds on |
|  [05]   | `BezierEdge` / `SmoothStepEdge` / `StepEdge` / `StraightEdge` | ctor    | four of the five shipped edge renderers                        |
|  [06]   | `SimpleBezierEdge` / `ConnectionLineType`                     | ctor    | the curve-free bezier and the connection-preview shape enum    |
|  [07]   | `getBezierPath(params)` / `getSimpleBezierPath(params)`       | static  | `[path, labelX, labelY, offsetX, offsetY]`                     |
|  [08]   | `getSmoothStepPath(params)` / `getStraightPath(params)`       | static  | the orthogonal and direct paths on the same return tuple       |
|  [09]   | `getBezierEdgeCenter(params)` / `getEdgeCenter(params)`       | static  | label anchor points for a custom edge                          |
|  [10]   | `getNodesBounds(nodes, params?)` / `getViewportForBounds()`   | static  | the fit-view math as pure functions                            |
|  [11]   | `getIncomers(node, nodes, edges)` / `getOutgoers(...)`        | static  | adjacency reads over a stored graph                            |
|  [12]   | `getConnectedEdges(nodes, edges)` / `ResizeControlVariant`    | static  | the incident edge set; handle-versus-line resize marks         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Controlled state is the one regime: `nodes`/`edges` are consumer-owned arrays, the canvas emits `NodeChange[]`/`EdgeChange[]` through `onNodesChange`/`onEdgesChange`, and `applyNodeChanges`/`applyEdgeChanges` fold a batch into the next array — every interaction (drag, select, measure, resize, delete) reaches state through that one fold, so undo, persistence, and remote authority all sit on a single seam.
- `nodeLookup` reconciles by REFERENCE identity: a node object identical to its previous render keeps its measured internals untouched, while a fresh object for an unchanged node re-derives position, extent clamp, handle bounds, and z-order. Every adapter mapping domain records into `Node[]` memoizes per node id and returns the same reference until that node's own data changes, so a whole-array rebuild each render is the defect the fold exists to avoid.
- `viewport` and `onViewportChange` bind the camera to consumer state the same way, while `defaultViewport` and `fitView` seed an uncontrolled one; the two spellings stay exclusive per canvas.
- Every recognizer is an independent switch — `panOnDrag`, `panOnScroll`, `zoomOnScroll`, `zoomOnPinch`, `zoomOnDoubleClick`, `selectionOnDrag`, `preventScrolling` — so the canvas surrenders any gesture class to a sibling recognizer without surrendering the rest, and `noDragClassName`/`noWheelClassName`/`noPanClassName` carve the same exemption per element.
- `nodeTypes`/`edgeTypes` map `Node['type']` to a `ComponentType<NodeProps>`, so a new visual family lands as one row beside one component and the canvas root never branches on kind. Both records mount outside the render body — a record rebuilt per render remounts every node.
- `zustand` is an implementation detail sealed inside `ReactFlowProvider`: the store is minted per provider through `useState`, published on a private context, and reachable only through `useStore`/`useStoreApi`. No module singleton exists and no consumer imports `zustand` to reach it, so two canvases on one page hold two independent stores.
- `ProOptions.hideAttribution` is a plain boolean the attribution component reads — no key, no license check, no runtime gate.

[STACKING]:
- `@effect-atom/atom-react` (`.api/effect-atom-atom-react.md`): `view/canvas`'s `Canvas.edge` mints the adapter atom — the domain graph lives in an `Atom.make` cell, a derived `Atom.map` projects `Node[]`/`Edge[]` through an `Atom.family`-keyed per-id memo that preserves node reference identity, and `onNodesChange`/`onEdgesChange` dispatch through `useAtomSet` folding `applyNodeChanges` inside the writer, so canvas mutation is one atom write and undo, URL sync, and persistence ride the same rail every other surface uses.
- `elkjs` (`.api/elkjs.md`): the layout seam — `getNodesBounds` and each node's measured `width`/`height` build the `ElkNode` graph, `elk.layout` answers absolute `x`/`y` with `ElkEdgeSection.bendPoints`, and the result lands as one `NodeReplaceChange[]` batch through `applyNodeChanges`; bend points ride an edge's `data` for a custom edge renderer to path, so the canvas stays solver-blind and the solver stays React-blind.
- `@use-gesture/react` (`.api/use-gesture-react.md`): the recognizer trade — a canvas wanting momentum, rubberband, or a modeled camera sets `zoomOnScroll={false}` `zoomOnPinch={false}` `panOnDrag={false}` and binds `useWheel`/`usePinch`/`useDrag` on the canvas ref with `eventOptions: { passive: false }`, driving `viewport` through `onViewportChange`'s controlled cell; the two recognizers never bind the same gesture class, since the internal d3-zoom and a gesture hook both claiming the wheel double-apply every delta.
- `react-aria-components` (`.api/react-aria-components.md`): node bodies render as ordinary React trees, so a node's buttons, menus, and fields are RAC primitives carrying `noDragClassName` on any pressable that must not start a node drag; `ariaLabelConfig` supplies the canvas string table and `announce(Message, Assertiveness?)` from `@react-aria/live-announcer` (`.api/react-aria-live-announcer.md`) speaks the selection and connection outcomes the canvas emits.
- `class-variance-authority` (`.api/class-variance-authority.md`) + `lucide-react` (`.api/lucide-react.md`): `cva()` selectors folded through the one `cn()` rail style node shells, handles, and edge stripes off `selected`/`dragging`/`connectable` state; `ControlButton` and node glyphs render `lucide-react` icons, and `colorMode` binds the canvas to the token plane's light and dark axis.
- `motion` (`.api/motion.md`): `EdgeLabelRenderer` and `ViewportPortal` children are plain DOM, so `AnimatePresence` and `motion.div` animate labels and in-viewport overlays there; the node transform stays canvas-owned, since an animated transform on a node shell fights the drag kernel writing the same property.
- `@tanstack/react-virtual` (`.api/tanstack-react-virtual.md`): `onlyRenderVisibleElements` is the canvas's own culling arm and windows nodes by viewport intersection, so `useVirtualizer` stays on the node-palette, sidebar, and inspector panes the canvas hosts beside it.

[LOCAL_ADMISSION]:
- Drive every canvas from `nodes`/`edges` + the change folds; `defaultNodes`/`defaultEdges` and `useNodesState`/`useEdgesState` serve throwaway examples alone, since an uncontrolled canvas holds graph truth the estate cannot read.
- Memoize the domain-to-`Node[]` projection per node id so unchanged nodes keep their object reference across renders.
- Hoist `nodeTypes`/`edgeTypes` to module scope or a `useMemo` and key them by the same string union the domain graph carries.
- Wrap any tree reaching `useReactFlow` outside `ReactFlow` in `ReactFlowProvider`; reach the store through `useStore`/`useStoreApi` and never through a `zustand` import.
- Import `@xyflow/react/dist/base.css` for the structural layer and let the token plane own every visual rule; `style.css` ships opinionated defaults the token bridge re-decides.
- Switch off a recognizer at the canvas before binding a sibling gesture engine to that gesture class.

[RAIL_LAW]:
- Package: `@xyflow/react` (over `@xyflow/system`)
- Owns: the pan-zoom viewport, the handle-anchored connection machine, node measurement and drag, selection and lasso, the `NodeChange`/`EdgeChange` algebra and its pure folds, the `nodeTypes`/`edgeTypes` renderer records, the in-viewport slots (`Panel`, `ViewportPortal`, `EdgeLabelRenderer`, `NodeToolbar`, `EdgeToolbar`, `NodeResizer`), the chrome trio (`Background`, `Controls`, `MiniMap`), and the edge-path geometry functions
- Accept: controlled `nodes`/`edges` folded through `applyNodeChanges`/`applyEdgeChanges`, a controlled `viewport` through `onViewportChange`, reference-stable node projections from an atom-backed domain graph, hoisted type records, solver geometry landing as replace changes, per-class recognizer surrender to a sibling gesture engine, RAC primitives inside node bodies
- Reject: an uncontrolled canvas holding graph truth, a whole-array node rebuild per render, inline `nodeTypes`/`edgeTypes` objects, a `zustand` import reaching the interior store, a second recognizer bound to a gesture class the canvas still claims, a hand-rolled layout pass where `elkjs` answers, hand-written cubic path math where the `get*Path` family answers, and a node kind branched at the canvas root instead of a type-record row

