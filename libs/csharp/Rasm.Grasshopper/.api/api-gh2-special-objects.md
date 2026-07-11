# [RASM_GRASSHOPPER_API_GH2_SPECIAL_OBJECTS]

`Grasshopper2.Parameters.Special` is the native interactive parameter-object family — document objects that hold a persistent value edited on the canvas rather than computed from wires. Each object derives the `Grasshopper2.Parameters` `AbstractParameter` base, persists its value through the `GrasshopperIO` `IReader`/`IWriter` pair, and projects its canvas control through `CreateAttributes`: the value inputs (`NumberSliderObject`, `NumberPickerObject`, `ToggleObject`, `ButtonObject`, `ValueObject`, `TextInputObject`, `ColourSwatchObject`), the editors and samplers (`GradientEditorObject`, `FunctionEditorObject`, `MaterialEditorObject`, `ImageSamplerObject`, `HistogramObject`, `QuickGraphObject`, `ProtractorObject`), the pickers and lists (`PresetPickerObject`, `ComplexPickerObject`, `ConstantPickerObject`, `MetaNamePickerObject`, `TemporalPickerObject`, `ValueListObject`), and the data and utility objects (`TimerObject`, `PathMapperObject`, `DataPanelObject`, `DataRecorderObject`, `TreeViewerObject`). `Shout`/`Listen`/`Relay` are the connection-routing special objects the `Grasshopper2.Components.Standard` `Cluster` boundary and wire relays bind. A new interactive object is a new row in this family, one persistent value plus one attributes projection, never a bespoke control outside the persistence and attribute contract.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grasshopper2` (Rhino 9 WIP Grasshopper2 SDK)
- assembly: `Grasshopper2.dll` (installed `Grasshopper2Plugin.rhp` managed plug-in; in-process)
- namespace: `Grasshopper2.Parameters.Special`, `Grasshopper2.Parameters`, `Grasshopper2.Types.Colour`, `Grasshopper2.UI.InputPanel`, `GrasshopperIO`
- base: `Grasshopper2.Parameters.AbstractParameter` — every special object is a persistent-value parameter
- io: `GrasshopperIO.IReader`/`IWriter` — the `#ctor(IReader)` + `Store(IWriter)` pair persists each object's value
- rail: interactive parameter objects

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: value input objects
- rail: interactive parameter objects
- note: each holds one editable value with a canvas control; the value round-trips through `IReader`/`IWriter` and the control through `CreateAttributes`.

| [INDEX] | [SYMBOL]             | [KIND]      | [CAPABILITY]                                              |
| :-----: | :------------------- | :---------- | :-------------------------------------------------------- |
|  [01]   | `NumberSliderObject` | value input | a dragged numeric value; grip display, colour, and format |
|  [02]   | `NumberPickerObject` | value input | a picked numeric value with tick snapping                 |
|  [03]   | `ToggleObject`       | value input | a boolean with named true/false states                    |
|  [04]   | `ButtonObject`       | value input | a momentary up/down value pair with press/release         |
|  [05]   | `ValueObject`        | value input | a parsed text-to-value with a notation vocabulary         |
|  [06]   | `TextInputObject`    | value input | a text value with an escaping mode                        |
|  [07]   | `ColourSwatchObject` | value input | a `Grasshopper2.Types.Colour.Colour` with a palette       |

[PUBLIC_TYPE_SCOPE]: editor, sampler, picker, and list objects
- rail: interactive parameter objects
- note: the editors carry a rich interactive value (gradient, function, material, image, histogram, graph); the pickers and lists select from an available set.

| [INDEX] | [SYMBOL]                                                                                         | [KIND]  | [CAPABILITY]                                                  |
| :-----: | :----------------------------------------------------------------------------------------------- | :------ | :------------------------------------------------------------ |
|  [01]   | `GradientEditorObject`                                                                           | editor  | a `GripGradient` with an interaction mode                     |
|  [02]   | `ImageSamplerObject`                                                                             | sampler | a bitmap sampled continuously to a normalized/luminance value |
|  [03]   | `HistogramObject`                                                                                | editor  | a bucketed distribution with style and palette                |
|  [04]   | `FunctionEditorObject` / `MaterialEditorObject` / `QuickGraphObject` / `ProtractorObject`        | editor  | function, material, graph, and angle editors                  |
|  [05]   | `PresetPickerObject`                                                                             | picker  | a multi-select over an available preset set                   |
|  [06]   | `ValueListObject`                                                                                | list    | an item list with a selection `Mode`                          |
|  [07]   | `ComplexPickerObject` / `ConstantPickerObject` / `MetaNamePickerObject` / `TemporalPickerObject` | picker  | complex-number, constant, meta-name, and temporal pickers     |

[PUBLIC_TYPE_SCOPE]: data, utility, and routing objects
- rail: interactive parameter objects
- note: the data objects inspect or record tree data; the routing objects carry a value across a cluster boundary or a frozen relay.

| [INDEX] | [SYMBOL]             | [KIND]  | [CAPABILITY]                                           |
| :-----: | :------------------- | :------ | :----------------------------------------------------- |
|  [01]   | `TimerObject`        | utility | a delay-driven expiry over a target-object set         |
|  [02]   | `PathMapperObject`   | data    | a tree-path remapping with a notation and warnings     |
|  [03]   | `DataPanelObject`    | data    | a tree-data display with column/path/type/item toggles |
|  [04]   | `DataRecorderObject` | data    | a frame-limited recording of streamed tree data        |
|  [05]   | `TreeViewerObject`   | data    | a canvas/viewport tree display with a gradient         |
|  [06]   | `Shout` / `Listen`   | routing | a cluster output/input boundary pin                    |
|  [07]   | `Relay`              | routing | a freezable wire relay caching its data                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: value input construction and state
- rail: interactive parameter objects
- note: each object constructs from a typed initial value, exposes its persistent value, and mints its canvas control through `CreateAttributes`; the `#ctor(IReader)`/`Store(IWriter)` pair persists it.

| [INDEX] | [SURFACE]                                                                                                                             | [CALL_SHAPE] | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------ | :----------- | :---------------------------------------------- |
|  [01]   | `NumberSliderObject(string, UiNumber)` / `InternalSlider` / `InternalNumber` / `GripDisplay` / `GripColour` / `GripFormat`            | slider       | a dragged numeric value with grip display state |
|  [02]   | `NumberPickerObject(string, double)` / `InternalPicker` / `InternalNumber` / `GripColour` / `SnapToTicks`                             | picker       | a picked numeric value with tick snapping       |
|  [03]   | `ToggleObject(bool)` / `ToggleState` / `TrueName` / `FalseName` / `TrueInfo` / `FalseInfo` / `StateNamesChanged`                      | toggle       | a boolean with named/annotated states           |
|  [04]   | `ButtonObject(IPear, IPear)` / `Action` / `UpTree` / `DownTree` / `UpColour` / `DownColour` / `Press` / `Release`                     | button       | a momentary up/down value pair                  |
|  [05]   | `ValueObject(string, string, Notation)` / `Text` / `Value` / `AssignTextAndValue(string)` / `Notations` / `AppendParsers(InputPanel)` | value        | a parsed text-to-value with notation parsers    |
|  [06]   | `ColourSwatchObject(string, Colour, string, bool)` / `Colour` / `SetColour(Colour, bool)` / `Palette` / `Discrete`                    | swatch       | a persistent colour with a palette              |

[ENTRYPOINT_SCOPE]: editors, samplers, pickers, and lists
- rail: interactive parameter objects
- note: the editors expose a rich interactive value; `ImageSamplerObject.SampleContinuous` reads a bitmap; the pickers resolve a selection over an available set.

| [INDEX] | [SURFACE]                                                                                                                                                | [CALL_SHAPE] | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------- | :------------------------------------------------ |
|  [01]   | `GradientEditorObject(string, GripGradient)` / `Gradient` / `Interaction`                                                                                | gradient     | a grip-gradient value with interaction state      |
|  [02]   | `ImageSamplerObject.Image` / `ImageUri` / `Normalised` / `Luminance` / `LimitBehaviour` / `DrawSamples`                                                  | image        | a sampled bitmap with normalization and luminance |
|  [03]   | `ImageSamplerObject.SampleContinuous(BitmapData, int, int, float, float, SamplingLimit)`                                                                 | sample       | a continuous per-coordinate bitmap sample         |
|  [04]   | `HistogramObject.Style` / `Palette` / `BucketCount` / `BucketRange` / `CreateCustomPalette(int)`                                                         | histogram    | a bucketed distribution with style and palette    |
|  [05]   | `PresetPickerObject(string)` / `AvailablePresets` / `SelectedNames` / `MultiSelect` / `SelectedPresets(out int[], out IPreset[])` / `IsSelected(string)` | preset       | a multi-select over an available preset set       |
|  [06]   | `ValueListObject.Items` / `ItemCount` / `Mode` / `Set(ValueListItem[], bool)` / `ItemSelected(int)` / `RepairSelection`                                  | list         | an item list with selection and repair            |

[ENTRYPOINT_SCOPE]: data, timer, and routing
- rail: interactive parameter objects
- note: `TimerObject` drives expiry over a target set; the data objects toggle display or record streamed data; `Shout`/`Listen`/`Relay` route a value across a boundary.

| [INDEX] | [SURFACE]                                                                                                                       | [CALL_SHAPE] | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------ | :----------- | :--------------------------------------------- |
|  [01]   | `TimerObject.Delay` / `Running` / `Manual` / `Targets` / `IsTarget(Guid)` / `AddTarget(Guid)` / `RemoveTarget(Guid)`            | timer        | a delay-driven expiry over a target-object set |
|  [02]   | `PathMapperObject.Mappings` / `Notation` / `OmitUnaffected` / `WarnAboutPaths` / `WarnAboutSites`                               | path map     | a tree-path remapping with warnings            |
|  [03]   | `DataPanelObject.ShowPaths` / `ShowIndices` / `ShowTypes` / `ShowItems` / `ShowMetas` / `ChangeDisplay(bool?, ...)`             | panel        | a tree-data display with column toggles        |
|  [04]   | `DataRecorderObject.Paused` / `MergeTrees` / `FrameLimit` / `IsEmpty` / `ClearRecordedData`                                     | recorder     | a frame-limited recording of streamed data     |
|  [05]   | `TreeViewerObject.CanvasDisplay` / `ViewportDisplay` / `DisplayGradient`                                                        | viewer       | a canvas/viewport tree display                 |
|  [06]   | `Shout.SurroundingCluster` / `ClusterOutput` / `StreamData` / `StreamPath` / `Listen.ShoutId` / `ClusterInput` / `ClusterIndex` | routing      | the cluster output/input boundary state        |
|  [07]   | `Relay.Frozen` / `FrozenDataIsStale` / `FrozenCachedData` / `SafeDisconnect` / `ResolveDisplayName`                             | relay        | a freezable wire relay caching its data        |

## [04]-[IMPLEMENTATION_LAW]

[SPECIAL_TOPOLOGY]:
- every special object is an `AbstractParameter` carrying a persistent value: it constructs from a typed initial value, exposes that value as a property, mints its canvas control through `CreateAttributes`, and round-trips through the `#ctor(IReader)`/`Store(IWriter)` pair — the value and its canvas attribute state are the two facts each object owns
- the value inputs hold one editable datum (`NumberSliderObject.InternalNumber`, `ToggleObject.ToggleState`, `ValueObject.Value`, `ColourSwatchObject.Colour`); the editors hold a rich value (`GradientEditorObject.Gradient`, `ImageSamplerObject.Image`, `HistogramObject.Palette`); the pickers hold a selection over an available set (`PresetPickerObject.AvailablePresets`/`SelectedNames`, `ValueListObject.Items`/`Mode`)
- the data objects read or record tree data through display toggles (`DataPanelObject.Show*`, `DataRecorderObject.FrameLimit`, `TreeViewerObject.CanvasDisplay`); `TimerObject` drives document expiry over a `Targets` set through `AddTarget`/`RemoveTarget`
- `Shout`/`Listen` are the cluster boundary pins the `Grasshopper2.Components.Standard` `Cluster` resolves (`Shout.SurroundingCluster`/`ClusterOutput`, `Listen.ShoutId`/`ClusterInput`), and `Relay` is the freezable inline relay caching `FrozenCachedData` when `Frozen`

[STACKING]:
- `api-thinktecture-runtime-extensions`(`.api/api-thinktecture-runtime-extensions.md`): the special-object family folds onto one `[Union]` of interactive objects, so a canvas object is dispatched by its variant rather than a `Type` switch; the per-object modes (`ValueListMode`, `ProtractorMode`, `TextInputEscaping`, `HistogramStyle`, `ButtonAction`, `GradientEditorObject.Interaction`) fold onto `[SmartEnum]`s; each persistent value is a `[ValueObject]` carrier so `ToggleObject.ToggleState`, `NumberSliderObject.InternalNumber`, and `ValueObject.Value` are typed rather than boxed
- `api-languageext`(`.api/api-languageext.md`): the `#ctor(IReader)`/`Store(IWriter)` persistence pair lifts onto `Fin`, so a malformed persisted value is a typed `Error` at read; `PresetPickerObject.SelectedPresets(out int[], out IPreset[])` and `ValueListObject`/`Relay` out-shaped reads lift onto `Fin`/`Option`, so an empty selection or a stale relay resolves on the rail rather than a `null`; `TimerObject.Targets` composes the document-object id set as a `Seq<Guid>`
- `api-unicolour`(`.api/api-unicolour.md`), kernel visual owner: `ColourSwatchObject.Colour` and `GradientEditorObject.Gradient` carry the host `Grasshopper2.Types.Colour` value at the boundary, and perceptual blending or gradient interpolation composes the Rasm kernel colour owner rather than a second in-object blend; `ImageSamplerObject.SampleContinuous` normalization and `HistogramObject` bucketing compose the kernel numeric owner
- `api-generator-equals`(`.api/api-generator-equals.md`): each persistent value and preset descriptor takes generated structural equality, so a value-change or selection compare is one generated equality

[LOCAL_ADMISSION]:
- the special-object `[Union]` is the one owner of native interactive canvas objects; a hand-rolled slider, toggle, or swatch widget beside it is the rejected form
- persistent values ride the `IReader`/`IWriter` pair lifted onto `Fin`; a bespoke serialization beside `Store` is never re-minted
- canvas control state is `CreateAttributes`; a re-derived attribute or hit-test surface beside it is the deleted form
- `Shout`/`Listen`/`Relay` are the routing pins the `api-gh2-standard-components` `Cluster` composes; a second cluster-boundary carrier beside them is the rejected form

[RAIL_LAW]:
- Package: `Grasshopper2.dll` (Rhino 9 WIP Grasshopper2 SDK, in-process managed plug-in; `Grasshopper2.Parameters.Special`)
- Owns: the interactive value inputs, the editor/sampler objects, the picker/list objects, the data/timer/utility objects, and the `Shout`/`Listen`/`Relay` routing pins — each a persistent value plus a `CreateAttributes` canvas projection
- Accept: a special object extending `AbstractParameter`, folded onto the interactive-object `[Union]`, its persistent value a `[ValueObject]` round-tripped through `IReader`/`IWriter` on `Fin`, its modes `[SmartEnum]`s, its colour/gradient blending composing the kernel colour owner, and its cluster boundary bound through `Shout`/`Listen`
- Reject: a hand-rolled interactive widget beside the `[Union]`; a bespoke value serialization beside `Store`; a re-derived attribute surface beside `CreateAttributes`; a second colour-blend or gradient interpolation in-object rather than composing the kernel; a second cluster-boundary carrier beside `Shout`/`Listen`/`Relay`
