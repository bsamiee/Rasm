# [TS_TESTS_API_TYPESCRIPT]

Workspace consumes this package on the GATE lane only: `tsc --build tsconfig.json` is the one compiler gate, walking the root solution's project references — no repo code imports this package. `import ts from 'typescript'` binds version identity alone; text-to-AST work in-process is not this package's capability.

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
|  [07]   | `unstable/fs` / `unstable/proto`        | host fs boundary / wire types   | server transport contracts                                   |

```ts
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

Binary is the gate, and configuration is the whole contract: `tsc --build tsconfig.json` walks the root solution's `references` — every folder package and spec project, each carrying its own `tsconfig.json` — and projects diagnostics to stderr/exit code. Root `tsconfig.json` owns only the root `*.config.ts` files, so a `-p … --noEmit` form typechecks those alone and greens falsely for the packages. Flag law lives in `tsconfig.base.json`; this catalog never mirrors it.

## [03]-[INTEGRATION]
