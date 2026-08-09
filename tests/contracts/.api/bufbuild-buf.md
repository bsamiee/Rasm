# [CONTRACTS_API_BUFBUILD_BUF]

`buf` gates the contract corpus: `lint` holds every `.proto` to the rules `buf.yaml` declares, `breaking` refuses a wire change against a prior tree, and `build` mints the `FileDescriptorSet` snapshot each source freezes beside it. One module — `tests/contracts` — feeds every verb, and a violation prints `path:line:col:message` under exit code 100.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@bufbuild/buf`
- package: `@bufbuild/buf` (Apache-2.0)
- module: three executables under `node_modules/.bin/` — `buf`, `protoc-gen-buf-lint`, `protoc-gen-buf-breaking`; a node shim dispatches to the platform binary an `@bufbuild/buf-<os>-<arch>` optional dependency carries
- runtime: node >=12; every verb resolves against the local module and reaches the registry only where a flag names it
- rail: proto lint, breaking refusal, descriptor-set minting, and formatting over the corpus

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `buf.yaml` v2 — the module and rule declaration `buf config init` scaffolds and every verb reads

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]                                                                |
| :-----: | :----------------------------------------- | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `version: v2`                              | enum          | the config generation; `v1beta1`/`v1` migrate through `buf config migrate`  |
|  [02]   | `modules: [{ path, name?, includes?, … }]` | struct        | one record per module root; `path` fixes what a proto path is relative to   |
|  [03]   | `lint.use` / `breaking.use`                | struct        | the categories in force, resolved through `buf config ls-*-rules`           |
|  [04]   | `lint.except` / `breaking.except`          | struct        | drops a named rule everywhere the config reaches                            |
|  [05]   | `lint.ignore` / `lint.ignore_only`         | struct        | drops every rule, or one named rule, for the listed paths ([A])             |
|  [06]   | `lint.service_suffix`                      | struct        | the suffix `SERVICE_SUFFIX` demands, default `Service`                      |
|  [07]   | `lint.enum_zero_value_suffix`              | struct        | the suffix `ENUM_ZERO_VALUE_SUFFIX` demands, default `_UNSPECIFIED`         |
|  [08]   | `lint.rpc_allow_same_request_response`     | struct        | admits one type on both sides of one rpc                                    |
|  [09]   | `lint.rpc_allow_google_protobuf_empty_*`   | struct        | admits `google.protobuf.Empty` as a request or response                     |
|  [10]   | `lint.allow_comment_ignores`               | struct        | admits an in-file `buf:lint:ignore <RULE>` comment directive                |
|  [11]   | `managed` (`buf.gen.yaml`)                 | struct        | rewrites file options during generation; `csharp_namespace` is an arm ([B]) |

- [IGNORE_PATHS]: buf resolves top-level `ignore` and `ignore_only` paths against the directory holding `buf.yaml`, so a root-seated config spells the full `tests/contracts/rasm/<family>/v1/<family>.proto`. `buf lint --error-format=config-ignore-yaml` prints a `version: v1` block whose paths are MODULE-relative — a starting point, never a v2 config to paste.
- [MANAGED_OVERRIDE]: managed mode accepts `csharp_namespace`, `csharp_namespace_prefix`, `go_package`, `java_package*`, `objc_class_prefix`, `php_namespace*`, `ruby_package*`, `optimize_for`, `cc_enable_arenas`, and the `jstype` field option; enabling it derives `csharp_namespace` from the package and overrides whatever a file declares inline.

[PUBLIC_TYPE_SCOPE]: lint categories — nested, each a superset of the one before, resolved from the binary through `buf config ls-lint-rules --version v2`

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                                                                          |
| :-----: | :---------- | :------------ | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `MINIMAL`   | enum          | package layout alone — `PACKAGE_DEFINED`, `PACKAGE_DIRECTORY_MATCH`, `PACKAGE_SAME_DIRECTORY`, cycles |
|  [02]   | `BASIC`     | enum          | `MINIMAL` plus casing, enum-zero, import, and per-package option-agreement rules                      |
|  [03]   | `STANDARD`  | enum          | `BASIC` plus `PACKAGE_VERSION_SUFFIX`, the rpc-naming trio, `SERVICE_SUFFIX`, `PROTOVALIDATE`         |
|  [04]   | `COMMENTS`  | enum          | non-empty comments on every enum, field, message, oneof, rpc, and service                             |
|  [05]   | `UNARY_RPC` | enum          | `RPC_NO_CLIENT_STREAMING`, `RPC_NO_SERVER_STREAMING` — refuses every streaming rpc                    |

[PUBLIC_TYPE_SCOPE]: breaking categories — chosen by what the corpus promises, resolved through `buf config ls-breaking-rules --version v2`

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                                                                      |
| :-----: | :---------- | :------------ | :------------------------------------------------------------------------------------------------ |
|  [01]   | `FILE`      | enum          | per-file identity — a moved message, renamed field, or deleted file fails; the strictest category |
|  [02]   | `PACKAGE`   | enum          | per-package identity — a type moves between files freely inside one package                       |
|  [03]   | `WIRE_JSON` | enum          | binary and JSON compatibility — a rename fails, a file move passes                                |
|  [04]   | `WIRE`      | enum          | binary compatibility alone — a field rename passes, a type or number change fails                 |
|  [05]   | `CSR`       | enum          | the registry-consumption profile, between `WIRE_JSON` and `PACKAGE`                               |

[PUBLIC_TYPE_SCOPE]: input formats and exit codes — the argument vocabulary every verb shares

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `dir` / `protofile` / `mod`                        | enum          | a source tree, one file with its deps, or a remote module |
|  [02]   | `binpb` / `json` / `txtpb` / `yaml`                | enum          | a prebuilt image or descriptor set in four encodings      |
|  [03]   | `git#branch=<b>` / `git#ref=<r>` / `tar` / `zip`   | enum          | a git ref or archive resolved without a checkout          |
|  [04]   | `--error-format text\|json\|github-actions\|junit` | enum          | violation rendering; `config-ignore-yaml` on `lint` alone |
|  [05]   | exit `0` / `100` / `1`                             | enum          | clean / rule violations found / config or build failure   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the gate verbs — each defaults its input to `.` and reads `buf.yaml` from the working directory upward

| [INDEX] | [SURFACE]                                                 | [SHAPE] | [CAPABILITY]                                                    |
| :-----: | :-------------------------------------------------------- | :------ | :-------------------------------------------------------------- |
|  [01]   | `buf lint [input]`                                        | command | rule violations at `path:line:col`; exit 100 when any fires     |
|  [02]   | `buf breaking [input] --against <input>`                  | command | refuses a change the chosen category forbids ([A])              |
|  [03]   | `buf breaking … --against-config <buf.yaml>`              | command | reads the baseline under the CURRENT rules ([B])                |
|  [04]   | `buf breaking … --against-registry`                       | command | compares to the registry default branch; excludes `--against`   |
|  [05]   | `buf build [input] -o <file>`                             | command | mints an image; `--as-file-descriptor-set` bares it ([C])       |
|  [06]   | `buf format [input] -w` / `-d --exit-code`                | command | canonical `.proto` layout; rewrite in place, or gate ([E])      |
|  [07]   | `buf generate [input] --template <buf.gen.yaml>`          | command | drives protoc plugins; unused here, see `[04]-[TOPOLOGY]`       |
|  [08]   | `buf ls-files [input] --format text\|json\|import`        | command | the file set a verb resolves, with or without imports           |
|  [09]   | `buf stats [input]`                                       | command | file, package, message, field, service, and rpc counts          |
|  [10]   | `buf export -o <dir>` / `buf convert --type --from --to`  | command | extract a source tree; recode one message across formats        |
|  [11]   | `buf config init` / `ls-lint-rules` / `ls-breaking-rules` | command | scaffold a v2 config; resolve the live rule set from the binary |
|  [12]   | `buf config migrate` / `ls-modules`                       | command | lift v1 config to v2; list the configured module roots          |
|  [13]   | `buf dep graph` / `dep update` / `dep prune`              | command | dependency graph and `buf.lock` maintenance                     |
|  [14]   | `protoc-gen-buf-lint` / `protoc-gen-buf-breaking`         | plugin  | the same rule engines as protoc plugins ([D])                   |

- [BREAKING_FILTERS]: `--path` filters the input AND the baseline, so filtering to a path absent from the baseline fails `image contains no files`; scope a per-source run by choosing the baseline, never by `--path`.
- [AGAINST_CONFIG]: without it, buf reads whatever config the baseline ref carries — a ref predating `buf.yaml` infers a module at its own root and every proto path shifts, so every file reads deleted.
- [BUILD_PARITY]: `--as-file-descriptor-set --exclude-imports --exclude-source-info` emits a bare, deterministic `FileDescriptorSet`; `--path` selects one source so the set holds exactly one file.
- [FORMAT_GATE]: `--exit-code` alone still writes every formatted file to stdout, so the gate form pairs it with `-d` and reads a diff; `-w` rewrites in place.
- [PROTOC_PLUGINS]: both read a `--buf-lint_opt` / `--buf-breaking_opt` JSON payload naming the config, so a protoc-driven build runs the corpus rules without buf owning generation.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Root `buf.yaml` declares one module at `tests/contracts`, so every proto path a rule, a violation, or a `buf.lock` names reads `rasm/<family>/v1/<family>.proto` from the corpus root while a `--path` argument and a top-level `ignore_only` entry stay workspace-relative. `PACKAGE_DIRECTORY_MATCH` therefore binds the directory to the package: `rasm.<family>.v1` lands at `rasm/<family>/v1/`, and a new family inherits that seat rather than re-deciding it.
- Snapshot and baseline are DIFFERENT artifacts. `<family>.descriptor.binpb` is a parity digest minted at `--exclude-imports` so three minters emitting one source agree byte-for-byte, each protoc bundle's well-known-type descriptors staying out of the comparison; that exclusion also leaves it unresolvable as an image, and `buf breaking --against <snapshot>` fails `could not resolve import` on any importing source. Git refs carry the baseline.
- `buf` owns the GATE axis alone — lint, breaking, formatting, and snapshot minting over one corpus. Each branch build owns its own generation inside the graph that consumes it, and no `buf.gen.yaml` exists: `Grpc.Tools` compiles the C# side through MSBuild at per-project `GrpcServices` and `Access` settings, `grpc_tools.protoc` runs in-process on the Python side, and `@bufbuild/protoc-gen-es` emits the TypeScript schemas. `protoc-gen-buf-lint` and `protoc-gen-buf-breaking` carry the corpus rules INTO those invocations, so one rule set reaches three builds with one authority per axis.
- `managed` mode owning all three loses: it derives `csharp_namespace` from the package and overrides the `option csharp_namespace = "Rasm.Channels"` both sources declare and `Rasm.Materials/Raster/set` binds by name, it forfeits per-project `GrpcServices` and `Access` control, and it forces committed generated output into three graphs that generate at build today.

[STACKING]:
- `@bufbuild/protobuf`(`libs/typescript/core/.api/bufbuild-protobuf.md`): `buf build --as-file-descriptor-set` emits exactly what `createFileRegistry(fileDescriptorSet)` decodes, so the snapshot this catalog mints is the runtime's descriptor-reflection input and `interchange/contract` walks it through `reflect`; that catalog owns the message runtime and this one owns the file that feeds it — a rule, category, or config key never appears there, and a codec entrypoint never appears here.
- `tests/contracts/README.md` (within-corpus edge): the corpus README owns the seam layout and the regeneration trigger, `MANIFEST.md` `[02.9]-[DESCRIPTOR_DRIFT]` owns the per-source snapshot rows, and this catalog owns the commands and rule vocabulary those two invoke.

[LOCAL_ADMISSION]:
- Invoke every verb as `node_modules/.bin/buf`, exactly as the corpus invokes each workspace tool; a globally-resolved `buf` binds no version the workspace pins.
- `buf.yaml` carries a refused rule as `except` where the estate's own ruled vocabulary displaces it everywhere, or `ignore_only` where one source's stated design departs; each entry carries its reason inline, and a lowered category standing in for either is refused.
- Each source regenerates its snapshot through `buf build --path <source> --as-file-descriptor-set --exclude-imports --exclude-source-info`; any other flag set forks the parity setting the drift contract compares.

[RAIL_LAW]:
- Package: `@bufbuild/buf`
- Owns: the proto rule engine (lint categories, breaking categories, per-rule `except`/`ignore_only`, the configurable suffix and rpc knobs), the deterministic `FileDescriptorSet` and image minting, canonical formatting, the file/stat/export/convert readers, and the two protoc plugins carrying the same engines into a foreign build
- Accept: one module root declared in a root `buf.yaml`, a git ref as the breaking baseline read under `--against-config`, the frozen per-source snapshot as a parity digest, and `@bufbuild/protobuf` as the runtime decoding what `build` emits
- Reject: a globally-resolved binary, a category lowered to quiet a finding, a snapshot minted at other flags, `--path` used to scope a baseline, a `managed` block rewriting an option a source declares inline, and a `buf.gen.yaml` standing beside a branch's own codegen
