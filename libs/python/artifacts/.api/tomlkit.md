# [PY_ARTIFACTS_API_TOMLKIT]

`tomlkit` supplies the style-preserving TOML surface for the artifacts structured-documents rail: parse/dump functions, a `TOMLDocument` container, and item factories (table, array, inline_table, key, datetime) that drive lossless TOML ingestion and emission preserving comments, whitespace, and ordering. The package owner composes `parse`, `dumps`, `document`, and the item factories into the structured-documents owner; it never re-implements TOML parsing tomlkit already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tomlkit`
- package: `tomlkit`
- import: `tomlkit`
- owner: `artifacts`
- rail: structured documents
- installed: `0.15.0` reflected via `python -c "import tomlkit"` on cp315
- entry points: none (library only)
- capability: style-preserving TOML parse/dump (comments, whitespace, ordering retained), programmatic document building, item factories, custom encoder registration

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and item types
- rail: structured documents

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE]  | [CAPABILITY]                                  |
| :-----: | :------------------ | :-------------- | :-------------------------------------------- |
|  [01]   | `TOMLDocument`      | document root   | a parsed/built TOML document preserving style |
|  [02]   | `items.Table`       | table item      | a `[table]` preserving comments/order         |
|  [03]   | `items.AoT`         | array-of-tables | an `[[array]]` of tables                      |
|  [04]   | `items.Array`       | array item      | a styled array                                |
|  [05]   | `items.InlineTable` | inline table    | a `{ }` inline table                          |
|  [06]   | `items.Key`         | key item        | a styled key                                  |
|  [07]   | `items.Item`        | item base       | the base of every styled value                |

[PUBLIC_TYPE_SCOPE]: faults
- rail: structured documents

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE] | [CAPABILITY]                        |
| :-----: | :------------------------ | :------------- | :---------------------------------- |
|  [01]   | `exceptions.TOMLKitError` | engine fault   | base tomlkit failure                |
|  [02]   | `exceptions.ParseError`   | parse fault    | malformed TOML source with line/col |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse, dump, and document build
- rail: structured documents

| [INDEX] | [SURFACE]          | [CALL_SHAPE]                    | [CAPABILITY]                    |
| :-----: | :----------------- | :------------------------------ | :------------------------------ |
|  [01]   | `parse`            | text or bytes source            | parse TOML source               |
|  [02]   | `loads`            | text or bytes source            | parse (alias)                   |
|  [03]   | `load`             | text or binary file object      | parse from a file object        |
|  [04]   | `dumps`            | mapping plus sort policy        | serialize to a string           |
|  [05]   | `dump`             | mapping plus target file object | serialize to a file object      |
|  [06]   | `document`         | no-arg factory                  | build an empty document         |
|  [07]   | `register_encoder` | encoder callback                | register a custom value encoder |

[ENTRYPOINT_SCOPE]: item factories
- rail: structured documents

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                  | [CAPABILITY]                         |
| :-----: | :--------------------------- | :---------------------------- | :----------------------------------- |
|  [01]   | `table`                      | optional super-table flag     | build a table                        |
|  [02]   | `aot`                        | no-arg factory                | build an array-of-tables             |
|  [03]   | `array`                      | optional raw array text       | build an array                       |
|  [04]   | `inline_table`               | no-arg factory                | build an inline table                |
|  [05]   | `key`                        | key value                     | build a styled key                   |
|  [06]   | `item`                       | value plus parent/sort policy | wrap a Python value as a styled item |
|  [07]   | `datetime` / `date` / `time` | raw temporal value            | build temporal items                 |
|  [08]   | `comment` / `nl` / `ws`      | trivia text or empty marker   | build trivia items                   |

## [04]-[IMPLEMENTATION_LAW]

[TOML_STYLED]:
- import: `import tomlkit` at boundary scope only; module-level import is banned by the manifest import policy.
- document axis: `TOMLDocument` is the single container; `parse`/`document` are the read/build entries, never parallel document types.
- item axis: the `table`/`aot`/`array`/`inline_table`/`key`/`item` factories are the styled-build row set; a programmatic edit uses factories so comments/whitespace survive, never a raw-dict rebuild that drops style.
- emit axis: `dumps`/`dump` with `sort_keys` is the single emission surface; the read/build/edit/emit round-trip preserves style.
- evidence: each parse/dump captures table count, key count, and output byte length as a structured-documents receipt.
- boundary: tomlkit owns TOML; the stdlib `tomllib` read-only path is not used where styled round-trip is admitted; XML routes to `lxml`, YAML to `ruamel.yaml`; live UI stays outside this package.

[RAIL_LAW]:
- Package: `tomlkit`
- Owns: style-preserving TOML parse/dump, programmatic document building, item factories, encoder registration
- Accept: lossless TOML round-trip feeding the structured-documents owner
- Reject: wrapper-renames of `parse`/`dumps`; a raw-dict rebuild that drops style; a `tomllib` fallback where styled round-trip is admitted; identity minting the runtime owns
