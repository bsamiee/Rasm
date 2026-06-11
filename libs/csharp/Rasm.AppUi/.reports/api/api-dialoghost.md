# [RASM_APPUI_API_DIALOGHOST]

`DialogHost.Avalonia` supplies modal dialog host controls, dialog sessions, identifiers, and show/close surfaces.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DialogHost.Avalonia`
- package: `DialogHost.Avalonia`
- assembly: `DialogHost.Avalonia`
- namespace: `DialogHostAvalonia`
- asset: runtime library
- rail: dialogs

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dialog family
- rail: dialogs

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE] | [CAPABILITY]          |
| :-----: | :----------------------- | :------------- | :-------------------- |
|   [1]   | `DialogHost`             | host control   | anchors dialog region |
|   [2]   | `DialogSession`          | session handle | controls dialog close |
|   [3]   | `DialogClosingEventArgs` | close event    | carries close state   |
|   [4]   | `DialogOpenedEventArgs`  | open event     | carries open state    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: dialog operations
- rail: dialogs

| [INDEX] | [SURFACE]            | [CALL_SHAPE]     | [CAPABILITY]        |
| :-----: | :------------------- | :--------------- | :------------------ |
|   [1]   | `Show`               | display method   | opens surface       |
|   [2]   | `Close`              | close method     | closes surface      |
|   [3]   | `IsOpen`             | state property   | tracks open state   |
|   [4]   | `DialogContent`      | content property | sets dialog body    |
|   [5]   | `Identifier`         | host key         | selects dialog host |
|   [6]   | `CloseDialogCommand` | close command    | closes dialog       |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `DialogHost.Avalonia`
- Owns: retained dialog orchestration
- Accept: dialogs are command receipts
- Reject: free-floating modal logic
