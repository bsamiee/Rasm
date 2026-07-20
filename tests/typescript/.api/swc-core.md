# [@swc/core] — the in-process TypeScript parser: the syntactic scanner lane the gauge kit composes

[PACKAGE_SURFACE]:
- package: `@swc/core` · version `1.15.43` · license `Apache-2.0`
- module: CJS with typed ESM surface; `export type * from '@swc/types'` re-exports the whole AST vocabulary, so `import { parseSync, type Module } from '@swc/core'` is the one binding.
- asset: the native (Rust) parser/transformer binding (`@swc/core-darwin-arm64` on this host) plus the `@swc/types` span-tagged AST declarations.
- runtime: node; `parseSync` is pure, synchronous, program-free — text in, `Module` out, throw on syntax error.
- plane: `plane:dev` — the syntactic scanner `@rasm/ts-testkit/gauges` composes; the workspace root also carries it as the `@swc-node/register` engine for nx tooling.
- rail: in-process syntactic parse.

`@swc/core` owns the lane `typescript` abandoned at its native pivot: a full-fidelity single-file AST at parse cost, in-process, immune to the regex evasions (multiline imports, template noise, comments) a text scan invites. The kit composes exactly one call — `parseSync` — and walks the returned JSON tree generically; the transform/minify surface stays out of the estate, where bundling belongs to vite.

## [01]-[PARSE_SURFACE]

[PUBLIC_SCOPE]: the parse lane the gauge engine composes. Every AST node is a plain JSON object carrying a `type` discriminant and a `span`; there is no class identity and no parent pointer, so a recursive `Object.values` walk visits every child.

| [INDEX] | [SYMBOL]                    | [TYPE]                                        | [CAPABILITY]                                                  |
| :-----: | :-------------------------- | :-------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `parseSync`                 | `(src, options?) => Module`                   | one source parsed standalone; throws on unparsable text       |
|  [02]   | `parse` / `parseFile[Sync]` | async twin / path-loading twins               | same contract over a promise or a file path                   |
|  [03]   | `ParseOptions` (ts)         | `{ syntax: 'typescript', tsx?, decorators? }` | `tsx` per-extension; `decorators: true` is strictly tolerant  |
|  [04]   | `Module`                    | `{ type: 'Module', body: ModuleItem[] }`      | the walk root                                                 |
|  [05]   | `ImportDeclaration`         | `{ source: StringLiteral, typeOnly }`         | static import; `typeOnly` is the declaration-level plane fact |
|  [06]   | `ExportNamedDeclaration`    | `{ source?: StringLiteral, typeOnly }`        | re-export; `ExportAllDeclaration` carries `source` alone      |
|  [07]   | `CallExpression`            | `{ callee: Super \| Import \| Expression }`   | `callee.type === 'Import'`: dynamic import at any depth       |
|  [08]   | `StringLiteral`             | `{ type: 'StringLiteral', value: string }`    | the specifier text                                            |

```ts signature
// index.d.ts — the composed scanner lane; the AST vocabulary rides `export type * from '@swc/types'`.
function parseSync(src: string, options?: ParseOptions): Module
interface TsParserConfig { syntax: 'typescript'; tsx?: boolean; decorators?: boolean }
interface Module extends Node, HasSpan, HasInterpreter { type: 'Module'; body: ModuleItem[] }
interface ImportDeclaration extends Node, HasSpan { type: 'ImportDeclaration'; specifiers: ImportSpecifier[]; source: StringLiteral; typeOnly: boolean }
interface ExportNamedDeclaration extends Node, HasSpan { type: 'ExportNamedDeclaration'; specifiers: ExportSpecifier[]; source?: StringLiteral; typeOnly: boolean }
interface CallExpression extends ExpressionBase { type: 'CallExpression'; callee: Super | Import | Expression; arguments: Argument[] }
```

## [02]-[INTEGRATION]

[STACK: `@swc/core` + `@rasm/ts-testkit/gauges`] — `Imports.scan` parses each supplied source once (`parseSync(text, { syntax: 'typescript', tsx: path.endsWith('.tsx'), decorators: true })`) and one generic recursive walk over every object value harvests three specifier forms — static import, re-export with specifier, `import(...)` call — each carrying its declaration-level `typeOnly` fact so the architecture suite can rule on type-plane traffic separately. An unparsable source throws loud: a span that fails to parse proves nothing.

[BOUNDARY vs `typescript`] — semantic truth belongs to the gate run (`typescript.md`); this lane sees one file at a time and proves nothing about resolution — exports-map and edge-ledger truth stays with the architecture suite's own rules over the harvested specifiers.

## [03]-[RAIL_LAW]

- Owns: in-process, program-free syntactic parsing over single TypeScript/TSX sources.
- Accept: `parseSync` with explicit `syntax`/`tsx` per file; generic `type`-discriminant walks with structural guards; `decorators: true` for tolerance.
- Reject: regex import scanning beside the AST lane; `transformSync`/`minifySync` in the test estate — build output belongs to vite; a swc-side semantic claim — the parser types nothing; inline type-cast escapes over the JSON tree where a structural guard names the shape.
- Boundary: parse failure is a loud throw, never an empty harvest — silence would turn an unparsable file into a vacuous green.
