# [PY_BRANCH_API_GRPCIO_TOOLS]

`grpcio-tools` drives the bundled `protoc` in-process through the native `_protoc_compiler` extension to generate message classes, `.pyi` type stubs, and gRPC stub/servicer code from `.proto` sources. It holds the codegen boundary for the transport rail — build-time generation into tracked packages, well-known-proto bundling, and the meta-path hooks backing `grpcio`'s runtime dynamic-stub import — so no domain code owns a compiler invocation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `grpcio-tools`
- package: `grpcio-tools` (`Apache-2.0`)
- module: `grpc_tools`
- namespaces: `grpc_tools.protoc`, `grpc_tools.command`
- asset: build/codegen tool binding the native `_protoc_compiler` extension; dev/build-time only
- rail: transport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dynamic-stub import family (`grpc_tools.protoc`)

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :------------ | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `ProtoFinder` | class         | `MetaPathFinder` resolving `_pb2`/`_pb2_grpc` module names to `.proto` files    |
|  [02]   | `ProtoLoader` | class         | `Loader` compiling a `.proto` to module code at import, caches topological deps |

[PUBLIC_TYPE_SCOPE]: setuptools integration (`grpc_tools.command`)

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                                 |
| :-----: | :------------------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `BuildPackageProtos` | class         | `setuptools.Command` for `setup.py`/`pyproject` builds, `--strict-mode` flag |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: code generation

| [INDEX] | [SURFACE]                                                       | [SHAPE] | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------------------------- | :------ | :---------------------------------------------- |
|  [01]   | `protoc.main(command_arguments) -> int`                         | static  | run bundled protoc; nonzero return is a failure |
|  [02]   | `protoc.entrypoint() -> None`                                   | static  | console shim prepending the bundled `_proto`    |
|  [03]   | `command.build_package_protos(package_root, strict_mode=False)` | static  | compile every `.proto` under root               |

[ENTRYPOINT_SCOPE]: dynamic-stub construction

| [INDEX] | [SURFACE]                                                                 | [SHAPE] | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------ | :------ | :--------------------------------------- |
|  [01]   | `ProtoFinder(suffix, codegen_fn)`                                         | ctor    | resolves `_pb2`/`_pb2_grpc` module names |
|  [02]   | `ProtoLoader(suffix, codegen_fn, module_name, protobuf_path, proto_root)` | ctor    | `exec_module` compiles and caches        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `protoc.main` takes a `protoc`-CLI argument list (`['-I<include>', '--python_out=<out>', '--pyi_out=<out>', '--grpc_python_out=<out>', '<file.proto>']`) and returns the native exit code (`0` success); a nonzero return is a compile failure, never an exception, so check it.
- Emit `--pyi_out` with every `--python_out` so generated messages carry the static field types `@beartype`/`pydantic` boundaries read; emit `--grpc_python_out` only for a `.proto` declaring a `service`.
- `grpc_tools/_proto` bundles the well-known protos; resolve with `importlib.resources.files('grpc_tools') / '_proto'` and pass its `-I` after the project includes so project protos win a name collision.
- Generated `*_pb2.py` carry message classes; `*_pb2_grpc.py` carry `<Service>Stub`, `<Service>Servicer`, and `add_<Service>Servicer_to_server(servicer, server)`.
- `build_package_protos` discovers every `.proto` under `package_root`, prepends `package_root` and the well-known include, and runs `protoc.main` per file; `strict_mode=True` raises on the first failure.
- `ProtoFinder`/`ProtoLoader` auto-install onto `sys.meta_path` at first `grpc_tools.protoc` import with the native `get_protos`/`get_services` functions, backing `grpc.protos(...)`/`grpc.services(...)`; `GRPC_PYTHON_DISABLE_DYNAMIC_STUBS=1` disables them.

[STACKING]:
- `grpcio`(`.api/grpcio.md`): generation mints the `<Service>Stub`/`<Service>Servicer` binding a `grpc.Channel`/`grpc.aio.Channel` and `add_<Service>Servicer_to_server`; generate AOT into a tracked package for production servers, reserving `grpc.protos`/`grpc.services` dynamic import for prototyping.
- `protobuf`(`.api/protobuf.md`): the `*_pb2.py` message classes are `protobuf` messages bound to the process's `protobuf` runtime — a descriptor-pool mismatch against that runtime fails at import.
- `msgspec`/`pydantic`(`.api/msgspec.md`, `.api/pydantic.md`): a `*_pb2` message is the wire shape, not the domain shape — map it onto a `msgspec.Struct`/pydantic model at the servicer boundary against the `--pyi_out` field types; domain code never threads a raw protobuf message.
- transport build: `build_package_protos(package_root, strict_mode=True)` runs at build into the tracked transport package, minting the AOT `*_pb2_grpc.py` the serve owner registers.

[LOCAL_ADMISSION]:
- AOT generation into a tracked output directory is the admitted path; `build_package_protos(..., strict_mode=True)` at build fails the build on a broken proto.

[RAIL_LAW]:
- Package: `grpcio-tools`
- Owns: in-process `protoc` codegen (messages, `.pyi` stubs, gRPC stubs/servicers), well-known proto bundling, setuptools build integration, and the dynamic-stub meta-path hooks
- Accept: `protoc.main` with explicit `-I` includes and `--pyi_out`, `build_package_protos(..., strict_mode=True)` into a tracked directory, AOT generation for production
- Reject: a hand-rolled `protoc` subprocess shell-out, a raw `*_pb2` message threaded into domain code, the dynamic import path on a production serve leg
