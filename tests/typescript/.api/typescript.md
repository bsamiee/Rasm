# [typescript] — the compiler package: the tsc gate binary and the syntactic scanner the gauge kit composes

[PACKAGE_SURFACE]:
- package: `typescript` · version `6.0.3` · license `Apache-2.0`
- module: CJS bundle (`lib/typescript.js`) with one flat namespace export — `import ts from 'typescript'` is the one binding; `lib/tsc.js` behind the `tsc`/`tsserver` bins.
- asset: the full compiler — scanner, parser, binder, checker, emitter, language service — plus the bundled `lib.*.d.ts` standard-library declarations.
- runtime: node; the parser lane is pure and synchronous (no filesystem, no program, no checker).
- plane: `plane:dev` — the `tsc` half of the dual compiler floor (`@effect/tsgo` is the native twin; `tsc` is the conformance authority whose diagnostic codes doctrine cites), the `@stryker-mutator/typescript-checker` engine, and the syntactic scanner `@rasm/ts-testkit/gauges` composes.
- rail: type gate binary / syntactic import scan.

The workspace consumes this package on two disjoint lanes. The GATE lane is process-level: `tsc -p tsconfig.json` runs beside `tsgo` as the dual floor, and the Stryker checker boots it per mutant — no repo code imports the checker. The SCANNER lane is the one programmatic composition: the kit's import-graph gauge parses each source with `ts.createSourceFile` (no program, no types, parents off) and walks `ts.forEachChild` with the `ts.is*` predicates to harvest every import/export/dynamic-import specifier — a full-fidelity AST at parse cost, immune to the regex evasions (multiline imports, template noise, comments) a text scan invites. The checker API stays out of the estate: a semantic question is the gate's job, never a gauge's.

## [01]-[SCANNER_SURFACE]

[PUBLIC_SCOPE]: the parse-and-walk lane the gauge engine composes — pure, synchronous, program-free.

| [INDEX] | [SYMBOL]                            | [TYPE]                                                                                         | [CAPABILITY]                                                        |
| :-----: | :---------------------------------- | :--------------------------------------------------------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | `ts.createSourceFile`               | `(fileName, sourceText, languageVersionOrOptions, setParentNodes?, scriptKind?) => SourceFile` | one file parsed standalone; `ScriptTarget.Latest`, parents off      |
|  [02]   | `ts.forEachChild<T>`                | `(node, cbNode, cbNodes?) => T \| undefined`                                                   | the one AST walk; recursion is the caller's visit function          |
|  [03]   | `ts.isImportDeclaration`            | `(node) => node is ImportDeclaration`                                                          | static import row; `.importClause?.isTypeOnly` splits the plane     |
|  [04]   | `ts.isExportDeclaration`            | `(node) => node is ExportDeclaration`                                                          | re-export row; `.isTypeOnly` + optional `.moduleSpecifier`          |
|  [05]   | `ts.isCallExpression`               | `(node) => node is CallExpression`                                                             | with `expression.kind === SyntaxKind.ImportKeyword`: dynamic import |
|  [06]   | `ts.isStringLiteral`                | `(node) => node is StringLiteral`                                                              | the specifier text; a non-literal specifier is a grammar error      |
|  [07]   | `ts.ScriptTarget` / `ts.SyntaxKind` | const enums on the namespace                                                                   | `ScriptTarget.Latest = 99`; `SyntaxKind.ImportKeyword = 102`        |
|  [08]   | `ts.version` / `ts.transpileModule` | `string` / `(input, transpileOptions) => TranspileOutput`                                      | engine identity; single-file erase without a program                |

```ts contract
// lib/typescript.d.ts — the composed scanner lane.
function createSourceFile(fileName: string, sourceText: string, languageVersionOrOptions: ScriptTarget | CreateSourceFileOptions, setParentNodes?: boolean, scriptKind?: ScriptKind): SourceFile
function forEachChild<T>(node: Node, cbNode: (node: Node) => T | undefined, cbNodes?: (nodes: NodeArray<Node>) => T | undefined): T | undefined
interface SourceFile extends Declaration, LocalsContainer { readonly statements: NodeArray<Statement>; fileName: string; text: string }
interface ImportDeclaration extends Statement { readonly importClause?: ImportClause; readonly moduleSpecifier: Expression }
interface ExportDeclaration extends DeclarationStatement, JSDocContainer { readonly isTypeOnly: boolean; readonly exportClause?: NamedExportBindings; readonly moduleSpecifier?: Expression }
interface CallExpression extends LeftHandSideExpression, Declaration { readonly expression: LeftHandSideExpression; readonly arguments: NodeArray<Expression> }
```

## [02]-[GATE_SURFACE]

The binary is the gate, and configuration is the whole contract: `tsc -p <tsconfig>` under `noEmit` walks the project references of the named config and projects diagnostics to stderr/exit code. The root `typecheck` script fans it across the root solution and every spec-estate project after `tsgo` passes — parity is the floor claim, and a construct the two compilers disagree on is rewritten, never suppressed. Flag law lives in `tsconfig.base.json`; this catalog never mirrors it.

## [03]-[INTEGRATION]

[STACK: `typescript` + `@rasm/ts-testkit/gauges`] — `Imports.scan` parses each supplied source once (`createSourceFile(path, text, ScriptTarget.Latest, false)`) and one recursive `forEachChild` visit harvests three specifier forms — static import, re-export with specifier, `import(...)` call — each carrying its `typeOnly` fact so the architecture suite can rule on type-plane traffic separately. The walk is the sanctioned boundary-adapter kernel: a platform callback seam whose accumulator detaches immutable at the return.

[STACK: `typescript` + `@stryker-mutator/typescript-checker`] — the checker boots this compiler against `tsconfig.base.json` to discard mutants that no longer type-check, keeping the mutation score a behavioral signal instead of a compile-error census (`stryker-mutator-typescript-checker.md`).

[BOUNDARY vs `@effect/tsgo`] — the native port runs first and fast; `tsc` decides conformance. Neither lane loads the other's API, and no gauge asks a checker question — semantic truth belongs to the gate run, syntactic structure to the scanner.

## [04]-[RAIL_LAW]

- Owns: the type gate binary and program-free syntactic parsing over single files.
- Accept: `createSourceFile` + `forEachChild` + `ts.is*` predicates for structural harvests; `ScriptTarget.Latest` with parents off for scan work; `transpileModule` where a scratch fixture needs erased output without a program.
- Reject: regex import scanning beside the AST lane; `ts.createProgram`/checker composition inside a gauge — a semantic audit routes to the gate; a second parser admission (babel, swc AST) for work this parser already owns; reading `.kind` numerals where a `ts.is*` predicate names the shape.
- Boundary: the scanner sees one file at a time and proves nothing about resolution — exports-map and edge-ledger truth stays with the architecture suite's own rules over the harvested specifiers.
