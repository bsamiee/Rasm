# [PY_ARTIFACTS_API_RUAMEL_YAML]

`ruamel-yaml` owns the round-trip YAML surface of the artifacts structured-documents rail: one configured `YAML` engine driving typ-selected load/dump, the comment-and-order-preserving `CommentedMap`/`CommentedSeq` containers, the styled-scalar family that fixes emission style, and tag/class registration. It preserves comments, key order, anchors/aliases, tags, and per-node styling across a load-edit-dump cycle; YAML routes here, XML to `lxml`, TOML to `tomlkit`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ruamel-yaml`
- package: `ruamel-yaml`
- import: `from ruamel.yaml import YAML`
- owner: `artifacts`
- rail: structured documents
- license: MIT
- entry points: none (library only)
- capability: round-trip YAML 1.1/1.2 load/dump preserving comments, key order, anchors/aliases, tags, and per-node block/flow/quote styling; `rt`/`safe`/`full`/`base` typ variants; multi-document streams; styled-scalar construction; programmatic comment/anchor attach; class registration (`@yaml_object`) and the low-level tag/resolver/representer hooks (`add_representer`/`add_constructor`/`add_implicit_resolver`/`add_path_resolver`); low-level compose/serialize/parse/emit event and node rails

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: engine and container roots
- rail: structured documents

| [INDEX] | [SYMBOL]               | [PACKAGE_ROLE]     | [CAPABILITY]                                                                          |
| :-----: | :--------------------- | :----------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `YAML`                 | engine root        | typ-selected load/dump engine; carries `indent`/`width`/`version`/styling config      |
|  [02]   | `CommentedMap`         | mapping container  | dict preserving comments, key order, merge keys, anchors; `.ca`/`.lc`/`.fa` access    |
|  [03]   | `CommentedSeq`         | sequence container | list preserving comments, styling, anchors                                            |
|  [04]   | `CommentedKeyMap`      | hashable container | round-trip map usable as a mapping key                                                |
|  [05]   | `CommentedKeySeq`      | hashable container | round-trip seq usable as a mapping key                                                |
|  [06]   | `TaggedScalar`         | tagged node        | a scalar carrying an explicit `!tag`                                                  |
|  [07]   | `CommentToken`         | comment node       | a parsed comment attached to a node via `.ca`                                         |
|  [08]   | `anchor.Anchor`        | anchor handle      | the `&name` anchor on a node (`.anchor`/`yaml_set_anchor`); from `ruamel.yaml.anchor` |
|  [09]   | `RoundTripConstructor` | round-trip codec   | the round-trip load half preserving fidelity (the `rt` typ)                           |
|  [10]   | `RoundTripRepresenter` | round-trip codec   | the round-trip dump half preserving fidelity (the `rt` typ)                           |
|  [11]   | `SafeConstructor`      | safe codec         | the safe typ load half; `add_constructor` extends it                                  |
|  [12]   | `SafeRepresenter`      | safe codec         | the safe typ dump half; `add_representer` extends it                                  |
|  [13]   | `Version`              | doc metadata       | `Version(major, minor)` (`ruamel.yaml.docinfo.Version`), carried by `YAML.version`    |
|  [14]   | `DocInfo`              | doc metadata       | directives/tags/version read from a parsed stream (`parsed.docinfo`)                  |
|  [15]   | `Tag`                  | tag handle         | a resolved `!tag`/`!!type` on a node (set on `CommentedMap`/`TaggedScalar`)           |
|  [16]   | `YAMLObject`           | class registration | base class binding a custom class to a tag for round-trip                             |
|  [17]   | `yaml_object`          | class registration | `@yaml_object(yaml)` decorator binding a class to a tag for round-trip                |

[PUBLIC_TYPE_SCOPE]: styled-scalar family
- rail: structured documents — `ruamel.yaml.scalarstring`, `scalarint`, `scalarfloat`, `scalarbool`, `timestamp`

Construct these in place of plain `str`/`int` to force emission style on dump; on round-trip load the engine reconstructs them so the style survives.

| [INDEX] | [SYMBOL]                                          | [PACKAGE_ROLE] | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------ | :------------- | :-------------------------------------------------------------- |
|  [01]   | `LiteralScalarString`                             | block scalar   | force `\|` literal block (preserve newlines, embedded code/SQL) |
|  [02]   | `FoldedScalarString`                              | block scalar   | force `>` folded block (wrap long prose)                        |
|  [03]   | `SingleQuotedScalarString`                        | quoted scalar  | force `'...'` quoting                                           |
|  [04]   | `DoubleQuotedScalarString`                        | quoted scalar  | force `"..."` quoting                                           |
|  [05]   | `PlainScalarString`                               | plain scalar   | force unquoted plain style                                      |
|  [06]   | `ScalarInt` / `BinaryInt` / `OctalInt` / `HexInt` | based int      | preserve/force `0b`/`0o`/`0x` integer representation            |
|  [07]   | `ScalarFloat`                                     | styled float   | preserve width/precision/exponent of a float literal            |
|  [08]   | `ScalarBoolean`                                   | styled bool    | preserve the exact `true`/`yes`/`on` boolean token              |
|  [09]   | `TimeStamp`                                       | timestamp      | a round-trip-preserving `datetime` subtype                      |

[PUBLIC_TYPE_SCOPE]: faults
- rail: structured documents — `ruamel.yaml.error`, `constructor`

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]    | [CAPABILITY]                                               |
| :-----: | :------------------------------ | :---------------- | :--------------------------------------------------------- |
|  [01]   | `YAMLError`                     | engine fault      | base YAML failure (top-level `ruamel.yaml` re-export)      |
|  [02]   | `error.MarkedYAMLError`         | located fault     | a failure carrying source `problem_mark` line/col          |
|  [03]   | `error.YAMLStreamError`         | stream fault      | bad/missing stream target on dump                          |
|  [04]   | `constructor.DuplicateKeyError` | constructor fault | raised when `allow_duplicate_keys=False` (default) on load |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: engine construction and configuration
- rail: structured documents

`YAML.__init__` is keyword-only; one instance is configured once and reused for load and dump. Output style rides the attributes below, never a per-call argument.

| [INDEX] | [SURFACE]                                                   | [CAPABILITY]                                                        |
| :-----: | :---------------------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | `YAML(*, typ='rt', pure=False, output=None, plug_ins=None)` | engine; `typ` in `'rt'`/`'safe'`/`'full'`/`'base'`, list to compose |
|  [02]   | `YAML.indent(mapping=None, sequence=None, offset=None)`     | block indent widths and sequence-dash offset                        |
|  [03]   | `YAML.width` (`int`)                                        | line-wrap column for folded/flow output                             |
|  [04]   | `YAML.version` (`(1,1)`/`(1,2)` or `str`)                   | force the `%YAML` directive and resolver version                    |
|  [05]   | `YAML.preserve_quotes` (`bool`)                             | keep original scalar quoting on round-trip                          |
|  [06]   | `YAML.default_flow_style` (`bool`/`None`)                   | block vs flow collection style (`False` = block)                    |
|  [07]   | `YAML.allow_duplicate_keys` (`bool`, default `False`)       | accept duplicate mapping keys instead of `DuplicateKeyError`        |
|  [08]   | `YAML.explicit_start` / `explicit_end` (`bool`)             | emit `---` / `...` document markers                                 |
|  [09]   | `YAML.sequence_dash_offset` / `top_level_colon_align`       | dash indentation and key-colon alignment                            |
|  [10]   | `YAML.tags` (`dict \| None`)                                | custom `!handle!` -> tag-prefix directive map for emit              |
|  [11]   | `YAML.compact` / `block_seq_indent`                         | compact flow-collection emission; block-sequence indent width       |

[ENTRYPOINT_SCOPE]: load / dump and node builders
- rail: structured documents

`load`/`dump` accept a `pathlib.Path` (engine owns open/close) or a text/byte stream; `dump(data, stream=None, transform=fn)` or a `StringIO` sink is the in-memory-string idiom.

| [INDEX] | [SURFACE]                                                       | [CAPABILITY]                                                          |
| :-----: | :-------------------------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `YAML.load(stream: Path \| str \| IO) -> Any`                   | parse one document to commented containers                            |
|  [02]   | `YAML.load_all(stream) -> Iterator[Any]`                        | iterate a multi-document (`---`-separated) stream                     |
|  [03]   | `YAML.dump(data, stream: Path \| IO = None, *, transform=None)` | emit one document; `transform(str)->str` post-processes the text      |
|  [04]   | `YAML.dump_all(documents: Iterable, stream, *, transform=None)` | emit a multi-document stream                                          |
|  [05]   | `YAML.map(**kw) -> CommentedMap`                                | build an engine-bound `CommentedMap` (correct typ wiring)             |
|  [06]   | `YAML.seq(*args) -> CommentedSeq`                               | build an engine-bound `CommentedSeq`                                  |
|  [07]   | `YAML.register_class(cls) -> cls`                               | register a `@yaml_object`-style class for round-trip (decorator/call) |

[ENTRYPOINT_SCOPE]: tag / codec / resolver extension hooks
- rail: structured documents — module-level `ruamel.yaml` functions and the `@yaml_object` decorator

For a custom domain type that is not a plain mapping/sequence, register a tag<->object codec instead of walking the tree. `register_class`/`@yaml_object(yaml)` is the high-level binding (declares `yaml_tag` + `to_yaml`/`from_yaml`); the `add_*` functions are the low-level hooks — `add_representer(data_type, fn, Dumper=None)` / `add_constructor(tag, fn, Loader=None)`, the `add_multi_*(tag_prefix, fn)` prefix-family variants, `add_implicit_resolver(tag, regexp, first=None)`, `add_path_resolver(tag, path, kind=None)` — extending the engine's own constructor/representer.

| [INDEX] | [SURFACE]                                         | [CAPABILITY]                                                                    |
| :-----: | :------------------------------------------------ | :------------------------------------------------------------------------------ |
|  [01]   | `yaml_object`                                     | `@yaml_object(yaml)` decorator binding a class to its `yaml_tag` for round-trip |
|  [02]   | `add_representer` / `add_constructor`             | tag<->object codec for one exact type (dump half / load half)                   |
|  [03]   | `add_multi_representer` / `add_multi_constructor` | codec for a tag-prefix family / subclass hierarchy                              |
|  [04]   | `add_implicit_resolver`                           | auto-resolve a plain scalar pattern to a tag (no explicit `!tag`)               |
|  [05]   | `add_path_resolver`                               | resolve a tag by its position in the document path                              |
|  [06]   | `DocInfo` / `Tag`                                 | inspect directives/version/tags via `parsed.docinfo`; the resolved node `.tag`  |

[ENTRYPOINT_SCOPE]: comment, anchor, and position attach
- rail: structured documents — `CommentedMap` / `CommentedSeq` methods

Attach comments and anchors to an in-memory tree before dump, or read source positions after load: `.ca` is the comment attribute, `.lc` the line/column, `.fa` the flow/block-style flag.

| [INDEX] | [SURFACE]                                                              | [CAPABILITY]                                              |
| :-----: | :--------------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `yaml_set_start_comment(comment, indent=0)`                            | leading block comment above the collection                |
|  [02]   | `yaml_set_comment_before_after_key(key, before=None, after=None, ...)` | comment lines before/after a specific key                 |
|  [03]   | `yaml_add_eol_comment(comment, key=NotNone, column=None)`              | end-of-line `# ...` comment on a key/index                |
|  [04]   | `insert(pos, key, value, comment=None)`                                | ordered insert at position with optional eol comment      |
|  [05]   | `yaml_set_anchor(value, always_dump=False)` / `yaml_anchor()`          | set/get the `&anchor` so later `*alias` refs survive      |
|  [06]   | `add_yaml_merge([(idx, CommentedMap)])` / `merge`                      | inject a `<<:` merge key referencing another mapping      |
|  [07]   | `.ca` / `.lc` / `.fa` (attribute access)                               | comment map / source line-col / flow-vs-block style flags |

[ENTRYPOINT_SCOPE]: low-level event and node rails
- rail: structured documents

`YAML` surfaces its four-stage pipeline for streaming transforms and node-level rewrites without full materialization.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                       | [CAPABILITY]                                    |
| :-----: | :--------------------------------- | :--------------------------------- | :---------------------------------------------- |
|  [01]   | `YAML.parse`                       | `parse(stream) -> Iterator[Event]` | token-event stream (scanner/parser stage)       |
|  [02]   | `YAML.compose` / `compose_all`     | `compose(stream) -> Node`          | representation-graph node tree (composer stage) |
|  [03]   | `YAML.serialize` / `serialize_all` | `serialize(node, stream)`          | node tree back to event/text                    |
|  [04]   | `YAML.emit`                        | `emit(events, stream)`             | event stream to text (emitter stage)            |

## [04]-[IMPLEMENTATION_LAW]

[YAML_ROUNDTRIP]:
- import: `from ruamel.yaml import YAML` at boundary scope only; module-level import is banned by the manifest import policy (the emit/lens owners declare `lazy from ruamel.yaml import YAML`).
- engine axis: one configured `YAML` instance owns load and dump; `typ` (`'rt'`/`'safe'`/`'full'`/`'base'`) selects the codec and `pure=True` forces the Python codec — never a per-mode `*Loader`/`*Dumper` class. `'rt'` is the default, so comments, key order, anchors, and styling survive a load->edit->dump cycle.
- container axis: `CommentedMap`/`CommentedSeq` are the round-trip carriers; edit the tree in place or build via `yaml.map()`/`yaml.seq()` — never round-trip through a plain `dict`, which drops `.ca`/`.lc` and collapses anchors.
- style axis: emission style is data-driven — the styled-scalar family (`LiteralScalarString` for embedded code/SQL blocks, `FoldedScalarString` for prose, the quoted/based-int types for exact representation) and the engine attributes (`indent`/`width`/`version`/`default_flow_style`); set style on the value object or the engine, never by string-munging the emitted text.
- stream axis: `load`/`dump` (single) and `load_all`/`dump_all` (multi-document) are engine rows; `dump(data, transform=fn)` or a `StringIO` sink yields a string without a temp file; a `pathlib.Path` argument lets the engine own open/close.
- tag axis: a custom domain type round-trips through `yaml.register_class(cls)` / `@yaml_object(yaml)` (declaring `yaml_tag` + `to_yaml`/`from_yaml`) or the low-level `add_representer`/`add_constructor` (exact type) and `add_multi_*`/`add_implicit_resolver`/`add_path_resolver` hooks — extend the engine's own constructor/representer. A loaded `!tag` lands as `TaggedScalar`/tagged `CommentedMap`, read through the node `.tag`/`DocInfo`.
- fault axis: every failure descends from `YAMLError`; `MarkedYAMLError` carries the source `problem_mark` line/col and `DuplicateKeyError` fires under the default `allow_duplicate_keys=False`, both lifting to a typed `expression.Error` config-failure arm at the boundary.
- evidence: each load/dump captures typ, document count, YAML version, anchor count, registered-tag count, and output byte length as a structured-documents receipt.
- boundary: ruamel.yaml owns YAML round-trip; XML routes to `lxml`, TOML to `tomlkit`; live UI stays outside this package.

[STACKING]:
- structured-text triad: `ruamel-yaml` (YAML) joins `tomlkit` (TOML, `libs/python/artifacts/.api/tomlkit.md`) and `lxml` (XML, `libs/python/artifacts/.api/lxml.md`) as the structured-documents rail's three fidelity parsers — each preserves comments/order/styling so a rewrite edits the loaded tree in place; route by format, never cross-parse. Detect (`exchange/detect#DETECT`) maps `application/yaml`/`application/x-yaml`/`text/yaml` -> `MediaClass.DATA` -> the ruamel reader.
- emit seam (`document/emit#DOCUMENT`): the `DocumentMode.YAML` -> `Backend(Band.CORE, _ruamel_emit, ReceiptKind.DOCUMENT)` row lowers a `DocumentNode` tree through a `YAML()` engine into an `io.StringIO` sink, the `@receipted` weave minting `ArtifactReceipt.Document` under the plan's PRE-RUN key — a structured-document tree egresses as a YAML sidecar on the same `produced` entrypoint as the PDF/office rows, `DocumentPlan.bound` fanning `DocumentMode.YAML` -> an empty spec.
- recover seam (`document/lens#LENS`): the `LensOp.YAML_READ` -> `Route(_yaml_arm, LensProvider.RUAMEL, {RUAMEL}, ('typ',))` row recovers a `DocumentNode` tuple via `YAML(typ=spec.typ).load_all(BytesIO(payload))` folded through the shared `_value_tree` recursion (mapping -> keyed block, sequence -> ordered list, scalar -> run) — the inverse of the emit lowering over one node algebra; `load_all` subsumes the single-document case with no `multi` knob.
- alias-bomb guard: the lens `_grown` post-order expand/combine frontier rebuilds a recursive YAML value graph on an immutable `Block` stack with cycle/alias-bomb detection, so adversarial anchor nesting never overflows the interpreter frame.
- both-tier rails: `msgspec.to_builtins`/`from_builtins` (`libs/python/.api/msgspec.md`) bridges structure<->builtins, `expression.Result[T, E]` (`libs/python/.api/expression.md`) lifts `MarkedYAMLError`/`DuplicateKeyError`, `@beartype` guards signatures; `jinja2`-templated YAML (`libs/python/artifacts/.api/jinja2.md`) renders to text first, then loads here.
- validate-then-mutate rail: `YAML.load` -> `msgspec`/`pydantic` decode of the plain values -> validate -> mutate the `CommentedMap` in place -> `YAML.dump` is the comment-and-order-preserving config-rewrite path (`libs/python/.api/pydantic.md`), the commented tree staying the durable artifact while typed models own validation — distinct from the lossy plain-value egress the lens uses.

[LOCAL_ADMISSION]:
- fidelity-preserving YAML processing feeding the structured-documents owner; a load->edit->dump cycle that must keep comments, order, anchors, or styling admits here, never `pyyaml`.

[RAIL_LAW]:
- Package: `ruamel-yaml`
- Owns: round-trip YAML 1.1/1.2 load/dump preserving comments/order/anchors/tags/styling, the `typ` codec variants, styled-scalar construction, programmatic comment/anchor attach, multi-document streams, class registration and the low-level tag/resolver/representer hooks, and the compose/serialize/parse/emit rails
- Accept: fidelity-preserving YAML feeding the structured-documents emit/lens seam; validate-then-mutate config rewrite over a loaded `CommentedMap`
- Reject: wrapper-renames of `load`/`dump`; the deprecated module-level `safe_load`/`round_trip_*` functions where a `YAML` instance applies; a `pyyaml` fallback where ruamel is admitted (it destroys comments/order/styling); a per-`typ` engine class where a `typ` row suffices; a manual pre/post tree walk or `str()` of a custom type where `register_class`/`add_representer` owns the tag codec; string-munging emitted text where a styled-scalar type or engine attribute owns the style; cross-parsing YAML through the XML/TOML owners; identity minting the runtime owns
