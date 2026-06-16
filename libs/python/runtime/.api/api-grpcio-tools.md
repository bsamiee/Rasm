# [PY_RUNTIME_API_GRPCIO_TOOLS]

`grpcio-tools` supplies the protobuf/gRPC code generator: the `grpc_tools.protoc` compiler entry that turns the C# `.proto` descriptors into Python message and server-stub modules. It is the runtime owner for the proto compilation step feeding the companion gRPC server. This distribution is NOT installed in the active environment, so its members are un-reflected and marked as a gap.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `grpcio-tools`
- package: `grpcio-tools`
- import: `grpc_tools`
- version: un-installed (manifest pin `grpcio-tools>=1.81.1; python_version<'3.13'`)
- owner: `runtime`
- rail: transport
- namespaces: `grpc_tools.protoc`
- capability: protobuf/gRPC code generation from `.proto` descriptors
- admission note: pinned with `python_version<'3.13'` in the root manifest; under `requires-python='>=3.15'` the marker is dead and the distribution is pruned from `uv.lock` and absent from the environment. First-class admission requires the floor/lock-scope decision recorded in the suite TASKLOG.

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: compiler surface
- rail: transport

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `grpc_tools.protoc` | compiler module | proto compilation entry |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: generation operations
- rail: transport

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
| [1] | `grpc_tools.protoc.main` | generate | run the protoc compiler over `.proto` inputs |

## [4]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- generation law: server stubs and message classes are generated from the C# `.proto` descriptors via `grpc_tools.protoc` as a build step; the runtime never hand-writes generated modules.
- contract law: generated artifacts mirror the existing C# `ComputeService`/`ArtifactSync` contract exactly; the generator is the single source of the Python stub surface.
- placement law: generated modules land beside the companion server source and are consumed through `.api/api-grpcio.md` (transport) and `.api/api-protobuf.md` (messages).

[LOCAL_ADMISSION]:
- This page documents the generation step; the runtime composes the generated stubs, not the generator itself, at runtime.
- The generation step runs only where the distribution is installable; under the current `>=3.15` floor it is unavailable, so generation is gated on the floor/lock-scope decision.

[RAIL_LAW]:
- Package: `grpcio-tools`
- Owns: the proto compilation step generating Python stubs from the C# contract
- Accept: `grpc_tools.protoc` build-step generation against the canonical `.proto` descriptors
- Reject: hand-written generated modules, divergence from the C# contract

## [5]-[TASKLOG_GAP]

[UN_REFLECTED]:
- `grpcio-tools` is absent from the active `>=3.15` environment; its full member surface beyond the stable `grpc_tools.protoc.main` compiler entry is un-reflected. Re-run `uv run python -m tools.assay api query --key grpcio-tools --sources pydist --full` once the floor/lock-scope decision admits a sub-3.15 environment, and complete the PUBLIC_TYPES and ENTRYPOINTS tables from the reflected surface.
