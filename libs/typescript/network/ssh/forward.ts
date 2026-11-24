import { Effect, Match, Schema } from 'effect';
import { Host, type NetAddress } from 'effect/net';

// --- [MODELS] --------------------------------------------------------------------------

const Socket = Schema.String.check(Schema.isPattern(/^\/[^:]+$/u)).pipe(Schema.decodeTo(Schema.UnixPathAddressFromString));
const Open = Schema.HostPortFromString.check(Schema.makeFilter(({ port }) => port > 0, { description: 'A port above 0, which OpenSSH requires of every forward end but a Remote bind' }));
const End = Schema.Union([Open, Socket]).annotate({
    description: 'host:port above port 0, or an absolute Unix socket path free of colon, the two ends an OpenSSH forward spec states',
});
const Allocated = Schema.Trim.pipe(Schema.decodeTo(Schema.FiniteFromString), Schema.decodeTo(Schema.Port));
const Spec = Schema.TaggedUnion({ Local: { bind: End, connect: End }, Remote: { bind: Schema.Union([Schema.HostPortFromString, Socket]), connect: End }, Dynamic: { bind: Open } }).annotate({
    description: 'Local listens here and connects there, Remote the reverse, Dynamic serves SOCKS here; a Remote TCP bind on port 0 takes the port the server allocates',
});
const spec = (...ends: readonly (Host.HostPort | NetAddress.UnixPathAddress)[]) => ends.map(Match.valueTags({ HostPort: Host.formatHostPort, UnixPathAddress: ({ path }) => path })).join(':');
const Forward = Object.assign(Spec, {
    argv: Spec.match({ Local: ({ bind, connect }) => ['-L', spec(bind, connect)], Remote: ({ bind, connect }) => ['-R', spec(bind, connect)], Dynamic: ({ bind }) => ['-D', spec(bind)] }),
    bound: (printed: string) =>
        Match.type<typeof Spec.Type>().pipe(
            Match.when({ _tag: 'Remote', bind: { _tag: 'HostPort', port: 0 } }, ({ bind: { host }, connect }) => Effect.orDie(Effect.map(Schema.decodeEffect(Allocated)(printed), (port) => Spec.cases.Remote.make({ bind: Host.hostPortUnsafe(host, port), connect })))),
            Match.orElse((wanted) => Effect.succeed(wanted)),
        ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Forward };
