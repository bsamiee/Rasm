# [RASM_APPUI_API_SKIASHARP]

`SkiaSharp` supplies raster surfaces, canvases, paths, paints, images, codecs, streams, text, colors, shaders, and GPU drawing primitives.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SkiaSharp`
- package: `SkiaSharp`
- assembly: `SkiaSharp`
- namespace: `SkiaSharp`
- asset: neutral runtime library
- asset: platform runtime libraries
- rail: visuals

## [2]-[PUBLIC_TYPES]

[DRAWING_TYPES]: canvas, paint, and geometry — rail: visuals

| [INDEX] | [SYMBOL]        | [KIND]          |
| :-----: | :-------------- | :-------------- |
|   [1]   | `SKCanvas`      | drawing surface |
|   [2]   | `SKPaint`       | paint state     |
|   [3]   | `SKPath`        | vector path     |
|   [4]   | `SKPathMeasure` | path measure    |
|   [5]   | `SKRect`        | bounds value    |
|   [6]   | `SKRoundRect`   | rounded bounds  |
|   [7]   | `SKPoint`       | point value     |
|   [8]   | `SKMatrix`      | transform value |

[SURFACE_AND_IMAGE_TYPES]: pixel and image ownership — rail: visuals

| [INDEX] | [SYMBOL]      | [KIND]          |
| :-----: | :------------ | :-------------- |
|   [1]   | `SKSurface`   | draw target     |
|   [2]   | `SKImage`     | immutable image |
|   [3]   | `SKBitmap`    | mutable bitmap  |
|   [4]   | `SKPixmap`    | pixel map       |
|   [5]   | `SKImageInfo` | image metadata  |
|   [6]   | `SKCodec`     | decode codec    |
|   [7]   | `SKData`      | byte buffer     |
|   [8]   | `SKDocument`  | document output |

[TEXT_AND_FONT_TYPES]: text and typeface surface — rail: visuals

| [INDEX] | [SYMBOL]            | [KIND]          |
| :-----: | :------------------ | :-------------- |
|   [1]   | `SKFont`            | font object     |
|   [2]   | `SKTypeface`        | typeface object |
|   [3]   | `SKFontManager`     | font registry   |
|   [4]   | `SKFontMetrics`     | font metrics    |
|   [5]   | `SKTextBlob`        | shaped text     |
|   [6]   | `SKTextBlobBuilder` | text builder    |

[PAINT_PIPELINE_TYPES]: color, shader, filter, and runtime render surfaces — rail: visuals

| [INDEX] | [SYMBOL]                 | [KIND]          |
| :-----: | :----------------------- | :-------------- |
|   [1]   | `SKColor`                | byte color      |
|   [2]   | `SKColorF`               | float color     |
|   [3]   | `SKColors`               | color constants |
|   [4]   | `SKColorSpace`           | color space     |
|   [5]   | `SKColorSpaceXyz`        | ICC primaries   |
|   [6]   | `SKColorSpaceTransferFn` | transfer curve  |
|   [7]   | `SKShader`               | shader object   |
|   [8]   | `SKShaderTileMode`       | gradient tile   |
|   [9]   | `SKImageFilter`          | image filter    |
|  [10]   | `SKColorFilter`          | color filter    |
|  [11]   | `SKPathEffect`           | path effect     |
|  [12]   | `SKMaskFilter`           | mask filter     |
|  [13]   | `SKRuntimeEffect`        | runtime shader  |

[CODEC_AND_FORMAT_TYPES]: encode format, pixel layout, and alpha enums — rail: visuals

| [INDEX] | [SYMBOL]               | [KIND]        |
| :-----: | :--------------------- | :------------ |
|   [1]   | `SKEncodedImageFormat` | encode format |
|   [2]   | `SKColorType`          | pixel layout  |
|   [3]   | `SKAlphaType`          | alpha mode    |

[GPU_TYPES]: GPU context and backend surface — rail: visuals

| [INDEX] | [SYMBOL]                | [KIND]           |
| :-----: | :---------------------- | :--------------- |
|   [1]   | `GRContext`             | GPU context      |
|   [2]   | `GRRecordingContext`    | GPU recording    |
|   [3]   | `GRBackendRenderTarget` | backend target   |
|   [4]   | `GRBackendTexture`      | backend texture  |
|   [5]   | `GRGlInterface`         | OpenGL interface |
|   [6]   | `GRMtlBackendContext`   | Metal backend    |
|   [7]   | `GRVkBackendContext`    | Vulkan backend   |
|   [8]   | `GRD3DBackendContext`   | Direct3D backend |

## [3]-[ENTRYPOINTS]

[CANVAS_ENTRYPOINTS]: drawing and transform operations
- rail: visuals

| [INDEX] | [SURFACE]       | [SURFACE_ROOT] | [RAIL]          |
| :-----: | :-------------- | :------------- | :-------------- |
|   [1]   | `DrawPath`      | `SKCanvas`     | path draw       |
|   [2]   | `DrawText`      | `SKCanvas`     | text draw       |
|   [3]   | `DrawImage`     | `SKCanvas`     | image draw      |
|   [4]   | `DrawRect`      | `SKCanvas`     | rectangle draw  |
|   [5]   | `DrawRoundRect` | `SKCanvas`     | round rect draw |
|   [6]   | `DrawCircle`    | `SKCanvas`     | circle draw     |
|   [7]   | `DrawLine`      | `SKCanvas`     | line draw       |
|   [8]   | `DrawColor`     | `SKCanvas`     | fill draw       |
|   [9]   | `ClipPath`      | `SKCanvas`     | clip path       |
|  [10]   | `Save`          | `SKCanvas`     | state save      |
|  [11]   | `Restore`       | `SKCanvas`     | state restore   |
|  [12]   | `Translate`     | `SKCanvas`     | transform       |
|  [13]   | `Scale`         | `SKCanvas`     | transform       |
|  [14]   | `RotateDegrees` | `SKCanvas`     | transform       |

[IMAGE_ENTRYPOINTS]: image, bitmap, codec, and pixel operations
- rail: visuals

| [INDEX] | [SURFACE]    | [SURFACE_ROOT] | [RAIL]         |
| :-----: | :----------- | :------------- | :------------- |
|   [1]   | `Create`     | `SKSurface`    | surface create |
|   [2]   | `Snapshot`   | `SKSurface`    | snapshot image |
|   [3]   | `Decode`     | `SKBitmap`     | bitmap decode  |
|   [4]   | `Encode`     | `SKBitmap`     | bitmap encode  |
|   [5]   | `GetPixels`  | `SKCodec`      | codec decode   |
|   [6]   | `FromBitmap` | `SKImage`      | image create   |
|   [7]   | `ReadPixels` | `SKImage`      | pixel read     |
|   [8]   | `ReadPixels` | `SKPixmap`     | pixel read     |
|   [9]   | `SetPixel`   | `SKBitmap`     | pixel write    |
|  [10]   | `Encode`     | `SKImage`      | image encode   |
|  [11]   | `ColorSpace` | `SKImage`      | image space    |
|  [12]   | `ToArray`    | `SKData`       | byte buffer    |

[DOCUMENT_AND_COLOR_ENTRYPOINTS]: paged document export and color-managed reproject
- rail: visuals

| [INDEX] | [SURFACE]          | [SURFACE_ROOT] | [RAIL]            |
| :-----: | :----------------- | :------------- | :---------------- |
|   [1]   | `CreatePdf`        | `SKDocument`   | PDF backend       |
|   [2]   | `CreateXps`        | `SKDocument`   | XPS backend       |
|   [3]   | `BeginPage`        | `SKDocument`   | page open         |
|   [4]   | `EndPage`          | `SKDocument`   | page commit       |
|   [5]   | `Close`            | `SKDocument`   | document finalize |
|   [6]   | `Abort`            | `SKDocument`   | document abort    |
|   [7]   | `CreateSrgb`       | `SKColorSpace` | sRGB space        |
|   [8]   | `CreateSrgbLinear` | `SKColorSpace` | linear space      |
|   [9]   | `CreateRgb`        | `SKColorSpace` | ICC-primary space |
|  [10]   | `Equal`            | `SKColorSpace` | space identity    |
|  [11]   | `WithColorSpace`   | `SKImageInfo`  | space retag       |

[TEXT_AND_PAINT_ENTRYPOINTS]: text, color, shader, and render operations
- rail: visuals

| [INDEX] | [SURFACE]              | [SURFACE_ROOT]    | [RAIL]          |
| :-----: | :--------------------- | :---------------- | :-------------- |
|   [1]   | `MeasureText`          | `SKFont`          | text measure    |
|   [2]   | `CreateTypeface`       | `SKFontManager`   | typeface lookup |
|   [3]   | `Create`               | `SKTextBlob`      | shaped text     |
|   [4]   | `Parse`                | `SKColor`         | color parse     |
|   [5]   | `CreateColor`          | `SKShader`        | color shader    |
|   [6]   | `CreateLinearGradient` | `SKShader`        | gradient shader |
|   [7]   | `CreateBlur`           | `SKImageFilter`   | blur filter     |
|   [8]   | `CreateDash`           | `SKPathEffect`    | dashed stroke   |
|   [9]   | `CreateShader`         | `SKRuntimeEffect` | runtime shader  |

[GPU_ENTRYPOINTS]: GPU context creation
- rail: visuals

| [INDEX] | [SURFACE]        | [SURFACE_ROOT]   | [RAIL]           |
| :-----: | :--------------- | :--------------- | :--------------- |
|   [1]   | `CreateGl`       | `GRContext`      | OpenGL context   |
|   [2]   | `CreateMetal`    | `GRContext`      | Metal context    |
|   [3]   | `CreateVulkan`   | `GRContext`      | Vulkan context   |
|   [4]   | `CreateDirect3D` | `GRContext`      | Direct3D context |
|   [5]   | `Create`         | `GRGlInterface`  | GL interface     |
|   [6]   | `Dispose`        | `SKNativeObject` | native release   |

## [4]-[IMPLEMENTATION_LAW]

[VISUALS_LAW]:
- Package: `SkiaSharp`
- Owns: raster drawing, offscreen surfaces, image codecs, text, colors, effects, and GPU context primitives
- Accept: custom visuals emit deterministic bitmap, encoded image, and diagnostic evidence
- Reject: GDI public vocabulary

[ASSET_LAW]:
- Package: `SkiaSharp`
- Owns: native-backed disposable objects and explicit pixel ownership
- Accept: every surface, image, codec, stream, and context is lifecycle-scoped
- Reject: ambient unmanaged resource ownership
