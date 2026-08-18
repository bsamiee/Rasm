# [RASM_GRASSHOPPER_API_GH2_COMPONENTS]

`Grasshopper2.Components` owns the GH2 component-authoring model — the `Component`/`ModularComponent` document object whose lifecycle runs pin registration, per-access and iteration-array processing, and variable-parameter mutation, with `IDataAccess` the sole item/pear/twig/tree seam into the running solution. Typed pins register through the `InputAdder`/`OutputAdder` families; `Garden` and the `Grasshopper2.Types.Conversion` brokers own tree construction and conversion, and `Plugin`/`PluginServer` own registration.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grasshopper2` 'Rhino 9 WIP Grasshopper2 SDK'
- assembly: `Grasshopper2.dll` (installed `Grasshopper2Plugin.rhp` managed plug-in; in-process, no NuGet redistribution)
- namespace: `Grasshopper2.Components`, `.Components.Standard`, `Grasshopper2.Parameters`, `.Parameters.Standard`, `Grasshopper2.Data`, `.Data.Meta`, `Grasshopper2.Doc`, `.Doc.Attributes`, `Grasshopper2.Types.Assistant`, `.Types.Conversion`, `Grasshopper2.Framework`, `Grasshopper2.Bake`, `GrasshopperIO`
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

- `Component` declares `public abstract class Component : ActiveObject, IBakeAware, IGuidAware`, so an authoring subclass inherits the `ActiveObject` document-object lifecycle and satisfies the bake and persistent-identity contracts without restating either.

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

[PUBLIC_TYPE_SCOPE]: component-attribute bases (`Grasshopper2.Doc`, `Grasshopper2.Doc.Attributes`)

`IAttributes` contract itself is `api-gh2-document.md`'s; this partition holds the concrete bases a component author subclasses and states their edge against that contract.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]  | [CAPABILITY]                                                             |
| :-----: | :---------------------------------------- | :------------- | :----------------------------------------------------------------------- |
|  [01]   | `Grasshopper2.Doc.Attributes<T>`          | abstract class | pivot, bounds, hit tests, tooltip, and the two-stage draw spine          |
|  [02]   | `Doc.Attributes.ComponentAttributes`      | class          | component layout boxes, ZUI grips, tentative pins, foreground decoration |
|  [03]   | `Doc.Attributes.ResizableAttributes<T>`   | abstract class | persisted size, edge grab, snapping, resize undo, explicit edge cursor   |
|  [04]   | `Doc.Attributes.IResizableAttributes`     | interface      | the `Size` contract the resize responder and the undo action read        |
|  [05]   | `Grasshopper2.Doc.ICursorAwareAttributes` | interface      | `Cursor CursorAt(PointF)` hover feedback                                 |

[PUBLIC_TYPE_SCOPE]: writable parameter modifiers (`Grasshopper2.Parameters.Standard`)

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :---------------------------------------------- | :------------ | :------------------------------------------------ |
|  [01]   | `VectorParameter` / `AngleParameter`            | parameter     | unitise/reverse, enforcement ordinal and reduce   |
|  [02]   | `BooleanParameter` / `ConnectionParameter`      | parameter     | negation and null policy, connection collection   |
|  [03]   | `IntegerParameter` / `NumberParameter`          | parameter     | index policy plus `UiInteger`/`UiNumber` hints    |
|  [04]   | `NumericParameter` / `CurveParameter`           | parameter     | exotic filtering, domain normalisation and flip   |
|  [05]   | `SurfaceParameter` / `TextParameter`            | parameter     | mesh admission, flavour, extensions, casing       |
|  [06]   | `TextPatternParameter` / `LanguageParameter`    | parameter     | pattern kind and case, culture-code carrier       |
|  [07]   | `IndexModifier` / `NumericFilter`               | enum          | integer indexing policy, exotic numeric admission |
|  [08]   | `CurveParameter.NormalisationMethod`            | nested enum   | the domain rule surfaces and curves both read     |
|  [09]   | `TextParameter.CasingBehaviour` / `TextFlavour` | enum          | casing projection and string-versus-file flavour  |
|  [10]   | `TextPatternKind`                               | enum          | the pattern dialect a text-pattern pin matches    |

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

| [INDEX] |                          [SURFACE]                          |  [SHAPE]  |                  [CAPABILITY]                 |
| :-----: | :---------------------------------------------------------- | :-------- | :-------------------------------------------- |
|   [01]  | `Component(Nomen)`                                          | author    | construct the component                       |
|   [02]  | `AddInputs(InputAdder)` / `AddOutputs(OutputAdder)`         | author    | declare the fixed pin surface                 |
|   [03]  | `Process(IDataAccess)`                                      | compute   | compute one access iteration                  |
|   [04]  | `Process(IDataAccess[], CancellationToken)`                 | compute   | dispatch the iteration array                  |
|   [05]  | `BeforeProcess(Solution)` / `PreProcess(Solution)`          | lifecycle | open the solution-scoped process              |
|   [06]  | `PostProcess(Solution, FleetingCustomData)`                 | lifecycle | close the solution-scoped process             |
|   [07]  | `PostProcessTree(ITree, int, Solution)`                     | lifecycle | finalize one output tree                      |
|   [08]  | `ComputeInternal(Solution, CallStack)`                      | lifecycle | drive internal computation                    |
|   [09]  | `Parameters`                                                | state     | expose the component's pin roster             |
|   [10]  | `Threading`                                                 | state     | select the `ThreadingState` processing policy |
|   [11]  | `SupportsVariableParameters`                                | gate      | expose variable-pin capability                |
|   [12]  | `CanCreateParameter(Side, int)` / `CanRemoveParameter(...)` | gate      | admit a variable-pin change                   |
|   [13]  | `DoCreateParameter(Side, int, ActionList)`                  | mutate    | create a pin with undo                        |
|   [14]  | `DoRemoveParameter(Side, int, ActionList)`                  | mutate    | remove a pin with undo                        |
|   [15]  | `VariableParameterMaintenance`                              | mutate    | reconcile the changed pin surface             |
|   [16]  | `BakeCapable`                                               | bake      | virtual bakeability gate                      |
|   [17]  | `BakeShapes(BakeContext, BakeUpdateMode) -> string[]`       | bake      | non-virtual call, baked-id roster             |
|   [18]  | `CreateAttributes`                                          | view      | construct object attributes                   |

[ENTRYPOINT_SCOPE]: document emission (`Grasshopper2.Bake`)

`Component : IBakeAware` supplies both halves: `BakeCapable` is `virtual` and overridable, `BakeShapes` is NOT — it is a call site whose body folds every output `IBakeAware` parameter, so a declaration-driven gate overrides the first and composes the second, never the reverse.

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `IBakeAware.BakeCapable -> bool` / `BakeShapes(BakeContext, BakeUpdateMode)` | contract | the two-member bake floor          |
|  [02]   | `BakeContext(string, Guid, RhinoDoc, ObjectAttributes?, UserPattern, …)`     | ctor     | bake into a live document          |
|  [03]   | `BakeContext(string, Guid, File3dm, ObjectAttributes?, UserPattern, …)`      | ctor     | bake into a `.3dm` file            |
|  [04]   | `BakeContext.WithProcess(string, Guid) -> BakeContext`                       | instance | re-key the process identity        |
|  [05]   | `BakeContext.BakeObject(IPear) -> string[]`                                  | instance | bake one value, returns ids        |
|  [06]   | `BakeContext.BakeTree(ITree, BakeUpdateMode) -> string[]`                    | instance | bake a whole tree, returns ids     |
|  [07]   | `BakeContext.BakeIdentifiers -> string[]`                                    | property | the accumulated identity roster    |
|  [08]   | `BakeContext.SpecificAttributes(IPear) -> ObjectAttributes`                  | instance | per-value attribute resolution     |
|  [09]   | `BakeContext.EnsureLayers(ITree, RhinoDoc \| File3dm, …)`                    | static   | pre-create the layers a tree names |
|  [10]   | `BakeContext.FindBakedObjects(RhinoDoc \| File3dm, BakeDataState)`           | static   | re-find prior bakes by key         |
|  [11]   | `BakeKey(string, Guid, int twig, int item, int part)`                        | ctor     | the per-value bake coordinate      |
|  [12]   | `BakeKey.With{Guid,Indices,Twig,Item,Part}`                                  | instance | non-destructive coordinate edits   |
|  [13]   | `BakeKey.AssignToObject(RhinoObject \| File3dmObject, bool, bool)`           | instance | stamp the key onto a baked object  |
|  [14]   | `UserPattern` / `MetaPattern`                                                | struct   | attribute defaults and overrides   |

- `BakeContext` targets exactly one sink — `TargetDocument` or `TargetFile3dm`, the other null — so live-document and file bakes are one context shape discriminated by which target the ctor filled, never two emitters; `ProcessName`/`ProcessGuid` name the run and `WithProcess` re-keys it.
- `BakeKey` is the `(process, twig, item, part)` coordinate that makes a bake re-findable: its `G2ObjGuid`/`G2ObjHash`/`G2ProcName`/`G2ProcGuid`/`G2Twig`/`G2Item`/`G2Part` user-string names are the stamped fields, and `BakeUpdateMode.Update` matches on them where `Add` never does.
- `BakeDataState` is `[Flags]` — `None=0`, `Invalid=1`, `Valid=2`, `Expired=4`, `Divorced=8` — so `FindBakedObjects` filters prior bakes by trust: `Divorced` marks data copied onto a different object and `Expired` marks a still-stamped object whose source run is gone.
- `UserPattern` carries the caller's defaults (`Mode`, `Group`, `Name`, `Layer`, `Colour`, `LineType`, `PlotColour`, `PlotWeight`, `SectionHatch`, `SectionAngle`, `SectionScale`) and `MetaPattern` the per-axis opt-in deciding which of those a value's own metadata may override (`Embed`, `UseMode`, `UseName`, `ProcessName`, `UseLayer`, `UseColour`, `UseWireDensity`, `UseLineType`, `UsePlotColour`, `UseSectionHatch`, with `AllSet`/`SomeSet` census) — two structs, one attribute resolution, and both `IStorable` so a bake profile persists with the document.

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
- `IDataAccess` context and iteration columns: `Index`, `Iterations`, `CustomData`, `Callstack`, `Solution`, `CountIn`/`CountOut`, `CoverageIn`/`CoverageOut`, `NameIn`/`NameOut`, `AccessIn`/`AccessOut`, `HasInputChanged(int)`, `GetNull(int)`/`GetNullArray`, `GetMeta(int)`/`GetMetaArray`, `GetIndex`/`GetIndices`/`GetIndexing`.
- `IDataAccess` dedicated typed reads own their conversion: `GetTransform(int, out Transform)` and `GetQuaternion(int, out Quaternion)` sit beside the generic path, and the `Rectify*` (`Domain`/`Enum`/`LessThan`/`LessThanOrEqualTo`/`NonNegative`/`Positive`) and `Verify*` (assistant, domain, twig-count, coincidence, colinearity, parallelism, zero/unit-vector) families gate reads without a hand-rolled guard ladder.
- `Connectivity`/`ConnectivityComplete` exist on no public `Component` surface and `ComputeInternal(Solution, CallStack)` is a nonpublic virtual — host plumbing, never an override seam.
- Adder rosters run wide: `InputAdder` 55 `Add*` members, `OutputAdder` 54, `ModularInputAdder` 94 (typed + `AddHidden*` twins), `ModularOutputAdder` 96; `ModularList.Show(int[, ActionList])`/`Hide(int[, ActionList])` drive modular visibility.

[ENTRYPOINT_SCOPE]: component-attribute overrides and resize state (`Grasshopper2.Doc`, `Grasshopper2.Doc.Attributes`)

| [INDEX] | [SURFACE]                                                                           | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `Attributes<T>.{Pivot, Bounds, AggregateBounds, Owner, Snappable}`                  | property | placement, extent, and owner state   |
|  [02]   | `Attributes<T>.IsCoincident(PointF)` / `IsContained` / `Intersects(RectangleF)`     | virtual  | hit, containment, and marquee tests  |
|  [03]   | `Attributes<T>.ShowTooltipAt(PointF)` / `HandleDoubleClick(PointF)`                 | virtual  | tooltip and double-click verdicts    |
|  [04]   | `Attributes<T>.PivotMoved(PointF, PointF)` / `Move(float, float)`                   | virtual  | relocation hook and mutation         |
|  [05]   | `Attributes<T>.Layout(Shape)` / `InvalidateLayout()`                                | abstract | required on every concrete base      |
|  [06]   | `Attributes<T>.Draw(Context, Skin)` / `protected Draw(Context, Skin, Capsule)`      | virtual  | outer pin-aware + inner capsule pass |
|  [07]   | `ComponentAttributes(Component owner)` / `{Central, Label, Content}Box`             | ctor     | construction + three layout boxes    |
|  [08]   | `ComponentAttributes.Responder -> Responses`                                        | property | private nested responder, hook point |
|  [09]   | `ComponentAttributes.Layout{Bounds, CentralBox, InputParameters, OutputParameters}` | virtual  | the four layout stages               |
|  [10]   | `ComponentAttributes.Draw(Context, Skin, Capsule)`                                  | sealed   | sealed; decoration is the only seam  |
|  [11]   | `ComponentAttributes.DrawForegroundDecorations(Context, Skin, Capsule, Shade)`      | virtual  | the post-content decoration seam     |
|  [12]   | `Draw{Background, Content, Icon, Name, Label, UserName}`                            | virtual  | per-region paint overrides           |
|  [13]   | `ShowTentative{Inputs, Outputs}` / `Tentative{Inputs, Outputs}`                     | property | animated ZUI grip advertisement      |
|  [14]   | `ResizableAttributes<T>(T owner, SizeF minimumSize, SizeF maximumSize)`             | ctor     | orders min/max, restores size        |
|  [15]   | `ResizableAttributes<T>.{Size, MinimumSize, MaximumSize}`                           | property | clamped, rounded, persisted extent   |
|  [16]   | `ResizableAttributes<T>.InvalidateLayout()`                                         | virtual  | empty base body; size-commit hook    |
|  [17]   | `ResizableAttributes<T>.Layout(Shape)`                                              | virtual  | lays out pins only; no capsule boxes |
|  [18]   | `ResizableAttributes<T>.Responder -> Responses`                                     | property | the private nested resize responder  |
|  [19]   | `const int ResizableAttributes<T>.EdgeSize = 6`                                     | constant | edge-grab and cursor padding, pixels |
|  [20]   | `ICursorAwareAttributes.CursorAt(PointF)`                                           | explicit | explicit on `ResizableAttributes<T>` |

- `ResizableAttributes<T> : Attributes<T>` directly, NOT `ComponentAttributes` — so `LayoutBounds`, `DrawForegroundDecorations`, and the three layout boxes exist only on the component base, and a resizable shell hooks `Layout(Shape)` and `protected Draw(Context, Skin, Capsule)` instead, reading its shade as `skin.Shades[Owner]`.
- `Size`'s setter is the whole commit: it clamps to `MinimumSize`/`MaximumSize`, rounds, writes `Owner.CustomValues.Set("Attr.Size", value)`, re-frames `Bounds` from `Pivot`, then calls the empty `InvalidateLayout()` — so overriding `InvalidateLayout` is the sanctioned committed-size hook and it fires during base construction before a subclass field is assigned.
- Resize gesture is the responder's, not the shell's: `MouseDown` mints a `ResizingFrame(Bounds, MinimumSize, MaximumSize, SnappingConstraints.CreateFromDocument(Owner.Document, Owner.InstanceId), SnappingSettings.Current)` and a `Grasshopper2.Undo.Actions.ResizeAction(Owner)`, `MouseDrag` advances it, `MouseUp` clears both canvas snap axes and commits through `Owner.Document.Undo.Do(("Resize", Owner.Nomen.Name), action)`, and `KeyDown` toggles `Settings.CanvasSnapToObjects`.
- `ICursorAwareAttributes.CursorAt` is an EXPLICIT implementation, so a subclass cannot override it and re-listing the interface on the subclass re-implements the map instead — silently replacing the host's edge-resize cursor with the subclass member; the base body pads by `EdgeSize`, zeroes the axes whose min and max agree, and returns `null` outside `Bounds.Contains(point, 0.5f)`.

[ENTRYPOINT_SCOPE]: writable parameter modifiers (`Grasshopper2.Parameters.Standard`)

| [INDEX] | [SURFACE]                                | [SHAPE]                              | [CAPABILITY]                                       |
| :-----: | :--------------------------------------- | :----------------------------------- | :------------------------------------------------- |
|  [01]   | `VectorParameter.UnitiseVectors`         | `bool`                               | unit-length projection                             |
|  [02]   | `VectorParameter.ReverseVectors`         | `bool`                               | sense projection                                   |
|  [03]   | `AngleParameter.EnforceKind`             | `int`                                | untyped 0/1/2/3 unit ordinal                       |
|  [04]   | `AngleParameter.ReduceAngles`            | `bool`                               | turn reduction                                     |
|  [05]   | `BooleanParameter.NegateValues`          | `bool`                               | negation                                           |
|  [06]   | `BooleanParameter.ReplaceNullsWithTrue`  | `bool`                               | null substitution to true                          |
|  [07]   | `BooleanParameter.ReplaceNullsWithFalse` | `bool`                               | null substitution to false                         |
|  [08]   | `ConnectionParameter.DoCollect`          | `bool`                               | collect connected source ids                       |
|  [09]   | `IntegerParameter.IsIndex`               | `bool`                               | index posture                                      |
|  [10]   | `IntegerParameter.Indexing`              | `IndexModifier`                      | wrap policy                                        |
|  [11]   | `IntegerParameter.Hint`                  | `UiInteger`                          | `Grasshopper2.UI` integer hint                     |
|  [12]   | `NumberParameter.Hint`                   | `UiNumber`                           | `Grasshopper2.UI` slider domain and precision hint |
|  [13]   | `NumericParameter.ExoticFilter`          | `NumericFilter`                      | admitted exotic numeric families                   |
|  [14]   | `CurveParameter.NormaliseDomains`        | `NormalisationMethod`                | curve domain rule                                  |
|  [15]   | `CurveParameter.FlipCurves`              | `bool`                               | curve sense flip                                   |
|  [16]   | `SurfaceParameter.AcceptMeshes`          | `bool`                               | mesh admission                                     |
|  [17]   | `SurfaceParameter.NormaliseDomains`      | `CurveParameter.NormalisationMethod` | surface domain rule                                |
|  [18]   | `SurfaceParameter.FlipSurfaces`          | `bool`                               | surface sense flip                                 |
|  [19]   | `TextParameter.Flavour`                  | `TextFlavour`                        | text flavour                                       |
|  [20]   | `TextParameter.FileExtensions`           | `string[]`                           | extension filter                                   |
|  [21]   | `TextParameter.WatchFiles`               | `bool`                               | file-watch toggle                                  |
|  [22]   | `TextParameter.Casing`                   | `CasingBehaviour`                    | casing policy                                      |
|  [23]   | `TextParameter.CleanWhitespace`          | `bool`                               | whitespace cleanup                                 |
|  [24]   | `TextPatternParameter.PatternKind`       | `TextPatternKind`                    | pattern dialect                                    |
|  [25]   | `TextPatternParameter.CaseSensitive`     | `bool`                               | case policy                                        |
|  [26]   | `IParameter.PersistentDataWeak`          | `ITree`                              | erased persisted-tree slot                         |

- every row above is a `public { get; set; }` auto-property on the concrete parameter, so a pin modifier is assigned after the adder returns and never through a declaration argument; `TextParameter.WatchFiles` alone initializes `true`.
- `AngleParameter.EnforceKind` is a raw host `int` with NO host enum behind it — the persisted `Integer32("EnforceKind")` and the host's own `== 1`/`== 2`/`== 3` toolbar reads ARE the protocol, so the ordinals `0` none, `1` degrees, `2` radians, `3` turns are host wire constants and a boundary vocabulary typing them carries them as an `int` column.

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
|  [20]   | `InputAdder.Add(IParameter, Requirement)`              | declare   | attach a directly-minted parameter               |
|  [21]   | `ModularInputAdder.RegularAdder -> InputAdder`         | access    | the regular adder every modular row delegates to |

- `PluginServer.LoadPlugin`: assembly harvesting stays internal to the load.
- `InputAdder.Add(IParameter, Requirement = MustExist)` is public, so any `Grasshopper2.Parameters.Standard` parameter constructed by hand attaches through it — the escape hatch for a pin kind whose typed `Add*` the host has marked obsolete.
- `InputAdder.AddLanguage(string, string, string, Access, Requirement)` alone carries `[Obsolete("Not actually obsolete, but consider adding Language Pin support to your component instead.")]`; the sibling `OutputAdder.AddLanguage(string, string, string, Access)` does not, so only the input leg needs the direct-mint route and no suppression is ever earned. `LanguageParameter` itself and the `Grasshopper2.Types.Linguistic.Language` culture-code enum carry no obsolescence.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Component` declares pins once through `AddInputs(InputAdder)`/`AddOutputs(OutputAdder)`, computes through `Process(IDataAccess)`, overrides `Process(IDataAccess[], CancellationToken)` for iteration-array policy, and reconciles variable pins through the `Can*`/`Do*Parameter(Side, int, ActionList)` pairs — every structural edit rides an `ActionList` for undo.
- `IDataAccess` is the sole in-`Process` seam: typed `Get*`/`Set*` over item/pear/twig/tree depth, tolerance and unit context, transform reads, and `AddRemark`/`AddWarning`/`AddError` messages; `Process` reads `access.Solution`/`access.Callstack`, never the `Document`.
- pins are `IParameter`s carrying an `Access` (`Item`/`Twig`/`Tree`) and a `Requirement` (`MustExist`/`MayBeNull`/`MayBeMissing`); the adder families are the one pin-declaration surface, and the modular adders extend each with label, colour, category, and hidden state.
- data is `Tree<T>` of `Twig<T>` of `Pear<T>`: `Garden` builds and folds trees (`PairWiseOp`/`PearWiseOp`), `Twig<T>.Convert`/`Apply` transform a branch, and the `Grasshopper2.Parameters.Standard` brokers and `ConversionServer` resolve a host object onto its concrete family carrying a `Merit` score.
- `ModularComponent` drives its pin surface from `__`-prefixed well-known keys (`__Icon`, `__Colour`, `__Optional`, `__Category`, `__HideByDefault`, `__HiddenWires`); `Plugin`/`PluginServer` own registration and `IoIdAttribute` stamps the persistent serialization id.
- Pin's writable policy is post-declaration property assignment on the concrete `Grasshopper2.Parameters.Standard` parameter the adder returned, never a declaration argument, so a policy carrier admits its whole column set and writes it in one pass against the exact parameter type.
- `CreateAttributes` returns the object's own `IAttributes`: `ComponentAttributes` is the component base whose `Responder` and `DrawForegroundDecorations` are the two extension seams, while `ResizableAttributes<T>` is a sibling over `Attributes<T>` carrying persisted size and its own resize responder — the two bases share no layout or decoration member, so one policy spine projects onto them through different host callbacks.

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
- Owns: the `Component`/`ModularComponent` authoring model, `IDataAccess`, the typed pin adder families and their writable `Grasshopper2.Parameters.Standard` modifier columns, the component-attribute bases (`Attributes<T>`, `ComponentAttributes`, `ResizableAttributes<T>`), the `Tree`/`Twig`/`Pear`/`Garden` data algebra, the `ConversionServer` brokers, and `Plugin`/`PluginServer` registration
- Accept: a `Component` declaring pins through the adder families, computing through `Process(IDataAccess)` with reads lifted onto `Fin`/`Validation`, `Access`/`Requirement`/`Side` folded onto `[SmartEnum]`s, trees built and folded through `Garden`, conversions carrying a `Merit` receipt, and registration through `PluginServer`
- Reject: a GH1 `GH_Component`/`SolveInstance`/`IGH_DataAccess`/`GH_Structure`/`GH_ParamAccess` shape; a hand-enumerated `Add*` pin wall; a `bool`+out read where `Fin`/`Option` gives the typed rail; a hand-rolled tree walker beside `Garden`; a `null`-out conversion beside the `Merit`-scored broker fold; the `IAttributes` contract itself (`api-gh2-document.md`) and the `Responses` dispatch family the attribute bases expose (`api-gh2-flex.md`); a `CS0618` suppression over `InputAdder.AddLanguage` where the public `Add(IParameter, Requirement)` seam declares the same parameter
