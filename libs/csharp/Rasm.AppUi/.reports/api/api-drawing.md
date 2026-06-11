# [RASM_APPUI_API_DRAWING]

`System.Drawing.Common` supplies compile support for host drawing surfaces and bitmap interop in host-aware projects.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Drawing.Common`
- package: `System.Drawing.Common`
- assembly: `System.Drawing.Common`
- namespace: `System.Drawing`
- asset: compile package
- rail: host-drawing

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: drawing family
- rail: host-drawing

| [INDEX] | [SYMBOL]    | [PACKAGE_ROLE]   | [CAPABILITY]                  |
| :-----: | :---------- | :--------------- | :---------------------------- |
|   [1]   | `Bitmap`    | drawing surface  | draws visual evidence         |
|   [2]   | `Graphics`  | drawing context  | anchors host-drawing contract |
|   [3]   | `Pen`       | stroke resource  | anchors host-drawing contract |
|   [4]   | `Brush`     | fill resource    | anchors host-drawing contract |
|   [5]   | `Color`     | drawing surface  | draws visual evidence         |
|   [6]   | `Rectangle` | bounds value     | anchors host-drawing contract |
|   [7]   | `Point`     | coordinate value | anchors host-drawing contract |
|   [8]   | `Size`      | extent value     | anchors host-drawing contract |
|   [9]   | `Image`     | drawing surface  | draws visual evidence         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: drawing operations
- rail: host-drawing

| [INDEX] | [SURFACE]       | [CALL_SHAPE]     | [CAPABILITY]         |
| :-----: | :-------------- | :--------------- | :------------------- |
|   [1]   | `FromImage`     | graphics factory | creates draw context |
|   [2]   | `DrawLine`      | rendering call   | renders evidence     |
|   [3]   | `DrawString`    | rendering call   | renders evidence     |
|   [4]   | `FillRectangle` | rendering call   | renders evidence     |
|   [5]   | `Save`          | save method      | writes image output  |
|   [6]   | `Dispose`       | operation call   | executes operation   |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `System.Drawing.Common`
- Owns: host drawing interop
- Accept: drawing stays boundary support
- Reject: GDI as product visual rail

