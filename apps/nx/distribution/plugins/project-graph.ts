import { matchesGlob } from 'node:path';
import { type CreateNodes, type CreateNodesContext, type CreateNodesResult, createNodesFromFiles, type TargetConfiguration } from '@nx/devkit';
import { getNamedInputs } from '@nx/devkit/internal';
import { aggregated, identity, inferred } from '@rasm/nx-devkit/inference';
import { Array, Effect, FileSystem, Function, Option, Path, pipe, Record, Result, Schema, UndefinedOr } from 'effect';
import { type Fields, HOSTS, type Host } from '#/plugins/host-packages';

// --- [TYPES] ---------------------------------------------------------------------------

interface Discovered {
    readonly file: string;
    readonly host: Host;
    readonly root: string;
    readonly fields: Fields;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const RELEASE = 'release';
const FILES = `**/{${Array.dedupe(HOSTS.map(({ manifest: file }) => file.split('/').at(-1) ?? file)).join(',')}}`;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), engines: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const published = (plugin: string, host: Host, inputs: NonNullable<TargetConfiguration['inputs']>): TargetConfiguration => ({
    executor: `${plugin}:release-publish`,
    dependsOn: ['^nx-release-publish'],
    inputs,
    defaultConfiguration: RELEASE,
    configurations: { [RELEASE]: {} },
    metadata: {
        technologies: [host.technology],
        description: UndefinedOr.match(host.publish, {
            onUndefined: () => `Attaches the ${host.technology} packages build wrote in its ${RELEASE} configuration to the project's GitHub release`,
            onDefined: ({ command }) => `Attaches the ${host.technology} packages build wrote in its ${RELEASE} configuration to the project's GitHub release and publishes them through ${command}`,
        }),
    },
});

const nodes = Effect.fnUntraced(function* (files: readonly string[], options: typeof Options.Encoded | undefined, context: CreateNodesContext) {
    const [fs, path, { outputPath, engines }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Schema.decodeEffect(Options)(options ?? {})]);
    const { name, inputs: own, cleaner } = yield* identity(import.meta.url, context.workspaceRoot);
    const read = yield* Effect.forEach(
        Array.getSomes(
            files.map((file) =>
                pipe(
                    Array.findFirst(HOSTS, (host) => matchesGlob(file, `**/${host.manifest}`)),
                    Option.map((host) => ({ file, host, root: path.join(file, path.relative(host.manifest, '.')) })),
                ),
            ),
        ),
        ({ file, host, root }) =>
            fs.readFileString(path.join(context.workspaceRoot, file)).pipe(
                Effect.flatMap((text) => host.read(text, file)),
                Effect.map(Option.map((fields): Discovered => ({ file, host, root, fields }))),
                Effect.result,
                Effect.map((result) => [file, result] as const),
            ),
        { concurrency: 'unbounded' },
    );
    const discovered = Array.getSomes(Array.getSuccesses(read.map(([, result]) => result))).filter(({ fields }) => fields.installed !== true);
    const holds = (other: Discovered, item: Discovered) => (other.fields.folders ?? []).some((folder) => !path.relative(path.join(other.root, folder), item.root).startsWith('..')) && !(other.fields.references ?? []).some((reference) => reference.name === item.fields.name && !reference.enabled);
    const hosts = Function.memoize((item: Discovered) => (item.fields.folders === undefined ? discovered.filter((other) => holds(other, item)) : [item]));
    const uses = (item: Discovered) => discovered.filter((other) => other !== item && (item.fields.references ?? []).some((reference) => reference.enabled && reference.name === other.fields.name));
    const described = (item: Discovered) => (item.host.standalone === true ? { descriptor: item.file, hosts: hosts(item).map((other) => other.file), dependencies: uses(item).map((other) => other.file), ...(engines === undefined ? {} : { engines }) } : {});
    const builds = (item: Discovered) =>
        Option.map(Option.fromUndefinedOr(item.host.build), ({ executor, command, variables, pack }): TargetConfiguration => {
            const named = pack === undefined ? { folder: item.fields.name, file: UndefinedOr.match(item.fields.version, { onUndefined: () => `${item.fields.name}-*`, onDefined: (version) => `${item.fields.name}-${version}*` }) } : { folder: '{projectName}', file: '*' };
            const destination = { outputPath: outputPath === undefined ? path.join('dist', item.root) : path.join(outputPath, item.host.technology, named.folder), ...described(item) };
            return {
                executor: `${name}:${executor}`,
                ...(pack === undefined ? {} : { dependsOn: ['build--*'] }),
                ...UndefinedOr.match(variables, { onUndefined: () => ({ options: destination }), onDefined: () => ({ options: {}, configurations: { [RELEASE]: destination } }) }),
                cache: item.host.archive !== undefined,
                inputs: [
                    '{projectRoot}/**/*',
                    'production' in getNamedInputs(item.root, context) ? '^production' : '^default',
                    ...(pack === undefined ? [] : [{ dependentTasksOutputFiles: '**/*' }]),
                    ...UndefinedOr.match(variables, { onUndefined: Array.empty, onDefined: Array.map((env) => ({ env })) }),
                    ...hosts(item)
                        .filter((other) => other !== item)
                        .map((other) => `{workspaceRoot}/${other.file}`),
                    { externalDependencies: [] },
                    ...own,
                ],
                ...(item.host.archive === undefined ? {} : { outputs: [path.join('{options.outputPath}', `${named.file}${item.host.archive}`)] }),
                metadata: { technologies: [item.host.technology], description: `Builds the ${item.host.technology} project its ${item.host.manifest} declares`, help: { command: `${command} --help`, example: { options: { outputPath: 'dist' } } } },
            };
        });
    const locking = new Map(yield* Effect.forEach(discovered, (item) => Effect.map(item.host.upgrade?.roots(context.workspaceRoot, item.root) ?? Effect.succeed([]), (roots) => [item, roots] as const), { concurrency: 'unbounded' }));
    const upgrades = (item: Discovered) =>
        UndefinedOr.match(item.host.upgrade, {
            onUndefined: () => ({}),
            onDefined: ({ tool, executor, manifest, before }) =>
                Record.fromIterableWith(locking.get(item) ?? [], (root) => [
                    root,
                    {
                        targets: {
                            ...aggregated(
                                {
                                    upgrade: {
                                        [tool]: {
                                            executor: `${name}:${executor}`,
                                            options: { manifest },
                                            outputs: ['{projectRoot}/{options.manifest}'],
                                            metadata: { technologies: [item.host.technology], description: `Rewrites the constraint rows of ${manifest} from the distributions the installed ${item.host.technology} bundles` },
                                        },
                                    },
                                },
                                [item.host.technology],
                            ),
                            [before]: { dependsOn: ['...', `upgrade--${tool}`] },
                        } satisfies Record<string, TargetConfiguration>,
                        tags: [`host:${item.host.technology}`],
                    },
                ]),
        });
    const installs = (item: Discovered): Option.Option<TargetConfiguration> =>
        Option.firstSomeOf([
            Option.map(
                Option.fromUndefinedOr(item.host.install),
                ({ command }): TargetConfiguration => ({
                    executor: `${name}:install`,
                    dependsOn: ['build'],
                    metadata: { technologies: [item.host.technology], description: `Installs the ${item.host.technology} packages build wrote in the run's configuration, through ${command}`, help: { command: `${command} --help`, example: { options: { dryRun: true } } } },
                }),
            ),
            Option.map(
                Option.fromUndefinedOr(item.host.place),
                (): TargetConfiguration => ({
                    executor: `${name}:place`,
                    dependsOn: [`build--!(${item.host.technology})`],
                    ...UndefinedOr.match(item.host.build?.variables, {
                        onUndefined: () => ({ metadata: { technologies: [item.host.technology], description: `Places the unpacked ${item.host.technology} plug-in the project's builds wrote into its host's per-user folder, keeping the plug-in's data unless removeData is set` } }),
                        onDefined: (variables) => ({
                            configurations: { [RELEASE]: { variables } },
                            metadata: {
                                technologies: [item.host.technology],
                                description: `Signs the unpacked ${item.host.technology} plug-in the project's builds wrote and places it into its host's per-user folder, with the certificate ${variables[0]} holds in ${RELEASE} and this machine's self-signed certificate otherwise`,
                            },
                        }),
                    }),
                }),
            ),
            Option.liftPredicate(
                {
                    executor: 'nx:noop',
                    dependsOn: [{ projects: hosts(item).map((other) => other.fields.name), target: 'build' }],
                    metadata: {
                        technologies: [item.host.technology],
                        description: `Builds ${hosts(item)
                            .map((other) => other.fields.name)
                            .join(', ')}, the host the result loads in`,
                    },
                },
                () => hosts(item).length > 0,
            ),
        ]);
    const owned = (item: Discovered, build: TargetConfiguration) => {
        const linked = Array.dedupe([...discovered.filter((other) => other !== item && hosts(other).includes(item)), ...uses(item)]);
        return {
            project: { name: item.fields.name, ...(linked.length === 0 ? {} : { implicitDependencies: linked.map((other) => other.fields.name) }) },
            check: { [item.host.technology]: { executor: 'nx:noop', dependsOn: [`build--${item.host.technology}`], inputs: build.inputs ?? [], metadata: { technologies: [item.host.technology], description: 'Compiles the project as its check' } } },
        };
    };
    const node = (item: Discovered): CreateNodesResult => {
        const build = builds(item);
        const standalone = Option.map(
            Option.filter(build, () => item.host.standalone === true),
            (target) => owned(item, target),
        );
        const { [item.root]: rooted, ...others }: Readonly<Record<string, { readonly targets: Readonly<Record<string, TargetConfiguration>>; readonly tags: readonly string[] }>> = upgrades(item);
        return {
            projects: {
                ...Record.map(others, ({ targets, tags }) => ({ targets, tags: [...tags] })),
                [item.root]: {
                    ...Option.getOrUndefined(Option.map(standalone, ({ project }) => project)),
                    targets: {
                        ...rooted?.targets,
                        ...aggregated(
                            {
                                build: Option.match(build, { onNone: () => ({}), onSome: (target) => ({ [item.host.technology]: target }) }),
                                check: Option.match(standalone, { onNone: () => ({}), onSome: ({ check }) => check }),
                                install: Option.match(installs(item), { onNone: () => ({}), onSome: (target) => ({ [item.host.install?.tool ?? item.host.technology]: target }) }),
                            },
                            [item.host.technology],
                            { executor: cleaner, tool: item.host.technology },
                        ),
                        ...(item.host.archive === undefined ? {} : { 'nx-release-publish': published(name, item.host, own) }),
                    },
                    tags: [...(item.host.build !== undefined && item.host.build.pack === undefined ? [`host:${item.host.technology}`] : []), ...(Option.isSome(build) && item.host.write !== undefined ? [RELEASE] : [])],
                    ...(Option.isSome(build) && item.host.write !== undefined ? { release: { version: { versionActions: `${name}/release/version-actions`, versionActionsOptions: { manifest: path.relative(item.root, item.file) } } } } : {}),
                },
            },
        };
    };
    const created: Readonly<Record<string, Result.Result<CreateNodesResult, Error>>> = Record.map(
        Record.fromEntries(read),
        Result.map((found) =>
            Option.match(
                Option.filter(found, ({ fields }) => fields.installed !== true),
                { onNone: () => ({}), onSome: node },
            ),
        ),
    );
    return yield* Effect.promise(() =>
        createNodesFromFiles(
            (file) => Result.match(created[file] ?? Result.succeed({}), { onFailure: (error) => Promise.reject(error), onSuccess: (result) => Promise.resolve(result) }),
            read.map(([file]) => file),
            options,
            context,
        ),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [FILES, (files, options, context) => inferred(nodes(files, options, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
