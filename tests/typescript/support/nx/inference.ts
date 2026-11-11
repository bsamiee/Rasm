import { findPackageJSON } from 'node:module';
import { workspaceRoot } from '@nx/devkit';
import { Cause, Effect, FileSystem, Path, Record } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { resetWorkspaceContext, setWorkspaceRoot } from 'nx/src/devkit-internals';
import { buildProjectGraphUsingProjectFileMap } from 'nx/src/project-graph/build-project-graph';
import { isAggregateProjectGraphError, isProjectConfigurationsError } from 'nx/src/project-graph/error-types';
import { getOnlyDefaultPlugins } from 'nx/src/project-graph/plugins/get-plugins';
import type { LoadedNxPlugin } from 'nx/src/project-graph/plugins/loaded-nx-plugin';
import { retrieveProjectConfigurations, retrieveWorkspaceFiles } from 'nx/src/project-graph/utils/retrieve-workspace-files';

// --- [OPERATIONS] ----------------------------------------------------------------------

const workspace = Effect.fn('workspace')(function* (files: Record.ReadonlyRecord<string, string>, packages: readonly string[], resolved: string | URL) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const root = yield* Effect.flatMap(fs.makeTempDirectoryScoped({ prefix: 'nx-workspace-' }), fs.realPath);
    yield* Effect.forEach(Record.toEntries(files), ([file, text]) => Effect.andThen(fs.makeDirectory(path.join(root, path.dirname(file)), { recursive: true }), fs.writeFileString(path.join(root, file), text)), { concurrency: 'unbounded', discard: true });
    yield* Effect.forEach(
        packages,
        Effect.fnUntraced(function* (name) {
            const manifest = yield* Effect.fromNullishOr(findPackageJSON(name, resolved));
            yield* fs.makeDirectory(path.join(root, 'node_modules', path.dirname(name)), { recursive: true });
            yield* fs.symlink(path.dirname(manifest), path.join(root, 'node_modules', name));
        }),
        { discard: true },
    );
    yield* Effect.acquireRelease(
        Effect.sync(() => {
            const prior = workspaceRoot;
            setWorkspaceRoot(root);
            return prior;
        }),
        (prior) =>
            Effect.sync(() => {
                setWorkspaceRoot(prior);
                resetWorkspaceContext();
            }),
    );
    return root;
});

const graphed = Effect.fn('graphed')(function* (root: string, plugins: readonly LoadedNxPlugin[]) {
    const refused = (cause: unknown) => (isProjectConfigurationsError(cause) || isAggregateProjectGraphError(cause) ? cause : new Cause.UnknownError(cause, 'Nx refused the workspace before reading a plugin result'));
    const defaultPlugins = yield* Effect.promise(() => getOnlyDefaultPlugins(root));
    const configured = yield* Effect.tryPromise({ try: () => retrieveProjectConfigurations({ specifiedPlugins: [...plugins], defaultPlugins }, root, readNxJson(root)), catch: refused });
    const { fileMap, rustReferences } = yield* Effect.promise(() => retrieveWorkspaceFiles(root, configured.projectRootMap));
    const { projectGraph } = yield* Effect.tryPromise({ try: () => buildProjectGraphUsingProjectFileMap(configured.projects, configured.externalNodes, fileMap, rustReferences, null, [...plugins], configured.sourceMaps), catch: refused });
    return projectGraph;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { graphed, workspace };
