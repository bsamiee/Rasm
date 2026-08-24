# [PY_CONTRACTS_RULINGS]

`python/contracts` rulings settle generated Python package decisions.

## [01]-[PACKAGES]

- `protoc-gen-py` moves with `protobuf-py` because emitted modules bind that runtime directly.
- `protoc-gen-connectrpc` and `connectrpc` resolve from one source coordinate because generated stubs bind runtime internals.
- `protovalidate` is direct runtime closure because it evaluates embedded rules on `protobuf-py` messages without a Python rule mirror.
- `expression` is direct runtime closure because artifact custody rails refusals as values and reconstructs a raise only at the generated-stream edge.

## [02]-[SHAPE]

- `rasm.contracts.gen` owns estate and support modules, while `rasm.contracts.vendor` owns publisher modules.
- Publisher AVSC resources remain exact package data; consumers parse them without transcribing schema objects.
- `io=async` emits the service family the branch serves and dials through asynchronous composition.
- Frame width, extent bounds, and identity width READ off the generated `buf.validate` descriptors — a Python literal restating one forks the law.
- Nonterminal frames carry exactly the declared frame width, so a fragmented stream refuses here exactly where every peer receiver refuses it.
- Extent is read before identity on every reference confirmation — a truncated stream reports the axis a caller acts on, not the digest it implies.

## [03]-[COLLAPSE]

- Generated messages are the sole protobuf vocabulary — a parallel Python model splits descriptor and validation authority.
- One `BodyAdmission` composes Protovalidate with every asynchronous Connect interceptor protocol; handlers and clients carry no validation prologue.
- Client refusals retain phase and violations on `AdmissionError`; server request constraints alone expose details, engine defects staying INTERNAL.
- Lazy client-stream refusals ride one per-call carrier that restores its own refusal — the body boundary maps iterator exceptions onto status.
- Artifact transfer stays frame-centric in the package, and `ArtifactTransfer` alone wraps direction-unique RPC envelopes; no consumer repeats it.
- `output` hands native producers a helper-owned path they seal and publish uncopied; `stage(Path)` copies caller-owned mutable paths before proof.
- One streaming SHA-256 pass proves ordered payload bytes into the canonical 32-byte `ArtifactRef`; payload bytes alone enter that digest.
- `references` walks set fields in descriptor order, repeated values in element order, maps by key, refuses `Any`, and collapses on extent coherence.
- `references` walks an explicit ancestry frontier because message nesting is caller-scaled depth; native recursion there forfeits at the frame limit.
- One `ArtifactSink.seal` folds every custody route — native, staged, framed — through one single-use latch, one spool proof, and one stated claim.
- Spool proof is `hashlib.file_digest` over the written spool, so no custody route keeps a chunked hash loop beside the one digest owner.
- Custody is a closed two-state family, never a sealed flag; a refused seal leaves it open so a caller corrects its claim and retries.
- One `ArtifactStream` parameterized by an envelope row serves bare frames, Fetch responses, and Put requests, and one inverse unwraps those rows.
- Artifact refusals are one closed record family carrying their own evidence; a proof tag beside nullable evidence slots is the deleted mirror.
- Transfer reads its call budget off the enclosing cancel scope — a `timeout_ms` parameter forwards a deadline that scope already carries.

## [04]-[STRUCTURE]

- `rasm` stays a PEP 420 namespace while `rasm.contracts` is the installable typed package boundary above the clean generation sweep.
- Estate generation carries reachable support imports under `gen`; publisher generation writes the collision-safe `vendor` root.
- Boundary modules, `__init__.py`, and `py.typed` stay above `gen` and `vendor`, so clean generation deletes no identity and no policy.
- Assay restores manifest-distributed publisher resources after Buf's clean sweep.
- Catalogue roster markers contain gate-emitted descriptor data; generator grammar remains the hand-maintained correspondence.

## [05]-[PROCESS]

- `assay contracts generate` authors modules, publisher resources, and catalog rows; the `rasm-contracts` member manifest builds and installs them.
