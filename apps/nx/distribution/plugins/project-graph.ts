import { findPackageJSON } from 'node:module';
import { layer } from '@effect/platform-node/NodeServices';
import { AggregateCreateNodesError, type CreateNodes, type CreateNodesResultArray } from '@nx/devkit';
import { Array, Effect, FileSystem, Option, Path, Record, Schema } from 'effect';
import { HOSTS, RELEASE } from '#/plugins/host-packages';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FILES = `**/{${Array.dedupe(Record.values(HOSTS).map(({ manifest: file }) => file.split('/').at(-1) ?? file)).join(',')}}`;

// --- [MODELS] --------------------------------------------------------------------------

const Manifest = Schema.fromJsonString(Schema.Struct({ name: Schema.String }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [
    FILES,
    (files, _options, { workspaceRoot }) =>
        Effect.runPromise(
            Effect.gen(function* () {
                const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
                const { name } = yield* Effect.fromNullishOr(findPackageJSON(import.meta.url)).pipe(Effect.flatMap(fs.readFileString), Effect.flatMap(Schema.decodeEffect(Manifest)));
                const found = yield* Effect.forEach(
                    files,
                    (file) =>
                        Option.match(
                            Array.findFirst(Record.values(HOSTS), (host) => file === host.manifest || file.endsWith(`/${host.manifest}`)),
                            {
                                onNone: () => Effect.succeedNone,
                                onSome: (host) =>
                                    Effect.map(
                                        Effect.flatMap(fs.readFileString(path.join(workspaceRoot, file)), (text) => host.read(text, file)),
                                        Option.map(() => ({ file, host })),
                                    ),
                            },
                        ),
                    { concurrency: 'unbounded' },
                );
                return Array.getSomes(found).map(({ file, host }): CreateNodesResultArray[number] => [
                    file,
                    {
                        projects: {
                            [path.join(file, ...host.manifest.split('/').map(() => '..'))]: {
                                targets: {
                                    install: {
                                        executor: `${name}:install`,
                                        dependsOn: ['build'],
                                        cache: false,
                                        metadata: { technologies: [host.technology], description: `Installs the ${host.technology} packages build wrote, through ${host.install.command}` },
                                    },
                                    'nx-release-publish': {
                                        executor: `${name}:release-publish`,
                                        dependsOn: ['^nx-release-publish'],
                                        defaultConfiguration: RELEASE,
                                        configurations: { [RELEASE]: {} },
                                        metadata: {
                                            technologies: [host.technology],
                                            description: `Attaches the ${host.technology} packages build wrote in its ${RELEASE} configuration to the project's GitHub release${Option.match(Option.fromUndefinedOr(host.publish), { onNone: () => '', onSome: ({ command }) => ` and publishes them through ${command}` })}`,
                                        },
                                    },
                                },
                            },
                        },
                    },
                ]);
            }).pipe(
                Effect.mapError((error) => new AggregateCreateNodesError([[null, new Error(error.message)]], [])),
                Effect.provide(layer),
            ),
        ),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
