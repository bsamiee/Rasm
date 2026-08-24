# [CONTRACTS_API_BUFBUILD_BUF]

`buf` owns both axes of the contract corpus from the repo root: `buf.yaml` names `buf.build/rasm/contracts` and gates every `.proto`; `buf.gen.yaml` drives every committed binding tree through plugins Buf resolves. Assay proves the default label is `main` and requires its resolved commit unchanged immediately before publication. Rule violations exit 100; every other non-zero is a tool failure.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@bufbuild/buf`
- package: `@bufbuild/buf` (Apache-2.0)
- module: `buf` and `protoc-gen-buf-lint` under `node_modules/.bin/`, a node shim over the `@bufbuild/buf-<os>-<arch>` binary
- runtime: node; verbs reach the registry only where a flag names it, and `protoc_builtin` rows take the machine `protoc` buf never ships
- rail: default-label resolution, proto lint, format diff, image building, generation, and gated BSR publication

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `buf.yaml` v2 — the gate axis every verb reads from the working directory upward

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY] | [CAPABILITY]                                                                |
| :-----: | :--------------------------------------- | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `version: v2`                            | enum          | the config generation; `v1beta1`/`v1` migrate through `buf config migrate`  |
|  [02]   | `modules: [{ path, name?, … }]`          | struct        | one record per module root; `path` fixes what a proto path is relative to   |
|  [03]   | `modules[].includes` / `excludes`        | struct        | narrows discovery inside one root; excluding every file faults the build    |
|  [04]   | `modules[].lint`                         | struct        | one module's own rule config, REPLACING the workspace default whole         |
|  [05]   | `modules[].<axis>.disable_builtin: true` | struct        | drops every built-in rule so plugin rules alone grade; default `false`      |
|  [06]   | `lint.use`                               | struct        | the categories in force, resolved through `buf config ls-lint-rules`        |
|  [07]   | `lint.except`                            | struct        | drops a named rule everywhere the config reaches                            |
|  [08]   | `lint.ignore` / `lint.ignore_only`       | struct        | drops every rule, or one named rule, for `buf.yaml`-relative paths          |
|  [09]   | `lint.service_suffix`                    | struct        | the suffix `SERVICE_SUFFIX` demands, default `Service`                      |
|  [10]   | `lint.enum_zero_value_suffix`            | struct        | the suffix `ENUM_ZERO_VALUE_SUFFIX` demands, default `_UNSPECIFIED`         |
|  [11]   | `lint.rpc_allow_*`                       | struct        | admits one type on both rpc sides, or `google.protobuf.Empty` on either     |
|  [12]   | `lint.disallow_comment_ignores`          | struct        | refuses in-file `buf:lint:ignore <RULE>` directives; default `false`        |
|  [13]   | `plugins: [{ plugin, options? }]`        | struct        | custom check plugins whose rules `use` names; a remote ref pins in the lock |
|  [14]   | `policies: [{ policy, ignore?, … }]`     | struct        | a shared lint rule file, local path or BSR policy name                      |
|  [15]   | `deps: [<bsr-module>]` + `buf.lock`      | struct        | registry dependencies `buf dep update` pins; the protovalidate seat         |
|  [16]   | `modules[].name`                         | struct        | BSR identity; only `tests/contracts/proto` names `buf.build/rasm/contracts` |

- `modules[].lint`: one module block replaces the workspace default whole; nothing merges.
- module block: an omitted `use` inside one falls to buf's built-in `STANDARD` default, never the workspace's category.
- `disable_builtin: true`: leaves a module with no rule at all where no plugin supplies one, so `buf lint` WARNs `No lint rules are configured` and exits 0.
- `lint.disallow_comment_ignores`: v2 INVERTS v1's `allow_comment_ignores`, and hard-rejects that key as `field allow_comment_ignores not found`.
- `lint.ignore`/`ignore_only`: paths resolve against the `buf.yaml` directory; `managed.disable` `path:` is module-relative.
- `buf config ls-lint-rules --configured-only --module-path <path>`: prints one module's resolved roster, the one census reading a carve as buf resolved it.

[PUBLIC_TYPE_SCOPE]: `buf.gen.yaml` v2 — the generation axis `buf generate` reads from the working directory alone, seated at the repo root beside `buf.yaml`

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :---------------------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `version: v2`                                   | enum          | the template generation                                           |
|  [02]   | `clean: true`                                   | struct        | Pre-run removal of each plugin `out` directory before generation  |
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
|  [14]   | `plugins[].revision`                            | int           | exact BSR rebuild behind the remote upstream plugin version       |

- `clean: true`: removes each configured plugin `out` directory before any plugin writes the current generation.
- Shared or nested `out` paths need deliberate seating because each configured directory participates in the pre-run clean phase.
- `-o <dir>`: prepends `<dir>` to every `out` and scopes the sweep there, so a scratch regeneration mirrors the committed trees untouched.
- `types`/`exclude_types`: resolve against EACH input image; an unknown FQN faults, while each filter further restricts the prior filtered image.
- `types`: exact messages retain referenced field types, methods retain request and response, services retain every method; Buf computes the transitive descriptor closure.
- `exclude_types`: removes the descriptors and every reference to them, so it never substitutes for `include_imports` or a package-owned runtime.
- `managed.disable` `path:`: module-root relative — the workspace spelling matches nothing silently; `module:` never matches a local nil-name module.
- `managed`: `disable` beats `override`; WKTs skip; `go_package` moves only under an override.
- `managed` `field_option`: `jstype` alone, on `disable` and `override` both; buf's own published example spells `js_type`, which the parser REJECTS.
- `managed` `file_option`: extends the defaults below with `java_package_prefix`/`_suffix`, `java_string_check_utf8`, `optimize_for`, `go_package`/`_prefix`, `cc_enable_arenas`, `csharp_namespace_prefix`, `php_metadata_namespace_suffix`, `ruby_package_suffix`, and `swift_prefix`.
- `managed` value typing: decoding names the wrong-shaped value — `optimize_for` takes `SPEED`/`CODE_SIZE`/`LITE_RUNTIME`, `jstype` takes `JS_NORMAL`/`JS_STRING`/`JS_NUMBER`.
- `plugins[].strategy`: defaults to `directory`, running one plugin process per directory of files-to-generate, so a local plugin holding cross-file state emits fragments; `all` collapses the run to one `CodeGeneratorRequest`.
- plugin failure: one failing plugin cancels its siblings, each printing `signal: killed` — the real error is the one other line.
- `plugins[].revision`: buf's v2 reference recommends nothing, while its v1 page recommends OMITTING it so the version's newest revision carries upstream fixes.

[PUBLIC_TYPE_SCOPE]: managed defaults — the file options `managed.enabled: true` OVERWRITES in every embedded descriptor, never fills

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `csharp_namespace`       | enum          | PascalCase per package segment, `.`-joined                          |
|  [02]   | `java_package`           | enum          | `com.<package>`; `java_package_prefix`/`_suffix` rows recompose it  |
|  [03]   | `java_outer_classname`   | enum          | PascalCase of the proto file name                                   |
|  [04]   | `java_multiple_files`    | enum          | `true`                                                              |
|  [05]   | `objc_class_prefix`      | enum          | uppercased segment initials padded to three with `X`; `GPB` → `GPX` |
|  [06]   | `php_namespace`          | enum          | PascalCase per package segment, `\`-joined                          |
|  [07]   | `php_metadata_namespace` | enum          | `<php_namespace>\GPBMetadata`                                       |
|  [08]   | `ruby_package`           | enum          | PascalCase per package segment, `::`-nested                         |

[PUBLIC_TYPE_SCOPE]: lint categories — nested, each a superset of the one before, resolved through `buf config ls-lint-rules --version v2`

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                                                                          |
| :-----: | :---------- | :------------ | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `MINIMAL`   | enum          | package layout alone — `PACKAGE_DEFINED`, `PACKAGE_DIRECTORY_MATCH`, `PACKAGE_SAME_DIRECTORY`, cycles |
|  [02]   | `BASIC`     | enum          | `MINIMAL` plus casing, enum-zero, import, and per-package option-agreement rules                      |
|  [03]   | `STANDARD`  | enum          | `BASIC` plus `PACKAGE_VERSION_SUFFIX`, the rpc-naming trio, `SERVICE_SUFFIX`, `PROTOVALIDATE`         |
|  [04]   | `COMMENTS`  | enum          | non-empty comments on every enum, field, message, oneof, rpc, and service                             |
|  [05]   | `UNARY_RPC` | enum          | `RPC_NO_CLIENT_STREAMING`, `RPC_NO_SERVER_STREAMING` — refuses every streaming rpc                    |

- `STANDARD`: `PACKAGE_VERSION_SUFFIX` carves at the workspace `except` alone, since estate packages spell no version segment and reshape in place.

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

| [INDEX] | [SURFACE]                                                   | [SHAPE] | [CAPABILITY]                                                    |
| :-----: | :---------------------------------------------------------- | :------ | :-------------------------------------------------------------- |
|  [01]   | `buf lint [input]`                                          | command | rule violations at `path:line:col`; exit 100 when any fires     |
|  [02]   | `buf format [input] --diff --exit-code`                     | command | canonical `.proto` layout as a diff; `-w` rewrites in place     |
|  [03]   | `buf build [input] -o <file>`                               | command | mints an image; `--as-file-descriptor-set` bares it             |
|  [04]   | `buf generate [input] [--template] [-o <dir>] [--clean]`    | command | drives every template plugin row; `-o` prefixes each `out`      |
|  [05]   | `buf ls-files [input] --format text\|json\|import`          | command | the file set a verb resolves, with or without imports           |
|  [06]   | `buf stats [input]`                                         | command | file, package, message, field, service, and rpc counts          |
|  [07]   | `buf config ls-modules` / `ls-*-rules`                      | command | the configured module roots; the live rule set from the binary  |
|  [08]   | `buf config init` / `migrate`                               | command | scaffold a v2 config; lift v1 config to v2                      |
|  [09]   | `buf dep update` / `graph` / `prune`                        | command | `buf.lock` maintenance and the dependency graph                 |
|  [10]   | `buf export -o <dir>` / `buf convert --type --from --to`    | command | extract a source tree; recode one message across formats        |
|  [11]   | `buf curl --protocol connect\|grpc\|grpcweb`                | command | call a running service; `--reflect` needs HTTP/2                |
|  [12]   | `protoc-gen-buf-lint`                                       | plugin  | the same rule engine as a protoc plugin                         |
|  [13]   | `buf registry module commit resolve <module> --format json` | command | resolves the default label to its immutable 32-hex commit       |
|  [14]   | `buf push [input] --exclude-unnamed --label main`           | command | pushes named modules alone; `--create` mints the module once    |
|  [15]   | `buf plugin update` / `prune` / `push`                      | command | pins, drops, and publishes the check plugins `buf.lock` carries |
|  [16]   | `buf lsp serve`                                             | command | the language server every editor integration dials              |

- `buf lint --path <p>`: refuses a module root (`specify this module path directly as an input`); it names a file or subdirectory.
- `buf registry module commit resolve`: JSON carries `commit` and `create_time`; the gate accepts only one lowercase 32-hex commit.
- `buf config ls-lint-rules --configured-only --module-path <path>`: prints one module's resolved roster; `--module-path` is required past one module.
- `buf alpha`: hidden from the top-level listing and source-marked `may be deleted`; `buf beta` lists but self-marks `likely to change`. Neither gates.
- `buf push --exclude-unnamed`: publishes the named estate module only; its unnamed local dependencies must not enter the named module image.
- `buf push --create`: mints the module before uploading — a failed upload strands a named empty module, and re-running the same push is the repair.
- `buf format --exit-code`: alone writes every formatted file to stdout — the gate form pairs it with `--diff`.
- `buf generate` `local:`: takes the literal binary name or path — a path resolves as a file, a bare name off `${PATH}`; no `protoc-gen-` prefixing.
- `buf build -o /dev/null`: proves the workspace compiles with no artifact; `buf ls-files` is the cheapest census of what every verb resolves.
- `buf build --as-file-descriptor-set`: bares the image to the `FileDescriptorSet` every runtime decodes; the raw image carries buf-only fields.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Root `buf.yaml` names the estate module at `tests/contracts/proto` as `buf.build/rasm/contracts`; publisher modules stay unnamed.
- Each publisher module lands as one `modules` row with one `managed.disable` `path:` row; no input row changes.
- That carve seats per publisher module, never per observed delta: `grpc/health/v1` diffs to nothing only because the `csharp_namespace` default derives the publisher's own spelling.
- Publisher modules clearing `STANDARD` whole carve nothing; `tests/contracts/vendor/cloudevents/proto` carries no rule block at all.
- Publisher modules failing `STANDARD` restate `use: [STANDARD]` in their own block and `except` the exact rules their publisher spelling breaks.
- `buf format --diff --exit-code tests/contracts/proto` names the estate module alone, so no publisher byte enters the format path.
- Root `buf.gen.yaml` feeds ONE workspace input (`directory: .`) to every plugin row; exact FQN filters resolve against that one image.
- `protocolbuffers/csharp` and `grpc/csharp` run as BSR remote plugins pinned by upstream version and exact revision; no C# generator rides `${PATH}`.
- `protoc-gen-es`, `protoc-gen-py`, and `protoc-gen-connectrpc` ride the pnpm catalog and the uv venv, each pinned as a pair with its runtime.
- `protoc-gen-jsonschema` rides the machine estate; `assay contracts` drives it through an inline template, never a `buf.gen.yaml` row.

[STACKING]:
- `@bufbuild/protobuf`(`libs/typescript/.api/bufbuild-protobuf.md`): `createFileRegistry(fileDescriptorSet)` decodes the bared image.
- `protobuf-py`(`libs/python/.api/protobuf-py.md`): `protobuf.wkt.FileDescriptorSet.from_binary` decodes the bared image the corpus gate reads.
- `tests/contracts/README.md` (within-corpus edge): the README owns authority, layout, and regeneration; this catalog owns commands, keys, and rules.

[LOCAL_ADMISSION]:
- Invoke every verb as `node_modules/.bin/buf` from the repo root; a global `buf` binds no pinned version and a foreign cwd re-roots every path.
- `buf.yaml` carves a rule as `except` where ruled vocabulary displaces it everywhere, else `ignore_only` on the one departing source.
- Whole vendored modules depart as a `modules[].lint` block restating `use: [STANDARD]` beside their `except` roster, never as a lowered category.
- `lint.disallow_comment_ignores: true` holds workspace-wide, so a carve is reviewable in `buf.yaml` and never an in-file directive.
- Every carve row states its why inline; a lowered category standing in for a carve is refused.
- Template rows carry one load-bearing clause per option and per carve; an option nothing reads is not added.

[RAIL_LAW]:
- Package: `@bufbuild/buf`
- Owns: the proto rule engine, BSR commit resolution and publication, canonical formatting, image minting, managed generation, and the readers
- Accept: one active named estate module whose default label is `main`, unnamed publisher modules carving by named rule alone, the resolved commit unchanged at publication, one workspace input, exact actor-root filters, version-and-revision-pinned remote plugins, generated-only outs
- Reject: a global binary, a mutable publication label, a lowered category, an in-file comment ignore, a descriptor snapshot, or a second driver
