import { Content, Resource, Server } from '@rasm/mcp';
import { Cause, Context, Deferred, Effect, flow, Layer, Option, Queue, Ref, Schema, Stream, SubscriptionRef } from 'effect';
import { McpSchema, Tool } from 'effect/ai';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PNG = 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg==';
const TEXT = 'This is a simple text response for testing.';
const STATIC = 'This is the content of the static text resource.';
const EMBEDDED = 'This is an embedded resource content.';
const FAILURE = 'This tool intentionally returns an error for testing';
const INTERNAL = 'connection string postgres://owner:secret@db.internal';
const STEPS = Object.values({ started: 0, halfway: 50, finished: 100 });
const IDS = ['beta', 'alpine', 'alpha'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Weather = Schema.Struct({ temperature: Schema.Finite, conditions: Schema.String });
const Attachment = Schema.Struct({ uri: Schema.String, mimeType: Schema.OptionFromOptionalKey(Schema.String), data: Schema.Uint8Array });
const Templated = Schema.Struct({ id: Schema.String, templateTest: Schema.Literal(true), data: Schema.String });
const Valued = Schema.Struct({ value: Schema.String });

// --- [SERVICES] ------------------------------------------------------------------------

class Probe extends Context.Service<Probe>()('@rasm/test-mcp/fixtures/Probe', {
    make: Effect.gen(function* () {
        const [calls, entered, interrupted, release, watched] = yield* Effect.all([Ref.make<readonly string[]>([]), Queue.unbounded<string>(), Queue.unbounded<string>(), Deferred.make<void>(), SubscriptionRef.make('open')]);
        return { calls, entered, interrupted, release, watched };
    }),
}) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const registered = Effect.fnUntraced(function* ({ calls, entered, interrupted, release, watched }: Probe['Service']) {
    const called = <A, E, R>(name: string, answer: Effect.Effect<A, E, R>) => Ref.update(calls, (held) => [...held, name]).pipe(Effect.andThen(Queue.offer(entered, name)), Effect.andThen(answer));
    const gated = (name: string) =>
        called(
            name,
            Deferred.await(release).pipe(
                Effect.as('released'),
                Effect.onInterrupt(() => Queue.offer(interrupted, name)),
            ),
        );
    yield* Server.registerTool(Tool.make('test_simple_text', { description: 'Returns simple text', success: Schema.String }).annotate(Tool.Readonly, true), () => called('test_simple_text', Effect.succeed(TEXT)));
    yield* Server.registerTool(Tool.make('test_value', { description: 'Returns the value it is given', parameters: Valued, success: Schema.String }).annotate(Tool.Readonly, true), ({ value }) => called('test_value', Effect.succeed(value)));
    yield* Server.registerTool(Tool.make('test_image_content', { description: 'Returns an image', success: Content.image('image/png') }).annotate(Tool.Readonly, true), () => called('test_image_content', Effect.succeed(Uint8Array.fromBase64(PNG))));
    yield* Server.registerTool(Tool.make('test_embedded_resource', { description: 'Returns an embedded resource', success: Content.attachment(Attachment) }).annotate(Tool.Readonly, true), () =>
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
    yield* Server.registerTool(Tool.make('gated_read', { description: 'Waits for release, reads only', success: Schema.String }).annotate(Tool.Readonly, true), () => gated('gated_read'));
    yield* Server.registerTool(Tool.make('gated_write', { description: 'Waits for release, changes state', success: Schema.String }), () => gated('gated_write'));
    yield* Resource.register({ name: 'static-text', description: 'A static text resource for testing', mimeType: 'text/plain', schema: Schema.String, changes: Stream.empty, content: Effect.succeed(STATIC) });
    yield* Resource.register({ name: 'static-binary', description: 'A static binary resource for testing', mimeType: 'image/png', schema: Schema.Uint8Array, changes: Stream.empty, content: Effect.succeed(Uint8Array.fromBase64(PNG)) });
    yield* Resource.register({ name: 'watched-resource', description: 'A resource whose value changes', mimeType: 'text/plain', schema: Schema.String, changes: SubscriptionRef.changes(watched), content: SubscriptionRef.get(watched) });
    yield* Resource.template`template/${McpSchema.param('id', Schema.String)}/data`({
        name: 'template',
        description: 'A resource template with one variable',
        schema: Templated,
        changes: Stream.empty,
        completion: { id: () => Effect.succeed([...IDS]) },
        content: (_, id) => called(`template ${id}`, Effect.succeed({ id, templateTest: true as const, data: `Data for ID: ${id}` })),
    });
});

const app = flow(registered, Layer.effectDiscard);

// --- [EXPORTS] -------------------------------------------------------------------------

export { app, EMBEDDED, FAILURE, IDS, INTERNAL, PNG, Probe, STATIC, STEPS, TEXT };
