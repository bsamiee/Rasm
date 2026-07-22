# [RASM_APPUI_API_LCMSNET]

`lcmsNET` owns ICC-managed device color for the print and export deliverable: the `Profile` handle, the compiled `Transform` pipeline, the `Intent` and `CmsFlags` policy vocabularies, and the `Cms` pixel-format hub, all under one root namespace binding Little CMS 2 through P/Invoke. It owns profiled device-color egress where `Wacton.Unicolour` owns screen-perceptual UI color — the screen-versus-ICC-device seam the two never cross. Native `lcms2` provisioning stays at the app-host distribution layer, never bundled with this assembly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lcmsNET`
- package: `lcmsNET` (`MIT`, Little CMS)
- assembly: `lcmsNET`
- namespace: `lcmsNET` (handles, enums, colorimetric structs, the `Cms` static hub)
- native: P/Invoke over the `lcms2` (`liblcms2`) shared library; the managed layer marshals, the native binary is external
- rail: color

## [02]-[PUBLIC_TYPES]

[HANDLE_AND_PRIMITIVE_TYPES]: disposable ICC handles, context scope, and colorimetric primitives

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :---------------------- | :------------ | :------------------------------ |
|  [01]   | `Profile`               | class         | ICC profile handle              |
|  [02]   | `Transform`             | class         | compiled color-conversion run   |
|  [03]   | `Context`               | class         | thread/plugin scope             |
|  [04]   | `Pipeline`              | class         | LUT/CLUT pipeline               |
|  [05]   | `Stage`                 | class         | matrix/CLUT/curve/identity op   |
|  [06]   | `ToneCurve`             | class         | gamma/parametric/table transfer |
|  [07]   | `MultiLocalizedUnicode` | class         | `mluc` localized text payload   |
|  [08]   | `NamedColorList`        | class         | named-color payload             |
|  [09]   | `CIEXYZ`                | struct        | tristimulus values              |
|  [10]   | `CIExyY`                | struct        | chromaticity values             |
|  [11]   | `CIELab`                | struct        | Lab values                      |
|  [12]   | `CIExyYTRIPLE`          | struct        | RGB primary chromaticities      |

[ENUM_VOCABULARY]: bounded color-management enums keyed by `Cms` and `Profile` policy

| [INDEX] | [SYMBOL]                 | [CAPABILITY]          |
| :-----: | :----------------------- | :-------------------- |
|  [01]   | `Intent`                 | rendering intent      |
|  [02]   | `CmsFlags`               | transform policy      |
|  [03]   | `PixelType`              | logical channel model |
|  [04]   | `ColorSpaceSignature`    | ICC color space       |
|  [05]   | `ProfileClassSignature`  | profile device class  |
|  [06]   | `TagSignature`           | ICC tag identity      |
|  [07]   | `InfoType`               | profile-info selector |
|  [08]   | `UsedDirection`          | intent direction      |
|  [09]   | `PostScriptResourceType` | PostScript resource   |

[INTENT_MEMBERS]:
- Standard: `Perceptual` `RelativeColorimetric` `Saturation` `AbsoluteColorimetric`
- K-only: `PreserveKOnlyPerceptual` `PreserveKOnlyRelativeColorimetric` `PreserveKOnlySaturation`
- K-plane: `PreserveKPlanePerceptual` `PreserveKPlaneRelativeColorimetric` `PreserveKPlaneSaturation`

[CMS_FLAG_MEMBERS]:
- Conversion: `BlackPointCompensation` `SoftProofing` `GamutCheck` `NullTransform` `NoNegatives` `NoWhiteOnWhiteFixUp`
- Compilation: `NoCache` `NoOptimize` `HighResPreCalc` `LowResPreCalc` `ForceCLut` `CLutPreLinearization` `CLutPostLinearization`
- Device link: `KeepSequence` `EightBitsDeviceLink` `GuessDeviceClass` `NoDefaultResourceDef`

[PIXEL_TYPE_MEMBERS]: `Gray` `RGB` `CMY` `CMYK` `YCbCr` `XYZ` `Lab` `Yxy` `HSV` `HLS` `MCH1`..`MCH15` `LabV2` `Any`

[SIGNATURE_MEMBERS]:
- `ColorSpaceSignature`: `XYZData` `LabData` `RgbData` `GrayData` `CmykData` across the ICC color-space set
- `ProfileClassSignature`: `Input` `Display` `Output` `Link` `Abstract` `ColorSpace` `NamedColor`
- `TagSignature`: ICC tag identities including `AToB0` `BToA0` `Gamut` `BlueColorant` `BlueTRC` `Copyright`

[TAG_PAYLOAD_TYPES]: strongly-typed ICC tag payloads and measurement helpers

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]             |
| :-----: | :-------------------------- | :------------ | :----------------------- |
|  [01]   | `ICCData`                   | class         | raw-data tag             |
|  [02]   | `UcrBg`                     | class         | under-color removal / BG |
|  [03]   | `VideoCardGamma`            | class         | VCGT tag                 |
|  [04]   | `Screening`                 | struct        | halftone-screening tag   |
|  [05]   | `ColorantOrder`             | struct        | colorant-order tag       |
|  [06]   | `ProfileSequenceDescriptor` | class         | device-link provenance   |
|  [07]   | `ProfileSequenceItem`       | class         | provenance entry         |
|  [08]   | `GamutBoundaryDescriptor`   | class         | gamut-boundary handle    |
|  [09]   | `Dict`                      | class         | `meta` dictionary        |
|  [10]   | `DictEntry`                 | class         | metadata entry           |
|  [11]   | `IT8`                       | class         | IT8.7/CGATS table I/O    |
|  [12]   | `DeltaE`                    | class         | color-difference metrics |
|  [13]   | `CAM02`                     | class         | CIECAM02 appearance      |
|  [14]   | `IOHandler`                 | class         | in-memory/stream I/O     |
|  [15]   | `LcmsNETException`          | class         | typed lcms2 failure      |

## [03]-[ENTRYPOINTS]

[PROFILE_LIFECYCLE]: create, open, save, and introspect on `Profile` — the transform operands (input device, output print, proofing target)

| [INDEX] | [SURFACE]                                                                                  | [SHAPE]  | [CAPABILITY]               |
| :-----: | :----------------------------------------------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `Open(string, string)`                                                                     | factory  | file ingress, path+access  |
|  [02]   | `Open(byte[])`                                                                             | factory  | in-memory ingress          |
|  [03]   | `Open(Context, IOHandler, bool)`                                                           | factory  | handler ingress, writeable |
|  [04]   | `Create_sRGB / Create_OkLab / CreateXYZ / CreateLab2 / CreateLab4`                         | factory  | built-in working spaces    |
|  [05]   | `CreateRGB(in CIExyY, in CIExyYTRIPLE, ToneCurve[])`                                       | factory  | RGB space from primaries   |
|  [06]   | `CreateGray(in CIExyY, ToneCurve)`                                                         | factory  | gray space from white/TRC  |
|  [07]   | `CreateInkLimitingDeviceLink(ColorSpaceSignature, double)`                                 | factory  | ink-limit device link      |
|  [08]   | `CreateLinearizationDeviceLink(ColorSpaceSignature, ToneCurve[])`                          | factory  | linearization device link  |
|  [09]   | `CreateDeviceLink(Transform, double, CmsFlags)`                                            | factory  | bake transform to link     |
|  [10]   | `CreateDeviceLinkFromCubeFile(string)`                                                     | factory  | `.cube` device link        |
|  [11]   | `CreateBCHSWabstract(int, double, double, double, double, int, int)`                       | factory  | BCHS abstract profile      |
|  [12]   | `CreateNull / CreatePlaceholder(Context?)`                                                 | factory  | null / empty shell         |
|  [13]   | `Save(string / byte[], out uint / IOHandler)`                                              | instance | file/memory/handler egress |
|  [14]   | `ReadTag(TagSignature) / ReadTag<T> / WriteTag<T>(TagSignature, in T)`                     | instance | tag read/write             |
|  [15]   | `HasTag / LinkTag / TagLinkedTo / GetTag(uint) / TagCount`                                 | instance | tag presence and enumerate |
|  [16]   | `ColorSpace / PCS / DeviceClass / Version / EncodedICCVersion`                             | property | space and version header   |
|  [17]   | `HeaderRenderingIntent / HeaderFlags / HeaderManufacturer / HeaderModel / HeaderProfileID` | property | header metadata            |
|  [18]   | `IsIntentSupported(Intent, UsedDirection) / IsCLUT(...) / IsMatrixShaper`                  | instance | pipeline-form probe        |
|  [19]   | `DetectBlackPoint / DetectDestinationBlackPoint(out CIEXYZ, Intent, CmsFlags)`             | instance | black-point detection      |
|  [20]   | `TotalAreaCoverage / DetectRGBGamma(double) / ComputeMD5`                                  | instance | TAC, gamma, MD5 id         |
|  [21]   | `GetProfileInfo(InfoType, string, string) / GetProfileInfoASCII`                           | instance | localized profile info     |
|  [22]   | `GetPostScriptColorSpaceArray / GetPostScriptColorRenderingDictionary`                     | instance | PostScript emission        |

[TRANSFORM_BUILD_EXECUTE]: one polymorphic `Create` fold and buffer execution on `Transform`
- `Create` is one name discriminating on argument shape; K-preservation rides a `PreserveK*` `Intent` and gamut warning rides `CmsFlags.GamutCheck` with the alarm color set on the `Context`.

| [INDEX] | [SURFACE]                                                                                    | [SHAPE]  | [CAPABILITY]               |
| :-----: | :------------------------------------------------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `Create(Profile, uint, Profile, uint, Intent, CmsFlags)`                                     | factory  | two-profile conversion     |
|  [02]   | `Create(Profile, uint, Profile, uint, Profile, Intent, Intent, CmsFlags)`                    | factory  | three-profile soft-proof   |
|  [03]   | `Create(Profile[], uint, uint, Intent, CmsFlags)`                                            | factory  | multiprofile chain         |
|  [04]   | `Create(Context, Profile[], bool[], Intent[], double[], Profile, int, uint, uint, CmsFlags)` | factory  | extended per-link build    |
|  [05]   | `DoTransform(byte[], byte[], int)`                                                           | instance | packed array run           |
|  [06]   | `DoTransform(ReadOnlySpan<byte>, Span<byte>, int)`                                           | instance | zero-copy span run         |
|  [07]   | `DoTransform(..., int, int, int, int, int, int)`                                             | instance | strided raster run         |
|  [08]   | `ChangeBuffersFormat(uint, uint)`                                                            | instance | rebind pixel formats       |
|  [09]   | `InputFormat / OutputFormat / Flags / NamedColorList`                                        | property | policy inspection          |
|  [10]   | `SetUserData(IntPtr, FreeUserData)`                                                          | instance | attach data + release hook |

[CMS_HUB]: static pixel-format vocabulary and colorimetric helpers on `Cms`
- `Cms.TYPE_*` constants are the `uint` formats `DoTransform`/`Create` consume — channel order, bytes-per-channel, planar/float flags packed into one word.

[SCREEN_FORMATS]: `TYPE_RGBA_8` `TYPE_RGB_8` `TYPE_BGR_8` `TYPE_ABGR_8`
[PRINT_FORMATS]: `TYPE_CMYK_8` `TYPE_CMYK_16` `TYPE_CMYK_DBL`
[PRECISION_FORMATS]: `TYPE_GRAY_8` `TYPE_GRAY_16` `TYPE_GRAY_HALF_FLT` `TYPE_Lab_8` `TYPE_Lab_16` `TYPE_XYZ_16` `TYPE_XYZ_FLT` `TYPE_RGB_DBL`

| [INDEX] | [SURFACE]                                                                            | [SHAPE]  | [CAPABILITY]            |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `ToColorSpaceSignature(PixelType) / ToPixelType(ColorSpaceSignature)`                | static   | space <-> pixel mapping |
|  [02]   | `ChannelsOf(ColorSpaceSignature)`                                                    | static   | channel count           |
|  [03]   | `WhitePointFromTemp(out CIExyY, double) / TempFromWhitePoint(out double, in CIExyY)` | static   | white-point <-> temp    |
|  [04]   | `AlarmCodes / AdaptationState / SupportedIntents / EncodedCMMVersion`                | property | context governance      |
|  [05]   | `SetErrorHandler(ErrorHandler)`                                                      | static   | error routing           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Profiles are the transform operands; a `Transform` compiles a fixed pipeline from an input, output, and optional proofing `Profile`, and `DoTransform` runs pixel buffers through it. Soft-proofing, K-preservation, and gamut warning are `Intent`/`CmsFlags` selections on that one build, never separate factories.
- `Profile`, `Transform`, and `Context` are `CmsHandle<T> : IDisposable`; the teardown frees the native object, so an undisposed handle leaks native memory.

[STACKING]:
- `api-pdfsharp.md`: a color-managed `TYPE_CMYK_*` buffer feeds `XGraphics`/`XImage` as the pixel source for the vector PDF, and `PdfDocumentOptions.ColorMode` carries the CMYK intent PDFsharp does not compute — `lcmsNET` the color authority, PDFsharp the page authority.
- `api-avalonia-color.md` (`Wacton.Unicolour`): OKLCH owns screen-perceptual token color and `lcmsNET` owns profiled device-color egress; the two meet only at a resolved color, never overlapping.
- `api-drafting-export.md`: DXF/OOXML export codecs stay color-agnostic; the ICC transform applies to raster/print content before hand-off, never re-implemented inside a codec.
- within-lib: the drafting/export path composes `Profile.Open` operands, one `Transform.Create` keyed to the output profile, and `DoTransform` over the render raster.

[LOCAL_ADMISSION]:
- Native `lcms2` (`liblcms2`) provisions at the app-host distribution layer as the `FFmpeg`/`libmpv` natives do; this assembly binds it through P/Invoke and ships no native binary. A missing runtime surfaces through `Cms.SetErrorHandler` and the `LcmsNETException` rail, never a silent null transform.

[RAIL_LAW]:
- Package: `lcmsNET`
- Owns: ICC-managed device color for the print/export deliverable — profile lifecycle, the compiled `Transform`, the `Intent` vocabulary including K-preservation, `CmsFlags` policy, device-link generation, and the `Cms` pixel-format hub with colorimetric helpers.
- Accept: a rendered `TYPE_RGBA_8` raster converted to device `TYPE_CMYK_8`/`_16` through a `Transform.Create` keyed to the output profile; K-only/K-plane black generation via a `PreserveK*` `Intent`; print preview as a three-profile soft-proofing `Transform` (`SoftProofing`|`GamutCheck`, alarm on `Context`); fixed device pairings baked to `Profile.CreateDeviceLink`; ink limits via `CreateInkLimitingDeviceLink`; sheet TAC via `Profile.TotalAreaCoverage`.
- Reject: hand-rolled ICC matrix/CLUT math or bespoke CMYK separation where `Transform` compiles the managed pipeline; a `Get`/`GetProofing`/`GetMultiprofile` factory family where one `Transform.Create` discriminates by argument shape; `Wacton.Unicolour` OKLCH as an ICC device-color substitute; an undisposed `Profile`/`Transform`/`Context` handle past the `CmsHandle<T>` teardown.
