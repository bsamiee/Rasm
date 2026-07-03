# [API_CATALOGUE] @bufbuild/buf

`@bufbuild/buf` supplies the `buf`, `protoc-gen-buf-breaking`, and `protoc-gen-buf-lint` binaries for the interchange codegen and schema-governance rail: workspace build, format, lint, breaking-change detection, code generation via `buf.gen.yaml`, dependency management, and BSR publishing.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@bufbuild/buf`
- package: `@bufbuild/buf`
- namespace: —
- asset: platform binaries under `bin/`; no TypeScript type exports
- rail: codegen, schema-governance

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: binary executables — all installed under `bin/`
- rail: codegen

| [INDEX] | [SYMBOL]                  | [CAPABILITY]                    |
| :-----: | :------------------------ | :------------------------------ |
|  [01]   | `buf`                     | primary CLI entrypoint          |
|  [02]   | `protoc-gen-buf-breaking` | `protoc` breaking-change plugin |
|  [03]   | `protoc-gen-buf-lint`     | `protoc` lint plugin            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: core workspace commands
- rail: codegen

| [INDEX] | [SURFACE]              | [ENTRY_FAMILY] | [DESCRIPTION]                                      |
| :-----: | :--------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `buf build [input]`    | compile        | compile `.proto` to Buf image or FileDescriptorSet |
|  [02]   | `buf format [-w\|-d]`  | format         | normalize `.proto` whitespace and style in place   |
|  [03]   | `buf lint [input]`     | lint           | run 40+ built-in lint rules against source         |
|  [04]   | `buf generate [input]` | codegen        | run `buf.gen.yaml` plugins over proto source       |
|  [05]   | `buf breaking [input]` | breaking       | detect wire/source/JSON breaking changes           |
|  [06]   | `buf push [source]`    | publish        | push named module to BSR                           |

[ENTRYPOINT_SCOPE]: dependency and registry commands
- rail: schema-governance

| [INDEX] | [SURFACE]              | [ENTRY_FAMILY] | [DESCRIPTION]                            |
| :-----: | :--------------------- | :------------- | :--------------------------------------- |
|  [01]   | `buf dep update`       | dep management | update pinned dependencies in `buf.lock` |
|  [02]   | `buf dep prune`        | dep management | remove unused entries from `buf.lock`    |
|  [03]   | `buf dep graph`        | dep management | print dependency graph                   |
|  [04]   | `buf config init`      | config         | scaffold `buf.yaml` for workspace        |
|  [05]   | `buf ls-files [input]` | source query   | list `.proto` files in workspace         |
|  [06]   | `buf stats [input]`    | source query   | report statistics for source or module   |

[ENTRYPOINT_SCOPE]: buf build flags
- rail: codegen

| [INDEX] | [FLAG]                     | [TYPE_FAMILY] | [DESCRIPTION]                                   |
| :-----: | :------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `--as-file-descriptor-set` | bool flag     | strip Buf image metadata                        |
|  [02]   | `-o, --output`             | path flag     | output format: `binpb`, `json`, `txtpb`, `yaml` |
|  [03]   | `--exclude-imports`        | bool flag     | omit imports from image                         |
|  [04]   | `--exclude-source-info`    | bool flag     | strip source info                               |
|  [05]   | `--type`                   | string flag   | restrict output to named types                  |

[ENTRYPOINT_SCOPE]: buf breaking flags
- rail: codegen

| [INDEX] | [FLAG]                   | [TYPE_FAMILY] | [DESCRIPTION]                             |
| :-----: | :----------------------- | :------------ | :---------------------------------------- |
|  [01]   | `--against`              | string flag   | baseline source, module, or image         |
|  [02]   | `--against-registry`     | bool flag     | compare against latest BSR default branch |
|  [03]   | `--exclude-imports`      | bool flag     | skip breaking checks on imports           |
|  [04]   | `--limit-to-input-files` | bool flag     | restrict check to files present in input  |

[ENTRYPOINT_SCOPE]: buf generate — buf.gen.yaml plugin shape
- rail: codegen

| [INDEX] | [FIELD]                     | [TYPE_FAMILY] | [DESCRIPTION]                              |
| :-----: | :-------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `version`                   | string        | template version: `v1beta1`, `v1`, `v2`    |
|  [02]   | `clean`                     | bool          | delete `out` directories before generation |
|  [03]   | `plugins[].remote`          | string        | BSR-hosted plugin reference with version   |
|  [04]   | `plugins[].local`           | string        | local binary name or path                  |
|  [05]   | `plugins[].out`             | string        | relative output directory                  |
|  [06]   | `plugins[].opt`             | string/array  | plugin option string(s)                    |
|  [07]   | `plugins[].include_imports` | bool          | include imported files in generation       |
|  [08]   | `plugins[].include_wkt`     | bool          | include well-known types in generation     |

[ENTRYPOINT_SCOPE]: buf lint flags
- rail: schema-governance

| [INDEX] | [FLAG]           | [TYPE_FAMILY] | [DESCRIPTION]                                     |
| :-----: | :--------------- | :------------ | :------------------------------------------------ |
|  [01]   | `--config`       | string flag   | specify `buf.yaml` path or inline data            |
|  [02]   | `--error-format` | string flag   | `text`, `json`, `msvs`, `junit`, `github-actions` |
|  [03]   | `--path`         | string flag   | limit lint to specific files/directories          |
|  [04]   | `--exclude-path` | string flag   | exclude specific files/directories                |

## [04]-[IMPLEMENTATION_LAW]

[CODEGEN_TOPOLOGY]:
- `buf generate` reads `buf.gen.yaml`; plugins may be `remote:` (BSR-hosted) or `local:` (PATH or path)
- `protoc-gen-es` is admitted as `local: protoc-gen-es` in `buf.gen.yaml`; the `buf.gen.yaml` `version` field must be `v1` or `v2`
- accepted input formats: `binpb`, `dir`, `git`, `json`, `mod`, `protofile`, `tar`, `txtpb`, `yaml`, `zip`
- `buf build` output formats: `binpb`, `json`, `txtpb`, `yaml`; default output is `/dev/null` (validate only)
- platform binaries are installed from optional dependencies: `@bufbuild/buf-darwin-arm64`, `@bufbuild/buf-darwin-x64`, `@bufbuild/buf-linux-x64`, `@bufbuild/buf-linux-aarch64`, `@bufbuild/buf-linux-armv7`, `@bufbuild/buf-win32-x64`, `@bufbuild/buf-win32-arm64`
- `index.js` exports an empty object; the package carries no TypeScript surface
- BSR dependency pins live in `buf.lock`; `buf dep update` refreshes them

[LOCAL_ADMISSION]:
- Admitted as a build-time CLI tool; no application-code import.
- `buf.gen.yaml` is the single generation configuration surface; do not encode plugin options in shell scripts.
- `buf breaking` runs against a Git ref, BSR module, Buf image, or archive; `--against` is required unless `--against-registry` is set.

[RAIL_LAW]:
- Package: `@bufbuild/buf`
- Owns: workspace compilation, lint, breaking-change detection, code generation, and BSR publishing
- Accept: `buf.gen.yaml` for generation; `buf.yaml` for workspace, lint, and breaking configuration
- Reject: hand-maintained `protoc -I` scripts; per-developer plugin binary management
