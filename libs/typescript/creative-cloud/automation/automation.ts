import { Array, Cause, Context, Duration, Effect, identity, Layer, Schedule, Schema, Sink, Stream, SubscriptionRef } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { RpcClient, RpcClientError, type RpcMessage, RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';
import type { Execute } from '#/apple-events';
import { Contract } from '#/contract';

// --- [MODELS] --------------------------------------------------------------------------

const MessageJson = Schema.fromJsonString(Schema.Unknown);

// --- [SERVICES] ------------------------------------------------------------------------

class Automation extends Context.Service<Automation>()('@rasm/automation/automation', {
    make: Effect.gen(function* () {
        const client = yield* RpcClient.make(Contract);
        const held = Effect.fnUntraced(function* <A, E, R>(changes: Stream.Stream<A, E, R>, subject: string) {
            const [first, rest] = yield* Stream.peel(changes, Sink.head());
            const latest = yield* Effect.flatMap(
                Effect.fromOption(first, () => new Cause.NoSuchElementError(`${subject} stream ended before first value`)),
                SubscriptionRef.make,
            );
            yield* Effect.forkScoped(Stream.runForEach(rest, (value) => SubscriptionRef.set(latest, value)));
            return latest;
        });
        const [installations, instances] = yield* Effect.all([held(client.installations(), 'installations'), held(client.instances(), 'instances')]);
        return { ...client, installations, instances, installed: Effect.flatMap(SubscriptionRef.get(installations), Effect.fromResult) };
    }),
}) {
    static readonly protocol = (pid: number, event: (message: string) => Pick<Execute, 'command' | 'arguments'>) =>
        Layer.effect(RpcClient.Protocol)(
            RpcClient.Protocol.make(
                Effect.fnUntraced(function* (writeResponse) {
                    const { execute } = yield* Automation;
                    const sent = Effect.fnUntraced(
                        function* (clientId: number, request: RpcMessage.RequestEncoded) {
                            const { reply } = yield* execute({ pid, ...event(yield* Schema.encodeEffect(MessageJson)(request)) });
                            // biome-ignore lint/nursery/noUnsafeTypeAssertion: RpcClient decodes each part through codecFor
                            const responses = Array.ensure(yield* Schema.decodeUnknownEffect(MessageJson)(reply)) as readonly RpcMessage.FromServerEncoded[];
                            yield* Effect.forEach(responses, (response) => writeResponse(clientId, response), { discard: true });
                        },
                        (effect, _, request) => Effect.mapError(effect, (cause) => RpcClientError.RpcClientError.make({ reason: RpcClientError.RpcClientDefect.make({ message: `${request.tag} request to process ${pid} failed`, cause }) })),
                    );
                    return {
                        send: (clientId: number, request: RpcMessage.FromClientEncoded) => (request._tag === 'Request' ? sent(clientId, request) : Effect.void),
                        supportsAck: false,
                        supportsTransferables: false,
                        codecFor: RpcSerialization.json.codecFor,
                    };
                }),
            ),
        );
    static readonly layer = (executable: string) =>
        Layer.effect(Automation, Automation.make).pipe(
            Layer.provide(RpcClient.layerProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0) })),
            Layer.provide([
                Layer.effect(Socket.Socket)(
                    Socket.fromTransformStream(
                        Effect.gen(function* () {
                            const requests = new TransformStream<Uint8Array, Uint8Array>();
                            const child = yield* ChildProcessSpawner.ChildProcessSpawner.use((spawner) => spawner.spawn(ChildProcess.make(executable, { stdin: Stream.orDie(Stream.fromReadableStream({ evaluate: () => requests.readable, onError: identity })), stderr: 'inherit' })));
                            return { readable: Stream.toReadableStream(child.stdout), writable: requests.writable };
                        }).pipe(Effect.mapError((cause) => Socket.SocketError.make({ reason: Socket.SocketOpenError.make({ kind: 'Unknown', cause }) }))),
                    ),
                ),
                RpcSerialization.layerNdJsonRpc(),
            ]),
        );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Automation };
