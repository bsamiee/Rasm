# [TYPESCRIPT]

TypeScript targets read Effect 4 modules, the platform packages their runtime supplies, and their host's own declarations before a line changes, as most hand-written logic in a TypeScript file restates a member one of them already exports.

## [01]-[SKILLS]

Skills load in table order, every matching row:

| [INDEX] | [TARGET]            | [SKILLS]                                 | [CONDITION]                                            |
| :-----: | :------------------ | :--------------------------------------- | :----------------------------------------------------- |
|  [01]   | Any TypeScript file | `typescript:coding`, `typescript:effect` | Extension `.ts` or `.tsx`                              |
|  [02]   | Tests               | `typescript:testing`, `code:test`        | `*.test.ts`, a suite under `tests/`, `@effect/vitest`  |
|  [03]   | Claude Code mod     | `sazandeh:mod-ui`                        | Imports `claude-code` or `@rasm/claude-code/*`         |
|  [04]   | Adobe UXP plugin    | `uxp:coding`                             | Imports `uxp`, `photoshop`, `indesign`, `premierepro`  |
|  [05]   | Chrome extension    | `chrome:extensions`                      | Folder holds a `manifest.json` with `manifest_version` |
|  [06]   | Browser UI          | `web:css`, `web:react-aria`              | Imports `react-aria-components`, or holds `.css` files |
|  [07]   | Removal checklist   | `code:rebuild`                           | Loaded after the rows above, never first               |

## [02]-[PACKAGES]

Core packages, read in order for each target:
1. Read the installed Effect version, as module members and names differ between 4.x releases and memory holds older forms: `jq -r .version <project>/node_modules/effect/package.json`
2. List the Effect modules the target imports, each line one module whose members the target can call in place of hand code: `rg -oN "from 'effect(/[a-z-]+)?'" <target> | sort -u`
3. Inventory each imported module per `[04]` before reading any member, as a member the target never imports is the one most often hand-written: `rg -n "^export (declare )?(const|function|class|interface|type|namespace|enum) \w+" <project>/node_modules/effect/dist/<Module>.d.ts`
4. Inventory the platform package of the target's runtime, as files, processes, sockets, and HTTP arrive as Effect services there: `rg -n "^export (declare )?(const|function|class|interface|type) \w+" <project>/node_modules/@effect/platform-node/dist/*.d.ts`
5. Read the host's declarations whole where the target runs inside one, as host members, limits, and refusals sit in its types and doc comments: `rg -n "^\s*(export )?(type|interface|function|const) \w+" .artifacts/vite/@rasm/function-hooks/.claude-plugin/types/claude-code/index.d.ts`
6. List Node built-ins on the pinned Node before any shell-out or text parse in a Node process, per memory `node-builtins-before-shell-outs`: `node -p "require('node:module').builtinModules.join('\n')"`
7. Read each workspace package the target imports (`@rasm/*`) whole with its consumers, as a library member often states the fact the target restates: `ast-grep outline <package-folder> --items exports --view expanded`

## [03]-[DISCOVERY]

Packages beyond core come from the pnpm catalog, whose comments state each package's purpose:
1. List every catalog row with its comment, the candidate set for any capability the target hand-writes: `yq '.catalog | to_entries | .[] | .key + "  " + (.value | line_comment)' pnpm-workspace.yaml`
2. Match each hand-written job in the target (a parser, a retry, a formatter, a diff, a glob, a cache) against catalog comments by its subject: `yq '.catalog | to_entries | .[] | .key + "  " + (.value | line_comment)' pnpm-workspace.yaml | rg -i '<subject>'`
3. Read the matched package's declarations per `[04]` and name the member replacing the hand code, or record the package as rejected with the reason
4. Search the npm registry where the catalog holds no match, through `search:web` and `search:code`, and add a row only after the member is named: `pnpm view <package> version description exports --json`

## [04]-[INVENTORY]

Inventories list every declaration of a module before targeted reads, written to `<scratch>/inventory/<package>.txt` and counted:
1. Resolve the module's declaration file from the package's export map, its `.d.ts` sitting beside the `.js` it names: `jq -r '.exports["<subpath>"] | if type == "object" then (.types // .default) else . end' <project>/node_modules/<package>/package.json`
2. List every exported declaration with its line, and every `export *` and `export * as` line naming a module to inventory next: `rg -n "^export (declare )?(const|function|class|abstract class|interface|type|namespace|enum) \w+|^export \* (as \w+ )?from" <file>.d.ts`
3. Count declarations per kind, the count stated in the report beside the inventory file: `rg -o "^export (declare )?(const|function|class|interface|type|namespace|enum)" <file>.d.ts | sort | uniq -c`
4. Read one member's signature, overloads, and doc through `search:code`, or the member's block with its doc comment: `rg -n -B12 -A20 "^export declare const <member>\b" <file>.d.ts`

Tool limits:
- `ast-grep outline` lists interfaces and classes of a `.d.ts` and omits `export declare const` members, which hold most Effect API
- `rg` declaration listing misses re-exports through `export { a as b }` lists, which step 2's `export` lines name for a follow-up read
- Signatures spanning lines need the step 4 block read, a one-line listing shows the name alone

## [05]-[FORMS]

TypeScript forms of the main skill's categories:

| [INDEX] | [SMELL]                                            | [REBUILD]                                               | [MEASURE]     |
| :-----: | :------------------------------------------------- | :------------------------------------------------------ | :------------ |
|  [01]   | `Context.Service` shape typed by hand              | `ReturnType<typeof make<Name>>` or its success type     | Signatures    |
|  [02]   | Names differing in case alone (`Engine`, `engine`) | Verb-noun builder (`makeEngine`), or one name           | Exports       |
|  [03]   | Interface beside a reducer or schema of one shape  | `typeof X.initialValue`, `typeof X.Type`, one name      | Declarations  |
|  [04]   | Interface restating a host or package type         | `Pick`, `Omit`, `Parameters`, `ReturnType` of owner     | Declarations  |
|  [05]   | Literal union restating a host union               | Host type, or `keyof` of the registering record         | Union members |
|  [06]   | `if`/`else` chain over a discriminant              | `Match.value`, `$match`, `Match.discriminators`         | Branches      |
|  [07]   | String literal deciding behavior                   | Tagged case, host enum, or owning member                | Literals      |
|  [08]   | Module `const` read once                           | Inline value, or derivation from its owner              | Constants     |
|  [09]   | Schema wrapping a type nothing decodes             | Type alone, or owner schema's field                     | Schemas       |
|  [10]   | Hand-written JSON Schema, retry, glob, or diff     | Member found in `[03]` (`Tool.getJsonSchemaFromSchema`) | Lines         |
|  [11]   | One-line forwarding function                       | Call at each reader, unless its generic links types     | Functions     |
|  [12]   | Spread per optional key restating a map            | `Struct.evolve`, which visits only keys present         | Lines         |
|  [13]   | Name list beside a block of matching calls         | One keyed record of the calls, `keyof` as the list      | Declarations  |

Host constraints decide forms that look like smells, read them before rebuilding:
- Claude Code mods: `claude plugin validate` follows `$` only into the module's own `function` declarations, so a `$`-taking builder stays a function
- Claude Code mods: an `on(...)` registration's value is kept unless `.catch(...)` sits in statement position, `void` and returned thunks fail
- Generic helpers correlating one type parameter across arguments (`rows<C>` tying `table[component]` to `e`) stay, as inlining loses the link
- Derived shapes widen or narrow an inferred member (a record key inferred `never`), and one stated type on that value keeps the derivation

## [06]-[NESTING]

Formatter output decides depth: `oxfmt` prints one argument per line once a call holds more than one inline function, and lint rule `max-nested-callbacks` is 3:
- Nested `Option.match` or `Effect.flatMap` callbacks -> one `pipe` from the innermost subject, each step data-last at one level
- Callback reading an `Effect.all` tuple -> `yield* Effect.all([...])` in a generator
- Generator passed to `Effect.gen` at the callback limit -> a generator statement, which adds no level
- Arms holding effects that branch again -> `Match` over a tuple of subjects, each arm one outcome
- Depth check, lines past 4 levels at the repository's 4-space indent: `awk -v n=4 '{ match($0, /^ */) } RLENGTH / 4 > n { print FILENAME ":" FNR ": " RLENGTH / 4 }' <file>`

## [07]-[VERIFY]

Proof of each pass, in order, once at the end of the pass:
1. Format and file checks: `hk fix <paths>`, then `hk check <paths>`
2. Type check of the edited project and every consumer project: `nx run-many -t check -p <projects>`
3. Suites covering the edited code: `nx run-many -t test -p <suites>`
4. Builds of products bundling the edited code: `nx run-many -t build -p <products>`
5. Host validator where one exists: `claude plugin validate .artifacts/vite/@rasm/<plugin> --json`
6. Counts before and after per the main skill's measure, each kept declaration listed with the fact it states
