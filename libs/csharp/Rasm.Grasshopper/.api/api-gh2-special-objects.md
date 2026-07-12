# [RASM_GRASSHOPPER_API_GH2_SPECIAL_OBJECTS]

`Grasshopper2.Parameters.Special` contains the native interactive parameters, editors, displays, schedulers, and connection-routing objects supplied by the installed Grasshopper 2 plug-in. Most types derive `Parameter` or `Parameter<T>`; `Listen` derives `GenericParameter`, and `TimerObject` derives `DocumentObject`. Public deserialization constructors accept `GrasshopperIO.IReader`, and the corresponding objects persist through public `Store(IWriter)` overrides. Canvas attributes are created by protected overrides and are not consumer entrypoints.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grasshopper2` 'installed Rhino WIP Grasshopper 2 SDK'

- assembly: `Grasshopper2.dll` from the managed `Grasshopper2Plugin.rhp` payload
- namespace: `Grasshopper2.Parameters.Special`
- adjacent namespaces: `Grasshopper2.Data`, `Grasshopper2.Parameters`, `Grasshopper2.Types.Colour`, `Grasshopper2.UI`, `GrasshopperIO`
- value carriers: `double`, `decimal`, `bool`, `string`, `object`, `DateTime`, `Complex`, `Angle`, `ITree`, `IPear`, `Colour`, `Gradient`, `DisplayMaterial`, `Maths.Constant`, `MetaName`, `IPreset`
- rail: in-process document-object API

## [02]-[PUBLIC_TYPES_AND_CONSTRUCTION]

[CONSTRUCTION_SCOPE]: value inputs

- rail: public constructors
- note: every row also exposes a public `#ctor(IReader)` deserialization constructor.

| [INDEX] | [TYPE]               | [BASE]              | [CONSTRUCTORS]                                                                         |
| :-----: | :------------------- | :------------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `NumberSliderObject` | `Parameter<double>` | `()`; `(string userName, UiNumber number)`                                             |
|  [02]   | `NumberPickerObject` | `Parameter<double>` | `()`; `(string userName, double number)`                                               |
|  [03]   | `ToggleObject`       | `Parameter<bool>`   | `()`; `(bool state)`                                                                   |
|  [04]   | `ButtonObject`       | `Parameter`         | `()`; `(IPear up, IPear down)`                                                         |
|  [05]   | `ValueObject`        | `Parameter`         | `()`; `(string userName, string content, Notation notations)`                          |
|  [06]   | `TextInputObject`    | `Parameter<string>` | `()`; `(string userName, string content)`                                              |
|  [07]   | `ColourSwatchObject` | `Parameter<Colour>` | `()`; `(string userName, Colour colour, string palette = null, bool discrete = false)` |

[CONSTRUCTION_SCOPE]: editors, samplers, pickers, and lists

- rail: public constructors
- note: `params Complex[]` is the declared final parameter of `ComplexPickerObject`; every row also exposes a public `#ctor(IReader)` constructor.

| [INDEX] | [TYPE]                 | [BASE]                       | [CONSTRUCTORS]                                      |
| :-----: | :--------------------- | :--------------------------- | :-------------------------------------------------- |
|  [01]   | `GradientEditorObject` | `Parameter<Gradient>`        | `()`; `(string userName, GripGradient gradient)`    |
|  [02]   | `FunctionEditorObject` | `Parameter<Function>`        | `()`; `(string userName, Function function)`        |
|  [03]   | `MaterialEditorObject` | `Parameter<DisplayMaterial>` | `()`; `(string userName, DisplayMaterial material)` |
|  [04]   | `ImageSamplerObject`   | `Parameter`                  | `()`                                                |
|  [05]   | `HistogramObject`      | `Parameter<double>`          | `()`                                                |
|  [06]   | `QuickGraphObject`     | `Parameter<double>`          | `()`                                                |
|  [07]   | `ProtractorObject`     | `Parameter<Angle>`           | `()`; `(string userName, Angle angle)`              |
|  [08]   | `PresetPickerObject`   | `Parameter`                  | `()`; `(string userName)`                           |
|  [09]   | `ComplexPickerObject`  | `Parameter<Complex>`         | `()`; `(string userName, params Complex[] values)`  |
|  [10]   | `ConstantPickerObject` | `Parameter`                  | `()`; `(string userName, Maths.Constant constant)`  |
|  [11]   | `MetaNamePickerObject` | `Parameter<MetaName>`        | `()`; `(string userName, MetaName key)`             |
|  [12]   | `TemporalPickerObject` | `Parameter<DateTime>`        | `()`; `(string userName, DateTime value)`           |
|  [13]   | `ValueListObject`      | `Parameter`                  | `()`                                                |

[CONSTRUCTION_SCOPE]: data, utility, and routing objects

- rail: public constructors
- note: every row exposes `()` and `(IReader reader)` as its complete public constructor set.

| [INDEX] | [TYPE]               | [BASE]             | [CAPABILITY]                                      |
| :-----: | :------------------- | :----------------- | :------------------------------------------------ |
|  [01]   | `TimerObject`        | `DocumentObject`   | schedules expiry for an object-id target set      |
|  [02]   | `PathMapperObject`   | `Parameter`        | remaps paths through parsed notation              |
|  [03]   | `DataPanelObject`    | `Parameter`        | projects tree values into a configurable panel    |
|  [04]   | `DataRecorderObject` | `Parameter`        | retains successive input trees                    |
|  [05]   | `TreeViewerObject`   | `Parameter`        | visualizes tree topology                          |
|  [06]   | `Shout`              | `Parameter`        | broadcasts tree data or marks a cluster output    |
|  [07]   | `Listen`             | `GenericParameter` | resolves a shout, file dependency, or cluster pin |
|  [08]   | `Relay`              | `Parameter`        | relays, freezes, and safely reconnects wires      |

## [03]-[PUBLIC_STATE]

[STATE_SCOPE]: value inputs

- rail: public members
- note: access is stated explicitly because several properties expose mutable host objects while withholding a property setter.

| [INDEX] | [TYPE]               | [MEMBERS]                                                 | [ACCESS]              |
| :-----: | :------------------- | :-------------------------------------------------------- | :-------------------- |
|  [01]   | `NumberSliderObject` | `InternalSlider: Slider`; `InternalNumber: UiNumber`      | get                   |
|  [02]   | `NumberSliderObject` | `GripDisplay`; `GripColour`; `GripFormat`                 | get/set               |
|  [03]   | `NumberPickerObject` | `InternalPicker: NumberPicker`; `InternalNumber: decimal` | get                   |
|  [04]   | `NumberPickerObject` | `GripColour`; `SnapToTicks`                               | get/set               |
|  [05]   | `ToggleObject`       | `ToggleState`                                             | get/set               |
|  [06]   | `ToggleObject`       | `TrueName`; `FalseName`; `TrueInfo`; `FalseInfo`          | get/private set       |
|  [07]   | `ToggleObject`       | `StateNamesChanged`                                       | event                 |
|  [08]   | `ButtonObject`       | `Action`; `UpTree`; `DownTree`                            | get/set               |
|  [09]   | `ButtonObject`       | `UpColour`; `DownColour`; `UpText`; `DownText`            | get/set               |
|  [10]   | `ButtonObject`       | `Press()`; `Release()`                                    | method                |
|  [11]   | `ValueObject`        | `Text: string`; `Value: object`                           | get/private set       |
|  [12]   | `ValueObject`        | `Notations`; `AssignTextAndValue(string)`                 | get/set; method       |
|  [13]   | `TextInputObject`    | `Values: string[]`; `OneEntryPerLine`                     | get; get/internal set |
|  [14]   | `TextInputObject`    | `Contents`; `Escaping`                                    | get/set               |
|  [15]   | `ColourSwatchObject` | `Colour`; `Palette`; `Discrete`                           | get/internal set      |
|  [16]   | `ColourSwatchObject` | `SetColour(Colour, bool immediate)`                       | method                |

`ButtonObject(IPear up, IPear down)` accepts null pears; a null pear leaves the corresponding tree unset, and a null `down` selects `ButtonAction.Single`. `ValueObject.AssignTextAndValue` normalizes null or empty text to `Text == string.Empty` and `Value == null`. `TextInputObject.Values` returns an empty array rather than null, and `Contents` reads as an empty string when its backing value is null. `ColourSwatchObject` rejects a null constructor colour, while `SetColour` accepts a null colour at runtime.

[STATE_SCOPE]: editors and samplers

- rail: public members
- note: reference values accepted as `null` are normalized by the installed implementation where stated.

| [INDEX] | [TYPE]                 | [MEMBERS]                                                          | [ACCESS]        |
| :-----: | :--------------------- | :----------------------------------------------------------------- | :-------------- |
|  [01]   | `GradientEditorObject` | `Parameter0`; `Parameter1`; `Interaction`                          | get/set         |
|  [02]   | `GradientEditorObject` | `Gradient: Gradient`                                               | get             |
|  [03]   | `FunctionEditorObject` | `Editor: FunctionEditorBase`                                       | get             |
|  [04]   | `MaterialEditorObject` | `ForeRotation`; `BackRotation`; `IdenticalForeAndBack`; `Material` | get/set         |
|  [05]   | `ImageSamplerObject`   | `Image: Bitmap`                                                    | get/private set |
|  [06]   | `ImageSamplerObject`   | `DisplayImage: Bitmap`                                             | get             |
|  [07]   | `ImageSamplerObject`   | `ImageUri`; `Normalised`; `Luminance`                              | get/set         |
|  [08]   | `ImageSamplerObject`   | `LimitBehaviour`; `DrawSamples`                                    | get/set         |
|  [09]   | `HistogramObject`      | `Style`; `Palette`; `BucketCount`; `BucketRange`                   | get/set         |
|  [10]   | `HistogramObject`      | `BucketCountText`; `Pins`                                          | get             |
|  [11]   | `QuickGraphObject`     | `Pins`                                                             | get             |
|  [12]   | `ProtractorObject`     | `Mode`; `Angle`                                                    | get/set         |

`GradientEditorObject.Interaction` normalizes null to the `Matter` interaction. `FunctionEditorObject.Editor` is a read-only property carrying a mutable editor. `MaterialEditorObject.Material` replaces null lazily with a default material. `ImageSamplerObject.ImageUri` and `DisplayImage` are nullable; public image mutation is URI based.

[STATE_SCOPE]: pickers and lists

- rail: public members
- note: selected preset names and list values use distinct public mutation surfaces.

| [INDEX] | [TYPE]                 | [MEMBERS]                                                        | [ACCESS]         |
| :-----: | :--------------------- | :--------------------------------------------------------------- | :--------------- |
|  [01]   | `PresetPickerObject`   | `AvailablePresets`; `SelectedNames`                              | get              |
|  [02]   | `PresetPickerObject`   | `UserNames`; `MultiSelect`; `Scroll0`; `Scroll1`                 | get/set          |
|  [03]   | `ComplexPickerObject`  | `Values: Complex[]`                                              | get/internal set |
|  [04]   | `ConstantPickerObject` | `Constant: Maths.Constant`                                       | get/set          |
|  [05]   | `MetaNamePickerObject` | `MetaKey: MetaName`                                              | get/set          |
|  [06]   | `TemporalPickerObject` | `Date: DateTime`                                                 | get/internal set |
|  [07]   | `ValueListObject`      | `ItemCount`; `Items`; `Pear(int)`; `ItemSelected(int)`           | get; method      |
|  [08]   | `ValueListObject`      | `Mode`; `SelectPrev`; `SelectNext`; `SelectItem`; `DeselectItem` | get/set; method  |
|  [09]   | `ValueListObject`      | `StateChanged`                                                   | event            |

`PresetPickerObject.UserNames` accepts `null`, which denotes no user-authored selection. `SelectedNames` returns `UserNames`, then the available preset fallback, then an empty array; it does not return `null`. `SelectedPresets(out int[] indices, out IPreset[] presets)` returns equal-length, non-null arrays, including two empty arrays when no available preset resolves. `PresetCollection` is public, but its constructor is internal; consumers obtain it through `AvailablePresets`. The public delegate fields `AvailablePresetsChanged` and `SelectedPresetsChanged` are assignable fields rather than C# events.

`ValueListObject.Items` exposes value/meta pears only. The installed `ValueListItem` carrier is internal, `Set(ValueListItem[], bool)` is internal, and `RepairSelection()` is private. The SDK therefore has no public item-list replacement surface; public code can inspect items and mutate selection only.

[STATE_SCOPE]: data and utility objects

- rail: public members

| [INDEX] | [TYPE]               | [MEMBERS]                                                              | [ACCESS]        |
| :-----: | :------------------- | :--------------------------------------------------------------------- | :-------------- |
|  [01]   | `TimerObject`        | `Delay`; `Running`; `Manual`                                           | get/set         |
|  [02]   | `TimerObject`        | `DelayText`; `DelayDisplayText`; `TargetCount`; `TargetIds`; `Targets` | get             |
|  [03]   | `TimerObject`        | `IsTarget`; `AddTarget`; `RemoveTarget`                                | method          |
|  [04]   | `PathMapperObject`   | `Notation`; `OmitUnaffected`; `WarnAboutPaths`; `WarnAboutSites`       | get/set         |
|  [05]   | `PathMapperObject`   | `Mappings: PathMappings`                                               | get             |
|  [06]   | `DataPanelObject`    | `VerticalOffset`; `ShowColumns`; `ShowPaths`; `ShowIndices`            | get/set         |
|  [07]   | `DataPanelObject`    | `ShowTypes`; `ShowItems`; `ShowMetas`; `ChangeDisplay`                 | get/set; method |
|  [08]   | `DataRecorderObject` | `Paused`; `MergeTrees`; `FrameLimit`                                   | get/set         |
|  [09]   | `DataRecorderObject` | `IsEmpty`; `Valence`; `ClearRecordedData()`                            | get; method     |
|  [10]   | `TreeViewerObject`   | `CanvasDisplay`; `ViewportDisplay`; `DisplayGradient`                  | get/set         |

`TimerObject.TargetIds` returns an empty array rather than `null`, while `Targets` omits unresolved identifiers and yields no values when the timer has no document. `PathMapperObject.Notation` normalizes `null` to an empty string. `TreeViewerObject.DisplayGradient` is nullable. In the installed build, `DataRecorderObject.IsEmpty` evaluates `_buckets.Count > 0`; its returned value is therefore true when recorded buckets exist despite the member name and XML summary.

[STATE_SCOPE]: connection routing

- rail: public members

| [INDEX] | [TYPE]   | [MEMBERS]                                                   | [ACCESS]        |
| :-----: | :------- | :---------------------------------------------------------- | :-------------- |
|  [01]   | `Shout`  | `ClusterOutput`; `StreamData`; `StreamPath`; `StreamBackup` | get/set         |
|  [02]   | `Shout`  | `UpdateNomenBasedOnState()`                                 | method          |
|  [03]   | `Listen` | `ShoutId`; `CurrentDependency`                              | get             |
|  [04]   | `Listen` | `DependencyIndex`; `DependencyA`..`DependencyD`             | get/set         |
|  [05]   | `Listen` | `ClusterInput`; `ClusterIndex`; `UpdateNomenBasedOnState()` | get/set; method |
|  [06]   | `Relay`  | `Frozen`                                                    | get/set         |
|  [07]   | `Relay`  | `FrozenDataIsStale`; `FrozenCachedData`; `DisplayName`      | get             |
|  [08]   | `Relay`  | `SafeDisconnect()`; `ResolveDisplayName()`                  | method          |

`Shout.StreamPath` is nullable. `Listen.ShoutId` uses `Guid.Empty` when name-based resolution is active, and `CurrentDependency` returns null when `DependencyIndex` does not select one of the four dependency slots. The surrounding cluster lookup and binomial resolver methods are private or internal and are not consumer surfaces.

Setting `Relay.Frozen` to true captures `State.Data.Tree()` into `FrozenCachedData`, marks the cache current, suppresses recipient expiration, and serves the captured tree during collection. Upstream expiration marks the cache stale. Thawing clears the cache, expires the relay when the captured data became stale, and resets the stale flag. `FrozenCachedData` is therefore nullable whenever the relay is thawed or no cache was restored. `SafeDisconnect()` returns an `ActionList`, copies every relay input connection to every downstream parameter, disconnects both sides of the relay, refreshes downstream relay names, and expires the downstream parameters. `ResolveDisplayName()` derives the display name from `UserName` or the distinct sorted names propagated by upstream relays.

## [04]-[NON_PUBLIC_BOUNDARIES]

[IMAGE_SAMPLING]: the installed assembly declares `private static Colour SampleContinuous(Eto.Drawing.BitmapData data, int w, int h, float x, float y, SamplingLimit edge)`. The return carrier is `Grasshopper2.Types.Colour.Colour`, but the method is not callable through the public SDK. `ImageSamplerObject` exposes image assignment through `ImageUri` and performs coordinate sampling only inside its own solution processing.

[LIST_ASSIGNMENT]: `ValueListObject.Set(ValueListItem[], bool)` and `ValueListObject.RepairSelection()` are not public. A consumer cannot construct the internal `ValueListItem` type or replace the list through those members.

[CANVAS_ATTRIBUTES]: every concrete `CreateAttributes()` override in this catalogue is protected. Consumers obtain attributes from the inherited document-object surface after the object creates them; they do not invoke the concrete factory directly.
