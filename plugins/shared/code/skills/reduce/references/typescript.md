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

## [02]-[SOURCES]

TypeScript targets read Effect, their runtime's platform package, and their host's declarations, as most hand-rolled code in this tree restates one of them. Package reads, one command per step:
1. Guide: Effect release the project resolves, its `AGENTS.md` and `ai-docs/` files it links read whole
2. Index: export listing of a package from a project declaring it, a package name adding its other entry points (`effect/ai`), groups as `.tsv` files
3. Runtime: `files`, `types`, and `lib` of the resolved tsconfig, choosing platform layers `typescript:effect` lists per host
4. Node: built-ins and flags of the pinned Node, for code a Node process runs alone, never a build-time Node beside another host
5. Host: each declaration file step 3 prints and every file augmenting its module, read at the members the target calls or restates
6. Workspace: each `@rasm/*` specifier the target imports, resolved from its project to a source file read whole
7. Catalog: each pnpm catalog candidate through step 2 from a project declaring it, else from `search:code`'s catalog tarball layout

```bash
jq -r .version <project>/node_modules/effect/package.json && fd -d 1 '^(AGENTS\.md|llms.*\.txt|ai-docs)$' <project>/node_modules/effect
node plugins/shared/search/skills/code/scripts/exports.ts <project> <specifier>
tsc --showConfig -p <project>/tsconfig.json | jq -c '{files: [.files[]? | select(endswith(".d.ts"))], types: .compilerOptions.types, lib: .compilerOptions.lib}'
node -p "require('node:module').builtinModules.join('\n')" && node --help
rg -l --no-ignore "^declare module '" <declaration-folder>
rg -oNI "(?:from |import\()'(@rasm/[^']+)'" -r '$1' -t ts -t js <target> | sort -u |
    (cd <project> && xargs node --input-type=module -e "import { fileURLToPath } from 'node:url'; for (const s of process.argv.slice(1)) console.log(s, fileURLToPath(import.meta.resolve(s)))")
```

## [03]-[MAPPING]

Maps list what each file of the target declares and exports and who reads it, one command per step, each run once over `<paths>`:
1. Exports: exported names per file with their kind, the list a pass narrows
2. Name families: exported names differing by case, plural, or a joined builder or runner word, each family read whole before judging it
3. Constants: exported upper-case constants, each traced to the reader and the source that should state it
4. Importers: files importing a module and the members each reads, package specifiers, `#/` subpaths, and renamed imports included
5. Text: sites spelling a member call, key, header, path, or message a declaration holds, and mentions across docs, skills, and manifests
6. Like work: clone pairs touching the target across `<roots>`, ranked by lines times similarity
7. Nesting: callback and block depth per site past one level, deepest first

```bash
ast-grep outline <paths> --items exports --json=stream | jq -r '.path as $p | .items[] | "\($p)\t\(.name)\t\(.symbolType)"'
ast-grep outline <paths> --items exports --json=stream | jq -r '.items[].name' | sort -u |
    awk '{ k = $1; if (k ~ /^(make|run|to|from|new|create|build|get|with)[A-Z]/) sub(/^(make|run|to|from|new|create|build|get|with)/, "", k)
        k = tolower(k); sub(/(of|for|s)$/, "", k); g[k] = g[k] " " $1; n[k]++ } END { for (k in g) if (n[k] > 1) print g[k] }'
ast-grep outline <paths> --items exports --json=stream | jq -r '.path as $p | .items[] | select(.name | test("^[A-Z][A-Z0-9_]+$")) | "\($p)\t\(.name)"'
knip --config .config/knip.json --trace-file <file> --no-progress --no-config-hints
rg -nw '<name>' .
jscpd --config .config/jscpd.json --similarity 0.6 --min-nodes 60 --format typescript,tsx,javascript,jsx --reporters json --output <dir>/jscpd <roots>;
    jq -r --arg t "$PWD/<target>/" '[.duplicates[] | select((.firstFile.name | startswith($t)) or (.secondFile.name | startswith($t)))] | sort_by(-(.lines * .similarity))[]
    | "\(.lines)\t\(.similarity)\t\(.firstFile.name):\(.firstFile.start)\t\(.secondFile.name):\(.secondFile.start)"' <dir>/jscpd/jscpd-report.json
oxlint -c <dir>/nesting.json --format json <paths> |
    jq -r '.diagnostics[] | select(.code | test("max-(nested-callbacks|depth)")) | [(.message | capture("\\((?<n>[0-9]+)\\)").n), .filename, .labels[0].span.line, .code] | @tsv' | sort -rn
```

Commands read by their own rules:
- Families: a type and its value under one name print once as the target form, and a family across files is one concept split apart or a shared word
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Nesting: `nesting.json` holds `{"rules": {"max-nested-callbacks": ["error", {"max": 1}], "max-depth": ["error", {"max": 1}]}}`, exit 1 on a site
- Limits: `.oxlintrc.json` sets `max-nested-callbacks` to 3 and `max-depth` to its default of 4, counting generators of `Effect.gen` and `Effect.fn` as callbacks

## [04]-[MECHANISMS]

TypeScript holds types and values in separate namespaces and derives types from values, so most reductions here remove a declaration by deriving it:
- One name: a type and the value stating it share one name under one export (`const Tally = Struct.makeReducer(...)` with `type Tally = typeof Tally.initialValue`, `interface Tool` with `const Tool`), readers importing one symbol
- Derived types: `typeof X.Type` of a schema, `ReturnType` and `Parameters` of a builder, `keyof typeof` of a keyed record, `Pick` and `Omit` over a host declaration, each in place of a hand-written copy
- Services: `Context.Service<Self>()(id, { make })` infers a service shape from its builder and holds it as `Self.make`, one name for tag, shape, and builder
- Cases: `Data.taggedEnum` or `Schema.TaggedUnion` for states, folded through `$match`, `match`, or `Match`, in place of flags, string kinds, and `if` chains
- Records: one `as const` record keyed by case holds every column, `keyof typeof` it as the key type, in place of a name list beside matching calls
- Exports: one `export { }` list under `[EXPORTS]` names each concept once, module-private by default, a type and its value exported as one name
- Members: package members found through the export listing (`Struct.evolve`, `Schedule`, `Tool.getJsonSchemaFromSchema`) in place of spreads, loops, and hand codecs
- Inference: a derived type inferring one member as `never` takes one stated type there, and a helper tying a key to its value type stays
- Generics: a variadic rest of generic tuples can collapse inference to its constraint, a first parameter typed apart (`mod, ...mods`) keeping it

Host rules bind where a host checks the code, each proved by one probe of its check over a fresh build:
- Claude Code mods: `claude plugin validate` passes `$` into the module's own `function` declarations alone, refusing `Self.make`, `Layer.succeed`, and stored values
- Builders: a service built from `$` keeps a `function` builder named by its verb, its shape derived from it
- Claude Code hooks: `on(...)` calls take statement position in a block-bodied thunk, as validate refuses `() => on(...).catch(h)` and `() => void on(...)`, and every event name is a literal

## [05]-[PATTERNS]

Patterns common in this tree's TypeScript, each a lead the map confirms before any rebuild, its rebuild holding under the condition beside it:

| [INDEX] | [PATTERN]                  | [LOOK_FOR]                                                   | [REBUILD]                                    |
| :-----: | :------------------------- | :----------------------------------------------------------- | :------------------------------------------- |
|  [01]   | Name families              | `X` beside `x`, `makeX`, `runX`, `Xs`, `XES` in one concept  | One name, type and value sharing it          |
|  [02]   | Service triads             | Tag class, `make<X>` builder, `ReturnType` shape per service | `Context.Service` `make` option              |
|  [03]   | Scattered operations       | Free functions exported beside the object they act on        | Members of that object                       |
|  [04]   | Exported constants         | Upper-case names, file names, keys, limits in an export list | Read from tool, manifest, or host            |
|  [05]   | Tables restating a source  | Record mapping a host union or package enum to values        | Derived from its source                      |
|  [06]   | Alias spam                 | Type alias read once, `Pick` or `Omit` renaming a host type  | Type inline or derived at its reader         |
|  [07]   | Interface beside its value | Interface restating a schema, reducer, or builder result     | `typeof X.Type` or builder type, one name    |
|  [08]   | Parallel lists             | Name union beside a block of matching calls                  | One keyed record of calls, `keyof typeof` it |
|  [09]   | Hand-rolled Effect         | Spread per optional key, hand JSON Schema, retry, glob, diff | Effect member from the export listing        |
|  [10]   | Callback towers            | Nested `Option.match`, `Effect.flatMap`, generator at limit  | One `pipe` from innermost subject, `Match`   |

Patterns hold under these conditions:
- Row [01] -> a family naming different jobs (`runHook` running effects beside type `Hook`) splits, the joined name taking its own job's word
- Row [02] -> a builder taking `$` in a Claude Code mod stays a `function` declaration, as the host rule above refuses `Self.make`
- Row [03] -> an operation a host calls by name (an `on()` handler, a render component) stays a function its registration reads
- Row [04] -> a constant more than one reader shares with no source stating it stays, file-private where one file reads it
- Row [05] -> `satisfies Record<HostUnion, V>` stays only where no source states the values

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
