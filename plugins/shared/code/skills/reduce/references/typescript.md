# [TYPESCRIPT]

TypeScript targets hold `.ts`, `.tsx`, `.mts`, `.cts`, `.js`, `.jsx`, and `.mjs` files alike, which `rg -t ts -t js` reads, `<project>` naming the project folder holding `package.json`, `<paths>` files a pass touches, and `<dir>` session scratch folder.

## [01]-[SKILLS]

| [INDEX] | [TARGET]         | [SKILLS]                                                 | [CONDITION]                                              |
| :-----: | :--------------- | :------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | Any TypeScript   | `typescript:coding`, `typescript:effect`                 | `.ts`, `.tsx`, or `.js` file                             |
|  [02]   | Tests            | `typescript:testing`                                     | `*.test.ts`, suite under `tests/`                        |
|  [03]   | Claude Code mod  | `plugin-authoring`, `sazandeh:mod-ui`                    | Imports `@rasm/claude-code/*`, host in tsconfig `files`  |
|  [04]   | Adobe UXP plugin | `uxp:coding`                                             | Imports `adobe:<host>`, `@adobe/*`, types `@adobe-uxp-*` |
|  [05]   | ExtendScript     | `uxp:coding`, `typescript:coding`, `typescript:effect`   | `tsconfig.extendscript.json` project, its callers        |
|  [06]   | Chrome extension | `chrome:extensions`                                      | `manifest.json` holding `manifest_version`               |
|  [07]   | Browser UI       | `web:css`, `web:react-aria`, `web:web-platform-baseline` | Imports `react-aria-components`, holds `.css`            |

ExtendScript files compile under `lib` `es5`, and a form holds there only where `es5` declares its members.

## [02]-[SURVEY]

Signals over `<paths>`, one command per step:
1. Size: lines and branching per file, largest first
2. Clones: jscpd AST pairs across `<roots>` touching the target, ranked by lines times similarity
3. Dependents: projects depending on the target's Nx project, every ecosystem in one graph file
4. Exports: exported names per file, `export *`, `export default`, and wrapped `export { }` lists included
5. Private readers: file-private declarations with one reader in their file, single-caller helpers and constants read once among them
6. Name pairs: declaration and member names differing by case or separator, each group judged, as a type and its value share a name by design
7. Nesting: depth per site past one level, deepest first, generators of `Effect.gen` and `Effect.fn` counted as callbacks

```bash
jscpd --config .config/jscpd.json --complexity --reporters json --output <dir>/jscpd <paths> &&
    jq -r --arg r "$PWD/" '.summary.files[] | [.lines, .complexity, (.path | ltrimstr($r))] | @tsv' <dir>/jscpd/jscpd-complexity.json
jscpd --config .config/jscpd.json --similarity 0.6 --min-nodes 60 --format typescript,tsx,javascript,jsx --reporters json --output <dir>/jscpd <roots>;
    jq -r --arg t "$PWD/<target>/" '[.duplicates[] | select((.firstFile.name | startswith($t)) or (.secondFile.name | startswith($t)))] | sort_by(-(.lines * .similarity))[]
    | "\(.lines)\t\(.similarity)\t\(.firstFile.name):\(.firstFile.start)\t\(.secondFile.name):\(.secondFile.start)"' <dir>/jscpd/jscpd-report.json
nx graph --file=<dir>/graph.json &&
    jq -r --arg p '<nx-project>' '.graph.dependencies | to_entries[] | select(any(.value[]; .target == $p)) | .key' <dir>/graph.json
ast-grep outline <paths> --items exports --json=stream | jq -r '"\(.items | length)\t\(.path)"'
comm -12 <(ast-grep outline <paths> --items structure --json=stream | jq -r '.path as $f | .items[] | select(.symbolType != "namespace" and (.isExported | not)) | "\($f)|\(.name)"' | sort -u) \
    <(rg -How --no-heading '[A-Za-z_$][\w$]*' <paths> | sed 's/:/|/' | sort | uniq -c | awk '$1 == 2 { print $2 }' | sort -u)
ast-grep outline <paths> --items structure --view expanded --json=stream | jq -r '.items[] | select(.symbolType != "namespace") | .name, (.members[]?.name)' | sort -u |
    awk '{ k = tolower($0); gsub(/[-_]/, "", k); g[k] = g[k] " " $0; n[k]++ } END { for (k in g) if (n[k] > 1) print g[k] }'
oxlint -c <dir>/nesting.json --format json <paths> |
    jq -r '.diagnostics[] | select(.code | test("max-(nested-callbacks|depth)")) | [(.message | capture("\\((?<n>[0-9]+)\\)").n), .filename, .labels[0].span.line, .code] | @tsv' | sort -rn
```

Commands exit by their own rules:
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Nesting: `nesting.json` holds `{"rules": {"max-nested-callbacks": ["error", {"max": 1}], "max-depth": ["error", {"max": 1}]}}`, exit 1 on a site
- Limits: `.oxlintrc.json` sets `max-nested-callbacks` to 3 and leaves `max-depth` at its default of 4, and committed code reports no site past them
- Rules: callbacks and blocks count apart, a callback inside a block printing a row for each

## [03]-[SOURCES]

Package reads, one command per step:
1. Guide: Effect release the project resolves, its `AGENTS.md` and `ai-docs/` files it links read whole
2. Index: export listing of a package from a project declaring it, a package name adding its other entry points (`effect/ai`), groups as `.tsv` files
3. Runtime: `files`, `types`, and `lib` of the resolved tsconfig, choosing platform layers `typescript:effect` lists per host
4. Node: built-ins and flags of the pinned Node, for code a Node process runs alone, never a build-time Node beside another host
5. Host: each declaration file step 3 prints and every file augmenting its module, outlined once into the scratch folder and read at member lines
6. Workspace: each `@rasm/*` specifier the target imports, resolved from its project to a source file read whole
7. Catalog: each pnpm catalog candidate through step 2 from a project declaring it, else from `search:code`'s catalog tarball layout

```bash
jq -r .version <project>/node_modules/effect/package.json && fd -d 1 '^(AGENTS\.md|llms.*\.txt|ai-docs)$' <project>/node_modules/effect
node plugins/shared/search/skills/code/scripts/exports.ts <project> <specifier>
tsc --showConfig -p <project>/tsconfig.json | jq -c '{files: [.files[]? | select(endswith(".d.ts"))], types: .compilerOptions.types, lib: .compilerOptions.lib}'
node -p "require('node:module').builtinModules.join('\n')" && node --help
rg -l --no-ignore "^declare module '" <declaration-folder> && ast-grep outline <declaration-file> --items structure --view expanded > <dir>/host-<module>.txt
rg -oNI "(?:from |import\()'(@rasm/[^']+)'" -r '$1' -t ts -t js <target> | sort -u |
    (cd <project> && xargs node --input-type=module -e "import { fileURLToPath } from 'node:url'; for (const s of process.argv.slice(1)) console.log(s, fileURLToPath(import.meta.resolve(s)))")
```

## [04]-[CONSUMERS]

Readers and leads across `<roots>`, one command per step:
1. Readers: reader sites by compiler identity across every tracked tsconfig, `.tsx`, `.js`, and cross-project readers included
2. Importers: tree per export through package specifiers, `#/` subpaths, and renamed imports, with the members each importer reads
3. Text: sites spelling a member call, key, header, path, or message the declaration holds
4. Mentions: removed or renamed name across the repository with no type filter, docs, skills, and manifests included
5. Leads: exports nothing reads, an entry file a host loads by path (`executor.ts`, `server/main.ts`) read as its host's reader first

```bash
node plugins/shared/code/skills/reduce/scripts/readers.ts <file> <symbol>
knip --config .config/knip.json --trace-file <file> --no-progress --no-config-hints
rg -nF '<member or literal>' -t ts -t js <roots>
rg -nw '<name>' .
knip --config .config/knip.json --workspace <project> --include-entry-exports --exports --reporter json --no-progress --no-config-hints |
    jq -r '.issues[] | select((.exports + .types) | length > 0) | "\(.file)\t\([(.exports + .types)[].name] | join(","))"'
```

## [05]-[FORMS]

TypeScript forms of each category, each rebuild holding under its stated condition alone:

| [INDEX] | [CATEGORY] | [FOUND]                                                    | [REBUILD]                                                      |
| :-----: | :--------- | :--------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | Redundant  | Interface beside schema or reducer of one shape            | `typeof X.Type`, or type of builder's returned value, one name |
|  [02]   | Duplicated | Hand-written JSON Schema, retry, glob, or diff             | Effect member from export listing (`Tool`, `Schedule`, `Glob`) |
|  [03]   | Duplicated | Spread per optional key restating a map                    | `Struct.evolve`, over own enumerable keys alone                |
|  [04]   | Types      | Service shape typed by hand beside its builder             | `ReturnType<typeof <builder>>`, where builder states contract  |
|  [05]   | Types      | Derived type inferring one member as `never`               | One stated type on its value, derivation kept                  |
|  [06]   | Literals   | Name list beside block of matching calls                   | One keyed record of calls, `keyof typeof` record as list       |
|  [07]   | Nesting    | `if` chain over discriminant                               | `Match.discriminatorsExhaustive`, `$match`, or `Match.value`   |
|  [08]   | Nesting    | Nested `Option.match` or `Effect.flatMap` over one subject | One `pipe` from innermost subject, each step data-last         |
|  [09]   | Nesting    | Generator at callback limit                                | Curried member, or statement of enclosing generator            |
|  [10]   | Names      | Builder a host validator holds apart from its type         | `make<Service>` function, service shape derived from it        |
|  [11]   | Exports    | Reader re-deriving through `ConstructorParameters`         | Type exported once from declaring module                       |

## [06]-[CHECK]

Check per pass, each command its own background call, `--files` taking `<paths>` joined by commas:

```bash
hk fix <paths>
nx affected -t check --files=<paths> --exclude=rasm
nx run <plugin>:build
claude plugin validate .artifacts/vite/<plugin> --json
```

- Scope: root project `rasm` runs repository-wide knip and jscpd, which fail on other work in a shared tree, and a pass excludes it
- Manifest: `<paths>` holding `package.json` or `pnpm-lock.yaml` -> same `nx affected` line without `--exclude=rasm`
- Plugin: Claude Code plug-in in `<paths>` -> validator over its fresh build, `<plugin>` as `nx show projects` prints it (`@rasm/function-hooks`)
- Other host -> validator its host skill names (`chrome:extensions`, `uxp:coding`)
