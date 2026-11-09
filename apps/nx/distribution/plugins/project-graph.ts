import { findPackageJSON } from 'node:module';
import { matchesGlob } from 'node:path';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesResultArray, type TargetConfiguration } from '@nx/devkit';
import { getNamedInputs, globWithWorkspaceContext } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Schema } from 'effect';
import { type Fields, HOSTS, type Host, RELEASE } from '#/plugins/host-packages';

// --- [TYPES] ---------------------------------------------------------------------------

interface Found {
    readonly file: string;
    readonly host: Host;
    readonly root: string;
    readonly fields: Fields;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = `**/{${Array.dedupe(HOSTS.map(({ manifest: file }) => file.split('/').at(-1) ?? file)).join(',')}}`;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), engines: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)) });
const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const aggregate = (technology: string, verb: string, inputs: TargetConfiguration['inputs']): TargetConfiguration => ({
    executor: 'nx:noop',
    dependsOn: [`${verb}--*`],
    inputs: ['...', ...(inputs ?? [])],
    metadata: { technologies: [technology], description: `Runs every ${verb}--* target` },
});

const published = (plugin: string, host: Host, inputs: NonNullable<TargetConfiguration['inputs']>): TargetConfiguration => ({
    executor: `${plugin}:release-publish`,
    dependsOn: ['^nx-release-publish'],
    inputs,
    defaultConfiguration: RELEASE,
    configurations: { [RELEASE]: {} },
    metadata: {
        technologies: [host.technology],
        description: `Attaches the ${host.technology} packages build wrote in its ${RELEASE} configuration to the project's GitHub release${Option.match(Option.fromUndefinedOr(host.publish), { onNone: () => '', onSome: ({ command }) => ` and publishes them through ${command}` })}`,
    },
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<typeof Options.Encoded> = [
    FILES,
    (files, options, context) =>
        Effect.runPromise(
            Effect.gen(function* () {
                const [fs, path, { outputPath, engines }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Schema.decodeEffect(Options)(options ?? {})]);
                const located = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url));
                const { name } = yield* Effect.flatMap(fs.readFileString(located), Schema.decodeEffect(Manifest));
                const tracked = yield* Effect.promise(() => globWithWorkspaceContext(context.workspaceRoot, [path.relative(context.workspaceRoot, located)]));
                const own = tracked.length === 0 ? [{ externalDependencies: [name] }] : [{ input: 'default', projects: [name] }, { externalDependencies: [] }];
                const [read, failures] = yield* Effect.partition(
                    files.flatMap((file) =>
                        Option.toArray(
                            Option.map(
                                Array.findFirst(HOSTS, (host) => matchesGlob(file, `**/${host.manifest}`)),
                                (host) => ({ file, host, root: path.join(file, path.relative(host.manifest, '.')) }),
                            ),
                        ),
                    ),
                    ({ file, host, root }) =>
                        fs.readFileString(path.join(context.workspaceRoot, file)).pipe(
                            Effect.flatMap((text) => host.read(text, file)),
                            Effect.mapBoth({ onFailure: (error): [string, Error] => [file, new Error(error.message)], onSuccess: Option.map((fields): Found => ({ file, host, root, fields })) }),
                        ),
                    { concurrency: 'unbounded' },
                );
                const found = Array.getSomes(read).filter(({ fields }) => fields.installed !== true);
                const holds = (other: Found, item: Found) => (other.fields.folders ?? []).some((folder) => !path.relative(path.join(other.root, folder), item.root).startsWith('..'));
                const hosts = (item: Found) => (item.fields.folders === undefined ? found.filter((other) => holds(other, item) && !(other.fields.references ?? []).some((reference) => reference.name === item.fields.name && !reference.enabled)) : [item]);
                const uses = (item: Found) => found.filter((other) => other !== item && (item.fields.references ?? []).some((reference) => reference.enabled && reference.name === other.fields.name));
                const described = (item: Found) => (item.host.standalone === true ? { descriptor: item.file, hosts: hosts(item).map((other) => other.file), dependencies: uses(item).map((other) => other.file), ...(engines === undefined ? {} : { engines }) } : {});
                const builds = (item: Found) =>
                    Option.map(
                        Option.fromUndefinedOr(item.host.build),
                        ({ executor, command }): TargetConfiguration => ({
                            executor: `${name}:${executor}`,
                            options: {
                                outputPath: outputPath === undefined ? path.join('dist', item.root) : path.join(outputPath, item.host.technology, item.fields.name),
                                ...described(item),
                            },
                            cache: item.host.archive !== undefined,
                            inputs: [
                                '{projectRoot}/**/*',
                                'production' in getNamedInputs(item.root, context) ? '^production' : '^default',
                                ...hosts(item)
                                    .filter((other) => other !== item)
                                    .map((other) => `{workspaceRoot}/${other.file}`),
                                { externalDependencies: [] },
                                ...own,
                            ],
                            ...(item.host.archive === undefined ? {} : { outputs: [path.join('{options.outputPath}', `${item.fields.name}-${item.fields.version ?? ''}*${item.host.archive}`)] }),
                            metadata: { technologies: [item.host.technology], description: `Builds the ${item.host.technology} project its ${item.host.manifest} declares`, help: { command: `${command} --help`, example: { options: { outputPath: 'dist' } } } },
                        }),
                    );
                const installs = (item: Found): Option.Option<TargetConfiguration> =>
                    Option.orElse(
                        Option.map(Option.fromUndefinedOr(item.host.install), ({ command }) => ({ executor: `${name}:install`, dependsOn: ['build'], inputs: own, metadata: { technologies: [item.host.technology], description: `Installs the ${item.host.technology} packages build wrote, through ${command}` } })),
                        () =>
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
                    );
                const owned = (item: Found, build: TargetConfiguration) => {
                    const linked = Array.dedupe([...found.filter((other) => other !== item && hosts(other).includes(item)), ...uses(item)]);
                    return {
                        project: { name: item.fields.name, ...(linked.length === 0 ? {} : { implicitDependencies: linked.map((other) => other.fields.name) }) },
                        targets: {
                            [`check--${item.host.technology}`]: { executor: 'nx:noop', dependsOn: [`build--${item.host.technology}`], inputs: build.inputs ?? [], metadata: { technologies: [item.host.technology], description: 'Compiles the project as its check' } },
                            check: aggregate(item.host.technology, 'check', build.inputs),
                        },
                    };
                };
                const nodes = found.map((item): CreateNodesResultArray[number] => {
                    const build = builds(item);
                    const standalone = Option.map(
                        Option.filter(build, () => item.host.standalone === true),
                        (target) => owned(item, target),
                    );
                    return [
                        item.file,
                        {
                            projects: {
                                [item.root]: {
                                    ...Option.match(standalone, { onNone: () => ({}), onSome: ({ project }) => project }),
                                    targets: {
                                        ...Option.match(build, { onNone: () => ({}), onSome: (target) => ({ [`build--${item.host.technology}`]: target, build: aggregate(item.host.technology, 'build', target.inputs) }) }),
                                        ...Option.match(standalone, { onNone: () => ({}), onSome: ({ targets }) => targets }),
                                        ...Option.match(installs(item), { onNone: () => ({}), onSome: (target) => ({ install: target }) }),
                                        ...(item.host.archive === undefined ? {} : { 'nx-release-publish': published(name, item.host, own) }),
                                    },
                                    ...(Option.isSome(build) && item.host.write !== undefined ? { tags: [RELEASE], release: { version: { versionActions: `${name}/release/version-actions`, versionActionsOptions: { manifest: path.basename(item.file) } } } } : {}),
                                },
                            },
                        },
                    ];
                });
                return { failures, nodes };
            }).pipe(
                Effect.mapError((error) => new AggregateCreateNodesError([[null, new Error(error.message)]], [])),
                Effect.flatMap(({ failures, nodes }) => (failures.length === 0 ? Effect.succeed(nodes) : Effect.fail(new AggregateCreateNodesError(failures, nodes)))),
                Effect.provide(layer),
            ),
        ),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
