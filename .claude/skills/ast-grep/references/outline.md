# [OUTLINE]

`ast-grep outline` maps declarations and their direct members to source ranges. Items include imports, functions, classes, structs, interfaces, modules, and enums. Members include fields, methods, constructors, and variants. Import, export, and visibility flags describe syntax, while language tooling resolves symbols, types, re-exports, and callers.

## [01]-[READING]

Use `nx run rasm:outline -- <paths> --items structure` for repository maps. The target adds repository extractors to the bundled language extractors and loads the XML grammar. Native options pass through unchanged. Use the CLI commands directly for isolated extractors or outside the repository.

Resolve target paths from the task, search hits, or `git diff --name-only`, run the row for the task, narrow the located symbol with `--match <symbol> --view expanded`, then `Read` only the printed line range.

| [INDEX] | [TASK] | [COMMAND] |
| :-----: | :----- | :----- |
| [01] | Map a directory | `ast-grep outline <dir>` — grouped exported names, `--type <t1>,<t2>` narrows |
| [02] | Understand a file before editing | `ast-grep outline <file>` — local structure with member digests |
| [03] | List a file's dependencies | `ast-grep outline <file> --items imports` |
| [04] | Find importers of a module | `ast-grep outline <dir> --items imports --match <module> --view signatures` |
| [05] | Enumerate public entry points | `ast-grep outline <dir> --items exports --view signatures` |
| [06] | Zoom into one symbol | `ast-grep outline <file> --match <symbol> --type <type> --view expanded` |
| [07] | Map structure after edits | `ast-grep outline <changed-paths> --items structure` |
| [08] | Outline piped code | `<producer> \| ast-grep outline --stdin -l <lang>` |
| [09] | Post-process entries | `ast-grep outline <path> --json=stream` — one file object per line, jq pipelines |

## [02]-[SIMPLIFICATION]

For a simplification review, map all module declarations with `--items structure`, then expand the owning operations and their imports.
Record the declarations that each correction can remove together, including callers and representations in other files. Moving a helper
outside the selected range does not reduce the operation's complexity.

Classify nested structure before selecting a correction:

- Execution scopes identify callbacks and function bodies, then resolve which package invokes or stores each callback
- Control scopes identify conditionals, loops, and exception regions, retaining alternative dispatch and resource cleanup as one operation
- Data-first calls identify an operation consuming another operation, then resolve the available overloads and evaluation order
- Data and schema constructors describe a value's required shape, with no execution-depth violation established by their braces

Use structural matches over the language's node kinds to locate each category. Report the owning declaration and nested range
for a candidate, then inspect its inputs, consumers, and contract. Outlines locate declarations, but do not compute execution depth or
prove that a schema, wrapper, or type adds no meaning. Use the registered depth rules for their stated categories and reviewed queries
for the remaining candidates. Keep the language's function boundaries and alternate dispatch semantics when adapting the search.
For TypeScript, `no-fourth-nesting-level` locates execution and control depth, and `no-nested-combinator-calls` locates data-first call depth.
Search `call_expression` nodes for imported schema constructors and `type_alias_declaration` nodes for repeated representations, then trace their consumers.

## [03]-[EXTRACTORS]

Bash function declarations need an explicit extractor, including inside injected shell regions:

```bash
ast-grep outline <path> --outline-rules tools/ast-grep/outline/bash-function.yml --items structure
```

`--items structure` includes functions in directory scans, which otherwise select exported items. Mixed file and directory arguments use the directory defaults for the whole invocation. Function declarations do not establish `export -f`.

- `--items structure\|exports\|imports\|all` selects top-level entries, default `structure` for a file or stdin and `exports` for a directory
- `--view names\|signatures\|digest\|expanded` sets detail ascending, default `digest` for a file or stdin and `names` for a directory
- `--match` is case-sensitive Rust regex over item names, signatures, and first source lines, `--type` filters symbol types, neither reaches members
- `--pub-members` hides private members, a member without extractable visibility counts as public
- JSON entries carry `symbolType`, `role`, zero-based `range` with byte offsets, `signature`, `astKind`, and import/export/public flags
- `customLanguages.<name>.outlineRules` registers one extractor file, repeated `--outline-rules <file>` arguments add independent extractors
- Use `--no-default-outline-rules` when replacing bundled extractors, preserving defaults when adding an uncovered construct
- Injected-language items merge into host source order with host-relative ranges
- Extractor transforms and rewriters derive names and signatures through the same rule engine
- `digest` includes member names with empty signatures, while `expanded` evaluates member signatures and their transforms

XML/MSBuild targets use the configured extractor. Add direct task members and imports when the task needs them:

```bash
ast-grep outline <path> \
  --outline-rules tools/ast-grep/outline/msbuild-task.yml \
  --outline-rules tools/ast-grep/outline/msbuild-import.yml \
  --items all --view expanded
```

JSON property extractors show top-level keys with their direct object members:

```bash
ast-grep outline <path> \
  --outline-rules tools/ast-grep/outline/json-property.yml \
  --outline-rules tools/ast-grep/outline/json-property-member.yml \
  --items all --view expanded
```

The repository target maps MSBuild evaluation groups through `msbuild-group`, with direct properties and item operations through
`msbuild-property` and `msbuild-item`. Group signatures retain labels and conditions, property signatures retain their assignments, and item
signatures retain `Include`, `Update`, or `Remove` with their attributes. Groups under the project or its `Choose` branches are included. Target-local groups, `ProjectExtensions`, property XML, and item metadata
remain inside their owning source ranges. Use `--match <group-label> --view expanded` to read a group. The map reports declarations without evaluating conditions or imports.

`msbuild-using-task` retains task factory, assembly, and condition attributes. Its direct `msbuild-task-parameter` members retain parameter types and required/output flags in source order without resolving the assembly or evaluating the condition.

TypeScript object and imported `Schema.Struct` initializers add their direct fields to bundled variable items. Member signatures retain the
written field expressions and computed keys without evaluating them. Spreads and nested object fields stay in the owning expression.
Parentheses, `as`, and `satisfies` preserve the declaration boundary. Import matching identifies written API references without
inferring the schema's resulting runtime type.

Python type aliases use the repository's `python-type-alias` extractor. Names preserve their identifiers, and signatures retain generic
parameters, defaults, and the assigned type expression. Conditional module declarations remain visible, while class and function locals stay
inside their owning source ranges. The `struct` category matches bundled TypeScript aliases. Export flags follow bundled Python declarations
without resolving `__all__` or runtime branch conditions.

Native Markdown outlines include ATX and Setext headings, including headings inside quotes and lists, while excluding code blocks.
Names preserve inline source spelling and multiline titles. Ranges cover the heading, including a Setext underline, and signatures show its
first source line. Read the following source for section contents. Use bundled extractors when they already supply the requested map.

GitHub YAML outlines use `github-job`, `github-composite`, and `github-step` through the repository target. Jobs own direct steps and nested workflow `parallel` descendants; composite `runs` owns direct steps. Members are named by `name`, then `id`, then the written operation. Expanded signatures preserve `run`, `uses`, `wait`, `wait-all`, and `cancel` values. Nested input data stays outside the map, and aliases are not expanded into additional declarations. Use `nx run rasm:outline -- .github --items structure --view expanded` to trace workflow calls.

## [04]-[CONSTRUCTION]

Keep each item or member extractor in its own file. Reuse a bundled extractor when it already describes the intended construct. Add extractors for missing constructs and replace defaults only when their selection or output conflicts with the requested map.

Declare outline utilities in the extractor's `utils` map. Outline compilation does not load utilities registered through `utilDirs`.

Choose the item boundary before its name or signature. Match the declaration through its named fields and restrict ancestry to the intended scope. Match members against the declaration selected by the parent extractor, including its scope, and reference its loaded extractor id through `parentRuleIds`. Distinguish direct members from nested declarations with a structural relation, then verify the engine's attachment with a nested fixture.

Derive `name` from the declaration's identifier or key. Preserve quoted keys when unquoting changes their spelling or ambiguity. Derive signatures from the declaration header through captures and rewriters, preserving generics, constraints, attributes, and heritage clauses. Remove the body with the captured declaration as its parent and no other. Headers can contain braces in type expressions, nested class bodies in heritage clauses, or function bodies in parameter defaults. Match the captured parent to distinguish equal body text at different locations.

Set `isImport`, `isExported`, and `isPublic` from the language's declaration form. Separate export status from runtime availability and member visibility from an absent access modifier. Select the language's default visibility when a modifier is optional.

## [05]-[CHECKS]

Assert exact item and member identities, order, and cardinality before checking selected signatures, flags, and source ranges. Partial metadata comparisons alone can admit extra entries. Include nested declarations, alternate declaration forms, Unicode before a declaration, and injected regions when the extractor applies to them. Check the requested views independently: omitted member signatures in `digest` do not establish a failed transformation.

Test each extractor in isolation and with the repository additions. When replacing defaults, check for lost constructs and duplicate entries. Run the durable outline tests through the TypeScript rule target after focused cases pass.
