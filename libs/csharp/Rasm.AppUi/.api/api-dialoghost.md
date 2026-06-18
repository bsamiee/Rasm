# [RASM_APPUI_API_DIALOGHOST]

`DialogHost.Avalonia` supplies modal dialog host controls, dialog sessions, identifiers, show/close surfaces, popup positioners, styles, and dialog events.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DialogHost.Avalonia`
- package: `DialogHost.Avalonia`
- assembly: `DialogHost.Avalonia`
- namespace: `DialogHostAvalonia`
- namespace: `DialogHostAvalonia.Positioners`
- asset: runtime library
- rail: dialogs

## [2]-[PUBLIC_TYPES]

[DIALOG_TYPES]: host, session, and event surfaces
- rail: dialogs

| [INDEX] | [SYMBOL]                 | [RAIL]          |
| :-----: | :----------------------- | :-------------- |
|   [1]   | `DialogHost`             | host control    |
|   [2]   | `DialogSession`          | session handle  |
|   [3]   | `DialogClosingEventArgs` | close event     |
|   [4]   | `DialogOpenedEventArgs`  | open event      |
|   [5]   | `DialogHostStyle`        | style surface   |
|   [6]   | `DialogHostStyles`       | style resources |

[POSITIONER_TYPES]: popup positioning
- rail: dialogs

| [INDEX] | [SYMBOL]                              | [RAIL]            |
| :-----: | :------------------------------------ | :---------------- |
|   [1]   | `IDialogPopupPositioner`              | position contract |
|   [2]   | `IDialogPopupPositionerConstrainable` | constrained popup |
|   [3]   | `CenteredDialogPopupPositioner`       | centered popup    |
|   [4]   | `AlignmentDialogPopupPositioner`      | aligned popup     |

[UTILITY_TYPES]: observable state surface
- rail: dialogs

| [INDEX] | [SYMBOL]             | [RAIL]       |
| :-----: | :------------------- | :----------- |
|   [1]   | `BehaviorSubject<T>` | dialog state |

## [3]-[ENTRYPOINTS]

[DIALOG_ENTRYPOINTS]: host and session operations
- rail: dialogs

| [INDEX] | [SURFACE]          | [SURFACE_ROOT]           | [RAIL]         |
| :-----: | :----------------- | :----------------------- | :------------- |
|   [1]   | `Show`             | `DialogHost`             | dialog open    |
|   [2]   | `Close`            | `DialogHost`             | dialog close   |
|   [3]   | `Pop`              | `DialogHost`             | stacked close  |
|   [4]   | `GetDialogSession` | `DialogHost`             | session lookup |
|   [5]   | `IsDialogOpen`     | `DialogHost`             | open lookup    |
|   [6]   | `UpdateContent`    | `DialogSession`          | content update |
|   [7]   | `Close`            | `DialogSession`          | session close  |
|   [8]   | `Cancel`           | `DialogClosingEventArgs` | close cancel   |

[HOST_PROPERTIES]: dialog host properties and commands
- rail: dialogs

| [INDEX] | [SURFACE]                  | [SURFACE_ROOT] | [RAIL]           |
| :-----: | :------------------------- | :------------- | :--------------- |
|   [1]   | `Identifier`               | `DialogHost`   | host key         |
|   [2]   | `IsOpen`                   | `DialogHost`   | open state       |
|   [3]   | `DialogContent`            | `DialogHost`   | dialog content   |
|   [4]   | `DialogContentTemplate`    | `DialogHost`   | content template |
|   [5]   | `OpenDialogCommand`        | `DialogHost`   | open command     |
|   [6]   | `CloseDialogCommand`       | `DialogHost`   | close command    |
|   [7]   | `CloseOnClickAway`         | `DialogHost`   | outside close    |
|   [8]   | `CurrentSession`           | `DialogHost`   | active session   |
|   [9]   | `CurrentSessions`          | `DialogHost`   | session stack    |
|  [10]   | `IsMultipleDialogsEnabled` | `DialogHost`   | stacked dialogs  |

[VISUAL_ENTRYPOINTS]: overlay, blur, style, and positioning
- rail: dialogs

| [INDEX] | [SURFACE]           | [SURFACE_ROOT]                        | [RAIL]          |
| :-----: | :------------------ | :------------------------------------ | :-------------- |
|   [1]   | `OverlayBackground` | `DialogHost`                          | overlay brush   |
|   [2]   | `DialogMargin`      | `DialogHost`                          | dialog margin   |
|   [3]   | `PopupPositioner`   | `DialogHost`                          | popup position  |
|   [4]   | `BlurBackground`    | `DialogHost`                          | background blur |
|   [5]   | `CornerRadius`      | `DialogHostStyle`                     | corner radius   |
|   [6]   | `BorderBrush`       | `DialogHostStyle`                     | border brush    |
|   [7]   | `Update`            | `IDialogPopupPositioner`              | popup layout    |
|   [8]   | `Constrain`         | `IDialogPopupPositionerConstrainable` | bounds          |

[EVENT_ENTRYPOINTS]: dialog event surfaces
- rail: dialogs

| [INDEX] | [SURFACE]               | [SURFACE_ROOT]           | [RAIL]         |
| :-----: | :---------------------- | :----------------------- | :------------- |
|   [1]   | `DialogOpened`          | `DialogHost`             | open event     |
|   [2]   | `DialogClosing`         | `DialogHost`             | close event    |
|   [3]   | `DialogOpenedCallback`  | `DialogHost`             | open callback  |
|   [4]   | `DialogClosingCallback` | `DialogHost`             | close callback |
|   [5]   | `Session`               | `DialogOpenedEventArgs`  | session        |
|   [6]   | `Session`               | `DialogClosingEventArgs` | session        |
|   [7]   | `Parameter`             | `DialogClosingEventArgs` | close value    |

## [4]-[IMPLEMENTATION_LAW]

[DIALOG_LAW]:
- Package: `DialogHost.Avalonia`
- Owns: retained dialog orchestration, session lifecycle, commands, host identifiers, style, overlay, and positioning
- Accept: dialogs are command receipts with explicit session and close state
- Reject: free-floating modal logic

[MODALITY_LAW]:
- Package: `DialogHost.Avalonia`
- Owns: panel, companion, sidecar, support, diagnostic, and downstream app dialog surfaces through one dialog rail
- Accept: modal state remains host-addressable and command-driven
- Reject: host-specific modal service families
