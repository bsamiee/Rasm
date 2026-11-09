import { parse, patch } from '@decimalturn/toml-patch';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import type { ProjectGraph, Tree } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { Array, Effect, Option, Path, Record, Schema } from 'effect';
import { FetchHttpClient, type HttpClient } from 'effect/http';
import { type AfterAllProjectsVersioned, VersionActions } from 'nx/release';
import { releases } from '#/executors/release-publish/executor';
import { CargoError, MANIFEST, spawned } from '#/plugins/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TABLES = ['dependencies', 'dev-dependencies', 'build-dependencies'];
const OPERATOR = /^[\^=~<>]+/u;

// --- [MODELS] --------------------------------------------------------------------------

const Table = Schema.Record(Schema.String, Schema.Unknown);
const Requirement = Schema.Union([Schema.String, Schema.Struct({ version: Schema.optionalKey(Schema.String), package: Schema.optionalKey(Schema.String), workspace: Schema.optionalKey(Schema.Boolean) })]);
const Dependencies = Schema.Record(Schema.String, Requirement);
const Manifest = Schema.Struct({
    package: Schema.optionalKey(Schema.Struct({ name: Schema.String, version: Schema.optionalKey(Schema.Union([Schema.String, Schema.Struct({ workspace: Schema.Boolean })])) })),
    workspace: Schema.optionalKey(Schema.Struct({ package: Schema.optionalKey(Schema.Struct({ version: Schema.optionalKey(Schema.String) })) })),
});
const Located = Schema.fromJsonString(Schema.Struct({ root: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | HttpClient.HttpClient>) => Effect.runPromise(effect.pipe(Effect.provide([layer, FetchHttpClient.layer])));

const table = (value: unknown): Readonly<Record<string, unknown>> => Option.getOrElse(Schema.decodeUnknownOption(Table)(value), () => ({}));

const read = Effect.fnUntraced(function* (tree: Tree, file: string) {
    const text = tree.read(file, 'utf-8') ?? '';
    const raw = table(yield* Effect.try({ try: (): unknown => parse(text), catch: (cause) => CargoError.make({ subject: file, detail: String(cause) }) }));
    return { text, raw, manifest: yield* Schema.decodeEffect(Manifest)(raw) };
});

const located = (tree: Tree, manifest: string) => Effect.map(Effect.all([Path.Path, Effect.flatMap(spawned('cargo', ['locate-project', '--workspace', '--manifest-path', manifest], tree.root), Schema.decodeEffect(Located))]), ([path, { root }]) => path.relative(tree.root, root));

const assigned = (value: Record<string, unknown>, [key, ...rest]: readonly string[], next: unknown): Record<string, unknown> => (key === undefined ? value : { ...value, [key]: rest.length === 0 ? next : assigned(table(value[key]), rest, next) });

const bumped = (requirement: string, version: string) => `${OPERATOR.exec(requirement)?.[0] ?? ''}${version}`;

const requirements = (raw: Readonly<Record<string, unknown>>, crates: ReadonlySet<string>) =>
    [...TABLES.map((held) => [held]), ...Record.keys(table(raw.target)).flatMap((platform) => TABLES.map((held) => ['target', platform, held])), ['workspace', 'dependencies']].flatMap((at) =>
        Record.collect(
            Option.getOrElse(Schema.decodeUnknownOption(Dependencies)(at.reduce<unknown>((value, key) => table(value)[key], raw)), () => ({})),
            (key, requirement) => (typeof requirement === 'string' ? { crate: key, at, field: [...at, key], current: requirement, inherited: false } : { crate: requirement.package ?? key, at, field: [...at, key, 'version'], current: requirement.version, inherited: requirement.workspace === true }),
        ).filter(({ crate }) => crates.has(crate)),
    );

// --- [SERVICES] ------------------------------------------------------------------------

class CargoVersionActions extends VersionActions {
    override readonly validManifestFilenames = [MANIFEST];
    override readonly excludeManifestsFromFormatting = true;

    override readDependencies(_tree: Tree, projectGraph: ProjectGraph) {
        return Promise.resolve((projectGraph.dependencies[this.projectGraphNode.name] ?? []).filter(({ target }) => target in projectGraph.nodes));
    }

    readCurrentVersionFromSourceManifest(tree: Tree) {
        const manifest = `${this.projectGraphNode.data.root}/${MANIFEST}`;
        return run(
            Effect.gen(function* () {
                const version = (yield* read(tree, manifest)).manifest.package?.version;
                return { manifestPath: manifest, currentVersion: typeof version === 'string' ? version : ((yield* read(tree, yield* located(tree, manifest))).manifest.workspace?.package?.version ?? '0.0.0') };
            }),
        );
    }

    readCurrentVersionFromRegistry(tree: Tree) {
        const { name, data } = this.projectGraphNode;
        return run(
            Effect.flatMap(read(tree, `${data.root}/${MANIFEST}`), ({ manifest }) => releases(manifest.package?.name ?? name)).pipe(
                Effect.map(({ url, found }) => ({ currentVersion: Option.getOrNull(Option.map(Array.last(found.filter(({ yanked }) => !yanked)), ({ vers }) => vers)), logText: `crates.io index ${url}` })),
            ),
        );
    }

    readCurrentVersionOfDependency(tree: Tree, projectGraph: ProjectGraph, dependencyProjectName: string) {
        const manifest = `${this.projectGraphNode.data.root}/${MANIFEST}`;
        const dependency = projectGraph.nodes[dependencyProjectName]?.name ?? dependencyProjectName;
        return run(
            Effect.gen(function* () {
                const crates = new Set([dependency]);
                const found = Array.head(requirements((yield* read(tree, manifest)).raw, crates));
                const source = Option.exists(found, ({ inherited }) => inherited) ? Array.head(requirements((yield* read(tree, yield* located(tree, manifest))).raw, crates)) : found;
                return {
                    currentVersion: Option.getOrNull(Option.flatMap(source, ({ current }) => Option.fromUndefinedOr(current))),
                    dependencyCollection: Option.getOrNull(Option.map(found, ({ at }) => at.join('.'))),
                };
            }),
        );
    }

    updateProjectVersion(tree: Tree, newVersion: string) {
        const manifest = `${this.projectGraphNode.data.root}/${MANIFEST}`;
        return run(
            Effect.gen(function* () {
                const own = yield* read(tree, manifest);
                const inherited = typeof own.manifest.package?.version !== 'string';
                const file = inherited ? yield* located(tree, manifest) : manifest;
                const held = inherited ? yield* read(tree, file) : own;
                tree.write(file, patch(held.text, assigned(held.raw, inherited ? ['workspace', 'package', 'version'] : ['package', 'version'], newVersion)));
                return [`New version ${newVersion} written to ${file}`];
            }),
        );
    }

    updateProjectDependencies(tree: Tree, projectGraph: ProjectGraph, dependenciesToUpdate: Record<string, string>) {
        const manifest = `${this.projectGraphNode.data.root}/${MANIFEST}`;
        const versions = new Map(Record.collect(dependenciesToUpdate, (project, version) => [projectGraph.nodes[project]?.name ?? project, version] as const));
        return run(
            Effect.gen(function* () {
                const held = yield* Effect.forEach(Array.dedupe([manifest, yield* located(tree, manifest)]), (file) => Effect.map(read(tree, file), (found) => ({ file, ...found })));
                const edits = held.flatMap(({ file, text, raw }) =>
                    requirements(raw, new Set(versions.keys())).flatMap(({ crate, field, current }) => {
                        const version = versions.get(crate);
                        return current === undefined || version === undefined ? [] : [{ file, text, raw, field, value: bumped(current, version) }];
                    }),
                );
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

const afterAllProjectsVersioned: AfterAllProjectsVersioned = (cwd, { dryRun }) =>
    run(
        Effect.gen(function* () {
            const path = yield* Path.Path;
            const locks = dryRun === true ? [] : yield* Effect.promise(() => globWithWorkspaceContext(cwd, ['**/Cargo.lock']));
            yield* Effect.forEach(locks, (lock) => spawned('cargo', ['update', '--workspace'], path.join(cwd, path.dirname(lock))));
            return { changedFiles: locks, deletedFiles: [] };
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { afterAllProjectsVersioned };
export default CargoVersionActions;
