# transport admission question — JsonPatch: apply-only vs delta-generation

## verdict

Apply-only. The admitted patch package suffices; no JSON-diff admission is warranted.

## evidence

- The admitted package's entire public surface is construction-plus-application: `JsonPatchDocument`/`JsonPatchDocument<TModel>` expose typed operation builders (`Add`/`Remove`/`Replace`/`Move`/`Copy`/`Test`, each with positional list overloads) and four `ApplyTo` overloads (throwing, error-delegate-accumulating, and adapter-swapping variants). The full decompiled type list contains no diff, generate, or compare member anywhere — delta generation is structurally absent, not merely undocumented.
- Patches in this stack originate at intent sites: the writer constructs operations through the expression-tree builders at the moment it decides the mutation, so the patch document is recorded intent and delta computation never occurs. A state-diff route would only be needed where no intent stream exists — and that scenario is already owned elsewhere: state convergence without intent is durability's op-log/LWW sync territory, and sparse partial-update intent inside binary contracts is the field-mask row the protobuf contract law already carries.
- Delta generation is also semantically lossy, independent of admission cost: a diff of two states cannot recover `Move`/`Copy` (it emits remove-plus-add), explodes list reorders into per-index replaces, and cannot synthesize `Test` concurrency guards — generated deltas are strictly weaker and typically larger than recorded operations. Admitting a JSON-diff package would buy a worse patch producer than the one the apply-only law already implies.
- Boundary risk of the apply-only posture: relayed patches from foreign producers (the untyped string-path document) apply through the same error-accumulating overload and fail per-operation with typed evidence, so foreign patches of arbitrary provenance are admissible without any diff capability.

## flag

If a future surface genuinely requires reconciling two opaque JSON states with no intent stream and no op-log (foreign-system import is the only plausible case), that is a new admission decision for a JSON-diff package at that page's research start — it does not retrofit into this page's law and nothing in the current transport, persistence, or durability scope needs it.
