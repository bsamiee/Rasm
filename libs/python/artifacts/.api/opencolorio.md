# [PY_ARTIFACTS_API_OPENCOLORIO]

`opencolorio` owns the CONFIG-DRIVEN color pipeline — the studio-grade transform graph the estate resolves a working space, a display view, and a look through. Configs name colorspaces, roles, displays, views, looks, named transforms, and file rules; asking it for a transform between two of them yields a `Processor`, which compiles to a `CPUProcessor` applying in place over `numpy` buffers or to a `GPUProcessor` emitting GLSL, MSL, HLSL, or OSL shader text. ACES CG and Studio configs ship inside the distribution, so an ACEScg scene-linear working space and its display renderings resolve with no config file on disk.

Boundaries against the estate's other color owners run on AUTHORITY, not on the math: `colour-science` owns colorimetry, appearance models, and spectral computation; `coloraide` owns CSS-space parsing and gamut mapping; the `imagecodecs` `cms_*` arms own the ICC-profile edge where a file carries an embedded profile. This owner holds the config — the versioned, shareable declaration of what a project's colorspaces MEAN — and every scene-linear working-space decision resolves through its `scene_linear` role rather than a synthesized profile or a hardcoded matrix.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `opencolorio`
- package: `opencolorio` (BSD-3-Clause, ASWF)
- module: `PyOpenColorIO`
- asset: sdist built at the Forge floor; `yaml-cpp`, `pystring`, `Imath`, `minizip-ng`, `expat`, and `OpenEXR` build in-tree through the project's own `ExternalProject` and only `lcms2` resolves from the Forge search path. This build carries `CMAKE_ARGS = -DOCIO_BUILD_APPS=OFF` from `[tool.uv.extra-build-variables]`, because the `oglapphelpers` ObjC++ Metal target cannot compile against the macOS SDK under the project's own `-Werror,-Wunguarded-availability-new`
- rail: color (config-driven transform graph and its CPU and GPU processors)
- target: `numpy` float32 buffers in place, or an `ImageDesc` view over strided memory
- capability: eight built-in ACES CG and Studio configs, the colorspace/role/display/view/look/named-transform graph, 98 builtin transforms, `Processor` optimization levels, in-place CPU application, shader emission for six GPU languages, LUT baking to every supported format, and `.ocioz` archive extraction

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the config graph and its compiled processors
- rail: color

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]      | [CAPABILITY]                                               |
| :-----: | :-------------------------------------- | :----------------- | :--------------------------------------------------------- |
|  [01]   | `Config`                                | graph root         | colorspaces, roles, displays, views, looks, file rules     |
|  [02]   | `BuiltinConfigRegistry`                 | shipped configs    | the eight config names, reachable as `ocio://<name>`       |
|  [03]   | `BuiltinTransformRegistry`              | shipped transforms | 98 named transforms usable with no config file             |
|  [04]   | `ColorSpace`                            | node               | family, encoding, bit depth, allocation, aliases, `isData` |
|  [05]   | `Processor`                             | compiled graph     | the transform between two ends; CPU and GPU compilation    |
|  [06]   | `CPUProcessor`                          | executor           | `apply`/`applyRGB`/`applyRGBA`; `isNoOp`, crosstalk probe  |
|  [07]   | `GPUProcessor` `GpuShaderDesc`          | executor           | `extractGpuShaderInfo(desc)` fills text, uniforms, LUTs    |
|  [08]   | `PackedImageDesc` `PlanarImageDesc`     | buffer view        | interleaved or planar memory with explicit strides         |
|  [09]   | `Baker`                                 | LUT egress         | bake an input-to-target or display/view chain to a LUT     |
|  [10]   | `Context` `FileRules` `ViewingRules`    | resolution policy  | environment variables, path rules, view filtering          |
|  [11]   | `Look` `NamedTransform` `ViewTransform` | graph members      | creative looks, standalone transforms, view transforms     |
|  [12]   | `Exception` `ExceptionMissingFile`      | fault              | every config, resolution, and transform fault              |

[PUBLIC_TYPE_SCOPE]: the transform families a config composes
- rail: color

| [INDEX] | [FAMILY]                                                           | [CAPABILITY]                                              |
| :-----: | :----------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `ColorSpaceTransform` `DisplayViewTransform` `LookTransform`       | graph-level moves between named config members            |
|  [02]   | `MatrixTransform` `RangeTransform` `ExponentTransform`             | primitive linear, clamp, and power operations             |
|  [03]   | `LogTransform` `LogAffineTransform` `LogCameraTransform`           | log encodings including camera-vendor toe handling        |
|  [04]   | `Lut1DTransform` `Lut3DTransform` `FileTransform`                  | inline LUTs and every format `getFormats()` lists         |
|  [05]   | `FixedFunctionTransform`                                           | ACES gamut-compress, glow, red-modifier, output transform |
|  [06]   | `CDLTransform` `ExposureContrastTransform`                         | ASC CDL grading and dynamic exposure and contrast         |
|  [07]   | `GradingPrimaryTransform` and its tone, RGB-curve, hue-curve peers | interactive grading driven by dynamic properties          |
|  [08]   | `GroupTransform` `BuiltinTransform` `AllocationTransform`          | composition, the shipped roster, GPU allocation hints     |

[PUBLIC_TYPE_SCOPE]: the closed policy vocabularies
- rail: color

| [INDEX] | [ENUM]                | [ROWS]                                                                     |
| :-----: | :-------------------- | :------------------------------------------------------------------------- |
|  [01]   | `BitDepth`            | `UINT8` `UINT10` `UINT12` `UINT14` `UINT16` `UINT32` `F16` `F32` `UNKNOWN` |
|  [02]   | `TransformDirection`  | `FORWARD` `INVERSE`, with the inverse and combine free functions           |
|  [03]   | `Interpolation`       | LUT interpolation: nearest, linear, tetrahedral, cubic, best, default      |
|  [04]   | `OptimizationFlags`   | `NONE` `LOSSLESS` `VERY_GOOD` `GOOD` `DRAFT` `ALL`, plus per-op bits       |
|  [05]   | `GpuLanguage`         | GLSL 1.2 through VK 4.6, `HLSL_DX11`, `MSL_2_0`, `OSL_1`, `CG`             |
|  [06]   | `ChannelOrdering`     | `RGB` `RGBA` `BGR` `BGRA` `ABGR` — the `PackedImageDesc` layout            |
|  [07]   | `Allocation`          | `UNIFORM` `LG2` `UNKNOWN` — the GPU allocation hint on a colorspace        |
|  [08]   | `ReferenceSpaceType`  | `SCENE` `DISPLAY` — which reference a colorspace connects to               |
|  [09]   | `DynamicPropertyType` | exposure, contrast, gamma, and the four grading property rows              |
|  [10]   | `LoggingLevel`        | the module logging level `SetLoggingLevel` drives                          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: config resolution
- rail: color
- [SHAPE]: static factories on `Config`, beside module-level process state

| [INDEX] | [SURFACE]                                            | [CAPABILITY]                                                        |
| :-----: | :--------------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | `Config.CreateFromBuiltinConfig(name) -> Config`     | a shipped config; `"ocio://default"` resolves the current CG config |
|  [02]   | `Config.CreateFromFile(path)`                        | a project `.ocio` config on disk                                    |
|  [03]   | `Config.CreateFromStream(str)` and `CreateFromEnv()` | an in-memory config, or the one `$OCIO` names                       |
|  [04]   | `Config.CreateRaw()`                                 | the minimal config carrying a single raw data colorspace            |
|  [05]   | `Config.CreateFromConfigIOProxy(proxy)`              | a config whose files resolve through a caller-owned proxy           |
|  [06]   | `BuiltinConfigRegistry().getBuiltinConfigs()`        | name, UI name, description, and preference flag per config          |
|  [07]   | `GetCurrentConfig()` and `SetCurrentConfig(config)`  | the process-wide config; a library tier never sets it               |
|  [08]   | `ExtractOCIOZArchive(archive, dest) -> None`         | unpack an `.ocioz` archive to a directory                           |
|  [09]   | `ClearAllCaches()` and `SetComputeHashFunction(fn)`  | process cache and file-hash policy                                  |

[ENTRYPOINT_SCOPE]: graph interrogation
- rail: color
- [SHAPE]: instance (`Config`)

| [INDEX] | [SURFACE]                                               | [CAPABILITY]                                                      |
| :-----: | :------------------------------------------------------ | :---------------------------------------------------------------- |
|  [01]   | `getRoles()`                                            | `(role, colorspace)` pairs; `scene_linear` owns the working space |
|  [02]   | `getColorSpaces()` and `getColorSpaceNames()`           | the colorspace roster; `getColorSpace(name)` resolves one         |
|  [03]   | `getDisplays()` and `getViews(display)`                 | display and view rosters, with the default-display accessors      |
|  [04]   | `getColorSpaceFromFilepath(path) -> tuple`              | the file-rules answer: colorspace plus rule index                 |
|  [05]   | `getDisplayViewColorSpaceName(display, view)`           | the colorspace a view lands in                                    |
|  [06]   | `getCanonicalName(name)` and `getInactiveColorSpaces()` | alias resolution and the hidden-space roster                      |
|  [07]   | `getLook(name)` and `getNamedTransform(name)`           | creative look and standalone-transform lookup                     |
|  [08]   | `getCurrentContext()` and `getEnvironmentVarNames()`    | the context whose variables expand inside file paths              |

[ENTRYPOINT_SCOPE]: processor compilation and application
- rail: color
- [SHAPE]: instance (`Config` to `Processor` to `CPUProcessor` or `GPUProcessor`)

| [INDEX] | [SURFACE]                                                        | [CAPABILITY]                                                   |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `Config.getProcessor(src, dst)`                                  | colorspace to colorspace; thirteen overloads cover every pair  |
|  [02]   | `Config.getProcessor(transform, direction)`                      | an ad-hoc transform compiled against the config                |
|  [03]   | `Processor.getDefaultCPUProcessor()`                             | CPU executor at `OPTIMIZATION_DEFAULT`                         |
|  [04]   | `Processor.getOptimizedCPUProcessor(inBD, outBD, flags)`         | CPU executor with explicit bit depths                          |
|  [05]   | `CPUProcessor.applyRGB(arr)` and `applyRGBA(arr)`                | apply IN PLACE over a contiguous float32 array                 |
|  [06]   | `CPUProcessor.apply(imageDesc)`                                  | apply over a packed or planar `ImageDesc` view                 |
|  [07]   | `CPUProcessor.isNoOp()`, `isIdentity()`, `hasChannelCrosstalk()` | skip an identity chain; crosstalk decides per-channel legality |
|  [08]   | `Processor.getDefaultGPUProcessor()`                             | GPU executor                                                   |
|  [09]   | `GpuShaderDesc.CreateShaderDesc(language=…)`                     | the shader descriptor; `GPU_LANGUAGE_MSL_2_0` is the Metal row |
|  [10]   | `GPUProcessor.extractGpuShaderInfo(desc)`                        | fills shader text, function name, uniforms, and LUT textures   |
|  [11]   | `Processor.createGroupTransform()` and `getCacheID()`            | the flattened op list, and the identity a receipt records      |
|  [12]   | `Baker.setInputSpace/setTargetSpace/setFormat` then `bake()`     | LUT egress                                                     |

- `Processor.getOptimizedCPUProcessor`: naming a non-F32 depth makes `PackedImageDesc` demand its full stride triple.
- `Baker`: `setDisplayView(display, view)` drives the display-view leg, `setShaperSpace`/`setShaperSize` the shaper leg, and `getFormats()` lists every writable format.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Configs hold the authority and roles are how a plane names its space — `scene_linear` resolves the working space (`ACEScg` on the shipped CG config), `data` the unmanaged raw space, `color_picking` and `texture_paint` the display-referred spaces. Pipelines naming a colorspace string directly bind to one config; naming a role survives a config swap, which is the whole reason a config exists.
- One `Config` compiles once and its `Processor` cache is internal, so a plane fold acquires the processor OUTSIDE the loop and applies inside it. `Processor.getCacheID()` is the identity that goes on the receipt, so a transform's version is recorded without serializing the graph.
- `CPUProcessor.applyRGB`/`applyRGBA` mutate the array IN PLACE and return `None`; callers wanting the source preserved copy first. That array is `float32` and contiguous — a strided or non-float view crosses through `PackedImageDesc` with explicit channel, x, and y strides instead.
- `isNoOp()` is the skip predicate every fold reads before applying: an identity chain costs nothing to detect and a full pass to run. `hasChannelCrosstalk()` decides whether a single-channel plane may be transformed at all — a crosstalk chain over one channel is meaningless.
- Bit depth is a PROCESSOR property, not a buffer property: `getOptimizedCPUProcessor(inBitDepth, outBitDepth, flags)` compiles the conversion into the chain, so an 8-bit ingress to a float working space converts once inside the optimized processor rather than through a caller-side cast.
- GPU emission is a DESCRIPTOR fill, never a string return — `GpuShaderDesc.CreateShaderDesc(language=…)` then `GPUProcessor.extractGpuShaderInfo(desc)`, after which `desc` carries the shader text, the entry function name, the uniform roster, and the LUT textures the caller must bind. Emitting text without binding those textures produces a shader that compiles and computes the wrong answer.
- Process-wide state is app-root only: `SetCurrentConfig`, `SetLoggingLevel`, `SetLoggingFunction`, `SetEnvVariable`, and `ClearAllCaches` mutate the process. Library tiers thread an explicit `Config` and reads `GetVersion()` for the receipt, and never touches the current-config slot.
- Config resolution from a file path is `getColorSpaceFromFilepath`, which returns the colorspace and rule index from the config's own `FileRules` — an ingest that infers a colorspace from a filename convention of its own forks the project's declared rules.
- Every fault is `PyOpenColorIO.Exception`, with `ExceptionMissingFile` for an unresolvable LUT or config reference, so the boundary adapter maps that one family onto the estate's typed fault rather than catching per call.

[STACKING]:
- `colour-science`(`.api/colour-science.md`): the split is AUTHORITY. Colorimetry, CAM16, spectral computation, chromatic adaptation, and gamut volumes stay with `colour-science`; what a project's `ACEScg` or display view MEANS stays with the config. Hardcoding a primaries matrix where a config role answers is the fork this owner exists to foreclose, and the shipped ACES configs make the role reachable without a config file.
- `coloraide`(`.api/coloraide.md`): CSS color spaces, interpolation, and gamut mapping for document and UI color stay with `coloraide`; scene-referred rendering color stays here. Neither ever transforms the plane the other owns.
- `imagecodecs`(`.api/imagecodecs.md`): `cms_profile`/`cms_transform` own the ICC edge — an embedded profile on an ingested file decodes there — and this owner takes the plane once it is in a config-named space. Scene-linear space resolves through the `scene_linear` role, never by synthesizing an ICC profile at `gamma=1.0`.
- `openexr`(`.api/openexr.md`): EXR and the deep-pixel codecs carry scene-linear VALUES and no space declaration, so the config resolves the space and the transform runs before the write or after the read, never inside a codec.
- `numpy`(`.api/numpy.md`): the application boundary is a contiguous `float32` array mutated in place; a plane at another depth or stride crosses through `PackedImageDesc`/`PlanarImageDesc` rather than through a copy.
- `msgspec`(`.api/msgspec.md`): the color policy — config source, role or colorspace name, display, view, look, optimization flags, GPU language — is a typed struct, so every name reaching the config is a validated tag and the policy itself is content-keyable.
- `beartype`(`.api/beartype.md`): the boundary contract is `NDArray[float32] -> None`, and a non-float or non-contiguous plane rejects there rather than inside pybind11.
- `structlog`(`.api/structlog.md`) / `opentelemetry`(`.api/opentelemetry-api.md`): each transform stamps `Processor.getCacheID()`, the config name, the source and destination pair, the optimization flags, and the texel count on the owning span; `GetVersion()` rides the startup census once.
- `anyio`(`.api/anyio.md`): CPU application is a native call releasing the GIL, so a large-plane transform crosses the runtime `RELEASING` thread arm.

[LOCAL_ADMISSION]:
- `import PyOpenColorIO` at boundary scope only, behind the `lazy import` proxy the native color owners use.
- Interactive grading with its dynamic properties, `MixingColorSpaceManager`, `ColorSpaceMenuHelper`, and `SystemMonitors` are UI-facing surfaces; a host-free producer composes the static transform chain alone.

[RAIL_LAW]:
- Package: `opencolorio`
- Owns: the config-driven color pipeline — config resolution from the shipped ACES CG and Studio set, a file, a stream, or a proxy; the colorspace, role, display, view, look, named-transform, and file-rule graph; processor compilation with explicit bit depths and optimization flags; in-place CPU application over `numpy` and `ImageDesc` buffers; shader emission for GLSL, MSL, HLSL, and OSL; LUT baking; `.ocioz` extraction
- Accept: spaces named by ROLE wherever a role exists; one processor acquired outside the fold and applied inside it; `isNoOp` and `hasChannelCrosstalk` read before applying; contiguous `float32` in place, or an `ImageDesc` view with explicit strides; bit-depth conversion compiled into an optimized processor; `getCacheID()` on every receipt; `getColorSpaceFromFilepath` for ingest classification
- Reject: a hardcoded primaries matrix, transfer curve, or colorspace string where a CONFIG ROLE answers — the carve: a spec-frozen transfer the wire itself freezes (the IEC 61966-2-1 sRGB pair, the ST 2084 PQ curve on a plane-vocabulary owner) and the fixed AP1 luminance row are TRANSCRIPTIONS of frozen constants, not config decisions, so the texture plane's encode ladder legally spells them inline and this reject binds colour-SPACE conversion alone; a synthesized ICC profile standing in for a config transform; process-wide `SetCurrentConfig`, `SetLoggingLevel`, or `SetEnvVariable` below the composition root; per-texel or per-call processor construction; GPU shader text emitted without binding the descriptor's LUT textures and uniforms; colorimetric or spectral computation claimed here, which is `colour-science`; CSS-space parsing or gamut mapping, which is `coloraide`; interactive grading and menu-helper surfaces in a host-free producer
