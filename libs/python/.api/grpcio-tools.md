# [PY_BRANCH_API_GRPCIO_TOOLS]

`grpcio-tools` supplies `grpc_tools.protoc.main`, which drives the `protoc` compiler in-process to generate Python message classes and gRPC stub/servicer classes from `.proto` files. It bundles the well-known proto files under `grpc_tools/_proto` and exposes `ProtoFinder`/`ProtoLoader` for runtime proto-to-module import without out-of-band code generation.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `grpcio-tools`
- package: `grpcio-tools`
- module: `grpc_tools`
- asset: build / codegen tool
- rail: transport
- namespaces: `grpc_tools.protoc`, `grpc_tools.command`

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: runtime import family
- rail: transport

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [RAIL]                                        |
| :-----: | :------------ | :------------ | :-------------------------------------------- |
|   [1]   | `ProtoFinder` | import finder | `sys.meta_path` hook for `.proto` file lookup |
|   [2]   | `ProtoLoader` | import loader | compiles proto at import time via `protoc`    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: code generation
- rail: transport

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :------------------------------------------------------------------------ | :------------- | :--------------------------------------------- |
|   [1]   | `protoc.main(command_arguments)`                                          | codegen        | run `protoc` in-process with args list         |
|   [2]   | `ProtoFinder(suffix, codegen_fn)`                                         | construction   | meta-path finder for `.proto`-backed modules   |
|   [3]   | `ProtoLoader(suffix, codegen_fn, module_name, protobuf_path, proto_root)` | construction   | proto module loader                            |
|   [4]   | `command.build_package_protos(proto_source_dir)`                          | build          | compile all `.proto` in a package directory    |
|   [5]   | `command.BuildPackageProtos`                                              | setuptools cmd | `setup.py` command that invokes proto compiler |

## [4]-[IMPLEMENTATION_LAW]

[CODEGEN_TOPOLOGY]:
- `protoc.main(command_arguments)` accepts a list matching the `protoc` CLI: `['-I<proto_include>', '--python_out=<out_dir>', '--grpc_python_out=<out_dir>', '<file.proto>']`
- the bundled `grpc_tools/_proto` directory contains well-known proto files and `google/protobuf/*.proto`; pass it with `-I` alongside project proto includes
- generated `_pb2.py` files contain message classes; generated `_pb2_grpc.py` files contain `Stub`, `Servicer`, and `add_<ServiceName>Servicer_to_server` functions
- `ProtoFinder` and `ProtoLoader` support runtime import of `.proto` files without pre-generation; add `ProtoFinder` to `sys.meta_path` before importing the proto module
- `build_package_protos` is the recommended build-time entrypoint; it walks a directory and calls `protoc.main` per discovered `.proto`

[LOCAL_ADMISSION]:
- Code generation runs at build time, not at runtime in production.
- Include path order: project protos first, then `grpc_tools._proto` well-known protos.
- Pass `--grpc_python_out` only when the `.proto` defines a `service`; message-only protos need `--python_out` only.

[RAIL_LAW]:
- Package: `grpcio-tools`
- Owns: in-process `protoc` codegen, well-known proto bundling, and setuptools integration
- Accept: `protoc.main` with explicit `-I` include paths, build-time generation into a tracked output directory
- Reject: runtime proto compilation in production servers, missing well-known proto include path
