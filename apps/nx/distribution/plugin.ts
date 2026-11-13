import { basename, join, matchesGlob, relative } from 'node:path';
import { platform } from 'node:process';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesContext, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { combineGlobPatterns } from '@nx/devkit/internal';
import { present, promised } from '@rasm/nx-devkit/process';
import { ancestors } from '@rasm/nx-devkit/project';
import { Array, Effect, FileSystem, Function, Option, Predicate, Record, Result, Schema, Struct, UndefinedOr } from 'effect';
import { type Fields, HOSTS, type Host } from '#/host-packages';

// --- [TYPES] ---------------------------------------------------------------------------

interface Declared {
    readonly file: string;
    readonly root: string;
    readonly fields: Fields;
    readonly lock?: string;
    readonly host: Host;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = '@rasm/nx-distribution';
const SHARED = '@rasm/nx-devkit';
const FILES = combineGlobPatterns(Array.dedupe(HOSTS.map(({ manifest }) => `**/${basename(manifest)}`)));
const WITHHELD = ['archive', 'build', 'operations', 'publish'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Placed = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String) }).annotate({ description: 'outputPath: workspace folder each host package is written under as <outputPath>/<technology>/<project name>, absent writes to dist/<project root>' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* (files: readonly string[], { workspaceRoot }: CreateNodesContext) {
    const fs = yield* FileSystem.FileSystem;
    const matched = Array.filterMap(files, (file) =>
        Result.map(
            Result.fromOption(
                Array.findFirst(HOSTS, (host) => matchesGlob(file, `**/${host.manifest}`)),
                Function.constVoid,
            ),
            (host) => ({ file, host, root: join(file, relative(host.manifest, '.')) }),
        ),
    );
    const local = Array.dedupe(matched.map(({ host }) => host)).filter(({ platforms = [platform] }) => platforms.includes(platform));
    const installing = yield* Effect.filter(local, ({ install }) => (install?.probe === undefined ? Effect.succeed(true) : Effect.map(present(Array.headNonEmpty(install.probe), Array.tailNonEmpty(install.probe)), Option.isSome)));
    const one = Effect.fnUntraced(function* ({ file, host, root }: (typeof matched)[number]) {
        const lock = Option.map(yield* Effect.findFirst(Array.cartesianWith(ancestors(root), Array.fromNullishOr(host.build?.lock), join), (at) => fs.exists(join(workspaceRoot, at))), (at) => ({ lock: at }));
        const engined = local.includes(host) && (host.held === undefined || (yield* host.held(join(workspaceRoot, file))));
        const fields = yield* Effect.flatMap(fs.readFileString(join(workspaceRoot, file)), (text) => host.read(text, file));
        const withheld: readonly ((typeof WITHHELD)[number] | 'install')[] = [...(engined ? [] : WITHHELD), ...(installing.includes(host) ? [] : (['install'] as const))];
        return Option.map(fields, (stated): Declared => ({ file, root, fields: stated, host: Struct.omit(host, withheld), ...Option.getOrUndefined(lock) }));
    });
    return yield* Effect.partition(matched, (row) => Effect.mapError(one(row), (error): [string, Error] => [row.file, error]), { concurrency: 'unbounded' });
});

const projects = (declared: readonly Declared[], placed: Option.Option<string>) => {
    const holds = (other: Declared, item: Declared) => (other.fields.folders ?? []).some((folder) => !relative(join(other.root, folder), item.root).startsWith('..')) && !(other.fields.references ?? []).some((reference) => reference.name === item.fields.name && !reference.enabled);
    const hosts = Function.memoize((item: Declared) => (item.fields.folders === undefined ? declared.filter((other) => holds(other, item)) : [item]));
    const uses = (item: Declared) => declared.filter((other) => other !== item && (item.fields.references ?? []).some((reference) => reference.enabled && reference.name === other.fields.name));
    const users = (item: Declared) => declared.filter((other) => !hosts(item).includes(other) && uses(other).includes(item));
    return declared.map((item) => {
        const loading = Array.dedupe([...hosts(item), ...users(item)]);
        const lone = item.fields.folders === undefined && item.host.standalone === true && !loading.some((other) => other !== item && other.host.build !== undefined);
        const offered: Host = lone ? Struct.omit(item.host, WITHHELD) : item.host;
        const { technology, archive, platforms = [], standalone = false, build, operations = {}, install, publish, write, manifest } = offered;
        const linked = Array.dedupe([...[...declared.filter((other) => other !== item && hosts(other).includes(item)), ...uses(item)].map((other) => other.fields.name), ...Object.values({ ...item.fields.packages }).map(({ project }) => project)]);
        const outputPath = Option.match(placed, { onNone: () => join('dist', item.root), onSome: (at) => join(at, technology, '{projectName}') });
        const settings = standalone ? { outputPath, descriptor: item.file, hosts: loading.map((other) => other.file), dependencies: uses(item).map((other) => other.file), packages: { ...item.fields.packages } } : { outputPath };
        const inputs: NonNullable<TargetConfiguration['inputs']> = [
            '{projectRoot}/**/*',
            '^default',
            ...Array.fromNullishOr(build?.pack).map(() => ({ dependentTasksOutputFiles: '**/*' })),
            ...Array.fromNullishOr(item.lock).map((lock) => `{workspaceRoot}/${lock}`),
            ...Array.fromNullishOr(build?.runtime).flatMap((runtimes) => runtimes.map((runtime) => ({ runtime }))),
            ...loading.filter((other) => other !== item).map((other) => `{workspaceRoot}/${other.file}`),
            { externalDependencies: [] },
        ];
        const required = Array.match(
            uses(item).filter((other) => !hosts(other).includes(item)),
            { onEmpty: () => [], onNonEmpty: (loaded) => [{ projects: loaded.map((other) => other.fields.name), target: 'install' }] },
        );
        const builder = UndefinedOr.match(build, { onUndefined: () => 'build', onDefined: () => `build--${technology}` });
        const packer = UndefinedOr.match(operations.pack, { onUndefined: () => builder, onDefined: () => `pack--${technology}` });
        const held = Array.match(
            hosts(item).filter((other) => other.host.build !== undefined),
            { onEmpty: () => [], onNonEmpty: (holders) => [{ projects: holders.map((other) => other.fields.name), target: 'build' }] },
        );
        const operated = (verb: keyof NonNullable<Host['operations']>, extra: TargetConfiguration) =>
            Array.fromNullishOr(build).flatMap(({ executor }) =>
                Array.fromNullishOr(operations[verb]).map(({ command, options }): readonly [string, TargetConfiguration] => [
                    verb,
                    { executor: `${PLUGIN}:${executor}`, parallelism: false, ...extra, options: { ...settings, ...options, ...extra.options }, metadata: { description: `Runs ${command} for the ${technology} project its ${manifest} declares` } },
                ]),
            );
        const installs = Option.match(
            Option.filter(Option.fromUndefinedOr(install), ({ platforms: on = [platform] }) => on.includes(platform)),
            {
                onSome: ({ command, unpacked = false }): readonly (readonly [string, TargetConfiguration])[] => [
                    ['install', { executor: `${PLUGIN}:install`, dependsOn: [unpacked ? `build--!(${technology})` : 'build'], metadata: { description: `Installs what build wrote in the run's configuration where its host loads it, through ${command}` } }],
                ],
                onNone: () => [
                    ...held.map((host): readonly [string, TargetConfiguration] => ['install', { executor: 'nx:noop', dependsOn: [host], metadata: { description: `Builds ${host.projects.join(', ')}, the host the result loads in` } }]),
                    ...(users(item).length > 0 ? operated('install', { dependsOn: [builder, ...held], options: { users: users(item).map((other) => other.file) } }) : []),
                ],
            },
        );
        const verbs: readonly (readonly [string, TargetConfiguration])[] = [
            ...Array.fromNullishOr(build).map(({ executor, command, pack }): readonly [string, TargetConfiguration] => [
                'build',
                {
                    executor: `${PLUGIN}:${executor}`,
                    dependsOn: [...Array.fromNullishOr(pack).map(() => 'build--*'), ...required],
                    options: settings,
                    parallelism: !standalone,
                    cache: archive !== undefined,
                    inputs,
                    outputs: UndefinedOr.match(operations.pack, { onUndefined: () => Array.fromNullishOr(archive).map((extension) => `{options.outputPath}/*${extension}`), onDefined: () => ['{options.outputPath}/package'] }),
                    metadata: { description: `Builds the ${technology} project its ${manifest} declares through ${command}` },
                },
            ]),
            ...Array.fromNullishOr(build)
                .filter(() => standalone)
                .map((): readonly [string, TargetConfiguration] => ['check', { executor: 'nx:noop', dependsOn: [builder], metadata: { description: 'Compiles the project as its check' } }]),
            ...operated('check', { cache: true, inputs, dependsOn: required }),
            ...(loading.some((other) => other !== item) ? operated('test', { cache: true, inputs, dependsOn: [`install--${technology}`], outputs: ['{options.outputPath}/report'] }) : []),
            ...operated('pack', { cache: true, inputs, dependsOn: [builder], outputs: ['{options.outputPath}/archive'] }),
            ...installs,
        ];
        const tools = Record.fromIterableWith(verbs, ([verb, target]): [string, TargetConfiguration] => [`${verb}--${technology}`, { ...target, metadata: { technologies: [technology], ...target.metadata } }]);
        const delivered: Record<string, TargetConfiguration> = {
            ...tools,
            ...Array.match(Record.keys(Record.filter(tools, ({ outputs = [] }) => outputs.length > 0)), {
                onEmpty: () => ({}),
                onNonEmpty: (written) => ({ [`clean--${technology}`]: { executor: `${SHARED}:clean`, options: { targets: written }, metadata: { technologies: [technology], description: `Removes the outputs ${written.join(', ')} declare` } } }),
            }),
            ...Record.fromIterableWith(Array.fromNullishOr(archive), (): [string, TargetConfiguration] => [
                'attach--gh',
                { executor: `${SHARED}:attach`, dependsOn: [packer], metadata: { technologies: [technology], description: `Uploads the files ${packer} wrote to the GitHub release of the version the project's manifest states through gh release upload` } },
            ]),
        };
        const released = Option.filter(Option.fromUndefinedOr(write), () => item.fields.version !== undefined);
        const configuration: ProjectConfiguration = {
            root: item.root,
            name: item.fields.name,
            metadata: { technologies: [technology] },
            tags: [...Option.toArray(released).map(() => 'release'), ...platforms.map((os) => `os:${os}`)],
            ...Array.match(standalone ? linked : [], { onEmpty: () => ({}), onNonEmpty: (implicitDependencies) => ({ implicitDependencies }) }),
            ...Option.getOrUndefined(Option.map(released, () => ({ release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { manifest: relative(item.root, item.file) } } } }))),
            targets: {
                ...delivered,
                ...Record.fromIterableWith(Array.dedupe(Record.keys(delivered).map((name) => name.slice(0, name.indexOf('--')))), (verb): [string, TargetConfiguration] => [verb, { dependsOn: [`${verb}--*`] }]),
                ...Record.fromIterableWith(Array.fromNullishOr(publish), ({ store, command }): [string, TargetConfiguration] => [
                    'nx-release-publish',
                    { executor: `${PLUGIN}:release-publish`, dependsOn: [packer, '^nx-release-publish'], metadata: { description: `Publishes the ${archive} packages of the project to ${store} through ${command}` } },
                ]),
            },
        };
        return [item.file, { projects: { [item.root]: configuration } }] as const;
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    (files, options, context) =>
        promised(
            Effect.gen(function* () {
                const { outputPath } = yield* Schema.decodeEffect(Placed)({ ...context.nxJsonConfiguration.pluginsConfig?.[SHARED], ...(Predicate.isObject(options) ? options : {}) });
                const [held, failures] = yield* read(files, context);
                const results = projects(
                    Array.getSomes(held).filter(({ fields }) => fields.installed !== true),
                    Option.fromUndefinedOr(outputPath),
                );
                return yield* Array.match(failures, { onEmpty: () => Effect.succeed(results), onNonEmpty: (refused) => Effect.fail(new AggregateCreateNodesError(refused, results)) });
            }),
        ),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
