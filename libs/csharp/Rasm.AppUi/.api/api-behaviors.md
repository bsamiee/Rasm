# [RASM_APPUI_API_BEHAVIORS]

`Xaml.Behaviors.Avalonia` attaches declarative behavior, trigger, and action graphs to Avalonia controls, holding view-models free of code-behind gesture, timing, picker, drag, clipboard, and HTTP plumbing. Kernel `Avalonia.Xaml.Interactivity` is the sole derive-from base; every other assembly attaches as a XAML leaf, feeding the behaviors rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Xaml.Behaviors.Avalonia`
- package: `Xaml.Behaviors.Avalonia` (MIT)
- assembly: meta-package with no own `lib/`; resolves to `Avalonia` and the `Xaml.Behaviors.*` `lib/net8.0` assemblies the `[01]` table rosters
- namespace: `Avalonia.Xaml.Interactivity`, `Avalonia.Xaml.Interactions.*` (never the `Xaml.Behaviors.*` package tokens)
- target: `net8.0` assets bind under the `net10.0` consumer
- rail: behaviors

Assembly and namespace cells drop the shared `Xaml.Behaviors.` and `Avalonia.Xaml.` roots.

| [INDEX] | [ASSEMBLY]                 | [NAMESPACE]                            | [CAPABILITY]              |
| :-----: | :------------------------- | :------------------------------------- | :------------------------ |
|  [01]   | `Interactivity`            | `Interactivity`                        | kernel and attachment     |
|  [02]   | `Interactions`             | `Interactions.Core/FileSystem/Network` | core and boundary actions |
|  [03]   | `Interactions.Custom`      | `Interactions.Custom`                  | custom behavior library   |
|  [04]   | `Interactions.DragAndDrop` | `Interactions.DragAndDrop`             | managed drag and drop     |
|  [05]   | `Interactions.Draggable`   | `Interactions.Draggable`               | layout drag and reorder   |
|  [06]   | `Interactions.Events`      | `Interactions.Events`                  | routed-event pairs        |
|  [07]   | `Interactions.Responsive`  | `Interactions.Responsive`              | adaptive class setters    |

## [02]-[PUBLIC_TYPES]

[KERNEL_TYPES]: `Avalonia.Xaml.Interactivity` — behavior/trigger/action bases, attach point, comparator enum

`[ComparisonConditionType]`: `Equal` `NotEqual` `LessThan` `LessThanOrEqual` `GreaterThan` `GreaterThanOrEqual`

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :--------------------------------------------- | :------------ | :------------------------------------ |
|  [01]   | `IBehavior` / `ITrigger` / `IAction`           | interface     | attach, fire, and execute contracts   |
|  [02]   | `Behavior` / `Behavior<T>`                     | behavior base | `AvaloniaObject` behavior base        |
|  [03]   | `StyledElementBehavior` / `<T>`                | behavior base | styling-aware behavior base           |
|  [04]   | `Trigger` / `Trigger<T>`                       | trigger base  | action-firing trigger base            |
|  [05]   | `StyledElementTrigger` / `<T>`                 | trigger base  | styling-aware trigger base            |
|  [06]   | `Action` / `StyledElementAction`               | action base   | executable-action bases               |
|  [07]   | `Interaction`                                  | static        | XAML `BehaviorsProperty` attach point |
|  [08]   | `BehaviorCollection` / `ActionCollection`      | collection    | attached behavior and action lists    |
|  [09]   | `Condition` / `ConditionCollection`            | class         | conditional-attach predicates         |
|  [10]   | `ComparisonConditionType`                      | enum          | data-trigger comparator               |
|  [11]   | `EventTriggerBase` / `InvokeCommandActionBase` | abstract base | event and command bases               |
|  [12]   | `DisposingBehavior` / `DisposingTrigger`       | behavior base | `IDisposable`-scoped bases            |

[CORE_INTERACTION_TYPES]: `Avalonia.Xaml.Interactions.Core` — command, data, timing, picker, clipboard

| [INDEX] | [SYMBOL]                                                                   | [TYPE_FAMILY] | [CAPABILITY]                |
| :-----: | :------------------------------------------------------------------------- | :------------ | :-------------------------- |
|  [01]   | `InvokeCommandAction`                                                      | action        | `ICommand` conversion       |
|  [02]   | `CallMethodAction`                                                         | action        | reflection method call      |
|  [03]   | `ChangePropertyAction`                                                     | action        | direct property set         |
|  [04]   | `AsyncActionGroup`                                                         | action        | sequential or parallel fan  |
|  [05]   | `ThrottleAction` / `DebounceAction` / `DelayAction`                        | action        | rate-limited dispatch       |
|  [06]   | `EventTriggerBehavior`                                                     | trigger       | routed-event action fan     |
|  [07]   | `DataTriggerBehavior`                                                      | trigger       | comparison gate             |
|  [08]   | `MultiDataTriggerBehavior`                                                 | trigger       | condition conjunction       |
|  [09]   | `ObservableStreamBehavior`                                                 | behavior      | observable action fan       |
|  [10]   | `ObservableTriggerBehavior`                                                | trigger       | observable trigger          |
|  [11]   | `TimerTrigger`                                                             | trigger       | interval trigger            |
|  [12]   | `TaskCompletedTrigger`                                                     | trigger       | task-completion trigger     |
|  [13]   | `DataTrigger` / `EventTrigger` / `MultiDataTrigger`                        | trigger       | action-carrying triggers    |
|  [14]   | `OpenFilePickerAction` / `OpenFolderPickerAction` / `SaveFilePickerAction` | action        | storage pickers             |
|  [15]   | `ButtonOpenFilePickerBehavior` / `MenuItem…` pickers                       | behavior      | control-hosted pickers      |
|  [16]   | `GetClipboardTextAction` / `SetClipboardTextAction`                        | action        | clipboard text              |
|  [17]   | `GetClipboardDataAction` / `SetClipboardDataObjectAction`                  | action        | clipboard data read/write   |
|  [18]   | `GetClipboardFormatsAction` / `ClearClipboardAction`                       | action        | clipboard formats and clear |

[FILESYSTEM_TYPES]: `Avalonia.Xaml.Interactions.FileSystem`

| [INDEX] | [SYMBOL]                                                               | [TYPE_FAMILY] | [CAPABILITY]         |
| :-----: | :--------------------------------------------------------------------- | :------------ | :------------------- |
|  [01]   | `WriteTextToFileAction`                                                | action        | text file write      |
|  [02]   | `CreateDirectoryAction` / `DeleteDirectoryAction` / `DeleteFileAction` | action        | file-system mutation |
|  [03]   | `FileSystemWatcherTrigger`                                             | trigger       | file-system watch    |

[NETWORK_TYPES]: `Avalonia.Xaml.Interactions.Network`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]         |
| :-----: | :-------------------------- | :------------ | :------------------- |
|  [01]   | `HttpRequestAction`         | action        | HTTP request         |
|  [02]   | `NetworkInformationTrigger` | trigger       | connectivity trigger |

[DRAGDROP_TYPES]: `Avalonia.Xaml.Interactions.DragAndDrop`

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [CAPABILITY]           |
| :-----: | :-------------------------------------------------- | :------------ | :--------------------- |
|  [01]   | `ContextDragBehavior` / `ContextDropBehavior`       | behavior      | data-context transfer  |
|  [02]   | `TypedDragBehavior`                                 | behavior      | typed transfer         |
|  [03]   | `IDragHandler` / `IDropHandler`                     | interface     | typed transfer handler |
|  [04]   | `DropHandlerBase`                                   | class         | drop-handler base      |
|  [05]   | `FilesDropBehavior` / `FilesPreviewBehavior`        | behavior      | file drop and preview  |
|  [06]   | `ContentControlFilesDropBehavior`                   | behavior      | content-control drop   |
|  [07]   | `ManagedDragDropService` / `ManagedContextDropArgs` | service       | cross-window transfer  |

[DRAGGABLE_TYPES]: `Avalonia.Xaml.Interactions.Draggable`

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY] | [CAPABILITY]     |
| :-----: | :----------------------------------------------------------- | :------------ | :--------------- |
|  [01]   | `CanvasDragBehavior` / `GridDragBehavior`                    | behavior      | container drag   |
|  [02]   | `ItemDragBehavior` / `ListReorderDragBehavior`               | behavior      | item reorder     |
|  [03]   | `MouseDragElementBehavior` / `MultiMouseDragElementBehavior` | behavior      | free drag        |
|  [04]   | `AutoScrollDuringDragBehavior`                               | behavior      | drag auto-scroll |

[EVENT_TYPES]: `Avalonia.Xaml.Interactions.Events` — `<Event>EventTrigger` / `<Event>EventBehavior` mint one typed routed-event trigger and behavior pair per Avalonia control event.

[RESPONSIVE_TYPES]: `Avalonia.Xaml.Interactions.Responsive`

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :----------------------------------------------- | :------------ | :-------------------- |
|  [01]   | `AdaptiveBehavior` / `AdaptiveClassSetter`       | behavior      | dimension breakpoints |
|  [02]   | `AspectRatioBehavior` / `AspectRatioClassSetter` | behavior      | ratio breakpoints     |

## [03]-[ENTRYPOINTS]

[ATTACH_ENTRYPOINTS]: `Interaction` static accessors and `Behavior` lifecycle — XAML attaches a `BehaviorCollection` via `i:Interaction.Behaviors`; code paths use the static accessors, and a `DisposingBehavior` keys Rx subscriptions to the lifecycle hooks.

| [INDEX] | [SURFACE]                                                                                | [SHAPE]  | [CAPABILITY]         |
| :-----: | :--------------------------------------------------------------------------------------- | :------- | :------------------- |
|  [01]   | `Interaction.BehaviorsProperty` (`AttachedProperty<BehaviorCollection?>`)                | attached | XAML attach point    |
|  [02]   | `Interaction.GetBehaviors(AvaloniaObject) -> BehaviorCollection`                         | static   | behavior lookup      |
|  [03]   | `Interaction.SetBehaviors(AvaloniaObject, BehaviorCollection?)`                          | static   | behavior assignment  |
|  [04]   | `Interaction.ExecuteActions(object?, ActionCollection?, object?) -> IEnumerable<object>` | static   | manual action fan    |
|  [05]   | `Behavior.Attach(AvaloniaObject?)` / `Detach()`                                          | instance | attach lifecycle     |
|  [06]   | `Behavior.OnAttached()` / `OnDetaching()`                                                | instance | core lifecycle hooks |
|  [07]   | `Behavior.OnAttachedToVisualTree()` / `OnDetachedFromVisualTree()`                       | instance | visual-tree hooks    |
|  [08]   | `Behavior.OnLoaded()` / `OnUnloaded()`                                                   | instance | load lifecycle       |
|  [09]   | `Behavior.OnDataContextChangedEvent()`                                                   | instance | data-context hook    |
|  [10]   | `Behavior.OnActualThemeVariantChangedEvent()`                                            | instance | theme-variant hook   |
|  [11]   | `Behavior.AssociatedObject` / `IsEnabledProperty`                                        | property | bound target + gate  |
|  [12]   | `IAction.Execute(object?, object?) -> object?`                                           | instance | action execution     |

[COMMAND_ACTION_ENTRYPOINTS]: command, async-group, and trigger properties, all `AvaloniaProperty` — `InvokeCommandActionBase` is the full MVVM command seam, mapping the event-args payload through an `IValueConverter` before invocation; `ThrottleAction`/`DebounceAction`/`DelayAction`/`TimerTrigger` carry `Interval` or `Delay` as `TimeSpan`.

| [INDEX] | [SURFACE]                                                     | [CAPABILITY]       |
| :-----: | :------------------------------------------------------------ | :----------------- |
|  [01]   | `InvokeCommandActionBase.Command` (`ICommand?`)               | command source     |
|  [02]   | `InvokeCommandActionBase.CommandParameter` (`object?`)        | command input      |
|  [03]   | `InvokeCommandActionBase.InputConverter` (`IValueConverter?`) | event conversion   |
|  [04]   | `InvokeCommandActionBase.InputConverterParameter`             | converter input    |
|  [05]   | `InvokeCommandActionBase.InputConverterLanguage`              | converter language |
|  [06]   | `InvokeCommandActionBase.PassEventArgsToCommand` (`bool`)     | raw event args     |
|  [07]   | `AsyncActionGroup.Mode` (`AsyncActionMode`)                   | fan policy         |
|  [08]   | `AsyncActionGroup.Actions` (`ActionCollection?`)              | child actions      |
|  [09]   | `DataTriggerBehavior.Binding` (`object?`)                     | compared input     |
|  [10]   | `DataTriggerBehavior.ComparisonCondition`                     | comparator         |
|  [11]   | `DataTriggerBehavior.Value` (`object?`)                       | comparison value   |
|  [12]   | `EventTriggerBehavior.EventName` (`string`)                   | routed event       |
|  [13]   | `EventTriggerBehavior.SourceObject`                           | event source       |
|  [14]   | `ObservableStreamBehavior.Source` (`IObservable<>`)           | stream source      |
|  [15]   | `ObservableStreamBehavior.Actions`                            | emitted action fan |

[SYSTEM_ENTRYPOINTS]: picker, clipboard, file, network, drag, and responsive properties, all `AvaloniaProperty` — `FileTypeFilter` strings parse through `FileFilterParser`.

| [INDEX] | [SURFACE]                                                            | [CAPABILITY]       |
| :-----: | :------------------------------------------------------------------- | :----------------- |
|  [01]   | `OpenFilePickerAction.AllowMultiple` (`bool`)                        | picker cardinality |
|  [02]   | `OpenFilePickerAction.FileTypeFilter` (`string?`)                    | picker filter      |
|  [03]   | `SetClipboardTextAction.Text` (`string`)                             | clipboard write    |
|  [04]   | `WriteTextToFileAction.Path` / `FileSystemWatcherTrigger.Path`       | file path          |
|  [05]   | `HttpRequestAction.Url`                                              | request URL        |
|  [06]   | `HttpRequestAction.Method` (`"GET"`)                                 | request method     |
|  [07]   | `HttpRequestAction.Content` / `ContentType`                          | request body       |
|  [08]   | `HttpRequestAction.ResponseContent` / `ResponseStatusCode`           | response capture   |
|  [09]   | `ContextDragBehavior.Handler` / `TypedDragBehavior.Handler`          | transfer handler   |
|  [10]   | `ListReorderDragBehavior.PlaceholderTemplate`                        | drop placeholder   |
|  [11]   | `AdaptiveBehavior.ClassSetters` / `AspectRatioBehavior.ClassSetters` | breakpoint classes |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Kernel `…Interactivity` is the sole derive-from surface; every other assembly attaches as a XAML leaf.
- `using` and XAML `xmlns` bind the CLR roots `Avalonia.Xaml.Interactivity` and `Avalonia.Xaml.Interactions.{Core,Custom,FileSystem,Network,DragAndDrop,Draggable,Events,Responsive}`; the `Xaml.Behaviors.*` tokens are package and assembly ids only.
- Bases subclass `AvaloniaObject` or `StyledElement`, so every behavior participates in styling, binding, and resource inheritance.

[STACKING]:
- `ReactiveUI`(`api-reactiveui.md`): a `ReactiveCommand<TIn,TOut>` (an `ICommand`) binds into `InvokeCommandAction.Command`, an `EventTriggerBehavior` fans the routed `EventArgs` through `InputConverter` into the command parameter, and `CanExecute` observables gate through the same `ICommand`, so a control gesture drives a view-model command with zero code-behind.
- `System.Reactive`(`api-reactive.md`): operators feed `ObservableStreamBehavior.Source`/`ObservableTriggerBehavior`, and an upstream `Observable.Throttle` collapses with `ThrottleAction`/`DebounceAction` — the behavior owns a view-rate limit, the operator a data-stream limit.
- `DynamicData`(`api-dynamicdata.md`): `.ToCollection()`/`.QueryWhenChanged()` change-set streams project into the observable behavior source, so collection and state changes drive `ActionCollection` fans off the same pipeline that feeds a `ReactiveObject`.
- `Avalonia`(`api-avalonia.md`): picker actions resolve `StorageProvider` via `TopLevel.GetTopLevel(control)`, and the selected `IStorageFile`/`IStorageFolder` is the file-boundary token AppUi maps to a domain path at the command edge.
- within-lib: `TypedDragBehavior` with `IDropHandler`/`DropHandlerBase` carries a typed payload and `ManagedDragDropService` moves it cross-window; binding the drop handler to a view-model `ReactiveCommand` routes reorder and drop mutations through the state rail, keeping `DynamicData` source lists the single reorder authority.

[LOCAL_ADMISSION]:
- Derive a Rasm behavior from `StyledElementBehavior`/`StyledElementTrigger`/`StyledElementAction` when it participates in styling, resource inheritance, or `{Binding}` on its own properties; from `Behavior`/`Trigger` otherwise.
- Base a behavior owning Rx subscriptions on `DisposingBehavior`/`DisposingTrigger`, so attach and detach drive subscription lifecycle deterministically.
- Gate data with `DataTriggerBehavior` (single `ComparisonConditionType` predicate) or `MultiDataTriggerBehavior` (AND of `Condition` rows); `ComparisonConditionType` is the canonical comparator.
- Picker, file-system, and network actions stay behind the command and permission boundary, mapping the selected token to a domain path at the command edge rather than mutating the file system in place.

[RAIL_LAW]:
- Package: `Xaml.Behaviors.Avalonia`
- Owns: declarative behavior, trigger, and action attachment across command, data, timing, picker, clipboard, file-system, network, drag/drop, routed-event, and responsive concerns
- Accept: control adaptation expressed as an attached `Behavior`/`Trigger`/`Action`; the base hierarchy for any Rasm-owned behavior
- Reject: code-behind gesture, timing, or picker forks; re-implementing throttle, debounce, or drag plumbing an admitted behavior owns
