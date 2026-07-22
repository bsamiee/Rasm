# [RASM_GRASSHOPPER_API_GH2_STANDARD_COMPONENTS]

`Grasshopper2.Components.Standard` owns three sealed composite document objects: `Cluster` nests a `Document` and binds outer inputs to inner `Listen` and outer outputs to inner `Shout`, `Chain` duplicates a preordered linear run and migrates its external connections, and `GH1InteropComponent` hosts an unupgraded Grasshopper 1 component from serialized identity, XML, name, and a converted icon. Loop execution stays assembly-internal behind `Cluster.LoopSolution`; the composite surface exposes only the `Accumulation`, `ClusterBoundary`, and `LoopContinuation` enums.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grasshopper2` standard components
- host: `Grasshopper2.dll` inside `Grasshopper2Plugin.rhp`, loaded in-process by Rhino 9 WIP
- namespace: `Grasshopper2.Components.Standard`, `Grasshopper2.Interop`
- rail: composite document objects and GH1 hosting

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: sealed `Component` composites and control enums; `Cluster` and `Chain` also implement `IDocumentParent` and `IPinCushion`.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `Cluster`             | class         | duplicated inner `Document`, boundary maps, entanglement, loop enablement |
|  [02]   | `Chain`               | class         | duplicated preordered object run and external connection migration        |
|  [03]   | `GH1InteropComponent` | class         | hosted GH1 execution from serialized identity/XML plus converted ETO icon |
|  [04]   | `Accumulation`        | enum          | `None=0`, `Last=1`, `List=2`, `Layered=3`                                 |
|  [05]   | `ClusterBoundary`     | enum          | `None=0`, `Input=1`, `Output=2`, `Index=3`                                |
|  [06]   | `LoopContinuation`    | enum          | `Continue=1`, `BreakAfter=2`, `BreakBefore=3`                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cluster construction and boundary maps

| [INDEX] | [SURFACE]                                                | [SHAPE]   | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------- | :-------- | :------------------------------------ |
|  [01]   | `Cluster()` · `Cluster(IReader)`                         | construct | empty or serialized shell             |
|  [02]   | `Cluster(IDocumentObject[], out Guid[][], out Guid[][])` | construct | duplicated graph plus migration maps  |
|  [03]   | `InnerDocument` · `EntangledClusters` · `RelayMessages`  | property  | document, entanglement, message relay |
|  [04]   | `LoopSolution`                                           | property  | public loop switch                    |
|  [05]   | `EnsureMaps(out Listen[], out Shout[]) : void`           | boundary  | cached input/output boundary arrays   |
|  [06]   | `Disentangle(ActionList)`                                | mutate    | new inner hash under undo actions     |

- `Cluster(IDocumentObject[], out inputMapping, out outputMapping)`: duplicates the input objects; `inputMapping[i]` and `outputMapping[i]` name the inner parameter ids whose input and output lists migrate for cluster input and output `i`.
- `EnsureMaps`: leaves a `Listen[]`/`Shout[]` entry runtime-null where no outer-to-inner match exists, and returns no success verdict.

[ENTRYPOINT_SCOPE]: chain construction and connection migration

| [INDEX] | [SURFACE]                                                          | [SHAPE]   | [CAPABILITY]                           |
| :-----: | :----------------------------------------------------------------- | :-------- | :------------------------------------- |
|  [01]   | `Chain()` · `Chain(IReader)`                                       | construct | empty or serialized shell              |
|  [02]   | `Chain(Guid, Guid, params IDocumentObject[])`                      | construct | duplicated preordered run              |
|  [03]   | `Links : IEnumerable<IDocumentObject>`                             | property  | borrowed hosted objects                |
|  [04]   | `OrderChainLinks(IDocumentObject[]) : IDocumentObject[]?`          | inspect   | `null` or the first object only        |
|  [05]   | `IsChainObject(IDocumentObject) : bool`                            | classify  | component/free-parameter admission     |
|  [06]   | `ValidateChain(IDocumentObject[], out, out, out) : bool`           | inspect   | always-false verdict plus diagnostic   |
|  [07]   | `MigrateConnections(IDocumentObject, IDocumentObject, ActionList)` | migrate   | external connection transfer with undo |

- `Chain(Guid leadingId, Guid trailingId, params IDocumentObject[])`: requires a causally sorted linear sequence, duplicated into an inert inner document with no published endpoint objects.
- `OrderChainLinks`: returns `null` for a null array or fewer than two objects, else a one-element array of `objects[0]` only.
- `ValidateChain`: initializes `first`/`last` to `null`, assigns neither, and always returns `false` because `ValidateLinearity` is an unconditional false gate; `message` carries the failed-admission or linearity diagnostic.

[ENTRYPOINT_SCOPE]: GH1 wrapper and host; `Grasshopper2.Interop.IGH_Component` is a sealed wrapper class around a live GH1 `Grasshopper.Kernel.IGH_Component`, not an interface.

| [INDEX] | [SURFACE]                                                                      | [SHAPE]   | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------------------------------- | :-------- | :------------------------------- |
|  [01]   | `new IGH_Component(object)`                                                    | wrap      | live GH1 wrapper                 |
|  [02]   | `InputCount` · `OutputCount` · `Input(int)` · `Output(int)`                    | inspect   | nullable indexed parameter reads |
|  [03]   | `TransferInputs(Component, (int,int)[])` · `TransferOutputs`                   | transfer  | selected parameter-property copy |
|  [04]   | `GH1InteropComponent()` · `(IReader)`                                          | construct | empty or persisted host          |
|  [05]   | `GH1InteropComponent(IGH_Component)`                                           | construct | identity/XML/name/icon capture   |
|  [06]   | `Grasshopper1Id` · `Grasshopper1Xml` · `Grasshopper1Name` · `Grasshopper1Icon` | property  | stored GH1 execution payload     |

- `GH1InteropComponent(IGH_Component)`: normalizes missing GH1 name and XML to empty strings; the empty ctor leaves `Grasshopper1Id` at `Guid.Empty` and the reference properties null until hydration.
- `Grasshopper1Icon`: converts the GH1 `System.Drawing.Bitmap` into a new `Eto.Drawing.Bitmap` at this boundary and retains it without disposal transfer, so property consumers borrow it for the component lifetime.
- `GH1InteropComponent` processing: invokes the loaded GH1 runtime with `Grasshopper1Xml`, never a runtime-free conversion.

## [04]-[INTERNAL_LOOP_BOUNDARY]

[INTERNAL_TYPE_SCOPE]: assembly-internal loop implementation; public-looking members on an internal declaring type stay inaccessible to `Rasm.Grasshopper`.

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

- No external surface accepts or returns `Loop`, `LoopingIteration`, `LoopingResults`, `LoopingAction`, or `LoopRepeats`; loop composition enters only through public `Cluster` state and document execution.

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Cluster` and `Chain` each duplicate their selected objects into an owned inner `Document`; the outer graph binds the inner boundary only through explicit map admission, never an automatic parameter rebuild.
- `Cluster` aligns outer inputs to `Listen` rows and outer outputs to `Shout` rows, and `EnsureMaps` returns those cached arrays by `out` with no validation or verdict.
- `Chain` transfers only through `MigrateConnections`; `OrderChainLinks` and `ValidateChain` carry no ordering or validation capability and enter no composition rail.
- `GH1InteropComponent` hosts against a live GH1 runtime and persists its XML, never upgrading the component.

[STACKING]:
- `api-languageext`(`.api/api-languageext.md`): `EnsureMaps` out-arrays lift onto `Option<Listen>`/`Option<Shout>` per boundary slot so a runtime-null entry is an absent case, and `OrderChainLinks`/`ValidateChain` — a stub order and an always-false verdict — lower onto `Fin` so a chain-admission refusal is a typed `Error` rather than a silent `false`.
- `api-thinktecture-runtime-extensions`(`.api/api-thinktecture-runtime-extensions.md`): `Accumulation`, `ClusterBoundary`, and `LoopContinuation` fold onto `[SmartEnum]` owners, so an accumulation mode, boundary role, or loop-continuation branch dispatches through one exhaustive `Switch`.
- `NativeObject`(within-lib): composes `Cluster`/`Chain` construction and `GH1InteropComponent` import admission, mapping outer-to-inner boundaries and the GH1 icon conversion at one seam.

[LOCAL_ADMISSION]:
- Public composition admits `Cluster`, `Chain`, `GH1InteropComponent`, `Accumulation`, `ClusterBoundary`, and `LoopContinuation`; internal loop types and private cluster/chain helpers never enter a public signature, generated vocabulary, factory, or operation rail.
- `EnsureMaps` lifts as a void host call followed by explicit array and element admission, never a boolean confirmation.
- `OrderChainLinks` and `ValidateChain` remain signature facts; public composition calls neither.
- `IGH_Component` is admitted as a wrapper around a validated live GH1 object, its nullable parameter and icon reads stay explicit, and transfer index pairs are range-admitted before invocation.

[RAIL_LAW]:
- Package: `Grasshopper2.dll` (`Grasshopper2.Components.Standard`, in-process)
- Owns: public cluster nesting and boundary maps, chain construction and connection migration, the composite control enums, GH1 wrapper-backed hosting, and the internal loop implementation behind clusters
- Accept: duplicated composite ownership, explicit map admission, caller-preordered chain construction with explicit endpoint ids, borrowed hosted objects and icons, and GH1 execution through the serialized host
- Reject: reaching private or internal loop, map, expiry, endpoint, reverse-migration, or repetition members; boolean treatment of `EnsureMaps`; functional use of `OrderChainLinks` or `ValidateChain`; interface treatment of `Grasshopper2.Interop.IGH_Component`; runtime-free treatment of `GH1InteropComponent`
