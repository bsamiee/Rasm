import { parse, patch } from '@decimalturn/toml-patch';
import { type ProjectGraph, readCachedProjectGraph, type Tree } from '@nx/devkit';
import { printed, runtime, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, Option, Path, Record, Schema, String } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { type AfterAllProjectsVersioned, VersionActions } from 'nx/release';
import { sortReversed } from 'verkit';
import { releases } from '#/executors/release-publish/executor';
import { LOCK, MANIFEST } from '#/plugins/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TABLES = ['dependencies', 'dev-dependencies', 'build-dependencies'];
const OPERATOR = /^[\^=~<>]+/u;

// --- [MODELS] --------------------------------------------------------------------------

const Table = Schema.Record(Schema.String, Schema.Unknown);
const Requirement = Schema.Union([Schema.String, Schema.Struct({ version: Schema.optionalKey(Schema.String), package: Schema.optionalKey(Schema.String), workspace: Schema.optionalKey(Schema.Boolean) })]);
const Dependencies = Schema.Record(Schema.String, Requirement);
const Manifest = Schema.Struct({
    package: Schema.optionalKey(Schema.Struct({ name: Schema.String, version: Schema.optionalKey(Schema.Union([Schema.String, Schema.Struct({ workspace: Schema.Literal(true) })])) })),
    workspace: Schema.optionalKey(Schema.Struct({ package: Schema.optionalKey(Schema.Struct({ version: Schema.optionalKey(Schema.String) })) })),
});
const Located = Schema.fromJsonString(Schema.Struct({ root: Schema.String }));
const Root = Schema.Struct({ skipLockFileUpdate: Schema.optionalKey(Schema.Boolean) });
const Options = Schema.Struct({ lock: Schema.String, registry: Schema.optionalKey(Schema.String) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const table = (value: unknown): Readonly<Record<string, unknown>> => Option.getOrElse(Schema.decodeUnknownOption(Table)(value), () => ({}));

const read = Effect.fnUntraced(function* (tree: Tree, file: string) {
    const text = yield* Effect.fromOption(Option.fromNullishOr(tree.read(file, 'utf-8')), () => ToolError.make({ subject: file, detail: 'is missing' }));
    const raw = table(yield* Effect.try({ try: (): unknown => parse(text), catch: ToolError.of(file, 'does not parse as TOML') }));
    return { text, raw, manifest: yield* Schema.decodeEffect(Manifest)(raw) };
});

const located = (tree: Tree, manifest: string) => Effect.map(Effect.all([Path.Path, Effect.flatMap(printed('cargo', ['locate-project', '--workspace', '--manifest-path', manifest], { cwd: tree.root }), Schema.decodeEffect(Located))]), ([path, { root }]) => path.relative(tree.root, root));

const versioned = Effect.fnUntraced(function* (tree: Tree, manifest: string) {
    const own = yield* read(tree, manifest);
    const version = own.manifest.package?.version;
    const inherited = typeof version === 'object';
    const file = inherited ? yield* located(tree, manifest) : manifest;
    const owner = file === manifest ? own : yield* read(tree, file);
    return { ...owner, file, inherited, field: inherited ? ['workspace', 'package', 'version'] : ['package', 'version'], current: (inherited ? owner.manifest.workspace?.package?.version : version) ?? '0.0.0' };
});

const assigned = (value: Record<string, unknown>, [key, ...rest]: readonly string[], next: unknown): Record<string, unknown> => (key === undefined ? value : { ...value, [key]: rest.length === 0 ? next : assigned(table(value[key]), rest, next) });

const bumped = (requirement: string, version: string) => (OPERATOR.test(version) ? version : Option.match(String.match(OPERATOR)(requirement), { onNone: () => version, onSome: ([operator]) => `${operator}${version}` }));

const requirements = (raw: Readonly<Record<string, unknown>>, crates: ReadonlySet<string>) =>
    [...TABLES.map((kind) => [kind]), ...Record.keys(table(Option.getOrUndefined(Record.get(raw, 'target')))).flatMap((platform) => TABLES.map((kind) => ['target', platform, kind])), ['workspace', 'dependencies']].flatMap((at) =>
        Record.collect(
            Option.getOrElse(Schema.decodeUnknownOption(Dependencies)(at.reduce<unknown>((value, key) => table(value)[key], raw)), () => ({})),
            (key, requirement) => (typeof requirement === 'string' ? { crate: key, at, field: [...at, key], current: requirement, inherited: false } : { crate: requirement.package ?? key, at, field: [...at, key, 'version'], current: requirement.version, inherited: requirement.workspace === true }),
        ).filter(({ crate }) => crates.has(crate)),
    );

// --- [SERVICES] ------------------------------------------------------------------------

class CargoVersionActions extends VersionActions {
    override readonly validManifestFilenames = [MANIFEST];
    override readonly excludeManifestsFromFormatting = true;

    override validate(tree: Tree) {
        const [{ name }, { name: group, projectsRelationship }, manifests] = [this.projectGraphNode, this.releaseGroup, this.manifestsToUpdate];
        return super
            .validate(tree)
            .then(() =>
                runtime.runPromise(
                    Effect.forEach(
                        manifests,
                        ({ manifestPath }) =>
                            Effect.flatMap(versioned(tree, manifestPath), ({ file, inherited }) =>
                                inherited && projectsRelationship === 'independent' ? ToolError.make({ subject: manifestPath, detail: `inherits the version in ${file}, which every inheriting member shares, so ${name} needs a fixed release group in place of the independent group ${group}` }) : Effect.void,
                            ),
                        { discard: true },
                    ),
                ),
            );
    }

    override readDependencies(_tree: Tree, projectGraph: ProjectGraph) {
        return Promise.resolve((projectGraph.dependencies[this.projectGraphNode.name] ?? []).filter(({ target }) => target in projectGraph.nodes));
    }

    readCurrentVersionFromSourceManifest(tree: Tree) {
        return runtime.runPromise(Effect.map(versioned(tree, `${this.projectGraphNode.data.root}/${MANIFEST}`), ({ file, current }) => ({ manifestPath: file, currentVersion: current })));
    }

    readCurrentVersionFromRegistry(tree: Tree) {
        const [{ name, data }, options] = [this.projectGraphNode, this.finalConfigForProject.versionActionsOptions];
        return runtime.runPromise(
            Effect.gen(function* () {
                const [{ registry }, { manifest }] = yield* Effect.all([Schema.decodeUnknownEffect(Options)(options), read(tree, `${data.root}/${MANIFEST}`)]);
                const published = yield* Effect.transposeOption(
                    Option.map(
                        Option.liftPredicate(manifest.package?.name ?? name, () => registry === undefined),
                        releases,
                    ),
                );
                return Option.getOrNull(Option.map(published, ({ url, published: versions }) => ({ currentVersion: Option.getOrNull(Array.head(sortReversed(versions.filter(({ yanked }) => yanked !== true).map(({ vers }) => vers)))), logText: `crates.io index ${url}` })));
            }).pipe(Effect.provide(FetchHttpClient.layer)),
        );
    }

    readCurrentVersionOfDependency(tree: Tree, projectGraph: ProjectGraph, dependencyProjectName: string) {
        const manifest = `${this.projectGraphNode.data.root}/${MANIFEST}`;
        const dependency = projectGraph.nodes[dependencyProjectName]?.name ?? dependencyProjectName;
        return runtime.runPromise(
            Effect.gen(function* () {
                const crates = new Set([dependency]);
                const declared = Array.head(requirements((yield* read(tree, manifest)).raw, crates));
                const source = Option.exists(declared, ({ inherited }) => inherited) ? Array.head(requirements((yield* read(tree, yield* located(tree, manifest))).raw, crates)) : declared;
                return {
                    currentVersion: Option.getOrNull(Option.flatMap(source, ({ current }) => Option.fromUndefinedOr(current))),
                    dependencyCollection: Option.getOrNull(Option.map(declared, ({ at }) => at.join('.'))),
                };
            }),
        );
    }

    updateProjectVersion(tree: Tree, newVersion: string) {
        return runtime.runPromise(
            Effect.forEach(this.manifestsToUpdate, ({ manifestPath }) =>
                Effect.map(versioned(tree, manifestPath), ({ file, text, raw, field }) => {
                    tree.write(file, patch(text, assigned(raw, field, newVersion)));
                    return `New version ${newVersion} written to ${file}`;
                }),
            ),
        );
    }

    updateProjectDependencies(tree: Tree, projectGraph: ProjectGraph, dependenciesToUpdate: Record<string, string>) {
        const manifests = this.manifestsToUpdate;
        const versions = new Map(Record.collect(dependenciesToUpdate, (project, version) => [projectGraph.nodes[project]?.name ?? project, version] as const));
        return runtime.runPromise(
            Effect.gen(function* () {
                const crates = new Set(versions.keys());
                const own = yield* Effect.forEach(manifests, ({ manifestPath }) => Effect.map(read(tree, manifestPath), (manifest) => ({ file: manifestPath, ...manifest })));
                const roots = yield* Effect.forEach(
                    own.filter(({ raw }) => requirements(raw, crates).some(({ inherited }) => inherited)),
                    ({ file }) => located(tree, file),
                );
                const shared = yield* Effect.forEach(
                    Array.difference(
                        Array.dedupe(roots),
                        own.map(({ file }) => file),
                    ),
                    (file) => Effect.map(read(tree, file), (manifest) => ({ file, ...manifest })),
                );
                const rows = [...own, ...shared].flatMap((manifest) => requirements(manifest.raw, crates).map((requirement) => ({ ...manifest, ...requirement })));
                const edits = Array.getSomes(rows.map(({ crate, current, ...row }) => Option.map(Option.zipWith(Option.fromUndefinedOr(current), Option.fromUndefinedOr(versions.get(crate)), bumped), (value) => ({ ...row, value }))));
                return Record.collect(
                    Array.groupBy(edits, ({ file }) => file),
                    (file, [first, ...rest]) => {
                        tree.write(
                            file,
                            patch(
                                first.text,
                                [first, ...rest].reduce((value, edit) => assigned(value, edit.field, edit.value), first.raw),
                            ),
                        );
                        return `Updated ${rest.length + 1} requirements in ${file}`;
                    },
                );
            }),
        );
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const afterAllProjectsVersioned: AfterAllProjectsVersioned = (cwd, { dryRun, rootVersionActionsOptions }) =>
    runtime.runPromise(
        Effect.gen(function* () {
            const [path, fs, options] = yield* Effect.all([Path.Path, FileSystem.FileSystem, Schema.decodeEffect(Root)(rootVersionActionsOptions ?? {})]);
            const locks =
                dryRun === true || options.skipLockFileUpdate === true
                    ? []
                    : Array.dedupe(
                          Object.values(readCachedProjectGraph().nodes).flatMap((node) =>
                              Schema.decodeUnknownOption(Options)(node.data.release?.version?.versionActionsOptions).pipe(
                                  Option.map(({ lock }) => lock),
                                  Option.filter((lock) => path.basename(lock) === LOCK),
                                  Option.toArray,
                              ),
                          ),
                      );
            const changed = yield* Effect.filter(locks, (lock) => Effect.zipWith(fs.readFileString(path.join(cwd, lock)), Effect.andThen(printed('cargo', ['update', '--workspace'], { cwd: path.join(cwd, path.dirname(lock)) }), fs.readFileString(path.join(cwd, lock))), (before, after) => before !== after));
            return { changedFiles: changed, deletedFiles: [] };
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { afterAllProjectsVersioned };
export default CargoVersionActions;
