# [OUTLINE]

`ast-grep outline` maps declarations and their direct members to source ranges.
Outlines locate what to act on: a declaration to edit, a member to attach a change to, a registration to trace, a rule to read.
Extractors repeating `fd`, `rg`, `jq`, or bundled extractor output are redundant.

## [01]-[READING]

- Repository outlines pass `--items structure`, members need `--view expanded`
- `--match` and `--type` reach items alone, `--match` prints `nothing found` per file without a match
- `--json=compact | grep '^\[{' | jq` over an item's `members` array reaches a member
- `<dir> -l <lang>` walks one language of a directory
- Named dot directories and ignored paths need no `--no-ignore` flag
- `<file> --items imports` lists a file's dependencies, `<dir> --items exports --view signatures` enumerates public entry points
- `<file> --match '^<symbol>$' --view expanded` expands one symbol, `<producer> | ast-grep outline --stdin -l <lang>` outlines piped code
- `<path> --json=stream | grep '^{' | jq -c '<filter>'` post-processes entries

## [02]-[EXTRACTORS]

- `isImport` defaults to `false`, `isExported` and `isPublic` to `true`
- File or stdin runs default to `--items structure --view digest`, a directory run or mixed arguments to `--items exports --view names`
- `names` and `signatures` extract no member, `digest` prints member names with empty signatures, `expanded` evaluates member signatures
- `--match` is case-sensitive Rust regex over item names and signatures and forces signature detail
- `--type` takes a comma list of `symbolType` values
- `--pub-members` drops members with `isPublic` false
- Text view prints the item name in place of an empty signature, `--json` holds `""` for it
- JSON entries hold `role`, `symbolType`, `name`, `range` (`byteOffset`, zero-based `start` and `end`), `signature`, and `astKind`
- Items add `isImport`, `isExported`, and `members` (omitted when empty), a member adds `isPublic`
- Injected regions (a `run:` shell block) merge into the host file's items in host order with host-relative ranges and the host path and language
- Bundled extractors cover rust, typescript, javascript, python, go, kotlin, java, swift, csharp, cpp, c, ruby, and php
- An extractor file holds one document per extractor separated by `---`, documents of several languages load from one file
- Rules load bundled first, then `customLanguages.<name>.outlineRules`, then `--outline-rules` in flag order, the first match on a node wins
- One file loaded through both `outlineRules` and `--outline-rules` registers each extractor once
- `--no-default-outline-rules` fails a member naming a bundled parent with `references unknown parent rule`, tsx members need the bundled set

## [03]-[CONSTRUCTION]

Reuse a bundled extractor selecting the construct.
Choose the item boundary before its name or signature, a declaration matched through its named fields with ancestry restricted to the intended scope.

- One extractor per construct with a predicate flag (`isExported: {inside: {kind: export_statement}}`), a pair of ids duplicates the rule
- Repository items over the wrapping node (`decorated_definition`, `export_statement`) take the declaration from a bundled item
- Traversal reaches a wrapper before the declaration it holds
- Matched items skip their subtree, an inner construct is a member or invisible
- Members attach by containment alone, `parentRuleIds` names every container id sharing the member syntax
- `utils` sit in the extractor's file, outline compilation loads no `utilDirs`
- `replace` chains (strip the body, collapse newlines, trim) build a multi-line header's signature
- Regex groups expand in `by`, missing groups leave their separator, the trim removes it
- `rewriters` joined by `, ` build a construct's signature from its children
- Rewriters bind their own metavariable name, a name the item bound through a shared util refuses another binding
- Literal `name` values name a construct the grammar leaves anonymous, binding nothing
- Signature fallback is the first line, the findable text (`--match '<gate>'`)
- Sibling pairs name an anonymous mapping, a literal name makes every construct one name
- `isPublic` takes `not: {has: {kind: <visibility-kind>, regex: '<private-marker>'}}` where the default is public
- Quoted keys stay quoted where unquoting changes their spelling
- Headers keep generics, constraints, attributes, and heritage clauses

An extractor is refused when the construct has no identifier or an agent reads its text whole:
a table row, a catalog `yq` prints, a `rule` or `fix` member inside a rule document, a workflow `env` or `permissions` block.
TOML has no ast-grep language, a `customLanguages` grammar is a question for the user.

## [04]-[CHECKS]

- Assert exact item and member identities, order, and cardinality before signatures, flags, and ranges, a partial comparison accepts extra entries
- Cardinality is a structural count, `ast-grep scan --inline-rules "$(cat <extractor>)" <path> --json=compact | jq length`
- Fixtures per extractor: a nested declaration of the same syntax, a quoted or flow key, a comment before the construct, a multi-line header
- Fixtures hold Unicode before a declaration and an injected region where the extractor applies to one
- Added members prove under `--view expanded`
- Proof runs `--outline-rules <existing>.yml --outline-rules <new>.yml` against the same command without the new file
- Check each requested view, omitted member signatures in `digest` prove no failed transformation
- Check a replaced default for lost constructs and duplicate entries
