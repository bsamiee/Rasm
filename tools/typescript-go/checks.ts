import { join } from 'node:path';
import { Array, Effect, Formatter, Option, Order, Schema } from 'effect';
import type { Node, SourceFile } from 'typescript/ast';
import { createFileSystemLayer } from 'typescript/fs';
import { API, type APIRequestGenerator, all, type CreateSnapshotParams, type Diagnostic, DiagnosticCategory, formatDiagnostics, type Project, type Snapshot } from 'typescript/sync';

// --- [TYPES] ---------------------------------------------------------------------------

interface Finding {
    readonly node: Node;
    readonly text: string;
}
type Check = (file: SourceFile, project: Project) => APIRequestGenerator<readonly Finding[]>;

// --- [ERRORS] --------------------------------------------------------------------------

const _Reason = Schema.TaggedUnion({
    Unlisted: { code: Schema.Finite },
    Failed: { cause: Schema.Defect() },
    Found: { count: Schema.Finite },
});
class CheckError extends Schema.TaggedError<CheckError, { readonly brand: unique symbol }>()('CheckError', { reason: _Reason }) {
    override get message(): string {
        return _Reason.match(this.reason, {
            Unlisted: ({ code }) => `git ls-files exited with code ${code}`,
            Failed: ({ cause }) => `TypeScript API request failed: ${Formatter.format(cause)}`,
            Found: ({ count }) => `checks reported ${count} finding${count === 1 ? '' : 's'}`,
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _request = <A>(request: () => A): Effect.Effect<A, CheckError> => Effect.try({ try: request, catch: (cause) => CheckError.make({ reason: { _tag: 'Failed', cause } }) });
function* _inspect(project: Project, name: string): APIRequestGenerator<readonly Diagnostic[]> {
    const file = yield* project.program.getSourceFile.gen(name);
    const source = Option.getOrThrowWith(Option.fromNullishOr(file), () => new Error(`${name} is a root file absent from the program of ${project.id}`));
    const checks = Object.entries<Check>({});
    const findings = yield* all(...checks.map(([, check]) => check(source, project)));
    return Array.zipWith(checks, findings, ([code], found) =>
        found.map(({ node, text }): Diagnostic => {
            const pos = node.getStart(source);
            return { fileName: source.fileName, pos, end: node.end, startPosition: source.getLineAndCharacterOfPosition(pos), endPosition: source.getLineAndCharacterOfPosition(node.end), code: Number(code), category: DiagnosticCategory.Error, source: 'RASM', text };
        }),
    ).flat();
}
const _diagnose = (cwd: string, params: CreateSnapshotParams, select: (snapshot: Snapshot) => readonly (readonly [Project, string])[]): Effect.Effect<readonly string[], CheckError> =>
    Effect.acquireUseRelease(
        _request(() => new API({ cwd })),
        (api) =>
            Effect.flatMap(
                _request(() => select(api.createSnapshot(params))),
                (selected) => _request(() => api.batch(...selected.map(([project, name]) => _inspect(project, name))).flatMap((diagnostics) => diagnostics.map((diagnostic) => formatDiagnostics([diagnostic], api).trimEnd()))),
            ),
        (api) => Effect.sync(() => api.close()),
    );
const diagnoseProjects = (cwd: string, configs: readonly string[]): Effect.Effect<readonly string[], CheckError> =>
    _diagnose(cwd, { openProjects: configs.map((config) => join(cwd, config)) }, (snapshot) => Array.sortWith(snapshot.getProjects(), (project) => project.id, Order.String).flatMap((project) => project.parsedCommandLine.fileNames.map((name) => [project, name] as const)));
const diagnoseSources = (cwd: string, sources: Readonly<Record<string, string>>): Effect.Effect<readonly string[], CheckError> => {
    const files = Object.entries(sources).map(([name, text]) => [join(cwd, name), text] as const);
    const names = files.map(([name]) => name);
    return _diagnose(cwd, { openFiles: names, fileSystem: createFileSystemLayer(files) }, (snapshot) => names.map((name) => [Option.getOrThrowWith(Option.fromNullishOr(snapshot.getDefaultProjectForFile(name)), () => new Error(`${name} opened into no project`)), name] as const));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Check, Finding };
export { CheckError, diagnoseProjects, diagnoseSources };
