import { glob } from 'node:fs/promises';
import { createProjectFileMapUsingProjectGraph, createProjectGraphAsync, type ExecutorContext, getOutputsForTargetAndConfiguration, type Tree } from '@nx/devkit';
import { FsTree, getDependencyConfigs } from '@nx/devkit/internal';
import { Array, Config, Effect, FileSystem, type ManagedRuntime, Option, Path, Record, Schema, type Scope, Stream } from 'effect';
import { type AfterAllProjectsVersioned, VersionActions } from 'nx/release';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { createReleaseGraph } from 'nx/src/command-line/release/utils/release-graph';
import { ReleaseVersion } from 'nx/src/command-line/release/utils/shared';
import { printed, promised, type runtime, ToolError } from '#/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Services = ManagedRuntime.ManagedRuntime.Services<typeof runtime>;

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
    const failed = ToolError.of(project, 'has no git tag: nx.json release does not resolve');
    const owned = yield* Effect.tryPromise({ try: () => createProjectFileMapUsingProjectGraph(context.projectGraph), catch: failed });
    const { error, nxReleaseConfig } = yield* Effect.tryPromise({ try: () => createNxReleaseConfig(context.projectGraph, owned, context.nxJsonConfiguration?.release), catch: failed });
    const resolved = yield* Effect.mapError(Effect.fromNullishOr(nxReleaseConfig), () => failed(error?.code));
    const graph = yield* Effect.tryPromise({ try: () => createReleaseGraph({ tree, projectGraph: context.projectGraph, nxReleaseConfig: resolved, filters: {}, firstRelease: false, verbose: context.isVerbose, skipVersionResolution: true }), catch: failed });
    const [{ name, releaseTag }, actions] = yield* Effect.mapError(Effect.all([Effect.fromNullishOr(graph.projectToReleaseGroup.get(project)), Effect.fromNullishOr(graph.projectsToVersionActions.get(project))]), ToolError.of(project, 'has no git tag: no release group of nx.json release holds it'));
    const { currentVersion } = yield* Effect.tryPromise({ try: () => actions.readCurrentVersionFromSourceManifest(tree), catch: ToolError.of(project, 'has no git tag: its manifest states no version') }).pipe(Effect.flatMap(Effect.fromNullishOr));
    return new ReleaseVersion({ version: currentVersion, releaseTagPattern: releaseTag.pattern, projectName: project, releaseGroupName: name }).gitTag;
});

const relocked =
    (lock: string, command: Array.NonEmptyReadonlyArray<string>): AfterAllProjectsVersioned =>
    (cwd, { dryRun, rootVersionActionsOptions }) =>
        promised(
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

const edited = (tree: Tree, at: string, edit: (text: string) => Effect.Effect<string, Error, Services>) => Effect.flatMap(Effect.mapError(Effect.fromNullishOr(tree.read(at, 'utf-8')), ToolError.of(at, 'is missing')), edit);

// --- [SERVICES] ------------------------------------------------------------------------

abstract class Manifested extends VersionActions {
    override readonly excludeManifestsFromFormatting = true;

    abstract stated(text: string, at: string): Effect.Effect<string, Error, Services>;

    abstract restated(text: string, version: string, at: string): Effect.Effect<string, Error, Services>;

    override get validManifestFilenames(): string[] {
        return Option.toArray(Option.map(Schema.decodeUnknownOption(Stated)(this.finalConfigForProject.versionActionsOptions), ({ manifest }) => manifest));
    }

    override readCurrentVersionFromSourceManifest(tree: Tree) {
        const reading = (text: string, at: string) => this.stated(text, at);
        const [manifests, name] = [this.manifestsToUpdate, this.projectGraphNode.name];
        return promised(
            Effect.gen(function* () {
                const { manifestPath } = yield* Effect.mapError(Effect.fromOption(Array.head(manifests)), ToolError.of(name, 'names no manifest in its versionActionsOptions'));
                return { currentVersion: yield* edited(tree, manifestPath, (text) => reading(text, manifestPath)), manifestPath };
            }),
        );
    }

    override readCurrentVersionFromRegistry(): ReturnType<VersionActions['readCurrentVersionFromRegistry']> {
        return promised(Effect.succeed(null));
    }

    override readCurrentVersionOfDependency(..._: Parameters<VersionActions['readCurrentVersionOfDependency']>): ReturnType<VersionActions['readCurrentVersionOfDependency']> {
        return promised(Effect.succeed({ currentVersion: null, dependencyCollection: null }));
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        return promised(
            Effect.forEach(this.manifestsToUpdate, ({ manifestPath }) =>
                Effect.map(
                    edited(tree, manifestPath, (text) => this.restated(text, newVersion, manifestPath)),
                    (text) => {
                        tree.write(manifestPath, text);
                        return `New version ${newVersion} written to manifest: ${manifestPath}`;
                    },
                ),
            ),
        );
    }

    override updateProjectDependencies(..._: Parameters<VersionActions['updateProjectDependencies']>): ReturnType<VersionActions['updateProjectDependencies']> {
        return promised(Effect.succeed(Array.empty<string>()));
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const executor = <A, E>(options: Schema.Decoder<A>, run: (options: A, context: ExecutorContext, project: string) => Effect.Effect<boolean, E, Services | Scope.Scope>) =>
    Object.assign(
        (raw: unknown, context: ExecutorContext) =>
            promised(
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
