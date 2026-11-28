import { Match, Schema } from 'effect';
import { Host as Address, type NetAddress } from 'effect/net';

// --- [CONSTANTS] -----------------------------------------------------------------------

const mux = { type: { Local: 1, Remote: 2, Dynamic: 3 }, streamlocal: 0xff_ff_ff_fe } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Streamlocal = Schema.String.check(Schema.isPattern(/^\/[^\0]*$/u))
    .pipe(Schema.decodeTo(Schema.UnixPathAddressFromString))
    .annotate({ description: 'Absolute Unix socket path free of NUL, which an SSH string field carries' });
const Open = Schema.HostPortFromString.check(Schema.makeFilter(({ port }) => port > 0, { description: 'A port above 0, which OpenSSH requires of every forward end but a Remote bind' }));
const End = Schema.Union([Open, Streamlocal]).annotate({ description: 'host:port above port 0, or an absolute Unix socket path, the two ends an OpenSSH forward states' });
const Spec = Schema.TaggedUnion({ Local: { bind: End, connect: End }, Remote: { bind: Schema.Union([Schema.HostPortFromString, Streamlocal]), connect: End }, Dynamic: { bind: Open } }).annotate({
    description: 'Local listens here and connects there, Remote the reverse, Dynamic serves SOCKS here; a Remote TCP bind on port 0 takes the port the server allocates',
});
const end = (at: Address.HostPort | NetAddress.UnixPathAddress) => Match.valueTags(at, { HostPort: ({ host, port }) => [Address.formatHost(host), port] as const, UnixPathAddress: ({ path }) => [path, mux.streamlocal] as const });
const Forward = Object.assign(Spec, {
    fields: (forward: typeof Spec.Type) => [mux.type[forward._tag], ...end(forward.bind), ...Spec.matchOrElse(forward, { Dynamic: () => ['socks', 0] as const }, ({ connect }) => end(connect))] as const,
    bound: (forward: typeof Spec.Type, port: number) =>
        Match.value(forward).pipe(
            Match.when({ _tag: 'Remote', bind: { _tag: 'HostPort', port: 0 } }, (remote) => Spec.cases.Remote.make({ bind: Address.hostPortUnsafe(remote.bind.host, port), connect: remote.connect })),
            Match.orElse(() => forward),
        ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Forward };
