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

| [INDEX] | [SYMBOL]                                            | [KIND]                                                                       |
| :-----: | :-------------------------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Profile`                                           | the ICC profile handle (`CmsHandle<Profile>`, `IDisposable`) — factory-created, tag-addressable, saveable |
|  [02]   | `Context`                                           | thread/plugin scope — `Create`/`Duplicate`, plugin registration, `AlarmCodes`/`AdaptationState` on the owning `Cms` hub |
|  [03]   | `CIEXYZ` / `CIExyY` / `CIELab` / `CIExyYTRIPLE`     | tristimulus / chromaticity / Lab / RGB-primaries colorimetric structs (`Colorimetric.cs`) |
|  [04]   | `ToneCurve`                                         | 1D transfer function — gamma/parametric/table build, evaluate, invert, join  |
|  [05]   | `Pipeline` / `Stage`                                | the multi-stage LUT/CLUT pipeline and its stages (matrix / CLUT / tone-curve / identity) |
|  [06]   | `MultiLocalizedUnicode`                             | multi-localized text tag payload (`mluc`) for profile descriptions           |
|  [07]   | `NamedColorList`                                    | spot/named-color tag payload                                                 |

[TRANSFORM_TYPES]: compiled color-conversion pipeline — rail: color

| [INDEX] | [SYMBOL]                                            | [KIND]                                                                       |
| :-----: | :-------------------------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Transform`                                         | the compiled transform (`CmsHandle<Transform>`, `IDisposable`) — one `Create` fold, `DoTransform` execution, `InputFormat`/`OutputFormat`/`Flags` |
|  [02]   | `FreeUserData`                                      | user-data release delegate for `SetUserData`                                 |

[ENUM_VOCABULARY]: bounded color-management vocabularies — rail: color

| [INDEX] | [SYMBOL]                                            | [KIND]                                                                       |
| :-----: | :-------------------------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Intent`                                            | rendering intent — `Perceptual`/`RelativeColorimetric`/`Saturation`/`AbsoluteColorimetric` PLUS K-preservation `PreserveKOnly{Perceptual,RelativeColorimetric,Saturation}` and `PreserveKPlane{Perceptual,RelativeColorimetric,Saturation}` |
|  [02]   | `CmsFlags`                                          | transform policy flags — `BlackPointCompensation`, `SoftProofing`, `GamutCheck`, `NoCache`, `NoOptimize`, `NullTransform`, `HighResPreCalc`/`LowResPreCalc`, `NoWhiteOnWhiteFixUp`, `KeepSequence`, `ForceCLut`, `CLut{Pre,Post}Linearization`, `EightBitsDeviceLink`, `GuessDeviceClass`, `NoNegatives`, `NoDefaultResourceDef` |
|  [03]   | `PixelType`                                         | logical channel model — `Gray`/`RGB`/`CMY`/`CMYK`/`YCbCr`/`XYZ`/`Lab`/`Yxy`/`HSV`/`HLS`/`MCH1`..`MCH15`/`LabV2`/`Any` |
|  [04]   | `ColorSpaceSignature`                               | ICC color-space signature — `XYZData`/`LabData`/`RgbData`/`GrayData`/`CmykData`/… |
|  [05]   | `ProfileClassSignature`                             | profile device class — `Input`/`Display`/`Output`/`Link`/`Abstract`/`ColorSpace`/`NamedColor` |
|  [06]   | `TagSignature`                                      | ICC tag identity (80+ members — `AToB0`/`BToA0`/`Gamut`/`BlueColorant`/`BlueTRC`/`Copyright`/…) |
|  [07]   | `InfoType` / `UsedDirection` / `PostScriptResourceType` | profile-info selector / intent-direction selector / PostScript resource kind |

[TAG_PAYLOAD_TYPES]: strongly-typed ICC tag payloads + measurement helpers — rail: color

| [INDEX] | [SYMBOL]                                            | [KIND]                                                                       |
| :-----: | :-------------------------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `ICCData` / `UcrBg` / `VideoCardGamma` / `Screening` / `ColorantOrder` | raw-data / under-color-removal+black-generation / VCGT / halftone-screening / colorant-order tag payloads |
|  [02]   | `ProfileSequenceDescriptor` / `ProfileSequenceItem` | device-link provenance chain                                                 |
|  [03]   | `GamutBoundaryDescriptor`                           | gamut-boundary (GBD) computation handle                                      |
|  [04]   | `Dict` / `DictEntry`                                | metadata dictionary tag (`meta`)                                             |
|  [05]   | `IT8`                                               | IT8.7/CGATS measurement-table reader/writer                                  |
|  [06]   | `DeltaE` / `CAM02`                                  | ΔE color-difference metrics and the CIECAM02 appearance model                |
|  [07]   | `IOHandler`                                         | abstract I/O sink for in-memory / stream profile read and write             |
|  [08]   | `LcmsNETException`                                  | typed failure rail over lcms2 error codes                                    |

## [03]-[ENTRYPOINTS]

[PROFILE_LIFECYCLE]: create / open / save / introspect on `Profile`
- rail: color
- Profiles are the transform operands: the input device profile, the output (print) profile, and the proofing (target-device) profile are each a `Profile`, opened from disk/memory or synthesized from primitives.

| [INDEX] | [SURFACE]                                                                 | [SURFACE_ROOT] | [RAIL] |
| :-----: | :------------------------------------------------------------------------ | :------------- | :----- |
|  [01]   | `Open(string filepath, string access)` / `Open(byte[] memory)` / `Open(Context, IOHandler, bool writeable)` | `Profile` | open an existing ICC profile (file / memory / handler) |
|  [02]   | `Create_sRGB(Context?)` / `CreateRGB(in CIExyY, in CIExyYTRIPLE, ToneCurve[])` / `CreateGray(in CIExyY, ToneCurve)` / `CreateXYZ` / `CreateLab2`/`CreateLab4(in CIExyY)` / `Create_OkLab` | `Profile` | synthesize a working-space profile |
|  [03]   | `CreateInkLimitingDeviceLink(ColorSpaceSignature, double limit)` / `CreateLinearizationDeviceLink(ColorSpaceSignature, ToneCurve[])` / `CreateDeviceLink(Transform, double version, CmsFlags)` / `CreateDeviceLinkFromCubeFile(string)` | `Profile` | build a device-link (ink-limit / linearization / baked-transform / .cube) |
|  [04]   | `CreateBCHSWabstract(int nLutPoints, double bright, double contrast, double hue, double saturation, int tempSrc, int tempDest)` / `CreateNull` / `CreatePlaceholder` | `Profile` | abstract adjustment / null / empty-placeholder profile |
|  [05]   | `Save(string filepath)` / `Save(byte[] memory, out uint bytesNeeded)` / `Save(IOHandler)` | `Profile` | persist the profile (file / memory / handler) |
|  [06]   | `ReadTag(TagSignature)` / `ReadTag<T>` / `WriteTag<T>(TagSignature, in T)` / `WriteTag(TagSignature, ICCData/UcrBg/VideoCardGamma)` / `HasTag` / `LinkTag` / `TagLinkedTo` / `GetTag(uint n)` / `TagCount` | `Profile` | tag read/write/link/enumerate |
|  [07]   | `ColorSpace` / `PCS` / `DeviceClass` / `Version` / `EncodedICCVersion` / `HeaderRenderingIntent` / `HeaderFlags` / `HeaderManufacturer` / `HeaderModel` / `HeaderProfileID` | `Profile` | header/space accessors |
|  [08]   | `IsIntentSupported(Intent, UsedDirection)` / `IsCLUT(Intent, UsedDirection)` / `IsMatrixShaper` / `DetectBlackPoint(out CIEXYZ, Intent, CmsFlags)` / `DetectDestinationBlackPoint(out CIEXYZ, Intent, CmsFlags)` / `TotalAreaCoverage` / `DetectRGBGamma(double)` / `ComputeMD5` | `Profile` | capability probe + black-point / TAC / gamma detection |
|  [09]   | `GetProfileInfo(InfoType, lang, country)` / `GetProfileInfoASCII(...)` / `GetPostScriptColorSpaceArray(...)` / `GetPostScriptColorRenderingDictionary(...)` | `Profile` | localized description + PostScript CSA/CRD emission |

[TRANSFORM_BUILD_EXECUTE]: one polymorphic `Create` fold + buffer execution on `Transform`
- rail: color
- `Create` is ONE name discriminating on argument shape: a two-profile device conversion, a three-profile SOFT-PROOFING build (proofing profile + separate `proofingIntent`), an N-profile multiprofile chain, or the fully-parameterized extended build (per-link BPC, per-link intents, per-link adaptation states, and an inserted gamut profile). K-preservation is selected by passing a `PreserveK*` `Intent`; gamut warning by `CmsFlags.GamutCheck` with the alarm color set on the `Context`.

| [INDEX] | [SURFACE]                                                                 | [SURFACE_ROOT] | [RAIL] |
| :-----: | :------------------------------------------------------------------------ | :------------- | :----- |
|  [01]   | `Create(Profile input, uint inputFormat, Profile output, uint outputFormat, Intent, CmsFlags)` (+ `Context` overload) | `Transform` | device-to-device conversion (e.g. `TYPE_RGBA_8` → `TYPE_CMYK_8`) |
|  [02]   | `Create(Profile input, uint inputFormat, Profile output, uint outputFormat, Profile proofing, Intent, Intent proofingIntent, CmsFlags)` (+ `Context` overload) | `Transform` | soft-proofing transform (`CmsFlags.SoftProofing`\|`GamutCheck`) |
|  [03]   | `Create(Profile[] profiles, uint inputFormat, uint outputFormat, Intent, CmsFlags)` (+ `Context` overload) | `Transform` | multiprofile chain (working → link → device) |
|  [04]   | `Create(Context, Profile[] profiles, bool[] bpc, Intent[] intents, double[] adaptationStates, Profile gamut, int gamutPCSPosition, uint inputFormat, uint outputFormat, CmsFlags)` | `Transform` | extended per-link build with inserted gamut check |
|  [05]   | `DoTransform(byte[] in, byte[] out, int pixelCount)` / `DoTransform(ReadOnlySpan<byte> in, Span<byte> out, int pixelCount)` | `Transform` | convert a packed pixel buffer |
|  [06]   | `DoTransform(in, out, int pixelsPerLine, int lineCount, int bytesPerLineIn, int bytesPerLineOut, int bytesPerPlaneIn, int bytesPerPlaneOut)` (byte[] + span forms) | `Transform` | strided/planar per-line conversion for a full raster |
|  [07]   | `ChangeBuffersFormat(uint inputFormat, uint outputFormat)` / `InputFormat` / `OutputFormat` / `Flags` / `NamedColorList` / `SetUserData(IntPtr, FreeUserData)` | `Transform` | in-place format re-bind + introspection |

[CMS_HUB]: static format vocabulary + colorimetric helpers on `Cms`
- rail: color
- The `uint` pixel formats `DoTransform`/`Create` consume are the `Cms.TYPE_*` constants: channel order, bytes-per-channel, and planar/float flags packed into one word.

| [INDEX] | [SURFACE]                                                                 | [SURFACE_ROOT] | [RAIL] |
| :-----: | :------------------------------------------------------------------------ | :------------- | :----- |
|  [01]   | `TYPE_RGBA_8` / `TYPE_RGB_8` / `TYPE_BGR_8` / `TYPE_ABGR_8`                | `Cms`          | 8-bit screen/interleaved formats (encode ingress) |
|  [02]   | `TYPE_CMYK_8` / `TYPE_CMYK_16` / `TYPE_CMYK_DBL`                           | `Cms`          | device-CMYK print formats (transform egress) |
|  [03]   | `TYPE_GRAY_8` / `TYPE_GRAY_16` / `TYPE_GRAY_HALF_FLT` / `TYPE_Lab_8` / `TYPE_Lab_16` / `TYPE_XYZ_16` / `TYPE_XYZ_FLT` / `TYPE_RGB_DBL` | `Cms` | gray / Lab / XYZ / high-precision formats |
|  [04]   | `ToColorSpaceSignature(PixelType)` / `ToPixelType(ColorSpaceSignature)` / `ChannelsOf(ColorSpaceSignature)` | `Cms` | space/channel derivation |
|  [05]   | `WhitePointFromTemp(out CIExyY, double tempK)` / `TempFromWhitePoint(out double, in CIExyY)` | `Cms` | correlated-color-temperature ↔ chromaticity |
|  [06]   | `AlarmCodes` / `AdaptationState` / `SupportedIntents` / `EncodedCMMVersion` / `SetErrorHandler(ErrorHandler)` | `Cms` | gamut-alarm color, adaptation policy, intent enumeration, CMM version, error routing |

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
