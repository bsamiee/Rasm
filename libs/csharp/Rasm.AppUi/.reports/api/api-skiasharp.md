# [RASM_APPUI_API_SKIASHARP]

`SkiaSharp` supplies raster surfaces, paths, paints, images, color, typefaces, and offscreen drawing primitives.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SkiaSharp`
- package: `SkiaSharp`
- assembly: `SkiaSharp`
- namespace: `SkiaSharp`
- asset: runtime library
- rail: visuals

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: drawing family
- rail: visuals

| [INDEX] | [SYMBOL]     | [PACKAGE_ROLE]  | [CAPABILITY]             |
| :-----: | :----------- | :-------------- | :----------------------- |
|   [1]   | `SKCanvas`   | drawing surface | draws visual evidence    |
|   [2]   | `SKPaint`    | drawing surface | draws visual evidence    |
|   [3]   | `SKPath`     | drawing surface | draws visual evidence    |
|   [4]   | `SKBitmap`   | drawing surface | draws visual evidence    |
|   [5]   | `SKImage`    | drawing surface | draws visual evidence    |
|   [6]   | `SKSurface`  | draw target     | anchors visuals contract |
|   [7]   | `SKColor`    | drawing surface | draws visual evidence    |
|   [8]   | `SKTypeface` | font face       | anchors visuals contract |
|   [9]   | `SKData`     | encoded bytes   | anchors visuals contract |
|  [10]   | `SKRect`     | draw bounds     | anchors visuals contract |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: drawing operations
- rail: visuals

| [INDEX] | [SURFACE]   | [CALL_SHAPE]   | [CAPABILITY]              |
| :-----: | :---------- | :------------- | :------------------------ |
|   [1]   | `DrawPath`  | rendering call | renders evidence          |
|   [2]   | `DrawText`  | rendering call | renders evidence          |
|   [3]   | `DrawImage` | rendering call | renders evidence          |
|   [4]   | `Encode`    | operation call | executes operation        |
|   [5]   | `Decode`    | operation call | executes operation        |
|   [6]   | `Create`    | factory call   | creates configured handle |
|   [7]   | `Save`      | save method    | writes image output       |
|   [8]   | `Restore`   | member surface | drives visuals behavior   |
|   [9]   | `Dispose`   | operation call | executes operation        |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `SkiaSharp`
- Owns: custom raster visuals
- Accept: offscreen visuals emit evidence
- Reject: GDI public vocabulary

