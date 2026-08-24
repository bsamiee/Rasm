# [PY_CONTRACTS]

`contracts` is the Python branch's wire boundary: generated corpus bindings, exact publisher assets, descriptor-driven Connect body admission, and verified artifact transfer, seated at the module root the `rasm-contracts` distribution builds from. Estate modules live below `rasm.contracts.gen` and publisher modules and resources below `rasm.contracts.vendor`, so a clean sweep rewrites both roots whole while identity, policy, and the hand-written boundary above them survive untouched.

## [01]-[ROUTER]

[GENERATED]:
- [01]-[CATALOGUE](.api/rasm-contracts.md): Generator symbol grammar, gate-emitted descriptor roster, and the import law each root carries.
- [02]-[EMISSION](rasm/contracts/gen): Estate families and their reachable support closure under one relative-import root.
- [03]-[PUBLISHER](rasm/contracts/vendor): Publisher messages, Connect stubs, and exact resources kept collision-safe beside the estate.

[BOUNDARY]:
- [04]-[ADMISSION](rasm/contracts/admission.py): Applies Protovalidate once to every asynchronous Connect body across all four RPC shapes.
- [05]-[ARTIFACT](rasm/contracts/artifact.py): Proves frame, extent, and identity on one custody lifecycle the generated transfer calls compose.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; admission rows ride the workspace manifests as bare names, `uv.lock` fixes every version, and this folder's `.api/` corroborates.

[GENERATORS]:
- `protoc-gen-py` — Emits typed message modules from the descriptor image.
- `protoc-gen-connectrpc` — Emits asynchronous service protocols, applications, and clients.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Python registry, whose charters own the full contracts; `libs/python/.api/` holds the shared API evidence.

[WIRE_RUNTIME]:
- `anyio` — Keeps artifact spool reads, writes, and cleanup off the service event loop.
- `protobuf-py` — Supplies generated message, descriptor, WKT, binary, and ProtoJSON surfaces.
- `connectrpc` — Supplies generated asynchronous Connect applications, clients, interceptors, and codecs.
- `expression` — Carries artifact custody outcomes on `Result` so refusals travel as values until a generated stream demands a raise.
- `protovalidate` — Evaluates embedded standard and CEL rules directly on generated `protobuf-py` messages.
