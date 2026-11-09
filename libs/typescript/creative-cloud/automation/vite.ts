import { createRequire } from 'node:module';
import { join } from 'node:path';
import { Config, Effect, Schema } from 'effect';
import type { HttpRouter } from 'effect/http';
import type { NetAddress } from 'effect/net';
import { mergeConfig, searchForWorkspaceRoot, type UserConfig } from 'vite';
import { Peer } from '#/peer';

// --- [TYPES] ---------------------------------------------------------------------------

type Link = Omit<ImportMetaEnv['LINK'], 'endpoint'> & { readonly path: Exclude<HttpRouter.PathInput, '*'>; readonly listen: NetAddress.InetAddress };

// --- [MODELS] --------------------------------------------------------------------------

const Defined = Schema.fromJsonString(Schema.Unknown).annotate({ description: 'Value a define entry replaces its key with, as JSON text' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const dialed = Effect.fnUntraced(function* ({ path, redial, listen }: Link) {
    const origin = Peer.origin(yield* Config.Port('LINK_PORT').pipe(Config.withDefault(listen.port)));
    const dialing: ImportMetaEnv['LINK'] = { endpoint: `${origin}${path}`, redial };
    return { 'import.meta.env.LINK': yield* Effect.orDie(Schema.encodeEffect(Defined)(dialing)) };
});
const bundle = (directory: string, config: UserConfig): UserConfig => {
    const { name }: { readonly name: string } = createRequire(join(directory, 'package.json'))('./package.json');
    return mergeConfig({ build: { outDir: join(searchForWorkspaceRoot(directory), '.artifacts', 'vite', name), emptyOutDir: false, rolldownOptions: { output: { codeSplitting: false } } } }, config);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { bundle, dialed, type Link };
