# [PATTERNS]

Pattern law binding two or more language branches: law admitted here binds every branch named, and single-branch law routes to the branch stack doctrine instead.

## [01]-[CONTENT_IDENTITY]

Content-addressed identity binds every branch that hashes, keys, or wires a value.

[CONTENT_KEY]:
- Binds: C#, Python, TypeScript, tooling.
- Law: Derived artifact keys on the content hash of its source, cache validity is key equality, never path or mtime.
- Law: Content identity federates through one frozen-name entry per branch — seed-zero `XxHash128` over caller-canonical bytes, each branch minting that entry in its own types and parity proven at the `tests/contracts/` `CANONICAL_BYTE_IDENTITY` fixture; a runtime-local hasher, a digest minted under the reserved name, or a one-branch algorithm swap forks every cross-runtime join, so a new hashing need composes the owning entry and re-keying is a coordinated estate migration, never a branch decision.
- Law: Keys persist and wire as 16 big-endian bytes — `:x32` is that big-endian hex — while `XxHash128` fills its hash-input buffer little-endian; peers normalize byte order exactly once at decode, and a raw-buffer parity check reversing neither side or both forks the key at the cross-runtime join.
- Law: An evidence record's identity slot carries the pre-run source key the hit test compares; a produced output's content address is a separate derived fact, and minting it into the slot silently defeats keyed elision.
- Law: A source with no canonical byte form — a live handle, a callable, a non-deterministic serialization — joins the key as environment-scoped identity and demotes admission to forced-live, never trusted to elide.
- Boundary: A security identity — a credential fingerprint, trust material — rides a cryptographic digest, and the speed hash keys caches and elision, never trust.

[WIRE_TOKEN]:
- Binds: All branches.
- Law: A wire token admits only the emitting owner's exact spelling, compared byte-wise at every peer; a tolerant parse re-emitting a normalized form forks the key.

[PREIMAGE_FRAMING]:
- Binds: All branches.
- Law: A multi-field hash preimage length-frames every variable-width field and count-frames every adjacent collection.
- Law: Separator-joined concatenation is rejected — a separator character inside one value shifts two field splits onto one digest, and fixed-width elements never mark a collection boundary; a spine of fixed-width digests concatenates injectively and needs no framing.
- Law: A composite identity rides a canonical codec — framed canonical bytes or canonical JSON — never a hand-rolled join or quote scheme injective on one ambiguity axis; an array-bearing key frames shape beside canonicalized dtype and layout bytes.

[PREIMAGE_COVERAGE]:
- Binds: All branches.
- Law: A content key's preimage covers every identity-bearing member, and a member outside it is declared derived on site.
- Law: A stored member the preimage omits is a split-brain whose stated re-word or re-order semantics is false unless the declaration names it derived-or-annotation.
- Law: Any input whose value shifts the produced output — a toolchain generation, a credential's content, a consumed template's digest — is identity-bearing wherever it lives and joins the preimage, never only the record's stored fields.
- Boundary: An input that cannot shift a produced success's bytes — an execution policy, a timeout, a retry budget — stays outside the content preimage, because policy keys when work fails, never what a success produces, and admitting it forks one content identity across policy motion.
- Boundary: A human-facing label cannot shift a success's bytes, so admitting it forks one content identity across renames.

## [02]-[PORTABILITY]

Portable operational behavior binds every branch a rail crosses.

[ROOT_DISCOVERY]:
- Binds: Python, C#, tooling.
- Law: Root and anchor discovery walks upward to a sentinel file, never a fixed `parents[N]` depth or a cwd assumption.

[TYPED_ENVELOPE]:
- Binds: All branches.
- Law: An operational rail returns one typed envelope, and failure rides the envelope, never sentinel values in data rows.

[EMPTY_FOLD]:
- Binds: All branches.
- Law: Pass and compliance verdicts quantified over a required evidence stream gate non-emptiness before the fold — universal quantification over an empty sequence passes vacuously — so empty required input fails closed, never reads as compliance.

## [03]-[TENANCY]

Tenant isolation binds every branch that pins or reads shared database session state.

[SESSION_GUC]:
- Binds: C#, TypeScript.
- Law: RLS session GUCs carry one namespace spelling shared by every branch that sets or reads them — a `SET`-side namespace and an RLS-predicate namespace that disagree read zero rows under a fail-closed policy, a defect no type or test at either end catches.
- Law: The namespace is `rasm.*` — `rasm.tenant`, `rasm.scope`, `rasm.subject` — minted once at the security custodian's `SessionCoordinate` catalog and read verbatim by every RLS predicate; `rasm.tenant` doubles as the telemetry tenant dimension, one vocabulary.
