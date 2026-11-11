import { hashArray } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { inferred, type Project, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Option, Result, Schema } from 'effect';
import { CONFIGS, documents, targets } from '#/targets';
import { LOCK, located, Metadata, Releases, report, type Workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = [LOCK, 'uv.toml', '.python-version', ...CONFIGS] as const;
const TECHNOLOGIES = ['python'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyzed = Effect.fnUntraced(function* ({ files, context: { workspaceRoot }, path }: Request<unknown>) {
    const locks = files.filter((file) => path.basename(file) === LOCK);
    const [read, unlocked] = yield* Effect.partition(
        locks,
        (lock) =>
            printed('uv', ['workspace', 'metadata', '--preview-features', 'workspace-metadata', '--quiet'], { cwd: path.join(workspaceRoot, path.dirname(lock)) }).pipe(
                Effect.flatMap(Schema.decodeEffect(Metadata)),
                Effect.mapBoth({ onFailure: (error): readonly [string, Error] => [lock, ToolError.of(`uv workspace metadata for ${lock}`, 'does not decode')(error)], onSuccess: (metadata) => ({ lock, metadata }) }),
            ),
        { concurrency: 'unbounded' },
    );
    const [{ documented, unparsed }, listed] = yield* Effect.all(
        [
            documents(
                workspaceRoot,
                read.flatMap(({ metadata }) => [metadata.workspace.path, ...metadata.members.map((member) => member.path)]).map((at) => path.relative(workspaceRoot, at) || '.'),
            ),
            Effect.result(printed('uv', ['python', 'list', 'cpython', '--only-downloads', '--all-versions', '--offline', '--output-format', 'json'], { cwd: workspaceRoot }).pipe(Effect.flatMap(Schema.decodeEffect(Releases)), Effect.mapError(ToolError.of('uv python list', 'does not decode')))),
        ],
        { concurrency: 'unbounded' },
    );
    const releases = Array.dedupe(Result.getOrElse(listed, () => []).map(({ version }) => version));
    const workspaces = yield* Effect.forEach(read, ({ lock, metadata }) => located(workspaceRoot, report(path, workspaceRoot, lock, metadata, documented, releases)), { concurrency: 'unbounded' });
    return Result.succeed({ report: workspaces, failures: [...unlocked, ...unparsed, ...workspaces.flatMap(({ failures }) => failures), ...Array.zip(locks, Option.toArray(Result.getFailure(listed)))] });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes, createDependencies } = inferred({
    module: import.meta.url,
    files: FILES,
    options: Schema.Struct({}),
    units: Effect.fnUntraced(function* ({ files, context, path }: Request<unknown>) {
        const locks = files.filter((file) => path.basename(file) === LOCK);
        const names = yield* Effect.promise(() =>
            globWithWorkspaceContext(
                context.workspaceRoot,
                locks.map((lock) => path.join(path.dirname(lock), '**')),
            ),
        );
        return Array.take(locks, 1).map((file): Unit => ({ key: LOCK, file, salt: hashArray(names) }));
    }),
    read: (missing: Array.NonEmptyReadonlyArray<Unit>, request: Request<unknown>) => Effect.map(analyzed(request), (reading) => missing.map(() => reading)),
    sources: () => ({ tracked: FILES.map((file) => `**/${file}`) }),
    projects: (workspaces: readonly Workspace[], _, { path, plugin, named, placed }) => {
        const roots = new Map(workspaces.flatMap(({ members }) => members).map((member) => [member.root, member] as const));
        return workspaces.flatMap(({ lock, members }) =>
            members.map((member): Project => {
                const stated = targets(path, member, roots, plugin, `^${named(member.root)}`, placed);
                return {
                    file: lock,
                    root: member.root,
                    technologies: TECHNOLOGIES,
                    clean: 'uv',
                    ...stated,
                    ...(member.name !== undefined && { name: member.name }),
                    configuration: {
                        ...(member.implicit !== undefined && { implicitDependencies: [...member.implicit] }),
                        ...(member.published && { release: { version: { versionActions: `${plugin.name}/release/version-actions`, versionActionsOptions: { lock: member.lock } } } }),
                    },
                };
            }),
        );
    },
    edges: (reports) => Effect.succeed(reports.flatMap(([, workspaces]) => workspaces.flatMap(({ edges }) => edges))),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
