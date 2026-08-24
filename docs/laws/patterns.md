# [PATTERNS]

Patterns bind every branch their `Binds` row names, and law reaching one branch alone routes to that branch's stack doctrine.

## [01]-[CONTENT_IDENTITY]

Content-addressed identity binds every branch that hashes, keys, or wires a value.

[CONTENT_KEY]:
- Binds: C#, Python, TypeScript, tooling.
- Law: Derived artifacts key on the content hash of their source, so cache validity is key equality, never path or mtime.
- Law: Content identity federates through one frozen-name entry per branch, seed-zero `XxHash128` over caller-canonical bytes.
- Law: `tests/contracts/` `content-identity` proves that entry's parity, so a new hashing need composes it and re-keying spans the estate.
- Law: Keys persist and wire as 16 big-endian bytes, `:x32` their hex, while `XxHash128` fills its hash-input buffer little-endian.
- Law: Peers normalize byte order exactly once at decode, and a parity check reversing neither side or both forks the key at the join.
- Law: Evidence records carry the pre-run source key in their identity slot, the key a hit test compares.
- Law: Produced-output content addresses stay a separate derived fact, and minting one into the identity slot defeats keyed elision.
- Law: Sources with no canonical byte form — a live handle, a callable, a nondeterministic serialization — join as environment-scoped identity.
- Law: Continuous coordinates key on an exact-predicate ordinal or a lattice quantum — raw float keys hash equal points apart and split welds.
- Law: Environment-scoped identity demotes admission to forced-live and never elides.
- Boundary: Security identities ride a cryptographic digest — credential fingerprints, trust material — and the speed hash keys caches, never trust.
- Boundary: Rounding sites carry the quantum in the key — it is part of the identity, so folds rounding differently address disjoint spaces.

[WIRE_TOKEN]:
- Binds: All branches.
- Law: Wire tokens admit the emitting owner's exact spelling alone, compared byte-wise at every peer, and a tolerant re-emit forks the key.
- Law: ONE ingest table maps peer-wire strings onto a branch's ordinal-keyed owner; re-keying the owner or mapping twice forks the correspondence.

[PREIMAGE_FRAMING]:
- Binds: All branches.
- Law: Multi-field hash preimages length-frame every variable-width field and count-frame every adjacent collection.
- Law: Preimages reject separator-joined concatenation — a separator inside one value shifts two field splits onto one digest.
- Law: Digest seeds folded from two or more fields are preimages and frame like any other — grammar exclusions never substitute for framing.
- Law: Fixed-width digest spines concatenate injectively and need no framing, while fixed-width elements never mark a collection boundary.
- Law: Composite identities ride a canonical codec — framed canonical bytes or canonical JSON — never a hand-rolled join or quote scheme.
- Law: Array-bearing keys frame shape beside canonicalized dtype and layout bytes.

[KEY_GRAMMAR]:
- Binds: All branches.
- Law: Free-form identity inputs compile a grammar only on a class proved TOTAL over the live cross-branch roster, never an illustrative sample.
- Law: Peers minting one identity segment share its character class byte-wise; `python:runtime/evidence/identity` owns the dotted `[a-z0-9_-]` class.

[PREIMAGE_COVERAGE]:
- Binds: All branches.
- Law: Content-key preimages cover every identity-bearing member, and a member outside one is declared derived on site.
- Law: Stored members outside the preimage read as split-brain, falsifying any re-word or re-order claim unless declared derived-or-annotation.
- Law: Inputs shifting the produced output join the preimage wherever they live — a toolchain generation, a credential, a consumed template's digest.
- Boundary: Inputs that cannot shift a success's bytes — an execution policy, a timeout, a retry budget — stay outside the content preimage.
- Boundary: Human-facing labels shift no success bytes, so admitting one forks content identity across renames.

## [02]-[PORTABILITY]

Portable operational behavior binds every branch a rail crosses.

[ROOT_DISCOVERY]:
- Binds: Python, C#, tooling.
- Law: Root and anchor discovery walks upward to a sentinel file, never a fixed `parents[N]` depth or a cwd assumption.

[TYPED_ENVELOPE]:
- Binds: All branches.
- Law: Operational rails return one typed envelope, and failure rides it, never a sentinel in a data row.

[EMPTY_FOLD]:
- Binds: All branches.
- Law: Pass and compliance verdicts gate non-emptiness before the fold, since quantification over an empty evidence stream passes vacuously.

[LANE_CARVE]:
- Binds: All branches.
- Law: Lanes declare their carve set in their own config, so a lane admitted later states its exemptions or sweeps the carved tree with every sibling.

## [03]-[TENANCY]

Tenant isolation binds every branch that pins or reads shared database session state.

[SESSION_GUC]:
- Binds: C#, TypeScript.
- Law: RLS session GUCs carry one namespace spelling every branch shares, since disagreeing `SET` and predicate spellings read zero rows fail-closed.
- Law: `SessionCoordinate` mints one `rasm.*` namespace every RLS predicate reads verbatim: `rasm.tenant` `rasm.scope` `rasm.subject` `rasm.plane`.
- Law: `rasm.tenant` doubles as the telemetry tenant dimension, one vocabulary across both planes.
- Law: Cross-tenant reads admit on a STATED `rasm.plane = 'maintenance'` arm — FORCE RLS zero-rows unpinned sessions, so posture admits, never role.
