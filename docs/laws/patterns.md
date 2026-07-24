# [PATTERNS]

Pattern law binding two or more language branches: a law admitted here binds every branch it names, and a single-branch law routes to that branch's stack doctrine instead.

## [01]-[CONTENT_IDENTITY]

Content-addressed identity binds every branch that hashes, keys, or wires a value.

[CONTENT_KEY]:
- Binds: C#, Python, TypeScript, tooling.
- Law: a derived artifact keys on the content hash of its source, and cache validity is key equality, never path or mtime.
- Law: content identity federates through one frozen-name entry — seed-zero `XxHash128` over caller-canonical bytes, owned at the C# kernel `ContentHash` with every peer projecting the identical algorithm and seed; a runtime-local hasher, a digest minted under the reserved name, or a one-branch algorithm swap forks every cross-runtime join, so a new hashing need composes the owning entry and re-keying is a coordinated estate migration, never a branch decision.
- Law: keys persist and wire as 16 big-endian bytes — `:x32` is that big-endian hex — while `XxHash128` fills its hash-input buffer little-endian; peers normalize byte order exactly once at decode, and a raw-buffer parity check reversing neither side or both forks the key at the cross-runtime join.
- Law: an evidence record's identity slot carries the pre-run source key the hit test compares; a produced output's content address is a separate derived fact, and minting it into the slot silently defeats keyed elision.
- Law: a source with no canonical byte form — a live handle, a callable, a non-deterministic serialization — joins the key as environment-scoped identity and demotes admission to forced-live, never trusted to elide.
- Boundary: a security identity — a credential fingerprint, trust material — rides a cryptographic digest, and the speed hash keys caches and elision, never trust.

[WIRE_TOKEN]:
- Binds: all branches.
- Law: a wire token admits only the emitting owner's exact spelling, compared byte-wise at every peer; a tolerant parse re-emitting a normalized form forks the key.

[PREIMAGE_FRAMING]:
- Binds: all branches.
- Law: a multi-field hash preimage length-frames every variable-width field and count-frames every adjacent collection.
- Law: separator-joined concatenation is rejected — a separator character inside one value shifts two field splits onto one digest, and fixed-width elements never mark a collection boundary; a spine of fixed-width digests concatenates injectively and needs no framing.
- Law: a composite identity rides a canonical codec — framed canonical bytes or canonical JSON — never a hand-rolled join or quote scheme injective on one ambiguity axis; an array-bearing key frames shape beside canonicalized dtype and layout bytes.

[PREIMAGE_COVERAGE]:
- Binds: all branches.
- Law: a content key's preimage covers every identity-bearing member, and a member outside it is declared derived on site.
- Law: a stored member the preimage omits is a split-brain whose stated re-word or re-order semantics is false unless the declaration names it derived-or-annotation.
- Law: any input whose value shifts the produced output — a toolchain generation, a credential's content, a consumed template's digest — is identity-bearing wherever it lives and joins the preimage, never only the record's stored fields.
- Boundary: an input that cannot shift a produced success's bytes — an execution policy, a timeout, a retry budget — stays outside the content preimage, because policy keys when work fails, never what a success produces, and admitting it forks one content identity across policy motion.
- Boundary: a human-facing label cannot shift a success's bytes, so admitting it forks one content identity across renames.

## [02]-[PORTABILITY]

Portable operational behavior binds every branch a rail crosses.

[ROOT_DISCOVERY]:
- Binds: Python, C#, tooling.
- Law: root and anchor discovery walks upward to a sentinel file, never a fixed `parents[N]` depth or a cwd assumption.

[TYPED_ENVELOPE]:
- Binds: all branches.
- Law: an operational rail returns one typed envelope, and failure rides the envelope, never sentinel values in data rows.

[EMPTY_FOLD]:
- Binds: all branches.
- Law: pass and compliance verdicts quantified over a required evidence stream gate non-emptiness before the fold — universal quantification over an empty sequence passes vacuously — so empty required input fails closed, never reads as compliance.

## [03]-[TENANCY]

Tenant isolation binds every branch that pins or reads shared database session state.

[SESSION_GUC]:
- Binds: C#, TypeScript.
- Law: RLS session GUCs carry one namespace spelling shared by every branch that sets or reads them — a `SET`-side namespace and an RLS-predicate namespace that disagree read zero rows under a fail-closed policy, a defect no type or test at either end catches.
- Law: the namespace is `rasm.*` — `rasm.tenant`, `rasm.scope`, `rasm.subject` — minted once at the security custodian's `SessionCoordinate` catalog and read verbatim by every RLS predicate; `rasm.tenant` doubles as the telemetry tenant dimension, one vocabulary.
