# vector-assets — bedrock

## SVG document pipeline

- Admission family: static `SKSvg.CreateFromFile/CreateFromStream/CreateFromSvg/CreateFromSvgDocument/CreateFromXmlReader` constructs the document owner.
- Instance `Load(stream | path [, SvgParameters] [, baseUri])` and `FromSvg(string)` return `SKPicture?` — null is the parse-failure sentinel, projected to the rail at the admission boundary, never propagated inward.
- The `baseUri` parameter resolves relative references inside the document; omitting it for documents with external references silently drops those subtrees — base-uri presence is an admission-policy fact per asset class.
- `SvgParameters(Entities, Css, CurrentColor, LoadOptions)` is the restyle row: entity substitution and CSS injection apply at load, and `CurrentColor` resolves the document's current-color keyword.
- Theming a vector asset is a parameters value at admission, not a post-draw color filter — the filter route re-tints every pixel per frame, the parameters route re-resolves once into the retained picture.
- `ReLoad(SvgParameters?)` re-renders from the retained original source without re-reading the stream, gated by the static `CacheOriginalStream` switch.
- `ReLoad` is the variant-matrix mechanism: one admitted asset yields N restyled pictures by parameter rows; with the source cache off, `ReLoad` has no source and the variant matrix silently degrades to single-variant — the switch is part of asset-pipeline policy, not a tuning knob.
- `SKSvgSettings` is the document-render policy record: `ColorType`/`AlphaType`/`Srgb`/`SrgbLinear` rows align SVG rasterization with the surrounding color law.
- `StandaloneViewport` pins intrinsic sizing for fragment documents that omit their own viewport — "asset with no intrinsic size" becomes unrepresentable past admission.
- `TypefaceProviders` routes SVG text through declared typefaces; without providers, text-bearing SVG resolves against the host registry and breaks evidence determinism — evidence-class assets either carry providers or carry no text.
- `EnableSvgFonts`/`EnableTextReferences` gate the text feature surface per asset class — off for icon-class assets, declared for document-class assets.
- The JavaScript rows (`EnableJavaScript`, `EnableExternalJavaScript`, timeout and statement caps) default off and stay off: an asset pipeline is not a script host; scripted SVG is rejected at admission, and the caps exist only for an explicit interactive-document surface that opts in.
- Output unification: `Picture` (retained `SKPicture`) is the canonical artifact — it draws onto any render-target row (raster frame, document page, shader tile) without re-parse.
- `Save(stream | path, background, format, quality, scaleX, scaleY)` is the rasterize-and-encode convenience for thumbnail and preview lanes — never the evidence route, which rides the pinned-projection law.
- `Save` defaults are policy facts: PNG format, full quality, unit scale, with the background color a required argument — transparent-background rasterization is an explicit transparent color, not an omitted parameter.
- The scale pair on `Save` rasterizes from vector source at target scale — scaling the vector at rasterization beats scaling the raster after, and any thumbnail ladder derives from one source by scale rows, never by resampling a master raster.
- Static `Draw(canvas, fragment, model, loader)` and `ToPicture(fragment, model, loader)` are the fragment-level rows for composing document subtrees into larger scenes.
- Retained-scene rows serve interaction, not drawing: `TryEnsureRetainedSceneGraph()`/`RetainedSceneGraph`/`HasRetainedSceneGraph` build the node-addressable scene on demand.
- `HitTestSceneNodes` and `TryGetPicturePoint(point, canvasMatrix, out picturePoint)` resolve pointer space to picture space — hit-testing is a scene-graph capability, never a pixel probe.
- `AnimationController` plus the `AnimationInvalidated` event drive animated documents; `AnimationMinimumRenderInterval` is the frame-rate floor, and `HasAnimations` is the cheap predicate that keeps static assets off the animation path entirely.
- The cost law: picture-only for icons and illustrations, scene graph only for hit-testable or animated documents — static assets never pay scene-graph construction.
- `Wireframe`/`WireframePicture` is the structural-diagnostic row: geometry without paint, rendered as overlay evidence — distinguishing path defects from paint defects without external tooling.
- Animated documents sample deterministically: `AnimationTime` is a settable value, so evidence frames render at pinned timestamps instead of wall-clock — animated SVG joins the render-hash law by sampling declared times, and a timeline regression is a set of per-timestamp hash receipts.
- Animation-layer caching (`UsesAnimationLayerCaching`) rebuilds only dirty animated subtrees as layer pictures — a small animated element in a large document re-renders its layer, not the document; the dirty-target count is the receipt that the partition works.
- The document owner exposes a `Sync` object for cross-thread coordination — load/restyle on workers and draw on the render thread serialize at the owner's declared seam, composing the marshaling law rather than inventing locks per call site.
- Restyle is reload, never mutation: parameter changes route through `ReLoad`; there is no in-place style mutation surface, which makes every styled state a value with an identity — the property that lets variant caches exist at all.
- Rebuilds defer disposal of replaced pictures: the animation/restyle machinery queues superseded pictures for deferred release — a variant rebuild never tears a picture out from under an in-flight draw, and the deferred queue is the seam where picture lifetime meets frame lifetime.
- The document owner is itself disposable and owns everything it retained — source, pictures, scene, controller; consumer-held pictures outliving the owner is the lifetime inversion the capsule grammar forbids.
- Pointer dispatch over interactive documents is a dispatcher query (`HitTestTopmostElement`) — the asset lane answers "which element", and what to do about it belongs to the interaction layer; the split keeps hit-testing capability in the document profile without dragging input handling into it.
- The UI-control surface (`Svg` control, `SvgImage`, `SvgSource` with `Path`/`Entities`/`Css`/`CurrentColor` init rows) is the shell-bound projection of the same document owner.
- `SvgSource.EnableThrowOnMissingResource` is the fail-loud admission switch for resource-backed sources — on for development surfaces, with absence projected to the rail in production composition.
- The shell source mirrors the document admission family as statics: `SvgSource.Load(path, baseUri, parameters)`, `LoadFromSvg`, `LoadFromStream`, `LoadFromSvgDocument` — same grammar rows, shell-typed result; the two families never diverge in admitted inputs.
- The shell's asset-loader and model seams are static composition slots (`s_assetLoader`, `s_skiaModel`) — one loader identity per process for shell-side SVG, set at composition, which keeps control-tree SVG and document-lane SVG resolving resources identically.
- The shell source retains the original stream alongside the parsed result — shell-side restyle inherits the same retained-source contract the document owner declares, and the retained stream is the reason shell parameter flips never re-touch storage.
- Document, export, and evidence lanes hold `SKSvg` directly — two consumption profiles over one pipeline, selected by process modality.
- A second vector grammar rides the same owner: `CreateFromVectorDrawable`/`LoadVectorDrawable`/`FromVectorDrawable` admit the drawable-XML format through identical signatures — grammar is an admission row, not a second pipeline, and downstream consumers cannot observe which grammar produced a picture.
- The owner retains both the parsed document (`SourceDocument`) and the render artifact (`Picture`) — restyle reads the retained source, render reads the artifact; consumers that only draw never touch the document side.
- `TryGetPicturePoint` takes the canvas matrix explicitly — hit-testing under pan/zoom transforms resolves correctly because the transform is an argument, not ambient state read from a canvas.
- The animation surface carries its own staleness vocabulary: `HasPendingAnimationFrame` and the dirty-target count let a host pump redraw only when the document actually changed — polling-free animation driving, with the invalidation event as the push channel.
- The asset-loader contract (`ISvgAssetLoader` with its default implementation) is the dependency seam for embedded raster and font resources inside documents — injected once at owner construction, which keeps document rendering hermetic under test.
- The custom draw operations (`SvgCustomDrawOperation`, `SvgSourceCustomDrawOperation`) are the shell-side render bridge: the control tree submits the retained picture into the shell's deferred-render pass — the picture crosses the bridge, never the document.
- The `Svg` control's `Stretch`/`StretchDirection` rows are layout policy over intrinsic bounds; `InvalidateVisual` is the manual redraw request for parameter changes the control cannot observe — both are shell-profile facts with no document-profile counterpart.
- `SvgImage` carries its own `Size` and `CurrentColor` rows — per-usage size and color override at the image surface without touching the shared source; one source, many sized/tinted usages is the image row's reason to exist.
- The markup rows (`SvgResourceExtension`, `SvgImageExtension`, the source type converter) construct sources from strings at markup parse — ergonomics for declarative trees; programmatic composition holds typed sources, and the converter path inherits the missing-resource policy switch.
- The control draws via custom draw operations submitted to the deferred render pass — the shell never rasterizes the SVG eagerly; invalidation re-submits the operation, and the picture behind it only rebuilds when source or parameters change.

## icon vocabulary as derived catalog

- Icon identity is the pair (`Symbol`, `IconVariant`) end-to-end: `Symbol` is a closed glyph enum thousands strong, `IconVariant` a byte enum (`Regular`, `Filled`, `Color`, `Light`) — an orthogonal rendering axis, not a second namespace.
- String-keyed icon registries, path-drawn copies of vocabulary glyphs, and ad-hoc glyph bitmaps are the three rejected forms; unknown symbols are unrepresentable because the vocabulary is the type.
- The domain icon catalog derives from the vocabulary, never re-declares it: a frozen role→(`Symbol`, `IconVariant`) map whose keys are bounded domain roles.
- Adding a domain icon is one row; renaming a glyph is impossible because names travel as enum members — the symbolic-reference law applied to iconography.
- The variant axis swings independently of role: density and theme select the variant column, the role selects the symbol row — variant flips touch zero role declarations.
- Vocabulary growth is additive and non-breaking: new glyph members extend the enum without disturbing existing members, and catalog keys insulate consumers — an icon-vocabulary upgrade is a dependency bump plus optional new rows, never a rename sweep.
- Rendering is font-backed: `SymbolIcon`/`SymbolImage` and the markup-extension rows construct glyph runs from an icon font — icon scale is `FontSize`, icon color is the foreground brush, and icons inherit text rendering quality.
- Font-backed rendering deletes the raster ladder: no per-icon bitmap assets, no resolution variants, no tint filters — one glyph, every size, every color.
- The raw-glyph rows (`FluentIcon`/`FluentImage`) accept arbitrary glyph input beside the `Symbol`-typed rows — the escape hatch for glyphs outside the vocabulary; catalog rows never use it, because raw glyphs reintroduce the unbounded namespace the enum closes.
- The outline row is a rendering-mode axis on the icon surface, orthogonal to variant — outline rendering composes with the identity pair rather than forking it into outline-suffixed names.
- The `Color` variant row is payload-bearing (multicolor glyphs) and ignores foreground tinting by design — catalogs mixing `Color` rows with tint-driven theming must mark those rows, because the theme axis silently stops applying on them.
- Icon-in-canvas composition: where icons enter a raw canvas scene rather than a control tree, the same identity law holds — the catalog row resolves to a glyph and renders through the shaped-text rail; rasterizing a control to smuggle an icon into a scene is the rejected detour.
- The string-conversion row (`Converter` on the icon control) exists for markup ergonomics only; programmatic icon selection always rides the enum — string round-trips through the converter re-introduce the unknown-name failure mode the enum deletes.
- The control family splits by render slot, not capability: icon controls fill icon-presenter slots, image variants (`SymbolImage` and the glyph-image rows) fill image-source slots — one identity pair serves both slots, so slot choice never forks icon declarations.
- The markup-extension rows (`ProvideValue` with `Symbol`/`IconVariant`/`FontSize`/`Foreground`) construct inline without a named resource per icon — per-icon resource dictionaries are the ceremony the extensions delete.
- The variant axis is exhaustive at the catalog, not at call sites: a catalog row missing a variant mapping fails catalog freeze; call sites select variants by axis value and cannot request an unmapped combination.
- Icon scale rides exactly one knob (`FontSize`) and icon color exactly one slot (foreground) — both arrive token-resolved; a size or color literal at an icon site is the same defect as a paint literal at a draw site.
- Font-backed icons inherit text determinism axes: edging, hinting, and font provenance govern icon raster output exactly as they govern text — evidence surfaces bearing icons pin the same axes the typography law pins, or icon antialiasing breaks hashes.
- Fallback iconography is catalog data: the missing/error/loading states of asset-bearing surfaces map to declared catalog rows — an inline placeholder bitmap is the rejected spelling of "fallback", because it escapes the variant and theme axes.

## raster loading, caching, identity

- Loader hierarchy is cache policy as type: base web loader (injected `HttpClient`, `disposeHttpClient` flag) → RAM-cached → disk-cached (`cacheFolder` row).
- Cache behavior is selected by loader instance held at composition; per-call cache flags are the rejected form.
- The injected `HttpClient` is the outbound-policy seam: resilience, timeout, and credential ownership attach to the client at composition — the loader never owns retries, and a loader-local retry loop would stack a second owner on the hop.
- The disk-cache folder default is a relative path — it resolves against the working directory, which floats by launch context; deployed composition declares an absolute, process-owned cache folder, and the relative default is a development-only convenience.
- `ProvideImageAsync(url[, storageProvider])` returns a nullable bitmap; null is the load-failure sentinel projected to absence at the boundary.
- The control-level `FallbackImage`, `IsLoading`, and `CurrentImage` rows are the visible load-state receipts — failure renders the declared fallback, never a blank or a throw on the render path.
- The disk key is a digest of the locator, not the content: cache identity is URL-derived, so a changed remote behind a stable URL stays stale forever, and two URLs serving identical bytes cache twice.
- The identity law splits the axes: locator key (where it came from) and content hash (what it is) are both recorded in the load receipt — content-addressed consumers key on the hash, cache layers key on the locator, and the receipt makes the distinction auditable.
- A wrong load surfaces as a receipt, not a draw fault: the load receipt carries (locator key, byte length, content hash, decoded info identity, codec admission result).
- A corrupted asset, a substituted remote, or a decode-shape drift each fail a receipt comparison upstream of any canvas — pixel-level faults stay reserved for genuine render defects.
- Byte length is the cheap pre-gate before hashing: a length mismatch rejects without computing the content hash — receipt comparison is ordered cheap-to-expensive, and the ordering is part of the receipt fold, not caller discipline.
- One hash family serves every receipt in the lane: per-receipt hash choices fork identity comparisons across tiers — the hash surface is declared once where the system's hashing law lives, and the asset lane consumes it.
- Fallback bitmaps are themselves admitted assets with receipts: a fallback that fails admission fails composition — the failure path cannot itself fail at runtime, because it was proven at boot.
- Codec admission for local and bundled rasters: `SKCodec.Create(path | stream, out SKCodecResult)` makes decode failure a typed result at admission instead of a null surprise.
- `SKCodecResult` is a full admission taxonomy: `Success`, `IncompleteInput`, `ErrorInInput`, `InvalidConversion`, `InvalidScale`, `InvalidParameters`, `InvalidInput`, `CouldNotRewind`, `InternalError` — the receipt's codec field carries the row, and each row routes differently.
- `IncompleteInput` is a partial-success row, not a failure: rows decoded so far are valid — the progressive-preview mechanism for truncated or still-arriving inputs, surfaced as a receipt fact (rows decoded versus expected) rather than folded into a boolean.
- `CouldNotRewind` is the stream-shape row: codecs probe then re-read, so non-seekable streams must be buffered at admission — a streaming source feeding a codec directly is the defect this row names.
- The brush-loader row targets brush slots (tiled and fill imagery) through the same loader contract as image slots — target kind is a projection row, not a second loading stack.
- The control's drawable wrapper row exists so a resolved bitmap can enter deferred render passes without re-entering the loader — resolution and presentation are decoupled at the wrapper seam.
- `SKCodec.Info` exposes the decode shape before pixels exist — dimension and format gates run pre-allocation.
- `GetPixels(info, …)` decodes directly into a pinned `SKImageInfo` — conversion-at-decode lands assets in the working format with no second pass.
- `SKCodec.EncodedOrigin` carries orientation metadata: decoded pixels are unrotated, and the origin transform must be applied at admission — skipping it makes hashes and draws diverge from every orientation-honoring viewer.
- The admission capsule bakes the origin transform into the decoded artifact so no downstream consumer ever sees pre-rotation pixels.
- Big-asset rows: `StartIncrementalDecode`/`IncrementalDecode(out rowsDecoded)` and the scanline family decode in slices under a memory budget — paged decode composes the staged-memory law instead of one monolithic allocation.
- Animated formats are a data surface on the same codec: `FrameCount`, `GetFrameInfo(index, out frameInfo)`, `RepetitionCount` expose an indexed frame table with per-frame durations — animation becomes rows over the codec capsule, not a separate decoder.
- The frame table hands animation driving to the host's motion pump: per-frame durations are data the pump schedules against — the codec never owns a timer, which keeps animated rasters under the same reduced-motion and frame-ceiling policy as every other moving surface.
- `SKBitmap.Decode` is the named rejected eager row: whole-image decode into a mutable bitmap with defaulted format — every capability it offers exists on the codec route with pinned formats and typed results.
- Lazy-vs-eager split: `SKImage.FromEncodedData` defers decode to first draw (`IsLazyGenerated`) and retains `EncodedData` for pass-through re-export — correct for draw-once and forward-the-bytes lanes.
- The codec route is the law wherever pixels are hashed, transformed, or drawn repeatedly: lazy images re-decode under cache pressure, and their decode shape floats with codec defaults — both fatal to identity work.
- Every decode/encode/page operation is a named using-scoped capsule: codec, intermediate pixmaps, streams, and scratch surfaces each live exactly one scope — the borrowed/owned grammar composed for the asset lane.
- An asset function that returns a live native intermediate has widened its contract by accident — the capsule's return type is the contract, and it is always a value.
- The loader hierarchy's extension seam is the protected hook pair: a custom cache tier overrides only the global-cache load/save pair while inheriting fetch, decode, and state mechanics — new tiers are two-method subclasses, never reimplementations of the loader contract.
- The attached-property surface (`SetSource`/`GetSource`, `GetIsLoading`, the global loader slot) retrofits async loading onto plain image controls — the row for surfaces that cannot adopt the dedicated control; the global loader slot is composition-root policy, set once.
- `ShouldLoaderChangeTriggerUpdate` declares whether swapping the loader instance re-resolves the current source — a reload-policy row that matters exactly when cache tiers are swapped at runtime (an offline/online transition); the row is declared with the loader swap or the swap is silently inert for already-resolved sources.
- `SKCodecOptions.FrameIndex`/`PriorFrame` make animated decode dependency-explicit: a frame decodes against its declared prior frame, so random access into an animation is a (frame, prior) pair, not a sequential replay — frame caching policy follows the dependency graph the options expose.
- The RAM tier's clear hook is the only in-process invalidation: content-keyed consumers invalidate by re-probing receipts, not by clearing tiers — clearing is an operational action, invalidation is an identity comparison, and conflating them turns every staleness fix into a cache stampede.
- `GetPixels(out byte[])` convenience rows allocate per call; the pinned-buffer `GetPixels(info, pixels)` rows are the steady-state spelling for thumbnail strips and atlas builds — allocation posture is chosen by overload, which makes it reviewable.
- `SKCodecOptions` carries subset and zero-init rows for partial decode — region thumbnails of giant rasters decode the window, never the full image; the subset row composes with the incremental decoder for bounded-memory previews of arbitrarily large inputs.
- One decode budget governs the lane: pinned `SKImageInfo` working format, slice size for incremental decode, and a byte ceiling per decode — three values in one policy record, so a pathological asset fails the budget gate with a receipt instead of an out-of-memory mid-render.

## divergent — svg-icon-catalog

- Parameterized restyle is the token bridge for vector assets: resolved theme values enter as `SvgParameters` (`Css`/`CurrentColor`) at admission, producing per-variant retained pictures.
- The asset variant matrix is (asset × variant rows), built by `ReLoad`, cached by (asset identity, variant key, catalog generation) — three forms deleted: draw-time tinting, duplicate per-theme files, runtime color filters.
- The SVG catalog and the icon catalog converge on one catalog shape: frozen key → (source identity, variant axis, resolved artifact), derived from a closed vocabulary.
- Glyph-backed rows resolve to glyph runs, document-backed rows resolve to pictures; consumers see one asset-by-key resolution surface and cannot observe which backing a key has — promoting an icon from glyph to full SVG (or back) is a row edit with zero consumer diffs.
- Intrinsic-size honesty: a vector asset has design-space bounds (picture cull rect or declared viewport) and the catalog row records them — layout reads recorded bounds, never rasterized measurements.
- Catalog failure taxonomy: parse-null (admission rejection with source identity), text-without-provider (freeze-time rejection for evidence-class rows), script-bearing source (admission rejection), missing variant cell (freeze-time rejection — the variant grid is total per asset class).
- Every taxon fails at catalog construction or admission — zero draw-time SVG failure modes survive, which is the property that lets render code treat assets as total.
- Asset keys obey the naming law: catalog keys are canonical role nouns, never file names — file identity is a row field, and renaming a source file is a row edit invisible to every consumer.
- Evidence eligibility is a catalog column for vector assets exactly as for typography roles: an evidence-class SVG row proves provider-resolved text and script-free source at freeze — the column unifies the determinism contract across asset kinds.
- The interaction-capable column pairs with the animation column: hit-testable rows carry scene-graph cost declarations — the catalog states which assets pay retained-scene memory, making the document/icon cost split auditable as data.
- The animation-capable subset is a declared catalog column: rows marked animated carry controller wiring and a frame-rate floor; static rows are structurally barred from the animation path — the column makes "which assets can animate" a query, not a runtime discovery.
- The whole asset manifest freezes at composition: catalog construction enumerates every declared asset, runs admission, and fails the process on any rejection — asset errors are boot-time, and the running system holds only admitted artifacts.
- Cold-start cost is a declared fold: preload rows mark assets resolved at freeze versus on first use — the prefetch set is catalog data, so startup-versus-first-paint tradeoffs are tuned by row edits with receipts proving the effect.

## divergent — raster-cache-identity

- The cache is one fold over tiers: probe RAM → probe disk → fetch origin, each tier emitting a hit/miss fact, the composite receipt recording which tier served and what identity it carried.
- Cache behavior becomes observable data: a cache-poisoning incident is diagnosable from receipts alone, with no instrumentation retrofit.
- Two-key discipline scales to every asset class: (locator, content hash) generalizes beyond web rasters to bundled files, generated thumbnails, and export artifacts.
- The locator names a slot, the hash names a value; staleness is exactly "slot's current hash ≠ recorded hash", and refresh policy — when to re-probe origin — is a policy row orthogonal to identity.
- Load identity composes the render-hash law: decoded-asset content hash plus pinned decode-info identity feed frame identity, so a baseline mismatch decomposes into asset drift (load receipt moved) versus render drift (same receipts, different frame hash).
- Triage starts from receipts, not pixel diffing — the decomposition is the operational payoff of carrying identity through the pipeline.
- Eviction posture is honest about its coarseness: the RAM tier clears whole and the disk tier ships no retention — retention and eviction policy live with the owning process's retention rows.
- The asset lane's contribution to retention is strictly the receipts that make external policy enforceable: per-entry locator, byte size, and last-served-tier facts — policy needs facts, and the lane's job is to emit them.
- The loader's storage-provider parameter is the local-file admission row — local picks ride the same loader contract and the same receipt shape as remote fetches, so provenance (remote, local, bundled) is a receipt field, not a code path.
- Concurrent first-loads of one locator are a single-flight concern: the tier fold keys in-flight resolution by locator so N simultaneous requests share one fetch — the stampede guard lives at the fold seam, and its presence is observable as one origin fact for N consumer facts.
- Receipts are durable evidence, not transient logs: persisted load receipts let an audit replay "what exactly did this process render from" — the receipt store is the asset lane's contribution to forensic reproducibility, composing the owning process's retention rows for its own lifecycle.
- Derived rasters (thumbnails, scaled previews) are content-addressed derived artifacts: the derivation key is (source content hash, derivation policy row) — a thumbnail never goes stale silently because its key moves with its source, and regenerating is idempotent by construction.
- The receipt schema carries its own shape generation: receipts written under an older schema row are readable by declared migration of the receipt fold — identity evidence outlives the code that wrote it, which is the property audits actually need.
- The decode budget gate composes into the tier fold, not beside it: a budget rejection is a tier-fold fact like any miss — oversized assets fail with the same receipt shape as missing assets, and consumers handle one rejection vocabulary.
- Receipt history drives cache warming: the most-served locators from persisted receipts form the warm set for the next boot — warming policy is derived data from evidence the lane already emits, requiring no separate usage tracking.
- Cross-process cache sharing is locator-keyed and hash-verified: a second process trusts a disk-tier entry only when the recorded content hash matches its own admission — the hash check converts shared caches from a trust boundary into a verification boundary.
- The two-key receipt makes A/B asset rollouts diffable: two builds rendering different pixels decompose into locator changes (intentional swaps) versus hash changes under stable locators (unintentional drift) — release review reads the receipt diff.
- Identity work never blocks the render path: hashes compute at admission and persist on receipts — draw-time identity checks are reads of recorded values, and a pipeline computing hashes per frame has misplaced the seam.
