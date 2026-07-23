# [PY_ARTIFACTS_API_TOMLKIT]

`tomlkit` owns the style-preserving TOML surface of the artifacts structured-documents rail across two axes the design pages compose. Egress lowers a `msgspec`-flattened structure to deterministic bytes through `dumps(to_builtins(node), sort_keys=True)`; ingress reads source to a `TOMLDocument` and projects a plain `dict`/`list`/scalar map through `parse(payload).unwrap()`. Styled-item factories and the `register_encoder` hook hold the comment-preserving round-trip depth a config-rewrite rail mutates in place; TOML routes here, XML to `lxml`, YAML to `ruamel-yaml`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tomlkit`
- package: `tomlkit` (MIT)
- import: `import tomlkit`
- owner: `artifacts`
- rail: structured documents
- entry points: none (library only)
- capability: style-preserving TOML round-trip (comments, whitespace, order, quoting retained), deterministic `sort_keys` emission, plain-builtins `dumps`/`parse().unwrap()` bridge, the styled-item factory family with polymorphic `item()` wrap, multiline-array and super-table style control, out-of-order-table flattening, lossy `unwrap()` projection, and value-encoder registration

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, container, and styled-item types
- rail: structured documents

Every styled value descends from `items.Item` and a programmatic edit mutates it in place so trivia survives. `Container` is the dict-like body shared by `TOMLDocument`, `Table`, and `InlineTable`; `Container.item(key)` returns an `OutOfOrderTableProxy` for a source-split table (`[a]` … `[a.c]`), which `unwrap()` flattens to one `dict`.

| [INDEX] | [SYMBOL]                             | [PACKAGE_ROLE]   | [CAPABILITY]                                                                 |
| :-----: | :----------------------------------- | :--------------- | :--------------------------------------------------------------------------- |
|  [01]   | `TOMLDocument`                       | document root    | a parsed/built TOML document preserving style (a `Container` subclass)       |
|  [02]   | `container.Container`                | body owner       | dict-like `body`/`add`/`append`/`remove`/`item`/`last_item`/`unwrap`         |
|  [03]   | `container.OutOfOrderTableProxy`     | split-table view | proxy for source-split table sections; `unwrap()` flattens to one `dict`     |
|  [04]   | `items.Item`                         | item base        | base of every styled value (`trivia`/`value`/`unwrap`/`as_string`/`comment`) |
|  [05]   | `items.Table`                        | table item       | a `[table]` preserving comments/order (`AbstractTable`; `is_super_table`)    |
|  [06]   | `items.AoT`                          | array-of-tables  | an `[[array]]` of tables (`body` list of `Table`, `insert`)                  |
|  [07]   | `items.Array`                        | array item       | a styled array (`multiline`/`add_line`/`item`/`insert`/`clear`)              |
|  [08]   | `items.InlineTable`                  | inline table     | a `{ }` inline table (`AbstractTable`; `append`)                             |
|  [09]   | `items.Integer/Float/Bool/String`    | scalar items     | styled scalars; `String.from_raw(value, type_, escape)` / `String.type`      |
|  [10]   | `items.Date/Time/DateTime`           | temporal items   | styled RFC-3339 temporals subclassing `date`/`time`/`datetime`               |
|  [11]   | `items.Key/SingleKey/DottedKey`      | key items        | styled bare/quoted/dotted keys; `is_dotted`/`is_multi`/`concat`              |
|  [12]   | `items.Comment/Whitespace/Trivia`    | trivia items     | comment, whitespace, `Trivia(indent, comment_ws, comment, trail)` carrier    |
|  [13]   | `items.StringType` / `items.KeyType` | style enums      | `StringType.{SLB,SLL,MLB,MLL}`; `KeyType.{Bare,Basic,Literal}`               |
|  [14]   | `items.Null`                         | absent marker    | the styled absent/removed-item sentinel                                      |
|  [15]   | `items.CUSTOM_ENCODERS`              | encoder registry | `list` of value encoders `item()` consults; `Encoder` `(value, /) -> Item`   |

[PUBLIC_TYPE_SCOPE]: faults
- rail: structured documents — `tomlkit.exceptions`

`except TOMLKitError` catches every fault; `except ParseError` catches only grammar faults and gives positional `line`/`col`.

| [INDEX] | [SYMBOL]                              | [PACKAGE_ROLE] | [CAPABILITY]                                                      |
| :-----: | :------------------------------------ | :------------- | :---------------------------------------------------------------- |
|  [01]   | `exceptions.TOMLKitError`             | engine root    | base of every tomlkit failure (`Exception`)                       |
|  [02]   | `exceptions.ParseError`               | parse base     | `ValueError + TOMLKitError`; malformed TOML carrying `line`/`col` |
|  [03]   | `exceptions.UnexpectedCharError`      | parse fault    | unexpected character during parse                                 |
|  [04]   | `exceptions.UnexpectedEofError`       | parse fault    | premature end of source                                           |
|  [05]   | `exceptions.EmptyKeyError`            | parse fault    | empty key                                                         |
|  [06]   | `exceptions.EmptyTableNameError`      | parse fault    | empty table name                                                  |
|  [07]   | `exceptions.InvalidNumberError`       | parse fault    | malformed number literal                                          |
|  [08]   | `exceptions.InvalidNumberOrDateError` | parse fault    | malformed number-or-date literal                                  |
|  [09]   | `exceptions.InvalidDateError`         | parse fault    | malformed date literal                                            |
|  [10]   | `exceptions.InvalidTimeError`         | parse fault    | malformed time literal                                            |
|  [11]   | `exceptions.InvalidDateTimeError`     | parse fault    | malformed datetime literal                                        |
|  [12]   | `exceptions.InvalidCharInStringError` | parse fault    | invalid character in string content                               |
|  [13]   | `exceptions.InvalidControlChar`       | parse fault    | invalid control character in string                               |
|  [14]   | `exceptions.InvalidUnicodeValueError` | parse fault    | invalid unicode escape value                                      |
|  [15]   | `exceptions.MixedArrayTypesError`     | parse fault    | heterogeneous array element types                                 |
|  [16]   | `exceptions.NonExistentKey`           | access fault   | `KeyError + TOMLKitError`; missing key on read                    |
|  [17]   | `exceptions.KeyAlreadyPresent`        | edit fault     | duplicate-key insert                                              |
|  [18]   | `exceptions.ConvertError`             | build fault    | `TypeError + ValueError + TOMLKitError`; un-encodable value       |
|  [19]   | `exceptions.InvalidStringError`       | build fault    | `ValueError + TOMLKitError`; invalid styled-string request        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse, dump, and document build
- rail: structured documents

`dumps` accepts any `Mapping`, so a plain `dict` from `msgspec.to_builtins` serializes directly without a pre-built styled tree, and `sort_keys` is the deterministic-bytes lever for a content-addressed pipeline. `register_encoder`/`unregister_encoder` install/remove a custom value encoder appended to the module-level `CUSTOM_ENCODERS` list that `item(value)` consults.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                             | [CAPABILITY]                        |
| :-----: | :------------------- | :----------------------------------------------------------------------- | :---------------------------------- |
|  [01]   | `parse`              | `parse(string: str \| bytes) -> TOMLDocument`                            | parse TOML source                   |
|  [02]   | `loads`              | `loads(string: str \| bytes) -> TOMLDocument`                            | parse (alias of `parse`)            |
|  [03]   | `load`               | `load(fp: IO[str] \| IO[bytes]) -> TOMLDocument`                         | parse from a file object            |
|  [04]   | `dumps`              | `dumps(data: Mapping[str, Any], sort_keys: bool=False) -> str`           | serialize to a string               |
|  [05]   | `dump`               | `dump(data: Mapping[str, Any], fp: IO[str], *, sort_keys=False) -> None` | serialize to a file object          |
|  [06]   | `document`           | `document() -> TOMLDocument`                                             | build an empty document             |
|  [07]   | `register_encoder`   | `register_encoder(encoder: E) -> E`                                      | append encoder to `CUSTOM_ENCODERS` |
|  [08]   | `unregister_encoder` | `unregister_encoder(encoder: Encoder) -> None`                           | remove a registered encoder         |

- `dumps`: annotated `Mapping`, yet a bare non-`Mapping` tomlkit wrapper (an `Item`/`AoT`/`Array`) is accepted too, lowering through its `as_string()`.

[ENTRYPOINT_SCOPE]: item factories
- rail: structured documents

`item(value)` is the polymorphic wrap discriminating on the Python value shape through a fixed precedence (`Item` passthrough -> `bool` -> `int` -> `float` -> `dict` -> `list`/`tuple` -> `date`/`time`/`datetime` -> registered `CUSTOM_ENCODERS`), so a `dict` becomes `Table` (or `InlineTable` under an array/inline parent), a `list` of all-`dict` under a table/no parent becomes `AoT` else `Array`, and `_sort_keys=True` sorts nested keys during the wrap. Bare factories below are the explicit constructors for a styled edit needing a specific item kind.

| [INDEX] | [SURFACE]      | [CALL_SHAPE]                                                                 | [CAPABILITY]                         |
| :-----: | :------------- | :--------------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `item`         | `item(value: Any, _parent=None, _sort_keys=False) -> Item`                   | polymorphic wrap (shape dispatch)    |
|  [02]   | `value`        | `value(raw: str) -> Item`                                                    | parse a single TOML value expression |
|  [03]   | `key_value`    | `key_value(src: str) -> tuple[Key, Item]`                                    | parse a `key = value` pair           |
|  [04]   | `table`        | `table(is_super_table: bool \| None=None) -> Table`                          | build a table (super-table flag)     |
|  [05]   | `aot`          | `aot() -> AoT`                                                               | build an array-of-tables             |
|  [06]   | `array`        | `array(raw: str='[]') -> Array`                                              | build an array from raw text         |
|  [07]   | `inline_table` | `inline_table() -> InlineTable`                                              | build an inline table                |
|  [08]   | `key`          | `key(k: str \| Iterable[str]) -> Key`                                        | build a styled bare or dotted key    |
|  [09]   | `integer`      | `integer(raw: str \| int)`                                                   | build a styled integer item          |
|  [10]   | `float_`       | `float_(raw: str \| float)`                                                  | build a styled float item            |
|  [11]   | `boolean`      | `boolean(raw: str \| bool)`                                                  | build a styled boolean item          |
|  [12]   | `string`       | `string(raw: str, *, literal=False, multiline=False, escape=True) -> String` | build a styled string                |
|  [13]   | `date`         | `date(raw: str)`                                                             | build a styled RFC-3339 date         |
|  [14]   | `time`         | `time(raw: str)`                                                             | build a styled RFC-3339 time         |
|  [15]   | `datetime`     | `datetime(raw: str)`                                                         | build a styled RFC-3339 datetime     |
|  [16]   | `comment`      | `comment(string: str)`                                                       | build a comment trivia item          |
|  [17]   | `nl` / `ws`    | `nl()` / `ws(src: str)`                                                      | build whitespace/newline trivia      |

- `key`: a single-element iterable (`key(['a'])`) returns a bare `SingleKey`, not a one-segment `DottedKey`; a multi-element iterable builds a `DottedKey`, a `str` a `SingleKey`.

[ENTRYPOINT_SCOPE]: container edit and round-trip
- rail: structured documents — `Container` / `TOMLDocument` / styled-item methods

`as_string()` re-emits the styled source; `unwrap()` projects to plain Python values (lossy — drops trivia, flattens `OutOfOrderTableProxy`, keeps `datetime`/`date`/`time` as stdlib temporals).

| [INDEX] | [SURFACE]                                              | [CAPABILITY]                                                            |
| :-----: | :----------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `Container.as_string()` / `Item.as_string()`           | re-emit the exact styled source string                                  |
|  [02]   | `Container.unwrap()` / `Item.unwrap()`                 | project to plain `dict`/`list`/scalar (drops style; flattens splits)    |
|  [03]   | `Container.body`                                       | the ordered `list[tuple[Key \| None, Item]]` backing (keyless = trivia) |
|  [04]   | `Container.add(key, item)` / `append` / `setdefault`   | style-preserving insert/merge                                           |
|  [05]   | `Container.remove(key)` / `item(key)` / `last_item()`  | style-preserving delete; keyed fetch; last inserted item                |
|  [06]   | `Array.add_line(...)` / `Array.multiline(bool)`        | append a styled line / toggle multiline emission                        |
|  [07]   | `Array.item(index)` / `insert(pos, value)` / `clear()` | indexed array item access and mutation                                  |
|  [08]   | `Table.append(key, item)` / `indent(n)`                | table insert, indentation                                               |
|  [09]   | `Table.is_super_table()`                               | super-table query                                                       |
|  [10]   | `AoT.body` / `AoT.insert(index, value)`                | the `list[Table]` backing; positional table insert                      |
|  [11]   | `Item.trivia` / `comment(text)` / `indent(n)`          | per-item trivia access and mutation                                     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `import tomlkit` (with `items`/`exceptions`/`container` for the typed vocabulary) at boundary scope only; the emit/lens owners declare `lazy import tomlkit`.
- egress axis (primary): `dumps(data, sort_keys=...)` accepts any `Mapping`; the canonical rail is `tomlkit.dumps(msgspec.to_builtins(node), sort_keys=True)`, lowering a `msgspec`-flattened structure straight to TOML bytes with no intermediate styled tree, `sort_keys=True` giving deterministic bytes for a content-addressed receipt.
- ingress axis (primary): `parse(string)` returns a `TOMLDocument`; `.unwrap()` projects to plain `dict`/`list`/scalar, flattening an `OutOfOrderTableProxy` to one `dict` and keeping RFC-3339 temporals as stdlib `date`/`time`/`datetime` so the recover-to `_value_node` recursion sees a uniform nested mapping. `loads` aliases `parse`; `load(fp)` reads a file object.
- document axis: `TOMLDocument` is the single container (a `Container` subclass); `parse`/`loads`/`load` are the read entries and `document()` mints an empty one. `Container` is the shared dict-like body for document and tables.
- item axis (styled depth): the container/scalar/key/trivia factories are the styled-build set that `item(value)` dispatches into by shape; a styled edit composes them so comments/whitespace/quoting survive, while plain emission lets `dumps` consume builtins directly — the styled path is used only when trivia must be authored or preserved.
- style-control axis: style lives on the item or the enum, never in string-munging emitted text — `Array.multiline(True)`/`add_line(...)` control multiline arrays, `Table.is_super_table()`/`indent(n)` control table style, and `String.from_raw(value, type_, escape)` with the `StringType.{SLB,SLL,MLB,MLL}` enum forces quoting.
- encoder axis: `register_encoder`/`unregister_encoder` append/remove a custom value-encoder on the runtime `items.CUSTOM_ENCODERS` list that `item(value)` consults; the encoder conforms to the `TYPE_CHECKING`-only `Encoder` protocol (`(value, /) -> Item`) and raises `ConvertError` for an unhandled value. `to_builtins(enc_hook=...)` flattens a domain type to primitives upstream as the first path; `register_encoder` is the styled-path fallback when the value must reach `dumps`/`item` as itself.
- fault axis: every failure descends from `exceptions.TOMLKitError`; the structured-documents fault rail catches `TOMLKitError` for the root and `ParseError` for malformed input, lifting each to a typed `expression.Error` arm at the boundary capsule.
- evidence: each emit/recover captures table count, key count, array-of-tables count, and output byte length as a structured-documents receipt fact; the deterministic `sort_keys=True` bytes feed the content key.
- boundary: tomlkit owns TOML round-trip; the read-only stdlib `tomllib` path is unused where styled round-trip or the unified parser is admitted; XML routes to `lxml`, YAML to `ruamel-yaml`; live UI stays outside this package.

[STACKING]:
- structured-text triad: `tomlkit` (TOML) joins `lxml` (XML, `libs/python/artifacts/.api/lxml.md`) and `ruamel-yaml` (YAML, `libs/python/artifacts/.api/ruamel-yaml.md`) as the structured-documents rail's three fidelity parsers — each preserves source structure so a rewrite edits the loaded tree in place; route by format, never cross-parse. Detect (`exchange/detect#DETECT`) maps `application/toml` -> `MediaClass.DATA` -> the tomlkit reader.
- emit seam (`document/emit#DOCUMENT`): `DocumentMode.TOML` binds `Backend(Band.CORE, _tomlkit_emit, ReceiptKind.DOCUMENT)`, lowering a `DocumentNode` tree through `tomlkit.dumps(msgspec.to_builtins(plan.node)).encode()` into one `EmitFact`; the `@receipted` weave mints `ArtifactReceipt.Document(key, bytes)` under the plan's PRE-RUN key. Its CORE band crosses `anyio.to_thread.run_sync` (`libs/python/.api/anyio.md`); a `ConvertError` lifts to the runtime `BoundaryFault` at the `async_boundary` capsule.
- format-delegate seam (`document/emit#DOCUMENT` `DocumentPlan.bound`): `DocumentPlan.bound` fans `DocumentMode.TOML` -> `(DocumentMode.TOML, _bare_spec)` through the emit owner's `produced` entrypoint, so the same bound `DocumentNode` tree a spec-sheet renders to DOCX/PDF/HTML also egresses as TOML — one owner bound many ways, the producer held inside the emit owner.
- recover seam (`document/lens#LENS`): the `LensOp.TOML_READ` -> `(_toml_arm, LensProvider.TOMLKIT)` core row recovers a `DocumentNode` via `tomlkit.parse(payload).unwrap()` then the shared `_value_node` recursion (mapping -> keyed `BlockNode`, sequence -> `ListNode(ORDERED)`, scalar -> `RunNode`), the exact inverse of the emit lowering over one node algebra; `unwrap()`'s proxy flattening guarantees `_value_node` never meets a split-table proxy.
- both-tier rails: `msgspec.to_builtins`/`from_builtins` (`libs/python/.api/msgspec.md`) bridges structure<->builtins, `expression.Result[T, E]` (`libs/python/.api/expression.md`) lifts `ParseError`/`ConvertError`, and `@beartype` guards signatures.
- diagnostics: a structured-documents receipt records the table/key/AoT tally and output byte length; `ParseError.line`/`.col` feeds a positional config diagnostic into `structlog` (`libs/python/.api/structlog.md`).
- retry: a network-fetched config source wraps `stamina.retry` (`libs/python/runtime/.api/stamina.md`) at the fetch boundary; the parse itself stays pure.
- validate-then-mutate rail (styled depth): `tomlkit.parse(src)` -> `msgspec`/`pydantic` decode of `doc.unwrap()` or a per-key read -> validate -> mutate the styled `TOMLDocument` in place via the factories -> `as_string()` is the comment-and-order-preserving config-rewrite path (`libs/python/.api/pydantic.md`), keeping the commented tree as the durable artifact while typed models own validation — the styled depth for a config tool, distinct from the lossy `unwrap()` egress the lens uses.

[LOCAL_ADMISSION]:
- style-preserving TOML processing feeding the structured-documents owner; a parse/edit/dump cycle that must keep comments, order, or quoting admits here, never the read-only stdlib `tomllib`.

[RAIL_LAW]:
- Package: `tomlkit`
- Owns: style-preserving TOML parse/dump, deterministic `sort_keys` emission, the plain-builtins `dumps`/`parse().unwrap()` bridge, programmatic styled-document building over the container/scalar/key/trivia factory family, polymorphic `item()` wrap, multiline-array and super-table style control, out-of-order-table flattening, lossy `unwrap()` projection, and value-encoder registration
- Accept: deterministic plain-builtins TOML emission (`dumps(to_builtins(node), sort_keys=True)`) and `parse().unwrap()` recovery feeding the `document` emit/lens seam; lossless styled round-trip for a config-rewrite rail; validation layered over `unwrap()` or per-key reads
- Reject: wrapper-renames of `parse`/`dumps`; a hand-walked `dict`->styled-item build where `dumps` accepts the builtins directly on egress; a raw-`dict` rebuild dropping style on the styled path; a per-type build family where `item()` discriminates on shape; a `tomllib` fallback where styled round-trip is admitted; a hand-special-cased value branch where a `to_builtins` `enc_hook` or `register_encoder` owns the encoding; cross-parsing TOML through the XML/YAML owners; identity minting the runtime owns
