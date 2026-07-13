# [PY_RUNTIME_API_TREE_SITTER]

`tree-sitter` (release, MIT) is the incremental parsing runtime: a reusable `Parser` driving a grammar `Language`, the resulting `Tree`/`Node` syntax tree with cursor and field-named traversal, byte/`Point` ranges, incremental `edit`+reparse with `changed_ranges`, and a `Query`/`QueryCursor` S-expression pattern-match surface with range/depth/limit scoping and predicate introspection. It is the runtime owner for structural source parsing the companion seam and the assay `code query` rail consume.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tree-sitter`
- package: `tree-sitter`
- version: `0.25.2`
- license: MIT
- import: `tree_sitter`
- owner: `runtime`
- rail: parsing
- namespaces: `tree_sitter`
- capability: incremental parser, grammar `Language` objects with full symbol/field/state introspection, syntax-tree traversal with cursors and field access, byte/`Point` ranges, incremental edits with `changed_ranges`, S-expression query matching with capture quantifiers + predicate/assertion introspection, and progress-callback-bounded parse/query runs
- timeout-shift: the legacy `Parser.timeout_micros` and `QueryCursor.timeout_micros` are both `@deprecated`; the modern bound is a `progress_callback` passed to `parse()` / `matches()` / `captures()` that returns `True` to CANCEL the run (returning `False` continues). A cancelled `parse` raises `ValueError("Parsing failed")`; a cancelled `matches`/`captures` returns the bounded partial results with no raise. Treat `progress_callback` as the live cancellation/timeout rail, `timeout_micros` as legacy.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parser and tree family
- rail: parsing

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [RAIL]                                                                                                |
| :-----: | :----------- | :------------ | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `Parser`     | parser        | incremental parse driver; settable `language`/`included_ranges`/`logger`; `timeout_micros` deprecated |
|  [02]   | `Language`   | grammar       | compiled grammar; `abi_version`/`semantic_version`/`name` + symbol/field/state introspection          |
|  [03]   | `Tree`       | tree          | parsed syntax tree; `root_node`/`walk`/`edit`/`changed_ranges`/`copy`                                 |
|  [04]   | `Node`       | node          | syntax-tree node; rich navigation + `text`/error flags                                                |
|  [05]   | `TreeCursor` | cursor        | efficient stateful tree walker                                                                        |
|  [06]   | `Range`      | range         | byte + `Point` span                                                                                   |
|  [07]   | `Point`      | point         | `(row, column)` byte position                                                                         |
|  [08]   | `LogType`    | enum          | parser log category (`PARSE`/`LEX`)                                                                   |

[PUBLIC_TYPE_SCOPE]: query family
- rail: parsing

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]        | [RAIL]                                                                                |
| :-----: | :------------------ | :------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `Query`             | query                | compiled S-expression pattern; introspectable                                         |
|  [02]   | `QueryCursor`       | cursor               | query execution cursor with range/depth/limit scoping                                 |
|  [03]   | `QueryPredicate`    | `Protocol`           | custom `#...?` predicate handler `(predicate, args, pattern_index, captures) -> bool` |
|  [04]   | `QueryError`        | fault (`ValueError`) | raised at `Query(...)` compile on malformed query source                              |
|  [05]   | `LookaheadIterator` | iterator             | valid next symbols from a grammar parse state                                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse and tree operations
- rail: parsing
- `Parser(language=None, *, included_ranges=None, logger=None)` binds the grammar once. `parse` is polymorphic on its first argument: a `ByteString` (`parse(source, old_tree=None, encoding='utf8'|'utf16'|'utf16le'|'utf16be')` returns a `Tree`) OR a read-callback `Callable[[int, Point], ByteString | None]` for streaming/rope-backed sources, which additionally accepts a `progress_callback: Callable[[int, bool], bool]` receiving the current byte offset — return `True` to cancel, and a cancelled parse raises `ValueError("Parsing failed")` rather than returning a partial `Tree`. `Parser.included_ranges` restricts parsing to byte ranges (for embedded languages); `logger` receives `(LogType, str)` parse/lex events. `Parser.timeout_micros` is `@deprecated` — bound a parse with the read-callback `progress_callback` instead. `Tree.root_node_with_offset(offset_bytes, offset_extent)` reparents a subtree under a byte/point offset for fragment parsing.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                                   |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `Parser(language, *, included_ranges, logger)`         | build          | parser bound to a grammar, optionally scoped/logged      |
|  [02]   | `Parser.parse(source, old_tree=None, encoding='utf8')` | parse          | bytes -> `Tree`; pass `old_tree` for incremental reparse |
|  [03]   | `Parser.parse(read_callback, …, progress_callback=)`   | parse          | streaming read-callback parse with cancellation          |
|  [04]   | `Parser.included_ranges` / `logger`                    | scope          | byte-range scope; `(LogType, str)` parse/lex events      |
|  [05]   | `Parser.reset()`                                       | reset          | discard partial parse state                              |
|  [06]   | `Tree.root_node` / `root_node_with_offset`             | navigate       | tree root, or a subtree rebased to an offset             |
|  [07]   | `Tree.walk()` / `Tree.language`                        | navigate       | obtain a `TreeCursor`; the producing grammar             |
|  [08]   | `Tree.edit(...)` / `Tree.changed_ranges(new_tree)`     | edit/diff      | apply a byte/point edit, diff vs a new tree              |
|  [09]   | `Tree.included_ranges` / `Tree.copy()`                 | scope          | parsed byte ranges; copy the tree                        |
|  [10]   | `Tree.print_dot_graph(file)`                           | scope          | emit a Graphviz dot graph                                |

[ENTRYPOINT_SCOPE]: node navigation
- rail: parsing
- Prefer field-named and named-child access over positional indexing; every member below is `Node.*`. `Node.text` returns the source slice as `bytes`. `named_descendant_for_byte_range`/`descendant_for_byte_range` locate the tightest node over a span; `child_with_descendant` finds the child containing a given node. Error recovery is queried via `is_error`/`has_error`/`is_missing`/`is_extra`.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :----------------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `children` / `named_children`                                      | navigate       | child enumeration                              |
|  [02]   | `child_by_field_name(name)` / `child_by_field_id(id)`              | navigate       | field-named / field-id child access            |
|  [03]   | `children_by_field_name` / `children_by_field_id`                  | navigate       | all children for a field                       |
|  [04]   | `field_name_for_child(i)` / `field_name_for_named_child(i)`        | navigate       | field name of a positional child               |
|  [05]   | `descendant_for_byte_range(start, end)`                            | navigate       | tightest node over a byte span                 |
|  [06]   | `named_descendant_for_byte_range`                                  | navigate       | tightest named node over a byte span           |
|  [07]   | `descendant_for_point_range(start, end)`                           | navigate       | tightest node over a `Point` span              |
|  [08]   | `named_descendant_for_point_range`                                 | navigate       | tightest named node over a `Point` span        |
|  [09]   | `first_child_for_byte(b)` / `first_named_child_for_byte`           | navigate       | child covering a byte offset                   |
|  [10]   | `child_with_descendant(node)`                                      | navigate       | child containing a descendant                  |
|  [11]   | `parent` / `next_sibling` / `prev_sibling`                         | navigate       | parent and sibling walk                        |
|  [12]   | `next_named_sibling` / `prev_named_sibling`                        | navigate       | named-sibling walk                             |
|  [13]   | `text` / `type` / `grammar_name` / `kind_id` / `grammar_id` / `id` | read           | source `bytes\|None`, type, grammar symbol, id |
|  [14]   | `is_named` / `is_error` / `has_error`                              | read           | named + error flags                            |
|  [15]   | `is_missing` / `is_extra` / `has_changes`                          | read           | recovery + incremental-dirty flags             |
|  [16]   | `parse_state` / `next_parse_state` / `descendant_count`            | read           | parse-state ids, subtree count                 |
|  [17]   | `child_count` / `named_child_count`                                | read           | child counts                                   |
|  [18]   | `start_byte` / `end_byte` / `byte_range`                           | read           | byte span from the node                        |
|  [19]   | `start_point` / `end_point` / `range`                              | read           | `Point` span from the node                     |
|  [20]   | `walk()`                                                           | navigate       | obtain a `TreeCursor` rooted at the node       |

[ENTRYPOINT_SCOPE]: cursor traversal
- rail: parsing
- `TreeCursor` is the allocation-light walk: depth-first via `goto_first_child`/`goto_last_child`/`goto_next_sibling`/`goto_previous_sibling`/`goto_parent`, with `goto_first_child_for_byte`/`goto_first_child_for_point`/`goto_descendant`/`reset`/`reset_to` for targeted descent. The byte/point descent moves return the child index (`int | None`); the sibling/child/parent moves return `bool`. `depth`/`descendant_index`/`field_name`/`field_id` expose position. `copy()` snapshots a cursor for branch exploration.

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :--------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `goto_first_child` / `goto_last_child` / `goto_parent`           | walk           | depth-first moves (`-> bool`)              |
|  [02]   | `goto_next_sibling` / `goto_previous_sibling`                    | walk           | sibling moves (`-> bool`)                  |
|  [03]   | `goto_first_child_for_byte(b)` / `goto_first_child_for_point(p)` | walk           | child covering a position (`-> int\|None`) |
|  [04]   | `goto_descendant(index)` / `reset(node)` / `reset_to(cursor)`    | walk           | jump to descendant index / re-seat cursor  |
|  [05]   | `cursor.node` / `depth` / `descendant_index`                     | read           | current node + depth                       |
|  [06]   | `cursor.field_name` / `field_id`                                 | read           | current field name/id                      |
|  [07]   | `cursor.copy()`                                                  | snapshot       | copy the cursor for branch exploration     |

[ENTRYPOINT_SCOPE]: query operations
- rail: parsing
- Compile a pattern with the `Query(language, source)` constructor (the deprecated `Language.query(...)` shim still exists but is `@deprecated`; never use it). Run it through a `QueryCursor`: `matches(node)` returns `list[tuple[int, dict[str, list[Node]]]]` (pattern index + capture-name -> nodes per match), `captures(node)` returns a `dict[str, list[Node]]` flattening all captures by name. Both accept a `predicate: QueryPredicate` callable (custom predicate handler for non-built-in `#...?` directives) and a `progress_callback: Callable[[int], bool]` receiving the cursor's byte offset — return `True` to cancel, and the run returns its bounded partial results as if complete — `progress_callback` is the modern bound; `QueryCursor.timeout_micros` is `@deprecated`. Scope a run with `set_byte_range`/`set_point_range`/`set_max_start_depth` and cap it with `match_limit`; `did_exceed_match_limit` flags truncation. Built-in predicates (`#eq?`/`#match?`) are evaluated internally; `Query.pattern_settings(i)` returns `#set!` directives and `pattern_assertions(i)` returns `#is?`/`#is-not?` assertions, while `is_pattern_rooted`/`is_pattern_non_local`/`is_pattern_guaranteed_at_step` expose pattern shape and `capture_quantifier(pattern_i, capture_i)` returns the `''`/`'?'`/`'*'`/`'+'` quantifier of a capture.

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY]  | [RAIL]                                        |
| :-----: | :------------------------------------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `Query(language, source)`                                            | build           | compile an S-expression query                 |
|  [02]   | `QueryCursor(query, *, match_limit=0xFFFFFFFF)`                      | build           | execution cursor for a compiled query         |
|  [03]   | `QueryCursor.matches(node, predicate=None, progress_callback=None)`  | match           | `list[(pattern_index, {capture: [Node]})]`    |
|  [04]   | `QueryCursor.captures(node, predicate=None, progress_callback=None)` | match           | `{capture_name: [Node]}` flattened mapping    |
|  [05]   | `QueryCursor.set_byte_range` / `set_point_range`                     | scope           | restrict run to a byte/point range            |
|  [06]   | `QueryCursor.set_max_start_depth`                                    | scope           | cap match start depth                         |
|  [07]   | `QueryCursor.match_limit` / `did_exceed_match_limit`                 | bound           | cap matches, detect truncation                |
|  [08]   | `Query.pattern_count` / `capture_count` / `string_count`             | introspect      | enumerate patterns/captures/strings           |
|  [09]   | `Query.capture_name(i)` / `string_value(i)`                          | introspect      | capture name / literal string by index        |
|  [10]   | `Query.pattern_settings(i)` / `pattern_assertions(i)`                | introspect      | `#set!` / `#is?` directives                   |
|  [11]   | `Query.start_byte_for_pattern(i)` / `end_byte_for_pattern(i)`        | introspect      | pattern source span                           |
|  [12]   | `Query.is_pattern_rooted(i)` / `is_pattern_non_local(i)`             | introspect      | pattern rooted / non-local flags              |
|  [13]   | `Query.is_pattern_guaranteed_at_step(i)` / `capture_quantifier(p,c)` | introspect      | step guarantee / capture quantifier           |
|  [14]   | `Query.disable_capture(name)` / `disable_pattern(i)`                 | tune            | suppress captures/patterns before running     |
|  [15]   | `QueryPredicate` (`Protocol`) / `QueryError`                         | predicate/fault | custom predicate shape; malformed-query raise |
|  [16]   | `Language.lookahead_iterator(state)`                                 | grammar         | `LookaheadIterator` over valid next symbols   |

[ENTRYPOINT_SCOPE]: grammar introspection
- rail: parsing
- `Language` is fully introspectable, not an opaque capsule; every member below is `Language.*` except the `LookaheadIterator.*` row. Symbol ids round-trip through `node_kind_for_id`/`id_for_node_kind(kind, named)`, field ids through `field_name_for_id`/`field_id_for_name`, and the parse-state machine through `next_state(state, id)` + `lookahead_iterator(state)`. `supertypes`/`subtypes(supertype)` expose the grammar's supertype hierarchy (the basis for typing captured nodes against grammar supertypes). Use these to resolve capture/field names to stable ids once and match on the integer thereafter, rather than re-resolving strings per node.

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :---------------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `abi_version` / `semantic_version` / `name`                             | introspect     | compat band, `(maj,min,patch)`, name          |
|  [02]   | `node_kind_count` / `parse_state_count` / `field_count`                 | introspect     | grammar size metrics                          |
|  [03]   | `node_kind_for_id(id)` / `id_for_node_kind(kind, named)`                | introspect     | symbol id <-> name round-trip                 |
|  [04]   | `node_kind_is_named(id)` / `is_visible(id)` / `is_supertype(id)`        | introspect     | symbol classification flags                   |
|  [05]   | `field_name_for_id(id)` / `field_id_for_name(name)`                     | introspect     | field id <-> name round-trip                  |
|  [06]   | `supertypes` / `subtypes(supertype)`                                    | introspect     | grammar supertype hierarchy                   |
|  [07]   | `next_state(state, id)` / `lookahead_iterator(state)`                   | introspect     | parse-state transition / valid next symbols   |
|  [08]   | `LookaheadIterator.names()` / `symbols()`                               | iterate        | enumerate valid next symbols                  |
|  [09]   | `LookaheadIterator.current_symbol_name` / `reset(state, language=None)` | iterate        | current symbol / re-seat iterator             |
|  [10]   | `copy()` / `eq` / `hash`                                                | identity       | copy or key a grammar; `Language` is hashable |

## [04]-[IMPLEMENTATION_LAW]

[PARSING_TOPOLOGY]:
- parser law: a `Parser` is constructed once per grammar and reused; reparsing edited source passes `old_tree` for incremental parse, never a full re-parse where an edit is known.
- traversal law: structural walks use `TreeCursor` (`walk`) for hot paths or field-named access (`child_by_field_name`/`child_by_field_id`); positional `children[i]` index guessing is deleted. Symbol/field name strings are resolved to ids once via `Language.id_for_node_kind`/`field_id_for_name` and matched as integers in hot loops, not re-compared as strings per node.
- query law: structural extraction compiles `Query(language, source)` and runs it through a `QueryCursor` with named captures and range/depth scoping; node-type string matching in a recursion is replaced by the constructor + query. `captures()` returns the flattened capture mapping; `matches()` is used only when the per-match pattern index or full per-match capture grouping is needed.
- bound law: a long parse or query is bounded with a `progress_callback` (return `True` to cancel — a cancelled query yields silently partial captures, so a consumer grading completeness bounds by a pre-gate or `match_limit`, never the cancel hook), never the `@deprecated` `timeout_micros`; cancellation is a callback, not a wall-clock cap.
- edit law: incremental updates call `Tree.edit` with the byte/point delta then reparse with `old_tree`; `changed_ranges(new_tree)` drives downstream re-processing rather than a whole-tree rescan, and `Node.has_changes` flags dirty subtrees.
- range law: positions are byte offsets and `Point` row/columns read from the node (`byte_range`/`range`), never recomputed from the source string; `Node.text` is the source slice (`bytes | None`) rather than a re-slice of the input.
- recovery law: malformed-input handling reads `has_error`/`is_error`/`is_missing` on the tree, not an exception — `parse` always returns a `Tree`. A malformed query SOURCE, by contrast, raises `QueryError` at compile time.

[LOCAL_ADMISSION]:
- the structural-parsing surface composes one reusable `Parser` per grammar; grammar `Language` objects arrive from the grammar packages (`.api/tree-sitter-python.md`, `.api/tree-sitter-typescript.md`) and are constructed once into a keyed grammar registry, never per parse.
- the dense rail stacks as: grammar `Language` (from a grammar package) -> reused `Parser` -> `Tree` -> `Query` compiled ONCE per pattern + `QueryCursor` with `set_byte_range`/`set_max_start_depth` scoping -> `captures()` keyed by capture name. Capture-name and node-kind strings are resolved to ids against the `Language` introspection surface once at registry build, so the hot match loop compares integers, not strings. This is the engine behind the assay `code query` rail: the bundled `HIGHLIGHTS_QUERY`/`TAGS_QUERY` grammar sources and ad hoc S-expression patterns compile through `Query`, run under a `QueryCursor`, and feed the structural-search receipt; the runtime does not re-implement the match loop.
- `Node.text` slices (`bytes`) and `Point` spans map straight onto the structural-search receipt's location facts — the same field-for-field discipline the `RetryDetails`->receipt mapping uses in `.api/stamina.md` — rather than being re-derived from the source buffer.
- this is structural source parsing for the companion seam, never a full language server or type checker.

[RAIL_LAW]:
- Package: `tree-sitter`
- Owns: incremental parsing (bytes or streaming read-callback), grammar `Language` introspection (symbol/field/state ids, supertype hierarchy), syntax-tree traversal with cursors and field access, incremental edits with `changed_ranges`, and structural query matching with capture quantifiers + predicate/assertion introspection
- Reject: per-parse parser construction, full re-parse of edited source, positional child index guessing, node-type string matching where a query fits, recomputing positions from source, the deprecated `Language.query`/`timeout_micros`/`Language.version` accessors
