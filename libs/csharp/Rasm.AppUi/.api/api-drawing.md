# [RASM_APPUI_API_DRAWING]

`System.Drawing.Common` supplies bitmap, image, graphics, drawing2D, font, icon, and imaging support for host drawing interop in host-aware projects.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Drawing.Common`
- package: `System.Drawing.Common`
- assembly: `System.Drawing.Common`
- namespace: `System.Drawing`
- namespace: `System.Drawing.Drawing2D`
- namespace: `System.Drawing.Imaging`
- asset: compile package
- rail: host-drawing

## [2]-[PUBLIC_TYPES]

[DRAWING_TYPES]: bitmap, graphics, and primitive surface
- rail: host-drawing

| [INDEX] | [SYMBOL]       | [RAIL]          |
| :-----: | :------------- | :-------------- |
|   [1]   | `Bitmap`       | bitmap image    |
|   [2]   | `Image`        | image base      |
|   [3]   | `Graphics`     | drawing context |
|   [4]   | `Pen`          | stroke resource |
|   [5]   | `Brush`        | fill resource   |
|   [6]   | `SolidBrush`   | solid fill      |
|   [7]   | `TextureBrush` | texture fill    |
|   [8]   | `Font`         | font resource   |
|   [9]   | `Icon`         | icon resource   |

[DRAWING2D_TYPES]: path, transform, and quality surface
- rail: host-drawing

| [INDEX] | [SYMBOL]              | [RAIL]        |
| :-----: | :-------------------- | :------------ |
|   [1]   | `GraphicsPath`        | vector path   |
|   [2]   | `Matrix`              | transform     |
|   [3]   | `LinearGradientBrush` | gradient fill |
|   [4]   | `PathGradientBrush`   | path gradient |
|   [5]   | `HatchBrush`          | hatch fill    |
|   [6]   | `SmoothingMode`       | quality mode  |
|   [7]   | `InterpolationMode`   | image quality |

[IMAGING_TYPES]: pixel and encoder surface
- rail: host-drawing

| [INDEX] | [SYMBOL]            | [RAIL]       |
| :-----: | :------------------ | :----------- |
|   [1]   | `BitmapData`        | pixel lock   |
|   [2]   | `ImageFormat`       | image format |
|   [3]   | `ImageCodecInfo`    | codec info   |
|   [4]   | `Encoder`           | encoder key  |
|   [5]   | `EncoderParameters` | encoder args |
|   [6]   | `ColorPalette`      | palette data |
|   [7]   | `PixelFormat`       | pixel format |

## [3]-[ENTRYPOINTS]

[BITMAP_ENTRYPOINTS]: bitmap and image operations
- rail: host-drawing

| [INDEX] | [SURFACE]    | [SURFACE_ROOT] | [RAIL]        |
| :-----: | :----------- | :------------- | :------------ |
|   [1]   | constructor  | `Bitmap`       | bitmap create |
|   [2]   | `FromStream` | `Image`        | image load    |
|   [3]   | `Clone`      | `Bitmap`       | bitmap clone  |
|   [4]   | `LockBits`   | `Bitmap`       | pixel lock    |
|   [5]   | `UnlockBits` | `Bitmap`       | pixel unlock  |
|   [6]   | `GetPixel`   | `Bitmap`       | pixel read    |
|   [7]   | `SetPixel`   | `Bitmap`       | pixel write   |
|   [8]   | `Save`       | `Image`        | image save    |
|   [9]   | `RotateFlip` | `Image`        | transform     |

[GRAPHICS_ENTRYPOINTS]: drawing operations
- rail: host-drawing

| [INDEX] | [SURFACE]       | [SURFACE_ROOT]  | [RAIL]          |
| :-----: | :-------------- | :-------------- | :-------------- |
|   [1]   | `FromImage`     | `Graphics`      | graphics create |
|   [2]   | `DrawLine`      | `Graphics`      | line draw       |
|   [3]   | `DrawString`    | `Graphics`      | text draw       |
|   [4]   | `DrawImage`     | `Graphics`      | image draw      |
|   [5]   | `DrawPath`      | `Graphics`      | path draw       |
|   [6]   | `FillRectangle` | `Graphics`      | rectangle fill  |
|   [7]   | `FillPath`      | `Graphics`      | path fill       |
|   [8]   | `MeasureString` | `Graphics`      | text measure    |
|   [9]   | `Dispose`       | drawing objects | native release  |

## [4]-[IMPLEMENTATION_LAW]

[HOST_DRAWING_LAW]:
- Package: `System.Drawing.Common`
- Owns: host drawing interop for bitmap exchange, legacy viewport capture, and host-provided image APIs
- Accept: drawing stays boundary support and never becomes the AppUi visual rail
- Reject: GDI as product visual rail

[BOUNDARY_LAW]:
- Package: `System.Drawing.Common`
- Owns: Rhino/GH/Eto compatibility where host APIs require drawing primitives
- Accept: Skia and Avalonia remain the product visual owners
- Reject: System.Drawing types in product shell, screen, theme, chart, or asset contracts
