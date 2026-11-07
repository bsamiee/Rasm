# [MODULES]

## [01]-[ENTRIES]

Entries split a package by the runtime its modules need, and `index.ts` re-exports the modules every importing runtime loads:

```json
{
    "exports": { ".": "./index.ts", "./node": "./node/index.ts", "./vite": "./vite.ts" },
    "imports": { "#/*": "./*" },
    "sideEffects": false
}
```

```ts
// --- [EXPORTS] -------------------------------------------------------------------------

export { Item, type ItemShape } from '#/item.ts';
export { ItemError } from '#/item-error.ts';
export { rankItems } from '#/rank-items.ts';
```

`item.ts`, `item-error.ts`, and `rank-items.ts` load in every runtime, `node/` holds modules reading `node:*` or `@effect/platform-node`, and `vite.ts` is a build plugin:
- Entries name a runtime (`./node`, `./uxp`, `./extendscript`, `./browser`, `./cloudflare`) or the dependency family a set isolates (`./pg`, `./shaping`)
- Entries of one module point at it, an entry of more modules at `<entry>/index.ts`
- Modules a consumer loads through `import()` are their own entry, outside every barrel the eager importer reads
- Barrels hold `export { ... } from` statements alone, one per module
- Root closures type-check under each importing runtime, a sibling entry holding what one runtime alone loads
- Root closures load no host module (`node:*`, `adobe:*`, `cloudflare:*`), stylesheet, or other entry of a library
- Root closures load no runtime package (`@effect/platform-*`, `@effect/sql-sqlite-node`, `react-dom` renderers)
- Root closures load no package whose load chain ships wasm or a Node addon (`harfbuzzjs`, `vite`)
- Entries of a package listing `types-for-adobe` load first-party modules alone

## [02]-[FILES]

Files hold one concept and its private parts:
- Names are the primary export in kebab-case, or the name sibling packages share for one concept (`contract.ts`, `manifest.ts`, `error.ts`)
- Sub-concepts only the owner's API uses (cases, options, reason classes) stay in the owner's file
- Files whose subsections are separate concepts split into one file each, a large single concept staying whole
- Folders hold one entry's runtime set, variants sharing one role scheme, or one concept's private parts, and no file shares a folder's name
- Helpers a second module reads move under `internal/` in a module named for their concern, no barrel re-exporting them

## [03]-[SURFACE]

Consumers import neither namespaces nor aliases, so public names qualify themselves:
- Data concepts are one class holding their data, statics that build, and methods that read, a type and a value of one name merging into it
- Operation families export each function under a verb-noun name
- Services are `Context.Service` classes with `make` and `static layer`, a layer bound to one runtime sitting in that runtime's entry
- Packages declare one error class in `error.ts`, its `reason` cases in the same module and its name qualified by its package (`HostError`, `MediaError`)
- Names unique within a package can clash in a consumer importing another package, the owner renaming each for what its value is

## [04]-[PROGRAMS]

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
- Importable Node modules can own CLI startup with a top-level `if (import.meta.main)` around `runMain`
- Arguments and handlers sit in an importable module, domain logic in concept modules
- Programs started per call import only what their verdict reads, heavy subsystems behind `import()`
- Program packages point `bin` at `main.ts`, their `exports` listing only contracts a second runtime imports
