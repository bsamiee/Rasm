# [RASM_GRASSHOPPER_API_GH2_COMPONENTS]

`Grasshopper2.Components` is the GH2 component-authoring model — the `Component`/`ModularComponent` document object whose lifecycle runs construction, pin registration, per-access and iteration-array processing, and variable-parameter mutation, with `IDataAccess` as the single item/pear/twig/tree access seam into the running solution. Typed pins register through the `InputAdder`/`OutputAdder` families, and their modular variants add label, colour, and hidden-pin state. The `Garden` tree algebra and `Grasshopper2.Types.Conversion` brokers own tree construction and conversion; `Grasshopper2.Framework` `Plugin`/`PluginServer` own registration, public loading, and object-to-plugin resolution. `IDataAccess`, the adder families, and `ComponentParameters` live under `Grasshopper2.Components`; geometry brokers live under `Grasshopper2.Parameters.Standard`; type assistants live under `Grasshopper2.Types.Assistant`; `Solution` and `FleetingCustomData` live under `Grasshopper2.Doc`; and `Connections` lives under `Grasshopper2.Parameters`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grasshopper2` 'Rhino 9 WIP Grasshopper2 SDK'

- assembly: `Grasshopper2.dll` (installed `Grasshopper2Plugin.rhp` managed plug-in; in-process, no NuGet redistribution)
- namespace: `Grasshopper2.Components`, `.Components.Standard`, `Grasshopper2.Parameters`, `.Parameters.Standard`, `Grasshopper2.Data`, `.Data.Meta`, `Grasshopper2.Types.Assistant`, `.Types.Conversion`, `Grasshopper2.Framework`, `GrasshopperIO`
- host: RhinoWIP `RhCore.framework` — `Rhino.Geometry` supplies the carrier types component pins bind
- io: `GrasshopperIO` `IoIdAttribute` stamps the persistent type id every document object serializes under
- rail: component-authoring model

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: component authoring and lifecycle

- rail: component-authoring model
- note: `Component` is the document-object base; `ModularComponent` adds the attribute-driven modular pin surface and the `__`-prefixed well-known custom-value keys; `ComponentParameters` owns the live input/output pin lists.

| [INDEX] | [SYMBOL]              | [KIND]            | [CAPABILITY]                                                                     |
| :-----: | :-------------------- | :---------------- | :------------------------------------------------------------------------------- |
|  [01]   | `Component`           | document object   | pin registration, `Process`, bake, threading, variable-parameter lifecycle       |
|  [02]   | `ModularComponent`    | modular component | `ModularInputs`/`ModularOutputs` plus icon/colour/category/hidden attribute keys |
|  [03]   | `ComponentParameters` | pin list          | live `Inputs`/`Outputs`, add/remove, and auto-maintenance of variable pins       |
|  [04]   | `Side`                | smart enum        | `Input` / `Output` — the pin-side discriminant every mutation carries            |
|  [05]   | `Plugin`              | plugin root       | author/version identity, `SatelliteAssemblies`, `ExportedTypes`, `OnLoaded`      |
|  [06]   | `PluginServer`        | registrar         | location/assembly plugin loading and object-to-plugin resolution                 |
|  [07]   | `IoIdAttribute`       | io identity       | the persistent type-id attribute (`GrasshopperIO`)                               |

[PUBLIC_TYPE_SCOPE]: data access and pin registration

- rail: component-authoring model
- note: `IDataAccess` is the ONE seam `Process` receives — typed get/set over item/pear/twig/tree plus tolerance, unit, transform, and message emission; the adder families are the typed pin-declaration surface split by direction.
- note: `IParameter.PersistentDataWeak` is `ITree { get; set; }`; `TypeAssistantWeak` is `ITypeAssistant { get; }`; `PresetsWeak` is `IPresets { get; }`.

| [INDEX] | [SYMBOL]             | [KIND]        | [CAPABILITY]                                                             |
| :-----: | :------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `IDataAccess`        | access seam   | typed data and host-context reads, output writes, messages, and progress |
|  [02]   | `InputAdder`         | pin adder     | typed input declaration with `Access` and `Requirement`                  |
|  [03]   | `OutputAdder`        | pin adder     | typed output declaration with `Access`                                   |
|  [04]   | `ModularInputAdder`  | modular adder | input declaration plus label, colour, category, and hidden state         |
|  [05]   | `ModularOutputAdder` | modular adder | output declaration plus label, colour, category, and hidden state        |
|  [06]   | `IParameter`         | pin contract  | access/presence plus writable persistent data and read-only services     |
|  [07]   | `Access`             | smart enum    | `Item` / `Twig` / `Tree` — the pin data-depth discriminant               |
|  [08]   | `Requirement`        | smart enum    | `MustExist` / `MayBeNull` / `MayBeMissing` — the pin presence contract   |
|  [09]   | `ITypeAssistant`     | type service  | read, parse, display, geometry, transform, measure, and bake projection  |

[PUBLIC_TYPE_SCOPE]: tree data algebra and conversion

- rail: component-authoring model
- note: `Tree<T>`/`Twig<T>`/`Pear<T>`/`Leaf<T>` are the goo carriers; `Garden` is the static tree-construction and tree-wise operation family; the brokers cast-or-convert a host object onto the concrete `Rhino.Geometry` family, and `ConversionServer` carries the merit-scored generic convert.

| [INDEX] | [SYMBOL]                        | [KIND]          | [CAPABILITY]                                                            |
| :-----: | :------------------------------ | :-------------- | :---------------------------------------------------------------------- |
|  [01]   | `Tree<T>` / `ITree`             | data tree       | `Paths` / `Twigs` / `Pears` — the pathed branch structure a pin carries |
|  [02]   | `Twig<T>` / `ITwig`             | branch          | one branch; `Convert` and expression `Apply` over its pears             |
|  [03]   | `Pear<T>` / `IPear`             | leaf datum      | one value plus `MetaData` — the atomic goo unit                         |
|  [04]   | `Garden`                        | tree factory    | `TreeFrom*` builders plus `PairWiseOp` / `PearWiseOp` tree-wise folds   |
|  [05]   | `ConversionServer`              | convert broker  | merit-scored `object → Type` conversion                                 |
|  [06]   | `CurveBroker` / `SurfaceBroker` | geometry broker | cast-or-convert onto the concrete `Rhino.Geometry` curve/surface family |
|  [07]   | `MetaData`                      | pear metadata   | the per-pear annotation `Garden` and `SetItem` thread                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: component lifecycle and variable parameters

- rail: component-authoring model
- note: `Process(IDataAccess)` is the compute hook; the `*Process` family brackets it with `Solution`-scoped phases; the `Can*`/`Do*Parameter` pairs gate and apply variable-pin mutation under an `ActionList`.

| [INDEX] | [SURFACE]                                                   | [CALL_SHAPE] | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------- | :----------- | :--------------------------------- |
|  [01]   | `Component(Nomen)`                                          | author       | construct the component            |
|  [02]   | `AddInputs(InputAdder)` / `AddOutputs(OutputAdder)`         | author       | declare the fixed pin surface      |
|  [03]   | `Process(IDataAccess)`                                      | compute      | compute one access iteration       |
|  [04]   | `Process(IDataAccess[], CancellationToken)`                 | compute      | dispatch the iteration array       |
|  [05]   | `BeforeProcess(Solution)` / `PreProcess(Solution)`          | lifecycle    | open the solution-scoped process   |
|  [06]   | `PostProcess(Solution, FleetingCustomData)`                 | lifecycle    | close the solution-scoped process  |
|  [07]   | `PostProcessTree(ITree, int, Solution)`                     | lifecycle    | finalize one output tree           |
|  [08]   | `ComputeInternal(Solution, CallStack)`                      | lifecycle    | drive internal computation         |
|  [09]   | `Parameters` / `Connectivity` / `ConnectivityComplete`      | state        | expose pins and wire connectivity  |
|  [10]   | `Threading`                                                 | state        | select component processing policy |
|  [11]   | `SupportsVariableParameters`                                | gate         | expose variable-pin capability     |
|  [12]   | `CanCreateParameter(Side, int)` / `CanRemoveParameter(...)` | gate         | admit a variable-pin change        |
|  [13]   | `DoCreateParameter(Side, int, ActionList)`                  | mutate       | create a pin with undo             |
|  [14]   | `DoRemoveParameter(Side, int, ActionList)`                  | mutate       | remove a pin with undo             |
|  [15]   | `VariableParameterMaintenance`                              | mutate       | reconcile the changed pin surface  |
|  [16]   | `BakeCapable` / `BakeShapes(BakeContext, BakeUpdateMode)`   | bake         | gate and emit baked shapes         |
|  [17]   | `CreateAttributes`                                          | view         | construct object attributes        |

[ENTRYPOINT_SCOPE]: data access get, set, and diagnostics

- rail: component-authoring model
- note: every typed getter is an out-parameter `bool`-shaped read the folder lifts onto the result rail; `Set*` writes a pin's tree, and `Add*` emits a document message with optional `MessageAction`s.

| [INDEX] | [SURFACE]                                                      | [CALL_SHAPE] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------- | :----------- | :----------------------------------------- |
|  [01]   | `GetItem<T>(int, out T)` / `GetPear<T>(int, out Pear<T>)`      | typed get    | item or pear read                          |
|  [02]   | `GetTwig<T>(int, out Twig<T>)` / `GetTree<T>(int, out ...)`    | typed get    | twig or tree read                          |
|  [03]   | `GetIPear(int, out IPear)` / `GetITwig(int, out ITwig)`        | weak get     | erased pear or twig read                   |
|  [04]   | `GetITree(int, out ITree)`                                     | weak get     | erased tree read                           |
|  [05]   | `GetTolerance(out double, out double)` / `GetTolerance(...)`   | context      | numeric and angular tolerance              |
|  [06]   | `GetUnitSystem(out UnitSystem)` / `GetUnitScaling(...)`        | context      | model units and scaling                    |
|  [07]   | `GetTransform(int, out Transform)`                             | typed get    | transform read                             |
|  [08]   | `GetQuaternion(int, out Quaternion)`                           | typed get    | quaternion read                            |
|  [09]   | `SetItem(int, object, MetaData)` / `SetPear(int, IPear)`       | set          | item or pear output write                  |
|  [10]   | `SetTwig(int, ITwig)` / `SetTree(int, ITree)`                  | set          | twig or tree output write                  |
|  [11]   | `AddRemark(...)` / `AddWarning(...)` / `AddError(...)`         | diagnostics  | document message emission                  |
|  [12]   | `SetProgress(int)`                                             | diagnostics  | component progress                         |
|  [13]   | `Solution` / `Callstack`                                       | state        | running solution and call stack            |
|  [14]   | `GetItemArray<T>(int, out T[])` / `GetIPears(int, out ...)`    | array get    | iteration-aligned values and pears         |
|  [15]   | `GetNullArray(int, out bool[])` / `GetMetaArray(int, out ...)` | array get    | iteration-aligned null and metadata rows   |
|  [16]   | `GetItemWithTypeAssistant(int, out object, out ...)`           | assisted get | value read paired with its type service    |
|  [17]   | `GetIPearWithTypeAssistant(int, out IPear, out ...)`           | assisted get | pear read paired with its type service     |
|  [18]   | `GetItemWithCurveAssistant(int, out object, out ...)`          | assisted get | value read paired with its curve service   |
|  [19]   | `GetItemWithSurfaceAssistant(int, out object, out ...)`        | assisted get | value read paired with its surface service |

[ENTRYPOINT_SCOPE]: pin declaration, tree construction, and conversion

- rail: component-authoring model
- note: the adder `Add*` roster is one typed method per carrier plus `AddGeneric`/`AddEnum<T>`/`AddTopological`; `Garden` builds and folds trees; the brokers resolve a host object onto its concrete geometry family.
- note: typed folds are `Garden.PairWiseOp<A, B, R>(Tree<A>, Tree<B>, Func<A, B, R>, CancellationToken)` and `Garden.PearWiseOp<T>(Tree<T>, Func<Pear<T>, Pear<T>>, CancellationToken)`.
- note: `CurveBroker.CastOrConvert` returns `CurveType`; `SurfaceBroker.CastOrConvert` returns `SurfaceLikeType`; both return values are the case discriminants.
- note: public loading is `PluginServer.LoadPlugin(string, out FailureInfo)` or `LoadPlugin(string, Assembly, out FailureInfo)`; assembly harvesting is internal.

| [INDEX] | [SURFACE]                                              | [CALL_SHAPE] | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------- | :----------- | :----------------------------------------------- |
|  [01]   | `InputAdder.AddGeneric / AddEnum<T> / AddTopological`  | declare      | generic, enum, and topological input pins        |
|  [02]   | `InputAdder.AddPoint / AddVector / AddCurve`           | declare      | point, vector, and curve input pins              |
|  [03]   | `InputAdder.AddSurface / AddMesh / AddMetaData`        | declare      | surface, mesh, and metadata input pins           |
|  [04]   | `OutputAdder.AddGeneric / AddPoint / AddVector`        | declare      | generic, point, and vector output pins           |
|  [05]   | `OutputAdder.AddCurve / AddSurface / AddMesh`          | declare      | curve, surface, and mesh output pins             |
|  [06]   | `OutputAdder.AddMetaData`                              | declare      | metadata output pins                             |
|  [07]   | `ModularOutputAdder.AddHiddenCurve / AddHiddenSurface` | declare      | categorized hidden geometry outputs              |
|  [08]   | `Garden.TreeFromList<T> / TreeFromPears<T>`            | build        | tree construction from values or pears           |
|  [09]   | `Garden.TreeFromLeaves<T> / TreeFromTwigs<T>`          | build        | tree construction from leaves or twigs           |
|  [10]   | `Garden.PairWiseOp<A, B, R>`                           | fold         | typed binary tree operation                      |
|  [11]   | `Garden.PearWiseOp<T>`                                 | fold         | typed unary pear operation                       |
|  [12]   | `Twig<T>.Convert<U>`                                   | transform    | branch conversion                                |
|  [13]   | `Twig<T>.Apply`                                        | transform    | branch expression evaluation                     |
|  [14]   | `ConversionServer.Convert`                             | convert      | merit-scored target-type conversion              |
|  [15]   | `CurveBroker.CastOrConvert`                            | convert      | curve-family conversion with `CurveType` result  |
|  [16]   | `SurfaceBroker.CastOrConvert`                          | convert      | surface conversion with `SurfaceLikeType` result |
|  [17]   | `PluginServer.LoadPlugin(string, out ...)`             | register     | public location loading                          |
|  [18]   | `PluginServer.LoadPlugin(string, Assembly, ...)`       | register     | public assembly loading                          |
|  [19]   | `PluginServer.FindPluginForObject`                     | resolve      | object or type to loaded-plugin resolution       |

## [04]-[IMPLEMENTATION_LAW]

[COMPONENT_TOPOLOGY]:

- `Component` is the compute-carrying document object: it declares pins once through `AddInputs(InputAdder)`/`AddOutputs(OutputAdder)`, computes through `Process(IDataAccess)`, may override `Process(IDataAccess[], CancellationToken)` for iteration-array policy, and reconciles variable pins through the `Can*`/`Do*Parameter(Side, int, ActionList)` pairs — `Side` is the `Input`/`Output` discriminant and every structural edit rides an `ActionList` for undo
- `IDataAccess` is the single execution seam: typed `Get*`/`Set*` over item/pear/twig/tree depth, tolerance and unit context, transform reads, and `AddRemark`/`AddWarning`/`AddError` message emission; the component never reaches the `Document` from inside `Process`, it reads `access.Solution`/`access.Callstack`
- pins are `IParameter`s declared with an `Access` (`Item`/`Twig`/`Tree`) and a `Requirement` (`MustExist`/`MayBeNull`/`MayBeMissing`); the adder families are the only pin-declaration surface, one typed `Add*` per carrier plus `AddGeneric`/`AddEnum<T>`/`AddTopological`, and the modular adders extend each with label, colour, category, and hidden-pin state
- data is `Tree<T>` of `Twig<T>` of `Pear<T>` (`Leaf<T>` the typed pear): `Garden` builds trees from every source shape and folds them tree-wise (`PairWiseOp`/`PearWiseOp`), `Twig<T>.Convert`/`Apply` transform a branch, and the `Grasshopper2.Parameters.Standard` brokers plus `ConversionServer` resolve a host object onto its concrete family carrying a `Merit` score
- `ModularComponent` drives its pin surface from `__`-prefixed well-known custom-value keys (`__Icon`, `__Colour`, `__Optional`, `__Category`, `__HideByDefault`, `__HiddenWires`); `Plugin`/`PluginServer` own registration, and `IoIdAttribute` stamps the persistent serialization id

[STACKING]:

- `api-thinktecture-runtime-extensions`(`.api/api-thinktecture-runtime-extensions.md`): the host `Access` (`Item`/`Twig`/`Tree`), `Requirement` (`MustExist`/`MayBeNull`/`MayBeMissing`), and `Side` (`Input`/`Output`) fold onto `[SmartEnum]` owners, so a pin depth, presence, or side is one exhaustive dispatch value; the typed `Add*` pin roster is generated from one `[SmartEnum]` pin-kind vocabulary rather than an enumerated method wall
- `api-languageext`(`.api/api-languageext.md`): every out-parameter `Get*(int, out T)` lifts onto `Fin<T>` (a missing/null pin resolving through `Requirement` onto `Option<T>` or an accumulating `Validation<Error, T>`), so `Process` reads its inputs as a fan-in that reports every unsatisfied pin at once rather than a chain of `bool` checks; the `Garden` tree-wise folds and `Twig<T>.Convert` compose `Seq`/`Traverse` so a `Tree<Fin<A>>` inverts to `Fin<Tree<A>>`
- `api-languageext`(`.api/api-languageext.md`): `ConversionServer.Convert(object, Type, out object, out Merit, out string)` and the discriminated broker folds lift onto a `Fin` carrying the `Merit` or family receipt, so a conversion refusal is a typed `Error`; the public `PluginServer.LoadPlugin` overloads lift their `bool` plus `out FailureInfo` contract to `Fin<Unit>`
- `api-generator-equals`(`.api/api-generator-equals.md`): pin identity and the `IoIdAttribute` type-id key take generated structural equality, so a persistent-value or pin-descriptor compare is one generated equality, never a hand-written `Equals`

[LOCAL_ADMISSION]:

- `Component`/`ModularComponent` is the one authoring base the folder extends; a GH1 `GH_Component`/`GH_ComponentAttributes` re-derivation beside it is the rejected form
- pin declaration composes the adder families through the generated pin-kind vocabulary; a hand-enumerated `Add*` wall or a `GH_ParamAccess` bool is never re-minted
- `IDataAccess` is the sole in-`Process` seam; reaching the `Document` or `Solution` graph from inside compute is the rejected form
- `Garden` and the brokers own tree construction and conversion; a hand-rolled branch walker or a `GH_Structure` re-implementation beside them is the deleted form

[RAIL_LAW]:

- Package: `Grasshopper2.dll` (Rhino 9 WIP Grasshopper2 SDK, in-process managed plug-in; `GrasshopperIO` serialization; `Rhino.Geometry` carriers)
- Owns: the `Component`/`ModularComponent` authoring model, `IDataAccess`, the typed pin adder families, the `Tree`/`Twig`/`Pear`/`Garden` data algebra, the `Grasshopper2.Parameters.Standard` and `ConversionServer` brokers, and `Plugin`/`PluginServer` registration
- Accept: a `Component` declaring pins through the adder families, computing through `Process(IDataAccess)` with reads lifted onto `Fin`/`Validation`, `Access`/`Requirement`/`Side` folded onto `[SmartEnum]`s, trees built and folded through `Garden`, conversions carrying a `Merit` receipt, and plugin registration through `PluginServer`
- Reject: a GH1 `GH_Component`/`SolveInstance`/`IGH_DataAccess`/`GH_Structure`/`GH_ParamAccess` shape; a hand-enumerated `Add*` pin wall; a `bool`+out read where `Fin`/`Option` gives the typed rail; a hand-rolled tree walker beside `Garden`; a `null`-out conversion beside the `Merit`-scored broker fold
