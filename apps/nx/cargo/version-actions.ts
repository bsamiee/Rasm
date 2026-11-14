import { execFile } from 'node:child_process';
import { basename, dirname, join } from 'node:path';
import { promisify } from 'node:util';
import { parse, patch } from '@decimalturn/toml-patch';
import { createProjectGraphAsync, type ProjectGraph, type Tree } from '@nx/devkit';
import { Array, Effect, FileSystem, flow, Option, Record, Schema } from 'effect';
import { Toml } from 'effect/encoding';
import { type AfterAllProjectsVersioned, VersionActions } from 'nx/release';
import { LOCK, MANIFEST, refused, run } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TABLES = ['dependencies', 'dev-dependencies', 'build-dependencies'];

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ lock: Schema.String, registry: Schema.optionalKey(Schema.String) });
const Unlocked = Schema.Struct({ skipLockFileUpdate: Schema.optionalKey(Schema.Boolean) });
const Table = Schema.Record(Schema.String, Schema.Unknown);
const Requirement = Schema.Union([Schema.String, Schema.Struct({ version: Schema.optionalKey(Schema.String), package: Schema.optionalKey(Schema.String), workspace: Schema.optionalKey(Schema.Boolean) })]);
const Dependencies = Schema.Record(Schema.String, Requirement);
const Manifest = Schema.Struct({
    package: Schema.optionalKey(Schema.Struct({ name: Schema.String, version: Schema.optionalKey(Schema.Union([Schema.String, Schema.Struct({ workspace: Schema.Literal(true) })])) })),
    workspace: Schema.optionalKey(Schema.Struct({ package: Schema.optionalKey(Schema.Struct({ version: Schema.optionalKey(Schema.String) })) })),
});
const Found = Schema.Record(Schema.String, Schema.String);

// --- [OPERATIONS] ----------------------------------------------------------------------

const table = (value: unknown): Readonly<Record<string, unknown>> => Option.getOrElse(Schema.decodeUnknownOption(Table)(value), () => ({}));

const read = Effect.fnUntraced(function* (tree: Tree, file: string) {
    const text = yield* Effect.mapError(Effect.fromNullishOr(tree.read(file, 'utf-8')), refused(file, 'is missing'));
    const raw = table(yield* Effect.try({ try: (): unknown => parse(text), catch: refused(file, 'does not parse as TOML') }));
    return { file, text, raw, manifest: yield* Schema.decodeEffect(Manifest)(raw) };
});

const versioned = Effect.fnUntraced(function* (tree: Tree, manifest: string, shared: string) {
    const own = yield* read(tree, manifest);
    const version = own.manifest.package?.version;
    const inherited = typeof version === 'object';
    const owner = inherited && shared !== manifest ? yield* read(tree, shared) : own;
    return { ...owner, inherited, field: inherited ? ['workspace', 'package', 'version'] : ['package', 'version'], current: (inherited ? owner.manifest.workspace?.package?.version : version) ?? '0.0.0' };
});

const crated = (tree: Tree, projectGraph: ProjectGraph, project: string) => {
    const manifest = `${projectGraph.nodes[project]?.data.root}/${MANIFEST}`;
    return tree.exists(manifest) ? Effect.map(read(tree, manifest), (held) => held.manifest.package?.name ?? project) : Effect.succeed(project);
};

const assigned = (value: Record<string, unknown>, [key, ...rest]: readonly string[], next: unknown): Record<string, unknown> => (key === undefined ? value : { ...value, [key]: rest.length === 0 ? next : assigned(table(value[key]), rest, next) });

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

    get stated() {
        const manifest = `${this.projectGraphNode.data.root}/${MANIFEST}`;
        return Effect.map(Schema.decodeUnknownEffect(Options)(this.finalConfigForProject.versionActionsOptions), ({ lock, registry }) => ({ manifest, registry, shared: join(dirname(lock), MANIFEST) }));
    }

    override validate(tree: Tree) {
        const [{ name }, { name: group, projectsRelationship }, manifests, stated] = [this.projectGraphNode, this.releaseGroup, this.manifestsToUpdate, this.stated];
        const checked = Effect.flatMap(stated, ({ shared }) =>
            Effect.forEach(
                manifests,
                ({ manifestPath }) =>
                    Effect.flatMap(versioned(tree, manifestPath, shared), ({ file, inherited }) =>
                        inherited && projectsRelationship === 'independent' ? Effect.fail(refused(manifestPath, `inherits the version in ${file}, which every inheriting member shares, so ${name} needs a fixed release group in place of the independent group ${group}`)()) : Effect.void,
                    ),
                { discard: true },
            ),
        );
        return super.validate(tree).then(() => run(checked));
    }

    readCurrentVersionFromSourceManifest(tree: Tree) {
        const { stated } = this;
        return run(
            Effect.gen(function* () {
                const { manifest, shared } = yield* stated;
                const { file, current } = yield* versioned(tree, manifest, shared);
                return { manifestPath: file, currentVersion: current };
            }),
        );
    }

    readCurrentVersionFromRegistry(tree: Tree) {
        const { stated } = this;
        return run(
            Effect.gen(function* () {
                const { manifest, registry } = yield* stated;
                const { manifest: held } = yield* read(tree, manifest);
                const crate = yield* Effect.mapError(Effect.fromNullishOr(held.package?.name), refused(manifest, 'names no package'));
                const search = ['search', crate, '--limit', '1', '--color', 'never', ...Array.fromNullishOr(registry).flatMap((named) => ['--registry', named])];
                const { stdout } = yield* Effect.tryPromise({ try: (signal) => promisify(execFile)('cargo', search, { encoding: 'utf8', cwd: tree.root, signal }), catch: refused(crate, 'cargo search does not exit 0') });
                const found = yield* Effect.flatMap(Effect.fromResult(Toml.parse(stdout.slice(0, stdout.indexOf('\n')))), flow(Schema.decodeUnknownEffect(Found), Effect.map(Record.get(crate))));
                const currentVersion = yield* Effect.mapError(Effect.fromOption(found), refused(crate, `is absent from the output of cargo ${search.join(' ')}`));
                return { currentVersion, logText: `cargo ${search.join(' ')}` };
            }),
        );
    }

    readCurrentVersionOfDependency(tree: Tree, projectGraph: ProjectGraph, dependencyProjectName: string) {
        const { stated } = this;
        return run(
            Effect.gen(function* () {
                const { manifest, shared } = yield* stated;
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
                        const { file, text, raw, field } = yield* versioned(tree, manifestPath, shared);
                        tree.write(file, patch(text, assigned(raw, field, newVersion)));
                        const [own, held] = yield* Effect.all([read(tree, manifestPath), read(tree, shared)]);
                        const rows = requirements(held.raw, new Set(Array.fromNullishOr(own.manifest.package?.name))).filter(({ at, current }) => at[0] === 'workspace' && current !== undefined);
                        tree.write(
                            shared,
                            patch(
                                held.text,
                                rows.reduce((value, row) => assigned(value, row.field, newVersion), held.raw),
                            ),
                        );
                        return `New version ${newVersion} written to ${file}`;
                    }),
                ),
            ),
        );
    }

    updateProjectDependencies(tree: Tree, projectGraph: ProjectGraph, dependenciesToUpdate: Record<string, string>) {
        const manifests = this.manifestsToUpdate;
        return run(
            Effect.gen(function* () {
                const versions = new Map(yield* Effect.forEach(Record.toEntries(dependenciesToUpdate), ([project, version]) => Effect.map(crated(tree, projectGraph, project), (crate) => [crate, version] as const)));
                const crates = new Set(versions.keys());
                const own = yield* Effect.forEach(manifests, ({ manifestPath }) => read(tree, manifestPath));
                const rows = own.flatMap((held) => requirements(held.raw, crates).map((requirement) => ({ ...held, ...requirement })));
                const edits = Array.getSomes(rows.map(({ crate, current, ...row }) => Option.map(Option.all([Option.fromUndefinedOr(current), Option.fromUndefinedOr(versions.get(crate))]), ([, value]) => ({ ...row, value }))));
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

const afterAllProjectsVersioned = Effect.fnUntraced(function* (cwd: string, { dryRun, rootVersionActionsOptions }: Parameters<AfterAllProjectsVersioned>[1]) {
    const [fs, { skipLockFileUpdate }] = yield* Effect.all([FileSystem.FileSystem, Schema.decodeEffect(Unlocked)(rootVersionActionsOptions ?? {})]);
    const stated = dryRun === true || skipLockFileUpdate === true ? [] : Object.values((yield* Effect.promise(() => createProjectGraphAsync())).nodes).map((node) => Schema.decodeUnknownOption(Options)(node.data.release?.version?.versionActionsOptions));
    const locks = Array.dedupe(Array.getSomes(stated).map(({ lock }) => lock)).filter((lock) => basename(lock) === LOCK);
    const refresh = (lock: string) => Effect.tryPromise({ try: (signal) => promisify(execFile)('cargo', ['update', '--workspace'], { cwd: join(cwd, dirname(lock)), signal }), catch: refused(lock, 'cargo update does not exit 0') });
    const changedFiles = yield* Effect.filter(locks, (lock) => Effect.zipWith(fs.readFileString(join(cwd, lock)), Effect.andThen(refresh(lock), fs.readFileString(join(cwd, lock))), (before, after) => before !== after));
    return { changedFiles, deletedFiles: [] };
}, run) satisfies AfterAllProjectsVersioned;

// --- [EXPORTS] -------------------------------------------------------------------------

export { afterAllProjectsVersioned };
export default CargoVersionActions;
