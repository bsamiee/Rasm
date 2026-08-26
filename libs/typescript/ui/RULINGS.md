# [TS_UI_RULINGS]

`typescript/ui` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `react`/`react-dom` HOLD on the canary channel — `useEffectEvent` and `<ViewTransition>` are canary-only members a stable bump silently drops.
- `prosemirror-*` is the ONE editor engine — its schema is DATA a roster derives every artifact from, and collab ships JSON Steps on the app wire.
- `lexical` rejected — collab reaches only through a second yjs CRDT beside core's algebra, and its class roster carries no codec-derivable grammar.
- `@tiptap/*` rejected on the license gate — comments, snapshots, tracked changes, and document conversion are payment-gated behind a token registry.
- `platejs`/`slate` rejected — Slate carries no schema, so the document grammar lands hand-authored with zero engine enforcement.
- `@milkdown/kit` and `remirror` rejected — each forwards ProseMirror behind an opinionated layer, so either admits the engine behind a forwarder.
- `prosemirror-schema-basic`/`prosemirror-example-setup` never land — a hardcoded node roster is seed data for the roster owner, never a dependency.
- `yjs`/`y-prosemirror` never land — core owns merge, causality, and presence, and a second CRDT engine forks the one algebra.
- `@xyflow/react` admitted — its interior `zustand` seals per provider, never consumer-imported; nodes, edges, viewport cross as controlled folds.
- `elkjs` admitted for compound nesting, ports, and routed edges as data — `elkjs/lib/elk-api` with a worker only; the bare entry bundles CJS.
- `@dagrejs/dagre` rejected — a ports-less, polyline-only subset of what `elk.layered` already answers.
- `tldraw` rejected — proprietary license with key enforcement and usage telemetry.
- `excalidraw` rejected — an application carrying its own document model, never an engine.
- `vis-timeline` rejected — peer-locked to legacy moment; timelines compose visx and d3 scales instead.
- `dhtmlx-gantt` rejected — the critical-path capability is PRO-gated, refusing the license gate.
- `embla-carousel` rejected — stable ships zero a11y and its plugin never left RC; scroll-snap composes the carousel; reopen at GA on loop wrap.
- `media-chrome` rejected — converged into the pre-GA Video.js successor; platform elements and RAC gauges compose the chrome; reopen at GA.
- `allotment` rejected — zero aria and zero keyboard handling in the shipped dist; a pointer-only splitter cannot ship.
- `react-resizable-panels` admitted — the N-panel constraint solver and the complete window-splitter pattern earn it over a `useMove` handroll.
- `@tailwindcss/typography` admitted — the prose plane's element-modifier and CSS-variable surface; the package owns only the token bridge.

## [02]-[SHAPE]

- Contract vocabularies materialize field-for-field — a clamp, remap, or local default forks the peer's semantics, and `viewer/mark` is the instance.
- Viewer animation reads ONE time authority, the rAF-fed `Clock` — a second clock un-reconciles the construction scrub mixer deltas derive from.
- Windowing a family `runtime:otel/vital` reads is admitted — one buffer serves every observer, so a second reader costs a callback and grades none.
- Every window bound and capture floor arrives on `Vital.Policy` from the root — a module constant assumes a consumer this package never meets.
- Environment read policy lands on the scene's `_ENV` fields at the read — a repeated key re-commits over carried handles, decoding nothing.
- Renderer-bound capability rides the acquisition record — one `KTX2Loader` per backend generation, since a captured bundle outlives the swap.
- Byte ports feeding `.buffer` to a decoder take `Uint8Array<ArrayBuffer>` — the bare spelling widens to `ArrayBufferLike`, which decoders refuse.
- Prefiltered domes bind their renderer while decoded sources do not — `Glb.Prefilter` re-derives the target at re-init, and a survivor is the leak.
- Environment consumption is producer-agnostic — `_sniff` reads container magic, never a filename or producer, so a producer-keyed arm has no place.
- Wire irradiance harmonics answer as a CPU read, never a scene light — the dome carries the diffuse term that `_harmonics` beside it doubles.
- Harmonic bands transcribe verbatim, the BASIS moving at the read — the `+Z`-up query normal un-rotates through the dome, so a permuted set forks.
- `viewer/scene` is the ONE prototype patcher — it pins accelerated raycast scoped and never restores it, since un-patching breaks a second viewport.
- Renderer-bound planes hold as a `ScopedRef` — its `set` acquires the successor before releasing the displaced one, so no lane sees a torn backend.
- Resource walks span the `Drawable` union, never one class — a `Mesh`-narrowed walk leaks a `Points` splat payload while the ledger drops its key.
- Residency eviction is ABSENCE from the successor's tile set — the manifest replaces whole, so a consumer-held row-state column names no producer.
- `Theme.Seed` crosses as composition data, not a wire family — each head expands one pigment set through its own contrast gate, keeping its pixels.
- Note politeness realizes at `_live` on the content element as the closed `status` | `alert` pair — the toast region carries no live semantics.
- Merge color scale DERIVES from `_paletteKeys`, the exact set `Theme.Palette.css` emits — a hand-listed hue names a key no `@theme` row carries.
- `run` stays a nullary intent over the surface's selection atom — pointer invocation SELECTS first, so a subject parameter forks the vocabulary.
- `Overlay.Command.run` returns a total Effect with its ports on the requirement channel — the row folds its own refusal that a void thunk re-opens.
- `Overlay.Command` declares `needs` and its refusal names the missing grants — a stored enabled bit freezes a verdict the live grants move past.
- `options.atoms` binds an app-owned slice over an adapter on the fold's cell — a `state`/`on<Slice>Change` pair beside it doubles the writer.
- Registry keys stay domain-blind — a consumer's order crosses as a rank VALUE on its own column, so a consumer-named key imports it downward.
- Widened materials re-stamp `{ STANDARD, PHYSICAL }` over `MeshStandardMaterial.prototype.copy` — their `.copy` throws, `setValues` clobbers `uuid`.
- Textures tag only the two working spaces `ColorManagement` registers — the WebGL upload hard-errors on others, so conversion stays producer-side.
- `Texture.premultiplyAlpha` binds DOM sources alone — ArrayBufferView uploads ignore it, so associated payloads ride `Material.premultipliedAlpha`.
- Generated `PlaneRef.artifact` carries blob identity and extent; `Set.key` identifies the document and `PlaneRef.file` stays metadata.
- Appearance seating pairs each generated `Set` with its carrier appearance key — baked sets verify that key and environment kinds never seat.
- Splat order is consumer-owned per view — the producer wire carries no ordering key, so back-to-front is a camera-epoch fold and never decode order.
- Canvas keeps React Flow's own recognizers — the graph mirrors through one adapter atom; a second recognizer over its d3-zoom double-binds.
- Persisted grain keys share ONE mint, `rasm.ui.<domain>.<grain>` — generation seals the VALUE, so a stale parcel refuses on content, never on key.
- Quarantine is the ONE unknown-shape posture — payloads hold as residue, live collab refuses on generation, quarantined atoms shift position math.
- Foreign packages own their version fields — the engine stamps and reads its own, so the repo transcribes that field verbatim and mints none.
- Key chords spell `Control` or `Mod` — react-aria's parser holds no `ctrl` token, so `Ctrl` parses as a KEY and drops the modifier silently.
- Multi-panel elements persist at the ELEMENT grain — layout, active panel, and cross-filter live in the workspace token a panel config patches.
- `AppUiSurfaceProgram` seats one app input: partition, unique-key control tree, and exact structured-variable layout closure; leaves stay support.

## [03]-[COLLAPSE]

- `Hook` owns the point roster, the `consult` selector, and the adopted-source pump alone — channels, arbiters, and the breach ring are `Tap.Bus`'s.
- Semantic tone homes at the `_TONES` authority as ONE closed vocabulary — every `_tone` table keys that set, so a per-surface member is the fork.
- Surfaces with two closed lifecycle axes give TONE to ONE — two tone columns resolve two palettes; `viewer/mark`'s status/priority split proves it.

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)
