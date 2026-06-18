# [RASM_APPUI_API_BEHAVIORS]

`Xaml.Behaviors.Avalonia` admits Avalonia behavior, trigger, action,
interaction, picker, file-system, drag, event, responsive, and clipboard
surfaces into the AppUi behavior rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Xaml.Behaviors.Avalonia`
- package: `Xaml.Behaviors.Avalonia`
- assembly: package admission asset
- namespace: behavior dependency assemblies
- asset: package admission
- rail: behaviors

## [2]-[PUBLIC_TYPES]

[PACKAGE_ASSET_SCOPE]: admitted assemblies
- rail: behaviors

| [INDEX] | [SYMBOL]                                  | [RAIL]             |
| :-----: | :---------------------------------------- | :----------------- |
|   [1]   | `Xaml.Behaviors.Interactivity`            | behavior bases     |
|   [2]   | `Xaml.Behaviors.Interactions`             | action surfaces    |
|   [3]   | `Xaml.Behaviors.Interactions.Custom`      | lifecycle actions  |
|   [4]   | `Xaml.Behaviors.Interactions.DragAndDrop` | drag operations    |
|   [5]   | `Xaml.Behaviors.Interactions.Draggable`   | draggable controls |
|   [6]   | `Xaml.Behaviors.Interactions.Events`      | routed triggers    |
|   [7]   | `Xaml.Behaviors.Interactions.Responsive`  | adaptive behaviors |

[INTERACTIVITY_TYPES]: behavior and trigger bases
- rail: behaviors

| [INDEX] | [SYMBOL]                | [RAIL]              |
| :-----: | :---------------------- | :------------------ |
|   [1]   | `Behavior`              | behavior base       |
|   [2]   | `Behavior<T>`           | typed behavior      |
|   [3]   | `StyledElementBehavior` | styled behavior     |
|   [4]   | `Trigger`               | trigger base        |
|   [5]   | `Trigger<T>`            | typed trigger       |
|   [6]   | `Action`                | action base         |
|   [7]   | `IAction`               | action contract     |
|   [8]   | `ActionCollection`      | action collection   |
|   [9]   | `BehaviorCollection`    | behavior collection |
|  [10]   | `Interaction`           | attached accessor   |

[INTERACTION_TYPES]: actions and system behaviors
- rail: behaviors

| [INDEX] | [SYMBOL]                    | [RAIL]           |
| :-----: | :-------------------------- | :--------------- |
|   [1]   | `InvokeCommandAction`       | command action   |
|   [2]   | `CallMethodAction`          | method action    |
|   [3]   | `ChangePropertyAction`      | property action  |
|   [4]   | `AsyncActionGroup`          | grouped action   |
|   [5]   | `DataTriggerBehavior`       | data trigger     |
|   [6]   | `MultiDataTriggerBehavior`  | compound trigger |
|   [7]   | `ObservableStreamBehavior`  | stream behavior  |
|   [8]   | `ThrottleAction`            | throttled action |
|   [9]   | `DebounceAction`            | debounced action |
|  [10]   | `TimerTrigger`              | timer trigger    |
|  [11]   | `TaskCompletedTrigger`      | task trigger     |
|  [12]   | `NetworkInformationTrigger` | network trigger  |

[SYSTEM_TYPES]: picker, file, clipboard, network, and drag surfaces
- rail: behaviors

| [INDEX] | [SYMBOL]                    | [RAIL]            |
| :-----: | :-------------------------- | :---------------- |
|   [1]   | `OpenFilePickerAction`      | file picker       |
|   [2]   | `OpenFolderPickerAction`    | folder picker     |
|   [3]   | `SaveFilePickerAction`      | save picker       |
|   [4]   | `GetClipboardTextAction`    | clipboard read    |
|   [5]   | `SetClipboardTextAction`    | clipboard write   |
|   [6]   | `GetClipboardFormatsAction` | clipboard formats |
|   [7]   | `FileSystemWatcherTrigger`  | file watch        |
|   [8]   | `WriteTextToFileAction`     | file write        |
|   [9]   | `HttpRequestAction`         | HTTP request      |
|  [10]   | `ContextDragBehavior`       | drag operation    |
|  [11]   | `ContextDropBehavior`       | drop operation    |
|  [12]   | `CanvasDragBehavior`        | canvas drag       |
|  [13]   | `ListReorderDragBehavior`   | list reorder      |
|  [14]   | `AdaptiveBehavior`          | adaptive layout   |
|  [15]   | `AspectRatioBehavior`       | layout constraint |

## [3]-[ENTRYPOINTS]

[BEHAVIOR_ENTRYPOINTS]: attachment operations
- rail: behaviors

| [INDEX] | [SURFACE]        | [SURFACE_ROOT] | [RAIL]              |
| :-----: | :--------------- | :------------- | :------------------ |
|   [1]   | `Attach`         | `Behavior`     | behavior attach     |
|   [2]   | `Detach`         | `Behavior`     | behavior detach     |
|   [3]   | `OnAttached`     | `Behavior`     | lifecycle hook      |
|   [4]   | `OnDetaching`    | `Behavior`     | lifecycle hook      |
|   [5]   | `GetBehaviors`   | `Interaction`  | behavior lookup     |
|   [6]   | `SetBehaviors`   | `Interaction`  | behavior assignment |
|   [7]   | `ExecuteActions` | `Interaction`  | action execution    |
|   [8]   | `Execute`        | `IAction`      | action execution    |

[ACTION_ENTRYPOINTS]: command, stream, and timing operations
- rail: behaviors

| [INDEX] | [SURFACE]                | [SURFACE_ROOT]             | [RAIL]             |
| :-----: | :----------------------- | :------------------------- | :----------------- |
|   [1]   | `Command`                | `InvokeCommandAction`      | command source     |
|   [2]   | `PassEventArgsToCommand` | command action base        | event argument     |
|   [3]   | `Actions`                | `AsyncActionGroup`         | action group       |
|   [4]   | `Binding`                | `DataTriggerBehavior`      | trigger binding    |
|   [5]   | `Conditions`             | `MultiDataTriggerBehavior` | trigger conditions |
|   [6]   | `Source`                 | `ObservableStreamBehavior` | stream source      |
|   [7]   | `Interval`               | `ThrottleAction`           | throttle interval  |
|   [8]   | `Delay`                  | `DebounceAction`           | debounce interval  |

[SYSTEM_ENTRYPOINTS]: picker, file, clipboard, network, and drag operations
- rail: behaviors

| [INDEX] | [SURFACE]         | [SURFACE_ROOT]             | [RAIL]           |
| :-----: | :---------------- | :------------------------- | :--------------- |
|   [1]   | `StorageProvider` | picker base                | storage provider |
|   [2]   | `AllowMultiple`   | open picker actions        | multi-select     |
|   [3]   | `FileTypeFilter`  | picker actions             | file filter      |
|   [4]   | `Text`            | `SetClipboardTextAction`   | clipboard write  |
|   [5]   | `Clipboard`       | `GetClipboardTextAction`   | clipboard read   |
|   [6]   | `Path`            | `WriteTextToFileAction`    | file write       |
|   [7]   | `Path`            | `FileSystemWatcherTrigger` | file watch       |
|   [8]   | `Url`             | `HttpRequestAction`        | HTTP request     |
|   [9]   | `Handler`         | `ContextDragBehavior`      | drag operation   |
|  [10]   | `Orientation`     | `ListReorderDragBehavior`  | drag tracking    |

## [4]-[IMPLEMENTATION_LAW]

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
