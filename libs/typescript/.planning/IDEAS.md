# [TYPESCRIPT_BRANCH_IDEAS]

The cross-package concert for the TypeScript branch — the higher-order ideas that couple two or more TS folders, distilled from the folder ideas, not the folder-local concepts. Each open idea is a card: a bracketed slug leader plus the capability, what it unlocks, and the cross-folder coupling or modern technique it draws on. A cross-language idea lives in `libs/.planning/IDEAS.md`, never here. A finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition.

## [1]-[OPEN]

[ONE_FOLD_ONE_BINDING]: the read path is one fold algebra in `projection` and one reactive spine in `ui`, with no view-state layer between them.
- `projection` derives every composite read model — a health-gated runtime view, an evidence-correlated-by-`ContentKey` timeline, a watermark-annotated window cell — as one declared `Subscribable` through the `keyedFold` algebra over base change streams, and `ui` binds each one directly through the one `AtomBinding` over `Atom.subscriptionRef`/`pull`.
- Unlocks composite reactive views owned at the right altitude (`projection`, not `ui`), one subscription per derived concept the atom bridge consumes verbatim, and a new feed that lands as a fold row rather than a parallel recombination in `ui` — the named branch prohibition the coupling forbids on both sides.
- Couples the `projection` reactive-query-surface idea to the `ui` atom-native-binding-collapse idea; draws on Effect `SubscriptionRef.changes`/`Subscribable` and the `@effect-atom/atom` native constructors so neither folder reinvents the other's plumbing.

[ONE_EGRESS_VERB_FACE]: every mutation in the branch crosses the single `interchange` `CommandGateway`, gated by the `projection` availability fold, from whatever surface raises it.
- The gateway reads the `projection` `AvailabilityStore` `isEnabled` fold at dial time so a disabled command never fires; the `ui` surfaces, the `platform` deep-link router through the `IntentRegistry`, and the `platform` background-sync redial drain all leave through that one verb face, never a transport directly.
- Unlocks a single observable egress where availability, intent resolution, and offline-drain convergence share one surface, so a new command is one gateway verb and a new caller composes the existing face.
- Couples `interchange` quarantine/gateway to `projection` availability and to the `platform` offline-cache redial drain and routing intent ingress; draws on the dial-time read-gate pattern with the gateway resident in the transport-owning folder so no `@connectrpc/*` reaches the fold tier.

[CONTENT_ADDRESSED_OFF_THREAD]: the content-addressed artifact blob is reassembled and digested off the main thread once, then keyed identically everywhere it is read.
- `interchange` lifts the artifact frame stream to a transferable `ReadableStream` piped to the `platform` `DecodeWorkerPool`, runs the Crc32-verify, stitch, and content-key digest with BYOB zero-copy reads off the main thread, and the resulting `ContentKey`-addressed blob is the one the `ui` GLB viewport renders and the `projection` evidence fold correlates on.
- Unlocks a responsive main thread during large GLB delivery, an explicit zero-copy worker seam a future WebTransport raw-byte leg feeds as one more transport case, and one digest the offline last-good store, the evidence correlation, and the mesh viewer all share — a second hash mint anywhere is the named cross-language drift defect.
- Couples `interchange` transferable-stream reassembly and content-key parity to the `platform` decode worker pool and the `ui` viewport leaf and `projection` evidence correlation; draws on transferable streams, BYOB reads, and the one C#-owned `XxHash128` seed reproduced bit-identically.

[HONEST_CLOCK_UNCERTAINTY]: the HLC skew band is a load-bearing read model from the convergence fold through to the rendered evidence timeline, never a render-only leaf.
- `projection` promotes `SkewBand` from a render-only projection into an ordering input — two rows closer in midpoint than the sum of their `radiusMs` are marked concurrent-uncertain rather than forced into a spurious HLC total order — and the `ui` observation routes render the "causally-ambiguous within +/-N ms" band on the evidence timeline and the conflict adjudication.
- Unlocks honest uncertainty about event ordering the desktop AppUi structurally cannot produce, a confidence interval that is load-bearing in both convergence adjudication and the rendered timeline, and a distributed-systems-correct read model surfaced as product UI.
- Couples the `projection` skew-aware-ordering and convergence ideas to the `ui` observation evidence-timeline route; draws on HLC clock-skew confidence-interval surfacing and the existing `SkewBand` `{ midpointMs, radiusMs }` shape.

[NATIVE_TRANSITION_PAIR]: the route transition is one fold owned across the `platform` navigation pipeline and the `ui` transition wrapper, paired with per-route vital attribution.
- `platform` re-founds the router on the Navigation API and folds `document.startViewTransition` around the guard-admitted location commit as one scoped resource with a reduced-motion gate, the `ui` `<ViewTransition>`/`<Activity>` wrappers preserve the backgrounded viewport GL context and tab state across the swap, and `platform` web-vitals resets and re-attributes INP and CLS per soft navigation off the same one location cell.
- Unlocks native GPU-composited route transitions with zero animation dependency, state-preserving hidden surfaces so a re-shown viewport keeps its uploaded GPU buffers hot, and per-route vital attribution keyed off one navigation seam rather than a parallel route tracker.
- Couples the `platform` navigation-api-router, view-transition-fold, and soft-nav-vitals ideas to the `ui` native-view-transitions and activity-preserved-surfaces ideas; draws on the Baseline Navigation API, the interop View Transitions API, the React `<ViewTransition>`/`<Activity>` components, and the 2025 soft-navigation reporting contract.

[CLOSED_FAMILY_LINT_FENCE]: a hand-rolled `_tag` literal union of two or more arms is a typecheck or lint failure across the branch, so the closed-family owner is `Data.taggedEnum`/`Schema.TaggedClass` by construction, never prose discipline.
- `shapes.md` `[5]` forbids a hand-rolled `type X = { readonly _tag: "A"; … } | { readonly _tag: "B"; … }` for a closed family — the `Data.TaggedEnum` generated `$match`/`$is` fold is the owner — and the corpus carries no such union today only because every cold grade catches it by hand, a check no mechanical gate enforces. A branch lint rule flags any `type`/`interface` union of two or more `{ readonly _tag: <literal>; … }` arms not produced by `Data.taggedEnum`/`Schema.TaggedClass`/`Data.TaggedError`, promoting the doctrine line to a build error; the rule exempts the `Schema.Union(Schema.Struct({ _tag: Schema.Literal(...) }), …)` wire-decode form (the boundary discriminated-union codec the C# seam mints) as a runtime `Schema`, not a hand-rolled domain type.
- Unlocks the closed-family invariant enforced once at the centralized lint surface across all five folders rather than re-checked by hand per page, so a slipped hand-rolled union is a failed build, not a missed grade, and the `interchange` `FaultDetail`, the `projection` window/convergence families, and the node-tier `DurableFault` rails are the only `_tag` unions that compile.
- Couples the branch shape doctrine to the centralized eslint/`tsgo` surface the module-boundary fence already lives on; draws on the same one-config-owns-the-rule posture as the `projection/**` `@connectrpc/*` ban, so the new rule is one row beside the existing import fences.

## [2]-[CLOSED]

None.
