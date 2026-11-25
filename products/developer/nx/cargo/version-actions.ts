import { dirname, join, posix } from 'node:path';
import { patch } from '@decimalturn/toml-patch';
import { type ProjectGraph, readCachedProjectGraph, type Tree } from '@nx/devkit';
import { spawned } from '@rasm/nx';
import { Array, Cause, Effect, FileSystem, flow, Option, Record, Result, Schema, String, Struct } from 'effect';
import { type AfterAllProjectsVersioned, VersionActions } from 'nx/release';
import { ACTIONS, MANIFEST, parsed, run, Table } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TABLES = ['dependencies', 'dev-dependencies', 'build-dependencies'];

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ lock: Schema.String, registry: Schema.optionalKey(Schema.String) });
const Unlocked = Schema.Struct({ skipLockFileUpdate: Schema.optionalKey(Schema.Boolean) });
const Requirement = Schema.Union([Schema.String, Schema.Struct({ version: Schema.optionalKey(Schema.String), package: Schema.optionalKey(Schema.String), workspace: Schema.optionalKey(Schema.Boolean) })]);
const Dependencies = Schema.Record(Schema.String, Requirement);
const Manifest = Schema.Struct({
    package: Schema.optionalKey(Schema.Struct({ name: Schema.String, version: Schema.optionalKey(Schema.Union([Schema.String, Schema.Struct({ workspace: Schema.Literal(true) })])) })),
    workspace: Schema.optionalKey(Schema.Struct({ package: Schema.optionalKey(Schema.Struct({ version: Schema.optionalKey(Schema.String) })) })),
});
const Found = Schema.Record(Schema.String, Schema.String);

// --- [OPERATIONS] ----------------------------------------------------------------------

const table = (value: unknown): Readonly<Record<string, unknown>> => Option.getOrElse(Schema.decodeUnknownOption(Table)(value), Record.empty);

const read = Effect.fnUntraced(function* (tree: Tree, file: string) {
    const text = yield* Effect.mapError(Effect.fromNullishOr(tree.read(file, 'utf-8')), () => new Cause.NoSuchElementError(`${file} does not exist`));
    const raw = table(yield* parsed(text, file));
    return { file, text, raw, manifest: yield* Schema.decodeEffect(Manifest)(raw) };
});

const versioned = Effect.fnUntraced(function* (tree: Tree, manifest: string, shared: string) {
    const own = yield* read(tree, manifest);
    const version = own.manifest.package?.version;
    const inherited = typeof version === 'object';
    const owner = inherited && shared !== manifest ? yield* read(tree, shared) : own;
    return { ...owner, name: own.manifest.package?.name, inherited, field: inherited ? ['workspace', 'package', 'version'] : ['package', 'version'], current: (inherited ? owner.manifest.workspace?.package?.version : version) ?? '0.0.0' };
});

const crated = (tree: Tree, projectGraph: ProjectGraph, project: string) => {
    const manifest = `${projectGraph.nodes[project]?.data.root}/${MANIFEST}`;
    return tree.exists(manifest) ? Effect.map(read(tree, manifest), (held) => held.manifest.package?.name ?? project) : Effect.succeed(project);
};

const assigned = (value: Record<string, unknown>, [key, ...rest]: readonly string[], next: unknown): Record<string, unknown> => (key === undefined ? value : { ...value, [key]: rest.length === 0 ? next : assigned(table(value[key]), rest, next) });

const patched = (tree: Tree, edits: readonly { readonly file: string; readonly text: string; readonly raw: Readonly<Record<string, unknown>>; readonly field: readonly string[]; readonly value: string }[]) =>
    Record.collect(Array.groupBy(edits, Struct.get('file')), (file, group) => {
        const { text, raw } = Array.headNonEmpty(group);
        const value = group.reduce((held, edit) => assigned(held, edit.field, edit.value), raw);
        tree.write(file, patch(text, value));
        return `Updated ${group.length} fields in ${file}`;
    });

const requirements = (raw: Readonly<Record<string, unknown>>, crates: ReadonlySet<string>) =>
    [...TABLES.map((kind) => [kind]), ...Record.keys(table(raw['target'])).flatMap((platform) => TABLES.map((kind) => ['target', platform, kind])), ['workspace', 'dependencies']].flatMap((at) =>
        Record.collect(Option.getOrElse(Schema.decodeUnknownOption(Dependencies)(at.reduce<unknown>((value, key) => table(value)[key], raw)), Record.empty), (key, requirement) =>
            typeof requirement === 'string' ? { crate: key, at, field: [...at, key], current: requirement, inherited: false } : { crate: requirement.package ?? key, at, field: [...at, key, 'version'], current: requirement.version, inherited: requirement.workspace === true },
        ).filter(({ crate }) => crates.has(crate)),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

class CargoVersionActions extends VersionActions {
    override readonly validManifestFilenames = [MANIFEST];
    override readonly excludeManifestsFromFormatting = true;

    get stated() {
        const manifest = `${this.projectGraphNode.data.root}/${MANIFEST}`;
        return Effect.map(Schema.decodeUnknownEffect(Options)(this.finalConfigForProject.versionActionsOptions), ({ lock, registry }) => ({ manifest, registry, shared: posix.join(posix.dirname(lock), MANIFEST) }));
    }

    override validate(tree: Tree) {
        const checked = Effect.gen({ self: this }, function* () {
            const { shared } = yield* this.stated;
            yield* Effect.forEach(
                this.manifestsToUpdate,
                ({ manifestPath }) =>
                    Effect.flatMap(versioned(tree, manifestPath, shared), ({ file, inherited }) =>
                        inherited && this.releaseGroup['projectsRelationship'] === 'independent'
                            ? new Cause.UnknownError(manifestPath, `${manifestPath}: inherits the version in ${file}, which every inheriting member shares, so ${this.projectGraphNode.name} needs a fixed release group in place of the independent group ${this.releaseGroup.name}`)
                            : Effect.void,
                    ),
                { discard: true },
            );
        });
        return super.validate(tree).then(() => run(checked));
    }

    readCurrentVersionFromSourceManifest(tree: Tree) {
        return this.stated.pipe(
            Effect.flatMap(({ manifest, shared }) => versioned(tree, manifest, shared)),
            Effect.map(({ file, current }) => ({ manifestPath: file, currentVersion: current })),
            run,
        );
    }

    readCurrentVersionFromRegistry(tree: Tree) {
        return run(
            Effect.gen({ self: this }, function* () {
                const { manifest, registry } = yield* this.stated;
                const { manifest: held } = yield* read(tree, manifest);
                const crate = yield* Effect.mapError(Effect.fromNullishOr(held.package?.name), () => new Cause.NoSuchElementError(`${manifest} states no package`));
                const search = ['cargo', 'search', crate, '--limit', '1', '--color', 'never', ...Array.fromNullishOr(registry).flatMap((named) => ['--registry', named])] as const;
                const shown = search.join(' ');
                const stdout = yield* spawned(search, { cwd: tree.root });
                const found = yield* Effect.flatMap(parsed(Array.headNonEmpty(String.split(stdout, '\n')), shown), flow(Schema.decodeUnknownEffect(Found), Effect.map(Record.get(crate))));
                const currentVersion = yield* Effect.fromOption(found, () => new Cause.NoSuchElementError(`${shown} prints no row for ${crate}`));
                return { currentVersion, logText: shown };
            }),
        );
    }

    readCurrentVersionOfDependency(tree: Tree, projectGraph: ProjectGraph, dependencyProjectName: string) {
        return run(
            Effect.gen({ self: this }, function* () {
                const { manifest, shared } = yield* this.stated;
                const crates = new Set([yield* crated(tree, projectGraph, dependencyProjectName)]);
                const declared = Array.head(requirements((yield* read(tree, manifest)).raw, crates));
                const source = Option.exists(declared, ({ inherited }) => inherited) ? Array.head(requirements((yield* read(tree, shared)).raw, crates)) : declared;
                const current = Option.flatMap(source, (requirement) => Option.fromUndefinedOr(requirement.current));
                return { currentVersion: Option.getOrNull(current), dependencyCollection: current.pipe(Option.andThen(Option.map(declared, ({ at }) => at.join('.'))), Option.getOrNull) };
            }),
        );
    }

    updateProjectVersion(tree: Tree, newVersion: string) {
        const manifests = this.manifestsToUpdate;
        return run(
            Effect.flatMap(this.stated, ({ shared }) =>
                Effect.forEach(
                    manifests,
                    Effect.fnUntraced(function* ({ manifestPath }) {
                        const owner = yield* versioned(tree, manifestPath, shared);
                        const held = owner.file === shared ? owner : yield* read(tree, shared);
                        const rows = requirements(held.raw, new Set(Array.fromNullishOr(owner.name))).filter(({ at, current }) => at[0] === 'workspace' && current !== undefined);
                        patched(tree, [{ ...owner, value: newVersion }, ...rows.map(({ field }) => ({ ...held, field, value: newVersion }))]);
                        return `New version ${newVersion} written to ${owner.file}`;
                    }),
                ),
            ),
        );
    }

    updateProjectDependencies(tree: Tree, projectGraph: ProjectGraph, dependenciesToUpdate: Record<string, string>) {
        return run(
            Effect.gen({ self: this }, function* () {
                const versions = new Map(yield* Effect.forEach(Record.toEntries(dependenciesToUpdate), ([project, version]) => Effect.map(crated(tree, projectGraph, project), (crate) => [crate, version] as const)));
                const crates = new Set(versions.keys());
                const own = yield* Effect.forEach(this.manifestsToUpdate, ({ manifestPath }) => read(tree, manifestPath));
                const rows = own.flatMap((held) => requirements(held.raw, crates).map((requirement) => ({ ...held, ...requirement })));
                const edits = Array.getSomes(rows.map(({ crate, current, ...row }) => Option.map(Option.all([Option.fromUndefinedOr(current), Option.fromUndefinedOr(versions.get(crate))]), ([, value]) => ({ ...row, value }))));
                return patched(tree, edits);
            }),
        );
    }
}

const afterAllProjectsVersioned = Effect.fnUntraced(function* (cwd: string, { dryRun, rootVersionActionsOptions }: Parameters<AfterAllProjectsVersioned>[1]) {
    const [fs, { skipLockFileUpdate }] = yield* Effect.all([FileSystem.FileSystem, Schema.decodeEffect(Unlocked)(rootVersionActionsOptions ?? {})]);
    const owned = dryRun === true || skipLockFileUpdate === true ? [] : Object.values((yield* Effect.sync(readCachedProjectGraph)).nodes).filter(({ data }) => data.release?.version?.versionActions === ACTIONS);
    const locks = Array.dedupe(Array.filterMap(owned, ({ data }) => Result.map(Schema.decodeUnknownResult(Options)(data.release?.version?.versionActionsOptions), Struct.get('lock'))));
    const changedFiles = yield* Effect.filter(locks, (lock) => Effect.zipWith(fs.readFileString(join(cwd, lock)), Effect.andThen(spawned(['cargo', 'update', '--workspace'], { cwd: join(cwd, dirname(lock)) }), fs.readFileString(join(cwd, lock))), (before, after) => before !== after));
    return { changedFiles, deletedFiles: [] };
}, run) satisfies AfterAllProjectsVersioned;

// --- [EXPORTS] -------------------------------------------------------------------------

export { afterAllProjectsVersioned };
export default CargoVersionActions;
