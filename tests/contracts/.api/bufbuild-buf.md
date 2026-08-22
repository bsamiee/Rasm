# [CONTRACTS_API_BUFBUILD_BUF]

`buf` owns both axes of the contract corpus from the repo root: `buf.yaml` gates every `.proto` (`lint`, `format --diff`, `breaking` FILE against `main`) and `buf.gen.yaml` drives every committed binding tree through protoc plugins buf itself resolves. Rule violations exit 100 with `path:line:col:message`; every other non-zero exit is a tool failure.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@bufbuild/buf`
- package: `@bufbuild/buf` (Apache-2.0)
- module: `buf`, `protoc-gen-buf-lint`, `protoc-gen-buf-breaking` under `node_modules/.bin/`, a node shim over the `@bufbuild/buf-<os>-<arch>` binary
- runtime: node; verbs reach the registry only where a flag names it, and `protoc_builtin` rows take the machine `protoc` buf never ships
- rail: proto lint, canonical-format diff, FILE-breaking refusal against `main`, image building, and plugin-driven generation over the corpus

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `buf.yaml` v2 — the gate axis every verb reads from the working directory upward

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]                                                               |
| :-----: | :----------------------------------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `version: v2`                              | enum          | the config generation; `v1beta1`/`v1` migrate through `buf config migrate` |
|  [02]   | `modules: [{ path, name?, includes?, … }]` | struct        | one record per module root; `path` fixes what a proto path is relative to  |
|  [03]   | `modules[].lint` / `modules[].breaking`    | struct        | one module's own rule config, REPLACING the workspace default whole        |
|  [04]   | `modules[].<axis>.disable_builtin: true`   | struct        | a module carrying no rule at all; `lint`/`breaking` WARN and pass it       |
|  [05]   | `lint.use` / `breaking.use`                | struct        | the categories in force, resolved through `buf config ls-*-rules`          |
|  [06]   | `lint.except` / `breaking.except`          | struct        | drops a named rule everywhere the config reaches                           |
|  [07]   | `lint.ignore` / `lint.ignore_only`         | struct        | drops every rule, or one named rule, for `buf.yaml`-relative paths         |
|  [08]   | `breaking.ignore` / `breaking.ignore_only` | struct        | the breaking-axis twins of row [07]                                        |
|  [09]   | `lint.service_suffix`                      | struct        | the suffix `SERVICE_SUFFIX` demands, default `Service`                     |
|  [10]   | `lint.enum_zero_value_suffix`              | struct        | the suffix `ENUM_ZERO_VALUE_SUFFIX` demands, default `_UNSPECIFIED`        |
|  [11]   | `lint.rpc_allow_*`                         | struct        | admits one type on both rpc sides, or `google.protobuf.Empty` on either    |
|  [12]   | `lint.allow_comment_ignores`               | struct        | admits an in-file `buf:lint:ignore <RULE>` comment directive               |
|  [13]   | `deps: [<bsr-module>]` + `buf.lock`        | struct        | registry dependencies `buf dep update` pins; the protovalidate seat        |

- `modules[].lint`/`modules[].breaking`: a module block replaces the workspace default whole; nothing merges.
- `disable_builtin: true`: `buf lint`/`buf breaking` warn `No lint rules are configured` for that module while its sibling grades; read the exit code.
- `lint.ignore`/`ignore_only`/`breaking.ignore`: paths resolve against the `buf.yaml` directory; `managed.disable` `path:` is module-relative.

[PUBLIC_TYPE_SCOPE]: `buf.gen.yaml` v2 — the generation axis `buf generate` reads from the working directory alone, seated at the repo root beside `buf.yaml`

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :---------------------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `version: v2`                                   | enum          | the template generation                                           |
|  [02]   | `clean: true`                                   | struct        | POST-run sweep of every file under an `out` the run did not write |
|  [03]   | `managed: { enabled, disable, override }`       | struct        | rewrites file and field options in every plugin's image           |
|  [04]   | `managed.disable: [{ path }, { file_option }]`  | struct        | opts a module-relative path or one file option out of rewriting   |
|  [05]   | `managed.override: [{ file_option, value, … }]` | struct        | prefix, suffix, and value rows keyed by `file_option`, scopable   |
|  [06]   | `inputs: [{ directory }]`                       | struct        | workspace or module root; ONE `directory: .` carries every module |
|  [07]   | `inputs: [{ module }, { proto_file }, …]`       | struct        | a BSR module, one source, a ref, an archive, or an image          |
|  [08]   | `inputs[].paths` / `exclude_paths`              | struct        | filters the input; entries read as `--path` does                  |
|  [09]   | `plugins[].local` / `remote` / `protoc_builtin` | struct        | literal binary name or path, BSR plugin, or protoc built-in       |
|  [10]   | `plugins[].protoc_path`                         | struct        | protoc binary for `protoc_builtin`; default `protoc` on `${PATH}` |
|  [11]   | `plugins[].out` / `opt` / `strategy`            | struct        | out root, option list, `directory` (default) or `all`             |
|  [12]   | `plugins[].types` / `exclude_types`             | struct        | per-input image filter; package tokens and FQNs                   |
|  [13]   | `plugins[].include_imports` / `include_wkt`     | struct        | widens emission past the targeted files; the CLI flag wins        |

- `clean: true`: sweeps AFTER a successful run — every file under each `out` the run did not write, emptied subdirectories included; the root stays.
- `clean: true`: written paths union across plugins sharing one `out` and across a nested `out`; a failing plugin cancels the sweep and its siblings.
- `-o <dir>`: prepends `<dir>` to every `out` and scopes the sweep there, so a scratch regeneration mirrors the committed trees untouched.
- `types`/`exclude_types`: resolve against EACH input's image — per-module inputs fail `exclusion of type "<pkg>": not found` on the other module.
- `types`/`exclude_types`: a package token drops the whole file; a message token strips the types and hands the emptied file to a per-file builtin.
- `managed.disable` `path:`: module-root relative — the workspace spelling matches nothing silently; `module:` never matches a local nil-name module.
- `managed`: `disable` beats `override`; `field_option` admits `jstype` alone; WKTs skip; `go_package` moves only under an override.
- plugin failure: one failing plugin cancels its siblings, each printing `signal: killed` — the real error is the one other line.

[PUBLIC_TYPE_SCOPE]: managed defaults — the file options `managed.enabled: true` OVERWRITES in every embedded descriptor, never fills

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                         |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `csharp_namespace`       | enum          | PascalCase per package segment, `.`-joined                           |
|  [02]   | `java_package`           | enum          | `com.<package>`; `java_package_prefix`/`_suffix` rows recompose it   |
|  [03]   | `java_outer_classname`   | enum          | PascalCase of the proto file name                                    |
|  [04]   | `java_multiple_files`    | enum          | `true`                                                               |
|  [05]   | `objc_class_prefix`      | enum          | uppercased segment initials padded to three with `X`; `GPB` → `GPX`  |
|  [06]   | `php_namespace`          | enum          | PascalCase per package segment, `\`-joined                           |
|  [07]   | `php_metadata_namespace` | enum          | `<php_namespace>\GPBMetadata`                                        |
|  [08]   | `ruby_package`           | enum          | PascalCase per package segment, `::`-nested                          |

[PUBLIC_TYPE_SCOPE]: lint categories — nested, each a superset of the one before, resolved through `buf config ls-lint-rules --version v2`

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

[PUBLIC_TYPE_SCOPE]: input formats, error formats, and exit codes — the argument vocabulary every verb shares

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :--------------------------------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `dir` / `protofile` / `mod`                    | enum          | a source tree, one file with its deps, or a remote module               |
|  [02]   | `binpb` / `json` / `txtpb` / `yaml`            | enum          | a prebuilt image or descriptor set in four encodings                    |
|  [03]   | `git#branch=<b>` / `git#ref=<r>` / `tar`/`zip` | enum          | a git ref or archive resolved without a checkout                        |
|  [04]   | `--error-format <format>`                      | enum          | `text` `json` `msvs` `junit` `github-actions` `gitlab-code-quality`     |
|  [05]   | `json` violation record                        | struct        | NDJSON `path`, `start_line`, `start_column`, `end_*`, `type`, `message` |
|  [06]   | exit `0` / `100` / `1`                         | enum          | clean / rule violations / config, build, module, or plugin failure      |

- exit `1`: stderr carries the single `Failure: …` line and stdout stays EMPTY, so the verdict reads stderr, never stdout rows.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the gate and generation verbs — each defaults its input to `.` and reads `buf.yaml` from the working directory upward

| [INDEX] | [SURFACE]                                                 | [SHAPE] | [CAPABILITY]                                                   |
| :-----: | :-------------------------------------------------------- | :------ | :------------------------------------------------------------- |
|  [01]   | `buf lint [input]`                                        | command | rule violations at `path:line:col`; exit 100 when any fires    |
|  [02]   | `buf format [input] --diff --exit-code`                   | command | canonical `.proto` layout as a diff; `-w` rewrites in place    |
|  [03]   | `buf breaking [input] --against <input> --against-config` | command | refuses a change the category forbids under CURRENT rules      |
|  [04]   | `buf build [input] -o <file>`                             | command | mints an image; `--as-file-descriptor-set` bares it            |
|  [05]   | `buf generate [input] [--template] [-o <dir>] [--clean]`  | command | drives every template plugin row; `-o` prefixes each `out`     |
|  [06]   | `buf ls-files [input] --format text\|json\|import`        | command | the file set a verb resolves, with or without imports          |
|  [07]   | `buf stats [input]`                                       | command | file, package, message, field, service, and rpc counts         |
|  [08]   | `buf config ls-modules` / `ls-*-rules`                    | command | the configured module roots; the live rule set from the binary |
|  [09]   | `buf config init` / `migrate`                             | command | scaffold a v2 config; lift v1 config to v2                     |
|  [10]   | `buf dep update` / `graph` / `prune`                      | command | `buf.lock` maintenance and the dependency graph                |
|  [11]   | `buf export -o <dir>` / `buf convert --type --from --to`  | command | extract a source tree; recode one message across formats       |
|  [12]   | `buf curl --protocol connect\|grpc\|grpcweb`              | command | call a running service; `--reflect` needs HTTP/2               |
|  [13]   | `protoc-gen-buf-lint` / `protoc-gen-buf-breaking`         | plugin  | the same rule engines as protoc plugins                        |

- `buf lint --path <p>`: refuses a module root (`specify this module path directly as an input`); it names a file or subdirectory.
- `buf breaking --path`: filters the input AND the baseline, so a path absent from the baseline fails `image contains no files`.
- `buf breaking`: fails exit 1 `Module "path: …" had no .proto files` on a module the baseline lacks — a tool-failure form, never exit 100.
- `buf breaking` without `--against-config`: reads the baseline ref's own config, so a ref predating the module set re-roots every path as deleted.
- `buf format --exit-code`: alone writes every formatted file to stdout — the gate form pairs it with `--diff`.
- `buf format`: has shipped non-idempotent releases, so the gate diffs and never writes.
- `buf generate` `local:`: takes the literal binary name or path — a path resolves as a file, a bare name off `${PATH}`; no `protoc-gen-` prefixing.
- `buf build -o /dev/null`: proves the workspace compiles with no artifact; `buf ls-files` is the cheapest census of what every verb resolves.
- `buf build --as-file-descriptor-set`: bares the image to the `FileDescriptorSet` every runtime decodes; the raw image carries buf-only fields.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Root `buf.yaml` declares the estate module at `tests/contracts/proto` and one module per publisher at `tests/contracts/vendor/<publisher>/proto`.
- Each publisher module lands as one `modules` row under `disable_builtin` on both axes with one `managed.disable` `path:` row; no input row changes.
- `buf format --diff --exit-code tests/contracts/proto` names the estate module alone, so no publisher byte enters the format path.
- Root `buf.gen.yaml` feeds ONE workspace input (`directory: .`) to every plugin row; type filters resolve against that one image as package tokens.
- `protoc_builtin: csharp` and `local: grpc_csharp_plugin` ride the machine `protoc` tier through the `protoc_path` `${PATH}` default.
- `protoc-gen-es`, `protoc-gen-py`, and `protoc-gen-connectrpc` ride the pnpm catalog and the uv venv, each pinned as a pair with its runtime.
- `protoc-gen-jsonschema` rides the machine estate; `assay contracts` drives it through an inline template, never a `buf.gen.yaml` row.

[STACKING]:
- `@bufbuild/protobuf`(`libs/typescript/core/.api/bufbuild-protobuf.md`): `createFileRegistry(fileDescriptorSet)` decodes the bared image.
- `protobuf-py`(`libs/python/.api/protobuf-py.md`): `protobuf.wkt.FileDescriptorSet.from_binary` decodes the bared image the corpus gate reads.
- `tests/contracts/README.md` (within-corpus edge): the README owns authority, layout, and regeneration; this catalog owns commands, keys, and rules.

[LOCAL_ADMISSION]:
- Invoke every verb as `node_modules/.bin/buf` from the repo root; a global `buf` binds no pinned version and a foreign cwd re-roots every path.
- `buf.yaml` carves a rule as `except` where ruled vocabulary displaces it everywhere, else `ignore_only` on the one departing source.
- Every carve row states its why inline; a lowered category standing in for a carve is refused.
- Template rows carry one load-bearing clause per option and per carve; an option nothing reads is not added.

[RAIL_LAW]:
- Package: `@bufbuild/buf`
- Owns: the proto rule engine, canonical formatting, image minting, plugin generation with managed options and the post-run sweep, and the readers
- Accept: root `buf.yaml` module roots, a git-ref baseline under `--against-config`, one workspace input with package filters, generated-only outs
- Reject: a global binary, a lowered category, `breaking.ignore` or a second `--against`, a descriptor snapshot, a per-module input, a second driver
