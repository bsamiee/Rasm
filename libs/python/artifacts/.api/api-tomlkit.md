# [PY_ARTIFACTS_API_TOMLKIT]

`tomlkit` supplies the style-preserving TOML surface for the artifacts structured-documents rail: parse/dump functions, a `TOMLDocument` container, and item factories (table, array, inline_table, key, datetime) that drive lossless TOML ingestion and emission preserving comments, whitespace, and ordering. The package owner composes `parse`, `dumps`, `document`, and the item factories into the structured-documents owner; it never re-implements TOML parsing tomlkit already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tomlkit`
- package: `tomlkit`
- import: `tomlkit`
- owner: `artifacts`
- rail: structured documents
- installed: `0.15.0` reflected via `python -c "import tomlkit"` on cp315
- entry points: none (library only)
- capability: style-preserving TOML parse/dump (comments, whitespace, ordering retained), programmatic document building, item factories, custom encoder registration

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and item types
- rail: structured documents

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `TOMLDocument` | document root | a parsed/built TOML document preserving style |
| [2] | `items.Table` | table item | a `[table]` preserving comments/order |
| [3] | `items.AoT` | array-of-tables | an `[[array]]` of tables |
| [4] | `items.Array` | array item | a styled array |
| [5] | `items.InlineTable` | inline table | a `{ }` inline table |
| [6] | `items.Key` | key item | a styled key |
| [7] | `items.Item` | item base | the base of every styled value |

[PUBLIC_TYPE_SCOPE]: faults
- rail: structured documents

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `exceptions.TOMLKitError` | engine fault | base tomlkit failure |
| [2] | `exceptions.ParseError` | parse fault | malformed TOML source with line/col |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse, dump, and document build
- rail: structured documents

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `parse` | `parse(string: str | bytes) -> TOMLDocument` | parse TOML source |
| [2] | `loads` | `loads(string: str | bytes) -> TOMLDocument` | parse (alias) |
| [3] | `load` | `load(fp: IO[str] | IO[bytes]) -> TOMLDocument` | parse from a file object |
| [4] | `dumps` | `dumps(data: Mapping[str, Any], sort_keys: bool = False) -> str` | serialize to a string |
| [5] | `dump` | `dump(data: Mapping[str, Any], fp: IO[str], *, sort_keys: bool = False) -> None` | serialize to a file object |
| [6] | `document` | `document() -> TOMLDocument` | build an empty document |
| [7] | `register_encoder` | `register_encoder(encoder) -> None` | register a custom value encoder |

[ENTRYPOINT_SCOPE]: item factories
- rail: structured documents

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
| :-----: | :-------- | :----------- | :----------- |
| [1] | `table` | `table(is_super_table: bool | None = None) -> Table` | build a table |
| [2] | `aot` | `aot() -> AoT` | build an array-of-tables |
| [3] | `array` | `array(raw: str | None = None) -> Array` | build an array |
| [4] | `inline_table` | `inline_table() -> InlineTable` | build an inline table |
| [5] | `key` | `key(k) -> Key` | build a styled key |
| [6] | `item` | `item(value, _parent=None, _sort_keys=False) -> Item` | wrap a Python value as a styled item |
| [7] | `datetime` / `date` / `time` | `datetime(raw) / date(raw) / time(raw) -> Item` | build temporal items |
| [8] | `comment` / `nl` / `ws` | `comment(s) / nl() / ws(s) -> Item` | build trivia items |

## [4]-[IMPLEMENTATION_LAW]

[TOML_STYLED]:
- import: `import tomlkit` at boundary scope only; module-level import is banned by the manifest import policy.
- document axis: `TOMLDocument` is the single container; `parse`/`document` are the read/build entries, never parallel document types.
- item axis: the `table`/`aot`/`array`/`inline_table`/`key`/`item` factories are the styled-build row set; a programmatic edit uses factories so comments/whitespace survive, never a raw-dict rebuild that drops style.
- emit axis: `dumps`/`dump` with `sort_keys` is the single emission surface; the read/build/edit/emit round-trip preserves style.
- evidence: each parse/dump captures table count, key count, and output byte length as a structured-documents receipt.
- boundary: tomlkit owns TOML; the stdlib `tomllib` read-only path is not used where styled round-trip is admitted; XML routes to `lxml`, YAML to `ruamel.yaml`; live UI stays outside this package.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `tomlkit`
- Owns: style-preserving TOML parse/dump, programmatic document building, item factories, encoder registration
- Accept: lossless TOML round-trip feeding the structured-documents owner
- Reject: wrapper-renames of `parse`/`dumps`; a raw-dict rebuild that drops style; a `tomllib` fallback where styled round-trip is admitted; identity minting the runtime owns
