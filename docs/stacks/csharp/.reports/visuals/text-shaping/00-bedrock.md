# text-shaping — bedrock

## shape-before-raster pipeline

- Two shaping altitudes, one law: `SKShaper` is the typeface-bound convenience; the raw `Buffer`+`Font`+`Feature` surface is the control altitude — both produce the same glyph stream, so escalation is a row swap, not a rewrite.
- `SKShaper.Shape(Buffer | string, [x, y], SKFont)` returns `Result { Codepoints, Clusters, Points, Width }` — glyph ids, source clusters, raster-space positions, and advance width in one value.
- `SKShaper.Shape` exposes no feature parameter: the moment OpenType features, script pinning, or cluster-level policy matter, the pipeline drops to `Font.Shape(buffer, params Feature[])`.
- The `SKPaint`-taking `Shape` overloads are `[Obsolete]`; shaping consumes `SKFont` — the same text/pigment split as the raster side: paint never enters shaping.
- Buffer protocol order is load-bearing: add text (`AddUtf8`/`AddUtf16`/`AddUtf32`/`AddCodepoints`), set or guess segment properties, shape, read `GlyphInfos` + `GlyphPositions`.
- After shaping, `ContentType` flips to glyphs and the buffer rejects further text until `ClearContents()`.
- `Reset()` clears segment properties as well as content — pooled-buffer code that calls `Reset` and forgets to re-set direction silently inherits guessed properties.
- `GuessSegmentProperties()` infers `Direction`, `Script`, `Language` from content — convenient and nondeterministic at the margins (mixed-script runs, neutral codepoints); deterministic pipelines set all three explicitly per run and reserve guessing for throwaway measurement.
- The item-window forms are the context-correctness mechanism: `AddUtf16(text, itemOffset, itemLength)` shapes a slice while the shaper sees the full string for context.
- Pre-slicing the string before adding it severs cross-boundary forms — joining-script connections and kerning pairs at run seams — the classic invisible defect in run-segmented layout.
- Output payloads: `GlyphInfo { Codepoint (glyph id after shaping), Cluster, Mask, GlyphFlags }` and `GlyphPosition { XAdvance, YAdvance, XOffset, YOffset }` in integer font-scale units.
- The scale law: the Skia-bound shaper sets the shaping font scale to a fixed 512×512 and projects positions by `font.Size / 512f`; raw-pipeline code must reproduce exactly this projection or glyphs land subtly off-scale.
- Y projection negates: shaping space is y-up, the raster canvas y-down — `YOffset` enters position math negated, and forgetting the negation lands diacritics below instead of above.
- `Result.Width` is the accumulated `XAdvance` projection — measurement without raster.
- Advance-sum measurement and ink-bounds measurement differ: advances ignore ink overshoot (italic overhang, swashes); layout reserves space by advances while invalidation rects use ink bounds — two distinct queries, never interchangeable.
- Face/font admission: `SKTypeface.OpenStream(out ttcIndex)` → `ToHarfBuzzBlob()` → `new Face(blob, ttcIndex)` → `new Font(face)` is the zero-copy bridge from a raster typeface to a shaping font.
- One font file admitted once feeds both the shaping and raster sides; loading the file twice — once per library — doubles memory and risks version skew between the two views of one font.
- The `ttcIndex` out-param is load-bearing for collection files: dropping it and constructing the face at index zero silently shapes with the wrong collection member.
- `Face.MakeImmutable()` after construction publishes the face safely for concurrent shaping; `Face.UnitsPerEm` is the design-unit denominator for any metric math done outside the fixed-scale projection.
- `Font.OpenTypeMetrics` and `TryGetGlyphExtents`/`GetHorizontalGlyphAdvances(ReadOnlySpan<uint>)` answer metric queries without running shaping — the cheap path for monospace fast-paths and cursor math over known glyphs.
- `Feature` is a value row `(Tag, Value, Start, End)` with `Feature.Parse`/`TryParse` accepting the CSS-like microsyntax.
- Range-scoped features are the precision tool: tabular numerals on a numeric span only, ligatures off inside an identifier span — one shaping pass, no run splitting.
- Buffer and font are mutable natives; the pooling law: one scratch `Buffer` per thread, fonts cached per (face, scale) pair — composing the process-static cache law rather than per-call construction.
- `Blob`/`Face`/`Font` lifetimes nest — blob outlives face outlives font — and the owning capsule releases in reverse order.
- Glyph stream → raster bridge: `SKTextBlobBuilder.AddPositionedRun(ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>)` for materialized arrays.
- The allocate forms (`AllocatePositionedRun`, `AllocateRawPositionedRun` → `SKRawRunBuffer<SKPoint>`) hand back span-backed run buffers for zero-intermediate fill — the high-rate path writes glyphs and positions directly into blob memory.
- The builder also carries the rotation-scale run family (`AllocateRotationScaleRun`) for per-glyph transforms — curved and perspective text stays inside the blob vocabulary instead of per-glyph canvas transforms.
- `Build()` yields an immutable reference-counted `SKTextBlob` drawn via `DrawText(blob, x, y, paint)` — shape once, draw many: the blob is the cacheable artifact, the buffer is scratch.
- Cluster semantics: `Cluster` values map glyphs back to source indices in the original encoding's code units; under bidi reordering clusters are non-monotonic in visual order.
- `ClusterLevel` rows — `MonotoneGraphemes` (default), `MonotoneCharacters`, `Characters` — select caret granularity; hit-testing and selection mapping cannot be retrofitted onto a stream shaped at the wrong level, so cluster level is a typography-role decision, not a call-site decision.
- `SerializeGlyphs([Font])` is the shaping-evidence channel: a stable text dump of the glyph stream for golden tests of shaping itself — shaping regressions caught without pixel comparison, independent of raster determinism.
- Buffer hygiene rows: `ReplacementCodepoint` (default U+FFFD) and `InvisibleGlyph` declare how invalid input and invisible characters materialize — both are policy values on the buffer, set at pool admission so every shape call inherits them.
- `Buffer.Length` counts codepoints before shaping and glyphs after — the same property changes meaning at the shape boundary, and code reading it must know which side of the boundary it stands on.
- `Add(codepoint, cluster)` admits single codepoints with explicit cluster assignment — the synthetic-run row for generated text (measurement probes, glyph-exact test fixtures) where cluster topology is constructed, not derived from encoding.
- `AddCodepoints(ReadOnlySpan<uint>)` admits raw codepoints without UTF decode — the row for pipelines that already hold codepoint streams, skipping a decode-encode round trip.
- `Language` pins locale-conditional forms: shaping selects language-specific glyph substitutions by the declared language row — two locales can legitimately shape identical text to different glyph streams, which is why language is part of the shaped-run cache key.
- `Buffer.Append(buffer, start, end)` concatenates shaped or unshaped content across buffers — the splice row for composing pre-shaped segments without re-shaping the union.
- `ReverseClusters()` and `NormalizeGlyphs()` are post-shape transforms for visual-order consumers; pipelines that hand positioned runs to the blob builder never need them — their presence in code marks a consumer doing its own bidi reordering, which the cluster map already encodes.
- `Font.GetHorizontalGlyphKerning(left, right)` exposes pair kerning outside shaping — legitimate only for shaping-exempt roles; shaped runs already carry kerning in their advances, and adding it twice is a visible spacing defect.
- `Font.SetFontFunctions(fontFunctions, fontData)` is the synthetic-font seam: programmatic glyph metrics without a font file — the row for generated glyph systems (unit-test fonts, procedural pictographs) that keeps them inside the standard pipeline.
- `Face.Tables`/`ReferenceTable(tag)` expose raw font tables, and the `Face(GetTableDelegate)` constructor builds a face from table callbacks — the subsetting and font-introspection seam; `Blob.FaceCount` gates collection-file admission before any face construction.
- `Font.SupportedShapers` enumerates available shaping engines — an environment receipt for shaping evidence, recorded once at pipeline admission.
- The `Font(Font parent)` sub-font constructor inherits and overrides — variation instances and per-run scale tweaks derive from one parent without re-opening the face.
- `Direction` is a four-row vocabulary (`LeftToRight`, `RightToLeft`, `TopToBottom`, `BottomToTop`) with an `Invalid` zero — vertical text is a direction row on the buffer, never a canvas rotation; rotating horizontal shaping produces wrong glyph forms and wrong metrics.
- `Script` derives direction: `Script.Parse`/`TryParse` admit the four-letter script tag and `HorizontalDirection` answers the script's native direction — run segmentation reads direction from the script row instead of carrying a parallel direction table.
- `Tag` is the four-character value type behind features, scripts, and tables (`Tag.Parse`, the four-char constructor, implicit widening to `uint`) — feature and table names travel as values, never magic integers.
- `BufferFlags` are paragraph-edge contracts for windowed shaping: `BeginningOfText`/`EndOfText` tell the shaper whether the window's edges are true text edges — streaming or chunked shaping without them mis-shapes edge-sensitive forms (initial/final letterforms).
- `PreserveDefaultIgnorables`/`RemoveDefaultIgnorables` and `DoNotInsertDottedCircle` are buffer-flag policy rows: editor surfaces preserve ignorables for caret fidelity, render surfaces remove them, and the dotted-circle row disables the shaper's malformed-cluster repair marker for diagnostic views that must show input as-is.
- `GlyphFlags.UnsafeToBreak` on shaped glyphs is the wrapping contract: breaking a line at an unflagged boundary preserves shaping; breaking at a flagged glyph requires re-shaping the affected line — the wrap fold consults the flag and re-shapes only flagged break sites, which bounds re-shape work to ligature/kerning spans.
- The shaping-side line metrics live in `FontExtents` (`Ascender`/`Descender`/`LineGap` via `TryGetHorizontalFontExtents`/`TryGetVerticalFontExtents`) — a second metrics authority beside the raster-side struct; the role row declares which side owns line layout, because the two can legitimately disagree on fonts with inconsistent tables.
- The glyph-id width seam: shaping returns `uint` glyph ids, the blob builder consumes `ushort` — the narrowing at the bridge is lossless because the font format caps glyph counts below the 16-bit ceiling, and the cast belongs in the bridge, not scattered at call sites.
- `SKShaper` binds one typeface for its lifetime (`SKShaper(SKTypeface)`); shapers are cached per typeface beside the font cache — per-call shaper construction rebuilds the shaping font and is the convenience altitude's hidden cost.
- `MeasureText(ReadOnlySpan<ushort> glyphs, out SKRect bounds)` on the raster font measures a shaped glyph array's ink bounds — the post-shape ink query that pairs with advance-sum width to give both measurement families from one shaped result.

## typography roles and fallback chains

- A typography role is one frozen row owning every axis both shaping and raster read: typeface chain, size, feature set, cluster level, `SKFontEdging`, `SKFontHinting`, `Subpixel`, `LinearMetrics`, synthetic-style knobs (`Embolden`, `ScaleX`, `SkewX`), and line-metric policy.
- A call site holds a resolved role, never loose font parameters — the role row is the unit of text determinism.
- Line layout derives from `SKFontMetrics`, never constants: `Ascent` (negative), `Descent`, `Leading`, `CapHeight`, `XHeight`; `SKFont.Spacing` is the recommended line advance.
- `UnderlinePosition`/`UnderlineThickness`/`Strikeout*` are `float?` validity-gated — absent in some fonts; absence projects as `Option` at the boundary, never a zero default that draws hairlines on the baseline.
- Hardcoded line heights break the moment a role's typeface chain changes — metrics-derived layout is the only chain-stable spelling.
- Fallback resolves at role-resolve time, not draw time: `SKFontManager.MatchCharacter(familyName, weight/width/slant | SKFontStyle, bcp47[], codepoint)` consults the host registry — correct for interactive UI, nondeterministic across machines.
- The deterministic chain law: evidence and document text resolve from packaged font data (`SKFontManager.CreateTypeface(SKData | stream, index)` or `SKTypeface.FromData`), producing a pinned ordered typeface list per role.
- Host `MatchCharacter` is the interactive-only tail row of a chain, never present in evidence paths — the chain's row order is itself policy data.
- `SKTypeface.FromFamilyName` never returns null — it silently substitutes the nearest host font; it is banned as an admission route: the fail-loud admissions are the data/stream forms, with absence projected to the rail.
- `SKFontManager.GetFontStyles(familyName)` enumerates a family's style set — the probe that distinguishes a true bold face from a synthetic-bold requirement at catalog construction.
- Run segmentation precedes shaping: text splits into runs by (script, direction, resolved typeface), where typeface resolution walks the role's chain probing coverage.
- Raster-side coverage probes: `SKFont.ContainsGlyphs` and `GetGlyphs(ReadOnlySpan<int>, Span<ushort>)` with glyph 0 (`.notdef`) marking misses — the segmenter is one fold producing (range, typeface, script, direction) rows that shape independently and concatenate by advance.
- Shaping-side probes (`Font.TryGetNominalGlyph`, `TryGetVariationGlyph` with variation selectors) are glyph-exact where the raster probe is codepoint-coarse — emoji and variation-selector sequences resolve correctly only through the shaping-side probe.
- Raster knob coupling: `Edging`/`Subpixel`/`Hinting` change advance rounding when `LinearMetrics` is false — measurement and draw must read the same role row or measured layout drifts from drawn pixels by accumulated rounding.
- `LinearMetrics: true` is the layout-stable row — fractional advances, no hint snapping — and the default for any role whose text is measured.
- Line-height policy is a role-row choice between two spellings: metrics-derived (ascent/descent/leading sum) or multiplier-over-size — mixed-role lines take the maximum across participating roles, and the policy row makes the arbitration declared instead of emergent.
- Baseline-grid snapping is a layout-policy row above metrics: rounded baselines stabilize dense UI text, fractional baselines preserve document fidelity — the row belongs to the surface class, not the role, because the same role legitimately renders on both surface kinds.
- Truncation is cluster-safe by construction: ellipsis insertion picks the last unflagged cluster boundary that fits (advance prefix-sums against the budget) and appends the ellipsis run — byte- or char-level truncation tears clusters and is foreclosed by routing truncation through the shaped result.
- `SKFont.BreakText(text, maxWidth, out measuredWidth)` is the raster-side truncation probe — legitimate only for unshaped roles (no features, no fallback); shaped roles truncate on cluster boundaries from the shaped result.
- `SKFont.GetGlyphPositions(glyphs | text, origin)` is the unshaped positioning row — advance-accumulated, no shaping; legal only for roles declared shaping-exempt (pinned ASCII identifiers in a known font), and the exemption is a role-row fact.
- Synthetic styles are last-resort rows: `Embolden` and `SkewX` on a role cell mark a chain that lacks a true bold/italic face — declared data, not silent fakery.
- `SKFontManager.FontFamilies`/`GetFontFamilies()` enumerate the host registry — an environment receipt for interactive surfaces, never an input to evidence chains.
- The full metrics struct carries envelope fields beyond line metrics: `Top`/`Bottom` (extreme glyph extents), `XMin`/`XMax`, `AverageCharacterWidth`/`MaxCharacterWidth` — envelope fields size scroll extents and caret columns; line fields (`Ascent`/`Descent`/`Leading`) size baselines; mixing the two families produces clipped ascenders or bloated line boxes.
- Glyph-name rows (`TryGetGlyphName`/`TryGetGlyphFromName`, `GlyphToString`) serve diagnostics and font tooling — production text never round-trips through glyph names.
- Style axes are typed values: `SKFontStyle` composes weight, width, and slant rows; `GetFontStyles(familyName)` enumerates a family's style set and `MatchFamily(familyName, style)` resolves within it — catalog freeze probes style sets so a declared bold cell is a verified face, not a hopeful request.
- Style matching is nearest-match, like family matching: a requested style absent from the family resolves to a neighbor silently — freeze-time enumeration is the only way to distinguish a true style from a substituted one.

## markdown AST projection

- One immutable pipeline built once and shared: `MarkdownPipelineBuilder` + `Use*` rows → `Build()`; `Markdown.Parse(text, pipeline)` → `MarkdownDocument`.
- Extension rows are individually admitted (`UsePipeTables`, `UseTaskLists`, `UseAutoIdentifiers`, `UseYamlFrontMatter`, `UseEmphasisExtras`, `UseFootnotes`, `UseMathematics`) or as the `UseAdvancedExtensions` bundle — the admitted set is policy declared beside the projection.
- The pipeline is thread-safe to reuse; per-parse state rides `MarkdownParserContext` — one pipeline per surface class, one context per parse.
- Regex or line-split markdown handling beside the pipeline is the rejected form; the AST is the only document model, and a parallel node model duplicating it is the second rejected form.
- Projection law: AST → typed render vocabulary, never HTML — the fold maps node families onto typography-role rows and shaped-run requests.
- Block arms: `HeadingBlock.Level` selects a role row; `FencedCodeBlock`/`CodeBlock` select the monospace role with verbatim lines; `QuoteBlock`/`ListBlock`/`ListItemBlock` produce indent/marker rows; `ThematicBreakBlock` produces a rule row.
- `LinkInline` is one node family with an `IsImage` discriminant — link and image are two render rows from one arm, and the image row hands off to the asset lane by reference, never inline decode.
- `LinkReferenceDefinition` nodes are non-rendering: reference-style links resolve against them during parse, so the projection's link arm sees resolved targets and the definition nodes project to nothing — a definition arm that renders is double-handling.
- List projection discriminates ordered versus bullet at the block row and threads item depth through the cursor channel — markers are computed rows (depth, ordinal), never literal strings stored in the projection.
- `AutolinkInline` and explicit links converge on one link row: the projection normalizes both into the same typed target value, so link handling downstream has one shape regardless of authoring syntax.
- Inline emphasis is a role-delta algebra, not per-node font construction: `EmphasisInline.DelimiterCount` (1 = italic, 2 = bold; extras extensions widen the delimiter set) composes as a transform stack over the active role during the inline fold.
- The fold state is (active role, accumulated spans); entering/leaving emphasis pushes/pops a delta — constructing a font per emphasis node is the allocation defect the algebra deletes.
- `LiteralInline.Content` is a `StringSlice` — a zero-copy window into source text; the projection carries slices to the shaping boundary and materializes strings only there, keeping the document fold allocation-light.
- Traversal rows: `Descendants()` untyped and `Descendants<T>()` constrained to block or inline subtrees — typed traversal is the query form (all headings, all links).
- The render projection is a structural catamorphism over `ContainerBlock`/`LeafBlock` children, because `Descendants` flattens the nesting that indent and quote rendering need; `LeafBlock.Inline` roots the inline fold per leaf.
- Source fidelity rows: `Span`/`Line`/`Column` on every node plus `MarkdownDocument.LineStartIndexes`, with `PreciseSourceLocation` opt-in on the builder — the mapping that lets rendered-text hit-testing resolve back to source offsets.
- `TrackTrivia` preserves round-trip whitespace for editing surfaces and stays off for render-only pipelines — span fidelity and trivia are separate opt-ins with separate costs.
- The projection output is itself a frozen artifact: (role-tagged paragraph stream + shaped-run cache keys) — re-render after a theme swap re-resolves roles without re-parsing, and re-parse after edit re-projects without re-theming; the two invalidation axes stay independent.
- `Markdown.ToPlainText(text, pipeline)` is the secondary projection for search, accessibility strings, and previews — derived from the same pipeline, never a second parser; `Normalize` is the canonical-form projection for stored markdown.
- `Markdown.Convert(text, renderer)` with a custom renderer base is the streaming-output spelling — legitimate for text-sink targets; the typed render projection still rides the AST fold, because a renderer emits a stream while the fold produces a queryable artifact.
- `BlockParsers`/`InlineParsers` on the builder are the parser-surgery rows — removing a parser is the strongest rejection (the syntax becomes literal text), stronger than projecting its node to a fallback arm; use removal for syntax a surface must never honor.

## divergent — shaping-pipeline

- The maximal pipeline is one pure function with a cache seam: `(text, role, script, direction) → ShapedRun` where `ShapedRun` is (glyph ids, positions, width, cluster map, blob) — label draw, chart axis text, document paragraphs, and measurement all call the same function.
- The cache key embeds the role-catalog generation stamp, so a typography swap invalidates by generation comparison, not enumeration — shaped-run staleness is one integer test.
- Measurement collapses into shaping: measure-then-draw is shape-once-read-twice; any independent measurement path is a parallel rail that disagrees under features and fallback — deleted, with the shaped run as the single measurement authority for shaped roles.
- Wrapping is a fold over cluster boundaries: line breaking consumes the cluster map (break opportunities only at cluster starts — grapheme-safe by the role's cluster level) and the advance prefix-sums; no re-shape per candidate break.
- Incremental relayout re-shapes only affected runs: the run cache keys make "edit one line" cost one line's shaping, because untouched (text, role) pairs hit the cache.
- Streaming text shapes in windows with edge flags: chunk boundaries declare whether they are true paragraph edges, so progressive document rendering shapes arriving text correctly without waiting for the full paragraph — the windowed forms plus edge flags are the whole mechanism.
- Vertical roles are first-class rows: a vertical direction row plus the vertical extents/advances family — vertical scripts land as role-row data through the same pipeline, with zero new shaping code; the canvas-rotation spelling is foreclosed by the direction vocabulary.
- Failure-mode taxonomy: empty-buffer shape (zero glyphs, legal); `.notdef` glyphs surviving fallback (coverage receipt emitted, render proceeds with visible tofu plus a fact); scale-unset font (positions all zero — guarded by setting scale at font-cache admission, unreachable at shape time); post-shape buffer reuse without clear (content-type rejection at the pool boundary). Every taxon fails at a declared seam, none mid-draw.
- Pooled buffers are thread-affine by construction: the per-thread scratch buffer never crosses threads, fonts and faces are immutable-after-admission and shared freely — the mutability split (buffer scratch versus frozen font) is what makes concurrent shaping safe without locks.
- Shaped-run memory is accountable from counts: glyph count times the two parallel payload arrays plus the blob — cache budgets compute from receipts, and the eviction policy reads sizes it can trust because the run records carry their counts.
- Golden shaping baselines live per grid cell: each (role, script-class) cell carries a serialized glyph-stream baseline for exemplar text — a font upgrade diffs every cell's dump in one pass, separating shaping changes from raster changes before any pixel renders.
- Feature policy is auditable from role rows alone: which features any text can carry is the union of role feature sets — a feature audit (ligature posture, numeral style consistency) is a catalog query, never a code search.

## divergent — typography-roles-fallback

- The role catalog and the fallback chain unify as one two-axis grid: role × script-class, each cell a pinned typeface — cells are precomputed at catalog freeze by probing packaged faces against script exemplar sets.
- Runtime run segmentation becomes a grid lookup, never a registry probe; a new language requirement lands as one script-class column with cells, zero shaping-code change.
- Synthetic-style honesty: a cell carrying `Embolden`/`SkewX` is a receipt that the chain lacks a true face — font-licensing gaps surface as catalog data instead of silently faked styles.
- Role rows carry their own evidence contract: each role declares whether it may appear on evidence surfaces; an evidence-permitted role whose chain contains the host-registry tail row fails catalog freeze — determinism becomes a structural catalog property checked once.
- Vertical-metrics arbitration: when a chain mixes faces, line metrics come from the role's primary face only — per-run metrics produce ransom-note baselines; one metrics owner per role, fallback glyphs ride the primary baseline grid.
- The coverage matrix is auditable inventory: (role × script-class) cells with their probe results form a completeness report — which scripts a product can render is a query over the frozen grid, not an empirical discovery in production.
- Language joins the grid as a third axis where locl-sensitive locales are supported: (role, script-class, language) resolves the shaping font and feature set together — locale support is grid data, and the shaped-run cache key already carries the axis.
- Role rows version with the catalog generation: a role edit is a new generation, and every shaped artifact keyed on the old generation invalidates by comparison — role evolution never requires enumerating consumers.
- Shaping-exemption claims are audited at freeze: a role declared shaping-exempt is probed against its actual character domain (pure ASCII, no features, single face) — an exemption that fails the probe is a freeze rejection, so the cheap path cannot silently serve text it would mis-render.

## divergent — markdig-ast-projection

- The whole projection is one catamorphism with two state channels: (role stack, layout cursor) — block arms manage indent and spacing in the cursor channel, inline arms manage role deltas in the stack channel.
- Every markdown extension lands as one arm over its node type; an unhandled node type is a total-dispatch compile break, not a silent skip.
- Extension admission pairs with arm coverage as one checked policy: enabling a parser extension without its projection arm is the drift the pairing makes structural — the `Use*` set and the arm set are declared together and verified together.
- Documents project incrementally by block identity: top-level blocks re-project independently (block `Span` plus content hash as the memo key) — markdown's top-level blocks are independent by grammar, so the document fold is embarrassingly incremental.
- The HTML escape hatches (`HtmlBlock`, `HtmlInline`) are projection-policy rows: render-as-verbatim-code, drop-with-receipt, or reject-at-admission — pass-through is unrepresentable because the render vocabulary has no raw-markup row.
- Front-matter is a boundary fold: the YAML block is data for the surrounding system, never rendered — its arm projects to a typed metadata value and removes the block from the render stream, keeping document chrome out of the typography lane.
- Table extensions project to grid rows whose cells re-enter the inline fold — a table is layout data (column count, alignments, cell spans) wrapping ordinary inline projections, never a separate text pipeline.
- Task-list items project onto the icon catalog: the checked/unchecked marker is an icon-catalog key, not a literal glyph — markdown checkboxes inherit theme and density through the same token route as every other icon.
- Math and diagram extensions project to typed handoff rows: the projection carries the raw expression plus a target-lane discriminant — the typography lane renders the placeholder role and delegates the payload, never interpreting foreign grammars inline.
- Auto-identifier anchors are navigation data: heading ids project into the document artifact's outline table — in-document navigation reads the outline, and the render stream stays free of anchor markup.
