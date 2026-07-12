# [RASM_API_SYSTEM_DRAWING_COMMON]

`System.Drawing.Common` (MIT) is a conditional host compile surface. `Directory.Build.props` references the package with `ExcludeAssets="runtime"`, and `NeedsRhinoHostUiSurface` binds a host `HintPath`; the catalog records the compile-only GDI+ type surface the Rhino/Eto seam needs without admitting package runtime ownership.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Drawing.Common`
- package: `System.Drawing.Common`
- license: MIT
- assembly: `System.Drawing.Common`
- namespace: `System.Drawing`, `System.Drawing.Imaging`, `System.Drawing.Drawing2D`
- asset: compile surface only under `ExcludeAssets="runtime"`; host reference binds under `NeedsRhinoHostUiSurface`
- rail: host-ui-compile

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: image and drawing carriers
- rail: host-ui-compile

| [INDEX] | [SYMBOL]                       | [CAPABILITY]                        |
| :-----: | :----------------------------- | :---------------------------------- |
|  [01]   | `Bitmap`                       | bitmap image carrier                |
|  [02]   | `Image`                        | image base class and stream/file IO |
|  [03]   | `Icon`                         | icon carrier                        |
|  [04]   | `Graphics`                     | drawing context                     |
|  [05]   | `Color`                        | ARGB colour value                   |
|  [06]   | `Point` / `PointF`             | point values                        |
|  [07]   | `Size` / `SizeF`               | size values                         |
|  [08]   | `Rectangle` / `RectangleF`     | rectangle values                    |
|  [09]   | `Font`                         | text font carrier                   |
|  [10]   | `Pen` / `Brush` / `SolidBrush` | drawing style carriers              |

[PUBLIC_TYPE_SCOPE]: imaging and transform contracts
- rail: host-ui-compile

| [INDEX] | [SYMBOL]        | [CAPABILITY]                      |
| :-----: | :-------------- | :-------------------------------- |
|  [01]   | `ImageFormat`   | image codec discriminator         |
|  [02]   | `PixelFormat`   | bitmap pixel format discriminator |
|  [03]   | `BitmapData`    | locked bitmap memory descriptor   |
|  [04]   | `ImageLockMode` | bitmap lock access discriminator  |
|  [05]   | `GraphicsPath`  | vector path carrier               |
|  [06]   | `Matrix`        | drawing transform matrix          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: host UI image seam
- rail: host-ui-compile

| [INDEX] | [SURFACE]                                                       | [CALL_SHAPE] | [CAPABILITY]                 |
| :-----: | :-------------------------------------------------------------- | :----------- | :--------------------------- |
|  [01]   | `new Bitmap(int width, int height)`                             | constructor  | creates a bitmap             |
|  [02]   | `new Bitmap(Stream)` / `new Bitmap(string)`                     | constructor  | loads bitmap content         |
|  [03]   | `Image.FromStream(Stream)`                                      | static read  | loads an image               |
|  [04]   | `Image.Save(Stream, ImageFormat)` / `Save(string, ImageFormat)` | instance     | writes encoded image content |
|  [05]   | `Bitmap.LockBits(Rectangle, ImageLockMode, PixelFormat)`        | instance     | locks pixel memory           |
|  [06]   | `Bitmap.UnlockBits(BitmapData)`                                 | instance     | releases locked pixel memory |
|  [07]   | `Graphics.FromImage(Image)`                                     | static       | creates a drawing context    |
|  [08]   | `Color.FromArgb(...)`                                           | static       | creates an ARGB colour       |

## [04]-[IMPLEMENTATION_LAW]

[COMPILE_ONLY]:
- The package reference excludes runtime assets.
- `NeedsRhinoHostUiSurface` binds the host reference through `HintPath`.
- The seam compiles against `System.Drawing` types where Rhino/Eto APIs expose them; it does not select or deploy a runtime provider.

[LOCAL_ADMISSION]:
- Host UI interop may name `Bitmap`, `Image`, `Icon`, `Graphics`, and primitive drawing values at the boundary.
- Library logic does not depend on `System.Drawing.Common` runtime behavior.
- Image processing, rendering, and platform graphics ownership stay with their dedicated surfaces; this catalog is only the host compile contract.

[RAIL_LAW]:
- Package: `System.Drawing.Common`
- Owns: compile-time `System.Drawing`/imaging type names for the Rhino/Eto host seam
- Accept: host compile references to bitmap, icon, graphics, drawing value, and imaging contracts
- Reject: runtime packaging, cross-platform drawing execution, or non-host image-processing ownership
