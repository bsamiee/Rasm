# [API_CATALOGUE] @bufbuild/protoc-gen-es

`@bufbuild/protoc-gen-es` supplies the `protoc-gen-es` binary and the `protocGenEs` plugin object — the `buf generate` `local:` plugin that emits one `_pb.ts`/`_pb.js`/`_pb.d.ts` per `.proto` input, each exporting the `<Name>Schema` (`GenMessage`) message tokens, `GenService` service tokens, and optional JSON/valid-type variants the `@bufbuild/protobuf` runtime and `@connectrpc/connect` `createClient` consume.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@bufbuild/protoc-gen-es`
- package: `@bufbuild/protoc-gen-es` (2.12.1, Apache-2.0)
- module format: CJS only (no `exports` map, no `type: module`); the `protoc-gen-es` `bin` entry plus the CJS plugin module `dist/cjs/src/protoc-gen-es-plugin.js` (`protocGenEs`), reachable only as a binary — never a barrel import
- runtime target: Node `>=20` build-time plugin; peer-depends `@bufbuild/protobuf`, version-locked to the runtime the emitted `_pb.ts` binds against (a runtime/plugin skew is a restore-time failure, never a silent drift)
- asset: `bin/protoc-gen-es` executable run by `buf generate`; emits one `_pb.ts`/`_pb.js`/`_pb.d.ts` per `.proto`, the emitted `.d.ts` the sole verification surface for the generated tokens
- rail: codegen — `buf.gen.yaml` `local:` plugin, never application-imported

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: plugin export
- rail: codegen

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]                    | [BOUNDARY_NOTE]                                              |
| :-----: | :------------ | :------------------------------- | :---------------------------------------------------------- |
|  [01]   | `protocGenEs` | `@bufbuild/protoplugin` `Plugin` | `createEcmaScriptPlugin({ name, version, parseOptions, generateTs, generateJs, generateDts })`; consumed only by `bin/protoc-gen-es`'s `runNodeJs` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: CLI invocation and plugin options
- rail: codegen

Options ride `buf.gen.yaml` `plugins[].opt` (or protoc `--es_opt=`). Corrected defaults: `target` defaults `js+dts` (multi-value `+`-joined), and `import_extension` defaults `none`.

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [BOUNDARY_NOTE]                                              |
| :-----: | :------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `protoc-gen-es`                        | binary         | the `buf generate` `local:` plugin entrypoint               |
|  [02]   | `target=ts` / `target=js` / `target=dts` | plugin option | output flavor; combine via `+` (default `js+dts`)          |
|  [03]   | `import_extension=none\|js\|ts`         | plugin option  | emitted import specifier extension (default `none`)         |
|  [04]   | `json_types=true\|1\|false\|0`          | plugin option  | emit the sibling `<Name>Json` type + `MessageJsonType` mirror |
|  [05]   | `valid_types=protovalidate_required\|legacy_required` | plugin option | (experimental) emit `<Name>ValidType`; `+`-joined |
|  [06]   | `js_import_style=module\|legacy_commonjs` | plugin option | emitted import/export style (default `module`)           |
|  [07]   | `ts_nocheck=true`                       | plugin option  | prepend `@ts-nocheck` for non-standard compiler settings   |
|  [08]   | `keep_empty_files=true` / `elide_plugin_version=true` | plugin option | emit files with no output; drop the plugin version stamp |

[ENTRYPOINT_SCOPE]: generated `_pb.ts` output shape
- rail: codegen

Every emitted file boots its `DescFile` via `@bufbuild/protobuf/codegenv2` `fileDesc(b64)` and exports these tokens; downstream interchange source imports the tokens, never the boot helpers.

| [INDEX] | [EMITTED_SYMBOL]         | [ENTRY_FAMILY]  | [BOUNDARY_NOTE]                                                    |
| :-----: | :----------------------- | :-------------- | :---------------------------------------------------------------- |
|  [01]   | `<Name>Schema`           | `GenMessage`    | `messageDesc(file, path)`; the `create`/`fromBinary`/`toBinary` input |
|  [02]   | `<Name>` (type)          | `Message<...>`  | the `MessageShape` interface for the message                      |
|  [03]   | `<Name>Json` (type)      | JSON mirror     | present only with `json_types=true`                               |
|  [04]   | `<Name>ValidType` (type) | valid mirror    | present only with `valid_types`; required-field-narrowed          |
|  [05]   | `<Enum>` + `<Enum>Schema` | `GenEnum`+enum | `tsEnum(desc)` object + `enumDesc(file, path)` token              |
|  [06]   | `<Service>`              | `GenService`    | `serviceDesc(file, path)`; the `createClient(<Service>, transport)` input |

## [04]-[IMPLEMENTATION_LAW]

[CODEGEN_TOPOLOGY]:
- `protocGenEs` = `createEcmaScriptPlugin` binding `parseOptions` (reads `json_types`/`valid_types`/`target`/`import_extension`), `generateTs`, `generateJs`, `generateDts`; one output file per `.proto` input suffixed `_pb.ts`/`_pb.js`/`_pb.d.ts`.
- `json_types` accepts `true`/`1`/`false`/`0`; `valid_types` accepts `protovalidate_required` and `legacy_required` joined by `+` and emits a valid-type variant only for messages that need one (`messageNeedsCustomValidType`).
- Internal `valid-types` helpers (`messageNeedsCustomValidType`, `isProtovalidateDisabled`) are not re-exported; the package has no `exports` map — only `bin/` and the CJS plugin module are reachable.

[STACKS_WITH]:
- `@bufbuild/protobuf` (`.api/bufbuild-protobuf.md`): the emitted `<Name>Schema` (`GenMessage`) is the ONE argument `create(<Name>Schema, init)`/`fromBinary(<Name>Schema, bytes)`/`toJson(<Name>Schema, msg)` discriminate on — the generated file carries the descriptor via `@bufbuild/protobuf/codegenv2` `fileDesc(b64)`, never a `new`-able class; `valid_types=protovalidate_required` emits `<Name>ValidType` (`MessageValidType<Desc>`) narrowing proto2/edition + protovalidate `required` fields to non-optional, and `json_types=true` emits `<Name>Json` (`MessageJsonType<Desc>`)
- `@connectrpc/connect` (`.api/connectrpc-connect.md`): the emitted `<Service>` `GenService` is the `createClient(<Service>, WireTransportLive)` input the `Transport/transport.md` fold binds one client per service against
- `@bufbuild/buf` (`.api/bufbuild-buf.md`): runs as the FIRST `local:` plugin row on the single `v2` `buf.gen.yaml` pass beside the capability-SDK plugin, both reading the same C# descriptor source; option validation is at generation time — an unknown or malformed `opt` key throws and fails `buf generate`, never a silent skip
- `protoc-gen-capability-es` (`.api/protoc-gen-capability-es.md`): the SECOND `local:` plugin on the same pipeline; the message-and-service `_pb.ts` this plugin emits is the tree the capability leg's `CommandService`/wire messages resolve from, never a parallel `buf.gen.yaml`
- `effect` (`.api/effect.md`): the `Codec/codec.md` dispatch table keys each proto row to its `<Name>Schema`; `<Name>ValidType` is the type the `Ingress/refinement.md` decode-enforcement rail brands as already-validated, so a validated message needs no re-check downstream, and `<Name>Json` makes the JSON codec leg fully typed rather than `JsonValue`-erased

[LOCAL_ADMISSION]:
- Admitted as a `buf.gen.yaml` `local:` plugin; no programmatic import in application source — downstream code imports the emitted `_pb.ts` tokens via `@bufbuild/protobuf`.
- The `peerDependency` on `@bufbuild/protobuf` locks the generator to the runtime version it emits against; a runtime/plugin version skew is a restore-time failure, not a silent drift.

[RAIL_LAW]:
- Package: `@bufbuild/protoc-gen-es`
- Owns: TypeScript/JavaScript code generation from `.proto` descriptors
- Accept: plugin options `target`/`import_extension`/`json_types`/`valid_types`/`ts_nocheck`
- Reject: programmatic import into application code, hand-rolled TypeScript message shapes, a `new`-able generated class
