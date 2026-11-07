import { basename, join, relative } from 'node:path';
import { Config, Effect, FileSystem, Schema } from 'effect';
import type { HttpRouter } from 'effect/http';
import type { NetAddress } from 'effect/net';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { mergeConfig, searchForWorkspaceRoot, type UserConfig } from 'vite';
import { Peer } from '#/peer.ts';
import { Version } from '#/requests.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Link = Omit<ImportMetaEnv['LINK'], 'endpoint'> & { readonly path: Exclude<HttpRouter.PathInput, '*'>; readonly listen: NetAddress.InetAddress };

// --- [CONSTANTS] -----------------------------------------------------------------------

const MINVER = /^(?<core>\d+\.\d+\.)(?<patch>\d+)(?:-\S*?(?:\.(?<height>\d+))?)?(?:\+\S*)?$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Described = Schema.fromJsonString(Schema.Struct({ description: Schema.String }));
const Defined = Schema.fromJsonString(Schema.Unknown).annotate({ description: 'Value a define entry replaces its key with, as JSON text' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const numbered = (_: string, core: string, patch: string, height = '0') => `${core}${Number(patch) + Number(height)}`;
const release = Effect.fnUntraced(function* (directory: string) {
    const [fs, spawner] = yield* Effect.all([FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner]);
    const [identifier, { description }] = yield* Effect.all([Config.String('PRODUCT_BUNDLE_IDENTIFIER'), Effect.flatMap(fs.readFileString(join(directory, 'package.json')), Schema.decodeEffect(Described))], { concurrency: 'unbounded' });
    const version = yield* spawner.string(ChildProcess.make('minver', ['-v', 'e'], { cwd: directory, stdin: 'ignore', stderr: 'inherit' })).pipe(
        Effect.map((described) => described.trim().replace(MINVER, numbered)),
        Effect.flatMap(Schema.decodeEffect(Version)),
    );
    return { id: `${identifier}.${basename(directory)}`, name: description, version };
});
const dialed = Effect.fnUntraced(function* ({ path, redial, listen }: Link) {
    const origin = Peer.origin(yield* Config.Port('LINK_PORT').pipe(Config.withDefault(listen.port)));
    const dialing: ImportMetaEnv['LINK'] = { endpoint: `${origin}${path}`, redial };
    return { origin, define: { 'import.meta.env.LINK': yield* Effect.orDie(Schema.encodeEffect(Defined)(dialing)) } };
});
const bundle = (directory: string, config: UserConfig): UserConfig => {
    const root = searchForWorkspaceRoot(directory);
    return mergeConfig({ build: { outDir: join(root, '.artifacts', relative(root, directory)), emptyOutDir: false, rolldownOptions: { experimental: { lazyBarrel: true }, output: { codeSplitting: false } } } }, config);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { bundle, dialed, type Link, release };
