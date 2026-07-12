# [RASM_APPUI_API_BEHAVIORS]

`Xaml.Behaviors.Avalonia` (MIT) is a meta-package with no `lib/` asset of its own. Its `net10.0` and `net8.0` dependency groups pull seven managed behavior assemblies: `Xaml.Behaviors.Interactivity` (the kernel), `Xaml.Behaviors.Interactions` (core, file-system, and network), `.Interactions.Custom` (the ~280-type behavior library), `.Interactions.DragAndDrop`, `.Interactions.Draggable`, `.Interactions.Events`, and `.Interactions.Responsive`. Every assembly ships only `lib/net8.0`, so the `net10.0` AppUi consumer binds the `net8.0` public API. The CLR namespace roots are `Avalonia.Xaml.Interactivity` and `Avalonia.Xaml.Interactions.*`, not the package identifier. The rail attaches declarative behavior, trigger, and action graphs to Avalonia controls, keeping view-models free of code-behind gesture, timing, picker, drag, clipboard, and HTTP plumbing.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Xaml.Behaviors.Avalonia` (meta) -> seven managed assemblies
- package: `Xaml.Behaviors.Avalonia`
- license: MIT
- asset: meta-package (no `lib/`); resolves to seven `lib/net8.0` assemblies + `Avalonia`
- consumer-bound TFM: `net8.0` assets under a `net10.0` consumer (no net10 lib ships)
- namespace root: `Avalonia.Xaml.Interactivity`, `Avalonia.Xaml.Interactions.*`
- rail: behaviors

Assembly and namespace cells omit the shared `Xaml.Behaviors.` and `Avalonia.Xaml.` roots.

| [INDEX] | [ASSEMBLY]                 | [NAMESPACE]                            | [CONCERN]                 |
| :-----: | :------------------------- | :------------------------------------- | :------------------------ |
|  [01]   | `Interactivity`            | `Interactivity`                        | kernel and attachment     |
|  [02]   | `Interactions`             | `Interactions.Core/FileSystem/Network` | core and boundary actions |
|  [03]   | `Interactions.Custom`      | `Interactions.Custom`                  | custom behavior library   |
|  [04]   | `Interactions.DragAndDrop` | `Interactions.DragAndDrop`             | managed drag and drop     |
|  [05]   | `Interactions.Draggable`   | `Interactions.Draggable`               | layout drag and reorder   |
|  [06]   | `Interactions.Events`      | `Interactions.Events`                  | routed-event pairs        |
|  [07]   | `Interactions.Responsive`  | `Interactions.Responsive`              | adaptive class setters    |

## [02]-[PUBLIC_TYPES]

[KERNEL_TYPES]: `Avalonia.Xaml.Interactivity` — bases and attach point
- rail: behaviors

The kernel is the only surface internal Rasm code derives from; everything else is XAML-attached. Bases are `AvaloniaObject` or `StyledElement` subclasses, so they participate in styling, binding, and resource inheritance.

Every row belongs to the behaviors rail. `ComparisonConditionType` admits `Equal`, `NotEqual`, `LessThan`, `LessThanOrEqual`, `GreaterThan`, and `GreaterThanOrEqual`.

| [INDEX] | [SYMBOL]                                       | [BASE_SHAPE]                                                |
| :-----: | :--------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `IBehavior`                                    | `{ AssociatedObject; Attach(AvaloniaObject?); Detach(); }`  |
|  [02]   | `ITrigger`                                     | `: IBehavior`                                               |
|  [03]   | `IAction`                                      | `object? Execute(object? sender, object? parameter)`        |
|  [04]   | `Behavior`                                     | `: AvaloniaObject, IBehavior, IBehaviorEventsHandler`       |
|  [05]   | `Behavior<T>`                                  | `: Behavior where T : AvaloniaObject`                       |
|  [06]   | `StyledElementBehavior`                        | `: StyledElement, IBehavior`                                |
|  [07]   | `StyledElementBehavior<T>`                     | `: StyledElementBehavior`                                   |
|  [08]   | `Trigger` / `Trigger<T>`                       | `: Behavior, ITrigger`                                      |
|  [09]   | `StyledElementTrigger` / `<T>`                 | `: StyledElementBehavior, ITrigger`                         |
|  [10]   | `Action`                                       | `: AvaloniaObject, IAction`                                 |
|  [11]   | `StyledElementAction`                          | `: StyledElement, IAction`                                  |
|  [12]   | `Interaction`                                  | static `BehaviorsProperty` attach point                     |
|  [13]   | `BehaviorCollection`                           | `: AvaloniaList<AvaloniaObject>` plus `Attach` and `Detach` |
|  [14]   | `ActionCollection`                             | `: AvaloniaList<AvaloniaObject>`                            |
|  [15]   | `Condition` / `ConditionCollection`            | conditional-attach predicates                               |
|  [16]   | `ComparisonConditionType`                      | data-trigger comparator enum                                |
|  [17]   | `EventTriggerBase` / `InvokeCommandActionBase` | event and command bases                                     |
|  [18]   | `DisposingBehavior` / `DisposingTrigger`       | `IDisposable`-scoped bases                                  |

[CORE_INTERACTION_TYPES]: `Avalonia.Xaml.Interactions.Core` — command, data, timing
- rail: behaviors

Every row belongs to the behaviors rail.

| [INDEX] | [SYMBOL]                                            | [BASE]                    | [CONCERN]                  |
| :-----: | :-------------------------------------------------- | :------------------------ | :------------------------- |
|  [01]   | `InvokeCommandAction`                               | `InvokeCommandActionBase` | command conversion         |
|  [02]   | `CallMethodAction`                                  | `StyledElementAction`     | reflection method call     |
|  [03]   | `ChangePropertyAction`                              | `StyledElementAction`     | direct property set        |
|  [04]   | `AsyncActionGroup`                                  | `StyledElementAction`     | sequential or parallel fan |
|  [05]   | `ThrottleAction` / `DebounceAction` / `DelayAction` | `StyledElementAction`     | rate-limited dispatch      |
|  [06]   | `EventTriggerBehavior`                              | `EventTriggerBase`        | routed-event action fan    |
|  [07]   | `DataTriggerBehavior`                               | `StyledElementTrigger`    | comparison gate            |
|  [08]   | `MultiDataTriggerBehavior`                          | `StyledElementTrigger`    | condition conjunction      |
|  [09]   | `ObservableStreamBehavior`                          | `StyledElementBehavior`   | observable action fan      |
|  [10]   | `ObservableTriggerBehavior`                         | `StyledElementTrigger`    | observable trigger         |
|  [11]   | `TimerTrigger`                                      | `StyledElementTrigger`    | interval trigger           |
|  [12]   | `TaskCompletedTrigger`                              | `StyledElementTrigger`    | task-completion trigger    |
|  [13]   | `DataTrigger` / `EventTrigger` / `MultiDataTrigger` | `StyledElementTrigger`    | action-carrying triggers   |

[SYSTEM_ACTION_TYPES]: picker / clipboard / file-system / network surfaces
- rail: behaviors

These boundary-touching actions remain behind explicit command and permission boundaries. Pickers route through Avalonia `IStorageProvider`.

| [INDEX] | [SYMBOL]                                                                   | [NAMESPACE]                | [CONCERN]            |
| :-----: | :------------------------------------------------------------------------- | :------------------------- | :------------------- |
|  [01]   | `OpenFilePickerAction` / `OpenFolderPickerAction` / `SaveFilePickerAction` | `…Interactions.Core`       | storage pickers      |
|  [02]   | `ButtonOpenFilePickerBehavior` / `MenuItem…` (open/folder/save)            | `…Interactions.Core`       | control pickers      |
|  [03]   | `GetClipboardTextAction` / `SetClipboardTextAction`                        | `…Interactions.Core`       | clipboard text       |
|  [04]   | `GetClipboardDataAction`                                                   | `…Interactions.Core`       | clipboard data read  |
|  [05]   | `SetClipboardDataObjectAction`                                             | `…Interactions.Core`       | clipboard data write |
|  [06]   | `GetClipboardFormatsAction`                                                | `…Interactions.Core`       | clipboard formats    |
|  [07]   | `ClearClipboardAction`                                                     | `…Interactions.Core`       | clipboard clear      |
|  [08]   | `WriteTextToFileAction`                                                    | `…Interactions.FileSystem` | text file write      |
|  [09]   | `CreateDirectoryAction` / `DeleteDirectoryAction` / `DeleteFileAction`     | `…Interactions.FileSystem` | file-system mutation |
|  [10]   | `FileSystemWatcherTrigger`                                                 | `…Interactions.FileSystem` | file-system watch    |
|  [11]   | `HttpRequestAction`                                                        | `…Interactions.Network`    | HTTP request         |
|  [12]   | `NetworkInformationTrigger`                                                | `…Interactions.Network`    | connectivity trigger |

[DRAG_DROP_TYPES]: drag/drop, draggable layout, routed events, responsive
- rail: behaviors

| [INDEX] | [SYMBOL]                                                     | [NAMESPACE]    | [CONCERN]               |
| :-----: | :----------------------------------------------------------- | :------------- | :---------------------- |
|  [01]   | `ContextDragBehavior` / `ContextDropBehavior`                | `…DragAndDrop` | data-context transfer   |
|  [02]   | `TypedDragBehavior` / `IDragHandler` / `IDropHandler`        | `…DragAndDrop` | typed transfer          |
|  [03]   | `DropHandlerBase`                                            | `…DragAndDrop` | drop-handler base       |
|  [04]   | `FilesDropBehavior` / `FilesPreviewBehavior`                 | `…DragAndDrop` | file drop and preview   |
|  [05]   | `ContentControlFilesDropBehavior`                            | `…DragAndDrop` | content-control drop    |
|  [06]   | `ManagedDragDropService` / `ManagedContextDropArgs`          | `…DragAndDrop` | cross-window transfer   |
|  [07]   | `CanvasDragBehavior` / `GridDragBehavior`                    | `…Draggable`   | container drag          |
|  [08]   | `ItemDragBehavior` / `ListReorderDragBehavior`               | `…Draggable`   | item reorder            |
|  [09]   | `MouseDragElementBehavior` / `MultiMouseDragElementBehavior` | `…Draggable`   | free drag               |
|  [10]   | `AutoScrollDuringDragBehavior`                               | `…Draggable`   | drag auto-scroll        |
|  [11]   | `<Event>EventTrigger` / `<Event>EventBehavior`               | `…Events`      | typed routed-event pair |
|  [12]   | `AdaptiveBehavior` / `AdaptiveClassSetter`                   | `…Responsive`  | dimension breakpoints   |
|  [13]   | `AspectRatioBehavior` / `AspectRatioClassSetter`             | `…Responsive`  | ratio breakpoints       |

## [03]-[ENTRYPOINTS]

[ATTACH_ENTRYPOINTS]: `Interaction` static accessor + base lifecycle
- rail: behaviors

XAML attaches a `BehaviorCollection` via `i:Interaction.Behaviors`; code paths use the static accessors. `Behavior` exposes a full virtual lifecycle beyond attach and detach, including visual-tree, logical-tree, loaded, unloaded, data-context, and theme-variant hooks that an Rx-scoped `DisposingBehavior` uses to key subscriptions.

| [INDEX] | [SURFACE]                                                                                     | [SURFACE_ROOT] | [RAIL]               |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------- | :------------------- |
|  [01]   | `Interaction.BehaviorsProperty` (`AttachedProperty<BehaviorCollection?>`)                     | `Interaction`  | XAML attach point    |
|  [02]   | `GetBehaviors(AvaloniaObject) -> BehaviorCollection`                                          | `Interaction`  | behavior lookup      |
|  [03]   | `SetBehaviors(AvaloniaObject, BehaviorCollection?)`                                           | `Interaction`  | behavior assignment  |
|  [04]   | `ExecuteActions(object? sender, ActionCollection?, object? parameter) -> IEnumerable<object>` | `Interaction`  | manual action fan    |
|  [05]   | `Attach(AvaloniaObject?)` / `Detach()`                                                        | `Behavior`     | attach lifecycle     |
|  [06]   | `OnAttached()` / `OnDetaching()`                                                              | `Behavior`     | core lifecycle hooks |
|  [07]   | `OnAttachedToVisualTree()` / `OnDetachedFromVisualTree()`                                     | `Behavior`     | visual-tree hooks    |
|  [08]   | `OnLoaded()` / `OnUnloaded()`                                                                 | `Behavior`     | load lifecycle       |
|  [09]   | `OnDataContextChangedEvent()`                                                                 | `Behavior`     | data-context hook    |
|  [10]   | `OnActualThemeVariantChangedEvent()`                                                          | `Behavior`     | theme-variant hook   |
|  [11]   | `AssociatedObject` / `IsEnabledProperty`                                                      | `Behavior`     | bound target + gate  |
|  [12]   | `Execute(object? sender, object? parameter) -> object?`                                       | `IAction`      | action execution     |

[COMMAND_ACTION_ENTRYPOINTS]: command, async-group, and timing properties
- rail: behaviors

`InvokeCommandActionBase` is the full MVVM command seam: it carries `Command`, `CommandParameter`, and an `IValueConverter` input pipeline that maps the event-args payload before invocation.

The timing types `ThrottleAction`, `DebounceAction`, `DelayAction`, and `TimerTrigger` carry `Interval` or `Delay` as `TimeSpan` policy.

| [INDEX] | [SURFACE]                                         | [SURFACE_ROOT]             | [CONCERN]          |
| :-----: | :------------------------------------------------ | :------------------------- | :----------------- |
|  [01]   | `Command` (`ICommand?`)                           | `InvokeCommandActionBase`  | command source     |
|  [02]   | `CommandParameter` (`object?`)                    | `InvokeCommandActionBase`  | command input      |
|  [03]   | `InputConverter` (`IValueConverter?`)             | `InvokeCommandActionBase`  | event conversion   |
|  [04]   | `InputConverterParameter`                         | `InvokeCommandActionBase`  | converter input    |
|  [05]   | `InputConverterLanguage`                          | `InvokeCommandActionBase`  | converter language |
|  [06]   | `PassEventArgsToCommand` (`bool`)                 | `InvokeCommandActionBase`  | raw event args     |
|  [07]   | `Mode` (`AsyncActionMode` Sequence/Parallel)      | `AsyncActionGroup`         | fan policy         |
|  [08]   | `Actions` (`ActionCollection?`)                   | `AsyncActionGroup`         | child actions      |
|  [09]   | `Binding` (`object?`)                             | `DataTriggerBehavior`      | compared input     |
|  [10]   | `ComparisonCondition` (`ComparisonConditionType`) | `DataTriggerBehavior`      | comparator         |
|  [11]   | `Value` (`object?`)                               | `DataTriggerBehavior`      | comparison value   |
|  [12]   | `EventName` (`string`)                            | `EventTriggerBehavior`     | routed event       |
|  [13]   | `SourceObject`                                    | `EventTriggerBehavior`     | event source       |
|  [14]   | `Source` (`IObservable<>`)                        | `ObservableStreamBehavior` | stream source      |
|  [15]   | `Actions`                                         | `ObservableStreamBehavior` | emitted action fan |

[SYSTEM_ENTRYPOINTS]: picker, clipboard, file, drag, responsive properties
- rail: behaviors

`FileTypeFilter` strings are parsed through `FileFilterParser`.

| [INDEX] | [SURFACE]                                   | [SURFACE_ROOT]                                       | [CONCERN]          |
| :-----: | :------------------------------------------ | :--------------------------------------------------- | :----------------- |
|  [01]   | `AllowMultiple` (`bool`)                    | `OpenFilePickerAction` family                        | picker cardinality |
|  [02]   | `FileTypeFilter` (`string?`)                | `OpenFilePickerAction` family                        | picker filter      |
|  [03]   | `Text` (`string`)                           | `SetClipboardTextAction`                             | clipboard write    |
|  [04]   | `Path`                                      | `WriteTextToFileAction` / `FileSystemWatcherTrigger` | file path          |
|  [05]   | `Url`                                       | `HttpRequestAction`                                  | request URL        |
|  [06]   | `Method` (`"GET"`)                          | `HttpRequestAction`                                  | request method     |
|  [07]   | `Content` / `ContentType`                   | `HttpRequestAction`                                  | request body       |
|  [08]   | `ResponseContent` / `ResponseStatusCode`    | `HttpRequestAction`                                  | response capture   |
|  [09]   | `Handler` (`IDragHandler`/`IDropHandler`)   | `ContextDragBehavior` / `TypedDragBehavior`          | transfer handler   |
|  [10]   | `PlaceholderTemplate`                       | `ListReorderDragBehavior`                            | drop placeholder   |
|  [11]   | `ClassSetters` (`AdaptiveClassSetter` rows) | `AdaptiveBehavior` / `AspectRatioBehavior`           | breakpoint classes |

## [04]-[INTEGRATION]

[STACK_REACTIVEUI_COMMANDS]:
- `api-reactiveui.md` `ReactiveCommand<TIn,TOut>` (an `ICommand`) binds directly into
  `InvokeCommandAction.Command`; an `EventTriggerBehavior` on a control event fans the
  routed `EventArgs` through `InputConverter` into the command parameter, so a control
  gesture drives a view-model `ReactiveCommand` with zero code-behind. `CanExecute`
  observables from ReactiveUI gate the action through the same `ICommand` contract.

[STACK_RX_STREAMS]:
- `ObservableStreamBehavior.Source` / `ObservableTriggerBehavior` accept any
  `IObservable<>`. `api-reactive.md` (`System.Reactive`) operators and
  `api-dynamicdata.md` change-set streams (`.ToCollection()`, `.QueryWhenChanged()`)
  project into that source so collection/state changes drive `ActionCollection` fans —
  the same Rx pipeline that feeds a `ReactiveObject` also drives view affordances.
  `ThrottleAction`/`DebounceAction` collapse with `Observable.Throttle` upstream; pick
  the behavior when the rate limit is a view concern and the operator when it is a
  data-stream concern.

[STACK_STORAGE_BOUNDARY]:
- The picker actions resolve `IStorageProvider` from the attached control's `TopLevel`
  (`api-avalonia.md`). The selected `IStorageFile`/`IStorageFolder` is the canonical
  AppHost file-boundary token; AppUi maps it to a domain path at the command edge
  rather than letting the action mutate the file system directly. `WriteTextToFileAction`
  and the file-system actions are gated behind the same permission boundary.

[STACK_DRAG_TYPED]:
- `TypedDragBehavior` + `IDropHandler`/`DropHandlerBase` carry a strongly-typed payload;
  `ManagedDragDropService` moves it across windows. Bind the drop handler to a
  view-model command so reorder/drop mutations route through the same `ReactiveCommand`
  rail as every other state change, keeping `DynamicData` source lists as the single
  reorder authority.

## [05]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Xaml.Behaviors.Avalonia` (seven managed assemblies, MIT)
- Owns: declarative behavior/trigger/action attachment across command, data, timing,
  picker, clipboard, file-system, network, drag/drop, routed-event, and responsive concerns
- Accept: control adaptation expressed as an attached `Behavior`/`Trigger`/`Action`; the
  base hierarchy (`Behavior`/`StyledElementBehavior`/`Trigger`/`StyledElementAction`) for
  any Rasm-owned behavior
- Reject: code-behind gesture/timing/picker forks; re-implementing throttle/debounce/drag
  plumbing that an admitted behavior already owns

[NAMESPACE_LAW]:
- CLR namespaces are `Avalonia.Xaml.Interactivity` and `Avalonia.Xaml.Interactions.{Core,Custom,FileSystem,Network,DragAndDrop,Draggable,Events,Responsive}`; the
  `Xaml.Behaviors.*` tokens are package/assembly ids only and never appear in `using` or XAML `xmlns`
- The kernel (`…Interactivity`) is the sole derive-from surface; all other assemblies are XAML-attached leaves

[BASE_SELECTION_LAW]:
- Derive Rasm behaviors from `StyledElementBehavior`/`StyledElementTrigger` (not `Behavior`/`Trigger`)
  when the behavior must participate in styling, resource inheritance, or `{Binding}` on its own
  properties; derive from `StyledElementAction` for actions
- Use `DisposingBehavior`/`DisposingTrigger` as the base whenever the behavior owns Rx subscriptions,
  so attach/detach drives subscription lifecycle deterministically

[CONDITION_LAW]:
- Data gating is `DataTriggerBehavior` (single `ComparisonConditionType` predicate) or
  `MultiDataTriggerBehavior` (AND of conditions); `ComparisonConditionType` is the canonical
  comparator enum — never hand-roll value comparison in a converter
