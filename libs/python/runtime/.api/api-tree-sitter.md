# [PY_RUNTIME_API_TREE_SITTER]

`tree-sitter` supplies the incremental parsing runtime: a `Parser` driving a grammar `Language`, the resulting `Tree` and `Node` syntax tree with cursor traversal, byte/point ranges, incremental edits, and a `Query`/`QueryCursor` pattern-match surface. It is the runtime owner for structural source parsing the companion seam consumes.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tree-sitter`
- package: `tree-sitter`
- import: `tree_sitter`
- version: `0.25.2`
- owner: `runtime`
- rail: parsing
- namespaces: `tree_sitter`
- capability: incremental parser, grammar language objects, syntax tree/node traversal, byte/point ranges, incremental edits, query pattern matching

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parser and tree family
- rail: parsing

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `Parser` | parser | incremental parse driver |
| [2] | `Language` | grammar | compiled grammar object |
| [3] | `Tree` | tree | parsed syntax tree |
| [4] | `Node` | node | syntax-tree node |
| [5] | `TreeCursor` | cursor | efficient tree walker |
| [6] | `Range` | range | byte/point span |
| [7] | `Point` | point | row/column position |

[PUBLIC_TYPE_SCOPE]: query family
- rail: parsing

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `Query` | query | compiled S-expression pattern |
| [2] | `QueryCursor` | cursor | query execution cursor |
| [3] | `QueryError` | fault | malformed query |
| [4] | `LookaheadIterator` | iterator | grammar lookahead states |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse and query operations
- rail: parsing

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
| [1] | `Parser(language)` | build | parser bound to a grammar |
| [2] | `Parser.parse` | parse | bytes to a `Tree` |
| [3] | `Parser.parse(old_tree=...)` | parse | incremental reparse |
| [4] | `Tree.root_node` | navigate | tree root node |
| [5] | `Tree.walk` | navigate | obtain a `TreeCursor` |
| [6] | `Tree.edit` | edit | apply a source edit |
| [7] | `Tree.changed_ranges` | diff | ranges differing from old tree |
| [8] | `Node.children` / `Node.named_children` | navigate | child enumeration |
| [9] | `Node.child_by_field_name` | navigate | field-named child |
| [10] | `Node.descendant_for_byte_range` | navigate | node covering a byte span |
| [11] | `Language.query` | build | compile a query |
| [12] | `QueryCursor.captures` | match | capture-name to nodes |
| [13] | `QueryCursor.matches` | match | full pattern matches |

## [4]-[IMPLEMENTATION_LAW]

[PARSING_TOPOLOGY]:
- parser law: a `Parser` is constructed once per grammar and reused; reparsing edited source passes `old_tree` for incremental parse, never a full re-parse where an edit is known.
- grammar law: grammars are admitted as `Language` rows from the `tree-sitter-python`/`tree-sitter-typescript` grammar packages; a new language is one grammar row, never a new parser class.
- traversal law: structural walks use `TreeCursor` (`walk`) for efficiency or field-named access (`child_by_field_name`); index-based child guessing is deleted.
- query law: structural extraction uses a compiled `Query` run through a `QueryCursor` with named captures; manual node-type string matching in a recursion is replaced by a query.
- edit law: incremental updates call `Tree.edit` with the byte/point delta then reparse with `old_tree`; `changed_ranges` drives downstream re-processing.
- range law: positions are byte offsets and `Point` row/columns from the node, never recomputed from the source string.

[LOCAL_ADMISSION]:
- The structural-parsing surface composes one reusable `Parser` per grammar; grammar `Language` objects arrive from the grammar packages (`.api/api-tree-sitter-python.md`, `.api/api-tree-sitter-typescript.md`).
- This is structural source parsing for the companion seam, never a full language server or type checker.

[RAIL_LAW]:
- Package: `tree-sitter`
- Owns: incremental parsing, syntax-tree traversal, incremental edits, and structural query matching
- Accept: reused `Parser`, grammar `Language` rows, `TreeCursor`/field traversal, compiled `Query` captures, incremental `edit`+reparse
- Reject: per-parse parser construction, full re-parse of edited source, index-based child guessing, node-type string matching where a query fits
