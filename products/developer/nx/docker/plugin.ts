import { existsSync } from 'node:fs';
import { findPackageJSON } from 'node:module';
import { join, posix, resolve } from 'node:path';
import process from 'node:process';
import { pathToFileURL } from 'node:url';
import { type CreateNodes, type CreateNodesResultArray, detectPackageManager, hashArray, type TargetConfiguration, type TargetDependencyConfig } from '@nx/devkit';
import { getLockFileName, PluginCache, quoteShellArg, workspaceDataDirectory } from '@nx/devkit/internal';
import { aggregated, anchored, fingerprinted, manifested, recalled, run, stamped } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Effect, FileSystem, Option, Predicate, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const OWN = fingerprinted(import.meta.dirname);
const MANIFEST = 'package.json';
const MISE = '.config/mise.toml';
const ROOT = '.';
const ENGINE: TargetDependencyConfig = { projects: [`directory:${ROOT}`], target: 'install--*' };
const DRIVEN: Readonly<Record<string, ReadonlySet<string>>> = { 'test--vitest': new Set(['testcontainers']), 'install--pulumi': new Set(['@pulumi/docker']) };

// --- [MODELS] --------------------------------------------------------------------------

const Required = Schema.fromJsonString(
    Schema.Struct({
        dependencies: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
        devDependencies: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    }),
).annotate({ description: 'The dependencies and devDependencies of a package.json, a package requiring an engine client of DRIVEN, itself or through one of its dependencies, running it in the target DRIVEN keys it under' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const engine = (workspaceRoot: string) =>
    Effect.map(manifested(workspaceRoot, ROOT), (stated) => {
        const started = ['colima', 'start', '--activate=false', '--ssh-config=false', '--mount', 'none', '--port-forwarder', 'grpc', '--memory', '8', '--disk', '20'];
        const install: TargetConfiguration = {
            command: `mkdir -p "\${COLIMA_HOME:-$HOME/.colima}" && ${started.map(quoteShellArg).join(' ')} --cpus "$(($(getconf _NPROCESSORS_ONLN) / 2))"`,
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
        return [MANIFEST, { projects: { [ROOT]: { root: ROOT, targets: aggregated({ install: { colima: install }, clean: { colima: clean } }, stated, 'colima', workspaceRoot) } } }] as const;
    });

const required = Effect.fnUntraced(function* (workspaceRoot: string, files: readonly string[]) {
    const rooted = anchored(workspaceRoot);
    const manifests = files.filter((file) => posix.basename(file) === MANIFEST && posix.dirname(file) !== ROOT && rooted(posix.dirname(file)));
    const [cache, key, stamp] = [new PluginCache<{ readonly sources: readonly string[]; readonly stamp: string; readonly drivers: CreateNodesResultArray }>(join(workspaceDataDirectory, 'docker.hash')), hashArray([yield* OWN, ...manifests]), stamped(workspaceRoot)];
    const read = (manifest: string) => FileSystem.FileSystem.use((fs) => fs.readFileString(resolve(workspaceRoot, manifest))).pipe(Effect.flatMap(Schema.decodeEffect(Required)));
    const driving = Effect.forEach(
        manifests,
        Effect.fnUntraced(function* (file) {
            const { dependencies, devDependencies } = yield* read(file);
            const named = Record.keys({ ...dependencies, ...devDependencies });
            const located = Array.getSomes(named.map((name) => Option.flatMap(Option.liftThrowable(findPackageJSON)(name, pathToFileURL(join(workspaceRoot, file))), Option.fromUndefinedOr)));
            const reached = yield* Effect.forEach(located, read, { concurrency: 'unbounded' });
            const requires = new Set([...named, ...reached.flatMap((held) => Record.keys(held.dependencies))]);
            const root = posix.dirname(file);
            const targets = Record.map(
                Record.filter(DRIVEN, (clients) => !clients.isDisjointFrom(requires)),
                (): TargetConfiguration => ({ dependsOn: ['...', ENGINE] }),
            );
            return Option.map(Option.liftPredicate(targets, Predicate.not(Record.isEmptyRecord)), (driven) => [file, { projects: { [root]: { root, targets: driven } } }] as const);
        }),
        { concurrency: 'unbounded' },
    );
    return yield* Effect.flatMap(
        recalled(cache, key, stamp),
        Option.match({
            onSome: ({ drivers }) => Effect.succeed(drivers),
            onNone: Effect.fnUntraced(function* () {
                const drivers: CreateNodesResultArray = Array.getSomes(yield* driving);
                const sources = [...manifests, getLockFileName(detectPackageManager(workspaceRoot))];
                cache.set(key, { sources, stamp: yield* stamp(sources), drivers });
                cache.writeToDisk();
                return drivers;
            }),
        }),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [`**/${MANIFEST}`, (files, _, { workspaceRoot }) => (process.platform === 'darwin' && existsSync(join(workspaceRoot, MISE)) ? run(Effect.map(Effect.all([engine(workspaceRoot), required(workspaceRoot, files)]), ([started, drivers]) => [started, ...drivers])) : [])];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
