# [RASM_GRASSHOPPER_API_GH2_STANDARD_COMPONENTS]

`Grasshopper2.Components.Standard` exposes two public composite document objects and one public GH1 host component. `Cluster` owns a nested `Document` and maps outer inputs to inner `Listen` objects and outer outputs to inner `Shout` objects; `Chain` owns a duplicated linear object run and migrates its external connections; and `GH1InteropComponent` serializes the identity, XML state, name, and converted icon needed to execute an otherwise unupgraded Grasshopper 1 component. Loop execution remains an assembly-internal implementation behind `Cluster.LoopSolution`; the composite/loop family exposes only the `Accumulation`, `ClusterBoundary`, and `LoopContinuation` enums.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grasshopper2` 'Rhino 9 WIP Grasshopper2 SDK'

- assembly: `Grasshopper2.dll` (`Grasshopper2Plugin.rhp`; in-process)
- namespaces: `Grasshopper2.Components.Standard`, `Grasshopper2.Components`, `Grasshopper2.Doc`, `Grasshopper2.Parameters.Special`, `Grasshopper2.Undo`, `Grasshopper2.Interop`
- public composites: `Cluster`, `Chain`
- public interop host: `GH1InteropComponent`
- rail: composite document objects and GH1 hosting

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: composite objects and control enums

- rail: composite document objects and GH1 hosting
- note: `Cluster`, `Chain`, and `GH1InteropComponent` are public sealed `Component` subclasses; `Cluster` and `Chain` also implement `IDocumentParent` and `IPinCushion`.

| [INDEX] | [SYMBOL]              | [KIND]                  | [CAPABILITY]                                                              |
| :-----: | :-------------------- | :---------------------- | :------------------------------------------------------------------------ |
|  [01]   | `Cluster`             | public sealed component | duplicated inner `Document`, boundary maps, entanglement, loop enablement |
|  [02]   | `Chain`               | public sealed component | duplicated preordered object run and external connection migration        |
|  [03]   | `GH1InteropComponent` | public sealed component | hosted GH1 execution from serialized identity/XML plus converted ETO icon |
|  [04]   | `Accumulation`        | public enum             | `None=0`, `Last=1`, `List=2`, `Layered=3`                                 |
|  [05]   | `ClusterBoundary`     | public enum             | `None=0`, `Input=1`, `Output=2`, `Index=3`                                |
|  [06]   | `LoopContinuation`    | public enum             | `Continue=1`, `BreakAfter=2`, `BreakBefore=3`                             |

## [03]-[PUBLIC_ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cluster construction and boundary maps

- rail: composite document objects
- note: the object-array constructor duplicates its input objects. `inputMapping[i]` names the inner parameter ids whose input lists migrate for cluster input `i`; `outputMapping[i]` names the inner parameter ids whose output lists migrate for cluster output `i`.

The full construction signature is `Cluster(IDocumentObject[] objects, out Guid[][] inputMapping, out Guid[][] outputMapping)`.

| [INDEX] | [SURFACE]                                                   | [CALL_SHAPE] | [CAPABILITY]                          |
| :-----: | :---------------------------------------------------------- | :----------- | :------------------------------------ |
|  [01]   | `Cluster()` · `Cluster(IReader)`                            | construct    | empty or serialized shell             |
|  [02]   | `Cluster(objects, out inputMapping, out outputMapping)`     | construct    | duplicated graph plus migration maps  |
|  [03]   | `InnerDocument` · `EntangledClusters` · `RelayMessages`     | state        | document, entanglement, message relay |
|  [04]   | `LoopSolution`                                              | state        | public loop switch                    |
|  [05]   | `EnsureMaps(out Listen[] mapIn, out Shout[] mapOut) : void` | boundary     | cached input/output boundary arrays   |
|  [06]   | `Disentangle(ActionList)`                                   | mutate       | new inner hash under undo actions     |

`EnsureMaps` assumes the outer parameters and inner boundary objects already agree. Its array references are populated, but individual entries are runtime-null when a match is absent despite the oblivious `Listen[]`/`Shout[]` metadata. It has no success result. `ClearMaps`, `CreateListeners`, `CreateShouters`, `FindAllListeners`, `FindAllShouters`, `ExternalSequences`, and `ExpireFull` are private implementation members.

[ENTRYPOINT_SCOPE]: chain construction and connection migration

- rail: composite document objects
- note: the object-array constructor requires a causally sorted linear sequence and duplicates the objects into an inert inner document; it does not publish its endpoint objects.

The full construction signature is `Chain(Guid leadingId, Guid trailingId, params IDocumentObject[] objects)`.

| [INDEX] | [SURFACE]                                                | [CALL_SHAPE] | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------- | :----------- | :------------------------------------- |
|  [01]   | `Chain()` · `Chain(IReader)`                             | construct    | empty or serialized shell              |
|  [02]   | `Chain(leadingId, trailingId, params objects)`           | construct    | duplicated preordered run              |
|  [03]   | `Links : IEnumerable<IDocumentObject>`                   | state        | borrowed hosted objects                |
|  [04]   | `OrderChainLinks(objects) : IDocumentObject[]?`          | inspect      | `null` or the first object only        |
|  [05]   | `IsChainObject(obj) : bool`                              | classify     | component/free-parameter admission     |
|  [06]   | `ValidateChain(chain, out first, out last, out message)` | inspect      | always-false verdict plus diagnostic   |
|  [07]   | `MigrateConnections(leading, trailing, actions) : void`  | migrate      | external connection transfer with undo |

`OrderChainLinks(IDocumentObject[] objects) : IDocumentObject[]?` returns `null` for a null array or fewer than two objects; every other input returns a one-element array containing only `objects[0]`. The declared validation signature is `ValidateChain(IDocumentObject[] chain, out IDocumentObject first, out IDocumentObject last, out string message) : bool`; nullable annotations are disabled, while runtime behavior is nullable. It initializes `first` and `last` to `null`, never assigns either output, and never succeeds because private `ValidateLinearity` unconditionally returns `false`; `message` carries the first failed admission diagnostic or the linearity diagnostic. These members preserve their public signatures as catalogue facts but supply no ordering or validation capability. `LeadingObject` and `TrailingObject` are private, `ObjectFromIndex` is internal, and `UnmigrateConnections` is private; none is a consumer surface.

[ENTRYPOINT_SCOPE]: GH1 wrapper and host component

- rail: GH1 hosting
- note: `Grasshopper2.Interop.IGH_Component` is a public sealed wrapper class around a live object implementing the GH1 `Grasshopper.Kernel.IGH_Component` interface; it is not an interface.

| [INDEX] | [SURFACE]                                         | [CALL_SHAPE] | [CAPABILITY]                     |
| :-----: | :------------------------------------------------ | :----------- | :------------------------------- |
|  [01]   | `new IGH_Component(object gh1Component)`          | wrap         | live GH1 wrapper                 |
|  [02]   | `InputCount` · `OutputCount` · `Input` · `Output` | inspect      | nullable indexed parameter reads |
|  [03]   | `TransferInputs` · `TransferOutputs`              | transfer     | selected parameter-property copy |
|  [04]   | `GH1InteropComponent()` · `(IReader)`             | construct    | empty or persisted host          |
|  [05]   | `GH1InteropComponent(IGH_Component wrapper)`      | construct    | identity/XML/name/icon capture   |
|  [06]   | `Grasshopper1Id` · `Xml` · `Name` · `Icon`        | state        | stored GH1 execution payload     |

The wrapper constructor normalizes missing GH1 name and XML text to empty strings. The public empty constructor leaves `Grasshopper1Id` at `Guid.Empty` and the reference properties uninitialized until hydration; the icon also remains `null` when the GH1 object or serialized record carries no icon. `IGH_Component.Icon` converts the GH1 GDI bitmap into a new `Eto.Drawing.Bitmap`; `GH1InteropComponent` retains that bitmap without exposing disposal or ownership transfer, so property consumers borrow it for the component lifetime. Processing calls the loaded GH1 runtime with `Grasshopper1Xml`; this host is not a runtime-free conversion.

## [04]-[INTERNAL_LOOP_BOUNDARY]

[INTERNAL_TYPE_SCOPE]: loop implementation

- rail: assembly implementation only
- note: public-looking members on an internal declaring type remain inaccessible to `Rasm.Grasshopper`.

| [INDEX] | [SYMBOL]                                 | [ACCESS]         | [SHAPE]                               |
| :-----: | :--------------------------------------- | :--------------- | :------------------------------------ |
|  [01]   | `Loop`                                   | internal sealed  | non-component cluster driver          |
|  [02]   | `LoopingIteration`                       | internal sealed  | path/index/repeat/tree/coverage state |
|  [03]   | `LoopingResults`                         | internal sealed  | count/phase/output-tree state         |
|  [04]   | `LoopingAction` · `LoopRepeats`          | internal enums   | action and repeat policy              |
|  [05]   | `Continuation()`                         | internal type    | public continuation method            |
|  [06]   | `Push(LoopingIteration) : bool`          | internal type    | bounded enqueue                       |
|  [07]   | `Flush() : void`                         | internal type    | queued execution                      |
|  [08]   | `AccumulatedTrees` · `InitialIterations` | internal type    | output array and initial stream       |
|  [09]   | `ResolveRepetition() : int`              | private instance | combined repeat count                 |

The internal constructor is `Loop(Cluster cluster, IDataAccess da, Listen[] mapIn, Shout[] mapOut)`. `LoopingIteration` constructs as `(Loop loop, Path path, int index, int repeats, ITree[] trees, Coverage[] coverages)`. `AccumulatedTrees` returns a fresh `ITree[]`; `InitialIterations` returns `IEnumerable<LoopingIteration>`; `Continuation` is an instance method; `Push` returns its enqueue verdict; and `ResolveRepetition` is called privately during `Loop` construction to populate the readonly `Repeats` field, using a negative result when no repeat counter is configured.

No external package surface accepts or returns `Loop`, `LoopingIteration`, `LoopingResults`, `LoopingAction`, or `LoopRepeats`. Loop composition enters only through public `Cluster` state and supported document execution.

## [05]-[IMPLEMENTATION_LAW]

[STANDARD_TOPOLOGY]:

- `Cluster` duplicates selected objects into its hosted document. Outer inputs align with `Listen` rows, outer outputs align with `Shout` rows, and `EnsureMaps` returns those cached arrays by `out` without validation or a verdict.
- Assigning or mutating `InnerDocument` does not rebuild cluster parameters automatically. Boundary-array length and non-null entries require explicit admission after `EnsureMaps`.
- `Chain` duplicates a preordered object sequence into an inert hosted document. `Links` exposes borrowed hosted objects; `MigrateConnections` is the sole public direction of connection transfer.
- `OrderChainLinks` returns `null` or a one-element first-object array, and `ValidateChain` always returns `false` because `ValidateLinearity` is an unconditional false gate; neither member enters a composition rail.
- `GH1InteropComponent` is a host, not an upgrader. It requires a loadable GH1 runtime during processing and persists the XML state consumed by that runtime.

[LOCAL_ADMISSION]:

- public composition admits `Cluster`, `Chain`, `GH1InteropComponent`, `Accumulation`, `ClusterBoundary`, and `LoopContinuation` only
- internal loop types and private cluster/chain helpers never appear in public signatures, generated vocabularies, factories, or operation rails
- `EnsureMaps` lifts as a void host call followed by explicit array and element admission; it never lifts as a boolean confirmation
- `OrderChainLinks` and `ValidateChain` remain signature facts only; public composition does not call either incomplete implementation
- `IGH_Component` is admitted as a wrapper class around an already validated live GH1 component object, its nullable parameter/icon reads remain explicit, and transfer index pairs are range-admitted before invocation

[RAIL_LAW]:

- Package: `Grasshopper2.dll` (`Grasshopper2.Components.Standard`; in-process)
- Owns: public cluster nesting and maps, public chain construction and connection migration, public control enums, GH1 wrapper-backed hosting, and the internal loop implementation behind clusters
- Accept: duplicated composite ownership, explicit map admission, caller-preordered chain construction with explicit endpoint ids, borrowed hosted objects and icons, and GH1 execution through the serialized host component
- Reject: calls to private/internal loop, map, expiry, endpoint, reverse-migration, or repetition members; boolean treatment of `EnsureMaps`; functional use of `OrderChainLinks` or `ValidateChain`; interface treatment of `Grasshopper2.Interop.IGH_Component`; runtime-free treatment of `GH1InteropComponent`
