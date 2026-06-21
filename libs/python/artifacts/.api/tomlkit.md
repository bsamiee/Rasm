# [PY_ARTIFACTS_API_TOMLKIT]

`tomlkit` supplies the style-preserving TOML surface for the artifacts structured-documents rail: parse/dump functions, a `TOMLDocument` container, a complete styled-item factory family (table, array-of-tables, array, inline_table, key, the scalar builders integer/float_/boolean/string/date/time/datetime, and the trivia builders comment/nl/ws), and encoder registration that drive lossless TOML ingestion and emission preserving comments, whitespace, ordering, and quoting style. The package owner composes `parse`, `dumps`, `document`, the item factories, and the `as_string`/`unwrap` round-trip into the structured-documents owner; it never re-implements TOML parsing tomlkit already owns and never rebuilds a document from a plain `dict` where the styled factories preserve trivia.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tomlkit`
- package: `tomlkit`
- import: `tomlkit`
- owner: `artifacts`
- rail: structured documents
- asset: pure-Python runtime library (no native build; `py3-none-any` wheel); zero install-time dependencies
- installed: `0.15.0` reflected via `tomlkit.__version__` on cp315
- entry points: none (library only)
- capability: style-preserving TOML parse/dump (comments, whitespace, ordering, quote style retained), programmatic document building, the full styled-item factory family, scalar/trivia item builders, multiline-array control, lossy `unwrap()` projection to plain values, and custom encoder registration/deregistration

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and styled-item types
- rail: structured documents

`TOMLDocument` is the round-trip container (a `Container` subclass) preserving the full source style. The `items` module carries the styled-value vocabulary: container items (`Table`/`AoT`/`Array`/`InlineTable`), scalar items (`Integer`/`Float`/`Bool`/`String`/`Date`/`Time`/`DateTime`), key items (`Key`/`SingleKey`/`DottedKey`), trivia (`Comment`/`Whitespace`/`Trivia`), and the `StringType`/`KeyType` style enums. Every styled value descends from `Item`; a programmatic edit mutates these in place so trivia survives.

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]   | [CAPABILITY]                                       |
| :-----: | :-------------------------------- | :--------------- | :------------------------------------------------- |
|  [01]   | `TOMLDocument`                    | document root    | a parsed/built TOML document preserving style      |
|  [02]   | `items.Item`                      | item base        | the base of every styled value                     |
|  [03]   | `items.Table`                     | table item       | a `[table]` preserving comments/order              |
|  [04]   | `items.AoT`                       | array-of-tables  | an `[[array]]` of tables                           |
|  [05]   | `items.Array`                     | array item       | a styled array (multiline-capable)                 |
|  [06]   | `items.InlineTable`               | inline table     | a `{ }` inline table                               |
|  [07]   | `items.Integer/Float/Bool/String` | scalar items     | styled scalars preserving raw spelling             |
|  [08]   | `items.Date/Time/DateTime`        | temporal items   | styled RFC-3339 temporals                          |
|  [09]   | `items.Key/SingleKey/DottedKey`   | key items        | styled bare/quoted/dotted keys                     |
|  [10]   | `items.Comment/Whitespace/Trivia` | trivia items     | comment, whitespace, and per-item trivia carrier   |
|  [11]   | `items.StringType` / `items.KeyType` | style enums   | `SLB`/`SLL`/`MLB`/`MLL` string + bare/quoted key styles |
|  [12]   | `items.Null`                      | absent marker    | the styled absent/removed-item sentinel            |

[PUBLIC_TYPE_SCOPE]: faults
- rail: structured documents

`exceptions.TOMLKitError` is the single root; `ParseError(ValueError, TOMLKitError)` is the malformed-source base carrying line/col and subclasses every grammar fault; the container/convert faults (`NonExistentKey(KeyError)`, `KeyAlreadyPresent`, `ConvertError(TypeError, ValueError)`, `InvalidStringError`) descend from `TOMLKitError` directly. One `except TOMLKitError` catches all; `except ParseError` catches only grammar faults.

| [INDEX] | [SYMBOL]                                     | [PACKAGE_ROLE]  | [CAPABILITY]                                         |
| :-----: | :------------------------------------------- | :-------------- | :--------------------------------------------------- |
|  [01]   | `exceptions.TOMLKitError`                    | engine root     | base of every tomlkit failure (`Exception`)          |
|  [02]   | `exceptions.ParseError`                      | parse base      | `ValueError + TOMLKitError`; malformed TOML w/ line/col |
|  [03]   | `exceptions.UnexpectedCharError`             | parse fault     | unexpected character during parse                    |
|  [04]   | `exceptions.UnexpectedEofError`              | parse fault     | premature end of source                              |
|  [05]   | `exceptions.EmptyKeyError` / `EmptyTableNameError` | parse fault | empty key / empty table name                         |
|  [06]   | `exceptions.InvalidNumberError` / `InvalidNumberOrDateError` | parse fault | malformed number/date literal                 |
|  [07]   | `exceptions.InvalidDateError` / `InvalidTimeError` / `InvalidDateTimeError` | parse fault | malformed temporal literal               |
|  [08]   | `exceptions.InvalidCharInStringError` / `InvalidControlChar` / `InvalidUnicodeValueError` | parse fault | malformed string content       |
|  [09]   | `exceptions.MixedArrayTypesError`            | parse fault     | heterogeneous array element types                    |
|  [10]   | `exceptions.NonExistentKey`                  | access fault    | `KeyError + TOMLKitError`; missing key on read       |
|  [11]   | `exceptions.KeyAlreadyPresent`               | edit fault      | duplicate-key insert                                 |
|  [12]   | `exceptions.ConvertError`                    | build fault     | `TypeError + ValueError + TOMLKitError`; un-encodable value |
|  [13]   | `exceptions.InvalidStringError`              | build fault     | `ValueError + TOMLKitError`; invalid styled-string request |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse, dump, and document build
- rail: structured documents

`parse`/`loads` accept `str | bytes` and return a `TOMLDocument`; `load(fp)` reads a text/binary file object; `dumps`/`dump` emit with an optional `sort_keys` policy; `document()` mints an empty container. `register_encoder`/`unregister_encoder` install/remove a custom value encoder for `item(value)` to consult.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                              | [CAPABILITY]                                       |
| :-----: | :------------------- | :------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `parse`              | `parse(string: str \| bytes) -> TOMLDocument`            | parse TOML source                                  |
|  [02]   | `loads`              | `loads(string: str \| bytes) -> TOMLDocument`            | parse (near-alias of `parse`)                      |
|  [03]   | `load`               | `load(fp: IO[str] \| IO[bytes]) -> TOMLDocument`         | parse from a file object                           |
|  [04]   | `dumps`              | `dumps(data: Mapping, sort_keys: bool=False) -> str`     | serialize to a string                              |
|  [05]   | `dump`               | `dump(data: Mapping, fp: IO[str], *, sort_keys=False)`   | serialize to a file object                         |
|  [06]   | `document`           | `document() -> TOMLDocument`                             | build an empty document                            |
|  [07]   | `register_encoder`   | `register_encoder(encoder: E) -> E`                      | register a custom value encoder                    |
|  [08]   | `unregister_encoder` | `unregister_encoder(encoder: Encoder) -> None`           | remove a previously registered encoder             |

[ENTRYPOINT_SCOPE]: item factories
- rail: structured documents

The container, key, scalar, and trivia factories are the single styled-build row set; a programmatic edit composes them so comments/whitespace/quoting survive. `item(value)` is the polymorphic wrap — it discriminates on the Python value shape (dict -> table, list -> array, datetime -> styled temporal) consulting registered encoders, so there is no per-type build family to call.

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]                                                    | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------- | :------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `item`                                         | `item(value: Any, _parent=None, _sort_keys=False) -> Item`     | polymorphic wrap of a Python value (discriminates on shape) |
|  [02]   | `value`                                        | `value(raw: str) -> Item`                                      | parse a single TOML value expression          |
|  [03]   | `key_value`                                    | `key_value(src: str) -> tuple[Key, Item]`                      | parse a `key = value` pair                     |
|  [04]   | `table`                                        | `table(is_super_table: bool \| None=None) -> Table`            | build a table (super-table flag)               |
|  [05]   | `aot`                                          | `aot() -> AoT`                                                 | build an array-of-tables                       |
|  [06]   | `array`                                        | `array(raw: str='[]') -> Array`                                | build an array from raw text                   |
|  [07]   | `inline_table`                                 | `inline_table() -> InlineTable`                                | build an inline table                          |
|  [08]   | `key`                                          | `key(k: str \| Iterable[str]) -> Key`                          | build a styled bare/dotted key                 |
|  [09]   | `integer` / `float_` / `boolean`              | `integer(raw: str \| int)` / `float_(raw)` / `boolean(raw)`    | build styled scalar items                      |
|  [10]   | `string`                                       | `string(raw: str, *, literal=False, multiline=False, escape=True) -> String` | build a styled string (literal/multiline) |
|  [11]   | `date` / `time` / `datetime`                  | `datetime(raw: str) -> DateTime` (and date/time)               | build styled temporal items                    |
|  [12]   | `comment` / `nl` / `ws`                       | `comment(string)` / `nl()` / `ws(src)`                         | build trivia items                             |

[ENTRYPOINT_SCOPE]: container edit and round-trip
- rail: structured documents

`TOMLDocument` and the container items expose a dict-like edit surface plus the style-bearing methods. `as_string()` re-emits the styled source; `unwrap()` projects to plain Python values (lossy — drops trivia); `Array.add_line`/`multiline` and `Table.add`/`indent`/`is_super_table` control emission style.

| [INDEX] | [SURFACE]                                              | [CAPABILITY]                                                |
| :-----: | :----------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `TOMLDocument.as_string()` / `Table.as_string()` / `Array.as_string()` | re-emit the exact styled source string             |
|  [02]   | `TOMLDocument.unwrap()` / `Item.unwrap()`              | project to plain `dict`/`list`/scalar (drops style/trivia)  |
|  [03]   | `TOMLDocument.add(key, item)` / `append` / `update` / `setdefault` | style-preserving insert/merge                   |
|  [04]   | `TOMLDocument.remove(key)` / `pop` / `popitem`         | style-preserving delete                                     |
|  [05]   | `Array.add_line(*items, ...)` / `Array.multiline(bool)` | append a styled line / toggle multiline emission           |
|  [06]   | `Table.add(key, item)` / `Table.indent(n)` / `Table.is_super_table()` | table insert, indentation, super-table query    |
|  [07]   | `Item.trivia` / `Item.comment(text)` / `Item.indent(n)` | per-item trivia access and mutation                        |

## [04]-[IMPLEMENTATION_LAW]

[TOML_STYLED]:
- import: `import tomlkit` (and `from tomlkit import items, exceptions` for the typed vocabulary) at boundary scope only; module-level import is banned by the manifest import policy.
- document axis: `TOMLDocument` is the single container; `parse`/`loads`/`load` are the read entries and `document()` builds an empty one, never parallel document types.
- item axis: the `table`/`aot`/`array`/`inline_table`/`key`/`integer`/`float_`/`boolean`/`string`/`date`/`time`/`datetime`/`comment`/`nl`/`ws` factories are the styled-build row set; `item(value)` is the polymorphic wrap that discriminates on the Python value shape; a programmatic edit composes factories so comments/whitespace/quoting survive, never a raw-`dict` rebuild that drops style.
- emit axis: `dumps`/`dump` with `sort_keys` and `as_string()` are the emission surfaces; the read/build/edit/emit round-trip preserves style; `unwrap()` is the explicit lossy projection when a plain-value snapshot (not a styled round-trip) is wanted.
- encoder axis: `register_encoder`/`unregister_encoder` install the custom value-encoder for `item(value)` to consult; a non-stdlib value type encodes through a registered encoder, never a hand-special-cased branch in the document owner.
- fault axis: every failure descends from `exceptions.TOMLKitError`; `ParseError` (with line/col) is the grammar-fault base, `NonExistentKey`/`KeyAlreadyPresent` are the access/edit faults, `ConvertError`/`InvalidStringError` are the build faults — the structured-documents fault rail catches `TOMLKitError` for the root and `ParseError` for malformed input.
- evidence: each parse/dump captures table count, key count, array-of-tables count, and output byte length as a structured-documents receipt.
- boundary: tomlkit owns TOML; the stdlib `tomllib` read-only path is not used where styled round-trip is admitted; XML routes to `lxml`, YAML to `ruamel-yaml`; live UI stays outside this package.

[STACKING]:
- `tomlkit.parse(src)` -> edit via the styled factories -> `as_string()` is the lossless config round-trip; a `msgspec`/`pydantic` boundary model validates the values read from `doc.unwrap()` (or per-key) while the styled `TOMLDocument` retains comments/order for re-emission, so validation and style-preservation are two layers over one parse.
- the `dumps(...)` string feeds a `stream-zip` `MemberFile` data iterable directly (a config artifact streamed into a bundle) or a `pymupdf`/`weasyprint` document, never written to an intermediate temp file.
- `register_encoder` internalizes a domain value type (e.g. a `numerics` quantity or a `pathlib.Path`) into the TOML emission so the structured-documents owner serializes domain objects through one registered encoder rather than pre-converting to primitives at every call site.
- `ParseError.line`/`.col` feed a structured-documents fault receipt with source coordinates, so a malformed-config diagnostic is positional, not a bare message.

[RAIL_LAW]:
- Package: `tomlkit`
- Owns: style-preserving TOML parse/dump, programmatic document building, the full styled-item factory family (container/scalar/key/trivia), polymorphic `item()` wrap, multiline-array and super-table style control, lossy `unwrap()` projection, and encoder registration/deregistration
- Accept: lossless TOML round-trip and styled programmatic edit feeding the structured-documents owner, with validation layered over `unwrap()`/per-key reads
- Reject: wrapper-renames of `parse`/`dumps`; a raw-`dict` rebuild that drops style; a per-type build family where `item()` discriminates; a `tomllib` fallback where styled round-trip is admitted; a hand-special-cased value branch where `register_encoder` owns the encoding; identity minting the runtime owns
