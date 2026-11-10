import { Effect, Match, Schema } from 'effect';
import { Host } from 'effect/net';

// --- [MODELS] --------------------------------------------------------------------------

const End = Schema.Union([Schema.HostPortFromString, Schema.String.check(Schema.isPattern(/^\/[^:]+$/u)).pipe(Schema.decodeTo(Schema.UnixPathAddressFromString))]).annotate({
    description: 'host:port, or an absolute Unix socket path free of colon, the two ends an OpenSSH forward spec states',
});
const Allocated = Schema.Trim.pipe(Schema.decodeTo(Schema.FiniteFromString), Schema.decodeTo(Schema.Port));
const Spec = Schema.TaggedUnion({ Local: { bind: End, connect: End }, Remote: { bind: End, connect: End }, Dynamic: { bind: Schema.HostPortFromString } }).annotate({
    description: 'Local listens here and connects there, Remote the reverse, Dynamic serves SOCKS here; a Remote TCP bind on port 0 takes the port the server allocates',
});
const spec = (...ends: readonly (typeof End.Type)[]) => ends.map(Match.valueTags({ HostPort: Host.formatHostPort, UnixPathAddress: ({ path }) => path })).join(':');
const Forward = Object.assign(Spec, {
    argv: Spec.match({ Local: ({ bind, connect }) => ['-L', spec(bind, connect)], Remote: ({ bind, connect }) => ['-R', spec(bind, connect)], Dynamic: ({ bind }) => ['-D', spec(bind)] }),
    bound: (wanted: typeof Spec.Type, printed: string) =>
        Match.value(wanted).pipe(
            Match.when({ _tag: 'Remote', bind: { _tag: 'HostPort', port: 0 } }, ({ bind: { host }, connect }) =>
                Schema.decodeEffect(Allocated)(printed).pipe(
                    Effect.flatMap((port) => Effect.fromResult(Host.hostPort(host, port))),
                    Effect.map((bind) => Spec.cases.Remote.make({ bind, connect })),
                    Effect.orDie,
                ),
            ),
            Match.orElse(Effect.succeed<typeof Spec.Type>),
        ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Forward };
