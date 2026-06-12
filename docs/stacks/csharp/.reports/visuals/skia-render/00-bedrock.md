# skia-render — bedrock

## surface ownership and the offscreen capsule

- `SKSurface`, `SKImage`, `SKPicture`, `SKColorSpace`, `SKShader`, and the filter objects are reference-counted natives (`ISKReferenceCounted`): `Dispose` decrements, never frees eagerly under sharing.
- The borrowed/owned capsule grammar composes directly onto the reference-count model: factory-returned objects are owned cases, accessor-returned objects are borrowed cases — the render lane adds the target-row vocabulary, not new capsule mechanics.
- `SKSurface.Canvas` is a borrowed accessor owned by the surface: the canvas must never be disposed by the consumer and dies with its surface.
- A capsule that returns the canvas has leaked a borrowed handle; capsules project values (images, hashes, receipts), never the canvas.
- Raster creation is one `Create` family of rows: `Create(SKImageInfo)`, `+rowBytes`, `+SKSurfaceProperties`, `Create(SKPixmap)`, and wrap-foreign-memory `Create(info, nint pixels, rowBytes, SKSurfaceReleaseDelegate, context)`.
- The release-delegate row is the only correct lifetime bridge for externally allocated pixel buffers; copying into a managed array to avoid the delegate doubles peak memory on large targets.
- The explicit `rowBytes` parameter exists for stride-aligned foreign buffers; computing it as `width * BytesPerPixel` is only valid for tightly packed memory — interop buffers with row padding require the declared stride.
- GPU rows mirror the raster rows over `GRContext`/`GRRecordingContext` × (`GRBackendRenderTarget` | `GRBackendTexture` | budgeted `SKImageInfo` with `sampleCount` and `GRSurfaceOrigin`).
- `GRContext.SetResourceCacheLimit(long)` and `GetResourceCacheUsage(out maxResources, out maxResourceBytes)` are the only GPU memory budget knobs; they belong in the render policy record, never at call sites.
- `GRContext.AbandonContext(releaseResources)` is the device-lost row: after abandonment every surface and texture bound to the context is dead, and the capsule that owns the context owns the teardown ordering.
- `SKSurface.CreateNull(width, height)` executes the full draw pipeline against no pixels — the measurement and cost-probe row: op counting and layout validation without raster expense.
- `Snapshot()` is copy-on-write: the returned `SKImage` shares the surface pixels until the next draw forces a copy — snapshot-then-encode is free, snapshot-then-keep-drawing pays one full-surface copy, so the capsule orders snapshot last.
- `Snapshot(SKRectI bounds)` snapshots a sub-rectangle — region evidence without full-frame capture.
- `Surface.Draw(canvas, x, y, paint)` composites one surface onto another canvas without an intermediate snapshot — the surface-to-surface composition row for tiled or layered pipelines.
- `PeekPixels()` returns a zero-copy `SKPixmap` view valid only while the surface lives — the hash-without-copy path; the pixmap is a borrowed view that must not escape the capsule scope.
- `ReadPixels(SKImageInfo dstInfo, …)` is the converting copy: it converts color type, alpha type, AND color space to `dstInfo` on exit, making it the canonical pinned-format projection for evidence.
- `Flush(submit, synchronous)` carries GPU semantics only; on raster surfaces it is inert — flush-before-snapshot on raster is cargo discipline, on GPU it is mandatory before external backend-texture reads.
- `SKImageInfo.BytesSize64`/`RowBytes64` exist because the `int` forms overflow past ~2GB; admission of large target dimensions gates on the 64-bit forms.
- `SKImageInfo` carries `With*` derivation rows (`WithSize`, `WithColorType`, …): derived infos preserve the rest of the declaration — evidence projections derive from the policy info instead of re-declaring fields.
- `SKBitmap` is the rejected working surface: mutable, not reference-counted, codec-era; the law is surface→image→encode, with `SKBitmap` admitted only where a third-party seam forces it.
- Foreign-pixel image admission has two rows with opposite ownership: `SKImage.FromPixelCopy(info, …)` copies and owns, `SKImage.FromPixels(info, …)` wraps and borrows — a wrapped image over a caller buffer is a borrowed case whose validity window is the buffer's lifetime, and confusing the two rows is a use-after-free in waiting.
- `SKImage.FromEncodedData(data, SKRectI subset)` admits a sub-rectangle of an encoded asset without decoding the whole — the crop-at-admission row for sprite extraction.
- `SKSurface.SurfaceProperties` is a borrowed accessor like the canvas: read for policy verification, never disposed by the reader.
- `ReadPixels` carries an `SKImageCachingHint` overload: the disallow-caching row prevents a one-shot evidence read from polluting the image cache with pixels nothing will reuse.
- GPU target distinction is capability-bearing: `GRBackendRenderTarget` wraps an existing framebuffer (view interop), `GRBackendTexture` wraps a texture (offscreen + sampling), and the budgeted-`SKImageInfo` row lets the context allocate — three rows, three ownership stories, one `Create` family.
- `GRSurfaceOrigin` is the vertical-flip trap: GL-style bottom-left origin surfaces render upside-down when treated as top-left; the origin is part of the target row's declaration, never inferred.
- `sampleCount` on GPU rows declares MSAA; raster surfaces have no MSAA row — antialiasing on raster is per-paint `IsAntialias`, which is one more reason GPU and raster output never hash-compare.
- The managed/native stream bridge: `SKWStream`/`SKFileWStream` rows exist beside `Stream` overloads on every document and encode entry — the native-stream rows avoid managed-wrapper chatter on hot export paths; both are owned disposables inside the export capsule.

## canvas state, recording, layers

- `Save()` returns the new depth; `RestoreToCount(int)` unwinds in bulk; `SaveCount` reads depth.
- A draw fold threads the saved count as fold state and restores to it in the failure arm — state balance becomes structural rather than disciplinary.
- `SKAutoCanvasRestore(canvas, doSave)` is the `IDisposable` bracket spelling for boundary code; its `Restore()` method permits early restore inside the scope.
- `SaveLayer` allocates an offscreen raster the size of its bounds: the unbounded `SaveLayer()` row allocates clip-sized layers and is the dominant hidden cost in naive group-opacity code.
- The law is `SaveLayer(SKRect limit, paint)` with computed bounds, or `SaveLayer(in SKCanvasSaveLayerRec)` for backdrop and flag control.
- A `SaveLayer` is earned only by group compositing — group alpha, blend mode, or image filter applied to a subtree; per-shape alpha rides the paint and needs no layer.
- Clip rows `ClipRect`/`ClipRoundRect`/`ClipPath`/`ClipRegion` all take `SKClipOperation` (Intersect/Difference) plus an `antialias` flag; clips are save/restore-scoped and monotonic — they only shrink.
- `QuickReject(rect | path)` before an expensive subtree is the cull law: it tests against device clip and total matrix without drawing.
- `IsClipEmpty`/`IsClipRect` and `GetLocalClipBounds`/`GetDeviceClipBounds` are the predicates for retained-scene partial-redraw decisions.
- `TotalMatrix44` is the full 4×4 transform truth; the 3×3 `TotalMatrix` is its projection — pipelines mixing perspective rows must read the 4×4 form.
- Recording law: `SKPictureRecorder.BeginRecording(cullRect[, useRTree])` → draw → `EndRecording()` yields an immutable `SKPicture`.
- `useRTree: true` builds a spatial index so playback against a clip skips out-of-bounds ops — the row for large static scenes invalidated in small rects; without it playback is linear in op count.
- `SKPicture` is the one retained display-list value: `Playback(canvas)` and `DrawPicture(picture, matrix?, paint?)` re-target one recording to raster, GPU, and document pages identically — render targets become rows over one recorded value.
- `SKPicture.ToShader(tileX, tileY, filterMode, localMatrix?, tileRect?)` turns the same recording into a paint shader — pattern fills from the scene vocabulary with no rasterize-then-tile round trip.
- `ApproximateBytesUsed` and `GetApproximateOperationCount(includeNested)` are the recording cost receipts.
- `UniqueId` is process-local identity, never content identity — using it as a cross-process cache key is the defect; content identity lives at the pixel projection.
- `Serialize`/`Deserialize` round-trip pictures as bytes but embed typefaces and images under codec defaults — picture bytes are transport, not evidence.
- `EndRecordingAsDrawable()`/`SKDrawable` is the re-evaluated row (callback runs per playback) versus the frozen picture; choose by whether content closes over state at record time.
- `DrawAtlas(atlas, sprites[], SKRotationScaleMatrix[], colors?, blend, sampling?, paint?)` draws N sprites from one image in one op — the instancing row that replaces N `DrawImage` calls for particle/glyph/marker swarms.
- `DrawImageNinePatch(image, centerRect, dst)` is the resizable-chrome row — stretchable panels without nine hand-cut draws.
- `Clear(color)` writes the color in source mode — it replaces pixels including alpha; `DrawColor` composites under the default blend — the two differ exactly on translucent fills over existing content, and confusing them is the ghost-frame defect on reused surfaces.
- `SKPicture.CullRect` is the recorded-bounds value: layout and invalidation read it as the picture's declared extent — content drawn outside the cull rect at record time may be elided, so the cull rect is a contract, not a hint.
- The `Flush(submit, synchronous)` flags split GPU semantics: submit pushes recorded work to the device queue, synchronous waits for completion — evidence reads from backend textures need both, frame pacing wants neither.
- `SKRoundRect` is the typed rounded-bounds value paired with `DrawRoundRect`/`ClipRoundRect` — per-corner radii ride the value, deleting path construction for the rounded-rectangle family.
- `SKRegion` with `DrawRegion`/`ClipRegion` is the pixel-aligned set algebra — invalidation unions and dirty-rect accumulation compose as region ops, never as float-rect lists with overlap bugs.

## paint, path, and effect law

- The text/pigment split is structural: every text property on `SKPaint` (`Typeface`, `TextSize`, `TextAlign`, `SubpixelText`, `HintingLevel`, `IsLinearText`, `FilterQuality`) is `[Obsolete]`; glyph state lives on `SKFont`.
- The canonical text draw is `DrawText(string, x, y, SKTextAlign, SKFont, SKPaint)`; the paint-only `DrawText` overloads are `[Obsolete]` rows — any paint-carried font in new code is the rejected form.
- Effect slots are orthogonal columns on one paint: `Shader`, `ColorFilter`, `ImageFilter`, `MaskFilter`, `PathEffect`, `Blender`/`BlendMode` — each slot holds an immutable reference-counted native shareable across paints.
- The resolve boundary constructs effect objects once and paints reference them; per-frame effect construction is the allocation defect.
- `SKPaint` is mutable scratch with exactly two legal lifetimes: a resolved long-lived paint owned by a token row, or a pooled scratch paint `Reset()` per use; `new SKPaint()` per draw call is rejected.
- `SKSamplingOptions` replaces the obsolete filter-quality scalar with a typed row: `(SKFilterMode, SKMipmapMode)`, cubic (`SKCubicResampler`), or anisotropic (`maxAniso`).
- Every `DrawImage` has a sampling overload; an image draw without explicit sampling inherits nearest-neighbor defaults that silently degrade scaled output — sampling is declared policy.
- `SetColor(SKColorF, SKColorSpace)` is the only color-managed paint color entry; the `SKColor` byte path assumes sRGB and quantizes to 8 bits before any conversion.
- Gradient construction has color-managed rows: `CreateLinearGradient`/`CreateRadialGradient`/`CreateTwoPointConicalGradient` accept `SKColorF[] + SKColorSpace` beside the byte-color forms — token-resolved gradients ride the float rows.
- `SKColorFilter` carries the per-pixel color algebra: `CreateColorMatrix(float[])`, `CreateTable(byte[])`, `CreateBlendMode`, `CreateLerp(weight, f0, f1)`, `CreateCompose(outer, inner)`, and the gamma pair `CreateSrgbToLinearGamma`/`CreateLinearToSrgbGamma` — color transforms compose as filter values, never as pixel loops.
- `SKMaskFilter.CreateBlur(style, sigma)` with `ConvertRadiusToSigma`/`ConvertSigmaToRadius` is the alpha-blur row; `SKImageFilter.CreateCompose` chains raster filters; `CreateMatrix(in matrix, sampling)` is the filter-space transform row.
- Path effects are declared geometry transforms: `CreateDash`, `CreateCorner(radius)`, `CreateDiscrete(segLength, deviation, seed)`, `CreateTrim(start, stop[, mode])`, `Create1DPath(path, advance, phase, style)`, composed via `CreateCompose`/`CreateSum` — stroke styling is data on the paint, never pre-deformed geometry.
- `CreateTrim` is the draw-fraction row for progress/reveal motion: animating trim re-uses one path value across frames.
- `SKPathMeasure(path, forceClosed, resScale)` answers `Length`, `GetPositionAndTangent(distance, out pos, out tan)` — placement-along-path (markers, flow arrows) reads the measure object instead of re-deriving arc length; it is a stateful native, pooled like other scratch.
- `SKRuntimeEffect` compile law: `CreateShader|CreateColorFilter|CreateBlender(sksl, out errors)` compiles once; the `errors` out-param is the rejection channel — null effect plus message, no exception.
- Per-frame runtime-effect work is `ToShader(uniforms, children, localMatrix?)` only; the builder forms (`BuildShader` → `SKRuntimeShaderBuilder`) own uniform-by-name update.
- SkSL effects are the escape hatch that keeps custom raster math inside the paint algebra instead of per-pixel managed loops.

## document export and durable commit

- `SKDocument.CreatePdf(path | Stream | SKWStream [, dpi | SKDocumentPdfMetadata])` and `CreateXps(… [, dpi])` share one page protocol.
- `BeginPage(w, h[, contentRect])` returns a canvas valid only until `EndPage()`; `Close()` finalizes; `Abort()` discards — pages are forward-only, so multi-page export is a fold over an ordered page sequence, never random access.
- The page canvas accepts the same recorded values as any target: drawing an `SKPicture` into a page preserves vectors and text; `DrawImage` embeds raster at `RasterDpi`.
- The export law routes scene content through pictures and reserves images for genuinely raster content — rasterizing vectors into a PDF is the fidelity defect.
- `SKDocumentPdfMetadata` rows: `Title/Author/Subject/Keywords/Creator/Producer`, `Creation`/`Modified` (`DateTime?`), `RasterDpi` (default 72f), `PdfA`, `EncodingQuality`.
- `EncodingQuality` defaults to 101 = lossless; values ≤100 switch embedded raster to JPEG at that quality — a discontinuity, not a gradient, and the lossless row is mandatory for artifacts re-entering hash gates.
- Determinism trap: PDF bytes embed timestamps and producer strings — byte identity over PDF output requires pinning `Creation`/`Modified` explicitly; otherwise document evidence hashes page rasterizations, never file bytes.
- Durable commit composes the atomic-write law: the document writes to a temporary sink, `Close()` completes before the commit step, and the rename-commit is durability-owned — the visuals contribution is the close-before-commit ordering edge and the receipt (page count, byte length, content identity).
- Simple encode rows: `SKImage.Encode(SKEncodedImageFormat, quality)` and parameterless `Encode()` (PNG).
- Knob-pinned encode rows live on `SKPixmap.Encode`: `SKPngEncoderOptions(filterFlags, zLibLevel)`, `SKJpegEncoderOptions(quality, downsample, alphaOption)`, `SKWebpEncoderOptions` — deterministic encoding goes image→`PeekPixels`→pixmap-encode, not through the image convenience row.
- `SKImage.EncodedData` returns the original encoded bytes when the image is codec-backed (`IsLazyGenerated`) — the re-encode-skip row for pass-through export; re-encoding a lazy image decodes and re-compresses for nothing.
- Document dpi is a coordinate-scale declaration: page coordinates are points at the default raster dpi (the declared 72f constant), and `RasterDpi` governs only embedded raster fidelity — conflating the two scales pages by accident.
- The XPS row shares the page protocol but not the metadata record — export policy that branches on format branches on exactly one row: the metadata argument's presence.
- `Abort()` versus `Close()` is the export rail's failure arm made explicit: the fold's error path calls abort and surfaces the typed failure; a capsule that disposes without closing has neither committed nor aborted, and the dispose-only path is the silent-loss defect.

## render-hash determinism

- Hash the pinned pixel projection, not the encoded artifact: `ReadPixels` into an explicit `SKImageInfo` (declared color type, alpha type, color space) and hash that buffer.
- Hashing encoder output couples frame identity to compression internals and metadata; the raw projection couples it only to the raster.
- Backend law: evidence renders on raster surfaces only — GPU rasterization varies by driver, MSAA resolve, and backend, so a GPU baseline is machine-local by construction; the GPU path exists for interactive throughput, never identity.
- Text determinism pins three independent axes: `SKSurfaceProperties` (pixel geometry drives LCD subpixel output), `SKFontEdging`/hinting on the font, and typeface provenance (packaged font data, never host lookup) — any unpinned axis diverges glyph antialiasing across hosts.
- Frame identity is a composite receipt: content hash + `SKImageInfo` identity + native-payload identity — baselines bind to the per-platform native library payload, so the baseline store keys by platform row.
- The native-asset load identity is part of visual evidence, not an out-of-band fact: cross-platform hashes legitimately differ, and the receipt makes the difference attributable.
- Alpha-type discipline: premultiplied is the native raster format; hashing an unpremul projection of a premul surface forces a lossy conversion per hash — pin the projection to premul unless a consumer contract demands otherwise, and never mix the two across a baseline's lifetime.
- The platform-default trap: `SKImageInfo.PlatformColorType` differs across hosts (BGRA vs RGBA ordering); any defaulted color type in an evidence path is a latent cross-platform hash break — evidence infos declare all four fields.
- Determinism axes as a checkable table: backend (raster), color (pinned info), text (edging/hinting/provenance), sampling (explicit options), state (no animation time, no host-dependent input) — a hash break is triaged by walking the axes, each independently auditable from declarations.
- Seeded effects are determinism rows too: the discrete path effect and noise shaders take explicit seeds — an unseeded stochastic effect in an evidence scene is a per-frame hash break by design, caught by requiring the seed in the effect's token row.
- Baseline storage is content-addressed by the composite receipt, not by test name: two tests rendering the same scene under the same policy share one baseline row, and a policy-record change re-keys every dependent baseline at once — baseline churn becomes proportional to policy change, not test count.

## color space law

- Construction rows: `CreateSrgb()`, `CreateSrgbLinear()`, `CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz)` for parametric spaces, `CreateIcc(bytes | SKData | profile)` for profiled input.
- Classification predicates: `IsSrgb`, `GammaIsLinear`, `GammaIsCloseToSrgb`; converters `ToLinearGamma()`/`ToSrgbGamma()`; introspection `GetNumericalTransferFunction(out fn)` and `ToColorSpaceXyz(out xyz)`.
- Equality is `SKColorSpace.Equal(a, b)` — reference equality lies because parametrically identical spaces are distinct natives; every color-space cache or policy comparison rides `Equal`.
- A null `ColorSpace` on `SKImageInfo` means passthrough — no conversion anywhere; fast and exactly wrong for evidence: untagged surfaces forfeit conversion correctness on every import and export.
- The law: every evidence and export surface is explicitly tagged; passthrough is reserved for measured hot paths whose inputs are pre-converted.
- Conversion happens at draw and at read: a tagged image onto a differently-tagged surface converts; `ReadPixels` with a differently-tagged `dstInfo` converts on exit — conversions are explicit in the declared infos, so an audit reads infos, not draw code.
- Wide-gamut posture: `RgbaF16`/`RgbaF16Clamped`/`RgbaF32` and `Rgba1010102`/`Bgra1010102` color types pair with linear or wide-primary spaces; `Bgr101010xXR` is the extended-range display row; `Srgba8888` tags 8-bit sRGB explicitly.
- 8-bit `SKColor` quantizes before tagging and cannot express wide-gamut values — the wide path is float (`SKColorF`) end-to-end or it is fiction.
- Linear-vs-gamma working space: blending and interpolation in gamma-encoded sRGB darken midtones; composite-heavy pipelines (layers, gradients, filters) declare a linear working surface and convert to sRGB at the final projection — one boundary conversion instead of wrong math everywhere.
- The gamma color-filter pair (`CreateSrgbToLinearGamma`/`CreateLinearToSrgbGamma`) is the in-pipeline correction row when a single draw needs linear math on a gamma surface — a scalpel, not a substitute for a tagged working space.

## divergent — surface-canvas-paint

- The maximal collapse is one render-policy record owning every determinism axis: `(SKImageInfo, SKSurfaceProperties, SKSamplingOptions, SKFontEdging, GRContext?)` — surface construction, paint defaults, image draws, and evidence projection all read this record.
- Target kinds (offscreen raster, GPU view, document page, picture recording) become constructor rows over the record; a new render target is one row, zero new pipeline code.
- Canvas folds compose as state-threaded expressions: a draw step is canvas→canvas only by convention; the load-bearing shape is a fold over an ordered op vocabulary where save-depth is fold state, making unbalanced save/restore unrepresentable instead of detected.
- Paint construction collapses into the token row: a resolved row is `(SKColorF, colorspace, style, strokeRow?, effectRow?)` with one materialize function — paint identity becomes row identity: shareable, hashable, diffable.
- Layer economics are computable before drawing: `CreateNull` plus op counting yields a cost probe; a scene exceeding a budgeted `SaveLayer` count per frame fails a structural perf gate without rendering a pixel.
- Rejected-form catalogue: `SKBitmap` working surfaces (mutable parallel rail), per-draw paint allocation (scratch without pool), unbounded `SaveLayer` (clip-sized hidden allocations), paint-carried text state (obsolete rail), `UniqueId` cache keys (process-local identity), implicit sampling (host-default degradation) — each row names the surface that deletes it.
- Capsule failure taxonomy: null surface from `Create` (unsupported info — typed rejection at admission carrying the info), oversized info (64-bit byte gate rejection), abandoned GPU context (every dependent handle dead — the capsule's context-epoch check converts use-after-abandon into a typed staleness rejection), runtime-effect compile failure (the `out errors` channel as evidence) — all four fail at declared seams, none mid-draw.
- Quantitative posture rows worth declaring once: a per-frame `SaveLayer` budget, a GPU resource-cache byte ceiling, and a per-scene op-count ceiling — three integers in the policy record that turn performance review into threshold checks against receipts.

## divergent — document-export-hash

- Export is one fold with receipts: `Seq<PageSpec> → SKDocument → Seq<PageReceipt>` where a `PageSpec` is (size, content picture, content rect?) and a `PageReceipt` is (index, raster hash at pinned dpi, op count).
- The document receipt is (page receipts, byte length, metadata identity); failure in any page arm aborts via `Abort()` — partially-written documents are unrepresentable in the rail because the durable commit fires only after `Close()` succeeds.
- Page-raster identity bridges vector export and pixel evidence: each page replays its picture onto a raster surface at pinned info and hashes — document evidence becomes codec-independent and timestamp-independent, verifiable per page.
- A metadata-only change keeps page hashes stable while byte identity moves; the dual receipt distinguishes the two causes without opening the file.
- One picture, three artifacts: the same `SKPicture` yields the interactive frame, the PDF page, and the evidence raster — divergence between any pair is always a target-policy difference (info, dpi, sampling), never a content difference, converting regression triage from "where did drawing diverge" to "which policy row differs".

## divergent — color-management

- Color policy is one frozen record: (working space, export space, evidence space, wide-gamut row?) with the conversion lattice derivable — working→export and working→evidence are declared once, and any draw-time conversion not derivable from the record is a defect surfaced by reading infos.
- Transfer-function algebra over `CreateRgb`: parametric spaces compose from a transfer curve plus a primaries matrix, so a new display posture is a data row, not code; `SKColorSpaceXyz.Identity` anchors the matrix algebra.
- ICC ingestion is the boundary row for foreign profiles, immediately re-expressed parametrically when `GetNumericalTransferFunction` succeeds — policy comparison stays value-based instead of profile-blob-based.
- F16-linear as the compositing fixed point: linear float working surfaces make blend math correct, keep wide-gamut headroom, and quantize exactly once at projection; the cost model is 2x memory on working surfaces only — intermediates, never baselines.
- Gamut-boundary behavior is part of the policy record, not the draw site: out-of-gamut handling on narrowing conversions is fixed by the declared space pair, and pipelines needing controlled mapping interpose one runtime color-filter row at the projection, keeping every draw call mapping-free.
