# [RASM_APPUI_API_BEHAVIORS]

`Xaml.Behaviors.Avalonia` supplies the behavior meta package that admits Avalonia interactivity and interaction assemblies into the behavior rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Xaml.Behaviors.Avalonia`
- package: `Xaml.Behaviors.Avalonia`
- assembly: meta package
- namespace: transitive behavior assemblies
- asset: dependency bundle
- rail: behaviors

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: admitted behavior assemblies
- rail: behaviors

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]       | [CAPABILITY]             |
| :-----: | :---------------------------------------- | :------------------- | :----------------------- |
|   [1]   | `Xaml.Behaviors.Interactivity`            | interactivity bundle | supplies behavior bases  |
|   [2]   | `Xaml.Behaviors.Interactions`             | interaction bundle   | supplies action families |
|   [3]   | `Xaml.Behaviors.Interactions.Custom`      | custom bundle        | supplies custom actions  |
|   [4]   | `Xaml.Behaviors.Interactions.DragAndDrop` | drag-drop bundle     | supplies drag behaviors  |
|   [5]   | `Xaml.Behaviors.Interactions.Draggable`   | draggable bundle     | supplies drag handles    |
|   [6]   | `Xaml.Behaviors.Interactions.Events`      | event bundle         | supplies event triggers  |
|   [7]   | `Xaml.Behaviors.Interactions.Responsive`  | responsive bundle    | supplies adaptive rules  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: package admission
- rail: behaviors

| [INDEX] | [SURFACE]                | [CALL_SHAPE]     | [CAPABILITY]             |
| :-----: | :----------------------- | :--------------- | :----------------------- |
|   [1]   | `dependencies`           | package metadata | admits behavior bundles  |
|   [2]   | `Avalonia` dependency    | package metadata | binds controls substrate |
|   [3]   | `interactivity assembly` | runtime asset    | supplies behavior bases  |
|   [4]   | `interaction assemblies` | runtime assets   | supplies action families |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Xaml.Behaviors.Avalonia`
- Owns: behavior package admission
- Accept: behavior bundles adapt controls to commands
- Reject: code-behind gesture forks
