# [RASM_GRASSHOPPER_API_GH2_SPECIAL_OBJECTS]

`Grasshopper2.Parameters.Special` owns the interactive document objects the installed Grasshopper 2 plug-in supplies: value inputs, editors, samplers, pickers, data panels, the expiry scheduler, and the shout/listen/relay connection routers. Every type derives the `Parameter`/`Parameter<T>` value contract — `Listen` on `GenericParameter`, `TimerObject` on `DocumentObject` — and round-trips through the host IO archive.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: host assembly `Grasshopper2`
- package: `Grasshopper2` (Rhino 9 WIP Grasshopper 2 plug-in SDK; not a NuGet pin — the in-process `Grasshopper2.dll` under `Grasshopper2Plugin.rhp` is the resolved asset)
- assembly: `Grasshopper2`
- namespace: `Grasshopper2.Parameters.Special`
- adjacent namespaces: `Grasshopper2.Data`, `Grasshopper2.Parameters`, `Grasshopper2.Types.Colour`, `Grasshopper2.UI`, `GrasshopperIO` resolve the value carriers, pears, and IO seams
- rail: host-grasshopper special objects

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: interactive special objects, ordered by role family. Every type also carries a public `()` and `(IReader)` deserialization constructor; the parameterized constructor rides its `[03]` roster line.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]                | [CAPABILITY]                                      |
| :-----: | :--------------------- | :--------------------------- | :------------------------------------------------ |
|  [01]   | `NumberSliderObject`   | `Parameter<double>`          | drag-slider numeric input                         |
|  [02]   | `NumberPickerObject`   | `Parameter<double>`          | stepped numeric picker                            |
|  [03]   | `ToggleObject`         | `Parameter<bool>`            | boolean toggle with named states                  |
|  [04]   | `ButtonObject`         | `Parameter`                  | momentary button emitting up/down trees           |
|  [05]   | `ValueObject`          | `Parameter`                  | notation-typed value input                        |
|  [06]   | `TextInputObject`      | `Parameter<string>`          | multi-line text input                             |
|  [07]   | `ColourSwatchObject`   | `Parameter<Colour>`          | palette colour swatch                             |
|  [08]   | `GradientEditorObject` | `Parameter<Gradient>`        | gradient grip editor                              |
|  [09]   | `FunctionEditorObject` | `Parameter<Function>`        | expression editor                                 |
|  [10]   | `MaterialEditorObject` | `Parameter<DisplayMaterial>` | display-material editor                           |
|  [11]   | `ImageSamplerObject`   | `Parameter`                  | image coordinate sampler                          |
|  [12]   | `HistogramObject`      | `Parameter<double>`          | bucketed histogram display                        |
|  [13]   | `QuickGraphObject`     | `Parameter<double>`          | quick numeric graph display                       |
|  [14]   | `ProtractorObject`     | `Parameter<Angle>`           | angle protractor input                            |
|  [15]   | `PresetPickerObject`   | `Parameter`                  | multi-select preset picker                        |
|  [16]   | `ComplexPickerObject`  | `Parameter<Complex>`         | complex-number picker                             |
|  [17]   | `ConstantPickerObject` | `Parameter`                  | math-constant picker                              |
|  [18]   | `MetaNamePickerObject` | `Parameter<MetaName>`        | meta-name key picker                              |
|  [19]   | `TemporalPickerObject` | `Parameter<DateTime>`        | date/time picker                                  |
|  [20]   | `ValueListObject`      | `Parameter`                  | selectable value list                             |
|  [21]   | `TimerObject`          | `DocumentObject`             | schedules expiry for an object-id target set      |
|  [22]   | `PathMapperObject`     | `Parameter`                  | remaps paths through parsed notation              |
|  [23]   | `DataPanelObject`      | `Parameter`                  | projects tree values into a panel                 |
|  [24]   | `DataRecorderObject`   | `Parameter`                  | retains successive input trees                    |
|  [25]   | `TreeViewerObject`     | `Parameter`                  | visualizes tree topology                          |
|  [26]   | `Shout`                | `Parameter`                  | broadcasts tree data or marks a cluster output    |
|  [27]   | `Listen`               | `GenericParameter`           | resolves a shout, file dependency, or cluster pin |
|  [28]   | `Relay`                | `Parameter`                  | relays, freezes, and safely reconnects wires      |

## [03]-[ENTRYPOINTS]

Each roster line carries one type's constructor and public members grouped by access; a caveat line states behavior a signature cannot show. `params Complex[]` is the declared final parameter of `ComplexPickerObject`.

[ENTRYPOINT_SCOPE]: value inputs
- `NumberSliderObject` — ctor `(string, UiNumber)`; get `InternalSlider: Slider` `InternalNumber: UiNumber`; get/set `GripDisplay` `GripColour` `GripFormat`
- `NumberPickerObject` — ctor `(string, double)`; get `InternalPicker: NumberPicker` `InternalNumber: decimal`; get/set `GripColour` `SnapToTicks`
- `ToggleObject` — ctor `(bool)`; get/set `ToggleState`; get/private-set `TrueName` `FalseName` `TrueInfo` `FalseInfo`; event `StateNamesChanged`
- `ButtonObject` — ctor `(IPear, IPear)`; get/set `Action` `UpTree` `DownTree` `UpColour` `DownColour` `UpText` `DownText`; method `Press()` `Release()`
- `ValueObject` — ctor `(string, string, Notation)`; get/private-set `Text: string` `Value: object`; get/set `Notations`; method `AssignTextAndValue(string)`
- `TextInputObject` — ctor `(string, string)`; get `Values: string[]`; get/internal-set `OneEntryPerLine`; get/set `Contents` `Escaping`
- `ColourSwatchObject` — ctor `(string, Colour, string, bool)`; get/internal-set `Colour` `Palette` `Discrete`; method `SetColour(Colour, bool)`
- `ButtonObject(IPear, IPear)`: a null pear leaves its tree unset, and a null `down` selects `ButtonAction.Single`.
- `ValueObject.AssignTextAndValue`: null or empty text normalizes to `Text == string.Empty` and `Value == null`.
- `TextInputObject.Values`: an empty array, never null; `Contents` reads an empty string when its backing value is null.
- `ColourSwatchObject`: rejects a null constructor colour, while `SetColour` accepts a null colour at runtime.

[ENTRYPOINT_SCOPE]: editors and samplers
- `GradientEditorObject` — ctor `(string, GripGradient)`; get `Gradient: Gradient`; get/set `Parameter0` `Parameter1` `Interaction`
- `FunctionEditorObject` — ctor `(string, Function)`; get `Editor: FunctionEditorBase`
- `MaterialEditorObject` — ctor `(string, DisplayMaterial)`; get/set `ForeRotation` `BackRotation` `IdenticalForeAndBack` `Material`
- `ImageSamplerObject` — get `DisplayImage: Bitmap`; get/private-set `Image: Bitmap`; get/set `ImageUri` `Normalised` `Luminance` `LimitBehaviour` `DrawSamples`
- `HistogramObject` — get `BucketCountText` `Pins`; get/set `Style` `Palette` `BucketCount` `BucketRange`
- `QuickGraphObject` — get `Pins`
- `ProtractorObject` — ctor `(string, Angle)`; get/set `Mode` `Angle`
- `GradientEditorObject.Interaction`: null normalizes to the `Matter` interaction.
- `FunctionEditorObject.Editor`: a read-only property carrying a mutable editor.
- `MaterialEditorObject.Material`: null replaced lazily with a default material.
- `ImageSamplerObject`: `ImageUri` and `DisplayImage` are nullable, and public image mutation is URI based.

[ENTRYPOINT_SCOPE]: pickers and lists
- `PresetPickerObject` — ctor `(string)`; get `AvailablePresets` `SelectedNames`; get/set `UserNames` `MultiSelect` `Scroll0` `Scroll1`
- `ComplexPickerObject` — ctor `(string, params Complex[])`; get/internal-set `Values: Complex[]`
- `ConstantPickerObject` — ctor `(string, Maths.Constant)`; get/set `Constant: Maths.Constant`
- `MetaNamePickerObject` — ctor `(string, MetaName)`; get/set `MetaKey: MetaName`
- `TemporalPickerObject` — ctor `(string, DateTime)`; get/internal-set `Date: DateTime`
- `ValueListObject` — get `ItemCount` `Items`; method `Pear(int)` `ItemSelected(int)`; get/set `Mode`; method `SelectPrev` `SelectNext` `SelectItem` `DeselectItem`; event `StateChanged`
- `PresetPickerObject.UserNames`: null denotes no user-authored selection; `SelectedNames` falls back user names, then available presets, then an empty array, never null.
- `PresetPickerObject.SelectedPresets(out int[], out IPreset[])`: equal-length, non-null arrays, two empty when no available preset resolves.
- `PresetCollection`: a public type with an internal constructor obtained through `AvailablePresets`; `AvailablePresetsChanged` and `SelectedPresetsChanged` are assignable delegate fields, not events.
- `ValueListObject.Items`: value/meta pears only; the internal `ValueListItem` carrier and the `Set(ValueListItem[], bool)`/`RepairSelection()` members withhold any public list-replacement surface, so public code inspects items and mutates selection alone.

[ENTRYPOINT_SCOPE]: data, utility, and connection routing
- `TimerObject` — get `DelayText` `DelayDisplayText` `TargetCount` `TargetIds` `Targets`; get/set `Delay` `Running` `Manual`; method `IsTarget` `AddTarget` `RemoveTarget`
- `PathMapperObject` — get `Mappings: PathMappings`; get/set `Notation` `OmitUnaffected` `WarnAboutPaths` `WarnAboutSites`
- `DataPanelObject` — get/set `VerticalOffset` `ShowColumns` `ShowPaths` `ShowIndices` `ShowTypes` `ShowItems` `ShowMetas`; method `ChangeDisplay`
- `DataRecorderObject` — get `IsEmpty` `Valence`; get/set `Paused` `MergeTrees` `FrameLimit`; method `ClearRecordedData()`
- `TreeViewerObject` — get/set `CanvasDisplay` `ViewportDisplay` `DisplayGradient`
- `Shout` — get/set `ClusterOutput` `StreamData` `StreamPath` `StreamBackup`; method `UpdateNomenBasedOnState()`
- `Listen` — get `ShoutId` `CurrentDependency`; get/set `DependencyIndex` `DependencyA`..`DependencyD` `ClusterInput` `ClusterIndex`; method `UpdateNomenBasedOnState()`
- `Relay` — get `FrozenDataIsStale` `FrozenCachedData` `DisplayName`; get/set `Frozen`; method `SafeDisconnect()` `ResolveDisplayName()`
- `TimerObject.TargetIds`: an empty array, never null; `Targets` omits unresolved identifiers and yields nothing when the timer has no document.
- `PathMapperObject.Notation`: null normalizes to an empty string.
- `DataRecorderObject.IsEmpty`: evaluates `_buckets.Count > 0`, so it reads true when recorded buckets exist despite the member name and XML summary.
- `TreeViewerObject.DisplayGradient` and `Shout.StreamPath`: nullable.
- `Listen.ShoutId`: `Guid.Empty` under name-based resolution; `CurrentDependency` returns null when `DependencyIndex` selects none of the four dependency slots.
- `Relay.Frozen`: setting true captures `State.Data.Tree()` into `FrozenCachedData`, marks the cache current, suppresses recipient expiration, and serves the captured tree during collection; upstream expiration marks it stale, and thawing clears the cache, expires the relay when the captured data went stale, and resets the flag, so `FrozenCachedData` is nullable whenever thawed or unrestored.
- `Relay.SafeDisconnect()`: returns an `ActionList`, copies every relay input to every downstream parameter, disconnects both sides, refreshes downstream relay names, and expires the downstream parameters; `ResolveDisplayName()` derives the name from `UserName` or the distinct sorted names propagated by upstream relays.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every object round-trips through its public `(IReader)` constructor and its `Store(IWriter)` override; a domain value crosses the archive only as that IO pair.
- Canvas attributes come from protected `CreateAttributes()` overrides; a consumer reads them off the inherited document-object surface and never invokes the concrete factory.
- Image sampling (`SampleContinuous`) and list replacement (`ValueListItem`, `Set(ValueListItem[], bool)`, `RepairSelection()`) are internal or private; the public surface samples through `ImageUri` and mutates list selection alone.
- A read that can miss states its miss in the value, not an exception: `TargetIds`/`SelectedNames`/`Values` normalize to empty, `Notation`/`Contents` to empty strings, and `StreamPath`/`CurrentDependency`/`FrozenCachedData`/`DisplayGradient` return null on the unset path.

[STACKING]:
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): nullable state reads (`Shout.StreamPath`, `Listen.CurrentDependency`, `Relay.FrozenCachedData`, `TreeViewerObject.DisplayGradient`) lower to `Option<T>`; a construction that rejects null (`ColourSwatchObject` colour) folds to `Fin<T>` where the reject maps to `Error`; the empty-array normalizers (`TimerObject.TargetIds`, `PresetPickerObject.SelectedNames`, `TextInputObject.Values`) carry as `Seq<T>`; `Relay.SafeDisconnect() -> ActionList` sequences through `Eff`; and slider/toggle/button interactive state rides an `Atom` cell.
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the host discriminants — `Notation`, `ButtonAction`, `ProtractorObject.Mode`, `HistogramObject.Style`, `LimitBehaviour`, `ValueListObject.Mode` — are owned as `[SmartEnum]` vocabularies so a state branch dispatches through exhaustive `Switch`, and a `Colour`/`Gradient`/`Angle`/`Complex` value carrier folds to a `[ComplexValueObject]` with structural equality.
- `api-gh2-io`: `(IReader)` construction and `Store(IWriter)` round-trip enter the archive through `IReader`/`IWriter`; `api-gh2-document`: every object joins the graph as a `DocumentObject`, and `TimerObject` expiry scheduling and `Relay.SafeDisconnect` mutation seal into one `ActionList`; `api-gh2-canvas`: protected `CreateAttributes` attributes read off the canvas; `api-gh2-components`: `IPear`/`ITree` data and the `Parameter<T>` value contract flow through these objects.

[LOCAL_ADMISSION]:
- These special objects are the Rasm.Grasshopper folder's interactive-parameter domain; each composes the Rasm kernel for host-agnostic logic and never references a sibling Rasm package.
- A value enters through the typed `Parameter<T>` contract; the internal `ValueListItem`/`SampleContinuous` members are not admitted, and canvas attributes arrive from the protected factory the base owns.

[RAIL_LAW]:
- Package: `Grasshopper2` (interactive special objects)
- Owns: the `Grasshopper2.Parameters.Special` value inputs, editors, samplers, pickers, data and utility panels, the expiry scheduler, and the shout/listen/relay routers
- Accept: parameter construction, public state read and mutation, interactive method invocation, and IO round-trip over these objects
- Reject: document graph mutation (`api-gh2-document`), canvas paint and picking (`api-gh2-canvas`), component declaration and pin typing (`api-gh2-components`), and the internal sampling and list-assignment members
