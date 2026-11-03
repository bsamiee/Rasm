import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cause, type Config, Context, Duration, Effect, Fiber, identity, Layer, Option, Record, Schedule, Sink, Stream, SubscriptionRef } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { RpcClient, RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';
import { Terms } from '#/apple-events';
import { Contract } from '#/contract';

// --- [SERVICES] ------------------------------------------------------------------------

class Automation extends Context.Service<Automation>()('@rasm/automation/automation', {
    make: Effect.fnUntraced(function* ({ exitCode }: ChildProcessSpawner.ChildProcessHandle) {
        const client = yield* RpcClient.make(Contract);
        const held = Effect.fnUntraced(function* <A, E, R>(subject: string, changes: Stream.Stream<A, E, R>) {
            const [first, rest] = yield* Stream.peel(changes, Sink.head());
            const latest = yield* SubscriptionRef.make(yield* Effect.fromOption(first, () => new Cause.NoSuchElementError(`${subject} ended before its first value`)));
            return [latest, yield* Effect.forkScoped(Stream.runForEach(rest, (value) => SubscriptionRef.set(latest, value)))] as const;
        });
        const [[installations, discovering], [instances, tracking]] = yield* Effect.all([held('installations', client.installations()), held('instances', client.instances())]);
        const installed = Effect.flatMap(SubscriptionRef.get(installations), Effect.fromResult);
        return {
            ...client,
            installations,
            instances,
            installed,
            exited: Effect.raceFirst(exitCode, Effect.andThen(Fiber.joinAll([discovering, tracking]), exitCode)),
            opening: Effect.fnUntraced(function* (toOpen: URL) {
                const [urls, rows] = yield* Effect.all([client.applications({ toOpen }), installed]);
                const located = Record.fromIterableBy(rows, (row) => row.url.href);
                return Array.getSomes(Array.map(urls, (url) => Record.get(located, url.href)));
            }),
            script: Effect.fnUntraced(function* ({ pid, ...script }: { readonly pid: number } & Parameters<typeof Terms.script>[1]) {
                const { url } = yield* Unmatched.single(yield* SubscriptionRef.get(instances), 'running instance', Option.some(String(pid)), (row) => [String(row.pid)]);
                const execute = yield* Terms.script(yield* client.terms({ url }), script);
                return yield* client.execute({ pid, ...execute });
            }),
        };
    }),
}) {
    static readonly layer = (executable: Config.Config<string>) =>
        Layer.unwrap(
            Effect.gen(function* () {
                const [requests, command] = [new TransformStream<Uint8Array, Uint8Array>(), yield* executable];
                const child = yield* ChildProcessSpawner.ChildProcessSpawner.use((spawner) => spawner.spawn(ChildProcess.make(command, { stdin: Stream.orDie(Stream.fromReadableStream({ evaluate: () => requests.readable, onError: identity })), stderr: 'inherit' })));
                return Layer.effect(Automation, Automation.make(child)).pipe(
                    Layer.provide(RpcClient.layerProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0) })),
                    Layer.provide([Layer.effect(Socket.Socket)(Socket.fromTransformStream(Effect.succeed({ readable: Stream.toReadableStream(child.stdout), writable: requests.writable }))), RpcSerialization.layerNdJsonRpc()]),
                );
            }),
        );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Automation };
