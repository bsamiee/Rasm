# [PY_ARTIFACTS_API_TOMLKIT]

`tomlkit` supplies the style-preserving TOML surface for the artifacts structured-documents rail, and it serves the corpus on TWO axes the design pages actually compose. The EGRESS axis is plain-builtins emission: `dumps(to_builtins(node), sort_keys=True)` lowers a `msgspec`-flattened structure straight to deterministic TOML bytes — the `document/emit#DOCUMENT` `DocumentMode.TOML` arm and the `document/emit#DOCUMENT` `DocumentPlan.bound` `DocumentMode.TOML` fan row both reach this path, no styled tree built. The INGRESS axis is parse-then-project: `parse(payload).unwrap()` reads source to a `TOMLDocument` (a `Container` subclass) and projects to a plain `dict`/`list`/scalar map the `document/lens#LENS` `TOML_READ` `_value_node` recursion folds back into a `DocumentNode`. The full styled-item factory family (`table`/`aot`/`array`/`inline_table`/`key`, the scalar builders `integer`/`float_`/`boolean`/`string`/`date`/`time`/`datetime`, the trivia builders `comment`/`nl`/`ws`) plus the `register_encoder` hook are the available depth for a comment-and-order-preserving config-rewrite rail that mutates the loaded tree in place; no current consumer needs that round-trip, but it is the categorical capability the owner holds. The package owner never re-implements TOML scanning tomlkit already owns; on egress it never hand-walks a `dict` into styled items where `dumps` accepts the builtins directly, and on the styled path it never round-trips through a bare `dict` where the factories preserve trivia.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tomlkit`
- package: `tomlkit`
- import: `import tomlkit` (and `from tomlkit import items, exceptions, container` for the typed vocabulary)
- owner: `artifacts`
- rail: structured documents
- license: MIT
- installed: `0.15.0`
- build floor: pure-Python universal wheel (`py3-none-any`, `Root-Is-Purelib: true`), `Requires-Python >=3.9` — no abi tag, no cp-gate, present on cp315
- entry points: none (library only)
- capability: style-preserving TOML parse/dump (comments, whitespace, ordering, quote style retained), deterministic `sort_keys` emission, plain-builtins `dumps`/`parse().unwrap()` round-trip, programmatic styled-document building, the full styled-item factory family, scalar/trivia item builders, multiline-array and super-table control, lossy `unwrap()` projection to plain values, out-of-order-table flattening, and custom value-encoder registration/deregistration

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, container, and styled-item types
- rail: structured documents

`TOMLDocument` is the round-trip container `parse`/`document` mint — a `Container` subclass preserving the full source style. `Container` is the dict-like body owner shared by `TOMLDocument`, `Table`, and `InlineTable`; `OutOfOrderTableProxy` is the read-through view `Container.item(key)` returns when a table's sub-sections are split across the source (e.g. `[a]` … `[b]` … `[a.c]`) — `unwrap()` flattens it to one plain `dict`, so the lens `unwrap()` ingress never sees a non-dict where a table is expected. The `items` module carries the styled-value vocabulary: container items (`Table`/`AoT`/`Array`/`InlineTable`), scalar items (`Integer`/`Float`/`Bool`/`String`/`Date`/`Time`/`DateTime`), key items (`Key`/`SingleKey`/`DottedKey`), trivia (`Comment`/`Whitespace`/`Trivia`), and the `StringType`/`KeyType` style enums. Every styled value descends from `Item`; a programmatic edit mutates these in place so trivia survives.

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]    | [CAPABILITY]                                                                    |
| :-----: | :-------------------------------- | :---------------- | :----------------------------------------------------------------------------- |
|  [01]   | `TOMLDocument`                    | document root     | a parsed/built TOML document preserving style (a `Container` subclass)          |
|  [02]   | `container.Container`             | body owner        | dict-like `body`/`add`/`append`/`remove`/`item`/`last_item`/`unwrap`/`as_string` shared by document and tables |
|  [03]   | `container.OutOfOrderTableProxy`  | split-table view  | read-through proxy `Container.item(key)` returns for source-split table sections; `unwrap()` flattens to one `dict` |
|  [04]   | `items.Item`                      | item base         | the base of every styled value (`trivia`/`value`/`unwrap`/`as_string`/`comment`/`indent`/`discriminant`) |
|  [05]   | `items.Table`                     | table item        | a `[table]` preserving comments/order (`AbstractTable`; `append`/`indent`/`is_super_table`)     |
|  [06]   | `items.AoT`                       | array-of-tables   | an `[[array]]` of tables (`body` list of `Table`, `insert`)                     |
|  [07]   | `items.Array`                     | array item        | a styled array (`multiline`/`add_line`/`item`/`insert`/`clear`, multiline-capable) |
|  [08]   | `items.InlineTable`               | inline table      | a `{ }` inline table (`AbstractTable`; `append`)                               |
|  [09]   | `items.Integer/Float/Bool/String` | scalar items      | styled scalars preserving raw spelling; `String.from_raw(value, type_, escape)` / `String.type` |
|  [10]   | `items.Date/Time/DateTime`        | temporal items    | styled RFC-3339 temporals subclassing `date`/`time`/`datetime`; `replace`/`astimezone` keep style |
|  [11]   | `items.Key/SingleKey/DottedKey`   | key items         | styled bare/quoted/dotted keys; `is_dotted`/`is_multi`/`concat`/`as_string`     |
|  [12]   | `items.Comment/Whitespace/Trivia` | trivia items      | comment, whitespace (`is_fixed`), and the per-item `Trivia(indent, comment_ws, comment, trail)` carrier |
|  [13]   | `items.StringType` / `items.KeyType` | style enums    | `StringType.{SLB,SLL,MLB,MLL}` (single/multi-line basic/literal) with `select`/`toggle`/`is_multiline`; `KeyType.{Bare,Basic,Literal}` |
|  [14]   | `items.Null`                      | absent marker     | the styled absent/removed-item sentinel                                         |
|  [15]   | `items.CUSTOM_ENCODERS`           | encoder registry  | the runtime `list` of registered value encoders `item()` consults (typed by the `TYPE_CHECKING`-only `Encoder` protocol `(value, /) -> Item`) |

[PUBLIC_TYPE_SCOPE]: faults
- rail: structured documents — `tomlkit.exceptions`

`exceptions.TOMLKitError(Exception)` is the single root. `ParseError(ValueError, TOMLKitError)` is the malformed-source base carrying `line`/`col` and is the parent of every grammar fault. The container/convert faults descend from `TOMLKitError` directly: `NonExistentKey(KeyError, TOMLKitError)`, `KeyAlreadyPresent(TOMLKitError)`, `ConvertError(TypeError, ValueError, TOMLKitError)`, `InvalidStringError(ValueError, TOMLKitError)`. One `except TOMLKitError` catches all; `except ParseError` catches only grammar faults and gives positional `line`/`col`.

| [INDEX] | [SYMBOL]                                     | [PACKAGE_ROLE]  | [CAPABILITY]                                              |
| :-----: | :------------------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `exceptions.TOMLKitError`                    | engine root     | base of every tomlkit failure (`Exception`)              |
|  [02]   | `exceptions.ParseError`                      | parse base      | `ValueError + TOMLKitError`; malformed TOML carrying `line`/`col` |
|  [03]   | `exceptions.UnexpectedCharError`             | parse fault     | unexpected character during parse                        |
|  [04]   | `exceptions.UnexpectedEofError`              | parse fault     | premature end of source                                  |
|  [05]   | `exceptions.EmptyKeyError` / `EmptyTableNameError` | parse fault | empty key / empty table name                         |
|  [06]   | `exceptions.InvalidNumberError` / `InvalidNumberOrDateError` | parse fault | malformed number / number-or-date literal     |
|  [07]   | `exceptions.InvalidDateError` / `InvalidTimeError` / `InvalidDateTimeError` | parse fault | malformed temporal literal               |
|  [08]   | `exceptions.InvalidCharInStringError` / `InvalidControlChar` / `InvalidUnicodeValueError` | parse fault | malformed string content       |
|  [09]   | `exceptions.MixedArrayTypesError`            | parse fault     | heterogeneous array element types                        |
|  [10]   | `exceptions.NonExistentKey`                  | access fault    | `KeyError + TOMLKitError`; missing key on read           |
|  [11]   | `exceptions.KeyAlreadyPresent`               | edit fault      | duplicate-key insert                                     |
|  [12]   | `exceptions.ConvertError`                    | build fault     | `TypeError + ValueError + TOMLKitError`; un-encodable value (also the contract a custom `Encoder` raises) |
|  [13]   | `exceptions.InvalidStringError`              | build fault     | `ValueError + TOMLKitError`; invalid styled-string request |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse, dump, and document build
- rail: structured documents

`parse`/`loads` accept `str | bytes` and return a `TOMLDocument`; `load(fp)` reads a text/binary file object; `dumps`/`dump` emit with an optional `sort_keys` policy (the deterministic-bytes lever for a content-addressed pipeline). `dumps` accepts any `Mapping` — a plain `dict` from `msgspec.to_builtins` dumps directly, never requiring a pre-built styled tree. `document()` mints an empty container; `register_encoder`/`unregister_encoder` install/remove a custom value encoder appended to the module-level `CUSTOM_ENCODERS` list that `item(value)` consults.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                     | [CAPABILITY]                                       |
| :-----: | :------------------- | :-------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `parse`              | `parse(string: str \| bytes) -> TOMLDocument`                   | parse TOML source                                  |
|  [02]   | `loads`              | `loads(string: str \| bytes) -> TOMLDocument`                   | parse (alias of `parse`)                           |
|  [03]   | `load`               | `load(fp: IO[str] \| IO[bytes]) -> TOMLDocument`                | parse from a file object                           |
|  [04]   | `dumps`              | `dumps(data: Mapping[str, Any], sort_keys: bool=False) -> str`  | serialize to a string; `sort_keys=True` for deterministic bytes |
|  [05]   | `dump`               | `dump(data: Mapping[str, Any], fp: IO[str], *, sort_keys=False) -> None` | serialize to a file object                |
|  [06]   | `document`           | `document() -> TOMLDocument`                                    | build an empty document                            |
|  [07]   | `register_encoder`   | `register_encoder(encoder: E) -> E`                             | append a custom value encoder to `CUSTOM_ENCODERS` (decorator-usable) |
|  [08]   | `unregister_encoder` | `unregister_encoder(encoder: Encoder) -> None`                 | remove a previously registered encoder (suppresses `ValueError` if absent) |

[ENTRYPOINT_SCOPE]: item factories
- rail: structured documents

The container, key, scalar, and trivia factories are the single styled-build row set; a programmatic edit composes them so comments/whitespace/quoting survive. `item(value)` is the polymorphic wrap — it discriminates on the Python value shape through a fixed precedence (`Item` passthrough -> `bool` -> `int` -> `float` -> `dict` -> `list`/`tuple` -> `date`/`time`/`datetime` -> registered `CUSTOM_ENCODERS`), so a `dict` becomes `Table` (or `InlineTable` under an array/inline parent), a `list` of all-`dict` under a table/no parent becomes `AoT` else `Array`, and there is no per-type build family to call. `_sort_keys=True` sorts nested-`dict` keys during the wrap. The bare factories below are the explicit constructors for a styled edit that needs a specific item kind.

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]                                                                  | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------- | :--------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `item`                                         | `item(value: Any, _parent=None, _sort_keys=False) -> Item`                   | polymorphic wrap (discriminates on shape; consults `CUSTOM_ENCODERS`) |
|  [02]   | `value`                                        | `value(raw: str) -> Item`                                                    | parse a single TOML value expression          |
|  [03]   | `key_value`                                    | `key_value(src: str) -> tuple[Key, Item]`                                    | parse a `key = value` pair                     |
|  [04]   | `table`                                        | `table(is_super_table: bool \| None=None) -> Table`                          | build a table (super-table flag)               |
|  [05]   | `aot`                                          | `aot() -> AoT`                                                               | build an array-of-tables                       |
|  [06]   | `array`                                        | `array(raw: str='[]') -> Array`                                              | build an array from raw text                   |
|  [07]   | `inline_table`                                 | `inline_table() -> InlineTable`                                             | build an inline table                          |
|  [08]   | `key`                                          | `key(k: str \| Iterable[str]) -> Key`                                        | build a styled bare key (`str`) or dotted key (`Iterable[str]`) |
|  [09]   | `integer` / `float_` / `boolean`              | `integer(raw: str \| int)` / `float_(raw: str \| float)` / `boolean(raw: str \| bool)` | build styled scalar items            |
|  [10]   | `string`                                       | `string(raw: str, *, literal=False, multiline=False, escape=True) -> String` | build a styled string (literal/multiline) |
|  [11]   | `date` / `time` / `datetime`                  | `date(raw: str)` / `time(raw: str)` / `datetime(raw: str)`                   | build styled RFC-3339 temporal items           |
|  [12]   | `comment` / `nl` / `ws`                       | `comment(string: str)` / `nl()` / `ws(src: str)`                            | build trivia items (`nl()` is `ws("\n")`)      |

[ENTRYPOINT_SCOPE]: container edit and round-trip
- rail: structured documents — `Container` / `TOMLDocument` / styled-item methods

`TOMLDocument` and the container items expose a dict-like edit surface plus the style-bearing methods. `as_string()` re-emits the styled source; `unwrap()` projects to plain Python values (lossy — drops trivia, flattens `OutOfOrderTableProxy`, keeps `datetime`/`date`/`time` as stdlib temporals); `Array.add_line`/`multiline` and `Table.add`/`indent`/`is_super_table` control emission style.

| [INDEX] | [SURFACE]                                              | [CAPABILITY]                                                |
| :-----: | :----------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `Container.as_string()` / `Item.as_string()`           | re-emit the exact styled source string (document, table, array, scalar) |
|  [02]   | `Container.unwrap()` / `Item.unwrap()`                 | project to plain `dict`/`list`/scalar (drops style/trivia; flattens split tables; stdlib temporals) |
|  [03]   | `Container.body`                                        | the ordered `list[tuple[Key \| None, Item]]` backing the document (keyless rows are trivia) |
|  [04]   | `Container.add(key, item)` / `append` / `setdefault`   | style-preserving insert/merge                              |
|  [05]   | `Container.remove(key)` / `item(key)` / `last_item()`  | style-preserving delete; keyed fetch (may return `OutOfOrderTableProxy`); last inserted item |
|  [06]   | `Array.add_line(*items, indent, comment, add_comma, newline)` / `Array.multiline(bool)` | append a styled line / toggle multiline emission |
|  [07]   | `Array.item(index)` / `Array.insert(pos, value)` / `Array.clear()` | indexed array item access and mutation         |
|  [08]   | `Table.append(key, item)` / `Table.indent(n)` / `Table.is_super_table()` | table insert, indentation, super-table query |
|  [09]   | `AoT.body` / `AoT.insert(index, value)`                | the `list[Table]` backing the array-of-tables; positional table insert |
|  [10]   | `Item.trivia` / `Item.comment(text)` / `Item.indent(n)` | per-item trivia access and mutation                       |

## [04]-[IMPLEMENTATION_LAW]

[TOML_STYLED]:
- import: `import tomlkit` (and `from tomlkit import items, exceptions, container` for the typed vocabulary) at boundary scope only; module-level import is banned by the manifest import policy (the emit/lens owners declare `lazy import tomlkit`).
- egress axis (primary): `dumps(data, sort_keys=...)` accepts any `Mapping`; the canonical emission rail is `tomlkit.dumps(msgspec.to_builtins(node), sort_keys=True)` — a `msgspec`-flattened structure lowers straight to TOML bytes with no intermediate styled tree, `sort_keys=True` giving deterministic bytes for a content-addressed receipt. A styled-tree build is reserved for a config-rewrite that must preserve comments/order, never the default emission.
- ingress axis (primary): `parse(string)` returns a `TOMLDocument`; `.unwrap()` is the projection to plain `dict`/`list`/scalar the recover-TO consumer folds — it flattens an `OutOfOrderTableProxy` (a source-split `[a]` … `[a.c]` table) into one `dict` and keeps RFC-3339 temporals as stdlib `date`/`time`/`datetime`, so the downstream `_value_node` recursion sees a uniform nested mapping. `loads` aliases `parse`; `load(fp)` reads a file object.
- document axis: `TOMLDocument` is the single container (a `Container` subclass); `parse`/`loads`/`load` are the read entries and `document()` builds an empty one — never parallel document types. `Container` is the shared dict-like body for document and tables; `Container.item(key)` may return an `OutOfOrderTableProxy`, which `unwrap()` resolves.
- item axis (styled depth): the `table`/`aot`/`array`/`inline_table`/`key`/`integer`/`float_`/`boolean`/`string`/`date`/`time`/`datetime`/`comment`/`nl`/`ws` factories are the styled-build row set; `item(value)` is the polymorphic wrap discriminating on the Python value shape (precedence `Item` -> `bool` -> `int` -> `float` -> `dict`->`Table`/`InlineTable` -> `list`->`AoT`/`Array` -> temporal -> `CUSTOM_ENCODERS`). A programmatic styled edit composes factories so comments/whitespace/quoting survive; for plain emission `dumps` consumes builtins directly, so the styled factories are used only when trivia must be authored or preserved.
- style-control axis: `Array.multiline(True)`/`add_line(...)` control multiline-array emission, `Table.is_super_table()`/`indent(n)` control table style, `String.from_raw(value, type_, escape)` and the `StringType.{SLB,SLL,MLB,MLL}` enum force quoting style, the based/temporal items preserve raw spelling — style lives on the item or the enum, never in string-munging the emitted text.
- encoder axis: `register_encoder`/`unregister_encoder` append/remove a custom value-encoder on the runtime `items.CUSTOM_ENCODERS` list for `item(value)` to consult when the built-in shape dispatch does not match; the encoder conforms to the `TYPE_CHECKING`-only `Encoder` protocol (`(value, /) -> Item`) and raises `ConvertError` when it does not handle the value. A non-stdlib value type that survives `msgspec.to_builtins` as an opaque object encodes through a registered encoder, never a hand-special-cased branch in the document owner. `to_builtins` with an `enc_hook` is the FIRST line of defense (flatten the domain type to primitives upstream); `register_encoder` is the styled-path fallback when the value must reach `dumps`/`item` as itself.
- fault axis: every failure descends from `exceptions.TOMLKitError`; `ParseError` (with `line`/`col`) is the grammar-fault base, `NonExistentKey`/`KeyAlreadyPresent` are the access/edit faults, `ConvertError`/`InvalidStringError` are the build faults — the structured-documents fault rail catches `TOMLKitError` for the root and `ParseError` for malformed input, lifting each to a typed `expression.Error` arm at the boundary capsule.
- evidence: each emit/recover captures table count, key count, array-of-tables count, and output byte length as a structured-documents receipt fact; the deterministic `sort_keys=True` bytes feed the content key.
- boundary: tomlkit owns TOML round-trip; the stdlib `tomllib` read-only path is not used where styled round-trip or the unified parser is admitted; XML routes to `lxml` (`libs/python/artifacts/.api/lxml.md`), YAML to `ruamel-yaml` (`libs/python/artifacts/.api/ruamel-yaml.md`); live UI stays outside this package.

[STACKING]:
- structured-text triad: `tomlkit` (TOML) joins `lxml` (XML round-trip) and `ruamel-yaml` (YAML round-trip) as the three fidelity parsers of the structured-documents rail — route by format, never cross-parse; each preserves source structure so a rewrite rail edits the loaded tree in place and re-emits. The detect owner (`exchange/detect#DETECT`) routes `application/toml` -> `MediaClass.DATA` -> the `tomlkit` reader, sitting beside the `msgspec`/`lxml`/`ruamel-yaml`/`csvkit` data-family readers.
- emit seam (`document/emit#DOCUMENT`): the `DocumentMode.TOML` -> `Backend(Band.CORE, _tomlkit_emit, ReceiptKind.OFFICE)` row lowers the `DocumentNode` tree through `tomlkit.dumps(msgspec.to_builtins(plan.node)).encode()` into one `EmitFact(data=...)`, the `@receipted` weave minting `ArtifactReceipt.Office(key, bytes)` over `ContentIdentity.of(mode.value, fact.data)` — so a structured-document tree egresses as a machine-readable TOML sidecar on the same `produced` modal entrypoint as the PDF/office rows. The CORE band crosses `anyio.to_thread.run_sync` (`libs/python/.api/anyio.md`); a `ConvertError` on an un-encodable builtins value converts to the runtime `BoundaryFault` at the `async_boundary` capsule, never an interior raise.
- format-delegate seam (`document/emit#DOCUMENT` `DocumentPlan.bound`): `DocumentPlan.bound` fans `DocumentMode.TOML` -> `(DocumentMode.TOML, _bare_spec)` and threads it through the emit owner's `produced` entrypoint, so the same bound `DocumentNode` tree a spec-sheet renders to DOCX/PDF/HTML also egresses as a TOML structured-data form — one owner bound many ways, the TOML producer held inside the emit owner, never re-authored in the pipeline.
- recover seam (`document/lens#LENS`): the `LensOp.TOML_READ` -> `(_toml_arm, LensProvider.TOMLKIT)` core row recovers a `DocumentNode` via `tomlkit.parse(payload).unwrap()` then the shared `_value_node` recursion — a mapping folds to a keyed `BlockNode`, a sequence to a `ListNode(ORDERED)`, a scalar to a `RunNode`; `unwrap()`'s `OutOfOrderTableProxy` flattening guarantees `_value_node` never meets a split-table proxy where a `dict` is expected. This is the exact inverse of the emit `dumps` lowering, completing the bidirectional `document` seam over the one node algebra.
- both-tier rails: emit/recover compose under the shared substrate; `msgspec.to_builtins`/`from_builtins` (`libs/python/.api/msgspec.md`) bridges structure<->builtins, `expression.Result[T, E]` (`libs/python/.api/expression.md`) lifts `ParseError`/`ConvertError`, and `@beartype` guards signatures.
- diagnostics: a structured-documents receipt records the table/key/AoT tally and output byte length; `ParseError.line`/`.col` feeds a positional config diagnostic into `structlog` (`libs/python/.api/structlog.md`).
- retry: a network-fetched config source wraps `stamina.retry` (`libs/python/runtime/.api/stamina.md`) at the fetch boundary; the parse itself stays pure and retry-free.
- validate-then-mutate rail (styled depth): `tomlkit.parse(src)` -> `msgspec`/`pydantic` decode of `doc.unwrap()` (or a per-key read) -> validate -> mutate the styled `TOMLDocument` in place via the factories -> `as_string()` is the comment-and-order-preserving config-rewrite path (`libs/python/.api/pydantic.md`), keeping the commented tree as the durable artifact while typed models own validation — the available depth for a config tool, distinct from the lossy `unwrap()` egress the lens uses.

[RAIL_LAW]:
- Package: `tomlkit`
- Owns: style-preserving TOML parse/dump, deterministic `sort_keys` emission, plain-builtins `dumps`/`parse().unwrap()` round-trip, programmatic styled-document building, the full styled-item factory family (container/scalar/key/trivia), polymorphic `item()` wrap, multiline-array and super-table style control, out-of-order-table flattening, lossy `unwrap()` projection, and value-encoder registration/deregistration
- Accept: deterministic plain-builtins TOML emission (`dumps(to_builtins(node), sort_keys=True)`) and `parse().unwrap()` recovery feeding the `document` emit/lens seam; lossless styled round-trip for a config-rewrite rail; validation layered over `unwrap()`/per-key reads
- Reject: wrapper-renames of `parse`/`dumps`; a hand-walked `dict`->styled-item build where `dumps` accepts the builtins directly on egress; a raw-`dict` rebuild that drops style on the styled path; a per-type build family where `item()` discriminates on shape; a `tomllib` fallback where the unified parser or styled round-trip is admitted; a hand-special-cased value branch where a `to_builtins` `enc_hook` or `register_encoder` owns the encoding; cross-parsing TOML through the XML/YAML owners; identity minting the runtime owns
