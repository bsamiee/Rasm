# [PY_BRANCH_API_GRPCIO_TOOLS]

`grpcio-tools` drives the bundled `protoc` compiler in-process (native `_protoc_compiler` extension) to generate Python message classes, type stubs, and gRPC stub/servicer classes from `.proto` files. It ships the well-known protos under `grpc_tools/_proto`, exposes `command.build_package_protos` for build-time generation, and installs `ProtoFinder`/`ProtoLoader` meta-path hooks that back the runtime dynamic-stub import path consumed by `grpcio`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `grpcio-tools`
- package: `grpcio-tools`
- module: `grpc_tools`
- version: `1.81.1`
- license: Apache-2.0
- asset: build / codegen tool (dev/build-time only)
- rail: transport
- namespaces: `grpc_tools.protoc`, `grpc_tools.command`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dynamic-stub import family (`grpc_tools.protoc`)
- rail: transport
- auto-installed onto `sys.meta_path` at first `grpc_tools.protoc` import unless `GRPC_PYTHON_DISABLE_DYNAMIC_STUBS` is set; back the `grpc.protos()`/`grpc.services()` runtime helpers.

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]          | [RAIL]                                                                               |
| :-----: | :------------ | :--------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `ProtoFinder` | `MetaPathFinder`       | `sys.meta_path` hook resolving `_pb2`/`_pb2_grpc` module names to `.proto` files     |
|  [02]   | `ProtoLoader` | `importlib.abc.Loader` | compiles a `.proto` to module code at import time, caches topologically-ordered deps |

[PUBLIC_TYPE_SCOPE]: setuptools integration (`grpc_tools.command`)
- rail: transport

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]        | [RAIL]                                                 |
| :-----: | :------------------- | :------------------- | :----------------------------------------------------- |
|  [01]   | `BuildPackageProtos` | `setuptools.Command` | `setup.py`/`pyproject` build command (`--strict-mode`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: code generation
- rail: transport
- flag and `run_main` mechanics carried by [CODEGEN_TOPOLOGY].

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :-------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `protoc.main(command_arguments) -> int`                         | codegen        | nonzero return is a compile failure             |
|  [02]   | `protoc.entrypoint() -> None`                                   | console script | CLI shim; prepends the bundled `_proto` include |
|  [03]   | `command.build_package_protos(package_root, strict_mode=False)` | build          | compile every `.proto` under root               |

[ENTRYPOINT_SCOPE]: dynamic-stub construction
- rail: transport

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY] | [RAIL]                                   |
| :-----: | :------------------------------------------------------------------------ | :------------- | :--------------------------------------- |
|  [01]   | `ProtoFinder(suffix, codegen_fn)`                                         | construction   | resolves `_pb2`/`_pb2_grpc` module names |
|  [02]   | `ProtoLoader(suffix, codegen_fn, module_name, protobuf_path, proto_root)` | construction   | loader; `exec_module` compiles + caches  |

## [04]-[IMPLEMENTATION_LAW]

[CODEGEN_TOPOLOGY]:
- `protoc.main(command_arguments)` accepts a list matching the `protoc` CLI and returns the native exit code (`0` == success): `['-I<proto_include>', '--python_out=<out>', '--pyi_out=<out>', '--grpc_python_out=<out>', '<file.proto>']`. Check the return value — a nonzero result is a compile failure, not an exception.
- emit `--pyi_out` alongside `--python_out` to generate `*_pb2.pyi` type stubs so the generated message classes carry static types the type checker and `@beartype`/`pydantic` boundaries can read; `--grpc_python_out` is emitted only when the `.proto` declares a `service`.
- the bundled `grpc_tools/_proto` directory carries the well-known protos (`google/protobuf/*.proto`); resolve its path with `importlib.resources.files('grpc_tools') / '_proto'` and pass it with `-I` AFTER the project proto includes so project protos win on name collision.
- generated `*_pb2.py` carry message classes; generated `*_pb2_grpc.py` carry `<Service>Stub`, `<Service>Servicer`, and `add_<Service>Servicer_to_server(servicer, server)`.
- `build_package_protos(package_root, strict_mode=...)` is the required build-time entrypoint: it discovers every `.proto` under `package_root`, prepends both `package_root` and the well-known include, and runs `protoc.main` per file. `strict_mode=True` raises on the first failure instead of warning — use it in CI so a broken proto fails the build.
- `ProtoFinder`/`ProtoLoader` auto-install onto `sys.meta_path` at import; they (with the native `get_protos`/`get_services` codegen functions) implement the runtime dynamic-stub path that `grpc.protos('pkg/svc.proto')` / `grpc.services(...)` resolve. Set `GRPC_PYTHON_DISABLE_DYNAMIC_STUBS=1` to disable the hooks entirely.

[STACKS_WITH]:
- `grpcio` (`.api/grpcio.md`): generation produces the `<Service>Stub`/`<Service>Servicer` that bind to a `grpc.Channel`/`grpc.aio.Channel` and `add_<Service>Servicer_to_server`. Generate AOT into a tracked package for production servers; reserve the `protoc`-backed dynamic-import path for prototyping. The generated servicer is the surface an `anyio`-hosted `grpc.aio` server registers.
- `protobuf` (`.api/protobuf.md`): the `*_pb2.py` message classes ARE `protobuf` messages; pin `grpcio-tools` and the `protobuf` runtime to the same major so the generated descriptor pool matches the runtime (`protobuf` is capped `<7.0` by OpenTelemetry, so the generated code must target that runtime).
- `msgspec`/`pydantic` (`.api/msgspec.md`, `.api/pydantic.md`): protobuf messages are the wire shape, not the domain shape — map a `*_pb2` message onto a `msgspec.Struct`/pydantic model at the servicer boundary (the `--pyi_out` stubs give the field types to map against); domain code never threads raw protobuf messages.

[LOCAL_ADMISSION]:
- Include-path order: project protos first, then the bundled `grpc_tools/_proto` well-known protos.
- Always emit `--pyi_out` with `--python_out`; emit `--grpc_python_out` only for service-bearing protos. Check `protoc.main`'s return code (or use `strict_mode=True`).

[RAIL_LAW]:
- Package: `grpcio-tools`
- Owns: in-process `protoc` codegen (messages + `.pyi` stubs + gRPC stubs/servicers), well-known proto bundling, setuptools build integration, and the dynamic-stub meta-path hooks
- Accept: `protoc.main` with explicit `-I` include paths and `--pyi_out`, `command.build_package_protos(..., strict_mode=True)` at build time into a tracked output directory, AOT generation for production
