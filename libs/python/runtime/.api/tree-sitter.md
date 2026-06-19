# [PY_RUNTIME_API_TREE_SITTER]

`tree-sitter` supplies the incremental parsing runtime: a `Parser` driving a grammar `Language`, the resulting `Tree` and `Node` syntax tree with cursor traversal, byte/point ranges, incremental edits, and a `Query`/`QueryCursor` pattern-match surface. It is the runtime owner for structural source parsing the companion seam consumes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `tree-sitter`
- package: `tree-sitter`
- import: `tree_sitter`
- owner: `runtime`
- rail: parsing
- namespaces: `tree_sitter`
- capability: incremental parser, grammar language objects, syntax tree/node traversal, byte/point ranges, incremental edits, query pattern matching

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parser and tree family
- rail: parsing

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [RAIL]                   |
| :-----: | :----------- | :------------ | :----------------------- |
|  [01]   | `Parser`     | parser        | incremental parse driver |
|  [02]   | `Language`   | grammar       | compiled grammar object  |
|  [03]   | `Tree`       | tree          | parsed syntax tree       |
|  [04]   | `Node`       | node          | syntax-tree node         |
|  [05]   | `TreeCursor` | cursor        | efficient tree walker    |
|  [06]   | `Range`      | range         | byte/point span          |
|  [07]   | `Point`      | point         | row/column position      |

[PUBLIC_TYPE_SCOPE]: query family
- rail: parsing

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :------------------ | :------------ | :---------------------------- |
|  [01]   | `Query`             | query         | compiled S-expression pattern |
|  [02]   | `QueryCursor`       | cursor        | query execution cursor        |
|  [03]   | `QueryError`        | fault         | malformed query               |
|  [04]   | `LookaheadIterator` | iterator      | grammar lookahead states      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: parse and query operations
- rail: parsing

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :-------------------------------------- | :------------- | :----------------------------- |
|  [01]   | `Parser(language)`                      | build          | parser bound to a grammar      |
|  [02]   | `Parser.parse`                          | parse          | bytes to a `Tree`              |
|  [03]   | `Parser.parse(old_tree=...)`            | parse          | incremental reparse            |
|  [04]   | `Tree.root_node`                        | navigate       | tree root node                 |
|  [05]   | `Tree.walk`                             | navigate       | obtain a `TreeCursor`          |
|  [06]   | `Tree.edit`                             | edit           | apply a source edit            |
|  [07]   | `Tree.changed_ranges`                   | diff           | ranges differing from old tree |
|  [08]   | `Node.children` / `Node.named_children` | navigate       | child enumeration              |
|  [09]   | `Node.child_by_field_name`              | navigate       | field-named child              |
|  [10]   | `Node.descendant_for_byte_range`        | navigate       | node covering a byte span      |
|  [11]   | `Query(language, pattern)`              | build          | compile a query                |
|  [12]   | `QueryCursor.captures`                  | match          | capture-name to nodes          |
|  [13]   | `QueryCursor.matches`                   | match          | full pattern matches           |

## [04]-[IMPLEMENTATION_LAW]

[PARSING_TOPOLOGY]:
- parser law: a `Parser` is constructed once per grammar and reused; reparsing edited source passes `old_tree` for incremental parse, never a full re-parse where an edit is known.
- grammar law: grammars are admitted as `Language` rows from the `tree-sitter-python`/`tree-sitter-typescript` grammar packages; a new language is one grammar row, never a new parser class.
- traversal law: structural walks use `TreeCursor` (`walk`) for efficiency or field-named access (`child_by_field_name`); index-based child guessing is deleted.
- query law: structural extraction compiles a `Query(language, pattern)` and runs it through a `QueryCursor` with named captures; the deprecated `Language.query(pattern)` shim and manual node-type string matching in a recursion are replaced by the constructor + query.
- edit law: incremental updates call `Tree.edit` with the byte/point delta then reparse with `old_tree`; `changed_ranges` drives downstream re-processing.
- range law: positions are byte offsets and `Point` row/columns from the node, never recomputed from the source string.

[LOCAL_ADMISSION]:
- The structural-parsing surface composes one reusable `Parser` per grammar; grammar `Language` objects arrive from the grammar packages (`.api/tree-sitter-python.md`, `.api/tree-sitter-typescript.md`).
- This is structural source parsing for the companion seam, never a full language server or type checker.

[RAIL_LAW]:
- Package: `tree-sitter`
- Owns: incremental parsing, syntax-tree traversal, incremental edits, and structural query matching
- Accept: reused `Parser`, grammar `Language` rows, `TreeCursor`/field traversal, compiled `Query` captures, incremental `edit`+reparse
- Reject: per-parse parser construction, full re-parse of edited source, index-based child guessing, node-type string matching where a query fits
