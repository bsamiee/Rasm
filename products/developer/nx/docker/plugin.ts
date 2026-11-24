import { findPackageJSON } from 'node:module';
import { join, posix, relative, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { type CreateNodes, type CreateNodesResultArray, detectPackageManager, hashArray, normalizePath, type TargetConfiguration } from '@nx/devkit';
import { getLockFileName, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { aggregated, anchored, fingerprinted, recalled, run, stamped, within } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Config, Effect, FileSystem, flow, Option, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const OWN = fingerprinted(import.meta.dirname);
const MANIFEST = 'package.json';
const ENGINE = 'colima';
const CLIENTS: ReadonlySet<string> = new Set(['testcontainers', '@pulumi/docker']);

// --- [MODELS] --------------------------------------------------------------------------

const Required = Schema.fromJsonString(
    Schema.Struct({
        dependencies: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
        devDependencies: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    }),
).annotate({ description: 'The dependencies and devDependencies of a package.json, a package requiring an engine client of CLIENTS, itself or through one of its dependencies, taking tag host:docker' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const engine = (workspaceRoot: string) => {
    const started = ['colima', 'start', '--activate=false', '--ssh-config=false', '--mount', 'none', '--port-forwarder', 'grpc', '--memory', '8', '--disk', '20'];
    const install: TargetConfiguration = {
        command: `mkdir -p "$COLIMA_HOME" && ${started.map(quoteShellArg).join(' ')} --cpus "$(($(getconf _NPROCESSORS_ONLN) / 2))"`,
        inputs: [{ externalDependencies: [] }],
        metadata: {
            description:
                'Starts the Colima VM holding the container engine DOCKER_HOST names on half the cores of this machine with a data disk capped at 20 GiB, in the COLIMA_HOME folder it creates first as Colima reads that variable only once the folder exists, returning once the engine answers and leaving the VM up after the run, with colima start',
            help: { command: 'colima start --help', example: { args: ['--verbose'] } },
        },
    };
    const clean: TargetConfiguration = {
        executor: `${distribution.name}:clean`,
        dependsOn: ['install--colima'],
        options: {
            commands: [
                ['docker', 'image', 'prune', '--force'],
                ['docker', 'builder', 'prune', '--force'],
                ['colima', 'ssh', '--', 'sudo', 'fstrim', '--all', '--verbose'],
            ],
        },
        configurations: { cache: { commands: [['colima', 'delete', '--data', '--force']] } },
        inputs: [{ externalDependencies: [] }],
        metadata: {
            description: 'Removes dangling images and build cache with docker image prune and docker builder prune, then discards the freed blocks of the VM disks with fstrim so their files shrink on this machine, and under configuration cache deletes the VM with its data with colima delete --data',
            help: { command: 'colima delete --help', example: { args: [] } },
        },
    };
    return Effect.map(
        Config.option(Config.String('COLIMA_HOME')),
        flow(
            Option.map((home) => normalizePath(relative(workspaceRoot, resolve(workspaceRoot, home)))),
            Option.filter(within),
            Option.map((root) => [MANIFEST, { projects: { [root]: { root, name: ENGINE, targets: aggregated({ install: { [ENGINE]: install }, clean: { [ENGINE]: clean } }, {}, ENGINE, workspaceRoot) } } }] as const),
            Option.toArray,
        ),
    );
};

const required = Effect.fnUntraced(function* (workspaceRoot: string, files: readonly string[]) {
    const rooted = anchored(workspaceRoot);
    const manifests = files.filter((file) => posix.basename(file) === MANIFEST && rooted(posix.dirname(file)));
    const cache = new PluginCache<{ readonly sources: readonly string[]; readonly tagged: CreateNodesResultArray } & { readonly stamp: string }>(join(workspaceDataDirectory, 'docker.hash'));
    const read = (manifest: string) => FileSystem.FileSystem.use((fs) => fs.readFileString(resolve(workspaceRoot, manifest))).pipe(Effect.flatMap(Schema.decodeEffect(Required)));
    const tagging = Effect.forEach(
        manifests,
        Effect.fnUntraced(function* (file) {
            const { dependencies, devDependencies } = yield* read(file);
            const named = Record.keys({ ...dependencies, ...devDependencies });
            const located = Array.getSomes(named.map((name) => Option.flatMap(Option.liftThrowable(findPackageJSON)(name, pathToFileURL(join(workspaceRoot, file))), Option.fromUndefinedOr)));
            const reached = yield* Effect.forEach(located, read, { concurrency: 'unbounded' });
            const root = posix.dirname(file);
            return Option.map(
                Option.liftPredicate(new Set([...named, ...reached.flatMap((held) => Record.keys(held.dependencies))]), (requires) => !CLIENTS.isDisjointFrom(requires)),
                (): CreateNodesResultArray[number] => [file, { projects: { [root]: { root, tags: ['host:docker'] } } }],
            );
        }),
        { concurrency: 'unbounded' },
    ).pipe(Effect.map((found) => ({ sources: [...manifests, getLockFileName(detectPackageManager(workspaceRoot))], tagged: Array.getSomes(found) })));
    return (yield* recalled(cache, hashArray([yield* OWN, ...manifests]), stamped(workspaceRoot), tagging, () => cache.writeToDisk())).tagged;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [`**/${MANIFEST}`, (files, _, { workspaceRoot }) => run(Effect.map(Effect.all([engine(workspaceRoot), required(workspaceRoot, files)]), Array.flatten))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
