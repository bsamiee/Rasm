# [API_CATALOGUE] @bufbuild/buf

`@bufbuild/buf` supplies the `buf`, `protoc-gen-buf-breaking`, and `protoc-gen-buf-lint` binaries — the build-time codegen driver and schema-governance CLI for the interchange rail: compile `.proto` to a Buf image or `FileDescriptorSet`, run `buf generate` over `buf.gen.yaml`, lint and breaking-change gate the C# wire source, `convert` fixtures between binary/text/JSON for the parity corpus, `curl` a live Connect/gRPC endpoint, `export` the C# protos into the workspace, and push modules to the BSR.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@bufbuild/buf`
- package: `@bufbuild/buf` (1.71.0, Apache-2.0)
- module format: no ESM/CJS API surface — `index.js` exports `{}`; three `bin` entries (`buf`, `protoc-gen-buf-breaking`, `protoc-gen-buf-lint`) shell the platform binary, never an application import
- runtime target: Node `>=12` launcher; the matching native binary resolves from an optional platform dep (`@bufbuild/buf-{darwin-arm64,darwin-x64,linux-aarch64,linux-x64,linux-armv7,win32-x64,win32-arm64}`)
- asset: platform-native `buf` executable (Go binary); build-time CLI only, no TypeScript declaration surface
- rail: codegen, schema-governance — build-time CLI only, never application-imported

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: binary executables under `bin/`
- rail: codegen

| [INDEX] | [SYMBOL]                  | [CAPABILITY]                                             |
| :-----: | :------------------------ | :------------------------------------------------------ |
|  [01]   | `buf`                     | primary CLI: build/generate/lint/breaking/convert/curl/export/push |
|  [02]   | `protoc-gen-buf-breaking` | `protoc`-plugin form of the breaking-change checker      |
|  [03]   | `protoc-gen-buf-lint`     | `protoc`-plugin form of the lint checker                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: core workspace commands
- rail: codegen

| [INDEX] | [SURFACE]              | [ENTRY_FAMILY] | [BOUNDARY_NOTE]                                              |
| :-----: | :--------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `buf build [input]`    | compile        | `.proto` → Buf image or `FileDescriptorSet`                 |
|  [02]   | `buf generate [input]` | codegen        | run `buf.gen.yaml` plugins (`protoc-gen-es` + capability leg) |
|  [03]   | `buf format [-w\|-d]`   | format         | normalize `.proto` whitespace/style in place or diff        |
|  [04]   | `buf lint [input]`     | lint           | run built-in lint rule groups against source                |
|  [05]   | `buf breaking [input]` | breaking       | detect wire/JSON/source breaking changes vs a baseline      |
|  [06]   | `buf ls-files [input]` / `buf stats [input]` | source query | list `.proto` files; report type/field statistics |

[ENTRYPOINT_SCOPE]: fixture, probe, and vendoring commands
- rail: codegen, schema-governance

The interchange-critical trio the older catalog omitted: `convert` mints the parity golden corpus, `curl` probes the live C# backend without hand-writing a client, `export` vendors the C# `.proto` tree into the TS workspace.

| [INDEX] | [SURFACE]                | [ENTRY_FAMILY] | [BOUNDARY_NOTE]                                                       |
| :-----: | :----------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `buf convert <input>`    | message codec  | `--from`/`--to` in `binpb\|json\|txtpb\|yaml`, `--type <fqn>`, `--validate` (protovalidate) |
|  [02]   | `buf curl <url>`         | RPC probe      | invoke a Connect/gRPC/gRPC-web endpoint à la cURL; schema from source/BSR/reflection |
|  [03]   | `buf export <source>`    | proto vendor   | `-o <dir>`, `--exclude-imports`, `--path`/`--exclude-path`, `--all`  |
|  [04]   | `buf dep update` / `prune` / `graph` | dep management | refresh/trim/print `buf.lock` BSR pins                    |
|  [05]   | `buf config init` / `config ls-lint-rules` / `config ls-breaking-rules` | config | scaffold `buf.yaml`; enumerate active rule sets |
|  [06]   | `buf push [source]` / `buf registry ...` | publish     | push a named module; manage BSR assets (superset of `push`)          |
|  [07]   | `buf lsp` / `buf beta ...` | tooling       | Buf Language Server; unstable beta verbs                            |

[ENTRYPOINT_SCOPE]: `buf generate` flags (CLI overrides of `buf.gen.yaml`)
- rail: codegen

| [INDEX] | [FLAG]              | [TYPE_FAMILY] | [BOUNDARY_NOTE]                                              |
| :-----: | :------------------ | :------------ | :---------------------------------------------------------- |
|  [01]   | `--template`        | string flag   | `buf.gen.yaml` path OR inline YAML/JSON template data       |
|  [02]   | `-o, --output`      | path flag     | base dir prepended to each plugin `out` (default `.`)       |
|  [03]   | `--include-imports` | bool flag     | also generate imported files (excludes WKT)                 |
|  [04]   | `--include-wkt`     | bool flag     | also generate well-known types; requires `--include-imports` |
|  [05]   | `--path` / `--exclude-path` | path flags | limit/exclude specific files or directories          |
|  [06]   | `--type`            | string flag   | restrict image to named `package/message/enum/service/method` |
|  [07]   | `--clean`           | bool flag     | delete plugin `out` targets before generation               |

[ENTRYPOINT_SCOPE]: `buf.gen.yaml` v2 plugin shape
- rail: codegen

| [INDEX] | [FIELD]                     | [TYPE_FAMILY] | [BOUNDARY_NOTE]                                        |
| :-----: | :-------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `version`                   | string        | `v1` or `v2` (interchange pins `v2`)                  |
|  [02]   | `clean`                     | bool          | delete `out` directories before generation            |
|  [03]   | `plugins[].local`           | string/array  | PATH binary name or path (`protoc-gen-es`, capability plugin) |
|  [04]   | `plugins[].remote`          | string        | BSR-hosted plugin reference with version              |
|  [05]   | `plugins[].out`             | string        | relative output directory                             |
|  [06]   | `plugins[].opt`             | string/array  | plugin option string(s) (`target=ts`, `json_types=true`) |
|  [07]   | `plugins[].include_imports` / `include_wkt` | bool | include imported/well-known files            |
|  [08]   | `plugins[].strategy`        | string        | `directory` or `all` invocation strategy              |
|  [09]   | `inputs[]` / `managed`      | list/map      | declared input sources; managed-mode field-option overrides |

[ENTRYPOINT_SCOPE]: `buf breaking` and `buf convert` flags
- rail: schema-governance

| [INDEX] | [FLAG]                   | [COMMAND]      | [BOUNDARY_NOTE]                                    |
| :-----: | :----------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `--against`              | breaking       | baseline source/module/image/git-ref (required unless registry) |
|  [02]   | `--against-registry`     | breaking       | compare against latest BSR default-branch commit   |
|  [03]   | `--exclude-imports` / `--limit-to-input-files` | breaking | scope the check to owned/input files   |
|  [04]   | `--error-format`         | build/lint/breaking | `text\|json\|msvs\|junit\|github-actions\|gitlab-code-quality` |
|  [05]   | `--from` / `--to` / `--type` / `--validate` | convert | payload location formats + full type name + protovalidate |

## [04]-[IMPLEMENTATION_LAW]

[CODEGEN_TOPOLOGY]:
- accepted `buf build`/`generate` inputs: `binpb`, `dir`, `git`, `json`, `mod`, `protofile`, `tar`, `txtpb`, `yaml`, `zip`; `buf build` output formats `binpb`/`json`/`txtpb`/`yaml`, default sink `/dev/null` (validate-only).
- `--as-file-descriptor-set` strips Buf image metadata to a bare `FileDescriptorSet`; `--exclude-imports`/`--exclude-source-info` trim image weight; `--type` restricts to named types.
- BSR dependency pins live in `buf.lock`; `buf dep update` refreshes, `buf dep prune` trims, `buf dep graph` prints.

[STACKS_WITH]:
- `@bufbuild/protoc-gen-es` + `protoc-gen-capability-es` (`.api/bufbuild-protoc-gen-es.md`, `.api/protoc-gen-capability-es.md`): `buf generate` reads the single `v2` `buf.gen.yaml` and runs both `local:` plugin rows on one pass — `protoc-gen-es` (message/service `_pb.ts`) then the capability-SDK plugin — over the C# `.proto`/descriptor source; `local:` names resolve from the pnpm-installed `bin/`, never a hand-run `protoc -I` or a per-developer plugin binary
- `@bufbuild/protobuf` (`.api/bufbuild-protobuf.md`): the emitted `<Name>Schema`/`GenService` tokens are the `create`/`createClient` inputs; `buf convert <input> --type <fqn> --from binpb --to json` (and the inverse) mints the `ONE_WIRE_FIXTURE_CORPUS` golden pairs — a C#-emitted `binpb` fixture and its canonical `json`/`txtpb` twin — `--validate` applying protovalidate at conversion so a malformed fixture fails at the boundary, not in a decode test
- `@connectrpc/connect-web` (`.api/connectrpc-connect-web.md`): `buf curl <baseUrl>/<pkg>.<Service>/<Method> --schema <source>` dials the live C# Connect/gRPC-web backend to confirm the wire before the TS `createConnectTransport` client is built — the transport-layer smoke that needs no generated client
- `effect` (`.api/effect.md`): buf never enters the runtime, so it stacks onto the effect rails purely as their build-time upstream — the `buf convert` corpus is what a `Codec/parity.md` `Schema.decodeUnknown`→`toJson` round-trip is `equals`-checked against, and `buf breaking --against <git-ref>`/`--against-registry` is the CI gate on the C# wire contract the effect `WireTransportLive` dial trusts, `--error-format github-actions` folding findings into the check run
- `descriptor.md` vendor/lint governance: `buf export <c#-proto-src> -o proto --exclude-imports` vendors the authoritative C# `.proto` tree into the workspace so `buf build`/`generate` read one owned source of truth, never a cross-repo path; `buf lint` runs the built-in rule groups, reading `buf.yaml`, never inline shell rule flags

[LOCAL_ADMISSION]:
- Admitted as a build-time CLI tool; no application-code import — `index.js` is an empty object.
- `buf.gen.yaml` is the single generation-config surface and `buf.yaml` the single workspace/lint/breaking surface; do not encode plugin options or rule sets in shell scripts.
- `buf breaking` requires `--against` unless `--against-registry` is set.

[RAIL_LAW]:
- Package: `@bufbuild/buf`
- Owns: workspace compile, codegen driver, lint, breaking-change gate, fixture conversion, live RPC probe, proto vendoring, BSR publish
- Accept: `buf.gen.yaml` (generation), `buf.yaml` (workspace/lint/breaking), `buf.lock` (BSR pins)
- Reject: hand-maintained `protoc -I` scripts, per-developer plugin binary management, inline shell lint/breaking rule flags
