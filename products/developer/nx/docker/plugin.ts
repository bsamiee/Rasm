import { join, posix, relative, resolve } from 'node:path';
import { type CreateNodes, type CreateNodesResultArray, hashArray, normalizePath } from '@nx/devkit';
import { readModulePackageJson } from '@nx/devkit/internal';
import { aggregated, anchored, EXECUTORS, fingerprinted, inferring, run, within } from '@rasm/nx/inference';
import { Array, Config, Effect, FileSystem, Option, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const OWN = fingerprinted(import.meta.dirname);
const MANIFEST = 'package.json';
const ENGINE = 'docker';
const CLIENTS: ReadonlySet<string> = new Set(['testcontainers', '@pulumi/docker']);

// --- [MODELS] --------------------------------------------------------------------------

const Required = Schema.fromJsonString(
    Schema.Struct({
        dependencies: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
        devDependencies: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    }),
).annotate({ description: 'The dependencies and devDependencies of a package.json, a package requiring an engine client of CLIENTS, itself or through one of its dependencies, taking tag host:docker' });

// --- [COMPOSITION] ---------------------------------------------------------------------

const nodes = inferring(Schema.Struct({}), run, function* (files, _, { workspaceRoot }, cached) {
    const [fs, anchor] = [yield* FileSystem.FileSystem, anchored(workspaceRoot)];
    const colima = {
        install: {
            [ENGINE]: {
                command: `mkdir -p "$COLIMA_HOME" && { colima status > /dev/null 2>&1 || colima start --activate=false --ssh-config=false --mount none --port-forwarder grpc --memory 8 --disk 20 --cpus "$(($(getconf _NPROCESSORS_ONLN) / 2))"; } && colima ssh -- sudo sh -c 'tune2fs -o discard "$(findmnt -nvo SOURCE -T /var/lib/docker)" && mount -o remount,discard /var/lib/docker'`,
                inputs: [{ externalDependencies: [] }],
                metadata: {
                    description:
                        'Starts the Colima VM of profile COLIMA_PROFILE holding the Docker Engine DOCKER_HOST names on half the cores of this machine with a data disk capped at 20 GiB, in the COLIMA_HOME folder it creates first as Colima reads that variable only once the folder exists, returning once the engine answers and leaving the VM up after the run, with colima start where colima status finds no VM running, then stores discard as a default mount option of the disk holding /var/lib/docker with tune2fs and remounts it with it, so blocks the VM frees return to this machine as it frees them',
                    help: { command: 'colima start --help', example: { args: ['--verbose'] } },
                },
            },
        },
        clean: {
            [ENGINE]: {
                executor: EXECUTORS.clean,
                dependsOn: [`install--${ENGINE}`],
                options: {
                    commands: [
                        ['docker', 'image', 'prune', '--all', '--force'],
                        ['docker', 'builder', 'prune', '--force'],
                    ],
                },
                configurations: { cache: { commands: [['colima', 'delete', '--data', '--force']] } },
                inputs: [{ externalDependencies: [] }],
                metadata: {
                    description:
                        'Removes images no container uses and dangling build cache with docker image prune and docker builder prune, their blocks returning to this machine through the discard mount option install--docker sets on the disk holding /var/lib/docker, and under configuration cache deletes the VM with its data with colima delete --data',
                    help: { command: 'colima delete --help', example: { args: [] } },
                },
            },
        },
    };
    const service = {
        install: {
            [ENGINE]: {
                command: 'docker version',
                inputs: [{ externalDependencies: [] }],
                metadata: {
                    description: 'Confirms the Docker Engine DOCKER_HOST names, else the default socket the system service manager keeps running, answers, printing the client and engine versions and failing while the engine is down, with docker version',
                    help: { command: 'docker version --help', example: { args: [] } },
                },
            },
        },
    };
    const folder = (found: Option.Option<string>, below: string) =>
        found.pipe(
            Option.map((at) => normalizePath(relative(workspaceRoot, resolve(workspaceRoot, at, below)))),
            Option.filter(within),
        );
    const backed = Option.map(folder(yield* Config.option(Config.String('COLIMA_HOME')), '.'), (root) => ({ root, verbs: colima }));
    const served = Option.map(folder(yield* Config.option(Config.String('CACHE_DIR')), ENGINE), (root) => ({ root, verbs: service }));
    const engine = Option.map(
        Option.orElse(backed, () => served),
        ({ root, verbs }) => [MANIFEST, { projects: { [root]: { root, name: ENGINE, targets: aggregated(verbs, {}, ENGINE, anchor) } } }] as const,
    );
    const sources = files.filter((file) => anchor.holds(posix.dirname(file)));
    const pass = yield* cached<{ readonly sources: readonly string[]; readonly tagged: readonly string[] }>('docker.hash', yield* OWN);
    const tagging = Effect.filter(
        sources,
        Effect.fnUntraced(function* (file) {
            const { dependencies, devDependencies } = yield* Effect.flatMap(fs.readFileString(join(workspaceRoot, file)), Schema.decodeEffect(Required));
            const named = Record.keys({ ...dependencies, ...devDependencies });
            const reached = Array.getSomes(named.map((name) => Option.liftThrowable(readModulePackageJson)(name, [join(workspaceRoot, posix.dirname(file))])));
            return !CLIENTS.isDisjointFrom(new Set([...named, ...reached.flatMap(({ packageJson }) => Record.keys(packageJson.dependencies ?? {}))]));
        }),
        { concurrency: 'unbounded' },
    );
    const { tagged } = yield* pass.recall(hashArray(sources), tagging.pipe(Effect.map((held) => ({ sources, tagged: held }))));
    return [...Option.toArray(engine), ...tagged.map((file): CreateNodesResultArray[number] => [file, { projects: { [posix.dirname(file)]: { root: posix.dirname(file), tags: ['host:docker'] } } }])];
});

const createNodes: CreateNodes = [`**/${MANIFEST}`, nodes];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
