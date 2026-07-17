---
include:
  - "libs/**"
---

# [PLANNING_CORPUS]

`libs/.planning/campaign-method.md` is the review standard for design-corpus work and `libs/.planning/README.md` owns the page grammar — judge against them, never generic documentation convention. Design pages live at `<pkg>/.planning/<sub-domain>/<page>.md`; a code fence IS the work product, and the measure of any planning item is the diff to a fence — a card, idea line, or framing paragraph never substitutes for the fence it frames. A diff landing a real source file (`.cs`/`.py`/`.ts`) under `libs/` breaches the planning-phase charter and is a finding on its own.

## [01]-[FENCE_TRUTH]

- Presume an external member is a phantom until the folder's `.api/` catalogue verifies its spelling, public accessibility, and deprecation status; verify a claimed ABSENCE at the same bar, because a false no-such-member claim steers every rebuild into the inferior spelling.
- A fence member whose body is a placeholder comment (`=> /* ... */ ;` or an empty braced body carrying only prose) is an illusory implementation — reject it before deeper review; a resource-release claim in prose or comments binds to a visible call in the shown body.
- Sweep every fence for local compilability — planning fences have no compiler, so these fictions survive fluent prose: a `ref struct` captured by a lambda or riding a generic carrier, an identifier neither declared in scope nor owned by the page, a value returned from a `void` member, a type name written as an invocation, a generated-owner row whose positional argument count drifts from its declared constructor arity, a railed factory result bound into a bare-typed slot with no flatten, a payload passed outside the factory's declared admitted union, a derivation from a registry row that does not exist.
- Prose signatures byte-match their fence declarations: an `Entry`/`Auto` row whose parameter list, rail, or return drifts from the fence is a wrong contract, and a fence signature edit ripples its prose rows in the same diff.
- A composing fence binds the seam member's current landed declaration — spelling, accessibility, arity, rail shape, owning namespace — never name presence; a member edit on any of those axes sweeps every composing fence repo-wide in the same change. No compiler exists here; the sweep is the build.

## [02]-[DISPATCH_AND_FOLDS]

- In a closed dispatch family folding one shared request shape, every column is consumed or loudly refused by every arm — an axis some arms read and others silently ignore converts an unrepresentable check into a silent pass. A policy column consumed only by a cache key, receipt, or log annotates rather than governs — demand the governing arm or the demotion; an arm that cannot honor a policy field refuses the combination loudly before provider work.
- A safety guarantee a shared rail claims — fence, lock, guarded predicate, tenant guard — is consumed structurally by the rail body from the case's data row; a per-case delegate whose body carries the guarantee makes the claim illusory.
- A closed union whose every case carries one identically-typed field re-projected by a total switch is operand restatement — demand the shared value as the consuming entrypoint's argument.
- Attack fold accumulators: a seed starting completeness flags true fabricates a receipt over an empty input set; a fact stream folded into a name-keyed bag silently drops all but one entry under a repeated key — set-valued findings emit as one typed list value; a subset test over a requirement row (`need <= available`) reads an empty requirement as vacuously satisfied, inverting a disabled row into always-enabled — demand the explicit empty-row short-circuit.
- Declared capability is consumed capability: a capability noun with no anchoring fence member, a receipt nothing mints, or a policy row nothing consumes is decorative density — demand the consuming member or the deletion.

## [03]-[PAGE_GRAMMAR]

- Card fields ride the closed ordered vocabulary `Owner`, `Cases`, `Entry`, `Auto`, `Output`, `Receipt`, `Packages`, `Growth`, `Boundary`; a filled field deciding nothing is a finding, and a card bullet carries only what the fence cannot show.
- `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]` task status, `[COMPLETE]`/`[DROPPED]` closure, and `[RESEARCH]` rows (`- [TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>`) are the corpus's closed vocabulary — never hedge, process-ledger, or `TODO` findings. A research row is recorded epistemic debt; a fence settling a member a sibling research row still declares unverified is a finding, and a resolved row deletes whole with no tombstone.
- Seam ledgers are one ```text seams``` fence per ARCHITECTURE page: rows `<SourceFile> <glyph> <lang:pkg/subdomain> # [<KIND>]: <shared shape>` with glyphs `→`/`←`/`⇄` and a closed `[KIND]` vocabulary; every edge appears on both endpoint folders with mirrored glyph and identical `[KIND]`, and an in-package relation stays in the codemap, never a seam.
- Only the cross-libs core (`libs/.planning/`) names another language; a branch or folder consumes a peer only through wire contracts.
- Decision-complete is the bar: a page whose realization demands an external lookup, a deferred investigation, or a guessed integration point is incomplete — the work, context, and integration points live on the page or its `.api/`.
