# [RASM_GRASSHOPPER_API_GH2_STANDARD_COMPONENTS]

`Grasshopper2.Components.Standard` is the shipped GH2 composite and iterative document-object family — the objects that hold or drive other objects rather than compute a single result. `Cluster` owns a nested inner `Document` bounded by `Shout`/`Listen` special-object pins and mapped to outer pins through `EnsureMaps`; `Chain` links an ordered run of document objects and migrates their connections; the `Loop`/`LoopContinuation`/`LoopingIteration`/`LoopRepeats` family drives repeated inner-document solution with break and repeat control; and `Accumulation` is the branch-accumulation mode a loop output carries. `GH1InteropComponent` is the one-way import boundary that wraps a `Grasshopper2.Interop.IGH_Component` and carries its id, name, icon, and source XML. Each object extends the `Grasshopper2.Components` `Component` base and reaches its inner graph through the `Grasshopper2.Doc` document surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grasshopper2` (Rhino 9 WIP Grasshopper2 SDK)
- assembly: `Grasshopper2.dll` (installed `Grasshopper2Plugin.rhp` managed plug-in; in-process)
- namespace: `Grasshopper2.Components.Standard`, `Grasshopper2.Components`, `Grasshopper2.Doc`, `Grasshopper2.Parameters.Special`, `Grasshopper2.Undo`, `Grasshopper2.Interop`
- base: `Grasshopper2.Components.Component` (`api-gh2-components`) — every standard object is a component
- inner: `Cluster.InnerDocument` and `Loop.Document` are nested `Grasshopper2.Doc.Document`s (`api-gh2-document`)
- rail: composite and iterative document objects

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: composite objects
- rail: composite and iterative document objects
- note: `Cluster` and `Chain` group other document objects; `Cluster` nests a full inner `Document` mapped through `Shout`/`Listen` boundary pins, `Chain` links an ordered object run and re-routes their wires.

| [INDEX] | [SYMBOL]          | [KIND]     | [CAPABILITY]                                                                  |
| :-----: | :---------------- | :--------- | :---------------------------------------------------------------------------- |
|  [01]   | `Cluster`         | composite  | nested inner `Document`, `Shout`/`Listen` maps, entanglement, expiry          |
|  [02]   | `ClusterBoundary` | smart enum | `None` / `Input` / `Output` / `Index` — a boundary-pin role                   |
|  [03]   | `Chain`           | composite  | ordered `Links`, `LeadingObject`/`TrailingObject`, connection migration       |
|  [04]   | `Accumulation`    | smart enum | `None` / `List` / `Layered` / `Last` — a loop-output branch-accumulation mode |

[PUBLIC_TYPE_SCOPE]: iterative loop family
- rail: composite and iterative document objects
- note: `Loop` drives repeated inner-document solution; `LoopingIteration` is one iteration's state, and the `LoopContinuation`/`LoopingAction`/`LoopRepeats` discriminants own break-timing, per-step action, and repeat-count semantics.

| [INDEX] | [SYMBOL]           | [KIND]           | [CAPABILITY]                                                              |
| :-----: | :----------------- | :--------------- | :------------------------------------------------------------------------ |
|  [01]   | `Loop`             | iterative object | repeated inner-document solution with break and repeat control            |
|  [02]   | `LoopingIteration` | iteration state  | `Index` / `Path` / `Repeats` / `Trees` / `Coverages` for one pass         |
|  [03]   | `LoopContinuation` | smart enum       | `Continue` / `BreakBefore` / `BreakAfter` — the break-timing discriminant |
|  [04]   | `LoopingAction`    | smart enum       | `Break` / `Continue` — the per-step loop action                           |
|  [05]   | `LoopRepeats`      | smart enum       | `Exact` / `Lower` / `Upper` — the repeat-count bound semantics            |

[PUBLIC_TYPE_SCOPE]: GH1 import boundary
- rail: composite and iterative document objects
- note: `GH1InteropComponent` is the one type that admits a legacy Grasshopper 1 component — it wraps the `Grasshopper2.Interop.IGH_Component` interop contract and preserves the source identity and XML for round-trip.

| [INDEX] | [SYMBOL]              | [KIND]      | [CAPABILITY]                                                        |
| :-----: | :-------------------- | :---------- | :------------------------------------------------------------------ |
|  [01]   | `GH1InteropComponent` | import shim | wraps `Grasshopper2.Interop.IGH_Component`; source id/name/icon/XML |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cluster lifecycle
- rail: composite and iterative document objects
- note: a `Cluster` constructs from a set of document objects, deriving inner/outer id maps; `EnsureMaps`/`FindAll*` resolve its `Shout`/`Listen` boundary pins, `Disentangle` separates entangled siblings, and `ExpireFull` invalidates the whole nested solution.

| [INDEX] | [SURFACE]                                                                                        | [CALL_SHAPE] | [CAPABILITY]                                               |
| :-----: | :----------------------------------------------------------------------------------------------- | :----------- | :--------------------------------------------------------- |
|  [01]   | `Cluster(IDocumentObject[], out Guid[][], out Guid[][])`                                         | construct    | build a cluster and its inner/outer id maps                |
|  [02]   | `InnerDocument` / `EntangledClusters` / `LoopSolution`                                           | state        | the nested document, entangled siblings, and loop solution |
|  [03]   | `EnsureMaps(out Listen[], out Shout[])` / `ClearMaps`                                            | boundary     | resolve or reset the input/output boundary pins            |
|  [04]   | `CreateListeners(IDocumentObject[], HashSet<Guid>)` / `CreateShouters(...)`                      | boundary     | mint the `Listen`/`Shout` boundary pins for a set          |
|  [05]   | `FindAllListeners(Document, out Listen[])` / `FindAllShouters(Document, out Shout[])`            | boundary     | enumerate every boundary pin in a document                 |
|  [06]   | `Disentangle(ActionList)` / `ExternalSequences(IEnumerable<Guid>, HashSet<Guid>)` / `ExpireFull` | mutate       | separate, sequence, and invalidate the cluster             |

[ENTRYPOINT_SCOPE]: chain construction and connection migration
- rail: composite and iterative document objects
- note: a `Chain` orders its links and validates the run; `MigrateConnections`/`UnmigrateConnections` re-route wires as objects enter or leave the chain, all under an `ActionList`.

| [INDEX] | [SURFACE]                                                                                                                       | [CALL_SHAPE] | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------ | :----------- | :------------------------------------------ |
|  [01]   | `Chain(Guid, Guid, IDocumentObject[])`                                                                                          | construct    | build a chain over an ordered object run    |
|  [02]   | `Links` / `LeadingObject` / `TrailingObject`                                                                                    | state        | the ordered links and the run endpoints     |
|  [03]   | `OrderChainLinks(IDocumentObject[])` / `ValidateChain(IDocumentObject[], out IDocumentObject, out IDocumentObject, out string)` | validate     | order the links and report the run validity |
|  [04]   | `IsChainObject(IDocumentObject)` / `ObjectFromIndex(int)`                                                                       | query        | membership and index lookup                 |
|  [05]   | `MigrateConnections(IDocumentObject, IDocumentObject, ActionList)` / `UnmigrateConnections(...)`                                | migrate      | re-route wires as objects join or leave     |

[ENTRYPOINT_SCOPE]: loop drive and iteration
- rail: composite and iterative document objects
- note: `Loop` pushes each `LoopingIteration`, resolves its repeat count, and flushes accumulated trees; `Continuation` reads the break-timing and `ThrowOnCancel` honours cancellation.

| [INDEX] | [SURFACE]                                                                                                             | [CALL_SHAPE] | [CAPABILITY]                                            |
| :-----: | :-------------------------------------------------------------------------------------------------------------------- | :----------- | :------------------------------------------------------ |
|  [01]   | `Push(LoopingIteration)` / `Continuation` / `Flush`                                                                   | drive        | advance, read continuation, and finalize the loop       |
|  [02]   | `ResolveRepetition` / `ThrowOnCancel`                                                                                 | control      | resolve the repeat count and honour cancellation        |
|  [03]   | `Iterations` / `InitialIterations` / `AccumulatedTrees` / `Solution` / `Document`                                     | state        | iteration count, accumulated output, and inner solution |
|  [04]   | `IsCyclical` / `IsProperLoop`                                                                                         | query        | topology classification of the loop                     |
|  [05]   | `GH1InteropComponent(IGH_Component)` / `Grasshopper1Id` / `Grasshopper1Name` / `Grasshopper1Icon` / `Grasshopper1Xml` | import       | wrap and preserve a legacy GH1 component                |

## [04]-[IMPLEMENTATION_LAW]

[STANDARD_TOPOLOGY]:
- `Cluster` is the nesting owner: it holds an inner `Grasshopper2.Doc.Document`, binds it to outer pins through `Listen` (input) and `Shout` (output) special objects resolved by `EnsureMaps`, tracks `EntangledClusters` sharing inner state, and invalidates through `ExpireFull` — its `LoopAccess`/`LoopAccumulation`/`LoopCycle`/`LoopRepeatIndex`/`LoopRepeatMethod` keys carry the loop-mode state when the cluster drives iteration
- `Chain` is the ordered-run owner: `OrderChainLinks` sorts its `Links` into `LeadingObject`…`TrailingObject`, `ValidateChain` reports the run validity, and `MigrateConnections`/`UnmigrateConnections` re-route wires under an `ActionList` as membership changes
- `Loop` is the iteration driver: it `Push`es each `LoopingIteration` (carrying `Index`, `Path`, `Repeats`, `Trees`, `Coverages`), reads `Continuation` (`Continue`/`BreakBefore`/`BreakAfter`), resolves the repeat count under `LoopRepeats` (`Exact`/`Lower`/`Upper`), accumulates outputs per `Accumulation` (`None`/`List`/`Layered`/`Last`), and `Flush`es `AccumulatedTrees`; `LoopingAction` is the `Break`/`Continue` per-step verb
- `GH1InteropComponent` is the sole legacy boundary: it wraps a `Grasshopper2.Interop.IGH_Component` and preserves `Grasshopper1Id`/`Grasshopper1Name`/`Grasshopper1Icon`/`Grasshopper1Xml` so a GH1 definition imports and round-trips without a GH1 runtime living inside GH2

[STACKING]:
- `api-thinktecture-runtime-extensions`(`.api/api-thinktecture-runtime-extensions.md`): `Accumulation`, `ClusterBoundary`, `LoopContinuation`, `LoopingAction`, and `LoopRepeats` fold onto `[SmartEnum]` owners, so accumulation mode, boundary role, break timing, per-step action, and repeat bound are each one exhaustive dispatch value rather than an `int` flag or a string compare
- `api-languageext`(`.api/api-languageext.md`): the loop drive is a `Seq`/`FoldM` over `LoopingIteration`s landing on `Fin`, so `ThrowOnCancel` and a break resolve as a short-circuit on the effect rail rather than a thrown exception; `Cluster.EnsureMaps`/`FindAll*` and `Chain.ValidateChain` out-parameters lift onto `Fin`/`Validation`, so a malformed cluster boundary or an invalid chain run reports as a typed `Error`
- `api-languageext`(`.api/api-languageext.md`): `GH1InteropComponent(IGH_Component)` is an import that lifts onto `Fin<Component>`, so a failed GH1 conversion is a typed boundary fault carrying the source `Grasshopper1Xml`, never a `null` component; `AccumulatedTrees` composes the `Garden` tree algebra (`api-gh2-components`) as the accumulation carrier
- `api-generator-equals`(`.api/api-generator-equals.md`): `LoopingIteration` and the cluster id maps take generated structural equality, so an iteration-state or map compare is one generated equality

[LOCAL_ADMISSION]:
- `Cluster`/`Chain`/`Loop` are the composite and iterative owners the folder extends; a hand-rolled sub-graph iterator or a bespoke wire-migration walker beside them is the rejected form
- the loop control discriminants compose the generated `[SmartEnum]` owners; an `int` iteration flag or a stringly-typed accumulation mode is never re-minted
- `GH1InteropComponent` is the only admitted GH1 surface, and only as an import boundary lifting onto `Fin`; authoring against `Grasshopper.Kernel` GH1 types anywhere else is the rejected form
- the inner `Document` is owned by `api-gh2-document`; a standard object composes that surface rather than re-deriving nested-document lifecycle

[RAIL_LAW]:
- Package: `Grasshopper2.dll` (Rhino 9 WIP Grasshopper2 SDK, in-process managed plug-in; `Grasshopper2.Components.Standard`)
- Owns: the `Cluster`/`ClusterBoundary` nesting lifecycle, the `Chain` ordered-run and connection migration, the `Loop`/`LoopingIteration`/`LoopContinuation`/`LoopRepeats`/`LoopingAction`/`Accumulation` iteration family, and the `GH1InteropComponent` import boundary
- Accept: a composite or iterative object extending `Component`, its inner graph reached through `api-gh2-document`, its control discriminants folded onto `[SmartEnum]`s, its loop drive a `Fin`-landing fold, its cluster/chain validation lifted onto `Fin`/`Validation`, and a GH1 definition admitted only through `GH1InteropComponent` onto `Fin<Component>`
- Reject: a hand-rolled sub-graph iterator or wire-migration walker; an `int`/string loop-control flag beside the `[SmartEnum]` owners; a GH1 `Grasshopper.Kernel` dependency outside the `GH1InteropComponent` import boundary; a nested-document lifecycle re-derived here rather than composed from `api-gh2-document`
