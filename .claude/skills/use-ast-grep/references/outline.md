# [OUTLINE]

`ast-grep outline` locates declarations and direct members by parsed source ranges. Use outlines to select code for reading or editing when no richer language navigator exists. Extractors repeating file discovery, text search, or bundled output add no capability.

## [01]-[READING]

Views select extraction work as well as presentation:
- `<dir> --items structure` includes local items, directory runs otherwise select exports
- `--view names` and `--view signatures` omit member compilation and extraction
- Names view omits signature templates, matcher transforms still execute
- `--view digest` extracts member names, `--view expanded` evaluates member signatures
- `--match` filters item names and signatures with a Rust regex and forces item signature evaluation in names view
- `--match` applies after member collection, filtering a large expanded outline does not avoid member work
- `--type` selects item `symbolType` values, neither `--type` nor `--match` reaches members
- `--json=compact | jq` over an item's `members` array selects a member
- `-l` selects files and forces their parser, enabled catalogs for other languages still compile
- Use inferred languages for outlines with embedded syntax, filter output afterward
- `<file> --items imports` lists dependencies, `<dir> --items exports --view signatures` lists public entry points
- `<producer> | ast-grep outline --stdin -l <lang>` outlines piped code
- `<path> --json=stream | jq -c '<filter>'` processes JSONL output directly
- Files without an applicable extractor print `nothing found` at exit 0

## [02]-[EXTRACTORS]

Extractor documents hold `id`, `language`, `role`, `symbolType`, `rule`, and `name`. Member documents add `parentRuleIds` naming item extractors.

- `role` is `item` or `member`, member parents cannot be other members
- Optional keys include `signature`, import/export/public flags, `constraints`, `utils`, `transform`, and `rewriters`
- `isImport` defaults to `false`, `isExported` and `isPublic` to `true`
- Literal import/export/public flags and `--type` can exclude extractors before compilation
- Predicate flags filter matched nodes after extraction
- File and stdin runs default to `--items structure --view digest`, directory or mixed arguments to `--items exports --view names`
- `--pub-members` excludes members with `isPublic` false
- Empty signatures render as item names in text and `""` in JSON
- JSON entries hold `role`, `symbolType`, `name`, `range`, `signature`, and `astKind`
- Ranges hold `byteOffset` and zero-based `start` and `end` positions
- Items add `isImport`, `isExported`, and optional `members`, members add `isPublic`
- Injected items retain host paths and positions and join host items in source order
- Extractor files separate documents with `---`, languages can share one file
- `--outline-rules` loads extractor catalogs, custom languages can set `outlineRules` in their config
- Project configuration has no root outline-rules key or per-language default suppression
- Loading order is bundled rules, custom-language `outlineRules`, then CLI paths, first matching extractor wins
- Repeated config and CLI paths compile the same extractors again, load a file through one mechanism
- `--no-default-outline-rules` globally disables bundled extractors, complete catalogs cover intended host and injected languages
- Members referencing disabled bundled parents fail with `references unknown parent rule`

## [03]-[CONSTRUCTION]

Match declaration boundaries through grammar fields and attach members to every item parent sharing their syntax. Native extraction traverses items once and indexes member candidates by kind and parent id.

- Bundled matches precede supplements, an identical node predicate cannot replace a bundled signature or visibility flag
- Wrapper items (`decorated_definition`, `export_statement`) can select declarations before bundled inner-node rules
- Matched items search for scoped members, matched members prune their subtrees
- Nested types emitted as members hide deeper declarations, arbitrary-depth hierarchy needs another language navigator
- Parent ids select member rules, unmatched wrappers can expose nested declarations requiring bounded ownership predicates
- Group bindings only when one visibility and ownership describes them all, otherwise emit separate items
- Document-local `utils` share structural predicates, outline compilation loads no global `utilDirs` and YAML anchors stop at `---`
- Capture names identify declaration names, literal names cover constructs with no grammar-provided identifier
- Extractor `name: $NAME` starts fallback signatures at the captured name's source line
- Other name templates fall back to the first nonempty matched source line
- Outline templates use grammar header captures or structural body removal, slicing at captured positions is unavailable
- Preserve multiline source and literal or comment contents, trim only synthetic signature boundaries
- Regex replacement groups expand in `by`, missing groups can leave separators requiring a trim
- Build each binding's signature from its fields, rewriting its full grouped declaration per binding costs quadratic work
- Rewriters bind distinct capture names when an enclosing shared util already binds a capture
- Visibility predicates exclude grammar-defined private forms, `isPublic` otherwise defaults to true
- Headers retain generics, constraints, attributes, heritage, and fields outside omitted bodies
- Quoted names retain delimiters when removing quotes changes their spelling
- Complete replacement catalogs retain required declaration kinds and parents, a custom parser serves syntax gaps alone

Derive coverage from grammar declaration kinds and bundled extractors. Compare names, signatures, ranges, visibility, and parent ownership across wrappers and modern syntax. Constructs read as anonymous rows or keyed blocks need no invented declaration names.

```bash
ast-grep outline --no-default-outline-rules --outline-rules <catalog> --items structure --view expanded <path>
```
