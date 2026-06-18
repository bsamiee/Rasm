# [API_CATALOGUE] @bufbuild/protoc-gen-es

`@bufbuild/protoc-gen-es` supplies the `protoc-gen-es` binary and the `protocGenEs` plugin object that `@bufbuild/protoplugin` executes to generate TypeScript and JavaScript message types, service descriptors, and optional JSON and valid-type variants from `.proto` files for the interchange codegen rail.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@bufbuild/protoc-gen-es`
- package: `@bufbuild/protoc-gen-es`
- namespace: —
- asset: CLI binary `bin/protoc-gen-es`; CommonJS module `dist/cjs/src/protoc-gen-es-plugin.js`
- rail: codegen

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin export
- rail: codegen

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [RAIL]                         |
| :-----: | :------------ | :------------ | :----------------------------- |
|   [1]   | `protocGenEs` | `Plugin`      | `@bufbuild/protoplugin` plugin |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: CLI invocation
- rail: codegen

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :-------------------------------------------- | :------------- | :--------------------------------- |
|   [1]   | `protoc-gen-es`                               | binary         | `buf generate` plugin entrypoint   |
|   [2]   | `--es_out=<dir>`                              | protoc flag    | output directory for protoc mode   |
|   [3]   | `--es_opt=target=ts`                          | plugin option  | TypeScript output target           |
|   [4]   | `--es_opt=target=js`                          | plugin option  | JavaScript output target           |
|   [5]   | `--es_opt=target=dts`                         | plugin option  | declaration-only output target     |
|   [6]   | `--es_opt=import_extension=js`                | plugin option  | import extension override          |
|   [7]   | `--es_opt=json_types=true`                    | plugin option  | emit JSON type variants            |
|   [8]   | `--es_opt=valid_types=protovalidate_required` | plugin option  | protovalidate required valid types |
|   [9]   | `--es_opt=valid_types=legacy_required`        | plugin option  | proto2 legacy required valid types |

[ENTRYPOINT_SCOPE]: programmatic plugin object
- rail: codegen

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :--------------------------------------------------- | :------------- | :------------------------------ |
|   [1]   | `protocGenEs`                                        | `Plugin`       | exported plugin for `runNodeJs` |
|   [2]   | `runNodeJs(protocGenEs)` via `@bufbuild/protoplugin` | runner call    | Node.js plugin entrypoint       |

## [4]-[IMPLEMENTATION_LAW]

[CODEGEN_TOPOLOGY]:
- binary: `bin/protoc-gen-es`; invoked by `buf generate` as `local: protoc-gen-es` in `buf.gen.yaml`
- plugin entry: `protocGenEs` created via `createEcmaScriptPlugin` from `@bufbuild/protoplugin`
- generators: `generateTs`, `generateJs`, `generateDts` — one generated file per `.proto` input file, suffixed `_pb.ts`/`_pb.js`/`_pb.d.ts`
- option parsing: `parseOptions` reads `json_types` and `valid_types` key-value pairs; unknown keys throw
- `json_types`: accepts `true`, `1`, `false`, `0`; emits a sibling JSON type per message when enabled
- `valid_types`: accepts `protovalidate_required` and `legacy_required` joined by `+`; emits valid-type variants when the message needs one

[LOCAL_ADMISSION]:
- Admitted as a `buf.gen.yaml` local plugin; no programmatic import in application source.
- `protocGenEs` is consumed only by the `bin/protoc-gen-es` runner; downstream consumers import the generated `_pb.ts` files via `@bufbuild/protobuf`.
- Option keys and values are validated at generation time; build fails on unknown or malformed options.

[RAIL_LAW]:
- Package: `@bufbuild/protoc-gen-es`
- Owns: TypeScript and JavaScript code generation from `.proto` descriptors
- Accept: plugin options `json_types`, `valid_types`; targets `ts`, `js`, `dts`
- Reject: programmatic import into application code; hand-rolled TypeScript message shapes
