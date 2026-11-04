import { Unmatched } from '@rasm/search';
import { Array, Cause, type Config, Context, Duration, Effect, Fiber, Function, flow, identity, Layer, Option, Record, Schedule, Schema, Sink, Stream, Struct, SubscriptionRef } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Rpc, RpcClient, RpcGroup, RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';
import { Execute, Terms } from '#/apple-events';
import { Installation, Launched, Perform, Pid, RunningInstance, Screenshot, State } from '#/applications';
import { AvailableFace } from '#/available-face';
import { HostError } from '#/host-error';

// --- [MODELS] --------------------------------------------------------------------------

const Failure = Schema.Union([Unmatched, Schema.ErrorInstance()]);
const Contract = RpcGroup.make(
    Rpc.make('installations', { success: Schema.Result(Schema.Array(Installation), Schema.ErrorInstance()).annotate({ description: 'On subscribe and each Spotlight update, or discovery error' }), error: Schema.ErrorInstance(), stream: true }),
    Rpc.make('instances', { success: Schema.Array(RunningInstance).annotate({ description: 'On subscribe and each change, exited instances kept until next change' }), stream: true }),
    Rpc.make('applications', { payload: { toOpen: Schema.URL }, success: Schema.Array(Schema.URL).annotate({ description: 'Launch Services applications for file, default first' }) }),
    Rpc.make('status', { payload: { pid: Pid }, success: State, error: Failure }),
    Rpc.make('launch', { payload: { url: Schema.URL }, success: Launched, error: Schema.ErrorInstance() }),
    Rpc.make('perform', { payload: { pid: Pid, ...Perform.fields }, error: Failure }),
    Rpc.make('terms', { payload: { url: Schema.URL }, success: Terms, error: Schema.ErrorInstance() }),
    Rpc.make('definition', { payload: { url: Schema.URL, kind: Terms.Kind, name: Schema.String }, success: Schema.String, error: Failure }),
    Rpc.make('execute', { payload: { pid: Pid, ...Execute.fields }, success: Execute.Result, error: Schema.Union([HostError, Failure]) }),
    Rpc.make('screenshot', { payload: { pid: Pid, ...Screenshot.fields, file: Schema.URL.annotate({ description: 'PNG file written' }) }, success: Screenshot.Result, error: Failure }),
    Rpc.make('faces', { success: Schema.Array(AvailableFace) }),
    Rpc.make('face', { payload: { name: AvailableFace.Name }, success: AvailableFace, error: Unmatched }),
);

// --- [SERVICES] ------------------------------------------------------------------------

class Automation extends Context.Service<Automation>()('@rasm/automation/node/Automation', {
    make: Effect.fnUntraced(function* ({ exitCode }: ChildProcessSpawner.ChildProcessHandle) {
        const client = yield* RpcClient.make(Contract);
        const [[installations, discovering], [instances, tracking]] = yield* Effect.all([watch('installations', client.installations()), watch('instances', client.instances())]);
        const installed = Effect.flatMap(SubscriptionRef.get(installations), Effect.fromResult);
        return {
            ...client,
            installations,
            instances,
            installed,
            exited: Effect.raceFirst(exitCode, Effect.andThen(Fiber.joinAll([discovering, tracking]), exitCode)),
            opening: (toOpen: URL) => Effect.map(Effect.all([client.applications({ toOpen }), installed]), Function.tupled(opening)),
            script: Effect.fn('Automation.script')(function* ({ pid, ...script }: { readonly pid: number } & Parameters<typeof Terms.script>[1]) {
                const { url } = yield* Unmatched.single(yield* SubscriptionRef.get(instances), 'running instance', Option.some(String(pid)), flow(Struct.get('pid'), String, Array.of));
                const execute = yield* Terms.script(yield* client.terms({ url }), script);
                return yield* client.execute({ pid, ...execute });
            }),
        };
    }),
}) {
    static readonly layer = Effect.fnUntraced(function* (executable: Config.Config<string>) {
        const [requests, command] = [new TransformStream<Uint8Array, Uint8Array>(), yield* executable];
        const child = yield* ChildProcessSpawner.ChildProcessSpawner.use((spawner) => spawner.spawn(ChildProcess.make(command, { stdin: Stream.orDie(Stream.fromReadableStream({ evaluate: Function.constant(requests.readable), onError: identity })), stderr: 'inherit' })));
        return Layer.effect(Automation, Automation.make(child)).pipe(
            Layer.provide(RpcClient.layerProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0) })),
            Layer.provide([Layer.effect(Socket.Socket)(Socket.fromTransformStream(Effect.succeed({ readable: Stream.toReadableStream(child.stdout), writable: requests.writable }))), RpcSerialization.layerNdJsonRpc()]),
        );
    }, Layer.unwrap);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const watch = Effect.fn('watch')(function* <A, E, R>(subject: string, changes: Stream.Stream<A, E, R>) {
    const [first, rest] = yield* Stream.peel(changes, Sink.head());
    const latest = yield* SubscriptionRef.make(yield* Effect.fromOption(first, () => new Cause.NoSuchElementError(`${subject} ended before its first value`)));
    return [latest, yield* Effect.forkScoped(Stream.runForEach(rest, (value) => SubscriptionRef.set(latest, value)))] as const;
});
const opening = (urls: readonly URL[], rows: readonly Installation[]) => {
    const located = Record.fromIterableBy(rows, (row) => row.url.href);
    return Array.getSomes(Array.map(urls, (url) => Record.get(located, url.href)));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Automation };
