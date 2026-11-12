import { glob } from 'node:fs/promises';
import { createProjectGraphAsync, type ExecutorContext, getOutputsForTargetAndConfiguration, joinPathFragments, type Tree } from '@nx/devkit';
import { getDependencyConfigs } from '@nx/devkit/internal';
import { Array, Config, Effect, FileSystem, type ManagedRuntime, Match, Option, Path, Record, Schema, type Scope, Stream, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import { type AfterAllProjectsVersioned, VersionActions } from 'nx/release';
import { createNxReleaseConfig } from 'nx/src/command-line/release/config/config';
import { ReleaseVersion } from 'nx/src/command-line/release/utils/shared';
import { createProjectFileMapUsingProjectGraph } from 'nx/src/project-graph/file-map-utils';
import { printed, runtime, ToolError } from '#/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Services = ManagedRuntime.ManagedRuntime.Services<typeof runtime> | Scope.Scope;

// --- [MODELS] --------------------------------------------------------------------------

const Dry = Schema.Struct({ dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each change and makes none, as NX_DRY_RUN=true does' }) });
const Released = Dry.mapFields(
    Struct.assign({
        nxReleaseVersionData: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ newVersion: Schema.NullOr(Schema.String) }))).annotate({
            description: 'Versions nx release set in this run, a project it left unversioned is skipped',
        }),
        registry: Schema.optionalKey(Schema.String).annotate({ description: 'Registry nx release publish passes as --registry, in the form the tool names one' }),
        firstRelease: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Set by nx release publish --first-release, when the registry holds no earlier version' }),
    }),
);
const Locked = Schema.Struct({ lock: Schema.String });
const Unlocked = Schema.Struct({ skipLockFileUpdate: Schema.optionalKey(Schema.Boolean) });
const Stated = Schema.Struct({ manifest: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const dried = ({ dryRun }: typeof Dry.Type) => Effect.map(Config.Boolean('NX_DRY_RUN').pipe(Config.withDefault(false)), (stated) => dryRun === true || stated);

const released = Effect.fnUntraced(function* (options: typeof Released.Type, project: string) {
    const dry = yield* dried(options);
    const unreleased = options.nxReleaseVersionData?.[project]?.newVersion === null;
    yield* unreleased ? Effect.logInfo(`Skipped ${project}, nx release left it unversioned`) : Effect.void;
    return { dry, unreleased };
});

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

const attached = Effect.fnUntraced(function* (context: ExecutorContext, project: string, version: string, files: Array.NonEmptyReadonlyArray<string>, dry: boolean) {
    const path = yield* Path.Path;
    const owned = yield* Effect.promise(() => createProjectFileMapUsingProjectGraph(context.projectGraph));
    const { nxReleaseConfig } = yield* Effect.promise(() => createNxReleaseConfig(context.projectGraph, owned, context.nxJsonConfiguration?.release));
    const [group, { releaseTag }] = yield* Effect.mapError(Effect.fromOption(Record.findFirst(nxReleaseConfig?.groups ?? {}, ({ projects }) => projects.includes(project))), ToolError.of(project, 'is in no release group of nx.json release'));
    const { gitTag } = new ReleaseVersion({ version, releaseTagPattern: releaseTag.pattern, projectName: project, releaseGroupName: group });
    const listed = yield* Effect.when(printed('gh', ['release', 'view', gitTag, '--json', 'assets', '--jq', '.assets[].name'], { cwd: context.root }), Effect.succeed(!dry));
    const held = Option.match(listed, { onNone: Array.empty<string>, onSome: (names) => names.split('\n') });
    const missing = files.filter((file) => !held.includes(path.basename(file)));
    const upload = ['gh', 'release', 'upload', gitTag, ...missing] as const;
    yield* Match.value({ dry, complete: missing.length === 0 }).pipe(
        Match.when({ complete: true }, () => Effect.logInfo(`Skipped ${files.join(', ')}, GitHub release ${gitTag} holds each`)),
        Match.when({ dry: true }, () => Effect.logInfo(`Would run ${CommandLine.display(upload)}`)),
        Match.orElse(() => Effect.andThen(printed('gh', Array.tailNonEmpty(upload), { cwd: context.root }), Effect.logInfo(`Attached ${files.join(', ')} to GitHub release ${gitTag}`))),
    );
    return { tag: gitTag, uploaded: missing };
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

export { attached, Dry, dried, executor, Manifested, Released, released, relocked, written };
