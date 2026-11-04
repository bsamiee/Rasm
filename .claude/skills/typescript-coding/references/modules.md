# [MODULES]

## [01]-[ENTRIES]

Entries split a package by the runtime its modules need, and `index.ts` re-exports the modules every importing runtime loads:

```json
{
    "exports": { ".": "./index.ts", "./node": "./node/index.ts", "./vite": "./vite.ts" },
    "imports": { "#/*": "./*.ts" },
    "sideEffects": false
}
```

```ts
// --- [EXPORTS] -------------------------------------------------------------------------

export { Item, type ItemShape } from '#/item';
export { ItemError } from '#/item-error';
export { rankItems } from '#/rank-items';
```

`item.ts`, `item-error.ts`, and `rank-items.ts` load in every runtime, `node/` holds modules reading `node:*` or `@effect/platform-node`, and `vite.ts` is a build plugin:
- Entries name a runtime (`./node`, `./uxp`, `./extendscript`, `./browser`, `./cloudflare`) or the dependency family a set isolates (`./pg`, `./shaping`)
- Entries of one module point at it, an entry of more modules at `<entry>/index.ts`
- Modules a consumer loads through `import()` are their own entry, outside every barrel the eager importer reads
- Barrels hold `export { ... } from` statements alone, one per module, generated from the layout by the `tools/nx` sync generator
- Root closures type-check with no host types and load no wasm, `adobe:*`, or Node module, a sibling entry holding what does

## [02]-[FILES]

Files hold one concept and its private parts:
- Names are the primary export in kebab-case, role names (`contract.ts`, `precondition.ts`) standing where sibling folders share one scheme
- Sub-concepts only the owner's API uses (cases, options, reason classes) stay in the owner's file
- Files whose subsections are separate concepts split into one file each, a large single concept staying whole
- Folders hold one entry's runtime set, variants sharing one role scheme, or one concept's private parts, and no file shares a folder's name
- Helpers a second module reads move under `internal/` in a module named for their concern, no barrel re-exporting them

## [03]-[SURFACE]

Consumers import neither namespaces nor aliases, so public names qualify themselves:
- Data concepts are classes with statics that build and methods that read, operation families export each function under a verb-noun name
- Services are `Context.Service` classes with `make` and `static layer`, a layer bound to one runtime sitting in that runtime's entry
- Packages declare one error class in the module named for it, its `reason` cases in the same module
- Names unique within a package can clash in a consumer importing another package, the owner renaming each for what its value is

## [04]-[TESTS]

Tests consume a package through its entries:
- Files sit under `tests/typescript/<source path without the language folder>` as `<module>.test.ts`
- Backends and runtimes share one `cases.ts`, each running it from a `<backend>.test.ts`
- Modules no entry lists are reached through their root `#/` path
- Programs are tested through their command or app value with test layers, never through `main.ts`

## [05]-[PROGRAMS]

Program files compose and run:

```ts
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Effect, Layer } from 'effect';
import { Command } from 'effect/cli';
import { cli } from '#/cli';
import manifest from '#/package.json' with { type: 'json' };
import { Store } from '#/store';

// --- [COMPOSITION] ---------------------------------------------------------------------

Command.run(cli, { version: manifest.version }).pipe(Effect.provide(Store.layer.pipe(Layer.provideMerge(layer))), runMain);
```

`cli` declares flags and handlers, `Store` is the program's service, and `layer` is the Node platform layer:
- `main.ts` holds platform layers, configuration, and one terminal run, and exports nothing
- Arguments and handlers sit in an importable module, domain logic in concept modules
- Programs started per call import only what their verdict reads, heavy subsystems behind `import()`
- Program packages point `bin` at `main.ts`, their `exports` listing only contracts a second runtime imports
