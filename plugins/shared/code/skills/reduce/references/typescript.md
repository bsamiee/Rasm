# [TYPESCRIPT]

TypeScript targets hold `.ts`, `.tsx`, `.mts`, `.cts`, `.js`, `.jsx`, and `.mjs` files alike, which `rg -t ts -t js` reads, `<project>` naming the project folder holding `package.json`, `<tsconfig>` the project's tsconfig whose `include` holds the target (`tsconfig.app.json` beside a `tsconfig.json` of `references` alone), `<paths>` files a pass touches, and `<dir>` session scratch folder.

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

ExtendScript files compile under `lib` `es5`, and a form holds there only where `es5` declares its members.

## [02]-[SOURCES]

TypeScript targets read `effect` with every subpath family and the platform package of their runtime every session, whatever they import, as most hand-rolled code in this tree restates one of them. Package reads, one command per step:
1. Guide: `effect` release the project resolves, its `AGENTS.md` and `ai-docs/` files read whole
2. Core: export listing of `effect`, groups as `.tsv` files, its `entries:` lines naming each subpath family
3. Families: export listing of every subpath family step 2 prints (`effect/http`, `effect/reactivity`), read against the target's jobs
4. Platform: export listing of `@effect/platform-<runtime>` (`node`, `bun`, `browser`, `cloudflare`) for the runtime running the target
5. Runtime: `files`, `types`, and `lib` of `<tsconfig>`, choosing platform layers `typescript:effect` lists per host
6. Node: built-ins and flags of the pinned Node, for code a Node process runs alone, never a build-time Node beside another host
7. Host: each declaration file step 5 prints and every file augmenting its module, read at the members the target calls or restates
8. Workspace: each `@rasm/*` specifier the target imports, resolved from its project to a source file read whole
9. Catalog: pnpm catalog listed whole, each package relevant to a job the target does by hand read through steps 2 and 3, then added to the project

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

Packages no project declares (a platform package beside `@effect/platform-node`, a catalog row) list from the tarball layout `search:code` names, `<project>` being that layout's folder.

## [03]-[MAPPING]

Maps list what each file of the target declares and exports and who reads it, one command per step, each run once over `<paths>`:
1. Exports: exported names per file with their kind, the list a pass narrows
2. Name families: exported names differing by case, plural, or a builder or runner word, and sets of 3 or more sharing a first or last word
3. Constants: exported upper-case constants, each traced to the reader and the source that should state it
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

Commands read by their own rules:
- Families: `<stem>` lines join one concept's type, value, and builders, `<word>-` and `-<word>` lines a shared first or last word
- Shared words: case per type (`ImageAsset`, `PdfAsset`) is one tagged union, member per value (`saveRequest`, `deleteRequest`) one keyed member
- Spread: family across files is one concept split apart or a shared word, its whole read deciding which
- Importers: knip prints each importer with the members it reads (`refs: [Action.cases.Shell.make]`), exit 0 with no finding
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Nesting: `nesting.json` holds `{"rules": {"max-nested-callbacks": ["error", {"max": 1}], "max-depth": ["error", {"max": 1}]}}`, exit 1 on a site
- Limits: `.oxlintrc.json` sets `max-nested-callbacks` to 3 and `max-depth` to its default of 4, counting `Effect.gen` and `Effect.fn` generators

## [04]-[MECHANISMS]

TypeScript keeps a value space and a type space, merges declarations of one name, and computes types from values, so most cuts here delete a declaration by deriving it from the one stating its fact. Root `tsconfig.base.json` decides which forms compile: `erasableSyntaxOnly` refuses `enum`, value `namespace`, and parameter properties, `noUncheckedIndexedAccess` types each read through a `string` key as `V | undefined`, and the patched `tsc` fails on every `@effect/language-service` diagnostic its `diagnosticSeverity` lists.

[NAMES]:
- Value and type: `const` and `type` of one concept share its name (`const Notice = Data.taggedEnum<Notice>()` beside its `type Notice`)
- Exports: one `export { Notice }` carries value, type, and statics under that name, readers importing one symbol
- Classes: `Schema.Class`, `Schema.TaggedError`, `Data.Class`, and `Context.Service` classes are type, constructor, and tag in one declaration
- Statics: class statics hold its codecs, tables, builders, and services (`UxpManifest.json`, `InterfaceMap.Store`) under its one name
- Records: `Schema.Class` or `Schema.Opaque<X>()(struct)` states a record schema and its type, in place of `Schema.Struct` with an alias
- Unions: unions, literals, and scalars keep `type X = typeof X.Type` beside their schema, as a class over a non-object base fails TS2509
- Members: case and member types read as `typeof X.cases.Tag.Type`, as `typescript/no-namespace` refuses a `declare namespace X` merging them
- Expandos: `Object.assign(value, { members })` puts members on a value no class holds (union schema, function, component)

[DERIVATION]:
- Schemas: `typeof X.Type` and `typeof X.Encoded` of a schema, and `X['field']` of a class, in place of a hand-written interface
- Cases: `typeof U.cases.Tag.Type` of a `Schema.TaggedUnion`, as `Extract<typeof U.Type, { _tag: 'Tag' }>` returns `never` for a misspelled tag
- Builders: `Effect.Success<ReturnType<typeof make>>` and `Effect.Error` of an effectful builder, bare `ReturnType` giving its `Effect`
- Generics: instantiation expression `ReturnType<typeof diff<PgTable>>` types a generic function's result at one argument
- Members: `Parameters<typeof f>[n]`, `Layer.Success` and `Layer.Error` of a layer, `NonNullable<X['k']>` of an optional field
- Modifiers: `Partial`, `Required`, `Readonly`, and `-?` derive optional and complete forms of one record, in place of a copy per form
- Keys: mapped types remap keys through an `as` clause over a template literal type (`` `on${Capitalize<K>}` ``), one union stating a name set
- Tables: `keyof typeof` a keyed record for its key union, `(typeof X)[K]` for one row, `Struct.keys` and `Record.map` for its key list and columns
- Literal sets: `Schema.Literals([...])` holds the union (`typeof X.Type`), the list (`X.literals`), and the decoder in one declaration
- Host types: `Pick`, `Omit`, and indexed access (`Parameters<EngineInterface['agent']['register']>[0]`) over a host or package declaration
- Manifests: `import plugin from '#/package.json' with { type: 'json' }` types and reads the name, version, or export map a manifest states

[INFERENCE]:
- `as const` keeps a table's keys, literal values, and tuple lengths, and `satisfies` checks it against a type without widening it
- `as const satisfies Record<HostUnion, V>` proves every case of the union present and keeps each literal, where `: Record<string, V>` erases the keys
- `satisfies` over a record of handlers (`{ readonly [N in Events]: Hook<N> }`) types each handler by its key, in place of a stated type per handler
- `const` type parameters (`<const L extends string>`) infer literal object fields and tuples from an argument, so callers drop `as const`
- `Data.TaggedEnum.WithGenerics<1>` declares one generic tagged enum (`Answer<L>`), in place of one enum per element type
- Stated return and local types restate inference, except on recursion, where an unannotated self-reference fails TS7023 or TS7024
- Derived types inferring one member as `never` take one stated type there, and a helper tying a key to its value type stays
- Variadic rests of generic tuples can collapse inference to their constraint, a first parameter typed apart (`mod, ...mods`) keeping it

[SCHEMA]:
- Unions: `Schema.TaggedUnion` gives `cases`, `guards`, `isAnyOf`, `match`, and `matchOrElse`, `Schema.toTaggedUnion` adding them to a `Union`
- Codecs: `Schema.fromJsonString` decodes text in place of `JSON.parse`, `Schema.encodeKeys` renames wire keys with the decoded type unchanged
- Defaults: `withDecodingDefaultKey` and `withConstructorDefault` fill a value once, in place of `?? default` at each reader
- Absence: `OptionFromOptionalKey` and `OptionFromNullOr` decode a missing or `null` field to `Option` at the boundary
- Fields: `X.fields`, `Struct.pick(X.fields, keys)`, `mapFields`, and `Class.extend` share fields, a restated field set drifting from its owner
- Refinements: `Schema.is(X)`, `U.guards.Tag`, `$is`, and `Predicate` members are type guards, in place of a hand `(v): v is X =>`

[MODULES]:
- ES modules export nothing by default, so a declaration absent from the file's one `export { }` list under `[EXPORTS]` is file-private
- Package `exports` in `package.json` decides what other packages reach, a module export no entry lists being package-internal through `#/` imports

[EFFECT]:
- Functions: `Effect.fnUntraced(function* (args) { })` or `Effect.fn('name')` replaces `(args) => Effect.gen(function* () { })`
- Services: `Context.Service<Self>()(id, { make })` infers its shape from `make`, held as `Self.make` under the tag's name
- Cycles: builders reading their own tag keep the shape stated in `Context.Service<Self, Shape>()`, as inference fails TS2506 there
- Keys: `deterministicKeys` computes each service and error key from package name and file path, so a moved declaration takes the key it names
- Cases: `Data.taggedEnum` or `Schema.TaggedUnion` for states, folded through `$match`, `match`, or `Match`, in place of flags and string kinds
- Reasons: `Effect.catchReason` and `Effect.catchTag` handle the one reason meaning absence, every other failure staying in `E`
- Clients: `HttpApiClient`, `RpcClient`, and `AtomHttpApi.Service` `query` and `mutation` derive a member per endpoint of their contract
- Members: Effect members from the export listing (`Struct.evolve`, `Record.getSomes`, `Schema.OptionFromOptionalKey`) replace spreads and codecs

Host rules bind where a host checks the code, each proved by one probe of its check over a fresh build:
- Claude Code mods: `claude plugin validate` follows `$` past an import into a `function` declaration alone, refusing a `const` bound to an arrow
- Refusals: `$` passed through a member call (`Draft.build($)`), a parameter, a destructured `const`, or a module `let` fails
- Builders: services built from `$` keep a `function` builder named by its verb, their shape `ReturnType<typeof makeDraft>`
- Hooks: `on(...)` calls take statement position in a block-bodied thunk, as validate refuses `() => on(...).catch(h)` for keeping its value
- Event names: every `on(...)` event name is a string literal in the call, so per-event registrations stay one thunk per event

## [05]-[PATTERNS]

Patterns common in this tree's TypeScript, each a lead the map confirms before any rebuild, its rebuild holding under the condition beside it:

| [INDEX] | [PATTERN]          | [LOOK_FOR]                                                      | [REBUILD]                                         |
| :-----: | :----------------- | :-------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | Name families      | `X` beside `x`, `makeX`, `Xs`, `XES`, shared-word sets          | One name, union of cases, or one keyed member     |
|  [02]   | Service shapes     | Shape stated beside `make`, `make<X>` and `ReturnType` shape    | `Context.Service` `make` option                   |
|  [03]   | Loose operations   | Functions, tables, constructor wrappers exported beside a type  | Statics or generated members of that type         |
|  [04]   | Exported constants | Upper-case names, file names, variable names in an export list  | Tool, manifest, host, or class static             |
|  [05]   | Hand tables        | `: Record<string, V>`, record restating a union, parallel lists | Source's values, else `as const satisfies`        |
|  [06]   | Hand shapes        | Interface beside schema or builder, copy per optionality, alias | Schema class, `Partial`, `Required`, type inline  |
|  [07]   | Hand cases         | Interface per case, `kind` string, `_tag === 'Tag'`, `Extract`  | Tagged union, `guards`, `isAnyOf`, `cases`        |
|  [08]   | Hand guards        | `(v): v is X =>` over `typeof`, `Array.isArray`, `length > 0`   | `Predicate`, `Array`, or `Schema.is` member       |
|  [09]   | Hand codecs        | `JSON.parse` then decode, rename after decode, `??` per reader  | `fromJsonString`, `encodeKeys`, decoding defaults |
|  [10]   | Hand-rolled Effect | `(a) => Effect.gen`, `isSome(x) ? x.value`, call per endpoint   | `fnUntraced`, `Option.match`, contract client     |
|  [11]   | Swallowed failures | `Effect.option`, `Effect.ignore`, `orElseSucceed` over all of E | `catchReason` or `catchTag` for its one reason    |
|  [12]   | Callback towers    | Nested `Option.match`, `Effect.flatMap`, generator at limit     | One `pipe` from innermost subject, `Match`        |

Patterns hold under these conditions:
- Row [01] -> family naming different jobs (`runHook` running effects beside type `Hook`) splits, its joined name taking its own job's word
- Row [02] -> builder taking `$` in a Claude Code mod stays a `function` declaration, and one reading its own tag keeps its stated shape
- Row [03] -> function taking `$` stays a top-level `function` declaration, and one a host calls by name stays the function its registration reads
- Row [04] -> constant more than one reader shares with no source stating it stays, file-private where one file reads it
- Row [05] -> table read by a decoded `string` keeps its `string` key, as `Record.get` refuses a `string` over literal keys (TS2345)
- Row [06] -> shape a host or contract spells keeps its fields, and a recursive function keeps its stated return type
- Row [07] -> host union discriminants (`part.type === 'Lit'` of a parser's tree) narrow through the host's field, no schema holding them
- Row [08] -> guard over a host class (`instanceof indesign.Enumerator`) stays where no `Predicate` member tests it
- Row [09] -> host value no schema decodes (a callback's argument a host passes) keeps `??` at its one read
- Row [11] -> finalizer cleanup and best-effort step logged through `Effect.ignore(effect, { log })` stay
- Row [12] -> generator at the `max-nested-callbacks` limit keeps a curried member or a generator statement, which adds no level

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
- Plugin: Claude Code plug-in or a library it bundles in `<paths>` -> validator over its fresh build, `<plugin>` as `nx show projects` prints it
- Validate: `.success` reads the verdict, and each refusal prints a `message` naming the bundle chunk and the rule it broke
- Other host -> validator its host skill names (`chrome:extensions`, `uxp:coding`)
