# [TYPESCRIPT]

TypeScript targets read Effect, their runtime's platform package, and their host's declarations at the release their project links, `<project>` naming the folder holding the target's `package.json`.

## [01]-[SKILLS]

| [INDEX] | [TARGET]         | [SKILLS]                                                 | [CONDITION]                                           |
| :-----: | :--------------- | :------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | Any TypeScript   | `typescript:coding`, `typescript:effect`                 | `.ts` or `.tsx` file                                  |
|  [02]   | Tests            | `typescript:testing`                                     | `*.test.ts`, suite under `tests/`                     |
|  [03]   | Claude Code mod  | `plugin-authoring`, `sazandeh:mod-ui` for drawn UI       | Imports `claude-code`                                 |
|  [04]   | Adobe UXP plugin | `uxp:coding`                                             | Imports `uxp`, `photoshop`, `indesign`, `premierepro` |
|  [05]   | Chrome extension | `chrome:extensions`                                      | `manifest.json` holding `manifest_version`            |
|  [06]   | Browser UI       | `web:css`, `web:react-aria`, `web:web-platform-baseline` | Imports `react-aria-components`, holds `.css`         |

## [02]-[SOURCES]

Package reads, one command per step:
1. Version: Effect release the project links, as the store holds older copies beside it, then its agent guide and docs folder read whole
2. Families: subpath entries (`effect/ai`, `effect/http`, `effect/process`), each listed by step 3 beside `effect`
3. Inventory: `search:code` export listing of an entry, its index of groups with summaries read whole, then each group file naming the job's field
4. Platform: `@effect/platform-<runtime>` the project declares, its layers providing files, processes, sockets, and HTTP, listed by step 3
5. Host: each declaration file the project's tsconfig names, then every sibling file augmenting the same module
6. Node: built-ins and flags of the pinned Node, before any shell-out or text parse in a Node process
7. Workspace: each `@rasm/*` specifier the target imports, then its source file resolved from the project
8. Catalog: central manifest read whole, each candidate package read through step 3 before any judgment of its fit

```bash
jq -r .version <project>/node_modules/effect/package.json && fd -d 1 '^(AGENTS\.md|llms.*\.txt|ai-docs)$' <project>/node_modules/effect
fd -d 1 -t d -E internal . <project>/node_modules/effect/src
node plugins/shared/search/skills/code/scripts/exports.ts <project> <specifier>
jq -r '(.dependencies // {}) + (.devDependencies // {}) | keys[] | select(startswith("@effect/platform-"))' <project>/package.json
jq -c '{files, types: .compilerOptions.types}' <project>/tsconfig*.json && rg -l "^declare module '<module>'" <declarations-folder>
node -p "require('node:module').builtinModules.join('\n')" && node --help
rg -oNI "from '(@rasm/[^']+)'" -r '$1' <target> | sort -u && (cd <project> && node --input-type=module -e "console.log(import.meta.resolve(process.argv[1]))" '<specifier>')
yq '.catalog, .catalogs' pnpm-workspace.yaml
```

## [03]-[CONSUMERS]

Readers and duplicates across `<roots>` (`libs products plugins tools tests`), one command per step:
1. Projects: every project depending on a file the unit changes
2. Importers: files importing a module by package specifier, subpath import (`#/<module>`), or relative path
3. Readers: reference sites of a declaration by name, renamed imports (`import { a as b }`) read at each importer
4. Duplicates by member or literal: sites spelling a member call, key, header, path, or message the declaration holds
5. Duplicates by structure: functions with near-identical trees, names of called members kept

```bash
nx show projects --affected --files=<file>
rg -l "from '[^']*/<module>(\.ts)?'" -g '*.{ts,tsx}' <roots>
rg -nw '<Symbol>' -g '*.{ts,tsx}' <roots>
rg -nF '<member or literal>' -g '*.{ts,tsx}' <roots>
jscpd --config .config/jscpd.json --similarity 0.6 --min-nodes 60 <target> <roots>
```

## [04]-[COUNTS]

Counts per file, one command per step:
1. Lines: per file, and lines added and removed per file of the uncommitted pass
2. Declarations: top-level declarations, class and object members uncounted
3. Exports: names in one-line `export { }` lists (`.oxfmtrc.json` `printWidth`) and inline exports, `export *` counted once
4. Nesting: callbacks past `.oxlintrc.json` `max-nested-callbacks`, generators of `Effect.gen` and `Effect.fn` included, other levels read per member

```bash
wc -l <files> && git diff --numstat -- <paths>
rg -c '^(export )?(declare )?(abstract )?(async )?(const|let|function\*?|class|interface|type|enum|namespace) ' <paths> -g '*.{ts,tsx}'
rg -o '^export (type )?\{[^}]*\}|^export (declare )?(abstract )?(async )?(const|let|function|class|interface|type|enum|namespace) ' <paths> -g '*.{ts,tsx}' | awk -F: '{ n[$1] += /\{/ ? gsub(/,/, "") + 1 : 1 } END { for (f in n) print n[f], f }'
oxlint --format json <paths> | jq -r '.diagnostics[] | select(.code == "eslint(max-nested-callbacks)") | [.filename, .labels[0].span.line, .message] | @tsv'
```

## [05]-[FORMS]

TypeScript forms of each category, each rebuild holding under its stated condition alone:

| [INDEX] | [CATEGORY]   | [FOUND]                                             | [REBUILD]                                                           |
| :-----: | :----------- | :-------------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | Pass-through | Helper whose type parameter ties a key to its value | Helper kept, as inlining loses that link                            |
|  [02]   | Literals     | Name list beside a block of matching calls          | One keyed record of calls, `keyof typeof` that record as the list   |
|  [03]   | Redundant    | Interface beside a schema or reducer of one shape   | `typeof X.Type` or `typeof X.initialValue` under one name           |
|  [04]   | Duplicated   | Hand-written JSON Schema, retry, glob, or diff      | Effect member from the export listing (`Tool`, `Schedule`, `Glob`)  |
|  [05]   | Duplicated   | Spread per optional key restating a map             | `Struct.evolve`, over own enumerable keys alone                     |
|  [06]   | Types        | Service shape typed by hand beside its builder      | `ReturnType<typeof <builder>>`, where the builder states a contract |
|  [07]   | Types        | Derived type inferring one member as `never`        | One stated type on that value, derivation kept                      |
|  [08]   | Nesting      | `if` chain over a discriminant                      | `Match.discriminatorsExhaustive`, `$match`, or `Match.value`        |
|  [09]   | Nesting      | Nested `Option.match` or `Effect.flatMap` callbacks | One `pipe` from innermost subject, each step data-last              |
|  [10]   | Nesting      | Generator at callback limit                         | Curried member, or a statement of an enclosing generator            |
|  [11]   | Names        | Builder a host validator holds apart from its type  | `make<Service>` function, its service shape derived from it         |
|  [12]   | Exports      | Reader re-deriving through `ConstructorParameters`  | Type exported once from its declaring module                        |

## [06]-[VERIFY]

Checks per pass, each command alone in the background, a host project's validator run over its fresh build:

```bash
hk fix <paths> && hk check <paths>
nx affected -t check --uncommitted
nx affected -t test --uncommitted
nx affected -t build --uncommitted
claude plugin validate .artifacts/vite/<plugin> --json
```
