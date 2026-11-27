import { createServer } from 'node:http';
import { layer as served } from '@effect/platform-node/NodeHttpServer';
import { layerWebSocketConstructor } from '@effect/platform-node/NodeSocket';
import { assert, it } from '@effect/vitest';
import { Description } from '@rasm/applications/description';
import { Link } from '@rasm/mcp/link';
import { HostError } from '@rasm/object-model/model';
import { Dialogs, Evaluation, EvaluationRpcs } from '@rasm/object-model/rpcs';
import { Plugin, Uxp } from '@rasm/uxp/plugin';
import { Array, Cause, Context, Deferred, Effect, Function, Layer, MutableRef, Option, Record, Schema, type Scope, Stream, Struct } from 'effect';
import { McpSchema, Tool, Toolkit } from 'effect/ai';
import { HttpRouter, HttpServer, Url } from 'effect/http';
import { Socket } from 'effect/socket';
import { vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ORIGIN = 'http://suite.test';
const DIALOGS = Record.fromIterableWith(['alert', 'confirm', 'prompt'], (name) => [
    name,
    () => {
        throw new Error(`host ${name}`);
    },
]);
const ANSWERED = "return Number(alert('saved') === undefined) + Number(confirm('replace?')) * 2 + Number(prompt('name', 'untitled') === null) * 4;";

// --- [MODELS] --------------------------------------------------------------------------

const Bound = Schema.Struct({ port: Schema.Int });
const Probe = Toolkit.make(Tool.make('probe', { description: 'Reads the host interaction level a request runs under', success: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const linked = Link.listen(EvaluationRpcs.merge(Description.Rpcs, Link.Rpcs.omit('tools/list', 'events')).middleware(Dialogs), ORIGIN);
const dialed = Effect.fnUntraced(function* (dialogs: (host: { level: string }) => Effect.Effect<unknown, HostError, Scope.Scope>, ended: Stream.Stream<never, HostError>) {
    const [ready, host, seen, subscribed] = [yield* Deferred.make<Effect.Success<typeof linked>>(), { level: 'interactive' }, MutableRef.make<readonly string[]>([]), yield* Deferred.make<void>()];
    const routes = Layer.effectDiscard(Effect.flatMap(linked, (listened) => Deferred.succeed(ready, listened)));
    const services = yield* Layer.build(Layer.provideMerge(HttpRouter.serve(routes, { disableLogger: true, disableListenLog: true }), served(createServer, { port: 0 })));
    const [{ port }, links] = yield* Effect.all([Schema.decodeUnknownEffect(Bound)(Context.get(services, HttpServer.HttpServer).address), Deferred.await(ready)]);
    const observed = Effect.sync(() => {
        MutableRef.update(seen, Array.append(host.level));
        return host.level;
    });
    const sockets = Layer.effect(Socket.WebSocketConstructor)(Effect.map(Socket.WebSocketConstructor, (make) => (url: string) => make(Url.setHost(new URL(url), `127.0.0.1:${port}`).href, { headers: { origin: ORIGIN } }))).pipe(Layer.provide(layerWebSocketConstructor));
    const link = Plugin.dial({
        application: Effect.succeed('Suite Host'),
        group: EvaluationRpcs,
        toolkit: Probe,
        handlers: Layer.merge(EvaluationRpcs.toLayer({ evaluate: () => Effect.map(observed, (level) => ({ result: Option.some(level) })) }), Probe.toLayer({ probe: () => observed })),
        source: Description.tools({ current: Effect.map(observed, (id) => ({ id, label: id })), select: () => Effect.void, changes: Stream.never }),
        events: Stream.merge(Stream.unwrap(Effect.as(Deferred.succeed(subscribed, undefined), Stream.never)), ended),
        dialogs: dialogs(host),
    });
    yield* Layer.build(link.pipe(Layer.provide(Layer.merge(Layer.succeed(Plugin)({ id: 'app.rasm.suite', host: { app: 'ID' }, requiredPermissions: { network: { domains: ['https://example.com', 'ws://localhost:7490'] } } }), sockets))));
    yield* Stream.runHead(Stream.filter(links.changes, Array.isArrayNonEmpty)).pipe(Effect.timeoutOrElse({ duration: '20 seconds', orElse: () => new Cause.TimeoutError(`the suite plugin linked to no listener on port ${port} within 20 seconds`) }));
    return {
        links,
        host,
        seen: Effect.sync(() => MutableRef.get(seen)),
        subscribed: Deferred.await(subscribed).pipe(Effect.timeoutOrElse({ duration: '20 seconds', orElse: () => new Cause.TimeoutError('the events subscription reached no host events of the suite plugin within 20 seconds') })),
    };
});
const raising = (error: Error): never => {
    throw error;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

vi.mock(import('adobe:uxp'), () => ({}));
vi.mock(import('adobe:fs'), () => ({}));

it.effect('names the body line an error was built on when the body read its stack before throwing', () =>
    Effect.map(Effect.flip(Uxp.execute({ root: {}, kinds: {} }, "(function body() {\n    const first = 1;\n    const error = new Error('early ' + first);\n    void error.stack;\n    throw error;\n})")), (failed) => {
        assert.instanceOf(failed, HostError);
        assert.deepStrictEqual([failed.reason, failed.subject], [{ _tag: 'Thrown', line: Option.some(3) }, 'early 1']);
    }),
);

it.effect(
    'decodes the host module require resolves by name through the given schema, and refuses a name it resolves none for',
    Effect.fnUntraced(function* () {
        yield* Effect.acquireRelease(
            Effect.sync(() => {
                vi.stubGlobal('require', (module: string) => (module === 'indesign' ? { app: { version: '21.2' } } : raising(new Error(`Cannot find module '${module}'`))));
            }),
            () => Effect.sync(() => vi.unstubAllGlobals()),
        );
        const [loaded, refused] = yield* Effect.all([Uxp.require('indesign', Schema.Struct({ app: Schema.Struct({ version: Schema.String }) })), Effect.flip(Uxp.require('photoshop', Schema.Never))]);
        assert.instanceOf(refused, HostError);
        assert.deepStrictEqual([loaded, refused.reason, refused.subject], [{ app: { version: '21.2' } }, { _tag: 'Refused' }, "require('photoshop')"]);
    }, Effect.scoped),
);

it.effect('streams the arguments of each host listener call and removes the listener it added once the stream ends', () =>
    Effect.gen(function* () {
        const added = new Set<(...args: readonly [number, string]) => void>();
        const heard = yield* Stream.runCollect(
            Stream.take(
                Uxp.listen<readonly [number, string]>('suite updates', {
                    add: (listener) => {
                        added.add(listener);
                        listener(3, 'three');
                        listener(5, 'five');
                    },
                    remove: (listener) => added.delete(listener),
                }),
                2,
            ),
        );
        assert.deepStrictEqual(
            [heard, added.size],
            [
                [
                    [3, 'three'],
                    [5, 'five'],
                ],
                0,
            ],
        );
    }),
);

it.effect('answers alert with undefined, confirm with false, and prompt with null inside an evaluated body, reaching no host dialog', () =>
    Effect.acquireRelease(
        Effect.sync(() => {
            Record.toEntries(DIALOGS).forEach(([name, raise]) => {
                vi.stubGlobal(name, raise);
            });
        }),
        () => Effect.sync(() => vi.unstubAllGlobals()),
    ).pipe(
        Effect.andThen(Uxp.execute({ root: {}, kinds: {} }, Evaluation.expression(ANSWERED, false))),
        Effect.map((result) => assert.deepStrictEqual(result, Option.some(5))),
        Effect.scoped,
    ),
);

it.live(
    'holds the host dialog suppression over each evaluate, describe, and tool call it serves and restores the host level after each, the events subscription holding none',
    Effect.fnUntraced(function* () {
        const { links, host, seen, subscribed } = yield* dialed((held) => Uxp.preserve(held, { level: 'suppressed' }), Stream.never);
        const client = yield* links.call(Function.constTrue, 'suite plugin');
        const evaluated = yield* client('evaluate', { code: 'unused', label: 'Run probe' });
        const between = host.level;
        yield* client('describe', {});
        const called = yield* client('tools/call', { name: 'probe', arguments: {}, context: { clientId: 1, protocolVersion: '2025-11-25', clientCapabilities: McpSchema.ClientCapabilities.make() } });
        yield* client('events', undefined).pipe(Stream.runDrain, Effect.forkScoped);
        yield* subscribed;
        assert.deepStrictEqual(
            [evaluated, between, called.isError === true, yield* seen, host.level, Array.map(yield* links.peers, Struct.get('requests'))],
            [{ result: Option.some('suppressed') }, 'interactive', false, ['suppressed', 'suppressed', 'suppressed', 'interactive'], 'interactive', [new Set(['evaluate', 'describe', 'operate'])]],
        );
    }, Effect.scoped),
);

it.live(
    'fails evaluate, describe, and tool calls with the HostError of a refused dialog suppression, decoded by the listening side, and runs no handler',
    Effect.fnUntraced(function* () {
        const refusal = HostError.refused('preferences level')();
        const { links, seen } = yield* dialed(() => refusal, Stream.never);
        const client = yield* links.call(Function.constTrue, 'suite plugin');
        const [evaluated, described, called] = yield* Effect.all([
            Effect.flip(client('evaluate', { code: 'unused', label: 'Run probe' })),
            Effect.flip(client('describe', {})),
            Effect.flip(client('tools/call', { name: 'probe', arguments: {}, context: { clientId: 1, protocolVersion: '2025-11-25', clientCapabilities: McpSchema.ClientCapabilities.make() } })),
        ]);
        assert.instanceOf(evaluated, HostError);
        assert.instanceOf(called, HostError);
        assert.deepStrictEqual([evaluated.message, described.message, called.message, yield* seen], [refusal.message, refusal.message, refusal.message, []]);
    }, Effect.scoped),
);

it.live(
    'fails the events subscription with the failure of the host event stream, the listening side reading its message',
    Effect.fnUntraced(function* () {
        const refusal = HostError.refused('theme updates')();
        const { links } = yield* dialed(() => Effect.void, Stream.fail(refusal));
        const client = yield* links.call(Function.constTrue, 'suite plugin');
        const failed = yield* client('events', undefined).pipe(Stream.runDrain, Effect.flip, Effect.timeoutOrElse({ duration: '20 seconds', orElse: () => new Cause.TimeoutError('the events subscription ended with no failure within 20 seconds') }));
        assert.strictEqual(failed.message, refusal.message);
    }, Effect.scoped),
);
