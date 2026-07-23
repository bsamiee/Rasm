# [PY_RUNTIME_API_TREE_SITTER]

`tree-sitter` owns incremental structural parsing: a grammar-bound reusable `Parser`, the resulting `Tree`/`Node` syntax tree with cursor and field-named traversal over byte/`Point` ranges, incremental `edit`+reparse with `changed_ranges`, and an S-expression `Query`/`QueryCursor` match surface with capture-quantifier, range/depth/limit scoping, and predicate introspection. It parses source structurally for the companion seam and the assay `code query` rail; it is not a language server or type checker.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tree-sitter`
- package: `tree-sitter` (`MIT`)
- module: `tree_sitter`
- rail: parsing
- namespaces: `tree_sitter`
- cancellation: `progress_callback` is the sole run bound — passed to `parse()`/`matches()`/`captures()`, it returns `True` to CANCEL. A cancelled `parse` raises `ValueError("Parsing failed")`; a cancelled `matches`/`captures` returns bounded partial results without raising. No wall-clock timeout knob exists.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parser and tree family

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                                                          |
| :-----: | :----------- | :------------ | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `Parser`     | parser        | incremental parse driver; settable `language`/`included_ranges`/`logger`; `progress_callback` cancels |
|  [02]   | `Language`   | grammar       | compiled grammar; `abi_version`/`semantic_version`/`name` + symbol/field/state introspection          |
|  [03]   | `Tree`       | tree          | parsed syntax tree; `root_node`/`walk`/`edit`/`changed_ranges`/`copy`                                 |
|  [04]   | `Node`       | node          | syntax-tree node; rich navigation + `text`/error flags                                                |
|  [05]   | `TreeCursor` | cursor        | efficient stateful tree walker                                                                        |
|  [06]   | `Range`      | range         | byte + `Point` span; `edit()` re-syncs the span to a source edit with no tree                         |
|  [07]   | `Point`      | point         | `(row, column)` position; `tuple[int, int]` subclass; `edit()` re-syncs to a source edit              |
|  [08]   | `LogType`    | enum          | parser log category (`PARSE`/`LEX`)                                                                   |

[PUBLIC_TYPE_SCOPE]: query family

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]        | [CAPABILITY]                                                                          |
| :-----: | :------------------ | :------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `Query`             | query                | compiled S-expression pattern; introspectable                                         |
|  [02]   | `QueryCursor`       | cursor               | query execution cursor with range/depth/limit scoping                                 |
|  [03]   | `QueryPredicate`    | `Protocol`           | custom `#...?` predicate handler `(predicate, args, pattern_index, captures) -> bool` |
|  [04]   | `QueryError`        | fault (`ValueError`) | raised at `Query(...)` compile on malformed query source                              |
|  [05]   | `LookaheadIterator` | iterator             | valid next symbols from a grammar parse state                                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse and tree operations
- `parse` is polymorphic on its first argument: a `ByteString` (`encoding='utf8'|'utf16'|'utf16le'|'utf16be'`) returns a `Tree`, or a read-callback `Callable[[int, Point], ByteString | None]` streams a rope-backed source and accepts `progress_callback`. `Parser.included_ranges` scopes an embedded-language parse to byte ranges; `logger` receives `(LogType, str)` events.

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                             |
| :-----: | :----------------------------------------------------- | :------- | :------------------------------------------------------- |
|  [01]   | `Parser(language, *, included_ranges, logger)`         | build    | parser bound to a grammar, optionally scoped/logged      |
|  [02]   | `Parser.parse(source, old_tree=None, encoding='utf8')` | parse    | bytes -> `Tree`; pass `old_tree` for incremental reparse |
|  [03]   | `Parser.parse(read_callback, …, progress_callback=)`   | parse    | streaming read-callback parse with cancellation          |
|  [04]   | `Parser.included_ranges` / `logger`                    | scope    | byte-range scope; `(LogType, str)` parse/lex events      |
|  [05]   | `Parser.reset()`                                       | reset    | discard partial parse state                              |
|  [06]   | `Tree.root_node` / `root_node_with_offset`             | navigate | tree root, or a subtree rebased to an offset             |
|  [07]   | `Tree.walk()` / `Tree.language`                        | navigate | obtain a `TreeCursor`; the producing grammar             |
|  [08]   | `Tree.edit(...)` / `Tree.changed_ranges(new_tree)`     | edit     | apply a byte/point edit, diff vs a new tree              |
|  [09]   | `Tree.included_ranges` / `Tree.copy()`                 | scope    | parsed byte ranges; copy the tree                        |
|  [10]   | `Tree.print_dot_graph(file)`                           | emit     | emit a Graphviz dot graph                                |
|  [11]   | `Point.edit(start_byte, …, new_end_point)`             | edit     | re-sync a bare point to a source edit, no tree           |
|  [12]   | `Range.edit(start_byte, …, new_end_point)`             | edit     | re-sync a bare range to a source edit, no tree           |

- `Point.edit`: takes the six `InputEdit` deltas and RETURNS `(edited_point, new_start_byte)` without mutating `self` (`Point` is an immutable `tuple` subclass).
- `Range.edit`: takes the same six deltas and mutates the range span in place, returning `None`.

[ENTRYPOINT_SCOPE]: node navigation
- Every member is `Node.*`; `text` returns the source slice as `bytes | None`. `descendant_for_*_range` locates the tightest node over a span, `child_with_descendant` the child containing a given node.

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `children` / `named_children`                                      | navigate | child enumeration                              |
|  [02]   | `child_by_field_name(name)` / `child_by_field_id(id)`              | navigate | field-named / field-id child access            |
|  [03]   | `children_by_field_name` / `children_by_field_id`                  | navigate | all children for a field                       |
|  [04]   | `field_name_for_child(i)` / `field_name_for_named_child(i)`        | navigate | field name of a positional child               |
|  [05]   | `descendant_for_byte_range(start, end)`                            | navigate | tightest node over a byte span                 |
|  [06]   | `named_descendant_for_byte_range`                                  | navigate | tightest named node over a byte span           |
|  [07]   | `descendant_for_point_range(start, end)`                           | navigate | tightest node over a `Point` span              |
|  [08]   | `named_descendant_for_point_range`                                 | navigate | tightest named node over a `Point` span        |
|  [09]   | `first_child_for_byte(b)` / `first_named_child_for_byte`           | navigate | child covering a byte offset                   |
|  [10]   | `child_with_descendant(node)`                                      | navigate | child containing a descendant                  |
|  [11]   | `parent` / `next_sibling` / `prev_sibling`                         | navigate | parent and sibling walk                        |
|  [12]   | `next_named_sibling` / `prev_named_sibling`                        | navigate | named-sibling walk                             |
|  [13]   | `text` / `type` / `grammar_name` / `kind_id` / `grammar_id` / `id` | read     | source `bytes\|None`, type, grammar symbol, id |
|  [14]   | `is_named` / `is_error` / `has_error`                              | read     | named + error flags                            |
|  [15]   | `is_missing` / `is_extra` / `has_changes`                          | read     | recovery + incremental-dirty flags             |
|  [16]   | `parse_state` / `next_parse_state` / `descendant_count`            | read     | parse-state ids, subtree count                 |
|  [17]   | `child_count` / `named_child_count`                                | read     | child counts                                   |
|  [18]   | `start_byte` / `end_byte` / `byte_range`                           | read     | byte span from the node                        |
|  [19]   | `start_point` / `end_point` / `range`                              | read     | `Point` span from the node                     |
|  [20]   | `walk()`                                                           | navigate | obtain a `TreeCursor` rooted at the node       |

[ENTRYPOINT_SCOPE]: cursor traversal
- `TreeCursor` is the allocation-light walk; byte/point descent moves return the child index (`int | None`), sibling/child/parent moves return `bool`.

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `goto_first_child` / `goto_last_child` / `goto_parent`           | walk     | depth-first moves (`-> bool`)              |
|  [02]   | `goto_next_sibling` / `goto_previous_sibling`                    | walk     | sibling moves (`-> bool`)                  |
|  [03]   | `goto_first_child_for_byte(b)` / `goto_first_child_for_point(p)` | walk     | child covering a position (`-> int\|None`) |
|  [04]   | `goto_descendant(index)` / `reset(node)` / `reset_to(cursor)`    | walk     | jump to descendant index / re-seat cursor  |
|  [05]   | `cursor.node` / `depth` / `descendant_index`                     | read     | current node + depth                       |
|  [06]   | `cursor.field_name` / `field_id`                                 | read     | current field name/id                      |
|  [07]   | `cursor.copy()`                                                  | snapshot | copy the cursor for branch exploration     |

[ENTRYPOINT_SCOPE]: query operations
- `captures(node)` flattens captures by name, `matches(node)` groups per pattern; both take a `predicate: QueryPredicate` for non-built-in `#...?` directives (`#eq?`/`#match?` evaluate internally). INTERSECTING (`set_byte_range`) and FULLY-CONTAINED (`set_containing_byte_range`) scopes combine.

| [INDEX] | [SURFACE]                                                            | [SHAPE]         | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------- | :-------------- | :--------------------------------------------- |
|  [01]   | `Query(language, source)`                                            | build           | compile an S-expression query                  |
|  [02]   | `QueryCursor(query, *, match_limit=0xFFFFFFFF)`                      | build           | execution cursor for a compiled query          |
|  [03]   | `QueryCursor.matches(node, predicate=None, progress_callback=None)`  | match           | `list[(pattern_index, {capture: [Node]})]`     |
|  [04]   | `QueryCursor.captures(node, predicate=None, progress_callback=None)` | match           | `{capture_name: [Node]}` flattened mapping     |
|  [05]   | `set_byte_range` / `set_point_range`                                 | scope           | run scoped to an INTERSECTING range            |
|  [06]   | `set_containing_byte_range` / `set_containing_point_range`           | scope           | restrict to matches FULLY CONTAINED in a range |
|  [07]   | `QueryCursor.set_max_start_depth`                                    | scope           | cap match start depth                          |
|  [08]   | `QueryCursor.match_limit` / `did_exceed_match_limit`                 | bound           | cap matches, detect truncation                 |
|  [09]   | `Query.pattern_count` / `capture_count` / `string_count`             | introspect      | enumerate patterns/captures/strings            |
|  [10]   | `Query.capture_name(i)` / `string_value(i)`                          | introspect      | capture name / literal string by index         |
|  [11]   | `Query.pattern_settings(i)` / `pattern_assertions(i)`                | introspect      | `#set!` / `#is?` directives                    |
|  [12]   | `Query.start_byte_for_pattern(i)` / `end_byte_for_pattern(i)`        | introspect      | pattern source span                            |
|  [13]   | `Query.is_pattern_rooted(i)` / `is_pattern_non_local(i)`             | introspect      | pattern rooted / non-local flags               |
|  [14]   | `Query.is_pattern_guaranteed_at_step(i)` / `capture_quantifier(p,c)` | introspect      | step guarantee / capture quantifier            |
|  [15]   | `Query.disable_capture(name)` / `disable_pattern(i)`                 | tune            | suppress captures/patterns before running      |
|  [16]   | `QueryPredicate` (`Protocol`) / `QueryError`                         | predicate/fault | custom predicate shape; malformed-query raise  |
|  [17]   | `Language.lookahead_iterator(state)`                                 | grammar         | `LookaheadIterator` over valid next symbols    |

[ENTRYPOINT_SCOPE]: grammar introspection
- Every member is `Language.*` except the `LookaheadIterator.*` rows; `Language` is a fully introspectable grammar, not an opaque capsule. Symbol ids round-trip through `node_kind_for_id`/`id_for_node_kind`, field ids through `field_name_for_id`/`field_id_for_name`, and `supertypes`/`subtypes(supertype)` expose the supertype hierarchy that types captured nodes.

| [INDEX] | [SURFACE]                                                               | [SHAPE]    | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------------- | :--------- | :-------------------------------------------- |
|  [01]   | `abi_version` / `semantic_version` / `name`                             | introspect | compat band, `(maj,min,patch)`, name          |
|  [02]   | `node_kind_count` / `parse_state_count` / `field_count`                 | introspect | grammar size metrics                          |
|  [03]   | `node_kind_for_id(id)` / `id_for_node_kind(kind, named)`                | introspect | symbol id <-> name round-trip                 |
|  [04]   | `node_kind_is_named(id)` / `is_visible(id)` / `is_supertype(id)`        | introspect | symbol classification flags                   |
|  [05]   | `field_name_for_id(id)` / `field_id_for_name(name)`                     | introspect | field id <-> name round-trip                  |
|  [06]   | `supertypes` / `subtypes(supertype)`                                    | introspect | grammar supertype hierarchy                   |
|  [07]   | `next_state(state, id)` / `lookahead_iterator(state)`                   | introspect | parse-state transition / valid next symbols   |
|  [08]   | `LookaheadIterator.names()` / `symbols()`                               | iterate    | enumerate valid next symbols                  |
|  [09]   | `LookaheadIterator.current_symbol_name` / `reset(state, language=None)` | iterate    | current symbol / re-seat iterator             |
|  [10]   | `copy()` / `eq` / `hash`                                                | identity   | copy or key a grammar; `Language` is hashable |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- parser: one `Parser` per grammar, reused; an edited source reparses with `old_tree`, never a full re-parse where the edit is known.
- traversal: hot walks use a `TreeCursor` (`walk`) or field-named access (`child_by_field_name`/`child_by_field_id`); positional `children[i]` guessing is deleted. Symbol/field names resolve to ids once via `id_for_node_kind`/`field_id_for_name` and match as integers in the hot loop.
- query: structural extraction compiles `Query(language, source)` once and runs it through a `QueryCursor` with named captures and range/depth scoping; `captures()` returns the flattened mapping, `matches()` serves only where the per-match pattern index or grouping is needed.
- bound: a long parse or query rides one `progress_callback` (`True` cancels); a cancelled query yields silently-partial captures, so completeness grades against a pre-gate or `match_limit`, never the cancel hook.
- edit: incremental updates call `Tree.edit` with the byte/point delta then reparse with `old_tree`; `changed_ranges(new_tree)` drives downstream re-processing and `Node.has_changes` flags dirty subtrees. A bare `Point`/`Range` re-syncs through `Point.edit`/`Range.edit`, never a manual offset recompute.
- range: positions read as byte offsets and `Point` row/columns off the node (`byte_range`/`range`); `Node.text` is the source slice (`bytes | None`), never a re-slice of the input.
- recovery: malformed input reads `has_error`/`is_error`/`is_missing` on the tree — `parse` always returns a `Tree`. A malformed query SOURCE instead raises `QueryError` at compile.

[STACKING]:
- `tree-sitter-python`(`.api/tree-sitter-python.md`) / `tree-sitter-typescript`(`.api/tree-sitter-typescript.md`): each grammar capsule (`language()`; `language_typescript()`/`language_tsx()`) wraps once into `tree_sitter.Language(...)` as a keyed grammar row feeding the reused `Parser`; the bundled `HIGHLIGHTS_QUERY`/`LOCALS_QUERY`/`TAGS_QUERY` sources compile through `Query` and run under a `QueryCursor`.
- assay `code query` rail: grammar `Language` -> reused `Parser` -> `Tree` -> `Query` compiled once + `QueryCursor` scoped by `set_byte_range`/`set_max_start_depth` -> `captures()` keyed by name. Capture-name and node-kind strings resolve to ids against the `Language` introspection surface at registry build, so the match loop compares integers. `Node.text` slices (`bytes`) and `Point` spans map field-for-field onto the structural-search receipt's location facts.

[LOCAL_ADMISSION]:
- one reusable `Parser` per grammar; grammar `Language` objects arrive from the grammar packages and construct once into a keyed registry, never per parse.
- structural source parsing for the companion seam; grammar mechanics arrive settled from the grammar catalogs, and this surface re-implements no match loop.

[RAIL_LAW]:
- Package: `tree-sitter`
- Owns: incremental parsing (bytes or streaming read-callback), grammar `Language` introspection (symbol/field/state ids, supertype hierarchy), cursor and field-named traversal, incremental edits with `changed_ranges`, and structural query matching with capture quantifiers + predicate/assertion introspection
- Accept: one reused `Parser` per grammar `Language`, `Query` compiled once per pattern run through a scoped `QueryCursor`, ids resolved once and matched as integers, positions read off the node
- Reject: per-parse parser construction, full re-parse of edited source, positional child-index guessing, node-type string matching where a query fits, position recompute where the node span or `Point.edit`/`Range.edit` serves
