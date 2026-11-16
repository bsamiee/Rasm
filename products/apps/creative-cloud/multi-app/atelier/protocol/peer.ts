import type { Installation } from '@rasm/applications';
import { dial, listen } from '@rasm/rpc';
import { Duration, Effect, type Layer, Option, Schema, SchemaGetter } from 'effect';
import { Url } from 'effect/http';
import type { Rpc, RpcClient, RpcGroup } from 'effect/rpc';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUNDLE = /(?<=\.app)(?:\/.*)?$|(?<=[^/])$/u;
const PATH = '/link';

// --- [MODELS] --------------------------------------------------------------------------

class Peer extends Schema.Class<Peer, { readonly brand: unique symbol }>('Peer')({
    application: Schema.Union([
        Schema.URLFromString.pipe(Schema.decode({ decode: SchemaGetter.transform((url) => Url.setPathname(url, url.pathname.replace(BUNDLE, '/'))), encode: SchemaGetter.passthrough() })).annotate({ description: 'Application bundle URL with trailing slash' }),
        Schema.String.annotate({ description: 'Application name, from a host that reports no bundle path' }),
    ]),
    extension: Schema.String.annotate({ description: 'UXP plugin or CEP extension identifier' }),
    root: Schema.OptionFromOptionalKey(Schema.URLFromString).annotate({ description: 'Directory the host loaded for this plugin' }),
}) {
    static readonly link = { url: new URL(PATH, 'ws://localhost:7490'), path: PATH, origin: 'file://', redial: Duration.seconds(1) } as const;
    static readonly serves =
        ({ url, name }: Pick<Installation, 'url' | 'name'>) =>
        ({ application }: Peer) =>
            typeof application === 'string' ? application === name : application.href === url.href;
    static readonly dial =
        <Rpcs extends Rpc.Any, H, EH, RH, E, R>(group: RpcGroup.RpcGroup<Rpcs>, handlers: Layer.Layer<H, EH, RH>, application: Effect.Effect<URL | string, E, R>) =>
        <F, R2>(extension: string, root: Effect.Effect<URL, F, R2>) =>
            dial(group, handlers, {
                url: Peer.link.url,
                redial: Peer.link.redial,
                peer: Effect.all([application, root]).pipe(Effect.flatMap(([named, loaded]) => Effect.orDie(Schema.encodeEffect(Peer)(Peer.make({ application: named, extension, root: Option.some(loaded) }))))),
            });
    static readonly listen = <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, middleware: Layer.Layer<Rpc.MiddlewareClient<Rpcs>, never, RpcClient.Protocol>) =>
        listen(group, { path: Peer.link.path, origin: Peer.link.origin, peer: Peer, key: ({ application, extension }) => `${extension} in ${String(application)}`, middleware });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Peer };
