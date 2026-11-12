import { glob } from 'node:fs/promises';
import { createProjectGraphAsync, type ExecutorContext, getOutputsForTargetAndConfiguration, joinPathFragments, type Tree } from '@nx/devkit';
import { getDependencyConfigs } from '@nx/devkit/internal';
import { Array, Config, Effect, FileSystem, type ManagedRuntime, Option, Path, Record, Schema, type Scope, Stream } from 'effect';
import { type AfterAllProjectsVersioned, VersionActions } from 'nx/release';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { createReleaseGraph } from 'nx/src/command-line/release/utils/release-graph';
import { ReleaseVersion } from 'nx/src/command-line/release/utils/shared';
import { FsTree } from 'nx/src/generators/tree';
import { createProjectFileMapUsingProjectGraph } from 'nx/src/project-graph/file-map-utils';
import { printed, runtime, ToolError } from '#/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Services = ManagedRuntime.ManagedRuntime.Services<typeof runtime> | Scope.Scope;

// --- [MODELS] --------------------------------------------------------------------------

const Dry = Schema.Struct({ dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }) });
const Locked = Schema.Struct({ lock: Schema.String });
const Unlocked = Schema.Struct({ skipLockFileUpdate: Schema.optionalKey(Schema.Boolean) });
const Stated = Schema.Struct({ manifest: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const dried = ({ dryRun }: typeof Dry.Type) => Effect.map(Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), (stated) => dryRun === true || stated);

const written = Effect.fnUntraced(function* (context: ExecutorContext, start: string) {
    const path = yield* Path.Path;
    const project = yield* Effect.fromNullishOr(context.projectName);
    const node = yield* Effect.fromNullishOr(context.projectGraph.nodes[project]);
    const targets = node.data.targets ?? {};
    const running = Record.fromIterableWith(
        Object.values(context.taskGraph?.tasks ?? {}).filter(({ target }) => target.project === project),
        ({ target }) => [target.target, target.configuration],
    );
    const reached = (names: ReadonlySet<string>): ReadonlySet<string> => {
        const next = new Set([
            ...names,
            ...[...names]
                .flatMap((name) => (name === start || (targets[name]?.outputs ?? []).length === 0 ? (getDependencyConfigs({ project, target: name }, {}, context.projectGraph, Record.keys(targets)) ?? []) : []))
                .filter(({ projects }) => projects?.includes(project) === true)
                .map(({ target }) => target),
        ]);
        return next.size === names.size ? names : reached(next);
    };
    const selected = [...reached(new Set([start]))].filter((target) => target !== start && target in targets);
    const outputs = selected
        .filter((target) => targets[target]?.outputs !== undefined)
        .flatMap((target) => {
            const configuration = running[target] ?? targets[target]?.defaultConfiguration;
            return getOutputsForTargetAndConfiguration({ project, target, ...(configuration === undefined ? {} : { configuration }) }, {}, node);
        });
    const listed = yield* Stream.runCollect(
        Stream.fromAsyncIterable(
            glob(
                outputs.filter((output) => !output.startsWith('!')).flatMap((output) => [output, path.join(output, '**')]),
                { cwd: context.root, exclude: outputs.filter((output) => output.startsWith('!')).map((output) => output.slice(1)), withFileTypes: true },
            ),
            ToolError.of(project, 'cannot list its build outputs'),
        ),
    );
    return { project, node, selected, outputs, files: Array.dedupe(listed.filter((dirent) => dirent.isFile()).map((dirent) => path.resolve(context.root, dirent.parentPath, dirent.name))).toSorted() };
});

const packaged = Effect.fnUntraced(function* (context: ExecutorContext) {
    const [path, target] = yield* Effect.all([Path.Path, Effect.fromNullishOr(context.targetName)]);
    const { project, files, outputs } = yield* written(context, target);
    return yield* Array.match(files, {
        onEmpty: () => ToolError.make({ subject: project, detail: `no file sits under ${outputs.join(', ')}, the outputs ${target} depends on; run its dependencies first` }),
        onNonEmpty: (found) => Effect.succeed(Array.map(found, (file) => path.relative(context.root, file))),
    });
});

const tagged = Effect.fnUntraced(function* (context: ExecutorContext, project: string) {
    const tree = new FsTree(context.root, context.isVerbose);
    const owned = yield* Effect.promise(() => createProjectFileMapUsingProjectGraph(context.projectGraph));
    const nxReleaseConfig = yield* Effect.flatMap(
        Effect.promise(() => createNxReleaseConfig(context.projectGraph, owned, context.nxJsonConfiguration?.release)),
        (resolved) => Effect.fromNullishOr(resolved.nxReleaseConfig),
    );
    const graph = yield* Effect.promise(() => createReleaseGraph({ tree, projectGraph: context.projectGraph, nxReleaseConfig, filters: {}, firstRelease: false, verbose: context.isVerbose, skipVersionResolution: true }));
    const [{ name, releaseTag }, actions] = yield* Effect.all([Effect.fromNullishOr(graph.projectToReleaseGroup.get(project)), Effect.fromNullishOr(graph.projectsToVersionActions.get(project))]);
    const { currentVersion: version } = yield* Effect.flatMap(
        Effect.promise(() => actions.readCurrentVersionFromSourceManifest(tree)),
        Effect.fromNullishOr,
    );
    return new ReleaseVersion({ version, releaseTagPattern: releaseTag.pattern, projectName: project, releaseGroupName: name }).gitTag;
});

const relocked =
    (lock: string, command: Array.NonEmptyReadonlyArray<string>): AfterAllProjectsVersioned =>
    (cwd, { dryRun, rootVersionActionsOptions }) =>
        runtime.runPromise(
            Effect.gen(function* () {
                const [path, fs, { skipLockFileUpdate }] = yield* Effect.all([Path.Path, FileSystem.FileSystem, Schema.decodeEffect(Unlocked)(rootVersionActionsOptions ?? {})]);
                const locks =
                    dryRun === true || skipLockFileUpdate === true
                        ? []
                        : Array.dedupe(
                              Object.values((yield* Effect.promise(() => createProjectGraphAsync())).nodes).flatMap((node) =>
                                  Schema.decodeUnknownOption(Locked)(node.data.release?.version?.versionActionsOptions).pipe(
                                      Option.map((held) => held.lock),
                                      Option.filter((held) => path.basename(held) === lock),
                                      Option.toArray,
                                  ),
                              ),
                          );
                const changed = yield* Effect.filter(locks, (held) =>
                    Effect.zipWith(fs.readFileString(path.join(cwd, held)), Effect.andThen(printed(Array.headNonEmpty(command), Array.tailNonEmpty(command), { cwd: path.join(cwd, path.dirname(held)) }), fs.readFileString(path.join(cwd, held))), (before, after) => before !== after),
                );
                return { changedFiles: changed, deletedFiles: [] };
            }),
        );

const edited = Effect.fnUntraced(function* ({ projectGraphNode: { name, data }, validManifestFilenames }: Manifested, tree: Tree, edit: (text: string, at: string) => Effect.Effect<string, Error, Services>) {
    const file = yield* Effect.mapError(Effect.fromOption(Array.head(validManifestFilenames)), ToolError.of(name, 'names no manifest in its versionActionsOptions'));
    const at = joinPathFragments(data.root, file);
    const text = yield* Effect.mapError(Effect.fromNullishOr(tree.read(at, 'utf-8')), ToolError.of(at, 'is missing'));
    return { at, result: yield* edit(text, at) };
});

// --- [SERVICES] ------------------------------------------------------------------------

abstract class Manifested extends VersionActions {
    override readonly excludeManifestsFromFormatting = true;

    abstract stated(text: string, at: string): Effect.Effect<string, Error, Services>;

    abstract restated(text: string, version: string, at: string): Effect.Effect<string, Error, Services>;

    override get validManifestFilenames(): string[] {
        return Option.toArray(Option.map(Schema.decodeUnknownOption(Stated)(this.finalConfigForProject.versionActionsOptions), ({ manifest }) => manifest));
    }

    override readCurrentVersionFromSourceManifest(tree: Tree) {
        return runtime.runPromise(
            Effect.scoped(
                Effect.map(
                    edited(this, tree, (text, at) => this.stated(text, at)),
                    ({ at, result }) => ({ currentVersion: result, manifestPath: at }),
                ),
            ),
        );
    }

    override readCurrentVersionFromRegistry() {
        return runtime.runPromise(Effect.succeed(null));
    }

    override readCurrentVersionOfDependency() {
        return runtime.runPromise(Effect.succeed({ currentVersion: null, dependencyCollection: null }));
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        return runtime.runPromise(
            Effect.scoped(
                Effect.map(
                    edited(this, tree, (text, at) => this.restated(text, newVersion, at)),
                    ({ at, result }) => {
                        tree.write(at, result);
                        return [`New version ${newVersion} written to manifest: ${at}`];
                    },
                ),
            ),
        );
    }

    override updateProjectDependencies() {
        return runtime.runPromise(Effect.succeed(Array.empty<string>()));
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const executor = <A, E>(options: Schema.Decoder<A>, run: (options: A, context: ExecutorContext, project: string) => Effect.Effect<boolean, E, Services>) =>
    Object.assign(
        (raw: unknown, context: ExecutorContext) =>
            runtime.runPromise(
                Effect.scoped(
                    Effect.gen(function* () {
                        const [decoded, project] = yield* Effect.all([Schema.decodeUnknownEffect(options)(raw), Effect.fromNullishOr(context.projectName)]);
                        return { success: yield* run(decoded, context, project) };
                    }),
                ),
            ),
        { options },
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Dry, dried, executor, Manifested, packaged, relocked, tagged, written };
