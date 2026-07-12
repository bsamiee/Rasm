# [RASM_APPUI_API_LCMSNET]

`lcmsNET` is the managed .NET binding to Little CMS 2 (`lcms2`) — the ICC / device-CMYK print-fidelity color-management owner. It carries the full lcms2 surface under one root namespace `lcmsNET`: `Profile` (the ICC profile handle — open/create/introspect/write tags), `Transform` (the compiled color-conversion pipeline — one polymorphic `Create` fold over device-to-device, soft-proofing, multiprofile, and gamut-checked builds), `Context` (the thread/plugin scope with alarm codes and adaptation state), the `Intent` rendering-intent vocabulary INCLUDING the black(K)-preservation intents, the `CmsFlags` transform policy, and the `Cms` static hub (pixel-format constants, colorimetric helpers, error routing). It is the print/proof counterpart to the screen-side `Wacton.Unicolour` OKLCH pipeline: `Unicolour` owns perceptual UI color, `lcmsNET` owns ICC-managed device color for the export deliverable. The native `lcms2` shared library is provisioned at the app-host distribution layer, never bundled with this assembly.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `lcmsNET`
- package: `lcmsNET`
- license: MIT (expression)
- assembly: `lcmsNET`
- namespace: `lcmsNET` (every public type — handles, enums, colorimetric structs, and the `Cms` static hub)
- target: `lib/net8.0` (bound by the `net10.0` consumer) + `lib/netstandard2.0`
- depends: `System.Memory` (rides the `netstandard2.0` asset; in-box on the `net8.0` floor)
- native: P/Invoke over the `lcms2` (Little CMS 2, `liblcms2`) shared library — the managed layer marshals; the native binary is external
- rail: color

## [02]-[PUBLIC_TYPES]

[PROFILE_TYPES]: ICC profile handle + colorimetric primitives — rail: color

Factories create the disposable `Profile` handle; it opens, saves, and addresses ICC tags. `Context.Create` and `Context.Duplicate` mint thread and plugin scopes, while `Cms.AlarmCodes` and `Cms.AdaptationState` govern the active context. `ToneCurve` builds gamma, parametric, and table transfer functions that evaluate, invert, and join. `Stage` admits matrix, CLUT, tone-curve, and identity operations into a `Pipeline`.

| [INDEX] | [SYMBOL]                | [KIND]                   |
| :-----: | :---------------------- | :----------------------- |
|  [01]   | `Profile`               | ICC profile handle       |
|  [02]   | `Context`               | thread/plugin scope      |
|  [03]   | `CIEXYZ`                | tristimulus values       |
|  [04]   | `CIExyY`                | chromaticity values      |
|  [05]   | `CIELab`                | Lab values               |
|  [06]   | `CIExyYTRIPLE`          | RGB primaries            |
|  [07]   | `ToneCurve`             | one-dimensional transfer |
|  [08]   | `Pipeline`              | LUT/CLUT pipeline        |
|  [09]   | `Stage`                 | pipeline operation       |
|  [10]   | `MultiLocalizedUnicode` | `mluc` text payload      |
|  [11]   | `NamedColorList`        | named-color payload      |

[TRANSFORM_TYPES]: compiled color-conversion pipeline — rail: color

`Transform` is the disposable `CmsHandle<Transform>` owner. Its `Create` overloads compile pipelines, `DoTransform` executes them, and `InputFormat`, `OutputFormat`, and `Flags` expose their policy.

| [INDEX] | [SYMBOL]       | [KIND]                     |
| :-----: | :------------- | :------------------------- |
|  [01]   | `Transform`    | compiled color transform   |
|  [02]   | `FreeUserData` | `SetUserData` release hook |

[ENUM_VOCABULARY]: bounded color-management vocabularies — rail: color

| [INDEX] | [SYMBOL]                 | [KIND]                |
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
- Standard: `Perceptual`, `RelativeColorimetric`, `Saturation`, `AbsoluteColorimetric`
- K-only preservation: `PreserveKOnlyPerceptual`, `PreserveKOnlyRelativeColorimetric`, `PreserveKOnlySaturation`
- K-plane preservation: `PreserveKPlanePerceptual`, `PreserveKPlaneRelativeColorimetric`, `PreserveKPlaneSaturation`

[CMS_FLAG_MEMBERS]:
- Conversion: `BlackPointCompensation`, `SoftProofing`, `GamutCheck`, `NullTransform`, `NoNegatives`, `NoWhiteOnWhiteFixUp`
- Compilation: `NoCache`, `NoOptimize`, `HighResPreCalc`, `LowResPreCalc`, `ForceCLut`, `CLutPreLinearization`, `CLutPostLinearization`
- Device link: `KeepSequence`, `EightBitsDeviceLink`, `GuessDeviceClass`, `NoDefaultResourceDef`

[PIXEL_TYPE_MEMBERS]: `Gray`, `RGB`, `CMY`, `CMYK`, `YCbCr`, `XYZ`, `Lab`, `Yxy`, `HSV`, `HLS`, `MCH1` through `MCH15`, `LabV2`, `Any`

[SIGNATURE_MEMBERS]:
- `ColorSpaceSignature`: `XYZData`, `LabData`, `RgbData`, `GrayData`, `CmykData`, and the remaining ICC color-space signatures
- `ProfileClassSignature`: `Input`, `Display`, `Output`, `Link`, `Abstract`, `ColorSpace`, `NamedColor`
- `TagSignature`: the ICC tag vocabulary, including `AToB0`, `BToA0`, `Gamut`, `BlueColorant`, `BlueTRC`, and `Copyright`

[TAG_PAYLOAD_TYPES]: strongly-typed ICC tag payloads + measurement helpers — rail: color

| [INDEX] | [SYMBOL]                    | [KIND]                    |
| :-----: | :-------------------------- | :------------------------ |
|  [01]   | `ICCData`                   | raw-data tag              |
|  [02]   | `UcrBg`                     | UCR/BG tag                |
|  [03]   | `VideoCardGamma`            | VCGT tag                  |
|  [04]   | `Screening`                 | halftone-screening tag    |
|  [05]   | `ColorantOrder`             | colorant-order tag        |
|  [06]   | `ProfileSequenceDescriptor` | device-link provenance    |
|  [07]   | `ProfileSequenceItem`       | provenance entry          |
|  [08]   | `GamutBoundaryDescriptor`   | gamut-boundary handle     |
|  [09]   | `Dict`                      | `meta` dictionary         |
|  [10]   | `DictEntry`                 | metadata entry            |
|  [11]   | `IT8`                       | IT8.7/CGATS table         |
|  [12]   | `DeltaE`                    | color-difference metrics  |
|  [13]   | `CAM02`                     | CIECAM02 appearance model |
|  [14]   | `IOHandler`                 | profile I/O sink          |
|  [15]   | `LcmsNETException`          | typed lcms2 failure       |

`UcrBg` carries under-color removal and black generation. `IT8` reads and writes measurement tables, while `IOHandler` binds in-memory and stream profile I/O.

## [03]-[ENTRYPOINTS]

[PROFILE_LIFECYCLE]: create / open / save / introspect on `Profile`
- rail: color
- Profiles are the transform operands: the input device profile, the output (print) profile, and the proofing (target-device) profile are each a `Profile`, opened from disk/memory or synthesized from primitives.

Every lifecycle group is rooted on `Profile` and rides the color rail.

| [INDEX] | [GROUP]         | [PURPOSE]             |
| :-----: | :-------------- | :-------------------- |
|  [01]   | open            | profile ingress       |
|  [02]   | working space   | profile synthesis     |
|  [03]   | device link     | conversion baking     |
|  [04]   | special profile | adjustment/null shell |
|  [05]   | save            | profile egress        |
|  [06]   | tags            | tag lifecycle         |
|  [07]   | header          | profile metadata      |
|  [08]   | analysis        | profile diagnostics   |
|  [09]   | description     | descriptive emission  |

[OPEN]:
- File: `Open(string filepath, string access)`
- Memory: `Open(byte[] memory)`
- Handler: `Open(Context, IOHandler, bool writeable)`

[WORKING_SPACE]:
- Built-ins: `Create_sRGB(Context?)`, `CreateXYZ`, `CreateLab2`, `CreateLab4(in CIExyY)`, `Create_OkLab`
- RGB: `CreateRGB(in CIExyY, in CIExyYTRIPLE, ToneCurve[])`
- Gray: `CreateGray(in CIExyY, ToneCurve)`

[DEVICE_LINK]:
- Ink limit: `CreateInkLimitingDeviceLink(ColorSpaceSignature, double limit)`
- Linearization: `CreateLinearizationDeviceLink(ColorSpaceSignature, ToneCurve[])`
- Transform: `CreateDeviceLink(Transform, double version, CmsFlags)`
- Cube: `CreateDeviceLinkFromCubeFile(string)`

[SPECIAL_PROFILE]:
- Adjustment: `CreateBCHSWabstract(int nLutPoints, double bright, double contrast, double hue, double saturation, int tempSrc, int tempDest)`
- Shells: `CreateNull`, `CreatePlaceholder`

[SAVE]:
- File: `Save(string filepath)`
- Memory: `Save(byte[] memory, out uint bytesNeeded)`
- Handler: `Save(IOHandler)`

[TAGS]:
- Read: `ReadTag(TagSignature)`, `ReadTag<T>`
- Write: `WriteTag<T>(TagSignature, in T)`, `WriteTag(TagSignature, ICCData/UcrBg/VideoCardGamma)`
- Link: `HasTag`, `LinkTag`, `TagLinkedTo`
- Enumerate: `GetTag(uint n)`, `TagCount`

[HEADER]:
- Space: `ColorSpace`, `PCS`, `DeviceClass`
- Version: `Version`, `EncodedICCVersion`
- Metadata: `HeaderRenderingIntent`, `HeaderFlags`, `HeaderManufacturer`, `HeaderModel`, `HeaderProfileID`

[ANALYSIS]:
- Capability: `IsIntentSupported(Intent, UsedDirection)`, `IsCLUT(Intent, UsedDirection)`, `IsMatrixShaper`
- Black point: `DetectBlackPoint(out CIEXYZ, Intent, CmsFlags)`, `DetectDestinationBlackPoint(out CIEXYZ, Intent, CmsFlags)`
- Device: `TotalAreaCoverage`, `DetectRGBGamma(double)`, `ComputeMD5`

[DESCRIPTION]:
- Profile: `GetProfileInfo(InfoType, lang, country)`, `GetProfileInfoASCII(...)`
- PostScript: `GetPostScriptColorSpaceArray(...)`, `GetPostScriptColorRenderingDictionary(...)`

[TRANSFORM_BUILD_EXECUTE]: one polymorphic `Create` fold + buffer execution on `Transform`
- rail: color
- `Create` is ONE name discriminating on argument shape: a two-profile device conversion, a three-profile SOFT-PROOFING build (proofing profile + separate `proofingIntent`), an N-profile multiprofile chain, or the fully-parameterized extended build (per-link BPC, per-link intents, per-link adaptation states, and an inserted gamut profile). K-preservation is selected by passing a `PreserveK*` `Intent`; gamut warning by `CmsFlags.GamutCheck` with the alarm color set on the `Context`.

Every build and execution group is rooted on `Transform` and rides the color rail.

| [INDEX] | [GROUP]       | [PURPOSE]          |
| :-----: | :------------ | :----------------- |
|  [01]   | device        | profile conversion |
|  [02]   | proofing      | gamut preview      |
|  [03]   | multiprofile  | profile chain      |
|  [04]   | extended      | per-link policy    |
|  [05]   | packed        | packed-buffer run  |
|  [06]   | raster        | strided raster run |
|  [07]   | configuration | runtime inspection |

[DEVICE_CREATE]:
- Surface: `Create(Profile input, uint inputFormat, Profile output, uint outputFormat, Intent, CmsFlags)`
- Variant: the `Context` overload applies the same device conversion, such as `TYPE_RGBA_8` to `TYPE_CMYK_8`.

[PROOFING_CREATE]:
- Surface: `Create(Profile input, uint inputFormat, Profile output, uint outputFormat, Profile proofing, Intent, Intent proofingIntent, CmsFlags)`
- Variant: the `Context` overload binds `CmsFlags.SoftProofing | CmsFlags.GamutCheck`.

[MULTIPROFILE_CREATE]:
- Surface: `Create(Profile[] profiles, uint inputFormat, uint outputFormat, Intent, CmsFlags)`
- Variant: the `Context` overload compiles a working-to-link-to-device chain.

[EXTENDED_CREATE]:
- Surface: `Create(Context, Profile[] profiles, bool[] bpc, Intent[] intents, double[] adaptationStates, Profile gamut, int gamutPCSPosition, uint inputFormat, uint outputFormat, CmsFlags)`
- Policy: per-link BPC, intent, adaptation state, and an inserted gamut check

[PACKED_EXECUTION]:
- Array: `DoTransform(byte[] in, byte[] out, int pixelCount)`
- Span: `DoTransform(ReadOnlySpan<byte> in, Span<byte> out, int pixelCount)`

[RASTER_EXECUTION]:
- Surface: `DoTransform(in, out, int pixelsPerLine, int lineCount, int bytesPerLineIn, int bytesPerLineOut, int bytesPerPlaneIn, int bytesPerPlaneOut)`
- Carriers: byte arrays and spans

[TRANSFORM_CONFIGURATION]:
- Rebind: `ChangeBuffersFormat(uint inputFormat, uint outputFormat)`
- Inspect: `InputFormat`, `OutputFormat`, `Flags`, `NamedColorList`
- User data: `SetUserData(IntPtr, FreeUserData)`

[CMS_HUB]: static format vocabulary + colorimetric helpers on `Cms`
- rail: color
- The `uint` pixel formats `DoTransform`/`Create` consume are the `Cms.TYPE_*` constants: channel order, bytes-per-channel, and planar/float flags packed into one word.

Every hub group is rooted on `Cms` and rides the color rail.

| [INDEX] | [GROUP]     | [PURPOSE]           |
| :-----: | :---------- | :------------------ |
|  [01]   | screen      | interleaved ingress |
|  [02]   | print       | CMYK egress         |
|  [03]   | precision   | analytic formats    |
|  [04]   | space       | channel derivation  |
|  [05]   | temperature | white-point mapping |
|  [06]   | policy      | context governance  |

[SCREEN_FORMATS]: `TYPE_RGBA_8`, `TYPE_RGB_8`, `TYPE_BGR_8`, `TYPE_ABGR_8`

[PRINT_FORMATS]: `TYPE_CMYK_8`, `TYPE_CMYK_16`, `TYPE_CMYK_DBL`

[PRECISION_FORMATS]: `TYPE_GRAY_8`, `TYPE_GRAY_16`, `TYPE_GRAY_HALF_FLT`, `TYPE_Lab_8`, `TYPE_Lab_16`, `TYPE_XYZ_16`, `TYPE_XYZ_FLT`, `TYPE_RGB_DBL`

[SPACE_DERIVATION]: `ToColorSpaceSignature(PixelType)`, `ToPixelType(ColorSpaceSignature)`, `ChannelsOf(ColorSpaceSignature)`

[WHITE_POINT_MAPPING]: `WhitePointFromTemp(out CIExyY, double tempK)`, `TempFromWhitePoint(out double, in CIExyY)`

[CONTEXT_POLICY]: `AlarmCodes`, `AdaptationState`, `SupportedIntents`, `EncodedCMMVersion`, `SetErrorHandler(ErrorHandler)`

## [04]-[IMPLEMENTATION_LAW]

[COLOR_LAW]:
- Package: `lcmsNET`
- Owns: ICC-managed device color for the print/export deliverable — profile lifecycle (`Profile.Open`/`Create*`/`Save`/tag read-write), the compiled `Transform` (the one polymorphic `Create` fold + `DoTransform`), the `Intent` vocabulary (including K-preservation `PreserveKOnly*`/`PreserveKPlane*`), `CmsFlags` policy (`SoftProofing`/`GamutCheck`/`BlackPointCompensation`), device-link generation (ink-limiting / linearization / baked-transform / .cube), and the `Cms` pixel-format vocabulary + colorimetric helpers.
- Accept: the drafting/export color-fidelity path converts a rendered `TYPE_RGBA_8` raster to a device-`TYPE_CMYK_8`/`_16` buffer through a `Transform.Create(input, output, Intent, CmsFlags)` keyed to the target output profile; K-only/K-plane black generation is selected by a `PreserveK*` `Intent`; on-screen print preview is a three-profile soft-proofing `Transform` (`SoftProofing`|`GamutCheck`, alarm color on the `Context`); a fixed device pairing bakes to a `Profile.CreateDeviceLink`; total-ink limits apply through `CreateInkLimitingDeviceLink`; sheet TAC is verified through `Profile.TotalAreaCoverage`.
- Reject: hand-rolling ICC matrix/CLUT math or a bespoke CMYK separation where `Transform` compiles the managed pipeline; a `Get`/`GetProofing`/`GetMultiprofile` transform-factory family where `Transform.Create` discriminates by argument shape; treating `Wacton.Unicolour` OKLCH (screen-perceptual) as an ICC device-color substitute — `Unicolour` owns UI token color, `lcmsNET` owns profiled print color; leaking an undisposed `Profile`/`Transform`/`Context` handle where the `CmsHandle<T> : IDisposable` teardown frees the native object.

[STACKING]:
- Stacks with `api-pdfsharp.md`: the color-managed `TYPE_CMYK_*` buffer is the pixel source drawn through `XGraphics`/`XImage` into the vector-PDF deliverable, and `PdfDocumentOptions.ColorMode` carries the CMYK intent PDFsharp's own model does not compute; `lcmsNET` is the color authority, PDFsharp the page authority.
- Stacks with `api-drafting-export.md`: the DXF/OOXML export codecs stay color-agnostic; the ICC transform is applied to raster/print content before hand-off, never re-implemented inside an export codec.
- Complements `api-avalonia-color.md` / `Wacton.Unicolour`: OKLCH owns the screen `ControlIntent`/`Theme/tokens` pipeline; `lcmsNET` owns the profiled device-color egress. The two never overlap — screen-perceptual versus ICC-device is the seam.

> [!IMPORTANT]
> The native `lcms2` (Little CMS 2, `liblcms2` ≥ 2.16) shared library is provisioned at the app-host distribution layer (`brew install little-cms2`, `apt install liblcms2-2`, or a side-loaded binary) exactly as the `FFmpeg`/`libmpv` natives are. This managed assembly binds `lcms2` through P/Invoke at load time and ships no native binary; a missing `lcms2` runtime is a host-provisioning fault, surfaced through `Cms.SetErrorHandler` and the `LcmsNETException` rail, never a silent null transform.
