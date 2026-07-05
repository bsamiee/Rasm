# [AUDIT_UI] — line-by-line doctrine audit of `libs/typescript/ui`

Scope: 13 design pages (`system/{token,act,atom,intl,primitive}`, `view/{form,table,overlay}`, `viewer/{scene,geo,mark,panel,probe}`). Findings only. Doctrine floor = `docs/stacks/typescript/` composed in full. API claims spot-verified against `libs/typescript/ui/.api` + `libs/typescript/.api`.

## [0]-[DOCTRINE_LAW_SET]

The sixteen `[02]-[DOCTRINE]` laws, stated by name, each with its bearing on this folder.

FLOW:
1. `EXPRESSION_SPINE` — domain logic is expression-shaped on `Effect<A,E,R>`; dependence sequences, independence accumulates; statements only in measured kernels/platform seams. Bears on every viewer fold (`_graft`, `_solve`, `_submit`) where imperative statements appear without the mark.
2. `BOUNDARY_ADMISSION` — raw material decodes exactly once at the edge; interior never re-validates, never sees `null`/`undefined`/sentinels. Bears on the wire-vocabulary consumption (`Feed.Document`, `BcfViewpoint`, `ControlIntent`, `Claim`) — the UI correctly consumes decoded, never re-mints.
3. `CAPABILITY_CHANNEL` — dependency travels the requirement channel as a Tag, satisfied only by the Layer graph. Bears on `GlbViewport` port (correct) and `Store.make` runtime mint (correct); no module-level live singleton except the sanctioned FFI caches.

SHAPE:
4. `SCHEMA_AUTHORITY` — Schema owner is the single shape authority; a hand interface/DTO beside it is the defect. Bears on `_Color`, message-catalog family, fault classes — correctly Schema/Data; the near-total absence of ad-hoc DTOs is a strength (wire shapes live in core).
5. `SHAPE_BUDGET` — one deep owner ≈ one-fifth of naive shapes; variants are tags in one closed family. Bears on the policy vocabularies (`_reasons`, `_tone`, `_rows`) — mostly compliant; the assembled-owner annotation form is the recurring miss (§Folder-1).
6. `DEEP_SURFACES` — one/two exports, `_`-interior, one terminal EXPORTS block, one name serves value+type. Bears on token (3 exports), scene (5 exports), geo (3) — surface-count pressure (§Folder-4).
7. `MODAL_ARITY` — one entrypoint discriminates on input value; no suffix twins, no boolean knobs. Bears on `Store.make` memoMap branch (§atom-1), `Gesture.useCanvas` clamp split (§act-2).
8. `ANTICIPATORY_COLLAPSE` — the owner is shaped for the family it absorbs; the next feature is one row. Bears on every closed op/reason/tone vocabulary — well realized (growth-as-row is stated throughout).

DERIVATION:
9. `VOCABULARY_VALUES` — one `as const` table, contract-checked, secondary surfaces derive; a hand union/enum is the defect. Well adhered (keyof-typeof hubs everywhere); the export-gate contract form is inconsistent (§Folder-1).
10. `DERIVED_TYPES` — types compute from one value/type anchor. Adhered (namespace hubs derive `Kind`/`Row`); inconsistent owner-annotation derivation (§Folder-1).
11. `SEMANTIC_NAMING` — canonical term, correct grammatical role, ≤3 words. Adhered.

MATERIAL:
12. `LIBRARY_DEPTH` — the Effect ecosystem is the standard library; use the deepest primitive. Bears on the ~14 admitted-but-unexploited packages (§Folder-2) and the JS-`Array`/`.forEach` residue at viewer seams (§scene/§geo).
13. `INLINE_COMPOSITION` — wrapping/policy attaches at the owner declaration. Adhered for atom combinators; the unmarked cast/kernel sites break recoverability (§Folder-3).

INTEGRATION:
14. `ROOT_REBUILD` — new capability woven as if always there; no shims. Adhered.
15. `ONE_HOP_RESOLUTION` — a name resolves in one hop; no forwarding shells. Adhered (`cn`, `Form.standard`).
16. `COMPOSED_IMPLEMENTATION` — features compose the page owners; components project through the atom bridge, never run effects, never own Layers. Strongly adhered (atom bridge is the single seam); the `motion`/charts capability gap is the "need with no composed spelling" (§Folder-2).

## [1]-[SYSTEM/TOKEN]

- `[2]` fence 219-235 — `Theme` owner annotated inline `{ readonly Color: typeof _Color; readonly kinds: typeof _kinds; … }` (7 members). Doctrine (`language.md#DEEP_MODULE_SITE`, `derivation.md#VOCABULARY_TABLE_SITE`) prescribes a `Theme.Shape` type in the merged `declare namespace`, flattened through `Types.Simplify<typeof _rows & {…}>`, and `const Theme: Theme.Shape`. The inline object-type is the corpus-wide restatement pattern; `act.md#Motion` already uses the correct `typeof _rows & {…}` intersection — the inconsistency is the finding. Collapse: home the shape as `Theme.Shape` in the namespace.
- `[1]` clusters — token exports THREE owners (`cn`, `Scale`, `Theme`) from one module against `DEEP_SURFACES`' one/two target. `Scale.css` already routes every row through `Theme.css` (fence 178-185), so `Scale` is a sub-plane of the color/emission authority. Collapse candidate: fold `Scale` into `Theme` (`Theme.Scale` static + `Theme.css`), leaving two exports (`cn`, `Theme`).
- `[2]` fence 70-73 — `_linear` reads `converted.coords[0] ?? 0` (×3). `?? 0` silently fabricates a zero coordinate under `noUncheckedIndexedAccess`. `language.md#STRICTNESS_CONSEQUENCE`: a provably-present index asserts in a marked kernel with the bound as evidence, never a fabricated fallback. `srgb-linear` coords are a fixed 3-tuple — mark the read or type `coords` as `[number,number,number]`. Same pattern at geo `_anchor`/`_rasterTiles` (§geo).
- `[2]` fence 44-53 — `_Color` `Schema.transformOrFail` is correct (non-throwing `tryColor`, `ParseResult` rail). `[R]`: strong adherence; the color authority feeding both CSS and `Theme.linear` render space is exactly the composed-owner form.

## [2]-[SYSTEM/ACT]

- `[4]` fence 106-111 — `onWheel` clamps zoom by inline `Math.min(bounds.max, Math.max(bounds.min, …))` while `onPinch` clamps via the recognizer's `scaleBounds: bounds` (fence 118). The card (`[4]` law) asserts "one bounds policy clamps every zoom write path" but the fence realizes two distinct clamp mechanisms against one bounds row. Collapse: extract `_clampZoom(bounds, z)` shared by both arms so the asserted single policy is structural, not prose.
- `[4]` fence 102-111 — each handler calls `options.read()` 1-2× (`onWheel` reads twice: spread + `.zoom`). Minor recomputation; read once into a const.
- `[6]` fence 204-210 — `Transition.run` uses `Effect.tryPromise({ try: …finished, catch: () => undefined }).pipe(Effect.ignore)`. `catch: () => undefined` yields `Effect<void, undefined>` — an `undefined` error channel. `rails-and-effects.md#CARRIER_EMBEDDING`: the bare `tryPromise(() => …)` mints `UnknownException`; `.pipe(Effect.ignore)` then discards it truthfully. Simplify to the bare form.
- `[R]`: the discrete/continuous ownership split, `mergeProps` fold, and `<Activity>`-parked loop are well-composed. `Motion` (169-172) uses the correct `typeof _rows & {…}` annotation — the model the other owners should follow.
- `[NOTE]` `motion` (the JS animation package, README `[MOTION_GESTURE]`) is admitted with a `.api` catalog but imported nowhere — the page deliberately chose CSS (`tw-animate-css`) + View Transitions. Either build the JS-motion lane the campaign's "advanced beautiful web apps" (law 9) implies, or cull `motion` (§Folder-2).

## [3]-[SYSTEM/ATOM]

- `[2]` fence 49-52 — `Store.make` branches on `options.memoMap === undefined ? Atom.runtime(layer) : Atom.context({memoMap})(layer)`. `surfaces-and-dispatch.md#FORM_CHOOSER[11]`: a boolean/presence branch selecting two bodies collapses to one derived body. The catalog ships `Atom.defaultMemoMap` (verified, `effect-atom-atom.md` row [07]); collapse to `Atom.context({ memoMap: options.memoMap ?? Atom.defaultMemoMap })(options.layer)` — one construction path, the branch deleted.
- `[7]` fence 207-226 — `History` owner annotated inline `{ readonly Op: typeof _Op; readonly make: <A>… }`. Same as §token-1: route to `History.Shape` in the namespace. The `_step` fold + `Data.taggedEnum` `_Op` are correct.
- `[7]` — `History` is a hand-rolled undo/redo transition owner over `Atom.writable`. Campaign law 4 (machines at research-paper depth) wants `@effect/experimental` `Machine.makeSerializable` (snapshot/restore, Subscribable-wired) where state outlives a traversal and answers requests; History answers undo/redo requests over persistent `past/present/future`. Defensible as a pure fold, but it is the strongest TS-owned machine candidate in the folder (§Folder-5).
- `[R]`: `LIVE_BRIDGE` (subscriptionRef/subscribable/pull/toStreamResult), `SELECTOR_RAIL` (map/mapResult/transform/family/debounce), `REMOTE_BINDING` (AtomHttpApi.Tag/AtomRpc.Tag) all verified against `effect-atom-atom.md`. Strong composed-implementation adherence — the atom bridge is the one graph seam per law 16.

## [4]-[SYSTEM/INTL]

- `[3]` fence 45-50 — `_native` cache does `_held.get(key) ?? _NATIVE[kind](locale)`, `_held.set(key, instance)`, `return instance as ReturnType<…>`. The card claims the exemption ("this card carries the exemption / the marked cast") but the FENCE carries no `// BOUNDARY ADAPTER` on its first line. `language.md#KERNEL_EXEMPTION_SITE`: the mark rides the first line so the exemption is recoverable from the declaration; `as` + Map mutation are legal only under it. Fix: mark `_native`.
- `[5]` fence 156-176 — `MessageSpec` family (`_Text`/`_Plural`/`_Select` as `Schema.TaggedStruct` in a `Schema.Union`) is correct (pure-data cases → TaggedStruct per `shapes.md#OWNER_SELECTION`). `[R]`.
- `[3]` fence 111-139 / `[6]` fence 235-249 — `Format` and `Message` owners inline-annotated (§token-1). `_collate` lifting `Intl.Collator` into `Order.make` via `Number.sign` is exemplary `values.md#ORDER_COMPOSITION`. `[R]`.
- `[R]`: zero-i18n-package plane, single epoch crossing (`Format.instant`), threshold-fold relative time, `Array.unfold` fallback chain — dense and doctrine-clean.

## [5]-[SYSTEM/PRIMITIVE]

- `[5]` fence 98-99 — `_fallbackRender` folds `fold(error as E, …)`. Card claims the cast exemption but the fence is an un-marked curried arrow, not a `// BOUNDARY ADAPTER` kernel. Same defect class as §intl-1: the React type-erasure `error as E` seam must carry the mark. Fix: mark the boundary.
- `[3]` `ROSTER_LAW` — pure prose law with no realized fence (the roster is RAC's; the page legislates composition). Defensible, but the realized `Primitive` owner (styled/recipes/toasts/boundary/sanitize) is thinner than the page's described scope. Acceptable given RAC owns the mechanics.
- `[6]` fence 113 — `DOMPurify.setConfig({ USE_PROFILES: { html: true }, FORBID_ATTR: ["style"] })` at module init is a side-effect statement at declaration top-level. `language.md`: load-time effects belong at the boot edge; a module-init config write in a domain module is `LOAD_TIME_EXECUTION` smell. Since the sanitize policy is a genuine module-global FFI seam, this is borderline; note it, prefer routing the config into the `sanitize` first-call or a boot row.
- `[R]`: `styled` (cva × composeRenderProps × cn), `VariantProps` lift, one sanitize gate — clean single-owner spine.

## [6]-[VIEW/FORM]

- `[4]` fence 79-86 — `_submit` folds refusal by hand-destructuring the Cause: `cause._tag === "Fail" && cause.error._tag === "DraftRefused"`. `rails-and-effects.md#FOLDED_OUTCOME`: the Cause tree is read through `Cause.failureOption`, never by probing `._tag === "Fail"`. As written, a `Die`, `Interrupt`, or `Sequential` cause falls through to the generic `{"": ["<submit-failed>"]}`, erasing evidence. Fix: `Exit.match({ onFailure: (cause) => Option.match(Cause.failureOption(cause), …) })` then match the tagged `DraftRefused`.
- `[4]` fence 74-88 — `startTransition(async () => { const outcome = await write(draft); … })` uses `await` in the action body. Legal at the React-19 form-action seam (`useFormStatus`/`requestFormReset` are Promise-shaped), but the exemption is unstated. Note the platform-forced seam; the deeper issue is the Cause probe above.
- `[5]` fence 94 — `useAtomRefProp(ref, key)` is cited but not confirmed in `effect-atom-atom-react.md` (catalog lists `AtomRef` fine-grained refs generically). Verify against shipped declarations or it is a phantom to delete.
- `[2]` fence 32-41 — `_errors` uses `ParseResult.ArrayFormatter.formatErrorSync` at the field-error reporting edge — compliant with `boundaries.md#ADMISSION` (terminal reporting edge only). `[R]`.

## [7]-[VIEW/TABLE]

- `[1]` fence 60-72 — `Grid.Slice` is a plain type alias over 11 foreign TanStack `*State` types held in one atom. The `[2]` law claims persistence "by backing the atom with `Atom.kvs` and its owning schema," but NO Schema owner for the slice is declared or derivable (the members are foreign wire-ish types). Either a `Schema` projecting the persistable subset (order/sizing/visibility) is missing, or the persistence claim is aspirational. Close: declare the persisted-subset Schema the `Atom.kvs` row needs.
- `[2]` fence 98-108 — `_banded` indexes `_CELL[column.kind]`. `_CELL` maps `bool/int/real/text/stamp`; if `Feed.Document` column `kind` carries members beyond those five, the access is a partial index returning `undefined` under `noUncheckedIndexedAccess`, silently producing `meta.cell: undefined`. Verify `column.kind` is exactly `keyof typeof _CELL`; otherwise lift to `Option` or make `_CELL` total over the band vocabulary.
- `[6]` fence 140-143 — `_range` uses `Array.sort(Array.dedupe([...pinned, ...defaultRangeExtractor(range)]), Order.number)` — clean. `[R]`.
- `[1]` fence 144-154 — `Grid` owner inline-annotated (§token-1).
- `[R]`: one-atom TableState, `functionalUpdate` fold, model-import discipline, selection-identity-by-`GlobalId` shared with the mark plane — strong single-owner realization.

## [8]-[VIEW/OVERLAY]

- `[1]`/`[3]`/`[4]`/`[5]` — the page is largely composition LAW with `declare const`/`declare namespace` fences; the realized `Overlay` owner exports only `middleware` + `virtual` (fence 92-98). Defensible (floating-ui/vaul/cmdk own mechanics), but the realized surface is the thinnest owner-to-page-scope ratio in the folder against the campaign's "deep owner, internalized intelligence" bar.
- `[4]` fence 77 — `declare const _specs: Record<string, Overlay.Command>` is app-side; the `Overlay.Command` vocabulary (fence 68-73) is a good closed row (`icon`/`label`/`keywords`/`run`). `run: () => void` is a synchronous callback — acceptable as the app-wired intent write.
- `[2]` fence 33-42 — `_middleware` returns the `offset→flip→shift→size` array; the `size.apply` DOM write is correctly named the platform-forced seam. `[R]`.
- `[NOTE]` the presence cohort (`[5]`) anchors via floating-ui `VirtualElement` — clean. No machine for overlay dismissal coordination (`FloatingTree`) though nested-dismissal is a genuine statechart; low priority.

## [9]-[VIEWER/SCENE]

- `[5]` fence 191, 209-210, 227, 236 — **`const roster: Array<AnimationMixer> = []` is cross-fiber shared mutable state.** Two concurrent `Stream.runForEach` arms (insert `roster.push`, evict `roster.splice`) run under `Effect.all([insert, evict], { concurrency: 2 })` (fence 235), and the frame loop reads `roster.forEach` (fence 236, 250). `concurrency.md#CELL_SELECT` names this exactly: "a `let`/array captured by concurrent closures" and "module-level mutable state" are the rejected forms; shared mutation is a cell with an owner (`Ref`/`SynchronizedRef`). The `held` ledger correctly uses `Ref`, but `roster` duplicates state already carried in `Glb.Graft.mixer` (each graft's `Option<AnimationMixer>`). Fix (split-brain + race, one move): delete `roster`; derive live mixers from `Ref.get(held)` in `advance` (fold the `HashMap` values' `Option<AnimationMixer>`), or hold mixers in a `Ref<HashSet>` written by both arms atomically. Highest-leverage correctness finding in the folder.
- `[5]` fence 171-182 — `_dispose` traverses with `child instanceof Mesh`, `Array.isArray`, `.forEach`, `Object.values` and no `// BOUNDARY ADAPTER` mark. Card calls three's `traverse` the platform-forced seam; `language.md#KERNEL_EXEMPTION_SITE` requires the mark on the first line. Fix: mark `_dispose`.
- `[5]` fence 199-206 — `gltf.animations.reduce((bound, clip) => { bound.clipAction(clip)…play(); return bound }, new AnimationMixer(…))` — statement-bearing reduce over a three FFI object, unmarked; same as `_dispose`.
- `[4]` fence 113-122 — `_lit` builds lights with `sun.position.set(...)` statements and `rows.reduce((held, light) => held.add(light), root)` — three FFI, unmarked statement seam.
- `[8]`/`[6]` — `Instanced` and the collapse rows (`_instanced`/`_batched`/`_merged`) use `.forEach`/`.reduce` over three builders (FFI, acceptable) but the whole-object annotation `Glb` owner (fence 431-459) inlines THIRTEEN `typeof _x` members — the most egregious §token-1 restatement; route to `Glb.Shape`.
- `[3]`/`[9]` — five exports (`Glb`, `GlbFault`, `GlbViewport`, `Instanced`, `Pbr`) — the widest surface in the folder against `DEEP_SURFACES` (§Folder-4). Genuinely distinct owners (port Tag/fault/backend/instancing/appearance); note as surface pressure, not a hard defect.
- `[R]`: `GlbFault` reason-vocabulary + policy-table getter (fence 56-78) is textbook `rails-and-effects.md#FAULT_ARCHITECTURE`; the `GlbViewport` Tag port (fence 37-40) is textbook `CAPABILITY_CHANNEL`; `Pbr.bind` field-for-field carriage with the single `Theme.linear` color seam is exemplary anti-drift discipline.

## [10]-[VIEWER/GEO]

- `[3]` fence 100-118 — `_payload`/`_drive` build the maplibre camera options; `LookAt` composes `map.calculateCameraOptionsFromTo(...)` spread into `easeTo` — the prior hand-rolled tangent-plane solve was correctly collapsed into the maplibre solver (verified against `maplibre-gl.md` Camera row / prior redteam). `[R]`.
- `[4]` fence 144-156 — `_anchor` reads `projected[0] ?? 0` / `projected[1] ?? 0`. Same `?? 0` fabrication as §token-3; a projected point is a 2-tuple — mark or type the read.
- `[5]` fence 240-248 — `_rasterTiles` `renderSubLayers` reads `props.tile.boundingBox[0][0] ?? 0` (×4). Nested `?? 0` on a proven bounding-box tuple — `noUncheckedIndexedAccess` handled by fabrication rather than a marked kernel or a typed tuple. Fix: type `boundingBox` as `[[number,number],[number,number]]` at the seam or mark.
- `[5]` fence 197-200 — `GeoFault` carries a 2-reason vocabulary with no policy table (unlike `GlbFault`). For a leaf decode/crs fault with no recovery this is acceptable, but note the asymmetry with the scene fault family.
- `[8]` fence 333-361 — `Geo` owner inline-annotated with 13 `typeof _x` members (§token-1); route to `Geo.Shape`.
- `[R]`: one WebGL context (maplibre + interleaved MapboxOverlay), atom-derived layer tree at one `setProps` sink, the DGGS cell family as one scheme-keyed table, the eight-capability extension roster, turf-as-planar-peer with WKB behind the core port — dense and correctly bounded.

## [11]-[VIEWER/MARK]

- `[5]` fence 144-157 / `[6]` fence 180-200 — the eye→target vector add is spelled component-wise (`position[i] + direction[i]`, ×3) in BOTH `_pin` and `_restore`. Duplication across two sites; extract `_target(camera): readonly [number,number,number]`. Minor.
- `[2]` fence 46-53 — `Selection.step` `$match` over `Data.taggedEnum` `Selection.Op`, each arm one `HashSet` combinator — exemplary `surfaces-and-dispatch.md#GENERATED_SURFACE`. `[R]`.
- `[3]` fence 71-84 — `_marquee` uses `deck.pickObjectsAsync` (WebGPU-safe async), `Array.getSomes(Array.map(hits, _fromInfo))` — clean absence harvest. `[R]`.
- `[1]`/`[7]` — `Selection` and `Mark` owners inline-annotated (§token-1).
- `[R]`: single `HashSet<GlobalId>` selection atom, closed op vocabulary, every pick pipeline resolving through the one `_decode` before the fold, undo via the History ride, restore-as-receipt (`{requested,resolved,missing}`) — strong single-truth discipline.

## [12]-[VIEWER/PANEL]

- `[5]` fence 168-204 — **`_solve` mischaracterizes a stateful resource as a kernel.** The `Exemption` line claims the interior `Map` is the kernel's draft, but `solver` (kiwi `Solver`) and `cells` (name→`Variable` Map) are captured by the returned `Panel.Solved.suggest`/`read` closures (fence 190-203) — the mutable draft ESCAPES as live state, which `language.md#KERNEL_EXEMPTION_SITE` names the rejected form ("an accumulator escaping as live state; leaks no mutable reference"). Kiwi's incremental `suggestValue` genuinely requires the live solver to persist — so this is a RESOURCE (`rails-and-effects.md#RESOURCE_BRACKET`) whose lifetime is the returned `Solved`, not a compute kernel. Concurrent `suggest` calls race the shared `solver`/`cells` with no serialization. Fix: model the live solver as a scoped resource with its mutation serialized (`SynchronizedRef`/one-fiber ownership), and mark the construction seam. Second-highest correctness finding.
- `[5]` fence 172-176 — `named` is a mutation-closure (`cells.get ?? new Variable; cells.set; return`) — the same shared-`cells` mutation; folds into the resource fix above.
- `[2]` fence 54-84 — `_fold` uses `HashMap.modifyAt` + `Match.valueTags` over the wire triple — textbook `values.md#KEYED_FOLD` + `surfaces-and-dispatch.md`. `[R]`.
- `[4]` fence 117-125 — `Panel.route` = `Match.type<ControlIntent>().pipe(Match.tagsExhaustive(sinks))` with `Sinks` a mapped record over `ControlIntent["_tag"]` — exemplary derived exhaustive dispatch. `[R]`.
- `[5]` fence 206-228 — `Panel` owner inline-annotated with 10 `typeof _x` members (§token-1).
- `[R]`: wire-carriage discipline (no clamp/remap/synthesize), the four-axis determinism law, receipt-reconciled optimistic round trip distinguished from `Atom.optimistic` — dense and correct apart from the solver-resource modeling.

## [13]-[VIEWER/PROBE]

- `[2]` fence 41-59 — `_stepped`/`_rows` fold `DeckMetrics` into a raw-accumulator seed with means projected at read — exemplary `computation.md#SEED_FOLD` (raw associative accumulators, derived stats at read). `[R]`.
- `[4]` fence 111-129 — `_board` label-keyed join via `HashMap.fromIterable` + `HashSet` + `Array.filterMap`/`appendAll`, one-sided rows preserved, unit-mismatch → no delta — clean. `[R]`.
- `[5]` fence 159-176 — `_capture` async readback + `Equal.equals` structural verdict — clean, no forced redraws (passive-probe law honored).
- `[5]` fence 196-214 — `Probe` owner inline-annotated (§token-1).
- `[R]`: strongest doctrine-clean page in the folder — passive capture, structural comparison, never-a-gate law, bounded verdict history.

## [FOLDER_VERDICT] — structural weaknesses ranked by leverage

1. **[UNEXPLOITED ADMITTED CAPABILITY — ~14 packages, whole capability class missing]** `LIBRARY_DEPTH`/`COMPOSED_IMPLEMENTATION` name an admitted capability no fence exploits a defect. README `[GRID_CHARTS]`+`[SPATIAL]`+`[MOTION_GESTURE]` admit — with full `.api` catalogs — `@perspective-dev/{client,viewer,viewer-datagrid,viewer-charts}`, `uplot`, `@observablehq/plot`, `d3`, `@visx/{axis,group,responsive,scale,shape}`, `typegpu`, and `motion`; ZERO of the 13 pages import any of them. `table.md` realizes only the TanStack grid — no charting/dataviz owner exists, though the cluster is literally named `GRID_CHARTS` and the campaign (law 9) demands the most advanced beautiful web apps. Resolution: author the missing `view/chart.ts` (or `viewer/analytics.ts`) dataviz owner realizing perspective/plot/uplot/visx as one parameterized owner AND the `motion`/`typegpu` lanes, or cull the unrealized packages from the roster and README. Highest leverage — a quarter of the roster is dead weight.

2. **[CROSS-FIBER / RESOURCE MUTATION — two genuine correctness defects]** scene `roster` (§scene-1, shared mutable `Array` raced by two concurrent stream arms, duplicating the `held` ledger) and panel `_solve` (§panel-1, live kiwi solver mischaracterized as a kernel, draft escaping into unsynchronized `suggest` closures). Both are `concurrency.md`/`language.md` rejected forms with real race exposure. Fix `roster` by deriving mixers from the `Ref<HashMap>` ledger; fix the solver by modeling it as a scoped serialized resource. Highest correctness leverage.

3. **[UNMARKED KERNEL / CAST SEAMS — recurring across the corpus]** intl `_native` (as-cast + Map mutation), primitive `_fallbackRender` (`error as E`), scene `_dispose`/`_lit`/`gltf.animations.reduce` (three-FFI statement seams), panel `_solve`, plus the `?? 0` index-fabrication at token `_linear`, geo `_anchor`/`_rasterTiles`. Every one is a legitimate platform seam, but none carries the `// BOUNDARY ADAPTER` mark `language.md#KERNEL_EXEMPTION_SITE` requires on the first line, and the `?? 0` sites fabricate values where a typed tuple or a marked `!` is the doctrine form. Systematic fix: mark every platform seam; type the proven-index tuples. Medium leverage, corpus-wide.

4. **[ASSEMBLED-OWNER ANNOTATION RESTATEMENT — pervasive consistency defect]** Nearly every owner (`Theme`, `Scale`, `Format`, `Message`, `Primitive`, `Gesture`, `Store`, `History`, `Form`, `Grid`, `Overlay`, `Selection`, `Mark`, `Panel`, `Probe`, `Glb`(×13 members), `Geo`(×13), `Pbr`, `Instanced`, `Camera`) is annotated with an inline `{ readonly member: typeof _member; … }` object type instead of the canonical `Owner.Shape` type declared in the merged `declare namespace` and flattened through `Types.Simplify` (`derivation.md#VOCABULARY_TABLE_SITE`, `language.md#DEEP_MODULE_SITE`, exemplars `Tier`/`Transit`/`Meter`). `act.md#Motion` alone uses the correct `typeof _rows & {…}` intersection — the inconsistency is the tell. Route every owner's annotation into its namespace `Shape`. Medium leverage; high consistency payoff. Attendant surface-count pressure: token (3 exports), scene (5), geo (3) against `DEEP_SURFACES`' one/two — fold `Scale`→`Theme`; scene's five are genuinely distinct owners.

5. **[MACHINE ABSENCE — campaign law 4 partially unmet]** Zero `@effect/experimental` `Machine` usage despite the campaign's research-paper-depth statechart/serializable-actor mandate. Most stateful transitions here are C#-authored (panel binding phase, BCF topic lifecycle) and correctly rendered as folds+tone-tables — so the gap is legitimately absorbed by the wire-authority boundary. The genuine TS-owned candidate is `History` (undo/redo actor over persistent `past/present/future` answering requests) and a potential viewer session/camera machine. Lower leverage than 1-4 because the fold forms are defensible, but the campaign explicitly calls for the machine owner where TS owns transitions.

6. **[EVIDENCE-ERASING CAUSE PROBE — form submit]** form `_submit` hand-destructures `cause._tag === "Fail" && cause.error._tag` instead of `Cause.failureOption`, dropping `Die`/`Interrupt`/`Sequential` evidence to a generic message (§form-1). Isolated but a real correctness/evidence loss on the submit rail.

Phantoms/verification residue: `useAtomRefProp` (form `[5]`) and `useAtomSubscribe` (token `[5]`) are cited but not confirmed in `effect-atom-atom-react.md`; verify against shipped declarations or delete. `_CELL[column.kind]` totality (table `[2]`) needs the band `kind` vocabulary confirmed as `keyof typeof _CELL`.
