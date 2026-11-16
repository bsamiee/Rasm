import { UxpManifest } from '@rasm/uxp';
import { Array, Effect, Schema } from 'effect';
import { applyEdits, modify } from 'jsonc-parser';
import type { Plugin } from 'vite';
import { Peer } from '#/peer';

// --- [MODELS] --------------------------------------------------------------------------

const Defined = Schema.fromJsonString(Schema.Unknown).annotate({ description: 'Value a define entry replaces its key with, as JSON text' });
const Permitted = Schema.fromJsonString(Schema.Struct({ requiredPermissions: Schema.Struct({ network: Schema.optionalKey(Schema.Struct({ domains: Schema.Union([Schema.Array(Schema.String), Schema.Literal('all')]) })) }) })).annotate({
    description: 'Network domains a UXP manifest permits',
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const linked = (): Plugin => ({
    name: 'atelier-link',
    enforce: 'post',
    config: () =>
        Schema.encodeEffect(Defined)({ endpoint: `${Peer.link.origin}${Peer.link.path}`, redial: Peer.link.redial } satisfies ImportMetaEnv['LINK']).pipe(
            Effect.map((link) => ({ define: { 'import.meta.env.LINK': link } })),
            Effect.runPromise,
        ),
    generateBundle: (_, bundle) => {
        const manifest = bundle[UxpManifest.file];
        return manifest?.type === 'asset'
            ? Effect.gen(function* () {
                  const text = typeof manifest.source === 'string' ? manifest.source : new TextDecoder().decode(manifest.source);
                  const { network } = (yield* Schema.decodeEffect(Permitted)(text)).requiredPermissions;
                  const domains = network?.domains ?? [];
                  yield* domains === 'all' ? Effect.void : Effect.sync(() => Object.assign(manifest, { source: applyEdits(text, modify(text, ['requiredPermissions', 'network', 'domains'], Array.dedupe(Array.append(domains, Peer.link.origin)), { formattingOptions: { insertSpaces: true, tabSize: 4 } })) }));
              }).pipe(Effect.runPromise)
            : undefined;
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { linked };
