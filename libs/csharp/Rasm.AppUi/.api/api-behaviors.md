# [RASM_APPUI_API_BEHAVIORS]

`Xaml.Behaviors.Avalonia` admits Avalonia behavior, trigger, action,
interaction, picker, file-system, drag, event, responsive, and clipboard
surfaces into the AppUi behavior rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Xaml.Behaviors.Avalonia`
- package: `Xaml.Behaviors.Avalonia`
- assembly: package admission asset
- namespace: behavior dependency assemblies
- asset: package admission
- rail: behaviors

## [02]-[PUBLIC_TYPES]

[PACKAGE_ASSET_SCOPE]: admitted assemblies
- rail: behaviors

| [INDEX] | [SYMBOL]                                  | [RAIL]             |
| :-----: | :---------------------------------------- | :----------------- |
|  [01]   | `Xaml.Behaviors.Interactivity`            | behavior bases     |
|  [02]   | `Xaml.Behaviors.Interactions`             | action surfaces    |
|  [03]   | `Xaml.Behaviors.Interactions.Custom`      | lifecycle actions  |
|  [04]   | `Xaml.Behaviors.Interactions.DragAndDrop` | drag operations    |
|  [05]   | `Xaml.Behaviors.Interactions.Draggable`   | draggable controls |
|  [06]   | `Xaml.Behaviors.Interactions.Events`      | routed triggers    |
|  [07]   | `Xaml.Behaviors.Interactions.Responsive`  | adaptive behaviors |

[INTERACTIVITY_TYPES]: behavior and trigger bases
- rail: behaviors

| [INDEX] | [SYMBOL]                | [RAIL]              |
| :-----: | :---------------------- | :------------------ |
|  [01]   | `Behavior`              | behavior base       |
|  [02]   | `Behavior<T>`           | typed behavior      |
|  [03]   | `StyledElementBehavior` | styled behavior     |
|  [04]   | `Trigger`               | trigger base        |
|  [05]   | `Trigger<T>`            | typed trigger       |
|  [06]   | `Action`                | action base         |
|  [07]   | `IAction`               | action contract     |
|  [08]   | `ActionCollection`      | action collection   |
|  [09]   | `BehaviorCollection`    | behavior collection |
|  [10]   | `Interaction`           | attached accessor   |

[INTERACTION_TYPES]: actions and system behaviors
- rail: behaviors

| [INDEX] | [SYMBOL]                    | [RAIL]           |
| :-----: | :-------------------------- | :--------------- |
|  [01]   | `InvokeCommandAction`       | command action   |
|  [02]   | `CallMethodAction`          | method action    |
|  [03]   | `ChangePropertyAction`      | property action  |
|  [04]   | `AsyncActionGroup`          | grouped action   |
|  [05]   | `DataTriggerBehavior`       | data trigger     |
|  [06]   | `MultiDataTriggerBehavior`  | compound trigger |
|  [07]   | `ObservableStreamBehavior`  | stream behavior  |
|  [08]   | `ThrottleAction`            | throttled action |
|  [09]   | `DebounceAction`            | debounced action |
|  [10]   | `TimerTrigger`              | timer trigger    |
|  [11]   | `TaskCompletedTrigger`      | task trigger     |
|  [12]   | `NetworkInformationTrigger` | network trigger  |

[SYSTEM_TYPES]: picker, file, clipboard, network, and drag surfaces
- rail: behaviors

| [INDEX] | [SYMBOL]                    | [RAIL]            |
| :-----: | :-------------------------- | :---------------- |
|  [01]   | `OpenFilePickerAction`      | file picker       |
|  [02]   | `OpenFolderPickerAction`    | folder picker     |
|  [03]   | `SaveFilePickerAction`      | save picker       |
|  [04]   | `GetClipboardTextAction`    | clipboard read    |
|  [05]   | `SetClipboardTextAction`    | clipboard write   |
|  [06]   | `GetClipboardFormatsAction` | clipboard formats |
|  [07]   | `FileSystemWatcherTrigger`  | file watch        |
|  [08]   | `WriteTextToFileAction`     | file write        |
|  [09]   | `HttpRequestAction`         | HTTP request      |
|  [10]   | `ContextDragBehavior`       | drag operation    |
|  [11]   | `ContextDropBehavior`       | drop operation    |
|  [12]   | `CanvasDragBehavior`        | canvas drag       |
|  [13]   | `ListReorderDragBehavior`   | list reorder      |
|  [14]   | `AdaptiveBehavior`          | adaptive layout   |
|  [15]   | `AspectRatioBehavior`       | layout constraint |

## [03]-[ENTRYPOINTS]

[BEHAVIOR_ENTRYPOINTS]: attachment operations
- rail: behaviors

| [INDEX] | [SURFACE]        | [SURFACE_ROOT] | [RAIL]              |
| :-----: | :--------------- | :------------- | :------------------ |
|  [01]   | `Attach`         | `Behavior`     | behavior attach     |
|  [02]   | `Detach`         | `Behavior`     | behavior detach     |
|  [03]   | `OnAttached`     | `Behavior`     | lifecycle hook      |
|  [04]   | `OnDetaching`    | `Behavior`     | lifecycle hook      |
|  [05]   | `GetBehaviors`   | `Interaction`  | behavior lookup     |
|  [06]   | `SetBehaviors`   | `Interaction`  | behavior assignment |
|  [07]   | `ExecuteActions` | `Interaction`  | action execution    |
|  [08]   | `Execute`        | `IAction`      | action execution    |

[ACTION_ENTRYPOINTS]: command, stream, and timing operations
- rail: behaviors

| [INDEX] | [SURFACE]                | [SURFACE_ROOT]             | [RAIL]             |
| :-----: | :----------------------- | :------------------------- | :----------------- |
|  [01]   | `Command`                | `InvokeCommandAction`      | command source     |
|  [02]   | `PassEventArgsToCommand` | command action base        | event argument     |
|  [03]   | `Actions`                | `AsyncActionGroup`         | action group       |
|  [04]   | `Binding`                | `DataTriggerBehavior`      | trigger binding    |
|  [05]   | `Conditions`             | `MultiDataTriggerBehavior` | trigger conditions |
|  [06]   | `Source`                 | `ObservableStreamBehavior` | stream source      |
|  [07]   | `Interval`               | `ThrottleAction`           | throttle interval  |
|  [08]   | `Delay`                  | `DebounceAction`           | debounce interval  |

[SYSTEM_ENTRYPOINTS]: picker, file, clipboard, network, and drag operations
- rail: behaviors

| [INDEX] | [SURFACE]         | [SURFACE_ROOT]             | [RAIL]           |
| :-----: | :---------------- | :------------------------- | :--------------- |
|  [01]   | `StorageProvider` | picker base                | storage provider |
|  [02]   | `AllowMultiple`   | open picker actions        | multi-select     |
|  [03]   | `FileTypeFilter`  | picker actions             | file filter      |
|  [04]   | `Text`            | `SetClipboardTextAction`   | clipboard write  |
|  [05]   | `Clipboard`       | `GetClipboardTextAction`   | clipboard read   |
|  [06]   | `Path`            | `WriteTextToFileAction`    | file write       |
|  [07]   | `Path`            | `FileSystemWatcherTrigger` | file watch       |
|  [08]   | `Url`             | `HttpRequestAction`        | HTTP request     |
|  [09]   | `Handler`         | `ContextDragBehavior`      | drag operation   |
|  [10]   | `Orientation`     | `ListReorderDragBehavior`  | drag tracking    |

## [04]-[IMPLEMENTATION_LAW]

[PACKAGE_ADMISSION]:
- package: `Xaml.Behaviors.Avalonia`
- admitted assemblies: interactivity, interactions, custom, drag-and-drop, draggable, events, responsive
- admission role: AppUi behavior rail receives the full behavior/action surface through one package reference
- boundary: behavior assets adapt controls to command, stream, picker, file, network, drag, and layout operations

[BEHAVIOR_RAIL]:
- attachment root: `Interaction`
- behavior roots: `Behavior`, `Behavior<T>`, `StyledElementBehavior`
- trigger roots: `Trigger`, `Trigger<T>`, routed event triggers, data triggers, timer triggers
- action roots: command, method, property, async group, clipboard, picker, file, HTTP, drag

[LOCAL_ADMISSION]:
- Behavior surfaces remain UI rail material and never become code-behind forks.
- System actions enter through explicit AppUi command and permission boundaries.
- Drag, pointer, routed event, and responsive surfaces share one behavior rail.
- Direct package dependencies stay admitted through this page unless AppUi declares a separate direct package reference.

[RAIL_LAW]:
- Package: `Xaml.Behaviors.Avalonia`
- Owns: behavior package admission and interaction surfaces
- Accept: behavior-based control adaptation
- Reject: code-behind gesture forks
