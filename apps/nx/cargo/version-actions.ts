import { dirname, join } from 'node:path';
import { parse, patch } from '@decimalturn/toml-patch';
import type { ProjectGraph, Tree } from '@nx/devkit';
import { relocked } from '@rasm/nx-devkit/executor';
import { printed, promised, ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, flow, Option, Record, Schema } from 'effect';
import { Toml } from 'effect/encoding';
import { VersionActions } from 'nx/release';
import { LOCK, MANIFEST } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TABLES = ['dependencies', 'dev-dependencies', 'build-dependencies'];

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ lock: Schema.String, registry: Schema.optionalKey(Schema.String) });
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
    const text = yield* Effect.mapError(Effect.fromNullishOr(tree.read(file, 'utf-8')), ToolError.of(file, 'is missing'));
    const raw = table(yield* Effect.try({ try: (): unknown => parse(text), catch: ToolError.of(file, 'does not parse as TOML') }));
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
        const refused = Effect.flatMap(stated, ({ shared }) =>
            Effect.forEach(
                manifests,
                ({ manifestPath }) =>
                    Effect.flatMap(versioned(tree, manifestPath, shared), ({ file, inherited }) =>
                        inherited && projectsRelationship === 'independent' ? ToolError.make({ subject: manifestPath, detail: `inherits the version in ${file}, which every inheriting member shares, so ${name} needs a fixed release group in place of the independent group ${group}` }) : Effect.void,
                    ),
                { discard: true },
            ),
        );
        return super.validate(tree).then(() => promised(refused));
    }

    readCurrentVersionFromSourceManifest(tree: Tree) {
        return promised(
            Effect.gen({ self: this }, function* () {
                const { manifest, shared } = yield* this.stated;
                const { file, current } = yield* versioned(tree, manifest, shared);
                return { manifestPath: file, currentVersion: current };
            }),
        );
    }

    readCurrentVersionFromRegistry(tree: Tree) {
        return promised(
            Effect.gen({ self: this }, function* () {
                const { manifest, registry } = yield* this.stated;
                const { manifest: held } = yield* read(tree, manifest);
                const crate = yield* Effect.mapError(Effect.fromNullishOr(held.package?.name), ToolError.of(manifest, 'names no package'));
                const search = ['search', crate, '--limit', '1', ...Array.fromNullishOr(registry).flatMap((named) => ['--registry', named])];
                const listed = yield* printed('cargo', search, { cwd: tree.root });
                const found = yield* Effect.flatMap(Effect.fromResult(Toml.parse(listed)), flow(Schema.decodeUnknownEffect(Found), Effect.map(Record.get(crate))));
                const currentVersion = yield* Effect.mapError(Effect.fromOption(found), ToolError.of(crate, `is absent from the output of cargo ${search.join(' ')}`));
                return { currentVersion, logText: `cargo ${search.join(' ')}` };
            }),
        );
    }

    readCurrentVersionOfDependency(tree: Tree, projectGraph: ProjectGraph, dependencyProjectName: string) {
        return promised(
            Effect.gen({ self: this }, function* () {
                const { manifest, shared } = yield* this.stated;
                const crates = new Set([yield* crated(tree, projectGraph, dependencyProjectName)]);
                const declared = Array.head(requirements((yield* read(tree, manifest)).raw, crates));
                const source = Option.exists(declared, ({ inherited }) => inherited) ? Array.head(requirements((yield* read(tree, shared)).raw, crates)) : declared;
                const current = Option.flatMap(source, (requirement) => Option.fromUndefinedOr(requirement.current));
                return {
                    currentVersion: Option.getOrNull(current),
                    dependencyCollection: Option.getOrNull(
                        Option.andThen(
                            current,
                            Option.map(declared, ({ at }) => at.join('.')),
                        ),
                    ),
                };
            }),
        );
    }

    updateProjectVersion(tree: Tree, newVersion: string) {
        const manifests = this.manifestsToUpdate;
        return promised(
            Effect.flatMap(this.stated, ({ shared }) =>
                Effect.forEach(manifests, ({ manifestPath }) =>
                    Effect.map(versioned(tree, manifestPath, shared), ({ file, text, raw, field }) => {
                        tree.write(file, patch(text, assigned(raw, field, newVersion)));
                        return `New version ${newVersion} written to ${file}`;
                    }),
                ),
            ),
        );
    }

    updateProjectDependencies(tree: Tree, projectGraph: ProjectGraph, dependenciesToUpdate: Record<string, string>) {
        const manifests = this.manifestsToUpdate;
        return promised(
            Effect.gen({ self: this }, function* () {
                const { shared } = yield* this.stated;
                const versions = new Map(yield* Effect.forEach(Record.toEntries(dependenciesToUpdate), ([project, version]) => Effect.map(crated(tree, projectGraph, project), (crate) => [crate, version] as const)));
                const crates = new Set(versions.keys());
                const own = yield* Effect.forEach(manifests, ({ manifestPath }) => read(tree, manifestPath));
                const inherits = own.some(({ file, raw }) => file !== shared && requirements(raw, crates).some(({ inherited }) => inherited));
                const rows = [...own, ...(inherits ? [yield* read(tree, shared)] : [])].flatMap((held) => requirements(held.raw, crates).map((requirement) => ({ ...held, ...requirement })));
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

const afterAllProjectsVersioned = relocked(LOCK, ['cargo', 'update', '--workspace']);

// --- [EXPORTS] -------------------------------------------------------------------------

export { afterAllProjectsVersioned };
export default CargoVersionActions;
