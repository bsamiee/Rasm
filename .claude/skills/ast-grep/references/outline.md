# [OUTLINE]

`ast-grep outline` maps declarations and their direct members to source ranges. Outlines locate what to act on: a declaration to edit, a member to attach a change to, a registration to trace, a rule to read. Extractors repeating `fd`, `rg`, `jq`, or bundled extractor output are redundant. Import, export, and visibility flags describe syntax.

## [01]-[READING]

`pnpm exec nx run rasm:outline -- <arguments>` runs each table row. `rasm:outline` loads every extractor under `tools/ast-grep/outline/*.yml` at run time beside the bundled extractors and the XML grammar, a new file needs no registration. `--color` fails the target schema, a piped run prints no color. Missing paths print `nothing found` under a target reporting success.

- Repository outlines pass `--items structure`, members need `--view expanded`
- `--match` and `--type` reach items alone, `--match` prints `nothing found` per file without a match
- `--json=compact | grep '^\[{' | jq` over an item's `members` array reaches a member
- `ast-grep outline <paths> --outline-rules tools/ast-grep/outline/<id>.yml` runs one extractor in isolation and serves a tree outside the repository
- `<dir> -l <lang>` walks one language of a directory
- Named dot directories and ignored paths need no `--no-ignore` flag
- Resolve target paths from the task, search matches, or `git diff --name-only`, run the row for the task, and `Read` the printed line range alone

| [INDEX] | [TASK]                                           | [ARGUMENTS]                                                                       |
| :-----: | :----------------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | List the repository extractors                   | `tools/ast-grep/outline --items structure`                                        |
|  [02]   | Read rules with their local utils                | `tools/ast-grep/rules tools/ast-grep/utils --items structure --view expanded`     |
|  [03]   | Read a rule's test cases                         | `tools/ast-grep/tests --items structure --view expanded`                          |
|  [04]   | Pair a rule with its test                        | `tools/ast-grep/rules tools/ast-grep/tests --items structure --match '^<id>$'`    |
|  [05]   | Trace workflows and composite actions            | `.github --items structure --view expanded`                                       |
|  [06]   | Find an Nx target and its command                | `package.json nx.json $(fd -g project.json .) --items structure --view expanded`  |
|  [07]   | Map the function-hooks plugin                    | `.claude/plugins/function-hooks/hooks --items structure --view expanded`          |
|  [08]   | Find a `claude-code.d.ts` declaration            | `.claude/types/claude-code.d.ts --items structure --view expanded --json=compact` |
|  [09]   | Find an MSBuild target, group, property, or item | `$(fd -e csproj -e props -e targets .) --items structure --view expanded`         |
|  [10]   | Find a Python type alias                         | `eng/scripts --items structure --type struct --view signatures`                   |
|  [11]   | Open one agent section                           | `.claude/agents --items structure --match '<gate>'`                               |
|  [12]   | List a file's dependencies                       | `<file> --items imports`                                                          |
|  [13]   | Enumerate public entry points                    | `<dir> --items exports --view signatures`                                         |
|  [14]   | Expand one symbol                                | `<file> --match '^<symbol>$' --view expanded`                                     |
|  [15]   | Outline piped code                               | `<producer> \| ast-grep outline --stdin -l <lang>`, the CLI alone                 |
|  [17]   | Post-process entries                             | `<path> --json=stream \| grep '^{' \| jq -c '<filter>'`                           |

For a simplification review, outline module declarations with `--items structure`, expand the owning operations and their imports, and record the declarations each correction removes with their callers and representations in other files. Outlines locate ranges and compute no execution depth.

## [02]-[EXTRACTORS]

Files under `tools/ast-grep/outline/` hold one extractor each. Items have no parent, the flag column shows where a default misleads `--items` or `--pub-members`:

| [INDEX] | [ID]                           | [LANGUAGE] | [PARENT]                                           | [FLAG]                               |
| :-----: | :----------------------------- | :--------- | :------------------------------------------------- | :----------------------------------- |
|  [01]   | `ast-grep-case`                | yaml       | `ast-grep-test`                                    | `isPublic` under the `valid` key     |
|  [02]   | `ast-grep-rule`                | yaml       |                                                    |                                      |
|  [03]   | `ast-grep-test`                | yaml       |                                                    |                                      |
|  [04]   | `ast-grep-util`                | yaml       | `ast-grep-rule`                                    | `isPublic: false`                    |
|  [05]   | `bash-function`                | bash       |                                                    | `isExported: false`                  |
|  [06]   | `github-composite`             | yaml       |                                                    |                                      |
|  [07]   | `github-job`                   | yaml       |                                                    |                                      |
|  [08]   | `github-step`                  | yaml       | `github-job` and `github-composite`                |                                      |
|  [09]   | `github-workflow`              | yaml       |                                                    |                                      |
|  [10]   | `msbuild-group`                | xml        |                                                    | `isExported: false`                  |
|  [11]   | `msbuild-item`                 | xml        | `msbuild-group`                                    |                                      |
|  [12]   | `msbuild-property`             | xml        | `msbuild-group`                                    |                                      |
|  [13]   | `msbuild-target-group`         | xml        | `msbuild-target`                                   |                                      |
|  [14]   | `msbuild-target`               | xml        |                                                    |                                      |
|  [15]   | `msbuild-task`                 | xml        | `msbuild-target`                                   |                                      |
|  [16]   | `nx-target-configuration`      | json       | `nx-target`                                        |                                      |
|  [17]   | `nx-target`                    | json       |                                                    |                                      |
|  [18]   | `python-type-alias`            | Python     |                                                    |                                      |
|  [19]   | `typescript-ambient-member`    | tsx        | `tsx-ambient-module`, `tsx-export-ambient-module`  | `isPublic` inside `export_statement` |
|  [20]   | `typescript-hook-registration` | tsx        | `tsx-top-level-arrow-function`, `tsx-export-const` |                                      |
|  [21]   | `typescript-object-member`     | tsx        | bundled `tsx-*` const and let items                |                                      |
|  [22]   | `typescript-test-case`         | tsx        | `typescript-test`                                  |                                      |
|  [23]   | `typescript-test`              | tsx        |                                                    | `isExported: false`                  |

- `isImport` defaults to `false`, `isExported` and `isPublic` to `true`
- File or stdin runs default to `--items structure --view digest`, a directory run or mixed arguments to `--items exports --view names`
- `names` and `signatures` extract no member, `digest` prints member names with empty signatures, and `expanded` evaluates member signatures
- `--match` is case-sensitive Rust regex over item names and signatures and forces signature detail
- `--type` takes a comma list of `symbolType` values
- `--pub-members` drops members with `isPublic` false
- Text view prints the item name in place of an empty signature, `--json` holds `""` for it
- JSON entries hold `role`, `symbolType`, `name`, `range` (`byteOffset`, zero-based `start` and `end`), `signature`, and `astKind`
- Items add `isImport`, `isExported`, and `members` (omitted when empty), and a member adds `isPublic`
- Injected regions (a `run:` shell block) merge into the host file's items in host order with host-relative ranges and the host path and language
- Bundled extractors cover rust, typescript, javascript, python, go, kotlin, java, swift, csharp, cpp, c, ruby, and php
- Rules load bundled first, then `customLanguages.<name>.outlineRules`, then `--outline-rules` in flag order, the first match on a node wins
- `--no-default-outline-rules` fails a member naming a bundled parent with `references unknown parent rule`, tsx members need the bundled set
- `nx-target` accepts an `object` or `array` value, `targetDefaults` in `nx.json` holds an array of objects and `package.json` one object
- `github-step` and `github-job` strip a block scalar indicator line, name a `run: |` step by its first command, and read `if: |` as `if: always()`
- `msbuild-group` and `msbuild-target` require the `Project` root, `NuGet.config` and `Workspace.slnx` map to no item

## [03]-[CONSTRUCTION]

Reuse a bundled extractor selecting the construct. Choose the item boundary before its name or signature, a declaration matched through its named fields with ancestry restricted to the intended scope.

- One extractor per construct with a predicate flag (`isExported: {inside: {kind: export_statement}}`), a pair of ids duplicates the rule
- Repository items over the wrapping node (`decorated_definition`, `export_statement`) take the declaration from a bundled item
- Traversal reaches a wrapper before the declaration it holds
- Matched items skip their subtree, an inner construct is a member or invisible (`typescript-test-case` under `typescript-test`)
- Members attach by containment alone, `parentRuleIds` names every container id sharing the member syntax
- `utils` sit in the extractor's file, outline compilation loads no `utilDirs`
- `replace` chains (strip the body, collapse newlines, trim) build a multi-line header's signature
- Regex groups expand in `by` (`msbuild-group` `$1 $2`)
- Missing groups leave their separator, the trim removes it
- `rewriters` joined by `, ` build a construct's signature from its children (`ast-grep-rule`, `github-job`, `nx-target`)
- Rewriters bind their own metavariable name, a name the item bound through a shared util refuses another binding
- Literal `name` values name a construct the grammar leaves anonymous, binding nothing
- Signature fallback is the first line, the findable text (`--match '<gate>'`)
- Sibling pairs name an anonymous mapping (`github-composite` reads the document `name`), a literal name makes every action one name
- `isPublic` takes `not: {has: {kind: <visibility-kind>, regex: '<private-marker>'}}` where the default is public, an absent modifier reads public
- Quoted keys stay quoted where unquoting changes their spelling
- Headers keep generics, constraints, attributes, and heritage clauses

Extractors refused:
- Policy table rows: a row has no identifier, the bundled const item's range locates the table
- `NuGet.config` and `Workspace.slnx` declarations: read whole, `rg -n 'Path=|key='` prints them
- Python cyclopts commands: `rg -n '^@_app'` prints them, the bundled function item holds the signature
- `pnpm-workspace.yaml` catalog: `yq '.catalog'` prints it
- Workflow `env`, `permissions`, `concurrency`, `defaults`: inside the workflow item's file, `rg` prints them
- Rule document `rule`, `fix`, `message` members: the item range locates them
- TOML (`pyproject.toml`, `mise.toml`): no ast-grep language, a `customLanguages` grammar is a question for the user

## [04]-[CHECKS]

- Assert exact item and member identities, order, and cardinality before signatures, flags, and ranges, a partial comparison accepts extra entries
- Cardinality is a structural count, `ast-grep scan --inline-rules "$(cat tools/ast-grep/outline/<id>.yml)" <path> --json=compact | jq length`
- `rg` indent counts count a nested key of the same indentation and prove no cardinality
- Fixtures per extractor: a nested declaration of the same syntax, a quoted or flow key, a comment before the construct, a multi-line header
- Fixtures hold Unicode before a declaration and an injected region where the extractor applies to one
- Added members prove under `--view expanded`
- Proof runs `--outline-rules <existing>.yml --outline-rules <new>.yml` against the same command without the new file
- Check each requested view, omitted member signatures in `digest` prove no failed transformation
- Check a replaced default for lost constructs and duplicate entries
- Outline files pass `ast-grep scan --report-style short tools/ast-grep/outline` with no line
- File order varies between runs of one directory
- Outlines compare through `difft --display inline <before.json> <after.json>` over `jq 'sort_by(.path)'` output
