import { dirname, join } from 'node:path';
import { parse, patch } from '@decimalturn/toml-patch';
import { joinPathFragments, type ProjectGraph, type Tree } from '@nx/devkit';
import { relocked } from '@rasm/nx-devkit/executor';
import { promised, ran, ToolError } from '@rasm/nx-devkit/process';
import { clean, explain, RANGE_PATTERN, satisfies } from '@renovatebot/pep440';
import { Array, Effect, FileSystem, Option, Predicate, Record, Schema, SemVer, String } from 'effect';
import { VersionActions } from 'nx/release';
import { LOCK, normalized } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PYPROJECT = 'pyproject.toml';
const PRERELEASES = { a: 'alpha', b: 'beta', rc: 'rc' } as const;
const REQUIREMENT = /^\s*(?<name>[a-z0-9](?:[a-z0-9._-]*[a-z0-9])?)\s*(?:\[[^\]]*\])?(?<specifier>[^;]*)/iu;
const RANGE = new RegExp(RANGE_PATTERN, 'giu');
const RAISED = new Set(['==', '===', '~=', '>=']);
const INSTALLED = /^ \+ (?<name>\S+)==(?<version>\S+)$/gmu;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ lock: Schema.String });
const Table = Schema.Record(Schema.String, Schema.Unknown);
const Project = Schema.Struct({
    project: Schema.Struct({
        name: Schema.String,
        version: Schema.String,
        dependencies: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        'optional-dependencies': Schema.Record(Schema.String, Schema.Array(Schema.String)).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    }),
    'dependency-groups': Schema.Record(Schema.String, Schema.Array(Schema.Unknown)).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
});
const Raw = Schema.Struct({ project: Table });
const Semantic = Schema.Struct({
    epoch: Schema.Literal(0),
    release: Schema.Tuple([Schema.Int, Schema.optionalKey(Schema.Int), Schema.optionalKey(Schema.Int)]),
    pre: Schema.NullOr(Schema.Tuple([Schema.Literals(Record.keys(PRERELEASES)), Schema.Int])),
    post: Schema.Null,
    dev: Schema.Null,
    local: Schema.Null,
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const semver = (version: string) =>
    Schema.decodeUnknownEffect(Semantic)(explain(version)).pipe(
        Effect.flatMap(({ release: [major, minor = 0, micro = 0], pre }) => Effect.fromResult(SemVer.fromInput({ major, minor, patch: micro, prerelease: pre === null ? [] : [PRERELEASES[pre[0]], pre[1]] }))),
        Effect.mapBoth({ onFailure: (cause) => ToolError.make({ subject: version, detail: 'has an epoch, post, dev, or local part, or more than three release numbers, which SemVer cannot state', cause }), onSuccess: SemVer.format }),
    );

const pep440 = (version: string) =>
    Effect.fromOption(
        Option.filter(Option.fromNullishOr(clean(version)), (cleaned) => explain(cleaned)?.post === null),
        () => ToolError.make({ subject: version, detail: 'has no PEP 440 form; use a prerelease id of alpha, beta, or rc' }),
    );

const required = (requirement: string) => Option.flatMap(Option.fromNullishOr(REQUIREMENT.exec(requirement)?.groups), ({ name, specifier = '' }) => Option.map(Option.fromNullishOr(name), (stated) => ({ name: normalized(stated), specifier: specifier.trim() })));

const raised = (floors: ReadonlyMap<string, string>, row: string) =>
    Option.match(
        Option.flatMap(required(row), ({ name, specifier }) => Option.filter(Option.fromUndefinedOr(floors.get(name)), () => String.isNonEmpty(specifier))),
        {
            onNone: () => Effect.succeed(row),
            onSome: (version) => {
                const marked = row.indexOf(';');
                const written = [...row.matchAll(RANGE)].reduceRight((text, clause) => {
                    const { operator, version: stated } = clause.groups ?? {};
                    const [held, next] = [explain(stated)?.release ?? [], explain(version)?.release ?? []];
                    const moved = held.length < next.length ? next.slice(0, held.length).join('.') : version;
                    return stated !== undefined && operator !== undefined && RAISED.has(operator) && (marked < 0 || clause.index < marked) ? `${text.slice(0, clause.index)}${clause[0].replace(stated, moved)}${text.slice(clause.index + clause[0].length)}` : text;
                }, row);
                return Option.exists(required(written), ({ specifier }) => satisfies(version, specifier)) ? Effect.succeed(written) : Effect.fail(ToolError.make({ subject: row, detail: `excludes the released ${version} once each lower bound moves to it` }));
            },
        },
    );

const project = Effect.fnUntraced(function* (tree: Tree, at: string) {
    const text = yield* Effect.mapError(Effect.fromNullishOr(tree.read(at, 'utf-8')), ToolError.of(at, 'is missing'));
    const parsed = yield* Effect.try({ try: (): unknown => parse(text), catch: ToolError.of(at, 'does not parse as TOML') }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Table)));
    const [decoded, raw] = yield* Effect.all([Schema.decodeUnknownEffect(Project)(parsed), Schema.decodeUnknownEffect(Raw)(parsed)]);
    return { text, parsed, project: decoded.project, groups: decoded['dependency-groups'], table: raw.project };
});

// --- [SERVICES] ------------------------------------------------------------------------

class UvVersionActions extends VersionActions {
    override readonly validManifestFilenames = [PYPROJECT];
    override readonly excludeManifestsFromFormatting = true;

    get manifest() {
        return joinPathFragments(this.projectGraphNode.data.root, PYPROJECT);
    }

    override readCurrentVersionFromSourceManifest(tree: Tree) {
        const manifestPath = this.manifest;
        return promised(
            project(tree, manifestPath).pipe(
                Effect.flatMap((held) => semver(held.project.version)),
                Effect.map((currentVersion) => ({ currentVersion, manifestPath })),
            ),
        );
    }

    override readCurrentVersionFromRegistry(tree: Tree) {
        return promised(
            Effect.gen({ self: this }, function* () {
                const fs = yield* FileSystem.FileSystem;
                const [{ lock }, { project: stated }, target] = yield* Effect.all([Schema.decodeUnknownEffect(Options)(this.finalConfigForProject.versionActionsOptions), project(tree, this.manifest), fs.makeTempDirectoryScoped()]);
                const install = ['pip', 'install', '--dry-run', '--no-deps', '--target', target, stated.name];
                const { stdout, stderr } = yield* ran('uv', install, { cwd: join(tree.root, dirname(lock)) });
                const found = Array.findFirst([...`${stdout}${stderr}`.matchAll(INSTALLED)], ({ groups }) => (groups?.['name'] !== undefined && normalized(groups['name']) === normalized(stated.name) ? Option.fromUndefinedOr(groups['version']) : Option.none()));
                const latest = yield* Effect.mapError(Effect.fromOption(found), ToolError.of(stated.name, `is absent from the output of uv ${install.join(' ')}`));
                return { currentVersion: yield* semver(latest), logText: `uv ${install.join(' ')}` };
            }).pipe(Effect.scoped),
        );
    }

    override readCurrentVersionOfDependency(tree: Tree, projectGraph: ProjectGraph, dependencyProjectName: string) {
        const manifest = this.manifest;
        return promised(
            Effect.gen(function* () {
                const node = yield* Effect.fromNullishOr(projectGraph.nodes[dependencyProjectName]);
                const [{ project: dependent, groups }, { project: dependency }] = yield* Effect.all([project(tree, manifest), project(tree, joinPathFragments(node.data.root, PYPROJECT))]);
                const collections = [
                    ['dependencies', dependent.dependencies] as const,
                    ...Record.collect(dependent['optional-dependencies'], (extra, rows) => [`optional-dependencies.${extra}`, rows] as const),
                    ...Record.collect(groups, (group, rows) => [`dependency-groups.${group}`, rows.filter(Predicate.isString)] as const),
                ];
                const floors = (row: string) => Option.exists(required(row), ({ name, specifier }) => String.isNonEmpty(specifier) && name === normalized(dependency.name));
                return yield* Option.match(
                    Array.findFirst(collections, ([, rows]) => rows.some(floors)),
                    {
                        onNone: () => Effect.succeed<{ readonly currentVersion: string | null; readonly dependencyCollection: string | null }>({ currentVersion: null, dependencyCollection: null }),
                        onSome: ([dependencyCollection]) => Effect.map(semver(dependency.version), (currentVersion) => ({ currentVersion, dependencyCollection })),
                    },
                );
            }),
        );
    }

    override updateProjectVersion(tree: Tree, newVersion: string) {
        return promised(
            Effect.forEach(
                this.manifestsToUpdate,
                Effect.fnUntraced(function* ({ manifestPath }) {
                    const [{ text, parsed, table }, version] = yield* Effect.all([project(tree, manifestPath), pep440(newVersion)]);
                    tree.write(manifestPath, patch(text, { ...parsed, project: { ...table, version } }));
                    return `New version ${version} written to manifest: ${manifestPath}`;
                }),
            ),
        );
    }

    override updateProjectDependencies(tree: Tree, projectGraph: ProjectGraph, dependenciesToUpdate: Record<string, string>) {
        const manifests = Record.isEmptyRecord(dependenciesToUpdate) ? [] : this.manifestsToUpdate;
        return promised(
            Effect.gen(function* () {
                const released = yield* Effect.forEach(Record.toEntries(dependenciesToUpdate), ([dependency, version]) => Effect.all([Effect.flatMap(Effect.fromNullishOr(projectGraph.nodes[dependency]), (node) => project(tree, joinPathFragments(node.data.root, PYPROJECT))), pep440(version)]));
                const floors = new Map(released.map(([held, version]) => [normalized(held.project.name), version] as const));
                const moved = (row: unknown) => (Predicate.isString(row) ? raised(floors, row) : Effect.succeed(row));
                const logs = yield* Effect.forEach(
                    manifests,
                    Effect.fnUntraced(function* ({ manifestPath }) {
                        const { text, parsed, table, project: dependent, groups } = yield* project(tree, manifestPath);
                        const [dependencies, optional, grouped] = yield* Effect.all([Effect.forEach(dependent.dependencies, moved), Effect.all(Record.map(dependent['optional-dependencies'], Effect.forEach(moved))), Effect.all(Record.map(groups, Effect.forEach(moved)))]);
                        tree.write(
                            manifestPath,
                            patch(text, {
                                ...parsed,
                                project: { ...table, ...('dependencies' in table ? { dependencies } : {}), ...(Record.isEmptyRecord(optional) ? {} : { 'optional-dependencies': optional }) },
                                ...(Record.isEmptyRecord(grouped) ? {} : { 'dependency-groups': grouped }),
                            }),
                        );
                        return [...floors].map(([name, floor]) => `Lower bound of ${name} moved to ${floor} in ${manifestPath}`);
                    }),
                );
                return logs.flat();
            }),
        );
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const afterAllProjectsVersioned = relocked(LOCK, ['uv', 'lock', '--no-locked']);

// --- [EXPORTS] -------------------------------------------------------------------------

export { afterAllProjectsVersioned };
export default UvVersionActions;
