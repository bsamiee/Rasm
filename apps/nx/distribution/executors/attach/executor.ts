import { createProjectFileMapUsingProjectGraph, type ExecutorContext } from '@nx/devkit';
import { FsTree } from '@nx/devkit/internal';
import { Completed } from '@rasm/process';
import { Effect } from 'effect';
import { ChildProcess, CommandLine } from 'effect/process';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { createReleaseGraph } from 'nx/src/command-line/release/utils/release-graph';
import { ReleaseVersion } from 'nx/src/command-line/release/utils/shared';
import { Dry, dried, packaged, refused, run } from '#/executor';

// --- [MODELS] --------------------------------------------------------------------------

const Attach = Dry.annotate({
    title: 'Attach',
    description: "Uploads the files a project's packaging targets wrote to the GitHub release whose tag nx release gives the version its manifest states, through gh release upload --clobber",
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const tagged = Effect.fnUntraced(function* (context: ExecutorContext, project: string) {
    const tree = new FsTree(context.root, context.isVerbose);
    const failed = refused(project, 'has no git tag: nx.json release does not resolve');
    const owned = yield* Effect.tryPromise({ try: () => createProjectFileMapUsingProjectGraph(context.projectGraph), catch: failed });
    const { error, nxReleaseConfig } = yield* Effect.tryPromise({ try: () => createNxReleaseConfig(context.projectGraph, owned, context.nxJsonConfiguration?.release), catch: failed });
    const resolved = yield* Effect.mapError(Effect.fromNullishOr(nxReleaseConfig), () => failed(error?.code));
    const graph = yield* Effect.tryPromise({ try: () => createReleaseGraph({ tree, projectGraph: context.projectGraph, nxReleaseConfig: resolved, filters: {}, firstRelease: false, verbose: context.isVerbose, skipVersionResolution: true }), catch: failed });
    const [{ name, releaseTag }, actions] = yield* Effect.mapError(Effect.all([Effect.fromNullishOr(graph.projectToReleaseGroup.get(project)), Effect.fromNullishOr(graph.projectsToVersionActions.get(project))]), refused(project, 'has no git tag: no release group of nx.json release holds it'));
    const { currentVersion } = yield* Effect.tryPromise({ try: () => actions.readCurrentVersionFromSourceManifest(tree), catch: refused(project, 'has no git tag: its manifest states no version') }).pipe(Effect.flatMap(Effect.fromNullishOr));
    return new ReleaseVersion({ version: currentVersion, releaseTagPattern: releaseTag.pattern, projectName: project, releaseGroupName: name }).gitTag;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* (stated: typeof Attach.Type, context: ExecutorContext) {
    const [dry, files, tag] = yield* Effect.all([dried(stated), packaged(context), Effect.flatMap(Effect.fromNullishOr(context.projectName), (project) => tagged(context, project))]);
    const upload = ['release', 'upload', tag, ...files, '--clobber'];
    yield* dry ? Effect.logInfo(`Would run ${CommandLine.display(['gh', ...upload])}`) : Completed.output(ChildProcess.make('gh', upload, { cwd: context.root, stdin: 'ignore' }));
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Attach, execute };
