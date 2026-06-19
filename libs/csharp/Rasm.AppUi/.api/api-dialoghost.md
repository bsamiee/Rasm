# [RASM_APPUI_API_DIALOGHOST]

`DialogHost.Avalonia` supplies modal dialog host controls, dialog sessions, identifiers, show/close surfaces, popup positioners, styles, and dialog events.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DialogHost.Avalonia`
- package: `DialogHost.Avalonia`
- assembly: `DialogHost.Avalonia`
- namespace: `DialogHostAvalonia`
- namespace: `DialogHostAvalonia.Positioners`
- asset: runtime library
- rail: dialogs

## [02]-[PUBLIC_TYPES]

[DIALOG_TYPES]: host, session, and event surfaces
- rail: dialogs

| [INDEX] | [SYMBOL]                 | [RAIL]          |
| :-----: | :----------------------- | :-------------- |
|  [01]   | `DialogHost`             | host control    |
|  [02]   | `DialogSession`          | session handle  |
|  [03]   | `DialogClosingEventArgs` | close event     |
|  [04]   | `DialogOpenedEventArgs`  | open event      |
|  [05]   | `DialogHostStyle`        | style surface   |
|  [06]   | `DialogHostStyles`       | style resources |

[POSITIONER_TYPES]: popup positioning
- rail: dialogs

| [INDEX] | [SYMBOL]                              | [RAIL]            |
| :-----: | :------------------------------------ | :---------------- |
|  [01]   | `IDialogPopupPositioner`              | position contract |
|  [02]   | `IDialogPopupPositionerConstrainable` | constrained popup |
|  [03]   | `CenteredDialogPopupPositioner`       | centered popup    |
|  [04]   | `AlignmentDialogPopupPositioner`      | aligned popup     |

[UTILITY_TYPES]: observable state surface
- rail: dialogs

| [INDEX] | [SYMBOL]             | [RAIL]       |
| :-----: | :------------------- | :----------- |
|  [01]   | `BehaviorSubject<T>` | dialog state |

## [03]-[ENTRYPOINTS]

[DIALOG_ENTRYPOINTS]: host and session operations
- rail: dialogs

| [INDEX] | [SURFACE]          | [SURFACE_ROOT]           | [RAIL]         |
| :-----: | :----------------- | :----------------------- | :------------- |
|  [01]   | `Show`             | `DialogHost`             | dialog open    |
|  [02]   | `Close`            | `DialogHost`             | dialog close   |
|  [03]   | `Pop`              | `DialogHost`             | stacked close  |
|  [04]   | `GetDialogSession` | `DialogHost`             | session lookup |
|  [05]   | `IsDialogOpen`     | `DialogHost`             | open lookup    |
|  [06]   | `UpdateContent`    | `DialogSession`          | content update |
|  [07]   | `Close`            | `DialogSession`          | session close  |
|  [08]   | `Cancel`           | `DialogClosingEventArgs` | close cancel   |

[HOST_PROPERTIES]: dialog host properties and commands
- rail: dialogs

| [INDEX] | [SURFACE]                  | [SURFACE_ROOT] | [RAIL]           |
| :-----: | :------------------------- | :------------- | :--------------- |
|  [01]   | `Identifier`               | `DialogHost`   | host key         |
|  [02]   | `IsOpen`                   | `DialogHost`   | open state       |
|  [03]   | `DialogContent`            | `DialogHost`   | dialog content   |
|  [04]   | `DialogContentTemplate`    | `DialogHost`   | content template |
|  [05]   | `OpenDialogCommand`        | `DialogHost`   | open command     |
|  [06]   | `CloseDialogCommand`       | `DialogHost`   | close command    |
|  [07]   | `CloseOnClickAway`         | `DialogHost`   | outside close    |
|  [08]   | `CurrentSession`           | `DialogHost`   | active session   |
|  [09]   | `CurrentSessions`          | `DialogHost`   | session stack    |
|  [10]   | `IsMultipleDialogsEnabled` | `DialogHost`   | stacked dialogs  |

[VISUAL_ENTRYPOINTS]: overlay, blur, style, and positioning
- rail: dialogs

| [INDEX] | [SURFACE]           | [SURFACE_ROOT]                        | [RAIL]          |
| :-----: | :------------------ | :------------------------------------ | :-------------- |
|  [01]   | `OverlayBackground` | `DialogHost`                          | overlay brush   |
|  [02]   | `DialogMargin`      | `DialogHost`                          | dialog margin   |
|  [03]   | `PopupPositioner`   | `DialogHost`                          | popup position  |
|  [04]   | `BlurBackground`    | `DialogHost`                          | background blur |
|  [05]   | `CornerRadius`      | `DialogHostStyle`                     | corner radius   |
|  [06]   | `BorderBrush`       | `DialogHostStyle`                     | border brush    |
|  [07]   | `Update`            | `IDialogPopupPositioner`              | popup layout    |
|  [08]   | `Constrain`         | `IDialogPopupPositionerConstrainable` | bounds          |

[EVENT_ENTRYPOINTS]: dialog event surfaces
- rail: dialogs

| [INDEX] | [SURFACE]               | [SURFACE_ROOT]           | [RAIL]         |
| :-----: | :---------------------- | :----------------------- | :------------- |
|  [01]   | `DialogOpened`          | `DialogHost`             | open event     |
|  [02]   | `DialogClosing`         | `DialogHost`             | close event    |
|  [03]   | `DialogOpenedCallback`  | `DialogHost`             | open callback  |
|  [04]   | `DialogClosingCallback` | `DialogHost`             | close callback |
|  [05]   | `Session`               | `DialogOpenedEventArgs`  | session        |
|  [06]   | `Session`               | `DialogClosingEventArgs` | session        |
|  [07]   | `Parameter`             | `DialogClosingEventArgs` | close value    |

## [04]-[IMPLEMENTATION_LAW]

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
