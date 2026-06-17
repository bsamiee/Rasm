# [PY_ARTIFACTS_API_RUAMEL_YAML]

`ruamel-yaml` supplies the round-trip YAML surface for the artifacts structured-documents rail: a single `YAML` engine owning typ-selected load/dump, comment-and-order-preserving container types, and class registration that drive YAML ingestion and emission preserving comments, key order, anchors, and styling. The package owner composes `YAML`, `CommentedMap`, and `CommentedSeq` into the structured-documents owner; it never re-implements YAML scanning the engine already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ruamel-yaml`
- package: `ruamel-yaml`
- import: `from ruamel.yaml import YAML`
- owner: `artifacts`
- rail: structured documents
- installed: `0.19.1` reflected via `python -c "import ruamel.yaml"` on cp315
- entry points: none (library only)
- capability: round-trip YAML 1.1/1.2 load/dump preserving comments, key order, anchors/aliases, and styling; safe/unsafe/base typ variants; multi-document streams; custom class registration

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: engine and container roots
- rail: structured documents

| [INDEX] | [SYMBOL]                                        | [PACKAGE_ROLE]     | [CAPABILITY]                                                   |
| :-----: | :---------------------------------------------- | :----------------- | :------------------------------------------------------------- |
|   [1]   | `YAML`                                          | engine root        | typ-selected load/dump engine with indent/width/version config |
|   [2]   | `CommentedMap`                                  | mapping container  | dict preserving comments, order, and merge keys                |
|   [3]   | `CommentedSeq`                                  | sequence container | list preserving comments and styling                           |
|   [4]   | `CommentToken`                                  | comment node       | a parsed comment attached to a node                            |
|   [5]   | `RoundTripConstructor` / `RoundTripRepresenter` | round-trip codec   | the load/dump halves preserving fidelity                       |
|   [6]   | `SafeConstructor` / `SafeRepresenter`           | safe codec         | the safe typ load/dump halves                                  |

[PUBLIC_TYPE_SCOPE]: faults
- rail: structured documents

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE] | [CAPABILITY]                    |
| :-----: | :---------------- | :------------- | :------------------------------ |
|   [1]   | `YAMLError`       | engine fault   | base YAML failure               |
|   [2]   | `MarkedYAMLError` | located fault  | a failure carrying source marks |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine construction and load/dump
- rail: structured documents

The `YAML` row carries typ, pure-Python, output, and plugin policy; load/dump rows reuse that configured engine.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]               | [CAPABILITY]                                |
| :-----: | :-------------------- | :------------------------- | :------------------------------------------ |
|   [1]   | `YAML`                | engine construction policy | engine (typ in 'rt'/'safe'/'unsafe'/'base') |
|   [2]   | `YAML.load`           | one stream                 | parse one document                          |
|   [3]   | `YAML.load_all`       | multi-document stream      | parse a multi-document stream               |
|   [4]   | `YAML.dump`           | data plus target stream    | emit one document                           |
|   [5]   | `YAML.dump_all`       | documents plus stream      | emit a multi-document stream                |
|   [6]   | `YAML.register_class` | round-trip class           | register a custom round-trip class          |
|   [7]   | `YAML.indent`         | indentation policy         | configure indentation                       |
|   [8]   | `YAML.version`        | version property           | YAML version property (1.1/1.2)             |

## [4]-[IMPLEMENTATION_LAW]

[YAML_ROUNDTRIP]:
- import: `from ruamel.yaml import YAML` at boundary scope only; module-level import is banned by the manifest import policy.
- engine axis: one `YAML` instance owns load and dump; `typ` ('rt'/'safe'/'unsafe'/'base') is the codec row, never a parallel loader/dumper class per mode.
- container axis: `CommentedMap`/`CommentedSeq` are the round-trip data carriers; the round-trip typ is the default so comments and order survive a load/dump cycle.
- stream axis: `load`/`dump` (single) and `load_all`/`dump_all` (multi-document) are rows on the engine, never separate engines.
- evidence: each load/dump captures typ, document count, YAML version, and output byte length as a structured-documents receipt.
- boundary: ruamel.yaml owns YAML; XML routes to `lxml`, TOML to `tomlkit`; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ruamel-yaml`
- Owns: round-trip YAML load/dump preserving comments/order/anchors/styling, typ variants, multi-document streams, class registration
- Accept: fidelity-preserving YAML processing feeding the structured-documents owner
- Reject: wrapper-renames of `load`/`dump`; a `pyyaml` fallback where ruamel is admitted; a per-typ engine class where a `typ` row suffices; identity minting the runtime owns
