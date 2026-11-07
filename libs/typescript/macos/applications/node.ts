import { Unmatched } from '@rasm/search';
import { Array, Context, Duration, Effect, Function, identity, Layer, Record, Schedule, Schema, Stream } from 'effect';
import type { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { Rpc, RpcClient, RpcGroup, RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';
import { Execute, Installation, Launch, Launching, Perform, Pid, Pressure, RunningInstance, Screenshot, State, Terms, WindowID } from '#/index.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Selection = typeof Selection.Type;

// --- [MODELS] --------------------------------------------------------------------------

const Selection = Schema.Struct({
    identifiers: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Bundle identifiers' })),
    prefix: Schema.optionalKey(Schema.String.annotate({ description: 'Bundle identifier prefix, joined with identifiers by or, every admitted application when both are absent' })),
});
const Failure = Schema.Union([Unmatched, Schema.ErrorInstance()]);
const Contract = RpcGroup.make(
    Rpc.make('installations', { payload: Selection.fields, success: Schema.Result(Schema.Array(Installation), Schema.ErrorInstance()).annotate({ description: 'On subscribe and each Spotlight update, or discovery error' }), stream: true }),
    Rpc.make('instances', { payload: Selection.fields, success: Schema.Array(RunningInstance).annotate({ description: 'On subscribe and each change, exited instances kept until next change' }), stream: true }),
    Rpc.make('pressure', { success: Schema.Result(Pressure, Schema.ErrorInstance()).annotate({ description: 'On subscribe and each kernel change, or read error' }), stream: true }),
    Rpc.make('applications', { payload: { toOpen: Schema.URL }, success: Schema.Array(Schema.URL).annotate({ description: 'Launch Services applications for file, default first' }) }),
    Rpc.make('read', { payload: { pid: Pid, window: Schema.optionalKey(WindowID.annotate({ description: 'Walks that row alone, absent walks every element' })) }, success: State, error: Failure }),
    Rpc.make('launch', { payload: Launch.fields, success: Launching, error: Failure, stream: true }),
    Rpc.make('perform', { payload: { pid: Pid, ...Perform.fields }, success: State.annotate({ description: 'State after the last action' }), error: Failure }),
    Rpc.make('quit', { payload: { pid: Pid, force: Schema.Boolean.pipe(Schema.withConstructorDefault(Effect.succeed(false))) }, success: State.annotate({ description: 'Last state, a dialog or unresponsive instance stays running' }), error: Failure }),
    Rpc.make('terms', { payload: { url: Schema.URL }, success: Terms, error: Failure }),
    Rpc.make('definition', { payload: { url: Schema.URL, kind: Terms.Kind, name: Schema.String }, success: Schema.String, error: Failure }),
    Rpc.make('execute', { payload: { pid: Pid, ...Execute.fields }, success: Execute.Result, error: Failure }),
    Rpc.make('screenshot', { payload: { pid: Pid, ...Screenshot.fields, directory: Schema.URL.annotate({ description: 'Directory receiving <number>.png' }) }, success: Screenshot.Result, error: Failure }),
);

// --- [SERVICES] ------------------------------------------------------------------------

class Automation extends Context.Service<Automation>()('@rasm/applications/node/Automation', {
    make: Effect.fnUntraced(function* ({ exitCode }: ChildProcessSpawner.ChildProcessHandle) {
        const client = yield* RpcClient.make(Contract);
        const installed = (selection: Selection) => client.installations(selection).pipe(Stream.runHead, Effect.flatMap(Effect.fromOption), Effect.flatMap(Effect.fromResult));
        return {
            ...client,
            installed,
            exited: exitCode,
            opening: (toOpen: URL, selection: Selection) => Effect.map(Effect.all([client.applications({ toOpen }), installed(selection)]), Function.tupled(opening)),
            script: Effect.fn('Automation.script')(function* ({ pid, url, ...script }: { readonly pid: number; readonly url: URL } & Parameters<typeof Terms.script>[1]) {
                const execute = yield* Terms.script(yield* client.terms({ url }), script);
                return yield* client.execute({ pid, ...execute });
            }),
        };
    }),
}) {
    static readonly layer = Effect.fnUntraced(function* (command: ChildProcess.Command) {
        const [requests, child] = [new TransformStream<Uint8Array, Uint8Array>(), yield* command];
        yield* Stream.fromReadableStream({ evaluate: Function.constant(requests.readable), onError: identity }).pipe(Stream.orDie, Stream.run(child.stdin), Effect.forkScoped);
        return Layer.effect(Automation, Automation.make(child)).pipe(
            Layer.provideMerge(RpcClient.layerProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0) })),
            Layer.provide([Layer.effect(Socket.Socket)(Socket.fromTransformStream(Effect.succeed({ readable: Stream.toReadableStream(child.stdout), writable: requests.writable }))), RpcSerialization.layerNdJsonRpc()]),
        );
    }, Layer.unwrap);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const opening = (urls: readonly URL[], rows: readonly Installation[]) => {
    const located = Record.fromIterableBy(rows, (row) => row.url.href);
    return Array.getSomes(Array.map(urls, (url) => Record.get(located, url.href)));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Automation };
