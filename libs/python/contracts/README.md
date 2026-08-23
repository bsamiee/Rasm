# [PY_CONTRACTS]

`contracts` owns the installable `rasm-contracts` distribution for generated corpus bindings, exact publisher assets, descriptor-driven Connect body admission, and verified artifact transfer. Estate modules live below `rasm.contracts.gen`; publisher modules and resources live below `rasm.contracts.vendor`.

## [01]-[ROUTER]

[GENERATED]:
- [01]-[CATALOGUE](.api/rasm-contracts.md): Generator symbol grammar, derived descriptor roster, and import law.
- [02]-[EMISSION](src/rasm/contracts): Clean-swept estate, support, publisher, Connect, and exact resource outputs.
- [03]-[PACKAGE](pyproject.toml): Workspace distribution metadata and direct runtime closure.

## [02]-[DOMAIN_PACKAGES]

[GENERATORS]:
- `protoc-gen-py` — Emits typed message modules from the descriptor image.
- `protoc-gen-connectrpc` — Emits asynchronous service protocols, applications, and clients.

## [03]-[SUBSTRATE_PACKAGES]

[WIRE_RUNTIME]:
- `anyio` — Keeps artifact spool reads, writes, and cleanup off the service event loop.
- `protobuf-py` — Supplies generated message, descriptor, WKT, binary, and ProtoJSON surfaces.
- `connectrpc` — Supplies generated asynchronous Connect applications, clients, interceptors, and codecs.
- `expression` — Carries artifact custody outcomes on `Result` so refusals travel as values until a generated stream demands a raise.
- `protovalidate` — Evaluates embedded standard and CEL rules directly on generated `protobuf-py` messages.

## [04]-[ADMISSION]

- `rasm.contracts.BodyAdmission` — Applies Protovalidate once to each asynchronous Connect request and response element across all four RPC shapes.
- `rasm.contracts.AdmissionError` — Retains client-side phase, engine cause, and structured violations for consumer-owned domain fault mapping.

## [05]-[ARTIFACT]

- `rasm.contracts.artifact.ArtifactLaw` — Reads frame width, extent bounds, and identity width off the generated `buf.validate` descriptors.
- `ArtifactTransfer` — Composes generated Fetch and Put calls under exact framing, spool identity proof, and receipt confirmation.
- `output`/`stage`/`receive` — Hold native outputs, caller inputs, and remote streams in one verified temporary-file lifecycle with sure cleanup.
- `ArtifactSink.seal` — Folds every custody route through one latch, one `hashlib.file_digest` spool proof, and one stated claim.
- `ArtifactStream` — Emits one sealed artifact as bare frames, Fetch responses, or Put requests from one envelope correspondence.
- `put_frames`/`fetch_frames` — Unwrap generated direction-unique envelopes onto the shared frame without handler-owned wrapper loops.
- `ArtifactRefusal` — Closes the artifact law as records carrying their own evidence; `ArtifactError` reconstructs one at the generated-stream edge.
- `references` — Discovers generated `ArtifactRef` values in descriptor order, rejects opaque `Any`, and collapses only extent-coherent duplicates.
