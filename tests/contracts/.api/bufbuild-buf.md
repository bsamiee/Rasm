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

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]                                                               |
| :-----: | :----------------------------------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `version: v2`                              | enum          | the config generation; `v1beta1`/`v1` migrate through `buf config migrate` |
|  [02]   | `modules: [{ path, name?, includes?, … }]` | struct        | one record per module root; `path` fixes what a proto path is relative to  |
|  [03]   | `lint.use` / `breaking.use`                | struct        | the categories in force, resolved through `buf config ls-*-rules`          |
|  [04]   | `lint.except` / `breaking.except`          | struct        | drops a named rule everywhere the config reaches                           |
|  [05]   | `lint.ignore` / `lint.ignore_only`         | struct        | drops every rule, or one named rule, for the listed paths                  |
|  [06]   | `lint.service_suffix`                      | struct        | the suffix `SERVICE_SUFFIX` demands, default `Service`                     |
|  [07]   | `lint.enum_zero_value_suffix`              | struct        | the suffix `ENUM_ZERO_VALUE_SUFFIX` demands, default `_UNSPECIFIED`        |
|  [08]   | `lint.rpc_allow_same_request_response`     | struct        | admits one type on both sides of one rpc                                   |
|  [09]   | `lint.rpc_allow_google_protobuf_empty_*`   | struct        | admits `google.protobuf.Empty` as a request or response                    |
|  [10]   | `lint.allow_comment_ignores`               | struct        | admits an in-file `buf:lint:ignore <RULE>` comment directive               |
|  [11]   | `modules[].lint` / `modules[].breaking`    | struct        | one module's own rule config, REPLACING the default whole                  |

- [IGNORE_PATHS]: buf resolves top-level `ignore` and `ignore_only` paths against the directory holding `buf.yaml`, so a root-seated config spells the full `tests/contracts/rasm/<family>/v1/<family>.proto`. `buf lint --error-format=config-ignore-yaml` prints a `version: v1` block whose paths are MODULE-relative — a starting point, never a v2 config to paste.
- [MODULE_SCOPE]: a module-level `lint`/`breaking` block overrides the workspace default in its ENTIRETY with nothing merged, and `disable_builtin: true` is how a module carries no rule at all. Two `modules` entries may share a `path` while no `.proto` is shared, and an `excludes` directory is a lawful `path` for a second entry.

[PUBLIC_TYPE_SCOPE]: `buf.gen.yaml` v2 — the generation template `buf generate` reads, seated at the repo root beside `buf.yaml`

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------------ | :------------ | :------------------------------------------------------------- |
|  [01]   | `version: v2`                                           | enum          | the template generation                                        |
|  [02]   | `clean: true`                                           | struct        | deletes every plugin `out` root before generation runs         |
|  [03]   | `inputs: [{ directory }]`                               | struct        | a LOCAL module root or workspace root                          |
|  [04]   | `inputs: [{ module }]`                                  | struct        | a BSR module reference; a local path fails `invalid mod path`  |
|  [05]   | `inputs: [{ proto_file, include_package_files? }]`      | struct        | one source, optionally with its package peers                  |
|  [06]   | `inputs: [{ git_repo, tarball, zip_archive, *_image }]` | struct        | a ref, archive, or prebuilt image resolved without a checkout  |
|  [07]   | `inputs[].paths` / `exclude_paths`                      | struct        | filters the input; entries read as `--path` does               |
|  [08]   | `inputs[].types` / `exclude_types`                      | struct        | narrows the input to a named type closure                      |
|  [09]   | `plugins: [{ local, out, opt, strategy }]`              | struct        | a locally-run plugin with its `out` root and `opt`             |
|  [10]   | `plugins: [{ remote }]` / `{ protoc_builtin }`          | struct        | a BSR-hosted plugin, or a protoc built-in under `protoc_path`  |
|  [11]   | `plugins[].include_imports` / `include_wkt`             | struct        | widens the emission past the targeted files; the CLI flag wins |
|  [12]   | `managed: { enabled, disable, override }`               | struct        | rewrites file and field options during generation              |

- [TEMPLATE_SEAT]: `buf generate` reads `buf.gen.yaml` from the working directory alone — it walks up for `buf.yaml`, never for the template — so the root seat beside `buf.yaml` is the one every verb discovers from the repo root with no `--template`. `local` accepts a `${PATH}` name or a file path; the estate spells `node_modules/.bin/protoc-gen-es`, so a bare `node_modules/.bin/buf generate` runs outside a pnpm or nx shell where the bare name fails `executable file not found in $PATH`. `clean: true` sweeps from each `out` every file the run did not write and keeps the root; `-o <dir>` prepends a base to every `out`.
- [GENERATE_INPUT]: a v2 template's `inputs` and its plugin `out` both resolve against the WORKING DIRECTORY, never against the template's own location, and `paths`/`exclude_paths` entries resolve there too. The workspace root and a declared module's own path are both lawful `directory` inputs over one module and target the same files; a path BELOW a declared module root refuses as contained by that module, and a `proto_file` or `paths` entry naming an `excludes` tree fails `no .proto files were targeted`. Emitted paths stay MODULE-relative under every form.
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
|  [02]   | `buf breaking [input] --against <input>`                  | command | refuses a change the chosen category forbids                    |
|  [03]   | `buf breaking … --against-config <buf.yaml>`              | command | reads the baseline under the CURRENT rules                      |
|  [04]   | `buf breaking … --against-registry`                       | command | compares to the registry default branch; excludes `--against`   |
|  [05]   | `buf build [input] -o <file>`                             | command | mints an image; `--as-file-descriptor-set` bares it             |
|  [06]   | `buf format [input] -w` / `-d --exit-code`                | command | canonical `.proto` layout; rewrite in place, or gate            |
|  [07]   | `buf generate [input] --template <buf.gen.yaml>`          | command | drives protoc plugins from the ROOT template; see `[02]`        |
|  [08]   | `buf ls-files [input] --format text\|json\|import`        | command | the file set a verb resolves, with or without imports           |
|  [09]   | `buf stats [input]`                                       | command | file, package, message, field, service, and rpc counts          |
|  [10]   | `buf export -o <dir>` / `buf convert --type --from --to`  | command | extract a source tree; recode one message across formats        |
|  [11]   | `buf config init` / `ls-lint-rules` / `ls-breaking-rules` | command | scaffold a v2 config; resolve the live rule set from the binary |
|  [12]   | `buf config migrate` / `ls-modules`                       | command | lift v1 config to v2; list the configured module roots          |
|  [13]   | `buf dep graph` / `dep update` / `dep prune`              | command | dependency graph and `buf.lock` maintenance                     |
|  [14]   | `protoc-gen-buf-lint` / `protoc-gen-buf-breaking`         | plugin  | the same rule engines as protoc plugins                         |

- [BREAKING_FILTERS]: `--path` filters the input AND the baseline, so filtering to a path absent from the baseline fails `image contains no files`; scope a per-source run by choosing the baseline, never by `--path`.
- [AGAINST_CONFIG]: absent `--against-config`, buf reads whatever config the baseline ref carries — a ref predating `buf.yaml` infers a module at its own root and every proto path shifts, so every file reads deleted.
- [BUILD_PARITY]: `--as-file-descriptor-set --exclude-imports --exclude-source-info` emits a bare, deterministic `FileDescriptorSet`; `--path` selects one source so the set holds exactly one file.
- [FORMAT_GATE]: `--exit-code` alone still writes every formatted file to stdout, so the gate form pairs it with `-d` and reads a diff; `-w` rewrites in place.
- [PROTOC_PLUGINS]: both read a `--buf-lint_opt` / `--buf-breaking_opt` JSON payload naming the config, so a protoc-driven build runs the corpus rules without buf owning generation.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Root `buf.yaml` declares one module at `tests/contracts`, so every proto path a rule, a violation, or a `buf.lock` names reads `rasm/<family>/v1/<family>.proto` from the corpus root while a `--path` argument and a top-level `ignore_only` entry stay workspace-relative. `PACKAGE_DIRECTORY_MATCH` therefore binds the directory to the package: `rasm.<family>.v1` lands at `rasm/<family>/v1/`, and a new family inherits that seat rather than re-deciding it.
- Snapshot and baseline are DIFFERENT artifacts. `<family>.descriptor.binpb` is a parity digest minted at `--exclude-imports` so three minters emitting one source agree byte-for-byte, each protoc bundle's well-known-type descriptors staying out of the comparison; that exclusion also leaves it unresolvable as an image, and `buf breaking --against <snapshot>` fails `could not resolve import` on any importing source. Git refs carry the baseline.
- `buf` owns BOTH axes at the repo root — `buf.yaml` the gate (lint, breaking, formatting, snapshot minting) and the sibling `buf.gen.yaml` the generation — over one corpus module, and the consumer's own build decides who takes a template row: a build already driving protoc keeps it, a build driving none takes one row. `Grpc.Tools` compiles the C# side through MSBuild `<Protobuf>` items at per-project `GrpcServices` and `Access` settings and `grpc_tools.protoc` runs in-process on the Python side, both off the same corpus root and neither committing output, so a template row for either would mint committed files beside an in-build producer of the same names (buf's BSR `protocolbuffers/csharp|python` rows also send the schema to the registry, and `grpcio-tools`' service generator is no protoc-plugin binary buf can run). TypeScript has no protoc, so `@bufbuild/protoc-gen-es` takes the root template's one row today. `protoc-gen-buf-lint` and `protoc-gen-buf-breaking` carry the corpus rules INTO the native invocations, so one rule set reaches three builds with one authority per axis. EXECUTION is the corpus-side gap `MANIFEST.md` `[02.9]` records at the same grain: buf mints every snapshot here while the C# and Python trees run no generator over those sources and compare no emission byte-for-byte, so three-minter parity holds by construction and stays unrun at those two peers.
- The root template names the corpus MODULE as its `directory` input — `tests/contracts` from the repo root, where `buf generate` discovers the template with no `--template` and `node_modules/.bin/protoc-gen-es` resolves as a file path under no `${PATH}` shell. The workspace default reaches the same files while one module stands and emits byte-identical output, so the module form is chosen for what it forecloses: a second `modules` entry would silently widen every plugin row onto a corpus no consumer asked for, where the named form admits it as one deliberate `inputs` row. Emitted paths are MODULE-relative under either form, which is what makes `rasm/<family>/v1/<family>_pb.ts` the import shape `typescript:core/interchange/format#PROTO_ENGINE` binds. `nx run workspace-foundation:proto` is the ONE entry: the root `package.json` `proto` target hashes `buf.yaml`, the template, every corpus `.proto`, and the `@bufbuild/buf` + `@bufbuild/protoc-gen-es` packages, caches each `out` tree as `outputs`, and `typecheck` depends on it, so a cache hit restores the emission and a corpus edit regenerates before the compiler reads; `pnpm proto` runs the same command uncached. Generated trees are never committed — the emission derives from the corpus plus a lockfile-pinned plugin, `clean: true` sweeps what a run did not write, each file's header stamps plugin version and options — and a new consumer is one `plugins` row whose `out` seats inside the consuming tree, landing with its `.gitignore` row and the nx `outputs` entry as one fact (`docs/laws/topology.md`).
- `excludes` is a GENERATION carve as well as a gate carve: `tests/contracts/io` leaves the module entirely, so no input form reaches the vendored CloudEvents source and the TypeScript binding it owes has no producer. Closing that owes a second `modules` entry at `path: tests/contracts/io` carrying `lint`/`breaking` blocks set to `disable_builtin: true`, a `--path` scope on the `buf format` gate so the vendored bytes keep the `docs/laws/scars.md` `[FROZEN_FOREIGN_ARTIFACT]` carve the current exclusion gives them for free, and a second `inputs` row in the root template whose plugin `out` seats BESIDE the estate tree, since `CloudEvent` collides with the message-envelope class every consumer imports. Until all three land together, the second module trades one carve for the other and the exclusion stands.
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
- Accept: one module root declared in a root `buf.yaml`, a git ref as the breaking baseline read under `--against-config`, the frozen per-source snapshot as a parity digest, `@bufbuild/protobuf` as the runtime decoding what `build` emits, and the root `buf.gen.yaml` naming that module as its `directory` input with one `plugins` row per consumer whose build drives no protoc
- Reject: a globally-resolved binary, a category lowered to quiet a finding, a snapshot minted at other flags, `--path` used to scope a baseline, a `managed` block rewriting an option a source declares inline, a `buf.gen.yaml` seated at this corpus or inside one branch, a template row for a consumer whose build already drives protoc, and a committed `out` tree
