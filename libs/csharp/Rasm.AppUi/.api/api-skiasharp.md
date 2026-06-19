# [RASM_APPUI_API_SKIASHARP]

`SkiaSharp` supplies raster surfaces, canvases, paths, paints, images, codecs, streams, text, colors, shaders, and GPU drawing primitives.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SkiaSharp`
- package: `SkiaSharp`
- assembly: `SkiaSharp`
- namespace: `SkiaSharp`
- asset: neutral runtime library
- asset: platform runtime libraries
- rail: visuals

## [02]-[PUBLIC_TYPES]

[DRAWING_TYPES]: canvas, paint, and geometry — rail: visuals

| [INDEX] | [SYMBOL]        | [KIND]          |
| :-----: | :-------------- | :-------------- |
|  [01]   | `SKCanvas`      | drawing surface |
|  [02]   | `SKPaint`       | paint state     |
|  [03]   | `SKPath`        | vector path     |
|  [04]   | `SKPathMeasure` | path measure    |
|  [05]   | `SKRect`        | bounds value    |
|  [06]   | `SKRoundRect`   | rounded bounds  |
|  [07]   | `SKPoint`       | point value     |
|  [08]   | `SKMatrix`      | transform value |

[SURFACE_AND_IMAGE_TYPES]: pixel and image ownership — rail: visuals

| [INDEX] | [SYMBOL]      | [KIND]          |
| :-----: | :------------ | :-------------- |
|  [01]   | `SKSurface`   | draw target     |
|  [02]   | `SKImage`     | immutable image |
|  [03]   | `SKBitmap`    | mutable bitmap  |
|  [04]   | `SKPixmap`    | pixel map       |
|  [05]   | `SKImageInfo` | image metadata  |
|  [06]   | `SKCodec`     | decode codec    |
|  [07]   | `SKData`      | byte buffer     |
|  [08]   | `SKDocument`  | document output |

[TEXT_AND_FONT_TYPES]: text and typeface surface — rail: visuals

| [INDEX] | [SYMBOL]            | [KIND]          |
| :-----: | :------------------ | :-------------- |
|  [01]   | `SKFont`            | font object     |
|  [02]   | `SKTypeface`        | typeface object |
|  [03]   | `SKFontManager`     | font registry   |
|  [04]   | `SKFontMetrics`     | font metrics    |
|  [05]   | `SKTextBlob`        | shaped text     |
|  [06]   | `SKTextBlobBuilder` | text builder    |

[PAINT_PIPELINE_TYPES]: color, shader, filter, and runtime render surfaces — rail: visuals

| [INDEX] | [SYMBOL]                 | [KIND]          |
| :-----: | :----------------------- | :-------------- |
|  [01]   | `SKColor`                | byte color      |
|  [02]   | `SKColorF`               | float color     |
|  [03]   | `SKColors`               | color constants |
|  [04]   | `SKColorSpace`           | color space     |
|  [05]   | `SKColorSpaceXyz`        | ICC primaries   |
|  [06]   | `SKColorSpaceTransferFn` | transfer curve  |
|  [07]   | `SKShader`               | shader object   |
|  [08]   | `SKShaderTileMode`       | gradient tile   |
|  [09]   | `SKImageFilter`          | image filter    |
|  [10]   | `SKColorFilter`          | color filter    |
|  [11]   | `SKPathEffect`           | path effect     |
|  [12]   | `SKMaskFilter`           | mask filter     |
|  [13]   | `SKRuntimeEffect`        | runtime shader  |

[CODEC_AND_FORMAT_TYPES]: encode format, pixel layout, and alpha enums — rail: visuals

| [INDEX] | [SYMBOL]               | [KIND]        |
| :-----: | :--------------------- | :------------ |
|  [01]   | `SKEncodedImageFormat` | encode format |
|  [02]   | `SKColorType`          | pixel layout  |
|  [03]   | `SKAlphaType`          | alpha mode    |

[GPU_TYPES]: GPU context and backend surface — rail: visuals

| [INDEX] | [SYMBOL]                | [KIND]           |
| :-----: | :---------------------- | :--------------- |
|  [01]   | `GRContext`             | GPU context      |
|  [02]   | `GRRecordingContext`    | GPU recording    |
|  [03]   | `GRBackendRenderTarget` | backend target   |
|  [04]   | `GRBackendTexture`      | backend texture  |
|  [05]   | `GRGlInterface`         | OpenGL interface |
|  [06]   | `GRMtlBackendContext`   | Metal backend    |
|  [07]   | `GRVkBackendContext`    | Vulkan backend   |
|  [08]   | `GRD3DBackendContext`   | Direct3D backend |

## [03]-[ENTRYPOINTS]

[CANVAS_ENTRYPOINTS]: drawing and transform operations
- rail: visuals

| [INDEX] | [SURFACE]       | [SURFACE_ROOT] | [RAIL]          |
| :-----: | :-------------- | :------------- | :-------------- |
|  [01]   | `DrawPath`      | `SKCanvas`     | path draw       |
|  [02]   | `DrawText`      | `SKCanvas`     | text draw       |
|  [03]   | `DrawImage`     | `SKCanvas`     | image draw      |
|  [04]   | `DrawRect`      | `SKCanvas`     | rectangle draw  |
|  [05]   | `DrawRoundRect` | `SKCanvas`     | round rect draw |
|  [06]   | `DrawCircle`    | `SKCanvas`     | circle draw     |
|  [07]   | `DrawLine`      | `SKCanvas`     | line draw       |
|  [08]   | `DrawColor`     | `SKCanvas`     | fill draw       |
|  [09]   | `ClipPath`      | `SKCanvas`     | clip path       |
|  [10]   | `Save`          | `SKCanvas`     | state save      |
|  [11]   | `Restore`       | `SKCanvas`     | state restore   |
|  [12]   | `Translate`     | `SKCanvas`     | transform       |
|  [13]   | `Scale`         | `SKCanvas`     | transform       |
|  [14]   | `RotateDegrees` | `SKCanvas`     | transform       |

[IMAGE_ENTRYPOINTS]: image, bitmap, codec, and pixel operations
- rail: visuals

| [INDEX] | [SURFACE]    | [SURFACE_ROOT] | [RAIL]         |
| :-----: | :----------- | :------------- | :------------- |
|  [01]   | `Create`     | `SKSurface`    | surface create |
|  [02]   | `Snapshot`   | `SKSurface`    | snapshot image |
|  [03]   | `Decode`     | `SKBitmap`     | bitmap decode  |
|  [04]   | `Encode`     | `SKBitmap`     | bitmap encode  |
|  [05]   | `GetPixels`  | `SKCodec`      | codec decode   |
|  [06]   | `FromBitmap` | `SKImage`      | image create   |
|  [07]   | `ReadPixels` | `SKImage`      | pixel read     |
|  [08]   | `ReadPixels` | `SKPixmap`     | pixel read     |
|  [09]   | `SetPixel`   | `SKBitmap`     | pixel write    |
|  [10]   | `Encode`     | `SKImage`      | image encode   |
|  [11]   | `ColorSpace` | `SKImage`      | image space    |
|  [12]   | `ToArray`    | `SKData`       | byte buffer    |

[DOCUMENT_AND_COLOR_ENTRYPOINTS]: paged document export and color-managed reproject
- rail: visuals

| [INDEX] | [SURFACE]          | [SURFACE_ROOT] | [RAIL]            |
| :-----: | :----------------- | :------------- | :---------------- |
|  [01]   | `CreatePdf`        | `SKDocument`   | PDF backend       |
|  [02]   | `CreateXps`        | `SKDocument`   | XPS backend       |
|  [03]   | `BeginPage`        | `SKDocument`   | page open         |
|  [04]   | `EndPage`          | `SKDocument`   | page commit       |
|  [05]   | `Close`            | `SKDocument`   | document finalize |
|  [06]   | `Abort`            | `SKDocument`   | document abort    |
|  [07]   | `CreateSrgb`       | `SKColorSpace` | sRGB space        |
|  [08]   | `CreateSrgbLinear` | `SKColorSpace` | linear space      |
|  [09]   | `CreateRgb`        | `SKColorSpace` | ICC-primary space |
|  [10]   | `Equal`            | `SKColorSpace` | space identity    |
|  [11]   | `WithColorSpace`   | `SKImageInfo`  | space retag       |

[TEXT_AND_PAINT_ENTRYPOINTS]: text, color, shader, and render operations
- rail: visuals

| [INDEX] | [SURFACE]              | [SURFACE_ROOT]    | [RAIL]          |
| :-----: | :--------------------- | :---------------- | :-------------- |
|  [01]   | `MeasureText`          | `SKFont`          | text measure    |
|  [02]   | `CreateTypeface`       | `SKFontManager`   | typeface lookup |
|  [03]   | `Create`               | `SKTextBlob`      | shaped text     |
|  [04]   | `Parse`                | `SKColor`         | color parse     |
|  [05]   | `CreateColor`          | `SKShader`        | color shader    |
|  [06]   | `CreateLinearGradient` | `SKShader`        | gradient shader |
|  [07]   | `CreateBlur`           | `SKImageFilter`   | blur filter     |
|  [08]   | `CreateDash`           | `SKPathEffect`    | dashed stroke   |
|  [09]   | `CreateShader`         | `SKRuntimeEffect` | runtime shader  |

[GPU_ENTRYPOINTS]: GPU context creation
- rail: visuals

| [INDEX] | [SURFACE]        | [SURFACE_ROOT]   | [RAIL]           |
| :-----: | :--------------- | :--------------- | :--------------- |
|  [01]   | `CreateGl`       | `GRContext`      | OpenGL context   |
|  [02]   | `CreateMetal`    | `GRContext`      | Metal context    |
|  [03]   | `CreateVulkan`   | `GRContext`      | Vulkan context   |
|  [04]   | `CreateDirect3D` | `GRContext`      | Direct3D context |
|  [05]   | `Create`         | `GRGlInterface`  | GL interface     |
|  [06]   | `Dispose`        | `SKNativeObject` | native release   |

## [04]-[IMPLEMENTATION_LAW]

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
