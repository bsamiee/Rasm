import { hashArray } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { inferred, type Project, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, Result, Schema } from 'effect';
import { CHECKERS, CONFIGS, documents, targets } from '#/targets';
import { folders, LOCK, located, Metadata, report, type Workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = [LOCK, 'uv.toml', '.python-version', ...CONFIGS] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ externalNodes: Schema.optionalKey(Schema.Boolean) }).annotate({
    description: 'externalNodes: true adds each registry package a lock holds as a pypi:<name> external node and links each member to the packages its lock entry installs, absent adds neither',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyzed = Effect.fnUntraced(function* ({ files, options, context: { workspaceRoot }, path }: Request<typeof Options.Type>) {
    const locks = files.filter((file) => path.basename(file) === LOCK);
    const [read, unlocked] = yield* Effect.partition(
        locks,
        (lock) =>
            printed('uv', ['workspace', 'metadata', '--frozen', '--preview-features', 'workspace-metadata', '--quiet'], { cwd: path.join(workspaceRoot, path.dirname(lock)) }).pipe(
                Effect.flatMap(Schema.decodeEffect(Metadata)),
                Effect.mapBoth({ onFailure: (error): readonly [string, Error] => [lock, ToolError.of(`uv workspace metadata for ${lock}`, 'does not decode')(error)], onSuccess: (metadata) => ({ lock, metadata }) }),
            ),
        { concurrency: 'unbounded' },
    );
    const { documented, unparsed } = yield* documents(
        workspaceRoot,
        read.flatMap(({ metadata }) => folders(path, workspaceRoot, metadata)),
    );
    const workspaces = yield* Effect.forEach(read, ({ lock, metadata }) => located(workspaceRoot, report(path, workspaceRoot, lock, metadata, documented, options.externalNodes === true)), { concurrency: 'unbounded' });
    return Result.succeed({ report: workspaces, failures: [...unlocked, ...unparsed, ...workspaces.flatMap(({ failures }) => failures)] });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes, createDependencies } = inferred({
    module: import.meta.url,
    files: FILES,
    options: Options,
    units: Effect.fnUntraced(function* ({ files, context, path }: Request<typeof Options.Type>) {
        const locks = files.filter((file) => path.basename(file) === LOCK);
        const names = yield* Effect.promise(() =>
            globWithWorkspaceContext(
                context.workspaceRoot,
                Array.cartesianWith(locks, Array.dedupe(Object.values(CHECKERS).flatMap(({ matches }) => matches)), (lock, pattern) => path.join(path.dirname(lock), '**', pattern)),
            ),
        );
        return Array.take(locks, 1).map((file): Unit => ({ key: LOCK, file, salt: hashArray(names) }));
    }),
    read: (missing: Array.NonEmptyReadonlyArray<Unit>, request: Request<typeof Options.Type>) => Effect.map(analyzed(request), (reading) => missing.map(() => reading)),
    sources: () => ({ tracked: FILES.map((file) => `**/${file}`) }),
    projects: (workspaces: readonly Workspace[], _, { path, plugin, placed }) =>
        workspaces.flatMap(({ lock, members, externals }) =>
            members.map(
                (member, index): Project => ({
                    file: lock,
                    root: member.root,
                    technologies: ['python'],
                    clean: 'uv',
                    name: member.name,
                    ...targets(path, member, plugin, placed),
                    ...(index === 0 && { externalNodes: externals }),
                    ...(member.published && { release: { lock: member.lock } }),
                    ...(member.implicit !== undefined && { configuration: { implicitDependencies: [...member.implicit] } }),
                }),
            ),
        ),
    edges: (reports) => Effect.succeed(reports.flatMap(([, workspaces]) => workspaces.flatMap(({ edges }) => edges))),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
