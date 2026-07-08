# [RASM_APPUI_API_BEHAVIORS]

`Xaml.Behaviors.Avalonia` (MIT) is a meta-package: it has no `lib/`
asset of its own, only `net10.0`/`net8.0` dependency groups that pull seven
managed behavior assemblies — `Xaml.Behaviors.Interactivity` (the kernel),
`Xaml.Behaviors.Interactions` (core + file-system + network), `.Interactions.Custom`
(the ~280-type behavior library), `.Interactions.DragAndDrop`,
`.Interactions.Draggable`, `.Interactions.Events`, and `.Interactions.Responsive`.
Every assembly ships `lib/net8.0` only; the net10.0 AppUi consumer binds the
net8.0 asset, so the surface is the net8.0 public API. The CLR namespace root is
`Avalonia.Xaml.Interactivity` and `Avalonia.Xaml.Interactions.*` — NOT the package
id. The rail attaches declarative behavior/trigger/action graphs to Avalonia
controls so view-models stay free of code-behind gesture, timing, picker, drag,
clipboard, and HTTP plumbing.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Xaml.Behaviors.Avalonia` (meta) -> seven managed assemblies
- package: `Xaml.Behaviors.Avalonia`
- license: MIT
- asset: meta-package (no `lib/`); resolves to seven `lib/net8.0` assemblies + `Avalonia`
- consumer-bound TFM: `net8.0` assets under a `net10.0` consumer (no net10 lib ships)
- namespace root: `Avalonia.Xaml.Interactivity`, `Avalonia.Xaml.Interactions.*`
- rail: behaviors

| [INDEX] | [ASSEMBLY]                            | [NAMESPACE]                                      | [CONCERN]                                          |
| :-----: | :------------------------------------ | :----------------------------------------------- | :------------------------------------------------- |
|  [01]   | `Xaml.Behaviors.Interactivity`        | `Avalonia.Xaml.Interactivity`                    | kernel: behavior/trigger/action bases, `Interaction` attach |
|  [02]   | `Xaml.Behaviors.Interactions`         | `…Interactions.Core` / `.FileSystem` / `.Network`| command/data/event/timer/picker/clipboard/file/HTTP actions |
|  [03]   | `Xaml.Behaviors.Interactions.Custom`  | `Avalonia.Xaml.Interactions.Custom`              | ~280 focus/animation/validation/window/scroll behaviors |
|  [04]   | `Xaml.Behaviors.Interactions.DragAndDrop` | `Avalonia.Xaml.Interactions.DragAndDrop`     | context/typed drag, drop handlers, managed payload transfer |
|  [05]   | `Xaml.Behaviors.Interactions.Draggable`   | `Avalonia.Xaml.Interactions.Draggable`       | canvas/grid/list reorder + mouse-drag-element behaviors |
|  [06]   | `Xaml.Behaviors.Interactions.Events`  | `Avalonia.Xaml.Interactions.Events`              | one event-trigger + event-behavior pair per routed event |
|  [07]   | `Xaml.Behaviors.Interactions.Responsive`  | `Avalonia.Xaml.Interactions.Responsive`      | breakpoint/aspect-ratio adaptive class setters     |

## [02]-[PUBLIC_TYPES]

[KERNEL_TYPES]: `Avalonia.Xaml.Interactivity` — bases and attach point
- rail: behaviors

The kernel is the only surface internal Rasm code derives from; everything else
is XAML-attached. Bases are `AvaloniaObject`/`StyledElement` subclasses, so they
participate in styling, binding, and resource inheritance.

| [INDEX] | [SYMBOL]                     | [BASE_SHAPE]                                  | [RAIL]                          |
| :-----: | :--------------------------- | :--------------------------------------------- | :------------------------------ |
|  [01]   | `IBehavior`                  | `{ AssociatedObject; Attach(AvaloniaObject?); Detach(); }` | behavior contract    |
|  [02]   | `ITrigger`                   | `: IBehavior`                                  | trigger contract                |
|  [03]   | `IAction`                    | `object? Execute(object? sender, object? parameter)` | action contract           |
|  [04]   | `Behavior`                   | `: AvaloniaObject, IBehavior, IBehaviorEventsHandler` | behavior base            |
|  [05]   | `Behavior<T>`                | `: Behavior where T : AvaloniaObject`          | typed behavior base             |
|  [06]   | `StyledElementBehavior`      | `: StyledElement, IBehavior`                   | stylable/bindable behavior base |
|  [07]   | `StyledElementBehavior<T>`   | `: StyledElementBehavior`                      | typed stylable behavior         |
|  [08]   | `Trigger` / `Trigger<T>`     | `: Behavior, ITrigger`                         | trigger base                    |
|  [09]   | `StyledElementTrigger` / `<T>` | `: StyledElementBehavior, ITrigger`          | stylable trigger base           |
|  [10]   | `Action`                     | `: AvaloniaObject, IAction`                    | action base                     |
|  [11]   | `StyledElementAction`        | `: StyledElement, IAction`                     | stylable action base (most actions derive here) |
|  [12]   | `Interaction`                | static — `BehaviorsProperty` attach point      | behavior attachment accessor    |
|  [13]   | `BehaviorCollection`         | `: AvaloniaList<AvaloniaObject>` + `Attach`/`Detach` | behavior collection       |
|  [14]   | `ActionCollection`           | `: AvaloniaList<AvaloniaObject>`               | action collection               |
|  [15]   | `Condition` / `ConditionCollection` | conditional-attach predicates           | conditional behavior gate       |
|  [16]   | `ComparisonConditionType`    | enum: `Equal NotEqual LessThan LessThanOrEqual GreaterThan GreaterThanOrEqual` | data-trigger comparator |
|  [17]   | `EventTriggerBase` / `InvokeCommandActionBase` | event/command base classes   | shared action/trigger plumbing  |
|  [18]   | `DisposingBehavior` / `DisposingTrigger` | `IDisposable`-scoped base          | rx-subscription lifecycle base  |

[CORE_INTERACTION_TYPES]: `Avalonia.Xaml.Interactions.Core` — command, data, timing
- rail: behaviors

| [INDEX] | [SYMBOL]                    | [BASE]                  | [RAIL]                          |
| :-----: | :-------------------------- | :---------------------- | :------------------------------ |
|  [01]   | `InvokeCommandAction`       | `InvokeCommandActionBase` | `ICommand` invoke + converter pipeline |
|  [02]   | `CallMethodAction`          | `StyledElementAction`   | reflection method call          |
|  [03]   | `ChangePropertyAction`      | `StyledElementAction`   | direct property set             |
|  [04]   | `AsyncActionGroup`          | `StyledElementAction`   | `AsyncActionMode` Sequence/Parallel fan of child actions |
|  [05]   | `ThrottleAction` / `DebounceAction` / `DelayAction` | `StyledElementAction` | rate-limited / delayed dispatch |
|  [06]   | `EventTriggerBehavior`      | `EventTriggerBase`      | fire actions on a routed event (`EventName`/`SourceObject`) |
|  [07]   | `DataTriggerBehavior`       | `StyledElementTrigger`  | `Binding`⟂`ComparisonCondition`⟂`Value` gate |
|  [08]   | `MultiDataTriggerBehavior`  | `StyledElementTrigger`  | AND-of-`DataTrigger` conditions |
|  [09]   | `ObservableStreamBehavior`  | `StyledElementBehavior` | `IObservable<>` source → child actions |
|  [10]   | `ObservableTriggerBehavior` | `StyledElementTrigger`  | trigger on observable emission  |
|  [11]   | `TimerTrigger`              | `StyledElementTrigger`  | interval/tick trigger           |
|  [12]   | `TaskCompletedTrigger`      | `StyledElementTrigger`  | `Task` completion trigger       |
|  [13]   | `DataTrigger` / `EventTrigger` / `MultiDataTrigger` | `StyledElementTrigger` | `Actions`-carrying trigger variants |

[SYSTEM_ACTION_TYPES]: picker / clipboard / file-system / network surfaces
- rail: behaviors

These are the boundary-touching actions; AppUi gates them behind explicit command
and permission boundaries. Pickers route through Avalonia `IStorageProvider`.

| [INDEX] | [SYMBOL]                                  | [NAMESPACE]                       | [RAIL]                     |
| :-----: | :---------------------------------------- | :-------------------------------- | :------------------------- |
|  [01]   | `OpenFilePickerAction` / `OpenFolderPickerAction` / `SaveFilePickerAction` | `…Interactions.Core` | `IStorageProvider` pickers |
|  [02]   | `ButtonOpenFilePickerBehavior` / `MenuItem…` (× open/folder/save) | `…Interactions.Core` | control-bound picker triggers |
|  [03]   | `GetClipboardTextAction` / `SetClipboardTextAction` | `…Interactions.Core`    | clipboard text r/w         |
|  [04]   | `GetClipboardDataAction` / `SetClipboardDataObjectAction` / `GetClipboardFormatsAction` / `ClearClipboardAction` | `…Interactions.Core` | clipboard data/format/clear |
|  [05]   | `WriteTextToFileAction`                   | `…Interactions.FileSystem`        | text file write            |
|  [06]   | `CreateDirectoryAction` / `DeleteDirectoryAction` / `DeleteFileAction` | `…Interactions.FileSystem` | file-system mutation   |
|  [07]   | `FileSystemWatcherTrigger`                | `…Interactions.FileSystem`        | `FileSystemWatcher` trigger |
|  [08]   | `HttpRequestAction`                       | `…Interactions.Network`           | HTTP request action        |
|  [09]   | `NetworkInformationTrigger`               | `…Interactions.Network`           | connectivity-change trigger |

[DRAG_DROP_TYPES]: drag/drop, draggable layout, routed events, responsive
- rail: behaviors

| [INDEX] | [SYMBOL]                                  | [NAMESPACE]                       | [RAIL]                     |
| :-----: | :---------------------------------------- | :-------------------------------- | :------------------------- |
|  [01]   | `ContextDragBehavior` / `ContextDropBehavior` | `…DragAndDrop`                | data-context drag/drop     |
|  [02]   | `TypedDragBehavior` / `IDragHandler` / `IDropHandler` / `DropHandlerBase` | `…DragAndDrop` | typed handler-driven drop  |
|  [03]   | `FilesDropBehavior` / `FilesPreviewBehavior` / `ContentControlFilesDropBehavior` | `…DragAndDrop` | OS file drop + preview |
|  [04]   | `ManagedDragDropService` / `ManagedContextDropArgs` | `…DragAndDrop`           | cross-window managed drag-drop service + drop args |
|  [05]   | `CanvasDragBehavior` / `GridDragBehavior` / `ItemDragBehavior` / `ListReorderDragBehavior` | `…Draggable` | container reorder/drag |
|  [06]   | `MouseDragElementBehavior` / `MultiMouseDragElementBehavior` / `AutoScrollDuringDragBehavior` | `…Draggable` | free-drag + auto-scroll |
|  [07]   | `<Event>EventTrigger` / `<Event>EventBehavior` | `…Events`                    | strongly-typed routed-event pair per pointer/key/drag/scroll/text event |
|  [08]   | `AdaptiveBehavior` / `AdaptiveClassSetter` | `…Responsive`                    | width/height breakpoint → CSS-class swap |
|  [09]   | `AspectRatioBehavior` / `AspectRatioClassSetter` | `…Responsive`              | aspect-ratio breakpoint → class swap |

## [03]-[ENTRYPOINTS]

[ATTACH_ENTRYPOINTS]: `Interaction` static accessor + base lifecycle
- rail: behaviors

XAML attaches a `BehaviorCollection` via `i:Interaction.Behaviors`; code paths use
the static accessors. `Behavior` exposes a full virtual lifecycle beyond
attach/detach — visual/logical-tree, loaded/unloaded, data-context, and theme-variant
hooks that an Rx-scoped `DisposingBehavior` keys subscriptions off.

| [INDEX] | [SURFACE]                                                    | [SURFACE_ROOT] | [RAIL]                |
| :-----: | :----------------------------------------------------------- | :------------- | :-------------------- |
|  [01]   | `Interaction.BehaviorsProperty` (`AttachedProperty<BehaviorCollection?>`) | `Interaction` | XAML attach point |
|  [02]   | `GetBehaviors(AvaloniaObject) -> BehaviorCollection`         | `Interaction`  | behavior lookup       |
|  [03]   | `SetBehaviors(AvaloniaObject, BehaviorCollection?)`          | `Interaction`  | behavior assignment   |
|  [04]   | `ExecuteActions(object? sender, ActionCollection?, object? parameter) -> IEnumerable<object>` | `Interaction` | manual action fan |
|  [05]   | `Attach(AvaloniaObject?)` / `Detach()`                       | `Behavior`     | attach lifecycle      |
|  [06]   | `OnAttached()` / `OnDetaching()`                             | `Behavior`     | core lifecycle hooks  |
|  [07]   | `OnAttachedToVisualTree()` / `OnDetachedFromVisualTree()`    | `Behavior`     | visual-tree hooks     |
|  [08]   | `OnLoaded()` / `OnUnloaded()` / `OnDataContextChangedEvent()` / `OnActualThemeVariantChangedEvent()` | `Behavior` | reactive lifecycle hooks |
|  [09]   | `AssociatedObject` / `IsEnabledProperty`                     | `Behavior`     | bound target + gate   |
|  [10]   | `Execute(object? sender, object? parameter) -> object?`      | `IAction`      | action execution      |

[COMMAND_ACTION_ENTRYPOINTS]: command, async-group, and timing properties
- rail: behaviors

`InvokeCommandActionBase` is the full MVVM command seam: it carries not just
`Command`/`CommandParameter` but an `IValueConverter` input pipeline that maps the
event-args payload before invocation.

| [INDEX] | [SURFACE]                                              | [SURFACE_ROOT]            | [RAIL]                  |
| :-----: | :----------------------------------------------------- | :------------------------ | :---------------------- |
|  [01]   | `Command` (`ICommand?`) / `CommandParameter` (`object?`) | `InvokeCommandActionBase` | command source          |
|  [02]   | `InputConverter` (`IValueConverter?`) / `InputConverterParameter` / `InputConverterLanguage` | `InvokeCommandActionBase` | event-args → command-param converter |
|  [03]   | `PassEventArgsToCommand` (`bool`)                      | `InvokeCommandActionBase` | raw event-args passthrough |
|  [04]   | `Mode` (`AsyncActionMode` Sequence/Parallel) / `Actions` (`ActionCollection?`) | `AsyncActionGroup` | async action fan policy |
|  [05]   | `Binding` (`object?`) / `ComparisonCondition` (`ComparisonConditionType`) / `Value` (`object?`) | `DataTriggerBehavior` | data-trigger predicate |
|  [06]   | `EventName` (`string`) / `SourceObject`                | `EventTriggerBehavior`    | routed-event source     |
|  [07]   | `Source` (`IObservable<>`) / `Actions`                | `ObservableStreamBehavior`| Rx stream → action fan  |
|  [08]   | `Interval` / `Delay` (`TimeSpan`)                      | `ThrottleAction` / `DebounceAction` / `DelayAction` / `TimerTrigger` | timing policy |

[SYSTEM_ENTRYPOINTS]: picker, clipboard, file, drag, responsive properties
- rail: behaviors

| [INDEX] | [SURFACE]                                  | [SURFACE_ROOT]                       | [RAIL]              |
| :-----: | :----------------------------------------- | :----------------------------------- | :------------------ |
|  [01]   | `AllowMultiple` (`bool`) / `FileTypeFilter` (`string?`) | `OpenFilePickerAction` family | picker policy (filter string parsed via `FileFilterParser`) |
|  [02]   | `Text` (`string`)                          | `SetClipboardTextAction`             | clipboard write     |
|  [03]   | `Path`                                     | `WriteTextToFileAction` / `FileSystemWatcherTrigger` | file path |
|  [04]   | `Url` / `Method` (`"GET"`) / `Content` / `ContentType` / `ResponseContent` / `ResponseStatusCode` | `HttpRequestAction` | HTTP request + response capture |
|  [05]   | `Handler` (`IDragHandler`/`IDropHandler`)  | `ContextDragBehavior` / `TypedDragBehavior` | drag/drop handler |
|  [06]   | `PlaceholderTemplate`                      | `ListReorderDragBehavior`            | reorder drop-placeholder visual |
|  [07]   | `ClassSetters` (`AdaptiveClassSetter` rows) | `AdaptiveBehavior` / `AspectRatioBehavior` | breakpoint → class rows |

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
