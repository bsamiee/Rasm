# [RASM_GRASSHOPPER_API_GH2_COMPONENTS]

`Grasshopper2.Components` is the GH2 component-authoring model — the `Component`/`ModularComponent` document object whose lifecycle runs construction, pin registration, `Process(IDataAccess)`, and variable-parameter mutation, with `IDataAccess` as the single item/pear/twig/tree access seam into the running solution. Typed pins register through the `InputAdder`/`OutputAdder` families (their `ModularInputAdder`/`ModularOutputAdder` variants adding label, colour, and hidden-pin state); the `Garden` tree algebra and `Grasshopper2.Types.Conversion` brokers own tree construction, conversion, and cast-or-convert; and `Grasshopper2.Framework` `Plugin`/`PluginServer` own registration and object-to-plugin resolution. `IDataAccess`, `InputAdder`, `OutputAdder`, and `ComponentParameters` live under `Grasshopper2.Components`; the geometry brokers under `Grasshopper2.Parameters.Standard`; `Solution` and `FleetingCustomData` under `Grasshopper2.Doc`; and `Connections` under `Grasshopper2.Parameters`. The GH1 `GH_Component`/`GH_ParamAccess`/`GH_Structure`/`IGH_DataAccess`/`SolveInstance` roster is the anti-shape this model replaces.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grasshopper2` (Rhino 9 WIP Grasshopper2 SDK)
- assembly: `Grasshopper2.dll` (installed `Grasshopper2Plugin.rhp` managed plug-in; in-process, no NuGet redistribution)
- namespace: `Grasshopper2.Components`, `.Components.Standard`, `Grasshopper2.Parameters`, `.Parameters.Standard`, `Grasshopper2.Data`, `.Data.Meta`, `Grasshopper2.Types.Conversion`, `Grasshopper2.Framework`, `GrasshopperIO`
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
|  [06]   | `PluginServer`        | registrar         | assembly harvest, plugin load, and object-to-plugin resolution                   |
|  [07]   | `IoIdAttribute`       | io identity       | the persistent type-id attribute (`GrasshopperIO`)                               |

[PUBLIC_TYPE_SCOPE]: data access and pin registration
- rail: component-authoring model
- note: `IDataAccess` is the ONE seam `Process` receives — typed get/set over item/pear/twig/tree plus tolerance, unit, transform, and message emission; the adder families are the typed pin-declaration surface split by direction.

| [INDEX] | [SYMBOL]                                   | [KIND]        | [CAPABILITY]                                                                      |
| :-----: | :----------------------------------------- | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `IDataAccess`                              | access seam   | typed get/set, tolerance/unit/transform reads, remark/warning/error emission      |
|  [02]   | `InputAdder`                               | pin adder     | typed input declaration with `Access` + `Requirement`                             |
|  [03]   | `OutputAdder`                              | pin adder     | typed output declaration with `Access`                                            |
|  [04]   | `ModularInputAdder` / `ModularOutputAdder` | modular adder | adder plus label, colour, and hidden-pin variants                                 |
|  [05]   | `IParameter`                               | pin contract  | `Access`, `Requirement`, `PersistentDataWeak`, `TypeAssistantWeak`, `PresetsWeak` |
|  [06]   | `Access`                                   | smart enum    | `Item` / `Twig` / `Tree` — the pin data-depth discriminant                        |
|  [07]   | `Requirement`                              | smart enum    | `MustExist` / `MayBeNull` / `MayBeMissing` — the pin presence contract            |

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

| [INDEX] | [SURFACE]                                                                                                                | [CALL_SHAPE] | [CAPABILITY]                                             |
| :-----: | :----------------------------------------------------------------------------------------------------------------------- | :----------- | :------------------------------------------------------- |
|  [01]   | `Component(Nomen nomen)` / `AddInputs(InputAdder)` / `AddOutputs(OutputAdder)`                                           | author       | construct and declare the pin surface                    |
|  [02]   | `Process(IDataAccess access)`                                                                                            | compute      | the per-solution compute hook                            |
|  [03]   | `BeforeProcess(Solution)` / `PreProcess(Solution)` / `PostProcess(Solution, FleetingCustomData)`                         | lifecycle    | the solution-scoped brackets around `Process`            |
|  [04]   | `PostProcessTree(ITree, int, Solution)` / `ComputeInternal(Solution, CallStack)`                                         | lifecycle    | per-output tree finalize and the internal compute driver |
|  [05]   | `Parameters` / `Connectivity` / `ConnectivityComplete` / `Threading`                                                     | state        | live pin list, wire connectivity, and threading policy   |
|  [06]   | `SupportsVariableParameters` / `CanCreateParameter(Side, int)` / `CanRemoveParameter(Side, int)`                         | gate         | variable-pin admissibility per side and index            |
|  [07]   | `DoCreateParameter(Side, int, ActionList)` / `DoRemoveParameter(Side, int, ActionList)` / `VariableParameterMaintenance` | mutate       | apply and reconcile a variable-pin change                |
|  [08]   | `BakeCapable` / `BakeShapes(BakeContext, BakeUpdateMode)` / `CreateAttributes`                                           | bake / view  | bake gate, bake emission, and attribute construction     |

[ENTRYPOINT_SCOPE]: data access get, set, and diagnostics
- rail: component-authoring model
- note: every typed getter is an out-parameter `bool`-shaped read the folder lifts onto the result rail; `Set*` writes a pin's tree, and `Add*` emits a document message with optional `MessageAction`s.

| [INDEX] | [SURFACE]                                                                                                                                       | [CALL_SHAPE] | [CAPABILITY]                                           |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------------- | :----------- | :----------------------------------------------------- |
|  [01]   | `GetItem<T>(int, out T)` / `GetPear<T>(int, out Pear<T>)` / `GetTwig<T>(int, out Twig<T>)` / `GetTree<T>(int, out Tree<T>)`                     | typed get    | pin read at each data depth                            |
|  [02]   | `GetIPear(int, out IPear)` / `GetITwig(int, out ITwig)` / `GetITree(int, out ITree)`                                                            | weak get     | untyped goo read                                       |
|  [03]   | `GetTolerance(out double, out double)` / `GetTolerance(out Angle)` / `GetUnitSystem(out UnitSystem)` / `GetUnitScaling(UnitSystem, out double)` | context      | model tolerance and unit context                       |
|  [04]   | `GetTransform(int, out Transform)` / `GetQuaternion(int, out Quaternion)`                                                                       | typed get    | transform and quaternion pin reads                     |
|  [05]   | `SetItem(int, object, MetaData)` / `SetPear(int, IPear)` / `SetTwig(int, ITwig)` / `SetTree(int, ITree)`                                        | set          | write an output pin at each depth                      |
|  [06]   | `AddRemark(string, string, MessageAction[])` / `AddWarning(...)` / `AddError(...)` / `SetProgress(int)`                                         | diagnostics  | document message emission and progress                 |
|  [07]   | `Solution` / `Callstack`                                                                                                                        | state        | the running solution and call stack the access exposes |

[ENTRYPOINT_SCOPE]: pin declaration, tree construction, and conversion
- rail: component-authoring model
- note: the adder `Add*` roster is one typed method per carrier plus `AddGeneric`/`AddEnum<T>`/`AddTopological`; `Garden` builds and folds trees; the brokers resolve a host object onto its concrete geometry family.

| [INDEX] | [SURFACE]                                                                                                                                                                             | [CALL_SHAPE] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :----------- | :----------------------------------------------- |
|  [01]   | `InputAdder.AddGeneric(name, nick, info, Access, Requirement)` / `AddEnum<T>(..., T, Access, Requirement)` / `AddTopological(...)`                                                    | declare      | generic, enum, and topological input pins        |
|  [02]   | `InputAdder.AddPoint / AddVector / AddCurve / AddSurface / AddMesh / AddMetaData(name, nick, info, Access, Requirement)`                                                              | declare      | the typed geometry/meta input roster             |
|  [03]   | `OutputAdder.AddGeneric / AddPoint / AddVector / AddCurve / AddSurface / AddMesh / AddMetaData(name, nick, info, Access)`                                                             | declare      | the typed output roster                          |
|  [04]   | `ModularOutputAdder.AddHiddenCurve(name, nick, info, category, Color, Access)` / `AddHiddenSurface(...)`                                                                              | declare      | coloured, categorized, hidden output pins        |
|  [05]   | `Garden.TreeFromList<T>(items, metas, wraps)` / `TreeFromPears<T>` / `TreeFromLeaves<T>` / `TreeFromTwigs<T>`                                                                         | build        | tree construction from each source shape         |
|  [06]   | `Garden.PairWiseOp<A, B, R>(Tree<A>, Tree<B>, Func<A, B, R>, CancellationToken)` / `PearWiseOp<T>(Tree<T>, Func<Pear<T>, Pear<T>>, CancellationToken)`                                | fold         | tree-wise binary and unary operations            |
|  [07]   | `Twig<T>.Convert<U>(ConversionDelegate<T, U>, CancellationToken, ConversionRecord)` / `Apply(Expression, Resolver, out IExpressionReport)`                                            | transform    | branch conversion and expression evaluation      |
|  [08]   | `ConversionServer.Convert(object, Type, out object, out Merit, out string)` / `CurveBroker.CastOrConvert(...)` / `SurfaceBroker.CastOrConvert(...)`                                   | convert      | merit-scored and geometry-family cast-or-convert |
|  [09]   | `Plugin(Guid, Nomen, Version)` / `PluginServer.LoadPlugin(string, out FailureInfo)` / `HarvestPluginFromAssembly(Assembly, out FailureInfo)` / `FindPluginForObject(IDocumentObject)` | register     | plugin identity, load, harvest, and resolution   |

## [04]-[IMPLEMENTATION_LAW]

[COMPONENT_TOPOLOGY]:
- `Component` is the compute-carrying document object: it declares pins once through `AddInputs(InputAdder)`/`AddOutputs(OutputAdder)`, computes through `Process(IDataAccess)`, and reconciles variable pins through the `Can*`/`Do*Parameter(Side, int, ActionList)` pairs — `Side` is the `Input`/`Output` discriminant and every structural edit rides an `ActionList` for undo
- `IDataAccess` is the single execution seam: typed `Get*`/`Set*` over item/pear/twig/tree depth, tolerance and unit context, transform reads, and `AddRemark`/`AddWarning`/`AddError` message emission; the component never reaches the `Document` from inside `Process`, it reads `access.Solution`/`access.Callstack`
- pins are `IParameter`s declared with an `Access` (`Item`/`Twig`/`Tree`) and a `Requirement` (`MustExist`/`MayBeNull`/`MayBeMissing`); the adder families are the only pin-declaration surface, one typed `Add*` per carrier plus `AddGeneric`/`AddEnum<T>`/`AddTopological`, and the modular adders extend each with label, colour, category, and hidden-pin state
- data is `Tree<T>` of `Twig<T>` of `Pear<T>` (`Leaf<T>` the typed pear): `Garden` builds trees from every source shape and folds them tree-wise (`PairWiseOp`/`PearWiseOp`), `Twig<T>.Convert`/`Apply` transform a branch, and the `Grasshopper2.Parameters.Standard` brokers plus `ConversionServer` resolve a host object onto its concrete family carrying a `Merit` score
- `ModularComponent` drives its pin surface from `__`-prefixed well-known custom-value keys (`__Icon`, `__Colour`, `__Optional`, `__Category`, `__HideByDefault`, `__HiddenWires`); `Plugin`/`PluginServer` own registration, and `IoIdAttribute` stamps the persistent serialization id

[STACKING]:
- `api-thinktecture-runtime-extensions`(`.api/api-thinktecture-runtime-extensions.md`): the host `Access` (`Item`/`Twig`/`Tree`), `Requirement` (`MustExist`/`MayBeNull`/`MayBeMissing`), and `Side` (`Input`/`Output`) fold onto `[SmartEnum]` owners, so a pin depth, presence, or side is one exhaustive dispatch value; the typed `Add*` pin roster is generated from one `[SmartEnum]` pin-kind vocabulary rather than an enumerated method wall
- `api-languageext`(`.api/api-languageext.md`): every out-parameter `Get*(int, out T)` lifts onto `Fin<T>` (a missing/null pin resolving through `Requirement` onto `Option<T>` or an accumulating `Validation<Error, T>`), so `Process` reads its inputs as a fan-in that reports every unsatisfied pin at once rather than a chain of `bool` checks; the `Garden` tree-wise folds and `Twig<T>.Convert` compose `Seq`/`Traverse` so a `Tree<Fin<A>>` inverts to `Fin<Tree<A>>`
- `api-languageext`(`.api/api-languageext.md`): `ConversionServer.Convert(object, Type, out object, out Merit, out string)` and the broker `CastOrConvert` folds lift onto a `Fin` carrying the `Merit` receipt, so a cast failure is a typed `Error`, never a `null` out-parameter; `PluginServer.LoadPlugin`/`HarvestPluginFromAssembly` `out FailureInfo` lifts to `Fin<Plugin>`
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
