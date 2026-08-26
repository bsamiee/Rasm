# [RASM_APPUI_API_BEHAVIORS]

`Xaml.Behaviors.Avalonia` attaches declarative behavior, trigger, and action graphs to Avalonia controls, holding view-models free of code-behind gesture, timing, picker, drag, clipboard, and HTTP plumbing. Kernel `Avalonia.Xaml.Interactivity` is the sole derive-from base; every other assembly attaches as a XAML leaf, feeding the behaviors layer.

## [01]-[PUBLIC_TYPES]

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

[CUSTOM_TYPES]: `Avalonia.Xaml.Interactions.Custom` — routed-event, gesture, focus, cursor, animation, and control-lifecycle library; the `[EVENT_TYPES]` generative pair covers a NAMED event, while these rows carry the routing strategy, handled-marking, and source overrides that pair cannot express.

| [INDEX] | [SYMBOL]                                                                      | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------------------------- | :------------ | :--------------------------------- |
|  [01]   | `RoutedEventTriggerBehavior`                                                  | trigger       | routed event + strategy + source   |
|  [02]   | `RoutedEventTrigger` / `RoutedEventTriggerBase` / `RoutedEventTriggerBase<T>` | trigger base  | typed routed-event trigger bases   |
|  [03]   | `AttachedToVisualTreeTriggerBase<T>` / `AttachedToLogicalTreeTriggerBase<T>`  | trigger base  | disposing tree-attachment bases    |
|  [04]   | `BindingTriggerBehavior` / `ValueChangedTriggerBehavior`                      | trigger       | binding and value gates            |
|  [05]   | `IfElseTrigger` / `PropertyChangedTrigger` / `SizeChangedTrigger`             | trigger       | branch, property, and bounds gates |
|  [06]   | `ExecuteCommandBehaviorBase` / `ExecuteCommandOn<Gesture>Behavior`            | behavior      | per-gesture command execution      |
|  [07]   | `FocusBehaviorBase` / `FocusTrapBehavior` / `FocusControlBehavior`            | behavior      | focus capture and trapping         |
|  [08]   | `SetCursorBehavior` / `PointerOverCursorBehavior`                             | behavior      | pointer cursor policy              |
|  [09]   | `ShowFlyoutAction` / `ShowContextMenuAction` / `ShowPopupAction`              | action        | transient-surface presentation     |
|  [10]   | `SetAutomationIdAction` / `AutomationNameBehavior`                            | behavior      | automation identity writes         |
|  [11]   | `ScreenReaderAnnounceAction`                                                  | action        | live-region announcement           |

[CUSTOM_GESTURE_TYPES]: `Avalonia.Xaml.Interactions.Custom` gesture family — one `RoutedEventTriggerBase<TArgs>` subclass per gesture event, each overriding the protected `RoutedEvent` with its own event and overriding `EventRoutingStrategyProperty` metadata to `Bubble`. Event cells drop the shared `InputElement.` root and the `…GestureTrigger` suffix is uniform.

| [INDEX] | [SYMBOL]                                      | [ROUTED_EVENT]                       | [EVENT_ARGS]                            |
| :-----: | :-------------------------------------------- | :----------------------------------- | :-------------------------------------- |
|  [01]   | `TappedGestureTrigger`                        | `TappedEvent`                        | `TappedEventArgs`                       |
|  [02]   | `DoubleTappedGestureTrigger`                  | `DoubleTappedEvent`                  | `TappedEventArgs`                       |
|  [03]   | `RightTappedGestureTrigger`                   | `RightTappedEvent`                   | `TappedEventArgs`                       |
|  [04]   | `HoldingGestureTrigger`                       | `HoldingEvent`                       | `HoldingRoutedEventArgs`                |
|  [05]   | `PinchGestureTrigger`                         | `PinchEvent`                         | `PinchEventArgs`                        |
|  [06]   | `PinchEndedGestureTrigger`                    | `PinchEndedEvent`                    | `PinchEndedEventArgs`                   |
|  [07]   | `PointerTouchPadGestureRotateGestureTrigger`  | `PointerTouchPadGestureRotateEvent`  | `PointerDeltaEventArgs`                 |
|  [08]   | `PointerTouchPadGestureMagnifyGestureTrigger` | `PointerTouchPadGestureMagnifyEvent` | `PointerDeltaEventArgs`                 |
|  [09]   | `PointerTouchPadGestureSwipeGestureTrigger`   | `PointerTouchPadGestureSwipeEvent`   | `PointerDeltaEventArgs`                 |
|  [10]   | `PullGestureGestureTrigger`                   | `PullGestureEvent`                   | `PullGestureEventArgs`                  |
|  [11]   | `PullGestureEndedGestureTrigger`              | `PullGestureEndedEvent`              | `PullGestureEndedEventArgs`             |
|  [12]   | `ScrollGestureGestureTrigger`                 | `ScrollGestureEvent`                 | `ScrollGestureEventArgs`                |
|  [13]   | `ScrollGestureEndedGestureTrigger`            | `ScrollGestureEndedEvent`            | `ScrollGestureEndedEventArgs`           |
|  [14]   | `ScrollGestureInertiaStartingGestureTrigger`  | `ScrollGestureInertiaStartingEvent`  | `ScrollGestureInertiaStartingEventArgs` |

[EVENT_TYPES]: `Avalonia.Xaml.Interactions.Events` — `<Event>EventTrigger` / `<Event>EventBehavior` mint one typed routed-event trigger and behavior pair per Avalonia control event.

- Gesture bindings bind this POINTER family, each member shipping both halves of the pair: `PointerPressed` `PointerReleased` `PointerMoved` `PointerEntered` `PointerExited` `PointerWheelChanged` `PointerCaptureLost`, beside the aggregate `PointerEvents` pair carrying the whole set through one attachment.

[RESPONSIVE_TYPES]: `Avalonia.Xaml.Interactions.Responsive`

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY] | [CAPABILITY]          |
| :-----: | :----------------------------------------------- | :------------ | :-------------------- |
|  [01]   | `AdaptiveBehavior` / `AdaptiveClassSetter`       | behavior      | dimension breakpoints |
|  [02]   | `AspectRatioBehavior` / `AspectRatioClassSetter` | behavior      | ratio breakpoints     |

## [02]-[ENTRYPOINTS]

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

[COMMAND_ACTION_ENTRYPOINTS]: command, async-group, and trigger properties, all `AvaloniaProperty` — `InvokeCommandActionBase` is the full MVVM command adapter, mapping the event-args payload through an `IValueConverter` before invocation; `ThrottleAction`/`DebounceAction`/`DelayAction`/`TimerTrigger` carry `Interval` or `Delay` as `TimeSpan`.

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

[CUSTOM_ROUTE_ENTRYPOINTS]: routed-event and gesture knobs on the `Interactions.Custom` bases — the whole family's routing surface is these five members, so a gesture row states its event through its own subclass and its routing through the two inherited properties.

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `RoutedEventTriggerBehavior.RoutedEvent` (`RoutedEvent?`)            | property | listened routed event                       |
|  [02]   | `RoutedEventTriggerBehavior.RoutingStrategies` (`RoutingStrategies`) | property | strategy filter, default `Direct \          |
|  [03]   | `RoutedEventTriggerBehavior.SourceInteractive` (`Interactive?`)      | property | listen target override                      |
|  [04]   | `RoutedEventTriggerBase.EventRoutingStrategy` (`RoutingStrategies`)  | property | per-gesture strategy, base default `Direct` |
|  [05]   | `RoutedEventTriggerBase.MarkAsHandled` (`bool`)                      | property | mark the routed event handled               |

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

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Kernel `…Interactivity` is the sole derive-from surface; every other assembly attaches as a XAML leaf.
- `using` and XAML `xmlns` bind the CLR roots `Avalonia.Xaml.Interactivity` and `Avalonia.Xaml.Interactions.{Core,Custom,FileSystem,Network,DragAndDrop,Draggable,Events,Responsive}`; the `Xaml.Behaviors.*` tokens are package and assembly ids only.
- Bases subclass `AvaloniaObject` or `StyledElement`, so every behavior participates in styling, binding, and resource inheritance.

[STACKING]:
- `ReactiveUI`(`api-reactiveui.md`): a `ReactiveCommand<TIn,TOut>` (an `ICommand`) binds into `InvokeCommandAction.Command`, an `EventTriggerBehavior` fans the routed `EventArgs` through `InputConverter` into the command parameter, and `CanExecute` observables gate through the same `ICommand`, so a control gesture drives a view-model command with zero code-behind.
- `System.Reactive`(`api-reactive.md`): operators feed `ObservableStreamBehavior.Source`/`ObservableTriggerBehavior`, and an upstream `Observable.Throttle` collapses with `ThrottleAction`/`DebounceAction` — the behavior owns a view-rate limit, the operator a data-stream limit.
- `DynamicData`(`api-dynamicdata.md`): `.ToCollection()`/`.QueryWhenChanged()` change-set streams project into the observable behavior source, so collection and state changes drive `ActionCollection` fans off the same pipeline that feeds a `ReactiveObject`.
- `Avalonia`(`api-avalonia.md`): picker actions resolve `StorageProvider` via `TopLevel.GetTopLevel(control)`, and the selected `IStorageFile`/`IStorageFolder` is the file-boundary token AppUi maps to a domain path at the command edge.
- within-lib: `TypedDragBehavior` with `IDropHandler`/`DropHandlerBase` carries a typed payload and `ManagedDragDropService` moves it cross-window; binding the drop handler to a view-model `ReactiveCommand` routes reorder and drop mutations through the state pipeline, keeping `DynamicData` source lists the single reorder authority.

[LOCAL_ADMISSION]:
- Derive a Rasm behavior from `StyledElementBehavior`/`StyledElementTrigger`/`StyledElementAction` when it participates in styling, resource inheritance, or `{Binding}` on its own properties; from `Behavior`/`Trigger` otherwise.
- Base a behavior owning Rx subscriptions on `DisposingBehavior`/`DisposingTrigger`, so attach and detach drive subscription lifecycle deterministically.
- Gate data with `DataTriggerBehavior` (single `ComparisonConditionType` predicate) or `MultiDataTriggerBehavior` (AND of `Condition` rows); `ComparisonConditionType` is the canonical comparator.
- Route a gesture through its `Interactions.Custom` `<Gesture>GestureTrigger` row: the subclass names the `InputElement` event, so a hand-wired `Tapped`/`Pinch`/`Holding`/touchpad handler and a `GestureRecognizers` fork are both deleted. `RoutedEventTriggerBehavior` is the escape hatch a named-event trigger cannot cover — a tunnelling strategy, a non-self source, or a handled-marking requirement — never a second spelling of an already-minted `<Event>EventTrigger`.
- Picker, file-system, and network actions stay behind the command and permission boundary, mapping the selected token to a domain path at the command edge rather than mutating the file system in place.
