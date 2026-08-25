# [TS_TESTS_API_TYPESCRIPT]

[PACKAGE_SURFACE]:
- package: `typescript` · license `Apache-2.0`
- module: ESM exports map — `.` resolves `lib/version.cjs` (version identity only); the API lives behind `typescript/unstable/{sync,async,fs,proto,ast,ast/*}` subpaths; `lib/tsc.js` behind the `tsc`/`tsserver` bins wraps the bundled native executable.
- asset: the native (Go) compiler binary with a thin JS client — AST type declarations, `is*` guards, scanner, node factory, and an IPC `API`/`Snapshot`/`Project` client that spawns the native server.
- runtime: node; every parse and semantic question rides the native server over IPC — the package ships no in-process text-to-AST parser.
- plane: `plane:dev` — the one compiler gate; `tsc` is the conformance authority whose diagnostic codes doctrine cites.
- rail: type gate binary.

Workspace consumes this package on the GATE lane only: `tsc --build tsconfig.json` is the one compiler gate, walking the root solution's project references — no repo code imports this package. Program-free syntactic parsing moved to `@swc/core` (`swc-core.md`) the day the flat `lib/typescript.js` namespace stopped shipping: `import ts from 'typescript'` now binds version identity alone, and text-to-AST work in-process is not this package's capability.

## [01]-[UNSTABLE_API_SURFACE]

[PUBLIC_SCOPE]: the server-backed subpaths, catalogued as boundary knowledge — no kit member or suite composes them; a semantic question routes to the gate run.

| [INDEX] | [SYMBOL]                                | [TYPE]                          | [CAPABILITY]                                                 |
| :-----: | :-------------------------------------- | :------------------------------ | :----------------------------------------------------------- |
|  [01]   | `unstable/sync` `API`                   | `new (options?) => API`         | spawns native server; `parseConfigFile`, `updateSnapshot`    |
|  [02]   | `unstable/sync` `Snapshot`              | `updateSnapshot() => Snapshot`  | `getProjects()`, `getDefaultProjectForFile`, disposable      |
|  [03]   | `unstable/sync` `Project.getSourceFile` | `(file) => SourceFile \| undef` | server-parsed AST for a file inside a real project           |
|  [04]   | `unstable/ast`                          | types + enums + guards          | `Node`, `SyntaxKind`, `ScriptTarget`, `Node.forEachChild`    |
|  [05]   | `unstable/ast/is`                       | `(node) => node is <T>`         | `isImportDeclaration`, `isStringLiteral`, `isCallExpression` |
|  [06]   | `unstable/ast/scanner` / `ast/factory`  | lexer / node constructors       | token stream and synthetic-node minting, parse-free          |
|  [07]   | `unstable/fs` / `unstable/proto`        | host fs seam / wire types       | server transport contracts                                   |

```ts signature
class API<FromLSP extends boolean = false> {
    constructor(options?: APIOptions | LSPConnectionOptions);
    parseConfigFile(file: DocumentIdentifier): ConfigResponse;
    updateSnapshot(params?: UpdateSnapshotParams): Snapshot;
    close(): void
}
class Snapshot { getProjects(): readonly Project[]; getDefaultProjectForFile(file: DocumentIdentifier): Project | undefined; dispose(): void }
interface Node { forEachChild<T>(visitor: (node: Node) => T, visitArray?: (nodes: NodeArray<Node>) => T): T | undefined }
```

## [02]-[GATE_SURFACE]

Binary is the gate, and configuration is the whole contract: `tsc --build tsconfig.json` walks the root solution's `references` — every folder package and spec project, each carrying its own `tsconfig.json` — and projects diagnostics to stderr/exit code. `files: []` empties the root config, so a `-p … --noEmit` form typechecks the empty solution shell and greens falsely. Flag law lives in `tsconfig.base.json`; this catalog never mirrors it.

## [03]-[INTEGRATION]

[STACK: `typescript` + the assay STATIC rail] — `assay static` drives `pnpm tsc --build tsconfig.json --pretty false` and parses the emitted diagnostics as the one type verdict; mutation runs uncheckered, so no Stryker plugin boots this compiler (`tests/typescript/README.md` [08]-[GATES]).

[BOUNDARY vs `@swc/core`] — syntactic import harvesting is swc's lane (`swc-core.md`): in-process, synchronous, program-free. Spawning the native server to answer a structural question is the rejected shape.

## [04]-[RAIL_LAW]

- Owns: the type gate binary; conformance diagnostics; the server-backed semantic API.
- Accept: `tsc -p` as a process gate; `unstable/ast` type declarations and `is*` guards where a consumer already holds a server-minted `Node`.
- Reject: importing `typescript` for in-process parsing — the `.` export is version identity, and the AST arrives only from the native server; spawning `API` inside a gauge for a syntactic question `@swc/core` answers in-process; reading `.kind` numerals where an `is*` predicate names the shape.
- Boundary: semantic truth belongs to the gate run; syntactic structure to the swc scanner; exports-map and edge-ledger truth to the architecture suite's own rules.
