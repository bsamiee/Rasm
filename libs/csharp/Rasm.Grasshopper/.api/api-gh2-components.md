# [RASM_GRASSHOPPER_API_GH2_COMPONENTS]

`Grasshopper2.Components` owns the GH2 component-authoring model — the `Component`/`ModularComponent` document object whose lifecycle runs pin registration, per-access and iteration-array processing, and variable-parameter mutation, with `IDataAccess` the sole item/pear/twig/tree seam into the running solution. Typed pins register through the `InputAdder`/`OutputAdder` families; `Garden` and the `Grasshopper2.Types.Conversion` brokers own tree construction and conversion, and `Plugin`/`PluginServer` own registration.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grasshopper2` 'Rhino 9 WIP Grasshopper2 SDK'
- assembly: `Grasshopper2.dll` (installed `Grasshopper2Plugin.rhp` managed plug-in; in-process, no NuGet redistribution)
- namespace: `Grasshopper2.Components`, `.Components.Standard`, `Grasshopper2.Parameters`, `.Parameters.Standard`, `Grasshopper2.Data`, `.Data.Meta`, `Grasshopper2.Doc`, `Grasshopper2.Types.Assistant`, `.Types.Conversion`, `Grasshopper2.Framework`, `GrasshopperIO`
- host: RhinoWIP `RhCore.framework` — `Rhino.Geometry` supplies the carrier types component pins bind
- io: `GrasshopperIO` `IoIdAttribute` stamps the persistent type id every document object serializes under
- rail: component-authoring model

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: component authoring and lifecycle

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [CAPABILITY]                                                                     |
| :-----: | :-------------------- | :---------------- | :------------------------------------------------------------------------------- |
|  [01]   | `Component`           | document object   | pin registration, `Process`, bake, threading, variable-parameter lifecycle       |
|  [02]   | `ModularComponent`    | modular component | `ModularInputs`/`ModularOutputs` plus icon/colour/category/hidden attribute keys |
|  [03]   | `ComponentParameters` | pin list          | live `Inputs`/`Outputs`, add/remove, and auto-maintenance of variable pins       |
|  [04]   | `Side`                | enum              | `Input` / `Output` — the pin-side discriminant every mutation carries            |
|  [05]   | `Plugin`              | plugin root       | author/version identity, `SatelliteAssemblies`, `ExportedTypes`, `OnLoaded`      |
|  [06]   | `PluginServer`        | registrar         | location/assembly plugin loading and object-to-plugin resolution                 |
|  [07]   | `IoIdAttribute`       | io identity       | the persistent type-id attribute (`GrasshopperIO`)                               |

[PUBLIC_TYPE_SCOPE]: data access and pin registration

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `IDataAccess`        | access seam   | typed data and host-context reads, output writes, messages, and progress |
|  [02]   | `InputAdder`         | pin adder     | typed input declaration with `Access` and `Requirement`                  |
|  [03]   | `OutputAdder`        | pin adder     | typed output declaration with `Access`                                   |
|  [04]   | `ModularInputAdder`  | modular adder | input declaration plus label, colour, category, and hidden state         |
|  [05]   | `ModularOutputAdder` | modular adder | output declaration plus label, colour, category, and hidden state        |
|  [06]   | `IParameter`         | pin contract  | access/presence plus writable persistent data and read-only services     |
|  [07]   | `Access`             | enum          | `Item` / `Twig` / `Tree` — the pin data-depth discriminant               |
|  [08]   | `Requirement`        | enum          | `MustExist` / `MayBeNull` / `MayBeMissing` — the pin presence contract   |
|  [09]   | `ITypeAssistant`     | type service  | read, parse, display, geometry, transform, measure, and bake projection  |

[PUBLIC_TYPE_SCOPE]: tree data algebra and conversion

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]   | [CAPABILITY]                                                            |
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

| [INDEX] | [SURFACE]                                                   | [SHAPE]   | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------- | :-------- | :--------------------------------- |
|  [01]   | `Component(Nomen)`                                          | author    | construct the component            |
|  [02]   | `AddInputs(InputAdder)` / `AddOutputs(OutputAdder)`         | author    | declare the fixed pin surface      |
|  [03]   | `Process(IDataAccess)`                                      | compute   | compute one access iteration       |
|  [04]   | `Process(IDataAccess[], CancellationToken)`                 | compute   | dispatch the iteration array       |
|  [05]   | `BeforeProcess(Solution)` / `PreProcess(Solution)`          | lifecycle | open the solution-scoped process   |
|  [06]   | `PostProcess(Solution, FleetingCustomData)`                 | lifecycle | close the solution-scoped process  |
|  [07]   | `PostProcessTree(ITree, int, Solution)`                     | lifecycle | finalize one output tree           |
|  [08]   | `ComputeInternal(Solution, CallStack)`                      | lifecycle | drive internal computation         |
|  [09]   | `Parameters` / `Connectivity` / `ConnectivityComplete`      | state     | expose pins and wire connectivity  |
|  [10]   | `Threading`                                                 | state     | select component processing policy |
|  [11]   | `SupportsVariableParameters`                                | gate      | expose variable-pin capability     |
|  [12]   | `CanCreateParameter(Side, int)` / `CanRemoveParameter(...)` | gate      | admit a variable-pin change        |
|  [13]   | `DoCreateParameter(Side, int, ActionList)`                  | mutate    | create a pin with undo             |
|  [14]   | `DoRemoveParameter(Side, int, ActionList)`                  | mutate    | remove a pin with undo             |
|  [15]   | `VariableParameterMaintenance`                              | mutate    | reconcile the changed pin surface  |
|  [16]   | `BakeCapable` / `BakeShapes(BakeContext, BakeUpdateMode)`   | bake      | gate and emit baked shapes         |
|  [17]   | `CreateAttributes`                                          | view      | construct object attributes        |

[ENTRYPOINT_SCOPE]: data access get, set, and diagnostics

| [INDEX] | [SURFACE]                                                      | [SHAPE]      | [CAPABILITY]                               |
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

- `IDataAccess.Get*`: each returns `bool`, the out-value binding only when the read succeeds.

[ENTRYPOINT_SCOPE]: pin declaration, tree construction, and conversion

| [INDEX] | [SURFACE]                                              | [SHAPE]   | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------- | :-------- | :----------------------------------------------- |
|  [01]   | `InputAdder.AddGeneric / AddEnum<T> / AddTopological`  | declare   | generic, enum, and topological input pins        |
|  [02]   | `InputAdder.AddPoint / AddVector / AddCurve`           | declare   | point, vector, and curve input pins              |
|  [03]   | `InputAdder.AddSurface / AddMesh / AddMetaData`        | declare   | surface, mesh, and metadata input pins           |
|  [04]   | `OutputAdder.AddGeneric / AddPoint / AddVector`        | declare   | generic, point, and vector output pins           |
|  [05]   | `OutputAdder.AddCurve / AddSurface / AddMesh`          | declare   | curve, surface, and mesh output pins             |
|  [06]   | `OutputAdder.AddMetaData`                              | declare   | metadata output pins                             |
|  [07]   | `ModularOutputAdder.AddHiddenCurve / AddHiddenSurface` | declare   | categorized hidden geometry outputs              |
|  [08]   | `Garden.TreeFromList<T> / TreeFromPears<T>`            | build     | tree construction from values or pears           |
|  [09]   | `Garden.TreeFromLeaves<T> / TreeFromTwigs<T>`          | build     | tree construction from leaves or twigs           |
|  [10]   | `Garden.PairWiseOp<A, B, R>`                           | fold      | typed binary tree operation                      |
|  [11]   | `Garden.PearWiseOp<T>`                                 | fold      | typed unary pear operation                       |
|  [12]   | `Twig<T>.Convert<U>`                                   | transform | branch conversion                                |
|  [13]   | `Twig<T>.Apply`                                        | transform | branch expression evaluation                     |
|  [14]   | `ConversionServer.Convert`                             | convert   | merit-scored target-type conversion              |
|  [15]   | `CurveBroker.CastOrConvert`                            | convert   | curve-family conversion with `CurveType` result  |
|  [16]   | `SurfaceBroker.CastOrConvert`                          | convert   | surface conversion with `SurfaceLikeType` result |
|  [17]   | `PluginServer.LoadPlugin(string, out ...)`             | register  | public location loading                          |
|  [18]   | `PluginServer.LoadPlugin(string, Assembly, ...)`       | register  | public assembly loading                          |
|  [19]   | `PluginServer.FindPluginForObject`                     | resolve   | object or type to loaded-plugin resolution       |

- `PluginServer.LoadPlugin`: assembly harvesting stays internal to the load.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Component` declares pins once through `AddInputs(InputAdder)`/`AddOutputs(OutputAdder)`, computes through `Process(IDataAccess)`, overrides `Process(IDataAccess[], CancellationToken)` for iteration-array policy, and reconciles variable pins through the `Can*`/`Do*Parameter(Side, int, ActionList)` pairs — every structural edit rides an `ActionList` for undo.
- `IDataAccess` is the sole in-`Process` seam: typed `Get*`/`Set*` over item/pear/twig/tree depth, tolerance and unit context, transform reads, and `AddRemark`/`AddWarning`/`AddError` messages; `Process` reads `access.Solution`/`access.Callstack`, never the `Document`.
- pins are `IParameter`s carrying an `Access` (`Item`/`Twig`/`Tree`) and a `Requirement` (`MustExist`/`MayBeNull`/`MayBeMissing`); the adder families are the one pin-declaration surface, and the modular adders extend each with label, colour, category, and hidden state.
- data is `Tree<T>` of `Twig<T>` of `Pear<T>`: `Garden` builds and folds trees (`PairWiseOp`/`PearWiseOp`), `Twig<T>.Convert`/`Apply` transform a branch, and the `Grasshopper2.Parameters.Standard` brokers and `ConversionServer` resolve a host object onto its concrete family carrying a `Merit` score.
- `ModularComponent` drives its pin surface from `__`-prefixed well-known keys (`__Icon`, `__Colour`, `__Optional`, `__Category`, `__HideByDefault`, `__HiddenWires`); `Plugin`/`PluginServer` own registration and `IoIdAttribute` stamps the persistent serialization id.

[STACKING]:
- `api-thinktecture-runtime-extensions`(`.api/api-thinktecture-runtime-extensions.md`): the host `Access`, `Requirement`, and `Side` enums fold onto `[SmartEnum]` owners, so a pin depth, presence, or side is one exhaustive dispatch value, and the typed `Add*` roster generates from one `[SmartEnum]` pin-kind vocabulary rather than an enumerated method wall.
- `api-languageext`(`.api/api-languageext.md`): every `Get*(int, out T)` lifts onto `Fin<T>` — a missing or null pin resolving through `Requirement` onto `Option<T>` or an accumulating `Validation<Error, T>` — so `Process` reads its inputs as a fan-in reporting every unsatisfied pin at once; the `Garden` folds and `Twig<T>.Convert` compose `Seq`/`Traverse` so a `Tree<Fin<A>>` inverts to `Fin<Tree<A>>`.
- `api-languageext`(`.api/api-languageext.md`): `ConversionServer.Convert(object, Type, out object, out Merit, out string)` and the discriminated broker folds lift onto a `Fin` carrying the `Merit` or family receipt, so a conversion refusal is a typed `Error`; the `PluginServer.LoadPlugin` overloads lift their `bool` and `out FailureInfo` to `Fin<Unit>`.
- `api-generator-equals`(`.api/api-generator-equals.md`): pin identity and the `IoIdAttribute` type-id key take generated structural equality, so a persistent-value or pin-descriptor compare is one generated equality.

[LOCAL_ADMISSION]:
- `Component`/`ModularComponent` is the one authoring base the folder extends.
- pin declaration composes the adder families through the generated pin-kind vocabulary.
- `IDataAccess` is the sole in-`Process` seam into data, context, and messages.
- `Garden` and the brokers own tree construction and conversion.

[RAIL_LAW]:
- Package: `Grasshopper2.dll` (Rhino 9 WIP Grasshopper2 SDK, in-process managed plug-in; `GrasshopperIO` serialization; `Rhino.Geometry` carriers)
- Owns: the `Component`/`ModularComponent` authoring model, `IDataAccess`, the typed pin adder families, the `Tree`/`Twig`/`Pear`/`Garden` data algebra, the `Grasshopper2.Parameters.Standard` and `ConversionServer` brokers, and `Plugin`/`PluginServer` registration
- Accept: a `Component` declaring pins through the adder families, computing through `Process(IDataAccess)` with reads lifted onto `Fin`/`Validation`, `Access`/`Requirement`/`Side` folded onto `[SmartEnum]`s, trees built and folded through `Garden`, conversions carrying a `Merit` receipt, and registration through `PluginServer`
- Reject: a GH1 `GH_Component`/`SolveInstance`/`IGH_DataAccess`/`GH_Structure`/`GH_ParamAccess` shape; a hand-enumerated `Add*` pin wall; a `bool`+out read where `Fin`/`Option` gives the typed rail; a hand-rolled tree walker beside `Garden`; a `null`-out conversion beside the `Merit`-scored broker fold
