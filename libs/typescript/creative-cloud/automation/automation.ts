import { Cause, Context, Duration, Effect, identity, Layer, Schedule, Sink, Stream, SubscriptionRef } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { RpcClient, RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';
import { Contract } from '#/contract';

// --- [SERVICES] ------------------------------------------------------------------------

class Host extends Context.Service<Host, ChildProcessSpawner.ChildProcessHandle>()('@rasm/automation/automation/Host') {}

class Automation extends Context.Service<Automation>()('@rasm/automation/automation', {
    make: Effect.gen(function* () {
        const [client, { exitCode: exited }] = yield* Effect.all([RpcClient.make(Contract), Host]);
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
        return { ...client, installations, instances, exited, installed: Effect.flatMap(SubscriptionRef.get(installations), Effect.fromResult) };
    }),
}) {
    static readonly layer = (executable: string) =>
        Layer.unwrap(
            Effect.gen(function* () {
                const requests = new TransformStream<Uint8Array, Uint8Array>();
                const child = yield* ChildProcessSpawner.ChildProcessSpawner.use((spawner) => spawner.spawn(ChildProcess.make(executable, { stdin: Stream.orDie(Stream.fromReadableStream({ evaluate: () => requests.readable, onError: identity })), stderr: 'inherit' })));
                return Layer.effect(Automation, Automation.make).pipe(
                    Layer.provide(RpcClient.layerProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0) })),
                    Layer.provide([Layer.succeed(Host)(child), Layer.effect(Socket.Socket)(Socket.fromTransformStream(Effect.succeed({ readable: Stream.toReadableStream(child.stdout), writable: requests.writable }))), RpcSerialization.layerNdJsonRpc()]),
                );
            }),
        );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Automation };
