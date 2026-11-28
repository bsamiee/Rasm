# [TYPESCRIPT]

TypeScript targets hold `.ts`, `.tsx`, `.mts`, `.cts`, `.js`, `.jsx`, and `.mjs` files, read by `rg -t ts -t js`. Placeholders: `<project>` names project folder holding `package.json`, `<tsconfig>` project tsconfig with `include` holding target (`tsconfig.app.json` beside `tsconfig.json` of `references` alone), `<paths>` files each pass edits, and `<dir>` session scratch folder.

## [01]-[SKILLS]

| [INDEX] | [TARGET]         | [SKILLS]                                                 | [CONDITION]                                              |
| :-----: | :--------------- | :------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | Any TypeScript   | `typescript:coding`, `typescript:effect`                 | Every target, whatever it imports                        |
|  [02]   | Tests            | `typescript:testing`                                     | `*.test.ts`, suite under `tests/`                        |
|  [03]   | Claude Code mod  | `plugin-authoring`, `sazandeh:mod-ui`                    | Imports `@rasm/claude-code/*`, host in tsconfig `files`  |
|  [04]   | Adobe UXP plugin | `uxp:coding`                                             | Imports `adobe:<host>`, `@adobe/*`, types `@adobe-uxp-*` |
|  [05]   | ExtendScript     | `uxp:coding`                                             | `tsconfig.extendscript.json` project, its callers        |
|  [06]   | Chrome extension | `chrome:extensions`                                      | `manifest.json` holding `manifest_version`               |
|  [07]   | Browser UI       | `web:css`, `web:react-aria`, `web:web-platform-baseline` | Imports `react-aria-components`, holds `.css`            |

ExtendScript files compile under `lib` `es5`, and forms compile there only where `es5` declares their members.

## [02]-[SOURCES]

TypeScript targets read `effect` with every subpath export and their runtime's platform package every session, whatever they import, as most hand-rolled code in this repository restates one of them. Package reads, one command per step:
1. Guide: `effect` release the project resolves, its `AGENTS.md` and `ai-docs/` files read whole
2. Core: export listing of `effect`, groups as `.tsv` files, its `entries:` lines naming each subpath export
3. Subpaths: export listing of every subpath export step 2 prints (`effect/http`, `effect/reactivity`), read against the target's jobs
4. Platform: export listing of `@effect/platform-<runtime>` (`node`, `bun`, `browser`, `cloudflare`) for the target's runtime
5. Runtime: `files`, `types`, and `lib` of `<tsconfig>`, for picking each platform layer `typescript:effect` lists per host
6. Node: built-ins and flags of pinned Node, for code a Node process runs alone, never a build-time Node beside another host
7. Host: each declaration file step 5 prints and every file augmenting its module, read at members the target calls or restates
8. Workspace: each `@rasm/*` specifier the target imports, resolved from its project to a source file read whole
9. Catalog: pnpm catalog listed whole, each package fitting a job the target does by hand read through steps 2 and 3, then added to its project

```bash
jq -r .version <project>/node_modules/effect/package.json && fd -d 1 '^(AGENTS\.md|llms.*\.txt|ai-docs)$' <project>/node_modules/effect
node plugins/shared/search/skills/code/scripts/exports.ts <project> effect
node plugins/shared/search/skills/code/scripts/exports.ts <project> effect | awk '/^entries:/ { on = 1; next } on { print $1 }' |
    xargs -n 1 node plugins/shared/search/skills/code/scripts/exports.ts <project>
node plugins/shared/search/skills/code/scripts/exports.ts <project> @effect/platform-<runtime>
tsc --showConfig -p <tsconfig> | jq -c '{files: [.files[]? | select(endswith(".d.ts"))], types: .compilerOptions.types, lib: .compilerOptions.lib}'
node -p "require('node:module').builtinModules.join('\n')" && node --help
rg -l --no-ignore "^declare module '" <declaration-folder>
rg -oNI "(?:from |import\()'(@rasm/[^']+)'" -r '$1' -t ts -t js <target> | sort -u |
    (cd <project> && xargs node --input-type=module -e "import { fileURLToPath } from 'node:url'; for (const s of process.argv.slice(1)) console.log(s, fileURLToPath(import.meta.resolve(s)))")
yq '.catalog, .catalogs' pnpm-workspace.yaml
```

Packages no project declares (platform package beside `@effect/platform-node`, catalog row) list from the tarball layout `search:code` names, with `<project>` as that layout's folder.

## [03]-[MAPPING]

Maps list what each file of the target declares and exports and who reads it, one command per step, each run once over `<paths>`:
1. Exports: exported names and kinds per file, for a pass to narrow
2. Name families: exported names differing by case, plural, or builder or runner word, and sets of 3 or more sharing a first or last word
3. Constants: exported upper-case constants, each traced to its readers and to the source owning its value
4. Importers: files importing a module and the members each reads, package specifiers, `#/` subpaths, and renamed imports included
5. Text: sites spelling a member call, key, header, path, or message a declaration holds, and mentions across docs, skills, and manifests
6. Like work: clone pairs touching the target across `<roots>`, ranked by lines times similarity
7. Nesting: callback and block depth per site past one level, deepest first

```bash
ast-grep outline <paths> --items exports --json=stream | jq -r '.path as $p | .items[] | "\($p)\t\(.name)\t\(.symbolType)"'
ast-grep outline <paths> --items exports --json=stream | jq -r '.items[].name' | sort -u |
    awk '{ k = $1; if (k ~ /^(make|run|to|from|new|create|build|get|with)[A-Z]/) sub(/^(make|run|to|from|new|create|build|get|with)/, "", k)
        k = tolower(k); sub(/(of|for|s)$/, "", k); g[k] = g[k] " " $1; n[k]++
        w = $1; gsub(/[A-Z][a-z0-9]/, " &", w); m = split(tolower(w), p, " ")
        if (m > 1) { g[p[1] "-"] = g[p[1] "-"] " " $1; n[p[1] "-"]++; g["-" p[m]] = g["-" p[m]] " " $1; n["-" p[m]]++ } }
        END { for (k in g) if (n[k] > (k ~ /-/ ? 2 : 1)) print k ":" g[k] }'
ast-grep outline <paths> --items exports --json=stream | jq -r '.path as $p | .items[] | select(.name | test("^[A-Z][A-Z0-9_]+$")) | "\($p)\t\(.name)"'
knip --config .config/knip.json --trace-file <file> --no-progress --no-config-hints
rg -nw '<name>' .
jscpd --config .config/jscpd.json --similarity 0.6 --min-nodes 60 --format typescript,tsx,javascript,jsx --reporters json --output <dir>/jscpd <roots>;
    jq -r --arg t "$PWD/<target>/" '[.duplicates[] | select((.firstFile.name | startswith($t)) or (.secondFile.name | startswith($t)))] | sort_by(-(.lines * .similarity))[]
    | "\(.lines)\t\(.similarity)\t\(.firstFile.name):\(.firstFile.start)\t\(.secondFile.name):\(.secondFile.start)"' <dir>/jscpd/jscpd-report.json
oxlint -c <dir>/nesting.json --format json <paths> |
    jq -r '.diagnostics[] | select(.code | test("max-(nested-callbacks|depth)")) | [(.message | capture("\\((?<n>[0-9]+)\\)").n), .filename, .labels[0].span.line, .code] | @tsv' | sort -rn
```

Command outputs read by their own rules:
- Families: `<stem>` lines join one concept's type, value, and builders, `<word>-` and `-<word>` lines group names sharing a first or last word
- Shared words: case per type (`ImageAsset`, `PdfAsset`) becomes one tagged union, member per value (`saveRequest`, `deleteRequest`) one keyed member
- Files: families across files are one concept split apart or a shared word, and reading each file whole decides which
- Importers: knip prints each importer with the members it reads (`refs: [Action.cases.Shell.make]`), and exits 0 with no finding
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Nesting: `nesting.json` holds `{"rules": {"max-nested-callbacks": ["error", {"max": 1}], "max-depth": ["error", {"max": 1}]}}`, exit 1 on any site
- Limits: `.oxlintrc.json` sets `max-nested-callbacks` to 3 and `max-depth` to its default 4, and both count `Effect.gen` and `Effect.fn` generators

## [04]-[MECHANISMS]

TypeScript keeps separate value and type spaces, merges declarations sharing a name, and computes types from values. Most cuts delete a declaration by deriving it from the declaration that states its fact. Root `tsconfig.base.json` decides which forms compile: `erasableSyntaxOnly` rejects `enum`, value `namespace`, and parameter properties, `noUncheckedIndexedAccess` types each read through a `string` key as `V | undefined`, and patched `tsc` fails on every `@effect/language-service` diagnostic its `diagnosticSeverity` lists.

[NAMES]:
- Value and type: `const` and `type` of one concept share one name (`const Notice = Data.taggedEnum<Notice>()` beside its `type Notice`)
- Exports: one `export { Notice }` exports value, type, and statics under one name, and readers import one symbol
- Classes: `Schema.Class`, `Schema.TaggedError`, `Data.Class`, and `Context.Service` classes are type, constructor, and tag in one declaration
- Statics: class statics hold codecs, tables, builders, and services (`UxpManifest.json`, `InterfaceMap.Store`) under the class name
- Records: `Schema.Class` or `Schema.Opaque<X>()(struct)` states a record schema and its type, in place of `Schema.Struct` with an alias
- Unions: unions, literals, and scalars keep `type X = typeof X.Type` beside their schema, as a class over a non-object base fails TS2509
- Members: case and member types read as `typeof X.cases.Tag.Type`, as `typescript/no-namespace` rejects a `declare namespace X` merging them
- Expandos: `Object.assign(value, { members })` puts members on a value no class holds (union schema, function, component)

[DERIVATION]:
- Schemas: `typeof X.Type` and `typeof X.Encoded` of a schema, and `X['field']` of a class, in place of a hand-written interface
- Cases: `typeof U.cases.Tag.Type` of a `Schema.TaggedUnion`, as `Extract<typeof U.Type, { _tag: 'Tag' }>` returns `never` for a misspelled tag
- Builders: `Effect.Success<ReturnType<typeof make>>` and `Effect.Error` of an effectful builder, as `ReturnType` alone gives its `Effect`
- Generics: instantiation expression `ReturnType<typeof diff<PgTable>>` types a generic function's result at one type argument
- Members: `Parameters<typeof f>[n]`, `Layer.Success` and `Layer.Error` of a layer, `NonNullable<X['k']>` of an optional field
- Modifiers: `Partial`, `Required`, `Readonly`, and `-?` derive optional and complete forms of one record, in place of a copy per form
- Keys: mapped types remap keys through an `as` clause over a template literal type (`` `on${Capitalize<K>}` ``), and one union states each name
- Tables: `keyof typeof` a keyed record for its key union, `(typeof X)[K]` for one row, `Struct.keys` and `Record.map` for its key list and columns
- Literal unions: `Schema.Literals([...])` holds union (`typeof X.Type`), list (`X.literals`), and decoder in one declaration
- Host types: `Pick`, `Omit`, and indexed access (`Parameters<EngineInterface['agent']['register']>[0]`) over a host or package declaration
- Manifests: `import plugin from '#/package.json' with { type: 'json' }` types and reads name, version, or export map of the manifest

[INFERENCE]:
- `as const` keeps a table's keys, literal values, and tuple lengths, and `satisfies` checks it against a type without widening it
- `as const satisfies Record<HostUnion, V>` proves every union case present and keeps each literal, where `: Record<string, V>` erases keys
- `satisfies` over a record of handlers (`{ readonly [N in Events]: Hook<N> }`) types each handler by its key, in place of a stated type per handler
- `const` type parameters (`<const L extends string>`) infer literal object fields and tuples from an argument, and callers drop `as const`
- `Data.TaggedEnum.WithGenerics<1>` declares one generic tagged enum (`Answer<L>`), in place of one enum per element type
- Stated return and local types restate inference, except on recursion, where an unannotated self-reference fails TS7023 or TS7024
- Derived types inferring one member as `never` take one stated type there, and a helper tying a key to its value type stays
- Rest parameters of generic tuple type can collapse inference to their constraint, and a first parameter typed apart (`mod, ...mods`) keeps it

[SCHEMA]:
- Unions: `Schema.TaggedUnion` gives `cases`, `guards`, `isAnyOf`, `match`, and `matchOrElse`, and `Schema.toTaggedUnion` adds them to a `Union`
- Codecs: `Schema.fromJsonString` decodes text in place of `JSON.parse`, `Schema.encodeKeys` renames wire keys with decoded type unchanged
- Defaults: `withDecodingDefaultKey` and `withConstructorDefault` fill a value once, in place of `?? default` at each reader
- Absence: `OptionFromOptionalKey` and `OptionFromNullOr` decode a missing or `null` field to `Option` at the boundary
- Fields: `X.fields`, `Struct.pick(X.fields, keys)`, `mapFields`, and `Class.extend` share fields, as a restated field set drifts from its owner
- Refinements: `Schema.is(X)`, `U.guards.Tag`, `$is`, and `Predicate` members are type guards, in place of a hand-written `(v): v is X =>`

[MODULES]:
- ES modules export nothing by default, and declarations absent from the file's one `export { }` list under `[EXPORTS]` stay file-private
- Package `exports` in `package.json` decides what other packages import, and module exports no entry lists stay package-internal through `#/` imports

[EFFECT]:
- Functions: `Effect.fnUntraced(function* (args) { })` or `Effect.fn('name')` replaces `(args) => Effect.gen(function* () { })`
- Services: `Context.Service<Self>()(id, { make })` infers its shape from `make` and holds it as `Self.make` under the tag's name
- Cycles: builders reading their own tag keep the shape stated in `Context.Service<Self, Shape>()`, as inference fails TS2506 there
- Keys: `deterministicKeys` computes each service and error key from package name and file path, and moved declarations take their new path's key
- Cases: `Data.taggedEnum` or `Schema.TaggedUnion` for states, folded through `$match`, `match`, or `Match`, in place of flags and string kinds
- Reasons: `Effect.catchReason` and `Effect.catchTag` handle one reason meaning absence and leave every other failure in `E`
- Clients: `HttpApiClient`, `RpcClient`, and `AtomHttpApi.Service` `query` and `mutation` derive a member per endpoint of their contract
- Members: Effect members from the export listing (`Struct.evolve`, `Record.getSomes`, `Schema.OptionFromOptionalKey`) replace spreads and codecs

Host rules apply where a host checks code, each proved by one run of its check over a fresh build:
- Claude Code mods: `claude plugin validate` follows `$` past an import into a `function` declaration alone, and rejects a `const` bound to an arrow
- Rejections: `$` passed through a member call (`Draft.build($)`), a parameter, a destructured `const`, or a module `let` fails
- Builders: services built from `$` keep a `function` builder named by its verb, with shape `ReturnType<typeof makeDraft>`
- Hooks: `on(...)` calls take statement position in a block-bodied thunk, as validate rejects `() => on(...).catch(h)` for returning its value
- Event names: every `on(...)` event name is a string literal in the call, and registrations stay one thunk per event

## [05]-[PATTERNS]

TypeScript across the repository repeats patterns below, each a candidate the map confirms before any rebuild:

| [INDEX] | [PATTERN]          | [LOOK_FOR]                                                      | [REBUILD]                                         |
| :-----: | :----------------- | :-------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | Name families      | `X` beside `x`, `makeX`, `Xs`, `XES`, shared-word sets          | One name, union of cases, or one keyed member     |
|  [02]   | Service shapes     | Shape stated beside `make`, `make<X>` and `ReturnType` shape    | `Context.Service` `make` option                   |
|  [03]   | Standalone exports | Functions, tables, constructor wrappers exported beside a type  | Statics or generated members of that type         |
|  [04]   | Exported constants | Upper-case names, file names, variable names in an export list  | Tool, manifest, host, or class static             |
|  [05]   | Hand-rolled tables | `: Record<string, V>`, record restating a union, parallel lists | Source's values, else `as const satisfies`        |
|  [06]   | Hand-rolled shapes | Interface beside schema or builder, copy per optionality, alias | Schema class, `Partial`, `Required`, type inline  |
|  [07]   | Hand-rolled cases  | Interface per case, `kind` string, `_tag === 'Tag'`, `Extract`  | Tagged union, `guards`, `isAnyOf`, `cases`        |
|  [08]   | Hand-rolled guards | `(v): v is X =>` over `typeof`, `Array.isArray`, `length > 0`   | `Predicate`, `Array`, or `Schema.is` member       |
|  [09]   | Hand-rolled codecs | `JSON.parse` then decode, rename after decode, `??` per reader  | `fromJsonString`, `encodeKeys`, decoding defaults |
|  [10]   | Hand-rolled Effect | `(a) => Effect.gen`, `isSome(x) ? x.value`, call per endpoint   | `fnUntraced`, `Option.match`, contract client     |
|  [11]   | Swallowed errors   | `Effect.option`, `Effect.ignore`, `orElseSucceed` over all of E | `catchReason` or `catchTag` for its one reason    |
|  [12]   | Nested callbacks   | Nested `Option.match`, `Effect.flatMap`, generator at limit     | One `pipe` from innermost subject, `Match`        |

Rows hold under conditions below:
- Row [01] -> families naming different jobs (`runHook` running effects beside type `Hook`) split, and each joined name takes its own job's word
- Row [02] -> builders taking `$` in a Claude Code mod stay `function` declarations, and builders reading their own tag keep their stated shape
- Row [03] -> functions taking `$` stay top-level `function` declarations, and functions a host calls by name stay what its registration reads
- Row [04] -> constants more than one reader shares with no source stating them stay, file-private where one file reads them
- Row [05] -> tables read by a decoded `string` keep `string` keys, as `Record.get` rejects a `string` over literal keys (TS2345)
- Row [06] -> shapes a host or contract spells keep their fields, and recursive functions keep their stated return type
- Row [07] -> host union discriminants (`part.type === 'Lit'` of a parser's tree) narrow through host fields, as no schema holds them
- Row [08] -> guards over a host class (`instanceof indesign.Enumerator`) stay where no `Predicate` member tests the class
- Row [09] -> host values no schema decodes (callback arguments a host passes) keep `??` at their one read
- Row [11] -> finalizer cleanup and best-effort steps logged through `Effect.ignore(effect, { log })` stay
- Row [12] -> generators at the `max-nested-callbacks` limit keep curried members or generator statements, neither adding a level

## [06]-[CHECK]

Checks run per pass, each command in its own background call, with `--files` taking `<paths>` joined by commas:

```bash
hk fix <paths>
nx affected -t check --files=<paths> --exclude=rasm
nx run <plugin>:build
claude plugin validate .artifacts/vite/<plugin> --json
```

- Scope: passes exclude root project `rasm`, as its repository-wide knip and jscpd fail on other work in a shared tree
- Manifest: `<paths>` holding `package.json` or `pnpm-lock.yaml` -> same `nx affected` line without `--exclude=rasm`
- Plugin: Claude Code plugin or a library it bundles in `<paths>` -> validator over its fresh build, `<plugin>` as `nx show projects` prints it
- Validate: `.success` holds the result, and each error prints a `message` naming bundle chunk and broken rule
- Other host -> validator its host skill names (`chrome:extensions`, `uxp:coding`)
