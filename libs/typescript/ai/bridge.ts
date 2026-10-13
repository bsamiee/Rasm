import { Duration, Effect, Schema, Scope } from 'effect';
import { UrlParams } from 'effect/http';
import { NetAddress } from 'effect/net';
import { type Rpc, type RpcGroup, RpcSerialization, RpcServer } from 'effect/rpc';
import { Socket, SocketServer } from 'effect/socket';

// --- [TYPES] ---------------------------------------------------------------------------

type Link = 'linked' | 'closed' | 'unreachable';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REDIAL = Duration.seconds(2);
const WIRE = RpcSerialization.layerSchemaBinary({ fingerprintPayloads: true });

// --- [OPERATIONS] ----------------------------------------------------------------------

const defaulted = <S extends Schema.Top>(schema: S, value: S['Type'] & S['Encoded']): Schema.withDecodingDefaultKey<S['Rebuild']> => schema.annotate({ default: value }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(value)));
const unit = <P extends Schema.Top>(preview: P): Schema.Struct<{ readonly description: Schema.String; readonly preview: Schema.OptionFromOptionalKey<Schema.Union<readonly [Schema.Literal<true>, P]>> }> =>
    Schema.Struct({
        description: Schema.String.annotate({ description: 'Present-progressive sentence shown while the call runs, naming its undo step' }),
        preview: Schema.OptionFromOptionalKey(Schema.Union([Schema.Literal(true), preview]).annotate({ description: 'Runs the call, then undoes every change, returning its result or refusal, with an image of a given target' })),
    });
const serve = Effect.fn('serve')(function* <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, { url, identity, concurrency, link }: { readonly url: string; readonly identity: UrlParams.Input; readonly concurrency: number | 'unbounded'; readonly link: (link: Link) => Effect.Effect<void> }) {
    const [{ reader, writer }, linked] = yield* Effect.all([Socket.makeWebSocket(`${url}?${UrlParams.toString(identity)}`), Scope.make()]);
    const protocol = yield* RpcServer.makeProtocolSocketServer.pipe(
        Effect.provideService(SocketServer.SocketServer, {
            address: NetAddress.inetAddressUnsafe(NetAddress.ipv4Unspecified, 0),
            run: (handler) =>
                handler(
                    Socket.make({
                        reader: reader.pipe(
                            Effect.tapError(() => link('unreachable')),
                            Effect.tap(() => Effect.acquireRelease(link('linked'), () => link('closed'))),
                        ),
                        writer,
                    }),
                ).pipe(Effect.exit, Effect.andThen(Effect.sleep(REDIAL)), Effect.forever),
        }),
        Effect.provide(WIRE),
        Scope.provide(linked),
    );
    yield* Effect.forkScoped(RpcServer.make(group, { concurrency, disableTracing: true }).pipe(Effect.provideService(RpcServer.Protocol, protocol)));
    yield* Effect.addFinalizer((exit) => Scope.close(linked, exit));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { defaulted, type Link, REDIAL, serve, unit, WIRE };
