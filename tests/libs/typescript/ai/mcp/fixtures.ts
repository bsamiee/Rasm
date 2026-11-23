import { Content, registerResource, registerTemplate, Server } from '@rasm/mcp';
import { Cause, Context, Effect, flow, Latch, Layer, MutableRef, Option, Queue, Ref, Result, Schema, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { type McpProtocol, McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PNG = 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg==';
const TEXT = 'This is a simple text response for testing.';
const STATIC = 'This is the content of the static text resource.';
const EMBEDDED = 'This is an embedded resource content.';
const FAILURE = 'This tool intentionally returns an error for testing';
const INTERNAL = 'connection string postgres://owner:secret@db.internal';
const STEPS = Object.values({ started: 0, halfway: 50, finished: 100 });
const IDS = ['beta', 'alpine', 'alpha'] as const;
const STATE = 'asked once';
const ASKED = 'a dialog asks';
const PATH = '/mcp';
const UPSTREAM = new URL(PATH, 'http://upstream.test');

// --- [MODELS] --------------------------------------------------------------------------

const Weather = Schema.Struct({ temperature: Schema.Finite, conditions: Schema.String });
const Attachment = Schema.Struct({ uri: Schema.String, mimeType: Schema.OptionFromOptionalKey(Schema.String), data: Schema.Uint8Array });
const Templated = Schema.Struct({ id: Schema.String, templateTest: Schema.Literal(true), data: Schema.String });
const Valued = Schema.Struct({ value: Schema.String });
const Echoing = Schema.Struct({ echoed: Schema.String });
const Tools = Toolkit.make(
    Tool.make('echo', { description: 'Echoes its value', parameters: Valued, success: Echoing }),
    Tool.make('secret', { description: 'Refused by the filter', success: Schema.String }),
    Tool.make('test_simple_text', { description: 'Shares a name with a tool of the server', success: Schema.String }),
);

// --- [SERVICES] ------------------------------------------------------------------------

class Probe extends Context.Service<Probe>()('@rasm/test-mcp/fixtures/Probe', {
    make: Effect.gen(function* () {
        const [calls, entered, interrupted, released, release, watched, blocked] = yield* Effect.all([Ref.make<readonly string[]>([]), Queue.unbounded<string>(), Queue.unbounded<string>(), Queue.unbounded<string>(), Latch.make(), SubscriptionRef.make('open'), SubscriptionRef.make(false)]);
        return { calls, entered, interrupted, released, release, watched, blocked };
    }),
}) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const registered = Effect.fnUntraced(function* ({ calls, entered, interrupted, released, release, watched, blocked }: Probe['Service']) {
    const called = <A, E, R>(name: string, answer: Effect.Effect<A, E, R>) => Ref.update(calls, (held) => [...held, name]).pipe(Effect.andThen(Queue.offer(entered, name)), Effect.andThen(answer));
    const gated = (name: string) =>
        called(
            name,
            release.await.pipe(
                Effect.as('released'),
                Effect.onInterrupt(() => Queue.offer(interrupted, name)),
            ),
        );
    yield* Server.registerTool(Tool.make('test_simple_text', { description: 'Returns simple text', success: Schema.String }).annotate(Tool.Readonly, true), () => called('test_simple_text', Effect.succeed(TEXT)));
    yield* Server.registerTool(Tool.make('test_value', { description: 'Returns the value it is given', parameters: Valued, success: Schema.String }).annotate(Tool.Readonly, true), ({ value }) => called('test_value', Effect.succeed(value)));
    yield* Server.registerTool(Tool.make('test_image_content', { description: 'Returns an image', success: Content.image('image/png') }).annotate(Tool.Readonly, true), () => called('test_image_content', Effect.succeed(Uint8Array.fromBase64(PNG))));
    yield* Server.registerTool(Tool.make('test_embedded_resource', { description: 'Returns an embedded resource', success: Content.embedded(Attachment) }).annotate(Tool.Readonly, true), () =>
        called('test_embedded_resource', Effect.succeed({ uri: 'test://embedded-resource', mimeType: Option.some('text/plain'), data: new TextEncoder().encode(EMBEDDED) })),
    );
    yield* Server.registerTool(Tool.make('test_structured', { description: 'Returns a structured result', success: Weather }).annotate(Tool.Readonly, true), () => called('test_structured', Effect.succeed({ temperature: 22.5, conditions: 'Partly cloudy' })));
    yield* Server.registerTool(Tool.make('test_error_handling', { description: 'Fails on every call', success: Schema.String }).annotate(Tool.Readonly, true), () => called('test_error_handling', Effect.fail(new Cause.IllegalArgumentError(FAILURE))));
    yield* Server.registerTool(Tool.make('test_defect', { description: 'Dies on every call', success: Schema.String }).annotate(Tool.Readonly, true), () => called('test_defect', Effect.die(new Cause.UnknownError(undefined, INTERNAL))));
    yield* Server.registerTool(Tool.make('test_hidden', { description: 'Hidden from every caller', success: Schema.String }).annotate(Tool.Readonly, true).annotate(Server.Visible, Effect.succeed(false)), () => called('test_hidden', Effect.succeed(TEXT)));
    yield* Server.registerTool(Tool.make('test_tool_with_progress', { description: 'Reports progress in three steps', success: Schema.String }).annotate(Tool.Readonly, true), () =>
        called(
            'test_tool_with_progress',
            Stream.fromIterable(STEPS).pipe(
                Server.progress((progress) => ({ progress, total: 100 })),
                Stream.runDrain,
                Effect.as('done'),
            ),
        ),
    );
    yield* Server.registerTool(Tool.make('test_scoped', { description: 'Holds a scoped resource while it runs', success: Schema.String }).annotate(Tool.Readonly, true), () =>
        called(
            'test_scoped',
            Effect.as(
                Effect.acquireRelease(Effect.void, () => Queue.offer(released, 'test_scoped')),
                'held',
            ),
        ),
    );
    yield* Server.registerTool(
        Tool.make('test_input', { description: 'Asks for client input, then answers with the state it sealed', success: Schema.String }).annotate(Tool.Readonly, true),
        Effect.fnUntraced(function* () {
            const [{ requestState }, { input }] = yield* Effect.all([McpSchema.McpRequestContext, Server.ToolCall]);
            return yield* called('test_input', UndefinedOr.match(requestState, { onUndefined: () => input(new McpSchema.InputRequired({ requestState: STATE })), onDefined: Effect.succeed }));
        }),
    );
    yield* Server.registerTool(Tool.make('gated_read', { description: 'Waits for release, reads only', success: Schema.String }).annotate(Tool.Readonly, true), () => gated('gated_read'));
    yield* Server.registerTool(Tool.make('gated_write', { description: 'Waits for release, changes state', success: Schema.String }), () => gated('gated_write'));
    yield* Server.registerTool(
        Tool.make('gated_early', { description: 'Waits for release, returning early while blocked', success: Schema.String }).annotate(
            Server.Ends,
            Stream.filterMap(SubscriptionRef.changes(blocked), (held) => (held ? Result.succeed(ASKED) : Result.failVoid)),
        ),
        () => gated('gated_early'),
    );
    yield* Server.registerTool(Tool.make('test_settle', { description: 'Settles the calls of its handle that returned early', success: Schema.String }), () => called('test_settle', Effect.as(Server.ToolCall.use(Struct.get('settle')), 'settled')));
    yield* registerResource({ name: 'static-text', description: 'A static text resource for testing', mimeType: 'text/plain', schema: Schema.String, changes: Stream.empty, content: Effect.succeed(STATIC) });
    yield* registerResource({ name: 'static-binary', description: 'A static binary resource for testing', mimeType: 'image/png', schema: Schema.Uint8Array, changes: Stream.empty, content: Effect.succeed(Uint8Array.fromBase64(PNG)) });
    yield* registerResource({ name: 'watched-resource', description: 'A resource whose value changes', mimeType: 'text/plain', schema: Schema.String, changes: SubscriptionRef.changes(watched), content: SubscriptionRef.get(watched) });
    yield* registerTemplate`template/${McpSchema.param('id', Schema.String)}/data`({
        name: 'template',
        description: 'A resource template with one variable',
        schema: Templated,
        changes: Stream.empty,
        completion: { id: () => Effect.succeed([...IDS]) },
        content: (_, id) => called(`template ${id}`, Effect.succeed({ id, templateTest: true as const, data: `Data for ID: ${id}` })),
    });
});

const app = flow(registered, Layer.effectDiscard);

const registrations = Layer.mergeAll(
    McpServer.toolkit(Tools).pipe(Layer.provide(Tools.toLayer({ echo: ({ value }) => Effect.succeed({ echoed: value }), secret: () => Effect.succeed('secret'), test_simple_text: () => Effect.succeed('upstream') }))),
    Layer.effectDiscard(McpServer.registerResource({ uri: 'upstream://note', name: 'note', mimeType: 'text/plain', content: Effect.succeed('upstream note') })),
    Layer.effectDiscard(McpServer.registerResource`upstream://items/${McpSchema.param('id', Schema.String)}`({ name: 'item', mimeType: 'text/plain', completion: { id: () => Effect.succeed(['apple', 'apricot', 'banana']) }, content: (_, id) => Effect.succeed(`item ${id}`) })),
    Layer.effectDiscard(McpServer.registerPrompt({ name: 'greet', parameters: { who: Schema.String }, completion: { who: () => Effect.succeed(['world', 'wide']) }, content: ({ who }) => Effect.succeed(`hello ${who}`) })),
);

const upstream = (protocol: McpProtocol.ProtocolAdapter, token: MutableRef.MutableRef<string>, rejected: MutableRef.MutableRef<number>) => {
    const guarded = HttpRouter.middleware(
        (next) =>
            Effect.flatMap(HttpServerRequest.HttpServerRequest, ({ headers }) =>
                headers['authorization'] === `Bearer ${MutableRef.get(token)}`
                    ? next
                    : Effect.as(
                          Effect.sync(() => MutableRef.increment(rejected)),
                          HttpServerResponse.empty({ status: 401 }),
                      ),
            ),
        { global: true },
    );
    return Effect.acquireRelease(
        Effect.sync(() => HttpRouter.toWebHandler(Layer.merge(McpServer.layerHttp({ name: 'upstream', version: '1.0.0', path: PATH, protocols: [protocol], registrations }).pipe(Layer.provideMerge(McpServer.McpServer.layer)), guarded), { disableLogger: true })),
        ({ dispose }) => Effect.promise(dispose),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { app, ASKED, Echoing, EMBEDDED, FAILURE, IDS, PNG, Probe, STATE, STATIC, STEPS, TEXT, UPSTREAM, upstream };
